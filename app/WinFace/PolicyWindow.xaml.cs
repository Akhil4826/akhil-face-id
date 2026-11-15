using System.IO;
using System.Windows;

namespace WinFace;

/// <summary>Shown once when the privacy policy / warning version changed after setup was already done.</summary>
public partial class PolicyWindow : Window
{
    public PolicyWindow()
    {
        InitializeComponent();
        string p = Path.Combine(Backend.Dir, "PRIVACY.md");
        if (!File.Exists(p)) p = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PRIVACY.md");
        Policy.Text = File.Exists(p) ? File.ReadAllText(p) : "Akhil Face ID Privacy Policy & Security Architecture\n\nAll biometric templates are stored strictly on this PC in TPM-encrypted storage.";
    }

    void OnAgree(object sender, RoutedEventArgs e) => Continue.IsEnabled = Agree.IsChecked == true;

    void OnContinue(object sender, RoutedEventArgs e)
    {
        AppState.PolicyAccepted = true;
        DialogResult = true;
    }

    void OnQuit(object sender, RoutedEventArgs e) => DialogResult = false;
}
