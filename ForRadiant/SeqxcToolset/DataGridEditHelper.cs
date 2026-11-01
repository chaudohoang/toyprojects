using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SeqxcToolset
{
    /// <summary>
    /// Editing behaviour shared by every task's grid: Excel-style paste, sane Backspace
    /// and Delete on a selected cell, and an undo stack for the bulk edits.
    ///
    /// Everything here only ever writes into bound properties whose name starts with
    /// "New" — a safety net so a rectangular selection that happens to span a read-only
    /// "Current" column can never overwrite it, even though that column can legitimately
    /// be part of the selected range.
    ///
    /// Copy needs no code: WPF's DataGrid already exports selected cells as
    /// tab/newline-delimited text on Ctrl+C.
    /// </summary>
    public static class DataGridEditHelper
    {
        /// <summary>One property's value before an edit, enough to put it back.</summary>
        private class CellEdit
        {
            public object Row;
            public string Property;
            public object OldValue;
        }

        /// <summary>
        /// Per-grid state, captured by the handlers rather than kept in a static map, so
        /// several grids never share an undo stack and nothing has to be cleaned up.
        /// </summary>
        private class GridState
        {
            // True between BeginningEdit and CellEditEnding. The keys below have to
            // behave completely differently inside a cell editor (where the TextBox
            // already does the right thing) than on a merely selected cell.
            public bool Editing;

            public readonly List<List<CellEdit>> Undo = new List<List<CellEdit>>();
        }

        /// <summary>How many bulk edits can be undone before the oldest is dropped.</summary>
        private const int UndoDepth = 25;

        public static void Attach(DataGrid grid)
        {
            var state = new GridState();

            grid.BeginningEdit += (s, e) => state.Editing = true;
            grid.CellEditEnding += (s, e) => state.Editing = false;

            grid.PreviewKeyDown += (s, e) =>
            {
                var mods = Keyboard.Modifiers;

                if (e.Key == Key.V && mods == ModifierKeys.Control)
                {
                    Paste(grid, state);
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Z && mods == ModifierKeys.Control)
                {
                    // While a cell editor is open the TextBox has its own undo for the
                    // text being typed; this stack is for whole-grid edits.
                    if (state.Editing) return;
                    Undo(grid, state);
                    e.Handled = true;
                    return;
                }

                if (mods != ModifierKeys.None) return;

                if (e.Key == Key.Delete)
                {
                    // Inside an editor, Delete is forward-delete and belongs to the
                    // TextBox. On a selected cell it clears the value outright.
                    if (state.Editing) return;
                    ClearSelected(grid, state);
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.Back)
                {
                    if (state.Editing) return;   // already editing: a normal Backspace
                    BackspaceOneCharacter(grid);
                    e.Handled = true;
                }
            };
        }

        // ---------------------------------------------------------------- Backspace

        /// <summary>
        /// Backspace on a selected cell deletes ONE character, like a text box.
        ///
        /// Left to itself the DataGrid enters edit mode with the whole value selected,
        /// so the Backspace wipes the lot and leaves an edit pending a commit. Here the
        /// edit is opened, the selection dropped, the caret put at the end, and exactly
        /// one character removed — then the editor stays open, so further Backspaces and
        /// typing behave normally and Enter commits as usual.
        ///
        /// The caret fix is deferred to Input priority because the editing TextBox does
        /// not exist until the DataGrid has finished preparing the cell.
        /// </summary>
        private static void BackspaceOneCharacter(DataGrid grid)
        {
            var column = grid.CurrentColumn;
            if (column == null || PropertyOf(column) == null) return;   // read-only column

            grid.BeginEdit();

            grid.Dispatcher.BeginInvoke(new Action(() =>
            {
                TextBox box = Keyboard.FocusedElement as TextBox;
                if (box == null) return;

                box.SelectionLength = 0;
                box.CaretIndex = box.Text.Length;
                if (box.CaretIndex == 0) return;   // already empty

                int caret = box.CaretIndex;
                box.Text = box.Text.Remove(caret - 1, 1);
                box.CaretIndex = caret - 1;
            }), DispatcherPriority.Input);
        }

        // ---------------------------------------------------------------- Delete

        /// <summary>
        /// Delete clears every selected "New" cell at once and writes straight to the
        /// row, without opening an editor — so it takes effect immediately rather than
        /// waiting for Enter. Recorded as one undo step, however many cells it touched.
        /// </summary>
        private static void ClearSelected(DataGrid grid, GridState state)
        {
            var cells = grid.SelectedCells;
            if (cells == null || cells.Count == 0) return;

            var batch = new List<CellEdit>();
            foreach (var cell in cells)
            {
                string property = PropertyOf(cell.Column);
                if (property == null) continue;
                SetValue(cell.Item, property, null, batch);
            }
            Push(state, batch);
        }

        // ---------------------------------------------------------------- Paste

        /// <summary>
        /// Pastes clipboard text starting at the top-left selected cell, spreading down
        /// rows (newline-delimited) and across columns (tab-delimited) as far as it fits.
        /// Recorded as one undo step.
        /// </summary>
        private static void Paste(DataGrid grid, GridState state)
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

            var batch = new List<CellEdit>();
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

                    string property = PropertyOf(grid.Columns[targetCol]);
                    if (property == null) continue;   // never write a "Current" column
                    SetValue(rowItem, property, cols[j].Trim(), batch);
                }
            }
            Push(state, batch);
        }

        // ---------------------------------------------------------------- Undo

        private static void Push(GridState state, List<CellEdit> batch)
        {
            if (batch.Count == 0) return;   // nothing changed, nothing to undo
            state.Undo.Add(batch);
            if (state.Undo.Count > UndoDepth) state.Undo.RemoveAt(0);
        }

        /// <summary>
        /// Reverts the last paste or Delete as a single step. Values go back through the
        /// same property setters, so the grid refreshes and the dirty highlight
        /// recalculates exactly as it would for a typed edit.
        ///
        /// Covers bulk edits only. A single typed cell is not on this stack — Esc
        /// abandons one in progress, and the editor's own Ctrl+Z works inside it.
        /// </summary>
        private static void Undo(DataGrid grid, GridState state)
        {
            int last = state.Undo.Count - 1;
            if (last < 0) return;

            var batch = state.Undo[last];
            state.Undo.RemoveAt(last);

            // Backwards, so a batch that wrote the same cell twice lands on its
            // original value rather than an intermediate one.
            var touched = new List<object>();
            for (int i = batch.Count - 1; i >= 0; i--)
            {
                var edit = batch[i];
                PropertyInfo prop = edit.Row.GetType().GetProperty(edit.Property);
                if (prop == null || !prop.CanWrite) continue;
                try { prop.SetValue(edit.Row, edit.OldValue); }
                catch { continue; }   // type changed under us; leave the rest alone
                if (!touched.Contains(edit.Row)) touched.Add(edit.Row);
            }

            // Nudge just the rows that changed, rather than grid.Items.Refresh(): a full
            // refresh rebuilds the view and throws away the scroll position, so undoing
            // a paste halfway down a 36-row grid would snap back to the top.
            foreach (object row in touched) RefreshDirty(row);
        }

        // ---------------------------------------------------------------- plumbing

        /// <summary>
        /// Asks a row model to re-announce IsDirty, if it offers that. Restoring a
        /// value through its own setter usually raises it already; the "(all)"
        /// convenience columns on Exposure Time and Luminance Scale are the exception,
        /// since they only fan out to the per-channel setters for a non-blank value.
        /// </summary>
        private static void RefreshDirty(object row)
        {
            if (row == null) return;
            try
            {
                MethodInfo m = row.GetType().GetMethod("RefreshDirty", Type.EmptyTypes);
                if (m != null) m.Invoke(row, null);
            }
            catch
            {
                // Cosmetic only — never let a repaint hint break an undo.
            }
        }

        /// <summary>
        /// The bound property a column writes to, or null when it is not a writable
        /// "New" column — which is the one guard every edit path here goes through.
        /// </summary>
        private static string PropertyOf(DataGridColumn column)
        {
            if (!(column is DataGridBoundColumn boundCol) ||
                !(boundCol.Binding is System.Windows.Data.Binding binding))
                return null;

            string propertyName = binding.Path?.Path;
            if (string.IsNullOrEmpty(propertyName) || !propertyName.StartsWith("New"))
                return null;

            return propertyName;
        }

        /// <summary>
        /// Writes one cell, recording its previous value in <paramref name="batch"/>
        /// first. A null or empty value clears the property, which for these row models
        /// means "no change" rather than "set to blank".
        /// </summary>
        private static void SetValue(object rowItem, string propertyName, string value, List<CellEdit> batch)
        {
            if (rowItem == null) return;

            PropertyInfo prop = rowItem.GetType().GetProperty(propertyName);
            if (prop == null || !prop.CanWrite) return;

            try
            {
                object newValue;
                if (prop.PropertyType == typeof(string))
                {
                    newValue = string.IsNullOrEmpty(value) ? null : value;
                }
                else if (prop.PropertyType == typeof(bool?))
                {
                    if (string.IsNullOrEmpty(value)) newValue = null;
                    else if (bool.TryParse(value, out var b)) newValue = (bool?)b;
                    else return;   // malformed for this type — leave the cell alone
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    if (!bool.TryParse(value, out var b)) return;
                    newValue = b;
                }
                else
                {
                    return;
                }

                object oldValue = prop.GetValue(rowItem);
                if (Equals(oldValue, newValue)) return;   // no-op: keep it off the undo stack

                batch.Add(new CellEdit { Row = rowItem, Property = propertyName, OldValue = oldValue });
                prop.SetValue(rowItem, newValue);
            }
            catch
            {
                // Malformed cell for this property type — skip it, keep going.
            }
        }
    }
}
