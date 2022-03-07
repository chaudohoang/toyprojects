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

namespace TCPServer
{
    public partial class Server : Form
    {
        public Server()
        {
            InitializeComponent();
        }

        SimpleTcpServer server;
        private void btnStart_Click(object sender, EventArgs e)
        {
            server = new SimpleTcpServer(txtIP.Text);
            server.Events.ClientConnected += Events_ClientConnected;
            server.Events.ClientDisconnected += Events_ClientDisconnected;
            server.Events.DataReceived += Events_DataReceived;
            server.Keepalive.EnableTcpKeepAlives = true;
            server.Keepalive.TcpKeepAliveInterval = 5;      // seconds to wait before sending subsequent keepalive
            server.Keepalive.TcpKeepAliveTime = 5;          // seconds to wait before sending a keepalive
            server.Keepalive.TcpKeepAliveRetryCount = 5;
            server.Start();
            txtLog.Text += $"Starting...{Environment.NewLine}";
            btnStart.Enabled = false;
            btnSend.Enabled = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            btnSend.Enabled = false;
            server = new SimpleTcpServer(txtIP.Text);
            server.Keepalive.EnableTcpKeepAlives = true;
            server.Keepalive.TcpKeepAliveInterval = 5;      // seconds to wait before sending subsequent keepalive
            server.Keepalive.TcpKeepAliveTime = 5;          // seconds to wait before sending a keepalive
            server.Keepalive.TcpKeepAliveRetryCount = 5;
            server.Events.ClientConnected += Events_ClientConnected;
            server.Events.ClientDisconnected += Events_ClientDisconnected;
            server.Events.DataReceived += Events_DataReceived;
        }

        private void Events_DataReceived(object sender, SuperSimpleTcp.DataReceivedEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog.Text += $"{e.IpPort} : {Encoding.UTF8.GetString(e.Data)}{Environment.NewLine}";
            });
            if (Encoding.UTF8.GetString(e.Data)=="notepad")
            {
                foreach (var process in Process.GetProcessesByName("notepad"))
                {
                    process.Kill();
                }
                Process.Start("notepad.exe");
            }
        }

        private void Events_ClientDisconnected(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog.Text += $"{e.IpPort} disconnected.{Environment.NewLine}";
                lstClientIP.Items.Remove(e.IpPort);
            });
            
        }

        private void Events_ClientConnected(object sender, SuperSimpleTcp.ConnectionEventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                txtLog.Text += $"{e.IpPort} connected.{Environment.NewLine}";
                lstClientIP.Items.Add(e.IpPort);
            });
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (server.IsListening)
            {
                if (!string.IsNullOrEmpty(txtMessage.Text) && lstClientIP.SelectedItem !=null) //check message & select client ip from listbox
                {
                    server.Send(lstClientIP.SelectedItem.ToString(), txtMessage.Text);
                    txtLog.Text += $"Server->{lstClientIP.SelectedItem.ToString()} : {txtMessage.Text}{Environment.NewLine}";
                    txtMessage.Text = string.Empty;
                }
            }
        }

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnSend_Click(this, new EventArgs());
            }
        }

        private void btnSendAll_Click(object sender, EventArgs e)
        {
            if (server.IsListening)
            {
                if (!string.IsNullOrEmpty(txtMessage.Text) && (lstClientIP.Items.Count > 0)) //check message & select client ip from listbox
                {
                    foreach (var ip in lstClientIP.Items)
                    {
                        server.Send(ip.ToString(), txtMessage.Text);
                        txtLog.Text += $"Server->{ip.ToString()} : {txtMessage.Text}{Environment.NewLine}";                        
                    }
                    txtMessage.Text = string.Empty;
                }
            }
        }
    }
}
