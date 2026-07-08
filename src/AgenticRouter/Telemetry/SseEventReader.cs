using System.Text.Json.Nodes;

namespace AgenticRouter.Telemetry;

/// <summary>
/// Minimal Server-Sent-Events scanner: yields the parsed JSON payload of each <c>data:</c> line in
/// an SSE stream. Used to pull usage information out of an already-fully-buffered streaming
/// response (see the "single-shot parse after buffering" note on <see cref="IUsageExtractor"/>) -
/// this is not an incremental/live SSE parser.
/// </summary>
public static class SseEventReader
{
    private const string DataPrefix = "data:";
    private const string DoneSentinel = "[DONE]";

    /// <summary>
    /// Enumerates the parsed JSON payload of every well-formed <c>data:</c> line in
    /// <paramref name="sseText"/>, in stream order. Lines that are the <c>[DONE]</c> sentinel, blank,
    /// or not valid JSON are silently skipped rather than throwing - a malformed or truncated buffer
    /// (e.g. the capture cap was hit mid-stream) should degrade to "fewer usable events," not a crash.
    /// </summary>
    public static IEnumerable<JsonObject> ReadDataEvents(string sseText)
    {
        ArgumentNullException.ThrowIfNull(sseText);

        foreach (var rawLine in sseText.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r').AsSpan().Trim();
            if (!line.StartsWith(DataPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            var payload = line[DataPrefix.Length..].Trim();
            if (payload.IsEmpty || payload.SequenceEqual(DoneSentinel))
            {
                continue;
            }

            JsonNode? node;
            try
            {
                node = JsonNode.Parse(payload.ToString());
            }
            catch (System.Text.Json.JsonException)
            {
                continue;
            }

            if (node is JsonObject obj)
            {
                yield return obj;
            }
        }
    }
}
