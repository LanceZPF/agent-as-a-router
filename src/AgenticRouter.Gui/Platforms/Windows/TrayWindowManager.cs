using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;

namespace AgenticRouter.Gui.Platforms.Windows;

/// <summary>
/// Makes the MAUI main window tray-resident on Windows. MAUI has no built-in system tray support, so
/// this is implemented directly against Win32: a Shell_NotifyIcon tray icon whose callback messages are
/// received by subclassing the main window's WndProc. The same subclass intercepts minimize and close so
/// both hide the window to the tray instead; only the tray menu's Exit actually quits.
/// </summary>
internal static class TrayWindowManager
{
    private const uint WM_TRAYICON = 0x8000 + 1; // WM_APP + 1
    private const uint WM_CLOSE = 0x0010;
    private const uint WM_SYSCOMMAND = 0x0112;
    private const int SC_MINIMIZE = 0xF020;
    private const int WM_LBUTTONDBLCLK = 0x0203;
    private const int WM_RBUTTONUP = 0x0205;
    private const int WM_CONTEXTMENU = 0x007B;

    private const int GWLP_WNDPROC = -4;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    private const int SW_RESTORE = 9;

    private const uint NIM_ADD = 0x0000;
    private const uint NIM_DELETE = 0x0002;
    private const uint NIF_MESSAGE = 0x0001;
    private const uint NIF_ICON = 0x0002;
    private const uint NIF_TIP = 0x0004;
    private const int IDI_APPLICATION = 32512;

    private const uint MF_STRING = 0x0000;
    private const uint MF_SEPARATOR = 0x0800;
    private const uint TPM_RIGHTBUTTON = 0x0002;
    private const uint TPM_NONOTIFY = 0x0080;
    private const uint TPM_RETURNCMD = 0x0100;
    private const uint SPI_GETWORKAREA = 0x0030;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;

    private const uint CMD_SHOW_DASHBOARD = 1;
    private const uint CMD_EXIT = 2;

    private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    private static IntPtr _hwnd;
    private static IntPtr _originalWndProc;

    // Rooted so the GC never collects the delegate backing the native function pointer installed below.
    private static WndProc? _wndProcDelegate;
    private static bool _isExiting;

    /// <summary>
    /// Called once from the MAUI Windows lifecycle when the native window is created: installs the
    /// WndProc subclass, adds the tray icon, centers the window, and hides it so the app starts in the
    /// tray only.
    /// </summary>
    public static void Attach(Microsoft.UI.Xaml.Window nativeWindow)
    {
        _hwnd = WinRT.Interop.WindowNative.GetWindowHandle(nativeWindow);

        _wndProcDelegate = WindowProc;
        _originalWndProc = SetWindowLongPtrW(_hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));

        AddTrayIcon();

