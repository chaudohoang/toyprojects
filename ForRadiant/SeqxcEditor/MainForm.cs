using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace SeqxcEditor {

    partial class MainForm : Form {

        // ── Controls ──────────────────────────────────────────────────────────
        ToolStrip            toolbar;
        ToolStripButton      btnOpen, btnSave, btnSaveAs, btnAll, btnNone,
                             btnClone, btnDelete, btnPaste, btnClearClip;
        ToolStripDropDownButton btnClipMenu;
        ToolStripLabel       lblClipInfo;
        StatusStrip          statusStrip;
        ToolStripStatusLabel lblStatus;

        // Left panel
        Panel        pnlLeft;
        Panel        pnlStepHeader;
        Label        lblStepsTitle, lblCheckedCount;
        Button       btnMvUp, btnMvDown;
        DataGridView dgvSteps;

        // Right panel
        Panel   pnlRight;
        Panel   pnlStepInfo;
        Label   lblStepName, lblEnabled, lblStepIdx, lblShared;
        Panel   pnlTabBar;
        Button  btnTabAnalysis, btnTabPatSetup;
        Panel   pnlContent;
        PropGrid pgAnalysis, pgPat;

        // ── State ─────────────────────────────────────────────────────────────
        string          focusedGuid;
        HashSet<string> checkedGuids    = new HashSet<string>();
        bool            suppressEvents;
        List<ClipItem>  clipList        = new List<ClipItem>();
        bool            showingAnalysis = true;

        // ── Constructor ───────────────────────────────────────────────────────
        public MainForm(string initialFile = null) {
            Text          = "SeqXC Editor";
            Size          = new Size(1060, 700);
            MinimumSize   = new Size(740, 480);
            StartPosition = FormStartPosition.CenterScreen;
            Font          = new Font("Segoe UI", 9);
            try { string ico = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico"); if (File.Exists(ico)) Icon = new Icon(ico); } catch { }
            BuildUI();
            WireEvents();
            if (initialFile != null && File.Exists(initialFile)) OpenFile(initialFile);
        }

        // ══════════════════════════════════════════════════════════════════════
        // BUILD UI
        // ══════════════════════════════════════════════════════════════════════

        void BuildUI() {
            SuspendLayout();
            BuildToolbar();
            BuildStatusBar();
            BuildLeftPanel();
            BuildRightPanel();

            var splitter = new Splitter { Dock = DockStyle.Left, Width = 4, BackColor = Color.FromArgb(200, 200, 200) };

            Controls.Add(pnlRight);
            Controls.Add(splitter);
            Controls.Add(pnlLeft);
            Controls.Add(statusStrip);
            Controls.Add(toolbar);
            ResumeLayout(true);
        }

        void BuildToolbar() {
            toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(4, 2, 4, 2), BackColor = Color.FromArgb(242, 242, 242), RenderMode = ToolStripRenderMode.System };

            btnOpen      = TBtn("📂 Open",        Color.FromArgb(0, 100, 200),  "Open .seqxc  (Ctrl+O)");
            btnSave      = TBtn("💾 Save",         Color.FromArgb(0, 130, 55),   "Save  (Ctrl+S)");
            btnSaveAs    = TBtn("Save As\u2026",   Color.FromArgb(160, 90, 0),   "Save to new location");
            btnAll       = TBtn("All",             Color.DimGray,                "Check all steps");
            btnNone      = TBtn("None",            Color.DimGray,                "Uncheck all steps");
            btnClone     = TBtn("Clone (0)",       Color.FromArgb(0, 110, 160),  "Clone checked steps");
            btnDelete    = TBtn("Delete (0)",      Color.FromArgb(180, 0, 0),    "Delete checked steps");
            btnPaste     = TBtn("Paste (0) \u2192 0 steps", Color.FromArgb(100, 0, 160), "Paste copy list to checked steps");
            btnClearClip = TBtn("\u00d7 Clip",     Color.Gray,                   "Clear copy list");
            btnClipMenu  = new ToolStripDropDownButton("Copy list (0) \u25bc") { ForeColor = Color.DimGray, Font = new Font("Segoe UI", 9), ToolTipText = "View copy list" };
            btnClipMenu.DropDownOpening += OnClipMenuOpening;
            lblClipInfo  = new ToolStripLabel { ForeColor = Color.Gray, Font = new Font("Segoe UI", 8) };

            btnSave.Enabled = btnSaveAs.Enabled = btnClone.Enabled = btnDelete.Enabled =
            btnPaste.Enabled = btnClearClip.Enabled = false;

            foreach (var i in new ToolStripItem[] { btnOpen, btnSave, btnSaveAs,
                new ToolStripSeparator(), btnAll, btnNone,
                new ToolStripSeparator(), btnClone, btnDelete,
                new ToolStripSeparator(), btnPaste, btnClearClip, btnClipMenu,
                new ToolStripSeparator(), lblClipInfo }) toolbar.Items.Add(i);
        }

        void BuildStatusBar() {
            statusStrip = new StatusStrip { SizingGrip = false };
            lblStatus   = new ToolStripStatusLabel("Ready  \u2014  open a .seqxc file  (Ctrl+O or drag && drop)") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            statusStrip.Items.Add(lblStatus);
        }

        void BuildLeftPanel() {
            // ── Header ────────────────────────────────────────────────────────
            pnlStepHeader = new Panel { Dock = DockStyle.Top, Height = 34, BackColor = Color.FromArgb(230, 233, 238) };
            lblStepsTitle = new Label { Text = "SEQUENCE STEPS", Font = new Font("Segoe UI", 8, FontStyle.Bold), ForeColor = Color.FromArgb(70, 70, 70), AutoSize = true };
            btnMvUp   = MvBtn("\u25b2"); btnMvDown = MvBtn("\u25bc");
            lblCheckedCount = new Label { Text = "0 checked", Font = new Font("Segoe UI", 8), ForeColor = Color.Gray, AutoSize = true };
            var flow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(4, 5, 4, 0) };
            flow.Controls.Add(lblStepsTitle); flow.Controls.Add(btnMvUp); flow.Controls.Add(btnMvDown); flow.Controls.Add(lblCheckedCount);
            pnlStepHeader.Controls.Add(flow);

            // ── Step DataGridView ─────────────────────────────────────────────
            dgvSteps = new DataGridView {
                Dock                        = DockStyle.Fill,
                BorderStyle                 = BorderStyle.None,
                RowHeadersVisible           = false,
                AllowUserToAddRows          = false,
                AllowUserToDeleteRows       = false,
                AllowUserToResizeRows       = false,
                MultiSelect                 = false,
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
                EditMode                    = DataGridViewEditMode.EditProgrammatically,
                BackgroundColor             = SystemColors.Window,
                GridColor                   = Color.FromArgb(225, 225, 225),
                CellBorderStyle             = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersVisible        = false,
                EnableHeadersVisualStyles   = false,
                Font                        = new Font("Consolas", 9),
                ShowCellToolTips            = false,
            };
            dgvSteps.RowTemplate.Height = 22;
            dgvSteps.DefaultCellStyle.SelectionBackColor = Color.FromArgb(198, 218, 255);
            dgvSteps.DefaultCellStyle.SelectionForeColor = Color.Black;

            // Col 0: checkbox
            var chkCol = new DataGridViewCheckBoxColumn { Name = "colChk", Width = 28, MinimumWidth = 28, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, SortMode = DataGridViewColumnSortMode.NotSortable, Resizable = DataGridViewTriState.False };
            chkCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            chkCol.DefaultCellStyle.NullValue  = false;

            // Col 1: name (fill)
            var nameCol = new DataGridViewTextBoxColumn { Name = "colName", ReadOnly = true, Width = 170, MinimumWidth = 80, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, SortMode = DataGridViewColumnSortMode.NotSortable, Resizable = DataGridViewTriState.True };

            // Col 2: badge (fixed)
            var badgeCol = new DataGridViewTextBoxColumn { Name = "colBadge", ReadOnly = true, Width = 160, MinimumWidth = 60, AutoSizeMode = DataGridViewAutoSizeColumnMode.None, SortMode = DataGridViewColumnSortMode.NotSortable, Resizable = DataGridViewTriState.True };
            badgeCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            badgeCol.DefaultCellStyle.Font      = new Font("Segoe UI", 8, FontStyle.Bold);

            dgvSteps.Columns.Add(chkCol); dgvSteps.Columns.Add(nameCol); dgvSteps.Columns.Add(badgeCol);

            pnlLeft = new Panel { Dock = DockStyle.Left, Width = 380 };
            pnlLeft.Controls.Add(dgvSteps);
            pnlLeft.Controls.Add(pnlStepHeader);
        }

        void BuildRightPanel() {
            pnlStepInfo = new Panel { Dock = DockStyle.Top, Height = 68, BackColor = Color.FromArgb(248, 250, 253), Padding = new Padding(8, 4, 8, 2) };
            lblStepName = new Label { Dock = DockStyle.Top, Height = 28, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Color.FromArgb(20, 30, 70), AutoSize = false };
            lblEnabled  = new Label { AutoSize = true, Font = new Font("Segoe UI", 8.5f) };
            lblStepIdx  = new Label { AutoSize = true, Font = new Font("Segoe UI", 8), ForeColor = Color.Gray };
            lblShared   = new Label { Dock = DockStyle.Top, Height = 18, Font = new Font("Segoe UI", 8), ForeColor = Color.FromArgb(160, 90, 0), AutoSize = false };
            var row2 = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 20, FlowDirection = FlowDirection.LeftToRight, WrapContents = false, Padding = new Padding(0) };
            row2.Controls.Add(lblEnabled); row2.Controls.Add(lblStepIdx);
            pnlStepInfo.Controls.Add(lblStepName); pnlStepInfo.Controls.Add(row2); pnlStepInfo.Controls.Add(lblShared);

            pnlTabBar = new Panel { Dock = DockStyle.Top, Height = 32, BackColor = Color.FromArgb(240, 242, 245) };
            btnTabAnalysis = TabBtn("Analysis", true);
            btnTabPatSetup = TabBtn("PatternSetup", false);
            btnTabPatSetup.Location = new Point(btnTabAnalysis.Width + 4, 1);
            pnlTabBar.Controls.Add(btnTabAnalysis); pnlTabBar.Controls.Add(btnTabPatSetup);

            pgAnalysis = new PropGrid { Visible = true  };
            pgPat      = new PropGrid { Visible = false };
            pnlContent = new Panel { Dock = DockStyle.Fill };
            pnlContent.Controls.Add(pgAnalysis); pnlContent.Controls.Add(pgPat);

            pnlRight = new Panel { Dock = DockStyle.Fill };
            pnlRight.Controls.Add(pnlContent);
            pnlRight.Controls.Add(pnlTabBar);
            pnlRight.Controls.Add(pnlStepInfo);
        }

        static ToolStripButton TBtn(string t, Color c, string tip) =>
            new ToolStripButton(t) { ForeColor = c, Font = new Font("Segoe UI", 9, FontStyle.Bold), ToolTipText = tip, Margin = new Padding(2,1,2,1), DisplayStyle = ToolStripItemDisplayStyle.Text };

        static Button MvBtn(string t) {
            var b = new Button { Text = t, FlatStyle = FlatStyle.Flat, Size = new Size(28, 22), ForeColor = Color.ForestGreen, Cursor = Cursors.Hand };
            b.FlatAppearance.BorderColor = Color.FromArgb(0, 140, 60);
            return b;
        }

        static Button TabBtn(string t, bool active) {
            var b = new Button {
                Text      = t,
                FlatStyle = FlatStyle.Flat,
                Height    = 30,
                Width     = t.Length * 8 + 24,
                Location  = new Point(4, 1),
                Cursor    = Cursors.Hand,
                Font      = new Font("Segoe UI", 9, active ? FontStyle.Bold : FontStyle.Regular),
                ForeColor = active ? Color.FromArgb(0, 70, 160) : Color.DimGray,
                BackColor = active ? Color.White : Color.FromArgb(245, 245, 245),
            };
            b.FlatAppearance.BorderColor = active ? Color.FromArgb(0, 100, 200) : Color.Silver;
            return b;
        }

        // ══════════════════════════════════════════════════════════════════════
        // WIRE EVENTS
        // ══════════════════════════════════════════════════════════════════════

        void WireEvents() {
            btnOpen.Click       += (s, e) => PickAndOpen();
            btnSave.Click       += (s, e) => Save();
            btnSaveAs.Click     += (s, e) => SaveAs();
            btnAll.Click        += (s, e) => CheckAll(true);
            btnNone.Click       += (s, e) => CheckAll(false);
            btnClone.Click      += (s, e) => CloneChecked();
            btnDelete.Click     += (s, e) => DeleteChecked();
            btnPaste.Click      += (s, e) => PasteToChecked();
            btnClearClip.Click  += (s, e) => ClearClip();
            btnMvUp.Click       += (s, e) => MoveStep(-1);
            btnMvDown.Click     += (s, e) => MoveStep(1);
            btnTabAnalysis.Click+= (s, e) => SwitchTab(true);
            btnTabPatSetup.Click+= (s, e) => SwitchTab(false);

            // Step grid events
            dgvSteps.SelectionChanged  += OnStepSelectionChanged;
            dgvSteps.CellMouseClick    += OnStepCellMouseClick;
            dgvSteps.CellFormatting    += OnStepCellFormatting;

            pgAnalysis.StatusChanged  += SetStatus;
            pgPat.StatusChanged       += SetStatus;
            pgAnalysis.AddToClipList  += OnAddToClipList;
            pgPat.AddToClipList       += OnAddToClipList;

            KeyPreview = true;
            KeyDown   += (s, e) => { if (e.Control && e.KeyCode == Keys.O) { PickAndOpen(); e.Handled = true; } if (e.Control && e.KeyCode == Keys.S) { Save(); e.Handled = true; } };
            AllowDrop  = true;
            DragEnter += (s, e) => { if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy; };
            DragDrop  += (s, e) => { var f = e.Data.GetData(DataFormats.FileDrop) as string[]; if (f?.Length > 0) OpenFile(f[0]); };
        }

        // ── Step grid event handlers ──────────────────────────────────────────

        void OnStepSelectionChanged(object sender, EventArgs e) {
            if (suppressEvents) return;
            var row = SelectedStepRow();
            var tag = row?.Tag as StepInfo;
            if (tag == null) return;
            // Refresh tag before using
            var step = SeqxcDoc.GetByGuid(tag.Guid);
            if (step != null) { row.Tag = step; FocusStep(step.Guid); }
        }

        void OnStepCellMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.RowIndex < 0 || e.RowIndex >= dgvSteps.Rows.Count) return;
            var row     = dgvSteps.Rows[e.RowIndex];
            var tagStep = row.Tag as StepInfo;
            if (tagStep == null) return;

            // Always fetch fresh from SeqxcDoc — row.Tag may be stale after a move+Reparse()
            var step = SeqxcDoc.GetByGuid(tagStep.Guid);
            if (step == null) return;
            row.Tag = step; // keep it fresh for next time

            if (e.Button == MouseButtons.Right) {
                SeqxcDoc.ToggleEnabled(step.Guid);
                UpdateStepRow(row, step);
                suppressEvents = true;
                FocusStep(step.Guid);
                suppressEvents = false;
            } else if (e.Button == MouseButtons.Left &&
                       (e.ColumnIndex == 0 || (ModifierKeys & Keys.Control) != 0)) {
                // Click checkbox column OR Ctrl+click anywhere: toggle check
                bool cur = Convert.ToBoolean(row.Cells["colChk"].Value);
                row.Cells["colChk"].Value = !cur;
                if (!cur) checkedGuids.Add(step.Guid); else checkedGuids.Remove(step.Guid);
                UpdateCounts(); pgAnalysis.UpdateChecked(checkedGuids); pgPat.UpdateChecked(checkedGuids);
            }
        }

        void OnStepCellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.RowIndex < 0 || e.RowIndex >= dgvSteps.Rows.Count) return;
            var step = dgvSteps.Rows[e.RowIndex].Tag as StepInfo;
            if (step == null) return;

            if (e.ColumnIndex == 2) {
                // Badge colors
                bool enabled = step.Enabled;
                e.CellStyle.ForeColor = !enabled ? SystemColors.GrayText :
                    step.Badge == "REG"  ? Color.FromArgb(0, 130, 60)  :
                    step.Badge == "POCB" ? Color.FromArgb(0, 80, 180)  :
                    step.Badge == "MEAS" ? Color.FromArgb(130, 80, 0)  : Color.DimGray;
                e.CellStyle.BackColor = !enabled ? SystemColors.Window :
                    step.Badge == "REG"  ? Color.FromArgb(230, 250, 235) :
                    step.Badge == "POCB" ? Color.FromArgb(228, 238, 255) :
                    step.Badge == "MEAS" ? Color.FromArgb(255, 245, 220) : Color.FromArgb(245, 245, 245);
                e.FormattingApplied = true;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // FILE
        // ══════════════════════════════════════════════════════════════════════

        void PickAndOpen() {
            using (var d = new OpenFileDialog { Title = "Open Sequence File", Filter = "TrueTest Sequence (*.seqxc)|*.seqxc|XML Files|*.xml|All Files|*.*", DefaultExt = "seqxc" })
                if (d.ShowDialog() == DialogResult.OK) OpenFile(d.FileName);
        }

        void OpenFile(string path) {
            try {
                SeqxcDoc.Load(path);
                Text = $"SeqXC Editor  \u2014  {path}";
                focusedGuid = null; checkedGuids.Clear(); ClearClip();
                RepopulateGrid();
                btnSave.Enabled = btnSaveAs.Enabled = true;
                UpdateCounts();
                SetStatus($"Loaded  \"{path}\"   \u00b7   {SeqxcDoc.Steps.Count} steps   \u00b7   {SeqxcDoc.PatternSetups.Count} pattern setups");
                if (SeqxcDoc.Steps.Count > 0) FocusStep(SeqxcDoc.Steps[0].Guid);
            } catch (Exception ex) {
                MessageBox.Show($"Failed to open:\n{ex.Message}", "Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void Save() {
            if (!SeqxcDoc.IsLoaded) return;
            try { SeqxcDoc.Save(SeqxcDoc.FilePath); SetStatus($"Saved  \u2192  {SeqxcDoc.FilePath}"); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        void SaveAs() {
            if (!SeqxcDoc.IsLoaded) return;
            using (var d = new SaveFileDialog { Title = "Save Sequence File", Filter = "TrueTest Sequence (*.seqxc)|*.seqxc|XML Files|*.xml", DefaultExt = "seqxc", FileName = Path.GetFileName(SeqxcDoc.FilePath), InitialDirectory = Path.GetDirectoryName(SeqxcDoc.FilePath) })
                if (d.ShowDialog() == DialogResult.OK)
                    try { SeqxcDoc.Save(d.FileName); Text = $"SeqXC Editor  \u2014  {d.FileName}"; SetStatus($"Saved as  \u2192  {d.FileName}"); }
                    catch (Exception ex) { MessageBox.Show(ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // STEP GRID MANAGEMENT
        // ══════════════════════════════════════════════════════════════════════

        void RepopulateGrid() {
            suppressEvents = true;
            dgvSteps.Rows.Clear();
            foreach (var step in SeqxcDoc.Steps) {
                string dn   = !string.IsNullOrWhiteSpace(step.PatternSetupName) ? step.PatternSetupName : step.Name;
                string dot  = step.Enabled ? "\u25cf" : "\u25cb";
                int    idx  = dgvSteps.Rows.Add(checkedGuids.Contains(step.Guid), $" {dot} {dn}", SeqxcDoc.TypeLabelFor(step.AnalysisTypeName));
                var    row  = dgvSteps.Rows[idx];
                row.Tag     = step;
                if (!step.Enabled) row.DefaultCellStyle.ForeColor = SystemColors.GrayText;
            }
            suppressEvents = false;
            UpdateCounts();
        }

        void UpdateStepRow(DataGridViewRow row, StepInfo step) {
            string dn  = !string.IsNullOrWhiteSpace(step.PatternSetupName) ? step.PatternSetupName : step.Name;
            row.Cells["colName"].Value              = $" {(step.Enabled ? "\u25cf" : "\u25cb")} {dn}";
            row.DefaultCellStyle.ForeColor          = step.Enabled ? SystemColors.WindowText : SystemColors.GrayText;
            row.DefaultCellStyle.SelectionForeColor = Color.Black;
        }

        DataGridViewRow SelectedStepRow() {
            if (dgvSteps.SelectedRows.Count == 0) return null;
            return dgvSteps.SelectedRows[0];
        }

        void FocusStep(string guid) {
            focusedGuid = guid;
            var step = SeqxcDoc.GetByGuid(guid); if (step == null) return;
            int idx    = SeqxcDoc.Steps.IndexOf(step);
            int shared = SeqxcDoc.SharedCount(step.PatternSetupName);
            int anCnt  = step.An?.ChildNodes.OfType<XmlElement>().Count() ?? 0;
            int psCnt  = step.Psn?.ChildNodes.OfType<XmlElement>().Count() ?? 0;

            lblStepName.Text     = step.Name;
            lblEnabled.Text      = "  " + (step.Enabled ? "\u25cf ENABLED" : "\u25cb DISABLED");
            lblEnabled.ForeColor = step.Enabled ? Color.ForestGreen : Color.Gray;
            lblStepIdx.Text      = $"   step {idx+1} of {SeqxcDoc.Steps.Count}";
            lblShared.Text       = shared > 1 ? $"  \u26a0  PatternSetup shared by {shared} steps \u2014 edits affect all" : "";
            btnTabAnalysis.Text  = $"Analysis  ({anCnt})";
            btnTabPatSetup.Text  = $"PatternSetup  ({psCnt})";

            pgAnalysis.SetNode(step.An,  "analysis",     checkedGuids);
            pgPat.SetNode(step.Psn,      "patternSetup", checkedGuids);
            pgAnalysis.SetClipCount(clipList.Count);
            pgPat.SetClipCount(clipList.Count);

            // Select in grid — only set Selected if not already, to avoid interrupting checkbox edits
            suppressEvents = true;
            for (int i = 0; i < dgvSteps.Rows.Count; i++) {
                if ((dgvSteps.Rows[i].Tag as StepInfo)?.Guid == guid) {
                    if (!dgvSteps.Rows[i].Selected)
                        dgvSteps.Rows[i].Selected = true;
                    int first = dgvSteps.FirstDisplayedScrollingRowIndex;
                    int last  = first + dgvSteps.DisplayedRowCount(false) - 1;
                    if (i < first || i > last)
                        dgvSteps.FirstDisplayedScrollingRowIndex = Math.Max(0, i);
                    break;
                }
            }
            suppressEvents = false;
        }

        // ══════════════════════════════════════════════════════════════════════
        // OPERATIONS
        // ══════════════════════════════════════════════════════════════════════

        void CheckAll(bool check) {
            suppressEvents = true;
            checkedGuids.Clear();
            foreach (DataGridViewRow row in dgvSteps.Rows) {
                row.Cells["colChk"].Value = check;
                if (check && row.Tag is StepInfo s) checkedGuids.Add(s.Guid);
            }
            suppressEvents = false;
            UpdateCounts(); pgAnalysis.UpdateChecked(checkedGuids); pgPat.UpdateChecked(checkedGuids);
        }

        void MoveStep(int dir) {
            if (string.IsNullOrEmpty(focusedGuid)) return;
            string g    = focusedGuid;
            int oldIdx  = SeqxcDoc.Steps.FindIndex(s => s.Guid == g);
            SeqxcDoc.MoveStep(g, dir);
            int newIdx  = SeqxcDoc.Steps.FindIndex(s => s.Guid == g);
            if (oldIdx < 0 || newIdx < 0 || oldIdx == newIdx) return;

            // Swap all cell data + Tag between the two rows — no add/remove
            suppressEvents = true;
            var rA = dgvSteps.Rows[oldIdx];
            var rB = dgvSteps.Rows[newIdx];

            var tmpTag   = rA.Tag;                      rA.Tag   = rB.Tag;                      rB.Tag   = tmpTag;
            var tmpChk   = rA.Cells["colChk"].Value;   rA.Cells["colChk"].Value  = rB.Cells["colChk"].Value;  rB.Cells["colChk"].Value  = tmpChk;
            var tmpName  = rA.Cells["colName"].Value;  rA.Cells["colName"].Value = rB.Cells["colName"].Value; rB.Cells["colName"].Value = tmpName;
            var tmpBadge = rA.Cells["colBadge"].Value; rA.Cells["colBadge"].Value= rB.Cells["colBadge"].Value;rB.Cells["colBadge"].Value= tmpBadge;
            var tmpFore  = rA.DefaultCellStyle.ForeColor; rA.DefaultCellStyle.ForeColor = rB.DefaultCellStyle.ForeColor; rB.DefaultCellStyle.ForeColor = tmpFore;

            // Reparse() created fresh StepInfo instances — update Tags so they're not stale
            rA.Tag = SeqxcDoc.GetByGuid(((StepInfo)rA.Tag).Guid);
            rB.Tag = SeqxcDoc.GetByGuid(((StepInfo)rB.Tag).Guid);

            dgvSteps.Rows[newIdx].Selected = true;
            int first2 = dgvSteps.FirstDisplayedScrollingRowIndex;
            int last2  = first2 + dgvSteps.DisplayedRowCount(false) - 1;
            if (newIdx < first2 || newIdx > last2)
                dgvSteps.FirstDisplayedScrollingRowIndex = Math.Max(0, newIdx);
            suppressEvents = false;
            focusedGuid = g;
        }

        void CloneChecked() {
            if (checkedGuids.Count == 0) return;
            string old  = focusedGuid;
            var cloned  = SeqxcDoc.CloneSteps(checkedGuids.ToList());
            checkedGuids.Clear(); RepopulateGrid();
            FocusStep(old ?? SeqxcDoc.Steps.FirstOrDefault()?.Guid);
            SetStatus($"Cloned:  {string.Join(", ", cloned)}");
        }

        void DeleteChecked() {
            if (checkedGuids.Count == 0) return;
            if (MessageBox.Show($"Delete {checkedGuids.Count} step(s)?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            SeqxcDoc.DeleteSteps(checkedGuids.ToList()); checkedGuids.Clear(); RepopulateGrid();
            var f = SeqxcDoc.Steps.FirstOrDefault(); if (f != null) FocusStep(f.Guid);
            SetStatus("Deleted steps");
        }

        void SwitchTab(bool analysis) {
            showingAnalysis      = analysis;
            pgAnalysis.Visible   = analysis;
            pgPat.Visible        = !analysis;
            btnTabAnalysis.Font  = new Font("Segoe UI", 9, analysis  ? FontStyle.Bold : FontStyle.Regular);
            btnTabAnalysis.ForeColor = analysis  ? Color.FromArgb(0,70,160) : Color.DimGray;
            btnTabAnalysis.BackColor = analysis  ? Color.White : Color.FromArgb(245,245,245);
            btnTabPatSetup.Font  = new Font("Segoe UI", 9, !analysis ? FontStyle.Bold : FontStyle.Regular);
            btnTabPatSetup.ForeColor = !analysis ? Color.FromArgb(0,70,160) : Color.DimGray;
            btnTabPatSetup.BackColor = !analysis ? Color.White : Color.FromArgb(245,245,245);
        }

        // ── Copy list ─────────────────────────────────────────────────────────

        void OnAddToClipList(PropRow row, string source) {
            var step     = SeqxcDoc.GetByGuid(focusedGuid);
            var existing = clipList.FirstOrDefault(c => c.Source == source && c.Row.FullPath == row.FullPath);
            if (existing != null) { existing.Row = row; existing.AnalysisType = step?.AnalysisTypeName; }
            else clipList.Add(new ClipItem { Row = row, Source = source, AnalysisType = step?.AnalysisTypeName, StepName = step?.Name });
            RefreshClipDisplay();
            SetStatus($"{(existing!=null?"Updated":"Added")} [{source}] {row.Tag}  \u2014  {clipList.Count} item{(clipList.Count==1?"":"s")} in copy list");
        }

        void PasteToChecked() {
            if (clipList.Count == 0 || checkedGuids.Count == 0) return;
            int pasted = 0;
            foreach (var guid in checkedGuids) {
                var step = SeqxcDoc.GetByGuid(guid); if (step == null) continue;
                SeqxcDoc.PasteClipItems(clipList, step); pasted++;
            }
            if (!string.IsNullOrEmpty(focusedGuid)) FocusStep(focusedGuid);
            SetStatus($"Pasted {clipList.Count} propert{(clipList.Count==1?"y":"ies")} to {pasted} steps");
        }

        void ClearClip() {
            clipList.Clear(); lblClipInfo.Text = "";
            btnPaste.Enabled = btnClearClip.Enabled = false;
            btnPaste.Text    = "Paste (0) \u2192 0 steps";
            pgAnalysis.SetClipCount(0); pgPat.SetClipCount(0);
            RefreshClipDisplay();
        }

        void RefreshClipDisplay() {
            int n = clipList.Count;
            btnClipMenu.Text     = $"Copy list ({n}) \u25bc";
            btnClipMenu.ForeColor= n > 0 ? Color.FromArgb(0,100,200) : Color.DimGray;
            lblClipInfo.Text     = n > 0 ? $"  {n} prop{(n==1?"":"s")} in list" : "";
            btnPaste.Text        = $"Paste ({n}) \u2192 {checkedGuids.Count} steps";
            btnPaste.Enabled     = n > 0 && checkedGuids.Count > 0;
            btnClearClip.Enabled = n > 0;
            pgAnalysis.SetClipCount(n); pgPat.SetClipCount(n);
        }

        void OnClipMenuOpening(object sender, EventArgs e) {
            btnClipMenu.DropDownItems.Clear();
            var an = clipList.Where(c => c.Source == "analysis").ToList();
            var ps = clipList.Where(c => c.Source == "patternSetup").ToList();
            Action<ClipItem> remove = ci => { clipList.Remove(ci); RefreshClipDisplay(); UpdateCounts(); };
            if (an.Count > 0) {
                btnClipMenu.DropDownItems.Add(new ToolStripLabel($"  Analysis  ({an.Count})") { Font = new Font("Segoe UI",8,FontStyle.Bold), ForeColor = Color.FromArgb(0,80,160) });
                foreach (var ci in an) { var c=ci; var m=new ToolStripMenuItem($"    {c.Row.Tag}  \u00d7"){ForeColor=Color.FromArgb(0,70,140)}; m.Click+=(s,_)=>remove(c); btnClipMenu.DropDownItems.Add(m); }
            }
            if (an.Count>0 && ps.Count>0) btnClipMenu.DropDownItems.Add(new ToolStripSeparator());
            if (ps.Count > 0) {
                btnClipMenu.DropDownItems.Add(new ToolStripLabel($"  PatternSetup  ({ps.Count})") { Font = new Font("Segoe UI",8,FontStyle.Bold), ForeColor = Color.FromArgb(0,120,50) });
                foreach (var ci in ps) { var c=ci; var m=new ToolStripMenuItem($"    {c.Row.Tag}  \u00d7"){ForeColor=Color.FromArgb(0,100,40)}; m.Click+=(s,_)=>remove(c); btnClipMenu.DropDownItems.Add(m); }
            }
            if (clipList.Count == 0) btnClipMenu.DropDownItems.Add(new ToolStripLabel("  (empty)") { ForeColor = Color.Gray });
            btnClipMenu.DropDownItems.Add(new ToolStripSeparator());
            var clr = new ToolStripMenuItem("\u00d7  Clear all"); clr.Click += (s,_) => ClearClip(); btnClipMenu.DropDownItems.Add(clr);
        }

        void UpdateCounts() {
            int n = checkedGuids.Count;
            lblCheckedCount.Text  = $"  {n} checked";
            btnClone.Text         = $"Clone ({n})";
            btnDelete.Text        = $"Delete ({n})";
            btnClone.Enabled      = n > 0;
            btnDelete.Enabled     = n > 0;
            btnPaste.Text         = $"Paste ({clipList.Count}) \u2192 {n} steps";
            btnPaste.Enabled      = clipList.Count > 0 && n > 0;
        }

        void SetStatus(string msg) { lblStatus.Text = msg; statusStrip.Refresh(); }
    }
}
