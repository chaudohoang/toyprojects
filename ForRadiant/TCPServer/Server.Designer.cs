
namespace TCPServer
{
    partial class Server
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Server));
			this.label1 = new System.Windows.Forms.Label();
			this.cbxIP1 = new System.Windows.Forms.ComboBox();
			this.txtLog1 = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btnStart1 = new System.Windows.Forms.Button();
			this.cbxMessage = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btnSend = new System.Windows.Forms.Button();
			this.btnSendAll = new System.Windows.Forms.Button();
			this.label4 = new System.Windows.Forms.Label();
			this.lstClientIP1 = new System.Windows.Forms.ListBox();
			this.lstClientIP2 = new System.Windows.Forms.ListBox();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.txtLog2 = new System.Windows.Forms.TextBox();
			this.cbxIP2 = new System.Windows.Forms.ComboBox();
			this.label7 = new System.Windows.Forms.Label();
			this.lstClientIP3 = new System.Windows.Forms.ListBox();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.txtLog3 = new System.Windows.Forms.TextBox();
			this.cbxIP3 = new System.Windows.Forms.ComboBox();
			this.label10 = new System.Windows.Forms.Label();
			this.lstClientIP4 = new System.Windows.Forms.ListBox();
			this.label11 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.txtLog4 = new System.Windows.Forms.TextBox();
			this.cbxIP4 = new System.Windows.Forms.ComboBox();
			this.label13 = new System.Windows.Forms.Label();
			this.lstClientIP5 = new System.Windows.Forms.ListBox();
			this.label14 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.txtLog5 = new System.Windows.Forms.TextBox();
			this.cbxIP5 = new System.Windows.Forms.ComboBox();
			this.label16 = new System.Windows.Forms.Label();
			this.btnStart2 = new System.Windows.Forms.Button();
			this.btnStart3 = new System.Windows.Forms.Button();
			this.btnStart4 = new System.Windows.Forms.Button();
			this.btnStart5 = new System.Windows.Forms.Button();
			this.btnRestart = new System.Windows.Forms.Button();
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.restartToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.btnNewInstance = new System.Windows.Forms.Button();
			this.btnStop1 = new System.Windows.Forms.Button();
			this.btnStop2 = new System.Windows.Forms.Button();
			this.btnStop3 = new System.Windows.Forms.Button();
			this.btnStop4 = new System.Windows.Forms.Button();
			this.btnStop5 = new System.Windows.Forms.Button();
			this.btnStop6 = new System.Windows.Forms.Button();
			this.btnStart6 = new System.Windows.Forms.Button();
			this.lstClientIP6 = new System.Windows.Forms.ListBox();
			this.label17 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.txtLog6 = new System.Windows.Forms.TextBox();
			this.cbxIP6 = new System.Windows.Forms.ComboBox();
			this.label19 = new System.Windows.Forms.Label();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.tabPage6 = new System.Windows.Forms.TabPage();
			this.contextMenuStrip1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.tabPage5.SuspendLayout();
			this.tabPage6.SuspendLayout();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(10, 11);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(47, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Server : ";
			// 
			// cbxIP1
			// 
			this.cbxIP1.FormattingEnabled = true;
			this.cbxIP1.Items.AddRange(new object[] {
            "192.168.0.100",
            "192.168.1.1",
            "192.168.2.2",
            "192.168.3.3",
            "192.168.4.4",
            "127.0.0.1"});
			this.cbxIP1.Location = new System.Drawing.Point(63, 8);
			this.cbxIP1.Name = "cbxIP1";
			this.cbxIP1.Size = new System.Drawing.Size(237, 21);
			this.cbxIP1.TabIndex = 1;
			this.cbxIP1.Text = "127.0.0.1";
			this.cbxIP1.DropDown += new System.EventHandler(this.cbxIP1_DropDown);
			// 
			// txtLog1
			// 
			this.txtLog1.Location = new System.Drawing.Point(6, 55);
			this.txtLog1.Multiline = true;
			this.txtLog1.Name = "txtLog1";
			this.txtLog1.ReadOnly = true;
			this.txtLog1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog1.Size = new System.Drawing.Size(525, 396);
			this.txtLog1.TabIndex = 23;
			this.txtLog1.TextChanged += new System.EventHandler(this.txtLog1_TextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(11, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Log : ";
			// 
			// btnStart1
			// 
			this.btnStart1.Location = new System.Drawing.Point(314, 6);
			this.btnStart1.Name = "btnStart1";
			this.btnStart1.Size = new System.Drawing.Size(55, 23);
			this.btnStart1.TabIndex = 3;
			this.btnStart1.Text = "Start";
			this.btnStart1.UseVisualStyleBackColor = true;
			this.btnStart1.Click += new System.EventHandler(this.btnStart1_Click);
			// 
			// cbxMessage
			// 
			this.cbxMessage.FormattingEnabled = true;
			this.cbxMessage.Items.AddRange(new object[] {
            "Restart TrueTest",
            "Open Notepad"});
			this.cbxMessage.Location = new System.Drawing.Point(74, 12);
			this.cbxMessage.Name = "cbxMessage";
			this.cbxMessage.Size = new System.Drawing.Size(646, 21);
			this.cbxMessage.TabIndex = 20;
			this.cbxMessage.DropDown += new System.EventHandler(this.cbxMessage_DropDown);
			this.cbxMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbxMessage_KeyDown);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(9, 15);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(59, 13);
			this.label3.TabIndex = 5;
			this.label3.Text = "Message : ";
			// 
			// btnSend
			// 
			this.btnSend.Location = new System.Drawing.Point(256, 40);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(55, 23);
			this.btnSend.TabIndex = 21;
			this.btnSend.Text = "Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// btnSendAll
			// 
			this.btnSendAll.Location = new System.Drawing.Point(195, 40);
			this.btnSendAll.Name = "btnSendAll";
			this.btnSendAll.Size = new System.Drawing.Size(55, 23);
			this.btnSendAll.TabIndex = 22;
			this.btnSendAll.Text = "Send All";
			this.btnSendAll.UseVisualStyleBackColor = true;
			this.btnSendAll.Click += new System.EventHandler(this.btnSendAll_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(476, 15);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(55, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Client IP : ";
			// 
			// lstClientIP1
			// 
			this.lstClientIP1.FormattingEnabled = true;
			this.lstClientIP1.Location = new System.Drawing.Point(537, 6);
			this.lstClientIP1.Name = "lstClientIP1";
			this.lstClientIP1.ScrollAlwaysVisible = true;
			this.lstClientIP1.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.lstClientIP1.Size = new System.Drawing.Size(161, 446);
			this.lstClientIP1.TabIndex = 24;
			// 
			// lstClientIP2
			// 
			this.lstClientIP2.FormattingEnabled = true;
			this.lstClientIP2.Location = new System.Drawing.Point(537, 6);
			this.lstClientIP2.Name = "lstClientIP2";
			this.lstClientIP2.ScrollAlwaysVisible = true;
			this.lstClientIP2.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.lstClientIP2.Size = new System.Drawing.Size(161, 446);
			this.lstClientIP2.TabIndex = 26;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(476, 15);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(55, 13);
			this.label5.TabIndex = 13;
			this.label5.Text = "Client IP : ";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(11, 32);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(34, 13);
			this.label6.TabIndex = 12;
			this.label6.Text = "Log : ";
			// 
			// txtLog2
			// 
			this.txtLog2.Location = new System.Drawing.Point(6, 55);
			this.txtLog2.Multiline = true;
			this.txtLog2.Name = "txtLog2";
			this.txtLog2.ReadOnly = true;
			this.txtLog2.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog2.Size = new System.Drawing.Size(525, 396);
			this.txtLog2.TabIndex = 25;
			this.txtLog2.TextChanged += new System.EventHandler(this.txtLog2_TextChanged);
			// 
			// cbxIP2
			// 
			this.cbxIP2.FormattingEnabled = true;
			this.cbxIP2.Items.AddRange(new object[] {
            "192.168.0.100",
            "192.168.1.1",
            "192.168.2.2",
            "192.168.3.3",
            "192.168.4.4",
            "127.0.0.1"});
			this.cbxIP2.Location = new System.Drawing.Point(63, 8);
			this.cbxIP2.Name = "cbxIP2";
			this.cbxIP2.Size = new System.Drawing.Size(237, 21);
			this.cbxIP2.TabIndex = 5;
			this.cbxIP2.Text = "127.0.0.1";
			this.cbxIP2.DropDown += new System.EventHandler(this.cbxIP2_DropDown);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(10, 11);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(47, 13);
			this.label7.TabIndex = 9;
			this.label7.Text = "Server : ";
			// 
			// lstClientIP3
			// 
			this.lstClientIP3.FormattingEnabled = true;
			this.lstClientIP3.Location = new System.Drawing.Point(537, 6);
			this.lstClientIP3.Name = "lstClientIP3";
			this.lstClientIP3.ScrollAlwaysVisible = true;
			this.lstClientIP3.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.lstClientIP3.Size = new System.Drawing.Size(161, 446);
			this.lstClientIP3.TabIndex = 28;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(476, 15);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(55, 13);
			this.label8.TabIndex = 19;
			this.label8.Text = "Client IP : ";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(11, 32);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(34, 13);
			this.label9.TabIndex = 18;
			this.label9.Text = "Log : ";
			// 
			// txtLog3
			// 
			this.txtLog3.Location = new System.Drawing.Point(6, 55);
			this.txtLog3.Multiline = true;
			this.txtLog3.Name = "txtLog3";
			this.txtLog3.ReadOnly = true;
			this.txtLog3.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog3.Size = new System.Drawing.Size(525, 396);
			this.txtLog3.TabIndex = 27;
			this.txtLog3.TextChanged += new System.EventHandler(this.txtLog3_TextChanged);
			// 
			// cbxIP3
			// 
			this.cbxIP3.FormattingEnabled = true;
			this.cbxIP3.Items.AddRange(new object[] {
            "192.168.0.100",
            "192.168.1.1",
            "192.168.2.2",
            "192.168.3.3",
            "192.168.4.4",
            "127.0.0.1"});
			this.cbxIP3.Location = new System.Drawing.Point(63, 8);
			this.cbxIP3.Name = "cbxIP3";
			this.cbxIP3.Size = new System.Drawing.Size(237, 21);
			this.cbxIP3.TabIndex = 8;
			this.cbxIP3.Text = "127.0.0.1";
			this.cbxIP3.DropDown += new System.EventHandler(this.cbxIP3_DropDown);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(10, 11);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(47, 13);
			this.label10.TabIndex = 15;
			this.label10.Text = "Server : ";
			// 
			// lstClientIP4
			// 
			this.lstClientIP4.FormattingEnabled = true;
			this.lstClientIP4.Location = new System.Drawing.Point(537, 6);
			this.lstClientIP4.Name = "lstClientIP4";
			this.lstClientIP4.ScrollAlwaysVisible = true;
			this.lstClientIP4.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.lstClientIP4.Size = new System.Drawing.Size(161, 446);
			this.lstClientIP4.TabIndex = 30;
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(476, 15);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(55, 13);
			this.label11.TabIndex = 25;
			this.label11.Text = "Client IP : ";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(11, 32);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(34, 13);
			this.label12.TabIndex = 24;
			this.label12.Text = "Log : ";
			// 
			// txtLog4
			// 
			this.txtLog4.Location = new System.Drawing.Point(6, 55);
			this.txtLog4.Multiline = true;
			this.txtLog4.Name = "txtLog4";
			this.txtLog4.ReadOnly = true;
			this.txtLog4.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog4.Size = new System.Drawing.Size(525, 396);
			this.txtLog4.TabIndex = 29;
			this.txtLog4.TextChanged += new System.EventHandler(this.txtLog4_TextChanged);
			// 
			// cbxIP4
			// 
			this.cbxIP4.FormattingEnabled = true;
			this.cbxIP4.Items.AddRange(new object[] {
            "192.168.0.100",
            "192.168.1.1",
            "192.168.2.2",
            "192.168.3.3",
            "192.168.4.4",
            "127.0.0.1"});
			this.cbxIP4.Location = new System.Drawing.Point(63, 8);
			this.cbxIP4.Name = "cbxIP4";
			this.cbxIP4.Size = new System.Drawing.Size(237, 21);
			this.cbxIP4.TabIndex = 11;
			this.cbxIP4.Text = "127.0.0.1";
			this.cbxIP4.DropDown += new System.EventHandler(this.cbxIP4_DropDown);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(10, 11);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(47, 13);
			this.label13.TabIndex = 21;
			this.label13.Text = "Server : ";
			// 
			// lstClientIP5
			// 
			this.lstClientIP5.FormattingEnabled = true;
			this.lstClientIP5.Location = new System.Drawing.Point(537, 6);
			this.lstClientIP5.Name = "lstClientIP5";
			this.lstClientIP5.ScrollAlwaysVisible = true;
			this.lstClientIP5.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.lstClientIP5.Size = new System.Drawing.Size(161, 446);
			this.lstClientIP5.TabIndex = 32;
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(476, 15);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(55, 13);
			this.label14.TabIndex = 31;
			this.label14.Text = "Client IP : ";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(11, 32);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(34, 13);
			this.label15.TabIndex = 30;
			this.label15.Text = "Log : ";
			// 
			// txtLog5
			// 
			this.txtLog5.Location = new System.Drawing.Point(6, 55);
			this.txtLog5.Multiline = true;
			this.txtLog5.Name = "txtLog5";
			this.txtLog5.ReadOnly = true;
			this.txtLog5.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog5.Size = new System.Drawing.Size(525, 396);
			this.txtLog5.TabIndex = 31;
			this.txtLog5.TextChanged += new System.EventHandler(this.txtLog5_TextChanged);
			// 
			// cbxIP5
			// 
			this.cbxIP5.FormattingEnabled = true;
			this.cbxIP5.Items.AddRange(new object[] {
            "192.168.0.100",
            "192.168.1.1",
            "192.168.2.2",
            "192.168.3.3",
            "192.168.4.4",
            "127.0.0.1"});
			this.cbxIP5.Location = new System.Drawing.Point(63, 8);
			this.cbxIP5.Name = "cbxIP5";
			this.cbxIP5.Size = new System.Drawing.Size(237, 21);
			this.cbxIP5.TabIndex = 14;
			this.cbxIP5.Text = "127.0.0.1";
			this.cbxIP5.DropDown += new System.EventHandler(this.cbxIP5_DropDown);
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(10, 11);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(47, 13);
			this.label16.TabIndex = 27;
			this.label16.Text = "Server : ";
			// 
			// btnStart2
			// 
			this.btnStart2.Location = new System.Drawing.Point(314, 6);
			this.btnStart2.Name = "btnStart2";
			this.btnStart2.Size = new System.Drawing.Size(55, 23);
			this.btnStart2.TabIndex = 6;
			this.btnStart2.Text = "Start";
			this.btnStart2.UseVisualStyleBackColor = true;
			this.btnStart2.Click += new System.EventHandler(this.btnStart2_Click);
			// 
			// btnStart3
			// 
			this.btnStart3.Location = new System.Drawing.Point(314, 6);
			this.btnStart3.Name = "btnStart3";
			this.btnStart3.Size = new System.Drawing.Size(55, 23);
			this.btnStart3.TabIndex = 9;
			this.btnStart3.Text = "Start";
			this.btnStart3.UseVisualStyleBackColor = true;
			this.btnStart3.Click += new System.EventHandler(this.btnStart3_Click);
			// 
			// btnStart4
			// 
			this.btnStart4.Location = new System.Drawing.Point(314, 6);
			this.btnStart4.Name = "btnStart4";
			this.btnStart4.Size = new System.Drawing.Size(55, 23);
			this.btnStart4.TabIndex = 12;
			this.btnStart4.Text = "Start";
			this.btnStart4.UseVisualStyleBackColor = true;
			this.btnStart4.Click += new System.EventHandler(this.btnStart4_Click);
			// 
			// btnStart5
			// 
			this.btnStart5.Location = new System.Drawing.Point(314, 6);
			this.btnStart5.Name = "btnStart5";
			this.btnStart5.Size = new System.Drawing.Size(55, 23);
			this.btnStart5.TabIndex = 15;
			this.btnStart5.Text = "Start";
			this.btnStart5.UseVisualStyleBackColor = true;
			this.btnStart5.Click += new System.EventHandler(this.btnStart5_Click);
			// 
			// btnRestart
			// 
			this.btnRestart.Location = new System.Drawing.Point(134, 40);
			this.btnRestart.Name = "btnRestart";
			this.btnRestart.Size = new System.Drawing.Size(55, 23);
			this.btnRestart.TabIndex = 33;
			this.btnRestart.Text = "Restart";
			this.btnRestart.UseVisualStyleBackColor = true;
			this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = "Server";
			this.notifyIcon1.Visible = true;
			this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem,
            this.restartToolStripMenuItem,
            this.exitToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(111, 70);
			// 
			// showToolStripMenuItem
			// 
			this.showToolStripMenuItem.Name = "showToolStripMenuItem";
			this.showToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
			this.showToolStripMenuItem.Text = "Show";
			this.showToolStripMenuItem.Click += new System.EventHandler(this.showToolStripMenuItem_Click);
			// 
			// restartToolStripMenuItem
			// 
			this.restartToolStripMenuItem.Name = "restartToolStripMenuItem";
			this.restartToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
			this.restartToolStripMenuItem.Text = "Restart";
			this.restartToolStripMenuItem.Click += new System.EventHandler(this.restartToolStripMenuItem_Click);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(110, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// btnNewInstance
			// 
			this.btnNewInstance.Location = new System.Drawing.Point(73, 40);
			this.btnNewInstance.Name = "btnNewInstance";
			this.btnNewInstance.Size = new System.Drawing.Size(55, 23);
			this.btnNewInstance.TabIndex = 34;
			this.btnNewInstance.Text = "New";
			this.btnNewInstance.UseVisualStyleBackColor = true;
			this.btnNewInstance.Click += new System.EventHandler(this.btnNewInstance_Click);
			// 
			// btnStop1
			// 
			this.btnStop1.Enabled = false;
			this.btnStop1.Location = new System.Drawing.Point(375, 6);
			this.btnStop1.Name = "btnStop1";
			this.btnStop1.Size = new System.Drawing.Size(55, 23);
			this.btnStop1.TabIndex = 4;
			this.btnStop1.Text = "Stop";
			this.btnStop1.UseVisualStyleBackColor = true;
			this.btnStop1.Click += new System.EventHandler(this.btnStop1_Click);
			// 
			// btnStop2
			// 
			this.btnStop2.Enabled = false;
			this.btnStop2.Location = new System.Drawing.Point(375, 6);
			this.btnStop2.Name = "btnStop2";
			this.btnStop2.Size = new System.Drawing.Size(55, 23);
			this.btnStop2.TabIndex = 7;
			this.btnStop2.Text = "Stop";
			this.btnStop2.UseVisualStyleBackColor = true;
			this.btnStop2.Click += new System.EventHandler(this.btnStop2_Click);
			// 
			// btnStop3
			// 
			this.btnStop3.Enabled = false;
			this.btnStop3.Location = new System.Drawing.Point(375, 6);
			this.btnStop3.Name = "btnStop3";
			this.btnStop3.Size = new System.Drawing.Size(55, 23);
			this.btnStop3.TabIndex = 10;
			this.btnStop3.Text = "Stop";
			this.btnStop3.UseVisualStyleBackColor = true;
			this.btnStop3.Click += new System.EventHandler(this.btnStop3_Click);
			// 
			// btnStop4
			// 
			this.btnStop4.Enabled = false;
			this.btnStop4.Location = new System.Drawing.Point(375, 6);
			this.btnStop4.Name = "btnStop4";
			this.btnStop4.Size = new System.Drawing.Size(55, 23);
			this.btnStop4.TabIndex = 13;
			this.btnStop4.Text = "Stop";
			this.btnStop4.UseVisualStyleBackColor = true;
			this.btnStop4.Click += new System.EventHandler(this.btnStop4_Click);
			// 
			// btnStop5
			// 
			this.btnStop5.Enabled = false;
			this.btnStop5.Location = new System.Drawing.Point(375, 6);
			this.btnStop5.Name = "btnStop5";
			this.btnStop5.Size = new System.Drawing.Size(55, 23);
			this.btnStop5.TabIndex = 16;
			this.btnStop5.Text = "Stop";
			this.btnStop5.UseVisualStyleBackColor = true;
			this.btnStop5.Click += new System.EventHandler(this.btnStop5_Click);
			// 
			// btnStop6
			// 
			this.btnStop6.Enabled = false;
			this.btnStop6.Location = new System.Drawing.Point(375, 6);
			this.btnStop6.Name = "btnStop6";
			this.btnStop6.Size = new System.Drawing.Size(55, 23);
			this.btnStop6.TabIndex = 19;
			this.btnStop6.Text = "Stop";
			this.btnStop6.UseVisualStyleBackColor = true;
			this.btnStop6.Click += new System.EventHandler(this.btnStop6_Click);
			// 
			// btnStart6
			// 
			this.btnStart6.Location = new System.Drawing.Point(314, 6);
			this.btnStart6.Name = "btnStart6";
			this.btnStart6.Size = new System.Drawing.Size(55, 23);
			this.btnStart6.TabIndex = 18;
			this.btnStart6.Text = "Start";
			this.btnStart6.UseVisualStyleBackColor = true;
			this.btnStart6.Click += new System.EventHandler(this.btnStart6_Click);
			// 
			// lstClientIP6
			// 
			this.lstClientIP6.FormattingEnabled = true;
			this.lstClientIP6.Location = new System.Drawing.Point(537, 6);
			this.lstClientIP6.Name = "lstClientIP6";
			this.lstClientIP6.ScrollAlwaysVisible = true;
			this.lstClientIP6.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
			this.lstClientIP6.Size = new System.Drawing.Size(161, 446);
			this.lstClientIP6.TabIndex = 37;
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(476, 15);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(55, 13);
			this.label17.TabIndex = 39;
			this.label17.Text = "Client IP : ";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(11, 32);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(34, 13);
			this.label18.TabIndex = 38;
			this.label18.Text = "Log : ";
			// 
			// txtLog6
			// 
			this.txtLog6.Location = new System.Drawing.Point(6, 55);
			this.txtLog6.Multiline = true;
			this.txtLog6.Name = "txtLog6";
			this.txtLog6.ReadOnly = true;
			this.txtLog6.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog6.Size = new System.Drawing.Size(525, 396);
			this.txtLog6.TabIndex = 36;
			this.txtLog6.TextChanged += new System.EventHandler(this.txtLog6_TextChanged);
			// 
			// cbxIP6
			// 
			this.cbxIP6.FormattingEnabled = true;
			this.cbxIP6.Items.AddRange(new object[] {
            "192.168.0.100",
            "192.168.1.1",
            "192.168.2.2",
            "192.168.3.3",
            "192.168.4.4",
            "127.0.0.1"});
			this.cbxIP6.Location = new System.Drawing.Point(63, 8);
			this.cbxIP6.Name = "cbxIP6";
			this.cbxIP6.Size = new System.Drawing.Size(237, 21);
			this.cbxIP6.TabIndex = 17;
			this.cbxIP6.Text = "127.0.0.1";
			this.cbxIP6.DropDown += new System.EventHandler(this.cbxIP6_DropDown);
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(10, 11);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(47, 13);
			this.label19.TabIndex = 35;
			this.label19.Text = "Server : ";
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Controls.Add(this.tabPage6);
			this.tabControl1.Location = new System.Drawing.Point(12, 69);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(712, 483);
			this.tabControl1.TabIndex = 40;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.label1);
			this.tabPage1.Controls.Add(this.cbxIP1);
			this.tabPage1.Controls.Add(this.txtLog1);
			this.tabPage1.Controls.Add(this.label2);
			this.tabPage1.Controls.Add(this.btnStart1);
			this.tabPage1.Controls.Add(this.btnStop1);
			this.tabPage1.Controls.Add(this.label4);
			this.tabPage1.Controls.Add(this.lstClientIP1);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(704, 457);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Server 1";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.txtLog2);
			this.tabPage2.Controls.Add(this.label7);
			this.tabPage2.Controls.Add(this.cbxIP2);
			this.tabPage2.Controls.Add(this.label6);
			this.tabPage2.Controls.Add(this.label5);
			this.tabPage2.Controls.Add(this.lstClientIP2);
			this.tabPage2.Controls.Add(this.btnStart2);
			this.tabPage2.Controls.Add(this.btnStop2);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(704, 457);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Server 2";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.lstClientIP3);
			this.tabPage3.Controls.Add(this.label10);
			this.tabPage3.Controls.Add(this.cbxIP3);
			this.tabPage3.Controls.Add(this.txtLog3);
			this.tabPage3.Controls.Add(this.label9);
			this.tabPage3.Controls.Add(this.label8);
			this.tabPage3.Controls.Add(this.btnStart3);
			this.tabPage3.Controls.Add(this.btnStop3);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage3.Size = new System.Drawing.Size(704, 457);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Server 3";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.lstClientIP4);
			this.tabPage4.Controls.Add(this.label13);
			this.tabPage4.Controls.Add(this.cbxIP4);
			this.tabPage4.Controls.Add(this.txtLog4);
			this.tabPage4.Controls.Add(this.label12);
			this.tabPage4.Controls.Add(this.label11);
			this.tabPage4.Controls.Add(this.btnStart4);
			this.tabPage4.Controls.Add(this.btnStop4);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage4.Size = new System.Drawing.Size(704, 457);
			this.tabPage4.TabIndex = 3;
			this.tabPage4.Text = "Server 4";
			this.tabPage4.UseVisualStyleBackColor = true;
			// 
			// tabPage5
			// 
			this.tabPage5.Controls.Add(this.lstClientIP5);
			this.tabPage5.Controls.Add(this.label16);
			this.tabPage5.Controls.Add(this.cbxIP5);
			this.tabPage5.Controls.Add(this.txtLog5);
			this.tabPage5.Controls.Add(this.label15);
			this.tabPage5.Controls.Add(this.label14);
			this.tabPage5.Controls.Add(this.btnStart5);
			this.tabPage5.Controls.Add(this.btnStop5);
			this.tabPage5.Location = new System.Drawing.Point(4, 22);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage5.Size = new System.Drawing.Size(704, 457);
			this.tabPage5.TabIndex = 4;
			this.tabPage5.Text = "Server 5";
			this.tabPage5.UseVisualStyleBackColor = true;
			// 
			// tabPage6
			// 
			this.tabPage6.Controls.Add(this.lstClientIP6);
			this.tabPage6.Controls.Add(this.label19);
			this.tabPage6.Controls.Add(this.btnStop6);
			this.tabPage6.Controls.Add(this.cbxIP6);
			this.tabPage6.Controls.Add(this.btnStart6);
			this.tabPage6.Controls.Add(this.txtLog6);
			this.tabPage6.Controls.Add(this.label18);
			this.tabPage6.Controls.Add(this.label17);
			this.tabPage6.Location = new System.Drawing.Point(4, 22);
			this.tabPage6.Name = "tabPage6";
			this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage6.Size = new System.Drawing.Size(704, 457);
			this.tabPage6.TabIndex = 5;
			this.tabPage6.Text = "Server 6";
			this.tabPage6.UseVisualStyleBackColor = true;
			// 
			// Server
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(729, 559);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.cbxMessage);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnNewInstance);
			this.Controls.Add(this.btnRestart);
			this.Controls.Add(this.btnSendAll);
			this.Controls.Add(this.btnSend);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Server";
			this.Text = "Server";
			this.Load += new System.EventHandler(this.Server_Load);
			this.Resize += new System.EventHandler(this.Server_Resize);
			this.contextMenuStrip1.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage1.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.tabPage2.PerformLayout();
			this.tabPage3.ResumeLayout(false);
			this.tabPage3.PerformLayout();
			this.tabPage4.ResumeLayout(false);
			this.tabPage4.PerformLayout();
			this.tabPage5.ResumeLayout(false);
			this.tabPage5.PerformLayout();
			this.tabPage6.ResumeLayout(false);
			this.tabPage6.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbxIP1;
        private System.Windows.Forms.TextBox txtLog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStart1;
        private System.Windows.Forms.ComboBox cbxMessage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnSendAll;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lstClientIP1;
        private System.Windows.Forms.ListBox lstClientIP2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtLog2;
        private System.Windows.Forms.ComboBox cbxIP2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ListBox lstClientIP3;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtLog3;
        private System.Windows.Forms.ComboBox cbxIP3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ListBox lstClientIP4;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtLog4;
        private System.Windows.Forms.ComboBox cbxIP4;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.ListBox lstClientIP5;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtLog5;
        private System.Windows.Forms.ComboBox cbxIP5;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Button btnStart2;
        private System.Windows.Forms.Button btnStart3;
        private System.Windows.Forms.Button btnStart4;
        private System.Windows.Forms.Button btnStart5;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem showToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restartToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Button btnNewInstance;
        private System.Windows.Forms.Button btnStop1;
        private System.Windows.Forms.Button btnStop2;
        private System.Windows.Forms.Button btnStop3;
        private System.Windows.Forms.Button btnStop4;
        private System.Windows.Forms.Button btnStop5;
        private System.Windows.Forms.Button btnStop6;
        private System.Windows.Forms.Button btnStart6;
        private System.Windows.Forms.ListBox lstClientIP6;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtLog6;
        private System.Windows.Forms.ComboBox cbxIP6;
        private System.Windows.Forms.Label label19;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.TabPage tabPage6;
	}
}

