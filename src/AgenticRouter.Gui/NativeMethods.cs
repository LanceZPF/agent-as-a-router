using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace AgenticRouter.Gui;

/// <summary>
/// Win32 interop for hiding/restoring the dashboard window. Photino has no cross-platform concept of a
/// window that is fully hidden (only minimized/maximized/restored - see
/// https://github.com/tryphotino/photino.NET/issues/171), so tray-icon behavior on Windows is implemented
/// directly against the native window handle.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class NativeMethods
{
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    /// <summary>Fully hides the window (removed from the taskbar and screen), leaving only the tray icon.</summary>
    public static void Hide(IntPtr windowHandle) => ShowWindow(windowHandle, SW_HIDE);

    /// <summary>Restores and brings the window to the foreground.</summary>
    public static void ShowAndActivate(IntPtr windowHandle)
    {
        ShowWindow(windowHandle, SW_RESTORE);
        ShowWindow(windowHandle, SW_SHOW);
        SetForegroundWindow(windowHandle);
    }
}
