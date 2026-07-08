# AgenticRouter GUI: Tab Reference

Detailed description of every tab (and the app-level chrome) as designed in
the React prototype (`prototype/`) and ported to .NET MAUI. Colors refer to
the dark slate palette (`Resources/Styles/Colors.xaml`): background
`#0f172a`, cards `#1e293b`, borders `#334155`, accent green `#10b981`,
amber `#f59e0b`, sky `#38bdf8`, red `#ef4444`.

## App chrome (shared)

**Header** — brand ("🤖 Router Optimization Engine"), a **system status
banner**, and a **Settings** button.

- *Status banner*: computed from provider budget utilization
  (spend ÷ cap). Providers at **≥100%** count as **BREACHED** (red 🚨),
  **≥80% and <100%** as **APPROACHING LIMIT** (amber ⚠️); otherwise the
  banner shows a pulsing green dot with "System Status: OK". When alerts
  exist the banner is clickable: it navigates to the Governance tab and
  flashes the worst offending provider card (red flash if breached, amber
  if approaching). The banner reflects the data service's provider
  snapshot: local Budget Cap edits on Governance cards are in-memory only
  (matching the prototype) and do not feed back into it until a
  persistence layer exists.
- *Ticker row*: three system-wide stats — **Total Saved** ($142.36, green),
  **System Tokens** (12.4M), **Avg. Cost Reduction** (74.20% ↓, green) —
  plus a pulsing "LIVE" indicator. The prototype hardcodes these values;
  the port carries them as named constants in the mock data service.

**Settings modal** — opened from the header. A "Destructive Actions Zone"
with two actions, each guarded by a type-to-confirm flow:

- **Reset Stats** (amber): requires typing `RESET` before the confirm
  button enables.
- **Clear History** (purge, red): requires typing `PURGE`.

Input is upper-cased as typed; Cancel abandons the flow; a footnote states
that configurations, routing rules and budget caps are preserved. With no
backend, confirming simply closes the modal (the ViewModel exposes the
confirmation state machine so a future backend can hook the execution).

## 1. Live Stream

Master/detail inspector over the stream of routed agent turns
(`RoutingEntry`), newest first. Static mock data — no simulated streaming.

**Left pane (~40%): the stream.**

- Search box filtering by session id (substring) or agent name
  (case-insensitive substring).
- One card per entry: session id (truncated, monospace) + timestamp;
  model name (sky blue, or amber when the entry is a fallback, with a ⚠
  badge); the savings line — green
  `Saved: $0.003380 (87.56% ↓)` for routed entries, muted
  `Saved: $0.000000 (0.00% ➔)` for fallbacks; agent name.
- Selecting a card (sky-blue border highlight) drives the right pane.

**Right pane: the drilldown for the selected entry.**

- *Header card*: trace id (`#a4f89c02…`, sky) and assigned agent.
- *Token Volume card*: three stat tiles (Prompt / Completion / Total
  tokens) and a split progress bar showing the prompt (sky) vs completion
  (green) share with percentage labels.
- *Cost Performance card*: Actual Allocated Cost, Worst-Case Pool Cost,
  and a Net Transaction Savings line (green for routed, muted for
  fallback).
- *Routing Decision Inspector* (collapsible): the router's per-step
  decision log. Each step renders as a status badge — **ok** (green left
  border, ✓), **warn** (amber left border, ⚠), **info** (sky left border,
  👉) — with monospace message text, e.g. "Budget nominal: gpt-4o-mini
  selected" → "Route Confirmed: gpt-4o-mini".

## 2. Cost Analytics

Per-turn line chart comparing **actual** conversation metrics against a
**theoretical baseline** (see `docs/analytics-metrics.md` for formulas).

- *Selectors*: conversation (a specific session or "All Conversations"
  aggregate) and one of seven metrics in rank order — Routing ROI, Total
  Turn Cost, Prompt + Completion Tokens, Tool Execution Loops, Cache Hit
  Rate, Time to First Token, Context Buffer Margin.
- *Header readout*: the final actual value plus its delta vs baseline,
  colored by favorability (each metric knows its direction of
  improvement).
- *Chart*: X = turn index, Y = metric value; actual = solid green line
  with point markers, baseline = dashed amber; integer turn axis,
  metric-formatted value axis, tooltips.

The multi-turn mock conversations compound context turn over turn, so
cumulative metrics trace the "hockey stick" the tab is designed to
surface.

(The prototype's original Cost Analytics showed a cumulative-savings area
chart and an agent-ROI bar list; those were superseded by the comparison
chart per the approved plan.)

## 3. Model Distribution

Token throughput and model usage breakdown, side by side.

- *Time filter bar*: Day / Month / 3-Month / 6-Month / Year segmented
  control. In the prototype this was cosmetic; the port makes it
  functional — each range selects a deterministic mock dataset with
  range-appropriate buckets (hours of day, days of week, weeks, or
  months). Free-form From/To date inputs from the prototype are not
  ported (no backing behavior existed).
- *Token Volume Histogram* (left, flexible width): grouped bar chart per
  bucket — prompt tokens (sky) beside completion tokens (green), Y axis
  in K/M notation (e.g. `3.1M`), tooltip with exact values.
- *Model Market Share* (right, ~42%): donut chart of execution volume
  share per model, using each model's fixed brand color from the mock
  data (gpt-4o-mini green, claude-3-haiku sky, gemini-1.5-flash indigo,
  fallback-local amber, claude-3-5-sonnet rose, text-embedding-3-small
  violet), with a dot legend and percentage tooltips.

## 4. Governance

Provider budget monitoring — a two-column grid of provider cards, sorted
by utilization descending so the most at-risk pool is always top-left.

Each card (OpenAI API / Anthropic Claude / Google Gemini / Local
Inference, each labeled with its pool):

- *Status chip*: **OK** (green) below 80% utilization, **WARNING**
  (amber) at ≥80%, **CRITICAL** (red) at ≥100%; the card border tints to
  match.
- *Budget Cap*: an editable dollar amount. Editing the cap recomputes
  utilization, status, bar and messages in-memory (invalid or
  non-positive input is ignored; nothing persists yet).
- *Current Spend* and a utilization progress bar
  (`$492.80 / $500.00 · 98.6% utilized`), bar colored by status.
- *Status message*: 🚨 "CRITICAL: Budget Exhausted. Fallback Engine
  Engaged." / ⚠️ "Approaching cap (Est. N days remaining)" / ✅ "Spend
  nominal".

Cards flash (red or amber) when the user arrives via the header status
banner, spotlighting the provider that triggered the alert.

## Data flow summary

All tabs read from `IRoutingDataService` (currently
`MockRoutingDataService`): routing entries feed Live Stream and Cost
Analytics; providers feed Governance and the status banner; token buckets
and model share feed Model Distribution; ticker constants feed the header.
Every ViewModel lives in `AgenticRouter.Gui.Core` and is unit-tested; the
MAUI pages are thin XAML bindings plus chart adapters (see
`docs/architecture.md`).
