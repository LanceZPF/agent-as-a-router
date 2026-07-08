using AgenticRouter.Gui.Charts;

namespace AgenticRouter.Gui.Charts.Tests;

/// <summary>
/// Covers <see cref="TokenCompoundingSeries"/>: the cumulative "hockey stick" series for the Cost
/// Analytics chart and the compact per-turn series for the conversation summary sparkline.
/// </summary>
public class TokenCompoundingSeriesTests
{
    [Fact]
    public void Build_NullTurns_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => TokenCompoundingSeries.Build(null!));
    }

    [Fact]
    public void Build_EmptyList_ReturnsEmpty()
    {
        var result = TokenCompoundingSeries.Build([]);

        Assert.Empty(result);
    }

    [Fact]
    public void Build_SingleTurn_ReturnsThatTurnsTotalsAsCumulative()
    {
        var result = TokenCompoundingSeries.Build([new TurnTokenPoint(1, 100, 40)]);

        var point = Assert.Single(result);
        Assert.Equal(1, point.TurnNumber);
        Assert.Equal(100, point.CumulativePromptTokens);
        Assert.Equal(40, point.CumulativeCompletionTokens);
        Assert.Equal(140, point.CumulativeTotalTokens);
    }

    [Fact]
    public void Build_MultipleTurnsInOrder_AccumulatesCorrectly()
    {
        IReadOnlyList<TurnTokenPoint> turns =
        [
            new TurnTokenPoint(1, 2104, 891),
            new TurnTokenPoint(2, 3240, 1205),
            new TurnTokenPoint(3, 4567, 1798),
        ];

        var result = TokenCompoundingSeries.Build(turns);

        Assert.Equal(3, result.Count);

        Assert.Equal(1, result[0].TurnNumber);
        Assert.Equal(2104, result[0].CumulativePromptTokens);
        Assert.Equal(891, result[0].CumulativeCompletionTokens);

        Assert.Equal(2, result[1].TurnNumber);
        Assert.Equal(2104 + 3240, result[1].CumulativePromptTokens);
        Assert.Equal(891 + 1205, result[1].CumulativeCompletionTokens);

        Assert.Equal(3, result[2].TurnNumber);
        Assert.Equal(2104 + 3240 + 4567, result[2].CumulativePromptTokens);
        Assert.Equal(891 + 1205 + 1798, result[2].CumulativeCompletionTokens);
        Assert.Equal(result[2].CumulativePromptTokens + result[2].CumulativeCompletionTokens, result[2].CumulativeTotalTokens);
    }

    [Fact]
    public void Build_UnsortedInput_IsSortedByTurnNumberBeforeAccumulating()
    {
        IReadOnlyList<TurnTokenPoint> turns =
        [
            new TurnTokenPoint(2, 300, 30),
            new TurnTokenPoint(1, 100, 10),
            new TurnTokenPoint(3, 500, 50),
        ];

        var result = TokenCompoundingSeries.Build(turns);

        Assert.Equal([1, 2, 3], result.Select(p => p.TurnNumber));
        // Turn 1 must be accumulated first regardless of input order, or turn 2's cumulative would be wrong.
        Assert.Equal(100, result[0].CumulativePromptTokens);
        Assert.Equal(100 + 300, result[1].CumulativePromptTokens);
        Assert.Equal(100 + 300 + 500, result[2].CumulativePromptTokens);
    }

    [Fact]
    public void Build_ZeroCompletionTokens_StillAccumulatesPrompt()
    {
        // Mirrors a real mock turn: a final summary-only turn with 0 completion tokens.
        IReadOnlyList<TurnTokenPoint> turns =
        [
            new TurnTokenPoint(1, 1000, 200),
            new TurnTokenPoint(2, 500, 0),
        ];

        var result = TokenCompoundingSeries.Build(turns);

        Assert.Equal(1500, result[1].CumulativePromptTokens);
        Assert.Equal(200, result[1].CumulativeCompletionTokens);
    }

    [Fact]
    public void BuildSparkline_NullTurns_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => TokenCompoundingSeries.BuildSparkline(null!));
    }

    [Fact]
    public void BuildSparkline_EmptyList_ReturnsEmpty()
    {
        var result = TokenCompoundingSeries.BuildSparkline([]);

        Assert.Empty(result);
    }

    [Fact]
    public void BuildSparkline_ReturnsPerTurnTotalsNotCumulative()
    {
        IReadOnlyList<TurnTokenPoint> turns =
        [
            new TurnTokenPoint(1, 100, 20),
            new TurnTokenPoint(2, 150, 25),
        ];

        var result = TokenCompoundingSeries.BuildSparkline(turns);

        // Per-turn totals (120, 175), not cumulative (120, 295) - a sparkline shows the trend shape,
        // not a running sum.
        Assert.Equal([120, 175], result);
    }

    [Fact]
    public void BuildSparkline_UnsortedInput_IsSortedByTurnNumber()
    {
        IReadOnlyList<TurnTokenPoint> turns =
        [
            new TurnTokenPoint(3, 50, 10),
            new TurnTokenPoint(1, 100, 20),
            new TurnTokenPoint(2, 150, 25),
        ];

        var result = TokenCompoundingSeries.BuildSparkline(turns);

        Assert.Equal([120, 175, 60], result);
    }
}
