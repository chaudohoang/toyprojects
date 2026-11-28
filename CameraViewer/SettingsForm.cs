using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CameraViewer
{
    /// <summary>Edits a copy of the config and writes it back only on OK.</summary>
    sealed class SettingsForm : Form
    {
        readonly Config _cfg;
        readonly TextBox _ip = new TextBox();
        readonly NumericUpDown _channels = new NumericUpDown { Minimum = 1, Maximum = 64 };
        readonly TextBox _user = new TextBox();
        readonly TextBox _pass = new TextBox { UseSystemPasswordChar = true };
        readonly CheckBox _showPass = new CheckBox { Text = "Hiện", AutoSize = true };
        readonly ComboBox _cols = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        readonly DataGridView _grid = new DataGridView();

        public SettingsForm(Config cfg)
        {
            _cfg = cfg;
            Text = "Cài đặt camera";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(980, 620);
            MinimumSize = new Size(760, 480);
            Font = new Font("Segoe UI", 9f);
            ShowInTaskbar = false;

            // ---- top: NVR + login
            var top = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 6, Padding = new Padding(10, 10, 10, 4) };
            for (int i = 0; i < 6; i++) top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            Label L(string t) => new Label { Text = t, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0, 6, 4, 0) };

            _ip.Width = 140; _user.Width = 140; _pass.Width = 140; _channels.Width = 60;
            var regen = new Button { Text = "Tạo lại danh sách kênh từ đầu ghi", AutoSize = true };
            regen.Click += (s, e) => RegenerateNvrRows();

            top.Controls.Add(L("Đầu ghi (NVR) IP:"), 0, 0);
            top.Controls.Add(_ip, 1, 0);
            top.Controls.Add(L("Số kênh:"), 2, 0);
            top.Controls.Add(_channels, 3, 0);
            top.Controls.Add(regen, 4, 0);
            top.SetColumnSpan(regen, 2);

            var passBox = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0), WrapContents = false };
            passBox.Controls.Add(_pass);
            passBox.Controls.Add(_showPass);
            var test = new Button { Text = "Kiểm tra đăng nhập", AutoSize = true, Margin = new Padding(10, 0, 0, 0) };
            test.Click += async (s, e) =>
            {
                test.Enabled = false;
                // Test the selected camera row if any, else NVR channel 1, with the login that row would use.
                _grid.EndEdit();
                var url = Config.UniviewUrl(_ip.Text.Trim(), 1, false);
                string user = _user.Text.Trim(), pass = _pass.Text;
                var row = _grid.CurrentRow;
                if (row != null && !row.IsNewRow)
                {
                    var cam = ReadRow(row);
                    if (cam != null && cam.Main.Length > 0)
                    {
                        url = cam.GridUrl;
                        new Config { User = user, Password = pass }.LoginFor(cam, url, out user, out pass);
                    }
                }
                var report = await System.Threading.Tasks.Task.Run(() => RtspProbe.Run(url, user, pass));
                test.Enabled = true;
                try
                {
                    var log = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.ExecutablePath), "camera-viewer.log");
                    System.IO.File.AppendAllText(log, $"--- Kiểm tra đăng nhập {DateTime.Now:HH:mm:ss}\r\n{report}\r\n");
                }
                catch { }
                MessageBox.Show(this, report + "\r\n200 OK = đúng.  401 Unauthorized = sai tài khoản/mật khẩu (hoặc tài khoản không có quyền xem).",
                    "Kiểm tra đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            passBox.Controls.Add(test);
            _showPass.Margin = new Padding(6, 5, 0, 0);
            _showPass.CheckedChanged += (s, e) => { _pass.UseSystemPasswordChar = !_showPass.Checked; _grid.Invalidate(); };

            top.Controls.Add(L("Tài khoản:"), 0, 1);
            top.Controls.Add(_user, 1, 1);
            top.Controls.Add(L("Mật khẩu:"), 2, 1);
            top.Controls.Add(passBox, 3, 1);
            top.SetColumnSpan(passBox, 2);

            _cols.Items.Add("Tự động");
            for (int i = 1; i <= 8; i++) _cols.Items.Add(i + " cột");
            _cols.Width = 100;
            top.Controls.Add(L("Lưới:"), 0, 2);
            top.Controls.Add(_cols, 1, 2);

            var note = new Label
            {
                Dock = DockStyle.Top,
                AutoSize = false,
                Height = 72,
                Padding = new Padding(12, 2, 12, 0),
                ForeColor = Color.DimGray,
                Text = "Tài khoản / mật khẩu ở trên dùng cho các kênh đầu ghi (dòng để trống cột Tài khoản riêng).\r\n" +
                       "Camera EZVIZ: Tài khoản riêng = admin, Mật khẩu riêng = mã xác minh 6 chữ in hoa trên nhãn dán dưới camera.\r\n" +
                       "\"Kiểm tra đăng nhập\" thử dòng đang chọn trong bảng (chưa chọn dòng nào thì thử kênh 1 đầu ghi).\r\n" +
                       "Luồng phụ để trống thì lưới dùng luôn luồng chính.",
            };

            // ---- grid
            _grid.Dock = DockStyle.Fill;
            _grid.AllowUserToAddRows = true;
            _grid.AllowUserToDeleteRows = true;
            _grid.RowHeadersWidth = 28;
            _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            _grid.BackgroundColor = SystemColors.Window;
            _grid.Columns.Add(new DataGridViewCheckBoxColumn { Name = "On", HeaderText = "Hiện", FillWeight = 8 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "Tên", FillWeight = 18 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Main", HeaderText = "Luồng chính (khi xem lớn)", FillWeight = 32 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sub", HeaderText = "Luồng phụ (trong lưới)", FillWeight = 32 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "User", HeaderText = "Tài khoản riêng", FillWeight = 10 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Pass", HeaderText = "Mật khẩu riêng", FillWeight = 10 });
            _grid.DefaultValuesNeeded += (s, e) => e.Row.Cells["On"].Value = true;
            // Show the per-camera password as dots unless "Hiện" is ticked; plain while editing.
            _grid.CellFormatting += (s, e) =>
            {
                if (e.ColumnIndex == _grid.Columns["Pass"].Index && e.Value is string p && p.Length > 0 && !_showPass.Checked)
                { e.Value = new string('●', p.Length); e.FormattingApplied = true; }
            };

            var gridWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 10, 0) };
            gridWrap.Controls.Add(_grid);

            // ---- bottom buttons
            var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 46, Padding = new Padding(10, 8, 10, 8), FlowDirection = FlowDirection.LeftToRight };
            var up = new Button { Text = "▲ Lên", AutoSize = true };
            var down = new Button { Text = "▼ Xuống", AutoSize = true };
            var del = new Button { Text = "Xóa dòng", AutoSize = true };
            up.Click += (s, e) => MoveRow(-1);
            down.Click += (s, e) => MoveRow(1);
            del.Click += (s, e) =>
            {
                var row = _grid.CurrentRow;
                if (row != null && !row.IsNewRow) _grid.Rows.Remove(row);
            };
            bottom.Controls.AddRange(new Control[] { up, down, del });

            var ok = new Button { Text = "Lưu", Width = 90, DialogResult = DialogResult.OK };
            var cancel = new Button { Text = "Hủy", Width = 90, DialogResult = DialogResult.Cancel };
            var right = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, FlowDirection = FlowDirection.RightToLeft, WrapContents = false };
            right.Controls.Add(cancel);
            right.Controls.Add(ok);
            var bottomBar = new Panel { Dock = DockStyle.Bottom, Height = 46 };
            bottom.Dock = DockStyle.Fill;
            right.Padding = new Padding(0, 8, 10, 8);
            bottomBar.Controls.Add(bottom);
            bottomBar.Controls.Add(right);

            AcceptButton = ok;
            CancelButton = cancel;
            ok.Click += (s, e) => { if (!Apply()) DialogResult = DialogResult.None; };

            Controls.Add(gridWrap);
            Controls.Add(note);
            Controls.Add(top);
            Controls.Add(bottomBar);

            // ---- load values
            _ip.Text = cfg.NvrIp;
            _channels.Value = Math.Max(1, Math.Min(64, cfg.NvrChannels));
            _user.Text = cfg.User;
            _pass.Text = cfg.Password;
            _cols.SelectedIndex = Math.Max(0, Math.Min(8, cfg.Columns));
            foreach (var c in cfg.Cameras) _grid.Rows.Add(c.Enabled, c.Name, c.Main, c.Sub, c.User, c.Pass);

            Shown += (s, e) => { if (string.IsNullOrEmpty(_pass.Text)) _pass.Focus(); };
        }

        List<CamInfo> ReadRows()
        {
            var list = new List<CamInfo>();
            foreach (DataGridViewRow r in _grid.Rows)
            {
                if (r.IsNewRow) continue;
                var cam = ReadRow(r);
                if (cam == null) continue;
                if (cam.Name.Length == 0) cam.Name = "Camera " + (list.Count + 1);
                list.Add(cam);
            }
            return list;
        }

        static CamInfo ReadRow(DataGridViewRow r)
        {
            string Cell(string n) => (r.Cells[n].Value as string ?? "").Trim();
            var main = Cell("Main");
            var name = Cell("Name");
            if (main.Length == 0 && name.Length == 0) return null;
            return new CamInfo
            {
                Enabled = r.Cells["On"].Value is bool b ? b : true,
                Name = name,
                Main = main,
                Sub = Cell("Sub"),
                User = Cell("User"),
                Pass = r.Cells["Pass"].Value as string ?? "",
            };
        }

        /// <summary>Rebuilds the NVR channel rows (keeping their names) and keeps any
        /// other camera rows after them.</summary>
        void RegenerateNvrRows()
        {
            var ip = _ip.Text.Trim();
            if (ip.Length == 0) { MessageBox.Show(this, "Nhập IP đầu ghi trước."); return; }
            var current = ReadRows();
            var nvr = Config.UniviewCameras(ip, (int)_channels.Value, current);
            var others = current.Where(c => !c.Main.Contains("/unicast/c")).ToList();

            _grid.Rows.Clear();
            foreach (var c in nvr.Concat(others)) _grid.Rows.Add(c.Enabled, c.Name, c.Main, c.Sub, c.User, c.Pass);
        }

        void MoveRow(int delta)
        {
            var row = _grid.CurrentRow;
            if (row == null || row.IsNewRow) return;
            int i = row.Index, j = i + delta;
            if (j < 0 || j >= _grid.Rows.Count - (_grid.AllowUserToAddRows ? 1 : 0)) return;
            var values = row.Cells.Cast<DataGridViewCell>().Select(c => c.Value).ToArray();
            _grid.Rows.RemoveAt(i);
            _grid.Rows.Insert(j, values);
            _grid.CurrentCell = _grid.Rows[j].Cells[1];
        }

        bool Apply()
        {
            _grid.EndEdit();
            var cams = ReadRows();
            var bad = cams.FirstOrDefault(c => c.Main.Length > 0 && !c.Main.Contains("://"));
            if (bad != null)
            {
                MessageBox.Show(this, $"Địa chỉ của \"{bad.Name}\" không hợp lệ (phải bắt đầu bằng rtsp://).", Text);
                return false;
            }
            _cfg.NvrIp = _ip.Text.Trim();
            _cfg.NvrChannels = (int)_channels.Value;
            _cfg.User = _user.Text.Trim();
            _cfg.Password = _pass.Text;
            _cfg.Columns = Math.Max(0, _cols.SelectedIndex);
            _cfg.Cameras = cams;
            return true;
        }
    }
}
