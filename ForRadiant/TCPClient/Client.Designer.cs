
namespace TCPClient
{
    partial class Client
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Client));
			this.label1 = new System.Windows.Forms.Label();
			this.cbxIP = new System.Windows.Forms.ComboBox();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btnConnect = new System.Windows.Forms.Button();
			this.cbxMessage = new System.Windows.Forms.ComboBox();
			this.label3 = new System.Windows.Forms.Label();
			this.btnSend = new System.Windows.Forms.Button();
			this.btnRestart = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(24, 66);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(47, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Server : ";
			// 
			// cbxIP
			// 
			this.cbxIP.FormattingEnabled = true;
			this.cbxIP.Items.AddRange(new object[] {
            "192.168.0.100",
            "192.168.1.1",
            "192.168.2.2",
            "192.168.3.3",
            "192.168.4.4",
            "127.0.0.1"});
			this.cbxIP.Location = new System.Drawing.Point(77, 63);
			this.cbxIP.Name = "cbxIP";
			this.cbxIP.Size = new System.Drawing.Size(237, 21);
			this.cbxIP.TabIndex = 1;
			this.cbxIP.Text = "127.0.0.1";
			this.cbxIP.DropDown += new System.EventHandler(this.cbxIP_DropDown);
			// 
			// txtLog
			// 
			this.txtLog.Location = new System.Drawing.Point(77, 93);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ReadOnly = true;
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(525, 396);
			this.txtLog.TabIndex = 6;
			this.txtLog.TextChanged += new System.EventHandler(this.txtLog_TextChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(37, 93);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 13);
			this.label2.TabIndex = 3;
			this.label2.Text = "Log : ";
			// 
			// btnConnect
			// 
			this.btnConnect.Location = new System.Drawing.Point(234, 34);
			this.btnConnect.Name = "btnConnect";
			this.btnConnect.Size = new System.Drawing.Size(55, 23);
			this.btnConnect.TabIndex = 3;
			this.btnConnect.Text = "Connect";
			this.btnConnect.UseVisualStyleBackColor = true;
			this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
			// 
			// cbxMessage
			// 
			this.cbxMessage.FormattingEnabled = true;
			this.cbxMessage.Items.AddRange(new object[] {
            "Restart TrueTest",
            "Open Notepad"});
			this.cbxMessage.Location = new System.Drawing.Point(77, 6);
			this.cbxMessage.Name = "cbxMessage";
			this.cbxMessage.Size = new System.Drawing.Size(525, 21);
			this.cbxMessage.TabIndex = 4;
			this.cbxMessage.DropDown += new System.EventHandler(this.cbxMessage_DropDown);
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
			this.btnSend.Location = new System.Drawing.Point(173, 34);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(55, 23);
			this.btnSend.TabIndex = 5;
			this.btnSend.Text = "Send";
			this.btnSend.UseVisualStyleBackColor = true;
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// btnRestart
			// 
			this.btnRestart.Location = new System.Drawing.Point(77, 34);
			this.btnRestart.Name = "btnRestart";
			this.btnRestart.Size = new System.Drawing.Size(90, 23);
			this.btnRestart.TabIndex = 7;
			this.btnRestart.Text = "Restart Client";
			this.btnRestart.UseVisualStyleBackColor = true;
			this.btnRestart.Click += new System.EventHandler(this.btnRestart_Click);
			// 
			// Client
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(611, 498);
			this.Controls.Add(this.cbxMessage);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnRestart);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.btnConnect);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.cbxIP);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Client";
			this.Text = "Client";
			this.Load += new System.EventHandler(this.Client_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbxIP;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.ComboBox cbxMessage;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnRestart;
	}
}

