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
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SHOWME)
            {
                ShowMe();
            }
            base.WndProc(ref m);
        }
        private void ShowMe()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            // get our current "TopMost" value (ours will always be false though)
            bool top = TopMost;
            // make our form jump to the top of everything
            TopMost = true;
            // set it back to whatever it was
            TopMost = top;
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

        SimpleTcpServer server1,server2,server3,server4,server5,server6,server7,server8;
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
            else if (message == "Shutdown TrueTest")
            {
                try
                {
                    foreach (var process in Process.GetProcessesByName("TrueTest"))
                    {
                        process.Kill();
                    }
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
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
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
            if (tabControl1.SelectedTab == tabPage1 && server1 != null && server1.IsListening)
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

            else if (tabControl1.SelectedTab == tabPage2 && server2 != null && server2.IsListening)
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

            else if (tabControl1.SelectedTab == tabPage3 && server3 != null && server3.IsListening)
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

            else if (tabControl1.SelectedTab == tabPage4 && server4 != null && server4.IsListening)
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

            else if (tabControl1.SelectedTab == tabPage5 && server5 != null && server5.IsListening)
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

            else if (tabControl1.SelectedTab == tabPage6 && server6 != null && server6.IsListening)
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

            else if (tabControl1.SelectedTab == tabPage7 && server7 != null && server7.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP7.SelectedItems.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP7.SelectedItems)
                    {
                        server7.Send(ip.ToString(), cbxMessage.Text);
                        txtLog7.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            else if (tabControl1.SelectedTab == tabPage8 && server8 != null && server8.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP8.SelectedItems.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP8.SelectedItems)
                    {
                        server8.Send(ip.ToString(), cbxMessage.Text);
                        txtLog8.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }
            cbxMessage.SelectAll();
        }

        private void btnStart1_Click(object sender, EventArgs e)
        {
            try
            {
                string ip;
                if (cbxIP1.Text.Contains(","))
                {
                    ip = cbxIP1.Text.Split(',')[1];
                }
                else ip = cbxIP1.Text;
                server1 = new SimpleTcpServer($"{ip}:9000");
                server1.Events.ClientConnected += Events_ClientConnected1;
                server1.Events.ClientDisconnected += Events_ClientDisconnected1;
                server1.Events.DataReceived += Events_DataReceived1;
                server1.Start();
                txtLog1.Text += $"Listening on {ip}...{Environment.NewLine}";
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
                string ip;
                if (cbxIP2.Text.Contains(","))
                {
                    ip = cbxIP2.Text.Split(',')[1];
                }
                else ip = cbxIP2.Text;
                server2 = new SimpleTcpServer($"{ip}:9000");
                server2.Events.ClientConnected += Events_ClientConnected2;
                server2.Events.ClientDisconnected += Events_ClientDisconnected2;
                server2.Events.DataReceived += Events_DataReceived2;
                server2.Start();
                txtLog2.Text += $"Listening on {ip}...{Environment.NewLine}";
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
                string ip;
                if (cbxIP3.Text.Contains(","))
                {
                    ip = cbxIP3.Text.Split(',')[1];
                }
                else ip = cbxIP3.Text;
                server3 = new SimpleTcpServer($"{ip}:9000");
                server3.Events.ClientConnected += Events_ClientConnected3;
                server3.Events.ClientDisconnected += Events_ClientDisconnected3;
                server3.Events.DataReceived += Events_DataReceived3;
                server3.Start();
                txtLog3.Text += $"Listening on {ip}...{Environment.NewLine}";
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
                string ip;
                if (cbxIP4.Text.Contains(","))
                {
                    ip = cbxIP4.Text.Split(',')[1];
                }
                else ip = cbxIP4.Text;
                server4 = new SimpleTcpServer($"{ip}:9000");
                server4.Events.ClientConnected += Events_ClientConnected4;
                server4.Events.ClientDisconnected += Events_ClientDisconnected4;
                server4.Events.DataReceived += Events_DataReceived4;
                server4.Start();
                txtLog4.Text += $"Listening on {ip}...{Environment.NewLine}";
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
                string ip;
                if (cbxIP5.Text.Contains(","))
                {
                    ip = cbxIP5.Text.Split(',')[1];
                }
                else ip = cbxIP5.Text;
                server5 = new SimpleTcpServer($"{ip}:9000");
                server5.Events.ClientConnected += Events_ClientConnected5;
                server5.Events.ClientDisconnected += Events_ClientDisconnected5;
                server5.Events.DataReceived += Events_DataReceived5;
                server5.Start();
                txtLog5.Text += $"Listening on {ip}...{Environment.NewLine}";
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

        private void Events_ClientDisconnected7(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog7.Text += $"{e.IpPort} disconnected.{Environment.NewLine}";
                lstClientIP7.Items.Remove(e.IpPort);
            });
        }

        private void Events_ClientDisconnected8(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog8.Text += $"{e.IpPort} disconnected.{Environment.NewLine}";
                lstClientIP8.Items.Remove(e.IpPort);
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

        private void Events_DataReceived7(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog7.Text += $"{e.IpPort} : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
            });
            HandleMessage(Encoding.UTF8.GetString(e.Data));
        }

        private void Events_DataReceived8(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog8.Text += $"{e.IpPort} : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
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

        private void Events_ClientConnected7(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog7.Text += $"{e.IpPort} connected.{Environment.NewLine}";
                lstClientIP7.Items.Add(e.IpPort);
                lstClientIP7.SetSelected(lstClientIP7.FindString(e.IpPort), true);
            });
        }

        private void Events_ClientConnected8(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog8.Text += $"{e.IpPort} connected.{Environment.NewLine}";
                lstClientIP8.Items.Add(e.IpPort);
                lstClientIP8.SetSelected(lstClientIP8.FindString(e.IpPort), true);
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
                bool listened = false;
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("10.119"))
                        {
                            cbxIP6.Text = addr.Address.ToString();
                            btnStart8_Click(this, new EventArgs());
                            listened = true;
                            break;
                        }
                        else if (addr.Address.ToString().StartsWith("10.121"))
                        {
                            cbxIP6.Text = addr.Address.ToString();
                            btnStart8_Click(this, new EventArgs());
                            listened = true;
                            break;
                        }

                    }
					if (listened)
					{
                        break;
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
                    else if (line.StartsWith("Server7IP") && !string.IsNullOrEmpty(line.Split('=')[1]))
                    {
                        cbxIP7.Text = line.Split('=')[1];
                    }
                    else if (line.StartsWith("Server8IP") && !string.IsNullOrEmpty(line.Split('=')[1]))
                    {
                        cbxIP8.Text = line.Split('=')[1];
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
                    else if (line.StartsWith("Server7AutoStart") && line.Split('=')[1].ToLower() == "true")
                    {
                        btnStart7_Click(this, new EventArgs());
                    }
                    else if (line.StartsWith("Server8AutoStart") && line.Split('=')[1].ToLower() == "true")
                    {
                        btnStart8_Click(this, new EventArgs());
                    }
                }                
            }
            catch (Exception)
            {

            }          
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

        private void btnStop7_Click(object sender, EventArgs e)
        {
            try
            {
                server7.Dispose();
                txtLog7.Text += $"Stopped.{Environment.NewLine}";
                btnStart7.Enabled = true;
                btnStop7.Enabled = false;
            }
            catch (Exception)
            {

            }
        }

        private void btnStop8_Click(object sender, EventArgs e)
        {
            try
            {
                server8.Dispose();
                txtLog8.Text += $"Stopped.{Environment.NewLine}";
                btnStart8.Enabled = true;
                btnStop8.Enabled = false;
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

        private void txtLog7_TextChanged(object sender, EventArgs e)
        {
            txtLog7.SelectionStart = txtLog7.Text.Length;
            txtLog7.ScrollToCaret();
        }

        private void txtLog8_TextChanged(object sender, EventArgs e)
        {
            txtLog8.SelectionStart = txtLog8.Text.Length;
            txtLog8.ScrollToCaret();
        }

        private void cbxIP1_DropDown(object sender, EventArgs e)
		{
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string messagelist = Path.Combine(appdir, "serverlist.csv");
                var listOfLines = File.ReadAllLines(messagelist)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                cbxIP1.Items.Clear();
                cbxIP1.Items.Add("192.168.0.100");
                cbxIP1.Items.Add("192.168.1.1");
                cbxIP1.Items.Add("192.168.2.2");
                cbxIP1.Items.Add("192.168.3.3");
                cbxIP1.Items.Add("192.168.4.4");
                cbxIP1.Items.Add("192.168.0.11");
                cbxIP1.Items.Add("192.168.1.100");
                cbxIP1.Items.Add("127.0.0.1");
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("192.168"))
                        {
                            cbxIP1.Items.Add(addr.Address.ToString());
                        }

                    }
                }
                foreach (var line in listOfLines)
                {
                    cbxIP1.Items.Add(line);
                }

            }
            catch (Exception)
            {

            }
        }

		private void cbxIP2_DropDown(object sender, EventArgs e)
		{
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string messagelist = Path.Combine(appdir, "serverlist.csv");
                var listOfLines = File.ReadAllLines(messagelist)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                cbxIP2.Items.Clear();
                cbxIP2.Items.Add("192.168.0.100");
                cbxIP2.Items.Add("192.168.1.1");
                cbxIP2.Items.Add("192.168.2.2");
                cbxIP2.Items.Add("192.168.3.3");
                cbxIP2.Items.Add("192.168.4.4");
                cbxIP2.Items.Add("192.168.0.11");
                cbxIP2.Items.Add("192.168.1.100");
                cbxIP2.Items.Add("127.0.0.1");
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("192.168"))
                        {
                            cbxIP2.Items.Add(addr.Address.ToString());
                        }

                    }
                }
                foreach (var line in listOfLines)
                {
                    cbxIP2.Items.Add(line);
                }

            }
            catch (Exception)
            {

            }
        }

		private void cbxIP3_DropDown(object sender, EventArgs e)
		{
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string messagelist = Path.Combine(appdir, "serverlist.csv");
                var listOfLines = File.ReadAllLines(messagelist)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                cbxIP3.Items.Clear();
                cbxIP3.Items.Add("192.168.0.100");
                cbxIP3.Items.Add("192.168.1.1");
                cbxIP3.Items.Add("192.168.2.2");
                cbxIP3.Items.Add("192.168.3.3");
                cbxIP3.Items.Add("192.168.4.4");
                cbxIP3.Items.Add("192.168.0.11");
                cbxIP3.Items.Add("192.168.1.100");
                cbxIP3.Items.Add("127.0.0.1");
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("192.168"))
                        {
                            cbxIP3.Items.Add(addr.Address.ToString());
                        }

                    }
                }
                foreach (var line in listOfLines)
                {
                    cbxIP3.Items.Add(line);
                }

            }
            catch (Exception)
            {

            }
        }

		private void cbxIP4_DropDown(object sender, EventArgs e)
		{
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string messagelist = Path.Combine(appdir, "serverlist.csv");
                var listOfLines = File.ReadAllLines(messagelist)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                cbxIP4.Items.Clear();
                cbxIP4.Items.Add("192.168.0.100");
                cbxIP4.Items.Add("192.168.1.1");
                cbxIP4.Items.Add("192.168.2.2");
                cbxIP4.Items.Add("192.168.3.3");
                cbxIP4.Items.Add("192.168.4.4");
                cbxIP4.Items.Add("192.168.0.11");
                cbxIP4.Items.Add("192.168.1.100");
                cbxIP4.Items.Add("127.0.0.1");
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("192.168"))
                        {
                            cbxIP4.Items.Add(addr.Address.ToString());
                        }

                    }
                }
                foreach (var line in listOfLines)
                {
                    cbxIP4.Items.Add(line);
                }

            }
            catch (Exception)
            {

            }
        }

		private void cbxIP5_DropDown(object sender, EventArgs e)
		{
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string messagelist = Path.Combine(appdir, "serverlist.csv");
                var listOfLines = File.ReadAllLines(messagelist)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                cbxIP5.Items.Clear();
                cbxIP5.Items.Add("192.168.0.100");
                cbxIP5.Items.Add("192.168.1.1");
                cbxIP5.Items.Add("192.168.2.2");
                cbxIP5.Items.Add("192.168.3.3");
                cbxIP5.Items.Add("192.168.4.4");
                cbxIP5.Items.Add("192.168.0.11");
                cbxIP5.Items.Add("192.168.1.100");
                cbxIP5.Items.Add("127.0.0.1");
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("192.168"))
                        {
                            cbxIP5.Items.Add(addr.Address.ToString());
                        }

                    }
                }
                foreach (var line in listOfLines)
                {
                    cbxIP5.Items.Add(line);
                }

            }
            catch (Exception)
            {

            }
        }

		private void cbxIP6_DropDown(object sender, EventArgs e)
		{
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string messagelist = Path.Combine(appdir, "serverlist.csv");
                var listOfLines = File.ReadAllLines(messagelist)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                cbxIP6.Items.Clear();
                cbxIP6.Items.Add("192.168.0.100");
                cbxIP6.Items.Add("192.168.1.1");
                cbxIP6.Items.Add("192.168.2.2");
                cbxIP6.Items.Add("192.168.3.3");
                cbxIP6.Items.Add("192.168.4.4");
                cbxIP6.Items.Add("192.168.0.11");
                cbxIP6.Items.Add("192.168.1.100");
                cbxIP6.Items.Add("127.0.0.1");
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("192.168"))
                        {
                            cbxIP6.Items.Add(addr.Address.ToString());
                        }

                    }
                }
                foreach (var line in listOfLines)
                {
                    cbxIP6.Items.Add(line);
                }

            }
            catch (Exception)
            {

            }
        }

        private void cbxIP7_DropDown(object sender, EventArgs e)
        {
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string messagelist = Path.Combine(appdir, "serverlist.csv");
                var listOfLines = File.ReadAllLines(messagelist)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                cbxIP7.Items.Clear();
                cbxIP7.Items.Add("192.168.0.100");
                cbxIP7.Items.Add("192.168.1.1");
                cbxIP7.Items.Add("192.168.2.2");
                cbxIP7.Items.Add("192.168.3.3");
                cbxIP7.Items.Add("192.168.4.4");
                cbxIP7.Items.Add("192.168.0.11");
                cbxIP7.Items.Add("192.168.1.100");
                cbxIP7.Items.Add("127.0.0.1");
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("192.168"))
                        {
                            cbxIP7.Items.Add(addr.Address.ToString());
                        }

                    }
                }
                foreach (var line in listOfLines)
                {
                    cbxIP7.Items.Add(line);
                }

            }
            catch (Exception)
            {

            }
        }

        private void cbxIP8_DropDown(object sender, EventArgs e)
        {
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string messagelist = Path.Combine(appdir, "serverlist.csv");
                var listOfLines = File.ReadAllLines(messagelist)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                cbxIP8.Items.Clear();
                cbxIP8.Items.Add("192.168.0.100");
                cbxIP8.Items.Add("192.168.1.1");
                cbxIP8.Items.Add("192.168.2.2");
                cbxIP8.Items.Add("192.168.3.3");
                cbxIP8.Items.Add("192.168.4.4");
                cbxIP8.Items.Add("192.168.0.11");
                cbxIP8.Items.Add("192.168.1.100");
                cbxIP8.Items.Add("127.0.0.1");
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("192.168"))
                        {
                            cbxIP8.Items.Add(addr.Address.ToString());
                        }

                    }
                }
                foreach (var line in listOfLines)
                {
                    cbxIP8.Items.Add(line);
                }

            }
            catch (Exception)
            {

            }
        }

        private void btnStart6_Click(object sender, EventArgs e)
        {
            try
            {
                string ip;
                if (cbxIP6.Text.Contains(","))
                {
                    ip = cbxIP6.Text.Split(',')[1];
                }
                else ip = cbxIP6.Text;
                server6 = new SimpleTcpServer($"{ip}:9000");
                server6.Events.ClientConnected += Events_ClientConnected6;
                server6.Events.ClientDisconnected += Events_ClientDisconnected6;
                server6.Events.DataReceived += Events_DataReceived6;
                server6.Start();
                txtLog6.Text += $"Listening on {ip}...{Environment.NewLine}";
                btnStart6.Enabled = false;
                btnStop6.Enabled = true;
            }
            catch (Exception)
            {

            }
        }

        private void btnStart7_Click(object sender, EventArgs e)
        {
            try
            {
                string ip;
                if (cbxIP7.Text.Contains(","))
                {
                    ip = cbxIP7.Text.Split(',')[1];
                }
                else ip = cbxIP7.Text;
                server7 = new SimpleTcpServer($"{ip}:9000");
                server7.Events.ClientConnected += Events_ClientConnected7;
                server7.Events.ClientDisconnected += Events_ClientDisconnected7;
                server7.Events.DataReceived += Events_DataReceived7;
                server7.Start();
                txtLog7.Text += $"Listening on {ip}...{Environment.NewLine}";
                btnStart7.Enabled = false;
                btnStop7.Enabled = true;
            }
            catch (Exception)
            {

            }
        }

        private void btnStart8_Click(object sender, EventArgs e)
        {
            try
            {
                string ip;
                if (cbxIP8.Text.Contains(","))
                {
                    ip = cbxIP8.Text.Split(',')[1];
                }
                else ip = cbxIP8.Text;
                server8 = new SimpleTcpServer($"{ip}:9000");
                server8.Events.ClientConnected += Events_ClientConnected8;
                server8.Events.ClientDisconnected += Events_ClientDisconnected8;
                server8.Events.DataReceived += Events_DataReceived8;
                server8.Start();
                txtLog8.Text += $"Listening on {ip}...{Environment.NewLine}";
                btnStart8.Enabled = false;
                btnStop8.Enabled = true;
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

            if (server7 != null && server7.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP7.Items.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP7.Items)
                    {
                        server7.Send(ip.ToString(), cbxMessage.Text);
                        txtLog7.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }

            if (server8 != null && server8.IsListening)
            {
                if (!string.IsNullOrEmpty(cbxMessage.Text) && (lstClientIP8.Items.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP8.Items)
                    {
                        server8.Send(ip.ToString(), cbxMessage.Text);
                        txtLog8.Text += $"-->{ip} : {cbxMessage.Text}{Environment.NewLine}";
                    }
                }
            }
            cbxMessage.SelectAll();
        }
    }
}
