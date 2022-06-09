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

        private void btnConnect_Click(object sender, EventArgs e)
        {            
            try
            {
                if (cbxIP.Text.Contains(","))
                {
                    client = new SimpleTcpClient($"{cbxIP.Text.Split(',')[1]}:9000");
                }
                else client = new SimpleTcpClient($"{cbxIP.Text}:9000");
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
            HandleMessage(Encoding.UTF8.GetString(e.Data));
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
                }
            }
            cbxMessage.SelectAll();
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

        private void cbxIP_DropDown(object sender, EventArgs e)
        {
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                string messagelist = Path.Combine(appdir, "serverlist.csv");
                var listOfLines = File.ReadAllLines(messagelist)
                          .Where(x => !string.IsNullOrWhiteSpace(x));
                cbxIP.Items.Clear();
                cbxIP.Items.Add("192.168.0.100");
                cbxIP.Items.Add("192.168.1.1");
                cbxIP.Items.Add("192.168.2.2");
                cbxIP.Items.Add("192.168.3.3");
                cbxIP.Items.Add("192.168.4.4");
                cbxIP.Items.Add("127.0.0.1");
                foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
                {
                    IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                    foreach (UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                    {
                        if (addr.Address.ToString().StartsWith("192.168"))
                        {
                            cbxIP.Items.Add(addr.Address.ToString());
                        }

                    }
                }
                foreach (var line in listOfLines)
                {                                    
                    cbxIP.Items.Add(line);
                }
                
            }
            catch (Exception)
            {

            }
        }
    }
}
