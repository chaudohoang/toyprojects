using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SeqxcToolset.Tasks.PatternStringTask
{
    public class PatternStringRowVM : INotifyPropertyChanged
    {
        public int Index { get; set; }

        /// <summary>1-based for display only — Index itself stays 0-based
        /// internally since -1 is used as a sentinel for "not in Items list".</summary>
        public string DisplayIndex => Index >= 0 ? (Index + 1).ToString() : "-";

        public bool Selected { get; set; }
        public string PatternSetupName { get; set; }
        public string AnalysisType { get; set; }
        public string UserName { get; set; }
        public string ResolvedTerminalName { get; set; }
        public bool IsAlias { get; set; }

        /// <summary>
        /// What the grid shows for the current value: the string itself (blank for the
        /// empty-but-present case, which is every entry in a freshly upgraded file),
        /// or a parenthesised note when there is nothing to edit.
        /// </summary>
        public string CurrentPatternString { get; set; }

        /// <summary>
        /// The actual on-disk value, kept separate from the display text above so a
        /// literal "(absent)" typed by a person can never be mistaken for the real
        /// thing. Null means the terminal is unresolved, or the file predates the
        /// PatternString property — in both cases there is no element to patch.
        /// </summary>
        public string CurrentRawValue { get; set; }

        /// <summary>False when there is no &lt;PatternString&gt; element to write to.</summary>
        public bool IsEditable => CurrentRawValue != null;

        private string _newPatternString;
        public string NewPatternString
        {
            get => _newPatternString;
            set
            {
                if (_newPatternString == value) return;
                _newPatternString = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        // Blank means "no change", matching every other task in this tool. The
        // consequence, since PatternString starts out empty everywhere, is that a
        // value can be set but not cleared back to empty from here — clearing is
        // deliberately out of scope rather than given a second, special meaning to
        // an empty cell.
        public bool IsDirty =>
            !string.IsNullOrEmpty(NewPatternString) &&
            NewPatternString != (CurrentRawValue ?? "");

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void RefreshDirty() => OnPropertyChanged(nameof(IsDirty));
    }
}
