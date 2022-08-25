
namespace AutoClose
{
	partial class AutoClose
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoClose));
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.txtInterval = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtProgramList = new System.Windows.Forms.TextBox();
			this.btnSaveList = new System.Windows.Forms.Button();
			this.btnReloadList = new System.Windows.Forms.Button();
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.chkHideTaskbarIcon = new System.Windows.Forms.CheckBox();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Interval = 1000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(110, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Auto Close Interval (s)";
			// 
			// txtInterval
			// 
			this.txtInterval.Location = new System.Drawing.Point(137, 6);
			this.txtInterval.Name = "txtInterval";
			this.txtInterval.Size = new System.Drawing.Size(167, 20);
			this.txtInterval.TabIndex = 1;
			this.txtInterval.Text = "300";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(119, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Auto Close Program List";
			// 
			// txtProgramList
			// 
			this.txtProgramList.Location = new System.Drawing.Point(137, 32);
			this.txtProgramList.Multiline = true;
			this.txtProgramList.Name = "txtProgramList";
			this.txtProgramList.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtProgramList.Size = new System.Drawing.Size(167, 161);
			this.txtProgramList.TabIndex = 3;
			// 
			// btnSaveList
			// 
			this.btnSaveList.Location = new System.Drawing.Point(15, 61);
			this.btnSaveList.Name = "btnSaveList";
			this.btnSaveList.Size = new System.Drawing.Size(107, 23);
			this.btnSaveList.TabIndex = 4;
			this.btnSaveList.Text = "Save List";
			this.btnSaveList.UseVisualStyleBackColor = true;
			this.btnSaveList.Click += new System.EventHandler(this.btnSaveList_Click);
			// 
			// btnReloadList
			// 
			this.btnReloadList.Location = new System.Drawing.Point(15, 90);
			this.btnReloadList.Name = "btnReloadList";
			this.btnReloadList.Size = new System.Drawing.Size(107, 23);
			this.btnReloadList.TabIndex = 5;
			this.btnReloadList.Text = "Reload List";
			this.btnReloadList.UseVisualStyleBackColor = true;
			this.btnReloadList.Click += new System.EventHandler(this.btnReloadList_Click);
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = "AutoClose";
			this.notifyIcon1.Visible = true;
			this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(94, 26);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
			this.exitToolStripMenuItem.Text = "Exit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click_1);
			// 
			// chkHideTaskbarIcon
			// 
			this.chkHideTaskbarIcon.AutoSize = true;
			this.chkHideTaskbarIcon.Location = new System.Drawing.Point(15, 119);
			this.chkHideTaskbarIcon.Name = "chkHideTaskbarIcon";
			this.chkHideTaskbarIcon.Size = new System.Drawing.Size(114, 30);
			this.chkHideTaskbarIcon.TabIndex = 7;
			this.chkHideTaskbarIcon.Text = "Hide Taskbar Icon\r\nWhen Minimized";
			this.chkHideTaskbarIcon.UseVisualStyleBackColor = true;
			// 
			// AutoClose
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(320, 204);
			this.Controls.Add(this.chkHideTaskbarIcon);
			this.Controls.Add(this.btnReloadList);
			this.Controls.Add(this.btnSaveList);
			this.Controls.Add(this.txtProgramList);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtInterval);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "AutoClose";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "AutoClose";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Resize += new System.EventHandler(this.AutoClose_Resize);
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Timer timer1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtInterval;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtProgramList;
		private System.Windows.Forms.Button btnSaveList;
		private System.Windows.Forms.Button btnReloadList;
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.CheckBox chkHideTaskbarIcon;
	}
}

