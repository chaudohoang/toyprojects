using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KitHelper
{
    // Pick an old install, a new one, and which kit folders to carry over (see KitMigration).
    // Returns the choice; the panel does the copy so it can log it.
    public partial class MigrateWindow : Window
    {
        public string From => CboFrom.SelectedItem as string;
        public string To => CboTo.SelectedItem as string;
        public List<CopyTarget> Targets => checks.Where(c => c.Value.IsChecked == true && c.Value.IsEnabled).Select(c => c.Key).ToList();
        public bool SkipEdtMobile => ChkEdt.IsChecked == true;

        readonly Dictionary<CopyTarget, CheckBox> checks = new Dictionary<CopyTarget, CheckBox>();
        readonly CheckBox ChkEdt = new CheckBox { Content = "Skip " + KitMigration.EdtMobile + "  (keep the new install's)", IsChecked = true, Margin = new Thickness(26, 0, 0, 8) };
        readonly List<string> installs = TrueTest.AllInstalls();
        readonly string root = TrueTest.Root;

        public MigrateWindow(string to)
        {
            InitializeComponent();
            foreach (var t in CopyTarget.All)
            {
                var c = new CheckBox { Margin = new Thickness(0, 0, 0, 8), Tag = t };
                c.Checked += (s, e) => Refresh();
                c.Unchecked += (s, e) => Refresh();
                checks[t] = c;
                Folders.Children.Add(c);
                if (t.Name == "LGDVHCustomApp") Folders.Children.Add(ChkEdt);
            }

            // To: what the panel has selected (normally the freshly installed "TrueTest 1.8").
            foreach (var i in installs) CboTo.Items.Add(i);
            CboTo.SelectedItem = installs.FirstOrDefault(i => string.Equals(i, to, StringComparison.OrdinalIgnoreCase))
                                 ?? installs.FirstOrDefault(i => !TrueTest.LooksRenamed(i));
            if (installs.Count < 2)
                TxtProblem.Text = $"Only {installs.Count} TrueTest install(s) under {root} - rename the old one before installing the new, then come back.";
        }

        // From = every other install; the default is the most recently renamed one.
        void CboTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string keep = From;
            var others = installs.Where(i => !string.Equals(i, To, StringComparison.OrdinalIgnoreCase)).ToList();
            CboFrom.Items.Clear();
            foreach (var i in others) CboFrom.Items.Add(i);
            CboFrom.SelectedItem = others.Contains(keep) ? keep : TrueTest.GuessSource(others);
        }

        void CboFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string from = From;
            TxtFromHint.Text = from == null ? ""
                : TrueTest.LooksRenamed(from) ? $"Renamed install, changed {TrueTest.ChangeTime(from).ToLocalTime():yyyy-MM-dd HH:mm}"
                : "Not a renamed install - make sure this is the old one";
            // A folder the old install does not have cannot be copied: greyed out, unticked.
            foreach (var kv in checks)
            {
                var have = kv.Key.SubFolders.Where(s => from != null && Directory.Exists(Path.Combine(from, s))).ToList();
                string label = string.Join(" + ", kv.Key.SubFolders);
                kv.Value.IsEnabled = have.Count > 0;
                kv.Value.IsChecked = have.Count > 0;
                kv.Value.Content = have.Count > 0 ? label : label + "   (not in this install)";
                kv.Value.Opacity = have.Count > 0 ? 1 : 0.5;
            }
            Refresh();
        }

        void Refresh()
        {
            var lgdvh = checks.First(c => c.Key.Name == "LGDVHCustomApp").Value;
            ChkEdt.IsEnabled = lgdvh.IsEnabled && lgdvh.IsChecked == true;
            ChkEdt.Opacity = ChkEdt.IsEnabled ? 1 : 0.5;
            BtnCopy.IsEnabled = From != null && To != null && Targets.Count > 0;
        }

        void Copy_Click(object sender, RoutedEventArgs e) => DialogResult = true;
        void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
        void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) DialogResult = false;
        }
    }
}
