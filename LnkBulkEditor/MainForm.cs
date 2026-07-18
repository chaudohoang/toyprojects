// LNK Bulk Editor — MainForm.cs
// Single-file WinForms app.  No NuGet required.
// Uses WScript.Shell COM (built into Windows) via C# dynamic.
// Build: open LnkBulkEditor.csproj in Visual Studio, or run build.bat

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LnkBulkEditor
{
    // ═══════════════════════════════════════════════════════════════════════════
    //  Entry point
    // ═══════════════════════════════════════════════════════════════════════════
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  Data model
    // ═══════════════════════════════════════════════════════════════════════════
    class ShortcutData
    {
        public string FilePath    { get; set; } = "";
        public string TargetPath  { get; set; } = "";
        public string Arguments   { get; set; } = "";
        public string WorkingDir  { get; set; } = "";
        public string Description { get; set; } = "";
        public string IconPath    { get; set; } = "";
        public int    IconIndex   { get; set; } = 0;
        public int    WindowStyle { get; set; } = 1;   // 1=Normal 3=Max 7=Min
        public string Hotkey      { get; set; } = "";
        public bool   IsDirty     { get; set; } = false;

        public string FileName    => Path.GetFileName(FilePath);
        public string DisplayName => Path.GetFileNameWithoutExtension(FilePath) + (IsDirty ? "  *" : "");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    //  Main form
    // ═══════════════════════════════════════════════════════════════════════════
    public class MainForm : Form
    {
        // ── Left sidebar ──────────────────────────────────────────────────────
        private ListBox lstFiles;
        private Button  btnLoad, btnClear;
        private Label   lblCount;

        // ── Header ────────────────────────────────────────────────────────────
        private Label   lblFilePath;
        private TextBox txtName;

        // ── Editable fields ───────────────────────────────────────────────────
        private TextBox       txtTarget, txtArguments, txtWorkDir,
                              txtDescription, txtIconPath, txtHotkey;
        private NumericUpDown nudIconIndex;
        private ComboBox      cmbWindowStyle;

        // ── Browse buttons ────────────────────────────────────────────────────
        private Button btnBrowseTarget, btnBrowseWorkDir, btnBrowseIcon;

        // ── Find & Replace bar ────────────────────────────────────────────────
        private TextBox   txtFind, txtReplace;
        private ComboBox  cmbReplaceScope;
        private Button    btnReplace;

        // ── Bottom bar ────────────────────────────────────────────────────────
        private Button btnPrev, btnNext, btnSaveCurrent, btnSaveAll;
        private Label  lblPosition, lblStatus;

        // ── State ─────────────────────────────────────────────────────────────
        private readonly List<ShortcutData> shortcuts = new List<ShortcutData>();
        private int  currentIndex = -1;
        private bool isLoading    = false;

        // ── Palette ───────────────────────────────────────────────────────────
        static readonly Color C_SIDEBAR = Color.FromArgb(235, 237, 242);
        static readonly Color C_DIVIDER = Color.FromArgb(200, 203, 212);
        static readonly Color C_SAVE    = Color.FromArgb(0,  120, 215);
        static readonly Color C_SAVEALL = Color.FromArgb(16, 137,  62);
        static readonly Color C_DIRTY   = Color.FromArgb(180,  80,   0);
        static readonly Color C_FNRBAR  = Color.FromArgb(255, 251, 230);

        // ═════════════════════════════════════════════════════════════════════
        //  Constructor
        // ═════════════════════════════════════════════════════════════════════
        public MainForm()
        {
            BuildUI();
            SetStatus("Drop .lnk files here, or click  Load…");
            RefreshAll();
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Build UI
        // ═════════════════════════════════════════════════════════════════════
        void BuildUI()
        {
            Text          = "LNK Bulk Editor";
            Size          = new Size(1020, 700);
            MinimumSize   = new Size(800, 520);
            StartPosition = FormStartPosition.CenterScreen;
            Font          = new Font("Segoe UI", 9.5f);
            BackColor     = Color.White;
            AllowDrop     = true;
            DragEnter += (s, e) => { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; };
            DragDrop  += OnDrop;

            // ── Sidebar ───────────────────────────────────────────────────────
            var pLeft = new Panel { Dock = DockStyle.Left, Width = 238, BackColor = C_SIDEBAR };

            var pSideTop = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = C_SIDEBAR, Padding = new Padding(7, 8, 7, 0) };
            btnLoad  = MakeBtn("Load…", 95, Color.FromArgb(60,60,60), Color.White);
            btnClear = MakeBtn("Clear",  68, Color.FromArgb(180,60,60), Color.White);
            btnLoad.Click  += OnLoad;
            btnClear.Click += (s, e) =>
            {
                if (shortcuts.Count == 0) return;
                if (MessageBox.Show("Remove all loaded files from the list?", "Confirm",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                shortcuts.Clear(); currentIndex = -1;
                RefreshAll(); SetStatus("List cleared.");
            };
            int bx = 7;
            PlaceBtn(pSideTop, btnLoad,  ref bx);
            PlaceBtn(pSideTop, btnClear, ref bx);

            lblCount = new Label
            {
                Dock = DockStyle.Top, Height = 22,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 8.5f), ForeColor = Color.Gray,
                Text = "No files loaded", BackColor = C_SIDEBAR
            };

            lstFiles = new ListBox
            {
                Dock = DockStyle.Fill, BorderStyle = BorderStyle.None,
                BackColor = C_SIDEBAR, IntegralHeight = false,
                Font = new Font("Segoe UI", 9.5f), ItemHeight = 22
            };
            lstFiles.DrawMode = DrawMode.OwnerDrawFixed;
            lstFiles.DrawItem += LstFiles_DrawItem;
            lstFiles.SelectedIndexChanged += OnListSelect;

            // Add in correct dock order: Fill first so Top controls eat into it correctly,
            // then suspend/resume so layout is recalculated once after all controls are added.
            pLeft.SuspendLayout();
            pLeft.Controls.Add(lstFiles);   // Fill  – added first  (processed last by layout)
            pLeft.Controls.Add(lblCount);   // Top   – added second
            pLeft.Controls.Add(pSideTop);   // Top   – added last   (processed first by layout)
            pLeft.ResumeLayout(performLayout: true);

            // ── Divider ───────────────────────────────────────────────────────
            var divider = new Panel { Dock = DockStyle.Left, Width = 1, BackColor = C_DIVIDER };

            // ── Right panel ───────────────────────────────────────────────────
            var pRight = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(18, 10, 18, 8) };

            // ── Header: Name (editable) + file path (read-only) ───────────────
            var pHead = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = Color.White };

            var lblNameL = new Label { Left = 0, Top = 5, Width = 46, Text = "Name:", Font = new Font("Segoe UI", 9.5f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            txtName = new TextBox
            {
                Left = 50, Top = 4, Width = 400, Font = new Font("Segoe UI", 9.5f),
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            var lblNameHint = new Label
            {
                Left = 458, Top = 6, Width = 220, ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                Text = "(renames the .lnk file on save)",
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            var lblFileL = new Label { Left = 0, Top = 36, Width = 46, Text = "File:", Font = new Font("Segoe UI", 9f), ForeColor = Color.Gray, TextAlign = ContentAlignment.MiddleLeft };
            lblFilePath  = new Label
            {
                Left = 50, Top = 36, Width = 600, ForeColor = Color.DarkGray, Text = "(none)", AutoEllipsis = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right
            };
            pHead.Controls.AddRange(new Control[] { lblNameL, txtName, lblNameHint, lblFileL, lblFilePath });

            // ── Find & Replace bar ─────────────────────────────────────────────
            // Outer strip (fixed height, yellow accent on left)
            var pFnR = new Panel { Dock = DockStyle.Top, Height = 42, BackColor = C_FNRBAR };
            var accent = new Panel { Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(255, 200, 0) };
            pFnR.Controls.Add(accent);

            // Inner row – holds all controls at fixed positions; clips when window is narrow.
            var pFnRInner = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = C_FNRBAR,
                Padding = new Padding(0, 6, 0, 0)
            };

            // Total bar width budget: form(1020) - sidebar(238) - divider(1) - padding(36) = ~745px
            // Layout: accent(4) + Find(32+6+150) + With(32+6+150) + In(20+6+148) + btn(90) = ~644px  ✓
            int fx = 8;
            Lbl(pFnRInner, "Find:", ref fx, 32);
            txtFind = FnRBox(pFnRInner, ref fx, 150);
            Lbl(pFnRInner, "With:", ref fx, 32);
            txtReplace = FnRBox(pFnRInner, ref fx, 150);
            Lbl(pFnRInner, "In:", ref fx, 20);

            cmbReplaceScope = new ComboBox
            {
                Left = fx, Top = 7, Width = 165,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            cmbReplaceScope.Items.AddRange(new object[]
            {
                "Target Path (all files)",
                "Start In (all files)",
                "Icon Path (all files)",
                "Arguments (all files)",
                "Description (all files)",
                "All text fields (all files)"
            });
            cmbReplaceScope.SelectedIndex = 0;
            pFnRInner.Controls.Add(cmbReplaceScope);
            fx += 169;

            btnReplace = new Button
            {
                Left = fx, Top = 5, Width = 94, Height = 26,
                Text = "Replace All", FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(80, 80, 80), ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f)
            };
            btnReplace.FlatAppearance.BorderSize = 0;
            btnReplace.Click += OnReplaceAll;
            pFnRInner.Controls.Add(btnReplace);
            // Result message goes to lblStatus (bottom bar) — no extra label needed here.

            pFnR.Controls.Add(pFnRInner);

            // ── Fields grid ───────────────────────────────────────────────────
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 3, AutoScroll = true,
                BackColor = Color.White, CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(0, 6, 0, 0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));

            int row = 0;
            txtTarget      = Field(); btnBrowseTarget  = BrowseBtn(); btnBrowseTarget.Click  += (s, e) => BrowseExe(txtTarget);
            txtWorkDir     = Field(); btnBrowseWorkDir = BrowseBtn(); btnBrowseWorkDir.Click += (s, e) => BrowseFolder(txtWorkDir);
            txtIconPath    = Field(); btnBrowseIcon    = BrowseBtn(); btnBrowseIcon.Click    += (s, e) => BrowseIcon();
            txtArguments   = Field();
            txtDescription = Field();
            txtHotkey      = Field();

            AddRow(tbl, ref row, "Target Path:",  txtTarget,      btnBrowseTarget);
            AddRow(tbl, ref row, "Arguments:",    txtArguments,   null);
            AddRow(tbl, ref row, "Start In:",     txtWorkDir,     btnBrowseWorkDir);
            AddRow(tbl, ref row, "Description:",  txtDescription, null);
            AddRow(tbl, ref row, "Icon Path:",    txtIconPath,    btnBrowseIcon);

            nudIconIndex = new NumericUpDown { Dock = DockStyle.Fill, Minimum = 0, Maximum = 9999, DecimalPlaces = 0, Font = new Font("Segoe UI", 9.5f) };
            AddRow(tbl, ref row, "Icon Index:", nudIconIndex, null);

            cmbWindowStyle = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 9.5f) };
            cmbWindowStyle.Items.Add("1 — Normal Window");
            cmbWindowStyle.Items.Add("3 — Maximized");
            cmbWindowStyle.Items.Add("7 — Minimized");
            AddRow(tbl, ref row, "Window Style:", cmbWindowStyle, null);

            AddRow(tbl, ref row, "Hotkey:", txtHotkey, null);

            for (int i = 0; i < row; i++) tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));

            var hint = new Label
            {
                Dock = DockStyle.Fill, ForeColor = Color.DarkGray,
                Font = new Font("Segoe UI", 8f, FontStyle.Italic),
                Text = "  Hotkey example:  CTRL+ALT+F1    |    Icon Index: 0 = first icon in the file",
                TextAlign = ContentAlignment.MiddleLeft
            };
            tbl.Controls.Add(hint, 0, row);
            tbl.SetColumnSpan(hint, 3);
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));

            // ── Bottom bar ────────────────────────────────────────────────────
            var pBot = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Color.White, Padding = new Padding(0, 8, 0, 0) };
            int px = 0;
            btnPrev = NavBtn("◄  Prev", ref px, 84); btnPrev.Click += (s, e) => Navigate(-1);
            lblPosition = new Label { Left = px, Top = 11, Width = 64, TextAlign = ContentAlignment.MiddleCenter, Text = "—/—" }; px += 68;
            btnNext = NavBtn("Next  ►", ref px, 84); btnNext.Click += (s, e) => Navigate(1); px += 18;
            btnSaveCurrent = NavBtn("Save Current", ref px, 120, C_SAVE,    Color.White); btnSaveCurrent.Click += OnSaveCurrent;
            px += 6;
            btnSaveAll     = NavBtn("Save All",     ref px,  95, C_SAVEALL, Color.White); btnSaveAll.Click     += OnSaveAll;
            px += 18;
            lblStatus = new Label { Left = px, Top = 11, Width = 440, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.DimGray };
            pBot.Controls.AddRange(new Control[] { btnPrev, lblPosition, btnNext, btnSaveCurrent, btnSaveAll, lblStatus });

            // ── Assembly ──────────────────────────────────────────────────────
            // Add to pRight in bottom-to-top dock order
            pRight.Controls.Add(tbl);
            pRight.Controls.Add(pBot);
            pRight.Controls.Add(pFnR);
            pRight.Controls.Add(pHead);

            Controls.Add(pRight);
            Controls.Add(divider);
            Controls.Add(pLeft);

            // ── Change events ─────────────────────────────────────────────────
            foreach (var tb in new[] { txtName, txtTarget, txtArguments, txtWorkDir, txtDescription, txtIconPath, txtHotkey })
                tb.TextChanged += OnFieldChanged;
            nudIconIndex.ValueChanged          += OnFieldChanged;
            cmbWindowStyle.SelectedIndexChanged += OnFieldChanged;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Small UI helpers
        // ═════════════════════════════════════════════════════════════════════
        TextBox Field() => new TextBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9.5f) };
        Button  BrowseBtn() => new Button { Text = "Browse…", Dock = DockStyle.Fill, FlatStyle = FlatStyle.System, Font = new Font("Segoe UI", 9f) };

        Button MakeBtn(string text, int w, Color back, Color fore)
        {
            var b = new Button { Text = text, Width = w, Height = 28, FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = fore, Font = new Font("Segoe UI", 9f) };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }
        void PlaceBtn(Control p, Button b, ref int x) { b.Top = p.Padding.Top; b.Left = x; x += b.Width + 5; p.Controls.Add(b); }

        Button NavBtn(string text, ref int x, int w = 80, Color? back = null, Color? fore = null)
        {
            var b = new Button
            {
                Text = text, Left = x, Top = 3, Width = w, Height = 30, FlatStyle = FlatStyle.Flat,
                BackColor = back ?? SystemColors.Control, ForeColor = fore ?? Color.Black, Font = new Font("Segoe UI", 9f)
            };
            b.FlatAppearance.BorderSize = back.HasValue ? 0 : 1;
            x += w + 4;
            return b;
        }

        void Lbl(Control p, string text, ref int x, int w)
        {
            var l = new Label { Left = x, Top = 10, Width = w, Text = text, Font = new Font("Segoe UI", 9f) };
            p.Controls.Add(l); x += w + 3;
        }
        TextBox FnRBox(Control p, ref int x, int w)
        {
            var t = new TextBox { Left = x, Top = 7, Width = w, Font = new Font("Segoe UI", 9f) };
            p.Controls.Add(t); x += w + 8;
            return t;
        }

        void AddRow(TableLayoutPanel t, ref int r, string label, Control ctrl, Control btn)
        {
            t.Controls.Add(new Label { Text = label, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 9.5f) }, 0, r);
            t.Controls.Add(ctrl, 1, r);
            t.Controls.Add(btn ?? (Control)new Label(), 2, r);
            r++;
        }

        void LstFiles_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= shortcuts.Count) return;
            e.DrawBackground();
            var d = shortcuts[e.Index];
            var brush = (e.State & DrawItemState.Selected) != 0
                ? SystemBrushes.HighlightText
                : (d.IsDirty ? new SolidBrush(C_DIRTY) : SystemBrushes.WindowText);
            e.Graphics.DrawString(d.DisplayName, e.Font, brush,
                new RectangleF(e.Bounds.X + 6, e.Bounds.Y + 2, e.Bounds.Width - 6, e.Bounds.Height));
        }

        // ═════════════════════════════════════════════════════════════════════
        //  COM: Read / Write .lnk
        // ═════════════════════════════════════════════════════════════════════
        ShortcutData ReadLnk(string path)
        {
            dynamic shell = null, sc = null;
            try
            {
                shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                sc    = shell.CreateShortcut(path);
                var d = new ShortcutData
                {
                    FilePath    = path,
                    TargetPath  = (string)sc.TargetPath        ?? "",
                    Arguments   = (string)sc.Arguments          ?? "",
                    WorkingDir  = (string)sc.WorkingDirectory   ?? "",
                    Description = (string)sc.Description        ?? "",
                    WindowStyle = (int)   sc.WindowStyle,
                    Hotkey      = (string)sc.Hotkey             ?? ""
                };
                string icon = (string)sc.IconLocation ?? "";
                var parts   = icon.Split(',');
                d.IconPath  = parts.Length > 0 ? parts[0].Trim() : "";
                d.IconIndex = parts.Length > 1 && int.TryParse(parts[1].Trim(), out int idx) ? Math.Max(0, idx) : 0;
                return d;
            }
            catch (Exception ex) { throw new Exception($"Cannot read '{Path.GetFileName(path)}': {ex.Message}"); }
            finally { SafeRelease(ref sc); SafeRelease(ref shell); }
        }

        void WriteLnk(ShortcutData d)
        {
            dynamic shell = null, sc = null;
            try
            {
                shell = Activator.CreateInstance(Type.GetTypeFromProgID("WScript.Shell"));
                sc    = shell.CreateShortcut(d.FilePath);
                sc.TargetPath       = d.TargetPath;
                sc.Arguments        = d.Arguments;
                sc.WorkingDirectory = d.WorkingDir;
                sc.Description      = d.Description;
                sc.WindowStyle      = d.WindowStyle;
                sc.Hotkey           = d.Hotkey;
                // If no icon path specified, fall back to the target exe's own icon (index 0),
                // which is the Windows default. Writing ",0" (empty path) would wipe the icon.
                string iconLoc = string.IsNullOrWhiteSpace(d.IconPath)
                    ? (string.IsNullOrWhiteSpace(d.TargetPath) ? "" : d.TargetPath + ",0")
                    : d.IconPath + "," + d.IconIndex;
                if (!string.IsNullOrEmpty(iconLoc))
                    sc.IconLocation = iconLoc;
                sc.Save();
            }
            catch (Exception ex) { throw new Exception($"Cannot save '{Path.GetFileName(d.FilePath)}': {ex.Message}"); }
            finally { SafeRelease(ref sc); SafeRelease(ref shell); }
        }

        void SafeRelease(ref dynamic obj)
        {
            if (obj == null) return;
            try { Marshal.ReleaseComObject(obj); } catch { }
            obj = null;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  UI refresh
        // ═════════════════════════════════════════════════════════════════════
        void RefreshAll()
        {
            isLoading = true;
            lstFiles.BeginUpdate();
            lstFiles.Items.Clear();
            foreach (var s in shortcuts) lstFiles.Items.Add(s.DisplayName);
            lstFiles.EndUpdate();

            bool any = shortcuts.Count > 0;
            lblCount.Text = any ? $"{shortcuts.Count} file(s) loaded" : "No files loaded";
            if (currentIndex >= shortcuts.Count) currentIndex = shortcuts.Count - 1;
            if (currentIndex < 0 && any)         currentIndex = 0;
            if (any)
            {
                lstFiles.SelectedIndex = currentIndex;
                // Scroll so the selected item is visible (ListBox has no EnsureVisible)
                int visible = lstFiles.ClientSize.Height / lstFiles.ItemHeight;
                if (currentIndex >= visible)
                    lstFiles.TopIndex = Math.Max(0, currentIndex - visible / 2);
                else
                    lstFiles.TopIndex = 0;
            }

            LoadFieldsFromCurrent();
            UpdateButtons();
            isLoading = false;
        }

        void LoadFieldsFromCurrent()
        {
            isLoading = true;
            bool any = currentIndex >= 0 && currentIndex < shortcuts.Count;
            EnableFields(any);

            if (!any)
            {
                lblFilePath.Text = "(none)";
                lblPosition.Text = "—/—";
                txtName.Text = txtTarget.Text = txtArguments.Text = txtWorkDir.Text =
                    txtDescription.Text = txtIconPath.Text = txtHotkey.Text = "";
                nudIconIndex.Value = 0;
                cmbWindowStyle.SelectedIndex = -1;
                isLoading = false;
                return;
            }

            var d = shortcuts[currentIndex];
            lblFilePath.Text             = d.FilePath;
            lblPosition.Text             = $"{currentIndex + 1} / {shortcuts.Count}";
            txtName.Text                 = Path.GetFileNameWithoutExtension(d.FilePath);
            txtTarget.Text               = d.TargetPath;
            txtArguments.Text            = d.Arguments;
            txtWorkDir.Text              = d.WorkingDir;
            txtDescription.Text          = d.Description;
            txtIconPath.Text             = d.IconPath;
            nudIconIndex.Value           = Math.Max(0, Math.Min(9999, d.IconIndex));
            cmbWindowStyle.SelectedIndex = d.WindowStyle == 3 ? 1 : d.WindowStyle == 7 ? 2 : 0;
            txtHotkey.Text               = d.Hotkey;
            isLoading = false;
        }

        void CollectFieldsIntoCurrent()
        {
            if (isLoading || currentIndex < 0 || currentIndex >= shortcuts.Count) return;
            var d = shortcuts[currentIndex];
            d.TargetPath   = txtTarget.Text;
            d.Arguments    = txtArguments.Text;
            d.WorkingDir   = txtWorkDir.Text;
            d.Description  = txtDescription.Text;
            d.IconPath     = txtIconPath.Text;
            d.IconIndex    = (int)nudIconIndex.Value;
            d.WindowStyle  = cmbWindowStyle.SelectedIndex == 1 ? 3 : cmbWindowStyle.SelectedIndex == 2 ? 7 : 1;
            d.Hotkey       = txtHotkey.Text;
        }

        void EnableFields(bool en)
        {
            foreach (Control c in new Control[]
            {
                txtName, txtTarget, txtArguments, txtWorkDir, txtDescription,
                txtIconPath, nudIconIndex, cmbWindowStyle, txtHotkey,
                btnBrowseTarget, btnBrowseWorkDir, btnBrowseIcon
            })
                c.Enabled = en;
        }

        void UpdateButtons()
        {
            bool any = shortcuts.Count > 0;
            btnPrev.Enabled        = currentIndex > 0;
            btnNext.Enabled        = currentIndex < shortcuts.Count - 1;
            btnSaveCurrent.Enabled = any;
            btnSaveAll.Enabled     = any;
            btnClear.Enabled       = any;
            btnReplace.Enabled     = any;
            if (!any) lblPosition.Text = "—/—";
        }

        void RefreshListItem(int idx)
        {
            if (idx < 0 || idx >= shortcuts.Count) return;
            bool was = isLoading; isLoading = true;
            lstFiles.Items[idx] = shortcuts[idx].DisplayName;
            lstFiles.Invalidate();
            isLoading = was;
        }

        void SetStatus(string msg, bool error = false)
        {
            lblStatus.Text      = msg;
            lblStatus.ForeColor = error ? Color.DarkRed : Color.DimGray;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Event handlers — load / navigate / field change
        // ═════════════════════════════════════════════════════════════════════
        void OnLoad(object s, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Select Shortcut Files (.lnk)",
                Filter = "Shortcuts (*.lnk)|*.lnk|All Files (*.*)|*.*",
                Multiselect = true
            };
            if (dlg.ShowDialog() == DialogResult.OK) LoadFiles(dlg.FileNames);
        }

        void OnDrop(object s, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var lnks  = files.Where(f => f.EndsWith(".lnk", StringComparison.OrdinalIgnoreCase)).ToArray();
            if (lnks.Length == 0) { SetStatus("No .lnk files in the dropped items.", true); return; }
            LoadFiles(lnks);
        }

        void LoadFiles(string[] files)
        {
            int added = 0, dupes = 0, failed = 0;
            var errors = new List<string>();
            foreach (var f in files)
            {
                if (shortcuts.Any(x => x.FilePath.Equals(f, StringComparison.OrdinalIgnoreCase)))
                { dupes++; continue; }
                try { shortcuts.Add(ReadLnk(f)); added++; }
                catch (Exception ex) { errors.Add(ex.Message); failed++; }
            }
            if (currentIndex < 0 && shortcuts.Count > 0) currentIndex = 0;
            RefreshAll();
            string msg = $"Loaded {added} file(s).";
            if (dupes  > 0) msg += $"  {dupes} duplicate(s) skipped.";
            if (failed > 0) msg += $"  {failed} failed.";
            SetStatus(msg, failed > 0);
            if (errors.Count > 0) MessageBox.Show(string.Join(Environment.NewLine, errors), "Load Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        void OnListSelect(object s, EventArgs e)
        {
            if (isLoading) return;
            int sel = lstFiles.SelectedIndex;
            if (sel < 0 || sel == currentIndex) return;
            CollectFieldsIntoCurrent();
            currentIndex = sel;
            LoadFieldsFromCurrent();
            UpdateButtons();
        }

        void Navigate(int delta)
        {
            CollectFieldsIntoCurrent();
            currentIndex = Math.Max(0, Math.Min(shortcuts.Count - 1, currentIndex + delta));
            isLoading = true;
            lstFiles.SelectedIndex = currentIndex;
            isLoading = false;
            LoadFieldsFromCurrent();
            UpdateButtons();
        }

        void OnFieldChanged(object s, EventArgs e)
        {
            if (isLoading || currentIndex < 0 || currentIndex >= shortcuts.Count) return;
            shortcuts[currentIndex].IsDirty = true;
            RefreshListItem(currentIndex);
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Save
        // ═════════════════════════════════════════════════════════════════════
        void OnSaveCurrent(object s, EventArgs e)
        {
            if (currentIndex < 0 || currentIndex >= shortcuts.Count) return;
            CollectFieldsIntoCurrent();
            var d = shortcuts[currentIndex];
            try
            {
                string newName = txtName.Text.Trim();
                if (newName.Length > 0) TryRename(d, newName);
                WriteLnk(d);
                d.IsDirty = false;
                lblFilePath.Text = d.FilePath;
                RefreshListItem(currentIndex);
                SetStatus($"Saved:  {d.FileName}");
            }
            catch (Exception ex) { SetStatus(ex.Message, true); }
        }

        void OnSaveAll(object s, EventArgs e)
        {
            // Flush name field for current item first
            if (currentIndex >= 0 && currentIndex < shortcuts.Count)
            {
                CollectFieldsIntoCurrent();
                string newName = txtName.Text.Trim();
                if (newName.Length > 0)
                    try { TryRename(shortcuts[currentIndex], newName); }
                    catch (Exception ex) { SetStatus(ex.Message, true); }
            }

            int ok = 0;
            var errors = new List<string>();
            for (int i = 0; i < shortcuts.Count; i++)
            {
                try { WriteLnk(shortcuts[i]); shortcuts[i].IsDirty = false; RefreshListItem(i); ok++; }
                catch (Exception ex) { errors.Add(ex.Message); }
            }

            if (currentIndex >= 0 && currentIndex < shortcuts.Count)
                lblFilePath.Text = shortcuts[currentIndex].FilePath;

            SetStatus($"Saved {ok} / {shortcuts.Count} file(s)." +
                      (errors.Count > 0 ? $"  {errors.Count} error(s)." : ""), errors.Count > 0);
            if (errors.Count > 0)
                MessageBox.Show(string.Join(Environment.NewLine, errors), "Save Errors", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        // Renames the .lnk file on disk and updates d.FilePath
        void TryRename(ShortcutData d, string newBaseName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
                newBaseName = newBaseName.Replace(c.ToString(), "");
            newBaseName = newBaseName.Trim();
            if (newBaseName.Length == 0) return;

            string currentBase = Path.GetFileNameWithoutExtension(d.FilePath);
            if (string.Equals(currentBase, newBaseName, StringComparison.OrdinalIgnoreCase)) return;

            string dir     = Path.GetDirectoryName(d.FilePath);
            string newPath = Path.Combine(dir, newBaseName + ".lnk");
            if (File.Exists(newPath))
                throw new Exception($"Cannot rename: '{newBaseName}.lnk' already exists.");
            File.Move(d.FilePath, newPath);
            d.FilePath = newPath;
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Find & Replace
        // ═════════════════════════════════════════════════════════════════════
        void OnReplaceAll(object s, EventArgs e)
        {
            if (shortcuts.Count == 0) return;

            string find    = txtFind.Text;
            string replace = txtReplace.Text;

            if (find.Length == 0)
            {
                SetStatus("Replace: Find text is empty.", error: true);
                return;
            }

            // Flush the form fields into the current shortcut before we start
            CollectFieldsIntoCurrent();

            int scope     = cmbReplaceScope.SelectedIndex;
            int fileCount = 0;
            int hitCount  = 0;

            foreach (var d in shortcuts)
            {
                bool changed = false;
                string Rep(string src)
                {
                    if (!src.Contains(find)) return src;
                    hitCount++;
                    changed = true;
                    return src.Replace(find, replace);
                }

                bool doTarget = scope == 0 || scope == 5;
                bool doWorkDir= scope == 1 || scope == 5;
                bool doIcon   = scope == 2 || scope == 5;
                bool doArgs   = scope == 3 || scope == 5;
                bool doDesc   = scope == 4 || scope == 5;

                if (doTarget)  d.TargetPath  = Rep(d.TargetPath);
                if (doWorkDir) d.WorkingDir  = Rep(d.WorkingDir);
                if (doIcon)    d.IconPath    = Rep(d.IconPath);
                if (doArgs)    d.Arguments   = Rep(d.Arguments);
                if (doDesc)    d.Description = Rep(d.Description);

                if (changed)
                {
                    d.IsDirty = true;
                    fileCount++;
                    RefreshListItem(shortcuts.IndexOf(d));
                }
            }

            // Re-populate the current item's fields from the (possibly updated) model
            LoadFieldsFromCurrent();

            if (hitCount == 0)
                SetStatus("Replace: no matches found.", error: true);
            else
                SetStatus($"Replaced {hitCount} occurrence(s) across {fileCount} file(s) — save to apply.");
        }

        // ═════════════════════════════════════════════════════════════════════
        //  Browse dialogs
        // ═════════════════════════════════════════════════════════════════════
        void BrowseExe(TextBox t)
        {
            using var dlg = new OpenFileDialog
            {
                Filter   = "Executables & Scripts (*.exe;*.bat;*.cmd;*.ps1)|*.exe;*.bat;*.cmd;*.ps1|All Files (*.*)|*.*",
                FileName = t.Text.Length > 0 ? t.Text : ""
            };
            if (dlg.ShowDialog() == DialogResult.OK) t.Text = dlg.FileName;
        }

        void BrowseFolder(TextBox t)
        {
            using var dlg = new FolderBrowserDialog { SelectedPath = t.Text };
            if (dlg.ShowDialog() == DialogResult.OK) t.Text = dlg.SelectedPath;
        }

        void BrowseIcon()
        {
            using var dlg = new OpenFileDialog
            {
                Title    = "Pick Icon Source",
                Filter   = "Icon sources (*.exe;*.dll;*.ico)|*.exe;*.dll;*.ico|All Files (*.*)|*.*",
                FileName = txtIconPath.Text.Length > 0 ? txtIconPath.Text : ""
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                txtIconPath.Text   = dlg.FileName;
                nudIconIndex.Value = 0;
            }
        }
    }
}
