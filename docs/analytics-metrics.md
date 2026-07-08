# Analytics Metrics: Definitions, Baselines and Rationale

The Cost Analytics tab plots a per-turn timeline comparing **actual**
conversation telemetry against a **theoretical baseline** for one of seven
metrics. X is the 1-based turn index within a conversation (entries sharing
a `SessionId`, ordered by `Timestamp`); Y is the selected metric.

Implemented in `src/AgenticRouter.Gui.Core/Analytics/`
(`MetricRegistry`, `AnalyticsService`, `BaselineDefaults`).

## The seven metrics (rank order, 1 = most important)

Σ denotes a running sum over turns 1..t.

| # | Id | Metric | Kind | Actual line at turn *t* | Baseline line at turn *t* |
|---|---|---|---|---|---|
| 1 | `routing-roi` | Routing ROI ($ saved) | Cumulative | Σ `SavingsAmount` | `TargetReduction` × Σ `WorstCaseCost` |
| 2 | `turn-cost` | Total turn cost ($) | Cumulative | Σ `ActualCost` | Σ `WorstCaseCost` |
| 3 | `tokens` | Prompt + Completion tokens | Cumulative | Σ (prompt + completion) | Σ (prompt + `PremiumVerbosity` × completion) |
| 4 | `tool-loops` | Tool execution loop count | Per turn | `ToolLoopCount` at *t* | flat `BaselineStepsPerTurn` |
| 5 | `cache-hit-rate` | Cache hit rate (%) | Cumulative | running mean of `CacheHitRate` × 100 | flat `TargetCacheHitRate` × 100 |
| 6 | `ttft` | TTFT / routing latency (ms) | Per turn | `TtftMs` at *t* | flat `TargetTtftMs` |
| 7 | `context-margin` | Context buffer margin (%) | Per turn | 100 − `ContextUsedPercent` at *t* | flat 100 − `ContextSafetyLimit` |

### Rank rationale

1. **Routing ROI** — the product's headline claim: dollars the routing
   decision saved. The primary "is the router earning its keep" view.
2. **Total turn cost** — raw spend vs. what the un-routed premium model
   would have cost; the direct budget view behind ROI.
3. **Prompt + Completion tokens** — the primary hockey-stick curve: token
   volume compounds as conversation context accumulates, which is what
   drives cost growth in the first place.
4. **Tool execution loop count** — steps per turn measures agent
   efficiency; runaway loops are the leading cost/latency pathology.
5. **Cache hit rate** — prompt caching is the main lever that bends the
   token cost curve back down.
6. **TTFT / routing latency** — user-perceived responsiveness, including
   the router's own overhead.
7. **Context buffer margin** — remaining context-window headroom; an
   early-warning signal rather than a day-to-day optimization target.

## Baseline constants (`BaselineDefaults`)

| Constant | Value | Source / rationale |
|---|---|---|
| `TargetReduction` | 0.75 | Rounded from the prototype header's "Avg. Cost Reduction 74.20%": the router should save ~75% of worst-case cost. |
| `PremiumVerbosity` | 1.4 | Un-routed premium models answer more verbosely; baseline inflates completion tokens by 40%. |
| `BaselineStepsPerTurn` | 3 | Expected tool-loop steps for a well-behaved agent turn. |
| `TargetCacheHitRate` | 0.70 | Target steady-state prompt-cache hit rate. |
| `TargetTtftMs` | 800 | Cited in the prototype's routing-step copy ("Response latency target: <800ms"). |
| `ContextSafetyLimit` | 80 (%) | No turn should use more than 80% of the context window, i.e. a minimum 20% margin. |

## Semantics and assumptions

- **Conversation** = entries sharing a `SessionId`, ordered by `Timestamp`;
  **turn** = 1-based index within that ordering.
- **Aggregate mode ("All Conversations")**: the value at turn *t* is the
  **mean** across all conversations that have a turn *t*; longer
  conversations extend the series alone. Both actual and baseline lines are
  averaged the same way.
- **Context margin** is stored as `ContextUsedPercent` (% of window used),
  so margin = 100 − used. Higher margin is better.
- **Delta favorability** (header readout color) follows each metric's
  direction: ROI, cache hit rate and context margin improve upward; cost,
  tokens, tool loops and TTFT improve downward (`MetricDefinition.HigherIsBetter`).
- An unknown session id yields an empty series; an unknown metric id throws
  `ArgumentException`.

## Mock data shape

`MockRoutingDataService` ports the prototype's 8 stream entries (new
telemetry fields backfilled) and adds four deterministic multi-turn
conversations — `e89a2bc` (8 turns, extending prototype entry "1"),
`k41m9pq` (10), `n62t8vw` (6), `r97s5xy` (8). Each generated turn re-sends
60% of the accumulated context as prompt (`ContextCarryFactor`), so token
and cost series compound turn over turn — the hockey stick the chart is
designed to show. Costs derive from per-token rates (cheap ≈ gpt-4o-mini,
premium ≈ un-routed frontier model). No randomness anywhere: tests and
screenshots are reproducible.
