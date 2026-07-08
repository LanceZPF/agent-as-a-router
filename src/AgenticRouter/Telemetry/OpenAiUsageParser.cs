using System.Text.Json.Nodes;

namespace AgenticRouter.Telemetry;

/// <summary>
/// Extracts token usage from OpenAI-compatible chat completion responses.
/// </summary>
public static class OpenAiUsageParser
{
    /// <summary>
    /// Extracts usage from a non-streaming <c>chat.completion</c> JSON body's top-level
    /// <c>usage.prompt_tokens</c>/<c>usage.completion_tokens</c> fields.
    /// </summary>
    public static bool TryExtractFromNonStreamingBody(string json, out UsageInfo usage)
    {
        usage = default;

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json);
        }
        catch (System.Text.Json.JsonException)
        {
            return false;
        }

        return node is JsonObject obj && TryExtractFromUsageContainer(obj, out usage);
    }

    /// <summary>
    /// Extracts usage from a buffered SSE stream. OpenAI only includes a <c>usage</c> object in a
    /// streaming response when the client requested <c>stream_options.include_usage = true</c>; when
    /// present, it appears once, in the final content chunk (with an empty <c>choices</c> array),
    /// before the <c>[DONE]</c> sentinel. Scans every event and keeps the last usable one, so a
    /// mid-stream event that happens to carry a (rare, non-final) usage field doesn't win over the
    /// real final one.
    /// </summary>
    public static bool TryExtractFromStreamingBuffer(string sseText, out UsageInfo usage)
    {
        usage = default;
        var found = false;

        foreach (var evt in SseEventReader.ReadDataEvents(sseText))
        {
            if (TryExtractFromUsageContainer(evt, out var candidate))
            {
                usage = candidate;
                found = true;
            }
        }

        return found;
    }

    private static bool TryExtractFromUsageContainer(JsonObject container, out UsageInfo usage)
    {
        usage = default;

        if (container["usage"] is not JsonObject usageObj)
        {
            return false;
        }

        if (!TryGetInt(usageObj, "prompt_tokens", out var promptTokens) ||
            !TryGetInt(usageObj, "completion_tokens", out var completionTokens))
        {
            return false;
        }

        usage = new UsageInfo(promptTokens, completionTokens);
        return true;
    }

    private static bool TryGetInt(JsonObject obj, string propertyName, out int value)
    {
        value = 0;
        return obj[propertyName] is JsonValue jsonValue && jsonValue.TryGetValue(out value);
    }
}
