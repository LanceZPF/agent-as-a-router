namespace AgenticRouter.Gui.Charts;

/// <summary>
/// Normalizes a series of numeric values into 2D points scaled to fit a rectangular viewBox, for
/// rendering as an inline SVG polyline sparkline. Pure and stateless.
/// </summary>
public static class SparklineLayout
{
    /// <summary>
    /// Normalizes <paramref name="values"/> into points spanning the full width, with the smallest
    /// value at the bottom and the largest at the top (SVG's Y axis grows downward, so the largest
    /// value gets the smallest Y).
    /// </summary>
    /// <param name="values">The series to plot, in display order (not sorted by this method).</param>
    /// <param name="width">The viewBox width in SVG user units.</param>
    /// <param name="height">The viewBox height in SVG user units.</param>
    /// <param name="padding">Vertical padding reserved at the top and bottom so peaks/troughs aren't clipped.</param>
    /// <returns>
    /// One (X, Y) point per input value, evenly spaced across <paramref name="width"/>. Empty input
    /// yields an empty result. A single value yields two points (left and right edge, same Y) so a
    /// flat line still renders instead of nothing. When every value is equal, all points sit on the
    /// vertical midline.
    /// </returns>
    public static IReadOnlyList<(double X, double Y)> Normalize(
        IReadOnlyList<int> values, double width, double height, double padding = 1)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);
        ArgumentOutOfRangeException.ThrowIfNegative(padding);

        if (values.Count == 0)
        {
            return [];
        }

        if (values.Count == 1)
        {
            var midY = height / 2;
            return [(0, midY), (width, midY)];
        }

        var min = values.Min();
        var max = values.Max();
        var range = max - min;
        var usableHeight = height - (padding * 2);

        var points = new List<(double X, double Y)>(values.Count);
        for (var i = 0; i < values.Count; i++)
        {
            var x = (double)i / (values.Count - 1) * width;
            var normalized = range == 0 ? 0.5 : (double)(values[i] - min) / range;
            var y = height - padding - (normalized * usableHeight);
            points.Add((x, y));
        }

        return points;
    }
}
