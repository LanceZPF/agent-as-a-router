namespace AgenticRouter.Gui.Telemetry;

/// <summary>One turn (routed request) within a live-aggregated conversation.</summary>
public sealed record LiveConversationTurn(
    string SessionId,
    int TurnNumber,
    string Agent,
    string Model,
    int PromptTokens,
    int CompletionTokens,
    decimal EstimatedCostUsd,
    bool IsFallback,
    long LatencyToHeadersMs,
    DateTimeOffset TimestampUtc);

/// <summary>A conversation (session) reconstructed from the live telemetry stream.</summary>
public sealed record LiveConversation(
    string SessionId,
    bool IsSessionSynthesized,
    DateTimeOffset FirstTimestampUtc,
    DateTimeOffset LastTimestampUtc,
    decimal TotalCost,
    int TotalPromptTokens,
    int TotalCompletionTokens,
    bool HasFallbackTurns,
    IReadOnlyList<LiveConversationTurn> Turns);

/// <summary>
/// Groups a flat stream of <see cref="RoutingTelemetryEventDto"/>s into conversations, mirroring the
/// shape <c>Models.Conversation</c>/<c>ConversationTurn</c> expect on the Gui side (see the mapping
/// layer in <c>AgenticRouter.Gui</c> that converts this pure output into those view-model types).
/// Pure and stateless: callers own how/when to re-run it as new events arrive (e.g. re-aggregate the
/// full accumulated event list on every new event - see the "Verification limitation" and
/// "Known gaps" notes on this feature in docs/gui/dashboard.md for why several
/// <c>ConversationTurn</c> fields - RoutingRoi, ToolExecutionSteps, CacheHitRate,
/// ContextBufferPercent, RequestSummary/ResponseSummary, RoutingSteps - have no live-data source yet
/// and are left at safe defaults by the Gui-side mapping layer, not by this aggregator).
/// </summary>
public static class ConversationAggregator
{
    /// <summary>
    /// Groups events by <see cref="RoutingTelemetryEventDto.SessionId"/>, orders each conversation's
    /// turns by <see cref="RoutingTelemetryEventDto.TurnNumber"/> (turn numbers are assigned server-side
    /// in arrival order, so this is also chronological order), and orders conversations by most
    /// recently active first.
    /// </summary>
    public static IReadOnlyList<LiveConversation> Aggregate(IReadOnlyList<RoutingTelemetryEventDto> events)
    {
        ArgumentNullException.ThrowIfNull(events);

        return events
            .GroupBy(e => e.SessionId, StringComparer.Ordinal)
            .Select(BuildConversation)
            .OrderByDescending(c => c.LastTimestampUtc)
            .ToList();
    }

    private static LiveConversation BuildConversation(IGrouping<string, RoutingTelemetryEventDto> group)
    {
        var orderedEvents = group.OrderBy(e => e.TurnNumber).ToList();

        var turns = orderedEvents
            .Select(e => new LiveConversationTurn(
                SessionId: e.SessionId,
                TurnNumber: e.TurnNumber,
                Agent: e.ResolvedModel,
                Model: e.ResolvedModel,
                PromptTokens: e.PromptTokens ?? 0,
                CompletionTokens: e.CompletionTokens ?? 0,
                EstimatedCostUsd: e.EstimatedCostUsd ?? 0m,
                IsFallback: e.IsFallback,
                LatencyToHeadersMs: e.LatencyToHeadersMs,
                TimestampUtc: e.TimestampUtc))
            .ToList();

        return new LiveConversation(
            SessionId: group.Key,
            IsSessionSynthesized: orderedEvents[0].IsSessionSynthesized,
            FirstTimestampUtc: orderedEvents[0].TimestampUtc,
            LastTimestampUtc: orderedEvents[^1].TimestampUtc,
            TotalCost: turns.Sum(t => t.EstimatedCostUsd),
            TotalPromptTokens: turns.Sum(t => t.PromptTokens),
            TotalCompletionTokens: turns.Sum(t => t.CompletionTokens),
            HasFallbackTurns: turns.Any(t => t.IsFallback),
            Turns: turns);
    }
}
