using System.Windows.Controls;
using SeqxcToolset.Core;

namespace SeqxcToolset.Tasks.LuminanceScaleTask
{
    public partial class LuminanceScaleView : UserControl
    {
        private readonly LuminanceScaleViewModel _vm = new LuminanceScaleViewModel();

        public LuminanceScaleView()
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
