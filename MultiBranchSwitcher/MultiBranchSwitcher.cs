// MultiBranchSwitcher.cs
// Single-file C# WinForms tool with two modes:
//
//   Switch in place  - scan a folder for git repositories and switch them all to one branch.
//   Clone from master - refresh a prepared "master" folder (fetch / prune / force / pull),
//                       copy it to a new folder named after the branch, then switch the copy.
//                       Repos that do not have the branch stay on master and are logged.
//
// Build: run build.bat (csc.exe, .NET Framework 4.x, no Visual Studio needed).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MultiBranchSwitcher
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    #region Process / git helpers

    internal sealed class ProcResult
    {
        public int ExitCode;
        public string StdOut = "";
        public string StdErr = "";
        public bool TimedOut;
        public bool Ok { get { return ExitCode == 0 && !TimedOut; } }

        public string Message
        {
            get
            {
                if (TimedOut) return "timed out";
                string s = (StdErr ?? "").Trim();
                if (s.Length == 0) s = (StdOut ?? "").Trim();
                s = s.Replace("\r", " ").Replace("\n", " ").Trim();
                return s.Length == 0 ? ("exit code " + ExitCode) : s;
            }
        }
    }

    internal static class Proc
    {
        public static ProcResult Run(string exe, string args, string workDir, int timeoutMs, Action<string> log)
        {
            var result = new ProcResult();
            var psi = new ProcessStartInfo(exe, args)
            {
                WorkingDirectory = workDir,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };
            // Never let git block on a credential / SSH prompt.
            psi.EnvironmentVariables["GIT_TERMINAL_PROMPT"] = "0";
            psi.EnvironmentVariables["GCM_INTERACTIVE"] = "never";
            psi.EnvironmentVariables["GIT_ASKPASS"] = "echo";
            psi.EnvironmentVariables["SSH_ASKPASS"] = "echo";

            if (log != null) log("  > " + Path.GetFileNameWithoutExtension(exe) + " " + args);

            var sbOut = new StringBuilder();
            var sbErr = new StringBuilder();

            try
            {
                using (var p = new Process())
                {
                    p.StartInfo = psi;
                    p.OutputDataReceived += (s, e) => { if (e.Data != null) sbOut.AppendLine(e.Data); };
                    p.ErrorDataReceived += (s, e) => { if (e.Data != null) sbErr.AppendLine(e.Data); };
                    p.Start();
                    p.BeginOutputReadLine();
                    p.BeginErrorReadLine();

                    if (!p.WaitForExit(timeoutMs))
                    {
                        result.TimedOut = true;
                        try { p.Kill(); } catch { }
                        try { p.WaitForExit(3000); } catch { }
                        result.ExitCode = -1;
                    }
                    else
                    {
                        result.ExitCode = p.ExitCode;
                    }
                }
            }
            catch (Exception ex)
            {
                result.ExitCode = -1;
                sbErr.AppendLine(ex.Message);
            }

            result.StdOut = sbOut.ToString();
            result.StdErr = sbErr.ToString();
            return result;
        }

        public static string FirstLine(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            using (var sr = new StringReader(s))
            {
                string line = sr.ReadLine();
                return line == null ? "" : line.Trim();
            }
        }

        public static List<string> Lines(string s)
        {
            var list = new List<string>();
            if (string.IsNullOrEmpty(s)) return list;
            using (var sr = new StringReader(s))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (line.Length > 0) list.Add(line);
                }
            }
            return list;
        }
    }

    internal static class Git
    {
        public static string GitExe = "git";

        public static ProcResult Run(string repoDir, string args, int timeoutMs, Action<string> log)
        {
            return Proc.Run(GitExe, args, repoDir, timeoutMs, log);
        }
    }

    internal static class Robo
    {
        public static string RoboExe = "robocopy";

        /// <summary>Copies srcDir into dstDir. Robocopy exit codes below 8 are success.</summary>
        public static ProcResult CopyTree(string srcDir, string dstDir, bool mirror, int timeoutMs, Action<string> log)
        {
            // Trailing backslashes break quoted robocopy arguments - strip them first.
            string src = srcDir.TrimEnd('\\', '/');
            string dst = dstDir.TrimEnd('\\', '/');
            string args = "\"" + src + "\" \"" + dst + "\" " +
                          (mirror ? "/MIR" : "/E") +
                          " /COPY:DAT /DCOPY:DAT /R:1 /W:1 /MT:16 /NFL /NDL /NJH /NJS /NP";
            var r = Proc.Run(RoboExe, args, Path.GetTempPath(), timeoutMs, log);
            // Robocopy: 0-7 = success//copied/extra files, 8+ = failures. -1 means it never started.
            if (!r.TimedOut && r.ExitCode >= 0 && r.ExitCode < 8) r.ExitCode = 0;
            return r;
        }

        /// <summary>Copies only the loose files sitting directly in srcDir (no recursion).</summary>
        public static ProcResult CopyTopLevelFiles(string srcDir, string dstDir, int timeoutMs, Action<string> log)
        {
            string src = srcDir.TrimEnd('\\', '/');
            string dst = dstDir.TrimEnd('\\', '/');
            string args = "\"" + src + "\" \"" + dst + "\" /R:1 /W:1 /NFL /NDL /NJH /NJS /NP";
            var r = Proc.Run(RoboExe, args, Path.GetTempPath(), timeoutMs, log);
            if (!r.TimedOut && r.ExitCode >= 0 && r.ExitCode < 8) r.ExitCode = 0;
            return r;
        }
    }

    #endregion

    #region Settings (simple INI next to the exe)

    internal sealed class Settings
    {
        public bool CloneMode = false;
        public string Root = "";
        public string Dest = "";
        public string Branch = "";
        public int Depth = 2;
        public bool Fetch = true;
        public bool Prune = true;
        public bool CreateTracking = true;
        public bool Force = false;
        public bool Pull = false;
        public bool DryRun = false;
        public bool Suffix = true;
        public bool Overwrite = false;
        public bool LongPaths = true;
        public int WinW = 1280, WinH = 760;
        public bool WinMax = true;
        public string GitExe = "git";

        private static string IniPath
        {
            get
            {
                string dir = Path.GetDirectoryName(Application.ExecutablePath);
                return Path.Combine(dir, "MultiBranchSwitcher.ini");
            }
        }

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                if (!File.Exists(IniPath)) return s;
                foreach (string raw in File.ReadAllLines(IniPath))
                {
                    string line = raw.Trim();
                    if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("[")) continue;
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = line.Substring(0, eq).Trim().ToLowerInvariant();
                    string val = line.Substring(eq + 1).Trim();
                    switch (key)
                    {
                        case "clonemode": s.CloneMode = val == "1"; break;
                        case "root": s.Root = val; break;
                        case "dest": s.Dest = val; break;
                        case "branch": s.Branch = val; break;
                        case "depth": int d; if (int.TryParse(val, out d)) s.Depth = Math.Max(0, Math.Min(6, d)); break;
                        case "fetch": s.Fetch = val == "1"; break;
                        case "prune": s.Prune = val == "1"; break;
                        case "createtracking": s.CreateTracking = val == "1"; break;
                        case "force": s.Force = val == "1"; break;
                        case "pull": s.Pull = val == "1"; break;
                        case "dryrun": s.DryRun = val == "1"; break;
                        case "suffix": s.Suffix = val == "1"; break;
                        case "overwrite": s.Overwrite = val == "1"; break;
                        case "longpaths": s.LongPaths = val == "1"; break;
                        case "winw": int ww; if (int.TryParse(val, out ww) && ww >= 860) s.WinW = ww; break;
                        case "winh": int wh; if (int.TryParse(val, out wh) && wh >= 560) s.WinH = wh; break;
                        case "winmax": s.WinMax = val == "1"; break;
                        case "gitexe": if (val.Length > 0) s.GitExe = val; break;
                    }
                }
            }
            catch { }
            return s;
        }

        public void Save()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("[MultiBranchSwitcher]");
                sb.AppendLine("CloneMode=" + (CloneMode ? "1" : "0"));
                sb.AppendLine("Root=" + Root);
                sb.AppendLine("Dest=" + Dest);
                sb.AppendLine("Branch=" + Branch);
                sb.AppendLine("Depth=" + Depth);
                sb.AppendLine("Fetch=" + (Fetch ? "1" : "0"));
                sb.AppendLine("Prune=" + (Prune ? "1" : "0"));
                sb.AppendLine("CreateTracking=" + (CreateTracking ? "1" : "0"));
                sb.AppendLine("Force=" + (Force ? "1" : "0"));
                sb.AppendLine("Pull=" + (Pull ? "1" : "0"));
                sb.AppendLine("DryRun=" + (DryRun ? "1" : "0"));
                sb.AppendLine("Suffix=" + (Suffix ? "1" : "0"));
                sb.AppendLine("Overwrite=" + (Overwrite ? "1" : "0"));
                sb.AppendLine("LongPaths=" + (LongPaths ? "1" : "0"));
                sb.AppendLine("WinW=" + WinW);
                sb.AppendLine("WinH=" + WinH);
                sb.AppendLine("WinMax=" + (WinMax ? "1" : "0"));
                sb.AppendLine("GitExe=" + GitExe);
                File.WriteAllText(IniPath, sb.ToString(), Encoding.UTF8);
            }
            catch { }
        }
    }

    #endregion

    internal sealed class RunOptions
    {
        public List<string> Branches = new List<string>();
        public bool CloneMode;
        public string DestRoot = "";
        public bool Fetch, Prune, Create, Force, Pull, Dry, Suffix, Overwrite, LongPaths;

        public RunOptions Copy() { return (RunOptions)MemberwiseClone(); }
    }

    internal sealed class MainForm : Form
    {
        // --- controls
        private RadioButton rdSwitch, rdClone;
        private Label lblRoot, lblDest;
        private TextBox txtRoot, txtDest, txtBranch;
        private Button btnBrowse, btnBrowseDest, btnScan, btnRun, btnStop, btnOpenDest;
        private NumericUpDown numDepth;
        private CheckBox chkFetch, chkPrune, chkCreate, chkForce, chkPull, chkDryRun, chkSuffix, chkOverwrite, chkLongPaths;
        private ListView lv;
        private TextBox txtLog;
        private StatusStrip status;
        private ToolStripStatusLabel lblStatus;
        private SplitContainer split;

        // --- state
        private readonly List<string> repos = new List<string>();
        private Thread worker;
        private volatile bool cancelRequested;
        private Settings cfg;
        private string lastAutoDest = "";
        private bool uiReady;
        private bool fittingColumns;
        private int lastFitWidth = -1;

        private const int TimeoutQuick = 30000;
        private const int TimeoutCheckout = 120000;
        private const int TimeoutNetwork = 300000;
        private const int TimeoutCopy = 3600000;

        public MainForm()
        {
            cfg = Settings.Load();
            Git.GitExe = cfg.GitExe;
            BuildUi();
            ApplySettingsToUi();
            uiReady = true;
            UpdateModeUi();
        }

        #region UI construction (no Designer)

        private void BuildUi()
        {
            Text = "Multi Branch Switcher / Cloner";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            MinimumSize = new Size(860, 560);
            Size = new Size(Math.Max(860, cfg.WinW), Math.Max(560, cfg.WinH));

            // Maximizing must wait for the handle: set before it exists and WinForms never records
            // the size above as the restore bounds, so restoring down lands on a tiny default window.
            Load += (s, e) =>
            {
                if (cfg.WinMax) WindowState = FormWindowState.Maximized;
            };

            // The exe icon is embedded by build.bat (/win32icon); reuse it for the window
            // so the title bar, Alt-Tab and taskbar all match.
            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); }
            catch { }
            Font = new Font("Segoe UI", 9f);

            // NOTE: docking is applied in reverse of the Controls collection order, so the
            // Fill control must be added FIRST and the Top/Bottom bars after it.
            var top = new Panel { Dock = DockStyle.Top, Height = 190, Padding = new Padding(10, 8, 10, 4) };

            const int L = 100;   // left edge of the input column

            // Row 0: mode
            var lblMode = new Label { Text = "Mode:", Left = 10, Top = 12, Width = 86, TextAlign = ContentAlignment.MiddleLeft };
            rdSwitch = new RadioButton { Text = "Switch repos in place", Left = L - 4, Top = 9, AutoSize = true, Checked = true };
            rdClone = new RadioButton { Text = "Clone master folder to a new branch folder", Left = L + 150, Top = 9, AutoSize = true };
            rdSwitch.CheckedChanged += (s, e) => UpdateModeUi();
            rdClone.CheckedChanged += (s, e) => UpdateModeUi();

            // Row 1: root / master folder
            lblRoot = new Label { Text = "Root folder:", Left = 10, Top = 44, Width = 86, TextAlign = ContentAlignment.MiddleLeft };
            txtRoot = new TextBox { Left = L, Top = 41, Width = 632, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            btnBrowse = new Button { Text = "Browse...", Left = 738, Top = 40, Width = 80, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            var lblDepth = new Label { Text = "Depth:", Left = 824, Top = 44, Width = 42, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            numDepth = new NumericUpDown { Left = 866, Top = 41, Width = 46, Minimum = 0, Maximum = 6, Value = 2, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnScan = new Button { Text = "Scan", Left = 918, Top = 40, Width = 64, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnBrowse.Click += (s, e) => BrowseRoot();
            btnScan.Click += (s, e) => ScanRepos(true);
            txtRoot.TextChanged += (s, e) => UpdateAutoDest();

            // Row 2: destination folder (clone mode)
            lblDest = new Label { Text = "New folder:", Left = 10, Top = 76, Width = 86, TextAlign = ContentAlignment.MiddleLeft };
            txtDest = new TextBox { Left = L, Top = 73, Width = 632, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            btnBrowseDest = new Button { Text = "Browse...", Left = 738, Top = 72, Width = 80, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnOpenDest = new Button { Text = "Open", Left = 824, Top = 72, Width = 64, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnBrowseDest.Click += (s, e) => BrowseDest();
            btnOpenDest.Click += (s, e) =>
            {
                string p = txtDest.Text.Trim();
                if (Directory.Exists(p)) try { Process.Start("explorer.exe", "\"" + p.TrimEnd('\\') + "\""); } catch { }
            };

            // Row 3: branch
            var lblBranch = new Label { Text = "Branch:", Left = 10, Top = 108, Width = 86, TextAlign = ContentAlignment.MiddleLeft };
            txtBranch = new TextBox { Left = L, Top = 105, Width = 718, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
            btnRun = new Button { Text = "Switch all", Left = 824, Top = 104, Width = 88, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnStop = new Button { Text = "Stop", Left = 918, Top = 104, Width = 64, Enabled = false, Anchor = AnchorStyles.Top | AnchorStyles.Right };
            btnRun.Click += (s, e) => StartRun();
            btnStop.Click += (s, e) => { cancelRequested = true; Log("Stop requested - finishing current repository..."); };
            txtBranch.TextChanged += (s, e) => UpdateAutoDest();
            txtBranch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; StartRun(); }
            };

            // Rows 4-5: options
            // AutoSize + flow layout: labels resize themselves and the rows re-space, so nothing
            // clips or overlaps when the text changes between modes or the DPI/font differs.
            var optMargin = new Padding(0, 3, 20, 3);
            chkFetch = new CheckBox { Text = "Fetch first", AutoSize = true, Margin = optMargin, Checked = true };
            chkPrune = new CheckBox { Text = "Prune", AutoSize = true, Margin = optMargin, Checked = true };
            chkCreate = new CheckBox { Text = "Create local branch from remote", AutoSize = true, Margin = optMargin, Checked = true };
            chkForce = new CheckBox { Text = "Force (discard local changes)", AutoSize = true, Margin = optMargin };
            chkPull = new CheckBox { Text = "Pull --ff-only", AutoSize = true, Margin = optMargin };
            chkDryRun = new CheckBox { Text = "Dry run (check only)", AutoSize = true, Margin = optMargin };
            chkSuffix = new CheckBox { Text = "Match name without folder prefix", AutoSize = true, Margin = optMargin, Checked = true };
            chkOverwrite = new CheckBox { Text = "Overwrite existing destination", AutoSize = true, Margin = optMargin };
            chkLongPaths = new CheckBox { Text = "Enable long paths (core.longpaths)", AutoSize = true, Margin = optMargin, Checked = true };

            var optRow1 = new FlowLayoutPanel
            {
                Left = L - 3, Top = 132, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false, FlowDirection = FlowDirection.LeftToRight, Margin = Padding.Empty
            };
            optRow1.Controls.AddRange(new Control[] { chkFetch, chkPrune, chkCreate, chkForce, chkPull });

            var optRow2 = new FlowLayoutPanel
            {
                Left = L - 3, Top = 158, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
                WrapContents = false, FlowDirection = FlowDirection.LeftToRight, Margin = Padding.Empty
            };
            optRow2.Controls.AddRange(new Control[] { chkDryRun, chkSuffix, chkOverwrite, chkLongPaths });

            top.Controls.AddRange(new Control[]
            {
                lblMode, rdSwitch, rdClone,
                lblRoot, txtRoot, btnBrowse, lblDepth, numDepth, btnScan,
                lblDest, txtDest, btnBrowseDest, btnOpenDest,
                lblBranch, txtBranch, btnRun, btnStop,
                optRow1, optRow2
            });

            // Split: list on top, log at bottom
            split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                Panel1MinSize = 120,
                Panel2MinSize = 80
            };
            Controls.Add(split);
            // SplitterDistance is validated against the current height, so set it once laid out.
            Shown += (s, e) =>
            {
                try { split.SplitterDistance = Math.Max(140, split.Height - 190); }
                catch { }
                FitColumns(true);
            };

            lv = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                MultiSelect = true
            };
            // Starting widths only; FitColumns() reflows them to the window on first show and on
            // every resize. Kept modest so no horizontal scrollbar flashes before that.
            lv.Columns.Add("Repository", 140);
            lv.Columns.Add("Branch", 360);
            lv.Columns.Add("Result", 260);
            lv.Columns.Add("Path", 420);
            lv.DoubleClick += (s, e) => OpenSelectedFolder();
            lv.Resize += (s, e) => FitColumns();
            var ctx = new ContextMenuStrip();
            ctx.Items.Add("Fit columns to window", null, (s, e) => FitColumns(true));
            ctx.Items.Add("Copy result rows", null, (s, e) => CopyRows());
            ctx.Items.Add("Open folder in Explorer", null, (s, e) => OpenSelectedFolder());
            lv.ContextMenuStrip = ctx;
            split.Panel1.Controls.Add(lv);

            txtLog = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 9f),
                BackColor = Color.White
            };
            split.Panel2.Controls.Add(txtLog);

            status = new StatusStrip();
            lblStatus = new ToolStripStatusLabel("Ready");
            status.Items.Add(lblStatus);

            Controls.Add(top);      // added after the Fill control on purpose
            Controls.Add(status);

            FormClosing += (s, e) =>
            {
                if (worker != null && worker.IsAlive)
                {
                    if (MessageBox.Show(this, "An operation is still running. Close anyway?",
                        "Multi Branch Switcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    {
                        e.Cancel = true;
                        return;
                    }
                    cancelRequested = true;
                }
                // RestoreBounds is the un-maximized size, which is what we want to store.
                var bounds = WindowState == FormWindowState.Normal ? Bounds : RestoreBounds;
                cfg.WinMax = WindowState == FormWindowState.Maximized;
                if (bounds.Width >= 860 && bounds.Height >= 560)
                {
                    cfg.WinW = bounds.Width;
                    cfg.WinH = bounds.Height;
                }
                SaveSettingsFromUi();
            };
        }

        private void UpdateModeUi()
        {
            if (!uiReady) return;
            bool clone = rdClone.Checked;
            lblRoot.Text = clone ? "Master folder:" : "Root folder:";
            lblDest.Enabled = clone;
            txtDest.Enabled = clone;
            btnBrowseDest.Enabled = clone;
            btnOpenDest.Enabled = clone;
            chkOverwrite.Enabled = clone;
            chkPull.Text = clone ? "Pull master --ff-only" : "Pull --ff-only";
            chkForce.Text = clone ? "Force (reset master to remote)" : "Force (discard local changes)";
            btnRun.Text = clone ? "Clone + switch" : "Switch all";
            lv.Columns[1].Text = clone ? "Branch in copy" : "Current branch";
            lv.Columns[3].Text = clone ? "Destination path" : "Path";
            UpdateAutoDest();
        }

        private void ApplySettingsToUi()
        {
            rdClone.Checked = cfg.CloneMode;
            rdSwitch.Checked = !cfg.CloneMode;
            txtRoot.Text = cfg.Root;
            txtDest.Text = cfg.Dest;
            txtBranch.Text = cfg.Branch;
            numDepth.Value = Math.Max(numDepth.Minimum, Math.Min(numDepth.Maximum, cfg.Depth));
            chkFetch.Checked = cfg.Fetch;
            chkPrune.Checked = cfg.Prune;
            chkCreate.Checked = cfg.CreateTracking;
            chkForce.Checked = cfg.Force;
            chkPull.Checked = cfg.Pull;
            chkDryRun.Checked = cfg.DryRun;
            chkSuffix.Checked = cfg.Suffix;
            chkOverwrite.Checked = cfg.Overwrite;
            chkLongPaths.Checked = cfg.LongPaths;
        }

        private void SaveSettingsFromUi()
        {
            cfg.CloneMode = rdClone.Checked;
            cfg.Root = txtRoot.Text.Trim();
            cfg.Dest = txtDest.Text.Trim();
            cfg.Branch = txtBranch.Text.Trim();
            cfg.Depth = (int)numDepth.Value;
            cfg.Fetch = chkFetch.Checked;
            cfg.Prune = chkPrune.Checked;
            cfg.CreateTracking = chkCreate.Checked;
            cfg.Force = chkForce.Checked;
            cfg.Pull = chkPull.Checked;
            cfg.DryRun = chkDryRun.Checked;
            cfg.Suffix = chkSuffix.Checked;
            cfg.Overwrite = chkOverwrite.Checked;
            cfg.LongPaths = chkLongPaths.Checked;
            cfg.Save();
        }

        #endregion

        #region Logging / UI helpers

        private void Log(string text)
        {
            if (InvokeRequired) { BeginInvoke((Action<string>)Log, text); return; }
            txtLog.AppendText(DateTime.Now.ToString("HH:mm:ss") + "  " + text + Environment.NewLine);
        }

        private void SetStatus(string text)
        {
            if (InvokeRequired) { BeginInvoke((Action<string>)SetStatus, text); return; }
            lblStatus.Text = text;
        }

        private void SetRowResult(int index, string branch, string result, Color color)
        {
            if (InvokeRequired)
            {
                BeginInvoke((Action<int, string, string, Color>)SetRowResult, index, branch, result, color);
                return;
            }
            if (index < 0 || index >= lv.Items.Count) return;
            var it = lv.Items[index];
            if (branch != null) it.SubItems[1].Text = branch;
            if (result != null) it.SubItems[2].Text = result;
            it.ForeColor = color;
            it.EnsureVisible();
        }

        private void SetRowPath(int index, string path)
        {
            if (InvokeRequired) { BeginInvoke((Action<int, string>)SetRowPath, index, path); return; }
            if (index < 0 || index >= lv.Items.Count) return;
            lv.Items[index].SubItems[3].Text = path;
        }

        private void SetBusy(bool busy)
        {
            if (InvokeRequired) { BeginInvoke((Action<bool>)SetBusy, busy); return; }
            btnRun.Enabled = !busy;
            btnScan.Enabled = !busy;
            btnBrowse.Enabled = !busy;
            btnBrowseDest.Enabled = !busy && rdClone.Checked;
            txtRoot.Enabled = !busy;
            txtDest.Enabled = !busy && rdClone.Checked;
            txtBranch.Enabled = !busy;
            numDepth.Enabled = !busy;
            rdSwitch.Enabled = !busy;
            rdClone.Enabled = !busy;
            btnStop.Enabled = busy;
            Cursor = busy ? Cursors.AppStarting : Cursors.Default;
        }

        /// <summary>Sizes the columns to exactly fill the list, so there is never a horizontal
        /// scrollbar. Widths are proportional to what each column's content actually needs, subject to
        /// per-column minimums; anything that still does not fit is ellipsised by the ListView.</summary>
        private void FitColumns() { FitColumns(false); }

        private void FitColumns(bool force)
        {
            if (InvokeRequired) { BeginInvoke((Action<bool>)FitColumns, force); return; }
            if (fittingColumns || lv.Columns.Count == 0) return;

            int n = lv.Columns.Count;
            int avail = lv.ClientSize.Width - 4;      // ClientSize already excludes the vertical bar
            if (avail < 240) return;
            if (!force && avail == lastFitWidth) return;
            lastFitWidth = avail;

            fittingColumns = true;
            lv.BeginUpdate();
            try
            {
                int[] min = { 70, 140, 120, 140 };
                // Fixed proportions, not measured content: the layout then depends only on the window
                // width, so it never jumps around when a run fills the list.
                int[] weight = { 12, 26, 24, 38 };      // Repository, Branch, Result, Path

                var w = new int[n];
                int total = 0;
                for (int i = 0; i < n; i++)
                {
                    w[i] = avail * (i < weight.Length ? weight[i] : 25) / 100;
                    int floor = i < min.Length ? min[i] : 100;
                    if (w[i] < floor) w[i] = floor;
                    total += w[i];
                }

                // Settle rounding and any minimum-clamping on the widest column.
                int diff = avail - total;
                if (diff > 0)
                {
                    int idx = 0;
                    for (int i = 1; i < n; i++) if (w[i] > w[idx]) idx = i;
                    w[idx] += diff;
                }
                else
                {
                    for (int pass = 0; pass < n && diff < 0; pass++)
                    {
                        int idx = -1;
                        for (int i = 0; i < n; i++)
                        {
                            int floor = i < min.Length ? min[i] : 100;
                            if (w[i] - floor > 0 && (idx < 0 || w[i] > w[idx])) idx = i;
                        }
                        if (idx < 0) break;
                        int room = w[idx] - (idx < min.Length ? min[idx] : 100);
                        int take = Math.Min(room, -diff);
                        w[idx] -= take;
                        diff += take;
                    }
                }

                for (int i = 0; i < n; i++) lv.Columns[i].Width = w[i];
            }
            finally
            {
                lv.EndUpdate();
                fittingColumns = false;
            }
        }

        private void OpenSelectedFolder()
        {
            if (lv.SelectedItems.Count == 0) return;
            string p = lv.SelectedItems[0].SubItems[3].Text;
            if (Directory.Exists(p))
                try { Process.Start("explorer.exe", "\"" + p.TrimEnd('\\') + "\""); } catch { }
        }

        private void CopyRows()
        {
            var sb = new StringBuilder();
            var items = lv.SelectedItems.Count > 0 ? (System.Collections.IList)lv.SelectedItems : lv.Items;
            foreach (ListViewItem it in items)
                sb.AppendLine(it.SubItems[0].Text + "\t" + it.SubItems[1].Text + "\t" + it.SubItems[2].Text);
            if (sb.Length > 0) { try { Clipboard.SetText(sb.ToString()); } catch { } }
        }

        #endregion

        #region Folder pickers / destination name

        private void BrowseRoot()
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = rdClone.Checked
                    ? "Select the prepared master folder that contains the repositories"
                    : "Select the folder that contains the cloned repositories";
                if (Directory.Exists(txtRoot.Text.Trim())) dlg.SelectedPath = txtRoot.Text.Trim();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtRoot.Text = dlg.SelectedPath;
                    ScanRepos(true);
                }
            }
        }

        private void BrowseDest()
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "Select or create the folder that will receive the branch copy";
                string cur = txtDest.Text.Trim();
                if (Directory.Exists(cur)) dlg.SelectedPath = cur;
                else
                {
                    try
                    {
                        string parent = Path.GetDirectoryName(cur.TrimEnd('\\'));
                        if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent)) dlg.SelectedPath = parent;
                    }
                    catch { }
                }
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    txtDest.Text = dlg.SelectedPath;
                    lastAutoDest = "";      // user chose explicitly - stop auto-filling
                }
            }
        }

        private static string SanitizeFolderName(string s)
        {
            var sb = new StringBuilder();
            char[] bad = Path.GetInvalidFileNameChars();
            foreach (char c in s)
                sb.Append(Array.IndexOf(bad, c) >= 0 ? '_' : c);
            return sb.ToString().Trim().Trim('.');
        }

        /// <summary>Suggests "&lt;parent of master&gt;\&lt;last branch segment&gt;" while the user has not
        /// overridden the destination by hand.</summary>
        private void UpdateAutoDest()
        {
            if (!uiReady || !rdClone.Checked) return;

            string cur = txtDest.Text.Trim();
            if (cur.Length != 0 && !cur.Equals(lastAutoDest, StringComparison.OrdinalIgnoreCase)) return;

            var branches = ParseBranchInput(txtBranch.Text);
            if (branches.Count == 0) return;
            string leaf = branches[0];
            int slash = leaf.LastIndexOf('/');
            if (slash >= 0) leaf = leaf.Substring(slash + 1);
            leaf = SanitizeFolderName(leaf);
            if (leaf.Length == 0) return;

            string root = txtRoot.Text.Trim().TrimEnd('\\');
            if (root.Length == 0) return;
            string parent;
            try { parent = Path.GetDirectoryName(root); }
            catch { return; }
            if (string.IsNullOrEmpty(parent)) return;

            string auto;
            try { auto = Path.Combine(parent, leaf); }
            catch { return; }

            txtDest.Text = auto;
            lastAutoDest = auto;
        }

        #endregion

        #region Scan

        private static bool IsRepo(string dir)
        {
            try
            {
                string g = Path.Combine(dir, ".git");
                return Directory.Exists(g) || File.Exists(g); // .git file = worktree / submodule
            }
            catch { return false; }
        }

        private static void FindRepos(string dir, int depth, int maxDepth, List<string> found)
        {
            if (IsRepo(dir)) { found.Add(dir); return; } // do not descend into a repo
            if (depth >= maxDepth) return;
            string[] subs;
            try { subs = Directory.GetDirectories(dir); }
            catch { return; }
            Array.Sort(subs, StringComparer.OrdinalIgnoreCase);
            foreach (string sub in subs)
            {
                string name = Path.GetFileName(sub);
                if (name.StartsWith(".") || name.Equals("node_modules", StringComparison.OrdinalIgnoreCase)) continue;
                try { if ((File.GetAttributes(sub) & FileAttributes.Hidden) != 0) continue; } catch { continue; }
                FindRepos(sub, depth + 1, maxDepth, found);
            }
        }

        private bool ScanRepos(bool readCurrentBranch)
        {
            string root = txtRoot.Text.Trim().Trim('"');
            if (root.Length == 0 || !Directory.Exists(root))
            {
                MessageBox.Show(this, "Pick an existing " + (rdClone.Checked ? "master" : "root") + " folder first.",
                    "Multi Branch Switcher", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            repos.Clear();
            lv.Items.Clear();
            FindRepos(root, 0, (int)numDepth.Value, repos);

            foreach (string r in repos)
            {
                var it = new ListViewItem(Path.GetFileName(r.TrimEnd('\\')));
                it.SubItems.Add("");
                it.SubItems.Add("");
                it.SubItems.Add(r);
                lv.Items.Add(it);
            }

            Log("Scan: found " + repos.Count + " repository(ies) under " + root + " (depth " + (int)numDepth.Value + ").");
            SetStatus(repos.Count + " repositories");

            if (repos.Count == 0)
            {
                Log("Nothing found. Increase Depth if your repos are nested deeper.");
                return false;
            }

            if (readCurrentBranch)
            {
                SetBusy(true);
                var t = new Thread(() =>
                {
                    for (int i = 0; i < repos.Count; i++)
                    {
                        var r = Git.Run(repos[i], "rev-parse --abbrev-ref HEAD", TimeoutQuick, null);
                        SetRowResult(i, r.Ok ? Proc.FirstLine(r.StdOut) : "?", "", SystemColors.WindowText);
                    }
                    SetBusy(false);
                    SetStatus(repos.Count + " repositories");
                });
                t.IsBackground = true;
                t.Start();
            }
            return true;
        }

        #endregion

        #region Run

        private static List<string> ParseBranchInput(string input)
        {
            var list = new List<string>();
            if (input == null) return list;
            foreach (string part in input.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string b = part.Trim().Trim('"').Trim();
                if (b.Length == 0) continue;
                if (b.StartsWith("refs/heads/", StringComparison.OrdinalIgnoreCase)) b = b.Substring(11);
                if (b.StartsWith("refs/remotes/", StringComparison.OrdinalIgnoreCase)) b = b.Substring(13);
                if (b.StartsWith("remotes/", StringComparison.OrdinalIgnoreCase)) b = b.Substring(8);
                b = b.Trim('/');
                if (b.Length > 0 && !list.Contains(b)) list.Add(b);
            }
            return list;
        }

        private static bool SamePath(string a, string b)
        {
            try
            {
                return Path.GetFullPath(a).TrimEnd('\\')
                    .Equals(Path.GetFullPath(b).TrimEnd('\\'), StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private static bool IsUnder(string child, string parent)
        {
            try
            {
                string c = Path.GetFullPath(child).TrimEnd('\\') + "\\";
                string p = Path.GetFullPath(parent).TrimEnd('\\') + "\\";
                return c.StartsWith(p, StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private void StartRun()
        {
            if (worker != null && worker.IsAlive) return;

            bool clone = rdClone.Checked;

            var branches = ParseBranchInput(txtBranch.Text);
            if (branches.Count == 0)
            {
                MessageBox.Show(this, "Type the branch name you want.", "Multi Branch Switcher",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtBranch.Focus();
                return;
            }

            if (repos.Count == 0 && !ScanRepos(false)) return;

            string masterRoot = txtRoot.Text.Trim().Trim('"').TrimEnd('\\');
            string destRoot = txtDest.Text.Trim().Trim('"').TrimEnd('\\');
            int existingCopies = 0;

            if (clone)
            {
                if (destRoot.Length == 0)
                {
                    MessageBox.Show(this, "Set the new folder that will receive the copy.", "Multi Branch Switcher",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtDest.Focus();
                    return;
                }
                if (SamePath(masterRoot, destRoot) || IsUnder(destRoot, masterRoot) || IsUnder(masterRoot, destRoot))
                {
                    MessageBox.Show(this, "The new folder must not be the master folder, nor inside it, nor contain it.",
                        "Multi Branch Switcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDest.Focus();
                    return;
                }
                try
                {
                    string full = Path.GetFullPath(destRoot);
                    string drive = Path.GetPathRoot(full);
                    if (!string.IsNullOrEmpty(drive) && !Directory.Exists(drive))
                    {
                        MessageBox.Show(this, "The drive " + drive + " does not exist.", "Multi Branch Switcher",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtDest.Focus();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "The new folder path is not valid:\r\n" + ex.Message,
                        "Multi Branch Switcher", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtDest.Focus();
                    return;
                }

                // How many repo folders are already sitting in the destination?
                foreach (string r in repos)
                {
                    string d = Path.Combine(destRoot, Path.GetFileName(r.TrimEnd('\\')));
                    try { if (Directory.Exists(d) && Directory.GetFileSystemEntries(d).Length > 0) existingCopies++; }
                    catch { }
                }
            }

            if (chkForce.Checked && !chkDryRun.Checked)
            {
                string msg = clone
                    ? "Force is enabled: each repository in the MASTER folder will be reset to its remote and untracked files removed.\r\n\r\nUncommitted work in the master folder will be lost. Continue?"
                    : "Force is enabled: uncommitted changes in these repositories will be DISCARDED.\r\n\r\nContinue?";
                if (MessageBox.Show(this, msg, "Multi Branch Switcher",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
            }

            if (clone && chkOverwrite.Checked && existingCopies > 0 && !chkDryRun.Checked)
            {
                if (MessageBox.Show(this,
                        existingCopies + " repository folder(s) already exist under\r\n" + destRoot +
                        "\r\n\r\nOverwrite is enabled, so they will be mirrored from master: files that are not in " +
                        "master get deleted, including any uncommitted work in those copies.\r\n\r\nContinue?",
                        "Multi Branch Switcher", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return;
            }

            if (clone && !chkDryRun.Checked)
            {
                try { Directory.CreateDirectory(destRoot); }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Cannot create the new folder:\r\n" + ex.Message, "Multi Branch Switcher",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            SaveSettingsFromUi();

            var opts = new RunOptions
            {
                Branches = branches,
                CloneMode = clone,
                DestRoot = destRoot,
                Fetch = chkFetch.Checked,
                Prune = chkPrune.Checked,
                Create = chkCreate.Checked,
                Force = chkForce.Checked,
                Pull = chkPull.Checked,
                Dry = chkDryRun.Checked,
                Suffix = chkSuffix.Checked,
                Overwrite = chkOverwrite.Checked,
                LongPaths = chkLongPaths.Checked
            };

            var repoList = new List<string>(repos);
            for (int i = 0; i < lv.Items.Count; i++) SetRowResult(i, null, "", SystemColors.WindowText);

            cancelRequested = false;
            SetBusy(true);
            Log("");
            Log("=== " + (opts.Dry ? "DRY RUN / " : "") + (clone ? "CLONE" : "SWITCH") + " -> " +
                string.Join(" | ", branches.ToArray()) + "   (" + repoList.Count + " repos)");
            if (clone)
            {
                Log("    master: " + masterRoot + "   ->   new: " + destRoot);
                if (existingCopies > 0 && !opts.Overwrite)
                    Log("    NOTE: " + existingCopies + " destination repo folder(s) already exist and will be " +
                        "skipped - tick 'Overwrite existing destination' to refresh them from master.");
                int budget = 259 - destRoot.Length - 1;   // chars left for "<Repo>\path\inside\repo"
                if (budget < 170)
                {
                    Log("    WARNING: the destination path is " + destRoot.Length + " chars, leaving only " +
                        budget + " for paths inside each repo.");
                    Log("    Deep repos may exceed the 260-char Windows limit. Consider a shorter folder name" +
                        (chkLongPaths.Checked ? "" : ", or tick 'Enable long paths'") + ".");
                }
            }

            worker = new Thread(() =>
            {
                int switched = 0, created = 0, already = 0, kept = 0, skipped = 0, failed = 0;
                for (int i = 0; i < repoList.Count; i++)
                {
                    if (cancelRequested)
                    {
                        SetRowResult(i, null, "Cancelled", SystemColors.GrayText);
                        continue;
                    }
                    SetStatus("(" + (i + 1) + "/" + repoList.Count + ") " + Path.GetFileName(repoList[i]));

                    Outcome outcome = opts.CloneMode
                        ? CloneRepo(i, repoList[i], opts)
                        : ProcessRepo(i, repoList[i], opts);

                    switch (outcome)
                    {
                        case Outcome.Switched: switched++; break;
                        case Outcome.CreatedAndSwitched: created++; break;
                        case Outcome.Already: already++; break;
                        case Outcome.KeptMaster: kept++; break;
                        case Outcome.Skipped: skipped++; break;
                        default: failed++; break;
                    }
                }

                if (opts.CloneMode && !opts.Dry && !cancelRequested)
                    CopyLooseRootFiles(masterRoot, opts.DestRoot);

                string summary = opts.CloneMode
                    ? "copied+switched=" + (switched + created) + ", already=" + already +
                      ", kept master=" + kept + ", skipped=" + skipped + ", failed=" + failed
                    : "switched=" + switched + ", created+switched=" + created + ", already=" + already +
                      ", skipped=" + skipped + ", failed=" + failed;
                Log("=== Done. " + summary);
                SetStatus("Done - " + summary);
                SetBusy(false);
            });
            worker.IsBackground = true;
            worker.Start();
        }

        private enum Outcome { Switched, CreatedAndSwitched, Already, KeptMaster, Skipped, Failed }

        /// <summary>True if <paramref name="full"/> equals <paramref name="tail"/> or ends with "/" + tail,
        /// so "LGD-Mobile_Release_20231120" matches "feature/LGD-Mobile_Release_20231120" but not
        /// "feature/old-LGD-Mobile_Release_20231120".</summary>
        private static bool EndsWithSegment(string full, string tail)
        {
            if (full.Equals(tail, StringComparison.OrdinalIgnoreCase)) return true;
            return full.Length > tail.Length + 1
                && full.EndsWith("/" + tail, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region Clone mode

        private Outcome CloneRepo(int row, string masterRepo, RunOptions o)
        {
            string name = Path.GetFileName(masterRepo.TrimEnd('\\'));
            string destRepo = Path.Combine(o.DestRoot, name);
            Log("--- " + name);
            SetRowPath(row, destRepo);

            var rCur = Git.Run(masterRepo, "rev-parse --abbrev-ref HEAD", TimeoutQuick, null);
            if (!rCur.Ok)
            {
                Log("    not a usable git repository: " + rCur.Message);
                SetRowResult(row, "?", "Not a git repo", Color.Firebrick);
                return Outcome.Failed;
            }
            string masterBranch = Proc.FirstLine(rCur.StdOut);
            SetRowResult(row, masterBranch, "refreshing master...", SystemColors.WindowText);

            if (o.LongPaths) EnsureLongPaths(masterRepo);

            // ---- 1. refresh the master copy
            if (o.Fetch)
            {
                var rf = Git.Run(masterRepo, "fetch --all" + (o.Prune ? " --prune" : ""), TimeoutNetwork, Log);
                if (!rf.Ok) Log("    fetch warning: " + rf.Message);
            }

            if (!o.Dry)
            {
                string upstream = "";
                var rUp = Git.Run(masterRepo, "rev-parse --abbrev-ref --symbolic-full-name @{u}", TimeoutQuick, null);
                if (rUp.Ok) upstream = Proc.FirstLine(rUp.StdOut);

                if (o.Force && upstream.Length > 0)
                {
                    var rr = Git.Run(masterRepo, "reset --hard \"" + upstream + "\"", TimeoutCheckout, Log);
                    if (!rr.Ok) Log("    reset warning: " + rr.Message);
                    var rc = Git.Run(masterRepo, "clean -fd", TimeoutCheckout, Log);
                    if (!rc.Ok) Log("    clean warning: " + rc.Message);
                }
                else if (o.Pull || o.Force)
                {
                    if (upstream.Length == 0)
                    {
                        Log("    " + masterBranch + " has no upstream - nothing to pull.");
                    }
                    else
                    {
                        var rp = Git.Run(masterRepo, "pull --ff-only", TimeoutNetwork, Log);
                        if (!rp.Ok) Log("    pull warning: " + rp.Message);
                    }
                }
            }

            // ---- 2. dry run stops here, after reporting what the branch lookup would find
            if (o.Dry)
            {
                string found, foundRemote, amb;
                ResolveBranch(masterRepo, o, out found, out foundRemote, out amb);
                if (amb != null)
                {
                    SetRowResult(row, masterBranch, "Ambiguous: " + amb, Color.DarkOrange);
                    return Outcome.Skipped;
                }
                if (found == null)
                {
                    Log("    branch not found - copy would stay on " + masterBranch + ".");
                    SetRowResult(row, masterBranch, "Would copy, keep " + masterBranch, Color.SteelBlue);
                    return Outcome.KeptMaster;
                }
                Log("    would copy to " + destRepo + " and switch to " + found);
                SetRowResult(row, masterBranch, "Would copy + switch to " + found, Color.SteelBlue);
                return Outcome.Already;
            }

            // ---- 3. copy master -> destination
            bool destExists = false;
            try { destExists = Directory.Exists(destRepo) && Directory.GetFileSystemEntries(destRepo).Length > 0; }
            catch { }

            if (destExists && !o.Overwrite)
            {
                Log("    destination already exists and Overwrite is off - not touched.");
                SetRowResult(row, "", "Destination exists - skipped", Color.DarkOrange);
                return Outcome.Skipped;
            }

            SetRowResult(row, masterBranch, destExists ? "mirroring..." : "copying...", SystemColors.WindowText);
            var rCopy = Robo.CopyTree(masterRepo, destRepo, destExists, TimeoutCopy, Log);
            if (!rCopy.Ok)
            {
                Log("    copy failed: " + rCopy.Message);
                SetRowResult(row, masterBranch, "Copy failed: " + rCopy.Message, Color.Firebrick);
                return Outcome.Failed;
            }
            Log("    copied to " + destRepo);

            // ---- 4. switch the copy. Refs came across with the copy, so no second fetch.
            var so = o.Copy();
            so.Fetch = false;
            so.Pull = false;
            so.Dry = false;
            return ProcessRepo(row, destRepo, so);
        }

        private void CopyLooseRootFiles(string masterRoot, string destRoot)
        {
            try
            {
                if (Directory.GetFiles(masterRoot).Length == 0) return;
            }
            catch { return; }

            var r = Robo.CopyTopLevelFiles(masterRoot, destRoot, TimeoutCopy, Log);
            if (r.Ok) Log("Copied loose files from the master folder root.");
            else Log("Loose root file copy warning: " + r.Message);
        }

        #endregion

        #region Branch resolution + switch

        /// <summary>Finds the best matching branch in <paramref name="repo"/>: exact local, exact remote,
        /// then optional whole-segment suffix match.</summary>
        private void ResolveBranch(string repo, RunOptions o, out string target, out string sourceRemote, out string ambiguous)
        {
            target = null; sourceRemote = null; ambiguous = null;

            var remotes = Proc.Lines(Git.Run(repo, "remote", TimeoutQuick, null).StdOut);
            int oi = remotes.FindIndex(x => x.Equals("origin", StringComparison.OrdinalIgnoreCase));
            if (oi > 0) { string org = remotes[oi]; remotes.RemoveAt(oi); remotes.Insert(0, org); }

            var localBranches = new List<string>();       // "feature/LGD-Mobile_Release_20231120"
            var remoteBranches = new List<string[]>();    // { remote, branch }
            var rRefs = Git.Run(repo, "for-each-ref --format=%(refname) refs/heads refs/remotes", TimeoutQuick, null);
            foreach (string refname in Proc.Lines(rRefs.StdOut))
            {
                if (refname.StartsWith("refs/heads/", StringComparison.Ordinal))
                {
                    localBranches.Add(refname.Substring(11));
                }
                else if (refname.StartsWith("refs/remotes/", StringComparison.Ordinal))
                {
                    string rest = refname.Substring(13);
                    if (rest.EndsWith("/HEAD", StringComparison.Ordinal)) continue;
                    foreach (string rem in remotes)
                    {
                        if (rest.StartsWith(rem + "/", StringComparison.Ordinal))
                        {
                            remoteBranches.Add(new[] { rem, rest.Substring(rem.Length + 1) });
                            break;
                        }
                    }
                }
            }

            foreach (string b in o.Branches)
            {
                var candidates = new List<string> { b };
                foreach (string rem in remotes)
                {
                    if (b.StartsWith(rem + "/", StringComparison.OrdinalIgnoreCase))
                    {
                        string stripped = b.Substring(rem.Length + 1);
                        if (stripped.Length > 0 && !candidates.Contains(stripped)) candidates.Add(stripped);
                    }
                }

                // 1. exact local
                foreach (string c in candidates)
                {
                    string cc = c;
                    string hit = localBranches.Find(x => x.Equals(cc, StringComparison.OrdinalIgnoreCase));
                    if (hit != null) { target = hit; sourceRemote = null; break; }
                }
                if (target != null) return;

                // 2. exact remote (remotes already ordered with origin first)
                foreach (string c in candidates)
                {
                    foreach (string rem in remotes)
                    {
                        string cc = c, rr = rem;
                        var hit = remoteBranches.Find(x =>
                            x[0].Equals(rr, StringComparison.Ordinal) &&
                            x[1].Equals(cc, StringComparison.OrdinalIgnoreCase));
                        if (hit != null) { target = hit[1]; sourceRemote = hit[0]; break; }
                    }
                    if (target != null) break;
                }
                if (target != null) return;

                if (!o.Suffix) continue;

                // 3. suffix match - "LGD-Mobile_Release_20231120" finds "feature/LGD-Mobile_Release_20231120"
                foreach (string c in candidates)
                {
                    var names = new List<string>();
                    foreach (string x in localBranches)
                        if (EndsWithSegment(x, c) && !names.Contains(x)) names.Add(x);

                    if (names.Count == 1) { target = names[0]; sourceRemote = null; break; }
                    if (names.Count > 1) { ambiguous = string.Join(", ", names.ToArray()); break; }

                    var remHits = new List<string[]>();
                    foreach (string[] x in remoteBranches)
                        if (EndsWithSegment(x[1], c) && !names.Contains(x[1]))
                        {
                            names.Add(x[1]);
                            remHits.Add(x);
                        }

                    if (names.Count == 1)
                    {
                        foreach (string rem in remotes)     // highest-priority remote carrying that name
                        {
                            string rr = rem;
                            var pick = remHits.Find(x => x[0].Equals(rr, StringComparison.Ordinal));
                            if (pick != null) { target = pick[1]; sourceRemote = pick[0]; break; }
                        }
                        if (target == null) { target = remHits[0][1]; sourceRemote = remHits[0][0]; }
                        break;
                    }
                    if (names.Count > 1) { ambiguous = string.Join(", ", names.ToArray()); break; }
                }
                if (target != null || ambiguous != null) return;
            }
        }

        private Outcome ProcessRepo(int row, string repo, RunOptions o)
        {
            string name = Path.GetFileName(repo.TrimEnd('\\'));
            if (!o.CloneMode) Log("--- " + name);

            var rCur = Git.Run(repo, "rev-parse --abbrev-ref HEAD", TimeoutQuick, null);
            string current = rCur.Ok ? Proc.FirstLine(rCur.StdOut) : "?";
            SetRowResult(row, current, "working...", SystemColors.WindowText);

            if (!rCur.Ok)
            {
                Log("    not a usable git repository: " + rCur.Message);
                SetRowResult(row, current, "Not a git repo", Color.Firebrick);
                return Outcome.Failed;
            }

            if (o.LongPaths) EnsureLongPaths(repo);

            if (o.Fetch)
            {
                var rf = Git.Run(repo, "fetch --all" + (o.Prune ? " --prune" : ""), TimeoutNetwork, Log);
                if (!rf.Ok) Log("    fetch warning: " + rf.Message);
            }

            string target, sourceRemote, ambiguous;
            ResolveBranch(repo, o, out target, out sourceRemote, out ambiguous);

            if (ambiguous != null)
            {
                Log("    ambiguous - several branches end with that name: " + ambiguous);
                SetRowResult(row, current, "Ambiguous: " + ambiguous, Color.DarkOrange);
                return Outcome.Skipped;
            }

            if (target == null)
            {
                if (o.CloneMode)
                {
                    Log("    branch not found - copy stays on " + current + ".");
                    SetRowResult(row, current, "Copied, kept " + current + " (branch not found)", Color.DarkGoldenrod);
                    return Outcome.KeptMaster;
                }
                Log("    branch not found (local or remote).");
                SetRowResult(row, current, "Branch not found", Color.DarkOrange);
                return Outcome.Skipped;
            }

            string where = sourceRemote == null ? "local" : sourceRemote + "/" + target;
            Log("    found: " + target + "  [" + where + "]");

            if (string.Equals(current, target, StringComparison.Ordinal))
            {
                SetRowResult(row, current, (o.CloneMode ? "Copied, already on " : "Already on ") + target, Color.DimGray);
                if (o.Pull) DoPull(repo);
                return Outcome.Already;
            }

            if (o.Dry)
            {
                SetRowResult(row, current, "Would switch to " + target + " [" + where + "]", Color.SteelBlue);
                return Outcome.Already;
            }

            if (!o.Force)
            {
                var rSt = Git.Run(repo, "status --porcelain", TimeoutQuick, null);
                if (rSt.Ok && rSt.StdOut.Trim().Length > 0)
                {
                    int changes = Proc.Lines(rSt.StdOut).Count;
                    Log("    skipped: " + changes + " uncommitted change(s). Enable Force to discard, or commit/stash first.");
                    SetRowResult(row, current, "Skipped - " + changes + " local change(s)", Color.DarkOrange);
                    return Outcome.Skipped;
                }
            }

            string checkoutArgs;
            bool creating = false;
            if (sourceRemote == null)
            {
                checkoutArgs = "checkout " + (o.Force ? "--force " : "") + "\"" + target + "\"";
            }
            else
            {
                if (!o.Create)
                {
                    Log("    branch exists only on " + sourceRemote + " and 'Create local branch from remote' is off.");
                    SetRowResult(row, current, "Remote only - creation disabled", Color.DarkOrange);
                    return Outcome.Skipped;
                }
                creating = true;
                checkoutArgs = "checkout " + (o.Force ? "--force " : "") + (o.Force ? "-B " : "-b ") +
                               "\"" + target + "\" --track \"" + sourceRemote + "/" + target + "\"";
            }

            var rCo = Git.Run(repo, checkoutArgs, TimeoutCheckout, Log);
            if (!rCo.Ok)
            {
                Log("    checkout failed: " + rCo.Message);
                bool tooLong = rCo.Message.IndexOf("too long", StringComparison.OrdinalIgnoreCase) >= 0;
                if (tooLong)
                {
                    // git may have moved HEAD before hitting MAX_PATH, leaving files missing.
                    Log("    the working tree is INCOMPLETE - some files were not written.");
                    if (!o.LongPaths)
                        Log("    tick 'Enable long paths (core.longpaths)' and run again, or use a shorter destination folder name.");
                    else
                        Log("    core.longpaths is on but the path is still too long for some tool - shorten the destination folder name.");
                    SetRowResult(row, current, "Failed - path too long, tree incomplete", Color.Firebrick);
                }
                else
                {
                    SetRowResult(row, current, "Failed: " + rCo.Message, Color.Firebrick);
                }
                return Outcome.Failed;
            }

            string now = Proc.FirstLine(Git.Run(repo, "rev-parse --abbrev-ref HEAD", TimeoutQuick, null).StdOut);
            Log("    now on " + now);
            string prefix = o.CloneMode ? "Copied + " : "";
            SetRowResult(row, now,
                prefix + (creating ? "created + switched (from " + sourceRemote + ")" : "switched"),
                Color.ForestGreen);

            if (o.Pull) DoPull(repo);

            return creating ? Outcome.CreatedAndSwitched : Outcome.Switched;
        }

        /// <summary>Sets core.longpaths so git uses the Unicode APIs and is not bound by MAX_PATH.
        /// Deep repos under a long destination folder name fail with "Filename too long" without it.</summary>
        private void EnsureLongPaths(string repo)
        {
            var r = Git.Run(repo, "config core.longpaths true", TimeoutQuick, null);
            if (!r.Ok) Log("    could not set core.longpaths: " + r.Message);
        }

        private void DoPull(string repo)
        {
            var rp = Git.Run(repo, "pull --ff-only", TimeoutNetwork, Log);
            if (rp.Ok) Log("    pull --ff-only ok");
            else Log("    pull warning: " + rp.Message);
        }

        #endregion
    }
}
