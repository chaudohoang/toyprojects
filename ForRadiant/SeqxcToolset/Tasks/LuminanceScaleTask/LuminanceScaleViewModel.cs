using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public ObservableCollection<LuminanceRowVM> Rows { get; } = new ObservableCollection<LuminanceRowVM>();

        private bool _showAllItems;
        public bool ShowAllItems
        {
            get => _showAllItems;
            set { _showAllItems = value; OnPropertyChanged(); RebuildRows(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand ClearNewValuesCommand { get; }

        public LuminanceScaleViewModel()
        {
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
