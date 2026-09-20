#!/usr/bin/env python3
"""Verify the saved v4.5 reconstructed OOD176 decisions against the public matrix."""
from __future__ import annotations

import argparse
import json
from pathlib import Path
from statistics import mean, stdev

ROOT = Path(__file__).resolve().parents[2]
DEFAULT_MATRIX = ROOT / "data/matrices/phase2_ood/unified/matrix_acrouter_ood176.json"
DEFAULT_DECISIONS = ROOT / "artifacts/v4.5/decisions.ood176.extract.jsonl"

def main() -> None:
    p = argparse.ArgumentParser()
    p.add_argument("--matrix", type=Path, default=DEFAULT_MATRIX)
    p.add_argument("--decisions", type=Path, default=DEFAULT_DECISIONS)
    p.add_argument("--condition", default="static+llm+memory")
    args = p.parse_args()
    matrix = json.loads(args.matrix.read_text())
    rows = [json.loads(line) for line in args.decisions.read_text().splitlines() if line.strip()]
    rows = [r for r in rows if r.get("dataset") == "OOD-176" and r.get("condition") == args.condition]
    seeds = sorted({int(r["seed"]) for r in rows})
    if len(seeds) != 10 or len(rows) != 10 * len(matrix["ids"]):
        raise RuntimeError(f"expected 10*176 rows for {args.condition}, got {len(rows)}")
    by_seed = {}
    for seed in seeds:
        selected = [r for r in rows if int(r["seed"]) == seed]
        if len(selected) != len(matrix["ids"]):
            raise RuntimeError(f"seed {seed} has {len(selected)} rows")
        resolved = 0
        for row in selected:
            cell = matrix["matrix"][row["task_id"]][row["chosen_model"]]
            expected = float(bool(cell.get("resolved", False)))
            if float(row["perf"]) != expected:
                raise RuntimeError(f"saved perf mismatch for {row['task_id']}")
            resolved += int(expected)
        by_seed[seed] = resolved / len(selected) * 100.0
    avg = mean(by_seed.values())
    sd = stdev(by_seed.values())
    print(json.dumps({"condition":args.condition,"seeds":seeds,"perf_pct_by_seed":by_seed,"mean_pct":avg,"sd_pct":sd}, indent=2))

if __name__ == "__main__":
    main()
