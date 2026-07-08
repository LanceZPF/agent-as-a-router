namespace AgenticRouter.Telemetry;

/// <summary>
/// One completed routed request, broadcast to connected dashboards over <see cref="TelemetryHub"/>.
/// This is the live counterpart to the GUI's mock <c>ConversationTurn</c> shape - see
/// <c>docs/gui/backlog.md</c> for the mapping decisions (e.g. "Agent" is the selected model, since
/// the router has no separate agent concept).
/// </summary>
/// <param name="SessionId">
/// The resolved (or synthesized, when none could be resolved - see <paramref name="IsSessionSynthesized"/>) session id.
/// </param>
/// <param name="TurnNumber">1-based position of this request within its session.</param>
/// <param name="IsSessionSynthesized">
/// <see langword="true"/> when no real session id was found in the request (see <see cref="ISessionIdResolver"/>)
/// and a fresh single-use id was generated instead, so this event is its own 1-turn "conversation."
/// </param>
/// <param name="RequestedModel">The client-facing model name from the request body.</param>
/// <param name="ResolvedModel">The upstream provider's model id the request was actually forwarded as.</param>
/// <param name="Provider">The provider key the request was routed to.</param>
/// <param name="IsFallback">Whether this request was served by fallback routing.</param>
/// <param name="PromptTokens">Extracted prompt/input token count, or <see langword="null"/> if usage couldn't be determined.</param>
/// <param name="CompletionTokens">Extracted completion/output token count, or <see langword="null"/> if usage couldn't be determined.</param>
/// <param name="EstimatedCostUsd">Estimated USD cost from <see cref="PricingOptions"/>, or <see langword="null"/> if usage or pricing is unavailable for this model.</param>
/// <param name="IsStreaming">Whether the response was a streaming (SSE) response.</param>
/// <param name="LatencyToHeadersMs">Milliseconds from sending the upstream request to receiving its response headers.</param>
/// <param name="TotalDurationMs">Milliseconds from sending the upstream request to the response body finishing.</param>
/// <param name="StatusCode">The upstream response's HTTP status code.</param>
/// <param name="TimestampUtc">When the request was routed.</param>
public sealed record RoutingTelemetryEvent(
    string SessionId,
    int TurnNumber,
    bool IsSessionSynthesized,
    string RequestedModel,
    string ResolvedModel,
    string Provider,
    bool IsFallback,
    int? PromptTokens,
    int? CompletionTokens,
    decimal? EstimatedCostUsd,
    bool IsStreaming,
    long LatencyToHeadersMs,
    long TotalDurationMs,
    int StatusCode,
    DateTimeOffset TimestampUtc);
