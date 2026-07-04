using Microsoft.AspNetCore.Components.WebView.Maui;

namespace AgenticRouter.Gui;

/// <summary>
/// The app's single page: a BlazorWebView filling the window, hosting the Razor dashboard rooted at
/// <see cref="Components.Dashboard"/>.
/// </summary>
public sealed class MainPage : ContentPage
{
    public MainPage()
    {
        Content = new BlazorWebView
        {
            HostPage = "wwwroot/index.html",
            RootComponents =
            {
                new RootComponent
                {
                    Selector = "#root",
                    ComponentType = typeof(Components.Dashboard),
                },
            },
        };
    }
}
