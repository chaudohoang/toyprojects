using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace SeqxcEditor {

    // ── PropRowVM ─────────────────────────────────────────────────────────────
    public class PropRowVM : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        void N(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));

        readonly PropRow _row;
        public PropRow Row => _row;

        bool _expanded;
        public bool IsExpanded {
            get => _expanded;
            set { _expanded = value; N(null); }
        }

        public bool IsComplex    => _row.IsComplex;
        public bool IsEditable   => !_row.IsComplex;
        public int  Indent       => _row.Indent;
        public string FullPath   => _row.FullPath;

        public Thickness IndentMargin    => new Thickness(_row.Indent * 12, 0, 0, 0);
        public Visibility ExpandBtnVisibility => _row.IsComplex ? Visibility.Visible : Visibility.Collapsed;
        public string ExpandIcon         => _expanded ? "▼" : "▶";

        public string PropText           => _row.DisplayName + (_row.IsComplex ? " [array]" : "");

        public string Value {
            get => _row.IsComplex
                ? (_expanded ? "— expanded —" : _row.Value)
                : _row.Value;
            set {
                if (_row.IsComplex || _row.El == null) return;
                _row.El.InnerText = value;
                _row.Value        = value;
                N(nameof(Value));
                Edited?.Invoke($"Edited {_row.Tag} = \"{value}\"");
            }
        }

        public Brush PropForeground  => _row.IsComplex
            ? new SolidColorBrush(Color.FromRgb(0, 70, 140))
            : SystemColors.WindowTextBrush;

        public Brush ValueForeground => _row.IsComplex ? Brushes.Gray : SystemColors.WindowTextBrush;

        public Action<string> Edited;

        public PropRowVM(PropRow row) { _row = row; }
    }

    // ── PropGrid UserControl ──────────────────────────────────────────────────
    public partial class PropGrid : UserControl {

        XmlNode         currentNode;
        string          currentTab;
        HashSet<string> checkedGuids = new HashSet<string>();
        HashSet<string> expanded     = new HashSet<string>();
        int             clipCount;

        readonly ObservableCollection<PropRowVM> items = new ObservableCollection<PropRowVM>();

        public event Action<string>          StatusChanged;
        public event Action<PropRow, string> AddToClipList;

        public PropGrid() {
            InitializeComponent();
            dgProps.ItemsSource = items;
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void SetNode(XmlNode node, string tab, HashSet<string> guids) {
            currentNode  = node;
            currentTab   = tab;
            checkedGuids = guids;
            ResetFilter();
            expanded.Clear();
            Reload();
        }

        public void UpdateChecked(HashSet<string> guids) => checkedGuids = guids;
        public void SetClipCount(int n)   => clipCount = n;

        // ── Filter ────────────────────────────────────────────────────────────

        void ResetFilter() {
            txtFilter.Text = "";
            filterHint.Visibility = Visibility.Visible;
        }

        void TxtFilter_GotFocus(object s, RoutedEventArgs e)  => filterHint.Visibility = Visibility.Collapsed;
        void TxtFilter_LostFocus(object s, RoutedEventArgs e) =>
            filterHint.Visibility = string.IsNullOrEmpty(txtFilter.Text) ? Visibility.Visible : Visibility.Collapsed;

        void TxtFilter_Changed(object s, TextChangedEventArgs e) => Reload();

        string FilterText => txtFilter.Text.Trim();

        // ── Rebuild ───────────────────────────────────────────────────────────

        void Reload() {
            if (currentNode == null) { items.Clear(); return; }

            // Remember scroll position
            var sv = GetScrollViewer(dgProps);
            double scrollOffset = sv?.VerticalOffset ?? 0;

            var newRows = Flatten(PropRow.FromNode(currentNode, FilterText));

            // Efficient update: add/remove only changed rows
            for (int i = 0; i < newRows.Count; i++) {
                var vm = new PropRowVM(newRows[i]) { IsExpanded = expanded.Contains(newRows[i].FullPath) };
                vm.Edited = StatusChanged;
                if (i < items.Count) items[i] = vm;
                else items.Add(vm);
            }
            while (items.Count > newRows.Count)
                items.RemoveAt(items.Count - 1);

            // Restore scroll
            sv?.ScrollToVerticalOffset(scrollOffset);
        }

        List<PropRow> Flatten(List<PropRow> top) {
            var r = new List<PropRow>();
            foreach (var row in top) {
                r.Add(row);
                if (row.IsComplex && expanded.Contains(row.FullPath))
                    InsertChildren(r, row);
            }
            return r;
        }

        void InsertChildren(List<PropRow> result, PropRow parent) {
            foreach (var c in PropRow.GetChildren(parent)) {
                result.Add(c);
                if (c.IsComplex && expanded.Contains(c.FullPath))
                    InsertChildren(result, c);
            }
        }

        // ── Event handlers ────────────────────────────────────────────────────

        void ExpandBtn_Click(object sender, RoutedEventArgs e) {
            if (((FrameworkElement)sender).Tag is PropRowVM vm && vm.IsComplex) {
                if (expanded.Contains(vm.FullPath)) expanded.Remove(vm.FullPath);
                else                                expanded.Add(vm.FullPath);
                vm.IsExpanded = expanded.Contains(vm.FullPath);
                Reload();
                e.Handled = true;
            }
        }

        void DgProps_RightClick(object sender, MouseButtonEventArgs e) {
            var vm = HitTestRow(e);
            if (vm == null) return;
            AddToClipList?.Invoke(vm.Row, currentTab);
            e.Handled = true;
        }

        void DgProps_LeftClick(object sender, MouseButtonEventArgs e) {
            // Clicking property column of complex row = expand/collapse
            var cell = FindVisualParent<DataGridCell>(e.OriginalSource as DependencyObject);
            if (cell == null) return;
            if (cell.Column?.DisplayIndex != 0) return;
            var vm = cell.DataContext as PropRowVM;
            if (vm?.IsComplex == true) {
                if (expanded.Contains(vm.FullPath)) expanded.Remove(vm.FullPath);
                else                                expanded.Add(vm.FullPath);
                vm.IsExpanded = expanded.Contains(vm.FullPath);
                Reload();
            }
        }

        void DgProps_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            // no-op; handled per-cell click
        }

        void ValueBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Escape) {
                var tb = (TextBox)sender;
                // Re-read from model to discard edit
                if (tb.DataContext is PropRowVM vm) tb.Text = vm.Row.Value;
                Keyboard.ClearFocus();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        PropRowVM HitTestRow(MouseButtonEventArgs e) {
            var row = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            return row?.DataContext as PropRowVM;
        }

        static T FindVisualParent<T>(DependencyObject child) where T : DependencyObject {
            while (child != null) {
                if (child is T t) return t;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        static ScrollViewer GetScrollViewer(DependencyObject obj) {
            if (obj == null) return null;
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++) {
                var c = VisualTreeHelper.GetChild(obj, i);
                if (c is ScrollViewer sv) return sv;
                var found = GetScrollViewer(c);
                if (found != null) return found;
            }
            return null;
        }
    }
}
