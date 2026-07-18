using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FTPUPloaderCS
{
    public partial class MainForm : Form
    {
        private IContainer components = null;
        public NotifyIcon notifyIcon1;
        public ContextMenuStrip contextMenuStrip1;
        public ToolStripMenuItem exit2ToolStripMenuItem;
        public MenuStrip menuStrip1;
        public ToolStripMenuItem commandToolStripMenuItem;
        public ToolStripMenuItem exitToolStripMenuItem;
        public ToolStripMenuItem settingsToolStripMenuItem;
        public ToolStripMenuItem startMinimizedToolStripMenuItem;
        public ToolStripMenuItem minimizedToTrayToolStripMenuItem;
        public ToolStripMenuItem helpToolStripMenuItem;
        public ToolStripMenuItem aboutToolStripMenuItem;
        public TextBox txtInterval;
        public Label label1;
        public TextBox txtUploadListPath;
        public Label Label3;
        public Label lblStatus;
        public TextBox txtMaximumUpload;
        public Label Label2;
        public Label lblFileStatus;
        public Label lblFileUploadStatus;
        public LinkLabel cmdStartUpload;
        public LinkLabel cmdStopUpload;
        public ToolStripMenuItem backupQueueAfterUploadToolStripMenuItem;
        public ToolStripMenuItem startMinimizedToolStripMenuItem2;
        public TextBox txtMaximumFailRetry;
        public Label Label4;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new Container();
            this.notifyIcon1 = new NotifyIcon(this.components);
            this.contextMenuStrip1 = new ContextMenuStrip(this.components);
            this.exit2ToolStripMenuItem = new ToolStripMenuItem();
            this.menuStrip1 = new MenuStrip();
            this.commandToolStripMenuItem = new ToolStripMenuItem();
            this.exitToolStripMenuItem = new ToolStripMenuItem();
            this.settingsToolStripMenuItem = new ToolStripMenuItem();
            this.startMinimizedToolStripMenuItem = new ToolStripMenuItem();
            this.minimizedToTrayToolStripMenuItem = new ToolStripMenuItem();
            this.backupQueueAfterUploadToolStripMenuItem = new ToolStripMenuItem();
            this.helpToolStripMenuItem = new ToolStripMenuItem();
            this.aboutToolStripMenuItem = new ToolStripMenuItem();
            this.txtInterval = new TextBox();
            this.label1 = new Label();
            this.txtUploadListPath = new TextBox();
            this.Label3 = new Label();
            this.lblStatus = new Label();
            this.txtMaximumUpload = new TextBox();
            this.Label2 = new Label();
            this.lblFileStatus = new Label();
            this.lblFileUploadStatus = new Label();
            this.cmdStartUpload = new LinkLabel();
            this.cmdStopUpload = new LinkLabel();
            this.txtMaximumFailRetry = new TextBox();
            this.Label4 = new Label();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = SystemIcons.Application;
            this.notifyIcon1.Text = "FTPUPloaderCS";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new ToolStripItem[] {
            this.exit2ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new Size(94, 26);
            // 
            // exit2ToolStripMenuItem
            // 
            this.exit2ToolStripMenuItem.Name = "exit2ToolStripMenuItem";
            this.exit2ToolStripMenuItem.Size = new Size(93, 22);
            this.exit2ToolStripMenuItem.Text = "Exit";
            this.exit2ToolStripMenuItem.Click += new EventHandler(this.exit2ToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new ToolStripItem[] {
            this.commandToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new Size(547, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // commandToolStripMenuItem
            // 
            this.commandToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.commandToolStripMenuItem.Name = "commandToolStripMenuItem";
            this.commandToolStripMenuItem.Size = new Size(76, 20);
            this.commandToolStripMenuItem.Text = "Command";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new Size(93, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.startMinimizedToolStripMenuItem,
            this.minimizedToTrayToolStripMenuItem,
            this.backupQueueAfterUploadToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // startMinimizedToolStripMenuItem
            // 
            this.startMinimizedToolStripMenuItem.CheckOnClick = true;
            this.startMinimizedToolStripMenuItem.Name = "startMinimizedToolStripMenuItem";
            this.startMinimizedToolStripMenuItem.Size = new Size(226, 22);
            this.startMinimizedToolStripMenuItem.Text = "Start Minimized";
            // 
            // minimizedToTrayToolStripMenuItem
            // 
            this.minimizedToTrayToolStripMenuItem.CheckOnClick = true;
            this.minimizedToTrayToolStripMenuItem.Name = "minimizedToTrayToolStripMenuItem";
            this.minimizedToTrayToolStripMenuItem.Size = new Size(226, 22);
            this.minimizedToTrayToolStripMenuItem.Text = "Minimized Hide Taskbar Icon";
            // 
            // backupQueueAfterUploadToolStripMenuItem
            // 
            this.backupQueueAfterUploadToolStripMenuItem.CheckOnClick = true;
            this.backupQueueAfterUploadToolStripMenuItem.Name = "backupQueueAfterUploadToolStripMenuItem";
            this.backupQueueAfterUploadToolStripMenuItem.Size = new Size(226, 22);
            this.backupQueueAfterUploadToolStripMenuItem.Text = "Backup Queue After Upload";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // txtInterval
            // 
            this.txtInterval.Location = new Point(170, 50);
            this.txtInterval.Name = "txtInterval";
            this.txtInterval.Size = new Size(365, 20);
            this.txtInterval.TabIndex = 3;
            this.txtInterval.Text = "10";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new Point(12, 53);
            this.label1.Name = "label1";
            this.label1.Size = new Size(110, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Auto Upload Every (s)";
            // 
            // txtUploadListPath
            // 
            this.txtUploadListPath.Location = new Point(170, 24);
            this.txtUploadListPath.Name = "txtUploadListPath";
            this.txtUploadListPath.Size = new Size(365, 20);
            this.txtUploadListPath.TabIndex = 1;
            this.txtUploadListPath.Text = "D:\\Program\\RVS\\UploadQueue";
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new Point(12, 27);
            this.Label3.Name = "Label3";
            this.Label3.Size = new Size(101, 13);
            this.Label3.TabIndex = 2;
            this.Label3.Text = "Upload Queue Path";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.ForeColor = Color.Blue;
            this.lblStatus.Location = new Point(12, 191);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new Size(0, 13);
            this.lblStatus.TabIndex = 6;
            // 
            // txtMaximumUpload
            // 
            this.txtMaximumUpload.Location = new Point(170, 76);
            this.txtMaximumUpload.Name = "txtMaximumUpload";
            this.txtMaximumUpload.Size = new Size(365, 20);
            this.txtMaximumUpload.TabIndex = 7;
            this.txtMaximumUpload.Text = "60";
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new Point(12, 79);
            this.Label2.Name = "Label2";
            this.Label2.Size = new Size(135, 13);
            this.Label2.TabIndex = 8;
            this.Label2.Text = "Maximum Uploads Per Run";
            // 
            // lblFileStatus
            // 
            this.lblFileStatus.AutoSize = true;
            this.lblFileStatus.ForeColor = Color.Purple;
            this.lblFileStatus.Location = new Point(12, 153);
            this.lblFileStatus.Name = "lblFileStatus";
            this.lblFileStatus.Size = new Size(0, 13);
            this.lblFileStatus.TabIndex = 11;
            // 
            // lblFileUploadStatus
            // 
            this.lblFileUploadStatus.AutoSize = true;
            this.lblFileUploadStatus.ForeColor = Color.Purple;
            this.lblFileUploadStatus.Location = new Point(12, 173);
            this.lblFileUploadStatus.Name = "lblFileUploadStatus";
            this.lblFileUploadStatus.Size = new Size(0, 13);
            this.lblFileUploadStatus.TabIndex = 12;
            // 
            // cmdStartUpload
            // 
            this.cmdStartUpload.AutoSize = true;
            this.cmdStartUpload.Location = new Point(12, 130);
            this.cmdStartUpload.Name = "cmdStartUpload";
            this.cmdStartUpload.Size = new Size(66, 13);
            this.cmdStartUpload.TabIndex = 13;
            this.cmdStartUpload.TabStop = true;
            this.cmdStartUpload.Text = "Start Upload";
            this.cmdStartUpload.LinkClicked += new LinkLabelLinkClickedEventHandler(this.cmdStartUpload_LinkClicked);
            // 
            // cmdStopUpload
            // 
            this.cmdStopUpload.AutoSize = true;
            this.cmdStopUpload.LinkColor = Color.Red;
            this.cmdStopUpload.Location = new Point(84, 130);
            this.cmdStopUpload.Name = "cmdStopUpload";
            this.cmdStopUpload.Size = new Size(66, 13);
            this.cmdStopUpload.TabIndex = 14;
            this.cmdStopUpload.TabStop = true;
            this.cmdStopUpload.Text = "Stop Upload";
            this.cmdStopUpload.LinkClicked += new LinkLabelLinkClickedEventHandler(this.cmdStopUpload_LinkClicked);
            // 
            // txtMaximumFailRetry
            // 
            this.txtMaximumFailRetry.Location = new Point(170, 102);
            this.txtMaximumFailRetry.Name = "txtMaximumFailRetry";
            this.txtMaximumFailRetry.Size = new Size(365, 20);
            this.txtMaximumFailRetry.TabIndex = 16;
            this.txtMaximumFailRetry.Text = "5";
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new Point(12, 105);
            this.Label4.Name = "Label4";
            this.Label4.Size = new Size(98, 13);
            this.Label4.TabIndex = 17;
            this.Label4.Text = "Maximum Fail Retry";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(547, 216);
            this.Controls.Add(this.txtMaximumFailRetry);
            this.Controls.Add(this.Label4);
            this.Controls.Add(this.cmdStopUpload);
            this.Controls.Add(this.cmdStartUpload);
            this.Controls.Add(this.lblFileUploadStatus);
            this.Controls.Add(this.lblFileStatus);
            this.Controls.Add(this.txtMaximumUpload);
            this.Controls.Add(this.Label2);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.txtUploadListPath);
            this.Controls.Add(this.Label3);
            this.Controls.Add(this.txtInterval);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "FTPUPloaderCS";
            this.Load += new EventHandler(this.MainForm_Load);
            this.Resize += new EventHandler(this.MainForm_Resize);
            this.FormClosing += new FormClosingEventHandler(this.MainForm_FormClosing);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
