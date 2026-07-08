namespace AgenticRouter.Gui.Telemetry;

/// <summary>
/// One routed request, received live over the <c>/telemetry/hub</c> SignalR hub. Property names and
/// shape intentionally mirror <c>AgenticRouter.Telemetry.RoutingTelemetryEvent</c> on the proxy side
/// exactly (SignalR's default JSON protocol matches by name, case-insensitively via its camelCase
/// wire convention) - the two types are independent (this project doesn't reference the proxy
/// project; see the csproj remarks) but must be kept in sync by hand if either side changes.
/// </summary>
public sealed record RoutingTelemetryEventDto(
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
