using System.Windows.Controls;
using SeqxcToolset.Core;

namespace SeqxcToolset.Tasks.ExposureTimeTask
{
    public class ExposureTimeTaskModule : ITaskModule
    {
        private readonly ExposureTimeView _view = new ExposureTimeView();

        public string TaskName => "Exposure Time";
        public UserControl View => _view;

        public void OnDocumentLoaded(SequenceDocument document)
        {
            _view.LoadDocument(document);
        }

        public bool HasPendingChanges => _view.HasPendingChanges;
        public SaveResult SaveAllInternal(string targetPath) => _view.SaveAllInternal(targetPath);
        public void ClearPendingChanges() => _view.ClearPendingChanges();
        public void RefreshFromDocument() => _view.RefreshFromDocument();
    }
}
