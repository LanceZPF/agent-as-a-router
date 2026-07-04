namespace AgenticRouter.Gui.Models;

/// <summary>Severity of a single routing-decision log step.</summary>
public enum StepStatus
{
    Ok,
    Warn,
    Info,
}

/// <summary>One step in the routing decision log shown in the Live Stream inspector.</summary>
public sealed record RoutingStep(StepStatus Status, string Message);

/// <summary>A single routing decision shown in the Live Stream tab.</summary>
public sealed record RoutingEntry(
    string Id,
    string SessionId,
    string TraceId,
    string Agent,
    string Model,
    bool IsFallback,
    int PromptTokens,
    int CompletionTokens,
    decimal ActualCost,
    decimal WorstCaseCost,
    decimal SavingsAmount,
    decimal SavingsPercent,
    string Timestamp,
    IReadOnlyList<RoutingStep> RoutingSteps);

/// <summary>Per-provider budget state shown in the Governance tab.</summary>
public sealed record Provider(
    string Id,
    string Name,
    string Label,
    decimal BudgetCap,
    decimal CurrentSpend,
    int? EstimatedDaysRemaining);

/// <summary>A point on the cumulative savings time series.</summary>
public sealed record CostDataPoint(string Time, decimal Cumulative);

/// <summary>Cost-reduction percentage and absolute savings for one agent.</summary>
public sealed record AgentRoi(string Agent, decimal Reduction, decimal Savings);

/// <summary>Prompt/completion token volume for one time slot.</summary>
public sealed record TokenBucket(string Slot, decimal Prompt, decimal Completion);

/// <summary>Market-share percentage (and display color) for one model.</summary>
public sealed record ModelShare(string Model, decimal Value, string Color);

