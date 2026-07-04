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

/// <summary>A single turn (one multi-step agentic workflow) within a conversation.</summary>
public sealed record ConversationTurn(
    string Id,
    string Agent,
    string Model,
    int TurnNumber,
    int PromptTokens,
    int CompletionTokens,
    decimal RoutingRoi,
    decimal TotalCost,
    int ToolExecutionSteps,
    decimal CacheHitRate,
    int TimeToFirstTokenMs,
    decimal ContextBufferPercent,
    string Timestamp,
    IReadOnlyList<RoutingStep> RoutingSteps,
    string? RequestSummary = null,
    string? ResponseSummary = null,
    bool IsFallback = false);

/// <summary>A conversation (session) whose turns are shown in the Live Stream tab.</summary>
public sealed record Conversation(
    string Id,
    string Title,
    string FirstTimestamp,
    string LastTimestamp,
    decimal TotalCost,
    int TotalPromptTokens,
    int TotalCompletionTokens,
    bool HasFallbackTurns,
    IReadOnlyList<ConversationTurn> Turns);

/// <summary>
/// Hard-coded mock data for the dashboard. The dashboard is not yet wired up to the live AgenticRouter
/// proxy; replacing this class with real telemetry is the intended integration seam.
/// </summary>
public static class MockData
{
    public static readonly IReadOnlyList<Conversation> Conversations =
    [
        new(
            Id: "sess-001",
            Title: "Code Review Analysis - PR #4521",
            FirstTimestamp: "14:15:32",
            LastTimestamp: "14:22:18",
            TotalCost: 0.045230m,
            TotalPromptTokens: 15456,
            TotalCompletionTokens: 3894,
            HasFallbackTurns: false,
            Turns:
            [
                new(
                    Id: "sess-001-t1",
                    Agent: "Code Review Bot",
                    Model: "claude-3-haiku",
                    TurnNumber: 1,
                    PromptTokens: 2104,
                    CompletionTokens: 891,
                    RoutingRoi: 85.01m,
                    TotalCost: 0.006310m,
                    ToolExecutionSteps: 2,
                    CacheHitRate: 0m,
                    TimeToFirstTokenMs: 245,
                    ContextBufferPercent: 26.2m,
                    Timestamp: "14:15:32",
                    RoutingSteps:
                    [
                        new(StepStatus.Ok, "Code diff parsed (214 changed lines)"),
                        new(StepStatus.Ok, "Anthropic budget nominal; claude-3-haiku selected"),
                        new(StepStatus.Info, "Route Confirmed: claude-3-haiku"),
                    ],
                    RequestSummary: """{"model":"claude-3-haiku","messages":[{"role":"user","content":"Review the diff for PR #4521 (src/auth/token_service.py, 214 lines)…"}]}""",
                    ResponseSummary: """{"content":"Found 3 issues: missing null check on refresh_token (L87), unbounded retry loop (L112), token logged in plaintext (L145)…"}"""),
                new(
                    Id: "sess-001-t2",
                    Agent: "Code Review Bot",
                    Model: "claude-3-haiku",
                    TurnNumber: 2,
                    PromptTokens: 3240,
                    CompletionTokens: 1205,
                    RoutingRoi: 84.50m,
                    TotalCost: 0.009870m,
                    ToolExecutionSteps: 3,
                    CacheHitRate: 72m,
                    TimeToFirstTokenMs: 198,
                    ContextBufferPercent: 40.5m,
                    Timestamp: "14:17:45",
                    RoutingSteps:
                    [
                        new(StepStatus.Ok, "History carried forward (3,240 prompt tokens)"),
                        new(StepStatus.Ok, "Prompt cache hit: 2,333 tokens read from cache"),
                        new(StepStatus.Info, "Route Confirmed: claude-3-haiku"),
                    ],
                    RequestSummary: """{"model":"claude-3-haiku","messages":[…2 prior turns…,{"role":"user","content":"Suggest a fix for the unbounded retry loop"}]}""",
                    ResponseSummary: """{"content":"Replace the while-loop with tenacity retry(stop=stop_after_attempt(3), wait=wait_exponential())…"}"""),
                new(
                    Id: "sess-001-t3",
                    Agent: "Code Review Bot",
                    Model: "claude-3-haiku",
                    TurnNumber: 3,
                    PromptTokens: 4567,
                    CompletionTokens: 1798,
                    RoutingRoi: 83.20m,
                    TotalCost: 0.013820m,
                    ToolExecutionSteps: 4,
                    CacheHitRate: 68m,
                    TimeToFirstTokenMs: 211,
                    ContextBufferPercent: 57.0m,
                    Timestamp: "14:19:22",
                    RoutingSteps:
                    [
                        new(StepStatus.Ok, "History carried forward (4,567 prompt tokens)"),
                        new(StepStatus.Warn, "Prompt growth trending up 41% turn-over-turn"),
                        new(StepStatus.Ok, "Prompt cache hit: 3,106 tokens read from cache"),
                        new(StepStatus.Info, "Route Confirmed: claude-3-haiku"),
                    ],
                    RequestSummary: """{"model":"claude-3-haiku","messages":[…4 prior turns…,{"role":"user","content":"Apply the same pattern to session_service.py"}]}""",
                    ResponseSummary: """{"content":"Patched 2 call sites; session_refresh now shares the retry policy. Diff attached…"}"""),
                new(
                    Id: "sess-001-t4",
                    Agent: "Code Review Bot",
                    Model: "claude-3-haiku",
                    TurnNumber: 4,
                    PromptTokens: 5545,
                    CompletionTokens: 0,
                    RoutingRoi: 88.75m,
                    TotalCost: 0.015230m,
                    ToolExecutionSteps: 1,
                    CacheHitRate: 75m,
                    TimeToFirstTokenMs: 189,
                    ContextBufferPercent: 69.2m,
                    Timestamp: "14:22:18",
                    RoutingSteps:
                    [
                        new(StepStatus.Ok, "Final summary pass (no completion requested)"),
                        new(StepStatus.Ok, "Prompt cache hit: 4,159 tokens read from cache"),
                        new(StepStatus.Info, "Route Confirmed: claude-3-haiku"),
                    ],
                    RequestSummary: """{"model":"claude-3-haiku","messages":[…6 prior turns…,{"role":"user","content":"Summarize all applied changes"}]}"""),
            ]),
        new(
            Id: "sess-002",
            Title: "Data Pipeline Debugging - ETL Job #892",
            FirstTimestamp: "14:08:15",
            LastTimestamp: "14:14:42",
            TotalCost: 0.025450m,
            TotalPromptTokens: 8932,
            TotalCompletionTokens: 2456,
            HasFallbackTurns: false,
            Turns:
            [
                new(
                    Id: "sess-002-t1",
                    Agent: "Data Analyst Wrapper",
                    Model: "gpt-4o-mini",
                    TurnNumber: 1,
                    PromptTokens: 1890,
                    CompletionTokens: 623,
                    RoutingRoi: 87.56m,
                    TotalCost: 0.005340m,
                    ToolExecutionSteps: 2,
                    CacheHitRate: 0m,
                    TimeToFirstTokenMs: 312,
                    ContextBufferPercent: 23.4m,
                    Timestamp: "14:08:15",
                    RoutingSteps:
                    [
                        new(StepStatus.Ok, "SQL error log parsed (1,234 lines)"),
                        new(StepStatus.Ok, "Short context: gpt-4o-mini sufficient"),
                        new(StepStatus.Info, "Route Confirmed: gpt-4o-mini"),
                    ],
                    RequestSummary: """{"model":"gpt-4o-mini","messages":[{"role":"user","content":"ETL job #892 failed at stage 3 with ORA-01555. Log excerpt follows…"}]}""",
                    ResponseSummary: """{"content":"ORA-01555 (snapshot too old): the MERGE reads a table mutated mid-run. Recommend isolating the read with a staging CTAS…"}"""),
                new(
                    Id: "sess-002-t2",
                    Agent: "Data Analyst Wrapper",
                    Model: "gpt-4o-mini",
                    TurnNumber: 2,
                    PromptTokens: 3456,
                    CompletionTokens: 912,
                    RoutingRoi: 86.20m,
                    TotalCost: 0.009870m,
                    ToolExecutionSteps: 3,
                    CacheHitRate: 45m,
                    TimeToFirstTokenMs: 267,
                    ContextBufferPercent: 42.8m,
                    Timestamp: "14:10:33",
                    RoutingSteps:
                    [
                        new(StepStatus.Ok, "Query execution plan added to context"),
                        new(StepStatus.Ok, "Prompt cache hit: 1,555 tokens read from cache"),
                        new(StepStatus.Info, "Route Confirmed: gpt-4o-mini"),
                    ],
                    RequestSummary: """{"model":"gpt-4o-mini","messages":[…2 prior turns…,{"role":"user","content":"Here is the EXPLAIN PLAN output for the failing MERGE…"}]}""",
                    ResponseSummary: """{"content":"The full-table scan on FACT_ORDERS forces a 40-minute read window. Add a partition-pruning predicate on LOAD_DATE…"}"""),
                new(
                    Id: "sess-002-t3",
                    Agent: "Data Analyst Wrapper",
                    Model: "gpt-4o-mini",
                    TurnNumber: 3,
                    PromptTokens: 3586,
                    CompletionTokens: 921,
                    RoutingRoi: 85.90m,
                    TotalCost: 0.010240m,
                    ToolExecutionSteps: 2,
                    CacheHitRate: 52m,
                    TimeToFirstTokenMs: 278,
                    ContextBufferPercent: 44.3m,
                    Timestamp: "14:14:42",
                    RoutingSteps:
                    [
                        new(StepStatus.Ok, "Fix verification pass (3,586 prompt tokens)"),
                        new(StepStatus.Ok, "Prompt cache hit: 1,865 tokens read from cache"),
                        new(StepStatus.Info, "Route Confirmed: gpt-4o-mini"),
                    ],
                    RequestSummary: """{"model":"gpt-4o-mini","messages":[…4 prior turns…,{"role":"user","content":"Validate the revised MERGE statement before I schedule the rerun"}]}""",
                    ResponseSummary: """{"content":"Revised statement is safe: partition pruning cuts the read window to ~90s, well inside undo retention…"}"""),
            ]),
        new(
            Id: "sess-003",
            Title: "Customer Support - Issue #78234",
            FirstTimestamp: "13:52:10",
            LastTimestamp: "14:05:33",
            TotalCost: 0.004560m,
            TotalPromptTokens: 6234,
            TotalCompletionTokens: 1845,
            HasFallbackTurns: true,
            Turns:
            [
                new(
                    Id: "sess-003-t1",
                    Agent: "Customer Support NLP",
                    Model: "claude-3-haiku",
                    TurnNumber: 1,
                    PromptTokens: 1456,
                    CompletionTokens: 567,
                    RoutingRoi: 82.30m,
                    TotalCost: 0.004560m,
                    ToolExecutionSteps: 1,
                    CacheHitRate: 0m,
                    TimeToFirstTokenMs: 189,
                    ContextBufferPercent: 18.0m,
                    Timestamp: "13:52:10",
                    RoutingSteps:
                    [
                        new(StepStatus.Ok, "Customer inquiry classified: billing dispute"),
                        new(StepStatus.Ok, "Anthropic budget nominal; claude-3-haiku selected"),
                        new(StepStatus.Info, "Route Confirmed: claude-3-haiku"),
                    ],
                    RequestSummary: """{"model":"claude-3-haiku","messages":[{"role":"user","content":"Ticket #78234: customer charged twice for the June invoice…"}]}""",
                    ResponseSummary: """{"content":"Duplicate charge confirmed against payment records. Draft refund response prepared for agent review…"}"""),
                new(
                    Id: "sess-003-t2",
                    Agent: "Customer Support NLP",
                    Model: "fallback-cheapest-local",
                    TurnNumber: 2,
                    PromptTokens: 2389,
                    CompletionTokens: 734,
                    RoutingRoi: 0m,
                    TotalCost: 0.000000m,
                    ToolExecutionSteps: 2,
                    CacheHitRate: 0m,
                    TimeToFirstTokenMs: 445,
                    ContextBufferPercent: 29.5m,
                    Timestamp: "13:54:28",
                    RoutingSteps:
                    [
                        new(StepStatus.Warn, "Anthropic hourly budget breached; routing restricted"),
                        new(StepStatus.Ok, "Fallback routing activated: local model"),
                        new(StepStatus.Info, "Route Confirmed: fallback-cheapest-local"),
                    ],
                    RequestSummary: """{"model":"fallback-cheapest-local","messages":[…2 prior turns…,{"role":"user","content":"Customer replied asking about the refund timeline"}]}""",
                    ResponseSummary: """{"content":"Refunds post within 5-7 business days of approval. Suggested reply drafted…"}""",
                    IsFallback: true),
                new(
                    Id: "sess-003-t3",
                    Agent: "Customer Support NLP",
                    Model: "fallback-cheapest-local",
                    TurnNumber: 3,
                    PromptTokens: 2389,
                    CompletionTokens: 544,
                    RoutingRoi: 0m,
                    TotalCost: 0.000000m,
                    ToolExecutionSteps: 1,
                    CacheHitRate: 0m,
                    TimeToFirstTokenMs: 512,
                    ContextBufferPercent: 29.5m,
                    Timestamp: "14:05:33",
                    RoutingSteps:
                    [
                        new(StepStatus.Warn, "Anthropic budget still breached; staying on fallback"),
                        new(StepStatus.Ok, "Local model serving request"),
                        new(StepStatus.Info, "Route Confirmed: fallback-cheapest-local"),
                    ],
                    RequestSummary: """{"model":"fallback-cheapest-local","messages":[…4 prior turns…,{"role":"user","content":"Close out the ticket with a resolution summary"}]}""",
                    ResponseSummary: """{"content":"Ticket #78234 resolved: duplicate June charge refunded, confirmation email queued…"}""",
                    IsFallback: true),
            ]),
    ];

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
