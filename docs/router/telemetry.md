# Routing Telemetry

> **Status: Implemented, but unverified in this repo's environment.** This repo's Linux CI/agent
> environment has no .NET SDK and cannot install one (network policy blocks the installer), so
> everything below was written by careful manual review against the existing, presumably-working
> code — it has never been compiled or run here. Treat it as review-verified, not test-verified,
> until it's built on a machine with the .NET 10 SDK. The server-side pieces do have unit test
> coverage (see "Tests" below) that should be run there to confirm.

## Purpose

`src/AgenticRouter/Telemetry/` captures per-request routing telemetry from the live traffic path
and pushes it to connected clients (currently `AgenticRouter.Gui`) over SignalR, so the dashboard's
Live Stream and Cost Analytics tabs can show real conversations instead of only `MockData`. See
[`../gui/dashboard.md`](../gui/dashboard.md) for how the GUI consumes this, and
[`../gui/backlog.md`](../gui/backlog.md) for the backlog items this closed out.

This is purely additive: every dependency it introduces into existing classes
(`ProxyServer`, `ProxyHostedService`, `ProxyMiddleware`) is an appended optional constructor
parameter with an internal default, so none of the proxy's existing request-forwarding behavior or
its existing tests change. Every telemetry operation (usage extraction, publishing) is wrapped in
try/catch so a telemetry failure can never affect the forwarded client response.

## What gets captured, per request

`ProxyMiddleware.InvokeAsync` builds and publishes one `RoutingTelemetryEvent` per forwarded
request, after the response has already been fully forwarded to the client:

| Field | Source |
|---|---|
| `SessionId`, `IsSessionSynthesized` | `SessionIdResolver` (see below); synthesized as a random GUID if nothing resolves |
| `TurnNumber` | `ConversationTurnTracker`, an in-memory per-session counter |
| `RequestedModel`, `ResolvedModel`, `Provider` | From the existing `ModelRouteResolver` resolution already computed for routing |
| `IsFallback` | Hardcoded `false` — `ModelRouteResolver` has no fallback-routing concept today, unlike the GUI's mock data which simulates one |
| `PromptTokens`, `CompletionTokens` | `UsageExtractor`, parsed from the captured response body (see below); `null` if extraction fails or the provider is unrecognized |
| `EstimatedCostUsd` | `PricingOptions` (`appsettings.json`'s `Pricing` section) applied to the extracted token counts; `null` if the model has no pricing entry or usage wasn't extracted |
| `IsStreaming` | Whether the upstream response's `Content-Type` was `text/event-stream` |
| `LatencyToHeadersMs` | Time from sending the upstream request to receiving response headers |
| `TotalDurationMs` | Time from sending the upstream request to finishing forwarding the full body |
| `StatusCode`, `TimestampUtc` | The forwarded response's status code; capture time |

### Session/conversation identification

`SessionIdResolver` mirrors the convention this repo's own `claude-code-router` TypeScript source
already uses (`resolveSessionId`/`extractSessionIdFromPayload`) rather than inventing a new one, in
priority order:

1. Header `x-claude-code-session-id`, then `x-claude-session-id`.
2. Body field `session_id`, `sessionId`, `conversation_id`, `conversationId`, `chat_id`, `chatId`,
   `thread_id`, or `threadId` (first match wins, in that order).
3. `metadata.user_id`, split on the literal `"_session_"` marker.
4. If nothing matches, a random GUID is synthesized and `IsSessionSynthesized` is set — the request
   is treated as its own single-turn "session" rather than dropped.

`ConversationTurnTracker` is a `ConcurrentDictionary<string, int>` counting turns per session,
process-lifetime only (not persisted, resets on restart).

### Token usage extraction

`UsageExtractor` dispatches on `provider` to `OpenAiUsageParser` or `AnthropicUsageParser`;
unrecognized providers (Alibaba, Zhipu, Moonshot, MiniMax) return no usage rather than throwing.
Both parsers handle streaming and non-streaming responses:

- **OpenAI**: `usage.prompt_tokens`/`usage.completion_tokens` from the non-streaming body, or from
  the final SSE `data:` chunk before `[DONE]` when streaming (only present if the client requested
  `stream_options.include_usage=true` — many real client requests won't have it, so a `null` usage
  on a streaming OpenAI response is an expected, common case, not a bug).
- **Anthropic**: `usage.input_tokens`/`usage.output_tokens` from the non-streaming body, or the
  `message_start` event's `message.usage.input_tokens` (fixed) combined with the *last*
  `message_delta` event's `usage.output_tokens` (cumulative) when streaming.

To extract usage without disrupting true streaming pass-through timing, `ProxyMiddleware` no longer
does a plain `Content.CopyToAsync(Response.Body)`. Instead `CopyAndCaptureAsync` loops
`ReadAsync`/`WriteAsync` manually, writing every chunk to the client immediately (same timing as
before) while separately appending a capped copy (4 MiB, `ArrayPool<byte>.Shared` buffer) to an
in-memory buffer for parsing after the response has finished forwarding.

### Pricing

`PricingOptions` (`appsettings.json`'s `Pricing` section) is a static
`Dictionary<string, ModelPrice>` of input/output cost per million tokens, keyed by model name.
**The checked-in values are illustrative placeholders** (see the `_comment` field in
`appsettings.json`), not verified against real, current provider pricing — replace them before
trusting `EstimatedCostUsd` for anything beyond rough relative comparison.

## Transport: SignalR

`ProxyServer` adds a second endpoint alongside the existing catch-all proxy `app.Run(...)`:
`app.UseRouting()` + `app.UseEndpoints(e => e.MapHub<TelemetryHub>("/telemetry/hub"))`, registered
via `services.AddSignalR()` on the **inner** Kestrel host's DI container (not the outer application
container `ProxyMiddleware` is constructed in — these are deliberately separate; see the code
comment on `ProxyHostedService` warning about a prior unbounded-recursion bug). `TelemetryHub` is
empty (pure server-push, no client-callable methods).

`TelemetryPublisher` bridges the two containers: it's a mutable singleton (registered as both
`TelemetryPublisher` and `ITelemetryPublisher`, resolving to the same instance) constructed in the
outer container, whose `IHubContext<TelemetryHub>` is only available after the inner host starts.
`ProxyServer.StartAsync` calls `_telemetryPublisher.AttachHubContext(...)` right after
`_host.StartAsync()` completes. Before that attachment, `PublishAsync` silently no-ops rather than
throwing, so a request handled before the hub context is attached (a narrow startup race) just
doesn't get a telemetry event, no error.

