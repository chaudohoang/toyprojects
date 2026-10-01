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

namespace SeqxcToolset.Tasks.ExposureTimeTask
{
    public class ExposureTimeViewModel : INotifyPropertyChanged
    {
        // Must match SequenceDocument.KnownChannels order/positions.
        private const int YIndex = 1;
        private const int XIndex = 2;
        private const int ZIndex = 3;

        private SequenceDocument _document;

        /// <summary>Key this task's remembered import columns are stored under.</summary>
        private const string TaskKey = "Exposure Time";

        public ObservableCollection<ExposureRowVM> Rows { get; } = new ObservableCollection<ExposureRowVM>();

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

        public ExposureTimeViewModel()
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
        /// Reads exposure times from an .xlsx: pick the workbook, then the sheet, the
        /// pattern-name column and the exposure column. Only those columns are read, so
        /// a rule sheet that also holds pattern index, pattern string and luminance
        /// cannot have its other fields written from here.
        /// </summary>
        private void ImportFromExcel()
        {
            if (_document == null)
            {
                ImportSummary = "Open a .seqxc file first.";
                return;
            }

            var rows = ExcelImportWindow.PickRows(TaskKey, "Exposure Time");
            if (rows == null) return;   // cancelled, or the workbook could not be read

            ApplyImportRows(rows);
        }

        /// <summary>
        /// Re-imports exposure times from the workbook already in use, going straight to the
        /// sheet and column picker without the file dialog.
        /// </summary>
        private void PickColumns()
        {
            if (_document == null)
            {
                ImportSummary = "Open a .seqxc file first.";
                return;
            }

            var rows = ExcelImportWindow.PickColumnsFromLastWorkbook(TaskKey, "Exposure Time");
            if (rows == null) return;

            ApplyImportRows(rows);
        }

