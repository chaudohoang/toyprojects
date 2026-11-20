using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;

namespace SeqxcEditor {

    // ── StepVM ────────────────────────────────────────────────────────────────
    public class StepVM : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        void N(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        StepInfo _step;
        public StepInfo Step {
            get => _step;
            set { _step = value; N(null); }
        }

        bool _checked;
        public bool IsChecked {
            get => _checked;
            set { _checked = value; N(nameof(IsChecked)); }
        }

        public string DisplayName =>
            $" {(_step.Enabled ? "●" : "○")} " +
            (!string.IsNullOrWhiteSpace(_step.PatternSetupName) ? _step.PatternSetupName : _step.Name);

        public string TypeLabel  => SeqxcDoc.TypeLabelFor(_step.AnalysisTypeName);
        public bool   IsEnabled  => _step.Enabled;
        public string Badge      => _step.Badge;

        public StepVM(StepInfo step) { _step = step; }

        public void Refresh() {
            var fresh = SeqxcDoc.GetByGuid(_step.Guid);
            if (fresh != null) _step = fresh;
            N(null);
        }
    }

    // ── MainWindow ────────────────────────────────────────────────────────────
    public partial class MainWindow : Window {

        readonly ObservableCollection<StepVM> stepItems = new ObservableCollection<StepVM>();

        string          focusedGuid;
        HashSet<string> checkedGuids  = new HashSet<string>();
        bool            suppressSel;
        List<ClipItem>  clipList      = new List<ClipItem>();
        bool            showAnalysis  = true;

        public MainWindow() {
            InitializeComponent();

            // Load icon
            try {
                string ico = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(ico)) Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri(ico));
            } catch { }

            dgSteps.ItemsSource = stepItems;

            pgAnalysis.StatusChanged     += SetStatus;
            pgPat.StatusChanged          += SetStatus;
            pgAnalysis.AddToClipList     += OnAddToClipList;
            pgPat.AddToClipList          += OnAddToClipList;

