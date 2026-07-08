# Architecture

## Solution layout

```
AgenticRouter.Gui.sln
src/
  AgenticRouter.Gui.Core/          # net8.0 class library — all feature logic
    Models/                        # RoutingEntry (+RoutingStep), Provider, TokenBucket,
                                   #   ModelShare, TickerStats
    Data/IRoutingDataService.cs    # data-source abstraction (entries, providers,
                                   #   buckets, share, ticker)
    Data/MockRoutingDataService.cs # deterministic mock data (no backend yet)
    Analytics/                     # metric registry, baselines, series computation
    ViewModels/                    # Analytics, LiveStream, ModelDistribution,
                                   #   Governance (+ProviderCard), Settings, SystemStatus
  AgenticRouter.Gui.App/           # .NET MAUI head — thin UI shell
    AppShell.xaml                  # 4 tabs mirroring the React prototype
    Views/AppHeaderView.xaml       # shared header: banner, ticker, Settings button
    Views/LiveStreamPage.xaml      # searchable stream + drilldown inspector
    Views/AnalyticsPage.xaml       # pickers + readout + LiveCharts2 comparison chart
    Views/ModelDistributionPage.xaml # time filter + histogram + market-share donut
    Views/GovernancePage.xaml      # provider budget cards with editable caps
    Views/SettingsPage.xaml        # modal destructive-actions zone (type-to-confirm)
    Charts/                        # ComparisonChartMapper, DistributionChartMapper
    Converters/                    # status/fallback/savings → color/text converters
    MauiProgram.cs                 # DI wiring
tests/
  AgenticRouter.Gui.Core.Tests/    # xUnit + coverlet (≥80% line+branch enforced)
docs/
prototype/                         # original React prototype — design reference only
```

See `docs/tabs.md` for what each tab shows and how it behaves.

## Why the logic lives in Core

A MAUI app can neither build nor run on plain Linux (platform workloads
required), but this repo's CI/agent environment is Linux. Everything with
behavior — domain model, mock data, metric formulas, aggregation, the
Analytics ViewModel — therefore lives in `AgenticRouter.Gui.Core`, a plain
`net8.0` library that builds and tests anywhere. The MAUI head contains
only declarative XAML, DI wiring, and a small chart adapter.

This is also why `AnalyticsViewModel` sits in Core rather than the App
project (a deliberate refinement of the original plan wording): it uses
only `CommunityToolkit.Mvvm` (`ObservableObject`, plain .NET), never MAUI
or LiveCharts types, so the full picker → recompute → readout flow is unit
tested on Linux.

## MVVM structure

One ViewModel per tab plus two app-chrome ViewModels, all in Core, all
synchronous (the data is in-memory mock data, so no async machinery is
warranted yet):

- `AnalyticsViewModel` — conversation/metric pickers, the computed
  `Series` (`IReadOnlyList<ComparisonPoint>`) and the header readout.
  `AnalyticsPage` listens for `Series` changes and pushes mapped
  LiveCharts2 series into the chart via `ComparisonChartMapper`.
- `LiveStreamViewModel` — search filter over the entry stream, selection,
  and the drilldown readouts (token split fraction/percent texts, cost
  texts, savings line). The Routing Decision Inspector renders
  `RoutingEntry.RoutingSteps` directly with status-colored badges.
- `ModelDistributionViewModel` — the functional time-filter
  (`TimeRangeOption` list; changing it swaps the `TokenBuckets` dataset)
  and the fixed `ModelShares`. `DistributionChartMapper` turns both into
  column/donut series.
- `GovernanceViewModel` + `ProviderCardViewModel` — cards sorted by
  utilization descending; the editable cap re-parses and recomputes
  utilization, status (OK <80% ≤ WARNING <100% ≤ CRITICAL) and messages;
  `FlashedProviderId`/`IsFlashing` carry the banner-navigation spotlight,
  which the page clears on a timer.
- `SettingsViewModel` — the type-to-confirm state machine (RESET/PURGE
  phrases, upper-cased input, `CanExecute`-gated confirm) raising
  `ActionExecuted`/`CloseRequested` events for the modal page.
- `SystemStatusViewModel` — banner counts and texts (breached ≥100%,
  approaching ≥80%), the alert target provider for the flash, and the
  ticker texts.

`AnalyticsService` is pure/static: entries in, series out. The per-metric
formulas live in a metric-id-keyed delegate table mirrored by
`MetricRegistry.All` (guarded by a test that every registered metric
computes).

Pages stay thin: XAML bindings, a handful of `IValueConverter`s for
status/fallback coloring, chart adapters, and the shared `AppHeaderView`
(resolved via `ServiceHelper` because XAML-created views can't use
constructor injection).

## Charting choice

**LiveCharts2** (`LiveChartsCore.SkiaSharpView.Maui` 2.0.5, MIT,
SkiaSharp-based): free, actively maintained, first-class MAUI support,
multi-series line charts with dashed strokes and tooltips out of the box.
Alternatives considered: Syncfusion charts (feature-rich but license
gated), Microcharts (too limited — no multi-series line comparison or
tooltips).

## Data flow

```
                    IRoutingDataService (MockRoutingDataService)
        ┌────────────────┬──────────────┬───────────────┬─────────────┐
        ▼                ▼              ▼               ▼             ▼
  RoutingEntry[]   Provider[]    TokenBucket[]   ModelShare[]   TickerStats
   │        │        │      │          │               │             │
   │        │        │      └── SystemStatusViewModel (banner) ──────┘
   │        │        ▼                 │               │
   │        │  GovernanceViewModel     └── ModelDistributionViewModel
   │        │   (cards, cap edits)                      │
   │        │                                           ▼
   │        └── LiveStreamViewModel          DistributionChartMapper
   │             (search, drilldown)          → column + donut charts
   ▼
AnalyticsService.ComputeComparisonSeries
   → AnalyticsViewModel.Series → ComparisonChartMapper → CartesianChart
```

A future live backend replaces `MockRoutingDataService` behind
`IRoutingDataService` in `MauiProgram.cs`; nothing else changes.

## Testing strategy

`tests/AgenticRouter.Gui.Core.Tests` (xUnit) covers the metric formulas
against hand-computed fixtures, grouping/ordering semantics, aggregate-mean
behavior with unequal conversation lengths, edge cases (empty input, single
turn, unknown session/metric), mock-data invariants (determinism, valid
ranges, compounding prompt growth, per-range bucket shapes) and every
ViewModel's behavior: Analytics recompute/readout, Live Stream
search/selection/token-split, Model Distribution range switching and K/M
formatting, Governance status thresholds and cap-edit recompute (including
invalid input), Settings type-to-confirm flows, and the status banner's
counts/texts/targets. A configurable `StubDataService` isolates ViewModel
tests from the mock data where useful. Coverage policy — ≥80% line **and**
branch over the Core assembly, excluding compiler/source-generator
boilerplate — is enforced by `coverlet.msbuild` whenever tests run with
`/p:CollectCoverage=true` (settings live in the test csproj).
Current: 100% line, ~95% branch, 100% method.
