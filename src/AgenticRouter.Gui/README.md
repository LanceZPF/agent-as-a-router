# AgenticRouter.Gui

A Windows system tray application providing UI/UX for the AgenticRouter proxy. This project is
independent of the `AgenticRouter` proxy service - it does not start, stop, or otherwise manage it.

## Behavior

- On launch, only a tray icon appears (no window, no console).
- Right-click the tray icon and select **Show Dashboard** to open the dashboard window (or double-click
  the icon). The dashboard is a single-page app rendered with [Photino](https://tryphotino.io/)
  (`wwwroot/index.html`).
- Clicking the dashboard window's minimize button, or its close (X) button, hides it back into the tray
  icon rather than minimizing to the taskbar or exiting the app.
- Select **Exit** from the tray context menu to actually quit.

## Current limitations

The dashboard currently displays hard-coded placeholder data only (see `wwwroot/app.js`) - it is not yet
connected to the running AgenticRouter proxy.

## Running

Requires Windows (the tray icon and window-hiding behavior use Win32 APIs via `NativeMethods.cs`).

```powershell
cd src/AgenticRouter.Gui
dotnet run
```
