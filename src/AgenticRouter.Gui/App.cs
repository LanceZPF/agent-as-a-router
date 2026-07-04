namespace AgenticRouter.Gui;

/// <summary>
/// MAUI application root. Creates the single dashboard window; tray behavior (launch hidden, hide on
/// minimize/close, tray context menu) is wired natively in
/// <see cref="Platforms.Windows.TrayWindowManager"/> once the native window exists.
/// </summary>
public sealed class App : Application
{
    protected override Window CreateWindow(IActivationState? activationState) =>
        new(new MainPage())
        {
            Title = "AgenticRouter Dashboard",
            Width = 1440,
            Height = 900,
        };
}
