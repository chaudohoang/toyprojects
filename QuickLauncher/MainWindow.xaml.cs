using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;

namespace QuickLauncher
{
    // The panel: two tabs of tiles (Games, Apps), opened from the floating icon and hidden again
    // as soon as focus goes elsewhere (like Kit Helper's).
    public partial class MainWindow : Window
    {
        public Window Anchor;
        public event Action<bool> BusyChanged;
        public event Action<string> Notify;

        readonly Tab games = Tab.Games(), apps = Tab.Apps();
        Tab cur;
        IEnumerable<Tab> Tabs => new[] { games, apps };

        readonly IconLoader icons;
        // One right-click menu for every tile, filled when it opens (a menu per tile was ~270 menus).
        readonly ContextMenu tileMenu = new ContextMenu();
        // Typing filters after a short pause, not on every keystroke.
        readonly DispatcherTimer searchDelay = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(90) };
        int modalDepth;

        public MainWindow()
        {
            InitializeComponent();
            icons = new IconLoader(Dispatcher, 64);   // tiles show icons at 64 DIP
            searchDelay.Tick += (o, e) => { searchDelay.Stop(); ApplyFilter(); };
            TxtVersion.Text = BuildInfo.Text;
            var s = Settings.Current;
            CboSort.SelectedIndex = int.TryParse(s.Get("Sort", "0"), out int si) && si >= 0 && si < CboSort.Items.Count ? si : 0;
            ChkHidden.IsChecked = s.Get("ShowHidden") == "1";
            foreach (var t in Tabs) t.LoadState();
            cur = s.Get("Tab") == apps.Key ? apps : games;
            (cur == apps ? TabApps : TabGames).IsChecked = true;
            ShowTab();

            KeyDown += (o, e) =>
            {
                if (e.Key == Key.Escape) HidePanel();
                else if (e.Key == Key.Tab && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    (cur == games ? TabApps : TabGames).IsChecked = true;
                    e.Handled = true;
                }
            };
            PreviewTextInput += (o, e) =>
            {
                // Typing anywhere in the panel goes to the search box.
                if (!(Keyboard.FocusedElement is TextBox)) { TxtSearch.Focus(); }
            };
            Closing += (o, e) => { if (!Program.Exiting) { e.Cancel = true; HidePanel(); } };
            Deactivated += (o, e) => AutoHide();
            Drop += (o, e) => { if (e.Data.GetData(DataFormats.FileDrop) is string[] p) AddDropped(p); };
        }

        // ---------------------------------------------------------------- tabs

