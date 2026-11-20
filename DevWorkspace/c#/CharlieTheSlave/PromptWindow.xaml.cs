using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

namespace CharlieTheSlave;

public sealed class ChatLine : INotifyPropertyChanged
{
    static readonly Brush UserBrush = Frozen("#3B5BDB");
    static readonly Brush BotBrush = Frozen("#2A2C36");

    string _text;

    public ChatLine(string text, bool fromUser)
    {
        _text = text;
        FromUser = fromUser;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool FromUser { get; }

    /// <summary>Mutable so a streamed LLM reply can grow in place.</summary>
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text)));
        }
    }

    public Brush Background => FromUser ? UserBrush : BotBrush;
    public HorizontalAlignment Align => FromUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;

    static Brush Frozen(string hex)
    {
        var b = (Brush)new BrushConverter().ConvertFromString(hex)!;
        b.Freeze();
        return b;
    }
}

public partial class PromptWindow : Window
{
    /// <summary>The system-wide key that stops a run; shown in the messages that mention it.</summary>
    public const string StopHotkeyName = "Ctrl+Alt+Q";

    readonly ObservableCollection<ChatLine> _lines = new();
    CancellationTokenSource? _cts;
    CancellationTokenSource? _chatCts;
    GlobalHotkey? _stopHotkey;
    readonly OllamaClient _ollama;

    public event Action<bool>? BusyChanged;

    bool IsBusy => _cts != null;

    public PromptWindow()
    {
        InitializeComponent();
        Messages.ItemsSource = _lines;

        var s = AppSettings.Current;
        _ollama = new OllamaClient(s.OllamaUrl, s.OllamaModel, systemPrompt: "");
        _actions = BuildActions();
        LoadCommands(announce: false);
        WatchCommandsFile();

        Say("Hey boss, what do you wanna do?");
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        // The POI export keeps TrueTest in front, so this window can't be typed into while it runs.
        _stopHotkey = GlobalHotkey.CtrlAlt(this, Key.Q, () =>
        {
            if (_cts == null && _chatCts == null) return;
            Say($"{StopHotkeyName} — stopping…");
            Stop();
        });
        if (_stopHotkey is { IsRegistered: false })
            Say($"⚠ Another program already owns {StopHotkeyName}, so that shortcut won't stop me. Typing \"stop\" still works.");
    }

    protected override void OnClosed(EventArgs e)
    {
        _stopHotkey?.Dispose();
        _stopHotkey = null;
        base.OnClosed(e);
    }

    public void ShowNear(Window anchor)
    {
        var wa = SystemParameters.WorkArea;
        double left = anchor.Left + anchor.Width - Width;
        double top = anchor.Top - Height;
        if (top < wa.Top) top = anchor.Top + anchor.Height;
        Left = Clamp(left, SystemParameters.VirtualScreenLeft,
            SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - Width);
        Top = top;

        Show();
        Activate();
        Input.Focus();
    }

    /// <summary>Continues an install after we restarted ourselves as administrator.</summary>
    public void ResumeInstall(string installerPath)
    {
        Say("Running as administrator now.");
        _ = RunInstallerAsync(installerPath);
    }

    async Task HandleAsync(string text)
    {
        if (await HandleAnswerAsync(text)) return;
        if (await HandlePendingAnswerAsync(text)) return;
        if (await RunCommandLineAsync(text, confirmed: false, depth: 0)) return;
        await AskOllamaAsync(text);
    }

    /// <summary>When Ollama last refused a connection, so we don't wait on it for every message after.</summary>
    DateTime? _ollamaOfflineAt;

    static readonly TimeSpan OllamaRetryAfter = TimeSpan.FromMinutes(1);

    /// <summary>What to say about input that isn't a command when there's no model to pass it to.</summary>
    void SayUnknown(string text) =>
        Say($"I don't know \"{text.Trim()}\". Type \"help\" to see what I can do.");

