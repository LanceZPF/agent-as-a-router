using System.Windows.Forms;

namespace AgenticRouter.Gui;

/// <summary>
/// Entry point for the AgenticRouter tray application. This process is independent of the AgenticRouter
/// proxy service - it only provides a Windows system tray icon and, on demand, a dashboard window.
/// </summary>
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TrayApplicationContext());
    }
}
