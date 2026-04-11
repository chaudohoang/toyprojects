using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace CredentialManager
{
    public class MainForm : Form
    {
        private Panel           pnlSidebar, pnlMain, pnlTitleBar, pnlHeader, pnlFooter;
        private Label           lblCategories, lblSectionTitle, lblSectionSub;
        private Button          btnLoadCsv, btnNewCategory, btnAddEntry;
        private FlowLayoutPanel pnlTabs, pnlCards;
        private TextBox         txtSearch;

        private readonly List<CsvCategory> _categories = new List<CsvCategory>();
        private CsvCategory _activeCategory;

        // Card panel cache: avoids rebuilding UI on every category switch
        private readonly Dictionary<CsvCategory, List<Panel>> _cardCache = new Dictionary<CsvCategory, List<Panel>>();

        // Drag-reorder
        private Button _dragTab;
        private int    _dragStartY;
        private bool   _dragging;

        private static readonly string SettingsPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");

        // ── Palette ───────────────────────────────────────────────────
        private static readonly Color BgPage       = Color.FromArgb(245, 244, 241);
        private static readonly Color BgPanel      = Color.FromArgb(255, 255, 255);
        private static readonly Color BgSidebar    = Color.FromArgb(250, 249, 247);
        private static readonly Color BgInput      = Color.FromArgb(252, 251, 249);
        private static readonly Color BgReadOnly   = Color.FromArgb(245, 244, 241);
        private static readonly Color Border       = Color.FromArgb(220, 218, 212);
        private static readonly Color TextPrim     = Color.FromArgb( 28,  27,  25);
        private static readonly Color TextSec      = Color.FromArgb( 90,  88,  84);
        private static readonly Color TextHint     = Color.FromArgb(148, 146, 140);
        private static readonly Color Accent       = Color.FromArgb( 55, 138, 221);
        private static readonly Color CopiedGreen  = Color.FromArgb( 29, 158, 117);
        private static readonly Color DragHover    = Color.FromArgb(235, 234, 228);
        private static readonly Color DangerRed    = Color.FromArgb(200,  60,  60);
        private static readonly Color AddGreen     = Color.FromArgb( 29, 158, 117);
        private static readonly Color EditBlue     = Color.FromArgb( 55, 138, 221);
        private static readonly Color DoneOrange   = Color.FromArgb(186, 117,  23);

        private static readonly Color[] DotColors =
        {
            Color.FromArgb( 55, 138, 221),
            Color.FromArgb( 29, 158, 117),
            Color.FromArgb(186, 117,  23),
            Color.FromArgb(212,  83, 126),
            Color.FromArgb(127, 119, 221),
            Color.FromArgb(226,  75,  74),
        };

        // ═════════════════════════════════════════════════════════════
        public MainForm()
        {
            InitializeComponent();
            Text          = "Credential Manager";
            Size          = new Size(820, 580);
            MinimumSize   = new Size(680, 480);
            BackColor     = BgPage;
            Font          = new Font("Segoe UI", 9.5f);
            StartPosition = FormStartPosition.CenterScreen;
            Icon          = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            Load        += MainForm_Load;
            FormClosing += MainForm_FormClosing;
        }

        // ═════════════════════════════════════════════════════════════
        //  STARTUP / SHUTDOWN
        // ═════════════════════════════════════════════════════════════
        private void MainForm_Load(object sender, EventArgs e)
        {
            if (!File.Exists(SettingsPath)) return;
            foreach (var path in File.ReadAllLines(SettingsPath).Select(l => l.Trim()).Where(l => l.Length > 0))
            {
                if (!File.Exists(path)) continue;
                try { _categories.Add(CsvCategory.FromFile(path)); } catch { }
            }
            if (_categories.Count > 0) { RebuildSidebar(); ActivateCategory(_categories[0]); }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
            => File.WriteAllLines(SettingsPath, _categories.Select(c => c.FilePath), Encoding.UTF8);

        // ═════════════════════════════════════════════════════════════
        //  UI INIT
        // ═════════════════════════════════════════════════════════════
        private void InitializeComponent()
        {
            SuspendLayout();

            // Title bar
            pnlTitleBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(250, 249, 247) };
            pnlTitleBar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border, 1), 0, pnlTitleBar.Height - 1, pnlTitleBar.Width, pnlTitleBar.Height - 1);
            var lockIcon = new PictureBox { Size = new Size(20, 20), Left = 12, Top = 10, BackColor = Color.Transparent };
            lockIcon.Paint += (s, e) => DrawLockIcon(e.Graphics, lockIcon.ClientRectangle);
            pnlTitleBar.Controls.Add(lockIcon);
            pnlTitleBar.Controls.Add(new Label { Text = "Credential Manager", Font = new Font("Segoe UI", 10f), ForeColor = TextPrim, AutoSize = true, Left = 38, Top = 11 });

            // Sidebar
            pnlSidebar = new Panel { Dock = DockStyle.Left, Width = 168, BackColor = BgSidebar };
            pnlSidebar.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border, 1), pnlSidebar.Width - 1, 0, pnlSidebar.Width - 1, pnlSidebar.Height);
            lblCategories = new Label { Text = "CATEGORIES", Font = new Font("Segoe UI", 7.5f), ForeColor = TextHint, AutoSize = false, Width = 152, Height = 22, Left = 8, Top = 10, TextAlign = ContentAlignment.MiddleLeft };
            pnlSidebar.Controls.Add(lblCategories);

            pnlTabs = new FlowLayoutPanel { Left = 0, Top = 34, Width = 168, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.Transparent, Padding = new Padding(0), AllowDrop = true };
            pnlTabs.DragOver += PnlTabs_DragOver;
            pnlTabs.DragDrop += PnlTabs_DragDrop;
            pnlSidebar.Controls.Add(pnlTabs);

            btnNewCategory = MakeSidebarBtn("+ New category", -1, isAdd: true);
            btnNewCategory.Click += BtnNewCategory_Click;
            pnlSidebar.Controls.Add(btnNewCategory);

            btnLoadCsv = MakeSidebarBtn("+ Load CSV", -1, isAdd: true);
            btnLoadCsv.Click += BtnLoadCsv_Click;
            pnlSidebar.Controls.Add(btnLoadCsv);
            PositionLoadBtn();
            pnlSidebar.Resize += (s, e) => PositionLoadBtn();

            // ── Fixed header (title row + search box) ────────────────
            pnlHeader = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = BgPage };
            pnlHeader.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border, 1), 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);

            lblSectionTitle = new Label { Left = 20, Top = 10, AutoSize = true, Font = new Font("Segoe UI", 11f), ForeColor = TextPrim, Text = "No category loaded" };
            lblSectionSub   = new Label { Left = 20, Top = 32, AutoSize = true, Font = new Font("Segoe UI", 8.5f), ForeColor = TextHint, Text = "Use '+ Load CSV' in the sidebar to begin" };

            txtSearch = new TextBox { Width = 190, Height = 24, BorderStyle = BorderStyle.FixedSingle, Font = new Font("Segoe UI", 9f), BackColor = BgPanel, ForeColor = TextSec, Visible = false, Top = 16 };
            SetCueBanner(txtSearch, "Search...");
            txtSearch.TextChanged += (s, e) => RenderCards();

            pnlHeader.Controls.Add(lblSectionTitle);
            pnlHeader.Controls.Add(lblSectionSub);
            pnlHeader.Controls.Add(txtSearch);
            pnlHeader.Resize += (s, e) => { txtSearch.Left = pnlHeader.Width - txtSearch.Width - 20; };

            // ── Fixed footer (Add entry button) ──────────────────────
            pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = BgPage, Visible = false };
            pnlFooter.Paint += (s, e) =>
                e.Graphics.DrawLine(new Pen(Border, 1), 0, 0, pnlFooter.Width, 0);

            btnAddEntry = new Button
            {
                Height = 32, Width = 560, Left = 20, Top = 9,
                Text = "+ Add entry", FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f), ForeColor = AddGreen,
                BackColor = Color.Transparent, Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
            };
            btnAddEntry.FlatAppearance.BorderSize         = 1;
            btnAddEntry.FlatAppearance.BorderColor        = AddGreen;
            btnAddEntry.FlatAppearance.MouseOverBackColor = Color.FromArgb(240, 250, 246);
            btnAddEntry.Click += (s, e) => AddNewEntry();
            pnlFooter.Controls.Add(btnAddEntry);

            // ── Scrollable cards area ─────────────────────────────────
            pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = BgPage, AutoScroll = true };
            pnlCards = new FlowLayoutPanel { Left = 20, Top = 12, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, BackColor = Color.Transparent };
            pnlMain.Controls.Add(pnlCards);

            // The right-side content area is pnlContent, holding header + scroll + footer
            var pnlContent = new Panel { Dock = DockStyle.Fill };
            pnlContent.Controls.Add(pnlMain);     // Fill — must be added first
            pnlContent.Controls.Add(pnlFooter);   // Bottom
            pnlContent.Controls.Add(pnlHeader);   // Top

            Controls.Add(pnlContent);
            Controls.Add(pnlSidebar);
            Controls.Add(pnlTitleBar);
            ResumeLayout(false);
        }

        private void PositionLoadBtn()
        {
            btnLoadCsv.Left     = 8;
            btnLoadCsv.Top      = pnlSidebar.Height - btnLoadCsv.Height - 10;
            btnNewCategory.Left = 8;
            btnNewCategory.Top  = btnLoadCsv.Top - btnNewCategory.Height - 4;
        }

        // ═════════════════════════════════════════════════════════════
        //  CSV LOADING
        // ═════════════════════════════════════════════════════════════
        private void BtnLoadCsv_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", Title = "Load credential CSV", Multiselect = true })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                foreach (var path in dlg.FileNames) TryLoadFile(path);
            }
            RebuildSidebar();
            if (_activeCategory == null && _categories.Count > 0) ActivateCategory(_categories[0]);
        }

        private void BtnNewCategory_Click(object sender, EventArgs e)
        {
            // Ask for category name
            string name = PromptDialog("New category", "Category name (becomes the filename):", "");
            if (string.IsNullOrWhiteSpace(name)) return;
            name = name.Trim();

            // Sanitise: strip characters not valid in filenames
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c.ToString(), "");
            if (string.IsNullOrWhiteSpace(name)) { MessageBox.Show("That name contains no valid characters.", "Invalid name", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // Ask where to save
            string savePath;
            using (var dlg = new SaveFileDialog
            {
                Title      = "Save new category CSV",
                FileName   = name + ".csv",
                Filter     = "CSV files (*.csv)|*.csv",
                DefaultExt = "csv"
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                savePath = dlg.FileName;
            }

            // Ask for column names (comma-separated)
            string colInput = PromptDialog("Columns", "Column names (comma-separated):", "Name,Username,Password,Email");
            if (colInput == null) return;   // cancelled
            var columns = colInput.Split(',').Select(c => c.Trim()).Where(c => c.Length > 0).ToList();
            if (columns.Count == 0) columns = new List<string> { "Name" };

            // Write header-only CSV
            try
            {
                File.WriteAllText(savePath, string.Join(",", columns) + Environment.NewLine, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not create file:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TryLoadFile(savePath);
            RebuildSidebar();
            var created = _categories.FirstOrDefault(c => c.FilePath == savePath);
            if (created != null) ActivateCategory(created);
        }

        // Simple one-line input dialog
        private string PromptDialog(string title, string prompt, string defaultValue)
        {
            var form  = new Form { Text = title, Size = new Size(380, 140), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false, Font = new Font("Segoe UI", 9.5f) };
            var lbl   = new Label  { Left = 16, Top = 14, Width = 340, Text = prompt, AutoSize = false, Height = 20 };
            var txt   = new TextBox{ Left = 16, Top = 36, Width = 340, Text = defaultValue, BorderStyle = BorderStyle.FixedSingle };
            var ok    = new Button { Text = "OK",     Left = 196, Top = 68, Width = 80, Height = 28, DialogResult = DialogResult.OK,     FlatStyle = FlatStyle.System };
            var cancel= new Button { Text = "Cancel", Left = 284, Top = 68, Width = 80, Height = 28, DialogResult = DialogResult.Cancel, FlatStyle = FlatStyle.System };
            txt.SelectAll();
            form.Controls.AddRange(new Control[] { lbl, txt, ok, cancel });
            form.AcceptButton = ok;
            form.CancelButton = cancel;
            return form.ShowDialog(this) == DialogResult.OK ? txt.Text : null;
        }

        private void TryLoadFile(string path)
        {
            try
            {
                var cat      = CsvCategory.FromFile(path);
                var existing = _categories.FirstOrDefault(c => c.Name.Equals(cat.Name, StringComparison.OrdinalIgnoreCase));
                if (existing != null)
                {
                    InvalidateCache(existing);
                    int idx = _categories.IndexOf(existing);
                    _categories[idx] = cat;
                    if (_activeCategory == existing) _activeCategory = cat;
                }
                else _categories.Add(cat);
            }
            catch (Exception ex) { MessageBox.Show($"Could not load {Path.GetFileName(path)}:\n{ex.Message}", "Load error", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        // ═════════════════════════════════════════════════════════════
        //  SIDEBAR
        // ═════════════════════════════════════════════════════════════
        private void RebuildSidebar()
        {
            pnlTabs.Controls.Clear();
            for (int i = 0; i < _categories.Count; i++)
            {
                var cat = _categories[i];
                var btn = MakeSidebarBtn(cat.Name, i);
                btn.Tag      = cat;
                btn.Click   += (s, e) => { if (!_dragging) ActivateCategory((CsvCategory)((Button)s).Tag); };
                btn.MouseUp += TabBtn_RightClick;
                btn.MouseDown += TabBtn_MouseDown;
                btn.MouseMove += TabBtn_MouseMove;
                btn.MouseUp   += TabBtn_MouseUp;
                pnlTabs.Controls.Add(btn);
            }
            HighlightActiveTab();
        }

        private Button MakeSidebarBtn(string text, int colorIndex, bool isAdd = false)
        {
            var btn = new Button { Width = 152, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.Transparent, ForeColor = isAdd ? TextHint : TextSec, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(isAdd ? 26 : 22, 0, 0, 0), Font = new Font("Segoe UI", 9f), Cursor = Cursors.Hand, Text = text, Margin = new Padding(8, 2, 8, 2), AllowDrop = !isAdd };
            btn.FlatAppearance.BorderSize         = isAdd ? 1 : 0;
            btn.FlatAppearance.BorderColor        = isAdd ? Border : BgSidebar; // Transparent not allowed; match bg to hide
            btn.FlatAppearance.MouseOverBackColor = DragHover;
            if (!isAdd && colorIndex >= 0)
            {
                var dot = DotColors[colorIndex % DotColors.Length];
                btn.Paint += (s, e) => { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; e.Graphics.FillEllipse(new SolidBrush(dot), 8, 11, 8, 8); };
            }
            return btn;
        }

        private void HighlightActiveTab()
        {
            foreach (Control c in pnlTabs.Controls)
                if (c is Button btn && btn.Tag is CsvCategory cat)
                { bool a = cat == _activeCategory; btn.BackColor = a ? BgPanel : Color.Transparent; btn.ForeColor = a ? TextPrim : TextSec; btn.FlatAppearance.BorderSize = a ? 1 : 0; btn.FlatAppearance.BorderColor = a ? Border : BgSidebar; }
        }

        // ── Right-click context menu ──────────────────────────────────
        private void TabBtn_RightClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;
            var btn = sender as Button;
            if (btn == null || !(btn.Tag is CsvCategory)) return;
            var cat = (CsvCategory)btn.Tag;
            var menu = new ContextMenuStrip { Font = new Font("Segoe UI", 9f) };

            var r = new ToolStripMenuItem("↺  Reload from file");
            r.Click += (s, _) =>
            {
                if (!File.Exists(cat.FilePath)) { MessageBox.Show($"File not found:\n{cat.FilePath}", "Reload error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                TryLoadFile(cat.FilePath); RebuildSidebar();
                var refreshed = _categories.FirstOrDefault(c => c.Name.Equals(cat.Name, StringComparison.OrdinalIgnoreCase));
                if (refreshed != null) ActivateCategory(refreshed);
            };
            var f = new ToolStripMenuItem("📂  Open file location");
            f.Click += (s, _) => { try { var d = Path.GetDirectoryName(cat.FilePath); if (Directory.Exists(d)) System.Diagnostics.Process.Start("explorer.exe", d); } catch { } };
            var x = new ToolStripMenuItem("✕  Remove from list");
            x.Click += (s, _) =>
            {
                _categories.Remove(cat);
                if (_activeCategory == cat) _activeCategory = null;
                RebuildSidebar();
                if (_activeCategory == null && _categories.Count > 0) ActivateCategory(_categories[0]);
                else if (_categories.Count == 0) ClearMainArea();
            };

            menu.Items.Add(r); menu.Items.Add(f); menu.Items.Add(new ToolStripSeparator()); menu.Items.Add(x);
            menu.Show(btn, e.Location);
        }

        // ── Drag-reorder ──────────────────────────────────────────────
        // ── Drag-reorder ──────────────────────────────────────────────
        // We pass the tab index as a string because Button is not OLE-serializable.
        private void TabBtn_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _dragTab    = sender as Button;
                _dragStartY = e.Y;
                _dragging   = false;
            }
        }

        private void TabBtn_MouseMove(object sender, MouseEventArgs e)
        {
            if (_dragTab == null || e.Button != MouseButtons.Left) return;
            if (Math.Abs(e.Y - _dragStartY) < 8) return;

            // Find the index of the button being dragged
            int idx = pnlTabs.Controls.IndexOf(_dragTab);
            if (idx < 0) return;

            _dragging = true;
            _dragTab.DoDragDrop(idx.ToString(), DragDropEffects.Move);
            _dragTab  = null;
            _dragging = false;
        }

        private void TabBtn_MouseUp(object sender, MouseEventArgs e) { _dragTab = null; _dragging = false; }

        private void PnlTabs_DragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.StringFormat)) { e.Effect = DragDropEffects.None; return; }
            e.Effect = DragDropEffects.Move;
            var pt = pnlTabs.PointToClient(new Point(e.X, e.Y));
            var target = ClosestTabAt(pt.Y);
            foreach (Control c in pnlTabs.Controls)
                if (c is Button b && b.Tag is CsvCategory)
                    b.BackColor = (b == target) ? DragHover : Color.Transparent;
        }

        private void PnlTabs_DragDrop(object sender, DragEventArgs e)
        {
            foreach (Control c in pnlTabs.Controls)
                if (c is Button b) b.BackColor = Color.Transparent;

            if (!e.Data.GetDataPresent(DataFormats.StringFormat)) return;
            int fromIdx;
            if (!int.TryParse((string)e.Data.GetData(DataFormats.StringFormat), out fromIdx)) return;

            var pt    = pnlTabs.PointToClient(new Point(e.X, e.Y));
            var toBtn = ClosestTabAt(pt.Y);
            if (toBtn == null) { RebuildSidebar(); return; }

            int toIdx = pnlTabs.Controls.IndexOf(toBtn);
            if (fromIdx == toIdx || fromIdx < 0 || toIdx < 0
                || fromIdx >= _categories.Count || toIdx >= _categories.Count)
            { RebuildSidebar(); return; }

            var moved = _categories[fromIdx];
            _categories.RemoveAt(fromIdx);
            _categories.Insert(toIdx, moved);
            RebuildSidebar();
            HighlightActiveTab();
        }

        // Find the tab button whose vertical midpoint is closest to a given Y
        private Button ClosestTabAt(int y)
        {
            Button best    = null;
            int    bestDist = int.MaxValue;
            foreach (Control c in pnlTabs.Controls)
            {
                if (!(c is Button b) || !(b.Tag is CsvCategory)) continue;
                int mid  = b.Top + b.Height / 2;
                int dist = Math.Abs(mid - y);
                if (dist < bestDist) { bestDist = dist; best = b; }
            }
            return best;
        }

        // ═════════════════════════════════════════════════════════════
        //  CATEGORY ACTIVATION
        // ═════════════════════════════════════════════════════════════
        private void ActivateCategory(CsvCategory cat)
        {
            _activeCategory      = cat;
            HighlightActiveTab();
            lblSectionTitle.Text = cat.Name;
            lblSectionSub.Text   = Path.GetFileName(cat.FilePath);
            txtSearch.Visible    = true;
            txtSearch.Left       = pnlHeader.Width - txtSearch.Width - 20;
            txtSearch.Text       = string.Empty;
            pnlFooter.Visible    = true;
            RenderCards();
        }

        private void ClearMainArea()
        {
            lblSectionTitle.Text = "No category loaded";
            lblSectionSub.Text   = "Use '+ Load CSV' in the sidebar to begin";
            txtSearch.Visible    = false;
            pnlFooter.Visible    = false;
            pnlCards.Controls.Clear();
        }

        // ═════════════════════════════════════════════════════════════
        //  CARD RENDERING  (cached per category)
        // ═════════════════════════════════════════════════════════════

        // Call this whenever a category's data changes so it gets rebuilt next switch
        private void InvalidateCache(CsvCategory cat)
        {
            if (cat != null && _cardCache.ContainsKey(cat))
                _cardCache.Remove(cat);
        }

        private void RenderCards()
        {
            pnlCards.Controls.Clear();
            if (_activeCategory == null) return;

            var filter       = txtSearch.Text.Trim().ToLower();
            bool isFiltering = filter.Length > 0;

            // ── Build or retrieve cached cards ────────────────────────
            if (!_cardCache.ContainsKey(_activeCategory))
            {
                var built = new List<Panel>();
                foreach (var row in _activeCategory.Rows)
                    built.Add(BuildCard(row, 560));
                _cardCache[_activeCategory] = built;
            }

            var allCards = _cardCache[_activeCategory];

            // ── Apply filter (just pick matching cards, no rebuild) ───
            List<Panel> visible;
            if (isFiltering)
            {
                visible = new List<Panel>();
                for (int i = 0; i < allCards.Count; i++)
                {
                    var row = _activeCategory.Rows[i];
                    if (row.Values.Any(v => v.ToLower().Contains(filter)))
                        visible.Add(allCards[i]);
                }
            }
            else
            {
                visible = allCards;
            }

            foreach (var card in visible)
                pnlCards.Controls.Add(card);

            int count = isFiltering ? visible.Count : _activeCategory.Rows.Count;
            lblSectionTitle.Text = $"{_activeCategory.Name}  ({count} accounts)";
            pnlFooter.Visible    = !isFiltering;
        }

        // ═════════════════════════════════════════════════════════════
        //  BUILD ONE CARD
        //  All fields start read-only. Edit/Done button toggles editing.
        // ═════════════════════════════════════════════════════════════
        private Panel BuildCard(CredentialRow row, int width)
        {
            var card = new Panel
            {
                Width     = width,
                AutoSize  = false,
                BackColor = BgPanel,
                Padding   = new Padding(14, 10, 14, 12),
                Margin    = new Padding(0, 0, 0, 10)
            };
            card.Paint += (s, e) =>
                e.Graphics.DrawRectangle(new Pen(Border, 1), new Rectangle(0, 0, card.Width - 1, card.Height - 1));

            // Collect all TextBox controls so Edit/Done can toggle them all at once
            var editableFields = new List<TextBox>();

            int y = 10;

            // ── Header row: title textbox | [Edit] [Remove] ───────────
            string firstCol = row.Columns.Count > 0 ? row.Columns[0] : "";
            string firstVal = row.Values.Count  > 0 ? row.Values[0]  : "";

            // Edit and Remove buttons – built first so we know their widths for layout
            var btnEdit = new Button
            {
                Width     = 46,
                Height    = 22,
                Text      = "Edit",
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = EditBlue,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
            };
            btnEdit.FlatAppearance.BorderSize         = 1;
            btnEdit.FlatAppearance.BorderColor        = EditBlue;
            btnEdit.FlatAppearance.MouseOverBackColor = Color.FromArgb(237, 245, 255);

            var btnRemove = new Button
            {
                Width     = 58,
                Height    = 22,
                Text      = "Remove",
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = DangerRed,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand,
            };
            btnRemove.FlatAppearance.BorderSize         = 1;
            btnRemove.FlatAppearance.BorderColor        = DangerRed;
            btnRemove.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 243, 243);

            // Title textbox (read-only by default)
            int headerBtnW = btnEdit.Width + 4 + btnRemove.Width; // 46 + 4 + 58 = 108
            var txtTitle = MakeField(firstVal, 14, y, width - 28 - headerBtnW - 8, readOnly: true, isBold: true);
            SetCueBanner(txtTitle, firstCol.Length > 0 ? firstCol + "..." : "Name");
            txtTitle.Tag = (object)(Action)(() => { if (row.Values.Count > 0) row.Values[0] = txtTitle.Text; });
            editableFields.Add(txtTitle);
            card.Controls.Add(txtTitle);

            // Position Edit and Remove
            int btnRight  = width - 14;
            btnRemove.Left = btnRight - btnRemove.Width;
            btnRemove.Top  = y + 1;
            btnEdit.Left   = btnRemove.Left - btnEdit.Width - 4;
            btnEdit.Top    = y + 1;

            // Remove handler
            btnRemove.Click += (s, e) =>
            {
                var label = row.Values.Count > 0 ? row.Values[0] : "this entry";
                if (MessageBox.Show($"Remove \"{label}\"?", "Confirm remove", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
                _activeCategory.Rows.Remove(row);
                SaveCategory(_activeCategory);
                InvalidateCache(_activeCategory);
                RenderCards();
            };
            card.Controls.Add(btnEdit);
            card.Controls.Add(btnRemove);
            y += 28;

            // Divider
            card.Controls.Add(new Panel { Left = 14, Top = y, Width = width - 28, Height = 1, BackColor = Border });
            y += 8;

            // ── Field rows (skip col 0, shown as title above) ─────────
            for (int i = 1; i < row.Columns.Count; i++)
            {
                if (i >= row.Values.Count) break;
                int ci     = i;
                string col = row.Columns[i];
                string val = row.Values[i];
                bool   isPw = col.ToLower().Contains("pass");

                // Column label
                card.Controls.Add(new Label { Left = 14, Top = y + 5, Width = 88, Height = 20, Font = new Font("Segoe UI", 8.5f), ForeColor = TextHint, Text = col });

                int fieldW  = width - 28;
                int btnAreaW = isPw ? 118 : 72;
                int right    = 14 + fieldW;

                var txt = MakeField(val, 106, y + 3, fieldW - 88 - 6 - btnAreaW - 4, readOnly: true);
                SetCueBanner(txt, isPw ? "••••••••••" : "empty");
                if (isPw) { txt.UseSystemPasswordChar = true; txt.Text = val; }

                // Save value back on change (called when Done is clicked)
                txt.Tag = (object)(Action)(() => { row.Values[ci] = txt.Text; });
                editableFields.Add(txt);
                card.Controls.Add(txt);

                // Copy button
                var btnCopy = MakeSmallBtn("Copy", right - 64, y + 4, 64);
                btnCopy.Click += (s, e) =>
                {
                    try { Clipboard.SetText(txt.Text); } catch { }
                    btnCopy.Text = "Copied!"; btnCopy.ForeColor = CopiedGreen; btnCopy.FlatAppearance.BorderColor = CopiedGreen;
                    var t = new System.Windows.Forms.Timer { Interval = 1400 };
                    t.Tick += (ts, te) => { btnCopy.Text = "Copy"; btnCopy.ForeColor = TextSec; btnCopy.FlatAppearance.BorderColor = Border; t.Stop(); t.Dispose(); };
                    t.Start();
                };
                card.Controls.Add(btnCopy);

                // Show/hide for passwords
                if (isPw)
                {
                    var btnEye = MakeSmallBtn("show", right - 64 - 50, y + 4, 46);
                    btnEye.ForeColor = TextHint;
                    btnEye.Click += (s, e) =>
                    {
                        txt.UseSystemPasswordChar = !txt.UseSystemPasswordChar;
                        btnEye.Text = txt.UseSystemPasswordChar ? "show" : "hide";
                    };
                    card.Controls.Add(btnEye);
                }

                y += 28;
            }

            card.Height = y + 10;

            // ── Edit / Done toggle ────────────────────────────────────
            btnEdit.Click += (s, e) =>
            {
                bool entering = btnEdit.Text == "Edit";

                if (entering)
                {
                    // Switch to edit mode
                    SetFieldsReadOnly(editableFields, readOnly: false);
                    btnEdit.Text                       = "Done";
                    btnEdit.ForeColor                  = DoneOrange;
                    btnEdit.FlatAppearance.BorderColor = DoneOrange;
                    btnEdit.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 249, 237);
                    // Draw an amber border around the card to signal edit mode
                    card.Paint -= CardBorderNormal;
                    card.Paint += CardBorderEditing;
                    card.Invalidate();
                    // Focus first field
                    editableFields.FirstOrDefault()?.Focus();
                }
                else
                {
                    // Commit all values
                    foreach (var tb in editableFields)
                        if (tb.Tag is Action save) save();

                    SaveCategory(_activeCategory);
                    InvalidateCache(_activeCategory);

                    // Switch back to read-only
                    SetFieldsReadOnly(editableFields, readOnly: true);
                    btnEdit.Text                       = "Edit";
                    btnEdit.ForeColor                  = EditBlue;
                    btnEdit.FlatAppearance.BorderColor = EditBlue;
                    btnEdit.FlatAppearance.MouseOverBackColor = Color.FromArgb(237, 245, 255);
                    card.Paint -= CardBorderEditing;
                    card.Paint += CardBorderNormal;
                    card.Invalidate();
                }
            };

            // Initial border paint handler stored as named delegate via closure
            card.Paint += CardBorderNormal;

            return card;
        }

        // Named border paint handlers so we can add/remove them cleanly
        private void CardBorderNormal (object s, PaintEventArgs e)
        {
            var c = (Panel)s;
            e.Graphics.DrawRectangle(new Pen(Border, 1), new Rectangle(0, 0, c.Width - 1, c.Height - 1));
        }
        private void CardBorderEditing(object s, PaintEventArgs e)
        {
            var c = (Panel)s;
            using var pen = new Pen(DoneOrange, 2f);
            e.Graphics.DrawRectangle(pen, new Rectangle(1, 1, c.Width - 2, c.Height - 2));
        }

        // Toggle read-only on all editable TextBoxes in a card
        private void SetFieldsReadOnly(List<TextBox> fields, bool readOnly)
        {
            foreach (var tb in fields)
            {
                tb.ReadOnly   = readOnly;
                tb.BackColor  = readOnly ? BgReadOnly : BgInput;
                tb.TabStop    = !readOnly;
                tb.BorderStyle = readOnly ? BorderStyle.None : BorderStyle.FixedSingle;
            }
        }

        // ═════════════════════════════════════════════════════════════
        //  ADD NEW ENTRY
        // ═════════════════════════════════════════════════════════════
        private void AddNewEntry()
        {
            if (_activeCategory == null) return;
            // Use the category's defined columns (always available from the header)
            var columns = _activeCategory.Columns;
            if (columns == null || columns.Count == 0)
                columns = new List<string> { "Name" };
            var newRow = new CredentialRow { Columns = columns, Values = columns.Select(_ => "").ToList() };
            _activeCategory.Rows.Add(newRow);
            SaveCategory(_activeCategory);
            InvalidateCache(_activeCategory);
            RenderCards();
            pnlMain.AutoScrollPosition = new Point(0, pnlMain.DisplayRectangle.Height);
        }

        // ═════════════════════════════════════════════════════════════
        //  SAVE CSV
        // ═════════════════════════════════════════════════════════════
        private static void SaveCategory(CsvCategory cat)
        {
            try
            {
                var sb = new StringBuilder();
                if (cat.Rows.Count > 0)
                {
                    sb.AppendLine(string.Join(",", cat.Rows[0].Columns.Select(CsvEscape)));
                    foreach (var row in cat.Rows)
                        sb.AppendLine(string.Join(",", row.Values.Select(CsvEscape)));
                }
                File.WriteAllText(cat.FilePath, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex) { MessageBox.Show($"Could not save {Path.GetFileName(cat.FilePath)}:\n{ex.Message}", "Save error", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        }

        private static string CsvEscape(string v)
        {
            if (v == null) return "";
            return (v.Contains(',') || v.Contains('"') || v.Contains('\n'))
                ? "\"" + v.Replace("\"", "\"\"") + "\""
                : v;
        }

        // ═════════════════════════════════════════════════════════════
        //  HELPERS
        // ═════════════════════════════════════════════════════════════
        private TextBox MakeField(string text, int left, int top, int width, bool readOnly, bool isBold = false)
        {
            var tb = new TextBox
            {
                Left        = left,
                Top         = top,
                Width       = Math.Max(10, width),
                Height      = 22,
                Text        = text,
                Font        = new Font("Segoe UI", isBold ? 9.5f : 9f),
                ForeColor   = isBold ? TextPrim : TextSec,
                BackColor   = readOnly ? BgReadOnly : BgInput,
                BorderStyle = readOnly ? BorderStyle.None : BorderStyle.FixedSingle,
                ReadOnly    = readOnly,
                TabStop     = !readOnly,
            };
            return tb;
        }

        private Button MakeSmallBtn(string text, int left, int top, int width)
        {
            var b = new Button { Width = width, Height = 22, Left = left, Top = top, Text = text, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8f), ForeColor = TextSec, BackColor = Color.Transparent, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderSize         = 1;
            b.FlatAppearance.BorderColor        = Border;
            b.FlatAppearance.MouseOverBackColor = DragHover;
            return b;
        }

        private static void DrawLockIcon(Graphics g, Rectangle r)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var pen = new Pen(Accent, 1.4f) { LineJoin = LineJoin.Round };
            g.DrawRectangle(pen, new Rectangle(r.Left + 3, r.Top + 8, r.Width - 6, r.Height - 9));
            g.DrawArc(pen, r.Left + 4, r.Top + 2, r.Width - 9, 9, 180, 180);
        }

        // Win32 cue banner = placeholder text for TextBox on .NET 4.8
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        private static void SetCueBanner(TextBox tb, string text)
        {
            // EM_SETCUEBANNER must be called after the handle is created
            if (tb.IsHandleCreated)
                SendMessage(tb.Handle, EM_SETCUEBANNER, IntPtr.Zero, text);
            else
                tb.HandleCreated += (s, e) => SendMessage(tb.Handle, EM_SETCUEBANNER, IntPtr.Zero, text);
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    //  DATA MODELS
    // ═══════════════════════════════════════════════════════════════════
    public class CredentialRow
    {
        public List<string> Columns { get; set; }
        public List<string> Values  { get; set; }
    }

    public class CsvCategory
    {
        public string              Name     { get; private set; }
        public string              FilePath { get; private set; }
        public List<string>        Columns  { get; private set; }   // header columns, always available
        public List<CredentialRow> Rows     { get; private set; }

        public static CsvCategory FromFile(string path)
        {
            var lines = File.ReadAllLines(path, Encoding.UTF8);
            if (lines.Length < 1) throw new Exception("CSV file is empty.");
            var columns = ParseCsvLine(lines[0]);
            var rows    = new List<CredentialRow>();
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                var values = ParseCsvLine(line);
                while (values.Count < columns.Count) values.Add(string.Empty);
                rows.Add(new CredentialRow { Columns = columns, Values = values });
            }
            return new CsvCategory { Name = Path.GetFileNameWithoutExtension(path), FilePath = path, Columns = columns, Rows = rows };
        }

        private static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>(); var sb = new StringBuilder(); bool inQ = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"') { if (inQ && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; } else inQ = !inQ; }
                else if (c == ',' && !inQ) { fields.Add(sb.ToString().Trim()); sb.Clear(); }
                else sb.Append(c);
            }
            fields.Add(sb.ToString().Trim());
            return fields;
        }
    }
}