    async Task AskOllamaAsync(string text)
    {
        // Without Ollama there's nothing to pass this to, so answer as a plain unknown command instead of
        // waiting for the connection to fail again on every message.
        if (_ollamaOfflineAt is { } since && DateTime.UtcNow - since < OllamaRetryAfter)
        {
            SayUnknown(text);
            return;
        }

        _chatCts?.Cancel(); // a new question replaces an answer still streaming
        var cts = _chatCts = new CancellationTokenSource();
        var line = Say("…");
        bool first = true;
        try
        {
            await _ollama.ChatAsync(text, token =>
            {
                // ChatAsync's awaits resume on the UI thread, so it's safe to touch the UI here.
                line.Text = first ? token.TrimStart() : line.Text + token;
                first = false;
                Scroller.ScrollToEnd();
            }, cts.Token);
            if (first) line.Text = "(no answer)";
            _ollamaOfflineAt = null;
        }
        catch (OperationCanceledException)
        {
            line.Text = first ? "(stopped)" : line.Text + " …(stopped)";
        }
        catch (HttpRequestException)
        {
            // No model to fall back on: treat it as an unknown command, and say why only the first time.
            bool firstTime = _ollamaOfflineAt == null;
            _ollamaOfflineAt = DateTime.UtcNow;
            line.Text = $"I don't know \"{text.Trim()}\". Type \"help\" to see what I can do." +
                        (firstTime
                            ? $"\n(Anything that isn't a command normally goes to Ollama, but I can't reach it at {_ollama.BaseUrl}.)"
                            : "");
        }
        catch (Exception ex)
        {
            line.Text = $"Ollama error: {ex.Message}";
        }
        finally
        {
            if (_chatCts == cts) _chatCts = null;
            cts.Dispose();
        }
    }

    void ClearChat()
    {
        _chatCts?.Cancel();
        _ollama.ClearHistory();
        _lines.Clear();
        Say("Fresh start. What can I do for you?");
    }

    void SetModel(string? name)
    {
        if (name != null)
        {
            _ollama.Model = name;
            AppSettings.Current.OllamaModel = name;
            AppSettings.Current.Save();
            Say($"OK, I'll answer with {name} from now on.");
        }
        else
        {
            Say($"I'm using {_ollama.Model} on {_ollama.BaseUrl}.");
        }
    }

    void ShowComponents()
    {
        var list = AppSettings.Current.TrueTestComponents;
        Say(list == null
            ? "I'll leave TrueTest's components as the installer suggests."
            : list.Count == 0
                ? "I'll untick every optional TrueTest component."
                : $"I'll keep only these TrueTest components ticked: {string.Join(", ", list)}.");
    }

    async Task UpdateTrueTestAsync(string? version, string? components)
    {
        var s = AppSettings.Current;
        if (components != null)
        {
            var spec = components.Trim().TrimEnd('.');
            s.TrueTestComponents = Regex.IsMatch(spec, @"^(the\s+)?(default|all|installer'?s?)\b", RegexOptions.IgnoreCase) ? null
                : Regex.IsMatch(spec, @"^(no|nothing|none)\b", RegexOptions.IgnoreCase) ? new List<string>()
                : spec.Split(new[] { ',', ';', '+' }, StringSplitOptions.RemoveEmptyEntries)
                      .SelectMany(part => part.Split(new[] { " and " }, StringSplitOptions.RemoveEmptyEntries))
                      .Select(part => part.Trim()).Where(part => part.Length > 0)
                      .ToList();
            s.Save();
        }
        ShowComponents();

        string? installer;
        if (version != null)
        {
            var found = FindInstallers(version);
            if (found.Count == 0)
            {
                // FindInstallers has already said why; let the user point at the file instead of giving up.
                Say("Pick the installer yourself and I'll run it.");
                installer = PickInstaller(null);
            }
            else if (found.Count == 1)
            {
                installer = found[0].Path;
                Say($"Found {Path.GetFileName(installer)}.");
            }
            else
            {
                Say($"Several different installers match {version}:\n" +
                    string.Join("\n", found.Select(f => "• " + Path.GetFileName(f.Path))) + "\nPlease pick one.");
                installer = PickInstaller(Path.GetDirectoryName(found[0].Path));
            }
        }
        else
        {
            installer = PickInstaller(null);
        }
        if (installer == null)
        {
            Say("OK, cancelled.");
            return;
        }

        s.LastInstallerFolder = Path.GetDirectoryName(installer);
        s.Save();
        await RunInstallerAsync(installer);
    }

