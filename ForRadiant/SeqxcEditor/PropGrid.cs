using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace SeqxcEditor {

    class PropGrid : UserControl {

        readonly TextBox      txtFilter;
        readonly DataGridView dgv;

        XmlNode         currentNode;
        string          currentTab;
        HashSet<string> checkedGuids = new HashSet<string>();
        List<PropRow>   visible      = new List<PropRow>();
        HashSet<string> expanded     = new HashSet<string>();
        int             clipCount;

        public event Action<string>          StatusChanged;
        public event Action<PropRow, string> AddToClipList;

        public PropGrid() {
            Dock        = DockStyle.Fill;
            BorderStyle = BorderStyle.None;

            txtFilter = new TextBox {
                Dock        = DockStyle.Top,
                Height      = 26,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 9),
                Text        = "Filter properties\u2026",
                ForeColor   = Color.Gray,
            };
            txtFilter.GotFocus    += (s, e) => { if (txtFilter.ForeColor == Color.Gray) { txtFilter.Text = ""; txtFilter.ForeColor = SystemColors.WindowText; } };
            txtFilter.LostFocus   += (s, e) => { if (txtFilter.Text.Trim() == "") { txtFilter.Text = "Filter properties\u2026"; txtFilter.ForeColor = Color.Gray; } };
            txtFilter.TextChanged += (s, e) => Reload(keepScroll: false);

            dgv = new DataGridView {
                Dock                        = DockStyle.Fill,
                BorderStyle                 = BorderStyle.None,
                RowHeadersVisible           = false,
                AllowUserToAddRows          = false,
                AllowUserToDeleteRows       = false,
                AllowUserToResizeRows       = false,
                MultiSelect                 = false,
                SelectionMode               = DataGridViewSelectionMode.FullRowSelect,
                EditMode                    = DataGridViewEditMode.EditOnKeystrokeOrF2,
                BackgroundColor             = SystemColors.Window,
                GridColor                   = Color.FromArgb(220, 220, 220),
                CellBorderStyle             = DataGridViewCellBorderStyle.SingleHorizontal,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing,
                ColumnHeadersHeight         = 24,
                EnableHeadersVisualStyles   = false,
                Font                        = new Font("Segoe UI", 9),
                // VirtualMode: CellValueNeeded supplies data; no row objects stored in grid
                VirtualMode                 = true,
            };
            dgv.RowTemplate.Height = 22;
            dgv.DefaultCellStyle.SelectionBackColor         = Color.FromArgb(198, 218, 255);
            dgv.DefaultCellStyle.SelectionForeColor         = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.BackColor     = Color.FromArgb(235, 237, 240);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor     = Color.Black;
            dgv.ColumnHeadersDefaultCellStyle.Font          = new Font("Segoe UI", 9, FontStyle.Bold);

            dgv.Columns.Add(new DataGridViewTextBoxColumn {
                Name = "colProp", HeaderText = "PROPERTY", ReadOnly = true,
                Width = 240, MinimumWidth = 80, SortMode = DataGridViewColumnSortMode.NotSortable,
            });
            dgv.Columns.Add(new DataGridViewTextBoxColumn {
                Name = "colVal", HeaderText = "VALUE",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                SortMode     = DataGridViewColumnSortMode.NotSortable,
            });

            dgv.VirtualMode         = true;
            dgv.CellValueNeeded    += OnCellValueNeeded;
            dgv.CellValuePushed    += OnCellValuePushed;
            dgv.CellMouseClick     += OnCellMouseClick;
            dgv.CellBeginEdit      += OnCellBeginEdit;
            dgv.CellFormatting     += OnCellFormatting;

            Controls.Add(txtFilter);
            Controls.Add(dgv);
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void SetNode(XmlNode node, string tab, HashSet<string> guids) {
            currentNode  = node;
            currentTab   = tab;
            checkedGuids = guids;
            txtFilter.Text = "Filter properties\u2026";
            txtFilter.ForeColor = Color.Gray;
            expanded.Clear();
            Reload(keepScroll: false);
        }

        public void UpdateChecked(HashSet<string> guids) => checkedGuids = guids;
        public void SetClipCount(int n) => clipCount = n;

        // ── Reload ────────────────────────────────────────────────────────────

        void Reload(bool keepScroll = true) {
            if (currentNode == null) { visible.Clear(); dgv.RowCount = 0; return; }

            int firstRow  = keepScroll && dgv.RowCount > 0 ? dgv.FirstDisplayedScrollingRowIndex : -1;
            int selPath_i = dgv.CurrentRow?.Index ?? -1;
            string selPath = selPath_i >= 0 && selPath_i < visible.Count ? visible[selPath_i].FullPath : null;

            visible = Flatten(PropRow.FromNode(currentNode, FilterText));

            // VirtualMode: just set RowCount — no clear/repopulate, no scroll jump
            dgv.RowCount = visible.Count;
            dgv.Invalidate();

            // Restore scroll
            if (firstRow > 0 && firstRow < dgv.RowCount)
                try { dgv.FirstDisplayedScrollingRowIndex = firstRow; } catch { }

            // Restore selection
            if (selPath != null) {
                int idx = visible.FindIndex(r => r.FullPath == selPath);
                if (idx >= 0) {
                    dgv.ClearSelection();
                    dgv.Rows[idx].Selected = true;
                }
            }
        }

        List<PropRow> Flatten(List<PropRow> top) {
            var r = new List<PropRow>();
            foreach (var row in top) {
                r.Add(row);
                if (row.IsComplex && expanded.Contains(row.FullPath))
                    InsertChildren(r, row);
            }
            return r;
        }

        void InsertChildren(List<PropRow> result, PropRow parent) {
            foreach (var c in PropRow.GetChildren(parent)) {
                result.Add(c);
                if (c.IsComplex && expanded.Contains(c.FullPath))
                    InsertChildren(result, c);
            }
        }

        string FilterText { get {
            string t = txtFilter.Text.Trim();
            return (t == "Filter properties\u2026" || t == "") ? "" : t;
        }}

        // ── Virtual mode data supply ──────────────────────────────────────────

        void OnCellValueNeeded(object sender, DataGridViewCellValueEventArgs e) {
            if (e.RowIndex < 0 || e.RowIndex >= visible.Count) return;
            var row = visible[e.RowIndex];
            if (e.ColumnIndex == 0)
                e.Value = new string(' ', row.Indent * 3)
                    + (row.IsComplex ? (expanded.Contains(row.FullPath) ? "\u25bc  " : "\u25b6  ") : "    ")
                    + row.DisplayName
                    + (row.IsComplex ? " [array]" : "");
            else
                e.Value = row.IsComplex
                    ? (expanded.Contains(row.FullPath) ? "\u2014 expanded \u2014" : row.Value)
                    : row.Value;
        }

        void OnCellValuePushed(object sender, DataGridViewCellValueEventArgs e) {
            if (e.ColumnIndex != 1 || e.RowIndex < 0 || e.RowIndex >= visible.Count) return;
            var row = visible[e.RowIndex];
            if (row == null || row.IsComplex) return;
            string val = e.Value?.ToString() ?? "";
            row.El.InnerText = val;
            row.Value        = val;
            StatusChanged?.Invoke($"Edited {row.Tag} = \"{val}\"");
        }

        // ── Cell formatting (colours for complex rows, indented rows) ─────────

        void OnCellFormatting(object sender, DataGridViewCellFormattingEventArgs e) {
            if (e.RowIndex < 0 || e.RowIndex >= visible.Count) return;
            var row = visible[e.RowIndex];
            if (row.IsComplex) {
                if (e.ColumnIndex == 0) { e.CellStyle.ForeColor = Color.FromArgb(0, 70, 140); e.FormattingApplied = true; }
                else { e.CellStyle.ForeColor = Color.Gray; e.FormattingApplied = true; }
            } else if (row.Indent > 0) {
                e.CellStyle.BackColor = Color.FromArgb(252, 252, 254);
                e.FormattingApplied   = true;
            }
        }

        // ── Cell interaction ──────────────────────────────────────────────────

        void OnCellMouseClick(object sender, DataGridViewCellMouseEventArgs e) {
            if (e.RowIndex < 0 || e.RowIndex >= visible.Count) return;
            var row = visible[e.RowIndex];

            if (e.Button == MouseButtons.Right) {
                // Right-click: add to copy list
                AddToClipList?.Invoke(row, currentTab);
                StatusChanged?.Invoke($"Added [{currentTab}] {row.Tag} to copy list  ({clipCount + 1} items)");
            } else if (e.Button == MouseButtons.Left && e.ColumnIndex == 0 && row.IsComplex) {
                // Click property name of complex row: expand/collapse
                if (expanded.Contains(row.FullPath)) expanded.Remove(row.FullPath);
                else expanded.Add(row.FullPath);
                Reload();
            }
        }

        void OnCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) {
            if (e.ColumnIndex != 1) { e.Cancel = true; return; }
            if (e.RowIndex < 0 || e.RowIndex >= visible.Count || visible[e.RowIndex].IsComplex)
                e.Cancel = true;
        }
    }
}
