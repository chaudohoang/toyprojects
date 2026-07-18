using System.Windows.Controls;
using SeqxcToolset.Core;

namespace SeqxcToolset.Tasks.PatternNumberTask
{
    public partial class PatternNumberView : UserControl
    {
        private readonly PatternNumberViewModel _vm = new PatternNumberViewModel();

        public PatternNumberView()
        {
            InitializeComponent();
            DataContext = _vm;
            ItemsGrid.ItemsSource = _vm.Rows;
            DataGridPasteHelper.Attach(ItemsGrid);
        }

        public void LoadDocument(SequenceDocument document)
        {
            _vm.LoadDocument(document);
        }

        public bool HasPendingChanges => _vm.HasPendingChanges;
        public SaveResult SaveAllInternal(string targetPath) => _vm.SaveAllInternal(targetPath);
        public void ClearPendingChanges() => _vm.ClearNewValues();
        public void RefreshFromDocument() => _vm.RebuildRows();

        private void ShowAllCheck_Changed(object sender, System.Windows.RoutedEventArgs e)
        {
            _vm.ShowAllItems = ShowAllCheck.IsChecked == true;
        }
    }
}