            // Start with no file loaded
            UpdateCounts();
        }

        // ══════════════════════════════════════════════════════════════════════
        // FILE
        // ══════════════════════════════════════════════════════════════════════

        void BtnOpen_Click(object s, RoutedEventArgs e)   => PickAndOpen();
        void BtnSave_Click(object s, RoutedEventArgs e)   => Save();
        void BtnSaveAs_Click(object s, RoutedEventArgs e) => SaveAs();

        void PickAndOpen() {
            var dlg = new Microsoft.Win32.OpenFileDialog {
                Title = "Open Sequence File",
                Filter = "TrueTest Sequence (*.seqxc)|*.seqxc|XML Files|*.xml|All Files|*.*",
            };
            if (dlg.ShowDialog() == true) OpenFile(dlg.FileName);
        }

        void OpenFile(string path) {
            try {
                SeqxcDoc.Load(path);
                Title = $"SeqXC Editor  \u2014  {path}";
                focusedGuid = null; checkedGuids.Clear(); ClearClip();
                RepopulateSteps();
                btnSave.IsEnabled = btnSaveAs.IsEnabled = true;
                UpdateCounts();
                SetStatus($"Loaded  \"{path}\"   \u00b7   {SeqxcDoc.Steps.Count} steps   \u00b7   {SeqxcDoc.PatternSetups.Count} pattern setups");
                if (SeqxcDoc.Steps.Count > 0) FocusStep(SeqxcDoc.Steps[0].Guid);
            } catch (Exception ex) {
                MessageBox.Show($"Failed to open:\n{ex.Message}", "Open Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void Save() {
            if (!SeqxcDoc.IsLoaded) return;
            try { SeqxcDoc.Save(SeqxcDoc.FilePath); SetStatus($"Saved  \u2192  {SeqxcDoc.FilePath}"); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        void SaveAs() {
            if (!SeqxcDoc.IsLoaded) return;
            var dlg = new Microsoft.Win32.SaveFileDialog {
                Title = "Save Sequence File",
                Filter = "TrueTest Sequence (*.seqxc)|*.seqxc|XML Files|*.xml",
                FileName = Path.GetFileName(SeqxcDoc.FilePath),
                InitialDirectory = Path.GetDirectoryName(SeqxcDoc.FilePath),
            };
            if (dlg.ShowDialog() != true) return;
            try {
                SeqxcDoc.Save(dlg.FileName);
                Title = $"SeqXC Editor  \u2014  {dlg.FileName}";
                SetStatus($"Saved as  \u2192  {dlg.FileName}");
            } catch (Exception ex) { MessageBox.Show(ex.Message, "Save Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        // ══════════════════════════════════════════════════════════════════════
        // STEP LIST
        // ══════════════════════════════════════════════════════════════════════

        void RepopulateSteps() {
            suppressSel = true;
            stepItems.Clear();
            foreach (var step in SeqxcDoc.Steps) {
                var vm = new StepVM(step) { IsChecked = checkedGuids.Contains(step.Guid) };
                vm.PropertyChanged += OnStepVmPropertyChanged;
                stepItems.Add(vm);
            }
            suppressSel = false;
            UpdateCounts();
        }

        void OnStepVmPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (suppressSel || e.PropertyName != nameof(StepVM.IsChecked)) return;
            var vm = (StepVM)sender;
            if (vm.IsChecked) checkedGuids.Add(vm.Step.Guid);
            else              checkedGuids.Remove(vm.Step.Guid);
            UpdateCounts();
            pgAnalysis.UpdateChecked(checkedGuids);
            pgPat.UpdateChecked(checkedGuids);
        }

        void DgSteps_SelectionChanged(object s, SelectionChangedEventArgs e) {
            if (suppressSel) return;
            if (dgSteps.SelectedItem is StepVM vm) {
                vm.Refresh();
                FocusStep(vm.Step.Guid);
            }
        }

        void DgSteps_RightClick(object s, MouseButtonEventArgs e) {
            // Find clicked row
            var row = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row?.DataContext is StepVM vm) {
                vm.Refresh();
                SeqxcDoc.ToggleEnabled(vm.Step.Guid);
                vm.Refresh();
                // Update info bar without changing selection
                suppressSel = true;
                UpdateStepInfoBar(vm.Step);
                suppressSel = false;
                e.Handled = true;
            }
        }

        void FocusStep(string guid) {
            focusedGuid = guid;
            var step = SeqxcDoc.GetByGuid(guid); if (step == null) return;
            int idx    = SeqxcDoc.Steps.IndexOf(step);
            int shared = SeqxcDoc.SharedCount(step.PatternSetupName);

            UpdateStepInfoBar(step);

            int anCnt = step.An?.ChildNodes.OfType<XmlElement>().Count() ?? 0;
            int psCnt = step.Psn?.ChildNodes.OfType<XmlElement>().Count() ?? 0;
            btnTabAnalysis.Content = $"Analysis  ({anCnt})";
            btnTabPatSetup.Content = $"PatternSetup  ({psCnt})";

            pgAnalysis.SetNode(step.An,  "analysis",     checkedGuids);
            pgPat.SetNode(step.Psn,      "patternSetup", checkedGuids);
            pgAnalysis.SetClipCount(clipList.Count);
            pgPat.SetClipCount(clipList.Count);

            // Select in grid without re-triggering SelectionChanged
            suppressSel = true;
            var vm = stepItems.FirstOrDefault(v => v.Step.Guid == guid);
            if (vm != null) {
                dgSteps.SelectedItem = vm;
                dgSteps.ScrollIntoView(vm);
            }
            suppressSel = false;
        }

        void UpdateStepInfoBar(StepInfo step) {
            int idx    = SeqxcDoc.Steps.IndexOf(step);
            int shared = SeqxcDoc.SharedCount(step.PatternSetupName);

            lblStepName.Text   = step.Name;
            lblEnabled.Text    = step.Enabled ? "\u25cf ENABLED" : "\u25cb DISABLED";
            lblEnabled.Foreground = step.Enabled
                ? System.Windows.Media.Brushes.ForestGreen
                : System.Windows.Media.Brushes.Gray;
            lblStepIdx.Text    = $"step {idx + 1} of {SeqxcDoc.Steps.Count}";

            if (shared > 1) {
                lblShared.Text       = $"\u26a0  PatternSetup shared by {shared} steps \u2014 edits affect all";
                lblShared.Visibility = Visibility.Visible;
            } else {
                lblShared.Visibility = Visibility.Collapsed;
            }
        }

        // ══════════════════════════════════════════════════════════════════════
        // OPERATIONS
        // ══════════════════════════════════════════════════════════════════════

        void BtnAll_Click(object s, RoutedEventArgs e)  => CheckAll(true);
        void BtnNone_Click(object s, RoutedEventArgs e) => CheckAll(false);

        void CheckAll(bool check) {
            suppressSel = true;
            checkedGuids.Clear();
            foreach (var vm in stepItems) {
                vm.IsChecked = check;
                if (check) checkedGuids.Add(vm.Step.Guid);
            }
            suppressSel = false;
            UpdateCounts(); pgAnalysis.UpdateChecked(checkedGuids); pgPat.UpdateChecked(checkedGuids);
        }

        void BtnMvUp_Click(object s, RoutedEventArgs e)   => MoveStep(-1);
        void BtnMvDown_Click(object s, RoutedEventArgs e) => MoveStep(1);

        void MoveStep(int dir) {
            if (string.IsNullOrEmpty(focusedGuid)) return;
            string g   = focusedGuid;
            int oldIdx = SeqxcDoc.Steps.FindIndex(s2 => s2.Guid == g);
            SeqxcDoc.MoveStep(g, dir);
            int newIdx = SeqxcDoc.Steps.FindIndex(s2 => s2.Guid == g);
            if (oldIdx < 0 || newIdx < 0 || oldIdx == newIdx) return;

            // Swap the two ObservableCollection items — WPF animates this smoothly, no flash
            suppressSel = true;
            stepItems.Move(oldIdx, newIdx);
            // Refresh both VMs to get fresh StepInfo references
            stepItems[oldIdx].Refresh();
            stepItems[newIdx].Refresh();
            dgSteps.SelectedItem = stepItems[newIdx];
            dgSteps.ScrollIntoView(stepItems[newIdx]);
            suppressSel = false;
            focusedGuid = g;
        }

        void BtnClone_Click(object s, RoutedEventArgs e) {
            if (checkedGuids.Count == 0) return;
            string old = focusedGuid;
            var cloned = SeqxcDoc.CloneSteps(checkedGuids.ToList());
            checkedGuids.Clear(); RepopulateSteps();
            FocusStep(old ?? SeqxcDoc.Steps.FirstOrDefault()?.Guid);
            SetStatus($"Cloned:  {string.Join(", ", cloned)}");
        }

        void BtnDelete_Click(object s, RoutedEventArgs e) {
            if (checkedGuids.Count == 0) return;
            if (MessageBox.Show($"Delete {checkedGuids.Count} step(s)?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            SeqxcDoc.DeleteSteps(checkedGuids.ToList()); checkedGuids.Clear(); RepopulateSteps();
            var f = SeqxcDoc.Steps.FirstOrDefault(); if (f != null) FocusStep(f.Guid);
            SetStatus("Deleted steps");
        }

        void BtnTabAnalysis_Click(object s, RoutedEventArgs e)  => SwitchTab(true);
        void BtnTabPatSetup_Click(object s, RoutedEventArgs e)  => SwitchTab(false);

        void SwitchTab(bool analysis) {
            showAnalysis = analysis;
            pgAnalysis.Visibility = analysis ? Visibility.Visible : Visibility.Collapsed;
            pgPat.Visibility      = analysis ? Visibility.Collapsed : Visibility.Visible;
            btnTabAnalysis.FontWeight = analysis  ? FontWeights.Bold   : FontWeights.Normal;
            btnTabPatSetup.FontWeight = !analysis ? FontWeights.Bold   : FontWeights.Normal;
            btnTabAnalysis.Foreground = analysis  ? System.Windows.Media.Brushes.DarkBlue : System.Windows.Media.Brushes.DimGray;
            btnTabPatSetup.Foreground = !analysis ? System.Windows.Media.Brushes.DarkBlue : System.Windows.Media.Brushes.DimGray;
            btnTabAnalysis.Background = analysis  ? System.Windows.Media.Brushes.White : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245,245,245));
            btnTabPatSetup.Background = !analysis ? System.Windows.Media.Brushes.White : new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(245,245,245));
        }

        // ══════════════════════════════════════════════════════════════════════
        // COPY LIST
        // ══════════════════════════════════════════════════════════════════════

        void OnAddToClipList(PropRow row, string source) {
            var step     = SeqxcDoc.GetByGuid(focusedGuid);
            var existing = clipList.FirstOrDefault(c => c.Source == source && c.Row.FullPath == row.FullPath);
            if (existing != null) { existing.Row = row; existing.AnalysisType = step?.AnalysisTypeName; }
            else clipList.Add(new ClipItem { Row = row, Source = source, AnalysisType = step?.AnalysisTypeName, StepName = step?.Name });
            RefreshClipDisplay();
            SetStatus($"{(existing != null ? "Updated" : "Added")} [{source}] {row.Tag}  \u2014  {clipList.Count} item{(clipList.Count == 1 ? "" : "s")} in copy list");
        }

        void BtnPaste_Click(object s, RoutedEventArgs e) {
            if (clipList.Count == 0 || checkedGuids.Count == 0) return;
            int pasted = 0;
            foreach (var guid in checkedGuids) {
                var step = SeqxcDoc.GetByGuid(guid); if (step == null) continue;
                SeqxcDoc.PasteClipItems(clipList, step); pasted++;
            }
            if (!string.IsNullOrEmpty(focusedGuid)) FocusStep(focusedGuid);
            SetStatus($"Pasted {clipList.Count} propert{(clipList.Count == 1 ? "y" : "ies")} to {pasted} steps");
        }

        void BtnClearClip_Click(object s, RoutedEventArgs e) => ClearClip();

        void ClearClip() {
            clipList.Clear();
            lblClipInfo.Text            = "";
            btnPaste.IsEnabled          = false;
            btnClearClip.IsEnabled      = false;
            btnPaste.Content            = "Paste (0) \u2192 0 steps";
            btnCopyList.Content         = "Copy list (0) \u25bc";
            pgAnalysis.SetClipCount(0); pgPat.SetClipCount(0);
        }

        void RefreshClipDisplay() {
            int n = clipList.Count;
            btnCopyList.Content    = $"Copy list ({n}) \u25bc";
            btnCopyList.Foreground = n > 0
                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0,100,200))
                : System.Windows.Media.Brushes.DimGray;
            lblClipInfo.Text       = n > 0 ? $"  {n} prop{(n == 1 ? "" : "s")} in list" : "";
            btnPaste.Content       = $"Paste ({n}) \u2192 {checkedGuids.Count} steps";
            btnPaste.IsEnabled     = n > 0 && checkedGuids.Count > 0;
            btnClearClip.IsEnabled = n > 0;
            pgAnalysis.SetClipCount(n); pgPat.SetClipCount(n);
        }

        void BtnCopyList_Click(object s, RoutedEventArgs e) {
            BuildCopyListMenu();
            btnCopyList.ContextMenu.IsOpen = true;
        }

        void BuildCopyListMenu() {
            var menu = new ContextMenu();
            var anItems = clipList.Where(c => c.Source == "analysis").ToList();
            var psItems = clipList.Where(c => c.Source == "patternSetup").ToList();

            if (anItems.Count > 0) {
                menu.Items.Add(new MenuItem { Header = $"  Analysis  ({anItems.Count})", IsEnabled = false,
                    FontWeight = FontWeights.Bold, Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0,80,160)) });
                foreach (var ci in anItems) {
                    var c = ci;
                    var mi = new MenuItem { Header = $"    {c.Row.Tag}  \u00d7" };
                    mi.Click += (s2, e2) => { clipList.Remove(c); RefreshClipDisplay(); UpdateCounts(); };
                    menu.Items.Add(mi);
                }
            }
            if (anItems.Count > 0 && psItems.Count > 0) menu.Items.Add(new Separator());
            if (psItems.Count > 0) {
                menu.Items.Add(new MenuItem { Header = $"  PatternSetup  ({psItems.Count})", IsEnabled = false,
                    FontWeight = FontWeights.Bold, Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(0,120,50)) });
                foreach (var ci in psItems) {
                    var c = ci;
                    var mi = new MenuItem { Header = $"    {c.Row.Tag}  \u00d7" };
                    mi.Click += (s2, e2) => { clipList.Remove(c); RefreshClipDisplay(); UpdateCounts(); };
                    menu.Items.Add(mi);
                }
            }
            if (clipList.Count == 0) menu.Items.Add(new MenuItem { Header = "  (empty)", IsEnabled = false });
            menu.Items.Add(new Separator());
            var clr = new MenuItem { Header = "\u00d7  Clear all" };
            clr.Click += (s2, e2) => ClearClip();
            menu.Items.Add(clr);
            btnCopyList.ContextMenu = menu;
        }

        // ══════════════════════════════════════════════════════════════════════
        // KEYBOARD, DRAG & DROP
        // ══════════════════════════════════════════════════════════════════════

        void Window_KeyDown(object s, KeyEventArgs e) {
            if (e.Key == Key.O && Keyboard.Modifiers == ModifierKeys.Control) { PickAndOpen(); e.Handled = true; }
            if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control) { Save();        e.Handled = true; }
        }

        void Window_DragEnter(object s, DragEventArgs e) {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        void Window_Drop(object s, DragEventArgs e) {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files?.Length > 0) OpenFile(files[0]);
        }

        // ══════════════════════════════════════════════════════════════════════
        // HELPERS
        // ══════════════════════════════════════════════════════════════════════

        void UpdateCounts() {
            int n = checkedGuids.Count;
            lblCheckedCount.Text   = $"  {n} checked";
            btnClone.Content       = $"Clone ({n})";
            btnDelete.Content      = $"Delete ({n})";
            btnClone.IsEnabled     = n > 0;
            btnDelete.IsEnabled    = n > 0;
            btnPaste.Content       = $"Paste ({clipList.Count}) \u2192 {n} steps";
            btnPaste.IsEnabled     = clipList.Count > 0 && n > 0;
        }

        void SetStatus(string msg) => lblStatus.Text = msg;

        static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject {
            while (child != null) {
                if (child is T t) return t;
                child = System.Windows.Media.VisualTreeHelper.GetParent(child);
            }
            return null;
        }
    }
}
