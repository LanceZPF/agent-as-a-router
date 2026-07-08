<div align="center">

# Agent-as-a-Router

**The official implementations of Agent-as-a-Router: Agentic Model Routing for Coding Tasks.**

<p>
  <a href="https://www.omnisource.cn/agent-as-a-router">
    <img alt="Homepage" src="https://img.shields.io/badge/Homepage-agent--as--a--router-1f6feb?style=for-the-badge">
  </a>
  <a href="https://arxiv.org/abs/2606.22902">
    <img alt="Paper" src="https://img.shields.io/badge/Paper-arXiv%3A2606.22902-b31b1b?style=for-the-badge">
  </a>
  <a href="https://huggingface.co/datasets/Lance1573/CodeRouterBench">
    <img alt="Benchmark" src="https://img.shields.io/badge/Benchmark-CodeRouterBench-ff9f1c?style=for-the-badge">
  </a>
</p>

<p>
  <a href="https://www.omnisource.cn/agent-as-a-router"><strong>Homepage</strong></a>
  |
  <a href="https://arxiv.org/abs/2606.22902"><strong>Paper</strong></a>
  |
  <a href="https://huggingface.co/datasets/Lance1573/CodeRouterBench"><strong>Benchmark</strong></a>
</p>

</div>

This repository also releases **CodeRouterBench**, the benchmark suite used to
evaluate agentic model routing across in-distribution coding tasks and the
current OOD176 agentic-programming task stream.

