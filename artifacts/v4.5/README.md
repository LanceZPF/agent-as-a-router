---
license: apache-2.0
base_model:
  - Qwen/Qwen3.5-0.8B
library_name: peft
pipeline_tag: text-generation
tags:
  - peft
  - lora
  - qwen3.5
  - model-routing
  - agent-as-a-router
---

# v4.5 reconstructed release

This directory records the reproducible v4.5 reconstructed mechanism used by the `release/v4.5-reconstructed` branch. It combines the v4 Qwen3.5-0.8B LoRA voter, static voters, online top-10 Memory-kNN, and a fixed Claude Opus 4.6 prior.

The Opus prior weight is 1.79, recovered by matching the archived seed-42 aggregate because the original v4.5 runner and exact prior weight are unavailable. This is therefore an aggregate-matched reconstruction, not an exact task-level replay of the missing original runner.

The corresponding Hugging Face model repository contains the v4 Qwen3.5-0.8B LoRA adapter used as the FT-LLM component. It is deliberately labeled as a component checkpoint and is not presented as an independent v4.5 weight.

| split | result |
| --- | ---: |
| ID (n=2919) | 48.59 ± 0.04% |
| OOD-112 | 63.75 ± 0.46% |
| OOD-176 | 63.07 ± 0.85% |

Use `python scripts/replay_v45_ood176.py` to verify the saved OOD176 decision records against the public matrix.