        // MAUI activates (shows) the window right after this lifecycle callback, which would undo an
        // immediate hide - so also queue a low-priority hide to run after that activation. A brief flash
        // on first launch is possible; the Photino version had the same characteristic.
        ShowWindowNative(_hwnd, SW_HIDE);
        nativeWindow.DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, () =>
        {
            CenterOnWorkArea();
            ShowWindowNative(_hwnd, SW_HIDE);
        });
    }

    private static IntPtr WindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        switch (msg)
        {
            case WM_TRAYICON:
                switch ((int)(lParam & 0xFFFF))
                {
                    case WM_LBUTTONDBLCLK:
                        ShowDashboard();
                        return IntPtr.Zero;
                    case WM_RBUTTONUP:
                    case WM_CONTEXTMENU:
                        ShowTrayMenu();
                        return IntPtr.Zero;
                }

                break;

            case WM_SYSCOMMAND when ((long)wParam & 0xFFF0) == SC_MINIMIZE:
                // Minimize hides to the tray instead of going to the taskbar.
                ShowWindowNative(hWnd, SW_HIDE);
                return IntPtr.Zero;

            case WM_CLOSE when !_isExiting:
                // The title bar X hides to the tray; only the tray menu's Exit really closes.
                ShowWindowNative(hWnd, SW_HIDE);
                return IntPtr.Zero;
        }

        return CallWindowProcW(_originalWndProc, hWnd, msg, wParam, lParam);
    }

    private static void ShowDashboard()
    {
        ShowWindowNative(_hwnd, SW_RESTORE);
        ShowWindowNative(_hwnd, SW_SHOW);
        SetForegroundWindow(_hwnd);
    }

    private static void ShowTrayMenu()
    {
        // Required by TrackPopupMenuEx: without foreground status the menu won't dismiss when the user
        // clicks elsewhere (see the Shell_NotifyIcon docs).
        SetForegroundWindow(_hwnd);

        var menu = CreatePopupMenu();
        try
        {
            AppendMenuW(menu, MF_STRING, (UIntPtr)CMD_SHOW_DASHBOARD, "Show Dashboard");
            AppendMenuW(menu, MF_SEPARATOR, UIntPtr.Zero, null);
            AppendMenuW(menu, MF_STRING, (UIntPtr)CMD_EXIT, "Exit");

            GetCursorPos(out var cursor);
            var command = TrackPopupMenuEx(menu, TPM_RETURNCMD | TPM_NONOTIFY | TPM_RIGHTBUTTON, cursor.X, cursor.Y, _hwnd, IntPtr.Zero);

            switch ((uint)command)
            {
                case CMD_SHOW_DASHBOARD:
                    ShowDashboard();
                    break;
                case CMD_EXIT:
                    ExitApplication();
                    break;
            }
        }
        finally
        {
            DestroyMenu(menu);
        }
    }

    private static void ExitApplication()
    {
        _isExiting = true;
        RemoveTrayIcon();

        // WndProc callbacks run on the UI thread, so Quit() can be called directly. The WM_CLOSE this
        // triggers passes through the subclass because _isExiting is now set.
        Application.Current?.Quit();
    }

    private static void AddTrayIcon()
    {
        var data = NewIconData();
        data.uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP;
        data.uCallbackMessage = WM_TRAYICON;
        data.hIcon = LoadIconW(IntPtr.Zero, (IntPtr)IDI_APPLICATION);
        data.szTip = "AgenticRouter";
        Shell_NotifyIconW(NIM_ADD, ref data);
    }

    private static void RemoveTrayIcon()
    {
        var data = NewIconData();
        Shell_NotifyIconW(NIM_DELETE, ref data);
    }

    private static NOTIFYICONDATAW NewIconData() => new()
    {
        cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATAW>(),
        hWnd = _hwnd,
        uID = 1,
        szTip = string.Empty,
        szInfo = string.Empty,
        szInfoTitle = string.Empty,
    };

    private static void CenterOnWorkArea()
    {
        var workArea = default(RECT);
        if (!SystemParametersInfoW(SPI_GETWORKAREA, 0, ref workArea, 0) || !GetWindowRect(_hwnd, out var window))
        {
            return;
        }

        var width = window.Right - window.Left;
        var height = window.Bottom - window.Top;
        var x = workArea.Left + ((workArea.Right - workArea.Left - width) / 2);
        var y = workArea.Top + ((workArea.Bottom - workArea.Top - height) / 2);
        SetWindowPos(_hwnd, IntPtr.Zero, x, y, 0, 0, SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATAW
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public uint uFlags;
        public uint uCallbackMessage;
        public IntPtr hIcon;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;

        public uint dwState;
        public uint dwStateMask;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;

        public uint uTimeoutOrVersion;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;

        public uint dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtrW(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr CallWindowProcW(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIconW(uint dwMessage, ref NOTIFYICONDATAW lpData);

    [DllImport("user32.dll")]
    private static extern IntPtr LoadIconW(IntPtr hInstance, IntPtr lpIconName);

    [DllImport("user32.dll", EntryPoint = "ShowWindow")]
    private static extern bool ShowWindowNative(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenuW(IntPtr hMenu, uint uFlags, UIntPtr uIDNewItem, string? lpNewItem);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    private static extern int TrackPopupMenuEx(IntPtr hMenu, uint uFlags, int x, int y, IntPtr hWnd, IntPtr lptpm);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool SystemParametersInfoW(uint uiAction, uint uiParam, ref RECT pvParam, uint fWinIni);
}