    /// <summary>
    /// Opens the folder the TrueTest installers live in, asking for it when there isn't one — the folder
    /// in commands.json is only a guess, and on a fresh machine it usually doesn't exist.
    /// </summary>
    void OpenInstallerFolder()
    {
        var folder = InstallerFolders().FirstOrDefault();
        if (folder == null)
        {
            var guess = Environment.ExpandEnvironmentVariables(_config.InstallerFolder ?? "");
            Say(guess.Length > 0
                ? $"There's no {guess} on this machine. Where do you keep the TrueTest installers?"
                : "I don't know where you keep the TrueTest installers. Where are they?");

            // WPF's OpenFolderDialog is .NET 8 only; FolderPicker shows the same Windows dialog.
            var picked = FolderPicker.Show(this, "Where are the TrueTest installers?");
            if (picked == null)
            {
                Say("OK, cancelled.");
                return;
            }
            folder = picked;
            AppSettings.Current.LastInstallerFolder = folder;
            AppSettings.Current.Save();
            Say("Thanks — I'll remember that.");
        }

        try
        {
            Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
            Say($"Opened {folder}");
        }
        catch (Exception ex)
        {
            Say($"❌ Couldn't open {folder}: {ex.Message}");
        }
    }

    string? PickInstaller(string? folder)
    {
        var s = AppSettings.Current;
        var dlg = new OpenFileDialog
        {
            Title = "Choose the TrueTest installer",
            Filter = "TrueTest installer|TrueTest_Installer*.exe|Programs (*.exe)|*.exe",
            InitialDirectory = folder ?? InstallerFolders().FirstOrDefault() ?? "",
        };
        return dlg.ShowDialog(this) == true ? dlg.FileName : null;
    }

    // Expanded, so commands.json can say "%USERPROFILE%\Downloads\Programs" and work on any machine.
    IEnumerable<string> InstallerFolders() =>
        new[] { AppSettings.Current.LastInstallerFolder, _config.InstallerFolder }
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Select(d => Environment.ExpandEnvironmentVariables(d!))
            .Where(Directory.Exists).Distinct(StringComparer.OrdinalIgnoreCase);

    static readonly Regex InstallerName = new(@"^TrueTest_Installer_(?<v>\d+(?:\.\d+){1,3})", RegexOptions.IgnoreCase);

    /// <summary>
    /// Finds installers whose version matches <paramref name="wanted"/>: "158" matches the last part
    /// (1.9.226.158), a dotted value matches the end of the version, "latest" takes the highest.
    /// Byte-identical copies (e.g. "…_2.exe") count once. Says what's available when nothing matches.
    /// </summary>
    List<(string Path, Version Version)> FindInstallers(string wanted)
    {
        var all = InstallerFolders()
            .SelectMany(d => Directory.EnumerateFiles(d, "TrueTest_Installer_*.exe"))
            .Select(p => (Path: p, M: InstallerName.Match(Path.GetFileName(p))))
            .Where(x => x.M.Success && Version.TryParse(x.M.Groups["v"].Value, out _))
            .Select(x => (x.Path, Version: Version.Parse(x.M.Groups["v"].Value)))
            .ToList();

        if (all.Count == 0)
        {
            Say($"I couldn't find any TrueTest installers in {string.Join(" or ", InstallerFolders())}.");
            return new();
        }

        List<(string Path, Version Version)> matches;
        if (wanted is "latest" or "newest")
        {
            var top = all.Max(x => x.Version)!;
            matches = all.Where(x => x.Version == top).ToList();
        }
        else
        {
            matches = all.Where(x =>
            {
                var v = x.Version.ToString();
                return v == wanted || v.EndsWith("." + wanted);
            }).ToList();
        }

        if (matches.Count == 0)
        {
            var available = all.Select(x => x.Version).Distinct().OrderBy(v => v).Select(v => v.ToString());
            Say($"There's no TrueTest {wanted} installer. Available versions: {string.Join(", ", available)}.");
            return matches;
        }

        // Drop copies of the same file (same version and size), preferring the shortest name.
        return matches
            .GroupBy(x => (x.Version, new FileInfo(x.Path).Length))
            .Select(g => g.OrderBy(x => x.Path.Length).First())
            .ToList();
    }

