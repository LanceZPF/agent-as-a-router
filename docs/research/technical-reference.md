# Agent-as-a-Router: Agentic Model Routing for Coding Tasks
**arXiv:2606.22902v3 — Full Technical Reference**

---

## Paper Metadata

| Field | Value |
|---|---|
| **Title** | Agent-as-a-Router: Agentic Model Routing for Coding Tasks |
| **arXiv ID** | 2606.22902v3 |
| **Manuscript Date** | 2026-06-22 (v3 posted 2026-06-26) |
| **Venue** | arXiv cs.AI |
| **Homepage** | https://omnisource.cn/agent-as-a-router |
| **Code / Benchmark** | https://github.com/LanceZPF/agent-as-a-router |

### Authors & Affiliations

| Author | Affiliation |
|---|---|
| Pengfei Zhou | National University of Singapore; DAMO Academy, Alibaba Group |
| Zhiwei Tang | DAMO Academy, Alibaba Group; Hupan Lab; Zhejiang University |
| Yixing Ma | University of California, Berkeley |
| Jiasheng Tang ‡ | DAMO Academy, Alibaba Group; Hupan Lab |
| Yizeng Han | DAMO Academy, Alibaba Group |
| Zhenglin Wan | National University of Singapore |
| Fanqing Meng | National University of Singapore |
| Wei Wang | The Hong Kong University of Science and Technology |
| Bohan Zhuang | Zhejiang University |
| Wangbo Zhao ‡ | The Hong Kong University of Science and Technology |
| Yang You ‡ | National University of Singapore |

‡ Corresponding Author

---

## Abstract

Real-world users typically have access to multiple LLMs from different providers, and these LLMs often excel at distinct domains, yet none dominate all. Routing each task to the most suitable model becomes critical for both performance and cost. Existing routers treat this as a **static, one-off classification problem**. The fundamental bottleneck is identified as **information deficit**: augmenting a vanilla LLM router with performance statistics at the task-dimension level yields a **15.3% relative gain**, surpassing a heuristic router built on the same dimension-level priors.

The proposed **Agent-as-a-Router** framework formalizes routing as a **C-A-F loop** (Context→Action→Feedback→Context). It closes the information gap by accumulating execution-grounded experience during deployment. The instantiation **ACRouter** is composed of an **Orchestrator**, a **Verifier**, and a **Memory** module. **CodeRouterBench** is introduced as an evaluation environment comprising ~10K task instances with verified scores from 8 frontier LLMs, enabling **regret-based** router comparison on streaming tasks.

---

## 1. Introduction

Most coding agents (Claude Code, Codex) solve all tasks using the same LLM — reasonable from a **provider-centric** perspective but suboptimal in **user-centric** scenarios. Experiments across 8 frontier models and various coding dimensions show that:

- The best model **varies per task**
- Always picking the globally strongest model still lags behind the per-task oracle

**Key question:** *Which model should handle each incoming task?*

Preliminary experiments reveal that a zero-shot LLM-as-a-Router (powered by Claude Sonnet 4.6) still falls short of the per-task oracle by a wide margin, suggesting the fundamental bottleneck extends beyond reasoning.

**Diagnosis:** The bottleneck is **information deficit**, not reasoning failure. Running only one ablation — varying the information available to the LLM router — with the same model produces:

| Setting | AvgPerf% |
|---|---|
| Vanilla zero-shot | 41.41 |
| + Per-dimension statistics | 47.74 (+15.3% relative) |
| DimensionBest heuristic (same info) | 47.50 |

LLM router given the same statistics as DimensionBest **exceeds** it (47.74 vs. 47.50).

### Three Core Contributions

1. **Framework** — Agent-as-a-Router formalizing routing as a C-A-F loop with cumulative regret as the natural streaming metric.
2. **Artifacts** — ACRouter (C-A-F instantiation) + CodeRouterBench (~10K tasks, 8 LLMs, execution-verified).
3. **Findings** — Information deficit is the routing bottleneck; ACRouter attains lowest cumulative regret on both ID and OOD tasks; static learners fail to generalize on OOD.

---

## 2. Related Work

### LLM Routing

- **RouteLLM** [Ong et al.] — routing as preference learning, trains classifiers on human preference data to predict which of two models produces better responses.
- **Meta-modeling approaches** [Šakota et al.; Shnitzer et al.] — learn to predict model performance from task features.
- **LLMRouterBench** [Li et al., 2026] — evaluates routing across 21 general NLU datasets with 33 models.
- **AutoMix** [Aggarwal et al.] — automatically mixes LLMs.
- **FrugalGPT** [Chen et al.] — cost-effective LLM use.
- **Hybrid LLM** [Ding et al.] — cost-efficient and quality-aware query routing.
- **LLM Router** [Varshney et al., 2026] — routing with prefill activations.
- **Adaptive VLM Routing** [Liu et al., 2026] — routing for computer use agents.

**Key distinction:** This paper proposes Agent-as-a-Router with the C-A-F loop for **adaptive coding routing** with regret-based streaming evaluation.

### Coding Agents

Evolved from single-call generators into multi-stage harness-based frameworks (planning, retrieval, code editing, execution, self-debugging). Multi-agent variants decompose stages across specialized roles. Production systems include Claude Code and Codex CLI. **ACRouter** addresses the fixed-backbone limitation by actively routing each task to the most suitable model within a continuous stream.

---

## 3. Agent-as-a-Router

### 3.1 Preliminary: The Performance Gap Diagnosis

**Table 1 — Preliminary ablation (Claude Sonnet 4.6, n=2,919 tasks)**

| Ablation | Interpretation | AvgPerf% | Perf/$ |
|---|---|---|---|
| Oracle | Best model per task (theoretical upper bound) | 57.00 | 8.20 |
| DimensionBest | Best model per dimension using dimension-level prior | 47.50 | 3.69 |
| Vanilla | Zero-shot LLM-as-a-router | 41.41 | 1.97 |
| +Dimension | + Task dimension description | 41.18 | 1.81 |
| **+Perf stats** | **+ Prior per-dimension performance statistics from probing set** | **47.74** | **1.71** |

**Key finding:** Providing the same statistics that DimensionBest encodes to an LLM router causes it to *exceed* DimensionBest (47.74 vs. 47.50). The +15.3% relative improvement (41.41→47.74) from adding performance statistics reveals that **information deficit, not reasoning failure, is the primary bottleneck**.

**Two design insights from this diagnosis:**
1. The router must **acquire** new execution-grounded information at each decision (verification).
2. The router must **accumulate** it across the task stream (memory).

### 3.2 The C-A-F Loop

#### Setup

- **Model pool:** $M = \{m_1, \ldots, m_M\}$ with $M$ models
- **Task stream:** $T = (t_1, \ldots, t_N)$ processed sequentially
- After each routing decision, the verified outcome feeds into context for the next decision

#### The Loop

At task $t_i$, the router observes Context $c_i$, selects Action $a_i \in [M]$, and receives verification Feedback $f_i$, which is memorized into $c_{i+1}$:

$$c_i \xrightarrow{\text{Decide}} a_i \xrightarrow{\text{Execute}} f_i \xrightarrow{\text{Memorize}} c_{i+1}$$

Each completed loop makes the next one more informed.

#### Per-Loop Components

| Component | Definition |
|---|---|
| **Context** $c_i = (p_i, d_i, \mathcal{H}_{<i})$ | $p_i$ = input prompt; $d_i$ = optional metadata (description, difficulty, language); $\mathcal{H}_{<i}$ = Memory state from all prior loops |
| **Action** $a_i \in [M]$ | Index of selected model $m_{a_i}$ from pool $M$ |
| **Feedback** $f_i = (\hat{s}_i, \hat{\kappa}_i)$ | $\hat{s}_i \in [0,1]$ = verifier-observed score; $\hat{\kappa}_i$ = monetary cost from token consumption and official prices |

