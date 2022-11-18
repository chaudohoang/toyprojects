
Imports System

Namespace RestartTT
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
			Me.txtRunCount = New System.Windows.Forms.TextBox()
			Me.label1 = New System.Windows.Forms.Label()
			Me.label2 = New System.Windows.Forms.Label()
			Me.cmdSaveSettings = New System.Windows.Forms.LinkLabel()
			Me.lblRunCount = New System.Windows.Forms.Label()
			Me.txtWait = New System.Windows.Forms.TextBox()
			Me.Label3 = New System.Windows.Forms.Label()
			Me.cmdResetRunCount = New System.Windows.Forms.LinkLabel()
			Me.cmdRestartTTNow = New System.Windows.Forms.LinkLabel()
			Me.contextMenuStrip1.SuspendLayout()
			Me.menuStrip1.SuspendLayout()
			Me.SuspendLayout()
			'
			'notifyIcon1
			'
			Me.notifyIcon1.ContextMenuStrip = Me.contextMenuStrip1
			Me.notifyIcon1.Icon = CType(resources.GetObject("notifyIcon1.Icon"), System.Drawing.Icon)
			Me.notifyIcon1.Text = "RestartTTDove2p0"
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
			Me.menuStrip1.Size = New System.Drawing.Size(349, 24)
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
			Me.startMinimizedToolStripMenuItem.CheckOnClick = True
			Me.startMinimizedToolStripMenuItem.Name = "startMinimizedToolStripMenuItem"
			Me.startMinimizedToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
			Me.startMinimizedToolStripMenuItem.Text = "Start Minimized"
			'
			'minimizedToTrayToolStripMenuItem
			'
			Me.minimizedToTrayToolStripMenuItem.CheckOnClick = True
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
			'txtRunCount
			'
			Me.txtRunCount.Location = New System.Drawing.Point(211, 21)
			Me.txtRunCount.Name = "txtRunCount"
			Me.txtRunCount.Size = New System.Drawing.Size(43, 20)
			Me.txtRunCount.TabIndex = 3
			Me.txtRunCount.Text = "100"
			'
			'label1
			'
			Me.label1.AutoSize = True
			Me.label1.Location = New System.Drawing.Point(12, 24)
			Me.label1.Name = "label1"
			Me.label1.Size = New System.Drawing.Size(193, 13)
			Me.label1.TabIndex = 4
			Me.label1.Text = "Auto Restart TT Every Number of Runs"
			'
			'label2
			'
			Me.label2.AutoSize = True
			Me.label2.ForeColor = System.Drawing.Color.Purple
			Me.label2.Location = New System.Drawing.Point(13, 94)
			Me.label2.Name = "label2"
			Me.label2.Size = New System.Drawing.Size(67, 13)
			Me.label2.TabIndex = 11
			Me.label2.Text = "Run Count : "
			'
			'cmdSaveSettings
			'
			Me.cmdSaveSettings.AutoSize = True
			Me.cmdSaveSettings.Location = New System.Drawing.Point(12, 71)
			Me.cmdSaveSettings.Name = "cmdSaveSettings"
			Me.cmdSaveSettings.Size = New System.Drawing.Size(68, 13)
			Me.cmdSaveSettings.TabIndex = 13
			Me.cmdSaveSettings.TabStop = True
			Me.cmdSaveSettings.Text = "Save Setting"
			'
			'lblRunCount
			'
			Me.lblRunCount.AutoSize = True
			Me.lblRunCount.ForeColor = System.Drawing.Color.Purple
			Me.lblRunCount.Location = New System.Drawing.Point(86, 94)
			Me.lblRunCount.Name = "lblRunCount"
			Me.lblRunCount.Size = New System.Drawing.Size(13, 13)
			Me.lblRunCount.TabIndex = 15
			Me.lblRunCount.Text = "0"
			'
			'txtWait
			'
			Me.txtWait.Location = New System.Drawing.Point(211, 46)
			Me.txtWait.Name = "txtWait"
			Me.txtWait.Size = New System.Drawing.Size(43, 20)
			Me.txtWait.TabIndex = 17
			Me.txtWait.Text = "3"
			'
			'Label3
			'
			Me.Label3.AutoSize = True
			Me.Label3.Location = New System.Drawing.Point(12, 49)
			Me.Label3.Name = "Label3"
			Me.Label3.Size = New System.Drawing.Size(130, 13)
			Me.Label3.TabIndex = 18
			Me.Label3.Text = "Wait before Restart TT (s)"
			'
			'cmdResetRunCount
			'
			Me.cmdResetRunCount.AutoSize = True
			Me.cmdResetRunCount.LinkColor = System.Drawing.Color.Green
			Me.cmdResetRunCount.Location = New System.Drawing.Point(86, 71)
			Me.cmdResetRunCount.Name = "cmdResetRunCount"
			Me.cmdResetRunCount.Size = New System.Drawing.Size(89, 13)
			Me.cmdResetRunCount.TabIndex = 19
			Me.cmdResetRunCount.TabStop = True
			Me.cmdResetRunCount.Text = "Reset Run Count"
			'
			'cmdRestartTTNow
			'
			Me.cmdRestartTTNow.AutoSize = True
			Me.cmdRestartTTNow.LinkColor = System.Drawing.Color.MediumVioletRed
			Me.cmdRestartTTNow.Location = New System.Drawing.Point(181, 71)
			Me.cmdRestartTTNow.Name = "cmdRestartTTNow"
			Me.cmdRestartTTNow.Size = New System.Drawing.Size(83, 13)
			Me.cmdRestartTTNow.TabIndex = 20
			Me.cmdRestartTTNow.TabStop = True
			Me.cmdRestartTTNow.Text = "Restart TT Now"
			'
			'MainForm
			'
			Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.ClientSize = New System.Drawing.Size(349, 118)
			Me.Controls.Add(Me.cmdRestartTTNow)
			Me.Controls.Add(Me.cmdResetRunCount)
			Me.Controls.Add(Me.txtWait)
			Me.Controls.Add(Me.Label3)
			Me.Controls.Add(Me.lblRunCount)
			Me.Controls.Add(Me.cmdSaveSettings)
			Me.Controls.Add(Me.label2)
			Me.Controls.Add(Me.txtRunCount)
			Me.Controls.Add(Me.label1)
			Me.Controls.Add(Me.menuStrip1)
			Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
			Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
			Me.MainMenuStrip = Me.menuStrip1
			Me.MaximizeBox = False
			Me.Name = "MainForm"
			Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
			Me.Text = "Restart TT Dove2p0"
			Me.contextMenuStrip1.ResumeLayout(False)
			Me.menuStrip1.ResumeLayout(False)
			Me.menuStrip1.PerformLayout()
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
		Private WithEvents txtRunCount As Windows.Forms.TextBox
		Private WithEvents label1 As Windows.Forms.Label
		Private WithEvents label2 As Windows.Forms.Label
		Friend WithEvents cmdSaveSettings As Windows.Forms.LinkLabel
		Private WithEvents lblRunCount As Windows.Forms.Label
		Friend WithEvents startMinimizedToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Private WithEvents txtWait As Windows.Forms.TextBox
		Private WithEvents Label3 As Windows.Forms.Label
		Friend WithEvents cmdResetRunCount As Windows.Forms.LinkLabel
		Friend WithEvents cmdRestartTTNow As Windows.Forms.LinkLabel
	End Class
End Namespace
