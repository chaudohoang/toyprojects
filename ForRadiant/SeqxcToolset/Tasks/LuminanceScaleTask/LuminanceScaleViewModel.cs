using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SeqxcToolset.Core;

namespace SeqxcToolset.Tasks.LuminanceScaleTask
{
    public class LuminanceScaleViewModel : INotifyPropertyChanged
    {
        private SequenceDocument _document;

        /// <summary>Key this task's remembered import columns are stored under.</summary>
        private const string TaskKey = "Luminance Scale";

        public ObservableCollection<LuminanceRowVM> Rows { get; } = new ObservableCollection<LuminanceRowVM>();

        private bool _showAllItems;
        public bool ShowAllItems
        {
            get => _showAllItems;
            set { _showAllItems = value; OnPropertyChanged(); RebuildRows(); }
        }

        private string _importSummary = "";
        public string ImportSummary
        {
            get => _importSummary;
            set { _importSummary = value; OnPropertyChanged(); }
        }


        /// <summary>
        /// Full path of the workbook an import would read, shown in the bottom bar so
        /// "Choose columns" is never a guess about which file it will use. The whole
        /// path rather than just the file name: two rule sheets in different folders can
        /// share a name, and it matches how the sequence path is shown in the top
        /// toolbar.
        /// </summary>
        public string ExcelFileLabel
        {
            get
            {
                string path = AppSettings.Current.LastXlsxPath;
                return string.IsNullOrEmpty(path) ? "(none chosen yet)" : path;
            }
        }

        /// <summary>Full path, for the label's tooltip.</summary>
        public string ExcelFilePath => AppSettings.Current.LastXlsxPath;

        public ICommand ImportFromExcelCommand { get; }
        public ICommand PickColumnsCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearNewValuesCommand { get; }

        public LuminanceScaleViewModel()
        {
            ImportFromExcelCommand = new RelayCommand(_ => ImportFromExcel());
            PickColumnsCommand = new RelayCommand(_ => PickColumns());
            SaveCommand = new RelayCommand(_ => SaveChanges());
            ClearNewValuesCommand = new RelayCommand(_ => ClearNewValues());

            // The workbook is app-wide: choosing one in another task has to update the
            // label here too, so follow the change rather than reading it once.
            AppSettings.LastXlsxPathChanged += (s, e) =>
            {
                OnPropertyChanged(nameof(ExcelFileLabel));
                OnPropertyChanged(nameof(ExcelFilePath));
            };
        }

        /// <summary>
        /// Reads luminance scales from an .xlsx: pick the workbook, then the sheet, the
        /// pattern-name column and the luminance column. Only those columns are read.
        /// </summary>
        private void ImportFromExcel()
        {
            if (_document == null)
            {
                ImportSummary = "Open a .seqxc file first.";
                return;
            }

            var rows = ExcelImportWindow.PickRows(TaskKey, "Luminance Scale");
            if (rows == null) return;   // cancelled, or the workbook could not be read

            ApplyImportRows(rows);
        }

        /// <summary>
        /// Re-imports luminance scales from the workbook already in use, going straight to the
        /// sheet and column picker without the file dialog.
        /// </summary>
        private void PickColumns()
        {
            if (_document == null)
            {
                ImportSummary = "Open a .seqxc file first.";
                return;
            }

            var rows = ExcelImportWindow.PickColumnsFromLastWorkbook(TaskKey, "Luminance Scale");
            if (rows == null) return;

            ApplyImportRows(rows);
        }

