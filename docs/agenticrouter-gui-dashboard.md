# AgenticRouter.Gui Dashboard

This document describes the dashboard SPA rendered inside `AgenticRouter.Gui`'s Photino window
(`src/AgenticRouter.Gui/dashboard/`). For the tray-app shell itself (tray icon, show/hide behavior,
build/run instructions), see [`src/AgenticRouter.Gui/README.md`](../src/AgenticRouter.Gui/README.md).

## Purpose

The dashboard presents routing, cost, and governance telemetry for the AgenticRouter proxy: which
requests were routed to which upstream model, how much that saved versus a worst-case baseline, token
volume trends, model market share, and per-provider budget status.

**Current status: all data is hard-coded mock data** (`dashboard/src/data/mockData.ts`). Nothing in the
dashboard reads from the live `AgenticRouter` proxy yet - it is a UI/UX shell to be wired up to real
telemetry in a later change.

## Stack

| Layer | Choice |
| --- | --- |
| UI framework | React 18 + TypeScript |
| Build tool | Vite 5 |
| Styling | Tailwind CSS (utility classes) + a handful of inline styles for state-driven colors |
| Charts | [Recharts](https://recharts.org/) 3 (line, bar, and pie/donut charts) |
| Icons | [lucide-react](https://lucide.dev/) |

The dashboard is a fully static single-page app: `npm run build` (from `dashboard/`) emits
`index.html` plus a JS/CSS bundle directly into `src/AgenticRouter.Gui/wwwroot/`, which Photino loads
from the local filesystem. There is no dev server, backend API, or router in production - all
navigation is client-side React state (`activeTab` in `App.tsx`).

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
- Status banner (center): reads live from `MOCK_PROVIDERS`' budget utilization.
  - All providers under 80% of budget: green pulsing dot + "System Status: OK".
  - Any provider ≥ 100%: red "🚨 N PROVIDER BREACHED" (or "N BREACHED" alongside approaching count).
  - Any provider ≥ 80% and < 100%: amber "⚠️ N PROVIDER APPROACHING LIMIT".
  - Clicking the banner (when there's an alert) jumps to the **Governance** tab and briefly flashes
    (`flash-amber`/`flash-red` CSS animation) the first breached/approaching provider's card.
- **Settings** button (top right) opens the settings modal.
- Ticker row: three mock aggregate stats (Total Saved, System Tokens, Avg. Cost Reduction) plus a `LIVE`
  indicator with a pulsing dot.

### Tabs

1. **Live Stream** (`LiveStream.tsx`, default tab) - a two-column view:
   - Left (40% width): a searchable, scrollable list of routing entries (`MOCK_ENTRIES`), newest first.
     Each card shows session ID, timestamp, routed model, savings, and originating agent; fallback
     entries get an amber `⚠` badge and border. Search filters by session ID or agent name.
   - Right: a drilldown for the selected entry - trace ID/agent header, a token volume breakdown
     (prompt/completion/total counts plus a proportional bar), a cost performance panel (actual cost vs.
     worst-case cost vs. net savings), and a collapsible "Routing Decision Inspector" showing the
     step-by-step routing log (`ok` = green check, `warn` = amber triangle, `info` = blue arrow).

2. **Cost Analytics** (`CostAnalytics.tsx`) - two stacked panels:
   - A cumulative savings line chart over time (`COST_DATA`), with a custom tooltip.
   - A horizontal bar chart of cost-reduction % by agent (`AGENT_ROI`), bars colored by reduction tier
     (≥85% green, ≥70% blue, else amber), with the percentage labeled at the end of each bar.

3. **Model Distribution** (`ModelDistribution.tsx`) - a time-range filter bar (Day/Month/3-Month/
   6-Month/Year - visual only, does not currently refilter data) with From/To text inputs, above:
   - A grouped bar chart of prompt vs. completion token volume by day (`TOKEN_BUCKETS`).
   - A donut chart of model market share by execution volume (`MODEL_SHARE`), with a custom legend below
     it.

4. **Governance** (`Governance.tsx`) - a 2-column grid of provider budget cards (`MOCK_PROVIDERS`),
   sorted by utilization (highest first). Each card shows the provider name/pool label, an editable
   (client-side only, not persisted) budget cap input, current spend, a utilization progress bar, and a
   status tag: `OK` (<80%), `WARNING` (80-99%, shows estimated days remaining), or `CRITICAL` (≥100%,
   "Fallback Engine Engaged"). Cards flash their border briefly when navigated to via the header's alert
   banner.

### Settings modal (`SettingsModal.tsx`)

Opened via the header's **Settings** button. A centered modal (dimmed/blurred backdrop, click-outside to
close) with a "Destructive Actions Zone": **Reset Stats** and **Clear History** buttons, each requiring
the user to type a literal confirmation word (`RESET` / `PURGE`) before the action button enables. No
action is actually wired to real data yet - confirming just closes the modal.

## Data model (`dashboard/src/data/mockData.ts`)

All dashboard state is derived from six exported mock constants, typed via exported interfaces:

- `MOCK_ENTRIES: RoutingEntry[]` - individual routing decisions (session/trace IDs, agent, model,
  fallback flag, token counts, actual vs. worst-case cost, savings, timestamp, and an ordered
  `routingSteps` log).
- `MOCK_PROVIDERS: Provider[]` - per-provider budget state (cap, current spend, estimated days
  remaining).
- `COST_DATA: CostDataPoint[]` - cumulative savings time series.
- `AGENT_ROI: AgentROI[]` - cost-reduction % and savings per agent.
- `TOKEN_BUCKETS: TokenBucket[]` - daily prompt/completion token volume.
- `MODEL_SHARE: ModelShare[]` - market-share percentage and color per model.

Wiring the dashboard to the live proxy means replacing these constants with data fetched from
`AgenticRouter`'s actual routing/telemetry, without needing to change the component layer.

## Known gaps / non-functional controls

These match the source design as received and are called out so they aren't mistaken for bugs:

- Model Distribution's time-range filter buttons and From/To inputs don't actually refilter the charts.
- Governance's budget cap input is editable but not persisted or wired to anything.
- Settings modal's Reset/Clear actions don't affect any data - they just close the modal once confirmed.
