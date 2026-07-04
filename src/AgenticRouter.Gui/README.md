# AgenticRouter.Gui

A Windows system tray application providing UI/UX for the AgenticRouter proxy. This project is
independent of the `AgenticRouter` proxy service - it does not start, stop, or otherwise manage it.

## Behavior

- On launch, only a tray icon appears (no window, no console).
- Right-click the tray icon and select **Show Dashboard** to open the dashboard window (or double-click
  the icon). The dashboard is a React single-page app rendered with [Photino](https://tryphotino.io/)
  (`wwwroot/index.html`) - see `dashboard/` below and
  [`docs/agenticrouter-gui-dashboard.md`](../../docs/agenticrouter-gui-dashboard.md) for details on the
  dashboard itself.
- Clicking the dashboard window's minimize button, or its close (X) button, hides it back into the tray
  icon rather than minimizing to the taskbar or exiting the app.
- Select **Exit** from the tray context menu to actually quit.

## Current limitations

The dashboard currently displays hard-coded mock data (see `dashboard/src/data/mockData.ts`) - it is not
yet connected to the running AgenticRouter proxy.

## The dashboard (`dashboard/`)

`wwwroot/` is *generated* - it is not checked into git (see the root `.gitignore`). The dashboard's
source (React + TypeScript + Vite + Tailwind + Recharts) lives in `dashboard/` and is built directly into
`wwwroot/` via Vite's `build.outDir`.

Before running or publishing `AgenticRouter.Gui` for the first time, or after changing anything under
`dashboard/`, build it:

```bash
cd src/AgenticRouter.Gui/dashboard
npm install
npm run build
```

Other useful commands from `dashboard/`:

```bash
npm run dev        # Vite dev server with hot reload, for iterating on the dashboard in a browser
npm run typecheck  # tsc --noEmit
npm run lint       # eslint
```

`vite.config.ts` sets `base: './'` so the built asset references are relative - required for Photino to
load them from the local filesystem (`Load("wwwroot/index.html")`) rather than as absolute web-root paths.

## Running

Requires Windows (the tray icon and window-hiding behavior use Win32 APIs via `NativeMethods.cs`).

```powershell
cd src/AgenticRouter.Gui
dotnet run
```
