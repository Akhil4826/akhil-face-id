using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WinFace.Pages;

public partial class HomePage : UserControl
{
    Status? _st;
    bool _systemOk;

    public HomePage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await Refresh();
    }

    async Task Refresh()
    {
        ActiveUserText.Text = $"{Environment.UserName} ({Environment.MachineName})";
        _st = await Status.Load();
        await RunChecks();
        ShowState();
        LoadRecentEvents();
    }

    void ShowState()
    {
        var st = _st;
        if (st == null)
        {
            HeroTitle.Text = "Akhil Face ID Service Unavailable";
            HeroSub.Text = "Native engine (fgsetup.exe) did not respond. Check permissions or reinstall.";
            return;
        }

        ModeOff.IsChecked = st.Mode == "off";
        ModeTest.IsChecked = st.Mode == "test";
        ModeLock.IsChecked = st.Mode == "lock";

        (HeroTitle.Text, HeroSub.Text, HeroIcon.Foreground) = st.Mode switch
        {
            "lock" => ("Face Unlock is Active", "Workstation lock screen will unlock when your face is verified.", (Brush)FindResource("Good")),
            "test" => ("Test Prompt Mode", "Active only during Windows CredUI authentication prompts, not on lock screen.", (Brush)FindResource("Warn")),
            _ => ("Face Recognition Offline", "Complete the setup steps below to activate biometric authentication.", (Brush)FindResource("SubText")),
        };

        CardProfilesCount.Text = $"{st.Faces.Count} / 3 Enrolled";
        if (st.Faces.Count > 0)
        {
            CardProfilesCount.Foreground = (Brush)FindResource("Good");
        }

        AlertCard.Visibility = st.Mode == "lock" && (st.Locked.Length > 0 || st.CameraChanged.Length > 0) ? Visibility.Visible : Visibility.Collapsed;
        AlertTitle.Text = st.CameraChanged.Length > 0 ? "Webcam Hardware Changed" : "Face Recognition Suspended";
        AlertText.Text = st.CameraChanged.Length > 0
            ? $"\"{st.CameraChanged}\" was detected instead of your configured hardware camera."
            : st.Locked;

        bool hasFace = st.Faces.Count > 0, hasPw = st.Password && st.Linked;
        ModeLock.IsEnabled = ModeTest.IsEnabled = hasFace && hasPw;
        ModeHint.Visibility = ModeLock.IsEnabled ? Visibility.Collapsed : Visibility.Visible;
        ModeHint.Text = "Please enroll at least one face profile and store your Windows password first.";

        Steps.Children.Clear();
        AddStep("System & Hardware Ready for Facial Recognition", _systemOk, null, null);
        AddStep("Enroll your Face (5-angle 3D mesh capture)", hasFace, "Enroll Face", "faces");
        AddStep("Vault Windows Password in Hardware TPM 2.0", hasPw, "Store Password", "password");
        AddStep("Test Biometric Liveness Verification (non-destructive)", false, "Run Test", "test", optional: true);
        AddStep("Activate Windows Lock Screen Integration", st.Mode == "lock", null, null);
        SetupCard.Visibility = st.Mode == "lock" && hasFace && hasPw && _systemOk ? Visibility.Collapsed : Visibility.Visible;
    }

    void LoadRecentEvents()
    {
        RecentEventsList.Children.Clear();
        string logPath = Backend.LogPath;
        if (!File.Exists(logPath))
        {
            RecentEventsList.Children.Add(new TextBlock
            {
                Text = "No previous activity events recorded yet.",
                Style = (Style)FindResource("Sub"),
                Margin = new Thickness(0, 4, 0, 4)
            });
            return;
        }

        try
        {
            var lines = File.ReadLines(logPath).Reverse().Take(5).ToList();
            if (lines.Count == 0)
            {
                RecentEventsList.Children.Add(new TextBlock
                {
                    Text = "Application event log is currently empty.",
                    Style = (Style)FindResource("Sub")
                });
                return;
            }

            foreach (var line in lines)
            {
                var row = new DockPanel { Margin = new Thickness(0, 4, 0, 4) };
                var icon = new TextBlock
                {
                    Style = (Style)FindResource("Icon"),
                    Text = line.Contains("FAIL") ? "" : "",
                    Foreground = (Brush)FindResource(line.Contains("FAIL") ? "Bad" : "Accent"),
                    FontSize = 13,
                    Width = 24
                };
                DockPanel.SetDock(icon, Dock.Left);
                row.Children.Add(icon);

                var txt = new TextBlock
                {
                    Text = line,
                    Style = (Style)FindResource("Sub"),
                    FontFamily = (FontFamily)FindResource("Mono"),
                    FontSize = 11.5,
                    Foreground = (Brush)FindResource("Text")
                };
                row.Children.Add(txt);
                RecentEventsList.Children.Add(row);
            }
        }
        catch (Exception ex)
        {
            RecentEventsList.Children.Add(new TextBlock { Text = $"Could not load logs: {ex.Message}", Style = (Style)FindResource("Sub") });
        }
    }

    void AddStep(string text, bool done, string? action, string? page, bool optional = false)
    {
        var row = new DockPanel { Margin = new Thickness(0, 6, 0, 6) };
        var mark = new TextBlock
        {
            Style = (Style)FindResource("Icon"),
            Text = done ? "" : optional ? "" : "",
            Foreground = (Brush)FindResource(done ? "Good" : "Faint"),
            Width = 28,
        };
        DockPanel.SetDock(mark, Dock.Left);
        row.Children.Add(mark);
        if (action != null && !done)
        {
            var b = new Button { Content = action, Padding = new Thickness(12, 5, 12, 5), FontSize = 12.5 };
            b.Click += (_, _) => MainWindow.Instance?.Go(page!);
            DockPanel.SetDock(b, Dock.Right);
            row.Children.Add(b);
        }
        row.Children.Add(new TextBlock
        {
            Text = text + (optional ? "  (recommended)" : ""),
            Style = (Style)FindResource("Body"),
            Foreground = (Brush)FindResource(done ? "SubText" : "Text"),
            VerticalAlignment = VerticalAlignment.Center,
        });
        Steps.Children.Add(row);
    }

    async Task RunChecks()
    {
        Checks.Children.Clear();
        var sac = SystemInfo.SmartAppControl();
        AddCheck("Smart App Control (SAC)", sac, sac.Item1 != Check.Ok ? ("Open Windows Security", SystemInfo.OpenSmartAppControlSettings) : null);
        AddCheck("Windows Credential Provider V2", SystemInfo.Provider(), null);
        AddCheck("Local Neural Inference Models (MediaPipe & ArcFace)", SystemInfo.Models(), null);
        var tpmRow = AddCheck("Hardware TPM 2.0 Security Vault", (Check.Unknown, "Querying TPM silicon..."), null);
        var camRow = AddCheck("Integrated Webcam (HP Wide Vision HD)", (Check.Unknown, "Negotiating Media Foundation capture..."), null);

        var tpm = await SystemInfo.Tpm();
        SetCheck(tpmRow, tpm);
        CardTpmStatus.Text = tpm.Item1 == Check.Ok ? "TPM 2.0 Ready" : "TPM Unready";

        (Check, string) cam = (Check.Bad, "No camera detected");
        try
        {
            var e = await Backend.Run("cameras");
            if (e is { } j && j.TryGetProperty("list", out var list))
            {
                var usable = list.EnumerateArray().Where(c => c.Bool("usable")).Select(c => c.Str("name")).ToList();
                bool infraredOnly = usable.Count == 0 && list.EnumerateArray().Any(c => c.Bool("infrared"));
                cam = usable.Count > 0 ? (Check.Ok, string.Join(", ", usable))
                    : infraredOnly ? (Check.Bad, "Only an infrared sensor was detected; standard colour webcam required.")
                    : (Check.Bad, "Virtual camera refused - hardware webcam required.");

                if (usable.Count > 0)
                {
                    CardCameraName.Text = usable[0];
                }
            }
        }
        catch (Exception ex) { cam = (Check.Bad, ex.Message); }
        SetCheck(camRow, cam);
        _systemOk = new[] { sac.Item1, SystemInfo.Provider().Item1, SystemInfo.Models().Item1, tpm.Item1, cam.Item1 }.All(c => c != Check.Bad);
    }

    DockPanel AddCheck(string name, (Check, string) state, (string, Action)? fix)
    {
        var row = new DockPanel { Margin = new Thickness(0, 7, 0, 7) };
        var dot = new System.Windows.Shapes.Ellipse { Width = 9, Height = 9, Margin = new Thickness(2, 5, 14, 0), VerticalAlignment = VerticalAlignment.Top };
        DockPanel.SetDock(dot, Dock.Left);
        row.Children.Add(dot);
        if (fix is { } f)
        {
            var b = new Button { Content = f.Item1, Padding = new Thickness(12, 5, 12, 5), FontSize = 12.5, VerticalAlignment = VerticalAlignment.Center };
            b.Click += (_, _) => f.Item2();
            DockPanel.SetDock(b, Dock.Right);
            row.Children.Add(b);
        }
        var sp = new StackPanel();
        sp.Children.Add(new TextBlock { Text = name, Style = (Style)FindResource("Body") });
        sp.Children.Add(new TextBlock { Style = (Style)FindResource("Sub") });
        row.Children.Add(sp);
        Checks.Children.Add(row);
        SetCheck(row, state);
        return row;
    }

    void SetCheck(DockPanel row, (Check, string) state)
    {
        var dot = row.Children.OfType<System.Windows.Shapes.Ellipse>().First();
        dot.Fill = (Brush)FindResource(state.Item1 switch { Check.Ok => "Good", Check.Warn => "Warn", Check.Bad => "Bad", _ => "Faint" });
        ((TextBlock)row.Children.OfType<StackPanel>().First().Children[1]).Text = state.Item2;
    }

    void OnStartRecognition(object sender, RoutedEventArgs e) => MainWindow.Instance?.Go("test");
    void OnEnrollFace(object sender, RoutedEventArgs e) => MainWindow.Instance?.Go("faces");
    void OnManageProfiles(object sender, RoutedEventArgs e) => MainWindow.Instance?.Go("faces");
    void OnOpenSecurity(object sender, RoutedEventArgs e) => MainWindow.Instance?.Go("security");
    void OnOpenSettings(object sender, RoutedEventArgs e) => MainWindow.Instance?.Go("settings");
    void OnViewFullLogs(object sender, RoutedEventArgs e) => MainWindow.Instance?.Go("logs");

    async void OnMode(object sender, RoutedEventArgs e)
    {
        if (sender is not RadioButton { Tag: string mode }) return;
        if (mode == "lock" && SystemInfo.SmartAppControl().Item1 == Check.Bad)
        {
            MessageBox.Show("Smart App Control is currently ON and will block the Credential Provider.\n\nPlease set Smart App Control to Evaluation or Off in Windows Security, then retry.",
                            "Akhil Face ID", MessageBoxButton.OK, MessageBoxImage.Warning);
            ShowState();
            return;
        }
        var r = await Backend.Run("mode", mode);
        if (r is { } j && j.IsError()) MessageBox.Show(j.Str("msg"), "Akhil Face ID", MessageBoxButton.OK, MessageBoxImage.Error);
        _st = await Status.Load();
        ShowState();
        if (MainWindow.Instance != null) await MainWindow.Instance.RefreshMode();
    }

    async void OnRecheck(object sender, RoutedEventArgs e) => await Refresh();

    void OnGuide(object sender, RoutedEventArgs e) => MainWindow.Instance?.ShowSetup();
    void OnSecurity(object sender, RoutedEventArgs e) => MainWindow.Instance?.Go("security");

    async void OnOff(object sender, RoutedEventArgs e)
    {
        await Backend.Run("mode", "off");
        EraseText.Text = "Facial recognition is paused. Enrolled templates and TPM vault are preserved.";
        EraseText.Foreground = (Brush)FindResource("SubText");
        _st = await Status.Load();
        ShowState();
        if (MainWindow.Instance != null) await MainWindow.Instance.RefreshMode();
    }

    async void OnErase(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(
                "This permanently erases all biometric data from this PC:\n\n" +
                "  •  All enrolled face biometric templates\n" +
                "  •  All intruder audit photos and cryptographic keys\n" +
                "  •  The hardware TPM 2.0 credential container\n" +
                "  •  Application event logs and local preferences\n\n" +
                "Your standard Windows PIN and password will remain completely unaffected.\n\nErase all data?",
                "Erase Biometric Vault", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No) != MessageBoxResult.Yes) return;
        EraseText.Text = "Erasing biometric vault and zeroing keys...";
        EraseText.Foreground = (Brush)FindResource("SubText");
        var r = await Backend.Run("erase");
        AppState.Clear();
        if (r is { } j && j.Str("t") == "done")
        {
            int left = (int)j.Num("tpm_keys_left");
            EraseText.Text = left == 0 ? "✓  All biometric data, logs, and TPM keys were completely erased."
                                       : $"Data erased; {left} TPM key(s) cleared.";
            EraseText.Foreground = (Brush)FindResource(left == 0 ? "Good" : "Warn");
        }
        else
        {
            EraseText.Text = r is { } er && er.IsError() ? er.Str("msg") : "Erase operation failed.";
            EraseText.Foreground = (Brush)FindResource("Bad");
        }
        _st = await Status.Load();
        ShowState();
        if (MainWindow.Instance != null) await MainWindow.Instance.RefreshMode();
    }
}
