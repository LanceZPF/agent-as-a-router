# AgenticRouter.Gui: Not-Yet-Implemented Work

A backlog of gaps between what the GUI docs describe/design and what `src/AgenticRouter.Gui/`
actually does today. Sourced from explicit statements in [`dashboard.md`](dashboard.md)'s "Known
gaps" section, [`src/AgenticRouter.Gui/README.md`](../../src/AgenticRouter.Gui/README.md)'s
"Current limitations," and deferred/optional items in
[`livestream-redesign-plan.md`](livestream-redesign-plan.md). Nothing here is inferred from code
archaeology alone — every item is a gap the docs themselves call out.

## 1. Wire the dashboard to live AgenticRouter proxy telemetry (headline item)

Every tab currently reads from the static `MockData` class in `Models/DashboardData.cs`; nothing
in the GUI talks to the running `AgenticRouter` proxy. This is the root cause of most of the
smaller gaps below — each is really "this can't be real until #1 exists":

- **Live Stream** — real `Conversation`/`ConversationTurn` data instead of the three hand-written
  mock conversations.
- **Cost Analytics** — real cumulative-savings time series and per-agent ROI instead of
  `MockData.CostData`/`AgentRoi`.
- **Model Distribution** — real `TokenBucket`/`ModelShare` data. The Day/Month/3-Month/6-Month/Year
  time-range filter bar and the From/To date inputs are currently **cosmetic only** — they don't
  refilter the charts — and only become meaningful once there's real time-series data to filter.
- **Governance** — real per-provider budget/spend data. The Budget Cap input is editable today but
  purely client-side: edits recompute the in-memory utilization/status/bar but aren't persisted
  anywhere and are lost on refresh; needs a real place to write to.
- **Dynamic chart axis ranges** — axis scales (e.g. the $0–$160 savings scale, the 0–6M token
  scale) are currently pinned to fit the mock data's known range; need to become dynamic once real
  data volume varies.
- **Settings modal actions** — Reset Stats / Clear History currently just close the modal with no
  effect; they need real actions once there's real state to reset or clear.

## 2. Real-time auto-refresh / streaming updates (Live Stream)

The original redesign plan specified a **live** dashboard: auto-refresh every 1–2 seconds, new
turns appearing at the bottom of the list without a page reload, and selection persistence across
updates. None of that is implemented — the app loads mock data once and stays static. Distinct
from item 1 (which is about the data *source*): this is about push/poll *mechanics* on top of a
live source, and would need designing regardless of how #1 is solved (SignalR? polling? something
else against the eventual telemetry API).

## 3. Token-compounding line chart in Cost Analytics

The Live Stream redesign plan explicitly deferred this: *"the line chart showing token compounding
by conversation should appear in the Cost Analytics tab, NOT in Live Stream. That's a separate
task."* Cost Analytics today only has the cumulative-savings line chart and the agent-ROI bar
chart — no per-turn "hockey stick" comparison view showing how a single conversation's token/cost
usage compounds turn over turn.

## 4. Token-compounding sparkline on the conversation summary card

Flagged as an **"(optional enhancement)"** in the redesign plan: a small sparkline/mini-chart on
`ConversationSummary.razor` showing token growth across the selected conversation's turns, so the
compounding trend is visible without scrolling the full turn list. Not built.

## 5. Keyboard-accessible tooltips

The redesign plan calls for tooltip keyboard accessibility (`aria-describedby` support for screen
readers). The shipped `wwwroot/js/tooltips.js` only responds to `mouseover`/`mouseleave` — there's
no `focus`/`blur` handling, so a keyboard-only user tabbing through the dashboard can't trigger any
`data-tip` tooltip.

## Minor / cosmetic (low priority)

- **Chart tooltips** use ApexCharts' default dark theme (restyled in `app.css` to match card
  styling) rather than a fully custom tooltip matching the original React design pixel-for-pixel.
  `dashboard.md` calls this a "minor visual difference," not a functional gap.
