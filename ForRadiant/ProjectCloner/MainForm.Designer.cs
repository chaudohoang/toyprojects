
namespace ProjectCloner
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.cppConsoleOriginalFolderTextBox = new System.Windows.Forms.TextBox();
            this.cppConsoleInputFolderTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cppConsoleRunBtn = new System.Windows.Forms.Button();
            this.cppConsoleOriginalFolderBrowseBtn = new System.Windows.Forms.Button();
            this.cppConsoleInputFolderBowseBtn = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "ProjectCloner";
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
            this.menuStrip1.Size = new System.Drawing.Size(730, 24);
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
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Location = new System.Drawing.Point(12, 27);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(706, 429);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.cppConsoleInputFolderBowseBtn);
            this.tabPage1.Controls.Add(this.cppConsoleOriginalFolderBrowseBtn);
            this.tabPage1.Controls.Add(this.cppConsoleRunBtn);
            this.tabPage1.Controls.Add(this.cppConsoleInputFolderTextBox);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.cppConsoleOriginalFolderTextBox);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(698, 403);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "C++Console";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(698, 403);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "C#Console";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(698, 403);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "VB.NetConsole";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(698, 403);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "C++WinForm";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Size = new System.Drawing.Size(698, 403);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "C#WinForm";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Size = new System.Drawing.Size(698, 403);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "VB.NetWinForm";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Original Project";
            // 
            // cppConsoleOriginalFolderTextBox
            // 
            this.cppConsoleOriginalFolderTextBox.AllowDrop = true;
            this.cppConsoleOriginalFolderTextBox.Location = new System.Drawing.Point(90, 6);
            this.cppConsoleOriginalFolderTextBox.Name = "cppConsoleOriginalFolderTextBox";
            this.cppConsoleOriginalFolderTextBox.Size = new System.Drawing.Size(521, 20);
            this.cppConsoleOriginalFolderTextBox.TabIndex = 1;
            this.cppConsoleOriginalFolderTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox_DragDrop);
            this.cppConsoleOriginalFolderTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox_DragEnter);
            // 
            // cppConsoleInputFolderTextBox
            // 
            this.cppConsoleInputFolderTextBox.AllowDrop = true;
            this.cppConsoleInputFolderTextBox.Location = new System.Drawing.Point(93, 32);
            this.cppConsoleInputFolderTextBox.Name = "cppConsoleInputFolderTextBox";
            this.cppConsoleInputFolderTextBox.Size = new System.Drawing.Size(518, 20);
            this.cppConsoleInputFolderTextBox.TabIndex = 3;
            this.cppConsoleInputFolderTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.TextBox_DragDrop);
            this.cppConsoleInputFolderTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.TextBox_DragEnter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Input Folder";
            // 
            // cppConsoleRunBtn
            // 
            this.cppConsoleRunBtn.Location = new System.Drawing.Point(6, 67);
            this.cppConsoleRunBtn.Name = "cppConsoleRunBtn";
            this.cppConsoleRunBtn.Size = new System.Drawing.Size(75, 23);
            this.cppConsoleRunBtn.TabIndex = 6;
            this.cppConsoleRunBtn.Text = "RUN";
            this.cppConsoleRunBtn.UseVisualStyleBackColor = true;
            this.cppConsoleRunBtn.Click += new System.EventHandler(this.runButton_Click);
            // 
            // cppConsoleOriginalFolderBrowseBtn
            // 
            this.cppConsoleOriginalFolderBrowseBtn.Location = new System.Drawing.Point(617, 4);
            this.cppConsoleOriginalFolderBrowseBtn.Name = "cppConsoleOriginalFolderBrowseBtn";
            this.cppConsoleOriginalFolderBrowseBtn.Size = new System.Drawing.Size(75, 23);
            this.cppConsoleOriginalFolderBrowseBtn.TabIndex = 7;
            this.cppConsoleOriginalFolderBrowseBtn.Text = "Browse";
            this.cppConsoleOriginalFolderBrowseBtn.UseVisualStyleBackColor = true;
            this.cppConsoleOriginalFolderBrowseBtn.Click += new System.EventHandler(this.browseOriginalFolderButton_Click);
            // 
            // cppConsoleInputFolderBowseBtn
            // 
            this.cppConsoleInputFolderBowseBtn.Location = new System.Drawing.Point(617, 30);
            this.cppConsoleInputFolderBowseBtn.Name = "cppConsoleInputFolderBowseBtn";
            this.cppConsoleInputFolderBowseBtn.Size = new System.Drawing.Size(75, 23);
            this.cppConsoleInputFolderBowseBtn.TabIndex = 8;
            this.cppConsoleInputFolderBowseBtn.Text = "Browse";
            this.cppConsoleInputFolderBowseBtn.UseVisualStyleBackColor = true;
            this.cppConsoleInputFolderBowseBtn.Click += new System.EventHandler(this.browseInputFolderButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 468);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.menuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ProjectCloner";
            this.Load += new System.EventHandler(this.ProjectCloner_Load);
            this.Resize += new System.EventHandler(this.ProjectCloner_Resize);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
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
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.TextBox cppConsoleInputFolderTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox cppConsoleOriginalFolderTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button cppConsoleRunBtn;
        private System.Windows.Forms.Button cppConsoleInputFolderBowseBtn;
        private System.Windows.Forms.Button cppConsoleOriginalFolderBrowseBtn;
    }
}

