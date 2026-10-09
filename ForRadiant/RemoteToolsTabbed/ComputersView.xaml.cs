using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace RemoteToolsTabbed
{
    /// <summary>One line of ping output.</summary>
    public sealed class PingLine
    {
        public PingLine(string text, Brush brush)
        {
            Text = text;
            Brush = brush;
        }

        public string Text { get; private set; }
        public Brush Brush { get; private set; }
    }

    /// <summary>The machine list in the sidebar.</summary>
    public partial class ComputersView : UserControl
    {
        private const int PingCount = 30;

        /// <summary>Height of a list's header band, used before the pinned copy has been laid out.</summary>
        private const double BandHeight = 27;

        private static readonly Brush PingInfo = new SolidColorBrush(Color.FromRgb(0xD8, 0xD8, 0xD8));
        private static readonly Brush PingGood = new SolidColorBrush(Color.FromRgb(0x78, 0xDC, 0x78));
        private static readonly Brush PingBad = new SolidColorBrush(Color.FromRgb(0xFF, 0x7A, 0x7A));

        private readonly ObservableCollection<Computer> _computers = new ObservableCollection<Computer>();
        private readonly ObservableCollection<PingLine> _pingLines = new ObservableCollection<PingLine>();
        private readonly List<MachineList> _lists;
        private readonly ICollectionView _view;
        private CancellationTokenSource _pingCts;
        private DataGridColumnHeadersPresenter _headers;
        private MachineList _activeList;
        private bool _pinnedBandQueued;

        public ComputersView()
        {
            InitializeComponent();

            List<string> pruned;
            _lists = MachineLists.Load(out pruned);

            List<string> notes = new List<string>();
            foreach (MachineList list in _lists)
                LoadInto(list, notes);

            if (pruned.Count > 0)
                notes.Add("Dropped " + Join(pruned) + " - the file is gone.");

            _view = CollectionViewSource.GetDefaultView(_computers);
            _view.Filter = o =>
            {
                Computer computer = o as Computer;
                return computer == null || computer.Matches(Search.Text);
            };

            // Always grouped, even with only the default list loaded: one band saying "All 421"
            // is the same answer to "which file is this row from" as three bands are, and it
            // keeps Expand all / Collapse all meaning the same thing however many are open.
            _view.GroupDescriptions.Add(new PropertyGroupDescription("List"));

            Grid.ItemsSource = _view;
            PingList.ItemsSource = _pingLines;
            _computers.CollectionChanged += (s, args) => UpdateTitle();
            UpdateTitle();
            SelectFirstIfNothingSelected();

            if (!File.Exists(Paths.Viewer))
                notes.Insert(0, "tvnviewer.exe is missing from " + Paths.AppFolder + " - remote control is unavailable.");

            Hint.Text = string.Join(Environment.NewLine, notes.ToArray());
        }

        public int MachineCount
        {
            get { return _computers.Count; }
        }

        /// <summary>The gear: the host runs the viewer so it can own the dialogs it puts up.</summary>
        public event EventHandler ViewerOptionsRequested;

        /// <summary>The chevron: put the list away (Ctrl+B).</summary>
        public event EventHandler CollapseRequested;

        /// <summary>Turned off while the viewer is open capturing the options.</summary>
        public bool ViewerOptionsEnabled
        {
            get { return BtnViewerOptions.IsEnabled; }
            set { BtnViewerOptions.IsEnabled = value; }
        }

        /// <summary>
        /// The collapse chevron. Shown while the list is docked in the main window; the flyout
        /// has a Dock button of its own and nothing to collapse into.
        /// </summary>
        public bool ShowCollapseButton
        {
            get { return BtnCollapse.Visibility == Visibility.Visible; }
            set { BtnCollapse.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
        }

        private void UpdateTitle()
        {
            Title.Text = string.Format("COMPUTERS  ({0})", _computers.Count);
        }

        private void ViewerOptions_Click(object sender, RoutedEventArgs e)
        {
            EventHandler handler = ViewerOptionsRequested;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void Collapse_Click(object sender, RoutedEventArgs e)
        {
            EventHandler handler = CollapseRequested;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        // ---------------------------------------------------------------- loaded CSVs

        /// <summary>Appends one CSV's rows; a read failure costs that list, not the others.</summary>
        private void LoadInto(MachineList list, List<string> notes)
        {
            try
            {
                foreach (Computer computer in ComputerList.Load(list))
                    _computers.Add(computer);
            }
            catch (Exception ex)
            {
                notes.Add("Could not read " + list.Path + " - " + ex.Message);
            }
        }

        // ------------------------------------------------------------ pinned band

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            if (_headers == null)
                _headers = FindChild<DataGridColumnHeadersPresenter>(Grid);

            QueuePinnedBand();
        }

        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            QueuePinnedBand();
        }

        private void Grid_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            QueuePinnedBand();
        }

        /// <summary>
        /// Always deferred, never measured in the handler: scrolling realises rows that have
        /// not been arranged yet, so reading their positions straight away reports where the
        /// last frame put them and pins the wrong list.
        /// </summary>
        private void QueuePinnedBand()
        {
            if (_pinnedBandQueued)
                return;

            _pinnedBandQueued = true;
            Dispatcher.BeginInvoke((Action)delegate
            {
                _pinnedBandQueued = false;
                UpdatePinnedBand();
            }, DispatcherPriority.Background);
        }

        /// <summary>
        /// Keeps the list you are scrolling through named: whatever is drawn at the top of the
        /// rows area decides. A row there means its list's own band has gone up out of sight,
        /// so a copy of that band is put in its place; a band there already says what it needs
        /// to and the copy stays hidden.
        ///
        /// What is at the top is hit-tested rather than worked out from the containers' own
        /// positions. Neither set of positions can be trusted: a virtualized GroupItem reports
        /// an estimated height for the rows it has not built, and a recycled row keeps the
        /// position it had before it was reused, which is how a row scrolled well off the top
        /// came back as the topmost one and pinned the list above the right one.
        ///
        /// The copy carries the group as its DataContext, so the same template and the same two
        /// handlers drive it - clicking it folds the list away, its x unloads the list.
        /// </summary>
        private void UpdatePinnedBand()
        {
            if (_headers == null || _view == null || _view.GroupDescriptions.Count == 0)
            {
                PinnedBand.Visibility = Visibility.Collapsed;
                return;
            }

            Point headers;
            if (!TryLocate(_headers, out headers))
                return;

            double rowsTop = headers.Y + _headers.ActualHeight;

            double probe = headers.X + 8;
            double strip = PinnedBand.ActualHeight > 0 ? PinnedBand.ActualHeight : BandHeight;

            MachineList list;
            bool band;

            // A band already at the top speaks for itself; a row there needs its list named.
            if (!TryHitRows(probe, rowsTop + 2, out list, out band) || band || list == null)
            {
                PinnedBand.Visibility = Visibility.Collapsed;
                return;
            }

            // The next list's band has arrived right underneath, so it is about to take over.
            // Standing a band on top of it would label the handful of rows it is covering and
            // leave two bands stacked, so the real one is left to it.
            MachineList next;
            bool nextIsBand;
            if (TryHitRows(probe, rowsTop + strip + 2, out next, out nextIsBand) && nextIsBand)
            {
                PinnedBand.Visibility = Visibility.Collapsed;
                return;
            }

            CollectionViewGroup group = GroupOf(list);
            if (group == null)
            {
                PinnedBand.Visibility = Visibility.Collapsed;
                return;
            }

            PinnedBand.DataContext = group;
            PinnedBand.Margin = new Thickness(headers.X, rowsTop, 0, 0);
            PinnedBand.Width = _headers.ActualWidth;
            PinnedBand.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// What the grid draws at the given point: a row, and then <paramref name="list"/> is
        /// the list it belongs to, or a list's band, and then <paramref name="band"/> is set.
        /// Returns false when the point is over neither. The pinned copy is taken out of the
        /// hit test for the length of the call - left in, it answers for the row it is covering
        /// and holds itself up for good.
        /// </summary>
        private bool TryHitRows(double x, double y, out MachineList list, out bool band)
        {
            list = null;
            band = false;

            IInputElement hit;

            // InputHitTest, not VisualTreeHelper.HitTest: only the input one honours
            // IsHitTestVisible, and the visual one happily returned the pinned band itself -
            // which then kept answering for the row it was covering.
            PinnedBand.IsHitTestVisible = false;
            try
            {
                hit = GridHost.InputHitTest(new Point(x, y));
            }
            finally
            {
                PinnedBand.IsHitTestVisible = true;
            }

            DependencyObject node = hit as DependencyObject;

            while (node != null)
            {
                DataGridRow row = node as DataGridRow;
                if (row != null)
                {
                    Computer computer = row.Item as Computer;
                    list = computer == null ? null : computer.List;
                    return true;
                }

                if (node is GroupItem)
                {
                    band = true;            // a real band is drawn there
                    return true;
                }

                node = VisualTreeHelper.GetParent(node);
            }

            return false;
        }

        private CollectionViewGroup GroupOf(MachineList list)
        {
            foreach (object item in _view.Groups)
            {
                CollectionViewGroup group = item as CollectionViewGroup;
                if (group != null && ReferenceEquals(group.Name, list))
                    return group;
            }

            return null;
        }

        /// <summary>
        /// Where an element sits inside <see cref="GridHost"/>. Returns false while it is not
        /// in the tree yet - TransformToAncestor throws rather than saying so.
        /// </summary>
        private bool TryLocate(Visual element, out Point position)
        {
            position = new Point();

            try
            {
                position = element.TransformToAncestor(GridHost).Transform(new Point(0, 0));
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static T FindChild<T>(DependencyObject root) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(root);

            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                T found = child as T ?? FindChild<T>(child);
                if (found != null)
                    return found;
            }

            return null;
        }

        private void ExpandAll_Click(object sender, RoutedEventArgs e)
        {
            SetAllExpanded(true);
        }

        private void CollapseAll_Click(object sender, RoutedEventArgs e)
        {
            SetAllExpanded(false);
        }

        private void SetAllExpanded(bool expanded)
        {
            foreach (MachineList list in _lists)
                list.IsExpanded = expanded;

            QueuePinnedBand();
        }

        /// <summary>
        /// A filter must never leave a match hidden behind a collapsed header, so any list
        /// that still has one is opened. Lists with nothing to show drop out of the view by
        /// themselves - an empty group is not rendered - so there is nothing to close here.
        /// </summary>
        private void ExpandListsWithMatches()
        {
            string text = Search.Text;
            if (string.IsNullOrWhiteSpace(text))
                return;

            foreach (Computer computer in _computers)
            {
                if (computer.List != null && !computer.List.IsExpanded && computer.Matches(text))
                    computer.List.IsExpanded = true;
            }
        }

        /// <summary>
        /// Adds an empty row to the selected row's list - the default list when nothing useful
        /// is selected - and opens it for editing where it belongs. The grid's own new-item row
        /// is switched off: with the lists grouped it sat alone at the very bottom, under every
        /// group, and whatever was typed there joined All.csv whichever list you were working in.
        /// </summary>
        private void AddMachine_Click(object sender, RoutedEventArgs e)
        {
            // Finish the row being typed before adding another: what was typed has to reach the
            // object, and committing leaves that row current, which is what names the list.
            Grid.CommitEdit(DataGridEditingUnit.Row, true);

            MachineList target = _activeList;
            if (target == null && _lists.Count > 0)
                target = _lists[0];

            Computer computer = new Computer { List = target };
            _activeList = target;

            if (target != null)
                target.IsExpanded = true;

            // Added, not refreshed. An ObservableCollection add drops itself into the right
            // group on its own, while _view.Refresh() resets the whole view and the grid lets
            // go of its selection - with a row still being edited that went unnoticed, because
            // editing follows the current cell rather than the selection, and every row added
            // after the first one then fell back to the default list.
            _computers.Add(computer);

            // An empty row matches no filter, so it would otherwise be added out of sight.
            if (Search.Text.Length > 0)
                Search.Clear();         // raises TextChanged, which refreshes the view

            // The row has to exist before it can be scrolled to and edited.
            Dispatcher.BeginInvoke((Action)delegate
            {
                Grid.SelectedItem = computer;
                Grid.ScrollIntoView(computer);
                Grid.CurrentCell = new DataGridCellInfo(computer, Grid.Columns[0]);
                Grid.Focus();
                Grid.BeginEdit();
            }, DispatcherPriority.Background);

            Status(target == null
                ? "Added an empty row - type a name and address, then Save Lists."
                : "Added an empty row to " + target.Title + " - type a name and address, then Save Lists.");
        }

        /// <summary>
        /// Remembers the list being worked in, for + Add machine to add to.
        ///
        /// Asking the grid for its selection at the moment of the click does not work. Adding a
        /// row leaves the grid with no selection and no current cell at all - the row being
        /// typed stays editable because editing follows the current cell, which hides it - so
        /// every row added after the first one read an empty selection and fell back to the
        /// default list. A list the user has actually been in is remembered here instead, and a
        /// deselection does not clear it.
        /// </summary>
        private void Grid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Computer row = Grid.SelectedItem as Computer;
            if (row != null && row.List != null)
                _activeList = row.List;
        }

        private void AddList_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Add a machine list";
            dialog.Filter = "Machine lists (*.csv)|*.csv|All files (*.*)|*.*";
            dialog.Multiselect = true;
            dialog.CheckFileExists = true;
            // Start where All.csv and the exe live - that is where the other lists
            // usually sit, and it beats whatever folder Windows last remembered.
            dialog.InitialDirectory = Paths.AppFolder;

            if (dialog.ShowDialog() != true)
                return;

            List<string> added = new List<string>();
            List<string> notes = new List<string>();

            foreach (string chosen in dialog.FileNames)
            {
                string path = MachineLists.FullPath(chosen);
                if (path == null)
                    continue;

                if (MachineLists.Contains(_lists, path))
                {
                    notes.Add(System.IO.Path.GetFileName(path) + " is already loaded.");
                    continue;
                }

                MachineList list = new MachineList(path, false);
                _lists.Add(list);
                LoadInto(list, notes);
                added.Add(list.Title);
            }

            if (added.Count > 0)
            {
                MachineLists.Save(_lists);
                _view.Refresh();
                SelectFirstIfNothingSelected();
                QueuePinnedBand();
            }

            if (notes.Count > 0)
                Hint.Text = string.Join(Environment.NewLine, notes.ToArray());

            Status(added.Count > 0
                ? "Added " + Join(added) + " - " + _computers.Count + " machines in " + _lists.Count + " lists."
                : "Nothing added.");
        }

        /// <summary>The group header band: folds that one list away.</summary>
        private void GroupHeader_Click(object sender, MouseButtonEventArgs e)
        {
            MachineList list = ListOf(sender);
            if (list != null)
                list.IsExpanded = !list.IsExpanded;

            QueuePinnedBand();
        }

        /// <summary>The MachineList the clicked group header belongs to, or null.</summary>
        private static MachineList ListOf(object sender)
        {
            FrameworkElement source = sender as FrameworkElement;
            CollectionViewGroup group = source == null ? null : source.DataContext as CollectionViewGroup;
            return group == null ? null : group.Name as MachineList;
        }

        /// <summary>The x on a group header: the group's own MachineList is its key.</summary>
        private void RemoveListHeader_Click(object sender, RoutedEventArgs e)
        {
            RemoveList(ListOf(sender));
        }

        /// <summary>
        /// Unloads one list. The CSV is left alone - re-adding it is one click, so this does
        /// not ask first. All.csv is the default list and is never removable.
        /// </summary>
        private void RemoveList(MachineList list)
        {
            if (list == null)
                return;

            if (list.IsDefault)
            {
                Status(list.Title + " is the default list and stays loaded.");
                return;
            }

            for (int i = _computers.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_computers[i].List, list))
                    _computers.RemoveAt(i);
            }

            _lists.Remove(list);

            if (ReferenceEquals(_activeList, list))
                _activeList = _lists.Count > 0 ? _lists[0] : null;

            MachineLists.Save(_lists);
            _view.Refresh();
            SelectFirstIfNothingSelected();
            QueuePinnedBand();
            Grid.Focus();           // the button that was clicked has just been templated away

            Status("Unloaded " + list.Title + " - the CSV itself was not deleted.");
        }

        private static string Join(List<string> parts)
        {
            return string.Join(", ", parts.ToArray());
        }

        /// <summary>Raised when the user asks to open one or more sessions.</summary>
        public event EventHandler<IReadOnlyList<Computer>> ConnectRequested;

        public event EventHandler<string> StatusChanged;

        public void FocusSearch()
        {
            Search.Focus();
            Search.SelectAll();
        }

        /// <summary>
        /// What the buttons act on: every selected row, or - when nothing is selected -
        /// whatever was typed into the search box.
        /// </summary>
        private List<Computer> Targets()
        {
            List<Computer> targets = new List<Computer>();

            foreach (object item in Grid.SelectedItems)
            {
                Computer computer = item as Computer;
                if (computer != null && !string.IsNullOrWhiteSpace(computer.Address))
                    targets.Add(computer);
            }

            if (targets.Count == 0)
            {
                string typed = Search.Text.Trim();
                if (typed.Length > 0)
                    targets.Add(new Computer { Name = typed, Address = typed });
            }

            targets.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return targets;
        }

        private void Connect()
        {
            List<Computer> targets = Targets();
            if (targets.Count == 0)
            {
                Status("Select a machine first, or type an address in the search box.");
                return;
            }

            EventHandler<IReadOnlyList<Computer>> handler = ConnectRequested;
            if (handler != null)
                handler(this, targets);
        }

        private void Status(string message)
        {
            EventHandler<string> handler = StatusChanged;
            if (handler != null)
                handler(this, message);
        }

        // ---------------------------------------------------------------- list / filter

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_view == null)
                return;

            _view.Refresh();
            ExpandListsWithMatches();
            SelectFirstIfNothingSelected();
            QueuePinnedBand();
        }

        /// <summary>
        /// Keeps a row under the buttons at all times, the way the old WinForms grid always
        /// had a current row: type a filter, press Enter, and the top match is what opens.
        /// </summary>
        private void SelectFirstIfNothingSelected()
        {
            if (Grid.SelectedItem != null)
                return;

            foreach (object item in _view)
            {
                Grid.SelectedItem = item;
                break;
            }
        }

        private void Search_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Search.Clear();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                Connect();
                e.Handled = true;
            }
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            Search.Clear();
            Search.Focus();
        }

        /// <summary>
        /// Only a double-click on a row opens a session. The column headers, the group bands
        /// and the empty space under the last row are all part of the grid, and a row is always
        /// kept selected, so testing the selection alone used to connect from any of them -
        /// easy to trigger by accident while folding a list away.
        /// </summary>
        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Hit-tested rather than taken from e.OriginalSource: a double-click puts the cell
            // into edit mode first, which pulls the element that was clicked out of the visual
            // tree, so by the time this runs there is nothing left to walk up from.
            if (RowUnder(Grid.InputHitTest(e.GetPosition(Grid))) != null && Grid.SelectedItem is Computer)
                Connect();
        }

        /// <summary>
        /// The row the given element sits in, or null. Walked by hand rather than with
        /// ItemsControl.ContainerFromElement, which does not reach through the group
        /// containers that sit between the grid and its rows once grouping is on.
        /// </summary>
        private static DataGridRow RowUnder(object source)
        {
            DependencyObject node = source as DependencyObject;

            while (node != null)
            {
                DataGridRow row = node as DataGridRow;
                if (row != null)
                    return row;

                node = node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
            }

            return null;
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Grid.SelectedItem is Computer)
            {
                Connect();
                e.Handled = true;
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            Connect();
        }

        private void FileShare_Click(object sender, RoutedEventArgs e)
        {
            foreach (Computer target in Targets())
            {
                try
                {
                    Process.Start(@"\\" + target.Address);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(@"\\" + target.Address + Environment.NewLine + ex.Message,
                        "Remote Tools", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Writes every loaded list back to its own CSV. A row typed straight into the grid
        /// has no source yet, so it joins the default list.
        /// </summary>
        private void SaveList_Click(object sender, RoutedEventArgs e)
        {
            MachineList fallback = _lists.Count > 0 ? _lists[0] : null;

            try
            {
                Grid.CommitEdit(DataGridEditingUnit.Row, true);

                foreach (Computer computer in _computers)
                {
                    if (computer != null && computer.List == null)
                        computer.List = fallback;
                }

                // An Add machine row that was never filled in is not written to the CSV, so
                // leaving it in the grid would only show a blank line that no save explains.
                for (int i = _computers.Count - 1; i >= 0; i--)
                {
                    Computer computer = _computers[i];
                    if (computer == null ||
                        (string.IsNullOrWhiteSpace(computer.Name) && string.IsNullOrWhiteSpace(computer.Address)))
                        _computers.RemoveAt(i);
                }

                foreach (MachineList list in _lists)
                    ComputerList.Save(list, _computers);

                Status(_lists.Count == 1
                    ? "Saved " + _computers.Count + " machines to " + Paths.Csv
                    : "Saved " + _computers.Count + " machines to " + _lists.Count + " lists.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving the list:" + Environment.NewLine + ex.Message,
                    "Remote Tools", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ---------------------------------------------------------------- ping

        private async void Ping_Click(object sender, RoutedEventArgs e)
        {
            if (_pingCts != null)
            {
                _pingCts.Cancel();
                return;
            }

            List<Computer> targets = Targets();
            if (targets.Count == 0)
            {
                Status("Select a machine first, or type an address in the search box.");
                return;
            }

            string host = targets[0].Address;
            _pingLines.Clear();
            Append("Pinging " + host + " ...", PingInfo);

            _pingCts = new CancellationTokenSource();
            BtnPing.Content = "Stop";
            try
            {
                await PingLoopAsync(host, _pingCts.Token);
            }
            finally
            {
                BtnPing.Content = "Ping";
                _pingCts.Dispose();
                _pingCts = null;
            }
        }

        private async Task PingLoopAsync(string host, CancellationToken token)
        {
            int replies = 0;

            using (Ping ping = new Ping())
            {
                for (int i = 0; i < PingCount && !token.IsCancellationRequested; i++)
                {
                    try
                    {
                        PingReply reply = await ping.SendPingAsync(host, 2000);
                        if (reply.Status == IPStatus.Success)
                        {
                            replies++;
                            // the sidebar panel is narrow - keep a reply to one line
                            Append(string.Format("reply  {0,4} ms   TTL {1}", reply.RoundtripTime,
                                reply.Options == null ? 0 : reply.Options.Ttl), PingGood);
                        }
                        else
                        {
                            Append(reply.Status.ToString(), PingBad);
                        }
                    }
                    catch (Exception ex)
                    {
                        Append(ex.InnerException != null ? ex.InnerException.Message : ex.Message, PingBad);
                        break;
                    }

                    try
                    {
                        await Task.Delay(800, token);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }

            Append(string.Format("--- {0}: {1} replies ---", host, replies), PingInfo);
        }

        private void Append(string text, Brush brush)
        {
            ShowPingPanel();
            _pingLines.Add(new PingLine(text, brush));
            PingScroller.ScrollToEnd();
        }

        /// <summary>
        /// The output panel takes no room in the sidebar until there is something to show;
        /// an empty black box above the buttons just ate space the machine list wanted.
        /// </summary>
        private void ShowPingPanel()
        {
            if (PingRow.Height.Value > 0)
                return;

            PingRow.Height = new GridLength(120);
            PingSplitter.Visibility = Visibility.Visible;
        }

        public void StopPing()
        {
            if (_pingCts != null)
                _pingCts.Cancel();
        }
    }
}
