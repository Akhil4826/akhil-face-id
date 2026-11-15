using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinFace.Pages;

public partial class SettingsPage : UserControl
{
    record Cam(string Name, string Id, bool Usable);
    Status? _st;

    public SettingsPage()
    {
        InitializeComponent();
        for (int i = 1; i <= 5; ++i)
            Fails.Children.Add(new RadioButton { Style = (Style)FindResource("Seg"), GroupName = "fails", Content = i.ToString(), Tag = i });
        Search.ValueChanged += (_, _) => SearchVal.Text = $"{Search.Value:0.0} s";
        Challenge.ValueChanged += (_, _) => ChallengeVal.Text = $"{Challenge.Value:0.0} s";
        Loaded += async (_, _) => await Load();
    }

    async Task Load()
    {
        _st = await Status.Load();
        if (_st == null) { SaveText.Text = "Could not read application settings."; return; }

        DataPathText.Text = Backend.DataDir;
        StartupLaunch.IsChecked = StartupManager.IsEnabled();

        Search.Value = _st.SearchMs / 1000.0;
        Challenge.Value = _st.ChallengeMs / 1000.0;
        StrictBalanced.IsChecked = _st.Strictness == 0;
        StrictStrict.IsChecked = _st.Strictness == 1;
        StrictRelaxed.IsChecked = _st.Strictness == 2;
        foreach (RadioButton rb in Fails.Children) rb.IsChecked = (int)rb.Tag == _st.MaxFails;
        Sounds.IsChecked = _st.Sounds;
        SearchVal.Text = $"{Search.Value:0.0} s";
        ChallengeVal.Text = $"{Challenge.Value:0.0} s";

        Cameras.Items.Clear();
        var auto = new Cam("Automatic (Recommended)", "auto", true);
        Cameras.Items.Add(CamItem(auto, "Automatically detects the built-in HP Wide Vision HD camera", true));
        bool any = false;
        var e = await Backend.Run("cameras");
        if (e is { } j && j.TryGetProperty("list", out var list))
            foreach (var c in list.EnumerateArray())
            {
                any = true;
                var cam = new Cam(c.Str("name"), c.Str("id"), c.Bool("usable"));
                string note = cam.Usable ? "Hardware Color Webcam (USB Bus)" 
                             : c.Bool("infrared") ? "Infrared (Windows Hello IR) sensor - not supported"
                             : "Virtual camera - blocked by hardware policy";
                var item = CamItem(cam, note, cam.Usable);
                Cameras.Items.Add(item);
                if (cam.Usable && _st.Camera.Length > 0 && cam.Id.Contains(_st.Camera, StringComparison.OrdinalIgnoreCase)) item.IsSelected = true;
            }
        if (Cameras.SelectedItem == null)
        {
            if (_st.Camera.Length == 0) ((ListBoxItem)Cameras.Items[0]).IsSelected = true;
            else SaveText.Text = "Previously selected camera disconnected - choose one above.";
        }
        if (!any) Cameras.Items.Add(new ListBoxItem { Content = new TextBlock { Text = "No cameras detected", Style = (Style)FindResource("Sub") }, IsEnabled = false });
    }

    ListBoxItem CamItem(Cam cam, string note, bool enabled)
    {
        var sp = new StackPanel { Margin = new Thickness(4) };
        sp.Children.Add(new TextBlock { Text = cam.Name, Style = (Style)FindResource("Body"), FontWeight = FontWeights.SemiBold });
        sp.Children.Add(new TextBlock { Text = note, Style = (Style)FindResource("Sub"), FontSize = 12 });
        return new ListBoxItem { Content = sp, Tag = cam, IsEnabled = enabled };
    }

    void OnStartupToggle(object sender, RoutedEventArgs e)
    {
        bool enable = StartupLaunch.IsChecked == true;
        StartupManager.SetEnabled(enable);
        SaveText.Text = enable ? "Automatic startup enabled." : "Automatic startup disabled.";
        SaveText.Foreground = (Brush)FindResource("Good");
    }

    void OnOpenDataFolder(object sender, RoutedEventArgs e)
    {
        try
        {
            if (!Directory.Exists(Backend.DataDir)) Directory.CreateDirectory(Backend.DataDir);
            Process.Start(new ProcessStartInfo("explorer.exe", Backend.DataDir) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open data directory: {ex.Message}", "Akhil Face ID", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    void OnPlay(object sender, RoutedEventArgs e)
    {
        string p = Path.Combine(Backend.Dir, "assets", "sfx_unlock.wav");
        if (!File.Exists(p)) p = Path.Combine(AppContext.BaseDirectory, "assets", "sfx_unlock.wav");
        if (File.Exists(p))
        {
            try { new System.Media.SoundPlayer(p).Play(); } catch { }
        }
    }

    async void OnSave(object sender, RoutedEventArgs e)
    {
        SaveButton.IsEnabled = false;
        SaveText.Text = "Saving configuration...";
        SaveText.Foreground = (Brush)FindResource("SubText");

        int strict = StrictStrict.IsChecked == true ? 1 : StrictRelaxed.IsChecked == true ? 2 : 0;
        int fails = Fails.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true) is { Tag: int f } ? f : 3;
        int searchMs = (int)Math.Round(Search.Value * 1000);
        int challengeMs = (int)Math.Round(Challenge.Value * 1000);
        bool sounds = Sounds.IsChecked == true;

        string camVal = Cameras.SelectedItem is ListBoxItem { Tag: Cam { Id: not "auto" } c } ? c.Id : "";

        await Backend.Run("set", "Strictness", strict.ToString());
        await Backend.Run("set", "MaxFails", fails.ToString());
        await Backend.Run("set", "SearchMs", searchMs.ToString());
        await Backend.Run("set", "ChallengeMs", challengeMs.ToString());
        await Backend.Run("set", "Sounds", sounds ? "1" : "0");
        await Backend.Run("set", "Camera", camVal);

        SaveText.Text = "✓ Settings saved successfully.";
        SaveText.Foreground = (Brush)FindResource("Good");
        SaveButton.IsEnabled = true;
        _st = await Status.Load();
    }
}