        /// <summary>
        /// Matching here is by EXACT PatternSetupName, deliberately not through the
        /// alias chain: CaptureFilter/ExposureTime live on each named PatternSetup's own
        /// element, so an alias does not share them — unlike PatternNumber and
        /// PatternString, where it does.
        ///
        /// The sheet carries one exposure column, which is broadcast to Y/X/Z through
        /// NewExpAll — the same one-way fan-out as typing in the "New Exp (all)" cell,
        /// and the usual case where the three channels agree. A channel that needs to
        /// differ is still overridden individually afterwards.
        ///
        /// A name with no matching step, or a repeat of one already given a DIFFERENT
        /// value, opens the same match picker the pattern tasks use: candidates ranked by
        /// NameMatch.Score, with Ignore and Ignore All Remaining. A rule sheet can name
        /// its patterns on a different scheme than the sequence does — "W48" against a
        /// sequence holding "g48"/"r48"/"b48" — and that mapping is a judgement only the
        /// person importing can make.
        ///
        /// What this deliberately does NOT copy from the pattern tasks is their
        /// "an exact match loses to a distinctly better fuzzy match" rule. Here it would
        /// backfire: "CalG" scores 55 against "Cal2G", over the threshold, so every
        /// cleanly-matching row would raise a dialog too. An exact name match is trusted.
        /// </summary>
        private void ApplyImportRows(IEnumerable<(string Name, string AsIs, string ToBe)> rows)
        {
            var assigned = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int matched = 0, duplicates = 0, mismatched = 0, ignored = 0;
            bool ignoreAllRemaining = false;

            foreach (var (name, asIs, toBe) in rows)
            {
                if (ignoreAllRemaining) { ignored++; continue; }

                // A repeat of the same pattern with the same value is the normal shape
                // of a per-step sheet, not a conflict.
                bool alreadyAssigned = assigned.TryGetValue(name, out var previous);
                if (alreadyAssigned && previous == toBe)
                {
                    duplicates++;
                    continue;
                }

                var targets = RowsNamed(name);

                // Trust an exact name match outright; anything else is for the person
                // importing to decide.
                if (targets.Count > 0 && !alreadyAssigned)
                {
                    Apply(targets, asIs, toBe, ref matched, ref mismatched);
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

                var chosen = RowsNamed(dlg.SelectedName);
                if (chosen.Count == 0) { ignored++; continue; }

                Apply(chosen, null, toBe, ref matched, ref mismatched);
                assigned[dlg.SelectedName] = toBe;
            }

            var parts = new List<string> { $"Matched {matched} row(s)." };
            if (duplicates > 0) parts.Add($"{duplicates} repeat(s) of the same pattern and value already covered.");
            if (mismatched > 0) parts.Add($"{mismatched} 'as-is' value(s) didn't match the file — applied anyway.");
            if (ignored > 0) parts.Add($"{ignored} row(s) ignored.");
            ImportSummary = string.Join("  ", parts);
        }

        private List<ExposureRowVM> RowsNamed(string name) =>
            Rows.Where(r => string.Equals(r.PatternSetupName, name, StringComparison.OrdinalIgnoreCase))
                .ToList();

        private void Apply(List<ExposureRowVM> targets, string asIs, string toBe,
            ref int matched, ref int mismatched)
        {
            foreach (var row in targets)
            {
                if (asIs != null &&
                    !(SameValue(row.YExposure, asIs) && SameValue(row.XExposure, asIs) && SameValue(row.ZExposure, asIs)))
                    mismatched++;

                row.NewExpAll = toBe;
                matched++;
            }
        }

        /// <summary>
        /// One entry per distinct step name, ranked by similarity to the imported name,
        /// showing its current exposure so the right one is recognisable.
        /// </summary>
        private List<ResolveMatchWindow.Candidate> BuildCandidates(string query)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<ResolveMatchWindow.Candidate>();

            foreach (var row in Rows)
            {
                if (string.IsNullOrEmpty(row.PatternSetupName) || !seen.Add(row.PatternSetupName)) continue;
                string appliedNote = !string.IsNullOrEmpty(row.NewExpAll)
                    ? $"   (already applied: {row.NewExpAll})"
                    : "";
                list.Add(new ResolveMatchWindow.Candidate
                {
                    Name = row.PatternSetupName,
                    Label = $"Step #{row.Index + 1}: {row.PatternSetupName}  (Y/X/Z: {row.YExposure}/{row.XExposure}/{row.ZExposure}){appliedNote}",
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
            foreach (var oldRow in Rows)
                oldRow.PropertyChanged -= Row_PropertyChanged;
            Rows.Clear();
            if (_document == null) return;

            var items = _showAllItems
                ? _document.Items
                : _document.Items.Where(i => i.Selected);

            foreach (var item in items)
            {
                var row = new ExposureRowVM
                {
                    Index = item.Index,
                    Selected = item.Selected,
                    PatternSetupName = item.PatternSetupName,
                    AnalysisType = ShortenType(item.AnalysisType)
                };

                foreach (var ch in _document.GetChannels(item.PatternSetupName))
                {
                    if (ch.Label.StartsWith("Y")) { row.YCapture = ch.Capture; row.YExposure = ch.ExposureMs; }
                    else if (ch.Label.StartsWith("X")) { row.XCapture = ch.Capture; row.XExposure = ch.ExposureMs; }
                    else if (ch.Label.StartsWith("Z")) { row.ZCapture = ch.Capture; row.ZExposure = ch.ExposureMs; }
                }

                row.PropertyChanged += Row_PropertyChanged;
                Rows.Add(row);
            }
        }

        // Two or more SequenceItems can share the same PatternSetupName (e.g.
        // "CalG" used at both a RegisterPixelsLGDN and a DemuraLGDNPOCB4p2
        // step) — since CaptureFilter/ExposureTime lives on that ONE shared
        // PatternSetup, they're literally the same underlying data. Editing
        // one row's New value should visibly mirror into every other row
        // with the same name, not just apply consistently at save time.
        private bool _isSyncingRows;
        private static readonly HashSet<string> NewFieldNames = new HashSet<string>
        {
            nameof(ExposureRowVM.NewYCapture), nameof(ExposureRowVM.NewYExposure),
            nameof(ExposureRowVM.NewXCapture), nameof(ExposureRowVM.NewXExposure),
            nameof(ExposureRowVM.NewZCapture), nameof(ExposureRowVM.NewZExposure),
            nameof(ExposureRowVM.NewExpAll)
        };

        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isSyncingRows) return;
            if (!(sender is ExposureRowVM row) || e.PropertyName == null) return;
            if (!NewFieldNames.Contains(e.PropertyName)) return; // ignores IsDirty itself

            _isSyncingRows = true;
            try
            {
                foreach (var other in Rows)
                {
                    if (ReferenceEquals(other, row)) continue;
                    if (!string.Equals(other.PatternSetupName, row.PatternSetupName, StringComparison.OrdinalIgnoreCase))
                        continue;

                    switch (e.PropertyName)
                    {
                        case nameof(ExposureRowVM.NewYCapture): other.NewYCapture = row.NewYCapture; break;
                        case nameof(ExposureRowVM.NewYExposure): other.NewYExposure = row.NewYExposure; break;
                        case nameof(ExposureRowVM.NewXCapture): other.NewXCapture = row.NewXCapture; break;
                        case nameof(ExposureRowVM.NewXExposure): other.NewXExposure = row.NewXExposure; break;
                        case nameof(ExposureRowVM.NewZCapture): other.NewZCapture = row.NewZCapture; break;
                        case nameof(ExposureRowVM.NewZExposure): other.NewZExposure = row.NewZExposure; break;
                        case nameof(ExposureRowVM.NewExpAll): other.NewExpAll = row.NewExpAll; break;
                    }
                }
            }
            finally
            {
                _isSyncingRows = false;
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
                row.NewYCapture = null; row.NewYExposure = null;
                row.NewXCapture = null; row.NewXExposure = null;
                row.NewZCapture = null; row.NewZExposure = null;
                row.NewExpAll = null;
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
                MessageBox.Show("Nothing to save — new values matched the current ones, or couldn't be parsed.",
                    "Seqxc Toolset", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (result.Warnings.Count > 0)
                MessageBox.Show("Saved with some warnings:\n\n" + string.Join("\n", result.Warnings),
                    "Saved with warnings", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                MessageBox.Show($"Saved {result.ChangeCount} change(s) to:\n{dlg.FileName}",
                    "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload from the file we just wrote, same as Pattern Numbers, so
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

        private SaveResult ApplyAndSave(List<ExposureRowVM> dirtyRows, string targetPath)
        {
            var changes = new List<ExposureChange>();
            var processedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in dirtyRows)
            {
                ApplyChannelIfChanged(row, YIndex, row.NewYCapture, row.YCapture, row.NewYExposure, row.YExposure, changes, processedKeys);
                ApplyChannelIfChanged(row, XIndex, row.NewXCapture, row.XCapture, row.NewXExposure, row.XExposure, changes, processedKeys);
                ApplyChannelIfChanged(row, ZIndex, row.NewZCapture, row.ZCapture, row.NewZExposure, row.ZExposure, changes, processedKeys);
            }

            if (changes.Count == 0) return new SaveResult();

            var warnings = _document.SaveExposureChanges(targetPath, changes);
            return new SaveResult { Warnings = warnings, ChangeCount = changes.Count };
        }

        private void ApplyChannelIfChanged(ExposureRowVM row, int channelIndex,
            bool? newCaptureVal, bool currentCapture, string newExposureStr, string currentExposure,
            List<ExposureChange> changes, HashSet<string> processedKeys)
        {
            bool? newCapture = (newCaptureVal.HasValue && newCaptureVal.Value != currentCapture)
                ? newCaptureVal
                : null;
            // Rows sharing a PatternSetupName point at the same underlying
            // element — only process each (name, channel, field) combo once,
            // otherwise the second row's "change" would fail to patch since
            // the first already updated the on-disk text.
            if (newCapture.HasValue && !processedKeys.Add($"{row.PatternSetupName}|{channelIndex}|capture"))
                newCapture = null;

            string newExposure = null;
            if (!string.IsNullOrEmpty(newExposureStr) && newExposureStr.Trim() != currentExposure)
                newExposure = newExposureStr.Trim();
            if (newExposure != null && !processedKeys.Add($"{row.PatternSetupName}|{channelIndex}|exposure"))
                newExposure = null;

            if (newCapture == null && newExposure == null) return;

            bool ok = _document.SetChannelValue(row.PatternSetupName, channelIndex,
                newCapture, newExposure, out var oldCapture, out var oldExposure);
            if (!ok) return;

            changes.Add(new ExposureChange
            {
                PatternSetupName = row.PatternSetupName,
                ChannelIndex = channelIndex,
                OldCapture = oldCapture,
                NewCapture = newCapture.HasValue ? (newCapture.Value ? "true" : "false") : null,
                OldExposure = oldExposure,
                NewExposure = newExposure
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
