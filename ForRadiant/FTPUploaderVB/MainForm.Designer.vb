﻿
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
			Me.UploadToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.AutoUploadTaskToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.StopAutoUploadToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.exitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.settingsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.minimizedToTrayToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.helpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.aboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.txtInterval = New System.Windows.Forms.TextBox()
			Me.label1 = New System.Windows.Forms.Label()
			Me.txtUploadListPath = New System.Windows.Forms.TextBox()
			Me.Label3 = New System.Windows.Forms.Label()
			Me.lblStatus = New System.Windows.Forms.Label()
			Me.lblAutoUpload = New System.Windows.Forms.Label()
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
			Me.contextMenuStrip1.Size = New System.Drawing.Size(181, 48)
			'
			'exit2ToolStripMenuItem
			'
			Me.exit2ToolStripMenuItem.Name = "exit2ToolStripMenuItem"
			Me.exit2ToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
			Me.exit2ToolStripMenuItem.Text = "Exit"
			'
			'menuStrip1
			'
			Me.menuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.commandToolStripMenuItem, Me.settingsToolStripMenuItem, Me.helpToolStripMenuItem})
			Me.menuStrip1.Location = New System.Drawing.Point(0, 0)
			Me.menuStrip1.Name = "menuStrip1"
			Me.menuStrip1.Size = New System.Drawing.Size(424, 24)
			Me.menuStrip1.TabIndex = 1
			Me.menuStrip1.Text = "menuStrip1"
			'
			'commandToolStripMenuItem
			'
			Me.commandToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.UploadToolStripMenuItem, Me.AutoUploadTaskToolStripMenuItem, Me.StopAutoUploadToolStripMenuItem, Me.exitToolStripMenuItem})
			Me.commandToolStripMenuItem.Name = "commandToolStripMenuItem"
			Me.commandToolStripMenuItem.Size = New System.Drawing.Size(76, 20)
			Me.commandToolStripMenuItem.Text = "Command"
			'
			'UploadToolStripMenuItem
			'
			Me.UploadToolStripMenuItem.Name = "UploadToolStripMenuItem"
			Me.UploadToolStripMenuItem.Size = New System.Drawing.Size(209, 22)
			Me.UploadToolStripMenuItem.Text = "Upload Now"
			'
			'AutoUploadTaskToolStripMenuItem
			'
			Me.AutoUploadTaskToolStripMenuItem.Name = "AutoUploadTaskToolStripMenuItem"
			Me.AutoUploadTaskToolStripMenuItem.Size = New System.Drawing.Size(209, 22)
			Me.AutoUploadTaskToolStripMenuItem.Text = "Start/Restart Auto Upload"
			'
			'StopAutoUploadToolStripMenuItem
			'
			Me.StopAutoUploadToolStripMenuItem.Name = "StopAutoUploadToolStripMenuItem"
			Me.StopAutoUploadToolStripMenuItem.Size = New System.Drawing.Size(209, 22)
			Me.StopAutoUploadToolStripMenuItem.Text = "Stop Auto Upload"
			'
			'exitToolStripMenuItem
			'
			Me.exitToolStripMenuItem.Name = "exitToolStripMenuItem"
			Me.exitToolStripMenuItem.Size = New System.Drawing.Size(209, 22)
			Me.exitToolStripMenuItem.Text = "Exit"
			'
			'settingsToolStripMenuItem
			'
			Me.settingsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.minimizedToTrayToolStripMenuItem})
			Me.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem"
			Me.settingsToolStripMenuItem.Size = New System.Drawing.Size(61, 20)
			Me.settingsToolStripMenuItem.Text = "Settings"
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
			Me.txtInterval.Size = New System.Drawing.Size(242, 20)
			Me.txtInterval.TabIndex = 3
			Me.txtInterval.Text = "60"
			'
			'label1
			'
			Me.label1.AutoSize = True
			Me.label1.Location = New System.Drawing.Point(12, 53)
			Me.label1.Name = "label1"
			Me.label1.Size = New System.Drawing.Size(152, 13)
			Me.label1.TabIndex = 4
			Me.label1.Text = "Auto Upload Check Interval (s)"
			'
			'txtUploadListPath
			'
			Me.txtUploadListPath.Location = New System.Drawing.Point(170, 24)
			Me.txtUploadListPath.Name = "txtUploadListPath"
			Me.txtUploadListPath.Size = New System.Drawing.Size(242, 20)
			Me.txtUploadListPath.TabIndex = 1
			Me.txtUploadListPath.Text = "D:\Program\RVS\UploadList"
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
			Me.lblStatus.Location = New System.Drawing.Point(12, 105)
			Me.lblStatus.Name = "lblStatus"
			Me.lblStatus.Size = New System.Drawing.Size(0, 13)
			Me.lblStatus.TabIndex = 6
			'
			'lblAutoUpload
			'
			Me.lblAutoUpload.AutoSize = True
			Me.lblAutoUpload.ForeColor = System.Drawing.Color.Red
			Me.lblAutoUpload.Location = New System.Drawing.Point(12, 80)
			Me.lblAutoUpload.Name = "lblAutoUpload"
			Me.lblAutoUpload.Size = New System.Drawing.Size(109, 13)
			Me.lblAutoUpload.TabIndex = 5
			Me.lblAutoUpload.Text = "Auto Upload Stopped"
			'
			'MainForm
			'
			Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.ClientSize = New System.Drawing.Size(424, 128)
			Me.Controls.Add(Me.lblAutoUpload)
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
		Friend WithEvents UploadToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Private WithEvents txtInterval As Windows.Forms.TextBox
		Private WithEvents label1 As Windows.Forms.Label
		Private WithEvents txtUploadListPath As Windows.Forms.TextBox
		Private WithEvents Label3 As Windows.Forms.Label
		Friend WithEvents AutoUploadTaskToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Private WithEvents lblStatus As Windows.Forms.Label
		Friend WithEvents StopAutoUploadToolStripMenuItem As Windows.Forms.ToolStripMenuItem
		Private WithEvents lblAutoUpload As Windows.Forms.Label
	End Class
End Namespace