#### Contextual-Bandit Equivalence

The C-A-F formulation relates to a **contextual multi-armed bandit** with $c_i$ as side information, $a_i$ as the arm pull, and $f_i$ as the feedback.

History: $h_i = (c_1, a_1, f_1, \ldots, c_{i-1}, a_{i-1}, f_{i-1}, c_i)$

Routing policy: $\pi(\cdot | h_i) \in \Delta_{M-1}$ (probability simplex over $M$ actions)

**Per-task reward** (cost-aware, user-specified weights $\epsilon_1 > 0$, $\epsilon_2 < 0$):

$$r_i(a_i) = \epsilon_1 s_i(a_i) + \epsilon_2 \kappa_i(a_i) \tag{2}$$

**Policy mean reward over the stream:**

$$V(\pi) = \frac{1}{N} \sum_{i=1}^{N} r_i(a_i) = \epsilon_1 \frac{1}{N}\sum_{i=1}^{N} s_i(a_i) + \epsilon_2 \frac{1}{N}\sum_{i=1}^{N} \kappa_i(a_i) \tag{3}$$

#### Per-Task Oracle and Cumulative Regret

**Full outcome matrix:** $O \in \mathbb{R}^{N \times M \times 2}$, where $O_{ij} = (s_{ij}, \kappa_{ij})$ stores the ground-truth score and cost of model $m_j$ on task $t_i$.

**Induced reward matrix:** $R \in \mathbb{R}^{N \times M}$

$$R_{ij} = \epsilon_1 s_{ij} + \epsilon_2 \kappa_{ij}, \quad i \in [N],\ j \in [M] \tag{4}$$

**Per-task oracle** (selects reward-maximizing model per task with full prior knowledge):

$$a^*_i = \arg\max_{j \in [M]} R_{ij}, \quad r^*_i = \max_{j \in [M]} R_{ij} \tag{5}$$

$$V^* = \frac{1}{N} \sum_{i=1}^{N} r^*_i = \frac{1}{N} \sum_{i=1}^{N} \max_{j \in [M]} R_{ij} \tag{6}$$

> **Note:** The per-task oracle is **not** equal to a single-best-arm policy that commits to one global optimal model.

**Cumulative regret** (lower is better):

$$\text{CumReg}_N(\pi) = \sum_{i=1}^{N} \delta_i = N(V^* - V(\pi)) \tag{7}$$

where $\delta_i = r^*_i - r_i(a_i) \geq 0$ is the single-task regret.

**Canonical evaluation reward weights throughout the paper:** $(\epsilon_1, \epsilon_2) = (1, -0.1)$

$$r_i(a_i) = s_i(a_i) - 0.1\,\kappa_i(a_i)$$

On ID single-turn tasks the cost term is mostly a soft tie-breaker; on OOD agentic tasks, multi-step backend calls make the cost term materially affect regret.

### 3.3 ACRouter Instantiation

The three design needs from §3.1:
1. Integrate available information at decision time → **Orchestrator**
2. Produce new execution-grounded information at each loop → **Verifier**
3. Accumulate it across loops so future decisions condition on past outcomes → **Memory**

#### Orchestrator (Integrating Information)

- **Inputs:** task prompt/metadata + DimensionBest prior + top-10 historical neighbors from Memory (cosine kNN)
- **Policy:** fine-tuned **Qwen3.5-0.8B** + heuristic rules via **weighted voting**
- The Orchestrator ensembles multiple signals to make the final dispatch decision

**Example routing decision (bug-fixing task):**

| Policy | Vote |
|---|---|
| `api_llm` (Qwen3.5-0.8B) | MiniMax-M2.7 |
| `logreg` | GLM-5 |
| `memory_kNN` | Kimi-K2.5 |
| `dim_best` | Kimi-K2.5 |

Voter weighted scores: Kimi-K2.5 → 1.47, MiniMax-M2.7 → 0.64, GLM-5 → 0.43 → **Argmax → Kimi-K2.5**

#### Verifier (Producing Information)

Evaluates model output and aggregates multiple signals into a unified performance score $u_i \in [0, 1]$:

$$u_i = \sum_{k \in \mathcal{K}_{d(t_i)}} w_{d(t_i), k} \cdot \hat{s}_k(a_i, t_i) \tag{8}$$

where:
- $d(t_i)$ = type of task $t_i$ (determines whether executable)
- $\mathcal{K}_{d(t_i)}$ = set of verification tools
- $\hat{s}_k(\cdot) \in [0, 1]$ = scalar score from the $k$-th tool
- $w_{d(t_i), k}$ = type-specific weights with $\sum_{k} w_{d(t_i), k} = 1$

**Verification tool tiers:**
1. **AST parsing** — structural validity check
2. **Sandbox execution** — run code in isolated environment
3. **Prompt-embedded tests** — extract and run in-prompt test cases
4. **Rule-based signals** — heuristic checks
5. **LLM-as-Judge / proxy metrics** — for non-executable dimensions

#### Memory (Accumulating Information)

- **Type:** Online vector store
- **Key:** Task embeddings (`voyage-code-3` / `BGE-large`)
- **Value:** Chosen model, observed performance, monetary cost, Verifier verification traces
- **Retrieval:** Cosine kNN, similarity threshold **0.5**, **k=10** neighbors
- **FIFO bound:** **20,000 entries**
- **Update:** Committed in-place after each loop
- **Warm-up:** 200 val tasks at initialization

Unlike static dimension-hashed routing, this embedding-based store enables **fine-grained, context-aware** decision making, making both past successes and recent failures of any candidate on similar tasks visible to the Orchestrator.

### 3.4 Decomposed Routing Policies

The C-A-F loop provides a unified taxonomy. Restricting/removing components yields baseline routing families:

**Table 4 — C-A-F Configuration Taxonomy**

| Method Family | Orchestrator | Verifier | Memory |
|---|---|---|---|
| Single-Model (Always-$m$) | Direct dispatch | — | — |
| Static: Heuristic (DimensionBest, kNN Retrieval) | Static lookup | — | Frozen probing-set prior |
| Static: Trained Policy (LogReg / TF-IDF+MLP / RouteLLM / Qwen3.5-FT) | Trained model | — | — |
| Dynamic: Online Bandit (LinUCB, LinTS) | $\arg\max$ rule | Reward only | Per-arm parametric posterior |
| **ACRouter (loop-complete)** | **LLM policy + tools** | **Sandbox-native** | **Online task-embedding kNN** |

#### Specific Baseline Configurations

- **Always-$m$:** Fixed model regardless of context. Reference performance floor.
- **DimensionBest:** Routes to dimensionally-best model using dimension-level prior. Coarse-grained oracle.
- **kNN Retrieval:** Model selected from nearest-neighbor tasks in frozen probing-set memory.
- **LogReg:** TF-IDF features → logistic regression trained on probing set.
- **TF-IDF+MLP:** TF-IDF features → MLP trained on probing set.
- **RouteLLM-MF:** Matrix Factorization variant (RouteLLM framework).
- **RouteLLM-BERT:** BERT-based variant (RouteLLM framework).
- **Qwen3.5-FT:** Qwen3.5 variants with LoRA fine-tuning on probing set (0.8B reported in main table).
- **LinUCB:** Linear Upper Confidence Bound contextual bandit ($\alpha = \lambda = 1$). Context: categorical (dimension/difficulty/language one-hot) or 64-dim Johnson–Lindenstrauss projection of task embedding. Warm-started on probing set (seed 42).
- **LinTS:** Linear Thompson Sampling ($v = 0.5$, $\lambda = 1$). Same context options as LinUCB.

---

## 4. CodeRouterBench

### 4.1 Overview and Motivation

