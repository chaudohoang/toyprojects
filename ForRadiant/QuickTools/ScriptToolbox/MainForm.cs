using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ScriptToolbox
{
    // All controls are built in code (no designer file) so the layout is readable in one place.
    public sealed class MainForm : Form
    {
        readonly TextBox txtSource = new TextBox();
        readonly Label lblSourceInfo = new Label { AutoSize = true, ForeColor = Color.DimGray };
        readonly ComboBox cboInstall = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        readonly Label lblRunning = new Label { AutoSize = true };
        readonly CheckBox chkCloseTT = new CheckBox { Text = "Close TrueTest before copying (offer restart after)", AutoSize = true, Checked = true };
        readonly ListView lvIni = new ListView { View = View.Details, FullRowSelect = true, GridLines = true, HeaderStyle = ColumnHeaderStyle.Nonclickable };
        readonly CheckBox chkDx = new CheckBox { Text = "Include DX ini", AutoSize = true };
        readonly TextBox txtRunInput = new TextBox();
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
            Text = "Script Toolbox" + (IsElevated() ? " (Administrator)" : "");
            Font = new Font("Segoe UI", 9f);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(980, 760);
            MinimumSize = new Size(820, 600);
            StartPosition = FormStartPosition.CenterScreen;
            AllowDrop = true;
            DragEnter += (s, e) => e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            DragDrop += OnDrop;

            BuildLayout();

            // Folder: command-line arg > the exe's own folder (the old tools always ran on the
            // folder they sat in, so dropping this exe into a package folder keeps that habit).
            string folder = !string.IsNullOrEmpty(startFolder) && Directory.Exists(startFolder)
                ? startFolder : AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            SetSource(folder);
            txtRunInput.Text = settings.Get("RunInput");
            chkCloseTT.Checked = settings.Get("CloseTrueTest") != "0";
            LoadInstalls();

            Shown += (s, e) => txtSource.Select(txtSource.TextLength, 0);
            FormClosing += (s, e) => SaveSettings();
            Log($"Script Toolbox ready. Drop a folder here to work on it, or a .txt to use as the run-test input.", LogLevel.Info);
        }

        // ---------------------------------------------------------------- layout

        void BuildLayout()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(8) };
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
            top.Controls.Add(Lbl("Package folder:"), 0, 0);
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
                cs.Controls.Add(b);
            }
            cs.Controls.Add(chkCloseTT);
            cs.Controls.Add(Hint("POCB skips *.tif, *.csv and LGD_VNTT.ini.\nOther targets copy every file in the folder."));
            col.Controls.Add(Group("Copy package folder to TrueTest", cs));

            var us = Stack();
            var ub = Btn("Unblock package folder", DoUnblock);
            ub.AutoSize = false; ub.Height = 30; ub.Width = 330; ub.TextAlign = ContentAlignment.MiddleLeft;
            actionControls.Add(ub);
            us.Controls.Add(ub);
            us.Controls.Add(Hint("Removes the \"downloaded from internet\" mark,\nincluding files in subfolders."));
            col.Controls.Add(Group("Unblock files", us));
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
            col.Controls.Add(Group("Vignetting  (" + Vignetting.Key + " in package *.ini)", vs));

            var g = Grid(3);
            g.ColumnStyles[1] = new ColumnStyle(SizeType.Absolute, 340);
            txtRunInput.Dock = DockStyle.Fill;
            cboWorkDir.Dock = DockStyle.Fill;
            g.Controls.Add(Lbl("Input .txt:"), 0, 0);
            g.Controls.Add(txtRunInput, 1, 0);
            g.Controls.Add(Btn("Browse…", BrowseRunInput), 2, 0);
            g.Controls.Add(Lbl("Working dir:"), 0, 1);
            g.Controls.Add(cboWorkDir, 1, 1);
            var buttons = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0) };
            btnRun.Click += (s, e) => DoRunTest();
            btnStop.Click += (s, e) => { try { runningTest?.Kill(); } catch { } };
            buttons.Controls.Add(btnRun); buttons.Controls.Add(btnStop);
            g.Controls.Add(buttons, 1, 2);
            actionControls.Add(btnRun);
            col.Controls.Add(Group("Run test  (RunTestOmit input file)", g));
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
            int files = Directory.GetFiles(folder).Count(f => !string.Equals(Path.GetFileName(f), ownExe, StringComparison.OrdinalIgnoreCase));
            int dirs = Directory.GetDirectories(folder).Length;
            int inis = Vignetting.IniFiles(folder).Length;
            lblSourceInfo.Text = $"{files} file(s), {dirs} subfolder(s), {inis} ini";
            lblSourceInfo.ForeColor = Color.DimGray;
            RefreshIni();
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
            settings.Set("RunInput", txtRunInput.Text);
            settings.Set("RunWorkDir", cboWorkDir.Text);
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
            string input = txtRunInput.Text.Trim(), workDir = cboWorkDir.Text.Trim();
            if (!File.Exists(input)) { Log("Run test: input file not found - " + input, LogLevel.Error); return; }
            if (!Directory.Exists(workDir)) { Log("Run test: working dir not found - " + workDir, LogLevel.Error); return; }
            SaveSettings();

            btnStop.Enabled = true;
            int code = await Busy(() =>
            {
                var psi = RunTest.Parse(input, workDir, Log);
                Log($"Running in {workDir}", LogLevel.Info);
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
            Log("Package folder not found: " + Source, LogLevel.Error);
            return false;
        }

        void BrowseSource()
        {
            using (var d = new FolderBrowserDialog { SelectedPath = Source, Description = "Package folder (the files to copy / edit)" })
                if (d.ShowDialog(this) == DialogResult.OK) SetSource(d.SelectedPath);
        }

        void BrowseRunInput()
        {
            using (var d = new OpenFileDialog { Filter = "Run input (*.txt)|*.txt|All files|*.*" })
            {
                string cur = txtRunInput.Text.Trim();
                d.InitialDirectory = File.Exists(cur) ? Path.GetDirectoryName(cur) : Source;
                if (d.ShowDialog(this) == DialogResult.OK) txtRunInput.Text = d.FileName;
            }
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            var paths = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (paths == null || paths.Length == 0) return;
            string p = paths[0];
            if (Directory.Exists(p)) SetSource(p);
            else if (string.Equals(Path.GetExtension(p), ".txt", StringComparison.OrdinalIgnoreCase)) txtRunInput.Text = p;
            else if (File.Exists(p)) SetSource(Path.GetDirectoryName(p));
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
                Process.Start(new ProcessStartInfo(Application.ExecutablePath, "\"" + Source + "\"") { UseShellExecute = true, Verb = "runas" });
                Close();
            }
            catch (Exception ex) { Log("Not elevated: " + ex.Message, LogLevel.Warn); }   // UAC declined
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

    // key=value lines in %APPDATA%\ScriptToolbox\settings.txt
    sealed class Settings
    {
        static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ScriptToolbox", "settings.txt");
        readonly Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                if (File.Exists(FilePath))
                    foreach (string line in File.ReadAllLines(FilePath))
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
