using ApexCharts;
using AgenticRouter.Gui.Platforms.Windows;
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
#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
#endif

        builder.ConfigureLifecycleEvents(events =>
            events.AddWindows(windows =>
                windows.OnWindowCreated(TrayWindowManager.Attach)));

        return builder.Build();
    }
}
