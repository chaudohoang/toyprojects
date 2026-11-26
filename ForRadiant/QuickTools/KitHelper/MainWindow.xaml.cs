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
            BtnExplorer.ToolTip = "Added automatically whenever Kit Helper starts; Remove lasts until the next start. Runs the copy in "
                                + InstalledCopy.Folder + ". Windows 11: under \"Show more options\".";
            LstLog.ItemsSource = log;

            foreach (var t in CopyTarget.All)
            {
                var target = t;
                var b = new Button
                {
                    Content = "Copy all → " + (t.Label ?? string.Join(" + ", t.SubFolders)) + (t.IncludeSubdirs ? "  (with subfolders)" : ""),
                    Style = (Style)FindResource("ActionButton"),
                };
                b.Click += (s, e) => DoCopy(target);
                copyButtons[t] = b;
                actionControls.Add(b);
                CopyButtons.Children.Add(b);
            }
            actionControls.AddRange(new UIElement[] { BtnPackage, BtnUnblock, BtnVigOn, BtnVigOff, BtnRun });

            // Folder: command-line arg (another copy passes the folder it sits in) > the most
            // recent folder when this is the installed copy > this exe's own folder.
            string recent = settings.Get("Recent").Split('|').FirstOrDefault(Directory.Exists);
            string folder = !string.IsNullOrEmpty(startFolder) && Directory.Exists(startFolder) ? startFolder
                : InstalledCopy.IsStatic(InstalledCopy.SelfExe) && recent != null ? recent
                : AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            foreach (string db in PocbTemplate.Databases) CboPocbDb.Items.Add(db);
            // The one picked last time, else the first of C/D/E:\DATABASE that exists on this PC.
            CboPocbDb.Text = settings.Get("PocbDatabase") is string savedDb && savedDb != "" ? savedDb
                           : PocbTemplate.Databases.FirstOrDefault(Directory.Exists) ?? PocbTemplate.Databases[0];
            UpdateDbInfo();
            LoadInstalls();
            SetSource(folder);
            UpdateExplorerButton();

            KeyDown += (s, e) => { if (e.Key == Key.Escape) HidePanel(); };
            Closing += (s, e) => { if (!Program.Exiting) { e.Cancel = true; HidePanel(); } };
            Deactivated += (s, e) => AutoHide();
            Log("Kit Helper ready. Drop a kit folder, or a run-input .txt, on the K icon to open it.", LogLevel.Info);
        }

        // ---------------------------------------------------------------- show / hide

        public void ShowNear(Window anchor)
        {
            anchor = anchor ?? Anchor;
            // Docked to the right edge of the screen the icon is on. Vertically next to the
            // icon: below it when the icon is in the top half, above it otherwise; always
            // fully on that screen.
            var wa = WorkAreaOf(anchor ?? this);
            if (Height > wa.Height) Height = wa.Height;
            Left = wa.Right - Width;
            double top = wa.Top;
            if (anchor != null)
                top = anchor.Top + anchor.Height / 2 < wa.Top + wa.Height / 2 ? anchor.Top + anchor.Height : anchor.Top - Height;
            Top = Clamp(top, wa.Top, wa.Bottom - Height);
            Show();
            if (WindowState == WindowState.Minimized) WindowState = WindowState.Normal;
            Activate();
            ResultsWindow.Current?.ShowWithPanel(this);     // the two always appear together
            if (currentConfirm != null && !currentConfirm.IsVisible) currentConfirm.Show();   // on top of the results
        }

        static double Clamp(double v, double min, double max) => v < min ? min : v > max ? max : v;

        // Work area (screen minus taskbar) of the monitor a window is on, in WPF units.
        // SystemParameters.WorkArea is the primary monitor only.
        static Rect WorkAreaOf(Window w)
        {
            var h = new WindowInteropHelper(w).Handle;
            var source = PresentationSource.FromVisual(w);
            if (h == IntPtr.Zero || source?.CompositionTarget == null) return SystemParameters.WorkArea;
            var px = System.Windows.Forms.Screen.FromHandle(h).WorkingArea;    // device pixels
            var toDip = source.CompositionTarget.TransformFromDevice;
            return new Rect(toDip.Transform(new Point(px.Left, px.Top)), toDip.Transform(new Point(px.Right, px.Bottom)));
        }

        // Clicking anywhere else hides the panel, like a menu. Two exceptions:
        //  - the click is on the floating icon: the icon's own click handler toggles, so hiding
        //    here would make that click re-open it;
        //  - one of the panel's own dialogs took focus (see Modal).
        int modalDepth;

        // Panel or results window deactivated. Hides the pair unless the focus just moved between
        // the two (checked once the new window is active) or onto the K icon.
        public void AutoHide()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                var results = ResultsWindow.Current;
                // Not the cursor position: a window that takes the focus while the mouse rests on the
                // panel (a program starting) must still hide it. The K icon is the exception (its
                // click toggles the panel itself).
                if (!IsVisible || modalDepth > 0 || Program.Exiting || IsActive || (results != null && results.IsActive)
                    || (currentConfirm != null && currentConfirm.IsActive) || CursorOver(Anchor)) return;
                HidePanel();
            }));
        }

        // Runs a dialog with auto-hide suspended: opening it deactivates the panel.
        // WPF's ShowDialog disables every window of the app, the floating K icon included, so a
        // click on the icon did nothing while e.g. the copy report was open (and it could sit
        // behind other apps). Once the dialog is up, the icon is enabled again; while InModal
        // its click calls FocusModal instead of toggling the panel.
        T Modal<T>(Func<T> dialog)
        {
            modalDepth++;
            // Queued before the dialog starts, so it runs inside the dialog's message loop, after
            // ShowDialog has disabled the windows (MessageBox pumps the dispatcher too).
            Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(EnableIcon));
            try { return dialog(); }
            finally { modalDepth--; }
        }

        public bool InModal => modalDepth > 0;

        void EnableIcon()
        {
            if (modalDepth == 0 || Anchor == null) return;
            IntPtr h = new WindowInteropHelper(Anchor).Handle;
            if (h != IntPtr.Zero) EnableWindow(h, true);
        }

        // Brings the panel and the dialog open on it to the front: the K icon's click while a
        // dialog is open. GetLastActivePopup finds that dialog (a Kit Helper window or a MessageBox).
        public void FocusModal()
        {
            if (!IsVisible) Show();
            IntPtr h = new WindowInteropHelper(this).Handle;
            IntPtr popup = GetLastActivePopup(h);
            SetForegroundWindow(popup != IntPtr.Zero ? popup : h);
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool EnableWindow(IntPtr hWnd, bool enable);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern IntPtr GetLastActivePopup(IntPtr hWnd);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

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
            ResultsWindow.Current?.Hide();
            currentConfirm?.Hide();             // still pending; back with the panel
            Hide();
        }

        // A folder opened with Kit Helper (Explorer right-click, another copy, a drop on the icon).
        // picks: several subfolders of folder dropped at once - copied together as one package.
        public void OpenFolder(string folder, string[] picks = null, bool offerCopy = true)
        {
            // Not under an open dialog: the folder would change beneath it. Bring the dialog back.
            if (InModal)
            {
                FocusModal();
                (Anchor as FloatingIconWindow)?.ShowToast("Close the open dialog first, then open the folder again.");
                return;
            }
            ShowNear(Anchor);
            Log(picks == null ? $"Opened {folder}" : $"Opened {picks.Length} folders in {folder}: {string.Join(", ", picks)}", LogLevel.Info);
            LoadInstalls();                             // TrueTest may have started or stopped since
            SetSource(folder.TrimEnd('\\'), picks);
            // Straight to the confirmation of what this folder is for (a package, several picked
            // folders, or the suggested button): the results window may be covering the buttons.
            if (offerCopy) Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(OfferSuggestedCopy));
        }

        // The kit files a copy would write from (New / Replace rows), for the Unblock option.
        static IEnumerable<string> CopiedSources(Copier.Result r) =>
            r.Items.Where(i => (i.Status == CopyStatus.New || i.Status == CopyStatus.Replaced) && i.From != null).Select(i => i.From);

        // Opens the confirmation of the highlighted copy, if any: Copy package, else the suggested
        // Copy all button. Nothing when no kit type was recognised, while busy or a dialog is up.
        void OfferSuggestedCopy()
        {
            if (InModal || !IsVisible || !BtnPackage.IsEnabled) return;
            if (BtnPackage.Visibility == Visibility.Visible) { Package_Click(BtnPackage, null); return; }
            var suggested = copyButtons.FirstOrDefault(kv => "suggested".Equals(kv.Value.Tag)).Key;
            if (suggested != null) DoCopy(suggested);
        }

        // A file dropped on the icon: open its folder; a .txt also becomes the run-test input.
        public void OpenFile(string path)
        {
            if (InModal) { OpenFolder(Path.GetDirectoryName(path), null, offerCopy: false); return; }   // just brings the dialog back
            OpenFolder(Path.GetDirectoryName(path), null, offerCopy: false);
            if (!string.Equals(Path.GetExtension(path), ".txt", StringComparison.OrdinalIgnoreCase)) return;
            var item = allRunInputs.FirstOrDefault(i => string.Equals(i.Path, path, StringComparison.OrdinalIgnoreCase));
            if (item != null) SelectInput(item.Path);
            else CboRunInput.Text = path;               // not a run input by its first line: use it anyway
            Dispatcher.BeginInvoke(new Action(OnRunInputChanged), DispatcherPriority.Background);
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

        // Subfolders of Source picked by a multi-folder drop, or null = the whole folder. While set,
        // the folder is offered only as a package of those subfolders (the single-target buttons
        // would copy the whole parent folder, so they are hidden).
        string[] picked;

        // The kit type recognised for the open folder (its button), or null: nothing recognised, or a package.
        CopyTarget detectedTarget;

        void UpdateCopyButtons()
        {
            bool all = ChkAllTargets.IsChecked == true;
            foreach (var kv in copyButtons)
                kv.Value.Visibility = detectedTarget == null || all || kv.Key == detectedTarget ? Visibility.Visible : Visibility.Collapsed;
        }

        void AllTargets_Changed(object sender, RoutedEventArgs e) => UpdateCopyButtons();

        // True when this button is not the detected one of a recognised kit: its copy leaves out files
        // of other kit types.
        bool RulesOut(CopyTarget target) =>
            detectedTarget != null && target != detectedTarget && string.Equals(Source, appliedSource, StringComparison.OrdinalIgnoreCase);

        // A kit's OMIT\ / PUC\ opened alone (Package.SingleTargetModels): the model folders its
        // suggested button copies, instead of the opened folder itself.
        string[] modelSources;
        CopyTarget modelTarget;

        void SetSource(string folder, string[] picks = null)
        {
            CancelConfirm();                    // it was for the previous folder
            appliedSource = folder;
            picked = picks;
            CboSource.Text = folder;
            if (!Directory.Exists(folder))
            {
                TxtSourceInfo.Text = "Folder not found.";
                TxtSourceInfo.Foreground = Brush("Err");
                LvIni.ItemsSource = null;
                return;
            }
            int files = Directory.GetFiles(folder).Count(f => !Kit.NeverCopy(f) && !string.Equals(Path.GetFileName(f), ownExe, StringComparison.OrdinalIgnoreCase));
            int dirs = Directory.GetDirectories(folder).Length;
            int inis = Vignetting.IniFiles(folder).Length;
            TxtSourceInfo.Text = picked != null ? $"{picked.Length} folders picked: {string.Join(", ", picked)}"
                               : $"{files} file(s), {dirs} subfolder(s), {inis} ini";
            TxtSourceInfo.Foreground = Brush("Dim");
            RememberRecent(folder);

            // Mark the copy target this folder is meant for.
            // A package is copied as a whole; its root matches no single target.
            bool package = Package.Is(folder, picked);
            string why = null;
            var suggested = package || picked != null ? null : Kit.Suggest(folder, out why);
            // A package (whole folder or several picked folders) is copied with Copy package only: a
            // single button would copy the folder's own loose files (.seqxc, .bmp, Kit Helper.exe),
            // not its parts. Open one part to use the single buttons.
            CopyButtons.Visibility = picked == null && !package ? Visibility.Visible : Visibility.Collapsed;
            modelSources = null; modelTarget = null;
            if (!package && picked == null && suggested == null)
            {
                var models = Package.SingleTargetModels(folder, out var mt, out string mwhy);
                if (models != null) { modelSources = models; modelTarget = suggested = mt; why = mwhy; }
            }
            foreach (var kv in copyButtons) kv.Value.Tag = kv.Key == suggested ? "suggested" : null;
            // Only the detected action is offered: another button on a recognised kit copies it to the
            // wrong place (an Omit kit's model folder into POCB4.2\<model>). Nothing recognised: every
            // button, as the manual fallback. "Show all copy targets" brings the others back, and they
            // leave out files of other kit types (Copier.ByCategory); it resets for each folder.
            detectedTarget = package || picked != null ? null : suggested;
            ChkAllTargets.IsChecked = false;
            ChkAllTargets.Visibility = detectedTarget != null ? Visibility.Visible : Visibility.Collapsed;
            UpdateCopyButtons();
            BtnPackage.Visibility = package ? Visibility.Visible : Visibility.Collapsed;
            BtnPackage.Tag = package ? "suggested" : null;
            TxtSuggest.Text = package ? (picked != null ? "Picked folders, copied as one package:\n" : "Package:\n") + Package.Describe(Package.Scan(folder, picked))
                                        + "\nTo copy one part only: untick the rest in the confirmation, or open that folder alone."
                            : picked != null ? "None of the picked folders is a kit folder Kit Helper knows. Open one folder to use the single copy buttons."
                            : suggested == null ? (Directory.Exists(folder) ? "Kit type not recognised: pick the copy target yourself, or open the folder that holds the kit files." : "")
                            : $"Detected: {suggested.Name}  ({why})";
            TxtSuggest.Visibility = TxtSuggest.Text == "" ? Visibility.Collapsed : Visibility.Visible;

            // Vignetting list: with a recognised kit or a package, the INIs in its subfolders too (an
            // InputIMG\<model>, a package's parts); else only the folder's own (a parent folder full of
            // kits must not have Set ON / OFF reach into all of them).
            deepInis = package || picked != null || suggested != null;
            RefreshIni();

            // Run inputs found in this folder; keep a browsed file from elsewhere if one is set.
            string current = RunInputPath;
            allRunInputs.Clear();
            foreach (string f in RunInputFolders(folder, package).SelectMany(Kit.RunInputs).Distinct(StringComparer.OrdinalIgnoreCase))
                allRunInputs.Add(new RunInputItem(f, folder));
            // The box lists one kind at a time (the mode's); start with the current input's kind, else
            // the first input's.
            var keep = allRunInputs.FirstOrDefault(i => string.Equals(i.Path, current, StringComparison.OrdinalIgnoreCase)) ?? allRunInputs.FirstOrDefault();
            if (keep != null) SelectInput(keep.Path);
            else
            {
                CboRunInput.Items.Clear();
                if (!File.Exists(current)) CboRunInput.Text = "";
            }
            OnRunInputChanged();
        }

        // Every run input found for the open folder; the box shows only the selected mode's.
        readonly List<RunInputItem> allRunInputs = new List<RunInputItem>();

        // Lists the inputs of one kind (Omit / PUC / POCBMobile) and selects keep, else the first;
        // none of that kind: an empty box.
        void ShowInputsFor(string mode, string keep)
        {
            var list = allRunInputs.Where(i => File.Exists(i.Path) && ModeOf(i.Path) == mode).ToList();
            foreach (var i in list) i.ShowFolder = list.Count(x => string.Equals(x.Name, i.Name, StringComparison.OrdinalIgnoreCase)) > 1;
            CboRunInput.Items.Clear();
            foreach (var i in list) CboRunInput.Items.Add(i);
            var sel = list.FirstOrDefault(i => string.Equals(i.Path, keep, StringComparison.OrdinalIgnoreCase)) ?? list.FirstOrDefault();
            if (sel != null) CboRunInput.SelectedItem = sel;
            else { CboRunInput.SelectedIndex = -1; CboRunInput.Text = ""; lastRunInput = ""; }
        }

        // Selects a listed input: its mode, then the list of that kind with it selected.
        void SelectInput(string path)
        {
            string mode = ModeOf(path);
            settingMode = true;
            RbPocb.IsChecked = mode == "POCBMobile";
            RbPuc.IsChecked = mode == "PUC";
            RbOmit.IsChecked = mode == "Omit";
            settingMode = false;
            ShowInputsFor(mode, path);
        }

        // Where Run test looks for inputs: the open folder, an OMIT\ / PUC\ folder's model folders, and
        // for a package (or several picked folders) every part - OMIT\<model>, PUC\<model>, a
        // POCBMobile kit, and a mirrored TrueTest\<sub> - so Omit, PUC and POCBMobile inputs of the
        // whole kit are listed together, each shown with its subfolder.
        IEnumerable<string> RunInputFolders(string folder, bool package)
        {
            var dirs = new List<string> { folder };
            if (modelSources != null) dirs.AddRange(modelSources);
            if (package)
                foreach (var u in Package.Scan(folder, picked).Units)
                {
                    dirs.Add(u.Folder);
                    if (u.Mirror != null && Directory.Exists(u.Folder)) dirs.AddRange(Directory.GetDirectories(u.Folder));
                }
            return dirs.Distinct(StringComparer.OrdinalIgnoreCase);
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
            public readonly string Name;
            readonly string folder;
            // Shown as the file name only (the list holds one kind at a time, and inputTemplate* /
            // inputTest* names already differ). Its folder, relative to the kit, is added only when
            // another listed input has the same name (ShowInputsFor).
            public bool ShowFolder;
            public RunInputItem(string path, string kit = null)
            {
                Path = path;
                Name = System.IO.Path.GetFileName(path);
                string dir = System.IO.Path.GetDirectoryName(path) ?? "", k = kit?.TrimEnd('\\');
                folder = k != null && dir.StartsWith(k + "\\", StringComparison.OrdinalIgnoreCase) ? dir.Substring(k.Length + 1)
                       : string.Equals(dir, k, StringComparison.OrdinalIgnoreCase) ? "" : System.IO.Path.GetFileName(dir);
            }
            public override string ToString() => ShowFolder && folder != "" ? $"{Name}  ({folder})" : Name;
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
            settingMode = true;                 // the input decides the mode: not a user's mode switch
            if (File.Exists(p))
            {
                bool pocb = RunTest.IsPocbMobileInput(p);
                RbPocb.IsChecked = pocb;
                RbPuc.IsChecked = !pocb && RunTest.LooksLikePuc(p);
            }
            RbOmit.IsChecked = RbPuc.IsChecked != true && RbPocb.IsChecked != true;
            settingMode = false;
            UpdateRunMode();
        }

        bool settingMode;

        static string ModeOf(string input) =>
            RunTest.IsPocbMobileInput(input) ? "POCBMobile" : RunTest.LooksLikePuc(input) ? "PUC" : "Omit";

        string CheckedMode => RbPocb.IsChecked == true ? "POCBMobile" : RbPuc.IsChecked == true ? "PUC" : "Omit";

        // The user switched Omit / PUC / POCBMobile: with several kits open (a package), jump to the
        // first input of that kind, so each run test is one click away. No such input: keep the box.
        void PickInputForMode()
        {
            string want = CheckedMode, cur = RunInputPath;
            // A file browsed from elsewhere that already matches the mode stays.
            if (File.Exists(cur) && ModeOf(cur) == want && !allRunInputs.Any(i => string.Equals(i.Path, cur, StringComparison.OrdinalIgnoreCase))) return;
            // Only that kind's inputs in the box; none: an empty box rather than another kind's input.
            ShowInputsFor(want, cur);
        }

        // SelectionChanged fires before the editable text follows the new item: read it after.
        void CboRunInput_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Dispatcher.BeginInvoke(new Action(OnRunInputChanged), DispatcherPriority.Background);
        void CboRunInput_LostFocus(object sender, KeyboardFocusChangedEventArgs e) => OnRunInputChanged();
        void RunMode_Changed(object sender, RoutedEventArgs e)
        {
            // RbOmit's IsChecked="True" raises Checked while the window is still loading, before the
            // other radios / the input box exist (1.10.20 crashed at start on exactly that).
            if (RbPuc == null || RbPocb == null || CboRunInput == null || TxtRunModeHint == null) return;
            if (!settingMode) PickInputForMode();
            UpdateRunMode();
        }

        // PUC runs where its input lives (RunTestPUC.exe had no working-dir choice: it ran in the
        // folder it sat in). POCBMobile runs in the selected install's POCBMobile folder. Omit runs
        // in the chosen ScratchDustDetect_NY folder.
        bool FixedWorkDir => RbPuc.IsChecked == true || RbPocb.IsChecked == true;

        void UpdateRunMode()
        {
            if (TxtRunModeHint == null) return;     // radio events while the window is still loading
            TxtRunModeHint.Text = RbPuc.IsChecked == true ? @"PUC runs in the kit folder, outputs to …\Test\"
                : RbPocb.IsChecked == true ? "Fills it into input.txt"
                : "Omit runs in the working dir";
            var pocbVisibility = RbPocb.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            foreach (UIElement c in new UIElement[] { LblPocbModel, CboPocbModel, LblPocbPid, PnlPocbPid })
                c.Visibility = pocbVisibility;
            if (RbPocb.IsChecked == true)
            {
                string p = RunInputPath;
                LoadPocbModels(File.Exists(p) ? PocbTemplate.SequenceModel(p) : null);
            }
            if (FixedWorkDir)
            {
                if (CboWorkDir.IsEnabled) omitWorkDir = CboWorkDir.Text;   // leaving Omit: remember it
                CboWorkDir.IsEnabled = false;
                CboWorkDir.Items.Clear();
                string p = RunInputPath;
                CboWorkDir.Text = RbPocb.IsChecked == true ? (Install == null ? "" : Path.Combine(Install, "POCBMobile"))
                    : File.Exists(p) ? Path.GetDirectoryName(p) : Source;
            }
            else if (!CboWorkDir.IsEnabled)
            {
                // Back to Omit: restore what the box held before the PUC kit (re-matched to the
                // install), NOT the value saved by a previous run - that brought back a custom
                // folder nobody had picked this session.
                CboWorkDir.IsEnabled = true;
                CboWorkDir.Text = omitWorkDir ?? "";
                LoadWorkDirs(useSaved: false);
            }
        }

        string omitWorkDir;

        // ---------------------------------------------------------------- POCBMobile values

        string PocbDatabase => CboPocbDb.Text.Trim().TrimEnd('\\');
        string PocbModel => (CboPocbModel.SelectedItem as ComboBoxItem)?.Tag as string;

        // Model list = <database>\InputIMG\*, the PUC models ("00" in the name) highlighted.
        // Selection: the given model (the kit sequence's), else the one picked before, else none.
        void LoadPocbModels(string prefer = null)
        {
            string keep = prefer ?? PocbModel;
            CboPocbModel.Items.Clear();
            foreach (string m in PocbTemplate.Models(PocbDatabase))
            {
                bool puc = PocbTemplate.IsPucModel(m);
                CboPocbModel.Items.Add(new ComboBoxItem
                {
                    Content = m,
                    Tag = m,
                    FontWeight = puc ? FontWeights.Bold : FontWeights.Normal,
                    Foreground = Brush(puc ? "Accent" : "Dim"),
                });
            }
            var hit = CboPocbModel.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(i => string.Equals((string)i.Tag, keep, StringComparison.OrdinalIgnoreCase))
                ?? CboPocbModel.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(i => string.Equals((string)i.Tag, PocbModel, StringComparison.OrdinalIgnoreCase));
            CboPocbModel.SelectedItem = hit;
        }

        // Folders appear in InputIMG while the panel is open (a new model's images): re-read on open.
        void CboPocbModel_DropDownOpened(object sender, EventArgs e) => LoadPocbModels();

        string lastPocbDb;
        void PocbDbChanged()
        {
            if (PocbDatabase == lastPocbDb) return;
            lastPocbDb = PocbDatabase;
            UpdateDbInfo();
            LoadPocbModels();
        }

        // Under the Database box: whether it exists and how many model folders it holds, plus
        // the other C/D/E:\DATABASE folders on this PC, so a wrong pick is visible at a glance.
        void UpdateDbInfo()
        {
            if (TxtDbInfo == null) return;
            string db = PocbDatabase;
            var others = PocbTemplate.Databases.Where(d => Directory.Exists(d) && !string.Equals(d, db, StringComparison.OrdinalIgnoreCase)).ToList();
            string also = others.Count > 0 ? "  ·  also on this PC: " + string.Join(", ", others) : "";
            if (!Directory.Exists(db))
            {
                TxtDbInfo.Text = "Folder not found." + also;
                TxtDbInfo.Foreground = Brush("Err");
                return;
            }
            int models = PocbTemplate.Models(db).Length;
            TxtDbInfo.Text = $"{models} model folder(s) in InputIMG" + also;
            TxtDbInfo.Foreground = Brush("Dim");
        }

        void OpenDb_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(PocbDatabase)) Process.Start("explorer.exe", "\"" + PocbDatabase + "\"");
            else Log("Database folder not found: " + PocbDatabase, LogLevel.Error);
        }
        // SelectionChanged fires before the editable text follows the new item: read it after.
        void CboPocbDb_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            Dispatcher.BeginInvoke(new Action(PocbDbChanged), DispatcherPriority.Background);
        void CboPocbDb_LostFocus(object sender, KeyboardFocusChangedEventArgs e) => PocbDbChanged();

        sealed class IniRow
        {
            public string Name { get; set; }
            public string Value { get; set; }
            public Brush Brush { get; set; }
        }

        bool deepInis;

        // The INIs the Vignetting list and Set ON / OFF work on: the open folder's (or the picked
        // folders'), with subfolders when a kit / package is recognised (deepInis).
        string[] KitInis()
        {
            string src = Source;
            var roots = string.Equals(src, appliedSource, StringComparison.OrdinalIgnoreCase) && picked != null
                ? picked.Select(p => Path.Combine(src, p)) : new[] { src };
            return roots.SelectMany(r => deepInis ? Vignetting.IniFilesDeep(r) : Vignetting.IniFiles(r)).ToArray();
        }

        // Shown relative to the open folder: "D994-CB-FMM_MATHON\LGD_COMP_INI_….ini".
        string IniName(string f)
        {
            string src = Source.TrimEnd('\\');
            return f.StartsWith(src + "\\", StringComparison.OrdinalIgnoreCase) ? f.Substring(src.Length + 1) : Path.GetFileName(f);
        }

        void RefreshIni()
        {
            var rows = new List<IniRow>();
            foreach (string f in KitInis())
            {
                string v;
                try { v = Vignetting.Read(f); } catch (Exception ex) { v = "error: " + ex.Message; }
                if (v == null) continue;     // ini without the key: not a vignetting ini
                rows.Add(new IniRow { Name = IniName(f), Value = v, Brush = Vignetting.IsDx(f) ? Brush("Dim") : Brush("Fg") });
            }
            if (rows.Count == 0) rows.Add(new IniRow { Name = "(no ini with " + Vignetting.Key + ")", Value = "", Brush = Brush("Dim") });
            LvIni.ItemsSource = rows;
        }

        void LoadInstalls()
        {
            string keep = Install ?? settings.Get("Install");
            var installs = TrueTest.Installs();
            // Refilling the list passes through "no install selected"; the working dir must not
            // react to that (it fell back to the custom folder, which then counted as a
            // deliberate choice and stuck). It is matched once, at the end.
            refillingInstalls = true;
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
            refillingInstalls = false;

            if (installs.Count == 0) Log($"No TrueTest install found under {TrueTest.Root}. Copy is unavailable.", LogLevel.Warn);
            LoadWorkDirs();
        }

        bool refillingInstalls;

        void CboInstall_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!refillingInstalls) LoadWorkDirs();
        }

        // useSaved: an empty box falls back to the working dir saved by the last run (start-up).
        void LoadWorkDirs(bool useSaved = true)
        {
            if (FixedWorkDir) { UpdateRunMode(); return; }   // fixed working dir (see UpdateRunMode)
            string keep = CboWorkDir.Text;
            if (string.IsNullOrEmpty(keep) && useSaved) keep = settings.Get("RunWorkDir");
            CboWorkDir.Items.Clear();
            foreach (var i in TrueTest.Installs()) CboWorkDir.Items.Add(Path.Combine(i, "ScratchDustDetect_NY"));
            CboWorkDir.Items.Add(RunTest.CustomWorkDir);
            // A TrueTest ScratchDustDetect_NY folder follows the selected install, so the run
            // tests the same TrueTest the kit is copied to. Anything else (the custom batch
            // folder, a typed path) was picked on purpose and is kept.
            bool followsInstall = string.IsNullOrEmpty(keep)
                || keep.StartsWith(TrueTest.Root + "\\", StringComparison.OrdinalIgnoreCase);
            // No install at all (none found): leave a TrueTest path alone rather than swap in
            // the custom folder.
            CboWorkDir.Text = !followsInstall ? keep
                : Install != null ? Path.Combine(Install, "ScratchDustDetect_NY")
                : !string.IsNullOrEmpty(keep) ? keep : RunTest.CustomWorkDir;
        }

        public void SaveSettings()
        {
            settings.Set("Install", Install);
            if (!FixedWorkDir) settings.Set("RunWorkDir", CboWorkDir.Text);
            settings.Set("PocbDatabase", PocbDatabase);
            settings.Save();
        }

        // ---------------------------------------------------------------- actions

        async void DoCopy(CopyTarget target)
        {
            if (!CheckSource()) return;
            if (target.ToDatabase) { DoCopyToDatabase(target); return; }
            if (Install == null) { Log("Pick a TrueTest install first.", LogLevel.Error); return; }
            string install = Install, source = Source;
            bool ruleOut = RulesOut(target);

            // OMIT\ / PUC\ opened alone: its model folder(s) are what this button copies.
            var sources = target == modelTarget && modelSources != null && string.Equals(source, appliedSource, StringComparison.OrdinalIgnoreCase)
                ? modelSources : new[] { source };
            string dests = string.Join(", ", target.For(sources[0]).SubFolders);
            string title = "Copy all → " + target.Name;
            string where = $"{string.Join("\n", sources)}\n→ {install}  [{dests}]"
                         + (ruleOut ? $"\nDetected as {detectedTarget.Name}: files of other kit types are left out." : "");
            Copier.Result CopyAll(LogFn log, Backup b, bool dryRun) => Copier.ByCategory(ruleOut, () =>
            {
                var all = new Copier.Result();
                foreach (string src in sources)
                {
                    var one = Copier.Run(src, install, target, ownExe, log, b, Package.TrueTestDir, dryRun);
                    all.Copied += one.Copied; all.Failed += one.Failed; all.Skipped += one.Skipped;
                    all.AccessDenied |= one.AccessDenied;
                    all.Items.AddRange(one.Items);
                }
                return all;
            });

            // Confirmation first, as for a package: the same copy as a dry run, files can be unticked,
            // and INIs carrying the vignetting key can be set on the way.
            var preview = CopyAll((s, l) => { }, null, dryRun: true);
            var confirm = new CopyResultWindow(title, where, preview, null, preview: true) { Owner = this };
            confirm.OfferVignetting(Vignetting.CopiedInis(preview));
            confirm.OfferUnblock(CopiedSources(preview));
            if (!await Confirm(confirm)) return;
            string vigValue = confirm.VignettingValue;
            bool vigDx = confirm.VignettingIncludeDx;
            var excluded = confirm.Excluded;
            if (!AskCloseTrueTest(install, "Copy to " + target.Name, out string restartPath)) return;
            if (confirm.UnblockFirst) await UnblockFolders(sources);

            Log($"Copy {string.Join(" + ", sources)} → {install} [{dests}]", LogLevel.Info);
            if (excluded.Count > 0) Log($"{excluded.Count} file(s) unticked, left as they are.", LogLevel.Info);
            var backup = new Backup(target.Name, source, install, null);
            var r = await Busy(() => Copier.Excluding(excluded, () =>
            {
                var res = CopyAll(Log, backup, dryRun: false);
                if (vigValue != null)
                {
                    Log($"Vignetting → {vigValue} in the copied INIs" + (vigDx ? " (DX included)" : " (DX left as is)"), LogLevel.Info);
                    Vignetting.ApplyToCopied(res, vigValue, vigDx, Log);
                }
                return res;
            }));
            backup.Report(Log);
            LogCopyResult(r);
            ShowCopyResult(title, where, r, backup);
            OfferRestart(restartPath);
            LoadInstalls();
        }

        // The per-file summary (CopyResultWindow) after any copy. Null r = the copy threw (logged).
        void ShowCopyResult(string title, string where, Copier.Result r, Backup backup)
        {
            if (r == null) return;
            // Not modal: a tab in the results window, so the next copy can start right away.
            var w = new CopyResultWindow(title, where, r, backup.Count > 0 ? backup.Root : null);
            ResultsWindow.Add(this, w, title, r.Items.Any(i => i.Status == CopyStatus.Failed));
        }

        // A model folder → <database>\InputIMG\<its name>, the database being the one picked in the
        // POCBMobile run panel. TrueTest copies the PUC dll from there when a PUC step runs, so
        // it need not be closed for this.
        async void DoCopyToDatabase(CopyTarget target)
        {
            string db = PocbDatabase, source = Source, model = Path.GetFileName(source);
            bool ruleOut = RulesOut(target);
            if (!Directory.Exists(db)) { Log($"Database folder not found: {db} (pick it under Database at the top).", LogLevel.Error); return; }
            string inputImg = Path.Combine(db, "InputIMG");
            var into = new CopyTarget { Name = target.Name, SubFolders = new[] { model }, IncludeSubdirs = false };
            // Confirm with the list of what will happen: the same copy, as a dry run.
            var preview = Copier.ByCategory(ruleOut, () =>
                Copier.Run(source, inputImg, into, ownExe, (s, l) => { }, null, Path.Combine(Package.DatabaseDir, "InputIMG"), dryRun: true));
            var w = await ConfirmCopy("Copy to Database model " + model, $"{source}\n→ {Path.Combine(inputImg, model)}"
                + (ruleOut ? $"\nDetected as {detectedTarget.Name}: files of other kit types are left out." : ""), preview, offerUnblock: true);
            if (w == null) return;
            var excluded = w.Excluded;
            if (w.UnblockFirst) await UnblockFolders(new[] { source });

            Log($"Copy {source} → {Path.Combine(inputImg, model)}", LogLevel.Info);
            var backup = new Backup(target.Name + " " + model, source, null, db);
            var r = await Busy(() => Copier.ByCategory(ruleOut, () => Copier.Excluding(excluded, () =>
                Copier.Run(source, inputImg, into, ownExe, Log, backup, Path.Combine(Package.DatabaseDir, "InputIMG")))));
            backup.Report(Log);
            LogCopyResult(r);
            ShowCopyResult("Copy to Database model " + model, $"{source}\n→ {Path.Combine(inputImg, model)}", r, backup);
        }

        async void Package_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckSource()) return;
            string source = Source;
            var picks = string.Equals(source, appliedSource, StringComparison.OrdinalIgnoreCase) ? picked : null;
            var layout = Package.Scan(source, picks);
            if (!Package.Is(source, picks)) { Log("Not a package: no kit folders found in " + source, LogLevel.Error); return; }
            string name = Path.GetFileName(source) + (picks != null ? $" ({picks.Length} folders)" : "");
            // Ask only for what the package needs: a TrueTest install and/or a database.
            string install = null, db = null;
            if (layout.NeedsInstall)
            {
                if (Install == null) { Log("Pick a TrueTest install first.", LogLevel.Error); return; }
                install = Install;
            }
            if (layout.NeedsDatabase)
            {
                db = PocbDatabase;
                if (!Directory.Exists(db)) { Log($"Database folder not found: {db} (pick it under Database at the top).", LogLevel.Error); return; }
            }

            string where = (install != null ? $"TrueTest install: {install}" : "") + (install != null && db != null ? "     " : "")
                         + (db != null ? $"Database: {db}" : "") + "\n" + Package.Describe(layout);
            // Confirmation, with the vignetting option when INIs carrying the key are copied.
            var preview = Package.Preview(source, install, db, ownExe, picks);
            var confirm = new CopyResultWindow("Copy package " + name, where, preview, null, preview: true) { Owner = this };
            confirm.OfferVignetting(Vignetting.CopiedInis(preview));
            confirm.OfferUnblock(CopiedSources(preview));
            if (!await Confirm(confirm)) return;
            string vigValue = confirm.VignettingValue;
            bool vigDx = confirm.VignettingIncludeDx;
            var excluded = confirm.Excluded;               // files unticked in the confirmation
            string restartPath = null;
            if (install != null && !AskCloseTrueTest(install, "Copy package", out restartPath)) return;
            if (confirm.UnblockFirst) await UnblockFolders(picks?.Select(p => Path.Combine(source, p)).ToArray() ?? new[] { source });

            Log($"Copy package {source} → {install}" + (db != null ? $" + {db}" : ""), LogLevel.Info);
            var backup = new Backup("Package " + name, source, install, db);
            if (excluded.Count > 0) Log($"{excluded.Count} file(s) unticked, left as they are.", LogLevel.Info);
            var r = await Busy(() => Copier.Excluding(excluded, () =>
            {
                var res = Package.Run(source, install, db, ownExe, Log, backup, picks);
                if (vigValue != null)
                {
                    Log($"Vignetting → {vigValue} in the copied INIs" + (vigDx ? " (DX included)" : " (DX left as is)"), LogLevel.Info);
                    Vignetting.ApplyToCopied(res, vigValue, vigDx, Log);
                }
                return res;
            }));
            backup.Report(Log);
            LogCopyResult(r);
            ShowCopyResult("Copy package " + name, where, r, backup);
            OfferRestart(restartPath);
            LoadInstalls();
        }

        // The confirmation before a copy: CopyResultWindow in preview mode. Returns it once Copy was
        // clicked (Excluded, UnblockFirst), or null when not confirmed.
        async Task<CopyResultWindow> ConfirmCopy(string title, string where, Copier.Result preview, bool offerUnblock = false)
        {
            var w = new CopyResultWindow(title, where, preview, null, preview: true);
            if (offerUnblock) w.OfferUnblock(CopiedSources(preview));
            return await Confirm(w) ? w : null;
        }

        // The pending confirmation. Not modal (the panel stays usable), and only one at a time: the
        // latest copy action - another Copy all button, or opening a folder / several folders / a
        // package - closes the pending one (its copy then never happens) and shows its own. Shown and
        // hidden with the panel, like the results window.
        CopyResultWindow currentConfirm;

        // True = Copy clicked; false = Cancel / ✕ / Esc, or replaced by a newer confirmation.
        Task<bool> Confirm(CopyResultWindow w)
        {
            CancelConfirm();
            var done = new TaskCompletionSource<bool>();
            w.Owner = this;
            w.Closed += (s, e) => { if (currentConfirm == w) currentConfirm = null; done.TrySetResult(w.Confirmed); };
            w.Deactivated += (s, e) => AutoHide();      // clicking away hides it with the panel, still pending
            currentConfirm = w;
            if (IsVisible) w.Show();            // else it appears with the panel (ShowNear)
            return done.Task;
        }

        void CancelConfirm()
        {
            var c = currentConfirm;
            currentConfirm = null;
            c?.Close();
        }

        // If TrueTest runs, ask whether to close it first: always, as a popup. (1.9.1 removed the
        // "Close TrueTest before copying" checkbox that gated this question: Chau prefers being asked.)
        // False = the user cancelled. restartPath = the TrueTest that was closed, or null.
        // copyTo: the install being copied into, shown under the question.
        bool AskCloseTrueTest(string copyTo, string title, out string restartPath)
        {
            restartPath = null;
            var p = TrueTest.FindRunning();
            if (p == null) return true;
            string path = TrueTest.PathOf(p);
            // 0 = close it, 1 = copy anyway, 2 / closed = cancel the copy.
            int answer = Modal(() => AskWindow.Ask(this, title, "TrueTest is running. Close it before copying?",
                $"Running:  {path ?? "(path unknown)"}\nCopy to:  {copyTo}", "Close TrueTest", "Copy without closing", "Cancel"));
            if (answer != 0 && answer != 1) return false;
            if (answer == 0)
            {
                Log("Closing TrueTest…", LogLevel.Info);
                try { p.Kill(); p.WaitForExit(10000); restartPath = path; Log("TrueTest closed.", LogLevel.Ok); }
                catch (Exception ex) { Log("Could not close TrueTest: " + ex.Message, LogLevel.Error); }
            }
            return true;
        }

        void LogCopyResult(Copier.Result r)
        {
            if (r == null) return;
            Log($"Done: {r.Copied} copied, {r.Failed} failed, {r.Skipped} skipped.", r.Failed == 0 ? LogLevel.Ok : LogLevel.Error);
            if (r.AccessDenied && !IsElevated())
                Log("Access denied writing to Program Files - use \"Run as admin\".", LogLevel.Warn);
        }

        void OfferRestart(string restartPath)
        {
            if (restartPath == null ||
                Modal(() => AskWindow.Ask(this, "TrueTest", "Copy finished. Restart TrueTest?", restartPath, "Restart", "Not now")) != 0) return;
            try
            {
                Process.Start(new ProcessStartInfo(restartPath) { WorkingDirectory = Path.GetDirectoryName(restartPath), UseShellExecute = true });
                Log("TrueTest restarted.", LogLevel.Ok);
            }
            catch (Exception ex) { Log("Restart failed: " + ex.Message, LogLevel.Error); }
        }

        // ---------------------------------------------------------------- kits → new TrueTest

        void Migrate_Click(object sender, RoutedEventArgs e) => ShowMigrate();

        // Also opened from the floating icon's menu.
        public async void ShowMigrate()
        {
            if (!IsVisible) ShowNear(Anchor);
            var w = new MigrateWindow(Install) { Owner = this };
            if (Modal(() => w.ShowDialog()) != true) return;
            string from = w.From, to = w.To;
            var targets = w.Targets;
            bool skipEdt = w.SkipEdtMobile;
            // Confirmation of the actual files, like every other copy, with the same Unblock and
            // Vignetting options.
            var preview = KitMigration.Run(from, to, targets, skipEdt, (s, l) => { }, null, dryRun: true);
            var confirm = new CopyResultWindow("Copy kits from " + Path.GetFileName(from), $"{from}\n→ {to}", preview, null, preview: true);
            confirm.OfferVignetting(Vignetting.CopiedInis(preview));
            confirm.OfferUnblock(CopiedSources(preview));
            if (!await Confirm(confirm)) return;
            var excluded = confirm.Excluded;
            string vigValue = confirm.VignettingValue;
            bool vigDx = confirm.VignettingIncludeDx;
            if (!AskCloseTrueTest(to, "Copy kits", out string restartPath)) return;
            if (confirm.UnblockFirst)
                await UnblockFolders(targets.SelectMany(t => t.SubFolders).Select(s => Path.Combine(from, s)).Where(Directory.Exists).ToArray());

            Log($"Copy kits {from} → {to} [{string.Join(", ", targets.SelectMany(t => t.SubFolders))}]"
                + (skipEdt && targets.Any(t => t.Name == "LGDVHCustomApp") ? $", skipping {KitMigration.EdtMobile}" : ""), LogLevel.Info);
            var backup = new Backup("Kits from " + Path.GetFileName(from), from, to, null);
            if (excluded.Count > 0) Log($"{excluded.Count} file(s) unticked, left as they are.", LogLevel.Info);
            var r = await Busy(() => Copier.Excluding(excluded, () =>
            {
                var res = KitMigration.Run(from, to, targets, skipEdt, Log, backup);
                if (vigValue != null)
                {
                    Log($"Vignetting → {vigValue} in the copied INIs" + (vigDx ? " (DX included)" : " (DX left as is)"), LogLevel.Info);
                    Vignetting.ApplyToCopied(res, vigValue, vigDx, Log);
                }
                return res;
            }));
            backup.Report(Log);
            LogCopyResult(r);
            ShowCopyResult("Copy kits from " + Path.GetFileName(from), $"{from}\n→ {to}", r, backup);
            OfferRestart(restartPath);
            LoadInstalls();
        }

        void VigOn_Click(object sender, RoutedEventArgs e) => DoVignetting("1");
        void VigOff_Click(object sender, RoutedEventArgs e) => DoVignetting("0");

        async void DoVignetting(string value)
        {
            if (!CheckSource()) return;
            bool dx = ChkDx.IsChecked == true;
            var inis = KitInis().Select(f => (path: f, name: IniName(f))).ToList();
            Log($"Set {Vignetting.Key} = {value}{(dx ? " (including DX)" : "")}", LogLevel.Info);
            await Busy(() =>
            {
                int changed = 0;
                foreach (var (f, name) in inis)
                {
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
            await UnblockFolders(new[] { Source });
        }

        // Unblock button, and before a copy when its confirmation's Unblock option is ticked.
        async Task UnblockFolders(string[] folders)
        {
            Log($"Unblock {string.Join(" + ", folders)}", LogLevel.Info);
            var r = await Busy(() =>
            {
                int un = 0, total = 0, failed = 0;
                foreach (string f in folders.Where(Directory.Exists))
                {
                    var one = Unblocker.Run(f, Log);
                    un += one.unblocked; total += one.total; failed += one.failed;
                }
                return (unblocked: un, total, failed);
            });
            // "0 unblocked" read like a failure when the files simply were not marked.
            if (r.failed > 0)
                Log($"Done: {r.unblocked} unblocked, {r.failed} failed, of {r.total} file(s).", LogLevel.Error);
            else if (r.total == 0)
                Log("Nothing to unblock: the folder has no files.", LogLevel.Ok);
            else if (r.unblocked == 0)
                Log($"Nothing to unblock: none of the {r.total} file(s) is marked as downloaded from the internet.", LogLevel.Ok);
            else
                Log($"Done: unblocked {r.unblocked} of {r.total} file(s); the rest were not marked.", LogLevel.Ok);
        }

        async void Run_Click(object sender, RoutedEventArgs e)
        {
            string input = RunInputPath, workDir = CboWorkDir.Text.Trim();
            bool puc = RbPuc.IsChecked == true, pocb = RbPocb.IsChecked == true;
            if (!File.Exists(input)) { Log("Run test: input file not found - " + input, LogLevel.Error); return; }
            if (pocb && Install == null) { Log("Run test: pick a TrueTest install for POCBMobile.", LogLevel.Error); return; }
            string pocbDb = PocbDatabase, pocbModel = PocbModel, pocbPid = TxtPocbPid.Text.Trim();
            if (pocb && !Directory.Exists(pocbDb)) { Log("Run test: database folder not found - " + pocbDb, LogLevel.Error); return; }
            if (pocb && pocbModel == null) { Log("Run test: pick the model (folder in " + Path.Combine(pocbDb, "InputIMG") + ").", LogLevel.Error); return; }
            if (pocb && pocbPid == "") { Log("Run test: type the panel ID (TEMP by default).", LogLevel.Error); return; }
            if (!pocb && !Directory.Exists(workDir)) { Log("Run test: working dir not found - " + workDir, LogLevel.Error); return; }
            string install = Install;
            SaveSettings();

            BtnStop.IsEnabled = true;
            int code = await Busy(() =>
            {
                Log($"Run test ({(pocb ? "POCBMobile" : puc ? "PUC" : "Omit")}) {Path.GetFileName(input)} in {(pocb ? Path.Combine(install, "POCBMobile") : workDir)}", LogLevel.Info);
                var psi = pocb ? RunTest.PreparePocbMobile(input, install, pocbDb, pocbModel, pocbPid, Log) : RunTest.Parse(input, workDir, puc, Log);
                Log($"> {psi.FileName} {psi.Arguments}", LogLevel.Info);
                using (var p = new Process { StartInfo = psi })
                {
                    p.OutputDataReceived += (s, ev) => { if (ev.Data != null) Log(ev.Data, LogLevel.Detail); };
                    p.ErrorDataReceived += (s, ev) => { if (ev.Data != null) Log(ev.Data, LogLevel.Warn); };
                    p.Start();
                    if (psi.RedirectStandardInput) p.StandardInput.Close();   // answers "Press Enter to exit"
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
        // Same folder: keep a multi-folder pick.
        void Refresh_Click(object sender, RoutedEventArgs e) =>
            SetSource(CboSource.Text.Trim(), string.Equals(CboSource.Text.Trim(), appliedSource, StringComparison.OrdinalIgnoreCase) ? picked : null);
        void Rescan_Click(object sender, RoutedEventArgs e) => LoadInstalls();

        void BrowseRunInput_Click(object sender, RoutedEventArgs e)
        {
            var d = new Microsoft.Win32.OpenFileDialog { Filter = "Run input (*.txt)|*.txt|All files|*.*" };
            string cur = RunInputPath;
            d.InitialDirectory = File.Exists(cur) ? Path.GetDirectoryName(cur) : Source;
            if (Modal(() => d.ShowDialog(this)) == true) { CboRunInput.Text = d.FileName; OnRunInputChanged(); }
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

        void Backups_Click(object sender, RoutedEventArgs e)
        {
            string dir = Backup.BaseDir;
            if (!Directory.Exists(dir)) { Log("No backups yet: " + dir + " is created by the first copy that overwrites a file.", LogLevel.Info); return; }
            Process.Start("explorer.exe", "\"" + dir + "\"");
        }

        // Every start: put "Open in Kit Helper" in place, or point it back at the installed copy.
        // Running the exe once is therefore all a PC needs. A Remove is deliberately NOT
        // remembered (Chau's call): it lasts until Kit Helper next starts, so to be rid of the
        // entry, remove it and stop starting the app. Returns the icon's note, or null.
        public string EnsureRightClickEntry() =>
            MenuPointsAtInstalled() ? null : ToggleExplorerMenu("add");

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
                if (action == "remove")
                {
                    foreach (var key in ExplorerKeys) Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(key, false);
                    summary = "Removed \"Open in Kit Helper\" from the folder right-click menu. It comes back the next time Kit Helper starts.";
                }
                else
                {
                    RegisterMenu();
                    summary = "Added \"Open in Kit Helper\" to the folder right-click menu.";
                }
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
            public LogLevel Level;
            public Brush Brush { get; set; }
            public override string ToString() => Text;   // what UI Automation / screen readers see
        }

        Brush Brush(string key) => (Brush)FindResource(key);

        // Every line, from any thread, goes through one FIFO queue. Before, worker-thread lines
        // were posted to the UI thread while UI-thread lines were added at once, so a "Done"
        // written right after an await could land above the last few "copied" lines.
        readonly System.Collections.Concurrent.ConcurrentQueue<LogLine> pendingLog = new System.Collections.Concurrent.ConcurrentQueue<LogLine>();
        int flushScheduled;

        void Log(string text, LogLevel level)
        {
            pendingLog.Enqueue(new LogLine { Text = $"{DateTime.Now:HH:mm:ss}  {text}", Level = level });
            if (Dispatcher.CheckAccess()) FlushLog();     // drains anything queued before this line too
            else if (System.Threading.Interlocked.Exchange(ref flushScheduled, 1) == 0)
                Dispatcher.BeginInvoke(new Action(FlushLog));
        }

        void FlushLog()
        {
            System.Threading.Interlocked.Exchange(ref flushScheduled, 0);
            bool any = false;
            while (pendingLog.TryDequeue(out var line))
            {
                line.Brush = Brush(line.Level == LogLevel.Detail ? "Dim" : line.Level == LogLevel.Ok ? "Ok"
                                 : line.Level == LogLevel.Warn ? "Warn" : line.Level == LogLevel.Error ? "Err" : "Fg");
                log.Add(line);
                any = true;
            }
            if (!any) return;
            while (log.Count > 5000) log.RemoveAt(0);
            // Down to the newest line only. ScrollIntoView also scrolled SIDEWAYS to the end of
            // a long line (a PUC command line), hiding the start of every other line.
            if (logScroller == null) logScroller = FindChild<ScrollViewer>(LstLog);
            if (logScroller != null) logScroller.ScrollToBottom();
            else LstLog.ScrollIntoView(log[log.Count - 1]);   // template not applied yet
        }

        ScrollViewer logScroller;

        static T FindChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var c = VisualTreeHelper.GetChild(parent, i);
                if (c is T t) return t;
                var deeper = FindChild<T>(c);
                if (deeper != null) return deeper;
            }
            return null;
        }

        void CopyLog_Click(object sender, RoutedEventArgs e)
        {
            try { Clipboard.SetText(string.Join(Environment.NewLine, log.Select(l => l.Text))); } catch { }
        }

        void ClearLog_Click(object sender, RoutedEventArgs e) => log.Clear();
    }
}
