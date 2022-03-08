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

namespace TCPServer
{
    public partial class Server : Form
    {
        public Server()
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

        SimpleTcpServer server1,server2,server3,server4,server5;

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (server1 != null && server1.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP1.SelectedItems.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP1.SelectedItems)
                    {
                        server1.Send(ip.ToString(), cbxMessage.Text);
                        txtLog1.Text += $"To {ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server2 != null && server2.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP2.SelectedItems.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP2.SelectedItems)
                    {
                        server2.Send(ip.ToString(), cbxMessage.Text);
                        txtLog2.Text += $"To {ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server3 != null && server3.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP3.SelectedItems.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP3.SelectedItems)
                    {
                        server3.Send(ip.ToString(), cbxMessage.Text);
                        txtLog3.Text += $"To {ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server4 != null && server4.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP4.SelectedItems.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP4.SelectedItems)
                    {
                        server4.Send(ip.ToString(), cbxMessage.Text);
                        txtLog4.Text += $"To {ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server5 != null && server5.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP5.SelectedItems.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP5.SelectedItems)
                    {
                        server5.Send(ip.ToString(), cbxMessage.Text);
                        txtLog5.Text += $"To {ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            cbxMessage.Text = string.Empty;
        }

        private void btnStart1_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbxIP1.Text == "Gooil")
                {
                    server1 = new SimpleTcpServer("192.168.0.50:9000");
                }
                else if (cbxIP1.Text == "Dooone1/Donga1")
                {
                    server1 = new SimpleTcpServer("192.168.1.1:9000");
                }
                else if (cbxIP1.Text == "Dooone2/Donga2")
                {
                    server1 = new SimpleTcpServer("192.168.2.2:9000");
                }
                else if (cbxIP1.Text == "Dooone3/Donga3")
                {
                    server1 = new SimpleTcpServer("192.168.3.3:9000");
                }
                else if (cbxIP1.Text == "Dooone4/Donga4")
                {
                    server1 = new SimpleTcpServer("192.168.4.4:9000");
                }
                else server1 = new SimpleTcpServer(cbxIP1.Text);
                server1.Events.ClientConnected += Events_ClientConnected1;
                server1.Events.ClientDisconnected += Events_ClientDisconnected1;
                server1.Events.DataReceived += Events_DataReceived1;
                server1.Start();
                txtLog1.Text += $"Starting...{Environment.NewLine}";
                btnStart1.Enabled = false;
            }
            catch (Exception)
            {

            }            
        }

        private void btnStart2_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbxIP2.Text == "Gooil")
                {
                    server2 = new SimpleTcpServer("192.168.0.50:9000");
                }
                else if (cbxIP2.Text == "Dooone1/Donga1")
                {
                    server2 = new SimpleTcpServer("192.168.1.1:9000");
                }
                else if (cbxIP2.Text == "Dooone2/Donga2")
                {
                    server2 = new SimpleTcpServer("192.168.2.2:9000");
                }
                else if (cbxIP2.Text == "Dooone3/Donga3")
                {
                    server2 = new SimpleTcpServer("192.168.3.3:9000");
                }
                else if (cbxIP2.Text == "Dooone4/Donga4")
                {
                    server2 = new SimpleTcpServer("192.168.4.4:9000");
                }
                else server2 = new SimpleTcpServer(cbxIP2.Text);
                server2.Events.ClientConnected += Events_ClientConnected2;
                server2.Events.ClientDisconnected += Events_ClientDisconnected2;
                server2.Events.DataReceived += Events_DataReceived2;
                server2.Start();
                txtLog2.Text += $"Starting...{Environment.NewLine}";
                btnStart2.Enabled = false;
            }
            catch (Exception)
            {

            }            
        }

        private void btnStart3_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbxIP3.Text == "Gooil")
                {
                    server3 = new SimpleTcpServer("192.168.0.50:9000");
                }
                else if (cbxIP3.Text == "Dooone1/Donga1")
                {
                    server3 = new SimpleTcpServer("192.168.1.1:9000");
                }
                else if (cbxIP3.Text == "Dooone2/Donga2")
                {
                    server3 = new SimpleTcpServer("192.168.2.2:9000");
                }
                else if (cbxIP3.Text == "Dooone3/Donga3")
                {
                    server3 = new SimpleTcpServer("192.168.3.3:9000");
                }
                else if (cbxIP3.Text == "Dooone4/Donga4")
                {
                    server3 = new SimpleTcpServer("192.168.4.4:9000");
                }
                else server3 = new SimpleTcpServer(cbxIP3.Text);
                server3.Events.ClientConnected += Events_ClientConnected3;
                server3.Events.ClientDisconnected += Events_ClientDisconnected3;
                server3.Events.DataReceived += Events_DataReceived3;
                server3.Start();
                txtLog3.Text += $"Starting...{Environment.NewLine}";
                btnStart3.Enabled = false;
            }
            catch (Exception)
            {

            }            
        }

        private void btnStart4_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbxIP4.Text == "Gooil")
                {
                    server4 = new SimpleTcpServer("192.168.0.50:9000");
                }
                else if (cbxIP4.Text == "Dooone1/Donga1")
                {
                    server4 = new SimpleTcpServer("192.168.1.1:9000");
                }
                else if (cbxIP4.Text == "Dooone2/Donga2")
                {
                    server4 = new SimpleTcpServer("192.168.2.2:9000");
                }
                else if (cbxIP4.Text == "Dooone3/Donga3")
                {
                    server4 = new SimpleTcpServer("192.168.3.3:9000");
                }
                else if (cbxIP4.Text == "Dooone4/Donga4")
                {
                    server4 = new SimpleTcpServer("192.168.4.4:9000");
                }
                else server4 = new SimpleTcpServer(cbxIP4.Text);
                server4.Events.ClientConnected += Events_ClientConnected4;
                server4.Events.ClientDisconnected += Events_ClientDisconnected4;
                server4.Events.DataReceived += Events_DataReceived4;
                server4.Start();
                txtLog4.Text += $"Starting...{Environment.NewLine}";
                btnStart4.Enabled = false;
            }
            catch (Exception)
            {

            }            
        }

        private void btnStart5_Click(object sender, EventArgs e)
        {
            try
            {
                if (cbxIP5.Text == "Gooil")
                {
                    server5 = new SimpleTcpServer("192.168.0.50:9000");
                }
                else if (cbxIP5.Text == "Dooone1/Donga1")
                {
                    server5 = new SimpleTcpServer("192.168.1.1:9000");
                }
                else if (cbxIP5.Text == "Dooone2/Donga2")
                {
                    server5 = new SimpleTcpServer("192.168.2.2:9000");
                }
                else if (cbxIP5.Text == "Dooone3/Donga3")
                {
                    server5 = new SimpleTcpServer("192.168.3.3:9000");
                }
                else if (cbxIP5.Text == "Dooone4/Donga4")
                {
                    server5 = new SimpleTcpServer("192.168.4.4:9000");
                }
                else server5 = new SimpleTcpServer(cbxIP5.Text);
                server5.Events.ClientConnected += Events_ClientConnected5;
                server5.Events.ClientDisconnected += Events_ClientDisconnected5;
                server5.Events.DataReceived += Events_DataReceived5;
                server5.Start();
                txtLog5.Text += $"Starting...{Environment.NewLine}";
                btnStart5.Enabled = false;
            }
            catch (Exception)
            {

            }            
        }

        private void Events_ClientDisconnected1(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog1.Text += $"{e.IpPort} disconnected.{Environment.NewLine}";
                lstClientIP1.Items.Remove(e.IpPort);
            });
        }

        private void Events_DataReceived1(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog1.Text += $"{e.IpPort} : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
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

        private void Events_ClientConnected1(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog1.Text += $"{e.IpPort} connected.{Environment.NewLine}";
                lstClientIP1.Items.Add(e.IpPort);
                lstClientIP1.SetSelected(lstClientIP1.FindString(e.IpPort), true);
            });
        }

        private void Events_ClientDisconnected2(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog2.Text += $"{e.IpPort} disconnected.{Environment.NewLine}";
                lstClientIP2.Items.Remove(e.IpPort);
                lstClientIP2.SetSelected(lstClientIP2.FindString(e.IpPort), true);
            });
        }

        private void Events_DataReceived2(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog2.Text += $"{e.IpPort} : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
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

        private void Events_ClientConnected2(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog2.Text += $"{e.IpPort} connected.{Environment.NewLine}";
                lstClientIP2.Items.Add(e.IpPort);
            });
        }

        private void Events_ClientDisconnected3(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog3.Text += $"{e.IpPort} disconnected.{Environment.NewLine}";
                lstClientIP3.Items.Remove(e.IpPort);
            });
        }

        private void Events_DataReceived3(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog3.Text += $"{e.IpPort} : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
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

        private void Events_ClientConnected3(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog3.Text += $"{e.IpPort} connected.{Environment.NewLine}";
                lstClientIP3.Items.Add(e.IpPort);
                lstClientIP3.SetSelected(lstClientIP3.FindString(e.IpPort), true);
            });
        }

        private void Events_ClientDisconnected4(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog4.Text += $"{e.IpPort} disconnected.{Environment.NewLine}";
                lstClientIP4.Items.Remove(e.IpPort);
            });
        }

        private void Events_DataReceived4(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog4.Text += $"{e.IpPort} : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
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

        private void Events_ClientConnected4(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog4.Text += $"{e.IpPort} connected.{Environment.NewLine}";
                lstClientIP4.Items.Add(e.IpPort);
                lstClientIP4.SetSelected(lstClientIP4.FindString(e.IpPort), true);
            });
        }

        private void Events_ClientDisconnected5(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog5.Text += $"{e.IpPort} disconnected.{Environment.NewLine}";
                lstClientIP5.Items.Remove(e.IpPort);
            });
        }

        private void Events_DataReceived5(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog5.Text += $"{e.IpPort} : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
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

        private void Events_ClientConnected5(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog5.Text += $"{e.IpPort} connected.{Environment.NewLine}";
                lstClientIP5.Items.Add(e.IpPort);
                lstClientIP5.SetSelected(lstClientIP5.FindString(e.IpPort), true);
            });
        }

        private void txtLog1_TextChanged(object sender, EventArgs e)
        {
            txtLog1.SelectionStart = txtLog1.Text.Length;
            txtLog1.ScrollToCaret();
        }

        private void txtLog2_TextChanged(object sender, EventArgs e)
        {
            txtLog2.SelectionStart = txtLog2.Text.Length;
            txtLog2.ScrollToCaret();
        }

        private void btnRestart_Click(object sender, EventArgs e)
        {
            Application.Restart();
            Environment.Exit(0);
        }

        private void Server_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
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

        private void Server_Resize(object sender, EventArgs e)
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

        private void cbxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(this, new EventArgs());
            }
        }

        private void txtLog3_TextChanged(object sender, EventArgs e)
        {
            txtLog3.SelectionStart = txtLog3.Text.Length;
            txtLog3.ScrollToCaret();
        }

        private void txtLog4_TextChanged(object sender, EventArgs e)
        {
            txtLog4.SelectionStart = txtLog4.Text.Length;
            txtLog4.ScrollToCaret();
        }

        private void txtLog5_TextChanged(object sender, EventArgs e)
        {
            txtLog5.SelectionStart = txtLog5.Text.Length;
            txtLog5.ScrollToCaret();
        }

        private void btnSendAll_Click(object sender, EventArgs e)
        {
            if (server1 != null && server1.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP1.Items.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP1.Items)
                    {
                        server1.Send(ip.ToString(), cbxMessage.Text);
                        txtLog1.Text += $"To {ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server2 != null && server2.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP2.Items.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP2.Items)
                    {
                        server2.Send(ip.ToString(), cbxMessage.Text);
                        txtLog2.Text += $"To {ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server3 != null && server3.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP3.Items.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP3.Items)
                    {
                        server3.Send(ip.ToString(), cbxMessage.Text);
                        txtLog3.Text += $"To {ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server4 != null && server4.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP4.Items.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP4.Items)
                    {
                        server4.Send(ip.ToString(), cbxMessage.Text);
                        txtLog4.Text += $"To {ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server5 != null && server5.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP5.Items.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP5.Items)
                    {
                        server5.Send(ip.ToString(), cbxMessage.Text);
                        txtLog5.Text += $"To {ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }
            cbxMessage.Text = string.Empty;
        }
    }
}
