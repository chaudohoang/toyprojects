
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
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(47, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Server : ";
            // 
            // cbxIP1
            // 
            this.cbxIP1.FormattingEnabled = true;
            this.cbxIP1.Items.AddRange(new object[] {
            "Gooil",
            "Dooone1/Donga1",
            "Dooone2/Donga2",
            "Dooone3/Donga3",
            "Dooone4/Donga4",
            "127.0.0.1:9000",
            "127.0.0.1:9001",
            "127.0.0.1:9002",
            "127.0.0.1:9003",
            "127.0.0.1:9004"});
            this.cbxIP1.Location = new System.Drawing.Point(77, 63);
            this.cbxIP1.Name = "cbxIP1";
            this.cbxIP1.Size = new System.Drawing.Size(237, 21);
            this.cbxIP1.TabIndex = 1;
            // 
            // txtLog1
            // 
            this.txtLog1.Location = new System.Drawing.Point(77, 90);
            this.txtLog1.Multiline = true;
            this.txtLog1.Name = "txtLog1";
            this.txtLog1.ReadOnly = true;
            this.txtLog1.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog1.Size = new System.Drawing.Size(237, 57);
            this.txtLog1.TabIndex = 3;
            this.txtLog1.TextChanged += new System.EventHandler(this.txtLog1_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(34, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Log : ";
            // 
            // btnStart1
            // 
            this.btnStart1.Location = new System.Drawing.Point(320, 61);
            this.btnStart1.Name = "btnStart1";
            this.btnStart1.Size = new System.Drawing.Size(75, 23);
            this.btnStart1.TabIndex = 2;
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
            this.cbxMessage.Location = new System.Drawing.Point(77, 6);
            this.cbxMessage.Name = "cbxMessage";
            this.cbxMessage.Size = new System.Drawing.Size(237, 21);
            this.cbxMessage.TabIndex = 21;
            this.cbxMessage.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cbxMessage_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Message : ";
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(239, 34);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 22;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnSendAll
            // 
            this.btnSendAll.Location = new System.Drawing.Point(158, 34);
            this.btnSendAll.Name = "btnSendAll";
            this.btnSendAll.Size = new System.Drawing.Size(75, 23);
            this.btnSendAll.TabIndex = 23;
            this.btnSendAll.Text = "Send All";
            this.btnSendAll.UseVisualStyleBackColor = true;
            this.btnSendAll.Click += new System.EventHandler(this.btnSendAll_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(401, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Client IP : ";
            // 
            // lstClientIP1
            // 
            this.lstClientIP1.FormattingEnabled = true;
            this.lstClientIP1.Location = new System.Drawing.Point(320, 90);
            this.lstClientIP1.Name = "lstClientIP1";
            this.lstClientIP1.ScrollAlwaysVisible = true;
            this.lstClientIP1.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lstClientIP1.Size = new System.Drawing.Size(203, 56);
            this.lstClientIP1.TabIndex = 4;
            // 
            // lstClientIP2
            // 
            this.lstClientIP2.FormattingEnabled = true;
            this.lstClientIP2.Location = new System.Drawing.Point(320, 183);
            this.lstClientIP2.Name = "lstClientIP2";
            this.lstClientIP2.ScrollAlwaysVisible = true;
            this.lstClientIP2.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lstClientIP2.Size = new System.Drawing.Size(203, 56);
            this.lstClientIP2.TabIndex = 8;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(401, 159);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Client IP : ";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 186);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Log : ";
            // 
            // txtLog2
            // 
            this.txtLog2.Location = new System.Drawing.Point(77, 183);
            this.txtLog2.Multiline = true;
            this.txtLog2.Name = "txtLog2";
            this.txtLog2.ReadOnly = true;
            this.txtLog2.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog2.Size = new System.Drawing.Size(237, 57);
            this.txtLog2.TabIndex = 7;
            this.txtLog2.TextChanged += new System.EventHandler(this.txtLog2_TextChanged);
            // 
            // cbxIP2
            // 
            this.cbxIP2.FormattingEnabled = true;
            this.cbxIP2.Items.AddRange(new object[] {
            "Gooil",
            "Dooone1/Donga1",
            "Dooone2/Donga2",
            "Dooone3/Donga3",
            "Dooone4/Donga4",
            "127.0.0.1:9000",
            "127.0.0.1:9001",
            "127.0.0.1:9002",
            "127.0.0.1:9003",
            "127.0.0.1:9004"});
            this.cbxIP2.Location = new System.Drawing.Point(77, 156);
            this.cbxIP2.Name = "cbxIP2";
            this.cbxIP2.Size = new System.Drawing.Size(237, 21);
            this.cbxIP2.TabIndex = 5;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 159);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Server : ";
            // 
            // lstClientIP3
            // 
            this.lstClientIP3.FormattingEnabled = true;
            this.lstClientIP3.Location = new System.Drawing.Point(320, 276);
            this.lstClientIP3.Name = "lstClientIP3";
            this.lstClientIP3.ScrollAlwaysVisible = true;
            this.lstClientIP3.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lstClientIP3.Size = new System.Drawing.Size(203, 56);
            this.lstClientIP3.TabIndex = 12;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(401, 252);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(55, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Client IP : ";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 279);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(34, 13);
            this.label9.TabIndex = 18;
            this.label9.Text = "Log : ";
            // 
            // txtLog3
            // 
            this.txtLog3.Location = new System.Drawing.Point(77, 276);
            this.txtLog3.Multiline = true;
            this.txtLog3.Name = "txtLog3";
            this.txtLog3.ReadOnly = true;
            this.txtLog3.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog3.Size = new System.Drawing.Size(237, 57);
            this.txtLog3.TabIndex = 11;
            this.txtLog3.TextChanged += new System.EventHandler(this.txtLog3_TextChanged);
            // 
            // cbxIP3
            // 
            this.cbxIP3.FormattingEnabled = true;
            this.cbxIP3.Items.AddRange(new object[] {
            "Gooil",
            "Dooone1/Donga1",
            "Dooone2/Donga2",
            "Dooone3/Donga3",
            "Dooone4/Donga4",
            "127.0.0.1:9000",
            "127.0.0.1:9001",
            "127.0.0.1:9002",
            "127.0.0.1:9003",
            "127.0.0.1:9004"});
            this.cbxIP3.Location = new System.Drawing.Point(77, 249);
            this.cbxIP3.Name = "cbxIP3";
            this.cbxIP3.Size = new System.Drawing.Size(237, 21);
            this.cbxIP3.TabIndex = 9;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(12, 252);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(47, 13);
            this.label10.TabIndex = 15;
            this.label10.Text = "Server : ";
            // 
            // lstClientIP4
            // 
            this.lstClientIP4.FormattingEnabled = true;
            this.lstClientIP4.Location = new System.Drawing.Point(320, 369);
            this.lstClientIP4.Name = "lstClientIP4";
            this.lstClientIP4.ScrollAlwaysVisible = true;
            this.lstClientIP4.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lstClientIP4.Size = new System.Drawing.Size(203, 56);
            this.lstClientIP4.TabIndex = 16;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(401, 345);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(55, 13);
            this.label11.TabIndex = 25;
            this.label11.Text = "Client IP : ";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(12, 372);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(34, 13);
            this.label12.TabIndex = 24;
            this.label12.Text = "Log : ";
            // 
            // txtLog4
            // 
            this.txtLog4.Location = new System.Drawing.Point(77, 369);
            this.txtLog4.Multiline = true;
            this.txtLog4.Name = "txtLog4";
            this.txtLog4.ReadOnly = true;
            this.txtLog4.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog4.Size = new System.Drawing.Size(237, 57);
            this.txtLog4.TabIndex = 15;
            this.txtLog4.TextChanged += new System.EventHandler(this.txtLog4_TextChanged);
            // 
            // cbxIP4
            // 
            this.cbxIP4.FormattingEnabled = true;
            this.cbxIP4.Items.AddRange(new object[] {
            "Gooil",
            "Dooone1/Donga1",
            "Dooone2/Donga2",
            "Dooone3/Donga3",
            "Dooone4/Donga4",
            "127.0.0.1:9000",
            "127.0.0.1:9001",
            "127.0.0.1:9002",
            "127.0.0.1:9003",
            "127.0.0.1:9004"});
            this.cbxIP4.Location = new System.Drawing.Point(77, 342);
            this.cbxIP4.Name = "cbxIP4";
            this.cbxIP4.Size = new System.Drawing.Size(237, 21);
            this.cbxIP4.TabIndex = 13;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(12, 345);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(47, 13);
            this.label13.TabIndex = 21;
            this.label13.Text = "Server : ";
            // 
            // lstClientIP5
            // 
            this.lstClientIP5.FormattingEnabled = true;
            this.lstClientIP5.Location = new System.Drawing.Point(320, 462);
            this.lstClientIP5.Name = "lstClientIP5";
            this.lstClientIP5.ScrollAlwaysVisible = true;
            this.lstClientIP5.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.lstClientIP5.Size = new System.Drawing.Size(203, 56);
            this.lstClientIP5.TabIndex = 20;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(401, 438);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(55, 13);
            this.label14.TabIndex = 31;
            this.label14.Text = "Client IP : ";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(12, 465);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(34, 13);
            this.label15.TabIndex = 30;
            this.label15.Text = "Log : ";
            // 
            // txtLog5
            // 
            this.txtLog5.Location = new System.Drawing.Point(77, 462);
            this.txtLog5.Multiline = true;
            this.txtLog5.Name = "txtLog5";
            this.txtLog5.ReadOnly = true;
            this.txtLog5.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog5.Size = new System.Drawing.Size(237, 57);
            this.txtLog5.TabIndex = 19;
            this.txtLog5.TextChanged += new System.EventHandler(this.txtLog5_TextChanged);
            // 
            // cbxIP5
            // 
            this.cbxIP5.FormattingEnabled = true;
            this.cbxIP5.Items.AddRange(new object[] {
            "Gooil",
            "Dooone1/Donga1",
            "Dooone2/Donga2",
            "Dooone3/Donga3",
            "Dooone4/Donga4",
            "127.0.0.1:9000",
            "127.0.0.1:9001",
            "127.0.0.1:9002",
            "127.0.0.1:9003",
            "127.0.0.1:9004"});
            this.cbxIP5.Location = new System.Drawing.Point(77, 435);
            this.cbxIP5.Name = "cbxIP5";
            this.cbxIP5.Size = new System.Drawing.Size(237, 21);
            this.cbxIP5.TabIndex = 17;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(12, 438);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(47, 13);
            this.label16.TabIndex = 27;
            this.label16.Text = "Server : ";
            // 
            // btnStart2
            // 
            this.btnStart2.Location = new System.Drawing.Point(320, 154);
            this.btnStart2.Name = "btnStart2";
            this.btnStart2.Size = new System.Drawing.Size(75, 23);
            this.btnStart2.TabIndex = 6;
            this.btnStart2.Text = "Start";
            this.btnStart2.UseVisualStyleBackColor = true;
            this.btnStart2.Click += new System.EventHandler(this.btnStart2_Click);
            // 
            // btnStart3
            // 
            this.btnStart3.Location = new System.Drawing.Point(320, 247);
            this.btnStart3.Name = "btnStart3";
            this.btnStart3.Size = new System.Drawing.Size(75, 23);
            this.btnStart3.TabIndex = 10;
            this.btnStart3.Text = "Start";
            this.btnStart3.UseVisualStyleBackColor = true;
            this.btnStart3.Click += new System.EventHandler(this.btnStart3_Click);
            // 
            // btnStart4
            // 
            this.btnStart4.Location = new System.Drawing.Point(320, 340);
            this.btnStart4.Name = "btnStart4";
            this.btnStart4.Size = new System.Drawing.Size(75, 23);
            this.btnStart4.TabIndex = 14;
            this.btnStart4.Text = "Start";
            this.btnStart4.UseVisualStyleBackColor = true;
            this.btnStart4.Click += new System.EventHandler(this.btnStart4_Click);
            // 
            // btnStart5
            // 
            this.btnStart5.Location = new System.Drawing.Point(320, 433);
            this.btnStart5.Name = "btnStart5";
            this.btnStart5.Size = new System.Drawing.Size(75, 23);
            this.btnStart5.TabIndex = 18;
            this.btnStart5.Text = "Start";
            this.btnStart5.UseVisualStyleBackColor = true;
            this.btnStart5.Click += new System.EventHandler(this.btnStart5_Click);
            // 
            // btnRestart
            // 
            this.btnRestart.Location = new System.Drawing.Point(77, 34);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(75, 23);
            this.btnRestart.TabIndex = 24;
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
            // Server
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(539, 532);
            this.Controls.Add(this.btnStart5);
            this.Controls.Add(this.btnStart4);
            this.Controls.Add(this.btnStart3);
            this.Controls.Add(this.btnStart2);
            this.Controls.Add(this.lstClientIP5);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.txtLog5);
            this.Controls.Add(this.cbxIP5);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.lstClientIP4);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.txtLog4);
            this.Controls.Add(this.cbxIP4);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.lstClientIP3);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.txtLog3);
            this.Controls.Add(this.cbxIP3);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.lstClientIP2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtLog2);
            this.Controls.Add(this.cbxIP2);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lstClientIP1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbxMessage);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.btnSendAll);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.btnStart1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtLog1);
            this.Controls.Add(this.cbxIP1);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Server";
            this.Text = "Server";
            this.Load += new System.EventHandler(this.Server_Load);
            this.Resize += new System.EventHandler(this.Server_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
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
    }
}

