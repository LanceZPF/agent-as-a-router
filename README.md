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
  <a href="https://huggingface.co/datasets/Lance1573/CodeRouterBench"><strong>CodeRouterBench</strong></a>
</p>

</div>

ACRouter routes coding tasks to different backend models under a
performance-cost tradeoff. This repository contains the reproducible
implementation, CodeRouterBench data, reference outputs, and small demos for
plugging the router into your own workflow.

Offline reproduction does not require API keys or live model calls. The current
public benchmark is **OOD176**. Older OOD112/SWE-MiniSandbox assets are kept as
legacy supplementary data under `data/ood/`.

For the full pre-cleanup guide with maintainer notes, extended data details,
and publishing commands, see [docs/HANDBOOK.md](docs/HANDBOOK.md).

## Runtime Integration Highlights

ACRouter also ships with ready-to-adapt runtime integrations for popular
Claude Code/Codex routing tools:

- [`claude-code-router/`](claude-code-router/) adds gateway-level ACRouter
  support to Claude Code Router, so routing can happen before a request is sent
  to the selected backend model.
- [`cc-switch/`](cc-switch/) adds proxy-level ACRouter support to cc-switch, so
  model selection can run before static provider/model mapping.

## Quick Start

```bash
cd open-source-acrouter

conda create -n acrouter python=3.11 -y
conda activate acrouter

python -m pip install --upgrade pip setuptools wheel
python -m pip install -r requirements.txt
python -m pip install -e .

python -m unittest discover -s tests
```

## Reproduce The Main Results

```bash
# In-distribution ID test replay.
python scripts/run_id.py --output-dir outputs/tmp/id

# ACRouter on OOD176.
python scripts/run_acrouter_ood176.py --output-dir outputs/tmp/acrouter_ood176

# OOD176 baselines and paper-style table.
python scripts/run_baselines_ood176.py --output-dir outputs/tmp/baselines_ood176
```

Expected headline outputs:

```text
ID n=2919 AvgPerf=50.14 CumReg=202.0 $Total=22.31 Perf/$=2.25 rAcc=0.2395
ACRouter-OOD176 n=176 AvgPerf=73.30 CumReg=15.9 $Total=86.72 Perf/$=0.85
```

All commands above write to `outputs/tmp/` so checked-in reference outputs are
not overwritten.

## v4.5 Reconstructed Release

The `release/v4.5-reconstructed` branch packages the reproducible v4.5
mechanism: the v4 Qwen3.5-0.8B LoRA voter, static voters, online top-10
Memory-kNN, and a fixed Claude Opus 4.6 prior. The prior weight is 1.79,
recovered by matching the archived seed-42 aggregate because the original
v4.5 runner and exact prior weight are unavailable. This is an
aggregate-matched reconstruction, not a new standalone v4.5 checkpoint.

```bash
git switch release/v4.5-reconstructed
python scripts/replay_v45_ood176.py
```

