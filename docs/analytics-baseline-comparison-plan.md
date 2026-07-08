# Analytics Tab (MAUI): Conversation vs. Theoretical Baseline Comparison Chart — Implementation Plan

> **Status:** Implemented (2026-07-06). See `docs/architecture.md` and `docs/analytics-metrics.md` for the as-built reference; one deviation: `AnalyticsViewModel` lives in Core (not the App head) so it stays unit-testable on Linux.
> **Date:** 2026-07-05
> **Scope:** `davidpizon/AgenticRouter.Gui`

## Context

The AgenticRouter GUI is being rewritten in **.NET MAUI**. This repo currently contains only the original React/Vite prototype as a flat bolt.new export with scrambled filenames (e.g. the real Analytics component lives in a file named `download`, the real `package.json` in `tsconfig.node.json`) — it cannot build and serves as the **design reference** (dark slate theme, 4 tabs: Live Stream, Cost Analytics, Model Distribution, Governance).

Goal: give the **Analytics ("Cost Analytics") tab** a line-graph timeline comparing actual conversation metrics against a theoretical baseline — a "hockey stick" chart where X = session turns and Y = a user-selected metric. Selectors: (a) a specific conversation or all conversations in aggregate; (b) one of the seven agreed metrics. Includes `/docs` documentation and unit tests at 80% coverage.

## Requirements confirmed by the user

- **Platform**: .NET MAUI (the GUI "is to be or has been rewritten in MAUI").
- **Metrics dropdown** — the seven agreed metrics, rank order (1 = most important):
  1. Routing ROI (cost savings from the routing decision)
  2. Total turn cost ($)
  3. Prompt + Completion tokens (the primary hockey-stick curve)
  4. Tool execution loop count (steps per turn)
  5. Cache hit rate (prompt caching)
  6. Time-to-First-Token (TTFT) / routing latency
  7. Context buffer margin (headroom remaining; stored as `ContextUsedPercent` = % of context window used, so margin = 100 − used)

## Open question / assumptions

1. **No MAUI code exists in this repo.** **Assumption: scaffold the MAUI solution here in `AgenticRouter.Gui`.** If the MAUI rewrite lives in another repo, re-target this plan — the domain/metric design carries over unchanged.
2. **Charting library**: LiveCharts2 (`LiveChartsCore.SkiaSharpView.Maui`, MIT, SkiaSharp-based) — free, actively maintained, supports multi-series line charts with dashed strokes for the baseline. (Alternatives: Syncfusion community license, Microcharts — less suitable.)
3. **Data**: no backend exists; a mock data service ports and extends the prototype's mock data. "Conversation" = turns sharing a `SessionId`, ordered by timestamp; "turn" = index within that ordering.
4. **Environment**: no .NET SDK in the Linux build container — Phase 0 installs it. A MAUI app cannot *run* on Linux, so all feature logic lives in a plain .NET library that builds/tests there; visual verification of the MAUI head happens on a developer machine (or CI with MAUI workloads).
5. **Scope**: the MAUI scaffold mirrors the prototype's 4 tabs, but only the Analytics tab is fully implemented; the other three get placeholder pages referencing the prototype.

## Phase 0 — Toolchain + solution scaffold

1. Install .NET SDK (latest LTS via `dotnet-install.sh`); attempt `dotnet workload install maui-android` for head-project compilation (best-effort — if workload/Android SDK install isn't feasible in the container, the MAUI head is still committed and verified on a developer machine; everything below is structured so tests don't depend on it).
2. Tidy the repo: move the React prototype files into `prototype/` with their **verified real names** (mapping recorded in `docs/repo-restructure.md` — e.g. `download` → `prototype/src/components/CostAnalytics.tsx`, `tsconfig.node.json` → `prototype/package.json`), so the root is free for the .NET solution.
3. Scaffold:
   - `AgenticRouter.Gui.sln`
   - `src/AgenticRouter.Gui.Core/` — plain class library (`net8.0`): domain models, metric registry, analytics computations, mock data. **No UI dependencies** — this is where 80% coverage is earned and verified on Linux.
   - `src/AgenticRouter.Gui.App/` — MAUI app (Shell with 4 tabs, dark slate theme ported from the prototype: bg `#0f172a`, cards `#1e293b`, borders `#334155`, accent green `#10b981`, baseline amber `#f59e0b`). References Core, `CommunityToolkit.Mvvm`, LiveCharts2. ViewModels live here but stay UI-framework-free (plain observable classes) so they're also unit-testable.
   - `tests/AgenticRouter.Gui.Core.Tests/` — xUnit + coverlet.msbuild.
   - **Gate:** `dotnet build` (Core + tests) and `dotnet test` pass before feature work.

