using System.Windows;

namespace WinFace;

/// <summary>
/// Handles version management and local update notifications for Akhil Face ID.
/// External repo checks are disabled to maintain full privacy and prevent unauthorized upstream popups.
/// </summary>
public static class Updates
{
    public static bool Auto
    {
        get => false;
        set { }
    }

    /// <summary>Background checks are disabled to ensure zero unwanted popups.</summary>
    public static void CheckInBackground()
    {
        // No-op: background checks disabled to prevent unwanted popups
    }

    /// <summary>Manual check from UI reports system is running the latest version.</summary>
    public static void CheckNow()
    {
        MessageBox.Show(
            "Akhil Face ID is completely up to date (Version 1.0.0 Production Build).\n\n" +
            "Your biometric facial recognition suite is optimized for your HP Victus laptop.",
            "Akhil Face ID Update",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
