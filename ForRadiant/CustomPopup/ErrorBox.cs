using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomPopup
{
    public partial class ErrorBox : Form
    {
        public string initialMessage;
        public ErrorBox()
        {
            InitializeComponent();
            BackgroundWorker blinker;
            blinker = new BackgroundWorker();
            blinker.DoWork += blinker_DoWork;
            if (blinker.IsBusy == false)
            {
                blinker.RunWorkerAsync();
            }

        }
        public ErrorBox(string message)
        {
            this.initialMessage = message;
            InitializeComponent();
            BackgroundWorker blinker;
            blinker = new BackgroundWorker();
            blinker.DoWork += blinker_DoWork;
            if (blinker.IsBusy == false)
            {
                blinker.RunWorkerAsync();
            }
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.StartPosition = FormStartPosition.Manual;
            Point start = new Point(Screen.PrimaryScreen.Bounds.Left + Screen.PrimaryScreen.Bounds.Width * 3 / 8, Screen.PrimaryScreen.Bounds.Top + Screen.PrimaryScreen.Bounds.Height * 3 / 8);
            this.Location = start;
            this.Width = Screen.PrimaryScreen.Bounds.Width * 1 / 4;
            this.Height = Screen.PrimaryScreen.Bounds.Height * 1 / 4;
            this._MinButton.Location = new Point(this.Width - this._MinButton.Width - this._CloseButton.Width, 0);
            this._CloseButton.Location = new Point(this.Width - this._CloseButton.Width, 0);

            Label lblMessage = new Label();
            lblMessage.ForeColor = Color.White;
            lblMessage.Font = new System.Drawing.Font("Segoe UI", 20);
            lblMessage.Dock = DockStyle.Fill;
            lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblMessage.Padding = new Padding(20, 20, 20, 20);
            lblMessage.Text = this.initialMessage;
            this.Controls.Add(lblMessage);            

        }
        private void blink()
        {


            if (this.BackColor == Color.Red)
                this.BackColor = Color.Black;
            else
                this.BackColor = Color.Red;
        }

        private void blinker_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                System.Threading.Thread.Sleep(500); // Set fast to slow.

                if (this.InvokeRequired)
                {
                    this.Invoke((Action)blink);
                }
                else
                {
                    blink();
                }
            }

        }

        Point offset;
        bool isTopPanelDragged = false;


        private void TopPanel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isTopPanelDragged = true;
                Point pointStartPosition = this.PointToScreen(new Point(e.X, e.Y));
                offset = new Point();
                offset.X = this.Location.X - pointStartPosition.X;
                offset.Y = this.Location.Y - pointStartPosition.Y;
            }
            else
            {
                isTopPanelDragged = false;
            }
        }

        private void TopPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (isTopPanelDragged)
            {
                Point newPoint = TopPanel.PointToScreen(new Point(e.X, e.Y));
                newPoint.Offset(offset);
                this.Location = newPoint;
            }
        }

        private void TopPanel_MouseUp(object sender, MouseEventArgs e)
        {
            isTopPanelDragged = false;
        }

        private void WindowTextLabel_MouseDown(object sender, MouseEventArgs e)
        {
            TopPanel_MouseDown(sender, e);
        }

        private void WindowTextLabel_MouseMove(object sender, MouseEventArgs e)
        {
            TopPanel_MouseMove(sender, e);
        }

        private void WindowTextLabel_MouseUp(object sender, MouseEventArgs e)
        {
            TopPanel_MouseUp(sender, e);
        }

        private void _CloseButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void _MinButton_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }
        

        private void buttonZ1_Click(object sender, EventArgs e)
        {
            this.Close();
        }


    }
}
