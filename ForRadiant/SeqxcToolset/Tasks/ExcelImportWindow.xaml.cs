using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using SeqxcToolset.Core;

namespace SeqxcToolset.Tasks
{
    /// <summary>
    /// Picks a sheet and two (optionally three) columns out of an .xlsx and hands back
    /// rows in the same Name / As-is / To-be shape a pasted Excel range produces, so a
    /// task can feed them through exactly the same matching path as a paste.
    ///
    /// Each task imports only the columns it edits — the caller names the value column
    /// it wants ("Pattern String", say) and nothing here knows or cares about the other
    /// tasks' fields. A sheet holding data for several tasks is simply imported once per
    /// task, which keeps one task's import from silently writing another's values.
    /// </summary>
    public partial class ExcelImportWindow : Window
    {
        /// <summary>One selectable column: its letter, its header text, and a sample.</summary>
        public class ColumnOption
        {
            public int Index;          // 0-based; -1 for the "(none)" entry
            public string Header;
            public string Display { get; set; }
        }

        public class PreviewRow
        {
            public int RowNumber { get; set; }
            public string Name { get; set; }
            public string AsIs { get; set; }
            public string ToBe { get; set; }
        }

        /// <summary>Populated when the dialog is accepted.</summary>
        public List<(string Name, string AsIs, string ToBe)> Rows { get; private set; }
            = new List<(string, string, string)>();

        /// <summary>What was actually chosen, for persisting as this task's preference.</summary>
        public ImportPrefs Chosen { get; private set; }

        private readonly string _path;
        private readonly string _valueLabel;
        private XlsxSheet _sheet;
        private bool _loading;          // suppresses the event storm while combos are repopulated
        private ImportPrefs _restore;   // consumed once, on the first sheet load

        public ExcelImportWindow(string path, string valueColumnLabel, ImportPrefs remembered = null)
        {
            InitializeComponent();

            _path = path;
            _valueLabel = valueColumnLabel;
            _restore = remembered;

            FileText.Text = path;
            ValueLabel.Text = valueColumnLabel + " column";
            Title = "Import " + valueColumnLabel + " from Excel";

            try
            {
                var names = XlsxReader.GetSheetNames(path);
                if (names.Count == 0)
                    throw new InvalidOperationException("The workbook contains no sheets.");

                _loading = true;
                foreach (var n in names) SheetCombo.Items.Add(n);
                _loading = false;

                // Reopen on the remembered sheet when this workbook still has it.
                int index = 0;
                if (!string.IsNullOrEmpty(_restore?.SheetName))
                {
                    int found = names.FindIndex(n =>
                        string.Equals(n, _restore.SheetName, StringComparison.OrdinalIgnoreCase));
                    if (found >= 0) index = found;
                }

                SheetCombo.SelectedIndex = index; // raises SelectionChanged, which loads the sheet
            }
            catch (Exception ex)
            {
                _loading = false;
                StatusText.Text = "Could not read the workbook: " + ex.Message;
                ImportButton.IsEnabled = false;
            }
        }

        private void SheetCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading || !(SheetCombo.SelectedItem is string sheetName)) return;

            try
            {
                _sheet = XlsxReader.ReadSheet(_path, sheetName);
            }
            catch (Exception ex)
            {
                _sheet = null;
                StatusText.Text = "Could not read that sheet: " + ex.Message;
                ImportButton.IsEnabled = false;
                return;
            }

            // The remembered choices only apply to the sheet they were made on.
            var restore = _restore != null &&
                          string.Equals(_restore.SheetName, sheetName, StringComparison.OrdinalIgnoreCase)
                ? _restore
                : null;
            _restore = null; // honoured once; switching sheets afterwards behaves normally

            int detected = XlsxReader.DetectHeaderRow(_sheet);
            int headerRow = restore != null && restore.HeaderRow > 0 ? restore.HeaderRow : detected;