Paper page: [Hugging Face Daily Papers](https://huggingface.co/papers/2606.22902)
and [arXiv:2606.22902](https://arxiv.org/abs/2606.22902).

ACRouter is a prototype agent-as-a-router framework for comparing an
agent-style router against single-model, heuristic, online-bandit, retrieval,
and trained-policy baselines on coding tasks.

This release is intentionally self-contained. It includes the code, compact
metrics data, saved baseline checkpoints, OOD matrices, reference outputs, and
tests needed to reproduce the reported tables without API keys or live model
calls.

The current public OOD benchmark in this bundle is **OOD176**. The older
OOD112/SWE-MiniSandbox reproduction is kept only as a legacy supplement and is
documented under `data/README.md` and the agentic artifact evidence tables.

## Quick Start

From a fresh machine or fresh Python environment:

```bash
cd open-source-acrouter

conda create -n acrouter python=3.11 -y
conda activate acrouter

python -m pip install --upgrade pip setuptools wheel
python -m pip install -r requirements.txt
python -m pip install -e .

python -m unittest discover -s tests
```

If the tests pass, run the main reproductions:

```bash
# In-distribution ID reproduction.
python scripts/run_id.py --output-dir outputs/tmp/id

# Current OOD176 ACRouter reproduction.
python scripts/run_acrouter_ood176.py --output-dir outputs/tmp/acrouter_ood176

# All OOD176 baselines and the paper-style table.
python scripts/run_baselines_ood176.py --output-dir outputs/tmp/baselines_ood176

# Custom benchmark/model pipeline example.
python scripts/run_pipeline.py --config configs/eval_pipeline.example.json
```

The commands above write to `outputs/tmp/` so the checked-in reference outputs
are not overwritten. Omit `--output-dir` to regenerate the reference locations
under `outputs/`.

## Runtime Integration Highlights

ACRouter also ships with ready-to-adapt runtime integrations for popular
Claude Code/Codex routing tools:

- [`claude-code-router/`](../claude-code-router/) adds gateway-level ACRouter
  support to Claude Code Router, so routing can happen before a request is sent
  to the selected backend model.
- [`cc-switch/`](../cc-switch/) adds proxy-level ACRouter support to cc-switch, so
  model selection can run before static provider/model mapping.

## Demo Routers

The repository includes two hands-on demos for users who want to apply
Agent-as-a-Router to their own coding workflow, not only replay the benchmark.

### Route Coding Problems Through APIs

`demos/api_coding_solver/` lets a user define one or more OpenAI-compatible API
backends plus a model list, then route a programming problem through the
candidates until a configured verifier passes. The default config targets
OpenRouter, but no token is stored in the repository.

```bash
export OPENROUTER_API_KEY="<set-this-in-your-shell>"

# Inspect the request plan without calling a live API.
python demos/api_coding_solver/solve.py \
  --config demos/api_coding_solver/models.example.json \
  --problem-file demos/api_coding_solver/problems/two_sum.txt \
  --dry-run

# Live run against the configured OpenRouter model list.
python demos/api_coding_solver/solve.py \
  --config demos/api_coding_solver/models.example.json \
  --problem-file demos/api_coding_solver/problems/two_sum.txt
```

Add or replace providers in `models.example.json` by changing `base_url`,
`api_key_env`, and `models`. The demo writes every prompt, response,
`solution.py`, verifier result, and final summary under its local `runs/`
folder.

### Route Into Codex, Claude Code, Or opencode

`demos/commercial_cli_router/` is a small router MVP for mature commercial
coding-agent products. It selects Codex, Claude Code, or opencode from a
configurable keyword policy, then invokes the selected local CLI. Users keep
their existing product login and permission model.

```bash
# Show the selected backend and exact rendered command.
python demos/commercial_cli_router/router_mvp.py \
  --prompt "Patch this repository so pytest passes" \
  --dry-run

# Force one backend for a real local project.
python demos/commercial_cli_router/router_mvp.py \
  --tool codex \
  --workdir /path/to/project \
  --prompt "Run the tests and fix the failing parser case"
```

The default command templates are in
`demos/commercial_cli_router/tools.example.json`. A private-by-default npm
wrapper is included under `demos/commercial_cli_router/npm/` for local
`npm link` usage; public npm or product marketplace publication still requires
maintainer account credentials and an explicit release step. The templates also
support optional `command_prefix` and `command_prefix_env` fields, so users can
wrap a backend with `ccswitch` or a similar command switcher without changing
router code.

## Hugging Face Download

The current public benchmark repo is
[`Lance1573/CodeRouterBench`](https://huggingface.co/datasets/Lance1573/CodeRouterBench).
Its root contains the canonical task-by-model tables, and the original nested
matrices are under `raw_matrices/`. The OOD176 replay matrix used by the scripts
is:

```text
raw_matrices/phase2_ood/unified/matrix_acrouter_ood176.json
```

Download the benchmark snapshot into a git-ignored local directory:

```bash
python scripts/download_hf_assets.py --dataset-dir .hf/CodeRouterBench

# Faster OOD176 replay-only download:
python scripts/download_hf_assets.py --minimal --dataset-dir .hf/CodeRouterBench

# Equivalent direct HF CLI command:
hf download Lance1573/CodeRouterBench \
  --repo-type dataset \
  --local-dir .hf/CodeRouterBench
```

`download_hf_assets.py` first tries `huggingface_hub.snapshot_download`. If a
local HTTP decoder fails, it falls back to a standard-library downloader with
`Accept-Encoding: identity`.

Run the OOD reproductions directly from that Hugging Face snapshot:

```bash
python scripts/run_acrouter_ood176.py \
  --hf-dataset-dir .hf/CodeRouterBench \
  --output-dir outputs/tmp/acrouter_ood176_hf

python scripts/run_baselines_ood176.py \
  --hf-dataset-dir .hf/CodeRouterBench \
  --output-dir outputs/tmp/baselines_ood176_hf
```

You can also let the script download the dataset first:

```bash
python scripts/run_acrouter_ood176.py --download-hf --output-dir outputs/tmp/acrouter_ood176_hf

# Or download only the OOD176 replay files before running:
python scripts/run_acrouter_ood176.py \
  --download-hf \
  --minimal-hf \
  --output-dir outputs/tmp/acrouter_ood176_hf

python scripts/run_baselines_ood176.py \
  --download-hf \
  --minimal-hf \
  --output-dir outputs/tmp/baselines_ood176_hf
```

As of the latest public Hugging Face check on 2026-06-23,
`Lance1573/CodeRouterBench` is public, and the optional trained router adapter
is public at
[`Lance1573/acrouter-qwen35-08b-router-lora`](https://huggingface.co/Lance1573/acrouter-qwen35-08b-router-lora).
Offline benchmark replay does not need model weights; download the adapter only
when you want to inspect or integrate the trained Qwen3.5-0.8B router.

```bash
# Benchmark only, no model weights.
python scripts/download_hf_assets.py --minimal --dataset-dir .hf/CodeRouterBench

# Benchmark plus the public trained router adapter.
python scripts/download_hf_assets.py \
  --minimal \
  --with-router-model \
  --dataset-dir .hf/CodeRouterBench \
  --model-dir .hf/router_model

# Or use a custom public/private router model repo.
python scripts/download_hf_assets.py \
  --router-model-repo owner_or_org/router_model_repo \
  --model-dir .hf/router_model
```

## Expected Outputs

`scripts/run_id.py` should print:

```text
ID n=2919 AvgPerf=50.14 CumReg=202.0 $Total=22.31 Perf/$=2.25 rAcc=0.2395
```

`scripts/run_acrouter_ood176.py` should print:

| split | n | AvgPerf% | CumReg | $Total | Perf/$ |
| --- | ---: | ---: | ---: | ---: | ---: |
| OOD176 | 176 | 73.30 | 15.9 | 86.72 | 0.85 |

`scripts/run_baselines_ood176.py` writes:

- `baseline_table.md`: paper-style table.
- `baseline_table.csv`: same table as CSV.
- `baseline_table.tex`: LaTeX table body.
- `baseline_metrics.json`: full metrics, notes, and source metadata.
- `decisions/*.jsonl`: per-task routing decisions for each baseline.

Checked-in reference files live in `outputs/baselines_ood176/`.

## CodeRouterBench Dataset

**CodeRouterBench** is the benchmark release, not a router output dump. Its
core tables are complete task-by-model result matrices:

- `data/coderouterbench/id_results_long.csv`: 9,999 ID tasks x 8 backend models
  = 79,992 result rows.
- `data/coderouterbench/id_probing_results_long.csv`: 7,080 probing tasks x 8
  backend models = 56,640 result rows. This is the merged original train +
  validation set.
- `data/coderouterbench/id_test_results_long.csv`: 2,919 ID test tasks x 8
  backend models = 23,352 result rows.
- `data/coderouterbench/ood176_results_long.csv`: 176 OOD tasks x 8 backend
  models = 1,408 result rows.
- `data/coderouterbench/id_tasks.jsonl` and
  `data/coderouterbench/ood176_tasks.jsonl`: task metadata.
- `data/coderouterbench/models.json`: canonical model list and pricing
  metadata.
- `data/coderouterbench/summary.json`: integrity counts and source paths.

Each result row records the task id, model, score or pass signal, cost, and
token/latency or verifier metadata when available. ACRouter decisions,
baseline traces, and paper tables are derived from these matrices and live
under `outputs/`.

For ID rows, `cost_usd` is computed from `data/id/tokens.jsonl` and
`data/matrices/phase1_id/model_pricing.json`; it is not copied from the legacy
compact observation matrix. Rows without token records are marked with
`cost_source=missing_token_record_zero_total` when the compact log records zero
total tokens. OOD176 rows are recomputed from `in_tok`, `out_tok`, and the same
pricing table.

Rebuild the user-facing benchmark tables from the nested source matrices:

```bash
python scripts/export_coderouterbench.py
```

## Add New Models Or Tasks

Use `scripts/run_pipeline.py` when you want to evaluate a new benchmark, a new
model set, or both. The pipeline accepts either:

- a ready-made matrix with the same schema as
  `data/matrices/phase2_ood/unified/matrix_acrouter_ood176.json`, or
- `tasks.jsonl` plus `model_results.jsonl`, which the script converts into that
  matrix schema before scoring.

Run the bundled toy example:

```bash
python scripts/run_pipeline.py --config configs/eval_pipeline.example.json
```

The config names models, optional prices, task/result files, and evaluations:

```json
{
  "tasks_path": "../examples/custom_benchmark/tasks.jsonl",
  "results_path": "../examples/custom_benchmark/model_results.jsonl",
  "models": {
    "toy-fast": {"input_per_1m": 5.00, "output_per_1m": 15.00},
    "toy-strong": {"input_per_1m": 100.00, "output_per_1m": 300.00}
  },
  "evaluations": [
    {
      "name": "ACRouter custom cascade",
      "type": "verify_and_escalate",
      "cheap_chain": ["toy-fast"],
      "escalate_to": "toy-strong",
      "k": 1
    },
    {"name": "Always toy-strong", "type": "always", "model": "toy-strong"},
    {"name": "Oracle", "type": "oracle"}
  ]
}
```

`model_results.jsonl` rows should include at least `task_id`, `model`, and
either `resolved` or `score`. Token fields are optional but let the pipeline
compute cost from the config:

```json
{"task_id":"my_task","model":"my_model","resolved":true,"apply_ok":true,"input_tokens":1200,"output_tokens":220}
```

The pipeline writes `matrix.json`, `summary.csv`, `summary.md`,
`metrics/*.json`, and `decisions/*.jsonl` under the configured output directory.
This is the recommended path for adding public benchmark tasks or model results
without touching the built-in OOD176 files.

## Inference Integration

For production or research workflows, import `ACRouter` and provide your own
model caller plus verifier:

```python
from acrouter_repro.inference import ACRouter

router = ACRouter(
    candidate_models=["cheap-model", "strong-model"],
    cheap_chain=["cheap-model"],
    escalate_to="strong-model",
    k=1,
)

decision = router.run_with_verifier(
    task={"task_id": "task_001", "dimension": "bug_fixing", "prompt": "..."},
    call_model=lambda model, task: call_your_backend(model, task),
    verify=lambda response, task, model: run_your_checks(response, task),
)

print(decision.chosen_model, decision.final_response)
```

A complete mock example is available at `examples/inference_demo.py`. Use
`router.route(task)` instead if your workflow wants only the model choice and
will handle execution separately.

## Agentic Artifacts

For automated readers and coding agents, `agentic-artifacts/` provides a compact
research-artifact entry layer over this repository. Start with:

```text
agentic-artifacts/PAPER.md
agentic-artifacts/manifest.json
agentic-artifacts/evidence/tables/score_matrix.md
```

That folder mirrors the important claims, experiment scope, key metrics,
baseline table, matrix pointers, reproduction commands, and design trace using
small Markdown, JSON, YAML, CSV, and HTML files. It does not replace the
canonical data under `data/` or reference outputs under `outputs/`; it points to
them with relative paths so agents can load only the evidence they need.

## Repository Layout

```text
agentic-artifacts/               Agent-readable manifest, claims, evidence map
configs/eval_pipeline.example.json Custom evaluation pipeline example
demos/api_coding_solver/          API-backed coding problem router demo
demos/commercial_cli_router/      Codex/Claude Code/opencode router MVP
examples/                         Custom benchmark and inference demos
src/acrouter_repro/              ACRouter reproduction package
src/routing/                     Baseline router implementations
scripts/run_id.py                ID ACRouter entrypoint
scripts/run_acrouter_ood176.py   ACRouter replay on OOD176
scripts/run_baselines_ood176.py  OOD176 baseline replay
scripts/run_pipeline.py          Config-driven custom model/task evaluation
scripts/export_coderouterbench.py Export task-by-model benchmark tables
tests/                           Unit and bundle-integrity tests

data/coderouterbench/            Canonical CodeRouterBench task x model tables
data/id/                         Phase-1 compact ID labels, splits, tokens
data/ood/                        Legacy OOD112 matrix, patches, verifier cache
data/matrices/phase1_acrouter_v2 Phase-1 observation/response matrices
data/matrices/phase2_ood/        Old112, New64, and unified OOD176 matrices
data/baseline_inputs/            Baseline decisions/checkpoints for OOD176 replay
outputs/                         Checked-in reference outputs
```

The command `python -m pip install -e .` makes `acrouter_repro` importable for
interactive use and downstream workflows. The repository is still script-first:
entrypoints add `src/` to `PYTHONPATH` automatically. If you choose not to
install the package, use:

```bash
export PYTHONPATH="$PWD/src:$PWD"
```

## CodeRouterBench Data

The release keeps data needed for offline scoring and reproduction. The
canonical public benchmark files are in `data/coderouterbench/`:

- `id_results_long.csv`: one row per ID task/model result.
- `id_probing_results_long.csv`: original train + validation merged into the
  probing set.
- `id_test_results_long.csv`: held-out ID test set, labeled `id_test`.
- `ood176_results_long.csv`: one row per OOD176 task/model result.
- `id_tasks.jsonl` and `ood176_tasks.jsonl`: task metadata.
- `models.json`: the eight canonical backend models and USD pricing metadata.
- `README.md`: a compact dataset card for Hugging Face Dataset uploads.

The nested source matrices remain available for audit and reproduction:

- `data/matrices/phase1_acrouter_v2/obs_matrix_clean.json` is the complete
  9,999-task x 8-model ID observation matrix.
- `data/matrices/phase1_acrouter_v2/response_matrix.json` stores the compact
  phase-1 response matrix used by the reproduction bundle.
- `data/matrices/phase2_ood/unified/matrix_acrouter_ood176.json` is the
  complete 176-task x 8-model OOD176 scoring matrix.
- `data/matrices/phase2_ood/raw/new64/matrix.json` records the filtered New64
  subset: FeatureBench 49 + LongCLI 14 + SWE-CI 1. The excluded 8 SWE-CI task
  IDs are recorded in the same JSON file.
- `data/id/` contains task dimensions, train/val/test splits, legacy compact
  labels, token counts, and saved voter decisions. Prefer
  `data/coderouterbench/id_results_long.csv` for public benchmark consumption.
- `data/ood/` contains the legacy OOD112 SWE-MiniSandbox matrix, patch-only
  model submissions, and a hash-checked sandbox cache for supplementary
  reproduction.

To rebuild the OOD176 unified matrix from the bundled raw snapshots:

```bash
python scripts/build_ood176_dataset.py
```

To overwrite and republish the dataset on Hugging Face, run from the repository
root:

```bash
python -m pip install -U huggingface_hub
hf auth login

bash scripts/upload_coderouterbench_hf.sh
```

The script stages the dataset card, canonical CodeRouterBench tables, raw
matrices, reference outputs, evidence artifacts, config example, and custom
benchmark example. It deliberately excludes `outputs/tmp/` validation scratch
runs. The final upload performs one `hf upload ... --delete "*"` commit so the
remote dataset is overwritten instead of appended to. To target another dataset
repo:

```bash
bash scripts/upload_coderouterbench_hf.sh owner_or_org/CodeRouterBench
```

To publish the trained Qwen3.5-0.8B router LoRA adapter, keep the local
training export next to this repository at `../Agentic_efficiency` or set
`ACROUTER_LOCAL_ROOT` to that directory, then run:

```bash
bash scripts/upload_qwen08b_router_hf.sh
```

The default target is
`Lance1573/acrouter-qwen35-08b-router-lora`. The script uploads only the PEFT
adapter, tokenizer assets, compact training config, evaluation metrics, and a
model card. It rewrites local base-model paths to the public
`Qwen/Qwen3.5-0.8B` identifier before upload. To target another model repo:

```bash
bash scripts/upload_qwen08b_router_hf.sh owner_or_org/acrouter-qwen35-08b-router-lora
```

## Dependency Sets

`requirements.txt` is the recommended default environment. It covers tests,
ACRouter reproduction, OOD176 baseline replay, saved trained-router checkpoints,
and legacy baseline training utilities.

```bash
python -m pip install -r requirements.txt
```

`requirements-sandbox.txt` extends the default environment with Python packages
needed for live SWE-bench or legacy SWE-MiniSandbox grading. You only need it if
you want to re-run Docker or Apptainer verification instead of using the bundled
hash-checked cache.

```bash
python -m pip install -r requirements-sandbox.txt
```

Live sandbox grading also needs non-Python assets:

- Docker or Apptainer installed on the host.
- SWE-bench Verified parquet data.
- Cached SWE-bench evaluation images or SIF files.

The default cached reproduction does not need those assets.

## Common Commands

Run the cleaned ID reproduction:

```bash
python scripts/run_id.py --output-dir outputs/tmp/id
```

Run the secret scanner:

```bash
bash scripts/check_no_secrets.sh
```

## Notes And Caveats

- No API keys are required. The release does not call external model APIs.
- `scripts/build_compact_data.py`, `scripts/build_ood_patch_bundle.py`, and
  `scripts/build_sandbox_cache.py` are maintainer scripts for rebuilding compact
  data from raw local experiment outputs. Normal users do not need them.

## Citation

If you use this bundle, please cite the associated arXiv paper.

```bibtex
@article{agent2026zhou,
  title         = {Agent-as-a-Router: Agentic Model Routing for Coding Tasks},
  author        = {Pengfei Zhou, Zhiwei Tang, Yixing Ma, Jiasheng Tang, Yizeng Han, Zhenglin Wan, Fanqing Meng, Wei Wang, Bohan Zhuang, Wangbo Zhao, Yang You},
  journal       = {arXiv preprint arXiv:2606.22902},
  year          = {2026},
  archivePrefix = {arXiv},
  eprint        = {2606.22902},
  url           = {https://arxiv.org/abs/2606.22902},
  note          = {Hugging Face Daily Papers: https://huggingface.co/papers/2606.22902},
}
```