## Phase 1 — Core domain + analytics computation (`src/AgenticRouter.Gui.Core`)

**Models** (`Models/RoutingEntry.cs`, ported from the prototype's `RoutingEntry` type plus the four fields the agreed metrics need): `Id, SessionId, TraceId, Agent, Model, IsFallback, PromptTokens, CompletionTokens, ActualCost, WorstCaseCost, SavingsAmount, SavingsPercent, Timestamp`, **new:** `ToolLoopCount` (steps per turn), `CacheHitRate` (0–1), `TtftMs`, `ContextUsedPercent` (0–100).

**Mock data** (`Data/IRoutingDataService.cs` + `MockRoutingDataService.cs`): port the prototype's 8 entries, backfill new fields, and add 4 multi-turn conversations (6–10 turns, reused `SessionId`s like `e89a2bc` plus new ones). Shape token/cost series so later turns accumulate context (hockey-stick growth).

**Analytics** (`Analytics/MetricRegistry.cs`, `Analytics/AnalyticsService.cs` — pure, static-friendly, fully unit-testable):

- `MetricRegistry.All`: the seven metrics in rank order — `{ Id, Label, Unit, Kind (Cumulative | PerTurn), Format }`.
- `ListConversations(entries)` → distinct sessions (id, agent label, turn count) for the picker.
- `ComputeComparisonSeries(entries, ConversationSelection (sessionId | All), metricId)` → `IReadOnlyList<ComparisonPoint { Turn, Actual, Baseline }>`.
- Documented baseline constants (`Analytics/BaselineDefaults.cs`): `TargetReduction = 0.75` (rounded from the prototype header's "Avg. Cost Reduction 74.20%"), `PremiumVerbosity = 1.4`, `BaselineStepsPerTurn = 3`, `TargetCacheHitRate = 0.70`, `TargetTtftMs = 800` (cited in prototype routing-step copy), `ContextSafetyLimit = 80` (%).

Metric/baseline formulas (per turn *t*; Σ = running sum over turns 1..t):

| # | Metric | Actual line | Theoretical baseline line |
|---|---|---|---|
| 1 | Routing ROI ($ saved) | Σ `SavingsAmount` | `TargetReduction` × Σ `WorstCaseCost` (target optimum) |
| 2 | Total turn cost ($) | Σ `ActualCost` | Σ `WorstCaseCost` (un-routed premium model) |
| 3 | Prompt + Completion tokens | Σ (prompt + completion) | Σ (prompt + `PremiumVerbosity` × completion) |
| 4 | Tool execution loop count | `ToolLoopCount` at turn *t* | flat `BaselineStepsPerTurn` |
| 5 | Cache hit rate (%) | running mean of `CacheHitRate` × 100 | flat `TargetCacheHitRate` × 100 |
| 6 | TTFT / routing latency (ms) | `TtftMs` at turn *t* | flat `TargetTtftMs` |
| 7 | Context buffer margin (%) | `100 − ContextUsedPercent` at turn *t* (remaining headroom) | flat `100 − ContextSafetyLimit` (= 20% minimum margin) |

Aggregate mode (All): per turn index, the **mean** across all conversations that have that turn.

## Phase 2 — MAUI UI (`src/AgenticRouter.Gui.App`)

- `AppShell.xaml`: `TabBar` with the 4 prototype tabs; Live Stream / Model Distribution / Governance are placeholder `ContentPage`s.
- `ViewModels/AnalyticsViewModel.cs` (CommunityToolkit.Mvvm `ObservableObject`): `Conversations`, `Metrics`, `SelectedConversation`, `SelectedMetric` (`[ObservableProperty]`, defaults: All conversations + Routing ROI), computed `Series` (plain `ComparisonPoint` list from `AnalyticsService`), plus header readout (final actual value + delta vs baseline, formatted per metric unit). **No LiveCharts types in the ViewModel** — keeps it testable.
- `Views/AnalyticsPage.xaml`: two `Picker`s (conversation, metric) bound to the ViewModel, header readout, and a LiveCharts2 `CartesianChart`. A thin adapter (`Charts/ComparisonChartMapper.cs`) maps `ComparisonPoint` lists → two `LineSeries`: actual = solid `#10b981`, baseline = dashed `#f59e0b`; X-axis "Turn" (integer labels), Y-axis formatted per metric unit; tooltip enabled. Styled to the dark slate card look.
- DI in `MauiProgram.cs`: register `IRoutingDataService` → `MockRoutingDataService`, `AnalyticsViewModel`, pages.

## Phase 3 — Testing (80% coverage)

`tests/AgenticRouter.Gui.Core.Tests` (xUnit + `coverlet.msbuild`, which supports the MSBuild threshold properties used below), plus ViewModel tests (ViewModel project/classes are plain .NET, referenced without MAUI):

- `AnalyticsServiceTests` — the bulk: grouping/ordering by timestamp, all seven metric formulas vs hand-computed values (cumulative vs per-turn kinds), aggregate mean with unequal conversation lengths, empty input, single turn, unknown sessionId.
- `MetricRegistryTests` — seven metrics, rank order, units/kinds.
- `MockRoutingDataServiceTests` — multi-turn sessions present, fields populated/valid ranges.
- `AnalyticsViewModelTests` — defaults, changing selection recomputes series and header readout.
- Coverage: `dotnet test /p:CollectCoverage=true /p:Threshold=80 /p:ThresholdType=line%2cbranch` (coverlet.msbuild) enforced over Core + ViewModels; the XAML/MAUI head is excluded (not runnable on Linux).

## Phase 4 — Documentation

- `docs/analytics-baseline-comparison-plan.md` — this plan.
- `docs/analytics-metrics.md` — the seven metrics with rank rationale, baseline formulas, constants, assumptions.
- `docs/architecture.md` — solution layout, MVVM structure, why logic lives in Core (Linux-testable), charting choice.
- `docs/repo-restructure.md` — verified prototype file-name mapping and why files moved to `prototype/`.
- `README.md` — setup (SDK, MAUI workloads), build/test/run instructions per platform.
- XML doc comments (`///`) on all public Core/ViewModel APIs; `GenerateDocumentationFile` on in Core.

## Verification

1. `dotnet build AgenticRouter.Gui.sln` (Core + tests; App head best-effort per workload availability).
2. `dotnet test` with coverlet — all green, ≥80% line+branch coverage on Core/ViewModels; report coverage numbers in the summary.
3. Golden-value spot check: print the Routing ROI series for one conversation and confirm hockey-stick shape (accelerating actual vs linear-ish baseline gap).
4. On a developer machine: `dotnet build -t:Run -f net8.0-windows10.0.19041.0` (or android/mac target) → open Cost Analytics tab, switch conversation/metric pickers, confirm chart + tooltip. Documented in README.
5. Commit in logical steps (restructure → Core → App → tests → docs).

## Appendix — Verified prototype file mapping

The prototype export's physical filenames do not match their contents. Verified mapping (to be applied in Phase 0):

| Current physical file | Actual content | Real path (under `prototype/`) |
|---|---|---|
| `LiveStream.tsx` | App shell | `src/App.tsx` |
| `ModelDistribution.tsx` | types + mock data | `src/data/mockData.ts` |
| `download` | Analytics tab (CostAnalytics) | `src/components/CostAnalytics.tsx` |
| `package-lock.json` | ModelDistribution component | `src/components/ModelDistribution.tsx` |
| `eslint.config.js` | Governance component | `src/components/Governance.tsx` |
| `index (1).html` | LiveStream component | `src/components/LiveStream.tsx` |
| `package.json` | SettingsModal component | `src/components/SettingsModal.tsx` |
| `CostAnalytics.tsx` | source stylesheet | `src/index.css` |
| `postcss.config.js` | entry point | `src/main.tsx` |
| `Governance.tsx` | vite-env types | `src/vite-env.d.ts` |
| `tsconfig.node.json` | real package.json | `package.json` |
| `vite.config.ts` (164 KB) | real package-lock.json | `package-lock.json` |
| `tsconfig.json` | real eslint flat config | `eslint.config.js` |
| `tsconfig.app.json` | real index.html | `index.html` |
| `tailwind.config.js` | real .gitignore | `.gitignore` |
| `App.tsx` | Netlify redirects | `public/_redirects` |
| `index.html`, `prompt` | bolt.new template metadata | `.bolt/` |
| `index.css`, `index-B9Z7yfCw.js`, `vite-env.d.ts` | built dist artifacts | delete (regenerable) |
