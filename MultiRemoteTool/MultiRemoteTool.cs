using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

// ═══════════════════════════════════════════════════════════════
//  MultiRemoteTool  —  universal remote connection address book
//  Each platform = one INI file  +  one CSV contacts file
//  No NuGet, no designer. .NET Framework 4.x compatible.
// ═══════════════════════════════════════════════════════════════

namespace MultiRemoteTool
{
    // ──────────────────────────────────────────────────────────
    //  Platform  (loaded from / saved to  <Name>.ini)
    // ──────────────────────────────────────────────────────────
    public class Platform
    {
        public string Name         { get; set; }
        public string ExeName      { get; set; }
        public string CommandLine  { get; set; }
        public string BookFileName { get; set; }  // optional — overrides default contacts_<Name>.csv

        public string CsvPath()
        {
            // Use explicit BookFileName if provided, otherwise derive from platform name
            if (!string.IsNullOrWhiteSpace(BookFileName))
                return Path.Combine(AppDir(), BookFileName.Trim());
            return Path.Combine(AppDir(), "contacts_" + SafeName() + ".csv");
        }
        public string IniPath()
        {
            return Path.Combine(AppDir(), SafeName() + ".ini");
        }
        public string SafeName()
        {
            var sb = new StringBuilder();
            foreach (char c in Name)
                sb.Append((char.IsLetterOrDigit(c) || c == '_' || c == '-') ? c : '_');
            return sb.ToString();
        }
        private static string AppDir()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public string BuildArgs(Contact c)
        {
            string cleanId = c.Id == null ? "" : new string(c.Id.Where(ch => !char.IsWhiteSpace(ch)).ToArray());
            string pwd     = c.Password ?? "";
            return CommandLine
                .Replace("{id}",              cleanId)
                .Replace("{pw}",              pwd)
                .Replace("<REMOTE-ID>",       cleanId)   // legacy
                .Replace("<REMOTE-PASSWORD>", pwd);
        }

        // Builds args with {exe} substituted — needed for pipe commands like AnyDesk
        public string BuildArgs(Contact c, string exePath)
        {
            string cleanId = c.Id == null ? "" : new string(c.Id.Where(ch => !char.IsWhiteSpace(ch)).ToArray());
            string pwd     = c.Password ?? "";
            return CommandLine
                .Replace("{exe}",             exePath ?? "")
                .Replace("{id}",              cleanId)
                .Replace("{pw}",              pwd)
                .Replace("<REMOTE-ID>",       cleanId)
                .Replace("<REMOTE-PASSWORD>", pwd);
        }

        public bool UsesPipe { get { return CommandLine.Contains("|"); } }

        public void SaveIni()
        {
            var lines = new List<string>
            {
                "[Platform]",
                "Name="        + Name,
                "ExeName="     + ExeName,
                "CommandLine=" + CommandLine
            };
            if (!string.IsNullOrWhiteSpace(BookFileName))
                lines.Add("BookFileName=" + BookFileName.Trim());
            File.WriteAllLines(IniPath(), lines.ToArray(), Encoding.UTF8);
        }

        public static Platform LoadIni(string iniPath)
        {
            var p = new Platform { Name = "", ExeName = "", CommandLine = "", BookFileName = "" };
            foreach (var raw in File.ReadAllLines(iniPath, Encoding.UTF8))
            {
                var line = raw.Trim();
                if (line.StartsWith("[") || line == "") continue;
                int eq = line.IndexOf('=');
                if (eq < 0) continue;
                string key = line.Substring(0, eq).Trim();
                string val = line.Substring(eq + 1).Trim();
                if      (key == "Name")         p.Name         = val;
                else if (key == "ExeName")      p.ExeName      = val;
                else if (key == "CommandLine")  p.CommandLine  = val;
                else if (key == "BookFileName") p.BookFileName = val;
            }
            return p;
        }