Existing routing benchmarks measure only single-shot accuracy and cannot support regret-based streaming evaluation. CodeRouterBench provides:
- Pre-collected per-task, per-model outcomes
- Controlled streaming environment
- 10 coding dimensions including OOD agentic programming
- ~10K tasks from 15+ source benchmarks

### 4.2 Construction Pipeline (Three Phases)

**Phase 1 — Data Collection:** 15+ benchmark sources unified into ~10K tasks across 10 coding dimensions.

**Phase 2 — Model Evaluation:** 8 frontier LLMs generate responses; scored via sandboxed execution and LLM-as-Judge. Produces observation matrix $O_{ij} = (s_{ij}, \kappa_{ij})$.

**Phase 3 — Routing Evaluation:** Several routing methods evaluated on AvgPerf, CumReg, and cost metrics with Pareto analysis.

### 4.3 Benchmark Statistics (Table 2)

| Statistic | Value |
|---|---|
| Coding dimensions | 10 |
| Source benchmarks | 15+ |
| Probing set tasks (train 60% + val 10%) | 7,080 |
| In-distribution test tasks (test 30%) | 2,919 |
| OOD test tasks (agentic programming) | 176 |
| **Total** | **10,111** |

### 4.4 Coding Dimensions (Table 5)

| Dimension | Description | Source(s) | Eval Method |
|---|---|---|---|
| Code Generation | Function-level synthesis | HumanEval+, MBPP+, BigCodeBench | Exec (pass@1) |
| Algorithm Design | Competitive programming | LiveCodeBench, BigCodeBench | Exec |
| Bug Fixing | Locate and repair defects | DebugBench, SWE-bench Lite | Exec |
| Code Completion | Fill-in-the-middle | CRUXEval, HumanEval+ variants | Exec |
| Code Refactoring | Improve code quality | Bugs2Fix, CanItEdit | Proxy+ (LLM-as-Judge) |
| Data Science | Data analysis pipelines | DS-1000, BigCodeBench | Exec |
| Multi-Language | Cross-language tasks | HumanEval-X, MultiPL-E | Exec |
| Code Understanding | Explain & summarize code | CodeXGLUE Summarization | Proxy+ (LLM-as-Judge) |
| Test Generation | Generate test suites | LiveCodeBench, HumanEval+ variants | Proxy+ (LLM-as-Judge) |
| **Agentic Programming (OOD)** | **Long-horizon, multi-file repo tasks** | **SWE-bench Verified, LongCLI-Bench, FeatureBench, SWE-CI** | **Sandbox (Docker)** |

- 7 dimensions use **execution-based scoring** (pass@1 in sandboxed environments)
- 3 dimensions use **proxy metrics** supplemented by LLM-as-Judge
- Each of the 9 single-turn dimensions contains **1,111 tasks** (306–347 per dimension in the test split)

### 4.5 Data Split Protocol

Deterministic MD5-based split (seed: `coding-router-v1`):

| Role | Split | Tasks | Purpose |
|---|---|---|---|
| **Probing set** | train 60% + val 10% | 7,080 | Develop routers: profile model strengths, train classifiers, calibrate DimensionBest, warm-start ACRouter Memory (200 val tasks) |
| **In-distribution test** | test 30% | 2,919 | Held-out controlled evaluation with execution-verified per-task per-model scores |
| **OOD test** | — | 176 | Agentic programming; no router accesses ground-truth from this dimension |

**OOD Task Sources:** SWE-bench Verified, LongCLI-Bench, FeatureBench, SWE-CI. Filtered for high prompt similarity with probing set. Evaluated with **SWE-Bench Docker harness** + **mini-swe-agent**, capped at **40 steps per task**.

### 4.6 Model Pool

**Eight frontier LLMs evaluated:**

Claude Opus 4.6, Claude Sonnet 4.6, GPT-5.4, Qwen3-Max, Qwen3.5-Plus, GLM-5, Kimi-K2.5, MiniMax-M2.7

**Key observation:** No single model dominates across all coding dimensions:

| Best Model | Strongest Dimension(s) | Metric |
|---|---|---|
| Claude Opus 4.6 | Code completion, Bug fixing, Code generation | Highest average (42.9%) |
| GLM-5 | Algorithm design | 47.2% vs. Opus 25.4% (+86% relative) |
| Qwen3-Max | Test generation | 82.7% vs. Opus 39.2% (+111% relative) |
| Kimi-K2.5 | Data science | 18.4% vs. Opus 14.2% (+30%) |

**5 distinct models serve as the dimension-best choice** across the 9 dimensions.

---

## 5. Empirical Validation

### 5.1 Metrics

| Metric | Definition |
|---|---|
| **AvgPerf** | Average performance score across all tasks |
| **CumReg** | Terminal cumulative regret (Eq. 7), $(\epsilon_1, \epsilon_2) = (1, -0.1)$ |
| **TotTok** | Total input + output token consumption (router + model) |
| **$Total** | Calculated USD based on TotTok and official pricing |
| **Perf/$** | Performance-to-cost ratio (AvgPerf% / Cost) |

For local GPU-served routers (finetuned LLMs, etc.): tokens priced at **$0.054/M** (H100 amortization, see §B.3.2).

### 5.2 Main Results (Table 3)

#### In-Distribution Test ($n = 2,919$, 9 dimensions)

| Router | AvgPerf% ↑ | CumReg ↓ | Perf/$ ↑ |
|---|---|---|---|
| **Oracle** | **57.00** | **0** | **8.20** |
| **ACRouter (ours)** | **49.98** | **205.5** | **3.79** |
| LinUCB | 46.84 | 296.9 | 4.38 |
| LinTS | 46.48 | 307.4 | 4.49 |
| DimensionBest | 47.50 | 277.4 | 3.69 |
| kNN Retrieval | 47.18 | 286.7 | 6.07 |
| LogReg | 47.26 | 284.4 | 6.27 |
| RouteLLM-BERT | 47.22 | 285.5 | 6.22 |
| TF-IDF+MLP | 46.97 | 292.8 | 6.11 |
| Qwen3.5-0.8B-Finetuned | 46.41 | 309.1 | 6.82 |
| RouteLLM-MF | 46.16 | 316.5 | 6.19 |
| Always-Opus 4.6 | 43.83 | 387.1 | 1.29 |
| Always-Kimi-K2.5 | 36.66 | 593.3 | 12.62 |
| Always-Qwen3.5-Plus | 37.16 | 580.2 | 2.05 |
| Random | 38.75 | 533.6 | 2.48 |

#### OOD Test ($n = 176$, agentic programming)

| Router | AvgPerf% ↑ | CumReg ↓ | Perf/$ ↑ |
|---|---|---|---|
| **Oracle** | **75.89** | **0** | **2.32** |
| **ACRouter (ours)** | **62.50** | **17.0** | **1.18** |
| LinUCB | 49.82 | 31.1 | 0.96 |
| LinTS | 46.43 | 35.9 | 0.75 |
| DimensionBest | — | — | — |
| kNN Retrieval | 14.29 | 66.7 | 1.45 |
| LogReg | 19.64 | 61.8 | 1.17 |
| RouteLLM-BERT | 21.43 | 59.4 | 1.30 |
| TF-IDF+MLP | 13.39 | 67.9 | 1.17 |
| Qwen3.5-0.8B-Finetuned | 55.36 | 27.2 | 0.74 |
| RouteLLM-MF | 8.93 | 72.7 | 0.94 |
| Always-Opus 4.6 | 57.14 | 26.7 | 0.64 |
| Always-Kimi-K2.5 | 18.75 | 62.3 | 1.22 |
| Always-Qwen3.5-Plus | 2.68 | 80.1 | 0.19 |
| Random | 31.25 | 50.4 | 0.85 |

> DimensionBest is not applicable to OOD: it requires predefined dimension-to-model mapping unavailable for unseen agentic-programming tasks.

