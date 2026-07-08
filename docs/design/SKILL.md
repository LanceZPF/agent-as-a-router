---
name: agenticrouter-design
description: Use this skill to generate well-branded interfaces and assets for AgenticRouter (the ACRouter coding-task model router and its Windows tray dashboard), either for production or throwaway prototypes/mocks/etc. Contains essential design guidelines, colors, type, fonts, assets, and UI kit components for prototyping.
user-invocable: true
---

Read the README.md file within this skill, and explore the other available files.

If creating visual artifacts (slides, mocks, throwaway prototypes, etc), copy assets out and create static HTML files for the user to view. If working on production code, you can copy assets and read the rules here to become an expert in designing with this brand.

If the user invokes this skill without any other guidance, ask them what they want to build or design, ask some questions, and act as an expert designer who outputs HTML artifacts _or_ production code, depending on the need.

Key things to know before you start:

- This is a **dark-theme-only, data-dense ops console** (routing/cost/governance telemetry for an LLM router), not a marketing product — keep copy terse and numeric, never adjective-heavy.
- Two fonts only: Inter for UI text, JetBrains Mono for every number/ID/timestamp. Never mix these up.
- No logo exists in the source — use plain type for the brand name; the only real vector asset is a small app-tray icon (`assets/appicon.svg`), not a wordmark.
- Colors: slate neutrals + sky (accent) / emerald (positive) / amber (warning) / red (critical), plus a fixed 12-color deterministic per-agent palette. See `tokens/colors.css`.
- Motion is minimal and functional only — a single pulsing "live" dot and a one-shot attention flash; no other looping or decorative animation.
- Components live in `components/`; the full interactive dashboard recreation lives in `ui_kits/dashboard/`.