/// <summary>
/// Hard-coded mock data for the dashboard. The dashboard is not yet wired up to the live AgenticRouter
/// proxy; replacing this class with real telemetry is the intended integration seam.
/// </summary>
public static class MockData
{
    public static readonly IReadOnlyList<RoutingEntry> Entries =
    [
        new(
            Id: "1",
            SessionId: "e89a2bc",
            TraceId: "a4f89c02",
            Agent: "Data Analyst Wrapper",
            Model: "gpt-4o-mini",
            IsFallback: false,
            PromptTokens: 1230,
            CompletionTokens: 702,
            ActualCost: 0.000480m,
            WorstCaseCost: 0.003860m,
            SavingsAmount: 0.003380m,
            SavingsPercent: 87.56m,
            Timestamp: "14:32:01",
            RoutingSteps:
            [
                new(StepStatus.Ok, "Input text contains source code telemetry"),
                new(StepStatus.Ok, "Budget nominal: gpt-4o-mini selected"),
                new(StepStatus.Ok, "Context window validated (1,932 tokens)"),
                new(StepStatus.Info, "Route Confirmed: gpt-4o-mini"),
            ]),
        new(
            Id: "2",
            SessionId: "d32a1ff",
            TraceId: "b7c21e45",
            Agent: "Code Review Bot",
            Model: "fallback-cheapest-local",
            IsFallback: true,
            PromptTokens: 890,
            CompletionTokens: 312,
            ActualCost: 0.000000m,
            WorstCaseCost: 0.002100m,
            SavingsAmount: 0.000000m,
            SavingsPercent: 0.00m,
            Timestamp: "14:31:48",
            RoutingSteps:
            [
                new(StepStatus.Ok, "Input text contains source code telemetry"),
                new(StepStatus.Warn, "OpenAI budget breached; routing restricted"),
                new(StepStatus.Ok, "Fallback routing activated for destination"),
                new(StepStatus.Info, "Route Confirmed: fallback-cheapest-local"),
            ]),
        new(
            Id: "3",
            SessionId: "f12b8de",
            TraceId: "c9d34f67",
            Agent: "Customer Support NLP",
            Model: "claude-3-haiku",
            IsFallback: false,
            PromptTokens: 2104,
            CompletionTokens: 891,
            ActualCost: 0.000631m,
            WorstCaseCost: 0.004210m,
            SavingsAmount: 0.003579m,
            SavingsPercent: 85.01m,
            Timestamp: "14:31:35",
            RoutingSteps:
            [
                new(StepStatus.Ok, "Conversation context parsed (3 turns)"),
                new(StepStatus.Ok, "Anthropic budget nominal; claude-3-haiku selected"),
                new(StepStatus.Ok, "Response latency target: <800ms"),
                new(StepStatus.Info, "Route Confirmed: claude-3-haiku"),
            ]),
        new(
            Id: "4",
            SessionId: "a45c7fe",
            TraceId: "d2e56g78",
            Agent: "SQL Query Optimizer",
            Model: "gpt-4o-mini",
            IsFallback: false,
            PromptTokens: 445,
            CompletionTokens: 203,
            ActualCost: 0.000096m,
            WorstCaseCost: 0.000780m,
            SavingsAmount: 0.000684m,
            SavingsPercent: 87.69m,
            Timestamp: "14:31:22",
            RoutingSteps:
            [
                new(StepStatus.Ok, "SQL schema fingerprint matched"),
                new(StepStatus.Ok, "Short context: mini-model sufficient"),
                new(StepStatus.Info, "Route Confirmed: gpt-4o-mini"),
            ]),
        new(
            Id: "5",
            SessionId: "c78d2ae",
            TraceId: "e3f67h89",
            Agent: "Log Anomaly Detector",
            Model: "gemini-1.5-flash",
            IsFallback: false,
            PromptTokens: 3840,
            CompletionTokens: 128,
            ActualCost: 0.000192m,
            WorstCaseCost: 0.002304m,
            SavingsAmount: 0.002112m,
            SavingsPercent: 91.67m,
            Timestamp: "14:31:09",
            RoutingSteps:
            [
                new(StepStatus.Ok, "Long-context log batch detected (3,840 tokens)"),
                new(StepStatus.Ok, "Gemini flash selected for cost efficiency"),
                new(StepStatus.Ok, "Output: classification only (128 tokens)"),
                new(StepStatus.Info, "Route Confirmed: gemini-1.5-flash"),
            ]),
        new(
            Id: "6",
            SessionId: "b91e3cd",
            TraceId: "f4a78i90",
            Agent: "Summarization Pipeline",
            Model: "fallback-cheapest-local",
            IsFallback: true,
            PromptTokens: 1560,
            CompletionTokens: 420,
            ActualCost: 0.000000m,
            WorstCaseCost: 0.003120m,
            SavingsAmount: 0.000000m,
            SavingsPercent: 0.00m,
            Timestamp: "14:30:55",
            RoutingSteps:
            [
                new(StepStatus.Ok, "Document chunking completed (4 chunks)"),
                new(StepStatus.Warn, "OpenAI monthly cap reached: fallback triggered"),
                new(StepStatus.Warn, "Gemini quota exhausted for current period"),
                new(StepStatus.Ok, "Local model activated as final fallback"),
                new(StepStatus.Info, "Route Confirmed: fallback-cheapest-local"),
            ]),
        new(
            Id: "7",
            SessionId: "g23f4bc",
            TraceId: "g5b89j01",
            Agent: "Embedding Generator",
            Model: "text-embedding-3-small",
            IsFallback: false,
            PromptTokens: 512,
            CompletionTokens: 0,
            ActualCost: 0.000002m,
            WorstCaseCost: 0.000010m,
            SavingsAmount: 0.000008m,
            SavingsPercent: 80.00m,
            Timestamp: "14:30:42",
            RoutingSteps:
            [
                new(StepStatus.Ok, "Embedding task detected: no completion needed"),
                new(StepStatus.Ok, "text-embedding-3-small selected (optimal cost)"),
                new(StepStatus.Info, "Route Confirmed: text-embedding-3-small"),
            ]),
        new(
            Id: "8",
            SessionId: "h67g8de",
            TraceId: "h6c90k12",
            Agent: "Data Analyst Wrapper",
            Model: "claude-3-5-sonnet",
            IsFallback: false,
            PromptTokens: 4200,
            CompletionTokens: 1890,
            ActualCost: 0.018270m,
            WorstCaseCost: 0.037800m,
            SavingsAmount: 0.019530m,
            SavingsPercent: 51.67m,
            Timestamp: "14:30:29",
            RoutingSteps:
            [
                new(StepStatus.Ok, "Complex reasoning task detected"),
                new(StepStatus.Ok, "High token count requires Sonnet-tier model"),
                new(StepStatus.Ok, "Anthropic budget nominal"),
                new(StepStatus.Info, "Route Confirmed: claude-3-5-sonnet"),
            ]),
    ];

