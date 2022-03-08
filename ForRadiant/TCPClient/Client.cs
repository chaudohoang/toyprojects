using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SuperSimpleTcp;
using System.Diagnostics;

namespace TCPClient
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }

        private void SetVersionInfo()
        {
            Version versionInfo = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DateTime startDate = new DateTime(2000, 1, 1);
            int diffDays = versionInfo.Build;
            DateTime computedDate = startDate.AddDays(diffDays);
            string lastBuilt = computedDate.ToShortDateString();
            //this.Text = string.Format("{0} - {1} ({2})",
            //            this.Text, versionInfo.ToString(), lastBuilt);
            this.Text = string.Format("{0} - {1}",
                        this.Text, versionInfo.ToString());
        }

        SimpleTcpClient client;

        private void btnConnect_Click(object sender, EventArgs e)
        {            
            try
            {
                if (cbxIP.Text == "Gooil")
                {
                    client = new SimpleTcpClient("192.168.0.50:9000");
                }
                else if (cbxIP.Text == "Dooone1")
                {
                    client = new SimpleTcpClient("192.168.1.1:9000");
                }
                else if (cbxIP.Text == "Dooone2")
                {
                    client = new SimpleTcpClient("192.168.2.2:9000");
                }
                else if (cbxIP.Text == "Dooone3")
                {
                    client = new SimpleTcpClient("192.168.3.3:9000");
                }
                else if (cbxIP.Text == "Dooone4")
                {
                    client = new SimpleTcpClient("192.168.4.4:9000");
                }
                else client = new SimpleTcpClient(cbxIP.Text);
                client.Events.Connected += Events_Connected;
                client.Events.DataReceived += Events_DataReceived;
                client.Events.Disconnected += Events_Disconnected;
                client.Connect();
                btnConnect.Enabled = false;
            }
            catch (Exception)
            {

            }
        }

        private void Events_Disconnected(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog.Text += $"Server disconnected.{Environment.NewLine}";
                btnConnect.Enabled = true;
            });
        }

        private void Events_DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog.Text += $"Server : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
            });
            if (Encoding.UTF8.GetString(e.Data) == "Open Notepad")
            {
                try
                {
                    Process.Start("notepad.exe");
                }
                catch (Exception)
                {

                }
                
            }
            else if (Encoding.UTF8.GetString(e.Data) == "Restart TrueTest")
            {
                try
                {
                    foreach (var process in Process.GetProcessesByName("TrueTest"))
                    {
                        process.Kill();
                    }
                    Process.Start("TrueTest.exe");
                }
                catch (Exception)
                {

                }
            }
        }

        private void Events_Connected(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog.Text += $"Server connected.{Environment.NewLine}";
            });
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (client != null && client.IsConnected)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text))
                {
                    client.Send(cbxMessage.Text);
                    txtLog.Text += $"Me : {cbxMessage.Text}{Environment.NewLine}";
                    cbxMessage.Text = string.Empty;
                }
            }
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            Application.Restart();
            Environment.Exit(0);
        }

        private void Client_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }

        private void showToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            Activate();
            this.WindowState = FormWindowState.Normal;
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
            Environment.Exit(0);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Client_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            Activate();
            this.WindowState = FormWindowState.Normal;
        }
    }
}
