#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
LOCAL_ROOT="${ACROUTER_LOCAL_ROOT:-$(cd "$REPO_ROOT/.." && pwd)/Agentic_efficiency}"
ADAPTER_DIR="${ADAPTER_DIR:-$LOCAL_ROOT/coding-router/models/finetuned_router_qwen35_08b_v4/adapter}"
HF_REPO="${1:-${HF_REPO:-Lance1573/acrouter-qwen35-08b-router-v45-reconstructed}}"
HF_BIN=(uvx --from huggingface_hub hf)
STAGE_DIR="$(mktemp -d "${TMPDIR:-/tmp}/acrouter-v45-hf.XXXXXX")"

for f in adapter_config.json adapter_model.safetensors tokenizer.json tokenizer_config.json; do
  test -f "$ADAPTER_DIR/$f" || { echo "missing $ADAPTER_DIR/$f" >&2; exit 1; }
  cp "$ADAPTER_DIR/$f" "$STAGE_DIR/$f"
done
cp "$REPO_ROOT/artifacts/v4.5/v4.5_reconstructed_config.json" "$STAGE_DIR/v4.5_reconstructed_config.json"
cp "$REPO_ROOT/artifacts/v4.5/README.md" "$STAGE_DIR/README.md"
if test -f "$ADAPTER_DIR/chat_template.jinja"; then cp "$ADAPTER_DIR/chat_template.jinja" "$STAGE_DIR/chat_template.jinja"; fi
if test -f "$LOCAL_ROOT/coding-router/models/finetuned_router_qwen35_08b_v4/training_config.json"; then cp "$LOCAL_ROOT/coding-router/models/finetuned_router_qwen35_08b_v4/training_config.json" "$STAGE_DIR/training_config.json"; fi
if test -f "$LOCAL_ROOT/coding-router/data/routing/results/finetuned_router_qwen35_08b_v4_metrics.json"; then cp "$LOCAL_ROOT/coding-router/data/routing/results/finetuned_router_qwen35_08b_v4_metrics.json" "$STAGE_DIR/ft_component_metrics.json"; fi

"${HF_BIN[@]}" auth whoami
"${HF_BIN[@]}" repo create "$HF_REPO" --repo-type model --exist-ok
"${HF_BIN[@]}" upload "$HF_REPO" "$STAGE_DIR" . --repo-type model --commit-message "Publish v4.5 reconstructed FT-LLM component"
echo "Uploaded https://huggingface.co/$HF_REPO"
