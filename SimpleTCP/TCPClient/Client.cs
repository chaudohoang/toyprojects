using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SuperSimpleTcp;

namespace TCPClient
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }

        SimpleTcpClient client;

        private void Events_Disconnected(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog.Text += $"Server disconnected.{Environment.NewLine}";
                btnSend.Enabled = false;
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

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (client.IsConnected)
            {
                if (!string.IsNullOrEmpty(txtMessage.Text))
                {
                    client.Send(txtMessage.Text);
                    txtLog.Text += $"Me : {txtMessage.Text}{Environment.NewLine}";
                    txtMessage.Text = string.Empty;
                }
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                client = new(txtIP.Text);
                client.Keepalive.EnableTcpKeepAlives = true;
                client.Keepalive.TcpKeepAliveInterval = 5;
                client.Keepalive.TcpKeepAliveTime = 5;
                client.Keepalive.TcpKeepAliveRetryCount = 5;
                client.Events.Connected += Events_Connected;
                client.Events.DataReceived += Events_DataReceived;
                client.Events.Disconnected += Events_Disconnected;
                client.ConnectWithRetries(60);
                btnConnect.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(this, new EventArgs());
            }
        }
    }
}
