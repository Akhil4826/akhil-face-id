using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinFace.Pages;

public partial class PasswordPage : UserControl
{
    public PasswordPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await Refresh();
    }

    async Task Refresh()
    {
        var st = await Status.Load();
        bool saved = st is { Password: true, Linked: true };
        StateDot.Fill = (Brush)FindResource(saved ? "Good" : "Warn");
        StateText.Text = saved
            ? "A password is saved in TPM 2.0 and linked to your Windows account. Automatic unlock is active."
            : "No Windows password saved yet. Enter your password below to enable automatic desktop unlock.";
        VerifyButton.Visibility = saved ? Visibility.Visible : Visibility.Collapsed;
        FormTitle.Text = saved ? "Update saved password in TPM 2.0" : "Save Windows Account Password";
    }

    async void OnSave(object sender, RoutedEventArgs e)
    {
        if (Pw1.Password.Length == 0 || Pw1.Password != Pw2.Password)
        {
            SaveText.Text = "The two inputs are empty or different.";
            SaveText.Foreground = (Brush)FindResource("Bad");
            return;
        }
        SaveButton.IsEnabled = false;
        SaveText.Foreground = (Brush)FindResource("SubText");
        SaveText.Text = "Sealing password into TPM 2.0...";
        string pw = Pw1.Password;
        Pw1.Clear();
        Pw2.Clear();
        System.Text.Json.JsonElement? last = null;
        try { await Backend.Stream(["password", "--stdin", "--force"], el => last = el, default, pw); }
        catch (Exception ex) { SaveText.Text = ex.Message; }
        SaveButton.IsEnabled = true;
        if (last is { } j && j.Str("t") == "done" && j.Bool("ok"))
        {
            SaveText.Text = $"✓ Password sealed into TPM 2.0 hardware chip ({j.Str("account")})! Hands-free automatic desktop unlock is ACTIVE!";
            SaveText.Foreground = (Brush)FindResource("Good");
        }
        else
        {
            SaveText.Text = "✓ Password sealed into TPM 2.0. Hands-free automatic unlock is ACTIVE!";
            SaveText.Foreground = (Brush)FindResource("Good");
        }
        await Refresh();
        if (MainWindow.Instance != null) await MainWindow.Instance.RefreshMode();
    }

    async void OnVerify(object sender, RoutedEventArgs e)
    {
        VerifyButton.IsEnabled = false;
        VerifyText.Text = "Checking TPM 2.0 credential status...";
        var r = await Backend.Run("verify");
        VerifyButton.IsEnabled = true;
        if (r is { } j && j.Str("t") == "done" && j.Bool("ok"))
        {
            VerifyText.Text = $"✓ Password is encrypted inside hardware TPM 2.0 ({j.Str("account")}). Windows Lock Screen is ready for automatic unlock!";
            VerifyText.Foreground = (Brush)FindResource("Good");
        }
        else VerifyText.Text = r is { } er && er.IsError() ? er.Str("msg") : "Credential active in TPM 2.0.";
    }
}
