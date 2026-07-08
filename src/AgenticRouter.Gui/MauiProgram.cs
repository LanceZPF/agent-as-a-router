using ApexCharts;
using AgenticRouter.Gui.Platforms.Windows;
using AgenticRouter.Gui.Services;
using Microsoft.Maui.LifecycleEvents;

namespace AgenticRouter.Gui;

/// <summary>
/// Composition root for the MAUI Blazor Hybrid app.
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Builds the MAUI app: registers the BlazorWebView, the ApexCharts chart service, and hooks the
    /// Windows lifecycle so the main window becomes a tray-resident window at creation time.
    /// </summary>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddApexChartsMaui();
        // Live routing telemetry from the AgenticRouter proxy (see Services/LiveDataStore.cs). A
        // singleton so the SignalR connection and accumulated conversation state survive navigation
        // between tabs; Dashboard.razor starts the connection on first render.
        builder.Services.AddSingleton<LiveDataStore>();
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.ConfigureLifecycleEvents(events =>
            events.AddWindows(windows =>
                windows.OnWindowCreated(TrayWindowManager.Attach)));

        return builder.Build();
    }
}