> Updated standalone GPT-5.4 backend run resolves **75.00%** of the same OOD split (Table 14), showing this OOD setting also exposes strong backend-level gains.

### 5.3 Key Observations

1. **ACRouter achieves the best AvgPerf on both ID and OOD** by dynamically accumulating context, with lower cost than always choosing Opus-4.6 (Perf/$: 3.79 ID, 1.18 OOD vs. Always-Opus 1.29 ID, 0.64 OOD).
2. **Static learners reach fair AvgPerf on ID but collapse on OOD.** Lightweight classifiers (LogReg, TF-IDF+MLP, RouteLLM-MF, RouteLLM-BERT) drop to 8.93%–21.43% AvgPerf on OOD — lower than Random (31.25%).
3. **Contextual bandits (LinUCB, LinTS) survive OOD better** (49.82% / 46.43%) because they update online, but lag ACRouter due to lacking context-aware reasoning from Orchestrator + Memory.
4. **Cumulative regret ordering:** ACRouter (205.5) < DimensionBest (277.4) < static classifiers (284–317) < bandits (297–307) < single models (387–593).
5. **Cost analysis (ID):** Always-Opus costs $34.02 for 43.83% AvgPerf; ACRouter achieves 49.98% for $13.21.

### 5.4 Limitations

- Provider-side cache hit rates are unobservable; dollar estimates rely on public token prices and measured token usage → **monetary cost serves only as a secondary reference metric**.
- Agentic programming uses **40-step limit** (vs. standard 250-step) for budget tractability.
- C-A-F instantiation uses LLM policy with memory-kNN ensemble; alternative instantiations (advanced parameter-level memory) remain future work.

---

## 6. Conclusion

Agent-as-a-Router formalizes routing as a C-A-F loop, naturally castable as a contextual bandit with cumulative regret as its streaming metric. ACRouter (Orchestrator + Verifier + Memory) attains the lowest cumulative regret on ID stream tasks and is the **only router** that maintains strong performance on OOD agentic-programming. The general principle: **actively closing the information gap through execution-grounded feedback** applies broadly to agent systems routing among heterogeneous tools or models.

---

## Appendix A: ACRouter System Architecture

### A.1 Two-Layer Architecture

#### Decision Layer (Core Modules)

**Orchestrator**
- Central coordinator selecting which candidate model to invoke
- Consults: Memory state, DimensionBest prior, top-10 historical neighbors (cosine kNN), task metadata
- Policy: fine-tuned Qwen3.5-0.8B + heuristic rules via weighted voting

**Verifier**
- Sandbox-native confidence estimator
- Aggregates multiple signal tiers (AST parse, sandbox execution, prompt-embedded tests, rule-based) into unified score $u_i \in [0, 1]$ via Eq. 8
- Produces the verdict written into Memory

**Memory**
- Online vector store (key: task embeddings via `voyage-code-3` / `BGE-large`)
- Value: chosen model, observed performance, monetary cost, Verifier trace
- Retrieval: cosine kNN, threshold **0.5**, **k=10**
- FIFO-bounded at **20K entries**
- Per-task granularity: past successes **and** recent failures of any candidate on similar tasks are visible

#### Tool Layer (Execution Infrastructure)

| Tool | Serves | Description |
|---|---|---|
| **Candidate Models** | Orchestrator | The 8 LLMs available for routing |
| **Routing Tools** | Orchestrator | Dimension-best lookup tables, trained classifiers, LLM-based selectors |
| **Evaluation Tools** | Verifier | AST parser, sandbox runner, self-consistency checker (k-sample output agreement), LLM-as-Judge, prompt-test extractor |
| **Embedding Encoder** | Memory | Maps prompt text to dense vector for kNN retrieval. Options: `voyage-code-3` (API) or `BGE-large` (local open-source) |
| **Infrastructure** | All | Execution sandbox (Docker, timeouts), context parser (extracts dimension/difficulty/language), online Memory updater |

> **Note:** Tool layer components are not independently comparable routing strategies — they are shared execution infrastructure.

### A.2 Per-Loop Reward Weights and Reported Regret

**Canonical evaluation reward:** $(\epsilon_1, \epsilon_2) = (1, -0.1)$

$$r_i(a_i) = s_i(a_i) - 0.1\,\kappa_i(a_i)$$

**Per-task (micro) oracle:**

$$\text{CumReg}_N(\pi) = \sum_{i=1}^{N} \left[\max_{j \in [M]} R_{ij} - R_{i, a_i}\right]$$

This is **not** the gap to a single-best-arm policy committing to one global model — it is the sum of per-task reward gaps against the per-task oracle.

**Bandit reward:** LinUCB and LinTS also use $(\epsilon_1, \epsilon_2) = (1, -0.1)$ for per-arm posterior updates. Every router is scored under the same canonical evaluation reward → CumReg column is directly comparable across method families.

---

## Appendix B: Benchmark and Setup Details

### B.1 API Pricing (Table 6)

All values mirrored verbatim from `configs/model_pricing.json` in the artifact (single source of truth).

| Model | $/M input | $/M output | Tier |
|---|---|---|---|
| Claude Opus 4.6 | $5.00 | $25.00 | premium |
| Claude Sonnet 4.6 | $3.00 | $15.00 | high |
| GPT-5.4 | $2.50 | $15.00 | high |
| Qwen3-Max | $1.20 | $6.00 | mid |
| GLM-5 | $0.88 | $3.22 | mid |
| Kimi-K2.5 | $0.60 | $3.07 | mid |
| Qwen3.5-Plus | $0.40 | $2.40 | low |
| MiniMax-M2.7 | $0.30 | $1.20 | low |

**Cost range:** Claude Opus 4.6 output ($25/M) is **over 20×** MiniMax-M2.7 output ($1.20/M).

### B.2 Self-Hosted Router Token Cost Derivation

**Hardware:** Single NVIDIA H100 80GB HBM3
**Rental rate:** $6.88/GPU-hour (CoreWeave / Lambda Cloud public pricing)
**Engine:** SGLang offline engine, Qwen3.5-0.8B in bfloat16 with flashInfer attention
**Decoding:** Greedy ($T=0$), `max_new_tokens=96`, stop on 3 consecutive newlines
**Batch size:** 64 requests
**Warmup:** First 32 prompts discarded (kernel compilation, KV-cache pre-allocation)
**Measurement window:** 300.5 seconds of steady-state generation over 2,919-task test split

**Measured throughput (over 300.5s):**

| Metric | Value |
|---|---|
| Requests processed | 22,848 |
| Input tokens | 8,393,247 |
| Output tokens | 2,152,601 |
| Combined sustained throughput | ~35,094 tokens/s |
| Input tokens/s | ~27,931 |
| Output tokens/s | ~7,163 |
| Input:Output ratio | ~4:1 (input-heavy routing prompt) |

**Derived per-token cost:**

$$\text{TPS} = \frac{8{,}393{,}247 + 2{,}152{,}601}{300.5\,\text{s}} \approx 35{,}094\,\text{tokens/s} \tag{9}$$

$$\frac{\$6.88}{1\,\text{hr}} \cdot \frac{1\,\text{hr}}{1.263 \times 10^8\,\text{tokens}} \cdot 10^6 = \$0.054\text{ per 1M tokens} \tag{10}$$

**Reproducibility:** ±5% on a different H100 with same SGLang + Qwen3.5-0.8B snapshot.

### B.3 Router Prompt Templates

#### Zero-Shot Prompt (Vanilla — used in main experiments for trained policy)

```
System: You are a coding task router. Your objective is to maximize the
performance-cost trade-off: choose the model that achieves the best quality
for its cost on this task. [8 models listed with capability descriptions,
sorted by cost tier]. Prefer cheaper models when quality is comparable.
Respond with JSON: {"model": "...", "reasoning": "..."}.
```

