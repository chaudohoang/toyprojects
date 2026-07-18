using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public ObservableCollection<ExposureRowVM> Rows { get; } = new ObservableCollection<ExposureRowVM>();

        private bool _showAllItems;
        public bool ShowAllItems
        {
            get => _showAllItems;
            set { _showAllItems = value; OnPropertyChanged(); RebuildRows(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand ClearNewValuesCommand { get; }

        public ExposureTimeViewModel()
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
