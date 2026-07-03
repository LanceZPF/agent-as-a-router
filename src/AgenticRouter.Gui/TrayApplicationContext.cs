using System.Windows.Forms;

namespace AgenticRouter.Gui;

/// <summary>
/// The application's <see cref="ApplicationContext"/>: keeps the Windows Forms message loop alive via the
/// tray icon alone, with no main form. The dashboard is a Photino window shown/hidden on demand.
/// </summary>
internal sealed class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly DashboardWindow _dashboard;

    public TrayApplicationContext()
    {
        _dashboard = new DashboardWindow();

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("Show Dashboard", image: null, onClick: (_, _) => _dashboard.Show());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", image: null, onClick: (_, _) => ExitApplication());

        _trayIcon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Text = "AgenticRouter",
            ContextMenuStrip = contextMenu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => _dashboard.Show();
    }

    private void ExitApplication()
    {
        _trayIcon.Visible = false;
        _dashboard.Exit();
        _dashboard.Dispose();
        Application.Exit();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _trayIcon.Dispose();
        }

        base.Dispose(disposing);
    }
}
