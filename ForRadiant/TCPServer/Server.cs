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
using System.IO;
using System.Net.NetworkInformation;

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

        SimpleTcpServer server1,server2,server3,server4,server5,server6;
        private void HandleMessage(string message)
        {
            if (message == "Open Notepad")
            {
                try
                {
                    Process.Start("notepad.exe");
                }
                catch (Exception)
                {

                }

            }
            else if (message == "Restart TrueTest")
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
            else if (message.StartsWith("start "))
            {
                try
                {
                    string command = message.Replace("start ", string.Empty);
                    Process.Start(command);
                }
                catch (Exception)
                {

                }
            }
            else if (message.StartsWith("cmd "))
            {
                try
                {
                    string command = message.Replace("cmd ", string.Empty);
                    Process process = new Process();
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.FileName = "cmd.exe";
                    startInfo.Arguments = command;
                    process.StartInfo = startInfo;
                    process.Start();
                }
                catch (Exception)
                {

                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (server1 != null && server1.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP1.SelectedItems.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP1.SelectedItems)
                    {
                        server1.Send(ip.ToString(), cbxMessage.Text);
                        txtLog1.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
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
                        txtLog2.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
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
                        txtLog3.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
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
                        txtLog4.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
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
                        txtLog5.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server6 != null && server6.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP6.SelectedItems.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP6.SelectedItems)
                    {
                        server6.Send(ip.ToString(), cbxMessage.Text);
                        txtLog6.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }
            cbxMessage.SelectAll();
        }

        private void btnStart1_Click(object sender, EventArgs e)
        {
            try
            {
                server1 = new SimpleTcpServer($"{cbxIP1.Text}:9000");
                server1.Events.ClientConnected += Events_ClientConnected1;
                server1.Events.ClientDisconnected += Events_ClientDisconnected1;
                server1.Events.DataReceived += Events_DataReceived1;
                server1.Start();
                txtLog1.Text += $"Listening...{Environment.NewLine}";
                btnStart1.Enabled = false;
                btnStop1.Enabled = true;
            }
            catch (Exception)
            {

            }            
        }

        private void btnStart2_Click(object sender, EventArgs e)
        {
            try
            {
                server2 = new SimpleTcpServer($"{cbxIP2.Text}:9000");
                server2.Events.ClientConnected += Events_ClientConnected2;
                server2.Events.ClientDisconnected += Events_ClientDisconnected2;
                server2.Events.DataReceived += Events_DataReceived2;
                server2.Start();
                txtLog2.Text += $"Listening...{Environment.NewLine}";
                btnStart2.Enabled = false;
                btnStop2.Enabled = true;
            }
            catch (Exception)
            {

            }            
        }

        private void btnStart3_Click(object sender, EventArgs e)
        {
            try
            {
                server3 = new SimpleTcpServer($"{cbxIP3.Text}:9000");
                server3.Events.ClientConnected += Events_ClientConnected3;
                server3.Events.ClientDisconnected += Events_ClientDisconnected3;
                server3.Events.DataReceived += Events_DataReceived3;
                server3.Start();
                txtLog3.Text += $"Listening...{Environment.NewLine}";
                btnStart3.Enabled = false;
                btnStop3.Enabled = true;
            }
            catch (Exception)
            {

            }            
        }

        private void btnStart4_Click(object sender, EventArgs e)
        {
            try
            {
                server4 = new SimpleTcpServer($"{cbxIP4.Text}:9000");
                server4.Events.ClientConnected += Events_ClientConnected4;
                server4.Events.ClientDisconnected += Events_ClientDisconnected4;
                server4.Events.DataReceived += Events_DataReceived4;
                server4.Start();
                txtLog4.Text += $"Listening...{Environment.NewLine}";
                btnStart4.Enabled = false;
                btnStop4.Enabled = true;
            }
            catch (Exception)
            {

            }            
        }

        private void btnStart5_Click(object sender, EventArgs e)
        {
            try
            {
                server5 = new SimpleTcpServer($"{cbxIP5.Text}:9000");
                server5.Events.ClientConnected += Events_ClientConnected5;
                server5.Events.ClientDisconnected += Events_ClientDisconnected5;
                server5.Events.DataReceived += Events_DataReceived5;
                server5.Start();
                txtLog5.Text += $"Listening...{Environment.NewLine}";
                btnStart5.Enabled = false;
                btnStop5.Enabled = true;
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
            HandleMessage(Encoding.UTF8.GetString(e.Data));
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
            });
        }

        private void Events_DataReceived2(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog2.Text += $"{e.IpPort} : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
            });
            HandleMessage(Encoding.UTF8.GetString(e.Data));
        }

        private void Events_ClientConnected2(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog2.Text += $"{e.IpPort} connected.{Environment.NewLine}";
                lstClientIP2.Items.Add(e.IpPort);
                lstClientIP2.SetSelected(lstClientIP2.FindString(e.IpPort), true);
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
            HandleMessage(Encoding.UTF8.GetString(e.Data));
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
            HandleMessage(Encoding.UTF8.GetString(e.Data));
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
            HandleMessage(Encoding.UTF8.GetString(e.Data));
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

        private void Events_ClientDisconnected6(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog6.Text += $"{e.IpPort} disconnected.{Environment.NewLine}";
                lstClientIP6.Items.Remove(e.IpPort);
            });
        }

        private void Events_DataReceived6(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog6.Text += $"{e.IpPort} : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
            });
            HandleMessage(Encoding.UTF8.GetString(e.Data));
        }

        private void Events_ClientConnected6(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog6.Text += $"{e.IpPort} connected.{Environment.NewLine}";
                lstClientIP6.Items.Add(e.IpPort);
                lstClientIP6.SetSelected(lstClientIP6.FindString(e.IpPort), true);
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
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string settings = Path.Combine(appdir, "settings.txt");
                var listOfLines = File.ReadAllLines(settings)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("10.119"))
                        {
                            cbxIP6.Text = addr.Address.ToString();
                            btnStart6_Click(this, new EventArgs());
                        }
                        else if (addr.Address.ToString().StartsWith("10.121"))
                        {
                            cbxIP6.Text = addr.Address.ToString();
                            btnStart6_Click(this, new EventArgs());
                        }
                        else if (addr.Address.ToString().StartsWith("192.168"))
                        {
                            cbxIP6.Text = addr.Address.ToString();
                            btnStart6_Click(this, new EventArgs());
                        }
                    }
                }
                foreach (var line in listOfLines)
                {
                    if (line.StartsWith("Server1IP") && !string.IsNullOrEmpty(line.Split('=')[1]))
                    {
                        cbxIP1.Text = line.Split('=')[1];
                    }
                    else if (line.StartsWith("Server2IP") && !string.IsNullOrEmpty(line.Split('=')[1]))
                    {
                        cbxIP2.Text = line.Split('=')[1];
                    }
                    else if (line.StartsWith("Server3IP") && !string.IsNullOrEmpty(line.Split('=')[1]))
                    {
                        cbxIP3.Text = line.Split('=')[1];
                    }
                    else if (line.StartsWith("Server4IP") && !string.IsNullOrEmpty(line.Split('=')[1]))
                    {
                        cbxIP4.Text = line.Split('=')[1];
                    }
                    else if (line.StartsWith("Server5IP") && !string.IsNullOrEmpty(line.Split('=')[1]))
                    {
                        cbxIP5.Text = line.Split('=')[1];
                    }
                    else if (line.StartsWith("Server6IP") && !string.IsNullOrEmpty(line.Split('=')[1]))
                    {
                        cbxIP6.Text = line.Split('=')[1];
                    }
                    else if (line.StartsWith("Server1AutoStart") && line.Split('=')[1].ToLower() == "true")
                    {
                        btnStart1_Click(this, new EventArgs());
                    }
                    else if (line.StartsWith("Server2AutoStart") && line.Split('=')[1].ToLower() == "true")
                    {
                        btnStart2_Click(this, new EventArgs());
                    }
                    else if (line.StartsWith("Server3AutoStart") && line.Split('=')[1].ToLower() == "true")
                    {
                        btnStart3_Click(this, new EventArgs());
                    }
                    else if (line.StartsWith("Server4AutoStart") && line.Split('=')[1].ToLower() == "true")
                    {
                        btnStart4_Click(this, new EventArgs());
                    }
                    else if (line.StartsWith("Server5AutoStart") && line.Split('=')[1].ToLower() == "true")
                    {
                        btnStart5_Click(this, new EventArgs());
                    }
                    else if (line.StartsWith("Server6AutoStart") && line.Split('=')[1].ToLower() == "true")
                    {
                        btnStart6_Click(this, new EventArgs());
                    }
                }                
            }
            catch (Exception)
            {

            }          
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

        private void cbxMessage_DropDown(object sender, EventArgs e)
        {
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string messagelist = Path.Combine(appdir, "messagelist.csv");
                var listOfLines = File.ReadAllLines(messagelist)
                          .Skip(1)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                cbxMessage.Items.Clear();
                foreach (var line in listOfLines)
                {
                    cbxMessage.Items.Add(line.Split(',')[0]);
                }
            }
            catch (Exception)
            {

            }
            
        }

        private void btnNewInstance_Click(object sender, EventArgs e)
        {
            Process p = new Process();
            p.StartInfo.FileName = Application.ExecutablePath;
            p.Start();
        }

        private void btnStop1_Click(object sender, EventArgs e)
        {
            try
            {
                server1.Dispose();
                txtLog1.Text += $"Stopped.{Environment.NewLine}";
                btnStart1.Enabled = true;
                btnStop1.Enabled = false;
            }
            catch (Exception)
            {

            }
        }

        private void btnStop2_Click(object sender, EventArgs e)
        {
            try
            {
                server2.Dispose();
                txtLog2.Text += $"Stopped.{Environment.NewLine}";
                btnStart2.Enabled = true;
                btnStop2.Enabled = false;
            }
            catch (Exception)
            {

            }
        }

        private void btnStop3_Click(object sender, EventArgs e)
        {
            try
            {
                server3.Dispose();
                txtLog3.Text += $"Stopped.{Environment.NewLine}";
                btnStart3.Enabled = true;
                btnStop3.Enabled = false;
            }
            catch (Exception)
            {

            }
        }

        private void btnStop4_Click(object sender, EventArgs e)
        {
            try
            {
                server4.Dispose();
                txtLog4.Text += $"Stopped.{Environment.NewLine}";
                btnStart4.Enabled = true;
                btnStop4.Enabled = false;
            }
            catch (Exception)
            {

            }
        }

        private void btnStop5_Click(object sender, EventArgs e)
        {
            try
            {
                server5.Dispose();
                txtLog5.Text += $"Stopped.{Environment.NewLine}";
                btnStart5.Enabled = true;
                btnStop5.Enabled = false;
            }
            catch (Exception)
            {

            }
        }

        private void btnStop6_Click(object sender, EventArgs e)
        {
            try
            {
                server6.Dispose();
                txtLog6.Text += $"Stopped.{Environment.NewLine}";
                btnStart6.Enabled = true;
                btnStop6.Enabled = false;
            }
            catch (Exception)
            {

            }
        }

        private void txtLog6_TextChanged(object sender, EventArgs e)
        {
            txtLog6.SelectionStart = txtLog6.Text.Length;
            txtLog6.ScrollToCaret();
        }

        private void btnStart6_Click(object sender, EventArgs e)
        {
            try
            {
                server6 = new SimpleTcpServer($"{cbxIP6.Text}:9000");
                server6.Events.ClientConnected += Events_ClientConnected6;
                server6.Events.ClientDisconnected += Events_ClientDisconnected6;
                server6.Events.DataReceived += Events_DataReceived6;
                server6.Start();
                txtLog6.Text += $"Listening...{Environment.NewLine}";
                btnStart6.Enabled = false;
                btnStop6.Enabled = true;
            }
            catch (Exception)
            {

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
                        txtLog1.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
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
                        txtLog2.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
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
                        txtLog3.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
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
                        txtLog4.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
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
                        txtLog5.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server6 != null && server6.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP6.Items.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP6.Items)
                    {
                        server6.Send(ip.ToString(), cbxMessage.Text);
                        txtLog6.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }
            cbxMessage.SelectAll();
        }
    }
}
