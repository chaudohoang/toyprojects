using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace RemoteToolsTabbed
{
    /// <summary>
    /// One loaded CSV: where it is, what to call its group header, and whether that group is
    /// open. Public because the grid groups on it and the header binds to it - a binding
    /// against an internal type silently produces nothing.
    /// </summary>
    public sealed class MachineList : INotifyPropertyChanged
    {
        private bool _isExpanded = true;

        public MachineList(string path, bool isDefault)
        {
            Path = path ?? string.Empty;
            IsDefault = isDefault;
            Title = System.IO.Path.GetFileNameWithoutExtension(Path);
            if (Title.Length == 0)
                Title = Path;
        }

        /// <summary>Full path to the CSV.</summary>
        public string Path { get; private set; }

        /// <summary>File name without the extension - what the group header shows.</summary>
        public string Title { get; private set; }

        /// <summary>All.csv next to the exe. Always present, never removable.</summary>
        public bool IsDefault { get; private set; }

        /// <summary>
        /// Whether this list's group is open in the grid. The state lives here rather than on
        /// the group header so it survives a re-filter, a reordering and Expand / Collapse all -
        /// a <see cref="System.Windows.Data.CollectionViewGroup"/> is rebuilt on every refresh,
        /// so anything remembered on the header itself would be lost on the next keystroke.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded == value)
                    return;

                _isExpanded = value;
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                    handler(this, new PropertyChangedEventArgs("IsExpanded"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return Title;
        }
    }

    /// <summary>
    /// Which CSVs the sidebar has open, remembered in lists.txt next to the exe - one full
    /// path per line, comments with ';'. All.csv is implicit: it is always first and is never
    /// written to the file, so the default list cannot be lost by editing or deleting lists.txt.
    ///
    /// A path that no longer exists is dropped at start-up and the file is rewritten without
    /// it, so a list deleted from disk stops coming back.
    /// </summary>
    internal static class MachineLists
    {
        public static string SettingsPath
        {
            get { return System.IO.Path.Combine(Paths.AppFolder, "lists.txt"); }
        }

        /// <summary>
        /// The default list plus every still-existing extra. <paramref name="pruned"/> receives
        /// the titles of the entries that were dropped because their file is gone; when there
        /// are any, lists.txt is rewritten so they do not return.
        /// </summary>
        public static List<MachineList> Load(out List<string> pruned)
        {
            List<MachineList> lists = new List<MachineList>();
            pruned = new List<string>();
            lists.Add(new MachineList(Paths.Csv, true));

            string[] lines;
            try
            {
                if (!File.Exists(SettingsPath))
                    return lists;

                lines = File.ReadAllLines(SettingsPath);
            }
            catch (Exception)
            {
                return lists;   // unreadable settings must not cost the user the default list
            }

            bool changed = false;

            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line[0] == ';' || line[0] == '#')
                    continue;

                string path = FullPath(line);
                if (path == null || Contains(lists, path))
                {
                    changed = true;         // unusable or a duplicate of one already loaded
                    continue;
                }

                if (!File.Exists(path))
                {
                    pruned.Add(new MachineList(path, false).Title);
                    changed = true;
                    continue;
                }

                lists.Add(new MachineList(path, false));
            }

            if (changed)
                Save(lists);

            return lists;
        }

        /// <summary>Rewrites lists.txt from the extras in <paramref name="lists"/>.</summary>
        public static void Save(IEnumerable<MachineList> lists)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("; Machine lists loaded into the sidebar - one CSV path per line.");
            sb.AppendLine("; All.csv next to the exe is always loaded and is not listed here.");

            foreach (MachineList list in lists)
            {
                if (list == null || list.IsDefault)
                    continue;

                sb.AppendLine(list.Path);
            }

            try
            {
                File.WriteAllText(SettingsPath, sb.ToString());
            }
            catch (Exception)
            {
                // Read-only install folder: the lists still work for this run, they just
                // will not come back next time. Not worth a dialog on every change.
            }
        }

        /// <summary>Absolute path, or null if the text is not a usable path at all.</summary>
        public static string FullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(Paths.AppFolder, path.Trim()));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool Contains(IEnumerable<MachineList> lists, string fullPath)
        {
            foreach (MachineList list in lists)
            {
                if (list != null && string.Equals(list.Path, fullPath, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
