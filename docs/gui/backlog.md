# AgenticRouter.Gui: Not-Yet-Implemented Work

A backlog of gaps between what the GUI docs describe/design and what `src/AgenticRouter.Gui/`
actually does today. Originally sourced from explicit statements in [`dashboard.md`](dashboard.md)'s
"Known gaps" section, [`src/AgenticRouter.Gui/README.md`](../../src/AgenticRouter.Gui/README.md)'s
"Current limitations," and deferred/optional items in
[`livestream-redesign-plan.md`](livestream-redesign-plan.md).

## Open

### 1. Wire the dashboard to live AgenticRouter proxy telemetry (headline item)

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

No telemetry-capture/exposure layer exists in `src/AgenticRouter/` today (no history endpoint, no
push transport) — this item requires proxy-side work, not just GUI-side wiring.

### 2. Real-time auto-refresh / streaming updates (Live Stream)

The original redesign plan specified a **live** dashboard: auto-refresh every 1–2 seconds, new
turns appearing at the bottom of the list without a page reload, and selection persistence across
updates. None of that is implemented — the app loads mock data once and stays static. Distinct
from item 1 (which is about the data *source*): this is about push/poll *mechanics* on top of a
live source. Preferred direction (per 2026-07-08 discussion): push via SignalR/WebSocket from the
proxy once item 1's telemetry layer exists, rather than polling.

## Recently completed

### ✅ Token-compounding line chart in Cost Analytics

Implemented in `CostAnalytics.razor`'s new "Token Compounding by Conversation" panel: a
conversation picker over `MockData.Conversations` plus a two-series line chart (cumulative prompt
tokens, cumulative completion tokens) per turn, built via
`AgenticRouter.Gui.Charts.TokenCompoundingSeries.Build`.

### ✅ Token-compounding sparkline on the conversation summary card

Implemented in `ConversationSummary.razor`: a compact inline SVG polyline ("Trend" stat) showing
per-turn total tokens, built via `AgenticRouter.Gui.Charts.TokenCompoundingSeries.BuildSparkline`
and `SparklineLayout.Normalize`.

### ✅ Keyboard-accessible tooltips

`wwwroot/js/tooltips.js` now shows/hides on `focusin`/`focusout` (in addition to hover), dismisses
on Escape, and the shared tooltip element is hidden via opacity rather than `display:none` so it
stays in the accessibility tree. Every `data-tip` element not nested inside a `<button>` carries
`tabindex="0"` and `aria-describedby="ls-tooltip"`. The handful nested inside a `<button>` (e.g. a
`TurnCard` header's sub-badges) intentionally skip `tabindex` — nesting a focusable element inside a
button is an ARIA anti-pattern — and the outer button carries a comprehensive `aria-label` instead.
Smoke-tested against a standalone HTML harness with Playwright/Chromium (see `dashboard.md`'s
"Verification limitation" note for why that, rather than a full app build, was the verification
method available in this environment).

## Minor / cosmetic (low priority)

- **Chart tooltips** use ApexCharts' default dark theme (restyled in `app.css` to match card
  styling) rather than a fully custom tooltip matching the original React design pixel-for-pixel.
  `dashboard.md` calls this a "minor visual difference," not a functional gap.
