using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SeqxcToolset.Core;

namespace SeqxcToolset.Tasks.PatternStringTask
{
    public class PatternStringViewModel : INotifyPropertyChanged
    {
        private SequenceDocument _document;

        /// <summary>Key this task's remembered import columns are stored under.</summary>
        private const string TaskKey = "Pattern Strings";

        public ObservableCollection<PatternStringRowVM> Rows { get; } = new ObservableCollection<PatternStringRowVM>();

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

        public PatternStringViewModel()
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
                var terminal = _document.ResolveTerminal(item.PatternSetupName);

                // Three distinct states, kept apart on purpose: no terminal at all,
                // a terminal whose file predates <PatternString>, and a terminal with
                // the element present (whose value is normally just empty for now).
                string raw = terminal?.PatternStringRaw;
                string display;
                if (terminal == null) display = "(unresolved)";
                else if (raw == null) display = "(absent)";
                else display = raw;

                var row = new PatternStringRowVM
                {
                    Index = item.Index,
                    Selected = item.Selected,
                    PatternSetupName = item.PatternSetupName,
                    AnalysisType = ShortenType(item.AnalysisType),
                    UserName = item.UserName,
                    ResolvedTerminalName = terminal?.Name,
                    IsAlias = terminal != null &&
                              !terminal.Name.Equals(item.PatternSetupName, StringComparison.OrdinalIgnoreCase),
                    CurrentPatternString = display,
                    CurrentRawValue = raw
                };
                row.PropertyChanged += Row_PropertyChanged;
                Rows.Add(row);
            }
        }

        // Same rule as Pattern Numbers: rows with the exact same PatternSetupName are
        // literally the same PatternSetupList entry, so syncing those live is safe and
        // unsurprising. Deliberately NOT syncing across alias siblings — those have
        // visibly different names and are surfaced at Save time via the
        // "shared by aliasing, continue?" confirmation instead.
        private bool _isSyncingRows;

        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isSyncingRows) return;
            if (!(sender is PatternStringRowVM row) || e.PropertyName != nameof(PatternStringRowVM.NewPatternString)) return;
            if (string.IsNullOrEmpty(row.PatternSetupName)) return;

            _isSyncingRows = true;
            try
            {
                foreach (var other in Rows)
                {
                    if (ReferenceEquals(other, row)) continue;
                    if (string.Equals(other.PatternSetupName, row.PatternSetupName, StringComparison.OrdinalIgnoreCase))
                        other.NewPatternString = row.NewPatternString;
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

        /// <summary>
        /// Reads pattern strings from an .xlsx: pick the workbook, then the sheet and
        /// the two (optionally three) columns.
        ///
        /// This task imports only the columns it edits. A sheet may well hold values
        /// for several tasks at once — the file this was built against carries pattern
        /// string, pattern index, exposure and luminance side by side — but each task
        /// asks for its own columns, so importing here can never write another task's
        /// field by accident.
        /// </summary>
        private void ImportFromExcel()
        {
            if (_document == null)
            {
                ImportSummary = "Open a .seqxc file first.";
                return;
            }

            var rows = ExcelImportWindow.PickRows(TaskKey, "Pattern String");
            if (rows == null) return;   // cancelled, or the workbook could not be read

            ApplyImportRows(rows);
        }

        /// <summary>
        /// Re-imports pattern strings from the workbook already in use, going straight to the
        /// sheet and column picker without the file dialog.
        /// </summary>
        private void PickColumns()
        {
            if (_document == null)
            {
                ImportSummary = "Open a .seqxc file first.";
                return;
            }

            var rows = ExcelImportWindow.PickColumnsFromLastWorkbook(TaskKey, "Pattern String");
            if (rows == null) return;

            ApplyImportRows(rows);
        }

        /// <summary>
        /// Matching for imported rows — same scope and rules as the Pattern Numbers
        /// import:
        ///
        /// Strict scope: only PatternSetups tied to a currently-visible row are
        /// eligible, for both auto-apply and the picker. Orphan library patterns
        /// and steps hidden by the Selected filter are never matched.
        ///
        /// A row's name resolves to a terminal PatternSetup, and EVERY row sharing
        /// that terminal gets the value in one shot. Unresolvable names, and names
        /// resolving to a terminal already assigned a DIFFERENT value earlier in the
        /// same import, go to the picker rather than being guessed at.
        /// </summary>
        private void ApplyImportRows(IEnumerable<(string Name, string AsIs, string ToBe)> rows)
        {
            var assignedTerminals = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int matched = 0, mismatched = 0, ignored = 0, duplicates = 0;
            var mismatchNames = new List<string>();
            bool ignoreAllRemaining = false;

            foreach (var (name, asIs, toBe) in rows)
            {
                if (ignoreAllRemaining) { ignored++; continue; }

                var terminal = _document.ResolveTerminal(name);
                if (terminal != null && !TerminalIsInScope(terminal))
                    terminal = null; // not tied to any currently-visible row — out of scope

                bool alreadyAssigned = terminal != null && assignedTerminals.ContainsKey(terminal.Name);

                // A repeat of a terminal already given the SAME value is not an
                // ambiguity — it is the ordinary shape of a per-step sheet, where one
                // pattern used at two steps gets a line each. The real rule lives in
                // the file: a sequence can list the same pattern twice, and the
                // sheet mirrors that row for row. Only a repeat carrying a DIFFERENT
                // value is genuinely ambiguous and still routes to the picker.
                if (alreadyAssigned && assignedTerminals[terminal.Name] == toBe)
                {
                    duplicates++;
                    continue;
                }

                bool trustworthyAutoMatch = terminal != null && !alreadyAssigned &&
                    !HasBetterScopedMatch(name, terminal);

                if (trustworthyAutoMatch)
                {
                    ApplyValueToTerminal(terminal, toBe, asIs, ref matched, ref mismatched, mismatchNames);
                    assignedTerminals[terminal.Name] = toBe;
                    continue;
                }

                string context = $"{name}   {toBe}   — should apply for which step?";
                var dlg = new ResolveMatchWindow(BuildCandidates(name), context)
                {
                    Owner = Application.Current?.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                dlg.ShowDialog();

                if (dlg.IgnoreAllRemaining) { ignoreAllRemaining = true; ignored++; continue; }
                if (dlg.Ignored || dlg.SelectedName == null) { ignored++; continue; }

                var chosen = _document.ResolveTerminal(dlg.SelectedName);
                if (chosen != null && TerminalIsInScope(chosen))
                {
                    ApplyValueToTerminal(chosen, toBe, null, ref matched, ref mismatched, mismatchNames);
                    assignedTerminals[chosen.Name] = toBe;
                }
                else
                {
                    ignored++;
                }
            }

            var sb = new StringBuilder();
            sb.Append($"Matched {matched} row(s).");
            if (duplicates > 0) sb.Append($"  {duplicates} repeat(s) of the same pattern and value already covered.");
            if (mismatched > 0) sb.Append($"  {mismatched} 'as-is' value(s) didn't match the file — applied anyway.");
            if (ignored > 0) sb.Append($"  {ignored} row(s) ignored.");
            ImportSummary = sb.ToString();
        }

        /// <summary>True if this terminal is tied to at least one currently-visible row.</summary>
        private bool TerminalIsInScope(PatternSetupInfo terminal) =>
            Rows.Any(r => string.Equals(r.ResolvedTerminalName, terminal.Name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Even when an exact-name match is in scope, don't auto-trust it if a
        /// different in-scope step fuzzy-matches the pasted name distinctly better.
        /// Forces the picker instead of guessing wrong.
        /// </summary>
        private bool HasBetterScopedMatch(string name, PatternSetupInfo terminal)
        {
            const int RelevanceThreshold = 40;
            return Rows.Any(r =>
                !string.Equals(r.ResolvedTerminalName, terminal.Name, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(r.PatternSetupName, name, StringComparison.OrdinalIgnoreCase) &&
                NameMatch.Score(name, r.PatternSetupName) >= RelevanceThreshold);
        }

        private void ApplyValueToTerminal(PatternSetupInfo terminal, string toBe, string asIs,
            ref int matched, ref int mismatched, List<string> mismatchNames)
        {
            var targetRows = Rows
                .Where(r => string.Equals(r.ResolvedTerminalName, terminal.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var row in targetRows)
            {
                if (asIs != null && (row.CurrentRawValue ?? "") != asIs)
                {
                    mismatched++;
                    mismatchNames.Add($"{terminal.Name} (file has '{row.CurrentRawValue}', sheet says '{asIs}')");
                }
                row.NewPatternString = toBe;
                matched++;
            }
        }

        private List<ResolveMatchWindow.Candidate> BuildCandidates(string query)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<ResolveMatchWindow.Candidate>();

            foreach (var row in Rows)
            {
                if (string.IsNullOrEmpty(row.PatternSetupName) || !seen.Add(row.PatternSetupName)) continue;
                string appliedNote = !string.IsNullOrEmpty(row.NewPatternString)
                    ? $"   (already applied: {row.NewPatternString})"
                    : "";
                list.Add(new ResolveMatchWindow.Candidate
                {
                    Name = row.PatternSetupName,
                    Label = $"Step #{row.Index + 1}: {row.PatternSetupName}  (current: {row.CurrentPatternString}){appliedNote}",
                    IsStep = true,
                    Score = NameMatch.Score(query, row.PatternSetupName)
                });
            }

            // Best match first: relevance score, then alphabetical as a stable tiebreaker.
            return list
                .OrderByDescending(c => c.Score)
                .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }


        public bool HasPendingChanges => Rows.Any(r => r.IsDirty);

        public void ClearNewValues()
        {
            foreach (var row in Rows)
                row.NewPatternString = null;
            ImportSummary = "";
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
                MessageBox.Show("Nothing to save — no rows have a new pattern string.",
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
            if (result.Cancelled) return;

            if (result.ChangeCount == 0)
            {
                MessageBox.Show("Nothing to save — no rows have a new pattern string.",
                    "Seqxc Toolset", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (result.Warnings.Count > 0)
                MessageBox.Show("Saved with some warnings:\n\n" + string.Join("\n", result.Warnings),
                    "Saved with warnings", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                MessageBox.Show($"Saved {result.ChangeCount} pattern string change(s) to:\n{dlg.FileName}",
                    "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload straight from the file we just wrote, rather than trusting
            // in-memory state — confirms the save actually landed and also makes
            // FilePath point at the saved copy for any further edits/saves.
            _document.Load(dlg.FileName);
            RebuildRows();
        }

        /// <summary>
        /// Used by the global "Save All Changes" flow: applies this task's pending
        /// edits and writes them to targetPath, but doesn't show its own success
        /// dialog or reload/refresh — the caller does that once for every task after
        /// the whole batch completes.
        /// </summary>
        public SaveResult SaveAllInternal(string targetPath)
        {
            var dirtyRows = Rows.Where(r => r.IsDirty).ToList();
            if (dirtyRows.Count == 0) return new SaveResult();
            return ApplyAndSave(dirtyRows, targetPath);
        }

        private SaveResult ApplyAndSave(List<PatternStringRowVM> dirtyRows, string targetPath)
        {
            // Resolve every dirty row down to its terminal PatternSetup and detect
            // conflicts (two dirty rows resolving to the same terminal with different
            // new values).
            var terminalTargets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var conflicts = new List<string>();
            var allAffectedSiblings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in dirtyRows)
            {
                string terminalName = row.ResolvedTerminalName ?? row.PatternSetupName;
                if (terminalTargets.TryGetValue(terminalName, out var existingValue) &&
                    existingValue != row.NewPatternString)
                {
                    conflicts.Add($"'{terminalName}' is set to both '{existingValue}' and '{row.NewPatternString}'");
                    continue;
                }
                terminalTargets[terminalName] = row.NewPatternString;

                foreach (var sib in _document.GetSiblingAliases(terminalName))
                    if (!terminalTargets.ContainsKey(sib) &&
                        dirtyRows.All(r => !r.PatternSetupName.Equals(sib, StringComparison.OrdinalIgnoreCase)))
                        allAffectedSiblings.Add($"{sib} (alias of {terminalName})");
            }

            if (conflicts.Count > 0)
            {
                MessageBox.Show("Conflicting new values were found:\n\n" + string.Join("\n", conflicts) +
                    "\n\nResolve these before saving.", "Conflict", MessageBoxButton.OK, MessageBoxImage.Warning);
                return new SaveResult { Cancelled = true };
            }

            if (allAffectedSiblings.Count > 0)
            {
                var confirm = MessageBox.Show(
                    "Some of these patterns are shared by other names via aliasing:\n\n" +
                    string.Join("\n", allAffectedSiblings) +
                    "\n\nChanging the terminal pattern string will affect those too. Continue?",
                    "Aliases affected", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return new SaveResult { Cancelled = true };
            }

            var changes = new List<PatternStringChange>();
            foreach (var kvp in terminalTargets)
            {
                var terminal = _document.PatternSetups.TryGetValue(kvp.Key, out var info) ? info : null;
                if (terminal == null) continue;

                // OldValue carries null through to the writer when the element is
                // missing, so it reports that specifically instead of a generic
                // "couldn't patch" — and the in-memory Set is skipped for the same
                // reason, leaving the document untouched.
                changes.Add(new PatternStringChange
                {
                    TerminalName = terminal.Name,
                    OldValue = terminal.PatternStringRaw,
                    NewValue = kvp.Value
                });
                if (terminal.PatternStringRaw != null)
                    _document.SetPatternString(terminal.Name, kvp.Value, out _, out _);
            }

            var warnings = _document.SavePatternStringChanges(targetPath, changes);

            // Entries with no element on disk were reported, not written — don't
            // count them as changes.
            int written = changes.Count(c => c.OldValue != null);
            return new SaveResult { Warnings = warnings, ChangeCount = written };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
