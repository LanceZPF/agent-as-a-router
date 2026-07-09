# 📺 Streaming Log Console Specification

> **Status: Proposed — not yet implemented.** There is no "Console" tab, log buffer, or streaming
> log transport anywhere in `AgenticRouter.Gui` or `src/AgenticRouter/` today — the four tabs are
> Live Stream, Cost Analytics, Model Distribution, and Governance (see
> [`dashboard.md`](dashboard.md)). This document is a UI/UX spec for a proposed fifth tab, written
> before any implementation work started. Treat everything below as design intent, not current
> behavior, until this banner is removed.

A lightweight, real-time log monitoring component with color-coded severity levels and toggleable
auto-scroll behavior.

## 🎛️ Toolbar Interface

The toolbar sits directly above the log viewport and contains the following control elements:

| Element | UI Type | Action / Behavior |
|---|---|---|
| Auto-Scroll | Toggle Switch / Button | ON: Viewport snaps to the newest log entry. OFF: Viewport stays frozen on the current view for manual reading. |
| Clear Buffer | Button (trashcan SVG icon) | Flushes the current text buffer and empties the console screen. |

---

## 🖼️ Console Viewport

The main display area features a dark-mode theme designed for maximum readability during
continuous log streaming.

### Visual Styling

- Font Family: Monospace (Fira Code, Courier New, or SF Mono)
- Font Size: 13px / Line Height: 1.5

### 🎨 Color-Coded Log Levels

Text coloring maps directly to log severity levels to allow for rapid visual scanning:

```
[2026-07-08 21:10:01] [DEBUG]  Connecting to internal database cluster...
[2026-07-08 21:10:02] [INFO]   Successfully connected to database: 'prod_db'.
[2026-07-08 21:10:15] [WARN]   API latency spike detected: 450ms (threshold: 200ms).
[2026-07-08 21:11:00] [ERROR]  Failed to write payload to session token cache.
[2026-07-08 21:11:01] [FATAL]  Out of memory error. Service shutting down.
```

- ⚪ Gray (`#A0A0A0`): `DEBUG` — Low-level diagnostic data.
- 🟢 Green (`#4CAF50`): `INFO` — Standard system operational events.
- 🟡 Yellow (`#FFC107`): `WARN` — Non-blocking anomalies or performance alerts.
- 🔴 Red (`#F44336`): `ERROR` — Operational failures requiring intervention.
- 🟣 Magenta (`#E91E63`): `FATAL` / `CRITICAL` — Total application crash.

---

## ⚙️ Core Behavior Rules

### 1. Auto-Scroll Logic

- **Enabled (Default)**: When a new log line arrives, the component automatically calculates the
  container's maximum scroll height and instantly scrolls down to display the new line.
- **Disabled**: New lines append to the bottom of the document out of view, but the scrollbar
  position does not change.

### 2. Smart-Disengage (UX Safeguard)

- If Auto-Scroll is ON and the user manually scrolls upward using their mouse wheel or trackpad,
  Auto-Scroll automatically switches to OFF.
- This prevents the text from violently jumping away from the user while they are actively trying
  to highlight or read an earlier log entry.