        /// <summary>
        /// Matching is by PatternSetupName, and then only onto steps that actually carry
        /// the fields: LuminanceScaleRed/Green/Blue live on a SequenceItem's own Analysis
        /// element and only some Analysis types have them. That filter is what makes a
        /// per-step sheet work — the rule sheet this was built for lists CalG twice, once
        /// with a luminance value and once blank, mirroring the two steps that use it, of
        /// which only one has the fields. The blank row carries no value so it is dropped
        /// before it gets here, and the valued row lands on the one step that can take it.
        ///
        /// The single sheet column is broadcast to Red/Green/Blue through NewAll, the same
        /// one-way fan-out as typing in the "New (all)" cell.
        ///
        /// A name with no matching step, or a repeat of one already given a DIFFERENT
        /// value, opens the same match picker the other tasks use. Candidates are limited
        /// to steps that actually carry luminance fields: offering one that can't take a
        /// value would let you choose a step and then see nothing happen.
        ///
        /// Like Exposure Time, this does not copy the pattern tasks' "an exact match
        /// loses to a distinctly better fuzzy match" rule — "CalG" scores 55 against
        /// "Cal2G", so it would raise a dialog on rows that already match cleanly.
        /// </summary>
        private void ApplyImportRows(IEnumerable<(string Name, string AsIs, string ToBe)> rows)
        {
            var assigned = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int matched = 0, duplicates = 0, mismatched = 0, noFields = 0, ignored = 0;
            bool ignoreAllRemaining = false;

            foreach (var (name, asIs, toBe) in rows)
            {
                if (ignoreAllRemaining) { ignored++; continue; }

                bool alreadyAssigned = assigned.TryGetValue(name, out var previous);
                if (alreadyAssigned && previous == toBe)
                {
                    duplicates++;
                    continue;
                }

                // A step whose Analysis type has no luminance fields has nothing to
                // write, so it is reported rather than counted as matched — and is not
                // offered in the picker either.
                var named = Rows.Where(r => string.Equals(r.PatternSetupName, name, StringComparison.OrdinalIgnoreCase)).ToList();
                var usable = UsableRowsNamed(name);
                if (named.Count > 0 && usable.Count == 0)
                {
                    noFields++;
                    continue;
                }

                if (usable.Count > 0 && !alreadyAssigned)
                {
                    Apply(usable, asIs, toBe, ref matched, ref mismatched);
                    assigned[name] = toBe;
                    continue;
                }

                var dlg = new ResolveMatchWindow(BuildCandidates(name),
                    $"{name}   {toBe}   — should apply for which step?")
                {
                    Owner = Application.Current?.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                dlg.ShowDialog();

                if (dlg.IgnoreAllRemaining) { ignoreAllRemaining = true; ignored++; continue; }
                if (dlg.Ignored || dlg.SelectedName == null) { ignored++; continue; }

                var chosen = UsableRowsNamed(dlg.SelectedName);
                if (chosen.Count == 0) { ignored++; continue; }

                Apply(chosen, null, toBe, ref matched, ref mismatched);
                assigned[dlg.SelectedName] = toBe;
            }

            var parts = new List<string> { $"Matched {matched} row(s)." };
            if (duplicates > 0) parts.Add($"{duplicates} repeat(s) of the same pattern and value already covered.");
            if (noFields > 0) parts.Add($"{noFields} pattern(s) matched a step with no luminance fields — skipped.");
            if (mismatched > 0) parts.Add($"{mismatched} 'as-is' value(s) didn't match the file — applied anyway.");
            if (ignored > 0) parts.Add($"{ignored} row(s) ignored.");
            ImportSummary = string.Join("  ", parts);
        }

        /// <summary>Steps with this name that actually carry luminance fields.</summary>
        private List<LuminanceRowVM> UsableRowsNamed(string name) =>
            Rows.Where(r => string.Equals(r.PatternSetupName, name, StringComparison.OrdinalIgnoreCase)
                            && r.HasLuminanceFields)
                .ToList();

        private void Apply(List<LuminanceRowVM> targets, string asIs, string toBe,
            ref int matched, ref int mismatched)
        {
            foreach (var row in targets)
            {
                if (asIs != null &&
                    !(SameValue(row.Red, asIs) && SameValue(row.Green, asIs) && SameValue(row.Blue, asIs)))
                    mismatched++;

                row.NewAll = toBe;
                matched++;
            }
        }

        /// <summary>
        /// One entry per distinct step name that can take a luminance value, ranked by
        /// similarity to the imported name and showing its current R/G/B.
        /// </summary>
        private List<ResolveMatchWindow.Candidate> BuildCandidates(string query)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<ResolveMatchWindow.Candidate>();

            foreach (var row in Rows)
            {
                if (!row.HasLuminanceFields) continue;
                if (string.IsNullOrEmpty(row.PatternSetupName) || !seen.Add(row.PatternSetupName)) continue;
                string appliedNote = !string.IsNullOrEmpty(row.NewAll)
                    ? $"   (already applied: {row.NewAll})"
                    : "";
                list.Add(new ResolveMatchWindow.Candidate
                {
                    Name = row.PatternSetupName,
                    Label = $"Step #{row.Index + 1}: {row.PatternSetupName}  (R/G/B: {row.Red}/{row.Green}/{row.Blue}){appliedNote}",
                    IsStep = true,
                    Score = NameMatch.Score(query, row.PatternSetupName)
                });
            }

            return list
                .OrderByDescending(c => c.Score)
                .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Compares a sheet value with one from the file numerically when both parse as
        /// numbers, so 100 and 100.0 are not reported as a difference. Only the "as is"
        /// check uses this; what gets written is the sheet's text exactly as given.
        /// </summary>
        private static bool SameValue(string a, string b)
        {
            if (a == null || b == null) return false;
            if (double.TryParse(a, NumberStyles.Any, CultureInfo.InvariantCulture, out double da) &&
                double.TryParse(b, NumberStyles.Any, CultureInfo.InvariantCulture, out double db))
                return da == db;
            return a.Trim() == b.Trim();
        }


        public void LoadDocument(SequenceDocument document)
        {
            _document = document;
            RebuildRows();
        }

        public void RebuildRows()
        {
            Rows.Clear();
            if (_document == null) return;

            var items = _showAllItems
                ? _document.Items
                : _document.Items.Where(i => i.Selected);

            foreach (var item in items)
            {
                Rows.Add(new LuminanceRowVM
                {
                    Index = item.Index,
                    Selected = item.Selected,
                    PatternSetupName = item.PatternSetupName,
                    AnalysisType = ShortenType(item.AnalysisType),
                    Red = item.LuminanceRed,
                    Green = item.LuminanceGreen,
                    Blue = item.LuminanceBlue
                });
            }
        }

        private static string ShortenType(string xsiType)
        {
            if (string.IsNullOrEmpty(xsiType)) return "";
            int dot = xsiType.LastIndexOf('.');
            return dot >= 0 ? xsiType.Substring(dot + 1) : xsiType;
        }

        public bool HasPendingChanges => Rows.Any(r => r.IsDirty);

        public void ClearNewValues()
        {
            foreach (var row in Rows)
            {
                row.NewRed = null;
                row.NewGreen = null;
                row.NewBlue = null;
                row.NewAll = null;
            }
        }

        private void SaveChanges()
        {
            if (_document == null)
            {
                MessageBox.Show("No file loaded.", "Seqxc Toolset");
                return;
            }

            var dirtyRows = Rows.Where(r => r.IsDirty).ToList();
            if (dirtyRows.Count == 0)
            {
                MessageBox.Show("Nothing to save — no rows have a new value.",
                    "Seqxc Toolset", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dlg = new SaveFileDialog
            {
                Filter = "TrueTest Sequence (*.seqxc)|*.seqxc|All files (*.*)|*.*",
                FileName = System.IO.Path.GetFileName(_document.FilePath),
                InitialDirectory = System.IO.Path.GetDirectoryName(_document.FilePath)
            };
            if (dlg.ShowDialog() != true) return;

            var result = ApplyAndSave(dirtyRows, dlg.FileName);

            if (result.ChangeCount == 0)
            {
                MessageBox.Show("Nothing to save — new values matched the current ones, or this step's " +
                    "Analysis type doesn't have Luminance Scale fields.",
                    "Seqxc Toolset", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (result.Warnings.Count > 0)
                MessageBox.Show("Saved with some warnings:\n\n" + string.Join("\n", result.Warnings),
                    "Saved with warnings", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                MessageBox.Show($"Saved {result.ChangeCount} change(s) to:\n{dlg.FileName}",
                    "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload from the file we just wrote, same as the other tasks, so
            // Current values always reflect what's actually on disk.
            _document.Load(dlg.FileName);
            RebuildRows();
        }

        /// <summary>
        /// Used by the global "Save All Changes" flow — see
        /// PatternNumberViewModel.SaveAllInternal for the general pattern.
        /// </summary>
        public SaveResult SaveAllInternal(string targetPath)
        {
            var dirtyRows = Rows.Where(r => r.IsDirty).ToList();
            if (dirtyRows.Count == 0) return new SaveResult();
            return ApplyAndSave(dirtyRows, targetPath);
        }

        private SaveResult ApplyAndSave(List<LuminanceRowVM> dirtyRows, string targetPath)
        {
            var changes = new List<LuminanceScaleChange>();
            foreach (var row in dirtyRows)
            {
                ApplyFieldIfChanged(row.Index, "LuminanceScaleRed", row.NewRed, row.Red, changes);
                ApplyFieldIfChanged(row.Index, "LuminanceScaleGreen", row.NewGreen, row.Green, changes);
                ApplyFieldIfChanged(row.Index, "LuminanceScaleBlue", row.NewBlue, row.Blue, changes);
            }

            if (changes.Count == 0) return new SaveResult();

            var warnings = _document.SaveLuminanceScaleChanges(targetPath, changes);
            return new SaveResult { Warnings = warnings, ChangeCount = changes.Count };
        }

        private void ApplyFieldIfChanged(int itemIndex, string fieldTag, string newVal, string currentVal,
            List<LuminanceScaleChange> changes)
        {
            if (string.IsNullOrEmpty(newVal) || currentVal == null || newVal.Trim() == currentVal) return;

            bool ok = _document.SetSequenceItemField(itemIndex, fieldTag, newVal.Trim(), out var oldValue);
            if (!ok) return;

            changes.Add(new LuminanceScaleChange
            {
                ItemIndex = itemIndex,
                FieldTag = fieldTag,
                OldValue = oldValue,
                NewValue = newVal.Trim()
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
