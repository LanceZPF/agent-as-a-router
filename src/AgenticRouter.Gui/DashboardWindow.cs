using System.Drawing;
using Photino.NET;

namespace AgenticRouter.Gui;

/// <summary>
/// Owns the single Photino-hosted dashboard window and runs its native message loop on a dedicated STA
/// thread, independent of the Windows Forms message loop that owns the tray icon on the main thread.
/// Photino's <see cref="PhotinoWindow.WaitForClose"/> both creates the native window and blocks pumping
/// its message loop, so it cannot share a thread with <c>Application.Run</c>.
/// </summary>
internal sealed class DashboardWindow : IDisposable
{
    private const string DashboardUrl = "wwwroot/index.html";

    private readonly Thread _uiThread;
    private readonly ManualResetEventSlim _windowReady = new(initialState: false);
    private volatile bool _isExiting;
    private PhotinoWindow? _window;
    private Exception? _startupException;

    public DashboardWindow()
    {
        _uiThread = new Thread(RunMessageLoop)
        {
            IsBackground = true,
            Name = "AgenticRouter.Gui.Dashboard"
        };
        _uiThread.SetApartmentState(ApartmentState.STA);
        _uiThread.Start();

        _windowReady.Wait();

        if (_startupException is not null)
        {
            throw new InvalidOperationException("Failed to create the dashboard window.", _startupException);
        }
    }

    /// <summary>Restores the dashboard window and brings it to the foreground.</summary>
    public void Show()
    {
        if (_window is not null)
        {
            NativeMethods.ShowAndActivate(_window.WindowHandle);
        }
    }

    /// <summary>Closes the native window for real and ends its message loop, allowing the process to exit.</summary>
    public void Exit()
    {
        _isExiting = true;
        _window?.Close();
    }

    private void RunMessageLoop()
    {
        try
        {
            _window = new PhotinoWindow()
                .SetTitle("AgenticRouter Dashboard")
                .SetUseOsDefaultSize(false)
                .SetSize(new Size(1024, 700))
                .Center()
                .RegisterWindowCreatedHandler((_, _) => NativeMethods.Hide(_window!.WindowHandle))
                .RegisterMinimizedHandler((_, _) => NativeMethods.Hide(_window!.WindowHandle))
                .RegisterWindowClosingHandler((_, _) =>
                {
                    if (_isExiting)
                    {
                        return false; // Let the real close proceed.
                    }

                    NativeMethods.Hide(_window!.WindowHandle);
                    return true; // Cancel the close; hide to tray instead.
                })
                .Load(DashboardUrl);
        }
        catch (Exception ex)
        {
            // Unblock the constructor rather than hanging forever if window creation fails (e.g. the
            // native Photino/WebView2 runtime is missing or misconfigured).
            _startupException = ex;
            _windowReady.Set();
            return;
        }

        _windowReady.Set();

        // Blocks this thread, pumping the native window's message loop, until Exit() closes it for real.
        _window.WaitForClose();
    }

    public void Dispose()
    {
        _windowReady.Dispose();
    }
}
