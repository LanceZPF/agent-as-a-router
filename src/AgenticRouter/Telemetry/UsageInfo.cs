namespace AgenticRouter.Telemetry;

/// <summary>
/// Token usage for one completed request, as reported by the upstream provider.
/// </summary>
/// <param name="PromptTokens">Prompt/input tokens.</param>
/// <param name="CompletionTokens">Completion/output tokens.</param>
public readonly record struct UsageInfo(int PromptTokens, int CompletionTokens);
