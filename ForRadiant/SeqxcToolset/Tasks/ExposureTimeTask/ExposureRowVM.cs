using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SeqxcToolset.Tasks.ExposureTimeTask
{
    public class ExposureRowVM : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public string DisplayIndex => (Index + 1).ToString();

        public string PatternSetupName { get; set; }
        public string AnalysisType { get; set; }

        public bool YCapture { get; set; }
        public string YExposure { get; set; }
        public bool XCapture { get; set; }
        public string XExposure { get; set; }
        public bool ZCapture { get; set; }
        public string ZExposure { get; set; }

        // Nullable = tri-state: null/indeterminate means "no override",
        // checked/unchecked is an explicit new value. Same "blank = no
        // change" rule as the text fields, just via the checkbox's third state.
        private bool? _newYCapture, _newXCapture, _newZCapture;
        private string _newYExposure, _newXExposure, _newZExposure;

        public bool? NewYCapture
        {
            get => _newYCapture;
            set { _newYCapture = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }
        public string NewYExposure
        {
            get => _newYExposure;
            set { _newYExposure = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }
        public bool? NewXCapture
        {
            get => _newXCapture;
            set { _newXCapture = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }
        public string NewXExposure
        {
            get => _newXExposure;
            set { _newXExposure = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }
        public bool? NewZCapture
        {
            get => _newZCapture;
            set { _newZCapture = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }
        public string NewZExposure
        {
            get => _newZExposure;
            set { _newZExposure = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }

        // Convenience column: typing here fans the value out to all three
        // channels at once (they're almost always the same). Clearing it
        // does NOT clear the individual fields — it's a one-way broadcast,
        // not a bound mirror, so per-channel overrides afterward still stick.
        private string _newExpAll;
        public string NewExpAll
        {
            get => _newExpAll;
            set
            {
                _newExpAll = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    NewYExposure = value;
                    NewXExposure = value;
                    NewZExposure = value;
                }
            }
        }

        private static bool ChangedBool(bool? newVal, bool current) => newVal.HasValue && newVal.Value != current;
        private static bool ChangedText(string newVal, string current) =>
            !string.IsNullOrEmpty(newVal) && newVal.Trim() != current;

        public bool IsDirty =>
            ChangedBool(NewYCapture, YCapture) || ChangedText(NewYExposure, YExposure) ||
            ChangedBool(NewXCapture, XCapture) || ChangedText(NewXExposure, XExposure) ||
            ChangedBool(NewZCapture, ZCapture) || ChangedText(NewZExposure, ZExposure);

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
