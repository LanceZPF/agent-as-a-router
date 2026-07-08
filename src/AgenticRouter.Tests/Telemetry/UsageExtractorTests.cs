using System.Text;
using AgenticRouter.Telemetry;

namespace AgenticRouter.Tests.Telemetry;

/// <summary>Covers <see cref="UsageExtractor"/>'s provider dispatch.</summary>
public class UsageExtractorTests
{
    private readonly UsageExtractor _extractor = new();

    [Fact]
    public void TryExtractUsage_OpenAiProvider_NonStreaming_DispatchesToOpenAiParser()
    {
        var body = Encoding.UTF8.GetBytes("""{"usage":{"prompt_tokens":10,"completion_tokens":5}}""");

        var result = _extractor.TryExtractUsage("openai", isStreaming: false, body, out var usage);

        Assert.True(result);
        Assert.Equal(10, usage.PromptTokens);
        Assert.Equal(5, usage.CompletionTokens);
    }

    [Fact]
    public void TryExtractUsage_AnthropicProvider_NonStreaming_DispatchesToAnthropicParser()
    {
        var body = Encoding.UTF8.GetBytes("""{"usage":{"input_tokens":20,"output_tokens":8}}""");

        var result = _extractor.TryExtractUsage("anthropic", isStreaming: false, body, out var usage);

        Assert.True(result);
        Assert.Equal(20, usage.PromptTokens);
        Assert.Equal(8, usage.CompletionTokens);
    }

    [Theory]
    [InlineData("OpenAI")]
    [InlineData("OPENAI")]
    [InlineData("openai")]
    public void TryExtractUsage_ProviderKeyIsCaseInsensitive(string providerKey)
    {
        var body = Encoding.UTF8.GetBytes("""{"usage":{"prompt_tokens":1,"completion_tokens":1}}""");

        var result = _extractor.TryExtractUsage(providerKey, isStreaming: false, body, out _);

        Assert.True(result);
    }

    [Fact]
    public void TryExtractUsage_UnknownProvider_ReturnsFalse()
    {
        var body = Encoding.UTF8.GetBytes("""{"usage":{"prompt_tokens":1,"completion_tokens":1}}""");

        var result = _extractor.TryExtractUsage("alibaba", isStreaming: false, body, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryExtractUsage_EmptyBuffer_ReturnsFalse()
    {
        var result = _extractor.TryExtractUsage("openai", isStreaming: false, ReadOnlyMemory<byte>.Empty, out _);

        Assert.False(result);
    }

    [Fact]
    public void TryExtractUsage_StreamingFlag_UsesStreamingParsePath()
    {
        var sse = Encoding.UTF8.GetBytes("data: {\"usage\":{\"prompt_tokens\":3,\"completion_tokens\":4}}\n\ndata: [DONE]\n\n");

        var result = _extractor.TryExtractUsage("openai", isStreaming: true, sse, out var usage);

        Assert.True(result);
        Assert.Equal(3, usage.PromptTokens);
        Assert.Equal(4, usage.CompletionTokens);
    }
}