    async Task RunInstallerAsync(string path)
    {
        if (!File.Exists(path))
        {
            Say($"Can't find {path}.");
            return;
        }

        // The installer runs elevated, and Windows only lets an elevated process click its buttons.
        if (!IsAdministrator())
        {
            Say("I need administrator rights to click through the installer. Restarting as admin…");
            try
            {
                Process.Start(new ProcessStartInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName)
                {
                    UseShellExecute = true,
                    Verb = "runas",
                    Arguments = $"--install \"{path}\"",
                });
                Application.Current.Shutdown();
            }
            catch (Win32Exception)
            {
                Say("Admin rights were declined, so I can't run the installer for you.");
            }
            return;
        }

        Say($"Installing {Path.GetFileName(path)}. Hands off the installer window while I click through it.");
        _cts = new CancellationTokenSource();
        BusyChanged?.Invoke(true);
        try
        {
            var automation = new InstallerAutomation(new Progress<string>(text => Say(text)), AppSettings.Current.TrueTestComponents);
            int? exitCode = await Task.Run(() => automation.RunAsync(path, _cts.Token));

            var version = InstalledVersion("TrueTest");
            var versionText = version != null ? $" Installed version: {version}." : "";
            if (exitCode is null or 0)
                Say($"✅ Done.{versionText}");
            else
                Say($"⚠ Installer finished with exit code {exitCode}.{versionText}");
        }
        catch (OperationCanceledException)
        {
            Say("Stopped auto-clicking. The installer is still open if you want to finish it yourself.");
        }
        catch (Exception ex)
        {
            Say($"❌ {ex.Message}");
        }
        finally
        {
            _cts.Dispose();
            _cts = null;
            BusyChanged?.Invoke(false);
        }
    }

    void Stop()
    {
        if (_cts == null && _chatCts == null) Say("Nothing is running.");
        _cts?.Cancel();
        _chatCts?.Cancel();
    }

    static bool IsAdministrator() =>
        new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    static string? InstalledVersion(string productName)
    {
        foreach (var view in new[] { RegistryView.Registry64, RegistryView.Registry32 })
        {
            using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, view);
            using var uninstall = hklm.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            if (uninstall == null) continue;
            foreach (var name in uninstall.GetSubKeyNames())
            {
                using var key = uninstall.OpenSubKey(name);
                if (key?.GetValue("DisplayName") is string dn && dn.IndexOf(productName, StringComparison.OrdinalIgnoreCase) >= 0)
                    return key.GetValue("DisplayVersion") as string;
            }
        }
        return null;
    }

    ChatLine Say(string text) => AddLine(text, fromUser: false);

    ChatLine AddLine(string text, bool fromUser)
    {
        var line = new ChatLine(text, fromUser);
        _lines.Add(line);
        Scroller.ScrollToEnd();
        return line;
    }

    // Everything typed this session, oldest first, and where Up/Down currently are in it.
    // _historyAt == _history.Count means "not browsing"; the box holds whatever is being typed.
    readonly List<string> _history = new();
    int _historyAt;

    /// <summary>
    /// Up and Down have to be caught on the way down: a TextBox treats them as its own and marks them
    /// handled, so they never reach the bubbling KeyDown.
    /// </summary>
    void Input_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Up or Key.Down)) return;
        e.Handled = true;
        Recall(e.Key == Key.Up ? -1 : 1);
    }

    async void Input_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        var text = Input.Text.Trim();
        if (text.Length == 0) return;

        if (_history.Count == 0 || _history[_history.Count - 1] != text) _history.Add(text);
        _historyAt = _history.Count;

        Input.Clear();
        AddLine(text, fromUser: true);
        await HandleAsync(text);
    }

    // Math.Clamp is .NET Core only.
    static T Clamp<T>(T value, T min, T max) where T : IComparable<T> =>
        value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;

    /// <summary>Walks the typed history like a console does; past the newest entry the box goes empty.</summary>
    void Recall(int step)
    {
        if (_history.Count == 0) return;
        _historyAt = Clamp(_historyAt + step, 0, _history.Count);
        Input.Text = _historyAt < _history.Count ? _history[_historyAt] : "";
        Input.CaretIndex = Input.Text.Length;
    }

    void Input_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
        Placeholder.Visibility = Input.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;

    void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Hide();
    }

    void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    void Hide_Click(object sender, RoutedEventArgs e) => Hide();
}
