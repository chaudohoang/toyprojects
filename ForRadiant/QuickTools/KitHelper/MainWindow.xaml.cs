using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace KitHelper
{
    // The panel the floating icon shows and hides. Hiding never closes it: state (folder, log,
    // selections) survives until the app exits from the icon's menu.
    public partial class MainWindow : Window
    {
        public event Action<bool> BusyChanged;
        public Window Anchor { get; set; }

        readonly Dictionary<CopyTarget, Button> copyButtons = new Dictionary<CopyTarget, Button>();
        readonly List<UIElement> actionControls = new List<UIElement>();
        readonly ObservableCollection<LogLine> log = new ObservableCollection<LogLine>();
        readonly Settings settings = Settings.Current;
        readonly string ownExe = Path.GetFileName(InstalledCopy.SelfExe);
        Process runningTest;
        string appliedSource;

        public MainWindow(string startFolder)
        {
            InitializeComponent();
            TxtVersion.Text = InstalledCopy.Self + (IsElevated() ? "  ·  Administrator" : "");
            BtnAdmin.Visibility = IsElevated() ? Visibility.Collapsed : Visibility.Visible;
            TxtVignettingTitle.Text = "Vignetting  (" + Vignetting.Key + " in kit *.ini)";
            TxtExplorerHint.Text = "Added automatically whenever Kit Helper starts, unless you remove it. Runs the copy in "
                                 + InstalledCopy.Folder + ". Windows 11: under \"Show more options\".";
            LstLog.ItemsSource = log;

            foreach (var t in CopyTarget.All)
            {
                var target = t;
                var b = new Button
                {
                    Content = "Copy all → " + string.Join(" + ", t.SubFolders) + (t.IncludeSubdirs ? "  (with subfolders)" : ""),
                    Style = (Style)FindResource("ActionButton"),
                };
                b.Click += (s, e) => DoCopy(target);
                copyButtons[t] = b;
                actionControls.Add(b);
                CopyButtons.Children.Add(b);
            }
            actionControls.AddRange(new UIElement[] { BtnUnblock, BtnVigOn, BtnVigOff, BtnRun });

            // Folder: command-line arg (another copy passes the folder it sits in) > the most
            // recent folder when this is the installed copy > this exe's own folder.
            string recent = settings.Get("Recent").Split('|').FirstOrDefault(Directory.Exists);
            string folder = !string.IsNullOrEmpty(startFolder) && Directory.Exists(startFolder) ? startFolder
                : InstalledCopy.IsStatic(InstalledCopy.SelfExe) && recent != null ? recent
                : AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            ChkCloseTT.IsChecked = settings.Get("CloseTrueTest") != "0";
            LoadInstalls();
            SetSource(folder);
            UpdateExplorerButton();

            Drop += OnDrop;
            KeyDown += (s, e) => { if (e.Key == Key.Escape) HidePanel(); };
            Closing += (s, e) => { if (!Program.Exiting) { e.Cancel = true; HidePanel(); } };
            Deactivated += (s, e) => AutoHide();
            Log("Kit Helper ready. Drop a kit folder on the icon or here, or a .txt to use as the run-test input.", LogLevel.Info);
        }

        // ---------------------------------------------------------------- show / hide

        public void ShowNear(Window anchor)
        {
            anchor = anchor ?? Anchor;
            var wa = SystemParameters.WorkArea;
            if (Height > wa.Height) Height = wa.Height;
            if (anchor != null)
            {
                // Above the icon, right edges aligned; below it if there is no room above.
                double left = anchor.Left + anchor.Width - Width;
                double top = anchor.Top - Height;
                if (top < SystemParameters.VirtualScreenTop) top = anchor.Top + anchor.Height;
                Left = Clamp(left, SystemParameters.VirtualScreenLeft,
                    SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - Width);
                Top = Clamp(top, SystemParameters.VirtualScreenTop,
                    SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - Height);
            }
            Show();
            if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
            Activate();
        }

        static double Clamp(double v, double min, double max) => v < min ? min : v > max ? max : v;

        // Clicking anywhere else hides the panel, like a menu. Two exceptions:
        //  - the click is on the floating icon: the icon's own click handler toggles, so hiding
        //    here would make that click re-open it;
        //  - one of the panel's own dialogs took focus (see Modal).
        int modalDepth;

        void AutoHide()
        {
            if (!IsVisible || modalDepth > 0 || Program.Exiting || CursorOver(Anchor)) return;
            HidePanel();
        }

        // Runs a dialog with auto-hide suspended: opening it deactivates the panel.
        T Modal<T>(Func<T> dialog)
        {
            modalDepth++;
            try { return dialog(); }
            finally { modalDepth--; }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT p);
        struct POINT { public int X, Y; }

        static bool CursorOver(Window w)
        {
            if (w == null || !w.IsVisible || !GetCursorPos(out POINT p)) return false;
            var local = w.PointFromScreen(new Point(p.X, p.Y));     // screen pixels → window DIPs
            return local.X >= 0 && local.Y >= 0 && local.X <= w.ActualWidth && local.Y <= w.ActualHeight;
        }

        public void Toggle()
        {
            if (IsVisible) HidePanel();
            else ShowNear(Anchor);
        }

        void HidePanel()
        {
            SaveSettings();
            Hide();
        }

        // A folder opened with Kit Helper (Explorer right-click, another copy, a drop on the icon).
        public void OpenFolder(string folder)
        {
            ShowNear(Anchor);
            Log($"Opened {folder}", LogLevel.Info);
            LoadInstalls();                             // TrueTest may have started or stopped since
            SetSource(folder.TrimEnd('\\'));
        }

        // From a second launch (see IpcHost). Empty = a plain relaunch: just show the panel.
        public void OpenForwarded(string folder)
        {
            if (folder == "") ShowNear(Anchor);
            else OpenFolder(folder);
        }

        public void OpenInExplorer()
        {
            if (Directory.Exists(Source)) Process.Start("explorer.exe", "\"" + Source + "\"");
        }

        void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        void Hide_Click(object sender, RoutedEventArgs e) => HidePanel();

        // ---------------------------------------------------------------- state

        string Source => CboSource.Text.Trim().TrimEnd('\\');
        string Install => CboInstall.SelectedItem as string;

        void SetSource(string folder)
        {
            appliedSource = folder;
            CboSource.Text = folder;
            if (!Directory.Exists(folder))
            {
                TxtSourceInfo.Text = "Folder not found.";
                TxtSourceInfo.Foreground = Brush("Err");
                LvIni.ItemsSource = null;
                return;
            }
            int files = Directory.GetFiles(folder).Count(f => !Kit.IsHelper(f) && !string.Equals(Path.GetFileName(f), ownExe, StringComparison.OrdinalIgnoreCase));
            int dirs = Directory.GetDirectories(folder).Length;
            int inis = Vignetting.IniFiles(folder).Length;
            TxtSourceInfo.Text = $"{files} file(s), {dirs} subfolder(s), {inis} ini";
            TxtSourceInfo.Foreground = Brush("Dim");
            RefreshIni();
            RememberRecent(folder);

            // Mark the copy target this folder is meant for.
            var suggested = Kit.Suggest(folder, out string why);
            foreach (var kv in copyButtons) kv.Value.Tag = kv.Key == suggested ? "suggested" : null;
            TxtSuggest.Text = suggested == null ? "" : $"Suggested: {suggested.Name}  ({why})";
            TxtSuggest.Visibility = suggested == null ? Visibility.Collapsed : Visibility.Visible;

            // Run inputs found in this folder; keep a browsed file from elsewhere if one is set.
            string current = RunInputPath;
            CboRunInput.Items.Clear();
            foreach (string f in Kit.RunInputs(folder)) CboRunInput.Items.Add(new RunInputItem(f));
            var match = CboRunInput.Items.Cast<RunInputItem>().FirstOrDefault(i => string.Equals(i.Path, current, StringComparison.OrdinalIgnoreCase));
            if (match != null) CboRunInput.SelectedItem = match;
            else if (CboRunInput.Items.Count > 0) CboRunInput.SelectedIndex = 0;
            else if (!File.Exists(current)) CboRunInput.Text = "";
            OnRunInputChanged();
        }

        void RememberRecent(string folder)
        {
            var recent = settings.Get("Recent").Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(f => !string.Equals(f, folder, StringComparison.OrdinalIgnoreCase) && Directory.Exists(f))
                .Prepend(folder).Take(15).ToList();
            settings.Set("Recent", string.Join("|", recent));
            CboSource.Items.Clear();
            foreach (var f in recent) CboSource.Items.Add(f);
            CboSource.Text = folder;
        }

        void CboSource_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { SetSource(CboSource.Text.Trim()); e.Handled = true; }
        }

        void CboSource_LostFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // Focus also moves into the combo's own drop-down; only a real leave counts.
            if (CboSource.IsKeyboardFocusWithin || CboSource.IsDropDownOpen) return;
            if (!string.Equals(CboSource.Text.Trim(), appliedSource, StringComparison.OrdinalIgnoreCase))
                SetSource(CboSource.Text.Trim());
        }

        void CboSource_DropDownClosed(object sender, EventArgs e)
        {
            if (CboSource.SelectedItem is string f && !string.Equals(f, appliedSource, StringComparison.OrdinalIgnoreCase))
                Dispatcher.BeginInvoke(new Action(() => SetSource(f)));
        }

        // A run input in the drop-down shows its file name; a typed or browsed path is used as is.
        sealed class RunInputItem
        {
            public readonly string Path;
            public RunInputItem(string path) { Path = path; }
            public override string ToString() => System.IO.Path.GetFileName(Path);
        }

        string RunInputPath
        {
            get
            {
                if (CboRunInput.SelectedItem is RunInputItem i && CboRunInput.Text == i.ToString()) return i.Path;
                string t = CboRunInput.Text.Trim();
                return t == "" || Path.IsPathRooted(t) ? t : Path.Combine(Source, t);
            }
        }

        string lastRunInput;
        void OnRunInputChanged()
        {
            string p = RunInputPath;
            if (p == lastRunInput) return;
            lastRunInput = p;
            if (File.Exists(p)) RbPuc.IsChecked = RunTest.LooksLikePuc(p);
            RbOmit.IsChecked = RbPuc.IsChecked != true;
            UpdateRunMode();
        }

        // SelectionChanged fires before the editable text follows the new item: read it after.
        void CboRunInput_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Dispatcher.BeginInvoke(new Action(OnRunInputChanged), DispatcherPriority.Background);
        void CboRunInput_LostFocus(object sender, KeyboardFocusChangedEventArgs e) => OnRunInputChanged();
        void RunMode_Changed(object sender, RoutedEventArgs e) => UpdateRunMode();

        // PUC runs where its input lives (RunTestPUC.exe had no working-dir choice: it ran in the
        // folder it sat in). Omit runs in the chosen ScratchDustDetect_NY folder.
        void UpdateRunMode()
        {
            if (RbPuc.IsChecked == true)
            {
                CboWorkDir.IsEnabled = false;
                CboWorkDir.Items.Clear();
                string p = RunInputPath;
                CboWorkDir.Text = File.Exists(p) ? Path.GetDirectoryName(p) : Source;
            }
            else if (!CboWorkDir.IsEnabled)
            {
                CboWorkDir.IsEnabled = true;
                CboWorkDir.Text = "";
                LoadWorkDirs();
            }
        }

        sealed class IniRow
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public Brush Brush { get; set; }
        }

        void RefreshIni()
        {
            var rows = new List<IniRow>();
            foreach (string f in Vignetting.IniFiles(Source))
            {
                string v;
                try { v = Vignetting.Read(f); } catch (Exception ex) { v = "error: " + ex.Message; }
                if (v == null) continue;     // ini without the key: not a vignetting ini
                rows.Add(new IniRow { Name = Path.GetFileName(f), Value = v, Brush = Vignetting.IsDx(f) ? Brush("Dim") : Brush("Fg") });
            }
            if (rows.Count == 0) rows.Add(new IniRow { Name = "(no ini with " + Vignetting.Key + ")", Value = "", Brush = Brush("Dim") });
            LvIni.ItemsSource = rows;
        }

        void LoadInstalls()
        {
            string keep = Install ?? settings.Get("Install");
            var installs = TrueTest.Installs();
            CboInstall.Items.Clear();
            foreach (var i in installs) CboInstall.Items.Add(i);

            // Prefer the install TrueTest is currently running from, then the last one used.
            var p = TrueTest.FindRunning();
            string runningPath = p == null ? null : TrueTest.PathOf(p);
            string runningInstall = runningPath == null ? null
                : installs.FirstOrDefault(i => runningPath.StartsWith(i + "\\", StringComparison.OrdinalIgnoreCase));
            TxtRunning.Text = p == null ? "TrueTest is not running."
                : "TrueTest is running" + (runningPath != null ? ": " + runningPath : " (path hidden - not elevated)");
            TxtRunning.Foreground = p == null ? Brush("Dim") : Brush("Warn");

            string pick = runningInstall ?? installs.FirstOrDefault(i => string.Equals(i, keep, StringComparison.OrdinalIgnoreCase));
            if (pick != null) CboInstall.SelectedItem = pick;
            else if (CboInstall.Items.Count > 0) CboInstall.SelectedIndex = CboInstall.Items.Count - 1;

            if (installs.Count == 0) Log($"No TrueTest install found under {TrueTest.Root}. Copy is unavailable.", LogLevel.Warn);
            LoadWorkDirs();
        }

        void CboInstall_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadWorkDirs();

        void LoadWorkDirs()
        {
            if (RbPuc.IsChecked == true) return;      // PUC's working dir is fixed (see UpdateRunMode)
            string keep = CboWorkDir.Text;
            if (string.IsNullOrEmpty(keep)) keep = settings.Get("RunWorkDir");
            CboWorkDir.Items.Clear();
            foreach (var i in TrueTest.Installs()) CboWorkDir.Items.Add(Path.Combine(i, "ScratchDustDetect_NY"));
            CboWorkDir.Items.Add(RunTest.CustomWorkDir);
            // Free text is allowed, so keep whatever was there even if it is not in the list.
            CboWorkDir.Text = !string.IsNullOrEmpty(keep) ? keep
                : Install != null ? Path.Combine(Install, "ScratchDustDetect_NY") : RunTest.CustomWorkDir;
        }

        public void SaveSettings()
        {
            settings.Set("Install", Install);
            if (RbPuc.IsChecked != true) settings.Set("RunWorkDir", CboWorkDir.Text);
            settings.Set("CloseTrueTest", ChkCloseTT.IsChecked == true ? "1" : "0");
            settings.Save();
        }

        // ---------------------------------------------------------------- actions

        async void DoCopy(CopyTarget target)
        {
            if (!CheckSource()) return;
            if (Install == null) { Log("Pick a TrueTest install first.", LogLevel.Error); return; }
            string install = Install, source = Source;

            string restartPath = null;
            if (ChkCloseTT.IsChecked == true)
            {
                var p = TrueTest.FindRunning();
                if (p != null)
                {
                    string path = TrueTest.PathOf(p);
                    var answer = Modal(() => MessageBox.Show(this,
                        $"TrueTest is running{(path != null ? ":\n" + path : ".")}\n\nClose it before copying to\n{install}?",
                        "Copy to " + target.Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Question));
                    if (answer == MessageBoxResult.Cancel) return;
                    if (answer == MessageBoxResult.Yes)
                    {
                        Log("Closing TrueTest…", LogLevel.Info);
                        try { p.Kill(); p.WaitForExit(10000); restartPath = path; Log("TrueTest closed.", LogLevel.Ok); }
                        catch (Exception ex) { Log("Could not close TrueTest: " + ex.Message, LogLevel.Error); }
                    }
                }
            }

            Log($"Copy {source} → {install} [{string.Join(", ", target.SubFolders)}]", LogLevel.Info);
            var r = await Busy(() => Copier.Run(source, install, target, ownExe, Log));
            if (r != null)
            {
                Log($"Done: {r.Copied} copied, {r.Failed} failed, {r.Skipped} skipped.", r.Failed == 0 ? LogLevel.Ok : LogLevel.Error);
                if (r.AccessDenied && !IsElevated())
                    Log("Access denied writing to Program Files - use \"Run as admin\".", LogLevel.Warn);
            }

            if (restartPath != null &&
                Modal(() => MessageBox.Show(this, "Copy finished. Restart TrueTest?\n\n" + restartPath, "TrueTest",
                                MessageBoxButton.YesNo, MessageBoxImage.Question)) == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(restartPath) { WorkingDirectory = Path.GetDirectoryName(restartPath), UseShellExecute = true });
                    Log("TrueTest restarted.", LogLevel.Ok);
                }
                catch (Exception ex) { Log("Restart failed: " + ex.Message, LogLevel.Error); }
            }
            LoadInstalls();
        }

        void VigOn_Click(object sender, RoutedEventArgs e) => DoVignetting("1");
        void VigOff_Click(object sender, RoutedEventArgs e) => DoVignetting("0");

        async void DoVignetting(string value)
        {
            if (!CheckSource()) return;
            bool dx = ChkDx.IsChecked == true;
            string source = Source;
            Log($"Set {Vignetting.Key} = {value}{(dx ? " (including DX)" : "")}", LogLevel.Info);
            await Busy(() =>
            {
                int changed = 0;
                foreach (string f in Vignetting.IniFiles(source))
                {
                    string name = Path.GetFileName(f);
                    try
                    {
                        if (Vignetting.IsDx(f) && !dx)
                        {
                            string cur = Vignetting.Read(f);
                            if (cur != null) Log($"  unchanged  {name} ({cur}) - DX", LogLevel.Detail);
                            continue;
                        }
                        string old = Vignetting.Set(f, value);
                        if (old == null) continue;
                        if (old == value) Log($"  already    {name} ({old})", LogLevel.Detail);
                        else { Log($"  updated    {name} ({old} → {value})", LogLevel.Detail); changed++; }
                    }
                    catch (Exception ex) { Log($"  FAILED     {name}: {ex.Message}", LogLevel.Error); }
                }
                Log($"Done: {changed} file(s) changed.", LogLevel.Ok);
                return 0;
            });
            RefreshIni();
        }

        async void Unblock_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSource()) return;
            string source = Source;
            Log($"Unblock {source}", LogLevel.Info);
            var r = await Busy(() => Unblocker.Run(source, Log));
            Log($"Done: {r.unblocked} unblocked of {r.total} file(s){(r.failed > 0 ? $", {r.failed} failed" : "")}.",
                r.failed == 0 ? LogLevel.Ok : LogLevel.Error);
        }

        async void Run_Click(object sender, RoutedEventArgs e)
        {
            string input = RunInputPath, workDir = CboWorkDir.Text.Trim();
            bool puc = RbPuc.IsChecked == true;
            if (!File.Exists(input)) { Log("Run test: input file not found - " + input, LogLevel.Error); return; }
            if (!Directory.Exists(workDir)) { Log("Run test: working dir not found - " + workDir, LogLevel.Error); return; }
            SaveSettings();

            BtnStop.IsEnabled = true;
            int code = await Busy(() =>
            {
                var psi = RunTest.Parse(input, workDir, puc, Log);
                Log($"Run test ({(puc ? "PUC" : "Omit")}) {Path.GetFileName(input)} in {workDir}", LogLevel.Info);
                Log($"> {psi.FileName} {psi.Arguments}", LogLevel.Info);
                using (var p = new Process { StartInfo = psi })
                {
                    p.OutputDataReceived += (s, ev) => { if (ev.Data != null) Log(ev.Data, LogLevel.Detail); };
                    p.ErrorDataReceived += (s, ev) => { if (ev.Data != null) Log(ev.Data, LogLevel.Warn); };
                    p.Start();
                    runningTest = p;
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                    runningTest = null;
                    return p.ExitCode;
                }
            }, -1);
            BtnStop.IsEnabled = false;
            Log(code == 0 ? "Exit code 0 - Succeeded" : $"Error: exit code {code}", code == 0 ? LogLevel.Ok : LogLevel.Error);
        }

        void Stop_Click(object sender, RoutedEventArgs e) { try { runningTest?.Kill(); } catch { } }

        // Runs work off the UI thread with the action buttons disabled. Exceptions are logged.
        async Task<T> Busy<T>(Func<T> work, T onError = default)
        {
            foreach (var c in actionControls) c.IsEnabled = false;
            Cursor = Cursors.AppStarting;
            BusyChanged?.Invoke(true);
            try { return await Task.Run(work); }
            catch (Exception ex) { Log("Error: " + ex.Message, LogLevel.Error); return onError; }
            finally
            {
                foreach (var c in actionControls) c.IsEnabled = true;
                Cursor = null;
                BusyChanged?.Invoke(false);
            }
        }

        bool CheckSource()
        {
            if (Directory.Exists(Source)) return true;
            Log("Kit folder not found: " + Source, LogLevel.Error);
            return false;
        }

        void Browse_Click(object sender, RoutedEventArgs e)
        {
            using (var d = new System.Windows.Forms.FolderBrowserDialog { SelectedPath = Source, Description = "Kit folder (the files to copy / edit)" })
                if (Modal(() => d.ShowDialog(new Win32Owner(this))) == System.Windows.Forms.DialogResult.OK) SetSource(d.SelectedPath);
        }

        sealed class Win32Owner : System.Windows.Forms.IWin32Window
        {
            public Win32Owner(Window w) { Handle = new WindowInteropHelper(w).Handle; }
            public IntPtr Handle { get; }
        }

        void Open_Click(object sender, RoutedEventArgs e) => OpenInExplorer();
        void Refresh_Click(object sender, RoutedEventArgs e) => SetSource(CboSource.Text.Trim());
        void Rescan_Click(object sender, RoutedEventArgs e) => LoadInstalls();

        void BrowseRunInput_Click(object sender, RoutedEventArgs e)
        {
            var d = new Microsoft.Win32.OpenFileDialog { Filter = "Run input (*.txt)|*.txt|All files|*.*" };
            string cur = RunInputPath;
            d.InitialDirectory = File.Exists(cur) ? Path.GetDirectoryName(cur) : Source;
            if (Modal(() => d.ShowDialog(this)) == true) { CboRunInput.Text = d.FileName; OnRunInputChanged(); }
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            if (!(e.Data.GetData(DataFormats.FileDrop) is string[] paths) || paths.Length == 0) return;
            string p = paths[0];
            if (Directory.Exists(p)) SetSource(p);
            else if (string.Equals(Path.GetExtension(p), ".txt", StringComparison.OrdinalIgnoreCase)) { CboRunInput.Text = p; OnRunInputChanged(); }
            else if (File.Exists(p)) SetSource(Path.GetDirectoryName(p));
        }

        // ---------------------------------------------------------------- Explorer right-click

        // "Open in Kit Helper" on the folder right-click menu, per user (HKCU, no admin).
        // Directory = right-click a folder; Directory\Background = right-click inside one.
        static readonly string[] ExplorerKeys =
        {
            @"Software\Classes\Directory\shell\KitHelper",
            @"Software\Classes\Directory\Background\shell\KitHelper",
        };

        static bool ExplorerMenuInstalled(out string registeredExe)
        {
            registeredExe = null;
            using (var k = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(ExplorerKeys[0] + @"\command"))
            {
                string cmd = k?.GetValue("") as string;
                if (cmd == null) return false;
                registeredExe = cmd.Split('"').Skip(1).FirstOrDefault();
                return true;
            }
        }

        // The entry always runs the installed copy, so "on" means: registered AND pointing there.
        // An entry pointing at some other copy (set up before 1.1.0) counts as not set up.
        public bool MenuPointsAtInstalled() => ExplorerMenuInstalled(out string exe) && InstalledCopy.IsStatic(exe);

        void UpdateExplorerButton()
        {
            bool registered = ExplorerMenuInstalled(out _);
            BtnExplorer.Content = MenuPointsAtInstalled() ? "Remove \"Open in Kit Helper\" from right-click"
                : registered ? "Move right-click entry to " + InstalledCopy.Folder + " (now: other copy)"
                : "Add \"Open in Kit Helper\" to folder right-click";

            var inst = InstalledCopy.Installed;
            var self = InstalledCopy.Self;
            if (inst == null) { TxtInstalled.Text = "Installed copy: none yet."; TxtInstalled.Foreground = Brush("Dim"); return; }
            int c = inst.CompareTo(self);
            TxtInstalled.Text = "Installed copy: " + inst
                + (InstalledCopy.IsStatic(InstalledCopy.SelfExe) ? "  (this window)"
                   : c > 0 ? "  - newer than this copy"
                   : c < 0 ? "  - older; Add/Remove updates it"
                   : "  - same as this copy");
            TxtInstalled.Foreground = c < 0 ? Brush("Warn") : Brush("Dim");
        }

        void Explorer_Click(object sender, RoutedEventArgs e) => ToggleExplorerMenu();

        // Double-click on the floating icon. Returns the note shown above the icon.
        public string AddRightClickEntry() =>
            MenuPointsAtInstalled()
                ? "\"Open in Kit Helper\" is already in the folder right-click menu."
                : ToggleExplorerMenu("add");

        // Every start: put "Open in Kit Helper" in place, or point it back at the installed copy,
        // unless the user removed it (then it stays off until Add). Running the exe once is
        // therefore all a PC needs. Returns the note for the icon, or null if nothing changed.
        public string EnsureRightClickEntry()
        {
            if (settings.Get("MenuRemoved") == "1" || MenuPointsAtInstalled()) return null;
            return ToggleExplorerMenu("add");
        }

        // action: "add" / "remove", or null = whichever the button currently offers.
        // Returns a one-line summary (for the icon's note); details go to the log.
        public string ToggleExplorerMenu(string action = null)
        {
            action = action ?? (MenuPointsAtInstalled() ? "remove" : "add");
            string summary;
            try
            {
                // Normally a no-op: this window already is the installed copy (see RunInstalled).
                Log(InstalledCopy.EnsureInstalled(), LogLevel.Info);
                // Remembered so the start-up check (EnsureRightClickEntry) respects a Remove.
                if (action == "remove")
                {
                    foreach (var key in ExplorerKeys) Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(key, false);
                    settings.Set("MenuRemoved", "1");
                    summary = "Removed \"Open in Kit Helper\" from the folder right-click menu. It stays off until you add it again.";
                }
                else
                {
                    RegisterMenu();
                    settings.Set("MenuRemoved", "");
                    summary = "Added \"Open in Kit Helper\" to the folder right-click menu.";
                }
                settings.Save();
                Log(summary, LogLevel.Ok);
            }
            catch (Exception ex)
            {
                summary = "Right-click menu: " + ex.Message;
                Log(summary, LogLevel.Error);
            }
            UpdateExplorerButton();
            return summary;
        }

        static void RegisterMenu()
        {
            string exe = InstalledCopy.StaticExe;
            foreach (var key in ExplorerKeys)
                using (var k = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(key))
                {
                    k.SetValue("", "Open in Kit Helper");
                    k.SetValue("Icon", "\"" + exe + "\"");
                    using (var c = k.CreateSubKey("command"))
                        c.SetValue("", "\"" + exe + "\" \"" + (key.Contains("Background") ? "%V" : "%1") + "\"");
                }
        }

        // Kit Helper was called Script Toolbox up to 1.2.1. On a PC that ran it, the installed
        // Kit Helper takes over its right-click entry and removes its installed exe, so the old
        // name cannot linger next to the new one. Settings move in Settings.Load.
        public void MigrateFromScriptToolbox()
        {
            if (!InstalledCopy.IsStatic(InstalledCopy.SelfExe)) return;
            string[] oldKeys =
            {
                @"Software\Classes\Directory\shell\ScriptToolbox",
                @"Software\Classes\Directory\Background\shell\ScriptToolbox",
            };
            try
            {
                bool had = false;
                foreach (var k in oldKeys)
                    using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(k)) had |= key != null;
                if (had)
                {
                    foreach (var k in oldKeys) Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(k, false);
                    if (!ExplorerMenuInstalled(out _)) RegisterMenu();
                    Log("Right-click entry renamed: \"Open in Script Toolbox\" → \"Open in Kit Helper\".", LogLevel.Ok);
                }
            }
            catch (Exception ex) { Log("Could not move the old right-click entry: " + ex.Message, LogLevel.Warn); }

            string oldExe = Path.Combine(InstalledCopy.Folder, "ScriptToolbox.exe");
            if (File.Exists(oldExe))
            {
                try { File.Delete(oldExe); Log($"Removed the old {oldExe} (renamed to Kit Helper).", LogLevel.Ok); }
                catch (Exception ex) { Log($"Old {oldExe} not removed ({ex.Message}); it is retried next start.", LogLevel.Warn); }
            }
            UpdateExplorerButton();
        }

        static bool IsElevated()
        {
            using (var id = WindowsIdentity.GetCurrent())
                return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
        }

        void Admin_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            try
            {
                Process.Start(new ProcessStartInfo(InstalledCopy.SelfExe, "\"" + Source + "\" " + Program.NewInstanceFlag) { UseShellExecute = true, Verb = "runas" });
                Program.Exit();
            }
            catch (Exception ex) { Log("Not elevated: " + ex.Message, LogLevel.Warn); }   // UAC declined
        }

        // ---------------------------------------------------------------- log

        sealed class LogLine
        {
            public string Text { get; set; }
            public Brush Brush { get; set; }
        }

        Brush Brush(string key) => (Brush)FindResource(key);

        void Log(string text, LogLevel level)
        {
            if (!Dispatcher.CheckAccess()) { Dispatcher.BeginInvoke(new Action(() => Log(text, level))); return; }
            string key = level == LogLevel.Detail ? "Dim" : level == LogLevel.Ok ? "Ok"
                       : level == LogLevel.Warn ? "Warn" : level == LogLevel.Error ? "Err" : "Fg";
            log.Add(new LogLine { Text = $"{DateTime.Now:HH:mm:ss}  {text}", Brush = Brush(key) });
            if (log.Count > 5000) log.RemoveAt(0);
            LstLog.ScrollIntoView(log[log.Count - 1]);
        }

        void CopyLog_Click(object sender, RoutedEventArgs e)
        {
            try { Clipboard.SetText(string.Join(Environment.NewLine, log.Select(l => l.Text))); } catch { }
        }

        void ClearLog_Click(object sender, RoutedEventArgs e) => log.Clear();
    }
}
