
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
            Me.txtWaitResult = New System.Windows.Forms.TextBox()
            Me.Label3 = New System.Windows.Forms.Label()
            Me.cmdResetRunCount = New System.Windows.Forms.LinkLabel()
            Me.cmdRestartTTNowResult = New System.Windows.Forms.LinkLabel()
            Me.TabControl1 = New System.Windows.Forms.TabControl()
            Me.TabPage1 = New System.Windows.Forms.TabPage()
            Me.lblResultMonitoringStatus = New System.Windows.Forms.Label()
            Me.cmdStopMonitorResultMessage = New System.Windows.Forms.LinkLabel()
            Me.cmdStartMonitorResultMessage = New System.Windows.Forms.LinkLabel()
            Me.TabPage2 = New System.Windows.Forms.TabPage()
            Me.lblRestartMonitoringStatus = New System.Windows.Forms.Label()
            Me.cmdRestartNowRestart = New System.Windows.Forms.LinkLabel()
            Me.txtWaitRestart = New System.Windows.Forms.TextBox()
            Me.Label4 = New System.Windows.Forms.Label()
            Me.cmdStopMonitorRestartMessage = New System.Windows.Forms.LinkLabel()
            Me.cmdStartMonitorRestartMessage = New System.Windows.Forms.LinkLabel()
            Me.TabPage3 = New System.Windows.Forms.TabPage()
            Me.lblOnInitializeMonitoringStatus = New System.Windows.Forms.Label()
            Me.cmdRestartNowOnInitialize = New System.Windows.Forms.LinkLabel()
            Me.txtWaitOnInitialize = New System.Windows.Forms.TextBox()
            Me.Label6 = New System.Windows.Forms.Label()
            Me.cmdStopMonitorOnInitializeMessage = New System.Windows.Forms.LinkLabel()
            Me.cmdStartMonitorOnInitializeMessage = New System.Windows.Forms.LinkLabel()
            Me.Label5 = New System.Windows.Forms.Label()
            Me.lblTodayLogPath = New System.Windows.Forms.Label()
            Me.contextMenuStrip1.SuspendLayout()
            Me.menuStrip1.SuspendLayout()
            Me.TabControl1.SuspendLayout()
            Me.TabPage1.SuspendLayout()
            Me.TabPage2.SuspendLayout()
            Me.TabPage3.SuspendLayout()
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
            Me.menuStrip1.Size = New System.Drawing.Size(546, 24)
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
            Me.aboutToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
            Me.aboutToolStripMenuItem.Text = "About"
            '
            'txtRunCount
            '
            Me.txtRunCount.Location = New System.Drawing.Point(205, 9)
            Me.txtRunCount.Name = "txtRunCount"
            Me.txtRunCount.Size = New System.Drawing.Size(43, 20)
            Me.txtRunCount.TabIndex = 3
            Me.txtRunCount.Text = "100"
            '
            'label1
            '
            Me.label1.AutoSize = True
            Me.label1.Location = New System.Drawing.Point(6, 12)
            Me.label1.Name = "label1"
            Me.label1.Size = New System.Drawing.Size(193, 13)
            Me.label1.TabIndex = 4
            Me.label1.Text = "Auto Restart TT Every Number of Runs"
            '
            'label2
            '
            Me.label2.AutoSize = True
            Me.label2.ForeColor = System.Drawing.Color.Purple
            Me.label2.Location = New System.Drawing.Point(7, 82)
            Me.label2.Name = "label2"
            Me.label2.Size = New System.Drawing.Size(67, 13)
            Me.label2.TabIndex = 11
            Me.label2.Text = "Run Count : "
            '
            'cmdSaveSettings
            '
            Me.cmdSaveSettings.AutoSize = True
            Me.cmdSaveSettings.Location = New System.Drawing.Point(6, 59)
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
            Me.lblRunCount.Location = New System.Drawing.Point(80, 82)
            Me.lblRunCount.Name = "lblRunCount"
            Me.lblRunCount.Size = New System.Drawing.Size(13, 13)
            Me.lblRunCount.TabIndex = 15
            Me.lblRunCount.Text = "0"
            '
            'txtWaitResult
            '
            Me.txtWaitResult.Location = New System.Drawing.Point(205, 34)
            Me.txtWaitResult.Name = "txtWaitResult"
            Me.txtWaitResult.Size = New System.Drawing.Size(43, 20)
            Me.txtWaitResult.TabIndex = 17
            Me.txtWaitResult.Text = "3"
            '
            'Label3
            '
            Me.Label3.AutoSize = True
            Me.Label3.Location = New System.Drawing.Point(6, 37)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New System.Drawing.Size(130, 13)
            Me.Label3.TabIndex = 18
            Me.Label3.Text = "Wait before Restart TT (s)"
            '
            'cmdResetRunCount
            '
            Me.cmdResetRunCount.AutoSize = True
            Me.cmdResetRunCount.LinkColor = System.Drawing.Color.Green
            Me.cmdResetRunCount.Location = New System.Drawing.Point(80, 59)
            Me.cmdResetRunCount.Name = "cmdResetRunCount"
            Me.cmdResetRunCount.Size = New System.Drawing.Size(89, 13)
            Me.cmdResetRunCount.TabIndex = 19
            Me.cmdResetRunCount.TabStop = True
            Me.cmdResetRunCount.Text = "Reset Run Count"
            '
            'cmdRestartTTNowResult
            '
            Me.cmdRestartTTNowResult.AutoSize = True
            Me.cmdRestartTTNowResult.LinkColor = System.Drawing.Color.MediumVioletRed
            Me.cmdRestartTTNowResult.Location = New System.Drawing.Point(175, 59)
            Me.cmdRestartTTNowResult.Name = "cmdRestartTTNowResult"
            Me.cmdRestartTTNowResult.Size = New System.Drawing.Size(83, 13)
            Me.cmdRestartTTNowResult.TabIndex = 20
            Me.cmdRestartTTNowResult.TabStop = True
            Me.cmdRestartTTNowResult.Text = "Restart TT Now"
            '
            'TabControl1
            '
            Me.TabControl1.Controls.Add(Me.TabPage1)
            Me.TabControl1.Controls.Add(Me.TabPage2)
            Me.TabControl1.Controls.Add(Me.TabPage3)
            Me.TabControl1.Location = New System.Drawing.Point(12, 60)
            Me.TabControl1.Name = "TabControl1"
            Me.TabControl1.SelectedIndex = 0
            Me.TabControl1.Size = New System.Drawing.Size(524, 276)
            Me.TabControl1.TabIndex = 21
            '
            'TabPage1
            '
            Me.TabPage1.Controls.Add(Me.lblResultMonitoringStatus)
            Me.TabPage1.Controls.Add(Me.cmdStopMonitorResultMessage)
            Me.TabPage1.Controls.Add(Me.cmdStartMonitorResultMessage)
            Me.TabPage1.Controls.Add(Me.label1)
            Me.TabPage1.Controls.Add(Me.txtRunCount)
            Me.TabPage1.Controls.Add(Me.cmdRestartTTNowResult)
            Me.TabPage1.Controls.Add(Me.label2)
            Me.TabPage1.Controls.Add(Me.cmdResetRunCount)
            Me.TabPage1.Controls.Add(Me.cmdSaveSettings)
            Me.TabPage1.Controls.Add(Me.txtWaitResult)
            Me.TabPage1.Controls.Add(Me.lblRunCount)
            Me.TabPage1.Controls.Add(Me.Label3)
            Me.TabPage1.Location = New System.Drawing.Point(4, 22)
            Me.TabPage1.Name = "TabPage1"
            Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPage1.Size = New System.Drawing.Size(516, 250)
            Me.TabPage1.TabIndex = 0
            Me.TabPage1.Text = "Monitor Result Message"
            Me.TabPage1.UseVisualStyleBackColor = True
            '
            'lblResultMonitoringStatus
            '
            Me.lblResultMonitoringStatus.AutoSize = True
            Me.lblResultMonitoringStatus.ForeColor = System.Drawing.Color.Blue
            Me.lblResultMonitoringStatus.Location = New System.Drawing.Point(7, 128)
            Me.lblResultMonitoringStatus.Name = "lblResultMonitoringStatus"
            Me.lblResultMonitoringStatus.Size = New System.Drawing.Size(46, 13)
            Me.lblResultMonitoringStatus.TabIndex = 23
            Me.lblResultMonitoringStatus.Text = "Status : "
            '
            'cmdStopMonitorResultMessage
            '
            Me.cmdStopMonitorResultMessage.AutoSize = True
            Me.cmdStopMonitorResultMessage.LinkColor = System.Drawing.Color.DarkCyan
            Me.cmdStopMonitorResultMessage.Location = New System.Drawing.Point(94, 107)
            Me.cmdStopMonitorResultMessage.Name = "cmdStopMonitorResultMessage"
            Me.cmdStopMonitorResultMessage.Size = New System.Drawing.Size(81, 13)
            Me.cmdStopMonitorResultMessage.TabIndex = 22
            Me.cmdStopMonitorResultMessage.TabStop = True
            Me.cmdStopMonitorResultMessage.Text = "Stop Monitoring"
            '
            'cmdStartMonitorResultMessage
            '
            Me.cmdStartMonitorResultMessage.AutoSize = True
            Me.cmdStartMonitorResultMessage.LinkColor = System.Drawing.Color.Chocolate
            Me.cmdStartMonitorResultMessage.Location = New System.Drawing.Point(7, 107)
            Me.cmdStartMonitorResultMessage.Name = "cmdStartMonitorResultMessage"
            Me.cmdStartMonitorResultMessage.Size = New System.Drawing.Size(81, 13)
            Me.cmdStartMonitorResultMessage.TabIndex = 21
            Me.cmdStartMonitorResultMessage.TabStop = True
            Me.cmdStartMonitorResultMessage.Text = "Start Monitoring"
            '
            'TabPage2
            '
            Me.TabPage2.Controls.Add(Me.lblRestartMonitoringStatus)
            Me.TabPage2.Controls.Add(Me.cmdRestartNowRestart)
            Me.TabPage2.Controls.Add(Me.txtWaitRestart)
            Me.TabPage2.Controls.Add(Me.Label4)
            Me.TabPage2.Controls.Add(Me.cmdStopMonitorRestartMessage)
            Me.TabPage2.Controls.Add(Me.cmdStartMonitorRestartMessage)
            Me.TabPage2.Location = New System.Drawing.Point(4, 22)
            Me.TabPage2.Name = "TabPage2"
            Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPage2.Size = New System.Drawing.Size(467, 250)
            Me.TabPage2.TabIndex = 1
            Me.TabPage2.Text = "Monitor Restart Message"
            Me.TabPage2.UseVisualStyleBackColor = True
            '
            'lblRestartMonitoringStatus
            '
            Me.lblRestartMonitoringStatus.AutoSize = True
            Me.lblRestartMonitoringStatus.ForeColor = System.Drawing.Color.Blue
            Me.lblRestartMonitoringStatus.Location = New System.Drawing.Point(7, 128)
            Me.lblRestartMonitoringStatus.Name = "lblRestartMonitoringStatus"
            Me.lblRestartMonitoringStatus.Size = New System.Drawing.Size(46, 13)
            Me.lblRestartMonitoringStatus.TabIndex = 29
            Me.lblRestartMonitoringStatus.Text = "Status : "
            '
            'cmdRestartNowRestart
            '
            Me.cmdRestartNowRestart.AutoSize = True
            Me.cmdRestartNowRestart.LinkColor = System.Drawing.Color.MediumVioletRed
            Me.cmdRestartNowRestart.Location = New System.Drawing.Point(176, 34)
            Me.cmdRestartNowRestart.Name = "cmdRestartNowRestart"
            Me.cmdRestartNowRestart.Size = New System.Drawing.Size(83, 13)
            Me.cmdRestartNowRestart.TabIndex = 28
            Me.cmdRestartNowRestart.TabStop = True
            Me.cmdRestartNowRestart.Text = "Restart TT Now"
            '
            'txtWaitRestart
            '
            Me.txtWaitRestart.Location = New System.Drawing.Point(206, 9)
            Me.txtWaitRestart.Name = "txtWaitRestart"
            Me.txtWaitRestart.Size = New System.Drawing.Size(43, 20)
            Me.txtWaitRestart.TabIndex = 26
            Me.txtWaitRestart.Text = "3"
            '
            'Label4
            '
            Me.Label4.AutoSize = True
            Me.Label4.Location = New System.Drawing.Point(7, 12)
            Me.Label4.Name = "Label4"
            Me.Label4.Size = New System.Drawing.Size(130, 13)
            Me.Label4.TabIndex = 27
            Me.Label4.Text = "Wait before Restart TT (s)"
            '
            'cmdStopMonitorRestartMessage
            '
            Me.cmdStopMonitorRestartMessage.AutoSize = True
            Me.cmdStopMonitorRestartMessage.LinkColor = System.Drawing.Color.DarkCyan
            Me.cmdStopMonitorRestartMessage.Location = New System.Drawing.Point(94, 107)
            Me.cmdStopMonitorRestartMessage.Name = "cmdStopMonitorRestartMessage"
            Me.cmdStopMonitorRestartMessage.Size = New System.Drawing.Size(81, 13)
            Me.cmdStopMonitorRestartMessage.TabIndex = 24
            Me.cmdStopMonitorRestartMessage.TabStop = True
            Me.cmdStopMonitorRestartMessage.Text = "Stop Monitoring"
            '
            'cmdStartMonitorRestartMessage
            '
            Me.cmdStartMonitorRestartMessage.AutoSize = True
            Me.cmdStartMonitorRestartMessage.LinkColor = System.Drawing.Color.Chocolate
            Me.cmdStartMonitorRestartMessage.Location = New System.Drawing.Point(7, 107)
            Me.cmdStartMonitorRestartMessage.Name = "cmdStartMonitorRestartMessage"
            Me.cmdStartMonitorRestartMessage.Size = New System.Drawing.Size(81, 13)
            Me.cmdStartMonitorRestartMessage.TabIndex = 23
            Me.cmdStartMonitorRestartMessage.TabStop = True
            Me.cmdStartMonitorRestartMessage.Text = "Start Monitoring"
            '
            'TabPage3
            '
            Me.TabPage3.Controls.Add(Me.lblOnInitializeMonitoringStatus)
            Me.TabPage3.Controls.Add(Me.cmdRestartNowOnInitialize)
            Me.TabPage3.Controls.Add(Me.txtWaitOnInitialize)
            Me.TabPage3.Controls.Add(Me.Label6)
            Me.TabPage3.Controls.Add(Me.cmdStopMonitorOnInitializeMessage)
            Me.TabPage3.Controls.Add(Me.cmdStartMonitorOnInitializeMessage)
            Me.TabPage3.Location = New System.Drawing.Point(4, 22)
            Me.TabPage3.Name = "TabPage3"
            Me.TabPage3.Size = New System.Drawing.Size(516, 250)
            Me.TabPage3.TabIndex = 2
            Me.TabPage3.Text = "Monitor On initialize #2"
            Me.TabPage3.UseVisualStyleBackColor = True
            '
            'lblOnInitializeMonitoringStatus
            '
            Me.lblOnInitializeMonitoringStatus.AutoSize = True
            Me.lblOnInitializeMonitoringStatus.ForeColor = System.Drawing.Color.Blue
            Me.lblOnInitializeMonitoringStatus.Location = New System.Drawing.Point(7, 128)
            Me.lblOnInitializeMonitoringStatus.Name = "lblOnInitializeMonitoringStatus"
            Me.lblOnInitializeMonitoringStatus.Size = New System.Drawing.Size(46, 13)
            Me.lblOnInitializeMonitoringStatus.TabIndex = 35
            Me.lblOnInitializeMonitoringStatus.Text = "Status : "
            '
            'cmdRestartNowOnInitialize
            '
            Me.cmdRestartNowOnInitialize.AutoSize = True
            Me.cmdRestartNowOnInitialize.LinkColor = System.Drawing.Color.MediumVioletRed
            Me.cmdRestartNowOnInitialize.Location = New System.Drawing.Point(176, 34)
            Me.cmdRestartNowOnInitialize.Name = "cmdRestartNowOnInitialize"
            Me.cmdRestartNowOnInitialize.Size = New System.Drawing.Size(83, 13)
            Me.cmdRestartNowOnInitialize.TabIndex = 34
            Me.cmdRestartNowOnInitialize.TabStop = True
            Me.cmdRestartNowOnInitialize.Text = "Restart TT Now"
            '
            'txtWaitOnInitialize
            '
            Me.txtWaitOnInitialize.Location = New System.Drawing.Point(206, 9)
            Me.txtWaitOnInitialize.Name = "txtWaitOnInitialize"
            Me.txtWaitOnInitialize.Size = New System.Drawing.Size(43, 20)
            Me.txtWaitOnInitialize.TabIndex = 32
            Me.txtWaitOnInitialize.Text = "3"
            '
            'Label6
            '
            Me.Label6.AutoSize = True
            Me.Label6.Location = New System.Drawing.Point(7, 12)
            Me.Label6.Name = "Label6"
            Me.Label6.Size = New System.Drawing.Size(130, 13)
            Me.Label6.TabIndex = 33
            Me.Label6.Text = "Wait before Restart TT (s)"
            '
            'cmdStopMonitorOnInitializeMessage
            '
            Me.cmdStopMonitorOnInitializeMessage.AutoSize = True
            Me.cmdStopMonitorOnInitializeMessage.LinkColor = System.Drawing.Color.DarkCyan
            Me.cmdStopMonitorOnInitializeMessage.Location = New System.Drawing.Point(94, 107)
            Me.cmdStopMonitorOnInitializeMessage.Name = "cmdStopMonitorOnInitializeMessage"
            Me.cmdStopMonitorOnInitializeMessage.Size = New System.Drawing.Size(81, 13)
            Me.cmdStopMonitorOnInitializeMessage.TabIndex = 31
            Me.cmdStopMonitorOnInitializeMessage.TabStop = True
            Me.cmdStopMonitorOnInitializeMessage.Text = "Stop Monitoring"
            '
            'cmdStartMonitorOnInitializeMessage
            '
            Me.cmdStartMonitorOnInitializeMessage.AutoSize = True
            Me.cmdStartMonitorOnInitializeMessage.LinkColor = System.Drawing.Color.Chocolate
            Me.cmdStartMonitorOnInitializeMessage.Location = New System.Drawing.Point(7, 107)
            Me.cmdStartMonitorOnInitializeMessage.Name = "cmdStartMonitorOnInitializeMessage"
            Me.cmdStartMonitorOnInitializeMessage.Size = New System.Drawing.Size(81, 13)
            Me.cmdStartMonitorOnInitializeMessage.TabIndex = 30
            Me.cmdStartMonitorOnInitializeMessage.TabStop = True
            Me.cmdStartMonitorOnInitializeMessage.Text = "Start Monitoring"
            '
            'Label5
            '
            Me.Label5.AutoSize = True
            Me.Label5.Location = New System.Drawing.Point(13, 31)
            Me.Label5.Name = "Label5"
            Me.Label5.Size = New System.Drawing.Size(67, 13)
            Me.Label5.TabIndex = 22
            Me.Label5.Text = "Today Log : "
            '
            'lblTodayLogPath
            '
            Me.lblTodayLogPath.AutoSize = True
            Me.lblTodayLogPath.Location = New System.Drawing.Point(86, 31)
            Me.lblTodayLogPath.Name = "lblTodayLogPath"
            Me.lblTodayLogPath.Size = New System.Drawing.Size(0, 13)
            Me.lblTodayLogPath.TabIndex = 23
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(546, 348)
            Me.Controls.Add(Me.lblTodayLogPath)
            Me.Controls.Add(Me.Label5)
            Me.Controls.Add(Me.TabControl1)
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
            Me.TabControl1.ResumeLayout(False)
            Me.TabPage1.ResumeLayout(False)
            Me.TabPage1.PerformLayout()
            Me.TabPage2.ResumeLayout(False)
            Me.TabPage2.PerformLayout()
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
		Private WithEvents txtRunCount As Windows.Forms.TextBox
		Private WithEvents label1 As Windows.Forms.Label
		Private WithEvents label2 As Windows.Forms.Label
		Friend WithEvents cmdSaveSettings As Windows.Forms.LinkLabel
		Private WithEvents lblRunCount As Windows.Forms.Label
		Friend WithEvents startMinimizedToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Private WithEvents txtWaitResult As Windows.Forms.TextBox
		Private WithEvents Label3 As Windows.Forms.Label
		Friend WithEvents cmdResetRunCount As Windows.Forms.LinkLabel
		Friend WithEvents cmdRestartTTNowResult As Windows.Forms.LinkLabel
		Friend WithEvents TabControl1 As Windows.Forms.TabControl
		Friend WithEvents TabPage1 As Windows.Forms.TabPage
		Friend WithEvents cmdStopMonitorResultMessage As Windows.Forms.LinkLabel
		Friend WithEvents cmdStartMonitorResultMessage As Windows.Forms.LinkLabel
		Friend WithEvents TabPage2 As Windows.Forms.TabPage
		Friend WithEvents cmdStopMonitorRestartMessage As Windows.Forms.LinkLabel
		Friend WithEvents cmdStartMonitorRestartMessage As Windows.Forms.LinkLabel
		Friend WithEvents cmdRestartNowRestart As Windows.Forms.LinkLabel
		Private WithEvents txtWaitRestart As Windows.Forms.TextBox
		Private WithEvents Label4 As Windows.Forms.Label
		Private WithEvents lblResultMonitoringStatus As Windows.Forms.Label
		Private WithEvents lblRestartMonitoringStatus As Windows.Forms.Label
		Friend WithEvents TabPage3 As Windows.Forms.TabPage
		Private WithEvents lblOnInitializeMonitoringStatus As Windows.Forms.Label
		Friend WithEvents cmdRestartNowOnInitialize As Windows.Forms.LinkLabel
		Private WithEvents txtWaitOnInitialize As Windows.Forms.TextBox
		Private WithEvents Label6 As Windows.Forms.Label
		Friend WithEvents cmdStopMonitorOnInitializeMessage As Windows.Forms.LinkLabel
		Friend WithEvents cmdStartMonitorOnInitializeMessage As Windows.Forms.LinkLabel
        Friend WithEvents Label5 As Windows.Forms.Label
        Friend WithEvents lblTodayLogPath As Windows.Forms.Label
    End Class
End Namespace
