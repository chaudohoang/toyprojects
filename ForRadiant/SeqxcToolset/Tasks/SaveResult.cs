using System.Collections.Generic;

namespace SeqxcToolset.Tasks
{
    /// <summary>
    /// Result of one task applying and writing its pending changes.
    /// Returned by ITaskModule.SaveAllInternal so the global "Save All
    /// Changes" flow can combine results from every task into one summary
    /// instead of each task showing its own dialog.
    /// </summary>
    public class SaveResult
    {
        public List<string> Warnings { get; set; } = new List<string>();
        public int ChangeCount { get; set; }

        /// <summary>True if the task had pending changes but the person
        /// declined a required confirmation (e.g. an alias-sharing warning),
        /// so nothing was written for this task.</summary>
        public bool Cancelled { get; set; }
    }
}
