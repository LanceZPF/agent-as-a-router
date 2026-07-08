# Agent-as-a-Router: Agentic Model Routing for Coding Tasks (arXiv:2606.22902v3)

## Paper Metadata
- **Title:** Agent-as-a-Router: Agentic Model Routing for Coding Tasks
- **Date in manuscript:** 2026-06-22 (arXiv v3 posted 2026-06-26)
- **Core artifact homepage:** https://omnisource.cn/agent-as-a-router
- **Code release:** https://github.com/LanceZPF/agent-as-a-router
- **Main idea:** Model routing for coding should be treated as an **adaptive, streaming decision loop** rather than a one-shot static classifier.

## Executive Summary
The paper argues that current LLM routers underperform mainly due to **information deficit**, not weak reasoning. The authors introduce **Agent-as-a-Router**, formalized as a **Context → Action → Feedback → Context (C-A-F)** loop, and instantiate it as **ACRouter** with three modules:
1. **Orchestrator** (routing decision),
2. **Verifier** (execution-grounded scoring),
3. **Memory** (online accumulation of verified history).

They build **CodeRouterBench** (~10K tasks, 10 dimensions, 8 frontier models) to evaluate routing in streaming settings via **cumulative regret** relative to a per-task oracle. Their method gets the best reported cumulative regret and strongest OOD routing behavior.

## Why This Work Exists
Most coding agents fix one backbone model. But in a multi-provider setting, model strengths differ by task type and cost tier. Routing every task to the same model leaves measurable quality/cost gains on the table.

Key diagnosis from Table 1:
- Vanilla zero-shot LLM router (Claude Sonnet 4.6): **41.41 AvgPerf%**
- +Task dimension metadata: **41.18 AvgPerf%**
- +Per-dimension performance statistics: **47.74 AvgPerf%**
- DimensionBest heuristic: **47.50 AvgPerf%**
- Oracle upper bound: **57.00 AvgPerf%**

**Interpretation:** Giving routers better priors closes much of the gap (+15.3% relative gain from 41.41 → 47.74), so missing information is a larger bottleneck than inference skill.

## Formalization: C-A-F Loop
For model pool \(M=\{m_1,\dots,m_M\}\) and task stream \(T=(t_1,\dots,t_N)\):
- Observe context \(c_i\)
- Select action \(a_i\in[M]\)
- Execute and receive feedback \(f_i\)
- Update memory for \(c_{i+1}\)

Reward is cost-aware:
\[
r_i(a_i)=\epsilon_1 s_i(a_i)+\epsilon_2 \kappa_i(a_i)
\]
where \(s_i\) is quality score, \(\kappa_i\) is monetary cost, and in evaluation they use \((\epsilon_1,\epsilon_2)=(1,-0.1)\).

Per-task oracle reward:
\[
r_i^*=\max_j R_{ij},\quad R_{ij}=\epsilon_1 s_{ij}+\epsilon_2\kappa_{ij}
\]

Cumulative regret:
\[
\text{CumReg}_N(\pi)=\sum_{i=1}^{N}(r_i^*-r_i(a_i))
\]
Lower is better.

## ACRouter Design Details
### 1) Orchestrator (information integration)
- Inputs: task prompt/metadata + DimensionBest prior + top-10 memory neighbors.
- Policy: fine-tuned **Qwen3.5-0.8B** + heuristic voting.

### 2) Verifier (new execution-grounded info)
- Aggregates tool signals into unified score \(u_i\in[0,1]\): AST parsing, sandbox execution, prompt-embedded tests, rule signals, and LLM-judge/proxy where needed.

### 3) Memory (information accumulation)
- Online vector store keyed by task embeddings (voyage-code-3 / BGE-large).
- Stores chosen model, performance, cost, and traces.
- Retrieval: cosine kNN, threshold 0.5, k=10.
- FIFO bound: 20K entries.

## Decomposed Routing Taxonomy (from C-A-F view)
- **Single-model:** always pick one model.
- **Static heuristic:** DimensionBest, frozen kNN retrieval.
- **Static trained policy:** LogReg, TF-IDF+MLP, RouteLLM variants, Qwen3.5-FT.
- **Dynamic online bandit:** LinUCB / LinTS.
- **Loop-complete agentic router:** ACRouter.

