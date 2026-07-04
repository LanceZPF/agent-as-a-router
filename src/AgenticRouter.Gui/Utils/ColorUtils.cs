namespace AgenticRouter.Gui.Utils;

/// <summary>Utility for generating deterministic colors by agent name.</summary>
public static class ColorUtils
{
    private static readonly string[] ColorPalette =
    [
        "#10b981", // emerald
        "#38bdf8", // cyan
        "#818cf8", // indigo
        "#fb7185", // rose
        "#f59e0b", // amber
        "#a78bfa", // purple
        "#14b8a6", // teal
        "#0ea5e9", // sky
        "#6366f1", // indigo-2
        "#ec4899", // pink
        "#f97316", // orange
        "#06b6d4", // cyan-2
    ];

    /// <summary>Generate a deterministic color for an agent name.</summary>
    public static string GetColorForAgent(string agentName)
    {
        if (string.IsNullOrEmpty(agentName))
            return ColorPalette[0];

        var hash = agentName.GetHashCode();
        var colorIndex = Math.Abs(hash) % ColorPalette.Length;
        return ColorPalette[colorIndex];
    }
}
