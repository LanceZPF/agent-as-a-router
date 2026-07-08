namespace AgenticRouter.Telemetry;

/// <summary>
/// Per-model USD pricing, bound from the <c>Pricing</c> configuration section, used to estimate the
/// dollar cost of a routed request from its token usage.
/// </summary>
public sealed class PricingOptions
{
    /// <summary>
    /// Gets the configuration section name used for pricing settings.
    /// </summary>
    public const string SectionName = "Pricing";

    /// <summary>
    /// Gets the configured price per model, keyed by the client-facing model name (matching
    /// <c>ModelRouting:ModelList[].ModelName</c>). A model absent from this dictionary simply has no
    /// estimated cost - callers should treat that as "unknown," not an error.
    /// </summary>
    public Dictionary<string, ModelPrice> Models { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}

/// <summary>
/// USD price per 1,000,000 tokens for one model.
/// </summary>
/// <param name="InputPerMillionTokens">USD per 1,000,000 prompt/input tokens.</param>
/// <param name="OutputPerMillionTokens">USD per 1,000,000 completion/output tokens.</param>
public sealed record ModelPrice(decimal InputPerMillionTokens, decimal OutputPerMillionTokens)
{
    /// <summary>
    /// Estimates the USD cost of a request given its token usage.
    /// </summary>
    public decimal EstimateCost(int promptTokens, int completionTokens) =>
        (promptTokens / 1_000_000m * InputPerMillionTokens) + (completionTokens / 1_000_000m * OutputPerMillionTokens);
}
