
Imports System

Namespace FTPUploaderVB
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
            Me.txtInterval = New System.Windows.Forms.TextBox()
            Me.label1 = New System.Windows.Forms.Label()
            Me.txtUploadListPath = New System.Windows.Forms.TextBox()
            Me.Label3 = New System.Windows.Forms.Label()
            Me.lblStatus = New System.Windows.Forms.Label()
            Me.txtMaximumUpload = New System.Windows.Forms.TextBox()
            Me.Label2 = New System.Windows.Forms.Label()
            Me.lblFileStatus = New System.Windows.Forms.Label()
            Me.lblFileUploadStatus = New System.Windows.Forms.Label()
            Me.cmdStartUpload = New System.Windows.Forms.LinkLabel()
            Me.cmdStopUpload = New System.Windows.Forms.LinkLabel()
            Me.txtMaximumFailRetry = New System.Windows.Forms.TextBox()
            Me.Label4 = New System.Windows.Forms.Label()
            Me.backupQueueAfterUploadToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.contextMenuStrip1.SuspendLayout()
            Me.menuStrip1.SuspendLayout()
            Me.SuspendLayout()
            '
            'notifyIcon1
            '
            Me.notifyIcon1.ContextMenuStrip = Me.contextMenuStrip1
            Me.notifyIcon1.Icon = CType(resources.GetObject("notifyIcon1.Icon"), System.Drawing.Icon)
            Me.notifyIcon1.Text = "FTPUploaderVB"
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
            Me.menuStrip1.Size = New System.Drawing.Size(547, 24)
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
            Me.settingsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.startMinimizedToolStripMenuItem, Me.minimizedToTrayToolStripMenuItem, Me.backupQueueAfterUploadToolStripMenuItem})
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
            'txtInterval
            '
            Me.txtInterval.Location = New System.Drawing.Point(170, 50)
            Me.txtInterval.Name = "txtInterval"
            Me.txtInterval.Size = New System.Drawing.Size(365, 20)
            Me.txtInterval.TabIndex = 3
            Me.txtInterval.Text = "10"
            '
            'label1
            '
            Me.label1.AutoSize = True
            Me.label1.Location = New System.Drawing.Point(12, 53)
            Me.label1.Name = "label1"
            Me.label1.Size = New System.Drawing.Size(110, 13)
            Me.label1.TabIndex = 4
            Me.label1.Text = "Auto Upload Every (s)"
            '
            'txtUploadListPath
            '
            Me.txtUploadListPath.Location = New System.Drawing.Point(170, 24)
            Me.txtUploadListPath.Name = "txtUploadListPath"
            Me.txtUploadListPath.Size = New System.Drawing.Size(365, 20)
            Me.txtUploadListPath.TabIndex = 1
            Me.txtUploadListPath.Text = "D:\Program\RVS\UploadQueue"
            '
            'Label3
            '
            Me.Label3.AutoSize = True
            Me.Label3.Location = New System.Drawing.Point(12, 27)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New System.Drawing.Size(101, 13)
            Me.Label3.TabIndex = 2
            Me.Label3.Text = "Upload Queue Path"
            '
            'lblStatus
            '
            Me.lblStatus.AutoSize = True
            Me.lblStatus.ForeColor = System.Drawing.Color.Blue
            Me.lblStatus.Location = New System.Drawing.Point(12, 191)
            Me.lblStatus.Name = "lblStatus"
            Me.lblStatus.Size = New System.Drawing.Size(0, 13)
            Me.lblStatus.TabIndex = 6
            '
            'txtMaximumUpload
            '
            Me.txtMaximumUpload.Location = New System.Drawing.Point(170, 76)
            Me.txtMaximumUpload.Name = "txtMaximumUpload"
            Me.txtMaximumUpload.Size = New System.Drawing.Size(365, 20)
            Me.txtMaximumUpload.TabIndex = 7
            Me.txtMaximumUpload.Text = "60"
            '
            'Label2
            '
            Me.Label2.AutoSize = True
            Me.Label2.Location = New System.Drawing.Point(12, 79)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(135, 13)
            Me.Label2.TabIndex = 8
            Me.Label2.Text = "Maximum Uploads Per Run"
            '
            'lblFileStatus
            '
            Me.lblFileStatus.AutoSize = True
            Me.lblFileStatus.ForeColor = System.Drawing.Color.Purple
            Me.lblFileStatus.Location = New System.Drawing.Point(12, 153)
            Me.lblFileStatus.Name = "lblFileStatus"
            Me.lblFileStatus.Size = New System.Drawing.Size(0, 13)
            Me.lblFileStatus.TabIndex = 11
            '
            'lblFileUploadStatus
            '
            Me.lblFileUploadStatus.AutoSize = True
            Me.lblFileUploadStatus.ForeColor = System.Drawing.Color.Purple
            Me.lblFileUploadStatus.Location = New System.Drawing.Point(12, 173)
            Me.lblFileUploadStatus.Name = "lblFileUploadStatus"
            Me.lblFileUploadStatus.Size = New System.Drawing.Size(0, 13)
            Me.lblFileUploadStatus.TabIndex = 12
            '
            'cmdStartUpload
            '
            Me.cmdStartUpload.AutoSize = True
            Me.cmdStartUpload.Location = New System.Drawing.Point(12, 130)
            Me.cmdStartUpload.Name = "cmdStartUpload"
            Me.cmdStartUpload.Size = New System.Drawing.Size(66, 13)
            Me.cmdStartUpload.TabIndex = 13
            Me.cmdStartUpload.TabStop = True
            Me.cmdStartUpload.Text = "Start Upload"
            '
            'cmdStopUpload
            '
            Me.cmdStopUpload.AutoSize = True
            Me.cmdStopUpload.LinkColor = System.Drawing.Color.Red
            Me.cmdStopUpload.Location = New System.Drawing.Point(84, 130)
            Me.cmdStopUpload.Name = "cmdStopUpload"
            Me.cmdStopUpload.Size = New System.Drawing.Size(66, 13)
            Me.cmdStopUpload.TabIndex = 14
            Me.cmdStopUpload.TabStop = True
            Me.cmdStopUpload.Text = "Stop Upload"
            '
            'txtMaximumFailRetry
            '
            Me.txtMaximumFailRetry.Location = New System.Drawing.Point(170, 102)
            Me.txtMaximumFailRetry.Name = "txtMaximumFailRetry"
            Me.txtMaximumFailRetry.Size = New System.Drawing.Size(365, 20)
            Me.txtMaximumFailRetry.TabIndex = 16
            Me.txtMaximumFailRetry.Text = "5"
            '
            'Label4
            '
            Me.Label4.AutoSize = True
            Me.Label4.Location = New System.Drawing.Point(12, 105)
            Me.Label4.Name = "Label4"
            Me.Label4.Size = New System.Drawing.Size(98, 13)
            Me.Label4.TabIndex = 17
            Me.Label4.Text = "Maximum Fail Retry"
            '
            'backupQueueAfterUploadToolStripMenuItem
            '
            Me.backupQueueAfterUploadToolStripMenuItem.CheckOnClick = True
            Me.backupQueueAfterUploadToolStripMenuItem.Name = "backupQueueAfterUploadToolStripMenuItem"
            Me.backupQueueAfterUploadToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
            Me.backupQueueAfterUploadToolStripMenuItem.Text = "Backup Queue After Upload"
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(547, 216)
            Me.Controls.Add(Me.txtMaximumFailRetry)
            Me.Controls.Add(Me.Label4)
            Me.Controls.Add(Me.cmdStopUpload)
            Me.Controls.Add(Me.cmdStartUpload)
            Me.Controls.Add(Me.lblFileUploadStatus)
            Me.Controls.Add(Me.lblFileStatus)
            Me.Controls.Add(Me.txtMaximumUpload)
            Me.Controls.Add(Me.Label2)
            Me.Controls.Add(Me.lblStatus)
            Me.Controls.Add(Me.txtUploadListPath)
            Me.Controls.Add(Me.Label3)
            Me.Controls.Add(Me.txtInterval)
            Me.Controls.Add(Me.label1)
            Me.Controls.Add(Me.menuStrip1)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
            Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
            Me.MainMenuStrip = Me.menuStrip1
            Me.MaximizeBox = False
            Me.Name = "MainForm"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "FTPUploaderVB"
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
		Private WithEvents txtInterval As Windows.Forms.TextBox
		Private WithEvents label1 As Windows.Forms.Label
		Private WithEvents txtUploadListPath As Windows.Forms.TextBox
		Private WithEvents Label3 As Windows.Forms.Label
		Private WithEvents lblStatus As Windows.Forms.Label
		Private WithEvents txtMaximumUpload As Windows.Forms.TextBox
		Private WithEvents Label2 As Windows.Forms.Label
		Private WithEvents lblFileStatus As Windows.Forms.Label
		Private WithEvents lblFileUploadStatus As Windows.Forms.Label
		Friend WithEvents cmdStartUpload As Windows.Forms.LinkLabel
		Friend WithEvents cmdStopUpload As Windows.Forms.LinkLabel
		Friend WithEvents startMinimizedToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Private WithEvents txtMaximumFailRetry As Windows.Forms.TextBox
		Private WithEvents Label4 As Windows.Forms.Label
        Friend WithEvents backupQueueAfterUploadToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    End Class
End Namespace
