using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace WinFace;

/// <summary>
/// Native Windows System Tray integration using Win32 Shell_NotifyIcon.
/// Provides minimize-to-tray, tray tooltip, right-click context menu, and desktop notifications.
/// </summary>
public sealed class TrayManager : IDisposable
{
    private const int WM_USER = 0x0400;
    private const int WM_TRAYICON = WM_USER + 101;
    private const int WM_LBUTTONUP = 0x0202;
    private const int WM_RBUTTONUP = 0x0205;
    private const int NIM_ADD = 0x00000000;
    private const int NIM_MODIFY = 0x00000001;
    private const int NIM_DELETE = 0x00000002;
    private const int NIF_MESSAGE = 0x00000001;
    private const int NIF_ICON = 0x00000002;
    private const int NIF_TIP = 0x00000004;
    private const int NIF_INFO = 0x00000010;
    private const int NIIF_INFO = 0x00000001;

    private const uint IMAGE_ICON = 1;
    private const uint LR_LOADFROMFILE = 0x00000010;
    private const uint LR_DEFAULTSIZE = 0x00000040;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct NOTIFYICONDATA
    {
        public int cbSize;
        public IntPtr hWnd;
        public int uID;
        public int uFlags;
        public int uCallbackMessage;
        public IntPtr hIcon;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string szTip;
        public int dwState;
        public int dwStateMask;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szInfo;
        public int uTimeoutOrVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string szInfoTitle;
        public int dwInfoFlags;
        public Guid guidItem;
        public IntPtr hBalloonIcon;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern bool Shell_NotifyIcon(int dwMessage, ref NOTIFYICONDATA lpData);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr CreatePopupMenu();

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool AppendMenu(IntPtr hMenu, int uFlags, IntPtr uIDNewItem, string lpNewItem);

    [DllImport("user32.dll")]
    private static extern int TrackPopupMenu(IntPtr hMenu, int uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

    [DllImport("user32.dll")]
    private static extern bool DestroyMenu(IntPtr hMenu);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT { public int X; public int Y; }

    private const int TPM_RETURNCMD = 0x0100;
    private const int MF_STRING = 0x0000;
    private const int MF_SEPARATOR = 0x0800;

    private readonly Window _window;
    private IntPtr _hwnd;
    private IntPtr _hIcon;
    private bool _installed;

    public TrayManager(Window window)
    {
        _window = window;
    }

    public void Install()
    {
        var helper = new WindowInteropHelper(_window);
        _hwnd = helper.EnsureHandle();

        var source = HwndSource.FromHwnd(_hwnd);
        source?.AddHook(WndProc);

        string icoPath = Path.Combine(AppContext.BaseDirectory, "akhilfaceid.ico");
        if (!File.Exists(icoPath)) icoPath = Path.Combine(AppContext.BaseDirectory, "winface.ico");
        if (File.Exists(icoPath))
        {
            _hIcon = LoadImage(IntPtr.Zero, icoPath, IMAGE_ICON, 16, 16, LR_LOADFROMFILE);
        }

        var nid = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = WM_TRAYICON,
            hIcon = _hIcon,
            szTip = "Akhil Face ID - Active & Secure"
        };

        _installed = Shell_NotifyIcon(NIM_ADD, ref nid);
    }

    public void ShowNotification(string title, string message)
    {
        if (!_installed) return;
        var nid = new NOTIFYICONDATA
        {
            cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = 1,
            uFlags = NIF_INFO,
            szInfoTitle = title,
            szInfo = message,
            dwInfoFlags = NIIF_INFO
        };
        Shell_NotifyIcon(NIM_MODIFY, ref nid);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_TRAYICON)
        {
            int action = lParam.ToInt32();
            if (action == WM_LBUTTONUP)
            {
                RestoreWindow();
                handled = true;
            }
            else if (action == WM_RBUTTONUP)
            {
                ShowContextMenu();
                handled = true;
            }
        }
        return IntPtr.Zero;
    }

    private void RestoreWindow()
    {
        _window.Show();
        _window.WindowState = WindowState.Normal;
        SetForegroundWindow(_hwnd);
        _window.Activate();
    }

    private void ShowContextMenu()
    {
        IntPtr hMenu = CreatePopupMenu();
        AppendMenu(hMenu, MF_STRING, (IntPtr)101, "Open Akhil Face ID");
        AppendMenu(hMenu, MF_STRING, (IntPtr)102, "Start Face Recognition Test");
        AppendMenu(hMenu, MF_STRING, (IntPtr)103, "Security Diagnostics");
        AppendMenu(hMenu, MF_SEPARATOR, IntPtr.Zero, string.Empty);
        AppendMenu(hMenu, MF_STRING, (IntPtr)104, "Exit");

        GetCursorPos(out POINT pt);
        SetForegroundWindow(_hwnd);
        int cmd = TrackPopupMenu(hMenu, TPM_RETURNCMD, pt.X, pt.Y, 0, _hwnd, IntPtr.Zero);
        DestroyMenu(hMenu);

        switch (cmd)
        {
            case 101:
                RestoreWindow();
                break;
            case 102:
                RestoreWindow();
                MainWindow.Instance?.Go("test");
                break;
            case 103:
                RestoreWindow();
                MainWindow.Instance?.Go("security");
                break;
            case 104:
                Application.Current.Shutdown();
                break;
        }
    }

    public void Dispose()
    {
        if (_installed)
        {
            var nid = new NOTIFYICONDATA
            {
                cbSize = Marshal.SizeOf<NOTIFYICONDATA>(),
                hWnd = _hwnd,
                uID = 1
            };
            Shell_NotifyIcon(NIM_DELETE, ref nid);
            _installed = false;
        }

        if (_hIcon != IntPtr.Zero)
        {
            DestroyIcon(_hIcon);
            _hIcon = IntPtr.Zero;
        }
    }
}

