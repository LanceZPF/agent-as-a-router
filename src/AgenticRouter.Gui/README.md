# AgenticRouter.Gui

A Windows system tray application providing UI/UX for the AgenticRouter proxy, built as a
**.NET MAUI Blazor Hybrid** app. This project is independent of the `AgenticRouter` proxy service - it
does not start, stop, or otherwise manage it.

## Behavior

- On launch, only a tray icon appears (no window, no console).
- Right-click the tray icon and select **Show Dashboard** to open the dashboard window (or double-click
  the icon). The dashboard is a Razor single-page app hosted in a `BlazorWebView` - see
  [`docs/gui/dashboard.md`](../../docs/gui/dashboard.md) for a full
  description of the UI.
- Clicking the dashboard window's minimize button, or its close (X) button, hides it back into the tray
  icon rather than minimizing to the taskbar or exiting the app.
- Select **Exit** from the tray context menu to actually quit.

## Current limitations

The Live Stream tab and the Cost Analytics tab's Token Compounding chart connect to the running
AgenticRouter proxy's telemetry hub (`Services/LiveDataStore.cs`, default
`http://localhost:5001/telemetry/hub`, not yet configurable) and show nothing until the proxy is
reachable and has forwarded at least one request. The rest of the dashboard (Model Distribution,
Governance, the header ticker, and Cost Analytics' other two charts) still reads from hard-coded
mock data (`Models/DashboardData.cs`) - no telemetry source exists for that data yet. See
[`docs/router/telemetry.md`](../../docs/router/telemetry.md) for the full pipeline and
[`docs/gui/backlog.md`](../../docs/gui/backlog.md) for what's left.

## Project layout

| Path | Purpose |
| --- | --- |
| `App.cs`, `MainPage.cs`, `MauiProgram.cs` | MAUI shell: one window hosting a full-window `BlazorWebView`. |
| `Components/` | The dashboard's Razor components (tabs, cards, settings modal, icons). |
| `Models/DashboardData.cs` | Dashboard data model + the mock data. |
| `Services/LiveDataStore.cs` | SignalR client connecting to the proxy's telemetry hub; accumulates and re-aggregates live routing events into `Conversation`/`ConversationTurn` records. |
| `Services/LiveConversationMapper.cs` | Maps `AgenticRouter.Gui.Telemetry`'s live-aggregation output onto the dashboard's `Conversation`/`ConversationTurn` view-model shape, with honest defaults for fields telemetry doesn't cover. |
| `Platforms/Windows/TrayWindowManager.cs` | Win32 tray icon + WndProc subclass implementing the tray-resident window behavior (MAUI has no built-in tray support). |
| `wwwroot/` | Blazor host page and the dashboard stylesheet (`css/app.css`). Static source - no build step. |

Charts are rendered with [Blazor-ApexCharts](https://github.com/apexcharts/Blazor-ApexCharts) (the
`Blazor-ApexCharts-MAUI` package), which bundles its JavaScript as static web assets, so the charts work
offline inside the WebView.

## Prerequisites

- Windows 10 1809+ (the app targets `net10.0-windows10.0.19041.0` and uses Win32 tray APIs).
- The **.NET MAUI workload**: either check ".NET Multi-platform App UI development" in the Visual Studio
  installer, or run `dotnet workload install maui-windows`.
- The Microsoft Edge **WebView2 runtime** (preinstalled on Windows 11 and most updated Windows 10
  machines).

## Running

```powershell
cd src/AgenticRouter.Gui
dotnet run
```

Or open the solution in Visual Studio and press F5 (the "Windows Machine" profile runs the app
unpackaged - no MSIX registration or signing needed).

Note: the app starts minimized to the system tray by design. If nothing seems to happen after launch,
look for the AgenticRouter icon in the tray, right-click it, and choose **Show Dashboard**.
