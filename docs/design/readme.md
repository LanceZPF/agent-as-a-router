# AgenticRouter Design System

A design system built from **[davidpizon/agent-as-a-router](https://github.com/davidpizon/agent-as-a-router)**, specifically its `AgenticRouter.Gui` project (`src/AgenticRouter.Gui/`) and the docs describing it (`docs/agenticrouter-gui-dashboard.md`). Explore those paths further — and the repo's root `README.md` / `AGENTS.md` — for deeper product and architecture context than what's captured here.

## Company / product context

**Agent-as-a-Router (ACRouter)** is a research project + reference implementation for routing coding tasks to different backend LLMs under a performance-cost tradeoff (paper: [arXiv:2606.22902](https://arxiv.org/abs/2606.22902), homepage: `omnisource.cn/agent-as-a-router`, benchmark: `CodeRouterBench` on Hugging Face). The repo is primarily a Python reproduction package, evaluation scripts, and benchmark data — mostly developer/researcher-facing, with no consumer marketing site.

The one real **product UI** in the repo is **AgenticRouter.Gui**: a Windows system-tray application (.NET MAUI Blazor Hybrid) that shows a dashboard of routing, cost, and governance telemetry for the ACRouter proxy. It launches hidden in the tray; right-click → "Show Dashboard" opens a single window hosting a Razor single-page dashboard. **This design system is built entirely around that dashboard's visual language.**

Important caveat: the dashboard's actual Razor component markup (`Components/*.razor`) is **not present in the repository** — only the compiled Tailwind stylesheet (`wwwroot/css/app.css`), the mock data model (`Models/DashboardData.cs`), the agent-color utility (`Utils/ColorUtils.cs`), and an extremely detailed prose spec (`docs/agenticrouter-gui-dashboard.md`) describing every tab, card, and interaction. Everything in this system's `ui_kits/dashboard/` was reconstructed from that CSS + data + spec, not copied component source — treat it as a faithful recreation, not a byte-for-byte port. If `Components/*.razor` becomes available later, re-check the UI kit against it.

Currently the dashboard displays **hard-coded mock data only** — it is not wired to the live proxy yet. This design system's mock data mirrors that same fixture data.

## No logo

The source repository defines no company or product logo/brand mark. `assets/appicon.svg` is the app's actual Windows tray/taskbar icon (a simple geometric mark: a dark rounded square with a network of dots), copied verbatim from `src/AgenticRouter.Gui/Resources/AppIcon/appicon.svg` — kept as a reference, but it is **not** a wordmark or logotype, and none should be invented for the company.

`assets/icon-app.svg` (+ `-256.png`/`-32.png`/`-16.png` raster exports) is a newly designed **app/tray icon** — requested directly by the user as a refresh of the same tray+dashboard icon slot: a friendly, original robot-face glyph (head only, smiling eyes, no mouth or torso — not any copyrighted character), flat and gradient-free, built only from this system's existing color tokens (slate body/antenna, deep-navy eyes for contrast against the light head, emerald antenna tip). It's a UI utility icon, not a brand mark — same category as the original `appicon.svg`, just a friendlier take. Use it for the system tray glyph and the dashboard header's small mark (see `guidelines/brand-app-icon.card.html`). Everywhere else, render "AgenticRouter" or "Router Optimization Engine" in plain type (see Typography below).

## Index — what's in this project

- `styles.css` — the single global CSS entry point (imports everything under `tokens/`).
- `tokens/` — `colors.css`, `typography.css`, `spacing.css`, `effects.css`, `fonts.css`.
- `assets/appicon.svg` — the product's real tray-icon SVG (see "No logo" above).
- `guidelines/` — 11 foundation specimen cards (Colors ×4, Type ×2, Spacing ×2, Brand ×3) shown in the Design System tab.
- `components/` — 11 reusable React primitives, grouped by concern:
  - `core/Icon`
  - `feedback/Badge`, `feedback/Tooltip`, `feedback/ProgressBar`
  - `navigation/Tabs`
  - `overlay/Modal`
  - `forms/Button`, `forms/Input`
  - `data/Card`, `data/StatItem`, `data/AgentChip`
- `ui_kits/dashboard/` — the AgenticRouter Dashboard recreation: `index.html` (interactive, all 4 tabs + Settings modal clickable), `Dashboard.jsx`, `Header.jsx`, `LiveStream.jsx`, `CostAnalytics.jsx`, `ModelDistribution.jsx`, `Governance.jsx`, `SettingsModal.jsx`, `mockData.js`.
- `SKILL.md` — portable skill definition for use in Claude Code or other agent environments.

## Intentional additions

The source defines a dashboard's worth of ad-hoc markup (per `docs/agenticrouter-gui-dashboard.md`), not a named component library, so the component set below was factored out of that spec rather than copied one-to-one from a component inventory:

- **Button** — the doc implies primary/secondary/destructive actions (Settings' "Reset Stats"/"Clear History", the tray's "Show Dashboard") but never names a `Button` component explicitly.
- **Input** — same reasoning, for the budget-cap and confirmation-word fields.
- **Tooltip** — generalized from the doc's `data-tip` / `js/tooltips.js` floating-tooltip mechanism into a reusable component.

Everything else (Icon, Badge, ProgressBar, Tabs, Modal, Card, StatItem, AgentChip) maps directly to a named piece of the source (`Icon.razor`, the OK/WARNING/CRITICAL tags, `.progress-bar-track`, the 4-tab nav, `SettingsModal.razor`, `ConversationCard`/`TurnCard`/provider cards, the per-turn stat strip, and the agent color-dot chip).

---

## CONTENT FUNDAMENTALS

The product has almost no marketing copy — it's an internal ops console, so "content" here means **UI microcopy and log/telemetry phrasing**, not brand voice copywriting. Drawing from `docs/agenticrouter-gui-dashboard.md` and `Models/DashboardData.cs`:

- **Voice**: terse, technical, third-person/systems-oriented — never "you"/"we". Labels read like a systems-monitoring tool, not a consumer app: "System Status: OK", "Fallback Engine Engaged", "Route Confirmed: claude-3-haiku".
- **Casing**: Title Case for headers and tab labels ("Live Stream", "Cost Analytics", "Model Distribution", "Governance"); UPPERCASE for short status tags ("OK", "WARNING", "CRITICAL", "LIVE"); sentence case for routing-log messages and mock request/response text.
- **Numbers over adjectives**: nearly every piece of copy is a number or an ID, not prose — costs to 4-6 decimal places (`$0.006310`), token counts, percentages to 1-2 decimals, trace/session IDs (`a4f89c02`). This is a data-dense console; don't pad it with descriptive sentences where a number belongs.
- **Routing-log phrasing pattern**: short declarative fragments, present tense, systems-log style — `"Prompt cache hit: 2,333 tokens read from cache"`, `"Anthropic hourly budget breached; routing restricted"`, `"Route Confirmed: <model>"` as the standing final line of every log.
- **No emoji as decoration** — the only emoji-like glyphs are functional status glyphs: 🤖 (header brand mark, used once), 🚨 (critical breach), ⚠️/⚠ (warning/fallback), ● (live dot, rendered as a real dot not emoji). Never used for tone or flourish.
- **Mock request/response text** (for turn drill-downs) reads like real engineer-to-agent chat: specific file names, line numbers, ticket numbers — `"Review the diff for PR #4521 (src/auth/token_service.py, 214 changed lines)"`. Keep invented example content this specific and concrete, never generic filler like "Please review this code."

---

## VISUAL FOUNDATIONS

Source of truth: `src/AgenticRouter.Gui/wwwroot/css/app.css` (compiled Tailwind) + the "Visual theme" section of `docs/agenticrouter-gui-dashboard.md`.

- **Theme**: dark only, no light mode, no toggle. This is fixed at the app level (`html,body,#root { background:#0f172a }`).
- **Color**: a slate neutral scale (`#0f172a` page → `#1e293b` cards/header/nav → `#334155` borders → `#e2e8f0` primary text) plus four semantic hues used consistently: sky `#38bdf8` (accent/info/active nav), emerald `#10b981` (positive/savings/OK), amber `#f59e0b` (warning), red `#ef4444` (critical). A separate 12-color deterministic palette (FNV-1a hash of agent name) tints agent-specific rows/borders/chips — never assigned by hand, always computed from the name so it's stable across sessions.
- **Type**: two families only — **Inter** for all UI text, **JetBrains Mono** for every number (costs, token counts, timestamps, session/trace IDs, model names). This split is load-bearing: if a value could ever be a number or code-like token, it's mono; everything else is Inter. Sizes used: 10px uppercase micro-labels, 11-12px body/stat values, 13-14px card titles, 18px (`text-lg`) for the single header brand title. Weights: 500/600/700 only, no light weights.
- **Spacing**: a small, tight scale (2px–24px, i.e. Tailwind's 0.125rem–1.5rem) — dense, information-forward layout, not airy marketing spacing. Cards use ~10-14px internal padding; gaps between stat items are 8-20px depending on density.
- **Backgrounds**: flat solid fills only. No photography, no illustration, no gradients, no textures/patterns, no grain. The one exception is a subtle radial-free flat `rgba` overlay + blur behind modals (`rgba(15,23,42,0.7)` + 2px blur) — functional dimming, not decorative.
- **Animation**: minimal and purely functional — no entrance choreography, no bounces, no easing flourishes.
  - A single continuous loop: `pulse-dot` (2s ease-in-out, opacity 1→0.4→1) on the "LIVE" indicator and the green status dot. This is the *only* infinite/looping animation in the product.
  - A one-shot attention flash (`flash-amber`/`flash-red`, 1.2s × 3 repetitions, box-shadow pulse) fires exactly once per user action (clicking the header's alert banner), never passively.
  - Everything else is a plain 150ms `cubic-bezier(0.4,0,0.2,1)` color/background/border transition — hovers, active-tab underline.
- **Hover states**: cards lighten from `#1e293b` → `#263548` (`.card-hover:hover`); interactive text/icons go `hover:opacity-80` or shift to a lighter shade (e.g. `hover:text-slate-200`); no color inversion, no underlines-on-hover for non-link UI.
- **Press/active states**: not explicitly defined in the source (no separate `:active` rules) — treat press as an immediate application of the hover state, no scale/shrink effect anywhere in this product.
- **Borders**: 1px solid is the *only* structural depth cue — every card, input, and divider is a 1px slate-700 (or semantic-tinted) border. No inner/outer shadows, no elevation system, no colored left-border-only card convention (cards that do get a colored edge, like agent-tinted turn cards, tint the *entire* border plus a slightly heavier 3px left edge — never a plain white/neutral card with only a colored strip).
- **Corner radii**: a small, restrained set — 2px (progress track), 4px (`rounded`, tags/chips), 8px (`rounded-lg`, cards/modals/inputs), and full/circular (dots, avatars). Never larger than 8px on a rectangular surface.
- **Cards**: solid `#1e293b` fill, 1px border, `rounded-lg` (8px), no shadow whatsoever — depth comes purely from fill contrast against the `#0f172a` page and the border, never from box-shadow.
- **Transparency/blur**: used exactly once, functionally — the modal backdrop (dim + 2px blur) to focus attention on the dialog. Not used decoratively on cards or panels.
- **Layout**: the whole app is a fixed, non-scrolling `100vh` shell; individual panels scroll internally. A draggable split-pane divider (Live Stream) is the one resizable layout element, clamped 20-65%.
- **Imagery**: none. This is a data-dense console with zero photography or illustration; all visual interest comes from color-coded data (charts, chips, badges), not imagery.

## ICONOGRAPHY

The source describes icons as **"small inline SVG glyphs"** (`Components/Icon.razor`) — a single custom glyph component, not a named icon library, and no actual `.svg`/icon-font files are committed to the repo to copy. **Substitution flagged**: this design system's `Icon` component (`components/core/Icon.jsx`) sources glyphs from **Lucide** (via `unpkg.com/lucide-static`, recolored with a CSS `mask-image` so it always matches the current text color) as the closest available match to the described minimal-stroke style. If real icon assets from the Razor components become available, swap them in and drop the CDN dependency.

- No icon font is used or implied by the source.
- No PNG icons — everything implied is vector/inline SVG.
- Emoji ARE used, but only as functional status glyphs, never decoratively — see Content Fundamentals above (🤖, 🚨, ⚠️/⚠).
- No unicode-symbol-as-icon convention beyond the plain "✕" close glyph and "⚙" settings glyph implied by the Settings button.
- The one real vector asset copied from the source is `assets/appicon.svg` (see "No logo" above) — copy it wherever the literal app icon is needed (window/taskbar icon mockups); don't use it as a stand-in logo elsewhere.

---

## SKILL.md

See `SKILL.md` at the project root — a portable skill definition (Claude-Code/Agent-Skills compatible) pointing at this system for anyone prototyping AgenticRouter-branded interfaces.
