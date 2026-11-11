using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ArpgNameRandomizer
{
    public class MainForm : Form
    {
        readonly string appDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string FavoritesPath => Path.Combine(appDir, "favorites.txt");
        string SettingsPath => Path.Combine(appDir, "settings.txt");

        readonly Random rnd = new Random();
        // Names shown since the app started (plus favourites), so a name never comes up twice in a session.
        readonly HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        NamePool pool;
        string poolKey;

        ComboBox cboGame, cboClass, cboGender, cboStyle;
        NumericUpDown numMaxLen, numCount;
        TextBox txtPrefix;
        Label lblRules, lblStatus, lblPool;
        ListBox lstNames, lstFavorites;
        ToolTip tip = new ToolTip();

        public MainForm()
        {
            Text = "ARPG Name Randomizer - " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            Font = new Font("Segoe UI", 9.5f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(880, 620);
            MinimumSize = new Size(700, 420);
            StartPosition = FormStartPosition.CenterScreen;

            BuildUi();
            LoadFavorites();
            foreach (string f in lstFavorites.Items) seen.Add(f);
            LoadSettings();
            UpdatePoolLabel();
            FormClosing += (s, e) => SaveSettings();
            Shown += (s, e) => Generate();
        }

        void BuildUi()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(8) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            // ---- options ----
            var opts = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            int w = 210;
            cboGame = Combo(w); cboClass = Combo(w); cboGender = Combo(w); cboStyle = Combo(w);
            cboGame.Items.AddRange(NameData.Games.ToArray());
            cboGender.Items.AddRange(new object[] { "Any", "Male", "Female" });
            cboStyle.Items.AddRange(new object[] { "Mixed", "Fantasy name (Morvath)", "Compound (Bonecaller)", "Name + title (KaelDuskborn)" });
            numMaxLen = new NumericUpDown { Width = 70, Minimum = 3, Maximum = 23, Value = 14 };
            numCount = new NumericUpDown { Width = 70, Minimum = 1, Maximum = 200, Value = 30 };
            txtPrefix = new TextBox { Width = w };
            txtPrefix.KeyPress += (s, e) => { if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };
            lblRules = new Label { AutoSize = true, MaximumSize = new Size(w, 0), ForeColor = Color.DimGray, Margin = new Padding(3, 8, 3, 3) };
            var btnGen = new Button { Text = "Generate", Width = w, Height = 36, Font = new Font(Font, FontStyle.Bold) };
            btnGen.Click += (s, e) => Generate();
            AcceptButton = btnGen;
            lblPool = new Label { AutoSize = true, MaximumSize = new Size(w, 0), Margin = new Padding(3, 8, 3, 3) };

            opts.Controls.AddRange(new Control[]
            {
                Lbl("Game"), cboGame,
                Lbl("Class"), cboClass,
                Lbl("Gender (made-up names only)"), cboGender,
                Lbl("Style"), cboStyle,
                Lbl("Max length"), numMaxLen,
                Lbl("Starts with (optional)"), txtPrefix,
                Lbl("How many"), numCount,
                btnGen,
                lblPool,
                lblRules,
            });
            cboGame.SelectedIndexChanged += (s, e) => OnGameChanged();
            cboClass.SelectedIndexChanged += (s, e) => UpdatePoolLabel();
            cboGender.SelectedIndexChanged += (s, e) => UpdatePoolLabel();
            cboStyle.SelectedIndexChanged += (s, e) => UpdatePoolLabel();
            numMaxLen.ValueChanged += (s, e) => UpdatePoolLabel();
            txtPrefix.TextChanged += (s, e) => UpdatePoolLabel();

            // ---- results ----
            var mid = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1 };
            mid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mid.Controls.Add(Lbl("Names - click to copy, double-click to add to favourites"), 0, 0);
            lstNames = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 13f), IntegralHeight = false };
            lstNames.Click += (s, e) => CopySelected(lstNames);
            lstNames.DoubleClick += (s, e) => AddFavorite(lstNames.SelectedItem as string);
            lstNames.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F || e.KeyCode == Keys.Insert) AddFavorite(lstNames.SelectedItem as string);
                if (e.Control && e.KeyCode == Keys.C) CopySelected(lstNames);
            };
            mid.Controls.Add(lstNames, 0, 1);

            // ---- favourites ----
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.Controls.Add(Lbl("Favourites (saved)"), 0, 0);
            lstFavorites = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11f), IntegralHeight = false };
            lstFavorites.Click += (s, e) => CopySelected(lstFavorites);
            lstFavorites.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) RemoveFavorite(); };
            right.Controls.Add(lstFavorites, 0, 1);
            var btnRemove = new Button { Text = "Remove", AutoSize = true };
            btnRemove.Click += (s, e) => RemoveFavorite();
            tip.SetToolTip(btnRemove, "Remove the selected favourite (or press Delete)");
            right.Controls.Add(btnRemove, 0, 2);

            root.Controls.Add(opts, 0, 0);
            root.Controls.Add(mid, 1, 0);
            root.Controls.Add(right, 2, 0);

            lblStatus = new Label { Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(10, 0, 0, 0), BackColor = SystemColors.ControlLight };

            Controls.Add(root);
            Controls.Add(lblStatus);
        }

        static Label Lbl(string text) => new Label { Text = text, AutoSize = true, Margin = new Padding(3, 8, 3, 2) };
        static ComboBox Combo(int width) => new ComboBox { Width = width, DropDownStyle = ComboBoxStyle.DropDownList };

        Game CurrentGame => cboGame.SelectedItem as Game;

        void OnGameChanged()
        {
            var g = CurrentGame;
            if (g == null) return;
            string prevClass = (cboClass.SelectedItem as GameClass)?.Name;
            cboClass.Items.Clear();
            cboClass.Items.AddRange(g.Classes.ToArray());
            int idx = g.Classes.FindIndex(c => c.Name == prevClass);
            cboClass.SelectedIndex = idx >= 0 ? idx : 0;

            decimal keep = numMaxLen.Value;
            numMaxLen.Minimum = Math.Max(3, g.MinLen);
            numMaxLen.Maximum = g.MaxLen;
            numMaxLen.Value = Math.Min(Math.Max(keep, numMaxLen.Minimum), numMaxLen.Maximum);

            lblRules.Text = g.Rules;
        }

        NamePool CurrentPool()
        {
            var g = CurrentGame;
            var cls = cboClass.SelectedItem as GameClass;
            if (g == null || cls == null || cboGender.SelectedIndex < 0) return null;
            string key = string.Join("|", g.Name, cls.Name, cboGender.SelectedIndex, numMaxLen.Value, txtPrefix.Text.Trim().ToLowerInvariant());
            if (key != poolKey) { pool = new NamePool(g, cls, (Gender)cboGender.SelectedIndex, (int)numMaxLen.Value, txtPrefix.Text); poolKey = key; }
            return pool;
        }

        Style CurrentStyle => cboStyle.SelectedIndex < 0 ? Style.Mixed : (Style)cboStyle.SelectedIndex;

        void UpdatePoolLabel()
        {
            var p = CurrentPool();
            if (p == null || lblPool == null) return;
            long total = p.Total(CurrentStyle), left = p.Remaining(CurrentStyle, seen);
            lblPool.Text = $"{total:N0} possible names for these settings, {left:N0} not shown yet";
        }

        void Generate()
        {
            var p = CurrentPool();
            if (p == null) return;

            int want = (int)numCount.Value;
            var names = p.Take(want, CurrentStyle, seen, rnd);
            bool wrapped = false;
            if (names.Count < want && p.Total(CurrentStyle) > 0)
            {
                // Every name for these settings was shown: start the cycle over (favourites stay out).
                var keep = new HashSet<string>(names, StringComparer.OrdinalIgnoreCase);
                seen.Clear();
                foreach (string f in lstFavorites.Items) seen.Add(f);
                foreach (var n in keep) seen.Add(n);
                names.AddRange(p.Take(want - names.Count, CurrentStyle, seen, rnd));
                wrapped = true;
            }

            lstNames.BeginUpdate();
            lstNames.Items.Clear();
            lstNames.Items.AddRange(names.ToArray());
            lstNames.EndUpdate();
            UpdatePoolLabel();

            var cls = (GameClass)cboClass.SelectedItem;
            if (p.Total(CurrentStyle) == 0)
                Status("No names fit those settings - try a longer max length or a shorter prefix.");
            else if (wrapped)
                Status("Every name for these settings has been shown - starting over.");
            else
                Status($"{names.Count} new names for {CurrentGame.Name} / {cls.Name}.");
        }

        void CopySelected(ListBox list)
        {
            if (!(list.SelectedItem is string name)) return;
            try { Clipboard.SetText(name); Status($"Copied \"{name}\""); }
            catch (Exception ex) { Status("Copy failed: " + ex.Message); }
        }

        void AddFavorite(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            if (lstFavorites.Items.Cast<string>().Any(f => f.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                Status($"\"{name}\" is already a favourite");
                return;
            }
            lstFavorites.Items.Add(name);
            SaveFavorites();
            Status($"Added \"{name}\" to favourites");
        }

        void RemoveFavorite()
        {
            int i = lstFavorites.SelectedIndex;
            if (i < 0) return;
            lstFavorites.Items.RemoveAt(i);
            if (lstFavorites.Items.Count > 0) lstFavorites.SelectedIndex = Math.Min(i, lstFavorites.Items.Count - 1);
            SaveFavorites();
        }

        void Status(string text) => lblStatus.Text = text;

        void LoadFavorites()
        {
            try
            {
                if (File.Exists(FavoritesPath))
                    lstFavorites.Items.AddRange(File.ReadAllLines(FavoritesPath).Select(l => l.Trim()).Where(l => l.Length > 0).ToArray());
            }
            catch (Exception ex) { Status("Could not read favorites.txt: " + ex.Message); }
        }

        void SaveFavorites()
        {
            try { File.WriteAllLines(FavoritesPath, lstFavorites.Items.Cast<string>()); }
            catch (Exception ex) { Status("Could not save favorites.txt: " + ex.Message); }
        }

        // settings.txt: simple key=value lines
        void LoadSettings()
        {
            var d = new Dictionary<string, string>();
            try
            {
                if (File.Exists(SettingsPath))
                    foreach (var line in File.ReadAllLines(SettingsPath))
                    {
                        int eq = line.IndexOf('=');
                        if (eq > 0) d[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
                    }
            }
            catch { }

            int gi = d.TryGetValue("game", out var gs) ? NameData.Games.FindIndex(x => x.Name == gs) : -1;
            cboGame.SelectedIndex = gi >= 0 ? gi : 0;
            if (d.TryGetValue("class", out var cs))
            {
                int ci = CurrentGame.Classes.FindIndex(x => x.Name == cs);
                if (ci >= 0) cboClass.SelectedIndex = ci;
            }
            cboGender.SelectedIndex = Int(d, "gender", 0, 0, cboGender.Items.Count - 1);
            cboStyle.SelectedIndex = Int(d, "style", 0, 0, cboStyle.Items.Count - 1);
            numMaxLen.Value = Int(d, "maxlen", 14, (int)numMaxLen.Minimum, (int)numMaxLen.Maximum);
            numCount.Value = Int(d, "count", 30, 1, 200);
            if (d.TryGetValue("prefix", out var p)) txtPrefix.Text = p;
        }

        static int Int(Dictionary<string, string> d, string key, int def, int min, int max)
            => d.TryGetValue(key, out var s) && int.TryParse(s, out int v) ? Math.Min(Math.Max(v, min), max) : def;

        void SaveSettings()
        {
            try
            {
                File.WriteAllLines(SettingsPath, new[]
                {
                    "game=" + CurrentGame?.Name,
                    "class=" + (cboClass.SelectedItem as GameClass)?.Name,
                    "gender=" + cboGender.SelectedIndex,
                    "style=" + cboStyle.SelectedIndex,
                    "maxlen=" + numMaxLen.Value,
                    "count=" + numCount.Value,
                    "prefix=" + txtPrefix.Text,
                });
            }
            catch { }
        }
    }
}
