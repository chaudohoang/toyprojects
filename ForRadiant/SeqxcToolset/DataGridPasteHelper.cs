using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SeqxcToolset
{
    /// <summary>
    /// Adds Excel-style paste to a DataGrid: select a range of cells (requires
    /// SelectionUnit="Cell" in XAML), Ctrl+V pastes clipboard text starting at
    /// the top-left selected cell, spreading across rows (newline-delimited)
    /// and columns (tab-delimited) as far as it fits.
    ///
    /// Copy needs no extra code — WPF's DataGrid already exports selected
    /// cells as tab/newline text on Ctrl+C.
    ///
    /// Only ever writes into bound properties whose name starts with "New" —
    /// a safety net so a rectangular selection that happens to span a
    /// read-only "Current" column can never overwrite it, even though that
    /// column could technically be part of the selected range.
    /// </summary>
    public static class DataGridPasteHelper
    {
        public static void Attach(DataGrid grid)
        {
            grid.PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
                {
                    Paste(grid);
                    e.Handled = true;
                }
            };
        }

        private static void Paste(DataGrid grid)
        {
            if (!Clipboard.ContainsText()) return;
            string text = Clipboard.GetText();
            if (string.IsNullOrEmpty(text)) return;

            var selected = grid.SelectedCells;
            if (selected == null || selected.Count == 0) return;

            // Anchor = top-left of the current selection, regardless of click order.
            int anchorRow = int.MaxValue, anchorCol = int.MaxValue;
            foreach (var cell in selected)
            {
                int r = grid.Items.IndexOf(cell.Item);
                int c = grid.Columns.IndexOf(cell.Column);
                if (r < anchorRow) anchorRow = r;
                if (c < anchorCol) anchorCol = c;
            }
            if (anchorRow == int.MaxValue) return;

            var lines = text.Replace("\r\n", "\n").TrimEnd('\n').Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                int targetRow = anchorRow + i;
                if (targetRow >= grid.Items.Count) break;
                object rowItem = grid.Items[targetRow];

                var cols = lines[i].Split('\t');
                for (int j = 0; j < cols.Length; j++)
                {
                    int targetCol = anchorCol + j;
                    if (targetCol >= grid.Columns.Count) break;
                    SetCellValue(rowItem, grid.Columns[targetCol], cols[j].Trim());
                }
            }
        }

        private static void SetCellValue(object rowItem, DataGridColumn column, string value)
        {
            if (!(column is DataGridBoundColumn boundCol) || !(boundCol.Binding is System.Windows.Data.Binding binding))
                return;

            string propertyName = binding.Path?.Path;
            if (string.IsNullOrEmpty(propertyName) || !propertyName.StartsWith("New"))
                return; // never write into a read-only "Current" column

            PropertyInfo prop = rowItem.GetType().GetProperty(propertyName);
            if (prop == null || !prop.CanWrite) return;

            try
            {
                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(rowItem, string.IsNullOrEmpty(value) ? null : value);
                }
                else if (prop.PropertyType == typeof(bool?))
                {
                    if (string.IsNullOrEmpty(value)) prop.SetValue(rowItem, null);
                    else if (bool.TryParse(value, out var b)) prop.SetValue(rowItem, (bool?)b);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    if (bool.TryParse(value, out var b)) prop.SetValue(rowItem, b);
                }
            }
            catch
            {
                // Malformed cell for this property type — skip it, keep pasting the rest.
            }
        }
    }
}