## CodeRouterBench
### Scale and Split
- Total unified tasks: **10,111**
- Probing set (train+val): **7,080**
- In-distribution test: **2,919**
- OOD agentic programming: **176**
- Deterministic md5 split for 9 single-turn dimensions (70/30 role split with fixed seed protocol).

### Dimensions
9 single-turn dimensions: code generation, algorithm design, bug fixing, code completion, code refactoring, data science, multi-language, code understanding, test generation.

1 OOD dimension: **agentic programming** (SWE-bench Verified, LongCLI-Bench, FeatureBench, SWE-CI), evaluated via Docker harness and mini-swe-agent.

### Candidate Model Pool (8)
Claude Opus 4.6, Claude Sonnet 4.6, GPT-5.4, Qwen3-Max, Qwen3.5-Plus, GLM-5, Kimi-K2.5, MiniMax-M2.7.

## Cost Model
### API pricing (USD / 1M tokens)
- Opus 4.6: in $5.00, out $25.00
- Sonnet 4.6: in $3.00, out $15.00
- GPT-5.4: in $2.50, out $15.00
- Qwen3-Max: in $1.20, out $6.00
- GLM-5: in $0.88, out $3.22
- Kimi-K2.5: in $0.60, out $3.07
- Qwen3.5-Plus: in $0.40, out $2.40
- MiniMax-M2.7: in $0.30, out $1.20

### Self-hosted router token pricing
Derived from H100 amortization and measured throughput (~35,094 tok/s combined):
- **$0.054 / 1M tokens** for self-hosted router-side generation.

## Main Results (Table 3)
### In-distribution (n=2,919)
- Oracle: AvgPerf 57.00, CumReg 0, Perf/$ 8.20
- **ACRouter:** **49.98**, **205.5**, 3.79
- DimensionBest: 47.50, 277.4, 3.69
- LinUCB: 46.84, 296.9, 4.38
- LinTS: 46.48, 307.4, 4.49
- Best static trained policies are ~46.16–47.26 AvgPerf.
- Always-Opus: 43.83 AvgPerf, 387.1 CumReg, poor cost efficiency (1.29 Perf/$).

### OOD agentic programming (n=176)
- Oracle: AvgPerf 75.89, CumReg 0, Perf/$ 2.32
- **ACRouter:** **62.50**, **17.0**, 1.18
- LinUCB: 49.82, 31.1, 0.96
- LinTS: 46.43, 35.9, 0.75
- Qwen3.5-0.8B-FT: 55.36, 27.2, 0.74
- Always-Opus: 57.14, 26.7, 0.64
- Several static routers collapse badly on OOD (e.g., 8.93–21.43 AvgPerf bands for some methods).

## Core Findings
1. **Information deficit > reasoning deficit** for routing quality.
2. **No single backend dominates all coding dimensions.**
3. **Loop-complete adaptive routing wins** on both ID and OOD cumulative regret.
4. Lightweight static learners can look strong in-distribution but **generalize poorly** under distribution shift.

## Supplementary Highlights
- Qwen router scaling (0.8B → 27B with unified LoRA setup) gives only small gains (~0.5 AvgPerf points), suggesting **capacity is not the main bottleneck** once fine-tuned.
- Variance decomposition reports dimension identity explains ~27% of oracle-choice entropy; the rest is task-specific signal.
- OOD metric uses harness-verified resolved-rate (not patch-apply-only success), emphasizing strict evaluation.

## Limitations Reported by Authors
- Provider-side cache hit rates are unavailable; dollar estimates rely on public token prices and measured token counts.
- OOD agentic eval uses 40-step limit (vs larger conventional budgets) for tractability.
- C-A-F is one instantiation; richer memory mechanisms remain future work.

## Practical Build Guide (from paper discussion)
1. Set up model/tool/sandbox layer.
2. Profile models on a probing set.
3. Start with DimensionBest baseline.
4. Add cheap classifier router for strong Perf/$.
5. Enable full C-A-F loop for shifting distributions.
6. Customize verifier/routing tools.
7. Extend benchmark with new models and dimensions.

## Takeaway
This paper reframes coding-model routing as a **continual, feedback-driven control problem**. The central result is not just a better router, but evidence that **execution-grounded memory closes the routing information gap** and materially improves regret, robustness, and practical deployment trade-offs.