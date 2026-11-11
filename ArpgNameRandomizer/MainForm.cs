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
        NumericUpDown numMaxLen;
        TextBox txtPrefix;
        Label lblRules, lblStatus, lblName;
        ListBox lstFavorites;
        ToolTip tip = new ToolTip();

        public MainForm()
        {
            Text = "ARPG Name Randomizer - " + Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            Icon = Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            Font = new Font("Segoe UI", 9.5f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(820, 500);
            MinimumSize = new Size(700, 420);
            StartPosition = FormStartPosition.CenterScreen;

            BuildUi();
            LoadFavorites();
            foreach (string f in lstFavorites.Items) seen.Add(f);
            LoadSettings();
            FormClosing += (s, e) => SaveSettings();
            Shown += (s, e) => Generate();
        }

        void BuildUi()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1, Padding = new Padding(8) };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));

            // ---- options ----
            var opts = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
            int w = 210;
            cboGame = Combo(w); cboClass = Combo(w); cboGender = Combo(w); cboStyle = Combo(w);
            cboGame.Items.AddRange(NameData.Games.ToArray());
            cboGender.Items.AddRange(new object[] { "Any", "Male", "Female" });
            cboStyle.Items.AddRange(new object[] { "Mixed", "Fantasy name (Morvath)", "Compound (Bonecaller)", "Name + title (KaelDuskborn)" });
            numMaxLen = new NumericUpDown { Width = 70, Minimum = 3, Maximum = 23, Value = 14 };
            txtPrefix = new TextBox { Width = w };
            txtPrefix.KeyPress += (s, e) => { if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar)) e.Handled = true; };
            lblRules = new Label { AutoSize = true, MaximumSize = new Size(w, 0), ForeColor = Color.DimGray, Margin = new Padding(3, 8, 3, 3) };

            opts.Controls.AddRange(new Control[]
            {
                Lbl("Game"), cboGame,
                Lbl("Class"), cboClass,
                Lbl("Gender (made-up names only)"), cboGender,
                Lbl("Style"), cboStyle,
                Lbl("Max length"), numMaxLen,
                Lbl("Starts with (optional)"), txtPrefix,
                lblRules,
            });
            cboGame.SelectedIndexChanged += (s, e) => OnGameChanged();

            // ---- the name: one at a time, big ----
            var mid = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            mid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mid.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lblName = new Label { AutoSize = false, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 26f, FontStyle.Bold), Cursor = Cursors.Hand };
            lblName.Click += (s, e) => CopyName(lblName.Text);
            lblName.Resize += (s, e) => FitName();
            tip.SetToolTip(lblName, "Click to copy");
            mid.Controls.Add(lblName, 0, 0);
            mid.Controls.Add(new Label { Text = "Click the name to copy it", AutoSize = true, ForeColor = Color.DimGray, Anchor = AnchorStyles.None }, 0, 1);

            var buttons = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.None, WrapContents = false, Margin = new Padding(3, 12, 3, 24) };
            var btnGen = new Button { Text = "Generate", Width = 170, Height = 44, Font = new Font(Font.FontFamily, 11f, FontStyle.Bold) };
            btnGen.Click += (s, e) => Generate();
            AcceptButton = btnGen;
            var btnFav = new Button { Text = "Add to favourites", Width = 150, Height = 44 };
            btnFav.Click += (s, e) => AddFavorite(lblName.Text);
            buttons.Controls.Add(btnGen);
            buttons.Controls.Add(btnFav);
            mid.Controls.Add(buttons, 0, 2);
            Shown += (s, e) => btnGen.Focus();   // Enter or Space = next name

            // ---- favourites ----
            var right = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            right.Controls.Add(Lbl("Favourites (saved)"), 0, 0);
            lstFavorites = new ListBox { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11f), IntegralHeight = false };
            lstFavorites.Click += (s, e) => CopyName(lstFavorites.SelectedItem as string);
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

        void Generate()
        {
            var p = CurrentPool();
            if (p == null) return;
            if (p.Total(CurrentStyle) == 0)
            {
                lblName.Text = "";
                Status("No names fit those settings - try a longer max length or a shorter prefix.");
                return;
            }

            var names = p.Take(1, CurrentStyle, seen, rnd);
            if (names.Count == 0)
            {
                // Every name for these settings was shown: start the cycle over (favourites stay out).
                seen.Clear();
                foreach (string f in lstFavorites.Items) seen.Add(f);
                names = p.Take(1, CurrentStyle, seen, rnd);
            }
            if (names.Count == 0) { lblName.Text = ""; Status("Every name for these settings is a favourite already."); return; }
            lblName.Text = names[0];
            FitName();
            Status($"{CurrentGame.Name} / {((GameClass)cboClass.SelectedItem).Name}");
        }

        // Shrink the font for long names (PoE allows 23 letters) so they stay on one line.
        void FitName()
        {
            int room = lblName.ClientSize.Width - 8;
            if (room < 100 || string.IsNullOrEmpty(lblName.Text)) return;   // not laid out yet
            float size = 26f;
            while (size > 12f && TextRenderer.MeasureText(lblName.Text, new Font("Segoe UI", size, FontStyle.Bold), new Size(int.MaxValue, int.MaxValue), TextFormatFlags.SingleLine).Width > room)
                size -= 1f;
            if (Math.Abs(lblName.Font.Size - size) > 0.1f) lblName.Font = new Font("Segoe UI", size, FontStyle.Bold);
        }

        void CopyName(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
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
                    "prefix=" + txtPrefix.Text,
                });
            }
            catch { }
        }
    }
}
