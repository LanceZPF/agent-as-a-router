using AgenticRouter.Gui.Charts;

namespace AgenticRouter.Gui.Charts.Tests;

/// <summary>Covers <see cref="SparklineLayout"/>: the coordinate math behind the mini token-growth chart.</summary>
public class SparklineLayoutTests
{
    [Fact]
    public void Normalize_NullValues_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => SparklineLayout.Normalize(null!, 100, 24));
    }

    [Theory]
    [InlineData(0, 24)]
    [InlineData(-10, 24)]
    [InlineData(100, 0)]
    [InlineData(100, -5)]
    public void Normalize_NonPositiveDimensions_Throws(double width, double height)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SparklineLayout.Normalize([1, 2, 3], width, height));
    }

    [Fact]
    public void Normalize_NegativePadding_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SparklineLayout.Normalize([1, 2, 3], 100, 24, padding: -1));
    }

    [Fact]
    public void Normalize_EmptyValues_ReturnsEmpty()
    {
        var result = SparklineLayout.Normalize([], 100, 24);

        Assert.Empty(result);
    }

    [Fact]
    public void Normalize_SingleValue_ReturnsFlatLineAcrossFullWidth()
    {
        var result = SparklineLayout.Normalize([42], 100, 24);

        Assert.Equal(2, result.Count);
        Assert.Equal((0, 12), result[0]);
        Assert.Equal((100, 12), result[1]);
    }

    [Fact]
    public void Normalize_AllEqualValues_PlotsFlatMidline()
    {
        var result = SparklineLayout.Normalize([5, 5, 5], 100, 24);

        Assert.All(result, p => Assert.Equal(12, p.Y));
    }

    [Fact]
    public void Normalize_IncreasingValues_LargestValueHasSmallestY()
    {
        // SVG's Y axis grows downward, so the largest value (drawn at the top of the sparkline)
        // must have the smallest Y coordinate, and the smallest value the largest Y coordinate.
        var result = SparklineLayout.Normalize([10, 20, 30], 100, 24, padding: 1);

        Assert.Equal(23, result[0].Y); // smallest value (10) -> bottom (height - padding)
        Assert.Equal(1, result[2].Y);  // largest value (30) -> top (padding)
        Assert.True(result[0].Y > result[1].Y);
        Assert.True(result[1].Y > result[2].Y);
    }

    [Fact]
    public void Normalize_PointsAreEvenlySpacedAcrossFullWidth()
    {
        var result = SparklineLayout.Normalize([1, 2, 3, 4, 5], 100, 24);

        Assert.Equal(0, result[0].X);
        Assert.Equal(25, result[1].X);
        Assert.Equal(50, result[2].X);
        Assert.Equal(75, result[3].X);
        Assert.Equal(100, result[4].X);
    }

    [Fact]
    public void Normalize_RespectsCustomPadding()
    {
        var result = SparklineLayout.Normalize([0, 100], 100, 24, padding: 4);

        Assert.Equal(20, result[0].Y); // smallest value -> height - padding
        Assert.Equal(4, result[1].Y);  // largest value -> padding
    }
}
