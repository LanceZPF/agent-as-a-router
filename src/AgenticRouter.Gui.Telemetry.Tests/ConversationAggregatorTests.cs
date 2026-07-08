using AgenticRouter.Gui.Telemetry;

namespace AgenticRouter.Gui.Telemetry.Tests;

/// <summary>Covers <see cref="ConversationAggregator.Aggregate"/>: grouping, ordering, and totals.</summary>
public class ConversationAggregatorTests
{
    private static readonly DateTimeOffset BaseTime = new(2026, 7, 8, 12, 0, 0, TimeSpan.Zero);

    private static RoutingTelemetryEventDto CreateEvent(
        string sessionId = "session-1",
        int turnNumber = 1,
        bool isSessionSynthesized = false,
        string requestedModel = "gpt-4o",
        string resolvedModel = "gpt-4o-mini",
        string provider = "openai",
        bool isFallback = false,
        int? promptTokens = 100,
        int? completionTokens = 50,
        decimal? estimatedCostUsd = 0.01m,
        bool isStreaming = false,
        long latencyToHeadersMs = 120,
        long totalDurationMs = 400,
        int statusCode = 200,
        DateTimeOffset? timestampUtc = null)
    {
        return new RoutingTelemetryEventDto(
            SessionId: sessionId,
            TurnNumber: turnNumber,
            IsSessionSynthesized: isSessionSynthesized,
            RequestedModel: requestedModel,
            ResolvedModel: resolvedModel,
            Provider: provider,
            IsFallback: isFallback,
            PromptTokens: promptTokens,
            CompletionTokens: completionTokens,
            EstimatedCostUsd: estimatedCostUsd,
            IsStreaming: isStreaming,
            LatencyToHeadersMs: latencyToHeadersMs,
            TotalDurationMs: totalDurationMs,
            StatusCode: statusCode,
            TimestampUtc: timestampUtc ?? BaseTime.AddMinutes(turnNumber));
    }