    public static readonly IReadOnlyList<Provider> Providers =
    [
        new(Id: "openai", Name: "OpenAI API", Label: "Production Pool", BudgetCap: 500m, CurrentSpend: 492.80m, EstimatedDaysRemaining: 0),
        new(Id: "anthropic", Name: "Anthropic Claude", Label: "Inference Pool", BudgetCap: 300m, CurrentSpend: 258.40m, EstimatedDaysRemaining: 3),
        new(Id: "gemini", Name: "Google Gemini", Label: "Analytics Pool", BudgetCap: 200m, CurrentSpend: 62.40m, EstimatedDaysRemaining: 21),
        new(Id: "local", Name: "Local Inference", Label: "Fallback Pool", BudgetCap: 50m, CurrentSpend: 8.20m, EstimatedDaysRemaining: null),
    ];

    public static readonly IReadOnlyList<CostDataPoint> CostData =
    [
        new("Jun 1", 0m),
        new("Jun 3", 4.20m),
        new("Jun 5", 9.80m),
        new("Jun 7", 17.60m),
        new("Jun 9", 26.10m),
        new("Jun 11", 38.40m),
        new("Jun 13", 51.20m),
        new("Jun 15", 67.80m),
        new("Jun 17", 82.50m),
        new("Jun 19", 99.10m),
        new("Jun 21", 112.40m),
        new("Jun 23", 124.70m),
        new("Jun 25", 133.20m),
        new("Jun 27", 138.90m),
        new("Jun 29", 141.50m),
        new("Jul 1", 142.36m),
    ];

    public static readonly IReadOnlyList<AgentRoi> AgentRoi =
    [
        new("Log Anomaly Detector", 91.67m, 38.20m),
        new("SQL Query Optimizer", 87.69m, 22.40m),
        new("Data Analyst Wrapper", 85.12m, 41.80m),
        new("Customer Support NLP", 84.30m, 18.60m),
        new("Summarization Pipeline", 79.50m, 12.40m),
        new("Embedding Generator", 78.20m, 5.80m),
        new("Code Review Bot", 64.10m, 2.90m),
    ];

    public static readonly IReadOnlyList<TokenBucket> TokenBuckets =
    [
        new("Mon", 2_840_000m, 980_000m),
        new("Tue", 3_120_000m, 1_140_000m),
        new("Wed", 4_200_000m, 1_680_000m),
        new("Thu", 3_890_000m, 1_520_000m),
        new("Fri", 2_960_000m, 1_020_000m),
        new("Sat", 1_840_000m, 620_000m),
        new("Sun", 1_240_000m, 380_000m),
    ];

    public static readonly IReadOnlyList<ModelShare> ModelShares =
    [
        new("gpt-4o-mini", 38m, "#10b981"),
        new("claude-3-haiku", 22m, "#38bdf8"),
        new("gemini-1.5-flash", 18m, "#818cf8"),
        new("fallback-local", 10m, "#f59e0b"),
        new("claude-3-5-sonnet", 7m, "#fb7185"),
        new("text-embedding-3-small", 5m, "#a78bfa"),
    ];
}
