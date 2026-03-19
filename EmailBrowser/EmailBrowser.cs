using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EmailBrowser
{
    public class MailEntry
    {
        public string Mail { get; set; }
        public string BrowserPath { get; set; }
        public string MailURL { get; set; }

        public override string ToString() => Mail;
    }

    public class MainForm : Form
    {
        private ListBox listBoxEmails;
        private Button btnRead;
        private Button btnLoadCSV;
        private Label lblTitle;
        private Label lblCount;
        private TextBox txtSearch;
        private Label lblSearch;
        private Panel panelTop;
        private Panel panelBottom;
        private Panel panelList;

        private List<MailEntry> allEntries = new List<MailEntry>();
        private List<MailEntry> filteredEntries = new List<MailEntry>();
        private string csvPath = "";

        public MainForm()
        {
            InitializeComponent();
            TryAutoLoadCSV();
        }

        private void InitializeComponent()
        {
            this.Text = "Email Browser";
            this.Size = new Size(700, 600);
            this.MinimumSize = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 250);
            this.Font = new Font("Segoe UI", 9f);

            // Load icon from embedded exe resource
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            // ── Top Panel ──
            panelTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 110,
                BackColor = Color.FromArgb(30, 30, 60),
                Padding = new Padding(16, 10, 16, 10)
            };

            lblTitle = new Label
            {
                Text = "📧  Email Browser",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(16, 12)
            };

            lblSearch = new Label
            {
                Text = "Search:",
                ForeColor = Color.FromArgb(180, 180, 210),
                AutoSize = true,
                Location = new Point(16, 58)
            };

            txtSearch = new TextBox
            {
                Location = new Point(70, 55),
                Width = 350,
                BackColor = Color.FromArgb(50, 50, 80),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10f)
            };
            txtSearch.TextChanged += TxtSearch_TextChanged;

            btnLoadCSV = new Button
            {
                Text = "📂 Load CSV",
                Location = new Point(435, 52),
                Size = new Size(110, 28),
                BackColor = Color.FromArgb(70, 130, 180),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            btnLoadCSV.FlatAppearance.BorderSize = 0;
            btnLoadCSV.Click += BtnLoadCSV_Click;

            panelTop.Controls.AddRange(new Control[] { lblTitle, lblSearch, txtSearch, btnLoadCSV });

            // ── List Panel ──
            panelList = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 10, 16, 10),
                BackColor = Color.FromArgb(245, 245, 250)
            };

            listBoxEmails = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10f),
                BackColor = Color.White,
                ForeColor = Color.FromArgb(30, 30, 60),
                BorderStyle = BorderStyle.FixedSingle,
                SelectionMode = SelectionMode.One,
                ItemHeight = 22
            };
            listBoxEmails.SelectedIndexChanged += ListBoxEmails_SelectedIndexChanged;
            listBoxEmails.DoubleClick += (s, e) => OpenSelected();

            panelList.Controls.Add(listBoxEmails);

            // ── Bottom Panel ──
            panelBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(235, 235, 245),
                Padding = new Padding(16, 10, 16, 10)
            };

            lblCount = new Label
            {
                Text = "No entries loaded",
                ForeColor = Color.FromArgb(100, 100, 130),
                AutoSize = true,
                Location = new Point(16, 20)
            };

            btnRead = new Button
            {
                Text = "▶  Read Email",
                Size = new Size(140, 36),
                BackColor = Color.FromArgb(34, 139, 87),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Enabled = false
            };
            btnRead.FlatAppearance.BorderSize = 0;
            btnRead.Location = new Point(this.ClientSize.Width - 160, 12);
            btnRead.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnRead.Click += (s, e) => OpenSelected();

            panelBottom.Controls.AddRange(new Control[] { lblCount, btnRead });

            this.Controls.Add(panelList);
            this.Controls.Add(panelBottom);
            this.Controls.Add(panelTop);
        }

        private void TryAutoLoadCSV()
        {
            // Try to load mail_data.csv from same directory as exe
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string auto = Path.Combine(exeDir, "mail_data.csv");
            if (File.Exists(auto))
                LoadCSV(auto);
        }

        private void BtnLoadCSV_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Select mail_data.csv";
                dlg.Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                    LoadCSV(dlg.FileName);
            }
        }

        private void LoadCSV(string path)
        {
            try
            {
                csvPath = path;
                allEntries.Clear();

                var lines = File.ReadAllLines(path);
                bool first = true;
                foreach (var line in lines)
                {
                    if (first) { first = false; continue; } // skip header
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = SplitCSVLine(line);
                    if (parts.Length >= 3)
                    {
                        allEntries.Add(new MailEntry
                        {
                            Mail = parts[0].Trim(),
                            BrowserPath = parts[1].Trim(),
                            MailURL = parts[2].Trim()
                        });
                    }
                }

                ApplyFilter();
                this.Text = $"Email Browser — {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading CSV:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string[] SplitCSVLine(string line)
        {
            // Handle quoted fields
            var result = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();
            foreach (char c in line)
            {
                if (c == '"') { inQuotes = !inQuotes; }
                else if (c == ',' && !inQuotes) { result.Add(current.ToString()); current.Clear(); }
                else { current.Append(c); }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }

        private void ApplyFilter()
        {
            string search = txtSearch.Text.Trim().ToLower();
            filteredEntries = string.IsNullOrEmpty(search)
                ? allEntries.ToList()
                : allEntries.Where(e => e.Mail.ToLower().Contains(search)).ToList();

            listBoxEmails.BeginUpdate();
            listBoxEmails.Items.Clear();
            foreach (var entry in filteredEntries)
                listBoxEmails.Items.Add(entry.Mail);
            listBoxEmails.EndUpdate();

            lblCount.Text = $"{filteredEntries.Count} of {allEntries.Count} entries";
            btnRead.Enabled = false;
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e) => ApplyFilter();

        private void ListBoxEmails_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRead.Enabled = listBoxEmails.SelectedIndex >= 0;
        }

        private void OpenSelected()
        {
            int idx = listBoxEmails.SelectedIndex;
            if (idx < 0 || idx >= filteredEntries.Count) return;

            var entry = filteredEntries[idx];

            if (!File.Exists(entry.BrowserPath))
            {
                MessageBox.Show(
                    $"Brave browser not found at:\n{entry.BrowserPath}\n\nPlease check the CSV path.",
                    "Browser Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = entry.BrowserPath,
                    Arguments = $"\"{entry.MailURL}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open browser:\n{ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
