using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SeqxcToolset.Tasks.LuminanceScaleTask
{
    public class LuminanceRowVM : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public string DisplayIndex => (Index + 1).ToString();

        public string PatternSetupName { get; set; }
        public string AnalysisType { get; set; }

        // Null when this item's Analysis type doesn't have these fields at all.
        public string Red { get; set; }
        public string Green { get; set; }
        public string Blue { get; set; }

        // False for Analysis types (e.g. RegisterPixelsLGDN) that don't carry
        // Luminance Scale at all — used to disable editing for those rows,
        // since there's nothing there to apply a new value to.
        public bool HasLuminanceFields => Red != null || Green != null || Blue != null;

        private string _newRed, _newGreen, _newBlue, _newAll;

        public string NewRed
        {
            get => _newRed;
            set { _newRed = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }
        public string NewGreen
        {
            get => _newGreen;
            set { _newGreen = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }
        public string NewBlue
        {
            get => _newBlue;
            set { _newBlue = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsDirty)); }
        }

        // Convenience column: typing here fans the value out to Red/Green/Blue
        // at once (they're almost always the same). One-way broadcast, not a
        // bound mirror — editing an individual channel afterward still sticks.
        public string NewAll
        {
            get => _newAll;
            set
            {
                _newAll = value;
                OnPropertyChanged();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    NewRed = value;
                    NewGreen = value;
                    NewBlue = value;
                }
            }
        }

        private static bool Changed(string newVal, string current) =>
            !string.IsNullOrEmpty(newVal) && current != null && newVal.Trim() != current;

        public bool IsDirty => Changed(NewRed, Red) || Changed(NewGreen, Green) || Changed(NewBlue, Blue);

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