The wire event (`RoutingTelemetryEvent`) has an independent, hand-kept-in-sync twin on the GUI side,
`AgenticRouter.Gui.Telemetry.RoutingTelemetryEventDto` — the two projects don't reference each
other (they're two separately-deployable processes that only share a JSON contract), and SignalR's
default `JsonHubProtocol` matches property names case-insensitively via its camelCase wire
convention, so identical PascalCase property names on both sides "just work" without manual JSON
configuration.

## GUI consumption

`AgenticRouter.Gui.Telemetry.ConversationAggregator.Aggregate` (pure, unit-tested) groups a flat
list of `RoutingTelemetryEventDto`s into `LiveConversation`/`LiveConversationTurn` records by
`SessionId`, ordering turns by `TurnNumber` and conversations by most-recently-active first.

`AgenticRouter.Gui`'s `Services/LiveDataStore.cs` owns a `HubConnection` to
`http://localhost:5001/telemetry/hub` (the proxy's default port; not currently configurable — the
GUI has no settings-persistence mechanism yet), accumulates every received event, and re-runs
`ConversationAggregator.Aggregate` on the full accumulated list after each new event. It's
registered as a singleton in `MauiProgram.cs`, started once from `Dashboard.razor`'s
`OnInitializedAsync`, and connection failures (e.g. the proxy isn't running) are logged and
swallowed — the dashboard just shows no live conversations until a connection succeeds.

`Services/LiveConversationMapper.cs` then maps `LiveConversation`/`LiveConversationTurn` onto the
dashboard's existing `Models.Conversation`/`ConversationTurn` view-model shape. Several
`ConversationTurn` fields have no live-data source given this telemetry event's scope and are set
to **honest, explicit defaults rather than fabricated values**:

| Field | Default | Why |
|---|---|---|
| `RoutingRoi` | `0` | No "worst case" baseline cost is computed for live requests |
| `ToolExecutionSteps` | `0` | The proxy doesn't introspect tool calls within a turn |
| `CacheHitRate` | `0` | Prompt-cache usage isn't parsed from provider responses |
| `ContextBufferPercent` | `0` | No per-model context-window-size configuration exists |
| `RequestSummary`, `ResponseSummary` | `null` | Full payload text is deliberately not relayed over the telemetry channel (scope/privacy) |

`ConversationTurn.TimeToFirstTokenMs` **is** real — it's `LatencyToHeadersMs` from the event. The
Razor components already render these defaults gracefully (e.g. `TurnCard.razor` shows "—" for a
zero ROI or cache rate, `ConversationSummary.razor`'s avg-ROI does the same), so no component
changes were needed to consume live data safely.

`Dashboard.razor` uses `LiveDataStore.Conversations` for the Live Stream tab and the Cost Analytics
tab's `Conversations` parameter (feeding its token-compounding chart); `CostData`, `AgentRoi`,
`TokenBuckets`, `ModelShares`, and `Providers` remain `MockData` — those have no telemetry source at
all yet (no cumulative-savings baseline, no per-agent ROI concept, no token-bucket/model-share
aggregation, no provider-budget tracking). See [`../gui/backlog.md`](../gui/backlog.md) for what
that would take.

## Tests

`src/AgenticRouter.Tests/Telemetry/` covers every new server-side class (session resolution
priority/fallback, turn-counter concurrency via 200 parallel calls, both providers' streaming and
non-streaming usage extraction, SSE event parsing edge cases, pricing math, and the hub-context
bridge's attach/publish/fault-isolation behavior via mocked `IHubContext`), plus four new
integration-style cases appended to `ProxyMiddlewareTests.cs` covering the full request → telemetry
event path, turn-number persistence across requests, session synthesis, and fault isolation from the
client response. `AgenticRouter.Gui.Telemetry.Tests/` covers `ConversationAggregator`'s
grouping/ordering/summation, including null-token/cost handling and unsorted input.

`AgenticRouter.Gui`'s own `Services/LiveDataStore.cs` and `Services/LiveConversationMapper.cs` are
**not** unit-tested: like the rest of `AgenticRouter.Gui` (Razor components, `MauiProgram.cs`,
`TrayWindowManager.cs`), they depend on Windows-only MAUI/Blazor types (or, for `LiveDataStore`,
live `HubConnection` networking) and can't be built or tested in this repo's Linux environment. The
logic they wrap is tested where it's actually portable (`ConversationAggregator`, above).
