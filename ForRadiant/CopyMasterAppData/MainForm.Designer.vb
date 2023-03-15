
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
            Me.ListBox1 = New System.Windows.Forms.ListBox()
            Me.btnAddSdf = New System.Windows.Forms.Button()
            Me.btnAddXml = New System.Windows.Forms.Button()
            Me.btnAddCsv = New System.Windows.Forms.Button()
            Me.btnAddCustom = New System.Windows.Forms.Button()
            Me.btnDelItems = New System.Windows.Forms.Button()
            Me.btnClearList = New System.Windows.Forms.Button()
            Me.btnSaveList = New System.Windows.Forms.Button()
            Me.btnLoadList = New System.Windows.Forms.Button()
            Me.btnCopy = New System.Windows.Forms.Button()
            Me.Label1 = New System.Windows.Forms.Label()
            Me.contextMenuStrip1.SuspendLayout()
            Me.menuStrip1.SuspendLayout()
            Me.SuspendLayout()
            '
            'notifyIcon1
            '
            Me.notifyIcon1.ContextMenuStrip = Me.contextMenuStrip1
            Me.notifyIcon1.Icon = CType(resources.GetObject("notifyIcon1.Icon"), System.Drawing.Icon)
            Me.notifyIcon1.Text = "CopyMasterAppData"
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
            Me.menuStrip1.Size = New System.Drawing.Size(671, 24)
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
            'ListBox1
            '
            Me.ListBox1.FormattingEnabled = True
            Me.ListBox1.Location = New System.Drawing.Point(12, 80)
            Me.ListBox1.Name = "ListBox1"
            Me.ListBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
            Me.ListBox1.Size = New System.Drawing.Size(652, 589)
            Me.ListBox1.TabIndex = 2
            '
            'btnAddSdf
            '
            Me.btnAddSdf.Location = New System.Drawing.Point(12, 27)
            Me.btnAddSdf.Name = "btnAddSdf"
            Me.btnAddSdf.Size = New System.Drawing.Size(59, 47)
            Me.btnAddSdf.TabIndex = 3
            Me.btnAddSdf.Text = "Add Sdf"
            Me.btnAddSdf.UseVisualStyleBackColor = True
            '
            'btnAddXml
            '
            Me.btnAddXml.Location = New System.Drawing.Point(77, 27)
            Me.btnAddXml.Name = "btnAddXml"
            Me.btnAddXml.Size = New System.Drawing.Size(59, 47)
            Me.btnAddXml.TabIndex = 4
            Me.btnAddXml.Text = "Add Xml"
            Me.btnAddXml.UseVisualStyleBackColor = True
            '
            'btnAddCsv
            '
            Me.btnAddCsv.Location = New System.Drawing.Point(142, 27)
            Me.btnAddCsv.Name = "btnAddCsv"
            Me.btnAddCsv.Size = New System.Drawing.Size(59, 47)
            Me.btnAddCsv.TabIndex = 5
            Me.btnAddCsv.Text = "Add Csv"
            Me.btnAddCsv.UseVisualStyleBackColor = True
            '
            'btnAddCustom
            '
            Me.btnAddCustom.Location = New System.Drawing.Point(207, 27)
            Me.btnAddCustom.Name = "btnAddCustom"
            Me.btnAddCustom.Size = New System.Drawing.Size(59, 47)
            Me.btnAddCustom.TabIndex = 6
            Me.btnAddCustom.Text = "Add Custom"
            Me.btnAddCustom.UseVisualStyleBackColor = True
            '
            'btnDelItems
            '
            Me.btnDelItems.Location = New System.Drawing.Point(272, 27)
            Me.btnDelItems.Name = "btnDelItems"
            Me.btnDelItems.Size = New System.Drawing.Size(59, 47)
            Me.btnDelItems.TabIndex = 7
            Me.btnDelItems.Text = "Del Items"
            Me.btnDelItems.UseVisualStyleBackColor = True
            '
            'btnClearList
            '
            Me.btnClearList.Location = New System.Drawing.Point(337, 27)
            Me.btnClearList.Name = "btnClearList"
            Me.btnClearList.Size = New System.Drawing.Size(59, 47)
            Me.btnClearList.TabIndex = 8
            Me.btnClearList.Text = "Clear List"
            Me.btnClearList.UseVisualStyleBackColor = True
            '
            'btnSaveList
            '
            Me.btnSaveList.Location = New System.Drawing.Point(402, 27)
            Me.btnSaveList.Name = "btnSaveList"
            Me.btnSaveList.Size = New System.Drawing.Size(59, 47)
            Me.btnSaveList.TabIndex = 9
            Me.btnSaveList.Text = "Save List"
            Me.btnSaveList.UseVisualStyleBackColor = True
            '
            'btnLoadList
            '
            Me.btnLoadList.Location = New System.Drawing.Point(467, 27)
            Me.btnLoadList.Name = "btnLoadList"
            Me.btnLoadList.Size = New System.Drawing.Size(59, 47)
            Me.btnLoadList.TabIndex = 10
            Me.btnLoadList.Text = "Load List"
            Me.btnLoadList.UseVisualStyleBackColor = True
            '
            'btnCopy
            '
            Me.btnCopy.BackColor = System.Drawing.Color.AntiqueWhite
            Me.btnCopy.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.btnCopy.ForeColor = System.Drawing.Color.Blue
            Me.btnCopy.Location = New System.Drawing.Point(532, 27)
            Me.btnCopy.Name = "btnCopy"
            Me.btnCopy.Size = New System.Drawing.Size(59, 47)
            Me.btnCopy.TabIndex = 11
            Me.btnCopy.Text = "COPY !"
            Me.btnCopy.UseVisualStyleBackColor = False
            '
            'Label1
            '
            Me.Label1.AutoSize = True
            Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label1.Location = New System.Drawing.Point(606, 44)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(0, 13)
            Me.Label1.TabIndex = 12
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(671, 675)
            Me.Controls.Add(Me.Label1)
            Me.Controls.Add(Me.btnCopy)
            Me.Controls.Add(Me.btnLoadList)
            Me.Controls.Add(Me.btnSaveList)
            Me.Controls.Add(Me.btnClearList)
            Me.Controls.Add(Me.btnDelItems)
            Me.Controls.Add(Me.btnAddCustom)
            Me.Controls.Add(Me.btnAddCsv)
            Me.Controls.Add(Me.btnAddXml)
            Me.Controls.Add(Me.btnAddSdf)
            Me.Controls.Add(Me.ListBox1)
            Me.Controls.Add(Me.menuStrip1)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
            Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
            Me.MainMenuStrip = Me.menuStrip1
            Me.MaximizeBox = False
            Me.Name = "MainForm"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "CopyMasterAppData"
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
        Friend WithEvents ListBox1 As Windows.Forms.ListBox
        Friend WithEvents btnAddSdf As Windows.Forms.Button
        Friend WithEvents btnAddXml As Windows.Forms.Button
        Friend WithEvents btnAddCsv As Windows.Forms.Button
        Friend WithEvents btnAddCustom As Windows.Forms.Button
        Friend WithEvents btnDelItems As Windows.Forms.Button
        Friend WithEvents btnClearList As Windows.Forms.Button
        Friend WithEvents btnSaveList As Windows.Forms.Button
        Friend WithEvents btnLoadList As Windows.Forms.Button
        Friend WithEvents btnCopy As Windows.Forms.Button
        Friend WithEvents Label1 As Windows.Forms.Label
    End Class
End Namespace
