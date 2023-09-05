
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
            Me.helpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.aboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.richTextBox1 = New System.Windows.Forms.RichTextBox()
            Me.dataGridView1 = New System.Windows.Forms.DataGridView()
            Me.btnDeleteSelectedNow = New System.Windows.Forms.Button()
            Me.btnSaveList = New System.Windows.Forms.Button()
            Me.btnDeleteAllNow = New System.Windows.Forms.Button()
            Me.lblMonitoringStatus = New System.Windows.Forms.Label()
            Me.cmdStopMonitor = New System.Windows.Forms.LinkLabel()
            Me.cmdStartMonitor = New System.Windows.Forms.LinkLabel()
            Me.btnReloadList = New System.Windows.Forms.Button()
            Me.btnLogClear = New System.Windows.Forms.Button()
            Me.btnVewItemLog = New System.Windows.Forms.Button()
            Me.contextMenuStrip1.SuspendLayout()
            Me.menuStrip1.SuspendLayout()
            CType(Me.dataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
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
            Me.commandToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.exitToolStripMenuItem})
            Me.commandToolStripMenuItem.Name = "commandToolStripMenuItem"
            Me.commandToolStripMenuItem.Size = New System.Drawing.Size(76, 20)
            Me.commandToolStripMenuItem.Text = "Command"
            '
            'exitToolStripMenuItem
            '
            Me.exitToolStripMenuItem.Name = "exitToolStripMenuItem"
            Me.exitToolStripMenuItem.Size = New System.Drawing.Size(93, 22)
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
            Me.richTextBox1.Location = New System.Drawing.Point(12, 298)
            Me.richTextBox1.Name = "richTextBox1"
            Me.richTextBox1.Size = New System.Drawing.Size(864, 218)
            Me.richTextBox1.TabIndex = 12
            Me.richTextBox1.Text = ""
            '
            'dataGridView1
            '
            Me.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
            Me.dataGridView1.Location = New System.Drawing.Point(12, 27)
            Me.dataGridView1.Name = "dataGridView1"
            Me.dataGridView1.Size = New System.Drawing.Size(864, 236)
            Me.dataGridView1.TabIndex = 10
            '
            'btnDeleteSelectedNow
            '
            Me.btnDeleteSelectedNow.Location = New System.Drawing.Point(12, 269)
            Me.btnDeleteSelectedNow.Name = "btnDeleteSelectedNow"
            Me.btnDeleteSelectedNow.Size = New System.Drawing.Size(119, 23)
            Me.btnDeleteSelectedNow.TabIndex = 8
            Me.btnDeleteSelectedNow.Text = "Delete Selected Now"
            Me.btnDeleteSelectedNow.UseVisualStyleBackColor = True
            '
            'btnSaveList
            '
            Me.btnSaveList.Location = New System.Drawing.Point(231, 269)
            Me.btnSaveList.Name = "btnSaveList"
            Me.btnSaveList.Size = New System.Drawing.Size(72, 23)
            Me.btnSaveList.TabIndex = 9
            Me.btnSaveList.Text = "Save List"
            Me.btnSaveList.UseVisualStyleBackColor = True
            '
            'btnDeleteAllNow
            '
            Me.btnDeleteAllNow.Location = New System.Drawing.Point(137, 269)
            Me.btnDeleteAllNow.Name = "btnDeleteAllNow"
            Me.btnDeleteAllNow.Size = New System.Drawing.Size(88, 23)
            Me.btnDeleteAllNow.TabIndex = 8
            Me.btnDeleteAllNow.Text = "Delete All Now"
            Me.btnDeleteAllNow.UseVisualStyleBackColor = True
            '
            'lblMonitoringStatus
            '
            Me.lblMonitoringStatus.AutoSize = True
            Me.lblMonitoringStatus.ForeColor = System.Drawing.Color.Blue
            Me.lblMonitoringStatus.Location = New System.Drawing.Point(744, 274)
            Me.lblMonitoringStatus.Name = "lblMonitoringStatus"
            Me.lblMonitoringStatus.Size = New System.Drawing.Size(46, 13)
            Me.lblMonitoringStatus.TabIndex = 38
            Me.lblMonitoringStatus.Text = "Status : "
            '
            'cmdStopMonitor
            '
            Me.cmdStopMonitor.AutoSize = True
            Me.cmdStopMonitor.LinkColor = System.Drawing.Color.DarkCyan
            Me.cmdStopMonitor.Location = New System.Drawing.Point(657, 274)
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
            Me.cmdStartMonitor.Location = New System.Drawing.Point(570, 274)
            Me.cmdStartMonitor.Name = "cmdStartMonitor"
            Me.cmdStartMonitor.Size = New System.Drawing.Size(81, 13)
            Me.cmdStartMonitor.TabIndex = 36
            Me.cmdStartMonitor.TabStop = True
            Me.cmdStartMonitor.Text = "Start Monitoring"
            '
            'btnReloadList
            '
            Me.btnReloadList.Location = New System.Drawing.Point(309, 269)
            Me.btnReloadList.Name = "btnReloadList"
            Me.btnReloadList.Size = New System.Drawing.Size(72, 23)
            Me.btnReloadList.TabIndex = 39
            Me.btnReloadList.Text = "Reload List"
            Me.btnReloadList.UseVisualStyleBackColor = True
            '
            'btnLogClear
            '
            Me.btnLogClear.Location = New System.Drawing.Point(500, 269)
            Me.btnLogClear.Name = "btnLogClear"
            Me.btnLogClear.Size = New System.Drawing.Size(64, 23)
            Me.btnLogClear.TabIndex = 39
            Me.btnLogClear.Text = "Log Clear"
            Me.btnLogClear.UseVisualStyleBackColor = True
            '
            'btnVewItemLog
            '
            Me.btnVewItemLog.Location = New System.Drawing.Point(387, 269)
            Me.btnVewItemLog.Name = "btnVewItemLog"
            Me.btnVewItemLog.Size = New System.Drawing.Size(107, 23)
            Me.btnVewItemLog.TabIndex = 40
            Me.btnVewItemLog.Text = "View Selected Log"
            Me.btnVewItemLog.UseVisualStyleBackColor = True
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(888, 528)
            Me.Controls.Add(Me.btnVewItemLog)
            Me.Controls.Add(Me.btnLogClear)
            Me.Controls.Add(Me.btnReloadList)
            Me.Controls.Add(Me.lblMonitoringStatus)
            Me.Controls.Add(Me.cmdStopMonitor)
            Me.Controls.Add(Me.cmdStartMonitor)
            Me.Controls.Add(Me.richTextBox1)
            Me.Controls.Add(Me.dataGridView1)
            Me.Controls.Add(Me.btnDeleteAllNow)
            Me.Controls.Add(Me.btnDeleteSelectedNow)
            Me.Controls.Add(Me.btnSaveList)
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
        Private WithEvents btnDeleteSelectedNow As Windows.Forms.Button
        Private WithEvents btnSaveList As Windows.Forms.Button
        Private WithEvents btnDeleteAllNow As Windows.Forms.Button
        Private WithEvents lblMonitoringStatus As Windows.Forms.Label
        Friend WithEvents cmdStopMonitor As Windows.Forms.LinkLabel
        Friend WithEvents cmdStartMonitor As Windows.Forms.LinkLabel
        Private WithEvents btnReloadList As Windows.Forms.Button
        Private WithEvents btnLogClear As Windows.Forms.Button
        Private WithEvents btnVewItemLog As Windows.Forms.Button
    End Class
End Namespace
