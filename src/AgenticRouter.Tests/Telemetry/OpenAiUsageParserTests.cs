using AgenticRouter.Telemetry;

namespace AgenticRouter.Tests.Telemetry;

/// <summary>Covers <see cref="OpenAiUsageParser"/>.</summary>
public class OpenAiUsageParserTests
{
    [Fact]
    public void TryExtractFromNonStreamingBody_ValidUsage_ReturnsTrueWithValues()
    {
        const string json = """{"id":"chatcmpl-1","choices":[],"usage":{"prompt_tokens":120,"completion_tokens":45,"total_tokens":165}}""";

        var result = OpenAiUsageParser.TryExtractFromNonStreamingBody(json, out var usage);

        Assert.True(result);
        Assert.Equal(120, usage.PromptTokens);
        Assert.Equal(45, usage.CompletionTokens);
    }

    [Fact]
    public void TryExtractFromNonStreamingBody_MissingUsage_ReturnsFalse()
    {
        const string json = """{"id":"chatcmpl-1","choices":[]}""";

        var result = OpenAiUsageParser.TryExtractFromNonStreamingBody(json, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryExtractFromNonStreamingBody_MalformedJson_ReturnsFalse()
    {
        var result = OpenAiUsageParser.TryExtractFromNonStreamingBody("{ not json", out _);

        Assert.False(result);
    }

    [Fact]
    public void TryExtractFromNonStreamingBody_UsageMissingCompletionTokens_ReturnsFalse()
    {
        const string json = """{"usage":{"prompt_tokens":120}}""";

        var result = OpenAiUsageParser.TryExtractFromNonStreamingBody(json, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryExtractFromStreamingBuffer_FinalChunkWithUsage_ReturnsTrueWithValues()
    {
        var sse =
            "data: {\"id\":\"1\",\"choices\":[{\"delta\":{\"content\":\"Hi\"}}]}\n\n" +
            "data: {\"id\":\"1\",\"choices\":[{\"delta\":{\"content\":\" there\"}}]}\n\n" +
            "data: {\"id\":\"1\",\"choices\":[],\"usage\":{\"prompt_tokens\":80,\"completion_tokens\":12,\"total_tokens\":92}}\n\n" +
            "data: [DONE]\n\n";

        var result = OpenAiUsageParser.TryExtractFromStreamingBuffer(sse, out var usage);

        Assert.True(result);
        Assert.Equal(80, usage.PromptTokens);
        Assert.Equal(12, usage.CompletionTokens);
    }

    [Fact]
    public void TryExtractFromStreamingBuffer_NoUsageAnywhere_ReturnsFalse()
    {
        // The common case: client didn't request stream_options.include_usage, so OpenAI never sends usage.
        var sse =
            "data: {\"id\":\"1\",\"choices\":[{\"delta\":{\"content\":\"Hi\"}}]}\n\n" +
            "data: [DONE]\n\n";

        var result = OpenAiUsageParser.TryExtractFromStreamingBuffer(sse, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryExtractFromStreamingBuffer_MultipleUsageChunks_TakesTheLastOne()
    {
        var sse =
            "data: {\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":1}}\n\n" +
            "data: {\"usage\":{\"prompt_tokens\":10,\"completion_tokens\":99}}\n\n" +
            "data: [DONE]\n\n";

        var result = OpenAiUsageParser.TryExtractFromStreamingBuffer(sse, out var usage);

        Assert.True(result);
        Assert.Equal(99, usage.CompletionTokens);
    }

    [Fact]
    public void TryExtractFromStreamingBuffer_EmptyBuffer_ReturnsFalse()
    {
        var result = OpenAiUsageParser.TryExtractFromStreamingBuffer(string.Empty, out _);

        Assert.False(result);
    }
}