#### Few-Shot Prompt (3-shot)

Same system prompt as zero-shot, with 3 demonstration examples prepended.

**Example selection strategy:**
1. **Same-dimension priority** — all 3 examples from same dimension as target task; fill remaining from other dimensions if needed
2. **Non-trivial examples only** — exclude tasks where all 8 models achieve identical scores
3. **Oracle labels** — each example shows oracle-best model + full 8-model score distribution
4. **Prompt truncation** — examples truncated to 300 chars; target task included in full
5. **Fixed seed 42** for reproducibility

**Few-shot prompt structure:**
```
## Examples
### Example 1
- Dimension: bug_fixing
- Difficulty: medium
- Language: python
- Prompt: <first 300 chars>...
- Best model: glm-5
- Scores: claude-opus-4-6: 0.72, claude-sonnet-4-6: 0.70, glm-5: 0.73, gpt-5.4: 0.68, ...
### Example 2 / Example 3 [...similar...]
---
## Task to Route
**Dimension**: code_completion
**Difficulty**: hard
**Language**: python
**Prompt**: <full task prompt>
```

**Response parsing fallback chain:**
1. Direct JSON parse
2. Regex extraction from markdown code blocks
3. Model-name string matching
4. Fallback to `claude-sonnet-4-6`

#### +Perf Stats Ablation Prompt

Model capability descriptions replaced with tabular performance data from the probing set:

```
Model capabilities (average scores per dimension):
Claude Opus 4.6: code_gen=0.315, algo=0.254, bug_fix=0.717, completion=0.860,
  refac=0.607, ds=0.142, multi=0.408, underst=0.193, test_gen=0.392.
[... similar for all 8 models]
```

The **Anonymized variant** rewrites every model identifier to `Model-A` through `Model-H` to remove provider-specific prior knowledge.

---

## Appendix C: Supplementary Experiments

### C.1 LLM-as-a-Router Across All 8 Models (Table 7)

**Setup:** All 8 candidate models evaluated as LLM-as-a-Router in 0-shot, 3-shot, and agent-mode on the 2,919-task ID test split.

**Panel A: LLM-as-a-Router (0-shot)**

| Router LLM | AvgPerf% | CumReg ↓ | $Total | Perf/$ ↑ |
|---|---|---|---|---|
| Qwen3.5-Plus | 46.87 | 296.3 | $13.06 | 3.59 |
| Qwen3-Max | 46.60 | 304.1 | $11.82 | 3.94 |
| Kimi-K2.5 | 46.29 | 313.3 | $14.11 | 3.28 |
| GPT-5.4 | 45.91 | 324.0 | $10.26 | 4.48 |
| GLM-5 | 45.42 | 338.7 | $14.56 | 3.12 |
| MiniMax-M2.7 | 42.11 | 435.6 | $16.90 | 2.49 |
| Claude Sonnet 4.6 | 41.41 | 456.4 | $21.02 | 1.97 |
| Claude Opus 4.6 | 39.27 | 518.9 | $21.20 | 1.85 |

**Panel A (cont.): LLM-as-a-Router (3-shot)**

| Router LLM | AvgPerf% | CumReg ↓ | $Total | Perf/$ ↑ |
|---|---|---|---|---|
| GLM-5 | 46.03 | 320.8 | $12.22 | 3.77 |
| Qwen3-Max | 45.80 | 327.7 | $14.63 | 3.13 |
| GPT-5.4 | 45.66 | 331.3 | $9.80 | 4.66 |
| Claude Sonnet 4.6 | 45.53 | 335.3 | $12.01 | 3.79 |
| Qwen3.5-Plus | 45.39 | 339.5 | $12.64 | 3.59 |
| MiniMax-M2.7 | 45.29 | 342.5 | $13.88 | 3.26 |
| Kimi-K2.5 | 45.10 | 348.5 | $17.98 | 2.51 |
| Claude Opus 4.6 | 41.17 | 463.5 | $21.34 | 1.93 |

**Panel B: Agent Online (200-task warm-up, seed=42)**

| Router LLM | AvgPerf% | CumReg ↓ | $Total | Perf/$ ↑ |
|---|---|---|---|---|
| MiniMax-M2.7 | 45.27 | 343.2 | $15.30 | 2.96 |
| GLM-5 | 45.21 | 345.0 | $15.84 | 2.85 |
| Qwen3-Max | 44.92 | 353.7 | $18.00 | 2.50 |
| Qwen3.5-Plus | 43.98 | 381.0 | $16.17 | 2.72 |
| Claude Sonnet 4.6 | 43.65 | 390.7 | $16.84 | 2.59 |
| Kimi-K2.5 | 42.70 | 417.9 | $12.45 | 3.43 |
| GPT-5.4 | 40.95 | 468.9 | $11.24 | 3.64 |
| *Random (ref)* | 38.75 | 533.6 | $15.64 | 2.48 |

**Three key findings from Table 7:**
1. **Coding ability ≠ routing ability:** Claude Opus 4.6 ranks **last** in both 0-shot and 3-shot despite being the strongest individual coder. All 8 LLM routers remain below DimensionBest.
2. **Few-shot prompting does not reliably solve routing:** Slight improvement for some models, but substantial regret to oracle remains.
3. **Agent-mode memory is mixed:** Helps MiniMax-M2.7 but does not uniformly improve stronger prompt-following routers.

### C.2 Qwen Router Scaling Sweep (Table 8)

**Setup:** 5 Qwen3.5 sizes (0.8B / 2B / 4B / 9B / 27B) under unified LoRA fine-tuning:
- Target modules: attention + MLP
- Rank: $r = 16$, $\alpha = 32$, dropout: 0.05
- Evaluated on $n = 2,919$ ID test split (canonical)

| Size | AvgPerf% ↑ | Gap% ↓ | Toks/task | CumReg ↓ |
|---|---|---|---|---|
| 0.8B | 46.41 | 18.6 | 500 | 309.1 |
| 2B | 46.69 | 18.1 | 501 | — |
| 4B | 46.21 | 18.9 | 492 | — |
| 9B | 46.56 | 18.3 | 496 | — |
| 27B | 46.74 | 18.0 | 495 | — |

**Key findings:**
1. **Finetuning is the gate, not size.** Without FT, base Qwen3.5 9B/27B/35B emit malformed routing tokens → parser falls back to `claude-sonnet-4-6` → AvgPerf collapses to ~41.31. Once finetuned, every size lifts to ≥46.21%.
2. **Scale yields diminishing returns.** Across ~30× parameter range, AvgPerf moves only ~0.5 points (46.21–46.74). **Capacity is not the bottleneck.**
3. **0.8B is the defensible cost choice.** Reported in Table 3 as the most cost-efficient finetuned policy.
4. **CumReg not archived for ≥2B sizes** (per-task decisions not preserved); only 0.8B CumReg=309.1 reported.

---

## Appendix D: Supplementary Analyses

### D.1 Model Capability Profiles

Radar chart analysis across 9 dimensions reveals distinctly non-circular shapes:

| Model | Strengths | Weaknesses |
|---|---|---|
| Claude Opus 4.6 | Code completion (0.860), Bug fixing (0.717), Code generation (0.315) | Algorithm design (0.254), Data science (0.142) |
| GLM-5 | Algorithm design (0.472 probing), Bug fixing (0.728 probing) | Code generation, Multi-language |
| Qwen3-Max | Test generation (0.827 probing) | Code generation, Data science |
| Kimi-K2.5 | Data science (0.184), Code understanding (0.195) | Algorithm design |
| GPT-5.4 | Code refactoring (0.644), Test generation (0.764), Code completion (0.639) | Data science |

### D.2 Full Model × Dimension Score Matrices