            _loading = true;
            HeaderRowBox.Text = headerRow.ToString(CultureInfo.InvariantCulture);
            _loading = false;
            HeaderHintText.Text = headerRow == detected
                ? $"detected (sheet has {_sheet.Rows.Count} non-empty rows)"
                : $"remembered (detected {detected})";

            RebuildColumns(restore);
        }

        private void HeaderRowBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_loading || _sheet == null) return;
            HeaderHintText.Text = "";
            // Re-pick the columns: a different header row means different header names,
            // so a selection made against the old row would be meaningless.
            RebuildColumns(null);
        }

        private void Column_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_loading) return;
            RebuildPreview();
        }

        private int HeaderRow =>
            int.TryParse(HeaderRowBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int r) && r > 0
                ? r
                : -1;

        /// <summary>
        /// Repopulates the three column lists for the current sheet and header row.
        /// With <paramref name="restore"/> set, selections come from the remembered
        /// header names, each falling back to auto-detection if that header is gone.
        /// </summary>
        private void RebuildColumns(ImportPrefs restore)
        {
            _loading = true;
            NameCombo.Items.Clear();
            ValueCombo.Items.Clear();
            AsIsCombo.Items.Clear();

            int headerRow = HeaderRow;
            if (_sheet == null || headerRow < 0)
            {
                _loading = false;
                StatusText.Text = "Enter a header row number.";
                ImportButton.IsEnabled = false;
                return;
            }

            // Offer every column the sheet uses, not just the ones with a header —
            // a header cell can be blank above a column that still holds data.
            var options = new List<ColumnOption>();
            for (int col = 0; col <= _sheet.MaxColumnIndex; col++)
            {
                string header = _sheet.Cell(headerRow, col);
                string sample = FirstDataValue(col, headerRow);
                if (string.IsNullOrEmpty(header) && string.IsNullOrEmpty(sample)) continue;

                string letter = XlsxReader.ColumnName(col);
                string display = string.IsNullOrEmpty(header)
                    ? (string.IsNullOrEmpty(sample) ? letter : $"{letter}  —  (no header, e.g. \"{Trim(sample)}\")")
                    : (string.IsNullOrEmpty(sample) ? $"{letter}  —  {header}" : $"{letter}  —  {header}  (e.g. \"{Trim(sample)}\")");

                options.Add(new ColumnOption { Index = col, Header = header, Display = display });
            }

            var none = new ColumnOption { Index = -1, Header = "", Display = "(none)" };
            AsIsCombo.Items.Add(none);

            foreach (var o in options)
            {
                NameCombo.Items.Add(o);
                ValueCombo.Items.Add(o);
                AsIsCombo.Items.Add(o);
            }

            NameCombo.SelectedItem =
                ByHeader(options, restore?.NameHeader)
                ?? PickColumn(options, "pattern", exclude: Normalize(_valueLabel))
                ?? options.FirstOrDefault();

            ValueCombo.SelectedItem =
                ByHeader(options, restore?.ValueHeader)
                ?? PickColumn(options, Normalize(_valueLabel), exclude: null);

            AsIsCombo.SelectedItem =
                (restore != null && string.IsNullOrEmpty(restore.AsIsHeader))
                    ? none
                    : (object)ByHeader(options, restore?.AsIsHeader) ?? none;

            _loading = false;
            RebuildPreview();
        }

        /// <summary>Exact header-text match, used to restore a remembered column.</summary>
        private static ColumnOption ByHeader(List<ColumnOption> options, string header) =>
            string.IsNullOrEmpty(header)
                ? null
                : options.FirstOrDefault(o => string.Equals(o.Header, header, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Picks the column whose header best matches <paramref name="wanted"/>
        /// (already normalized): an exact match first, then a containing match that
        /// isn't the column we're trying to stay clear of. "Pattern" must not win the
        /// Pattern String slot, and "Pattern String" must not win the name slot, which
        /// is what the exclude is for.
        /// </summary>
        private static ColumnOption PickColumn(List<ColumnOption> options, string wanted, string exclude)
        {
            var exact = options.FirstOrDefault(o => Normalize(o.Header) == wanted);
            if (exact != null) return exact;

            return options.FirstOrDefault(o =>
            {
                string h = Normalize(o.Header);
                if (h.Length == 0 || !h.Contains(wanted)) return false;
                return exclude == null || h != exclude;
            });
        }

        /// <summary>Lowercased letters and digits only, so "L/v Scale" and "lvscale" match.</summary>
        private static string Normalize(string s) =>
            string.IsNullOrEmpty(s)
                ? ""
                : new string(s.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

        private static string Trim(string s) =>
            s != null && s.Length > 24 ? s.Substring(0, 24) + "…" : s;

        /// <summary>First non-empty value below the header, to show as a sample.</summary>
        private string FirstDataValue(int col, int headerRow)
        {
            foreach (var kv in _sheet.Rows)
            {
                if (kv.Key <= headerRow) continue;
                if (kv.Value.TryGetValue(col, out var v) && !string.IsNullOrEmpty(v)) return v;
            }
            return "";
        }

        private void RebuildPreview()
        {
            var rows = BuildRows(out int skippedNoName, out int skippedNoValue);

            PreviewGrid.ItemsSource = rows
                .Take(200)
                .Select(r => new PreviewRow { RowNumber = r.Row, Name = r.Name, AsIs = r.AsIs, ToBe = r.ToBe })
                .ToList();

            if (_sheet == null)
            {
                StatusText.Text = "";
                ImportButton.IsEnabled = false;
                return;
            }

            if (NameCombo.SelectedItem == null || ValueCombo.SelectedItem == null)
            {
                StatusText.Text = "Pick the pattern name column and the " + _valueLabel + " column.";
                ImportButton.IsEnabled = false;
                return;
            }

            var parts = new List<string> { $"{rows.Count} row(s) ready" };
            if (skippedNoValue > 0) parts.Add($"{skippedNoValue} skipped with no value");
            if (skippedNoName > 0) parts.Add($"{skippedNoName} skipped with no name");
            StatusText.Text = string.Join(", ", parts) + ".";
            ImportButton.IsEnabled = rows.Count > 0;
        }

        private List<(int Row, string Name, string AsIs, string ToBe)> BuildRows(
            out int skippedNoName, out int skippedNoValue)
        {
            skippedNoName = 0;
            skippedNoValue = 0;
            var result = new List<(int, string, string, string)>();

            int headerRow = HeaderRow;
            if (_sheet == null || headerRow < 0) return result;
            if (!(NameCombo.SelectedItem is ColumnOption nameCol)) return result;
            if (!(ValueCombo.SelectedItem is ColumnOption valueCol)) return result;
            var asIsCol = AsIsCombo.SelectedItem as ColumnOption;

            foreach (var kv in _sheet.Rows)
            {
                if (kv.Key <= headerRow) continue; // header and anything above it

                string name = kv.Value.TryGetValue(nameCol.Index, out var n) ? n.Trim() : "";
                string toBe = kv.Value.TryGetValue(valueCol.Index, out var v) ? v.Trim() : "";
                string asIs = asIsCol != null && asIsCol.Index >= 0 &&
                              kv.Value.TryGetValue(asIsCol.Index, out var a) ? a.Trim() : null;

                if (string.IsNullOrEmpty(name)) { if (!string.IsNullOrEmpty(toBe)) skippedNoName++; continue; }

                // A blank value means "nothing to set for this pattern", exactly as a
                // blank cell in a pasted range does — not "clear it".
                if (string.IsNullOrEmpty(toBe)) { skippedNoValue++; continue; }

                result.Add((kv.Key, name, asIs, toBe));
            }

            return result;
        }

        private void Import_Click(object sender, RoutedEventArgs e)
        {
            var rows = BuildRows(out _, out _);
            if (rows.Count == 0)
            {
                StatusText.Text = "Nothing to import with those columns.";
                return;
            }

            Rows = rows.Select(r => (r.Name, r.AsIs, r.ToBe)).ToList();
            Chosen = new ImportPrefs
            {
                SheetName = SheetCombo.SelectedItem as string,
                HeaderRow = HeaderRow,
                NameHeader = (NameCombo.SelectedItem as ColumnOption)?.Header,
                ValueHeader = (ValueCombo.SelectedItem as ColumnOption)?.Header,
                AsIsHeader = (AsIsCombo.SelectedItem as ColumnOption)?.Index >= 0
                    ? (AsIsCombo.SelectedItem as ColumnOption)?.Header
                    : null
            };
            DialogResult = true;
        }

        /// <summary>
        /// Full import flow: choose a workbook (the file dialog opens where the last one
        /// came from), then the sheet and columns. Returns null if anything was cancelled
        /// or failed, so a caller can simply bail out.
        /// </summary>
        public static List<(string Name, string AsIs, string ToBe)> PickRows(string taskName, string valueColumnLabel)
        {
            var settings = AppSettings.Current;

            var pick = new OpenFileDialog
            {
                // Both are the same zip-of-XML format; .xls (the old binary format) is
                // not, and would have to be re-saved as .xlsx first.
                Filter = "Excel workbook (*.xlsx;*.xlsm)|*.xlsx;*.xlsm|All files (*.*)|*.*",
                Title = $"Import {valueColumnLabel} from Excel"
            };

            try
            {
                if (!string.IsNullOrEmpty(settings.LastXlsxPath) && File.Exists(settings.LastXlsxPath))
                {
                    pick.FileName = Path.GetFileName(settings.LastXlsxPath);
                    pick.InitialDirectory = Path.GetDirectoryName(settings.LastXlsxPath);
                }
            }
            catch
            {
                // A bad remembered path just means no seeding.
            }

            if (pick.ShowDialog() != true) return null;

            return ShowFor(pick.FileName, taskName, valueColumnLabel);
        }

        /// <summary>
        /// Columns-only re-import: skips the file dialog and goes straight to the sheet
        /// and column picker for the workbook last used. For when the file is settled
        /// and it's the column choice that's being changed — importing a second field
        /// out of the same sheet, or correcting a mis-picked column — so that doesn't
        /// mean walking the file dialog again every time.
        /// </summary>
        public static List<(string Name, string AsIs, string ToBe)> PickColumnsFromLastWorkbook(
            string taskName, string valueColumnLabel)
        {
            string path = AppSettings.Current.LastXlsxPath;

            bool missing;
            try { missing = string.IsNullOrEmpty(path) || !File.Exists(path); }
            catch { missing = true; }

            if (missing)
            {
                MessageBox.Show(
                    string.IsNullOrEmpty(path)
                        ? "No workbook has been opened yet — use \"Load .xlsx...\" first."
                        : $"That workbook is no longer there:\n\n{path}\n\nUse \"Load .xlsx...\" to pick another.",
                    "Pick columns", MessageBoxButton.OK, MessageBoxImage.Information);
                return null;
            }

            return ShowFor(path, taskName, valueColumnLabel);
        }

        /// <summary>
        /// Shows the picker for one workbook and persists what was chosen. Shared by both
        /// entry points so the settings round-trip and error handling exist once.
        /// </summary>
        private static List<(string Name, string AsIs, string ToBe)> ShowFor(
            string path, string taskName, string valueColumnLabel)
        {
            var settings = AppSettings.Current;
            try
            {
                var dlg = new ExcelImportWindow(path, valueColumnLabel, settings.PrefsFor(taskName))
                {
                    Owner = Application.Current?.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                if (dlg.ShowDialog() != true) return null;

                settings.LastXlsxPath = path;
                if (dlg.Chosen != null && !string.IsNullOrEmpty(taskName))
                    settings.Imports[taskName] = dlg.Chosen;
                AppSettings.Save();

                return dlg.Rows;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not read that workbook:\n\n{ex.Message}",
                    "Import from Excel", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
    }
}
