namespace AgenticRouter.Gui.Utils;

/// <summary>Deterministic display-color assignment for agents shown in the Live Stream tab.</summary>
public static class ColorUtils
{
    private static readonly string[] Palette =
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

    /// <summary>
    /// Maps an agent name to a palette color. Uses FNV-1a rather than string.GetHashCode(),
    /// which is randomized per process and would reshuffle colors on every app launch.
    /// </summary>
    public static string GetColorForAgent(string? agentName)
    {
        if (string.IsNullOrEmpty(agentName))
        {
            return Palette[0];
        }

        var hash = 2166136261u;
        foreach (var c in agentName)
        {
            hash = (hash ^ c) * 16777619u;
        }

        return Palette[(int)(hash % (uint)Palette.Length)];
    }
}