        public static List<Platform> LoadAll()
        {
            var list = new List<Platform>();
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            foreach (var f in Directory.GetFiles(dir, "*.ini"))
            {
                try
                {
                    var p = LoadIni(f);
                    // FIX #5: skip INI files that aren't valid platform configs
                    // (empty Name would make SafeName() return "" and corrupt file paths)
                    if (!string.IsNullOrWhiteSpace(p.Name))
                        list.Add(p);
                }
                catch { }
            }
            list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
            return list;
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Contact
    // ──────────────────────────────────────────────────────────
    public class Contact
    {
        public string Name     { get; set; }
        public string Id       { get; set; }
        public string Password { get; set; }
        public Contact() { Name = ""; Id = ""; Password = ""; }
    }

    // ──────────────────────────────────────────────────────────
    //  CSV store  (per-platform)
    // ──────────────────────────────────────────────────────────
    public static class CsvStore
    {
        public static List<Contact> Load(string path)
        {
            var list = new List<Contact>();
            if (!File.Exists(path)) return list;
            foreach (var line in File.ReadAllLines(path, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = SplitCsv(line);
                if (parts.Length < 2) continue;
                list.Add(new Contact
                {
                    Name     = parts[0].Trim(),
                    Id       = parts[1].Trim(),
                    Password = parts.Length >= 3 ? parts[2].Trim() : ""
                });
            }
            return list;
        }

        public static void Save(string path, List<Contact> contacts)
        {
            var lines = contacts.Select(c =>
                Esc(c.Name) + "," + Esc(c.Id) + "," + Esc(c.Password));
            File.WriteAllLines(path, lines.ToArray(), Encoding.UTF8);
        }

        // FIX #6: handle "" escaped quotes inside quoted fields
        public static string[] SplitCsv(string line)
        {
            var result = new List<string>();
            bool inQ = false;
            var cur = new StringBuilder();
            int i = 0;
            while (i < line.Length)
            {
                char ch = line[i];
                if (ch == '"')
                {
                    if (inQ && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // "" inside a quoted field = literal double-quote character
                        cur.Append('"');
                        i += 2;
                        continue;
                    }
                    inQ = !inQ;
                }
                else if (ch == ',' && !inQ)
                {
                    result.Add(cur.ToString());
                    cur.Clear();
                }
                else
                {
                    cur.Append(ch);
                }
                i++;
            }
            result.Add(cur.ToString());
            return result.ToArray();
        }

        public static string Esc(string v)
        {
            if (v == null) return "";
            if (v.Contains(",") || v.Contains("\"") || v.Contains("\n"))
                return "\"" + v.Replace("\"", "\"\"") + "\"";
            return v;
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Platform add/edit dialog
    // ──────────────────────────────────────────────────────────
    public class PlatformDialog : Form
    {
        public Platform Result { get; private set; }
        private TextBox _txtName, _txtExe, _txtCmd, _txtBook;
        private Button  _btnOk, _btnCancel;

        public PlatformDialog(Platform existing)
        {
            bool isEdit     = (existing != null);
            Text            = isEdit ? "Edit Platform" : "Add Platform";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MinimizeBox     = false; MaximizeBox = false;
            ClientSize      = new Size(500, 260);
            Font            = new Font("Segoe UI", 9.5f);
            BackColor       = Color.FromArgb(245, 245, 245);

            int lx = 12, lw = 110, tx = 128, tw = 356, rowH = 38, y = 16;

            AddLabel("Platform Name:", lx, y + 2, lw);
            _txtName = AddTB(tx, y, tw);

            y += rowH;
            AddLabel("Executable:", lx, y + 2, lw);
            _txtExe = AddTB(tx, y, tw - 90);
            var btnBrowse = new Button
            {
                Text      = "Browse...",
                Location  = new Point(tx + tw - 86, y),
                Size      = new Size(86, 24),
                FlatStyle = FlatStyle.Flat
            };
            btnBrowse.FlatAppearance.BorderColor = Color.FromArgb(180,180,180);
            btnBrowse.Click += delegate
            {
                using (var d = new OpenFileDialog
                {
                    Title  = "Select executable",
                    Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*"
                })
                {
                    if (d.ShowDialog() == DialogResult.OK)
                        _txtExe.Text = d.FileName;
                }
            };
            Controls.Add(btnBrowse);

            y += rowH;
            AddLabel("Command:", lx, y + 2, lw);
            _txtCmd = AddTB(tx, y, tw);

            y += rowH;
            var hint = new Label
            {
                Text      = "Placeholders:  {id}  {pw}  {exe}  (use | for stdin pipe)",
                Location  = new Point(tx, y),
                Size      = new Size(tw, 18),
                ForeColor = Color.FromArgb(100, 100, 100),
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic)
            };
            Controls.Add(hint);

            y += 22;
            var example = new Label
            {
                Text      = "e.g.  --connect {id} --password {pw}",
                Location  = new Point(tx, y),
                Size      = new Size(tw, 18),
                ForeColor = Color.FromArgb(130, 130, 130),
                Font      = new Font("Consolas", 8f)
            };
            Controls.Add(example);

            y += rowH;
            AddLabel("Book File:", lx, y + 2, lw);
            _txtBook = AddTB(tx, y, tw);
            var bookHint = new Label
            {
                Text      = "Optional — leave blank to use  contacts_<Name>.csv",
                Location  = new Point(tx, y + 26),
                Size      = new Size(tw, 16),
                ForeColor = Color.FromArgb(130, 130, 130),
                Font      = new Font("Segoe UI", 8f, FontStyle.Italic)
            };
            Controls.Add(bookHint);

            y += rowH + 18;
            _btnOk = new Button
            {
                Text = "Save", DialogResult = DialogResult.OK,
                Location = new Point(tx + tw - 170, y),
                Size     = new Size(80, 28),
                BackColor = Color.FromArgb(0, 120, 212), ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _btnOk.FlatAppearance.BorderSize = 0;

            _btnCancel = new Button
            {
                Text = "Cancel", DialogResult = DialogResult.Cancel,
                Location = new Point(tx + tw - 84, y),
                Size     = new Size(80, 28), FlatStyle = FlatStyle.Flat
            };
            _btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180,180,180);

            Controls.AddRange(new Control[] { _btnOk, _btnCancel });
            AcceptButton = _btnOk; CancelButton = _btnCancel;
            ClientSize   = new Size(500, y + 44);

            if (isEdit)
            {
                _txtName.Text = existing.Name;
                _txtExe.Text  = existing.ExeName;
                _txtCmd.Text  = existing.CommandLine;
                _txtBook.Text = existing.BookFileName ?? "";
                // Name IS editable — MainForm handles the INI/CSV rename on save
            }

            _btnOk.Click += delegate
            {
                if (string.IsNullOrWhiteSpace(_txtName.Text))
                { Warn("Platform Name is required."); return; }
                if (string.IsNullOrWhiteSpace(_txtExe.Text))
                { Warn("Executable is required."); return; }
                if (string.IsNullOrWhiteSpace(_txtCmd.Text))
                { Warn("Command line is required."); return; }

                Result = new Platform
                {
                    Name         = _txtName.Text.Trim(),
                    ExeName      = _txtExe.Text.Trim(),
                    CommandLine  = _txtCmd.Text.Trim(),
                    BookFileName = _txtBook.Text.Trim()
                };
            };
        }

        private void Warn(string msg) {
            MessageBox.Show(msg, "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            DialogResult = DialogResult.None;
        }
        private void AddLabel(string t, int x, int y, int w)
        {
            Controls.Add(new Label { Text = t, Location = new Point(x, y),
                Size = new Size(w, 22), TextAlign = ContentAlignment.MiddleRight });
        }
        private TextBox AddTB(int x, int y, int w)
        {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 24),
                BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(tb);
            return tb;
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Contact add/edit dialog
    // ──────────────────────────────────────────────────────────
    public class ContactDialog : Form
    {
        public Contact Result { get; private set; }
        private TextBox _txtName, _txtId, _txtPwd;

        public ContactDialog(Contact existing, string platformName)
        {
            Text            = (existing == null ? "Add Contact" : "Edit Contact")
                              + " — " + platformName;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MinimizeBox     = false; MaximizeBox = false;
            ClientSize      = new Size(360, 200);
            Font            = new Font("Segoe UI", 9.5f);
            BackColor       = Color.FromArgb(245, 245, 245);

            int lx = 12, lw = 90, tx = 108, tw = 236, rowH = 34, y = 16;
            AddLabel("Name:",      lx, y + 2, lw); _txtName = AddTB(tx, y, tw); y += rowH;
            AddLabel("Remote ID:", lx, y + 2, lw); _txtId   = AddTB(tx, y, tw); y += rowH;
            AddLabel("Password:",  lx, y + 2, lw); _txtPwd  = AddTB(tx, y, tw);
            _txtPwd.UseSystemPasswordChar = true;

            y += 28;
            var chk = new CheckBox { Text = "Show password", Location = new Point(tx, y), AutoSize = true,
                ForeColor = Color.FromArgb(90, 90, 90) };
            chk.CheckedChanged += delegate { _txtPwd.UseSystemPasswordChar = !chk.Checked; };
            Controls.Add(chk);

            y += 30;
            var btnOk = new Button { Text = "Save", DialogResult = DialogResult.OK,
                Location = new Point(tx + tw - 170, y), Size = new Size(80, 28),
                BackColor = Color.FromArgb(0, 120, 212), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel,
                Location = new Point(tx + tw - 84, y), Size = new Size(80, 28), FlatStyle = FlatStyle.Flat };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(180,180,180);

            Controls.AddRange(new Control[] { btnOk, btnCancel });
            AcceptButton = btnOk; CancelButton = btnCancel;
            ClientSize   = new Size(360, y + 44);

            if (existing != null)
            { _txtName.Text = existing.Name; _txtId.Text = existing.Id; _txtPwd.Text = existing.Password; }

            btnOk.Click += delegate
            {
                if (string.IsNullOrWhiteSpace(_txtId.Text))
                {
                    MessageBox.Show("Remote ID is required.", "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None; return;
                }
                Result = new Contact { Name = _txtName.Text.Trim(),
                    Id = _txtId.Text.Trim(), Password = _txtPwd.Text };
            };
        }

        private void AddLabel(string t, int x, int y, int w) {
            Controls.Add(new Label { Text = t, Location = new Point(x, y),
                Size = new Size(w, 22), TextAlign = ContentAlignment.MiddleRight });
        }
        private TextBox AddTB(int x, int y, int w) {
            var tb = new TextBox { Location = new Point(x, y), Size = new Size(w, 24),
                BorderStyle = BorderStyle.FixedSingle };
            Controls.Add(tb); return tb;
        }
    }

    // ──────────────────────────────────────────────────────────
    //  ContactListView — viewport-sized owner-drawn panel.
    //  Contains its own VScrollBar so Height never exceeds the
    //  Win32 32767px window limit (would clip rows 745+ at 44px/row).
    // ──────────────────────────────────────────────────────────
    public class ContactListView : Panel
    {
        private const int RowH = 44;

        private List<Contact>    _contacts    = new List<Contact>();
        private int              _selectedIdx = -1;
        private int              _hoverIdx    = -1;
        private int              _firstRow    = 0;   // index of top visible row
        private Func<int, int[]> _getCols;
        private VScrollBar       _scroll;

        private readonly Color _odd, _even, _hover, _border, _text, _muted;
        private static readonly Color ColSel       = Color.FromArgb(210, 232, 255);
        private static readonly Color ColSelBorder = Color.FromArgb(0, 120, 212);

        public event Action<Contact> SelectionChanged;
        public event Action<Contact> RowDoubleClicked;

        public Contact SelectedContact
        {
            get
            {
                return (_selectedIdx >= 0 && _selectedIdx < _contacts.Count)
                    ? _contacts[_selectedIdx] : null;
            }
        }

        public int SelectedIndex { get { return _selectedIdx; } }

        public ContactListView(Func<int, int[]> getCols,
            Color odd, Color even, Color hover, Color border, Color text, Color muted)
        {
            _getCols = getCols;
            _odd=odd; _even=even; _hover=hover; _border=border; _text=text; _muted=muted;

            DoubleBuffered = true;
            Cursor         = Cursors.Hand;

            _scroll = new VScrollBar
            {
                Dock        = DockStyle.Right,
                SmallChange = 3,
                LargeChange = 10,
                Minimum     = 0,
                Maximum     = 0,
                Visible     = false
            };
            _scroll.ValueChanged += delegate
            {
                _firstRow = _scroll.Value;
                Invalidate();
            };
            Controls.Add(_scroll);
        }

        // ── Public API ────────────────────────────────────────
        public void SetContacts(List<Contact> contacts)
        {
            _contacts    = contacts ?? new List<Contact>();
            _selectedIdx = -1;
            _hoverIdx    = -1;
            _firstRow    = 0;
            UpdateScrollBar();
            Invalidate();
        }

        public void ClearSelection()
        {
            if (_selectedIdx < 0) return;
            int old = _selectedIdx; _selectedIdx = -1;
            InvalidateRow(old);
            if (SelectionChanged != null) SelectionChanged(null);
        }

        // ── Layout ────────────────────────────────────────────
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            UpdateScrollBar();
            Invalidate();
        }

        private void UpdateScrollBar()
        {
            // Use floor division so the max scroll position always shows complete rows —
            // ceiling would allow the last row to be partially clipped at the bottom.
            int fullVisible = Math.Max(1, Height / RowH);
            int maxFirst    = Math.Max(0, _contacts.Count - fullVisible);

            if (_contacts.Count <= fullVisible)
            {
                _scroll.Visible = false;
                _firstRow       = 0;
            }
            else
            {
                // WinForms VScrollBar: effective max position = Maximum - LargeChange + 1
                // We want that to equal maxFirst, so: Maximum = maxFirst + LargeChange - 1
                _scroll.LargeChange = Math.Max(1, fullVisible);
                _scroll.SmallChange = Math.Max(1, fullVisible / 5);
                _scroll.Minimum     = 0;
                _scroll.Maximum     = maxFirst + _scroll.LargeChange - 1;
                _scroll.Visible     = true;
                if (_firstRow > maxFirst) _firstRow = maxFirst;
                _scroll.Value = _firstRow;
            }
        }

        private int VisibleRows()
        {
            return Math.Max(1, (Height + RowH - 1) / RowH);
        }

        private int DrawWidth()
        {
            return _scroll.Visible ? Width - _scroll.Width : Width;
        }

        // ── Mouse ─────────────────────────────────────────────
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (!_scroll.Visible) return;
            int lines = -(e.Delta / 120) * _scroll.SmallChange;
            int nv    = Math.Max(0, Math.Min(_scroll.Maximum - _scroll.LargeChange + 1,
                                             _firstRow + lines));
            _scroll.Value = nv;
            base.OnMouseWheel(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int idx = HitRow(e.Y);
            if (idx != _hoverIdx)
            {
                int old = _hoverIdx; _hoverIdx = idx;
                InvalidateRow(old); InvalidateRow(idx);
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            int old = _hoverIdx; _hoverIdx = -1;
            InvalidateRow(old);
            base.OnMouseLeave(e);
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            int idx = HitRow(e.Y);
            if (idx < 0) return;
            int old = _selectedIdx; _selectedIdx = idx;
            InvalidateRow(old); InvalidateRow(idx);
            if (SelectionChanged != null) SelectionChanged(_contacts[idx]);
            base.OnMouseClick(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            int idx = HitRow(e.Y);
            if (idx >= 0 && RowDoubleClicked != null)
                RowDoubleClicked(_contacts[idx]);
            base.OnMouseDoubleClick(e);
        }

        private int HitRow(int mouseY)
        {
            int i = _firstRow + mouseY / RowH;
            return (i >= 0 && i < _contacts.Count && mouseY >= 0) ? i : -1;
        }

        // ── Paint ─────────────────────────────────────────────
        protected override void OnPaint(PaintEventArgs e)
        {
            var g     = e.Graphics;
            int dw    = DrawWidth();
            int clip0 = e.ClipRectangle.Top;
            int clip1 = e.ClipRectangle.Bottom;

            // Rows that intersect the clip rectangle
            int first = _firstRow + clip0 / RowH;
            int last  = _firstRow + clip1 / RowH;
            first = Math.Max(_firstRow, first);
            last  = Math.Min(_contacts.Count - 1, last);

            for (int i = first; i <= last; i++)
            {
                int drawY = (i - _firstRow) * RowH;
                DrawRow(g, drawY, i, _contacts[i], dw);
            }

            // Fill background below last row
            int contentBottom = (_contacts.Count - _firstRow) * RowH;
            if (contentBottom < Height)
            {
                using (var br = new SolidBrush(BackColor))
                    g.FillRectangle(br, 0, contentBottom, dw, Height - contentBottom);
            }
        }

        private void DrawRow(Graphics g, int drawY, int i, Contact c, int dw)
        {
            var cols = _getCols(dw);

            Color bg = (i == _selectedIdx) ? ColSel
                     : (i == _hoverIdx)    ? _hover
                     : (i % 2 == 0)        ? _odd : _even;
            using (var br = new SolidBrush(bg)) g.FillRectangle(br, 0, drawY, dw, RowH);

            using (var pen = new Pen((i == _selectedIdx) ? ColSelBorder : _border,
                                     (i == _selectedIdx) ? 2f : 1f))
                g.DrawLine(pen, 0, drawY + RowH - 1, dw, drawY + RowH - 1);

            // Row number
            using (var fnt = new Font("Segoe UI", 8f))
            using (var br  = new SolidBrush(_muted))
            using (var sf  = new StringFormat { Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center })
                g.DrawString((i + 1).ToString(), fnt, br,
                    new RectangleF(cols[0], drawY, cols[1] - cols[0], RowH), sf);

            // Avatar
            int avS = 32, avX = cols[1] + 10, avY = drawY + 6;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var br = new SolidBrush(AvatarColor(c.Name + c.Id)))
                g.FillEllipse(br, avX, avY, avS, avS);
            using (var fnt = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var br  = new SolidBrush(Color.White))
            using (var sf  = new StringFormat { Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center })
                g.DrawString(Initials(c.Name, c.Id), fnt, br,
                    new RectangleF(avX, avY, avS, avS), sf);
            g.SmoothingMode = SmoothingMode.Default;

            // Text columns
            using (var sfT = new StringFormat { LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
            using (var mF  = new Font("Segoe UI", 9.5f))
            using (var moF = new Font("Consolas", 9f))
            using (var mB  = new SolidBrush(_text))
            using (var muB = new SolidBrush(_muted))
            {
                int pad = 8;
                string dn     = string.IsNullOrWhiteSpace(c.Name) ? "(no name)" : c.Name;
                string masked = string.IsNullOrWhiteSpace(c.Password) ? "-" : "........";
                g.DrawString(dn,     mF,  mB,  new RectangleF(cols[2]+pad, drawY, cols[3]-cols[2]-pad*2, RowH), sfT);
                g.DrawString(c.Id,   moF, mB,  new RectangleF(cols[3]+pad, drawY, cols[4]-cols[3]-pad*2, RowH), sfT);
                g.DrawString(masked, mF,  muB, new RectangleF(cols[4]+pad, drawY, cols[5]-cols[4]-pad*2, RowH), sfT);
            }
        }

        private void InvalidateRow(int i)
        {
            if (i < _firstRow || i > _firstRow + VisibleRows()) return;
            int drawY = (i - _firstRow) * RowH;
            Invalidate(new Rectangle(0, drawY, DrawWidth(), RowH));
        }

        private static string Initials(string name, string id)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                var parts = name.Trim().Split(new char[]{' '});
                if (parts.Length >= 2) return ("" + parts[0][0] + parts[parts.Length-1][0]).ToUpper();
                return name.Substring(0, Math.Min(2, name.Length)).ToUpper();
            }
            return id.Length >= 2 ? id.Substring(0, 2).ToUpper() : "?";
        }

        private static Color AvatarColor(string seed)
        {
            Color[] pal = { Color.FromArgb(24,100,171), Color.FromArgb(16,124,100),
                Color.FromArgb(135,54,93),  Color.FromArgb(175,85,0),
                Color.FromArgb(68,83,153),  Color.FromArgb(196,43,28),
                Color.FromArgb(67,116,73) };
            int h = 0; foreach (char c in seed) h = h * 31 + c;
            return pal[Math.Abs(h) % pal.Length];
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Sidebar platform button
    // ──────────────────────────────────────────────────────────
    public class SidebarButton : Panel
    {
        public Platform Platform { get; private set; }
        public event Action<Platform> Selected;
        public event Action<Platform> EditClicked;
        public event Action<Platform> ExportClicked;
        public event Action<Platform> RemoveClicked;
        public event Action<Platform> DeleteClicked;

        // Set externally when the cache is populated so the badge updates
        // (-1 = not yet loaded, badge hidden)
        private int _contactCount = -1;
        public int ContactCount
        {
            get { return _contactCount; }
            set { _contactCount = value; }
        }

        private static readonly Color ColNormal    = Color.FromArgb(30, 30, 30);
        private static readonly Color ColHover     = Color.FromArgb(50, 50, 50);
        private static readonly Color ColActive    = Color.FromArgb(0, 95, 170);
        private static readonly Color ColText      = Color.FromArgb(220, 220, 220);
        private static readonly Color ColTextSel   = Color.White;
        private static readonly Color ColAccentBar = Color.FromArgb(0, 120, 212);
        private static readonly Color ColBadge     = Color.FromArgb(0, 140, 100);

        private const int RowHeight = 44;
        private const int BtnW  = 18;
        private const int BtnH  = 20;
        private const int BtnGap = 2;
        private const int BtnRight = 5;

        private bool   _hover;
        private bool   _isSelected;
        private Button _btnEdit, _btnExport, _btnRemove, _btnDelete;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected          = value;
                _btnEdit.Visible     = value;
                _btnExport.Visible   = value;
                _btnRemove.Visible   = value;
                _btnDelete.Visible   = value;
                Invalidate();
            }
        }

        public SidebarButton(Platform p)
        {
            Platform       = p;
            Height         = RowHeight;
            Dock           = DockStyle.Top;
            Cursor         = Cursors.Hand;
            DoubleBuffered = true;
            TabStop        = false;

            _btnEdit   = MakeActionBtn("✎", Color.FromArgb(65, 65, 70));
            _btnExport = MakeActionBtn("↑", Color.FromArgb(65, 65, 70));
            _btnRemove = MakeActionBtn("–", Color.FromArgb(100, 80, 20));
            _btnDelete = MakeActionBtn("✕", Color.FromArgb(160, 35, 20));

            var tip = new ToolTip();
            tip.SetToolTip(_btnEdit,   "Edit platform");
            tip.SetToolTip(_btnExport, "Export platform");
            tip.SetToolTip(_btnRemove, "Remove from list (keep files)");
            tip.SetToolTip(_btnDelete, "Delete platform files permanently");

            _btnEdit.Click   += delegate { if (EditClicked   != null) EditClicked(Platform); };
            _btnExport.Click += delegate { if (ExportClicked != null) ExportClicked(Platform); };
            _btnRemove.Click += delegate { if (RemoveClicked != null) RemoveClicked(Platform); };
            _btnDelete.Click += delegate { if (DeleteClicked != null) DeleteClicked(Platform); };

            Controls.AddRange(new Control[] { _btnEdit, _btnExport, _btnRemove, _btnDelete });

            MouseEnter += delegate { _hover = true;  Invalidate(); };
            MouseLeave += delegate
            {
                if (!ClientRectangle.Contains(PointToClient(Cursor.Position)))
                { _hover = false; Invalidate(); }
            };
            MouseClick += delegate(object s, MouseEventArgs e)
            {
                if (Selected != null) Selected(Platform);
            };
            Paint  += OnPaint;
            Resize += delegate { PositionActionButtons(); };
        }

        public void UpdatePlatform(Platform p) { Platform = p; Invalidate(); }

        private void PositionActionButtons()
        {
            int totalBtnsW = BtnW * 4 + BtnGap * 3;
            int x = Width - BtnRight - totalBtnsW;
            int y = (RowHeight - BtnH) / 2;
            _btnEdit.SetBounds  (x,                       y, BtnW, BtnH);
            _btnExport.SetBounds(x +  (BtnW+BtnGap),      y, BtnW, BtnH);
            _btnRemove.SetBounds(x + 2*(BtnW+BtnGap),     y, BtnW, BtnH);
            _btnDelete.SetBounds(x + 3*(BtnW+BtnGap),     y, BtnW, BtnH);
        }

        private void OnPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            Color bg = _isSelected ? ColActive : (_hover ? ColHover : ColNormal);
            g.Clear(bg);

            if (_isSelected)
                using (var br = new SolidBrush(ColAccentBar))
                    g.FillRectangle(br, 0, 0, 4, Height);

            // Avatar circle
            string init = Platform.Name.Length > 0 ? Platform.Name.Substring(0, 1).ToUpper() : "?";
            Color avC   = AvatarColor(Platform.Name);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (var br = new SolidBrush(avC)) g.FillEllipse(br, 10, 10, 24, 24);
            using (var fnt = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (var br  = new SolidBrush(Color.White))
            using (var sf  = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                g.DrawString(init, fnt, br, new RectangleF(10, 10, 24, 24), sf);
            g.SmoothingMode = SmoothingMode.Default;

            // Platform name — shrinks to leave room for buttons (and badge when selected)
            int btnAreaW  = _isSelected ? BtnRight + BtnW * 4 + BtnGap * 3 + 4 : 6;
            int nameRight = Width - btnAreaW - (_isSelected && ContactCount >= 0 ? 40 : 0);
            nameRight     = Math.Max(42, nameRight);
            using (var fnt = new Font("Segoe UI", 9.5f, _isSelected ? FontStyle.Bold : FontStyle.Regular))
            using (var br  = new SolidBrush(_isSelected ? ColTextSel : ColText))
            using (var sf  = new StringFormat { LineAlignment = StringAlignment.Center,
                Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap })
                g.DrawString(Platform.Name, fnt, br,
                    new RectangleF(42, 0, nameRight - 42, RowHeight), sf);

            // Contact count badge — shown when count is known (selected or not)
            if (ContactCount >= 0)
            {
                string badge = ContactCount.ToString();
                using (var fnt = new Font("Segoe UI", 7.5f, FontStyle.Bold))
                {
                    var sz  = TextRenderer.MeasureText(badge, fnt);
                    int bw  = sz.Width + 8;
                    int bh  = 16;
                    // When selected, offset left of the action buttons
                    int rightEdge = _isSelected
                        ? Width - BtnRight - (BtnW * 4 + BtnGap * 3) - 4
                        : Width - 6;
                    int bx  = rightEdge - bw;
                    int by  = (RowHeight - bh) / 2;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var br = new SolidBrush(ColBadge))
                        g.FillRectangle(br, bx, by, bw, bh);
                    g.SmoothingMode = SmoothingMode.Default;
                    using (var br = new SolidBrush(Color.White))
                    using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                        g.DrawString(badge, fnt, br, new RectangleF(bx, by, bw, bh), sf);
                }
            }

            PositionActionButtons();
        }

        private Button MakeActionBtn(string symbol, Color back)
        {
            var b = new Button
            {
                Text      = symbol,
                BackColor = back,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(BtnW, BtnH),
                Cursor    = Cursors.Hand,
                Font      = new Font("Segoe UI", 8.5f),
                Visible   = false,
                TabStop   = false   // never steal keyboard focus from the contact list
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static Color AvatarColor(string seed)
        {
            Color[] pal = new Color[]
            {
                Color.FromArgb(24,100,171), Color.FromArgb(16,124,100),
                Color.FromArgb(135,54,93),  Color.FromArgb(175,85,0),
                Color.FromArgb(68,83,153),  Color.FromArgb(196,43,28),
                Color.FromArgb(67,116,73)
            };
            int h = 0; foreach (char c in seed) h = h * 31 + c;
            return pal[Math.Abs(h) % pal.Length];
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Main form
    // ──────────────────────────────────────────────────────────
    public class MainForm : Form
    {
        private static readonly Color ColBg        = Color.FromArgb(250, 250, 250);
        private static readonly Color ColSidebar   = Color.FromArgb(30,  30,  30);
        private static readonly Color ColAccent    = Color.FromArgb(0,  120, 212);
        private static readonly Color ColDanger    = Color.FromArgb(196,  43,  28);
        private static readonly Color ColRowOdd    = Color.White;
        private static readonly Color ColRowEven   = Color.FromArgb(244, 247, 251);
        private static readonly Color ColRowHover  = Color.FromArgb(224, 238, 253);
        private static readonly Color ColRowBorder = Color.FromArgb(220, 220, 225);
        private static readonly Color ColText      = Color.FromArgb( 30,  30,  30);
        private static readonly Color ColMuted     = Color.FromArgb(100, 100, 100);

        private List<Platform> _platforms = new List<Platform>();
        private Platform       _selected;
        private List<Contact>  _contacts  = new List<Contact>();
        private List<Contact>  _filtered  = new List<Contact>();
        private string         _search    = "";

        // ── Contact cache ─────────────────────────────────────
        private readonly Dictionary<string, List<Contact>> _contactCache
            = new Dictionary<string, List<Contact>>(StringComparer.OrdinalIgnoreCase);

        // ── Row panel cache ───────────────────────────────────
        // No longer needed — ContactListView is a single panel, switching is instant.

        // ── Async load ────────────────────────────────────────
        private System.Threading.CancellationTokenSource _loadCts;

        // Row height — matches ContactListView row height
        private const int VRowH = 44;

        private Panel   _sidebar;
        private Panel   _sidebarScroll;
        private Panel   _sidebarFooter;
        private Panel   _main;
        private Panel   _topBar;
        private Panel   _contactActionBar;
        private Label   _lblPlatformTitle;
        private Label   _lblExeInfo;
        private ToolTip _exeTip;
        private Label   _lblActionContact;
        private TextBox _txtSearch;
        private Button  _btnAddContact, _btnImport, _btnExport, _btnReload;
        private Button  _btnConnect, _btnEditContact, _btnDeleteContact;
        private Panel   _colHeader;
        private Panel   _listPanel;
        private ContactListView _contactListView;
        private Label   _lblEmpty;
        private Label   _lblLoading;
        private Label   _lblNoPlat;

        // ── Contact selection state ───────────────────────────
        private Contact    _selectedContact;
        private StatusStrip          _status;
        private ToolStripStatusLabel _lblStatus;

        // FIX #4: track placeholder state explicitly instead of relying on ForeColor
        private bool _searchIsPlaceholder = true;

        public MainForm()
        {
            SuspendLayout();
            BuildForm();
            ResumeLayout();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Global hotkeys — only active when a contact is selected
            if (_selectedContact != null)
            {
                if (keyData == Keys.Return)
                {
                    OnConnect(_selectedContact);
                    return true;
                }
                if (keyData == Keys.Delete)
                {
                    OnDelete(_selectedContact);
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void BuildForm()
        {
            Text          = "Multi Remote Tool";
            MinimumSize   = new Size(820, 500);
            Size          = new Size(1020, 620);
            StartPosition = FormStartPosition.CenterScreen;
            Font          = new Font("Segoe UI", 9.5f);
            BackColor     = ColBg;

            // Load icon from app folder — shows in title bar, taskbar, and Alt+Tab
            string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
            if (File.Exists(icoPath))
                try { this.Icon = new Icon(icoPath); } catch { }

            // ── Sidebar ───────────────────────────────────────
            _sidebar = new Panel { Dock = DockStyle.Left, Width = 240, BackColor = ColSidebar };

            var sidebarHeader = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(18, 18, 18) };
            var lblApp = new Label
            {
                Text      = "Remote Platform",
                ForeColor = Color.FromArgb(0, 150, 255),
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                Padding   = new Padding(12, 0, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft
            };
            sidebarHeader.Controls.Add(lblApp);

            _sidebarScroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = ColSidebar };

            // ── Sidebar footer (Add / Import platform) ────────
            _sidebarFooter = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.FromArgb(22, 22, 22) };
            _sidebarFooter.Paint += delegate(object s, PaintEventArgs e)
            {
                using (var pen = new Pen(Color.FromArgb(45, 45, 45)))
                    e.Graphics.DrawLine(pen, 0, 0, _sidebarFooter.Width, 0);
            };

            var btnImportPlat = new Button
            {
                Text      = "↓  Import Platform",
                Dock      = DockStyle.Top,
                Height    = 38,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(150, 150, 150),
                Font      = new Font("Segoe UI", 9f),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            btnImportPlat.FlatAppearance.BorderSize = 0;
            btnImportPlat.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 40);
            btnImportPlat.Click += delegate { ImportPlatform(); };

            var btnAddPlat = new Button
            {
                Text      = "+  Add Platform",
                Dock      = DockStyle.Bottom,
                Height    = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Color.FromArgb(0, 170, 255),
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                TabStop   = false
            };
            btnAddPlat.FlatAppearance.BorderSize = 0;
            btnAddPlat.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 40, 40);
            btnAddPlat.Click += delegate { AddPlatform(); };

            _sidebarFooter.Controls.Add(btnImportPlat);
            _sidebarFooter.Controls.Add(btnAddPlat);

            _sidebar.Controls.Add(_sidebarScroll);
            _sidebar.Controls.Add(_sidebarFooter);
            _sidebar.Controls.Add(sidebarHeader);

            // ── Splitter ──────────────────────────────────────
            var splitter = new Splitter { Dock = DockStyle.Left, Width = 3,
                BackColor = Color.FromArgb(50, 50, 50), MinSize = 150, MinExtra = 400 };

            // ── Main area ─────────────────────────────────────
            _main = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };

            _topBar = new Panel { Dock = DockStyle.Top, Height = 56,
                BackColor = Color.FromArgb(240, 242, 246), Padding = new Padding(12, 0, 12, 0) };
            _topBar.Paint += delegate(object s, PaintEventArgs e)
            {
                using (var p = new Pen(Color.FromArgb(210,210,215)))
                    e.Graphics.DrawLine(p, 0, _topBar.Height-1, _topBar.Width, _topBar.Height-1);
            };

            _lblPlatformTitle = new Label { Text = "Select a platform",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold), ForeColor = ColText,
                AutoSize = false, Size = new Size(260, 28), Location = new Point(12, 6),
                TextAlign = ContentAlignment.MiddleLeft };
            _lblExeInfo = new Label { Text = "", Font = new Font("Segoe UI", 8.5f, FontStyle.Italic), ForeColor = ColMuted,
                AutoSize = false, Size = new Size(400, 18), Location = new Point(12, 34),
                TextAlign = ContentAlignment.MiddleLeft };
            var exeTip = new ToolTip();
            exeTip.SetToolTip(_lblExeInfo, "Full path shown in status bar");
            _exeTip = exeTip;

            // FIX #4: use explicit _searchIsPlaceholder flag
            _txtSearch = new TextBox { Size = new Size(200, 24), BorderStyle = BorderStyle.FixedSingle,
                ForeColor = Color.FromArgb(140, 140, 140), Text = "Search contacts..." };
            _txtSearch.GotFocus += delegate
            {
                if (_searchIsPlaceholder)
                {
                    _searchIsPlaceholder = false;
                    _txtSearch.Text      = "";
                    _txtSearch.ForeColor = Color.FromArgb(30, 30, 30);
                }
            };
            _txtSearch.LostFocus += delegate
            {
                if (string.IsNullOrWhiteSpace(_txtSearch.Text))
                    ResetSearchPlaceholder();
            };
            _txtSearch.TextChanged += delegate
            {
                if (_searchIsPlaceholder) return;
                _search = _txtSearch.Text;
                DoRefresh();
            };

            _btnAddContact = MakeTopBtn("+ Add Contact", ColAccent, Color.White);
            _btnReload     = MakeTopBtn("⟳ Reload", Color.FromArgb(80,80,84), Color.White);
            _btnImport     = MakeTopBtn("Import", Color.FromArgb(80,80,84), Color.White);
            _btnExport     = MakeTopBtn("Export", Color.FromArgb(80,80,84), Color.White);

            _btnAddContact.Click += delegate { AddContact(); };
            _btnReload.Click     += delegate { ReloadContacts(); };
            _btnImport.Click     += delegate { ImportCsv(); };
            _btnExport.Click     += delegate { ExportCsv(); };

            // Platform action buttons are created above in the sidebar action bar section

            _topBar.Controls.AddRange(new Control[]
            {
                _lblPlatformTitle, _lblExeInfo,
                _txtSearch, _btnAddContact, _btnReload, _btnImport, _btnExport
            });
            _topBar.Resize += delegate { LayoutTopBar(); };

            _colHeader = new Panel { Dock = DockStyle.Top, Height = 28, BackColor = Color.FromArgb(235, 235, 238) };
            _colHeader.Paint += DrawColHeader;

            _listPanel = new Panel { Dock = DockStyle.Fill, BackColor = ColBg };

            // Status labels: absolutely positioned overlays — Dock.Fill breaks AutoScroll panels
            // because multiple Fill controls collapse each other.  We resize them manually.
            _lblEmpty = new Label
            {
                Text      = "No contacts yet. Click \"+ Add Contact\" to create one.",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = ColMuted,
                Font      = new Font("Segoe UI", 10f),
                Visible   = false
            };
            _lblLoading = new Label
            {
                Text      = "Loading contacts...",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = ColAccent,
                Font      = new Font("Segoe UI", 10f, FontStyle.Italic),
                Visible   = false
            };
            _lblNoPlat = new Label
            {
                Text      = "No platforms yet.\n\nClick \"+ Add Platform\" in the sidebar to get started.",
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = ColMuted,
                Font      = new Font("Segoe UI", 11f),
                Visible   = true
            };
            _listPanel.Controls.AddRange(new Control[] { _lblEmpty, _lblLoading, _lblNoPlat });

            _contactListView = new ContactListView(GetColumnWidths,
                ColRowOdd, ColRowEven, ColRowHover, ColRowBorder, ColText, ColMuted)
            { Dock = DockStyle.Fill };
            _contactListView.SelectionChanged += OnRowClicked;
            _contactListView.RowDoubleClicked += delegate(Contact c) { if (c != null) OnConnect(c); };
            _listPanel.Controls.Add(_contactListView);

            _listPanel.Resize += delegate { PositionOverlayLabels(); };
            PositionOverlayLabels();

            // ── Contact action bar ────────────────────────────
            // Sits between column header and list. Hidden until a row is selected.
            _contactActionBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 38,
                BackColor = Color.FromArgb(228, 241, 255),
                Visible   = false
            };
            _contactActionBar.Paint += delegate(object s, PaintEventArgs e)
            {
                using (var p = new Pen(Color.FromArgb(180, 210, 245)))
                    e.Graphics.DrawLine(p, 0, _contactActionBar.Height - 1,
                        _contactActionBar.Width, _contactActionBar.Height - 1);
            };
            _lblActionContact = new Label
            {
                AutoSize  = false, Location = new Point(10, 0),
                Size      = new Size(320, 38),
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 80, 160),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _btnConnect = MakeActionBarBtn("Connect", ColAccent,  Color.White);
            _btnEditContact   = MakeActionBarBtn("Edit",    Color.FromArgb(80, 80, 84), Color.White);
            _btnDeleteContact = MakeActionBarBtn("Delete",  ColDanger, Color.White);
            _btnConnect.Click       += delegate { if (_selectedContact != null) OnConnect(_selectedContact); };
            _btnEditContact.Click   += delegate { if (_selectedContact != null) OnEdit(_selectedContact); };
            _btnDeleteContact.Click += delegate { if (_selectedContact != null) OnDelete(_selectedContact); };
            _contactActionBar.Controls.AddRange(new Control[]
                { _lblActionContact, _btnConnect, _btnEditContact, _btnDeleteContact });
            _contactActionBar.Resize += delegate { LayoutActionBar(); };

            _status    = new StatusStrip { BackColor = Color.FromArgb(230, 230, 233) };
            _lblStatus = new ToolStripStatusLabel("Ready") { ForeColor = ColMuted };
            _status.Items.Add(_lblStatus);

            _main.Controls.Add(_listPanel);
            _main.Controls.Add(_contactActionBar);
            _main.Controls.Add(_colHeader);
            _main.Controls.Add(_topBar);

            Controls.Add(_main);
            Controls.Add(splitter);
            Controls.Add(_sidebar);
            Controls.Add(_status);

            LayoutTopBar();
            SetPlatformSelected(false);

            // Load contacts AFTER the form is shown so the message loop is running
            // and async continuations have a valid synchronization context
            Shown      += delegate { LoadPlatforms(); };
            FormClosing += delegate { SaveAllSettings(); };
        }

        private Button MakeActionBarBtn(string text, Color back, Color fore)
        {
            var b = new Button { Text = text, BackColor = back, ForeColor = fore,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5f), Size = new Size(74, 26) };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void LayoutActionBar()
        {
            int right = _contactActionBar.ClientSize.Width - 8;
            int y     = (_contactActionBar.Height - _btnConnect.Height) / 2;
            _btnDeleteContact.Location = new Point(right - _btnDeleteContact.Width, y);
            _btnEditContact.Location   = new Point(_btnDeleteContact.Left - _btnEditContact.Width - 6, y);
            _btnConnect.Location       = new Point(_btnEditContact.Left - _btnConnect.Width - 6, y);
            _lblActionContact.Width    = _btnConnect.Left - 14;
        }

        // FIX #4: centralised helper — resets search box to placeholder state
        private void PositionOverlayLabels()
        {
            var r = new Rectangle(0, 0, _listPanel.ClientSize.Width, _listPanel.ClientSize.Height);
            foreach (var lbl in new[] { _lblEmpty, _lblLoading, _lblNoPlat })
                lbl.SetBounds(r.X, r.Y, r.Width, r.Height);
        }

        private void ResetSearchPlaceholder()
        {
            _searchIsPlaceholder = true;
            _search              = "";
            _txtSearch.ForeColor = Color.FromArgb(140, 140, 140);
            _txtSearch.Text      = "Search contacts...";
        }

        private void LayoutTopBar()
        {
            int right = _topBar.ClientSize.Width - 12;

            _btnExport.Location     = new Point(right - _btnExport.Width, 14);
            _btnImport.Location     = new Point(_btnExport.Left - _btnImport.Width - 6, 14);
            _btnReload.Location     = new Point(_btnImport.Left - _btnReload.Width - 6, 14);
            _btnAddContact.Location = new Point(_btnReload.Left - _btnAddContact.Width - 6, 14);
            _txtSearch.Location     = new Point(_btnAddContact.Left - _txtSearch.Width - 14, 16);

            int titleWidth = Math.Max(60, _txtSearch.Left - 10 - 12);
            _lblPlatformTitle.Size = new Size(titleWidth, 28);
            _lblExeInfo.Size       = new Size(titleWidth, 18);
        }

        private Button MakeTopBtn(string text, Color back, Color fore)
        {
            var font = new Font("Segoe UI", 8.5f);
            // Measure actual text width so nothing gets clipped; add 20px horizontal padding
            int w = TextRenderer.MeasureText(text, font).Width + 20;
            var b = new Button { Text = text, BackColor = back, ForeColor = fore,
                FlatStyle = FlatStyle.Flat,
                Size      = new Size(w, 26),
                Cursor    = Cursors.Hand,
                Font      = font };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void SetPlatformSelected(bool yes)
        {
            _colHeader.Visible     = yes;
            _btnAddContact.Visible = yes;
            _btnReload.Visible     = yes;
            _btnImport.Visible     = yes;
            _btnExport.Visible     = yes;
            _txtSearch.Visible     = yes;
            if (!yes) { _contactActionBar.Visible = false; ClearSelection(); }

            _lblNoPlat.Visible = !yes;
            if (!yes && _platforms.Count > 0)
                _lblNoPlat.Text = "← Select a platform from the sidebar.";
            else if (!yes)
                _lblNoPlat.Text = "No platforms yet.\n\nClick \"+ Add Platform\" in the sidebar to get started.";
        }

        private void ClearSelection()
        {
            _selectedContact = null;
            _contactListView.ClearSelection();
            _contactActionBar.Visible = false;
        }

        private void OnRowClicked(Contact c)
        {
            if (c == null) { ClearSelection(); return; }
            _selectedContact = c;
            string name = string.IsNullOrWhiteSpace(c.Name) ? "(no name)" : c.Name;
            int rowNum = _contactListView.SelectedIndex + 1;
            _lblActionContact.Text    = rowNum + ".  " + name + "   " + c.Id;
            _contactActionBar.Visible = true;
            LayoutActionBar();
        }

        // ── Platform sidebar ──────────────────────────────────
        private void LoadPlatforms()
        {
            string loadedRaw = LoadSetting("LoadedPlatforms");
            var allOnDisk    = Platform.LoadAll();

            if (string.IsNullOrWhiteSpace(loadedRaw))
            {
                // First run: show everything found on disk
                _platforms = allOnDisk;
            }
            else
            {
                // Preserve the saved order — first entry is the platform to auto-select
                var whitelist = loadedRaw.Split(new char[]{','})
                    .Select(n => n.Trim())
                    .Where(n => n.Length > 0)
                    .ToList();
                _platforms = whitelist
                    .Select(name => allOnDisk.FirstOrDefault(p =>
                        p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    .Where(p => p != null)
                    .ToList();
            }

            RebuildSidebar();
            if (_platforms.Count > 0)
                SelectPlatform(_platforms[0]);   // first entry = last selected
            PreloadAllPlatforms();
        }

        private static string SettingsPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.txt");
        }

        // Read a single key from settings.txt
        private static string LoadSetting(string key)
        {
            try
            {
                if (!File.Exists(SettingsPath())) return "";
                foreach (var line in File.ReadAllLines(SettingsPath(), Encoding.UTF8))
                {
                    var t = line.Trim();
                    if (t.StartsWith(key + "="))
                        return t.Substring(key.Length + 1).Trim();
                }
            }
            catch { }
            return "";
        }

        // Write all settings atomically
        private void SaveAllSettings()
        {
            try
            {
                // Selected platform is saved as the FIRST entry in LoadedPlatforms
                // so it is automatically restored on next startup without a separate key.
                var ordered = new List<string>();
                if (_selected != null) ordered.Add(_selected.Name);
                foreach (var p in _platforms)
                    if (!ordered.Contains(p.Name)) ordered.Add(p.Name);

                File.WriteAllText(SettingsPath(),
                    "LoadedPlatforms=" + string.Join(",", ordered) + Environment.NewLine,
                    Encoding.UTF8);
            }
            catch { }
        }

        // Read previously visited platforms and warm the cache for each
        private void PreloadLoadedPlatforms()
        {
            string raw = LoadSetting("LoadedPlatforms");
            if (string.IsNullOrWhiteSpace(raw)) return;
            var names = raw.Split(new char[]{','});
            foreach (var name in names)
            {
                var n    = name.Trim();
                var plat = _platforms.FirstOrDefault(p =>
                    p.Name.Equals(n, StringComparison.OrdinalIgnoreCase));
                if (plat == null || _contactCache.ContainsKey(plat.Name)) continue;
                var capture = plat;
                System.Threading.Tasks.Task.Run(() =>
                {
                    if (!_contactCache.ContainsKey(capture.Name))
                        _contactCache[capture.Name] = CsvStore.Load(capture.CsvPath());
                });
            }
        }

        private void PreloadAllPlatforms()
        {
            PreloadLoadedPlatforms();
            foreach (var p in _platforms)
            {
                var plat = p;
                System.Threading.Tasks.Task.Run(() =>
                {
                    if (!_contactCache.ContainsKey(plat.Name))
                        _contactCache[plat.Name] = CsvStore.Load(plat.CsvPath());
                }).ContinueWith(t =>
                {
                    // Marshal badge update back to UI thread
                    if (IsHandleCreated)
                        BeginInvoke(new Action(UpdateSidebarBadges));
                });
            }
        }

        private void UpdateSidebarBadges()
        {
            foreach (SidebarButton btn in _sidebarScroll.Controls.OfType<SidebarButton>())
            {
                List<Contact> contacts;
                if (_contactCache.TryGetValue(btn.Platform.Name, out contacts))
                {
                    btn.ContactCount = contacts.Count;
                    btn.Invalidate();
                }
            }
        }

        private void RebuildSidebar()
        {
            var old = _sidebarScroll.Controls.OfType<SidebarButton>().ToList();
            foreach (var b in old) { _sidebarScroll.Controls.Remove(b); b.Dispose(); }

            for (int i = _platforms.Count - 1; i >= 0; i--)
            {
                var p   = _platforms[i];
                var btn = new SidebarButton(p);
                btn.Selected      += SelectPlatform;
                btn.EditClicked   += delegate(Platform pl) { EditPlatform(); };
                btn.ExportClicked += delegate(Platform pl) { ExportPlatform(); };
                btn.RemoveClicked += delegate(Platform pl) { RemovePlatform(); };
                btn.DeleteClicked += delegate(Platform pl) { DeletePlatform(); };
                if (_selected != null && p.Name == _selected.Name)
                    btn.IsSelected = true;
                _sidebarScroll.Controls.Add(btn);
            }
            if (_selected == null)
                SetPlatformSelected(false);
        }

        private void SelectPlatform(Platform p)
        {
            _selected = p;
            SaveAllSettings();
            ClearSelection();
            ResetSearchPlaceholder();

            foreach (SidebarButton btn in _sidebarScroll.Controls.OfType<SidebarButton>())
            {
                btn.IsSelected = (btn.Platform.Name == p.Name);
                btn.Invalidate();
            }

            _lblPlatformTitle.Text = p.Name;
            _lblExeInfo.Text       = p.ExeName;
            _topBar.Refresh();

            SetPlatformSelected(true);
            LoadContactsAsync(p);
            UpdateStatusExe();
            // Move keyboard focus to the list so Enter/Delete hotkeys work immediately
            _contactListView.Focus();
        }

        // ── Contact cache helpers ─────────────────────────────

        private List<Contact> GetCachedContacts(Platform p)
        {
            List<Contact> list;
            if (!_contactCache.TryGetValue(p.Name, out list))
            {
                list = CsvStore.Load(p.CsvPath());
                _contactCache[p.Name] = list;
            }
            return list;
        }

        // Call after any write so the next GetCachedContacts re-reads from disk.
        // Not needed when we mutate _contacts in place (add/edit/delete) because
        // _contacts IS the cached list — but needed after import / rename.
        private void InvalidateCache(string platformName)
        {
            _contactCache.Remove(platformName);
        }

        // ── Async contact loading & rendering ─────────────────
        //
        // LoadContactsAsync  — reads CSV (always completes + caches, even if platform
        //                      is switched mid-read) then calls RenderFilteredAsync.
        //
        // RenderFilteredAsync — applies the current search filter and creates ContactRow
        //                       controls in batches of 50, yielding between each batch.
        //                       Cancelled when the user switches platform or types a new
        //                       search term.
        //
        // DoRefresh          — data is already in memory; just cancels any in-progress
        //                      render and starts a fresh RenderFilteredAsync.

        private async void LoadContactsAsync(Platform p)
        {
            CancelRender();
            var ct = _loadCts.Token;

            // Fast path: data already cached — render instantly, no loading label
            List<Contact> cached;
            if (_contactCache.TryGetValue(p.Name, out cached))
            {
                if (ct.IsCancellationRequested || _selected == null || _selected.Name != p.Name) return;
                _contacts = cached;
                await RenderFilteredAsync(ct, showLoading: false);
                return;
            }

            // Slow path: first visit — read CSV from disk
            _contactListView.SetContacts(null);
            _lblLoading.Text    = "Loading contacts...";
            _lblLoading.Visible = true;
            _lblEmpty.Visible   = false;
            _lblLoading.BringToFront();
            _lblLoading.Refresh();
            _lblStatus.Text = p.Name + " — loading...";

            List<Contact> loaded;
            try
            {
                loaded = await System.Threading.Tasks.Task.Run(
                    () => CsvStore.Load(p.CsvPath()));
            }
            catch (Exception ex)
            {
                if (ct.IsCancellationRequested) return;
                _lblLoading.Visible = false;
                MessageBox.Show("Failed to load contacts:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _contactCache[p.Name] = loaded;
            if (ct.IsCancellationRequested || _selected == null || _selected.Name != p.Name) return;

            _contacts = loaded;
            UpdateSidebarBadges();
            await RenderFilteredAsync(ct, showLoading: true);
        }

        // Cancel any in-progress render and get a fresh token
        private void CancelRender()
        {
            if (_loadCts != null) { _loadCts.Cancel(); _loadCts.Dispose(); }
            _loadCts = new System.Threading.CancellationTokenSource();
        }

        private void ReloadContacts()
        {
            if (_selected == null) return;
            InvalidateCache(_selected.Name);
            LoadContactsAsync(_selected);
            // Badge updates inside LoadContactsAsync after data is loaded
        }

        private void UpdateStatusExe()
        {
            if (_selected == null) return;
            string exePath = FindExe(_selected.ExeName);
            if (exePath != null)
            {
                _lblExeInfo.Text      = Path.GetFileName(exePath) + "  ✓";
                _lblExeInfo.ForeColor = Color.FromArgb(16, 124, 100);
                _exeTip.SetToolTip(_lblExeInfo, exePath);
                _lblStatus.Text       = _selected.Name + " — exe found: " + exePath;
            }
            else
            {
                _lblExeInfo.Text      = _selected.ExeName + "  ✗ not found";
                _lblExeInfo.ForeColor = Color.FromArgb(196, 43, 28);
                _exeTip.SetToolTip(_lblExeInfo, "Not found — check exe name or browse to full path");
                _lblStatus.Text       = _selected.Name + " — exe NOT found: " + _selected.ExeName
                    + "  (install it or place it next to MultiRemoteTool.exe)";
            }
        }

        // FIX #3: correct search order — system PATH first, app folder as fallback
        private string FindExe(string exeName)
        {
            // Absolute path given explicitly
            if (Path.IsPathRooted(exeName) && File.Exists(exeName)) return exeName;

            // 1. System PATH
            string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach (var dir in pathEnv.Split(new char[]{';'}))
            {
                try
                {
                    string full = Path.Combine(dir.Trim(), exeName);
                    if (File.Exists(full)) return full;
                }
                catch { }
            }

            // 2. Common install locations (direct, no subdirectory search)
            string[] common = new string[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),         exeName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),      exeName),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), exeName)
            };
            foreach (var c in common)
                if (File.Exists(c)) return c;

            // 3. App folder (standalone / portable exe dropped next to us)
            string local = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, exeName);
            if (File.Exists(local)) return local;

            return null;
        }

        // ── Platform CRUD ─────────────────────────────────────
        private void AddPlatform()
        {
            using (var dlg = new PlatformDialog(null))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                var p = dlg.Result;
                if (_platforms.Any(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    MessageBox.Show("A platform with that name already exists.", "Duplicate",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                p.SaveIni();
                _platforms.Add(p);
                _platforms.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                RebuildSidebar();
                SelectPlatform(p);
            }
        }

        private void EditPlatform()
        {
            if (_selected == null) return;
            using (var dlg = new PlatformDialog(_selected))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                bool nameChanged = !_selected.Name.Equals(
                    dlg.Result.Name, StringComparison.OrdinalIgnoreCase);

                if (nameChanged)
                {
                    // Guard against colliding with an existing platform
                    if (_platforms.Any(x => x != _selected &&
                        x.Name.Equals(dlg.Result.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("A platform with that name already exists.", "Duplicate",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Remember old file paths before updating the name
                    string oldIni  = _selected.IniPath();
                    string oldCsv  = _selected.CsvPath();
                    string oldName = _selected.Name;

                    // Apply all changes
                    _selected.Name         = dlg.Result.Name;
                    _selected.ExeName      = dlg.Result.ExeName;
                    _selected.CommandLine  = dlg.Result.CommandLine;
                    _selected.BookFileName = dlg.Result.BookFileName;

                    // Save new INI first, THEN delete old
                    _selected.SaveIni();
                    if (!string.Equals(oldIni, _selected.IniPath(), StringComparison.OrdinalIgnoreCase))
                        try { File.Delete(oldIni); } catch { }

                    // Move CSV to match the new name so contacts aren't lost
                    if (File.Exists(oldCsv))
                    {
                        try { File.Move(oldCsv, _selected.CsvPath()); }
                        catch { }
                    }

                    // Cache key was the old name — migrate it to the new name
                    if (_contactCache.ContainsKey(oldName))
                    {
                        _contactCache[_selected.Name] = _contactCache[oldName];
                        _contactCache.Remove(oldName);
                    }
                }
                else
                {
                    _selected.ExeName      = dlg.Result.ExeName;
                    _selected.CommandLine  = dlg.Result.CommandLine;
                    _selected.BookFileName = dlg.Result.BookFileName;
                    _selected.SaveIni();
                }

                _platforms.Sort((a, b) =>
                    string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                RebuildSidebar();
                SelectPlatform(_selected);
            }
        }

        private void RemovePlatform()
        {
            if (_selected == null) return;
            _platforms.Remove(_selected);
            _contactCache.Remove(_selected.Name);
            _selected = null;
            SaveAllSettings();   // LoadedPlatforms no longer contains this platform
            RebuildSidebar();
            SetPlatformSelected(false);
            _lblPlatformTitle.Text = "Select a platform";
            _lblExeInfo.Text       = "";
            _lblExeInfo.ForeColor  = ColMuted;
            ClearList();
            _lblStatus.Text = "Platform removed from list (files kept on disk).";
        }

        private void DeletePlatform()
        {
            if (_selected == null) return;
            var r = MessageBox.Show(
                "Delete platform \"" + _selected.Name + "\"?\n\n" +
                "This will delete its INI file and contacts CSV.",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;
            try { File.Delete(_selected.IniPath()); } catch { }
            try { File.Delete(_selected.CsvPath()); } catch { }
            _platforms.Remove(_selected);
            _selected = null;
            SaveAllSettings();
            RebuildSidebar();
            SetPlatformSelected(false);
            _lblPlatformTitle.Text = "Select a platform";
            _lblExeInfo.Text       = "";
            _lblExeInfo.ForeColor  = ColMuted;
            ClearList();
            _lblStatus.Text = "Platform deleted.";
        }

        // ── Column header ─────────────────────────────────────
        private void DrawColHeader(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            int w = _listPanel.ClientSize.Width;
            var cols = GetColumnWidths(w);
            g.Clear(Color.FromArgb(235, 235, 238));
            using (var fnt = new Font("Segoe UI", 8.5f, FontStyle.Bold))
            using (var br  = new SolidBrush(Color.FromArgb(80, 80, 85)))
            using (var sf  = new StringFormat { Alignment = StringAlignment.Near, LineAlignment = StringAlignment.Center })
            using (var sfC = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                g.DrawString("#",         fnt, br, new RectangleF(cols[0],   0, cols[1]-cols[0], 28), sfC);
                g.DrawString("Name",      fnt, br, new RectangleF(cols[2]+8, 0, cols[3]-cols[2], 28), sf);
                g.DrawString("Remote ID", fnt, br, new RectangleF(cols[3]+8, 0, cols[4]-cols[3], 28), sf);
                g.DrawString("Password",  fnt, br, new RectangleF(cols[4]+8, 0, w-cols[4],       28), sf);
            }
            using (var p = new Pen(Color.FromArgb(210,210,215))) g.DrawLine(p, 0, 27, w, 27);
        }

        private int[] GetColumnWidths(int total)
        {
            // [0]=left  [1]=after row# [2]=after avatar  [3]=after name  [4]=after ID  [5]=right
            int rnW = 36, avW = 52, rem = Math.Max(0, total - rnW - avW);
            int nW = (int)(rem * 0.40), iW = (int)(rem * 0.38), pW = rem - nW - iW;
            return new int[] { 0, rnW, rnW+avW, rnW+avW+nW, rnW+avW+nW+iW, total };
        }

        // ── Contact list ──────────────────────────────────────
        private void DoRefresh()
        {
            CancelRender();
            var _ = RenderFilteredAsync(_loadCts.Token);
        }

        private void ClearList()
        {
            CancelRender();
            _contactListView.SetContacts(null);
            _lblEmpty.Visible   = false;
            _lblLoading.Visible = false;
        }

        private async System.Threading.Tasks.Task RenderFilteredAsync(
            System.Threading.CancellationToken ct, bool showLoading = false)
        {
            var platform = _selected;

            _filtered = string.IsNullOrWhiteSpace(_search)
                ? _contacts.ToList()
                : _contacts.Where(c =>
                    c.Name.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    c.Id.IndexOf(_search,   StringComparison.OrdinalIgnoreCase) >= 0).ToList();

            if (showLoading)
            {
                _lblLoading.Text    = "Loading contacts...";
                _lblLoading.Visible = true;
                _lblLoading.BringToFront();
                _lblLoading.Refresh();
            }
            _lblEmpty.Visible = false;

            // ContactListView.SetContacts is synchronous and instant regardless of count —
            // it just sets a list reference and calls Invalidate() for a single redraw.
            _contactListView.SetContacts(_filtered);

            if (ct.IsCancellationRequested) return;

            _lblLoading.Visible = false;
            _lblEmpty.Visible   = (_filtered.Count == 0 && _selected != null);
            if (_lblEmpty.Visible) _lblEmpty.BringToFront();
            _colHeader.Invalidate();
            if (_selected != null)
                _lblStatus.Text = _selected.Name + " — " + _contacts.Count
                    + " contact(s), showing " + _filtered.Count;

            // Return a completed task (no actual async needed - kept async signature
            // so callers don't need to change)
            await System.Threading.Tasks.Task.CompletedTask;
        }

        // ── Contact CRUD ──────────────────────────────────────
        private void AddContact()
        {
            if (_selected == null) return;
            using (var dlg = new ContactDialog(null, _selected.Name))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                _contacts.Add(dlg.Result);
                CsvStore.Save(_selected.CsvPath(), _contacts);
                _contactCache[_selected.Name] = _contacts;
                UpdateSidebarBadges();
                DoRefresh();
            }
        }

        private void OnEdit(Contact c)
        {
            if (_selected == null) return;
            using (var dlg = new ContactDialog(c, _selected.Name))
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                c.Name = dlg.Result.Name; c.Id = dlg.Result.Id; c.Password = dlg.Result.Password;
                CsvStore.Save(_selected.CsvPath(), _contacts);
                _contactCache[_selected.Name] = _contacts;
                UpdateSidebarBadges();
                ClearSelection();
                DoRefresh();
            }
        }

        private void OnDelete(Contact c)
        {
            string n = string.IsNullOrWhiteSpace(c.Name) ? c.Id : c.Name;
            if (MessageBox.Show("Delete \"" + n + "\"?", "Confirm Delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            _contacts.Remove(c);
            CsvStore.Save(_selected.CsvPath(), _contacts);
            _contactCache[_selected.Name] = _contacts;
            UpdateSidebarBadges();
            ClearSelection();
            DoRefresh();
        }

        // ── Connect ───────────────────────────────────────────
        private void OnConnect(Contact c)
        {
            if (_selected == null) return;
            string exePath = FindExe(_selected.ExeName);

            if (exePath == null)
            {
                var r = MessageBox.Show(
                    "\"" + _selected.ExeName + "\" was not found on this system or in the app folder.\n\n" +
                    "Options:\n" +
                    "  1. Install " + _selected.Name + " on this PC\n" +
                    "  2. Place " + _selected.ExeName + " next to MultiRemoteTool.exe\n\n" +
                    "Would you like to open the app folder now?",
                    _selected.Name + " Not Found",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (r == DialogResult.Yes)
                    Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
                return;
            }

            try
            {
                string args = _selected.BuildArgs(c, exePath);
                ProcessStartInfo psi;

                if (_selected.UsesPipe)
                {
                    // Command contains | — must run through cmd.exe so the shell
                    // handles stdin redirection (e.g. echo {pw} | anydesk.exe {id} --with-password)
                    psi = new ProcessStartInfo
                    {
                        FileName        = "cmd.exe",
                        Arguments       = "/c \"" + args + "\"",
                        UseShellExecute = false,
                        CreateNoWindow  = true
                    };
                }
                else
                {
                    psi = new ProcessStartInfo
                    {
                        FileName        = exePath,
                        Arguments       = args,
                        UseShellExecute = false
                    };
                }

                Process.Start(psi);
                _lblStatus.Text = "Connecting via " + _selected.Name + " to " + c.Id + "...";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to launch:\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Platform Import / Export ──────────────────────────

        // Returns true if two paths point to the same file (handles relative vs absolute,
        // different casing on Windows) — used to skip self-copy operations
        private static bool SamePath(string a, string b)
        {
            try
            {
                return string.Equals(
                    Path.GetFullPath(a), Path.GetFullPath(b),
                    StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }
        private void ImportPlatform()
        {
            using (var dlg = new OpenFileDialog
            {
                Title           = "Import Platform INI",
                Filter          = "INI files (*.ini)|*.ini|All files (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var p = Platform.LoadIni(dlg.FileName);
                    if (string.IsNullOrWhiteSpace(p.Name) ||
                        string.IsNullOrWhiteSpace(p.ExeName))
                    {
                        MessageBox.Show("The selected file doesn't look like a valid platform INI.\n" +
                            "Make sure it was exported from MultiRemoteTool.",
                            "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    if (_platforms.Any(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        var r = MessageBox.Show(
                            "A platform named \"" + p.Name + "\" already exists.\n\n" +
                            "Overwrite its settings?",
                            "Duplicate Platform", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (r != DialogResult.Yes) return;
                    }

                    // Copy INI into app directory — skip if source is already there
                    string iniDest = p.IniPath();
                    if (!SamePath(dlg.FileName, iniDest))
                        File.Copy(dlg.FileName, iniDest, overwrite: true);

                    // Also import contacts CSV if one sits alongside the INI
                    string importDir  = Path.GetDirectoryName(dlg.FileName);
                    string stem       = Path.GetFileNameWithoutExtension(dlg.FileName);
                    string srcCsv     = Path.Combine(importDir, "contacts_" + stem + ".csv");
                    string destCsv    = p.CsvPath();
                    if (File.Exists(srcCsv) && !SamePath(srcCsv, destCsv))
                        File.Copy(srcCsv, destCsv, overwrite: true);

                    // Add to the active list if not already present, then rebuild
                    if (!_platforms.Any(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                        _platforms.Add(p);
                    _platforms.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
                    SaveAllSettings();   // persist the updated LoadedPlatforms whitelist
                    RebuildSidebar();
                    var imported = _platforms.FirstOrDefault(x =>
                        x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
                    if (imported != null) SelectPlatform(imported);
                    _lblStatus.Text = "Platform \"" + p.Name + "\" imported.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Import failed:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportPlatform()
        {
            if (_selected == null) return;
            using (var dlg = new SaveFileDialog
            {
                Title           = "Export Platform INI",
                Filter          = "INI files (*.ini)|*.ini",
                FileName        = _selected.SafeName() + ".ini",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory
            })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    // Name in the exported INI follows the chosen filename stem,
                    // so saving as "RustDesk.ini" produces Name=RustDesk
                    string exportedName = Path.GetFileNameWithoutExtension(dlg.FileName);
                    var lines = new string[]
                    {
                        "[Platform]",
                        "Name="        + exportedName,
                        "ExeName="     + _selected.ExeName,
                        "CommandLine=" + _selected.CommandLine
                    };
                    File.WriteAllLines(dlg.FileName, lines, Encoding.UTF8);

                    // Also export contacts CSV alongside the INI (renamed to match)
                    string srcCsv  = _selected.CsvPath();
                    string destDir = Path.GetDirectoryName(dlg.FileName);
                    string destCsv = Path.Combine(destDir, "contacts_" + exportedName + ".csv");
                    if (File.Exists(srcCsv) && !SamePath(srcCsv, destCsv))
                        File.Copy(srcCsv, destCsv, overwrite: true);
                    else if (!File.Exists(srcCsv))
                        File.WriteAllText(destCsv, "", Encoding.UTF8); // create empty CSV

                    _lblStatus.Text = "Platform \"" + _selected.Name + "\" exported.";
                    MessageBox.Show(
                        "Platform exported:\n  " + Path.GetFileName(dlg.FileName) +
                        "\n  " + Path.GetFileName(destCsv) + "\n\n" +
                        "Place both files next to MultiRemoteTool.exe on the target machine,\n" +
                        "or use \"Import Platform\" to add them.",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Import / Export ───────────────────────────────────

        // FIX #1: use CsvStore.Load instead of naive split
        private void ImportCsv()
        {
            if (_selected == null) return;
            using (var dlg = new OpenFileDialog { Title = "Import contacts CSV",
                Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var parsed = CsvStore.Load(dlg.FileName);
                    int added = 0;
                    foreach (var ct in parsed)
                        if (!_contacts.Any(x => x.Id == ct.Id)) { _contacts.Add(ct); added++; }
                    CsvStore.Save(_selected.CsvPath(), _contacts);
                    InvalidateCache(_selected.Name);
                    UpdateSidebarBadges();
                    DoRefresh();
                    MessageBox.Show("Imported " + added + " new contact(s).\n" +
                        (parsed.Count - added) + " duplicate(s) skipped.",
                        "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Import failed:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // FIX #2: use CsvStore.Save so fields are properly escaped
        private void ExportCsv()
        {
            if (_selected == null) return;
            using (var dlg = new SaveFileDialog { Title = "Export contacts CSV",
                Filter = "CSV files (*.csv)|*.csv",
                FileName = "contacts_" + _selected.SafeName() + "_backup.csv",
                InitialDirectory = AppDomain.CurrentDomain.BaseDirectory })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    CsvStore.Save(dlg.FileName, _contacts);
                    MessageBox.Show("Exported " + _contacts.Count + " contact(s).",
                        "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export failed:\n" + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    // ──────────────────────────────────────────────────────────
    //  Entry point
    // ──────────────────────────────────────────────────────────
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