#### Table 9 — In-Distribution Test Split ($n = 2,919$)

> Bold = dimension-best model learned on probing split. Missing tasks contribute 0 (missing-as-0 convention).

| Model | CdGen | Algo | Bug | Comp | Refac | DS | Multi | Und | TstGn | AVG |
|---|---|---|---|---|---|---|---|---|---|---|
| **Claude Opus 4.6** | **.337** | .275 | **.722** | **.837** | .612 | .109 | **.432** | .190 | .388 | **.438** |
| GPT-5.4 | .285 | .302 | .565 | .663 | **.652** | .061 | .366 | .148 | .744 | .422 |
| Claude Sonnet 4.6 | .311 | .296 | .707 | .729 | .600 | .035 | .431 | .177 | .391 | .413 |
| GLM-5 | .313 | **.265** | **.607** | .561 | .529 | .102 | .378 | .173 | .589 | .378 |
| Qwen3-Max | .285 | **.388** | .648 | .602 | .332 | .096 | .372 | .124 | **.789** | .400 |
| Qwen3.5-Plus | .288 | **.409** | .652 | .531 | .276 | .087 | .382 | .151 | .698 | .372 |
| Kimi-K2.5 | .271 | .280 | .632 | .568 | .372 | **.151** | **.400** | **.185** | .415 | .367 |
| MiniMax-M2.7 | .245 | .069 | .504 | .583 | .610 | .125 | .361 | .179 | .471 | .361 |

#### Table 10 — Probing Set (used to train routers and classifiers)

| Model | CdGen | Algo | Bug | Comp | Refac | DS | Multi | Und | TstGn | AVG |
|---|---|---|---|---|---|---|---|---|---|---|
| **Claude Opus 4.6** | **.315** | .254 | **.717** | **.860** | .607 | **.142** | **.408** | .193 | .392 | **.429** |
| GPT-5.4 | .282 | .257 | .567 | .639 | **.644** | .063 | .346 | .150 | .764 | .412 |
| Claude Sonnet 4.6 | .275 | .258 | .698 | .751 | .615 | .068 | .407 | .180 | .395 | .402 |
| GLM-5 | **.298** | **.472** | **.728** | .537 | .516 | .079 | .362 | .174 | .592 | .399 |
| Qwen3-Max | .262 | .310 | .660 | .591 | .336 | .111 | .350 | .123 | **.827** | .392 |
| Qwen3.5-Plus | .282 | .397 | .666 | .538 | .296 | .114 | .355 | .149 | .714 | .371 |
| Kimi-K2.5 | .269 | .254 | .653 | .590 | .386 | **.184** | **.372** | **.195** | .430 | .370 |
| MiniMax-M2.7 | .239 | .073 | .528 | .563 | **.603** | .145 | .331 | .184 | .494 | .360 |

#### Table 11 — Full Single-Turn Corpus ($n = 9,999$, all splits)

| Model | CdGen | Algo | Bug | Comp | Refac | DS | Multi | Und | TstGn | AVG |
|---|---|---|---|---|---|---|---|---|---|---|
| Claude Opus 4.6 | **.317** | .260 | **.719** | **.851** | .610 | .130 | **.414** | .194 | .396 | **.432** |
| Claude Sonnet 4.6 | .287 | .275 | .701 | .743 | .607 | .054 | .413 | .179 | .396 | .406 |
| GPT-5.4 | .280 | .270 | .565 | .643 | **.646** | .062 | .351 | .150 | .753 | .413 |
| GLM-5 | .297 | **.479** | **.717** | .546 | .521 | .083 | .365 | .174 | .592 | .402 |
| Qwen3-Max | .265 | .336 | .651 | .593 | .335 | .107 | .356 | .123 | **.823** | .394 |
| Qwen3.5-Plus | .282 | .405 | .661 | .533 | .292 | .105 | .362 | .150 | .706 | .371 |
| Kimi-K2.5 | .268 | .265 | .645 | .584 | .380 | **.173** | **.378** | **.194** | .423 | .369 |
| MiniMax-M2.7 | .240 | .070 | .517 | .572 | .603 | .134 | .338 | .183 | .480 | .358 |

**Notable missing-task rates (contributing 0):** GLM-5 ~14.5%, Qwen3.5-Plus ~10.7%, MiniMax-M2.7 ~5.4%, others <2%.

### D.3 Variance Decomposition

**Mutual information** $I(y_t; d(t))$ between oracle-assigned model $y_t = \arg\max_m s(t, m)$ and task dimension $d(t)$ on the ID test split:

- Dimension identity captures **~27% of the entropy** of $y_t$
- Enough to explain why DimensionBest reaches ~0.475 AvgPerf
- Well short of the 0.570 per-task oracle

The remaining **~73% routing signal** lies in per-task content (algorithm choice, API patterns, edge-case handling) — exactly what ACRouter's task-embedding Memory keys on, rather than the lower-resolution dimension hash used by DimensionBest.

### D.4 Router Bias Distribution

**Finding:** The Vanilla 0-shot LLM router's model-selection distribution is **nearly uniform** across dimensions, failing to exploit the strong dimensional structure that DimensionBest leverages.

Providing the per-dimension performance prior (+Perf stats) lets the LLM router match and slightly exceed DimensionBest with no architectural change. This confirms: **information bottleneck, not reasoning bottleneck**.

### D.5 Cost Comparison — Three Deployment Regimes

| Tier | Methods | $Total (ID) | AvgPerf% | Perf/$ |
|---|---|---|---|---|
| **Cheap** | Trained classifiers (LogReg $7.54, RouteLLM-MF $7.46, TF-IDF+MLP $7.69, RouteLLM-BERT $7.59, Qwen-FT $6.81) | $6.81–$7.69 | 46.16–47.26 | 6.11–6.82 |
| **Moderate** | DimensionBest ($12.89), ACRouter ($13.21), online bandits ($10–11) | $10–$13.21 | 46.84–49.98 | 3.69–4.49 |
| **Premium** | Always-Opus ($34.02), Always-Qwen3.5+ ($18.14), Random ($15.64) | $15.64–$34.02 | 37.16–43.83 | 1.29–2.48 |

**Key ID cost insight:** Always-Opus pays $34.02 for 43.83% AvgPerf; ACRouter achieves 49.98% AvgPerf for only $13.21 — **a 6.15 AvgPerf improvement at 61% lower cost**.

**Cheapest floor:** Always-Kimi-K2.5 at $2.90 sets the cost floor with 36.66% AvgPerf (Perf/$: 12.62, highest in Table 3) at a substantial AvgPerf cost.

### D.6 In-Distribution Per-Method Breakdown (Table 12)

Full breakdown for $n = 2,919$ across 9 single-turn coding dimensions including explicit dollar costs:

| Family | Router | AvgPerf% ↑ | CumReg ↓ | $Total | Perf/$ ↑ |
|---|---|---|---|---|---|
| Bound | Oracle | 57.00 | 0 | $6.95 | 8.20 |
| **Agentic** | **ACRouter** | **49.98** | **205.5** | **$13.21** | **3.79** |
| Bandit | LinTS | 46.48 | 307.4 | $10.35 | 4.49 |
| Bandit | LinUCB | 46.84 | 296.9 | $10.69 | 4.38 |
| Heuristic | DimensionBest | 47.50 | 277.4 | $12.89 | 3.69 |
| Heuristic | kNN Retrieval | 47.18 | 286.7 | $7.77 | 6.07 |
| Trained | LogReg | 47.26 | 284.4 | $7.54 | 6.27 |
| Trained | RouteLLM-BERT | 47.22 | 285.5 | $7.59 | 6.22 |
| Trained | TF-IDF+MLP | 46.97 | 292.8 | $7.69 | 6.11 |
| Trained | Qwen3.5-0.8B-FT | 46.41 | 309.1 | $6.81 | 6.82 |
| Trained | RouteLLM-MF | 46.16 | 316.5 | $7.46 | 6.19 |
| Single-Model | Always-Opus 4.6 | 43.83 | 387.1 | $34.02 | 1.29 |
| Single-Model | Always-Kimi-K2.5 | 36.66 | 593.3 | $2.90 | 12.62 |
| Single-Model | Always-Qwen3.5-Plus | 37.16 | 580.2 | $18.14 | 2.05 |
| Single-Model | Random | 38.75 | 533.6 | $15.64 | 2.48 |

