using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KitHelper
{
    // All controls are built in code (no designer file) so the layout is readable in one place.
    public sealed class MainForm : Form
    {
        // Kit folder, with the recently used ones in the drop-down.
        readonly ComboBox txtSource = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown, MaxDropDownItems = 15 };
        readonly Label lblSourceInfo = new Label { AutoSize = true, ForeColor = Color.DimGray };
        readonly Dictionary<CopyTarget, Button> copyButtons = new Dictionary<CopyTarget, Button>();
        readonly Label lblSuggest = new Label { AutoSize = true, ForeColor = Color.SteelBlue, Margin = new Padding(3, 2, 3, 3) };
        readonly Button btnExplorer = new Button { AutoSize = true, Margin = new Padding(3) };
        readonly Label lblInstalled = new Label { AutoSize = true, Margin = new Padding(3, 2, 3, 3) };
        readonly RadioButton rbOmit = new RadioButton { Text = "Omit", AutoSize = true, Checked = true };
        readonly RadioButton rbPuc = new RadioButton { Text = "PUC", AutoSize = true };
        readonly ComboBox cboInstall = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        readonly Label lblRunning = new Label { AutoSize = true };
        readonly CheckBox chkCloseTT = new CheckBox { Text = "Close TrueTest before copying (offer restart after)", AutoSize = true, Checked = true };
        readonly ListView lvIni = new ListView { View = View.Details, FullRowSelect = true, GridLines = true, HeaderStyle = ColumnHeaderStyle.Nonclickable };
        readonly CheckBox chkDx = new CheckBox { Text = "Include DX ini", AutoSize = true };
        readonly ComboBox txtRunInput = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
        readonly ComboBox cboWorkDir = new ComboBox { DropDownStyle = ComboBoxStyle.DropDown };
        readonly Button btnRun = new Button { Text = "Run", AutoSize = true };
        readonly Button btnStop = new Button { Text = "Stop", AutoSize = true, Enabled = false };
        readonly RichTextBox rtbLog = new RichTextBox { ReadOnly = true, Font = new Font("Consolas", 9f), BackColor = Color.White, WordWrap = false, DetectUrls = false };
        readonly List<Control> actionControls = new List<Control>();

        readonly Settings settings = Settings.Load();
        readonly string ownExe = Path.GetFileName(Application.ExecutablePath);
        Process runningTest;

        public MainForm(string startFolder)
        {
            Text = $"Kit Helper {InstalledCopy.Self}" + (IsElevated() ? " (Administrator)" : "");
            Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(980, 840);
            MinimumSize = new Size(900, 700);
            StartPosition = FormStartPosition.CenterScreen;
            AllowDrop = true;
            DragEnter += (s, e) => e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            DragDrop += OnDrop;

            BuildLayout();

            // Folder: command-line arg (another copy passes the folder it sits in) > the most
            // recent folder when this is the installed copy > this exe's own folder.
            string recent = settings.Get("Recent").Split('|').FirstOrDefault(Directory.Exists);
            string folder = !string.IsNullOrEmpty(startFolder) && Directory.Exists(startFolder) ? startFolder
                : InstalledCopy.IsStatic(InstalledCopy.SelfExe) && recent != null ? recent
                : AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            chkCloseTT.Checked = settings.Get("CloseTrueTest") != "0";
            LoadInstalls();
            SetSource(folder);
            UpdateExplorerButton();

            Shown += (s, e) =>
            {
                txtSource.Select(txtSource.Text.Length, 0);
                MigrateFromScriptToolbox();
            };
            FormClosing += (s, e) => SaveSettings();
            Log($"Kit Helper ready. Drop a folder here to work on it, or a .txt to use as the run-test input.", LogLevel.Info);
        }

        // ---------------------------------------------------------------- layout

        void BuildLayout()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
            // Without an explicit 100% column the root sizes to its content, and a long folder
            // path in the combo then widens the whole layout past the window edge.
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(root);

            // --- top: folder + TrueTest install
            var top = Grid(5);
            top.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 100);
            txtSource.Dock = DockStyle.Fill;
            txtSource.Leave += (s, e) => SetSource(txtSource.Text.Trim());
            txtSource.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { SetSource(txtSource.Text.Trim()); e.SuppressKeyPress = true; } };
            txtSource.SelectionChangeCommitted += (s, e) => { if (txtSource.SelectedItem is string f) BeginInvoke(new Action(() => SetSource(f))); };
            top.Controls.Add(Lbl("Kit folder:"), 0, 0);
            top.Controls.Add(txtSource, 1, 0);
            top.Controls.Add(Btn("Browse…", BrowseSource), 2, 0);
            top.Controls.Add(Btn("Open", () => { if (Directory.Exists(txtSource.Text)) Process.Start("explorer.exe", "\"" + txtSource.Text + "\""); }), 3, 0);
            top.Controls.Add(Btn("Refresh", () => SetSource(txtSource.Text.Trim())), 4, 0);
            top.Controls.Add(lblSourceInfo, 1, 1);
            top.SetColumnSpan(lblSourceInfo, 4);

            cboInstall.Dock = DockStyle.Fill;
            cboInstall.SelectedIndexChanged += (s, e) => { LoadWorkDirs(); };
            top.Controls.Add(Lbl("TrueTest install:"), 0, 2);
            top.Controls.Add(cboInstall, 1, 2);
            top.Controls.Add(Btn("Rescan", LoadInstalls), 2, 2);
            if (!IsElevated())
            {
                var admin = Btn("Run as admin", RestartElevated);
                top.Controls.Add(admin, 3, 2);
                top.SetColumnSpan(admin, 2);
            }
            top.Controls.Add(lblRunning, 1, 3);
            top.SetColumnSpan(lblRunning, 4);
            root.Controls.Add(top, 0, 0);

            // --- middle: two columns of tool groups
            var mid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, AutoSize = true };
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42));
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 58));
            mid.Controls.Add(LeftColumn(), 0, 0);
            mid.Controls.Add(RightColumn(), 1, 0);
            root.Controls.Add(mid, 0, 1);

            // --- bottom: log
            var logBox = new GroupBox { Text = "Log", Dock = DockStyle.Fill, Padding = new Padding(6) };
            rtbLog.Dock = DockStyle.Fill;
            var clear = new LinkLabel { Text = "Clear", AutoSize = true, Dock = DockStyle.Bottom, TextAlign = ContentAlignment.MiddleRight };
            clear.LinkClicked += (s, e) => rtbLog.Clear();
            logBox.Controls.Add(rtbLog);
            logBox.Controls.Add(clear);
            root.Controls.Add(logBox, 0, 2);
        }

        Control LeftColumn()
        {
            var col = Stack();

            var cs = Stack();
            foreach (var t in CopyTarget.All)
            {
                var target = t;
                string label = "Copy all → " + string.Join(" + ", t.SubFolders) + (t.IncludeSubdirs ? "  (with subfolders)" : "");
                var b = Btn(label, () => DoCopy(target));
                b.AutoSize = false; b.Height = 30; b.Width = 330; b.TextAlign = ContentAlignment.MiddleLeft;
                actionControls.Add(b);
                copyButtons[t] = b;
                cs.Controls.Add(b);
            }
            cs.Controls.Add(lblSuggest);
            cs.Controls.Add(chkCloseTT);
            cs.Controls.Add(Hint("POCB skips *.tif, *.csv and LGD_VNTT.ini.\nOther targets copy every file in the folder."));
            col.Controls.Add(Group("Copy kit folder to TrueTest", cs));

            var us = Stack();
            var ub = Btn("Unblock kit folder", DoUnblock);
            ub.AutoSize = false; ub.Height = 30; ub.Width = 330; ub.TextAlign = ContentAlignment.MiddleLeft;
            actionControls.Add(ub);
            us.Controls.Add(ub);
            us.Controls.Add(Hint("Removes the \"downloaded from internet\" mark,\nincluding files in subfolders."));
            btnExplorer.AutoSize = false; btnExplorer.Height = 30; btnExplorer.Width = 330; btnExplorer.TextAlign = ContentAlignment.MiddleLeft;
            btnExplorer.Margin = new Padding(3, 10, 3, 3);
            btnExplorer.Click += (s, e) => ToggleExplorerMenu();
            us.Controls.Add(btnExplorer);
            us.Controls.Add(Hint("Copies this exe to " + InstalledCopy.Folder + " first; the right-click\nentry runs that copy. Windows 11: under \"Show more options\"."));
            us.Controls.Add(lblInstalled);
            col.Controls.Add(Group("Other", us));
            return col;
        }

        Control RightColumn()
        {
            var col = Stack();
            col.Dock = DockStyle.Fill;

            var vs = Stack();
            lvIni.Columns.Add("INI file", 360);
            lvIni.Columns.Add("Value", 70);
            lvIni.Width = 500; lvIni.Height = 150;
            vs.Controls.Add(lvIni);
            var row = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            var on = Btn("Set ON (1)", () => DoVignetting("1"));
            var off = Btn("Set OFF (0)", () => DoVignetting("0"));
            actionControls.Add(on); actionControls.Add(off);
            chkDx.Margin = new Padding(12, 7, 3, 3);
            row.Controls.Add(on); row.Controls.Add(off); row.Controls.Add(chkDx);
            vs.Controls.Add(row);
            col.Controls.Add(Group("Vignetting  (" + Vignetting.Key + " in kit *.ini)", vs));

            var g = Grid(3);
            g.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, 340);
            txtRunInput.Dock = DockStyle.Fill;
            cboWorkDir.Dock = DockStyle.Fill;
            txtRunInput.SelectedIndexChanged += (s, e) => OnRunInputChanged();
            txtRunInput.Leave += (s, e) => OnRunInputChanged();
            g.Controls.Add(Lbl("Input .txt:"), 0, 0);
            g.Controls.Add(txtRunInput, 1, 0);
            g.Controls.Add(Btn("Browse…", BrowseRunInput), 2, 0);
            var modes = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            rbPuc.CheckedChanged += (s, e) => UpdateRunMode();
            modes.Controls.Add(rbOmit); modes.Controls.Add(rbPuc);
            modes.Controls.Add(Hint("PUC runs in the kit folder, outputs to …\\Test\\"));
            g.Controls.Add(Lbl("Mode:"), 0, 1);
            g.Controls.Add(modes, 1, 1);
            g.SetColumnSpan(modes, 2);
            g.Controls.Add(Lbl("Working dir:"), 0, 2);
            g.Controls.Add(cboWorkDir, 1, 2);
            var buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            btnRun.Click += (s, e) => DoRunTest();
            btnStop.Click += (s, e) => { try { runningTest?.Kill(); } catch { } };
            buttons.Controls.Add(btnRun); buttons.Controls.Add(btnStop);
            g.Controls.Add(buttons, 1, 3);
            actionControls.Add(btnRun);
            col.Controls.Add(Group("Run test  (RunTestOmit / RunTestPUC input file)", g));
            return col;
        }

        static TableLayoutPanel Grid(int cols)
        {
            var g = new TableLayoutPanel { ColumnCount = cols, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0) };
            for (int i = 0; i < cols; i++) g.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            return g;
        }
        static FlowLayoutPanel Stack() =>
            new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0) };
        // An AutoSize GroupBox measures its children by their bounds, so the inner panel must
        // NOT be docked - a docked child gives the box a zero preferred size.
        static GroupBox Group(string text, Control inner)
        {
            var g = new GroupBox { Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(8, 4, 8, 4), Margin = new Padding(3, 3, 3, 6) };
            inner.Dock = DockStyle.None;
            inner.Location = new Point(8, 20);
            g.Controls.Add(inner);
            return g;
        }
        static Label Lbl(string text) => new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(3, 7, 3, 3) };
        static Label Hint(string text) => new Label { Text = text, AutoSize = true, ForeColor = Color.DimGray, Margin = new Padding(3, 2, 3, 3) };
        static Button Btn(string text, Action click)
        {
            var b = new Button { Text = text, AutoSize = true, Margin = new Padding(3) };
            b.Click += (s, e) => click();
            return b;
        }

        // ---------------------------------------------------------------- state

        string Source => txtSource.Text.Trim().TrimEnd('\\');
        string Install => cboInstall.SelectedItem as string;

        void SetSource(string folder)
        {
            txtSource.Text = folder;
            if (!Directory.Exists(folder))
            {
                lblSourceInfo.Text = "Folder not found.";
                lblSourceInfo.ForeColor = Color.Firebrick;
                lvIni.Items.Clear();
                return;
            }
            int files = Directory.GetFiles(folder).Count(f => !Kit.IsHelper(f) && !string.Equals(Path.GetFileName(f), ownExe, StringComparison.OrdinalIgnoreCase));
            int dirs = Directory.GetDirectories(folder).Length;
            int inis = Vignetting.IniFiles(folder).Length;
            lblSourceInfo.Text = $"{files} file(s), {dirs} subfolder(s), {inis} ini";
            lblSourceInfo.ForeColor = Color.DimGray;
            RefreshIni();
            RememberRecent(folder);

            // Mark the copy target this folder is meant for.
            var suggested = Kit.Suggest(folder, out string why);
            foreach (var kv in copyButtons)
            {
                bool on = kv.Key == suggested;
                kv.Value.Font = new Font(Font, on ? FontStyle.Bold : FontStyle.Regular);
                kv.Value.BackColor = on ? Color.FromArgb(220, 235, 252) : SystemColors.Control;
                kv.Value.UseVisualStyleBackColor = !on;
            }
            lblSuggest.Text = suggested == null ? "" : $"Suggested: {suggested.Name}  ({why})";

            // Run inputs found in this folder; keep a browsed file from elsewhere if one is set.
            string current = RunInputPath;
            txtRunInput.Items.Clear();
            foreach (string f in Kit.RunInputs(folder)) txtRunInput.Items.Add(new RunInputItem(f));
            var match = txtRunInput.Items.Cast<RunInputItem>().FirstOrDefault(i => string.Equals(i.Path, current, StringComparison.OrdinalIgnoreCase));
            if (match != null) txtRunInput.SelectedItem = match;
            else if (txtRunInput.Items.Count > 0) txtRunInput.SelectedIndex = 0;
            else if (!File.Exists(current)) txtRunInput.Text = "";
            OnRunInputChanged();
        }

        void RememberRecent(string folder)
        {
            var recent = settings.Get("Recent").Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(f => !string.Equals(f, folder, StringComparison.OrdinalIgnoreCase) && Directory.Exists(f))
                .Prepend(folder).Take(15).ToList();
            settings.Set("Recent", string.Join("|", recent));
            int caret = txtSource.SelectionStart;
            txtSource.BeginUpdate();
            txtSource.Items.Clear();
            foreach (var f in recent) txtSource.Items.Add(f);
            txtSource.EndUpdate();
            txtSource.Text = folder;
            txtSource.Select(Math.Min(caret, folder.Length), 0);
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
                if (txtRunInput.SelectedItem is RunInputItem i && txtRunInput.Text == i.ToString()) return i.Path;
                string t = txtRunInput.Text.Trim();
                return t == "" || Path.IsPathRooted(t) ? t : Path.Combine(Source, t);
            }
        }

        string lastRunInput;
        void OnRunInputChanged()
        {
            string p = RunInputPath;
            if (p == lastRunInput) return;
            lastRunInput = p;
            if (File.Exists(p)) rbPuc.Checked = RunTest.LooksLikePuc(p);
            rbOmit.Checked = !rbPuc.Checked;
            UpdateRunMode();
        }

        // PUC runs where its input lives (RunTestPUC.exe had no working-dir choice: it ran in the
        // folder it sat in). Omit runs in the chosen ScratchDustDetect_NY folder.
        void UpdateRunMode()
        {
            if (rbPuc.Checked)
            {
                cboWorkDir.Enabled = false;
                // Clear the list first: a combo whose text matched an item re-selects that item
                // when its handle is created, overwriting a Text set before the form was shown.
                cboWorkDir.Items.Clear();
                string p = RunInputPath;
                cboWorkDir.Text = File.Exists(p) ? Path.GetDirectoryName(p) : Source;
            }
            else if (!cboWorkDir.Enabled)
            {
                cboWorkDir.Enabled = true;
                cboWorkDir.Text = "";
                LoadWorkDirs();
            }
        }

        void RefreshIni()
        {
            lvIni.BeginUpdate();
            lvIni.Items.Clear();
            foreach (string f in Vignetting.IniFiles(Source))
            {
                string v;
                try { v = Vignetting.Read(f); } catch (Exception ex) { v = "error: " + ex.Message; }
                if (v == null) continue;     // ini without the key: not a vignetting ini
                var it = new ListViewItem(new[] { Path.GetFileName(f), v });
                if (Vignetting.IsDx(f)) it.ForeColor = Color.DimGray;
                lvIni.Items.Add(it);
            }
            if (lvIni.Items.Count == 0) lvIni.Items.Add(new ListViewItem(new[] { "(no ini with " + Vignetting.Key + ")", "" }) { ForeColor = Color.DimGray });
            lvIni.EndUpdate();
        }

        void LoadInstalls()
        {
            string keep = Install ?? settings.Get("Install");
            var installs = TrueTest.Installs();
            cboInstall.Items.Clear();
            foreach (var i in installs) cboInstall.Items.Add(i);

            // Prefer the install TrueTest is currently running from, then the last one used.
            var p = TrueTest.FindRunning();
            string runningPath = p == null ? null : TrueTest.PathOf(p);
            string runningInstall = runningPath == null ? null
                : installs.FirstOrDefault(i => runningPath.StartsWith(i + "\\", StringComparison.OrdinalIgnoreCase));
            lblRunning.Text = p == null ? "TrueTest is not running."
                : "TrueTest is running" + (runningPath != null ? ": " + runningPath : " (path hidden - not elevated)");
            lblRunning.ForeColor = p == null ? Color.DimGray : Color.DarkOrange;

            string pick = runningInstall ?? installs.FirstOrDefault(i => string.Equals(i, keep, StringComparison.OrdinalIgnoreCase));
            if (pick != null) cboInstall.SelectedItem = pick;
            else if (cboInstall.Items.Count > 0) cboInstall.SelectedIndex = cboInstall.Items.Count - 1;

            if (installs.Count == 0) Log($"No TrueTest install found under {TrueTest.Root}. Copy is unavailable.", LogLevel.Warn);
            LoadWorkDirs();
        }

        void LoadWorkDirs()
        {
            if (rbPuc.Checked) return;      // PUC's working dir is fixed (see UpdateRunMode)
            string keep = cboWorkDir.Text;
            if (string.IsNullOrEmpty(keep)) keep = settings.Get("RunWorkDir");
            cboWorkDir.Items.Clear();
            foreach (var i in TrueTest.Installs()) cboWorkDir.Items.Add(Path.Combine(i, "ScratchDustDetect_NY"));
            cboWorkDir.Items.Add(RunTest.CustomWorkDir);
            // Free text is allowed, so keep whatever was there even if it is not in the list.
            cboWorkDir.Text = !string.IsNullOrEmpty(keep) ? keep
                : Install != null ? Path.Combine(Install, "ScratchDustDetect_NY") : RunTest.CustomWorkDir;
        }

        void SaveSettings()
        {
            settings.Set("Install", Install);
            if (!rbPuc.Checked) settings.Set("RunWorkDir", cboWorkDir.Text);
            settings.Set("CloseTrueTest", chkCloseTT.Checked ? "1" : "0");
            settings.Save();
        }

        // ---------------------------------------------------------------- actions

        async void DoCopy(CopyTarget target)
        {
            if (!CheckSource()) return;
            if (Install == null) { Log("Pick a TrueTest install first.", LogLevel.Error); return; }
            string install = Install, source = Source;

            string restartPath = null;
            if (chkCloseTT.Checked)
            {
                var p = TrueTest.FindRunning();
                if (p != null)
                {
                    string path = TrueTest.PathOf(p);
                    var answer = MessageBox.Show(this,
                        $"TrueTest is running{(path != null ? ":\n" + path : ".")}\n\nClose it before copying to\n{install}?",
                        "Copy to " + target.Name, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (answer == DialogResult.Cancel) return;
                    if (answer == DialogResult.Yes)
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
                MessageBox.Show(this, "Copy finished. Restart TrueTest?\n\n" + restartPath, "TrueTest",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

        async void DoVignetting(string value)
        {
            if (!CheckSource()) return;
            bool dx = chkDx.Checked;
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

        async void DoUnblock()
        {
            if (!CheckSource()) return;
            string source = Source;
            Log($"Unblock {source}", LogLevel.Info);
            var r = await Busy(() => Unblocker.Run(source, Log));
            Log($"Done: {r.unblocked} unblocked of {r.total} file(s){(r.failed > 0 ? $", {r.failed} failed" : "")}.",
                r.failed == 0 ? LogLevel.Ok : LogLevel.Error);
        }

        async void DoRunTest()
        {
            string input = RunInputPath, workDir = cboWorkDir.Text.Trim();
            bool puc = rbPuc.Checked;
            if (!File.Exists(input)) { Log("Run test: input file not found - " + input, LogLevel.Error); return; }
            if (!Directory.Exists(workDir)) { Log("Run test: working dir not found - " + workDir, LogLevel.Error); return; }
            SaveSettings();

            btnStop.Enabled = true;
            int code = await Busy(() =>
            {
                var psi = RunTest.Parse(input, workDir, puc, Log);
                Log($"Run test ({(puc ? "PUC" : "Omit")}) {Path.GetFileName(input)} in {workDir}", LogLevel.Info);
                Log($"> {psi.FileName} {psi.Arguments}", LogLevel.Info);
                using (var p = new Process { StartInfo = psi })
                {
                    p.OutputDataReceived += (s, e) => { if (e.Data != null) Log(e.Data, LogLevel.Detail); };
                    p.ErrorDataReceived += (s, e) => { if (e.Data != null) Log(e.Data, LogLevel.Warn); };
                    p.Start();
                    runningTest = p;
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();
                    p.WaitForExit();
                    runningTest = null;
                    return p.ExitCode;
                }
            }, -1);
            btnStop.Enabled = false;
            Log(code == 0 ? "Exit code 0 - Succeeded" : $"Error: exit code {code}", code == 0 ? LogLevel.Ok : LogLevel.Error);
        }

        // Runs work off the UI thread with the action buttons disabled. Exceptions are logged.
        async Task<T> Busy<T>(Func<T> work, T onError = default)
        {
            foreach (var c in actionControls) c.Enabled = false;
            UseWaitCursor = true;
            try { return await Task.Run(work); }
            catch (Exception ex) { Log("Error: " + ex.Message, LogLevel.Error); return onError; }
            finally
            {
                foreach (var c in actionControls) c.Enabled = true;
                UseWaitCursor = false;
            }
        }

        bool CheckSource()
        {
            if (Directory.Exists(Source)) return true;
            Log("Kit folder not found: " + Source, LogLevel.Error);
            return false;
        }

        void BrowseSource()
        {
            using (var d = new FolderBrowserDialog { SelectedPath = Source, Description = "Kit folder (the files to copy / edit)" })
                if (d.ShowDialog(this) == DialogResult.OK) SetSource(d.SelectedPath);
        }

        void BrowseRunInput()
        {
            using (var d = new OpenFileDialog { Filter = "Run input (*.txt)|*.txt|All files|*.*" })
            {
                string cur = RunInputPath;
                d.InitialDirectory = File.Exists(cur) ? Path.GetDirectoryName(cur) : Source;
                if (d.ShowDialog(this) == DialogResult.OK) { txtRunInput.Text = d.FileName; OnRunInputChanged(); }
            }
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null || paths.Length == 0) return;
            string p = paths[0];
            if (Directory.Exists(p)) SetSource(p);
            else if (string.Equals(Path.GetExtension(p), ".txt", StringComparison.OrdinalIgnoreCase)) { txtRunInput.Text = p; OnRunInputChanged(); }
            else if (File.Exists(p)) SetSource(Path.GetDirectoryName(p));
        }

        // "Open in Kit Helper" on the folder right-click menu, per user (HKCU, no admin).
        // Directory = right-click a folder; Directory\Background = right-click inside one.
        static readonly string[] ExplorerKeys =
        {
            @"Software\Classes\Directory\shell\KitHelper",
            @"Software\Classes\Directory\Background\shell\KitHelper",
        };

        bool ExplorerMenuInstalled(out string registeredExe)
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
        bool MenuPointsAtInstalled() => ExplorerMenuInstalled(out string exe) && InstalledCopy.IsStatic(exe);

        void UpdateExplorerButton()
        {
            bool registered = ExplorerMenuInstalled(out _);
            btnExplorer.Text = MenuPointsAtInstalled() ? "Remove \"Open in Kit Helper\" from right-click"
                : registered ? "Move right-click entry to " + InstalledCopy.Folder + " (now: other copy)"
                : "Add \"Open in Kit Helper\" to folder right-click";

            var inst = InstalledCopy.Installed;
            var self = InstalledCopy.Self;
            if (inst == null) { lblInstalled.Text = "Installed copy: none yet."; lblInstalled.ForeColor = Color.DimGray; return; }
            int c = inst.CompareTo(self);
            lblInstalled.Text = "Installed copy: " + inst
                + (InstalledCopy.IsStatic(InstalledCopy.SelfExe) ? "  (this window)"
                   : c > 0 ? "  - newer than this copy"
                   : c < 0 ? "  - older; Add/Remove updates it"
                   : "  - same as this copy");
            lblInstalled.ForeColor = c < 0 ? Color.DarkOrange : Color.DimGray;
        }

        // action: "add" / "remove", or null = whichever the button currently offers.
        void ToggleExplorerMenu(string action = null)
        {
            action = action ?? (MenuPointsAtInstalled() ? "remove" : "add");
            try
            {
                // Normally a no-op: this window already is the installed copy (see RunInstalled).
                Log(InstalledCopy.EnsureInstalled(), LogLevel.Info);
                if (action == "remove")
                {
                    foreach (var key in ExplorerKeys) Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree(key, false);
                    Log("Removed \"Open in Kit Helper\" from the Explorer right-click menu.", LogLevel.Ok);
                }
                else
                {
                    RegisterMenu();
                    Log($"Explorer right-click → \"Open in Kit Helper\" now runs {InstalledCopy.StaticExe}", LogLevel.Ok);
                }
            }
            catch (Exception ex) { Log("Explorer menu: " + ex.Message, LogLevel.Error); }
            UpdateExplorerButton();
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
        void MigrateFromScriptToolbox()
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

        void RestartElevated()
        {
            SaveSettings();
            try
            {
                Process.Start(new ProcessStartInfo(Application.ExecutablePath, "\"" + Source + "\" " + Program.NewInstanceFlag) { UseShellExecute = true, Verb = "runas" });
                Close();
            }
            catch (Exception ex) { Log("Not elevated: " + ex.Message, LogLevel.Warn); }   // UAC declined
        }

        // ---------------------------------------------------------------- single instance

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SingleInstance.AllowFromLowerIntegrity(Handle);
        }

        // A second launch sends its folder here (see SingleInstance.ForwardToRunning).
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == SingleInstance.WM_COPYDATA)
            {
                string folder = SingleInstance.Read(ref m);
                if (folder == SingleInstance.QuitToken)
                {
                    // A newer build was started and takes over from this window.
                    m.Result = (IntPtr)1;
                    BeginInvoke(new Action(Close));
                    return;
                }
                if (folder != null)
                {
                    m.Result = (IntPtr)1;
                    // Reply first; the sender is blocked in SendMessage until WndProc returns.
                    BeginInvoke(new Action(() => OpenForwarded(folder)));
                    return;
                }
            }
            base.WndProc(ref m);
        }

        void OpenForwarded(string folder)
        {
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
            Activate();
            if (folder == "") return;                   // plain relaunch: just come forward
            Log($"Opened {folder}", LogLevel.Info);
            LoadInstalls();                             // TrueTest may have started or stopped since
            SetSource(folder.TrimEnd('\\'));
        }

        // ---------------------------------------------------------------- log

        void Log(string text, LogLevel level)
        {
            if (InvokeRequired) { BeginInvoke(new Action(() => Log(text, level))); return; }
            Color c;
            switch (level)
            {
                case LogLevel.Detail: c = Color.DimGray; break;
                case LogLevel.Ok: c = Color.ForestGreen; break;
                case LogLevel.Warn: c = Color.DarkOrange; break;
                case LogLevel.Error: c = Color.Firebrick; break;
                default: c = Color.Black; break;
            }
            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionColor = c;
            rtbLog.AppendText($"{DateTime.Now:HH:mm:ss}  {text}\n");
            rtbLog.ScrollToCaret();
        }
    }

    // key=value lines in %APPDATA%\KitHelper\settings.txt
    sealed class Settings
    {
        static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "KitHelper", "settings.txt");
        readonly Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                // First run after the rename: start from Script Toolbox's settings (left in place).
                string old = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScriptToolbox", "settings.txt");
                string path = File.Exists(FilePath) ? FilePath : File.Exists(old) ? old : null;
                if (path != null)
                    foreach (string line in File.ReadAllLines(path))
                    {
                        int i = line.IndexOf('=');
                        if (i > 0) s.values[line.Substring(0, i)] = line.Substring(i + 1);
                    }
            }
            catch { }
            return s;
        }

        public string Get(string key) => values.TryGetValue(key, out var v) ? v : "";
        public void Set(string key, string value) => values[key] = value ?? "";

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
                File.WriteAllLines(FilePath, values.Select(kv => kv.Key + "=" + kv.Value));
            }
            catch { }
        }
    }
}
