
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
            Me.exitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.settingsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.startMinimizedToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.minimizedToTrayToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.MonitorAutomaticallyToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.helpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.aboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.dataGridView1 = New System.Windows.Forms.DataGridView()
            Me.lblMonitoringStatus = New System.Windows.Forms.Label()
            Me.cmdStopMonitor = New System.Windows.Forms.LinkLabel()
            Me.cmdStartMonitor = New System.Windows.Forms.LinkLabel()
            Me.TabControl1 = New System.Windows.Forms.TabControl()
            Me.TabPage1 = New System.Windows.Forms.TabPage()
            Me.btnViewLog = New System.Windows.Forms.Button()
            Me.btnClearLogs = New System.Windows.Forms.Button()
            Me.btnSaveSettings = New System.Windows.Forms.Button()
            Me.btnReloadSettings = New System.Windows.Forms.Button()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.txtLogpath = New System.Windows.Forms.TextBox()
            Me.SourceFolder = New System.Windows.Forms.DataGridViewTextBoxColumn()
            Me.DestinationFolder = New System.Windows.Forms.DataGridViewTextBoxColumn()
            Me.ExcludePaths = New System.Windows.Forms.DataGridViewTextBoxColumn()
            Me.ExcludeFolders = New System.Windows.Forms.DataGridViewTextBoxColumn()
            Me.ExcludeFiles = New System.Windows.Forms.DataGridViewTextBoxColumn()
            Me.contextMenuStrip1.SuspendLayout()
            Me.menuStrip1.SuspendLayout()
            CType(Me.dataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.TabControl1.SuspendLayout()
            Me.TabPage1.SuspendLayout()
            Me.SuspendLayout()
            '
            'notifyIcon1
            '
            Me.notifyIcon1.ContextMenuStrip = Me.contextMenuStrip1
            Me.notifyIcon1.Icon = CType(resources.GetObject("notifyIcon1.Icon"), System.Drawing.Icon)
            Me.notifyIcon1.Text = "AutoBackupData"
            Me.notifyIcon1.Visible = True
            '
            'contextMenuStrip1
            '
            Me.contextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.exit2ToolStripMenuItem})
            Me.contextMenuStrip1.Name = "contextMenuStrip1"
            Me.contextMenuStrip1.Size = New System.Drawing.Size(93, 26)
            '
            'exit2ToolStripMenuItem
            '
            Me.exit2ToolStripMenuItem.Name = "exit2ToolStripMenuItem"
            Me.exit2ToolStripMenuItem.Size = New System.Drawing.Size(92, 22)
            Me.exit2ToolStripMenuItem.Text = "Exit"
            '
            'menuStrip1
            '
            Me.menuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.commandToolStripMenuItem, Me.settingsToolStripMenuItem, Me.helpToolStripMenuItem})
            Me.menuStrip1.Location = New System.Drawing.Point(0, 0)
            Me.menuStrip1.Name = "menuStrip1"
            Me.menuStrip1.Size = New System.Drawing.Size(907, 24)
            Me.menuStrip1.TabIndex = 1
            Me.menuStrip1.Text = "menuStrip1"
            '
            'commandToolStripMenuItem
            '
            Me.commandToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.exitToolStripMenuItem})
            Me.commandToolStripMenuItem.Name = "commandToolStripMenuItem"
            Me.commandToolStripMenuItem.Size = New System.Drawing.Size(76, 20)
            Me.commandToolStripMenuItem.Text = "Command"
            '
            'exitToolStripMenuItem
            '
            Me.exitToolStripMenuItem.Name = "exitToolStripMenuItem"
            Me.exitToolStripMenuItem.Size = New System.Drawing.Size(92, 22)
            Me.exitToolStripMenuItem.Text = "Exit"
            '
            'settingsToolStripMenuItem
            '
            Me.settingsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.startMinimizedToolStripMenuItem, Me.minimizedToTrayToolStripMenuItem, Me.MonitorAutomaticallyToolStripMenuItem})
            Me.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem"
            Me.settingsToolStripMenuItem.Size = New System.Drawing.Size(61, 20)
            Me.settingsToolStripMenuItem.Text = "Settings"
            '
            'startMinimizedToolStripMenuItem
            '
            Me.startMinimizedToolStripMenuItem.CheckOnClick = True
            Me.startMinimizedToolStripMenuItem.Name = "startMinimizedToolStripMenuItem"
            Me.startMinimizedToolStripMenuItem.Size = New System.Drawing.Size(227, 22)
            Me.startMinimizedToolStripMenuItem.Text = "Start Minimized"
            '
            'minimizedToTrayToolStripMenuItem
            '
            Me.minimizedToTrayToolStripMenuItem.Checked = True
            Me.minimizedToTrayToolStripMenuItem.CheckOnClick = True
            Me.minimizedToTrayToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
            Me.minimizedToTrayToolStripMenuItem.Name = "minimizedToTrayToolStripMenuItem"
            Me.minimizedToTrayToolStripMenuItem.Size = New System.Drawing.Size(227, 22)
            Me.minimizedToTrayToolStripMenuItem.Text = "Minimized Hide Taskbar Icon"
            '
            'MonitorAutomaticallyToolStripMenuItem
            '
            Me.MonitorAutomaticallyToolStripMenuItem.Checked = True
            Me.MonitorAutomaticallyToolStripMenuItem.CheckOnClick = True
            Me.MonitorAutomaticallyToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
            Me.MonitorAutomaticallyToolStripMenuItem.Name = "MonitorAutomaticallyToolStripMenuItem"
            Me.MonitorAutomaticallyToolStripMenuItem.Size = New System.Drawing.Size(227, 22)
            Me.MonitorAutomaticallyToolStripMenuItem.Text = "Monitor Automatically"
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
            'dataGridView1
            '
            Me.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells
            Me.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
            Me.dataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.SourceFolder, Me.DestinationFolder, Me.ExcludePaths, Me.ExcludeFolders, Me.ExcludeFiles})
            Me.dataGridView1.Location = New System.Drawing.Point(6, 6)
            Me.dataGridView1.Name = "dataGridView1"
            Me.dataGridView1.Size = New System.Drawing.Size(864, 356)
            Me.dataGridView1.TabIndex = 10
            '
            'lblMonitoringStatus
            '
            Me.lblMonitoringStatus.AutoSize = True
            Me.lblMonitoringStatus.ForeColor = System.Drawing.Color.Blue
            Me.lblMonitoringStatus.Location = New System.Drawing.Point(193, 30)
            Me.lblMonitoringStatus.Name = "lblMonitoringStatus"
            Me.lblMonitoringStatus.Size = New System.Drawing.Size(46, 13)
            Me.lblMonitoringStatus.TabIndex = 38
            Me.lblMonitoringStatus.Text = "Status : "
            '
            'cmdStopMonitor
            '
            Me.cmdStopMonitor.AutoSize = True
            Me.cmdStopMonitor.LinkColor = System.Drawing.Color.DarkCyan
            Me.cmdStopMonitor.Location = New System.Drawing.Point(106, 30)
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
            Me.cmdStartMonitor.Location = New System.Drawing.Point(19, 30)
            Me.cmdStartMonitor.Name = "cmdStartMonitor"
            Me.cmdStartMonitor.Size = New System.Drawing.Size(81, 13)
            Me.cmdStartMonitor.TabIndex = 36
            Me.cmdStartMonitor.TabStop = True
            Me.cmdStartMonitor.Text = "Start Monitoring"
            '
            'TabControl1
            '
            Me.TabControl1.Controls.Add(Me.TabPage1)
            Me.TabControl1.Location = New System.Drawing.Point(12, 76)
            Me.TabControl1.Name = "TabControl1"
            Me.TabControl1.SelectedIndex = 0
            Me.TabControl1.Size = New System.Drawing.Size(885, 421)
            Me.TabControl1.TabIndex = 44
            '
            'TabPage1
            '
            Me.TabPage1.Controls.Add(Me.btnViewLog)
            Me.TabPage1.Controls.Add(Me.dataGridView1)
            Me.TabPage1.Controls.Add(Me.btnClearLogs)
            Me.TabPage1.Controls.Add(Me.btnSaveSettings)
            Me.TabPage1.Controls.Add(Me.btnReloadSettings)
            Me.TabPage1.Location = New System.Drawing.Point(4, 22)
            Me.TabPage1.Name = "TabPage1"
            Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPage1.Size = New System.Drawing.Size(877, 395)
            Me.TabPage1.TabIndex = 0
            Me.TabPage1.Text = "Backup Settings"
            Me.TabPage1.UseVisualStyleBackColor = True
            '
            'btnViewLog
            '
            Me.btnViewLog.Location = New System.Drawing.Point(209, 368)
            Me.btnViewLog.Name = "btnViewLog"
            Me.btnViewLog.Size = New System.Drawing.Size(69, 23)
            Me.btnViewLog.TabIndex = 53
            Me.btnViewLog.Text = "View Logs"
            Me.btnViewLog.UseVisualStyleBackColor = True
            '
            'btnClearLogs
            '
            Me.btnClearLogs.Location = New System.Drawing.Point(284, 368)
            Me.btnClearLogs.Name = "btnClearLogs"
            Me.btnClearLogs.Size = New System.Drawing.Size(75, 23)
            Me.btnClearLogs.TabIndex = 52
            Me.btnClearLogs.Text = "Clear Logs"
            Me.btnClearLogs.UseVisualStyleBackColor = True
            '
            'btnSaveSettings
            '
            Me.btnSaveSettings.Location = New System.Drawing.Point(8, 368)
            Me.btnSaveSettings.Name = "btnSaveSettings"
            Me.btnSaveSettings.Size = New System.Drawing.Size(85, 23)
            Me.btnSaveSettings.TabIndex = 45
            Me.btnSaveSettings.Text = "Save Settings"
            Me.btnSaveSettings.UseVisualStyleBackColor = True
            '
            'btnReloadSettings
            '
            Me.btnReloadSettings.Location = New System.Drawing.Point(99, 368)
            Me.btnReloadSettings.Name = "btnReloadSettings"
            Me.btnReloadSettings.Size = New System.Drawing.Size(104, 23)
            Me.btnReloadSettings.TabIndex = 46
            Me.btnReloadSettings.Text = "Reload Settings"
            Me.btnReloadSettings.UseVisualStyleBackColor = True
            '
            'Label1
            '
            Me.Label1.AutoSize = True
            Me.Label1.Location = New System.Drawing.Point(19, 53)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(56, 13)
            Me.Label1.TabIndex = 45
            Me.Label1.Text = "Log Path :"
            '
            'txtLogpath
            '
            Me.txtLogpath.Location = New System.Drawing.Point(81, 50)
            Me.txtLogpath.Name = "txtLogpath"
            Me.txtLogpath.Size = New System.Drawing.Size(274, 20)
            Me.txtLogpath.TabIndex = 53
            Me.txtLogpath.Text = "D:\Program\RVS\AutoBackupDataLog"
            '
            'SourceFolder
            '
            Me.SourceFolder.HeaderText = "Source Folder"
            Me.SourceFolder.Name = "SourceFolder"
            Me.SourceFolder.Width = 90
            '
            'DestinationFolder
            '
            Me.DestinationFolder.HeaderText = "Destination Folder"
            Me.DestinationFolder.Name = "DestinationFolder"
            Me.DestinationFolder.Width = 107
            '
            'ExcludePaths
            '
            Me.ExcludePaths.HeaderText = "Exclude Paths"
            Me.ExcludePaths.Name = "ExcludePaths"
            Me.ExcludePaths.Width = 92
            '
            'ExcludeFolders
            '
            Me.ExcludeFolders.HeaderText = "Exclude Folders"
            Me.ExcludeFolders.Name = "ExcludeFolders"
            Me.ExcludeFolders.Width = 98
            '
            'ExcludeFiles
            '
            Me.ExcludeFiles.HeaderText = "Exclude Files"
            Me.ExcludeFiles.Name = "ExcludeFiles"
            Me.ExcludeFiles.Width = 87
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(907, 506)
            Me.Controls.Add(Me.txtLogpath)
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.TabControl1)
            Me.Controls.Add(Me.lblMonitoringStatus)
            Me.Controls.Add(Me.cmdStopMonitor)
            Me.Controls.Add(Me.cmdStartMonitor)
            Me.Controls.Add(Me.menuStrip1)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
            Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
            Me.MainMenuStrip = Me.menuStrip1
            Me.MaximizeBox = False
            Me.Name = "MainForm"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "AutoBackupData"
            Me.contextMenuStrip1.ResumeLayout(False)
            Me.menuStrip1.ResumeLayout(False)
            Me.menuStrip1.PerformLayout()
            CType(Me.dataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
            Me.TabControl1.ResumeLayout(False)
            Me.TabPage1.ResumeLayout(False)
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
        Private WithEvents dataGridView1 As Windows.Forms.DataGridView
        Private WithEvents lblMonitoringStatus As Windows.Forms.Label
        Friend WithEvents cmdStopMonitor As Windows.Forms.LinkLabel
        Friend WithEvents cmdStartMonitor As Windows.Forms.LinkLabel
        Friend WithEvents TabControl1 As Windows.Forms.TabControl
        Friend WithEvents TabPage1 As Windows.Forms.TabPage
        Friend WithEvents btnSaveSettings As Windows.Forms.Button
        Friend WithEvents btnReloadSettings As Windows.Forms.Button
        Friend WithEvents btnClearLogs As Windows.Forms.Button
        Friend WithEvents Label1 As Windows.Forms.Label
        Friend WithEvents txtLogpath As Windows.Forms.TextBox
        Friend WithEvents MonitorAutomaticallyToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents btnViewLog As Windows.Forms.Button
        Friend WithEvents SourceFolder As Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents DestinationFolder As Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents ExcludePaths As Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents ExcludeFolders As Windows.Forms.DataGridViewTextBoxColumn
        Friend WithEvents ExcludeFiles As Windows.Forms.DataGridViewTextBoxColumn
    End Class
End Namespace
