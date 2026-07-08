using AgenticRouter.Gui.Models;
using AgenticRouter.Gui.Telemetry;

namespace AgenticRouter.Gui.Services;

/// <summary>
/// Maps the live-telemetry aggregation output (<see cref="LiveConversation"/> /
/// <see cref="LiveConversationTurn"/>, produced by <c>AgenticRouter.Gui.Telemetry</c>) onto the
/// dashboard's existing view-model shape (<see cref="Conversation"/> / <see cref="ConversationTurn"/>)
/// so <c>LiveStream</c>, <c>ConversationCard</c>, <c>ConversationSummary</c>, and <c>TurnCard</c> can
/// render live data without knowing it isn't <see cref="MockData"/>.
/// </summary>
/// <remarks>
/// Several <see cref="ConversationTurn"/> fields have no live-data source given the proxy's current
/// telemetry scope and are set to explicit, honest defaults here rather than fabricated:
/// <list type="bullet">
/// <item><see cref="ConversationTurn.RoutingRoi"/> - no "worst case" baseline cost is computed for live
/// requests (defaults to 0).</item>
/// <item><see cref="ConversationTurn.ToolExecutionSteps"/> - the proxy does not introspect tool calls
/// within a turn (defaults to 0).</item>
/// <item><see cref="ConversationTurn.CacheHitRate"/> - prompt-cache usage is not parsed from provider
/// responses (defaults to 0).</item>
/// <item><see cref="ConversationTurn.ContextBufferPercent"/> - no per-model context-window-size
/// configuration exists (defaults to 0).</item>
/// <item><see cref="ConversationTurn.RequestSummary"/> / <see cref="ConversationTurn.ResponseSummary"/> -
/// full payload text is deliberately not relayed over the telemetry channel (defaults to null).</item>
/// </list>
/// <see cref="ConversationTurn.TimeToFirstTokenMs"/> IS real: it is
/// <see cref="LiveConversationTurn.LatencyToHeadersMs"/>, the time from sending the upstream request to
/// receiving response headers. See docs/gui/dashboard.md for the full real-vs-defaulted breakdown.
///
/// This class is intentionally left without a dedicated unit-test project: like the rest of
/// AgenticRouter.Gui (Razor components, MauiProgram, TrayWindowManager), it depends on Windows-only
/// MAUI/Blazor types and cannot be built or tested outside a Windows .NET SDK. The testable logic it
/// wraps - grouping/ordering/summation - lives in and is covered by
/// AgenticRouter.Gui.Telemetry.ConversationAggregator (see ConversationAggregatorTests).
/// </remarks>
public static class LiveConversationMapper
{
    public static Conversation ToModel(LiveConversation conversation)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        return new Conversation(
            Id: conversation.SessionId,
            Title: BuildTitle(conversation),
            FirstTimestamp: FormatTimestamp(conversation.FirstTimestampUtc),
            LastTimestamp: FormatTimestamp(conversation.LastTimestampUtc),
            TotalCost: conversation.TotalCost,
            TotalPromptTokens: conversation.TotalPromptTokens,
            TotalCompletionTokens: conversation.TotalCompletionTokens,
            HasFallbackTurns: conversation.HasFallbackTurns,
            Turns: conversation.Turns.Select(ToModel).ToList());
    }

    private static ConversationTurn ToModel(LiveConversationTurn turn)
    {
        return new ConversationTurn(
            Id: $"{turn.SessionId}-t{turn.TurnNumber}",
            Agent: turn.Agent,
            Model: turn.Model,
            TurnNumber: turn.TurnNumber,
            PromptTokens: turn.PromptTokens,
            CompletionTokens: turn.CompletionTokens,
            RoutingRoi: 0m,
            TotalCost: turn.EstimatedCostUsd,
            ToolExecutionSteps: 0,
            CacheHitRate: 0m,
            TimeToFirstTokenMs: (int)Math.Clamp(turn.LatencyToHeadersMs, 0, int.MaxValue),
            ContextBufferPercent: 0m,
            Timestamp: FormatTimestamp(turn.TimestampUtc),
            RoutingSteps: BuildRoutingSteps(turn),
            RequestSummary: null,
            ResponseSummary: null,
            IsFallback: turn.IsFallback);
    }

    private static IReadOnlyList<RoutingStep> BuildRoutingSteps(LiveConversationTurn turn)
    {
        List<RoutingStep> steps = [];
        if (turn.IsFallback)
        {
            steps.Add(new RoutingStep(StepStatus.Warn, "Fallback routing was used for this turn."));
        }

        steps.Add(new RoutingStep(StepStatus.Info, $"Route Confirmed: {turn.Model}"));
        return steps;
    }

    private static string BuildTitle(LiveConversation conversation) =>
        conversation.IsSessionSynthesized
            ? $"Untracked session ({ShortId(conversation.SessionId)})"
            : $"Session {ShortId(conversation.SessionId)}";

    private static string ShortId(string sessionId) =>
        sessionId.Length <= 8 ? sessionId : sessionId[..8];

    private static string FormatTimestamp(DateTimeOffset timestamp) =>
        timestamp.ToLocalTime().ToString("HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
}
