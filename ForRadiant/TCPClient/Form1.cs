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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        SimpleTcpClient client;

        private void btnConnect_Click(object sender, EventArgs e)
        {
            string IPPort="";
            if (cbxIP.Text=="Dooone1")
            {
                IPPort = "192.168.1.1:9000";
            }
            else if(cbxIP.Text == "Dooone2")
            {
                IPPort = "192.168.2.2:9000";
            }
            else if (cbxIP.Text == "Dooone3")
            {
                IPPort = "192.168.3.3:9000";
            }
            else if (cbxIP.Text == "Dooone4")
            {
                IPPort = "192.168.4.4:9000";
            }
            else if (cbxIP.Text == "Gooil")
            {
                IPPort = "192.168.0.50:9000";
            }
            else IPPort = "127.0.0.1:9000";
            client = new SimpleTcpClient(IPPort);
            client.Events.Connected += Events_Connected;
            client.Events.DataReceived += Events_DataReceived;
            client.Events.Disconnected += Events_Disconnected;
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
            if (Encoding.UTF8.GetString(e.Data) == "notepad")
            {
                foreach (var process in Process.GetProcessesByName("notepad"))
                {
                    process.Kill();
                }
                Process.Start("notepad.exe");
            }
        }

        private void Events_Connected(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog.Text += $"Server connected.{Environment.NewLine}";
            });
        }
    }
}
