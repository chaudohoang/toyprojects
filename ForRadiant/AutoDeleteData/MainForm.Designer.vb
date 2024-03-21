
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
            Me.SaveAllListToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ReloadAllListToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.ClearAllLogsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
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
            Me.dataGridView2 = New System.Windows.Forms.DataGridView()
            Me.chkMonitorDisk = New System.Windows.Forms.CheckBox()
            Me.chkMonitorFolders = New System.Windows.Forms.CheckBox()
            Me.TabControl1 = New System.Windows.Forms.TabControl()
            Me.TabPage1 = New System.Windows.Forms.TabPage()
            Me.btnClearLogs = New System.Windows.Forms.Button()
            Me.btnSaveList1 = New System.Windows.Forms.Button()
            Me.btnReloadList1 = New System.Windows.Forms.Button()
            Me.btnViewSelectedFolderLog = New System.Windows.Forms.Button()
            Me.btnDelSelectedFolder = New System.Windows.Forms.Button()
            Me.btnDelAllFolders = New System.Windows.Forms.Button()
            Me.TabPage2 = New System.Windows.Forms.TabPage()
            Me.btnClearLogs2 = New System.Windows.Forms.Button()
            Me.btnViewSelectedDiskLog = New System.Windows.Forms.Button()
            Me.btnCheckAllDiskFreeSpace = New System.Windows.Forms.Button()
            Me.btnCheckSelectedDiskFreeSpace = New System.Windows.Forms.Button()
            Me.dataGridView3 = New System.Windows.Forms.DataGridView()
            Me.btnReloadList2 = New System.Windows.Forms.Button()
            Me.btnSaveList2 = New System.Windows.Forms.Button()
            Me.TabPage3 = New System.Windows.Forms.TabPage()
            Me.Label3 = New System.Windows.Forms.Label()
            Me.btnEnableEditExcludeFolderList = New System.Windows.Forms.Button()
            Me.btnReloadList4 = New System.Windows.Forms.Button()
            Me.btnSaveList4 = New System.Windows.Forms.Button()
            Me.txtExcludeFolderNames = New System.Windows.Forms.TextBox()
            Me.btnEnableEditExcludeFileList = New System.Windows.Forms.Button()
            Me.btnReloadList3 = New System.Windows.Forms.Button()
            Me.btnSaveList3 = New System.Windows.Forms.Button()
            Me.txtExcludeFileNames = New System.Windows.Forms.TextBox()
            Me.Label2 = New System.Windows.Forms.Label()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.txtLogpath = New System.Windows.Forms.TextBox()
            Me.contextMenuStrip1.SuspendLayout()
            Me.menuStrip1.SuspendLayout()
            CType(Me.dataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
            CType(Me.dataGridView2, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.TabControl1.SuspendLayout()
            Me.TabPage1.SuspendLayout()
            Me.TabPage2.SuspendLayout()
            CType(Me.dataGridView3, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.TabPage3.SuspendLayout()
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
            Me.menuStrip1.Size = New System.Drawing.Size(907, 24)
            Me.menuStrip1.TabIndex = 1
            Me.menuStrip1.Text = "menuStrip1"
            '
            'commandToolStripMenuItem
            '
            Me.commandToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.SaveAllListToolStripMenuItem, Me.ReloadAllListToolStripMenuItem, Me.ClearAllLogsToolStripMenuItem, Me.exitToolStripMenuItem})
            Me.commandToolStripMenuItem.Name = "commandToolStripMenuItem"
            Me.commandToolStripMenuItem.Size = New System.Drawing.Size(76, 20)
            Me.commandToolStripMenuItem.Text = "Command"
            '
            'SaveAllListToolStripMenuItem
            '
            Me.SaveAllListToolStripMenuItem.Name = "SaveAllListToolStripMenuItem"
            Me.SaveAllListToolStripMenuItem.Size = New System.Drawing.Size(148, 22)
            Me.SaveAllListToolStripMenuItem.Text = "Save All List"
            '
            'ReloadAllListToolStripMenuItem
            '
            Me.ReloadAllListToolStripMenuItem.Name = "ReloadAllListToolStripMenuItem"
            Me.ReloadAllListToolStripMenuItem.Size = New System.Drawing.Size(148, 22)
            Me.ReloadAllListToolStripMenuItem.Text = "Reload All List"
            '
            'ClearAllLogsToolStripMenuItem
            '
            Me.ClearAllLogsToolStripMenuItem.Name = "ClearAllLogsToolStripMenuItem"
            Me.ClearAllLogsToolStripMenuItem.Size = New System.Drawing.Size(148, 22)
            Me.ClearAllLogsToolStripMenuItem.Text = "Clear All Logs"
            '
            'exitToolStripMenuItem
            '
            Me.exitToolStripMenuItem.Name = "exitToolStripMenuItem"
            Me.exitToolStripMenuItem.Size = New System.Drawing.Size(148, 22)
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
            'MonitorAutomaticallyToolStripMenuItem
            '
            Me.MonitorAutomaticallyToolStripMenuItem.Checked = True
            Me.MonitorAutomaticallyToolStripMenuItem.CheckOnClick = True
            Me.MonitorAutomaticallyToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
            Me.MonitorAutomaticallyToolStripMenuItem.Name = "MonitorAutomaticallyToolStripMenuItem"
            Me.MonitorAutomaticallyToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
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
            Me.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
            Me.dataGridView1.Location = New System.Drawing.Point(6, 6)
            Me.dataGridView1.Name = "dataGridView1"
            Me.dataGridView1.Size = New System.Drawing.Size(864, 474)
            Me.dataGridView1.TabIndex = 10
            '
            'lblMonitoringStatus
            '
            Me.lblMonitoringStatus.AutoSize = True
            Me.lblMonitoringStatus.ForeColor = System.Drawing.Color.Blue
            Me.lblMonitoringStatus.Location = New System.Drawing.Point(535, 28)
            Me.lblMonitoringStatus.Name = "lblMonitoringStatus"
            Me.lblMonitoringStatus.Size = New System.Drawing.Size(46, 13)
            Me.lblMonitoringStatus.TabIndex = 38
            Me.lblMonitoringStatus.Text = "Status : "
            '
            'cmdStopMonitor
            '
            Me.cmdStopMonitor.AutoSize = True
            Me.cmdStopMonitor.LinkColor = System.Drawing.Color.DarkCyan
            Me.cmdStopMonitor.Location = New System.Drawing.Point(448, 28)
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
            Me.cmdStartMonitor.Location = New System.Drawing.Point(361, 28)
            Me.cmdStartMonitor.Name = "cmdStartMonitor"
            Me.cmdStartMonitor.Size = New System.Drawing.Size(81, 13)
            Me.cmdStartMonitor.TabIndex = 36
            Me.cmdStartMonitor.TabStop = True
            Me.cmdStartMonitor.Text = "Start Monitoring"
            '
            'dataGridView2
            '
            Me.dataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
            Me.dataGridView2.Location = New System.Drawing.Point(6, 6)
            Me.dataGridView2.Name = "dataGridView2"
            Me.dataGridView2.Size = New System.Drawing.Size(864, 127)
            Me.dataGridView2.TabIndex = 41
            '
            'chkMonitorDisk
            '
            Me.chkMonitorDisk.AutoSize = True
            Me.chkMonitorDisk.Checked = True
            Me.chkMonitorDisk.CheckState = System.Windows.Forms.CheckState.Checked
            Me.chkMonitorDisk.Location = New System.Drawing.Point(212, 27)
            Me.chkMonitorDisk.Name = "chkMonitorDisk"
            Me.chkMonitorDisk.Size = New System.Drawing.Size(143, 17)
            Me.chkMonitorDisk.TabIndex = 42
            Me.chkMonitorDisk.Text = "Monitor Disk Free Space"
            Me.chkMonitorDisk.UseVisualStyleBackColor = True
            '
            'chkMonitorFolders
            '
            Me.chkMonitorFolders.AutoSize = True
            Me.chkMonitorFolders.Checked = True
            Me.chkMonitorFolders.CheckState = System.Windows.Forms.CheckState.Checked
            Me.chkMonitorFolders.Location = New System.Drawing.Point(16, 27)
            Me.chkMonitorFolders.Name = "chkMonitorFolders"
            Me.chkMonitorFolders.Size = New System.Drawing.Size(190, 17)
            Me.chkMonitorFolders.TabIndex = 43
            Me.chkMonitorFolders.Text = "Monitor Files/Folders Created Date"
            Me.chkMonitorFolders.UseVisualStyleBackColor = True
            '
            'TabControl1
            '
            Me.TabControl1.Controls.Add(Me.TabPage1)
            Me.TabControl1.Controls.Add(Me.TabPage2)
            Me.TabControl1.Controls.Add(Me.TabPage3)
            Me.TabControl1.Location = New System.Drawing.Point(12, 76)
            Me.TabControl1.Name = "TabControl1"
            Me.TabControl1.SelectedIndex = 0
            Me.TabControl1.Size = New System.Drawing.Size(885, 541)
            Me.TabControl1.TabIndex = 44
            '
            'TabPage1
            '
            Me.TabPage1.Controls.Add(Me.dataGridView1)
            Me.TabPage1.Controls.Add(Me.btnClearLogs)
            Me.TabPage1.Controls.Add(Me.btnSaveList1)
            Me.TabPage1.Controls.Add(Me.btnReloadList1)
            Me.TabPage1.Controls.Add(Me.btnViewSelectedFolderLog)
            Me.TabPage1.Controls.Add(Me.btnDelSelectedFolder)
            Me.TabPage1.Controls.Add(Me.btnDelAllFolders)
            Me.TabPage1.Location = New System.Drawing.Point(4, 22)
            Me.TabPage1.Name = "TabPage1"
            Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPage1.Size = New System.Drawing.Size(877, 515)
            Me.TabPage1.TabIndex = 0
            Me.TabPage1.Text = "Monitor Files/Folders Created Date"
            Me.TabPage1.UseVisualStyleBackColor = True
            '
            'btnClearLogs
            '
            Me.btnClearLogs.Location = New System.Drawing.Point(546, 486)
            Me.btnClearLogs.Name = "btnClearLogs"
            Me.btnClearLogs.Size = New System.Drawing.Size(75, 23)
            Me.btnClearLogs.TabIndex = 52
            Me.btnClearLogs.Text = "Clear Logs"
            Me.btnClearLogs.UseVisualStyleBackColor = True
            '
            'btnSaveList1
            '
            Me.btnSaveList1.Location = New System.Drawing.Point(6, 486)
            Me.btnSaveList1.Name = "btnSaveList1"
            Me.btnSaveList1.Size = New System.Drawing.Size(75, 23)
            Me.btnSaveList1.TabIndex = 45
            Me.btnSaveList1.Text = "Save List"
            Me.btnSaveList1.UseVisualStyleBackColor = True
            '
            'btnReloadList1
            '
            Me.btnReloadList1.Location = New System.Drawing.Point(87, 486)
            Me.btnReloadList1.Name = "btnReloadList1"
            Me.btnReloadList1.Size = New System.Drawing.Size(75, 23)
            Me.btnReloadList1.TabIndex = 46
            Me.btnReloadList1.Text = "Reload List"
            Me.btnReloadList1.UseVisualStyleBackColor = True
            '
            'btnViewSelectedFolderLog
            '
            Me.btnViewSelectedFolderLog.Location = New System.Drawing.Point(403, 486)
            Me.btnViewSelectedFolderLog.Name = "btnViewSelectedFolderLog"
            Me.btnViewSelectedFolderLog.Size = New System.Drawing.Size(137, 23)
            Me.btnViewSelectedFolderLog.TabIndex = 51
            Me.btnViewSelectedFolderLog.Text = "View Selected Folder Log"
            Me.btnViewSelectedFolderLog.UseVisualStyleBackColor = True
            '
            'btnDelSelectedFolder
            '
            Me.btnDelSelectedFolder.Location = New System.Drawing.Point(168, 486)
            Me.btnDelSelectedFolder.Name = "btnDelSelectedFolder"
            Me.btnDelSelectedFolder.Size = New System.Drawing.Size(124, 23)
            Me.btnDelSelectedFolder.TabIndex = 47
            Me.btnDelSelectedFolder.Text = "Delete Selected Folder"
            Me.btnDelSelectedFolder.UseVisualStyleBackColor = True
            '
            'btnDelAllFolders
            '
            Me.btnDelAllFolders.Location = New System.Drawing.Point(298, 486)
            Me.btnDelAllFolders.Name = "btnDelAllFolders"
            Me.btnDelAllFolders.Size = New System.Drawing.Size(99, 23)
            Me.btnDelAllFolders.TabIndex = 48
            Me.btnDelAllFolders.Text = "Delete All Folders"
            Me.btnDelAllFolders.UseVisualStyleBackColor = True
            '
            'TabPage2
            '
            Me.TabPage2.Controls.Add(Me.btnClearLogs2)
            Me.TabPage2.Controls.Add(Me.btnViewSelectedDiskLog)
            Me.TabPage2.Controls.Add(Me.btnCheckAllDiskFreeSpace)
            Me.TabPage2.Controls.Add(Me.btnCheckSelectedDiskFreeSpace)
            Me.TabPage2.Controls.Add(Me.dataGridView3)
            Me.TabPage2.Controls.Add(Me.dataGridView2)
            Me.TabPage2.Controls.Add(Me.btnReloadList2)
            Me.TabPage2.Controls.Add(Me.btnSaveList2)
            Me.TabPage2.Location = New System.Drawing.Point(4, 22)
            Me.TabPage2.Name = "TabPage2"
            Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPage2.Size = New System.Drawing.Size(877, 515)
            Me.TabPage2.TabIndex = 1
            Me.TabPage2.Text = "Monitor Disk Free Space"
            Me.TabPage2.UseVisualStyleBackColor = True
            '
            'btnClearLogs2
            '
            Me.btnClearLogs2.Location = New System.Drawing.Point(658, 486)
            Me.btnClearLogs2.Name = "btnClearLogs2"
            Me.btnClearLogs2.Size = New System.Drawing.Size(75, 23)
            Me.btnClearLogs2.TabIndex = 56
            Me.btnClearLogs2.Text = "Clear Logs"
            Me.btnClearLogs2.UseVisualStyleBackColor = True
            '
            'btnViewSelectedDiskLog
            '
            Me.btnViewSelectedDiskLog.Location = New System.Drawing.Point(520, 486)
            Me.btnViewSelectedDiskLog.Name = "btnViewSelectedDiskLog"
            Me.btnViewSelectedDiskLog.Size = New System.Drawing.Size(132, 23)
            Me.btnViewSelectedDiskLog.TabIndex = 55
            Me.btnViewSelectedDiskLog.Text = "View Selected Disk Log"
            Me.btnViewSelectedDiskLog.UseVisualStyleBackColor = True
            '
            'btnCheckAllDiskFreeSpace
            '
            Me.btnCheckAllDiskFreeSpace.Location = New System.Drawing.Point(359, 486)
            Me.btnCheckAllDiskFreeSpace.Name = "btnCheckAllDiskFreeSpace"
            Me.btnCheckAllDiskFreeSpace.Size = New System.Drawing.Size(152, 23)
            Me.btnCheckAllDiskFreeSpace.TabIndex = 54
            Me.btnCheckAllDiskFreeSpace.Text = "Check All Disk Free Space"
            Me.btnCheckAllDiskFreeSpace.UseVisualStyleBackColor = True
            '
            'btnCheckSelectedDiskFreeSpace
            '
            Me.btnCheckSelectedDiskFreeSpace.Location = New System.Drawing.Point(166, 486)
            Me.btnCheckSelectedDiskFreeSpace.Name = "btnCheckSelectedDiskFreeSpace"
            Me.btnCheckSelectedDiskFreeSpace.Size = New System.Drawing.Size(187, 23)
            Me.btnCheckSelectedDiskFreeSpace.TabIndex = 53
            Me.btnCheckSelectedDiskFreeSpace.Text = "Check Selected Disk Free Space"
            Me.btnCheckSelectedDiskFreeSpace.UseVisualStyleBackColor = True
            '
            'dataGridView3
            '
            Me.dataGridView3.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
            Me.dataGridView3.Location = New System.Drawing.Point(7, 139)
            Me.dataGridView3.Name = "dataGridView3"
            Me.dataGridView3.Size = New System.Drawing.Size(864, 341)
            Me.dataGridView3.TabIndex = 42
            '
            'btnReloadList2
            '
            Me.btnReloadList2.Location = New System.Drawing.Point(85, 486)
            Me.btnReloadList2.Name = "btnReloadList2"
            Me.btnReloadList2.Size = New System.Drawing.Size(75, 23)
            Me.btnReloadList2.TabIndex = 50
            Me.btnReloadList2.Text = "Reload List"
            Me.btnReloadList2.UseVisualStyleBackColor = True
            '
            'btnSaveList2
            '
            Me.btnSaveList2.Location = New System.Drawing.Point(4, 486)
            Me.btnSaveList2.Name = "btnSaveList2"
            Me.btnSaveList2.Size = New System.Drawing.Size(75, 23)
            Me.btnSaveList2.TabIndex = 49
            Me.btnSaveList2.Text = "Save List"
            Me.btnSaveList2.UseVisualStyleBackColor = True
            '
            'TabPage3
            '
            Me.TabPage3.Controls.Add(Me.Label3)
            Me.TabPage3.Controls.Add(Me.btnEnableEditExcludeFolderList)
            Me.TabPage3.Controls.Add(Me.btnReloadList4)
            Me.TabPage3.Controls.Add(Me.btnSaveList4)
            Me.TabPage3.Controls.Add(Me.txtExcludeFolderNames)
            Me.TabPage3.Controls.Add(Me.btnEnableEditExcludeFileList)
            Me.TabPage3.Controls.Add(Me.btnReloadList3)
            Me.TabPage3.Controls.Add(Me.btnSaveList3)
            Me.TabPage3.Controls.Add(Me.txtExcludeFileNames)
            Me.TabPage3.Controls.Add(Me.Label2)
            Me.TabPage3.Location = New System.Drawing.Point(4, 22)
            Me.TabPage3.Name = "TabPage3"
            Me.TabPage3.Size = New System.Drawing.Size(877, 515)
            Me.TabPage3.TabIndex = 2
            Me.TabPage3.Text = "Exclude Settings"
            Me.TabPage3.UseVisualStyleBackColor = True
            '
            'Label3
            '
            Me.Label3.AutoSize = True
            Me.Label3.Location = New System.Drawing.Point(7, 244)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New System.Drawing.Size(122, 13)
            Me.Label3.TabIndex = 58
            Me.Label3.Text = "Exclude Folder Names : "
            '
            'btnEnableEditExcludeFolderList
            '
            Me.btnEnableEditExcludeFolderList.Location = New System.Drawing.Point(23, 269)
            Me.btnEnableEditExcludeFolderList.Name = "btnEnableEditExcludeFolderList"
            Me.btnEnableEditExcludeFolderList.Size = New System.Drawing.Size(96, 23)
            Me.btnEnableEditExcludeFolderList.TabIndex = 57
            Me.btnEnableEditExcludeFolderList.Text = "Edit List"
            Me.btnEnableEditExcludeFolderList.UseVisualStyleBackColor = True
            '
            'btnReloadList4
            '
            Me.btnReloadList4.Location = New System.Drawing.Point(23, 327)
            Me.btnReloadList4.Name = "btnReloadList4"
            Me.btnReloadList4.Size = New System.Drawing.Size(96, 23)
            Me.btnReloadList4.TabIndex = 56
            Me.btnReloadList4.Text = "Reload List"
            Me.btnReloadList4.UseVisualStyleBackColor = True
            '
            'btnSaveList4
            '
            Me.btnSaveList4.Location = New System.Drawing.Point(23, 298)
            Me.btnSaveList4.Name = "btnSaveList4"
            Me.btnSaveList4.Size = New System.Drawing.Size(96, 23)
            Me.btnSaveList4.TabIndex = 55
            Me.btnSaveList4.Text = "Save List"
            Me.btnSaveList4.UseVisualStyleBackColor = True
            '
            'txtExcludeFolderNames
            '
            Me.txtExcludeFolderNames.Location = New System.Drawing.Point(135, 244)
            Me.txtExcludeFolderNames.Multiline = True
            Me.txtExcludeFolderNames.Name = "txtExcludeFolderNames"
            Me.txtExcludeFolderNames.ReadOnly = True
            Me.txtExcludeFolderNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
            Me.txtExcludeFolderNames.Size = New System.Drawing.Size(721, 212)
            Me.txtExcludeFolderNames.TabIndex = 54
            '
            'btnEnableEditExcludeFileList
            '
            Me.btnEnableEditExcludeFileList.Location = New System.Drawing.Point(23, 45)
            Me.btnEnableEditExcludeFileList.Name = "btnEnableEditExcludeFileList"
            Me.btnEnableEditExcludeFileList.Size = New System.Drawing.Size(96, 23)
            Me.btnEnableEditExcludeFileList.TabIndex = 53
            Me.btnEnableEditExcludeFileList.Text = "Edit List"
            Me.btnEnableEditExcludeFileList.UseVisualStyleBackColor = True
            '
            'btnReloadList3
            '
            Me.btnReloadList3.Location = New System.Drawing.Point(23, 103)
            Me.btnReloadList3.Name = "btnReloadList3"
            Me.btnReloadList3.Size = New System.Drawing.Size(96, 23)
            Me.btnReloadList3.TabIndex = 52
            Me.btnReloadList3.Text = "Reload List"
            Me.btnReloadList3.UseVisualStyleBackColor = True
            '
            'btnSaveList3
            '
            Me.btnSaveList3.Location = New System.Drawing.Point(23, 74)
            Me.btnSaveList3.Name = "btnSaveList3"
            Me.btnSaveList3.Size = New System.Drawing.Size(96, 23)
            Me.btnSaveList3.TabIndex = 51
            Me.btnSaveList3.Text = "Save List"
            Me.btnSaveList3.UseVisualStyleBackColor = True
            '
            'txtExcludeFileNames
            '
            Me.txtExcludeFileNames.Location = New System.Drawing.Point(135, 15)
            Me.txtExcludeFileNames.Multiline = True
            Me.txtExcludeFileNames.Name = "txtExcludeFileNames"
            Me.txtExcludeFileNames.ReadOnly = True
            Me.txtExcludeFileNames.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
            Me.txtExcludeFileNames.Size = New System.Drawing.Size(721, 212)
            Me.txtExcludeFileNames.TabIndex = 1
            '
            'Label2
            '
            Me.Label2.AutoSize = True
            Me.Label2.Location = New System.Drawing.Point(20, 18)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(109, 13)
            Me.Label2.TabIndex = 0
            Me.Label2.Text = "Exclude File Names : "
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
            Me.txtLogpath.Text = "D:\Program\RVS\AutoDeleteDataLog"
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(907, 623)
            Me.Controls.Add(Me.txtLogpath)
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.TabControl1)
            Me.Controls.Add(Me.chkMonitorFolders)
            Me.Controls.Add(Me.chkMonitorDisk)
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
            Me.Text = "AutoDeleteData"
            Me.contextMenuStrip1.ResumeLayout(False)
            Me.menuStrip1.ResumeLayout(False)
            Me.menuStrip1.PerformLayout()
            CType(Me.dataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
            CType(Me.dataGridView2, System.ComponentModel.ISupportInitialize).EndInit()
            Me.TabControl1.ResumeLayout(False)
            Me.TabPage1.ResumeLayout(False)
            Me.TabPage2.ResumeLayout(False)
            CType(Me.dataGridView3, System.ComponentModel.ISupportInitialize).EndInit()
            Me.TabPage3.ResumeLayout(False)
            Me.TabPage3.PerformLayout()
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
        Private WithEvents dataGridView2 As Windows.Forms.DataGridView
        Friend WithEvents SaveAllListToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents ReloadAllListToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents ClearAllLogsToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents chkMonitorDisk As Windows.Forms.CheckBox
        Friend WithEvents chkMonitorFolders As Windows.Forms.CheckBox
        Friend WithEvents TabControl1 As Windows.Forms.TabControl
        Friend WithEvents TabPage1 As Windows.Forms.TabPage
        Friend WithEvents TabPage2 As Windows.Forms.TabPage
        Friend WithEvents btnSaveList1 As Windows.Forms.Button
        Friend WithEvents btnReloadList1 As Windows.Forms.Button
        Friend WithEvents btnDelSelectedFolder As Windows.Forms.Button
        Private WithEvents dataGridView3 As Windows.Forms.DataGridView
        Friend WithEvents btnDelAllFolders As Windows.Forms.Button
        Friend WithEvents btnSaveList2 As Windows.Forms.Button
        Friend WithEvents btnReloadList2 As Windows.Forms.Button
        Friend WithEvents btnViewSelectedFolderLog As Windows.Forms.Button
        Friend WithEvents btnClearLogs As Windows.Forms.Button
        Friend WithEvents btnCheckSelectedDiskFreeSpace As Windows.Forms.Button
        Friend WithEvents btnClearLogs2 As Windows.Forms.Button
        Friend WithEvents btnViewSelectedDiskLog As Windows.Forms.Button
        Friend WithEvents btnCheckAllDiskFreeSpace As Windows.Forms.Button
        Friend WithEvents Label1 As Windows.Forms.Label
        Friend WithEvents txtLogpath As Windows.Forms.TextBox
        Friend WithEvents TabPage3 As Windows.Forms.TabPage
        Friend WithEvents txtExcludeFileNames As Windows.Forms.TextBox
        Friend WithEvents Label2 As Windows.Forms.Label
        Friend WithEvents btnReloadList3 As Windows.Forms.Button
        Friend WithEvents btnSaveList3 As Windows.Forms.Button
        Friend WithEvents btnEnableEditExcludeFileList As Windows.Forms.Button
        Friend WithEvents MonitorAutomaticallyToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents Label3 As Windows.Forms.Label
        Friend WithEvents btnEnableEditExcludeFolderList As Windows.Forms.Button
        Friend WithEvents btnReloadList4 As Windows.Forms.Button
        Friend WithEvents btnSaveList4 As Windows.Forms.Button
        Friend WithEvents txtExcludeFolderNames As Windows.Forms.TextBox
    End Class
End Namespace
