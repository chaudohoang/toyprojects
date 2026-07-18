using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SeqxcToolset.Tasks.PatternNumberTask
{
    public class PatternRowVM : INotifyPropertyChanged
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

        public string CurrentPatternNumber { get; set; }

        private string _newPatternNumber;
        public string NewPatternNumber
        {
            get => _newPatternNumber;
            set
            {
                if (_newPatternNumber == value) return;
                _newPatternNumber = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        public bool IsDirty =>
            !string.IsNullOrEmpty(NewPatternNumber) &&
            NewPatternNumber != CurrentPatternNumber;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void RefreshDirty() => OnPropertyChanged(nameof(IsDirty));
    }
}
