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

namespace SeqxcToolset.Tasks.PatternNumberTask
{
    public class PatternNumberViewModel : INotifyPropertyChanged
    {
        private SequenceDocument _document;

        public ObservableCollection<PatternRowVM> Rows { get; } = new ObservableCollection<PatternRowVM>();

        private bool _showAllItems;
        public bool ShowAllItems
        {
            get => _showAllItems;
            set { _showAllItems = value; OnPropertyChanged(); RebuildRows(); }
        }

        private string _pasteText = "";
        public string PasteText
        {
            get => _pasteText;
            set { _pasteText = value; OnPropertyChanged(); }
        }

        private string _importSummary = "";
        public string ImportSummary
        {
            get => _importSummary;
            set { _importSummary = value; OnPropertyChanged(); }
        }

        public ICommand ParseImportCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearNewValuesCommand { get; }

        public PatternNumberViewModel()
        {
            ParseImportCommand = new RelayCommand(_ => ParseImport());
            SaveCommand = new RelayCommand(_ => SaveChanges());
            ClearNewValuesCommand = new RelayCommand(_ => ClearNewValues());
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
                var row = new PatternRowVM
                {
                    Index = item.Index,
                    Selected = item.Selected,
                    PatternSetupName = item.PatternSetupName,
                    AnalysisType = ShortenType(item.AnalysisType),
                    UserName = item.UserName,
                    ResolvedTerminalName = terminal?.Name,
                    IsAlias = terminal != null &&
                              !terminal.Name.Equals(item.PatternSetupName, StringComparison.OrdinalIgnoreCase),
                    CurrentPatternNumber = terminal?.PatternNumberRaw ?? "(unresolved)"
                };
                row.PropertyChanged += Row_PropertyChanged;
                Rows.Add(row);
            }
        }

        // Rows with the exact same PatternSetupName are literally the same
        // PatternSetupList entry (e.g. two "CalG" steps) — syncing those live
        // is safe and unsurprising, since the sameness is obvious just from
        // reading the name. Deliberately NOT syncing across alias siblings
        // (r216/g216/b216 -> W216): those have visibly different names, and
        // that side effect is already surfaced explicitly at Save time via
        // the "shared by aliasing, continue?" confirmation — silently
        // cascading it live here would bypass that warning.
        private bool _isSyncingRows;

