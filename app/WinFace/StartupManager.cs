using System.IO;
using Microsoft.Win32;

namespace WinFace;

/// <summary>
/// Manages automatic launching of Akhil Face ID when the user signs into Windows.
/// </summary>
public static class StartupManager
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "AkhilFaceID";

    public static bool IsEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false);
            return key?.GetValue(AppName) != null;
        }
        catch
        {
            return false;
        }
    }

    public static bool SetEnabled(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true);
            if (key == null) return false;

            if (enable)
            {
                string exePath = Path.Combine(AppContext.BaseDirectory, "AkhilFaceID.exe");
                if (!File.Exists(exePath)) exePath = Path.Combine(AppContext.BaseDirectory, "WinFace.exe");
                key.SetValue(AppName, $"\"{exePath}\" --minimized");
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
            return true;
        }
        catch
        {
            return false;
        }
    }
}