The replay verifies the saved OOD176 decisions against the public matrix and
reports `63.07 +/- 0.85%` for Static + LLM + Memory. The corresponding
component adapter is published at
[`Lance1573/acrouter-qwen35-08b-router-v45-reconstructed`](https://huggingface.co/Lance1573/acrouter-qwen35-08b-router-v45-reconstructed).
It is the v4 LoRA FT-LLM component; the orchestration configuration and
reconstruction provenance live under [`artifacts/v4.5/`](artifacts/v4.5/).

## Hugging Face Assets

Dataset:
[`Lance1573/CodeRouterBench`](https://huggingface.co/datasets/Lance1573/CodeRouterBench)

Optional trained router adapter:
[`Lance1573/acrouter-qwen35-08b-router-lora`](https://huggingface.co/Lance1573/acrouter-qwen35-08b-router-lora)

Download only the files needed for OOD176 replay:

```bash
python scripts/download_hf_assets.py --minimal --dataset-dir .hf/CodeRouterBench
```

Download the benchmark plus the optional trained router adapter:

```bash
python scripts/download_hf_assets.py \
  --minimal \
  --with-router-model \
  --dataset-dir .hf/CodeRouterBench \
  --model-dir .hf/router_model
```

Run directly from a downloaded Hugging Face snapshot:

```bash
python scripts/run_acrouter_ood176.py \
  --hf-dataset-dir .hf/CodeRouterBench \
  --output-dir outputs/tmp/acrouter_ood176_hf

python scripts/run_baselines_ood176.py \
  --hf-dataset-dir .hf/CodeRouterBench \
  --output-dir outputs/tmp/baselines_ood176_hf
```

The replay matrix used by these commands is:

```text
raw_matrices/phase2_ood/unified/matrix_acrouter_ood176.json
```

## Demo Routers

### API Coding Solver

`demos/api_coding_solver/` routes one programming problem through an
OpenRouter/OpenAI-compatible model list until a verifier passes.

```bash
export OPENROUTER_API_KEY="<set-this-in-your-shell>"

python demos/api_coding_solver/solve.py \
  --config demos/api_coding_solver/models.example.json \
  --problem-file demos/api_coding_solver/problems/two_sum.txt \
  --dry-run

python demos/api_coding_solver/solve.py \
  --config demos/api_coding_solver/models.example.json \
  --problem-file demos/api_coding_solver/problems/two_sum.txt
```

Edit `demos/api_coding_solver/models.example.json` to add providers, model
names, or a stronger verifier command.

### A Router Demo for Codex / Claude Code / Opencode 

`demos/commercial_cli_router/` routes a prompt into a local Codex, Claude Code,
or Opencode CLI command.

```bash
python demos/commercial_cli_router/router_mvp.py \
  --prompt "Patch this repository so pytest passes" \
  --dry-run

python demos/commercial_cli_router/router_mvp.py \
  --tool codex \
  --workdir /path/to/project \
  --prompt "Run the tests and fix the failing parser case"
```

Command templates live in
`demos/commercial_cli_router/tools.example.json`. A private local npm wrapper is
available under `demos/commercial_cli_router/npm/` for `npm link` workflows.
Set `ACROUTER_CODEX_PREFIX`, `ACROUTER_CLAUDE_PREFIX`, or
`ACROUTER_OPENCODE_PREFIX` to wrap a backend with `ccswitch` or another command
switcher.

## Add Your Own Benchmark Or Models

Use the config-driven pipeline when you already have task/model results:

```bash
python scripts/run_pipeline.py --config configs/eval_pipeline.example.json
```

The config can point to either a ready matrix or `tasks.jsonl` plus
`model_results.jsonl`. Result rows should include `task_id`, `model`, and
either `resolved` or `score`; token fields are optional but allow cost
calculation.

For inference-time integration, import `ACRouter`:

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

See `examples/inference_demo.py` for a complete mock workflow.

## Data And Pricing

CodeRouterBench is a task-by-model benchmark release:

- `data/coderouterbench/id_results_long.csv`: 9,999 ID tasks x 8 models.
- `data/coderouterbench/ood176_results_long.csv`: 176 OOD tasks x 8 models.
- `data/coderouterbench/id_tasks.jsonl` and `ood176_tasks.jsonl`: task metadata.
- `data/coderouterbench/models.json`: canonical backend models and USD pricing.
- `outputs/baselines_ood176/`: checked-in reference tables and decisions.

All bundled USD costs are computed from
`data/matrices/phase1_id/model_pricing.json`, the pricing table mirrored from
the internal release source.

The public OOD176 matrix is under
`data/matrices/phase2_ood/unified/matrix_acrouter_ood176.json`. Raw old112 and
new64 snapshots remain under `data/matrices/phase2_ood/raw/`.

## Project Layout

```text
configs/                       Example evaluation pipeline config
demos/                         API and commercial-CLI router demos
examples/                      Custom benchmark and inference examples
src/acrouter_repro/            Reproduction and inference package
src/routing/                   Baseline router implementations
scripts/                       Replay, export, download, and pipeline scripts
tests/                         Unit and bundle-integrity tests

data/coderouterbench/          Public CodeRouterBench tables
data/matrices/                 ID/OOD source matrices and pricing
outputs/                       Checked-in reference outputs
agentic-artifacts/             Agent-readable research evidence
```

## Development Checks

```bash
python -m unittest discover -s tests
python -m py_compile $(find scripts src tests examples demos -name '*.py' -print)
bash scripts/check_no_secrets.sh
```

Install `requirements-sandbox.txt` only if you need to run live Docker or
Apptainer verification. The default offline replay uses cached results.

## Citation

```bibtex
@article{agent2026zhou,
  title         = {Agent-as-a-Router: Agentic Model Routing for Coding Tasks},
  author        = {Pengfei Zhou, Zhiwei Tang, Yixing Ma, Jiasheng Tang, Yizeng Han, Zhenglin Wan, Fanqing Meng, Wei Wang, Bohan Zhuang, Wangbo Zhao, Yang You},
  journal       = {arXiv preprint arXiv:2606.22902},
  year          = {2026},
  archivePrefix = {arXiv},
  eprint        = {2606.22902},
  url           = {https://arxiv.org/abs/2606.22902}
}
```
