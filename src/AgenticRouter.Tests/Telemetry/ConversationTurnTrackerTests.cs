using AgenticRouter.Telemetry;

namespace AgenticRouter.Tests.Telemetry;

/// <summary>Covers <see cref="ConversationTurnTracker"/>.</summary>
public class ConversationTurnTrackerTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NextTurn_NullOrWhitespaceSessionId_Throws(string? sessionId)
    {
        var tracker = new ConversationTurnTracker();

        Assert.Throws<ArgumentException>(() => tracker.NextTurn(sessionId!));
    }

    [Fact]
    public void NextTurn_FirstCallForSession_ReturnsOne()
    {
        var tracker = new ConversationTurnTracker();

        var turn = tracker.NextTurn("sess-1");

        Assert.Equal(1, turn);
    }

    [Fact]
    public void NextTurn_SubsequentCallsForSameSession_IncrementSequentially()
    {
        var tracker = new ConversationTurnTracker();

        Assert.Equal(1, tracker.NextTurn("sess-1"));
        Assert.Equal(2, tracker.NextTurn("sess-1"));
        Assert.Equal(3, tracker.NextTurn("sess-1"));
    }

    [Fact]
    public void NextTurn_DifferentSessions_HaveIndependentCounters()
    {
        var tracker = new ConversationTurnTracker();

        Assert.Equal(1, tracker.NextTurn("sess-a"));
        Assert.Equal(1, tracker.NextTurn("sess-b"));
        Assert.Equal(2, tracker.NextTurn("sess-a"));
        Assert.Equal(2, tracker.NextTurn("sess-b"));
    }

    [Fact]
    public void NextTurn_ConcurrentCallsForSameSession_ProduceAGaplessUniqueSequence()
    {
        var tracker = new ConversationTurnTracker();
        const int callCount = 200;

        var tasks = Enumerable.Range(0, callCount)
            .Select(_ => Task.Run(() => tracker.NextTurn("sess-concurrent")))
            .ToArray();

        Task.WaitAll(tasks);

        var results = tasks.Select(t => t.Result).OrderBy(v => v).ToArray();
        var expected = Enumerable.Range(1, callCount).ToArray();
        Assert.Equal(expected, results);
    }
}
