namespace AgenticRouter.Gui.WinUI;

/// <summary>
/// WinUI bootstrap for the MAUI app. The XAML compiler also generates the process entry point from the
/// companion App.xaml, which is why this class must remain XAML-backed.
/// </summary>
public partial class App : MauiWinUIApplication
{
    public App()
    {
        InitializeComponent();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