        void Tab_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized || TxtRoots == null) return;
            var t = sender == TabApps ? apps : games;
            if (t == cur) return;
            cur.Roots = ParseRoots();          // keep an unsaved edit of the folder box
            cur = t;
            ShowTab();
            Scroller.ScrollToTop();
            TxtSearch.Focus();
        }

        // Everything on screen that depends on the tab.
        void ShowTab()
        {
            TxtRoots.Text = string.Join("; ", cur.Roots);
            LblRoots.Text = cur == games ? "Game folders" : "App folders";
            SearchHint.Text = $"Search {cur.Many}…  (Enter {(cur == games ? "plays" : "opens")} the first one)";
            TxtHelp.Text = $"Click: {cur.Verb.ToLowerInvariant()} · Right-click: run as admin, choose exe, rename, hide";
            BtnRescan.IsEnabled = !cur.Scanning;
            TxtStatus.Text = cur.Status;
            ApplyFilter();
        }

        void UpdateTabLabels()
        {
            TabGames.Content = $"Games  {games.Items.Count(g => !g.Hidden && g.Exe != null)}";
            TabApps.Content = $"Apps  {apps.Items.Count(g => !g.Hidden && g.Exe != null)}";
        }

        // ---------------------------------------------------------------- start-up / scan

        // The saved lists show at once; a rescan of both tabs then runs in the background to
        // pick up what was installed or removed since.
        public void StartupScan()
        {
            ApplyFilter();
            RescanAll();
        }

        public void RescanAll() { foreach (var t in Tabs) Rescan(t); }

        async void Rescan(Tab t)
        {
            if (t.Scanning) return;
            if (t == cur) t.Roots = ParseRoots();
            var roots = t.Roots;
            Settings.Current.Save();
            if (t == cur) TxtRoots.Text = string.Join("; ", roots);

            t.Scanning = true;
            if (t == cur) BtnRescan.IsEnabled = false;
            SetBusy();
            var sw = Stopwatch.StartNew();
            var old = t.Items.ToList();
            // Apps leave out Start menu entries for programs inside the game folders.
            var skip = t == apps ? games.Roots : null;
            try
            {
                var found = await Task.Run(() => Library.Rescan(roots, old,
                    p => Dispatcher.BeginInvoke(new Action(() => SetStatus(t, "Scanning " + p + "…"))), t.StartMenu, skip));
                // Manual entries come from the list as it is NOW, not the snapshot the scan started
                // from: an exe dropped on the icon (or removed) during the scan must not be undone.
                found.RemoveAll(g => g.Manual);
                found.AddRange(t.Items.Where(g => g.Manual && File.Exists(g.Folder)));
                int added = found.Count(g => !g.Manual && !old.Any(o => string.Equals(o.Folder, g.Folder, StringComparison.OrdinalIgnoreCase)));
                int removed = old.Count(o => !o.Manual && !found.Any(g => string.Equals(o.Folder, g.Folder, StringComparison.OrdinalIgnoreCase)));
                t.Items = found;
                t.LastScan = DateTime.Now;
                if (t == cur) ApplyFilter(); else UpdateTabLabels();
                string delta = added + removed == 0 ? "no changes" : $"{added} new, {removed} gone";
                string where = roots.Count + " folder(s)" + (t.StartMenu ? " + Start menu" : "");
                SetStatus(t, $"Scanned {where} in {sw.Elapsed.TotalSeconds:0.0} s: {found.Count} {t.Many}, {delta}.");
                SaveList(t);
                if (old.Count > 0 && added > 0) Notify?.Invoke($"{added} new {(added == 1 ? t.One : t.Many)} found.");
            }
            catch (Exception ex) { SetStatus(t, "Scan failed: " + ex.Message); }
            finally
            {
                t.Scanning = false;
                if (t == cur) BtnRescan.IsEnabled = true;
                SetBusy();
            }
        }

        void SetBusy() => BusyChanged?.Invoke(Tabs.Any(t => t.Scanning));

        void SetStatus(Tab t, string text)
        {
            t.Status = text;
            if (t == cur) TxtStatus.Text = text;
        }

        // Every change to a list goes through here; a failed save is shown, not swallowed.
        void SaveList(Tab t)
        {
            string err = Library.Save(t.ListFile, t.Items);
            if (err != null)
            {
                SetStatus(t, $"Could not save the {t.One} list: " + err);
                Notify?.Invoke($"Could not save the {t.One} list: " + err);
            }
        }

        List<string> ParseRoots() =>
            TxtRoots.Text.Split(';').Select(r => r.Trim().Trim('"').TrimEnd('\\')).Where(r => r != "")
                .Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        void TxtRoots_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) Rescan(cur); }
        void Rescan_Click(object sender, RoutedEventArgs e) => Rescan(cur);

        void AddRoot_Click(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog { Description = $"A folder that holds one folder per {cur.One}" })
            {
                if (Modal(() => dlg.ShowDialog()) != System.Windows.Forms.DialogResult.OK) return;
                AddRoot(dlg.SelectedPath);
            }
        }

        void AddRoot(string folder)
        {
            var roots = ParseRoots();
            if (!roots.Contains(folder.TrimEnd('\\'), StringComparer.OrdinalIgnoreCase)) roots.Add(folder.TrimEnd('\\'));
            TxtRoots.Text = string.Join("; ", roots);
            Rescan(cur);
        }

        // Dropped on the icon or the panel: goes to the tab that is showing (or was, if the panel
        // is hidden). An exe or a desktop shortcut becomes an entry, a folder a scanned folder.
        public void AddDropped(string[] paths)
        {
            var t = cur;
            var notes = new List<string>();
            foreach (string p in paths)
            {
                if (Directory.Exists(p)) { AddRoot(p); notes.Add($"Added {t.One} folder {p}"); continue; }
                bool exe = File.Exists(p) && Path.GetExtension(p).Equals(".exe", StringComparison.OrdinalIgnoreCase);
                bool shortcut = File.Exists(p) && Shortcut.Is(p);
                if (!exe && !shortcut) { notes.Add(Path.GetFileName(p) + ": drop an .exe, a shortcut, or a folder."); continue; }

                string target;
                try { target = shortcut ? Shortcut.Resolve(p) : p; }
                catch (Exception ex) { notes.Add(Path.GetFileName(p) + ": " + ex.Message); continue; }

                // Already listed: the exe it starts, the exe it was added as, or one of its
                // candidates. If it is hidden, dropping it is a clear "I want this": unhide it.
                var existing = t.Items.FirstOrDefault(g => string.Equals(g.Exe, target, StringComparison.OrdinalIgnoreCase)
                                                        || string.Equals(g.Folder, target, StringComparison.OrdinalIgnoreCase))
                            ?? t.Items.FirstOrDefault(g => g.Candidates.Contains(target, StringComparer.OrdinalIgnoreCase));
                if (existing != null)
                {
                    var did = new List<string>();
                    if (existing.Hidden) { existing.Hidden = false; did.Add("shown again (was hidden)"); }
                    // Another of its exes (POE.lnk starts PathOfExile_x64.exe, the scan picked
                    // PathOfExile.exe): the dropped one is how it is really started.
                    if (!string.Equals(existing.Exe, target, StringComparison.OrdinalIgnoreCase))
                    {
                        existing.UserExe = target;
                        existing.Icon = null;
                        did.Add("now starts " + Path.GetFileName(target));
                    }
                    if (!FolderOn(t, existing)) { t.FoldersOff.Remove(FolderKeyOf(existing)); did.Add("its folder switched back on"); }
                    existing.Refresh();
                    notes.Add(existing.Name + (did.Count == 0 ? " is already in the list." : ": " + string.Join(", ", did) + "."));
                    continue;
                }

                // A shortcut is named after itself ("RustDesk.lnk" → "RustDesk"), not its target's folder.
                var added = Library.Manual(target, shortcut ? Path.GetFileNameWithoutExtension(p) : null);
                t.Items.Add(added);
                t.FoldersOff.Remove(FolderKeyOf(added));
                notes.Add($"Added {added.Name} to {t.Key}");
            }
            SaveList(t);
            ApplyFilter();
            if (notes.Count > 0) Notify?.Invoke(notes.Count <= 3 ? string.Join("\n", notes) : $"{string.Join("\n", notes.Take(3))}\n…and {notes.Count - 3} more.");
        }

        // ---------------------------------------------------------------- filter / sort

        void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchHint.Visibility = TxtSearch.Text == "" ? Visibility.Visible : Visibility.Collapsed;
            BtnClearSearch.Visibility = TxtSearch.Text == "" ? Visibility.Collapsed : Visibility.Visible;
            // Cleared (✕, Esc, after a launch): at once. Typing: after a short pause.
            searchDelay.Stop();
            if (TxtSearch.Text == "") ApplyFilter(); else searchDelay.Start();
        }

        void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = "";
            TxtSearch.Focus();
        }

        void Search_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Enter right after typing: filter now, so it launches the first match of what was typed.
            if (e.Key == Key.Enter && searchDelay.IsEnabled) { searchDelay.Stop(); ApplyFilter(); }
            if (e.Key == Key.Enter && Tiles.Items.Count > 0) { Launch((Game)Tiles.Items[0], false); e.Handled = true; }
            else if (e.Key == Key.Escape && TxtSearch.Text != "") { TxtSearch.Text = ""; e.Handled = true; }
            else if (e.Key == Key.Down && Tiles.Items.Count > 0)
            {
                (Tiles.ItemContainerGenerator.ContainerFromIndex(0) as ContentPresenter)?.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                e.Handled = true;
            }
        }

        void Sort_Changed(object sender, SelectionChangedEventArgs e) { if (cur != null) ApplyFilter(); }
        void Filter_Changed(object sender, RoutedEventArgs e) { if (cur != null) ApplyFilter(); }

        // ---- folder toggles: one on/off pill per scanned folder, plus "Start menu" (Apps) and
        // "Added manually". Every pill is on until switched off; switched-off ones are remembered per tab.
        const string ManualKey = "\u0001manual";

        static string FolderKeyOf(Game g) => g.Manual ? ManualKey : g.Root;
        static bool FolderOn(Tab t, Game g) => !t.FoldersOff.Contains(FolderKeyOf(g));

        // Rebuilt with every filter pass so the counts follow hide / rescan.
        void FillFolders(bool showHidden)
        {
            var visible = cur.Items.Where(g => g.Exe != null && (showHidden || !g.Hidden)).ToList();
            var keys = new List<string>();
            if (cur.StartMenu) keys.Add(Library.StartMenuKey);
            keys.AddRange(ParseRoots());
            keys.AddRange(visible.Where(g => !g.Manual && !g.IsStartMenu).Select(g => g.Root)
                .Where(r => !keys.Contains(r, StringComparer.OrdinalIgnoreCase)).Distinct(StringComparer.OrdinalIgnoreCase));
            if (visible.Any(g => g.Manual)) keys.Add(ManualKey);

            FolderToggles.Children.Clear();
            foreach (string key in keys)
            {
                int n = visible.Count(g => string.Equals(FolderKeyOf(g), key, StringComparison.OrdinalIgnoreCase));
                string label = key == ManualKey ? "Added manually" : key == Library.StartMenuKey ? "Start menu" : key;
                var chip = new ToggleButton
                {
                    Style = (Style)FindResource("Chip"),
                    Content = $"{label}  {n}",
                    IsChecked = !cur.FoldersOff.Contains(key),
                    ToolTip = "Click: show / hide these · Right-click: show only these (again: all)",
                };
                var t = cur;
                chip.Click += (s, e) =>
                {
                    if (chip.IsChecked == true) t.FoldersOff.Remove(key); else t.FoldersOff.Add(key);
                    ApplyFilter();
                };
                chip.MouseRightButtonUp += (s, e) =>
                {
                    bool alreadySolo = !t.FoldersOff.Contains(key) && keys.All(k => k == key || t.FoldersOff.Contains(k));
                    t.FoldersOff.Clear();
                    if (!alreadySolo) foreach (string k in keys) if (k != key) t.FoldersOff.Add(k);
                    ApplyFilter();
                    e.Handled = true;
                };
                FolderToggles.Children.Add(chip);
            }
        }

        void ApplyFilter()
        {
            if (Tiles == null || cur == null) return;
            string q = TxtSearch.Text.Trim();
            bool showHidden = ChkHidden.IsChecked == true;
            FillFolders(showHidden);
            var words = q.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<Game> v = cur.Items.Where(g => g.Exe != null && (showHidden || !g.Hidden) && FolderOn(cur, g));
            if (words.Length > 0)
                v = v.Where(g => words.All(w => g.Name.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0
                                             || (!g.IsStartMenu && Path.GetFileName(g.Folder).IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0)));

            switch (CboSort.SelectedIndex)
            {
                case 1: v = v.OrderBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase); break;
                case 2: v = v.OrderByDescending(g => g.Plays).ThenByDescending(g => g.LastPlayed).ThenBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase); break;
                case 3: v = v.OrderBy(g => g.IsStartMenu ? "" : g.Root, StringComparer.OrdinalIgnoreCase).ThenBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase); break;
                default: v = v.OrderByDescending(g => g.LastPlayed ?? DateTime.MinValue).ThenBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase); break;
            }
            var list = v.ToList();
            Tiles.ItemsSource = list;

            UpdateTabLabels();
            int hidden = cur.Items.Count(g => g.Hidden);
            TxtCount.Text = hidden > 0 ? $"{hidden} hidden" : "";
            TxtEmpty.Visibility = list.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            TxtEmpty.Text = cur.Scanning && cur.Items.Count == 0 ? $"Looking for {cur.Many}…"
                : cur.Items.Count == 0 ? $"No {cur.Many} found. Add a folder that holds one folder per {cur.One}, or drop an .exe or a shortcut here."
                : !cur.Items.Any(g => FolderOn(cur, g)) ? "Every folder is switched off. Click one above to show it."
                : q == "" ? $"No {cur.Many} in the folders switched on."
                : $"No {cur.One} matches \"{q}\"" + (cur.FoldersOff.Count > 0 ? " in the folders switched on." : ".")
                  + (OtherTabMatches(q) is string other ? "\n" + other : "");
            LoadIcons(list);
        }

        // "3 matches in Apps": a search that comes up empty here may be meant for the other tab.
        string OtherTabMatches(string q)
        {
            if (q == "") return null;
            var other = cur == games ? apps : games;
            var words = q.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int n = other.Items.Count(g => !g.Hidden && g.Exe != null && words.All(w => g.Name.IndexOf(w, StringComparison.OrdinalIgnoreCase) >= 0));
            return n == 0 ? null : $"{n} {(n == 1 ? "match" : "matches")} in {other.Key} (Ctrl+Tab).";
        }

        // Icons arrive from the loader's own thread as they are read; the panel never waits for them.
        void LoadIcons(List<Game> list)
        {
            foreach (var g in list.Where(g => g.Icon == null && g.Exe != null))
            {
                string exe = g.Exe;
                icons.Get(exe, img => { if (string.Equals(g.Exe, exe, StringComparison.OrdinalIgnoreCase)) g.Icon = img; });
            }
        }

        // The orb's Recent menu: both tabs together.
        public IEnumerable<Game> Recent(int n) =>
            Tabs.SelectMany(t => t.Items).Where(g => g.LastPlayed != null && !g.Hidden && g.Exe != null)
                .OrderByDescending(g => g.LastPlayed).Take(n);

        Tab TabOf(Game g) => apps.Items.Contains(g) ? apps : games;

        // ---------------------------------------------------------------- launch

        void Tile_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is Game g) Launch(g, false);
        }

        public void Launch(Game g, bool admin)
        {
            string exe = g.Exe;
            if (exe == null || !Library.TargetExists(exe)) { Notify?.Invoke(g.Name + ": exe not found. Rescan, or right-click → Choose exe."); return; }
            try
            {
                // A Start menu entry without a file ("shell:AppsFolder\…": Store apps, shortcuts)
                // is started by the shell, exactly as Start does it.
                Process.Start(new ProcessStartInfo(exe)
                {
                    WorkingDirectory = StartMenu.IsShellTarget(exe) ? "" : Path.GetDirectoryName(exe),
                    UseShellExecute = true,
                    Verb = admin ? "runas" : "",
                });
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223) { return; }   // UAC cancelled
            catch (Exception ex) { Notify?.Invoke("Could not start " + g.Name + ": " + ex.Message); return; }

            var t = TabOf(g);
            g.LastPlayed = DateTime.Now;
            g.Plays++;
            g.Refresh();
            SaveList(t);
            Notify?.Invoke("Starting " + g.Name + "…");
            HidePanel();
            TxtSearch.Text = "";
            // Re-sort now: "Recently used" / "Most used" just changed. (Clearing the search
            // re-filters only when there was text, so a launch from the grid left the old order.)
            ApplyFilter();
        }

        // ---------------------------------------------------------------- tile menu

        void Tile_RightClick(object sender, MouseButtonEventArgs e)
        {
            var btn = (Button)sender;
            if (!(btn.DataContext is Game g)) return;
            e.Handled = true;
            FillTileMenu(g);
            tileMenu.PlacementTarget = btn;
            tileMenu.Placement = PlacementMode.MousePoint;
            tileMenu.IsOpen = true;
        }

        void FillTileMenu(Game g)
        {
            var t = TabOf(g);
            var m = tileMenu;
            m.Items.Clear();
            bool shellOnly = StartMenu.IsShellTarget(g.Exe);

            m.Items.Add(Item(t.Verb, () => Launch(g, false), bold: true));
            m.Items.Add(Item("Run as administrator", () => Launch(g, true)));
            m.Items.Add(new Separator());

            var choose = new MenuItem { Header = "Choose exe" };
            foreach (string c in g.Candidates.Take(15))
            {
                string rel = StartMenu.IsShellTarget(c) ? "Start menu entry"
                    : c.StartsWith(g.Folder, StringComparison.OrdinalIgnoreCase) && c.Length > g.Folder.Length
                    ? c.Substring(g.Folder.Length).TrimStart('\\') : c;
                var mi = Item(rel.Replace("_", "__"), () => SetExe(g, c));
                mi.IsCheckable = true;
                mi.IsChecked = string.Equals(c, g.Exe, StringComparison.OrdinalIgnoreCase);
                choose.Items.Add(mi);
            }
            if (choose.Items.Count > 0) choose.Items.Add(new Separator());
            choose.Items.Add(Item("Browse…", () => BrowseExe(g)));
            if (g.UserExe != null) choose.Items.Add(Item("Back to automatic pick", () => SetExe(g, null)));
            m.Items.Add(choose);

            m.Items.Add(Item("Rename…", () => Rename(g)));
            var open = Item("Open folder", () => Process.Start("explorer.exe", "/select,\"" + g.Exe + "\""));
            open.IsEnabled = !shellOnly;   // a Store app has no folder to show
            m.Items.Add(open);
            m.Items.Add(new Separator());
            m.Items.Add(Item(g.Hidden ? "Unhide" : "Hide", () => { g.Hidden = !g.Hidden; g.Refresh(); SaveList(t); ApplyFilter(); }));
            if (g.Manual)
                m.Items.Add(Item("Remove from list", () =>
                {
                    t.Items.Remove(g);
                    // A kept copy of a dropped shortcut belongs to this entry alone.
                    if (g.Folder.StartsWith(Shortcut.Dir + "\\", StringComparison.OrdinalIgnoreCase))
                        try { File.Delete(g.Folder); } catch { }
                    SaveList(t);
                    ApplyFilter();
                }));
        }

        static MenuItem Item(string header, Action a, bool bold = false)
        {
            var mi = new MenuItem { Header = header };
            if (bold) mi.FontWeight = FontWeights.SemiBold;
            mi.Click += (s, e) => a();
            return mi;
        }

        void SetExe(Game g, string exe)
        {
            g.UserExe = exe;
            g.Icon = null;
            g.Refresh();
            SaveList(TabOf(g));
            LoadIcons(new List<Game> { g });
        }

        void BrowseExe(Game g)
        {
            string start = Directory.Exists(g.Folder) ? g.Folder
                : g.Exe != null && !StartMenu.IsShellTarget(g.Exe) ? Path.GetDirectoryName(g.Exe) : null;
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Exe to start " + g.Name,
                Filter = "Programs (*.exe)|*.exe",
                InitialDirectory = start ?? "",
            };
            if (Modal(() => dlg.ShowDialog(this)) == true) SetExe(g, dlg.FileName);
        }

        void Rename(Game g)
        {
            string what = g.IsStartMenu ? "the Start menu entry " + g.AutoName : Path.GetFileName(g.Folder);
            string name = Modal(() => InputWindow.Ask(this, "Rename", "Name shown for " + what + " (empty = automatic):", g.Name));
            if (name == null) return;
            g.UserName = name.Trim() == "" || name.Trim() == g.AutoName ? null : name.Trim();
            g.Refresh();
            SaveList(TabOf(g));
            ApplyFilter();
        }

        // ---------------------------------------------------------------- show / hide

        public void ShowNear(Window anchor)
        {
            anchor = anchor ?? Anchor;
            // Next to the icon: below it when the icon is in the top half of the screen, above
            // otherwise, always fully on that screen.
            var wa = WorkAreaOf(anchor ?? this);
            if (Height > wa.Height) Height = wa.Height;
            double iconCentre = anchor.Left + anchor.Width / 2;
            Left = Clamp(iconCentre - Width / 2, wa.Left, wa.Right - Width);
            double top = anchor.Top + anchor.Height / 2 < wa.Top + wa.Height / 2 ? anchor.Top + anchor.Height : anchor.Top - Height;
            Top = Clamp(top, wa.Top, wa.Bottom - Height);
            Show();
            Activate();
            TxtSearch.Focus();
            TxtSearch.SelectAll();
            Scroller.ScrollToTop();
            foreach (var t in Tabs)
                if (!t.Scanning && t.LastScan is DateTime last && DateTime.Now - last > TimeSpan.FromMinutes(30)) Rescan(t);
        }

        public void Toggle()
        {
            if (IsVisible) HidePanel();
            else ShowNear(Anchor);
        }

        void HidePanel()
        {
            SaveAll();
            Hide();
        }

        public void SaveAll()
        {
            var s = Settings.Current;
            s.Set("Sort", CboSort.SelectedIndex.ToString());
            s.Set("ShowHidden", ChkHidden.IsChecked == true ? "1" : "0");
            s.Set("Tab", cur.Key);
            cur.Roots = ParseRoots();
            foreach (var t in Tabs) t.SaveState();
            s.Save();
        }

        void AutoHide()
        {
            if (!IsVisible || modalDepth > 0 || Program.Exiting || CursorOver(Anchor)) return;
            // A context menu or combo drop-down took focus: not leaving the panel.
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
            {
                if (!IsActive && modalDepth == 0 && !OwnPopupOpen()) HidePanel();
            }));
        }

        bool OwnPopupOpen() => CboSort.IsDropDownOpen || tileMenu.IsOpen || (Keyboard.FocusedElement is MenuItem);

        T Modal<T>(Func<T> dialog)
        {
            modalDepth++;
            try { return dialog(); }
            finally { modalDepth--; Activate(); }
        }

        void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        void Hide_Click(object sender, RoutedEventArgs e) => HidePanel();

        static double Clamp(double v, double min, double max) => v < min ? min : v > max ? max : v;

        static Rect WorkAreaOf(Window w)
        {
            var h = new WindowInteropHelper(w).Handle;
            var source = PresentationSource.FromVisual(w);
            if (h == IntPtr.Zero || source?.CompositionTarget == null) return SystemParameters.WorkArea;
            var px = System.Windows.Forms.Screen.FromHandle(h).WorkingArea;
            var toDip = source.CompositionTarget.TransformFromDevice;
            return new Rect(toDip.Transform(new Point(px.Left, px.Top)), toDip.Transform(new Point(px.Right, px.Bottom)));
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT p);
        struct POINT { public int X, Y; }

        static bool CursorOver(Window w)
        {
            if (w == null || !w.IsVisible || !GetCursorPos(out POINT p)) return false;
            var local = w.PointFromScreen(new Point(p.X, p.Y));
            return local.X >= 0 && local.Y >= 0 && local.X <= w.ActualWidth && local.Y <= w.ActualHeight;
        }
    }
}