### D.7 OOD Per-Method Breakdown (Table 13)

Full breakdown for $n = 176$ agentic-programming OOD tasks:

| Family | Router | AvgPerf% ↑ | CumReg ↓ | $Total | Perf/$ ↑ |
|---|---|---|---|---|---|
| Bound | Oracle | 75.89 | 0 | $32.71 | 2.32 |
| **Agentic** | **ACRouter** | **62.50** | **17.0** | **$52.97** | **1.18** |
| Bandit | LinTS | 46.43 | 35.9 | $61.91 | 0.75 |
| Bandit | LinUCB | 49.82 | 31.1 | $51.90 | 0.96 |
| Heuristic | DimensionBest | — | — | — | — |
| Heuristic | kNN Retrieval | 14.29 | 66.7 | $9.86 | 1.45 |
| Trained | LogReg | 19.64 | 61.8 | $16.79 | 1.17 |
| Trained | RouteLLM-BERT | 21.43 | 59.4 | $16.48 | 1.30 |
| Trained | TF-IDF+MLP | 13.39 | 67.9 | $11.44 | 1.17 |
| Trained | Qwen3.5-0.8B-FT | 55.36 | 27.2 | $74.81 | 0.74 |
| Trained | RouteLLM-MF | 8.93 | 72.7 | $9.50 | 0.94 |
| Single-Model | Always-Opus 4.6 | 57.14 | 26.7 | $89.28 | 0.64 |
| Single-Model | Always-Kimi-K2.5 | 18.75 | 62.3 | $15.37 | 1.22 |
| Single-Model | Always-Qwen3.5-Plus | 2.68 | 80.1 | $14.11 | 0.19 |
| Single-Model | Random | 31.25 | 50.4 | $36.76 | 0.85 |

**Why static baselines collapse on OOD:**
- 9 single-turn probing dimensions: dominated by short, single-file tasks
- OOD set: requires multi-step planning, file navigation, iterative debugging
- Trained classifiers condition on prompt features that **no longer carry the same signal** in the OOD distribution and **cannot acquire new feedback** during evaluation

