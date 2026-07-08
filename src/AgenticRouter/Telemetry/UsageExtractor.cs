using System.Text;

namespace AgenticRouter.Telemetry;

/// <summary>
/// Extracts token usage from an already-fully-buffered upstream response body, dispatching to the
/// right provider-specific parser. This is a single-shot parse over a captured byte buffer (see
/// <c>ProxyMiddleware</c>'s response tap), not an incremental/live SSE parser - simpler and lower-risk
/// than parsing each chunk as it arrives, at the cost of only knowing usage once the response (or the
/// capture cap) is reached.
/// </summary>
public interface IUsageExtractor
{
    /// <summary>
    /// Attempts to extract token usage for a completed request.
    /// </summary>
    /// <param name="provider">The provider key the request was routed to (e.g. <c>"openai"</c>, <c>"anthropic"</c>).</param>
    /// <param name="isStreaming">Whether the response was a streaming (SSE) response.</param>
    /// <param name="bufferedResponseBody">The captured response bytes (see the capture-cap note on the caller - may be truncated for very large responses).</param>
    /// <param name="usage">The extracted usage, when this method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if usage could be determined; otherwise <see langword="false"/> (unknown provider, malformed/truncated body, or the provider's response genuinely omitted usage).</returns>
    bool TryExtractUsage(string provider, bool isStreaming, ReadOnlyMemory<byte> bufferedResponseBody, out UsageInfo usage);
}

/// <inheritdoc cref="IUsageExtractor" />
public sealed class UsageExtractor : IUsageExtractor
{
    /// <inheritdoc />
    public bool TryExtractUsage(string provider, bool isStreaming, ReadOnlyMemory<byte> bufferedResponseBody, out UsageInfo usage)
    {
        usage = default;

        if (bufferedResponseBody.IsEmpty)
        {
            return false;
        }

        string text;
        try
        {
            text = Encoding.UTF8.GetString(bufferedResponseBody.Span);
        }
        catch (DecoderFallbackException)
        {
            return false;
        }

        return provider.ToLowerInvariant() switch
        {
            "openai" => isStreaming
                ? OpenAiUsageParser.TryExtractFromStreamingBuffer(text, out usage)
                : OpenAiUsageParser.TryExtractFromNonStreamingBody(text, out usage),
            "anthropic" => isStreaming
                ? AnthropicUsageParser.TryExtractFromStreamingBuffer(text, out usage)
                : AnthropicUsageParser.TryExtractFromNonStreamingBody(text, out usage),
            // Unknown/unsupported provider (e.g. alibaba, zhipu, moonshot, minimax): no parser wired
            // up yet. Fail gracefully rather than guessing at an unverified response shape.
            _ => false,
        };
    }
}
