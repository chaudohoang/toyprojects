using System.ComponentModel;

namespace RemoteToolsTabbed
{
    /// <summary>One row of the machine list.</summary>
    public sealed class Computer : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _address = string.Empty;
        private MachineList _list;

        public string Name
        {
            get { return _name; }
            set { if (_name != value) { _name = value ?? string.Empty; Raise("Name"); } }
        }

        public string Address
        {
            get { return _address; }
            set { if (_address != value) { _address = value ?? string.Empty; Raise("Address"); } }
        }

        /// <summary>
        /// Which loaded CSV this row came from, and the one Save List writes it back to.
        /// Null on a row just typed into the grid; saving adopts those into the default list.
        /// This is also what the grid groups on, so it has to be public - grouping on an
        /// internal property finds nothing and silently drops every row into one group.
        /// </summary>
        public MachineList List
        {
            get { return _list; }
            set
            {
                if (ReferenceEquals(_list, value))
                    return;

                _list = value;
                Raise("List");
                Raise("ListName");
            }
        }

        /// <summary>The source list's title, used by the search.</summary>
        public string ListName
        {
            get { return _list == null ? string.Empty : _list.Title; }
        }

        /// <summary>
        /// The list name counts as a match too: with every CSV merged into one grid, typing
        /// a list's name is how you narrow the view down to it.
        /// </summary>
        public bool Matches(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return true;

            text = text.Trim();
            return Name.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) >= 0
                || Address.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) >= 0
                || ListName.IndexOf(text, System.StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Raise(string property)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }
    }
}
