# AgenticRouter.Gui: Not-Yet-Implemented Work

A backlog of gaps between what the GUI docs describe/design and what `src/AgenticRouter.Gui/`
actually does today. Originally sourced from explicit statements in [`dashboard.md`](dashboard.md)'s
"Known gaps" section, [`src/AgenticRouter.Gui/README.md`](../../src/AgenticRouter.Gui/README.md)'s
"Current limitations," and deferred/optional items in
[`livestream-redesign-plan.md`](livestream-redesign-plan.md).

## Open

### 1. Extend live telemetry to the rest of the dashboard

Live Stream and Cost Analytics' Token Compounding chart now read live data (see "Recently
completed" below and [`../router/telemetry.md`](../router/telemetry.md)); everything else still
reads `MockData` because no telemetry source exists for it yet:

- **Cost Analytics' other two charts** — real cumulative-savings time series and per-agent ROI
  instead of `MockData.CostData`/`AgentRoi`. Cumulative savings needs a "worst case" baseline cost
  concept that doesn't exist in `ModelRouteResolver` today (the same gap that keeps per-turn
  `RoutingRoi` at 0 in live conversations); per-agent ROI has no "agent" concept upstream of "which
  model was selected" (see the "Agent = Model" note in `telemetry.md`).
- **Model Distribution** — real `TokenBucket`/`ModelShare` data. The Day/Month/3-Month/6-Month/Year
  time-range filter bar and the From/To date inputs are currently **cosmetic only** — they don't
  refilter the charts — and only become meaningful once there's real time-series data to filter.
- **Governance** — real per-provider budget/spend data. The Budget Cap input is editable today but
  purely client-side: edits recompute the in-memory utilization/status/bar but aren't persisted
  anywhere and are lost on refresh; needs a real place to write to.
- **Header ticker** (Total Saved / System Tokens / Avg. Cost Reduction) — still three hardcoded
  numbers, not derived from `LiveDataStore.Conversations` at all.
- **Dynamic chart axis ranges** — axis scales (e.g. the $0–$160 savings scale, the 0–6M token
  scale) are currently pinned to fit the mock data's known range; need to become dynamic once real
  data volume varies.
- **Settings modal actions** — Reset Stats / Clear History currently just close the modal with no
  effect; they need real actions once there's real state to reset or clear.
- **Configurable telemetry hub URL** — `Services/LiveDataStore.cs` hardcodes
  `http://localhost:5001/telemetry/hub`; there's no settings UI to point at a
  differently-configured proxy, since the GUI has no settings-persistence mechanism at all yet.

## Recently completed

### ✅ Wire the dashboard to live AgenticRouter proxy telemetry, with real-time push updates

`src/AgenticRouter/Telemetry/` now captures per-request session/turn tracking, OpenAI/Anthropic
token usage (streaming and non-streaming), and estimated cost, and pushes each request as a
`RoutingTelemetryEvent` over a SignalR hub (`/telemetry/hub`) as soon as it's forwarded — no polling.
`AgenticRouter.Gui`'s `Services/LiveDataStore.cs` consumes this live, and the Live Stream tab plus
Cost Analytics' Token Compounding chart now render real conversations instead of `MockData`. Full
pipeline, field-by-field data provenance, and what's still honestly defaulted (Routing ROI, Tool
Steps, Cache Hit Rate, Context Buffer, Request/Response text) vs. real (Time to First Token) is in
[`../router/telemetry.md`](../router/telemetry.md). This closes out both former "Open" headline
items (live wiring and real-time push) in one implementation, since SignalR push was the chosen
transport from the start rather than adding polling first.

### ✅ Token-compounding line chart in Cost Analytics

Implemented in `CostAnalytics.razor`'s new "Token Compounding by Conversation" panel: a
conversation picker (now over live conversations, see the live-telemetry item above) plus a
two-series line chart (cumulative prompt tokens, cumulative completion tokens) per turn, built via
`AgenticRouter.Gui.Charts.TokenCompoundingSeries.Build`.

### ✅ Token-compounding sparkline on the conversation summary card

Implemented in `ConversationSummary.razor`: a compact inline SVG polyline ("Trend" stat) showing
per-turn total tokens, built via `AgenticRouter.Gui.Charts.TokenCompoundingSeries.BuildSparkline`
and `SparklineLayout.Normalize`.

### ✅ Keyboard-accessible tooltips

`wwwroot/js/tooltips.js` now shows/hides on `focusin`/`focusout` (in addition to hover), dismisses
on Escape, and the shared tooltip element is hidden via opacity rather than `display:none` so it
stays in the accessibility tree. Every `data-tip` element not nested inside a `<button>` carries
`tabindex="0"` and `aria-describedby="ls-tooltip"`. The handful nested inside a `<button>` (e.g. a
`TurnCard` header's sub-badges) intentionally skip `tabindex` — nesting a focusable element inside a
button is an ARIA anti-pattern — and the outer button carries a comprehensive `aria-label` instead.
Smoke-tested against a standalone HTML harness with Playwright/Chromium (see `dashboard.md`'s
"Verification limitation" note for why that, rather than a full app build, was the verification
method available in this environment).

## Minor / cosmetic (low priority)

- **Chart tooltips** use ApexCharts' default dark theme (restyled in `app.css` to match card
  styling) rather than a fully custom tooltip matching the original React design pixel-for-pixel.
  `dashboard.md` calls this a "minor visual difference," not a functional gap.
