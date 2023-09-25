
Imports System

Namespace AutoDeleteData
	Partial Class MainForm
		''' <summary>
		''' Required designer variable.
		''' </summary>
		Private components As ComponentModel.IContainer = Nothing

		''' <summary>
		''' Clean up any resources being used.
		''' </summary>
		''' <paramname="disposing">true if managed resources should be disposed; otherwise, false.</param>
		Protected Overrides Sub Dispose(ByVal disposing As Boolean)
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
			MyBase.Dispose(disposing)
		End Sub

#Region "Windows Form Designer generated code"

		''' <summary>
		''' Required method for Designer support - do not modify
		''' the contents of this method with the code editor.
		''' </summary>
		Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
            Me.notifyIcon1 = New System.Windows.Forms.NotifyIcon(Me.components)
            Me.contextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
            Me.exit2ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.menuStrip1 = New System.Windows.Forms.MenuStrip()
            Me.commandToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.SaveListToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ReloadListToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.DeleteSelectedFolderToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.DeleteAllFoldersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.CheckSelectedDiskFreespaceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.CheckAllDisksFreepsaceToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ViewSelectedFolderLogToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ViewSelectedDiskLogToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ClearLogsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.exitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.settingsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.startMinimizedToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.minimizedToTrayToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.helpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.aboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.richTextBox1 = New System.Windows.Forms.RichTextBox()
            Me.dataGridView1 = New System.Windows.Forms.DataGridView()
            Me.lblMonitoringStatus = New System.Windows.Forms.Label()
            Me.cmdStopMonitor = New System.Windows.Forms.LinkLabel()
            Me.cmdStartMonitor = New System.Windows.Forms.LinkLabel()
            Me.dataGridView2 = New System.Windows.Forms.DataGridView()
            Me.chkMonitorDisk = New System.Windows.Forms.CheckBox()
            Me.chkMonitorFolders = New System.Windows.Forms.CheckBox()
            Me.contextMenuStrip1.SuspendLayout()
            Me.menuStrip1.SuspendLayout()
            CType(Me.dataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.dataGridView2, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'notifyIcon1
            '
            Me.notifyIcon1.ContextMenuStrip = Me.contextMenuStrip1
            Me.notifyIcon1.Icon = CType(resources.GetObject("notifyIcon1.Icon"), System.Drawing.Icon)
            Me.notifyIcon1.Text = "AutoDeleteData"
            Me.notifyIcon1.Visible = True
            '
            'contextMenuStrip1
            '
            Me.contextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.exit2ToolStripMenuItem})
            Me.contextMenuStrip1.Name = "contextMenuStrip1"
            Me.contextMenuStrip1.Size = New System.Drawing.Size(94, 26)
            '
            'exit2ToolStripMenuItem
            '
            Me.exit2ToolStripMenuItem.Name = "exit2ToolStripMenuItem"
            Me.exit2ToolStripMenuItem.Size = New System.Drawing.Size(93, 22)
            Me.exit2ToolStripMenuItem.Text = "Exit"
            '
            'menuStrip1
            '
            Me.menuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.commandToolStripMenuItem, Me.settingsToolStripMenuItem, Me.helpToolStripMenuItem})
            Me.menuStrip1.Location = New System.Drawing.Point(0, 0)
            Me.menuStrip1.Name = "menuStrip1"
            Me.menuStrip1.Size = New System.Drawing.Size(888, 24)
            Me.menuStrip1.TabIndex = 1
            Me.menuStrip1.Text = "menuStrip1"
            '
            'commandToolStripMenuItem
            '
            Me.commandToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SaveListToolStripMenuItem, Me.ReloadListToolStripMenuItem, Me.DeleteSelectedFolderToolStripMenuItem, Me.DeleteAllFoldersToolStripMenuItem, Me.CheckSelectedDiskFreespaceToolStripMenuItem, Me.CheckAllDisksFreepsaceToolStripMenuItem, Me.ViewSelectedFolderLogToolStripMenuItem, Me.ViewSelectedDiskLogToolStripMenuItem, Me.ClearLogsToolStripMenuItem, Me.exitToolStripMenuItem})
            Me.commandToolStripMenuItem.Name = "commandToolStripMenuItem"
            Me.commandToolStripMenuItem.Size = New System.Drawing.Size(76, 20)
            Me.commandToolStripMenuItem.Text = "Command"
            '
            'SaveListToolStripMenuItem
            '
            Me.SaveListToolStripMenuItem.Name = "SaveListToolStripMenuItem"
            Me.SaveListToolStripMenuItem.Size = New System.Drawing.Size(234, 22)
            Me.SaveListToolStripMenuItem.Text = "Save List"
            '
            'ReloadListToolStripMenuItem
            '
            Me.ReloadListToolStripMenuItem.Name = "ReloadListToolStripMenuItem"
            Me.ReloadListToolStripMenuItem.Size = New System.Drawing.Size(234, 22)
            Me.ReloadListToolStripMenuItem.Text = "Reload List"
            '
            'DeleteSelectedFolderToolStripMenuItem
            '
            Me.DeleteSelectedFolderToolStripMenuItem.Name = "DeleteSelectedFolderToolStripMenuItem"
            Me.DeleteSelectedFolderToolStripMenuItem.Size = New System.Drawing.Size(234, 22)
            Me.DeleteSelectedFolderToolStripMenuItem.Text = "Delete Selected Folder"
            '
            'DeleteAllFoldersToolStripMenuItem
            '
            Me.DeleteAllFoldersToolStripMenuItem.Name = "DeleteAllFoldersToolStripMenuItem"
            Me.DeleteAllFoldersToolStripMenuItem.Size = New System.Drawing.Size(234, 22)
            Me.DeleteAllFoldersToolStripMenuItem.Text = "Delete All Folders"
            '
            'CheckSelectedDiskFreespaceToolStripMenuItem
            '
            Me.CheckSelectedDiskFreespaceToolStripMenuItem.Name = "CheckSelectedDiskFreespaceToolStripMenuItem"
            Me.CheckSelectedDiskFreespaceToolStripMenuItem.Size = New System.Drawing.Size(234, 22)
            Me.CheckSelectedDiskFreespaceToolStripMenuItem.Text = "Check Selected Disk Freespace"
            '
            'CheckAllDisksFreepsaceToolStripMenuItem
            '
            Me.CheckAllDisksFreepsaceToolStripMenuItem.Name = "CheckAllDisksFreepsaceToolStripMenuItem"
            Me.CheckAllDisksFreepsaceToolStripMenuItem.Size = New System.Drawing.Size(234, 22)
            Me.CheckAllDisksFreepsaceToolStripMenuItem.Text = "Check All Disks Freepsace"
            '
            'ViewSelectedFolderLogToolStripMenuItem
            '
            Me.ViewSelectedFolderLogToolStripMenuItem.Name = "ViewSelectedFolderLogToolStripMenuItem"
            Me.ViewSelectedFolderLogToolStripMenuItem.Size = New System.Drawing.Size(234, 22)
            Me.ViewSelectedFolderLogToolStripMenuItem.Text = "View Selected Folder Log"
            '
            'ViewSelectedDiskLogToolStripMenuItem
            '
            Me.ViewSelectedDiskLogToolStripMenuItem.Name = "ViewSelectedDiskLogToolStripMenuItem"
            Me.ViewSelectedDiskLogToolStripMenuItem.Size = New System.Drawing.Size(234, 22)
            Me.ViewSelectedDiskLogToolStripMenuItem.Text = "View Selected Disk Log"
            '
            'ClearLogsToolStripMenuItem
            '
            Me.ClearLogsToolStripMenuItem.Name = "ClearLogsToolStripMenuItem"
            Me.ClearLogsToolStripMenuItem.Size = New System.Drawing.Size(234, 22)
            Me.ClearLogsToolStripMenuItem.Text = "Clear Logs"
            '
            'exitToolStripMenuItem
            '
            Me.exitToolStripMenuItem.Name = "exitToolStripMenuItem"
            Me.exitToolStripMenuItem.Size = New System.Drawing.Size(234, 22)
            Me.exitToolStripMenuItem.Text = "Exit"
            '
            'settingsToolStripMenuItem
            '
            Me.settingsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.startMinimizedToolStripMenuItem, Me.minimizedToTrayToolStripMenuItem})
            Me.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem"
            Me.settingsToolStripMenuItem.Size = New System.Drawing.Size(61, 20)
            Me.settingsToolStripMenuItem.Text = "Settings"
            '
            'startMinimizedToolStripMenuItem
            '
            Me.startMinimizedToolStripMenuItem.Checked = True
            Me.startMinimizedToolStripMenuItem.CheckOnClick = True
            Me.startMinimizedToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
            Me.startMinimizedToolStripMenuItem.Name = "startMinimizedToolStripMenuItem"
            Me.startMinimizedToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
            Me.startMinimizedToolStripMenuItem.Text = "Start Minimized"
            '
            'minimizedToTrayToolStripMenuItem
            '
            Me.minimizedToTrayToolStripMenuItem.Checked = True
            Me.minimizedToTrayToolStripMenuItem.CheckOnClick = True
            Me.minimizedToTrayToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
            Me.minimizedToTrayToolStripMenuItem.Name = "minimizedToTrayToolStripMenuItem"
            Me.minimizedToTrayToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
            Me.minimizedToTrayToolStripMenuItem.Text = "Minimized Hide Taskbar Icon"
            '
            'helpToolStripMenuItem
            '
            Me.helpToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.aboutToolStripMenuItem})
            Me.helpToolStripMenuItem.Name = "helpToolStripMenuItem"
            Me.helpToolStripMenuItem.Size = New System.Drawing.Size(44, 20)
            Me.helpToolStripMenuItem.Text = "Help"
            '
            'aboutToolStripMenuItem
            '
            Me.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem"
            Me.aboutToolStripMenuItem.Size = New System.Drawing.Size(107, 22)
            Me.aboutToolStripMenuItem.Text = "About"
            '
            'richTextBox1
            '
            Me.richTextBox1.BackColor = System.Drawing.SystemColors.Menu
            Me.richTextBox1.Location = New System.Drawing.Point(14, 520)
            Me.richTextBox1.Name = "richTextBox1"
            Me.richTextBox1.Size = New System.Drawing.Size(864, 218)
            Me.richTextBox1.TabIndex = 12
            Me.richTextBox1.Text = ""
            '
            'dataGridView1
            '
            Me.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
            Me.dataGridView1.Location = New System.Drawing.Point(14, 27)
            Me.dataGridView1.Name = "dataGridView1"
            Me.dataGridView1.Size = New System.Drawing.Size(864, 298)
            Me.dataGridView1.TabIndex = 10
            '
            'lblMonitoringStatus
            '
            Me.lblMonitoringStatus.AutoSize = True
            Me.lblMonitoringStatus.ForeColor = System.Drawing.Color.Blue
            Me.lblMonitoringStatus.Location = New System.Drawing.Point(461, 497)
            Me.lblMonitoringStatus.Name = "lblMonitoringStatus"
            Me.lblMonitoringStatus.Size = New System.Drawing.Size(46, 13)
            Me.lblMonitoringStatus.TabIndex = 38
            Me.lblMonitoringStatus.Text = "Status : "
            '
            'cmdStopMonitor
            '
            Me.cmdStopMonitor.AutoSize = True
            Me.cmdStopMonitor.LinkColor = System.Drawing.Color.DarkCyan
            Me.cmdStopMonitor.Location = New System.Drawing.Point(374, 497)
            Me.cmdStopMonitor.Name = "cmdStopMonitor"
            Me.cmdStopMonitor.Size = New System.Drawing.Size(81, 13)
            Me.cmdStopMonitor.TabIndex = 37
            Me.cmdStopMonitor.TabStop = True
            Me.cmdStopMonitor.Text = "Stop Monitoring"
            '
            'cmdStartMonitor
            '
            Me.cmdStartMonitor.AutoSize = True
            Me.cmdStartMonitor.LinkColor = System.Drawing.Color.Chocolate
            Me.cmdStartMonitor.Location = New System.Drawing.Point(287, 497)
            Me.cmdStartMonitor.Name = "cmdStartMonitor"
            Me.cmdStartMonitor.Size = New System.Drawing.Size(81, 13)
            Me.cmdStartMonitor.TabIndex = 36
            Me.cmdStartMonitor.TabStop = True
            Me.cmdStartMonitor.Text = "Start Monitoring"
            '
            'dataGridView2
            '
            Me.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
            Me.dataGridView2.Location = New System.Drawing.Point(14, 331)
            Me.dataGridView2.Name = "dataGridView2"
            Me.dataGridView2.Size = New System.Drawing.Size(862, 152)
            Me.dataGridView2.TabIndex = 41
            '
            'chkMonitorDisk
            '
            Me.chkMonitorDisk.AutoSize = True
            Me.chkMonitorDisk.Checked = True
            Me.chkMonitorDisk.CheckState = System.Windows.Forms.CheckState.Checked
            Me.chkMonitorDisk.Location = New System.Drawing.Point(118, 496)
            Me.chkMonitorDisk.Name = "chkMonitorDisk"
            Me.chkMonitorDisk.Size = New System.Drawing.Size(163, 17)
            Me.chkMonitorDisk.TabIndex = 42
            Me.chkMonitorDisk.Text = "Monitor Minimum Disk Space"
            Me.chkMonitorDisk.UseVisualStyleBackColor = True
            '
            'chkMonitorFolders
            '
            Me.chkMonitorFolders.AutoSize = True
            Me.chkMonitorFolders.Checked = True
            Me.chkMonitorFolders.CheckState = System.Windows.Forms.CheckState.Checked
            Me.chkMonitorFolders.Location = New System.Drawing.Point(14, 496)
            Me.chkMonitorFolders.Name = "chkMonitorFolders"
            Me.chkMonitorFolders.Size = New System.Drawing.Size(98, 17)
            Me.chkMonitorFolders.TabIndex = 43
            Me.chkMonitorFolders.Text = "Monitor Folders"
            Me.chkMonitorFolders.UseVisualStyleBackColor = True
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(888, 750)
            Me.Controls.Add(Me.chkMonitorFolders)
            Me.Controls.Add(Me.chkMonitorDisk)
            Me.Controls.Add(Me.dataGridView2)
            Me.Controls.Add(Me.lblMonitoringStatus)
            Me.Controls.Add(Me.cmdStopMonitor)
            Me.Controls.Add(Me.cmdStartMonitor)
            Me.Controls.Add(Me.richTextBox1)
            Me.Controls.Add(Me.dataGridView1)
            Me.Controls.Add(Me.menuStrip1)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
            Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
            Me.MainMenuStrip = Me.menuStrip1
            Me.MaximizeBox = False
            Me.Name = "MainForm"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "AutoDeleteData"
            Me.contextMenuStrip1.ResumeLayout(False)
            Me.menuStrip1.ResumeLayout(False)
            Me.menuStrip1.PerformLayout()
            CType(Me.dataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.dataGridView2, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

#End Region
        Friend WithEvents notifyIcon1 As Windows.Forms.NotifyIcon
		Friend WithEvents contextMenuStrip1 As Windows.Forms.ContextMenuStrip
		Friend WithEvents exit2ToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Friend WithEvents menuStrip1 As Windows.Forms.MenuStrip
		Friend WithEvents commandToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Friend WithEvents exitToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Friend WithEvents settingsToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Friend WithEvents minimizedToTrayToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Friend WithEvents helpToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Friend WithEvents aboutToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents startMinimizedToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Private WithEvents richTextBox1 As Windows.Forms.RichTextBox
        Private WithEvents dataGridView1 As Windows.Forms.DataGridView
        Private WithEvents lblMonitoringStatus As Windows.Forms.Label
        Friend WithEvents cmdStopMonitor As Windows.Forms.LinkLabel
        Friend WithEvents cmdStartMonitor As Windows.Forms.LinkLabel
        Private WithEvents dataGridView2 As Windows.Forms.DataGridView
        Friend WithEvents SaveListToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents ReloadListToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents DeleteSelectedFolderToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents DeleteAllFoldersToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents CheckSelectedDiskFreespaceToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents CheckAllDisksFreepsaceToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents ViewSelectedFolderLogToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents ViewSelectedDiskLogToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents ClearLogsToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents chkMonitorDisk As Windows.Forms.CheckBox
        Friend WithEvents chkMonitorFolders As Windows.Forms.CheckBox
    End Class
End Namespace