    [Fact]
    public void Aggregate_NullEvents_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ConversationAggregator.Aggregate(null!));
    }

    [Fact]
    public void Aggregate_EmptyList_ReturnsEmpty()
    {
        var result = ConversationAggregator.Aggregate([]);

        Assert.Empty(result);
    }

    [Fact]
    public void Aggregate_SingleEvent_ReturnsSingleOneTurnConversation()
    {
        var events = new[] { CreateEvent(sessionId: "session-1", turnNumber: 1) };

        var result = ConversationAggregator.Aggregate(events);

        var conversation = Assert.Single(result);
        Assert.Equal("session-1", conversation.SessionId);
        var turn = Assert.Single(conversation.Turns);
        Assert.Equal(1, turn.TurnNumber);
    }

    [Fact]
    public void Aggregate_MultipleEventsSameSession_GroupsOrdersAndSumsCorrectly()
    {
        var events = new[]
        {
            CreateEvent(sessionId: "session-1", turnNumber: 1, promptTokens: 100, completionTokens: 20, estimatedCostUsd: 0.01m),
            CreateEvent(sessionId: "session-1", turnNumber: 2, promptTokens: 150, completionTokens: 30, estimatedCostUsd: 0.02m),
            CreateEvent(sessionId: "session-1", turnNumber: 3, promptTokens: 200, completionTokens: 40, estimatedCostUsd: 0.03m),
        };

        var result = ConversationAggregator.Aggregate(events);

        var conversation = Assert.Single(result);
        Assert.Equal("session-1", conversation.SessionId);
        Assert.Equal(3, conversation.Turns.Count);
        Assert.Equal([1, 2, 3], conversation.Turns.Select(t => t.TurnNumber));
        Assert.Equal(450, conversation.TotalPromptTokens);
        Assert.Equal(90, conversation.TotalCompletionTokens);
        Assert.Equal(0.06m, conversation.TotalCost);
    }

    [Fact]
    public void Aggregate_MultipleSessions_ReturnsIndependentConversationsOrderedByLastTimestampDescending()
    {
        var events = new[]
        {
            CreateEvent(sessionId: "older", turnNumber: 1, timestampUtc: BaseTime),
            CreateEvent(sessionId: "newer", turnNumber: 1, timestampUtc: BaseTime.AddHours(1)),
            CreateEvent(sessionId: "middle", turnNumber: 1, timestampUtc: BaseTime.AddMinutes(30)),
        };

        var result = ConversationAggregator.Aggregate(events);

        Assert.Equal(["newer", "middle", "older"], result.Select(c => c.SessionId));
    }

    [Fact]
    public void Aggregate_UnsortedByTurnNumberInput_StillProducesCorrectlyOrderedTurns()
    {
        var events = new[]
        {
            CreateEvent(sessionId: "session-1", turnNumber: 3, timestampUtc: BaseTime.AddMinutes(3)),
            CreateEvent(sessionId: "session-1", turnNumber: 1, timestampUtc: BaseTime.AddMinutes(1)),
            CreateEvent(sessionId: "session-1", turnNumber: 2, timestampUtc: BaseTime.AddMinutes(2)),
        };

        var result = ConversationAggregator.Aggregate(events);

        var conversation = Assert.Single(result);
        Assert.Equal([1, 2, 3], conversation.Turns.Select(t => t.TurnNumber));
        Assert.Equal(BaseTime.AddMinutes(1), conversation.FirstTimestampUtc);
        Assert.Equal(BaseTime.AddMinutes(3), conversation.LastTimestampUtc);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Aggregate_IsSessionSynthesized_PropagatesFromFirstTurn(bool isSynthesized)
    {
        var events = new[] { CreateEvent(sessionId: "session-1", turnNumber: 1, isSessionSynthesized: isSynthesized) };

        var result = ConversationAggregator.Aggregate(events);

        Assert.Equal(isSynthesized, result[0].IsSessionSynthesized);
    }

    [Fact]
    public void Aggregate_NoFallbackTurns_HasFallbackTurnsIsFalse()
    {
        var events = new[]
        {
            CreateEvent(sessionId: "session-1", turnNumber: 1, isFallback: false),
            CreateEvent(sessionId: "session-1", turnNumber: 2, isFallback: false),
        };

        var result = ConversationAggregator.Aggregate(events);

        Assert.False(result[0].HasFallbackTurns);
    }

    [Fact]
    public void Aggregate_OneFallbackTurnAmongMany_HasFallbackTurnsIsTrue()
    {
        var events = new[]
        {
            CreateEvent(sessionId: "session-1", turnNumber: 1, isFallback: false),
            CreateEvent(sessionId: "session-1", turnNumber: 2, isFallback: true),
        };

        var result = ConversationAggregator.Aggregate(events);

        Assert.True(result[0].HasFallbackTurns);
    }

    [Fact]
    public void Aggregate_NullTokenAndCostFields_TreatedAsZeroInTotalsAndTurns()
    {
        var events = new[]
        {
            CreateEvent(sessionId: "session-1", turnNumber: 1, promptTokens: null, completionTokens: null, estimatedCostUsd: null),
        };

        var result = ConversationAggregator.Aggregate(events);

        var conversation = Assert.Single(result);
        Assert.Equal(0, conversation.TotalPromptTokens);
        Assert.Equal(0, conversation.TotalCompletionTokens);
        Assert.Equal(0m, conversation.TotalCost);
        var turn = Assert.Single(conversation.Turns);
        Assert.Equal(0, turn.PromptTokens);
        Assert.Equal(0, turn.CompletionTokens);
        Assert.Equal(0m, turn.EstimatedCostUsd);
    }

    [Fact]
    public void Aggregate_TurnAgentAndModel_BothSetToResolvedModel()
    {
        var events = new[] { CreateEvent(sessionId: "session-1", turnNumber: 1, resolvedModel: "claude-sonnet-5") };

        var result = ConversationAggregator.Aggregate(events);

        var turn = Assert.Single(result[0].Turns);
        Assert.Equal("claude-sonnet-5", turn.Agent);
        Assert.Equal("claude-sonnet-5", turn.Model);
    }
}
