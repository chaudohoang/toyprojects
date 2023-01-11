
Imports System

Namespace TemplateAppVB
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
			Me.minimizedToTrayToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.helpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.aboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
			Me.btnBrowse = New System.Windows.Forms.Button()
			Me.txtAdditionalSequence = New System.Windows.Forms.TextBox()
			Me.lblAddtionalTarget = New System.Windows.Forms.Label()
			Me.btnConvert = New System.Windows.Forms.Button()
			Me.Label1 = New System.Windows.Forms.Label()
			Me.btnCopyDI = New System.Windows.Forms.Button()
			Me.contextMenuStrip1.SuspendLayout()
			Me.menuStrip1.SuspendLayout()
			Me.SuspendLayout()
			'
			'notifyIcon1
			'
			Me.notifyIcon1.ContextMenuStrip = Me.contextMenuStrip1
			Me.notifyIcon1.Icon = CType(resources.GetObject("notifyIcon1.Icon"), System.Drawing.Icon)
			Me.notifyIcon1.Text = "Emu2p1SequenceConvert"
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
			Me.menuStrip1.Size = New System.Drawing.Size(749, 24)
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
			'btnBrowse
			'
			Me.btnBrowse.Location = New System.Drawing.Point(514, 49)
			Me.btnBrowse.Name = "btnBrowse"
			Me.btnBrowse.Size = New System.Drawing.Size(70, 73)
			Me.btnBrowse.TabIndex = 2
			Me.btnBrowse.Text = "Browse"
			Me.btnBrowse.UseVisualStyleBackColor = True
			'
			'txtAdditionalSequence
			'
			Me.txtAdditionalSequence.AllowDrop = True
			Me.txtAdditionalSequence.Location = New System.Drawing.Point(17, 49)
			Me.txtAdditionalSequence.Multiline = True
			Me.txtAdditionalSequence.Name = "txtAdditionalSequence"
			Me.txtAdditionalSequence.ScrollBars = System.Windows.Forms.ScrollBars.Both
			Me.txtAdditionalSequence.Size = New System.Drawing.Size(491, 73)
			Me.txtAdditionalSequence.TabIndex = 1
			'
			'lblAddtionalTarget
			'
			Me.lblAddtionalTarget.AutoSize = True
			Me.lblAddtionalTarget.Location = New System.Drawing.Point(12, 33)
			Me.lblAddtionalTarget.Name = "lblAddtionalTarget"
			Me.lblAddtionalTarget.Size = New System.Drawing.Size(99, 13)
			Me.lblAddtionalTarget.TabIndex = 78
			Me.lblAddtionalTarget.Text = "Target Sequence : "
			'
			'btnConvert
			'
			Me.btnConvert.Location = New System.Drawing.Point(590, 49)
			Me.btnConvert.Name = "btnConvert"
			Me.btnConvert.Size = New System.Drawing.Size(70, 73)
			Me.btnConvert.TabIndex = 3
			Me.btnConvert.Text = "Convert Sequences"
			Me.btnConvert.UseVisualStyleBackColor = True
			'
			'Label1
			'
			Me.Label1.AutoSize = True
			Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
			Me.Label1.Location = New System.Drawing.Point(14, 146)
			Me.Label1.Name = "Label1"
			Me.Label1.Size = New System.Drawing.Size(0, 20)
			Me.Label1.TabIndex = 81
			'
			'btnCopyDI
			'
			Me.btnCopyDI.Location = New System.Drawing.Point(666, 49)
			Me.btnCopyDI.Name = "btnCopyDI"
			Me.btnCopyDI.Size = New System.Drawing.Size(70, 73)
			Me.btnCopyDI.TabIndex = 4
			Me.btnCopyDI.Text = "Copy Emu2p1 DI Settings"
			Me.btnCopyDI.UseVisualStyleBackColor = True
			'
			'MainForm
			'
			Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.ClientSize = New System.Drawing.Size(749, 189)
			Me.Controls.Add(Me.btnCopyDI)
			Me.Controls.Add(Me.Label1)
			Me.Controls.Add(Me.btnConvert)
			Me.Controls.Add(Me.btnBrowse)
			Me.Controls.Add(Me.txtAdditionalSequence)
			Me.Controls.Add(Me.lblAddtionalTarget)
			Me.Controls.Add(Me.menuStrip1)
			Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
			Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
			Me.MainMenuStrip = Me.menuStrip1
			Me.MaximizeBox = False
			Me.Name = "MainForm"
			Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
			Me.Text = "Emu2p1SequenceConvert"
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
		Friend WithEvents btnBrowse As Windows.Forms.Button
		Friend WithEvents txtAdditionalSequence As Windows.Forms.TextBox
		Friend WithEvents lblAddtionalTarget As Windows.Forms.Label
		Friend WithEvents btnConvert As Windows.Forms.Button
		Friend WithEvents Label1 As Windows.Forms.Label
		Friend WithEvents btnCopyDI As Windows.Forms.Button
	End Class
End Namespace
