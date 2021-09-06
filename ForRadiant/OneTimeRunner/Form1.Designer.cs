
namespace OneTimeRunner
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.comWatchFolder = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comFileType = new System.Windows.Forms.ComboBox();
            this.fileSystemWatcher1 = new System.IO.FileSystemWatcher();
            this.comKillProcessTimeout = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comRetryCount = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkDelAfterRun = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).BeginInit();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "OneTimeRunner";
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
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Watch folder : ";
            // 
            // comWatchFolder
            // 
            this.comWatchFolder.FormattingEnabled = true;
            this.comWatchFolder.Location = new System.Drawing.Point(129, 6);
            this.comWatchFolder.Name = "comWatchFolder";
            this.comWatchFolder.Size = new System.Drawing.Size(261, 21);
            this.comWatchFolder.TabIndex = 2;
            this.comWatchFolder.Text = "D:\\Program\\RVS\\Tools\\OneTimeRun";
            this.comWatchFolder.DropDown += new System.EventHandler(this.comWatchFolder_DropDown);
            this.comWatchFolder.SelectedValueChanged += new System.EventHandler(this.comWatchFolder_SelectedValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "File type : ";
            // 
            // comFileType
            // 
            this.comFileType.FormattingEnabled = true;
            this.comFileType.Location = new System.Drawing.Point(129, 33);
            this.comFileType.Name = "comFileType";
            this.comFileType.Size = new System.Drawing.Size(261, 21);
            this.comFileType.TabIndex = 4;
            this.comFileType.Text = ".exe|.bat|.vbs";
            this.comFileType.DropDown += new System.EventHandler(this.comFileType_DropDown);
            // 
            // fileSystemWatcher1
            // 
            this.fileSystemWatcher1.EnableRaisingEvents = true;
            this.fileSystemWatcher1.IncludeSubdirectories = true;
            this.fileSystemWatcher1.Path = "D:\\Program\\RVS\\Tools\\OneTimeRun";
            this.fileSystemWatcher1.SynchronizingObject = this;
            this.fileSystemWatcher1.Changed += new System.IO.FileSystemEventHandler(this.fileSystemWatcher1_Changed);
            this.fileSystemWatcher1.Created += new System.IO.FileSystemEventHandler(this.fileSystemWatcher1_Created);
            this.fileSystemWatcher1.Deleted += new System.IO.FileSystemEventHandler(this.fileSystemWatcher1_Deleted);
            this.fileSystemWatcher1.Renamed += new System.IO.RenamedEventHandler(this.fileSystemWatcher1_Renamed);
            // 
            // comKillProcessTimeout
            // 
            this.comKillProcessTimeout.FormattingEnabled = true;
            this.comKillProcessTimeout.Items.AddRange(new object[] {
            "90",
            "150",
            "210",
            "270",
            "330"});
            this.comKillProcessTimeout.Location = new System.Drawing.Point(129, 87);
            this.comKillProcessTimeout.Name = "comKillProcessTimeout";
            this.comKillProcessTimeout.Size = new System.Drawing.Size(261, 21);
            this.comKillProcessTimeout.TabIndex = 6;
            this.comKillProcessTimeout.Text = "90";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 90);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Kill process timeout (s)";
            // 
            // comRetryCount
            // 
            this.comRetryCount.FormattingEnabled = true;
            this.comRetryCount.Items.AddRange(new object[] {
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.comRetryCount.Location = new System.Drawing.Point(129, 60);
            this.comRetryCount.Name = "comRetryCount";
            this.comRetryCount.Size = new System.Drawing.Size(261, 21);
            this.comRetryCount.TabIndex = 7;
            this.comRetryCount.Text = "5";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 63);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Retry count : ";
            // 
            // chkDelAfterRun
            // 
            this.chkDelAfterRun.AutoSize = true;
            this.chkDelAfterRun.Checked = true;
            this.chkDelAfterRun.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDelAfterRun.Location = new System.Drawing.Point(15, 115);
            this.chkDelAfterRun.Name = "chkDelAfterRun";
            this.chkDelAfterRun.Size = new System.Drawing.Size(140, 17);
            this.chkDelAfterRun.TabIndex = 10;
            this.chkDelAfterRun.Text = "Delete files after running";
            this.chkDelAfterRun.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(405, 144);
            this.Controls.Add(this.chkDelAfterRun);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.comRetryCount);
            this.Controls.Add(this.comKillProcessTimeout);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comFileType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.comWatchFolder);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "OneTimeRunner";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.fileSystemWatcher1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comWatchFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comFileType;
        private System.IO.FileSystemWatcher fileSystemWatcher1;
        private System.Windows.Forms.ComboBox comKillProcessTimeout;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comRetryCount;
        private System.Windows.Forms.CheckBox chkDelAfterRun;
    }
}

