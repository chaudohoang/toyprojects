
namespace FolderLock
{
	partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.exit2ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.commandToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.minimizedToTrayToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.bntLock = new System.Windows.Forms.Button();
            this.btnUnlock = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnUnlock2 = new System.Windows.Forms.Button();
            this.bntLock2 = new System.Windows.Forms.Button();
            this.btnBrowse2 = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnUnlock3 = new System.Windows.Forms.Button();
            this.bntLock3 = new System.Windows.Forms.Button();
            this.btnBrowse3 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnUnlock4 = new System.Windows.Forms.Button();
            this.bntLock4 = new System.Windows.Forms.Button();
            this.btnBrowse4 = new System.Windows.Forms.Button();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "FolderLock";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exit2ToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(94, 26);
            // 
            // exit2ToolStripMenuItem
            // 
            this.exit2ToolStripMenuItem.Name = "exit2ToolStripMenuItem";
            this.exit2ToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.exit2ToolStripMenuItem.Text = "Exit";
            this.exit2ToolStripMenuItem.Click += new System.EventHandler(this.exit2ToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.commandToolStripMenuItem,
            this.settingsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(723, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // commandToolStripMenuItem
            // 
            this.commandToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.commandToolStripMenuItem.Name = "commandToolStripMenuItem";
            this.commandToolStripMenuItem.Size = new System.Drawing.Size(76, 20);
            this.commandToolStripMenuItem.Text = "Command";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // settingsToolStripMenuItem
            // 
            this.settingsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.minimizedToTrayToolStripMenuItem});
            this.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            this.settingsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.settingsToolStripMenuItem.Text = "Settings";
            // 
            // minimizedToTrayToolStripMenuItem
            // 
            this.minimizedToTrayToolStripMenuItem.CheckOnClick = true;
            this.minimizedToTrayToolStripMenuItem.Name = "minimizedToTrayToolStripMenuItem";
            this.minimizedToTrayToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.minimizedToTrayToolStripMenuItem.Text = "Minimized Hide Taskbar Icon";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(473, 30);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 3;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // bntLock
            // 
            this.bntLock.Location = new System.Drawing.Point(554, 30);
            this.bntLock.Name = "bntLock";
            this.bntLock.Size = new System.Drawing.Size(75, 23);
            this.bntLock.TabIndex = 4;
            this.bntLock.Text = "Lock";
            this.bntLock.UseVisualStyleBackColor = true;
            this.bntLock.Click += new System.EventHandler(this.bntLock_Click);
            // 
            // btnUnlock
            // 
            this.btnUnlock.Location = new System.Drawing.Point(635, 30);
            this.btnUnlock.Name = "btnUnlock";
            this.btnUnlock.Size = new System.Drawing.Size(75, 23);
            this.btnUnlock.TabIndex = 5;
            this.btnUnlock.Text = "Unlock";
            this.btnUnlock.UseVisualStyleBackColor = true;
            this.btnUnlock.Click += new System.EventHandler(this.btnUnlock_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Custom Path";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(109, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Master AppData Path";
            // 
            // btnUnlock2
            // 
            this.btnUnlock2.Location = new System.Drawing.Point(635, 59);
            this.btnUnlock2.Name = "btnUnlock2";
            this.btnUnlock2.Size = new System.Drawing.Size(75, 23);
            this.btnUnlock2.TabIndex = 10;
            this.btnUnlock2.Text = "Unlock";
            this.btnUnlock2.UseVisualStyleBackColor = true;
            this.btnUnlock2.Click += new System.EventHandler(this.btnUnlock2_Click);
            // 
            // bntLock2
            // 
            this.bntLock2.Location = new System.Drawing.Point(554, 59);
            this.bntLock2.Name = "bntLock2";
            this.bntLock2.Size = new System.Drawing.Size(75, 23);
            this.bntLock2.TabIndex = 9;
            this.bntLock2.Text = "Lock";
            this.bntLock2.UseVisualStyleBackColor = true;
            this.bntLock2.Click += new System.EventHandler(this.bntLock2_Click);
            // 
            // btnBrowse2
            // 
            this.btnBrowse2.Location = new System.Drawing.Point(473, 59);
            this.btnBrowse2.Name = "btnBrowse2";
            this.btnBrowse2.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse2.TabIndex = 8;
            this.btnBrowse2.Text = "Browse";
            this.btnBrowse2.UseVisualStyleBackColor = true;
            this.btnBrowse2.Click += new System.EventHandler(this.btnBrowse2_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(134, 61);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(333, 20);
            this.textBox2.TabIndex = 7;
            this.textBox2.Text = "C:\\Radiant Vision Systems Data\\TrueTest\\Master AppData";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 94);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Master Sequence Path";
            // 
            // btnUnlock3
            // 
            this.btnUnlock3.Location = new System.Drawing.Point(635, 88);
            this.btnUnlock3.Name = "btnUnlock3";
            this.btnUnlock3.Size = new System.Drawing.Size(75, 23);
            this.btnUnlock3.TabIndex = 15;
            this.btnUnlock3.Text = "Unlock";
            this.btnUnlock3.UseVisualStyleBackColor = true;
            this.btnUnlock3.Click += new System.EventHandler(this.btnUnlock3_Click);
            // 
            // bntLock3
            // 
            this.bntLock3.Location = new System.Drawing.Point(554, 88);
            this.bntLock3.Name = "bntLock3";
            this.bntLock3.Size = new System.Drawing.Size(75, 23);
            this.bntLock3.TabIndex = 14;
            this.bntLock3.Text = "Lock";
            this.bntLock3.UseVisualStyleBackColor = true;
            this.bntLock3.Click += new System.EventHandler(this.bntLock3_Click);
            // 
            // btnBrowse3
            // 
            this.btnBrowse3.Location = new System.Drawing.Point(473, 88);
            this.btnBrowse3.Name = "btnBrowse3";
            this.btnBrowse3.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse3.TabIndex = 13;
            this.btnBrowse3.Text = "Browse";
            this.btnBrowse3.UseVisualStyleBackColor = true;
            this.btnBrowse3.Click += new System.EventHandler(this.btnBrowse3_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(134, 90);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(333, 20);
            this.textBox3.TabIndex = 12;
            this.textBox3.Text = "C:\\Radiant Vision Systems Data\\TrueTest\\Sequence\\Master";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 123);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 13);
            this.label4.TabIndex = 21;
            this.label4.Text = "Master Calibration Path";
            // 
            // btnUnlock4
            // 
            this.btnUnlock4.Location = new System.Drawing.Point(635, 117);
            this.btnUnlock4.Name = "btnUnlock4";
            this.btnUnlock4.Size = new System.Drawing.Size(75, 23);
            this.btnUnlock4.TabIndex = 20;
            this.btnUnlock4.Text = "Unlock";
            this.btnUnlock4.UseVisualStyleBackColor = true;
            this.btnUnlock4.Click += new System.EventHandler(this.btnUnlock4_Click);
            // 
            // bntLock4
            // 
            this.bntLock4.Location = new System.Drawing.Point(554, 117);
            this.bntLock4.Name = "bntLock4";
            this.bntLock4.Size = new System.Drawing.Size(75, 23);
            this.bntLock4.TabIndex = 19;
            this.bntLock4.Text = "Lock";
            this.bntLock4.UseVisualStyleBackColor = true;
            this.bntLock4.Click += new System.EventHandler(this.bntLock4_Click);
            // 
            // btnBrowse4
            // 
            this.btnBrowse4.Location = new System.Drawing.Point(473, 117);
            this.btnBrowse4.Name = "btnBrowse4";
            this.btnBrowse4.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse4.TabIndex = 18;
            this.btnBrowse4.Text = "Browse";
            this.btnBrowse4.UseVisualStyleBackColor = true;
            this.btnBrowse4.Click += new System.EventHandler(this.btnBrowse4_Click);
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(134, 119);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(333, 20);
            this.textBox4.TabIndex = 17;
            this.textBox4.Text = "C:\\Radiant Vision Systems Data\\TrueTest\\Sequence\\Calibration";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "",
            "D:\\Program\\RVS\\Master Sequence",
            "E:\\Program\\RVS\\Master Sequence"});
            this.comboBox1.Location = new System.Drawing.Point(134, 32);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(333, 21);
            this.comboBox1.TabIndex = 22;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(723, 155);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnUnlock4);
            this.Controls.Add(this.bntLock4);
            this.Controls.Add(this.btnBrowse4);
            this.Controls.Add(this.textBox4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnUnlock3);
            this.Controls.Add(this.bntLock3);
            this.Controls.Add(this.btnBrowse3);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnUnlock2);
            this.Controls.Add(this.bntLock2);
            this.Controls.Add(this.btnBrowse2);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnUnlock);
            this.Controls.Add(this.bntLock);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FolderLock";
            this.Load += new System.EventHandler(this.TemplateApp_Load);
            this.Resize += new System.EventHandler(this.TemplateApp_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem exit2ToolStripMenuItem;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem commandToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem settingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem minimizedToTrayToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Button bntLock;
        private System.Windows.Forms.Button btnUnlock;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnUnlock2;
        private System.Windows.Forms.Button bntLock2;
        private System.Windows.Forms.Button btnBrowse2;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnUnlock3;
        private System.Windows.Forms.Button bntLock3;
        private System.Windows.Forms.Button btnBrowse3;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnUnlock4;
        private System.Windows.Forms.Button bntLock4;
        private System.Windows.Forms.Button btnBrowse4;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}