**OOD AvgPerf column** = harness-graded **resolved-rate** (#resolved/176), NOT the looser `apply_ok` signal (patch applies cleanly without verifying tests pass).

#### Table 14 — Per-Model Score Matrix on 176-Task OOD Set

| Model | Resolved% ↑ | Apply_ok% ↑ | Calls/task | $Total |
|---|---|---|---|---|
| GPT-5.4 (updated) | **75.00** | — | — | $41.04 |
| **Claude Opus 4.6** | **57.14** | 79.46 | 19.0 | $89.28 |
| Claude Sonnet 4.6 | 49.11 | 68.75 | 16.4 | $77.23 |
| GLM-5 | 28.57 | 41.07 | 15.3 | $26.73 |
| Kimi-K2.5 | 18.75 | 26.79 | 12.3 | $15.35 |
| MiniMax-M2.7 | 14.29 | 17.86 | 7.1 | $3.58 |
| Qwen3-Max | 8.93 | 19.64 | 5.9 | $9.58 |
| Qwen3.5-Plus | 2.68 | 5.36 | 15.4 | $14.38 |
| **Per-task oracle** | **75.89** | — | — | $32.71 |

**OOD performance is more strongly ordered by base coding capability** than the ID split (where 5 distinct models serve as dimension-best).

---

## Appendix E: Discussion

### E.1 Why Routing Matters (Permanent Component)

1. **Model complementarity is not transient.** Each new model generation introduces new strengths — GLM-5 on algorithms, Qwen3-Max on test generation, Kimi-K2.5 on data science.
2. **Cost differentials persist.** Most expensive (Opus output $25/M) is **>20× the cheapest** (MiniMax-M2.7 output $1.20/M).
3. As the ecosystem expands with open-source, domain-specialized, and reasoning-specialized models, **routing value increases**.
4. **Routing is a permanent component of agent systems.**

### E.2 Practitioner's Guide: Building Your Own Agent Router

Step-by-step construction kit:

| Step | Action | Notes |
|---|---|---|
| 1 | **Set up tool layer** | Plug in candidate models and configure execution sandbox |
| 2 | **Profile via probing set** | Use CodeRouterBench (or own tasks via C-A-F) to build dimension × model performance matrix |
| 3 | **Start with DimensionBest** | Static Memory + lookup achieves ~83% of oracle AvgPerf at near-zero overhead — your baseline |
| 4 | **Add a classifier** | Swap to trained classifier (LogReg or RouteLLM) for cheaper deployment with comparable AvgPerf; Perf/$ = 6.11–6.82 |
| 5 | **Complete the C-A-F loop (ACRouter)** | When deploying on new distributions, activate all three modules; initialize Memory with available priors |
| 6 | **Customize tools** | Swap evaluation tools in Verifier (domain-specific tests), add custom routing tools |
| 7 | **Extend the benchmark** | New models: generate responses + scoring; new dimensions: task set + scoring function |

### E.3 Beyond Model Routing

The C-A-F loop (observe context → act → receive feedback → update context) is **not specific to model routing**. The same paradigm applies to:
- Tool selection
- API endpoint selection
- Prompt strategy selection
- Thinking-effort allocation
- Skill routing
- Sub-agent routing
- Memory routing
- Effort routing

The paper views coding routing as a concrete first instantiation of a **broader agent decision-making paradigm**.

### E.4 Extensibility Roadmap

| Version | Description |
|---|---|
| **V1 (this release)** | 10 dimensions, 8 models, ~80K execution-verified responses |
| **V2** | New models join by generating responses on existing task set (~11K API calls per model) |
| **V3** | New dimensions join by providing C-A-F triples with a scoring function |
| **Long-term** | Community-maintained living protocol; researchers contribute dimensions, models, routing methods via standardized interfaces |

---

## Appendix F: Representative Task Examples

### Code Generation

**Task 1 (Python):** `pairs_sum_to_zero(l)` — determine if two distinct elements sum to zero.
- Opus: score=1.00 (ORACLE), GPT-5.4: score=1.00, Qwen3.5: score=1.00
- Router decisions: DimensionBest→Opus | LLM 3-shot→Qwen3.5 | Oracle→Opus

**Task 2 (Python):** Find element appearing only once in sorted array.
- Opus: score=0.00 (ORACLE), GPT-5.4: score=0.00, Qwen3.5: score=0.00
- Router decisions: DimensionBest→Opus | LLM 3-shot→Qwen3.5 | Oracle→Opus

### Algorithm Design

**Task 1 (Python):** Minimum operations to make all elements ≥ k.
- All models score 0.00; Router: DimensionBest→Qwen3.5 | LLM 3-shot→Qwen3.5 | Oracle→Opus

**Task 2 (Python):** Dictionary of letters with random integer lists + population standard deviation.
- Opus/GPT-5.4/Qwen3.5: all score=1.00; Router: DimensionBest→Qwen3.5 | Oracle→Opus

### Bug Fixing

**Task 1 (Python3):** Fix `Counter`-based string step count bug.
- Opus: 0.89, GPT-5.4: 0.89, **Qwen3.5: 0.92 (ORACLE)**
- Router: DimensionBest→Opus | LLM 3-shot→Opus | Oracle→Qwen3.5

**Task 2 (Java):** Fix `numJewelsInStones` method.
- **Opus: 1.00 (ORACLE)**, GPT-5.4: 0.82, Qwen3.5: 0.93
- Router: DimensionBest→Opus | LLM 3-shot→Opus | Oracle→Opus

### Code Completion

**Task 1 (Python):** Predict output of `f(' Rock Paper SCISSORS ')` where `f(title)` returns `title.lower()`.
- **Opus: 1.00 (ORACLE)**, GPT-5.4: 1.00, Qwen3.5: 0.00
- Router: DimensionBest→Opus | LLM 3-shot→Opus | Oracle→Opus

**Task 2 (Python):** Trace output of string stripping function given `')'`.
- Opus/GPT-5.4/Qwen3.5: all 1.00; Router: DimensionBest→Opus | LLM 3-shot→Qwen3.5 | Oracle→Opus

### Code Refactoring

**Task 1 (Java):** Refactor/fix tokenized Java method.
- Opus: 0.70, **GPT-5.4: 0.72 (ORACLE)**, Qwen3.5: 0.72
- Router: DimensionBest→GPT-5.4 | LLM 3-shot→Opus | Oracle→GPT-5.4

**Task 2 (Java):** Refactor/fix object processing method.
- **Opus: 0.75 (ORACLE)**, GPT-5.4: 0.61, Qwen3.5: 0.14
- Router: DimensionBest→GPT-5.4 | LLM 3-shot→Opus | Oracle→Opus

### Data Science

**Task 1 (Python):** Plot DataFrame values with line chart, label axes "X" and "Y".
- All models score 0.00; Router: DimensionBest→Opus | LLM 3-shot→Opus | Oracle→Opus

**Task 2 (Python):** Daily turnover line chart function with error handling.
- All models score 0.00; Router: DimensionBest→Opus | LLM 3-shot→Opus | Oracle→Opus

### Multi-Language

**Task 1 (JavaScript):** Concatenate list of strings → `strings.join('')`.
- Opus/GPT-5.4/Qwen3.5: all 0.90; Router: DimensionBest→Opus | Oracle→Opus

**Task 2 (TypeScript):** Sort even-indexed elements while preserving odd-indexed positions.
- All models score 0.00; Router: DimensionBest→Opus | LLM 3-shot→Qwen3.5 | Oracle→Opus

### Code Understanding

**Task 1 (Python):** Summarize `generate_form(args)` function.
- **Opus: 0.10 (ORACLE)**, GPT-5.4: 0.05, Qwen3.5: 0.05
- Router: DimensionBest→Opus | LLM 3-shot→Opus | Oracle→Opus

**Task 2 (Python):** Summarize `set_completer_frame(self, frame=None)` method.
- **Opus: 0.18 (ORACLE)**, GPT-5.4: 0.12, Qwen3.5: 0.13
- Router: DimensionBest→Opus | LLM 3-shot→Opus | Oracle→Opus

### Test Generation

**Task 1 (Python):** Generate tests for `string_sequence(n)`.
- Opus: 0.00, GPT-5.4: 0.00, **Qwen3.5: 1.00 (ORACLE)**
- Router: DimensionBest→GPT-5.4 | LLM 3-shot→Opus | Oracle→Qwen3.5

**Task 2 (Python):** Generate tests for `frequency_lists(list1)`.
- All models score 0.00; Router: DimensionBest→GPT-5.4 | LLM 3-shot→Opus | Oracle→Opus

### Agentic Programming (OOD)

**Task 1 (Python):** Display fixture scope with `pytest --fixtures` (SWE-bench-style).
- **Opus: 0.72 (ORACLE)**, GPT-5.4: 0.65, Qwen3.5: 0.35
- Router: DimensionBest→Opus | LLM 3-shot→GPT-5.4 | Oracle→Opus

**Task 2 (Python):** Fix `.subs` error on `coth(log(tan(x)))` for integer values (SymPy bug fix).
- **Opus: 0.72 (ORACLE)**, GPT-5.4: 0.65, Qwen3.5: 0.35
- Router: DimensionBest→Opus | LLM 3-shot→GPT-5.4 | Oracle→Opus

---

## Core Findings Summary

| Finding | Evidence |
|---|---|
| **Information deficit > reasoning deficit** | +15.3% relative gain from adding per-dimension stats to LLM router; static routers underperform despite strong LLMs |
| **No single backend dominates all coding dimensions** | 5 distinct models serve as dimension-best across 9 dimensions; 12× cost range with complementary strengths |
| **Loop-complete adaptive routing wins on both ID and OOD** | ACRouter achieves lowest CumReg (205.5 ID, 17.0 OOD) and highest AvgPerf (49.98% ID, 62.50% OOD) among all routers |
| **Static learners overfit to training distribution** | Lightweight classifiers: 46–47% on ID → 8.93–21.43% on OOD (below Random 31.25%) |
| **Router capacity is not the bottleneck** | Qwen3.5 scaling 0.8B→27B: only ~0.5 AvgPerf gain; finetuning is the gate |
| **Dimension identity explains ~27% of oracle-choice entropy** | Remainder lies in per-task content; embedding-based Memory addresses this residual |

---

## References (Selected)

| # | Authors | Title | Venue |
|---|---|---|---|
| [1] | Aggarwal et al. | AutoMix: Automatically mixing language models | NeurIPS 2024 |
| [2] | Agrawal & Goyal | Thompson sampling for contextual bandits with linear payoffs | ICML 2013 |
| [3] | Anthropic | Claude Code: An agentic coding tool | 2025 |
| [6] | Austin et al. | Program synthesis with large language models | arXiv 2021 |
| [7] | Cassano et al. | Can it edit? Evaluating LLMs to follow code editing instructions | COLM |
| [9] | Chen et al. | SWE-CI: Evaluating agent capabilities in maintaining codebases via CI | arXiv 2026 |
| [13] | Feng et al. | LongCLI-Bench: Long-horizon agentic programming in CLIs | arXiv 2026 |
| [14] | Gu et al. | CRUXEval: Code reasoning, understanding and execution | ICML 2024 |
| [16] | Hu et al. | LoRA: Low-rank adaptation of large language models | ICLR 2022 |
| [17] | Jain et al. | LiveCodeBench: Holistic evaluation of LLMs for code | ICLR |
| [18] | Jimenez et al. | SWE-Bench: Can LLMs resolve real-world GitHub issues? | ICLR 2023 |
| [19] | Lai et al. | DS-1000: A reliable benchmark for data science code generation | ICML 2023 |
| [21] | Li et al. | LLMRouterBench: A massive benchmark for LLM routing | arXiv 2026 |
| [22] | Li et al. | A contextual-bandit approach to personalized news article recommendation | WWW 2010 |
| [24] | Lu et al. | CodeXGLUE: A ML benchmark for code understanding and generation | NeurIPS 2021 |
| [26] | Ong et al. | RouteLLM: Learning to route LLMs from preference data | ICLR |
| [27] | OpenAI | SWE-bench Verified | 2024 |
| [30] | Qian et al. | ChatDev: Communicative agents for software development | ACL 2024 |
| [31] | Qwen Team | Qwen3.5: Towards native multimodal agents | 2026 |
| [38] | Wang et al. | OpenHands: An open platform for AI software developers | ICLR 2025 |
| [41] | Yang et al. | SWE-agent: Agent-computer interfaces enable automated software engineering | NeurIPS 2024 |
| [44] | Zheng et al. | SGLang: Efficient execution of structured LM programs | NeurIPS 2024 |
| [46] | Zhou et al. | FeatureBench: Benchmarking agentic coding for complex feature development | arXiv 2026 |
| [47] | Zhuo et al. | BigCodeBench: Benchmarking code generation with diverse function calls | arXiv 2024 |
