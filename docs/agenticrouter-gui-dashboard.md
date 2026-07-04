# AgenticRouter.Gui Dashboard

This document describes the dashboard UI rendered inside `AgenticRouter.Gui`'s window
(`src/AgenticRouter.Gui/Components/`). For the tray-app shell itself (tray icon, show/hide behavior,
build/run instructions), see [`src/AgenticRouter.Gui/README.md`](../src/AgenticRouter.Gui/README.md).

## Purpose

The dashboard presents routing, cost, and governance telemetry for the AgenticRouter proxy: which
requests were routed to which upstream model, how much that saved versus a worst-case baseline, token
volume trends, model market share, and per-provider budget status.

**Current status: all data is hard-coded mock data** (`Models/DashboardData.cs`). Nothing in the
dashboard reads from the live `AgenticRouter` proxy yet - it is a UI/UX shell to be wired up to real
telemetry in a later change.

## Stack

| Layer | Choice |
| --- | --- |
| App shell | .NET MAUI (Windows-only, single window, tray-resident via Win32 interop) |
| UI framework | Razor components in a MAUI `BlazorWebView` (Blazor Hybrid) |
| Styling | A static stylesheet (`wwwroot/css/app.css`) containing the dashboard's compiled Tailwind utility classes plus custom rules; state-driven colors are inline styles in the components |
| Charts | [Blazor-ApexCharts](https://github.com/apexcharts/Blazor-ApexCharts) (`Blazor-ApexCharts-MAUI` package) - line, horizontal/grouped bar, and donut charts |
| Icons | Small inline SVG glyphs (`Components/Icon.razor`) |

The dashboard has no web build step: the Razor components compile with the .NET project, the stylesheet
is checked-in static content, and the chart JavaScript ships inside the NuGet package's static web
assets (so everything works offline). All navigation is client-side component state (`_activeTab` in
`Components/Dashboard.razor`); there is no router, dev server, or backend API.

The UI is a conversion of an earlier React/Vite/Tailwind implementation of the same design; the visual
design, layout, colors, and mock data carry over unchanged. Because the stylesheet is the *compiled*
Tailwind output of that design, new markup must stick to utility classes that already appear in it (or
add plain CSS to `app.css`) - there is no Tailwind build to generate new utilities.

## Visual theme

Dark theme only, fixed (no light mode / no theme toggle):

- Background: `#0f172a` (page) / `#1e293b` (cards, header, nav)
- Borders: `#334155`
- Text: `#e2e8f0` (primary), `#94a3b8` / `#64748b` / `#475569` (secondary/muted, descending emphasis)
- Accent (info/active): sky blue `#38bdf8`
- Positive/savings: emerald `#10b981`
- Warning: amber `#f59e0b`
- Critical: red `#ef4444`
- Fonts: Inter (UI text), JetBrains Mono (all numeric/monospace values - token counts, costs, timestamps,
  session/trace IDs)

The whole app is a fixed-height, non-scrolling shell (`h-screen overflow-hidden`) with individual panels
scrolling internally where their content can overflow.

## Layout

```
┌─────────────────────────────────────────────────────────────┐
│ 🤖 Router Optimization Engine   [status banner]   [Settings]│  <- header
│ Total Saved  System Tokens  Avg. Cost Reduction   ● LIVE     │  <- ticker row
├─────────────────────────────────────────────────────────────┤
│ [Live Stream] [Cost Analytics] [Model Distribution] [Gov...] │  <- tab bar
├─────────────────────────────────────────────────────────────┤
│                                                               │
│                        active tab content                   │
│                                                               │
└─────────────────────────────────────────────────────────────┘
```

### Header

- Brand: `🤖 Router Optimization Engine`.
- Status banner (center): reads live from `MockData.Providers`' budget utilization.
  - All providers under 80% of budget: green pulsing dot + "System Status: OK".
  - Any provider ≥ 100%: red "🚨 N PROVIDER BREACHED" (or "N BREACHED" alongside approaching count).
  - Any provider ≥ 80% and < 100%: amber "⚠️ N PROVIDER APPROACHING LIMIT".
  - Clicking the banner (when there's an alert) jumps to the **Governance** tab and briefly flashes
    (`flash-amber`/`flash-red` CSS animation) the first breached/approaching provider's card.
- **Settings** button (top right) opens the settings modal.
- Ticker row: three mock aggregate stats (Total Saved, System Tokens, Avg. Cost Reduction) plus a `LIVE`
  indicator with a pulsing dot.

### Tabs

1. **Live Stream** (`LiveStream.razor`, default tab) - a two-column view:
   - Left (40% width): a searchable, scrollable list of routing entries (`MockData.Entries`), newest
     first. Each card shows session ID, timestamp, routed model, savings, and originating agent; fallback
     entries get an amber `⚠` badge and border. Search filters by session ID or agent name.
   - Right: a drilldown for the selected entry - trace ID/agent header, a token volume breakdown
     (prompt/completion/total counts plus a proportional bar), a cost performance panel (actual cost vs.
     worst-case cost vs. net savings), and a collapsible "Routing Decision Inspector" showing the
     step-by-step routing log (`Ok` = green check, `Warn` = amber triangle, `Info` = blue arrow).

2. **Cost Analytics** (`CostAnalytics.razor`) - two stacked panels:
   - A cumulative savings line chart over time (`MockData.CostData`), with a dark tooltip.
   - A horizontal bar chart of cost-reduction % by agent (`MockData.AgentRoi`), bars colored by reduction
     tier (≥85% green, ≥70% blue, else amber), with the percentage labeled at the end of each bar.

3. **Model Distribution** (`ModelDistribution.razor`) - a time-range filter bar (Day/Month/3-Month/
   6-Month/Year - visual only, does not currently refilter data) with From/To text inputs, above:
   - A grouped bar chart of prompt vs. completion token volume by day (`MockData.TokenBuckets`).
   - A donut chart of model market share by execution volume (`MockData.ModelShares`), with a custom
     HTML legend below it.

4. **Governance** (`Governance.razor`) - a 2-column grid of provider budget cards
   (`MockData.Providers`), sorted by utilization (highest first). Each card shows the provider name/pool
   label, an editable (client-side only, not persisted) budget cap input, current spend, a utilization
   progress bar, and a status tag: `OK` (<80%), `WARNING` (80-99%, shows estimated days remaining), or
   `CRITICAL` (≥100%, "Fallback Engine Engaged"). Cards flash their border briefly when navigated to via
   the header's alert banner.

### Settings modal (`SettingsModal.razor`)

Opened via the header's **Settings** button. A centered modal (dimmed/blurred backdrop, click-outside to
close) with a "Destructive Actions Zone": **Reset Stats** and **Clear History** buttons, each requiring
the user to type a literal confirmation word (`RESET` / `PURGE`) before the action button enables. No
action is actually wired to real data yet - confirming just closes the modal.

## Data model (`Models/DashboardData.cs`)

All dashboard state is derived from six mock collections on the static `MockData` class, typed via C#
records:

- `MockData.Entries: RoutingEntry[]` - individual routing decisions (session/trace IDs, agent, model,
  fallback flag, token counts, actual vs. worst-case cost, savings, timestamp, and an ordered
  `RoutingSteps` log).
- `MockData.Providers: Provider[]` - per-provider budget state (cap, current spend, estimated days
  remaining).
- `MockData.CostData: CostDataPoint[]` - cumulative savings time series.
- `MockData.AgentRoi: AgentRoi[]` - cost-reduction % and savings per agent.
- `MockData.TokenBuckets: TokenBucket[]` - daily prompt/completion token volume.
- `MockData.ModelShares: ModelShare[]` - market-share percentage and color per model.

Wiring the dashboard to the live proxy means replacing these collections with data fetched from
`AgenticRouter`'s actual routing/telemetry, without needing to change the component layer.

## Known gaps / non-functional controls

These match the source design as received and are called out so they aren't mistaken for bugs:

- Model Distribution's time-range filter buttons and From/To inputs don't actually refilter the charts.
- Governance's budget cap input is editable but not persisted or wired to anything.
- Settings modal's Reset/Clear actions don't affect any data - they just close the modal once confirmed.
- Chart axis ranges (e.g. the $0-$160 savings scale, the 0-6M token scale) are pinned to fit the mock
  data; they'll need to become dynamic when real telemetry is wired in.
- The chart tooltips use ApexCharts' dark theme (restyled in `app.css` to match the card styling) rather
  than the fully custom tooltips of the original React implementation - minor visual differences are
  expected there.
