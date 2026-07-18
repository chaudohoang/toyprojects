using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using SeqxcToolset.Core;
using SeqxcToolset.Tasks;
using SeqxcToolset.Tasks.PatternNumberTask;
using SeqxcToolset.Tasks.ExposureTimeTask;
using SeqxcToolset.Tasks.LuminanceScaleTask;

namespace SeqxcToolset
{
    public partial class MainWindow : Window
    {
        private readonly List<ITaskModule> _tasks = new List<ITaskModule>();
        private readonly SequenceDocument _document = new SequenceDocument();

        public MainWindow()
        {
            InitializeComponent();
            RegisterTasks();
            TaskList.ItemsSource = _tasks;
            if (_tasks.Count > 0)
                TaskList.SelectedIndex = 0;
        }

        private void RegisterTasks()
        {
            // Add new task modules here as they're built.
            _tasks.Add(new PatternNumberTaskModule());
            _tasks.Add(new ExposureTimeTaskModule());
            _tasks.Add(new LuminanceScaleTaskModule());
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "TrueTest Sequence (*.seqxc)|*.seqxc|All files (*.*)|*.*",
                Title = "Open sequence file"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                _document.Load(dlg.FileName);
                FilePathText.Text = dlg.FileName;
                StatusText.Text = $"Loaded {_document.Items.Count} sequence items, " +
                                   $"{_document.PatternSetups.Count} pattern setups.";

                foreach (var task in _tasks)
                    task.OnDocumentLoaded(_document);

                if (TaskList.SelectedItem is ITaskModule current)
                    TaskHost.Content = current.View;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Failed to load file:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TaskList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TaskList.SelectedItem is ITaskModule task)
                TaskHost.Content = task.View;
        }

        private void ClearAllChanges_Click(object sender, RoutedEventArgs e)
        {
            int clearedCount = _tasks.Count(t => t.HasPendingChanges);
            foreach (var task in _tasks)
                task.ClearPendingChanges();

            StatusText.Text = clearedCount > 0
                ? $"Cleared pending changes in {clearedCount} task(s)."
                : "No pending changes to clear.";
        }

        private void SaveAllChanges_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_document.FilePath))
            {
                MessageBox.Show(this, "No file loaded.", "Seqxc Toolset");
                return;
            }

            var pendingTasks = _tasks.Where(t => t.HasPendingChanges).ToList();
            if (pendingTasks.Count == 0)
            {
                MessageBox.Show(this, "Nothing to save — no task has pending changes.",
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

            int totalChanges = 0;
            var allWarnings = new List<string>();
            var skipped = new List<string>();
            var saved = new List<string>();

            // Each task's save reads targetPath fresh and only touches its own
            // tags, so running them one after another — reloading _document
            // between each — layers all three onto the same file correctly.
            foreach (var task in pendingTasks)
            {
                var result = task.SaveAllInternal(dlg.FileName);

                if (result.Cancelled)
                {
                    skipped.Add(task.TaskName);
                    continue;
                }
                if (result.ChangeCount == 0) continue;

                totalChanges += result.ChangeCount;
                saved.Add(task.TaskName);
                allWarnings.AddRange(result.Warnings.Select(w => $"[{task.TaskName}] {w}"));

                _document.Load(dlg.FileName);
            }

            // Refresh every task's grid once, now that the whole batch is
            // committed — safe here since nothing is left pending to lose.
            foreach (var task in _tasks)
                task.RefreshFromDocument();

            FilePathText.Text = dlg.FileName;

            if (totalChanges == 0)
            {
                MessageBox.Show(this,
                    skipped.Count > 0
                        ? "Nothing was saved — cancelled for: " + string.Join(", ", skipped)
                        : "Nothing was saved.",
                    "Seqxc Toolset", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var summary = new StringBuilder();
            summary.Append($"Saved {totalChanges} total change(s) across {saved.Count} task(s) " +
                            $"({string.Join(", ", saved)}) to:\n{dlg.FileName}");
            if (skipped.Count > 0)
                summary.Append("\n\nSkipped (cancelled): " + string.Join(", ", skipped));
            if (allWarnings.Count > 0)
                summary.Append("\n\nWarnings:\n" + string.Join("\n", allWarnings));

            MessageBox.Show(this, summary.ToString(),
                allWarnings.Count > 0 ? "Saved with warnings" : "Saved",
                MessageBoxButton.OK, allWarnings.Count > 0 ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
    }
}
