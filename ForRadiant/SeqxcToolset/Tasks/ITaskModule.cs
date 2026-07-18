using System.Windows.Controls;
using SeqxcToolset.Core;

namespace SeqxcToolset.Tasks
{
    public interface ITaskModule
    {
        string TaskName { get; }
        UserControl View { get; }
        void OnDocumentLoaded(SequenceDocument document);

        /// <summary>True if this task has at least one unsaved "New" edit.</summary>
        bool HasPendingChanges { get; }

        /// <summary>
        /// Applies and writes this task's pending changes to targetPath
        /// (assumed already up to date with whatever any prior task in the
        /// same "Save All" pass just wrote). Does not show its own success
        /// dialog or reload/refresh — the caller orchestrates that once,
        /// after every task in the batch has run.
        /// </summary>
        SaveResult SaveAllInternal(string targetPath);

        /// <summary>Clears this task's pending "New" edits without saving.</summary>
        void ClearPendingChanges();

        /// <summary>Rebuilds this task's grid from the current document state.</summary>
        void RefreshFromDocument();
    }
}
