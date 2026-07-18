using System.Windows.Controls;
using SeqxcToolset.Core;

namespace SeqxcToolset.Tasks.PatternNumberTask
{
    public class PatternNumberTaskModule : ITaskModule
    {
        private readonly PatternNumberView _view = new PatternNumberView();

        public string TaskName => "Pattern Numbers";
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