        private void Row_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isSyncingRows) return;
            if (!(sender is PatternRowVM row) || e.PropertyName != nameof(PatternRowVM.NewPatternNumber)) return;
            if (string.IsNullOrEmpty(row.PatternSetupName)) return;

            _isSyncingRows = true;
            try
            {
                foreach (var other in Rows)
                {
                    if (ReferenceEquals(other, row)) continue;
                    if (string.Equals(other.PatternSetupName, row.PatternSetupName, StringComparison.OrdinalIgnoreCase))
                        other.NewPatternNumber = row.NewPatternNumber;
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
        /// Parses text pasted from an Excel range (tab-separated, 2 columns
        /// Name/To-be, or 3 columns Name/As-is/To-be).
        ///
        /// Strict scope: only PatternSetups tied to a currently-visible row —
        /// i.e. a selected SequenceItem, or any item at all if "Show all
        /// items" is checked — are ever eligible, for both auto-apply and the
        /// picker. Orphan library patterns with no item reference, and steps
        /// hidden by the Selected filter, are never matched.
        ///
        /// Matching rule: a pasted name resolves to a terminal PatternSetup.
        /// EVERY row sharing that exact terminal gets the same new value in
        /// one shot — e.g. two SequenceItems both named "CalG" are literally
        /// the same underlying pattern, so one pasted "CalG" line updates both.
        ///
        /// If a name can't be resolved within that scope, OR it resolves to a
        /// terminal that already received a value earlier in this same paste
        /// (a genuine ambiguity — e.g. several differently-valued "W16" lines
        /// that can't all be the same node), a picker pops up asking which
        /// item this particular line should apply to, or to ignore it.
        /// </summary>
        private void ParseImport()
        {
            if (string.IsNullOrWhiteSpace(PasteText))
            {
                ImportSummary = "Paste some rows first.";
                return;
            }
            if (_document == null)
            {
                ImportSummary = "Open a file first.";
                return;
            }

            var assignedTerminals = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            int matched = 0, mismatched = 0, ignored = 0;
            var mismatchNames = new List<string>();
            bool ignoreAllRemaining = false;

            foreach (var (name, asIs, toBe) in EnumeratePasteLines(PasteText))
            {
                if (ignoreAllRemaining) { ignored++; continue; }

                var terminal = _document.ResolveTerminal(name);
                if (terminal != null && !TerminalIsInScope(terminal))
                    terminal = null; // not tied to any currently-visible row — out of scope

                bool alreadyAssigned = terminal != null && assignedTerminals.ContainsKey(terminal.Name);
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
            if (mismatched > 0) sb.Append($"  {mismatched} 'as-is' value(s) didn't match the file — applied anyway.");
            if (ignored > 0) sb.Append($"  {ignored} row(s) ignored.");
            ImportSummary = sb.ToString();
        }

        /// <summary>True if this terminal is tied to at least one currently-visible row.</summary>
        private bool TerminalIsInScope(PatternSetupInfo terminal) =>
            Rows.Any(r => string.Equals(r.ResolvedTerminalName, terminal.Name, StringComparison.OrdinalIgnoreCase));

        /// <summary>
        /// Even when an exact-name match is in scope, don't auto-trust it if a
        /// different in-scope step fuzzy-matches the pasted name distinctly
        /// better — e.g. "W34" landing on a literal (but coincidental) "W34"
        /// step when "W34_10NIT" is clearly the intended target given the
        /// rest of the paste. Forces the picker instead of guessing wrong.
        /// </summary>
        private bool HasBetterScopedMatch(string name, PatternSetupInfo terminal)
        {
            const int RelevanceThreshold = 40;
            return Rows.Any(r =>
                !string.Equals(r.ResolvedTerminalName, terminal.Name, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(r.PatternSetupName, name, StringComparison.OrdinalIgnoreCase) &&
                ScoreMatch(name, r.PatternSetupName) >= RelevanceThreshold);
        }

        private void ApplyValueToTerminal(PatternSetupInfo terminal, string toBe, string asIs,
            ref int matched, ref int mismatched, List<string> mismatchNames)
        {
            var targetRows = Rows
                .Where(r => string.Equals(r.ResolvedTerminalName, terminal.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var row in targetRows)
            {
                if (!string.IsNullOrEmpty(asIs) && row.CurrentPatternNumber != asIs)
                {
                    mismatched++;
                    mismatchNames.Add($"{terminal.Name} (file has {row.CurrentPatternNumber}, sheet says {asIs})");
                }
                row.NewPatternNumber = toBe;
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
                string appliedNote = !string.IsNullOrEmpty(row.NewPatternNumber)
                    ? $"   (already applied: {row.NewPatternNumber})"
                    : "";
                list.Add(new ResolveMatchWindow.Candidate
                {
                    Name = row.PatternSetupName,
                    Label = $"Step #{row.Index + 1}: {row.PatternSetupName}  (current: {row.CurrentPatternNumber}){appliedNote}",
                    IsStep = true,
                    Score = ScoreMatch(query, row.PatternSetupName)
                });
            }

            // Best match first: relevance score, then alphabetical as a stable tiebreaker.
            return list
                .OrderByDescending(c => c.Score)
                .ThenBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>
        /// Rough relevance score between a pasted name (e.g. "R31") and a real
        /// PatternSetupName (e.g. "W31_step23_R"): tokenizes both into letter-
        /// and digit-runs and rewards shared tokens, with extra weight for a
        /// shared trailing letter token (the common "_R"/"_G"/"_B" channel
        /// suffix convention) and shared numbers.
        /// </summary>
        private static int ScoreMatch(string query, string candidateName)
        {
            if (string.IsNullOrEmpty(query) || string.IsNullOrEmpty(candidateName)) return 0;

            string q = query.Trim();
            string c = candidateName.Trim();
            if (c.Equals(q, StringComparison.OrdinalIgnoreCase)) return 1000;

            var qTokens = Regex.Matches(q, @"[A-Za-z]+|\d+").Cast<Match>().Select(m => m.Value).ToList();
            var cTokens = Regex.Matches(c, @"[A-Za-z]+|\d+").Cast<Match>().Select(m => m.Value).ToList();

            int score = 0;
            foreach (var qt in qTokens)
                foreach (var ct in cTokens)
                    if (string.Equals(qt, ct, StringComparison.OrdinalIgnoreCase))
                        score += char.IsDigit(qt[0]) ? 30 : 15;

            var qLastLetterToken = qTokens.LastOrDefault(t => char.IsLetter(t[0]));
            var cLastToken = cTokens.LastOrDefault();
            if (qLastLetterToken != null && cLastToken != null &&
                string.Equals(qLastLetterToken, cLastToken, StringComparison.OrdinalIgnoreCase))
                score += 25;

            if (c.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) score += 10;
            if (q.IndexOf(c, StringComparison.OrdinalIgnoreCase) >= 0) score += 5;

            return score;
        }

        private static IEnumerable<(string Name, string AsIs, string ToBe)> EnumeratePasteLines(string pasteText)
        {
            var lines = pasteText.Replace("\r\n", "\n").Split('\n');
            foreach (var rawLine in lines)
            {
                string line = rawLine.TrimEnd();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var cols = line.Split('\t');
                if (cols.Length < 2) continue;

                string name = cols[0].Trim();
                string asIs = cols.Length >= 3 ? cols[1].Trim() : null;
                string toBe = cols.Length >= 3 ? cols[2].Trim() : cols[1].Trim();
                if (string.IsNullOrEmpty(name)) continue;

                // Skip an obvious header row (e.g. "" / "As is" / "To be")
                if (!int.TryParse(toBe, out _) && !string.IsNullOrEmpty(toBe))
                    continue;

                yield return (name, asIs, toBe);
            }
        }

        public bool HasPendingChanges => Rows.Any(r => r.IsDirty);

        public void ClearNewValues()
        {
            foreach (var row in Rows)
                row.NewPatternNumber = null;
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
                MessageBox.Show("Nothing to save — no rows have a new pattern number.",
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
                MessageBox.Show("Nothing to save — no rows have a new pattern number.",
                    "Seqxc Toolset", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (result.Warnings.Count > 0)
                MessageBox.Show("Saved with some warnings:\n\n" + string.Join("\n", result.Warnings),
                    "Saved with warnings", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                MessageBox.Show($"Saved {result.ChangeCount} pattern number change(s) to:\n{dlg.FileName}",
                    "Saved", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload straight from the file we just wrote, rather than trusting
            // in-memory state — confirms the save actually landed and also makes
            // FilePath point at the saved copy for any further edits/saves.
            _document.Load(dlg.FileName);
            RebuildRows();
        }

        /// <summary>
        /// Used by the global "Save All Changes" flow: applies this task's
        /// pending edits and writes them to targetPath, but doesn't show its
        /// own success dialog or reload/refresh — the caller does that once
        /// for every task after the whole batch completes.
        /// </summary>
        public SaveResult SaveAllInternal(string targetPath)
        {
            var dirtyRows = Rows.Where(r => r.IsDirty).ToList();
            if (dirtyRows.Count == 0) return new SaveResult();
            return ApplyAndSave(dirtyRows, targetPath);
        }

        private SaveResult ApplyAndSave(List<PatternRowVM> dirtyRows, string targetPath)
        {
            // Resolve every dirty row down to its terminal PatternSetup and detect conflicts
            // (two dirty rows resolving to the same terminal with different new values).
            var terminalTargets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var conflicts = new List<string>();
            var allAffectedSiblings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var row in dirtyRows)
            {
                string terminalName = row.ResolvedTerminalName ?? row.PatternSetupName;
                if (terminalTargets.TryGetValue(terminalName, out var existingValue) &&
                    existingValue != row.NewPatternNumber)
                {
                    conflicts.Add($"'{terminalName}' is set to both {existingValue} and {row.NewPatternNumber}");
                    continue;
                }
                terminalTargets[terminalName] = row.NewPatternNumber;

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
                    "\n\nChanging the terminal pattern number will affect those too. Continue?",
                    "Aliases affected", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm != MessageBoxResult.Yes) return new SaveResult { Cancelled = true };
            }

            var changes = new List<PatternNumberChange>();
            foreach (var kvp in terminalTargets)
            {
                var terminal = _document.PatternSetups.TryGetValue(kvp.Key, out var info) ? info : null;
                if (terminal == null) continue;
                changes.Add(new PatternNumberChange
                {
                    TerminalName = terminal.Name,
                    OldValue = terminal.PatternNumberRaw,
                    NewValue = kvp.Value
                });
                _document.SetPatternNumber(terminal.Name, kvp.Value, out _, out _);
            }

            var warnings = _document.SaveMinimalDiff(targetPath, changes);
            return new SaveResult { Warnings = warnings, ChangeCount = changes.Count };
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
