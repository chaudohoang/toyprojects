using System;
using System.Drawing;
using System.Windows.Forms;

namespace HelloApp
{
    public class MainForm : Form
    {
        private Label  lblMessage;
        private Button btnHello;
        private TextBox txtName;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text            = "Hello WinForms";
            this.Size            = new Size(360, 220);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.StartPosition   = FormStartPosition.CenterScreen;

            lblMessage = new Label
            {
                Text      = "Enter your name:",
                Location  = new Point(20, 20),
                Size      = new Size(300, 24),
                Font      = new Font("Segoe UI", 10)
            };

            txtName = new TextBox
            {
                Location  = new Point(20, 50),
                Size      = new Size(300, 26),
                Font      = new Font("Segoe UI", 10)
            };

            btnHello = new Button
            {
                Text     = "Say Hello",
                Location = new Point(20, 90),
                Size     = new Size(100, 32),
                Font     = new Font("Segoe UI", 10)
            };
            btnHello.Click += new EventHandler(btnHello_Click);

            this.Controls.Add(lblMessage);
            this.Controls.Add(txtName);
            this.Controls.Add(btnHello);
        }

        private void btnHello_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            lblMessage.Text = string.IsNullOrEmpty(name)
                ? "Hello, World!"
                : $"Hello, {name}!";
        }
    }
}