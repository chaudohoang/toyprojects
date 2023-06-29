
Imports System

Namespace CopyMasterAppData
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
            Me.btnAddRFD = New System.Windows.Forms.Button()
            Me.btnLockSameSN = New System.Windows.Forms.Button()
            Me.btnUnlock = New System.Windows.Forms.Button()
            Me.btnLock = New System.Windows.Forms.Button()
            Me.btnDel = New System.Windows.Forms.Button()
            Me.TabControl1 = New System.Windows.Forms.TabControl()
            Me.TabPage1 = New System.Windows.Forms.TabPage()
            Me.TabPage2 = New System.Windows.Forms.TabPage()
            Me.TabPage3 = New System.Windows.Forms.TabPage()
            Me.ListBox2 = New System.Windows.Forms.ListBox()
            Me.btnSaveList2 = New System.Windows.Forms.Button()
            Me.btnUnlock2 = New System.Windows.Forms.Button()
            Me.btnLoadList2 = New System.Windows.Forms.Button()
            Me.btnClearList2 = New System.Windows.Forms.Button()
            Me.btnLock2 = New System.Windows.Forms.Button()
            Me.btnCopy2 = New System.Windows.Forms.Button()
            Me.btnDelItems2 = New System.Windows.Forms.Button()
            Me.btnDel2 = New System.Windows.Forms.Button()
            Me.btnAddCustom2 = New System.Windows.Forms.Button()
            Me.Label2 = New System.Windows.Forms.Label()
            Me.ListBox3 = New System.Windows.Forms.ListBox()
            Me.btnSaveList3 = New System.Windows.Forms.Button()
            Me.btnUnlock3 = New System.Windows.Forms.Button()
            Me.btnLoadList3 = New System.Windows.Forms.Button()
            Me.btnClearList3 = New System.Windows.Forms.Button()
            Me.btnLock3 = New System.Windows.Forms.Button()
            Me.btnCopy3 = New System.Windows.Forms.Button()
            Me.btnDelItems3 = New System.Windows.Forms.Button()
            Me.btnDel3 = New System.Windows.Forms.Button()
            Me.btnAddCustom3 = New System.Windows.Forms.Button()
            Me.Label3 = New System.Windows.Forms.Label()
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
            Me.notifyIcon1.Text = "CopyMaster"
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
            Me.menuStrip1.Size = New System.Drawing.Size(622, 24)
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
            Me.exitToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
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
            Me.ListBox1.Location = New System.Drawing.Point(7, 116)
            Me.ListBox1.Name = "ListBox1"
            Me.ListBox1.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
            Me.ListBox1.Size = New System.Drawing.Size(575, 537)
            Me.ListBox1.TabIndex = 2
            '
            'btnAddSdf
            '
            Me.btnAddSdf.Location = New System.Drawing.Point(7, 7)
            Me.btnAddSdf.Name = "btnAddSdf"
            Me.btnAddSdf.Size = New System.Drawing.Size(40, 47)
            Me.btnAddSdf.TabIndex = 3
            Me.btnAddSdf.Text = "Add Sdf"
            Me.btnAddSdf.UseVisualStyleBackColor = True
            '
            'btnAddXml
            '
            Me.btnAddXml.Location = New System.Drawing.Point(53, 7)
            Me.btnAddXml.Name = "btnAddXml"
            Me.btnAddXml.Size = New System.Drawing.Size(40, 47)
            Me.btnAddXml.TabIndex = 4
            Me.btnAddXml.Text = "Add Xml"
            Me.btnAddXml.UseVisualStyleBackColor = True
            '
            'btnAddCsv
            '
            Me.btnAddCsv.Location = New System.Drawing.Point(99, 7)
            Me.btnAddCsv.Name = "btnAddCsv"
            Me.btnAddCsv.Size = New System.Drawing.Size(40, 47)
            Me.btnAddCsv.TabIndex = 5
            Me.btnAddCsv.Text = "Add Csv"
            Me.btnAddCsv.UseVisualStyleBackColor = True
            '
            'btnAddCustom
            '
            Me.btnAddCustom.Location = New System.Drawing.Point(191, 7)
            Me.btnAddCustom.Name = "btnAddCustom"
            Me.btnAddCustom.Size = New System.Drawing.Size(52, 47)
            Me.btnAddCustom.TabIndex = 6
            Me.btnAddCustom.Text = "Add Custom"
            Me.btnAddCustom.UseVisualStyleBackColor = True
            '
            'btnDelItems
            '
            Me.btnDelItems.Location = New System.Drawing.Point(249, 7)
            Me.btnDelItems.Name = "btnDelItems"
            Me.btnDelItems.Size = New System.Drawing.Size(40, 47)
            Me.btnDelItems.TabIndex = 7
            Me.btnDelItems.Text = "Del Items"
            Me.btnDelItems.UseVisualStyleBackColor = True
            '
            'btnClearList
            '
            Me.btnClearList.Location = New System.Drawing.Point(295, 7)
            Me.btnClearList.Name = "btnClearList"
            Me.btnClearList.Size = New System.Drawing.Size(40, 47)
            Me.btnClearList.TabIndex = 8
            Me.btnClearList.Text = "Clear List"
            Me.btnClearList.UseVisualStyleBackColor = True
            '
            'btnSaveList
            '
            Me.btnSaveList.Location = New System.Drawing.Point(341, 7)
            Me.btnSaveList.Name = "btnSaveList"
            Me.btnSaveList.Size = New System.Drawing.Size(40, 47)
            Me.btnSaveList.TabIndex = 9
            Me.btnSaveList.Text = "Save List"
            Me.btnSaveList.UseVisualStyleBackColor = True
            '
            'btnLoadList
            '
            Me.btnLoadList.Location = New System.Drawing.Point(387, 7)
            Me.btnLoadList.Name = "btnLoadList"
            Me.btnLoadList.Size = New System.Drawing.Size(40, 47)
            Me.btnLoadList.TabIndex = 10
            Me.btnLoadList.Text = "Load List"
            Me.btnLoadList.UseVisualStyleBackColor = True
            '
            'btnCopy
            '
            Me.btnCopy.BackColor = System.Drawing.Color.AntiqueWhite
            Me.btnCopy.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.btnCopy.ForeColor = System.Drawing.Color.Blue
            Me.btnCopy.Location = New System.Drawing.Point(72, 60)
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
            Me.Label1.Location = New System.Drawing.Point(528, 77)
            Me.Label1.Name = "Label1"
            Me.Label1.Size = New System.Drawing.Size(0, 13)
            Me.Label1.TabIndex = 12
            '
            'btnAddRFD
            '
            Me.btnAddRFD.Location = New System.Drawing.Point(145, 7)
            Me.btnAddRFD.Name = "btnAddRFD"
            Me.btnAddRFD.Size = New System.Drawing.Size(40, 47)
            Me.btnAddRFD.TabIndex = 5
            Me.btnAddRFD.Text = "Add RFD"
            Me.btnAddRFD.UseVisualStyleBackColor = True
            '
            'btnLockSameSN
            '
            Me.btnLockSameSN.Location = New System.Drawing.Point(433, 6)
            Me.btnLockSameSN.Name = "btnLockSameSN"
            Me.btnLockSameSN.Size = New System.Drawing.Size(46, 47)
            Me.btnLockSameSN.TabIndex = 13
            Me.btnLockSameSN.Text = "Lock (SameSN)"
            Me.btnLockSameSN.UseVisualStyleBackColor = True
            '
            'btnUnlock
            '
            Me.btnUnlock.Location = New System.Drawing.Point(531, 6)
            Me.btnUnlock.Name = "btnUnlock"
            Me.btnUnlock.Size = New System.Drawing.Size(51, 47)
            Me.btnUnlock.TabIndex = 14
            Me.btnUnlock.Text = "Unlock"
            Me.btnUnlock.UseVisualStyleBackColor = True
            '
            'btnLock
            '
            Me.btnLock.Location = New System.Drawing.Point(485, 6)
            Me.btnLock.Name = "btnLock"
            Me.btnLock.Size = New System.Drawing.Size(40, 47)
            Me.btnLock.TabIndex = 13
            Me.btnLock.Text = "Lock"
            Me.btnLock.UseVisualStyleBackColor = True
            '
            'btnDel
            '
            Me.btnDel.BackColor = System.Drawing.Color.AntiqueWhite
            Me.btnDel.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.btnDel.ForeColor = System.Drawing.Color.Crimson
            Me.btnDel.Location = New System.Drawing.Point(7, 60)
            Me.btnDel.Name = "btnDel"
            Me.btnDel.Size = New System.Drawing.Size(59, 47)
            Me.btnDel.TabIndex = 11
            Me.btnDel.Text = "DEL !"
            Me.btnDel.UseVisualStyleBackColor = False
            '
            'TabControl1
            '
            Me.TabControl1.Controls.Add(Me.TabPage1)
            Me.TabControl1.Controls.Add(Me.TabPage2)
            Me.TabControl1.Controls.Add(Me.TabPage3)
            Me.TabControl1.Location = New System.Drawing.Point(12, 27)
            Me.TabControl1.Name = "TabControl1"
            Me.TabControl1.SelectedIndex = 0
            Me.TabControl1.Size = New System.Drawing.Size(599, 688)
            Me.TabControl1.TabIndex = 16
            '
            'TabPage1
            '
            Me.TabPage1.Controls.Add(Me.btnAddCsv)
            Me.TabPage1.Controls.Add(Me.btnAddSdf)
            Me.TabPage1.Controls.Add(Me.ListBox1)
            Me.TabPage1.Controls.Add(Me.btnSaveList)
            Me.TabPage1.Controls.Add(Me.btnUnlock)
            Me.TabPage1.Controls.Add(Me.btnLoadList)
            Me.TabPage1.Controls.Add(Me.btnClearList)
            Me.TabPage1.Controls.Add(Me.btnLock)
            Me.TabPage1.Controls.Add(Me.btnCopy)
            Me.TabPage1.Controls.Add(Me.btnAddXml)
            Me.TabPage1.Controls.Add(Me.btnDelItems)
            Me.TabPage1.Controls.Add(Me.btnLockSameSN)
            Me.TabPage1.Controls.Add(Me.btnDel)
            Me.TabPage1.Controls.Add(Me.btnAddRFD)
            Me.TabPage1.Controls.Add(Me.btnAddCustom)
            Me.TabPage1.Controls.Add(Me.Label1)
            Me.TabPage1.Location = New System.Drawing.Point(4, 22)
            Me.TabPage1.Name = "TabPage1"
            Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPage1.Size = New System.Drawing.Size(591, 662)
            Me.TabPage1.TabIndex = 0
            Me.TabPage1.Text = "Master AppData"
            Me.TabPage1.UseVisualStyleBackColor = True
            '
            'TabPage2
            '
            Me.TabPage2.Controls.Add(Me.ListBox2)
            Me.TabPage2.Controls.Add(Me.btnSaveList2)
            Me.TabPage2.Controls.Add(Me.btnUnlock2)
            Me.TabPage2.Controls.Add(Me.btnLoadList2)
            Me.TabPage2.Controls.Add(Me.btnClearList2)
            Me.TabPage2.Controls.Add(Me.btnLock2)
            Me.TabPage2.Controls.Add(Me.btnCopy2)
            Me.TabPage2.Controls.Add(Me.btnDelItems2)
            Me.TabPage2.Controls.Add(Me.btnDel2)
            Me.TabPage2.Controls.Add(Me.btnAddCustom2)
            Me.TabPage2.Controls.Add(Me.Label2)
            Me.TabPage2.Location = New System.Drawing.Point(4, 22)
            Me.TabPage2.Name = "TabPage2"
            Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPage2.Size = New System.Drawing.Size(591, 662)
            Me.TabPage2.TabIndex = 1
            Me.TabPage2.Text = "Master Sequence"
            Me.TabPage2.UseVisualStyleBackColor = True
            '
            'TabPage3
            '
            Me.TabPage3.Controls.Add(Me.Label3)
            Me.TabPage3.Controls.Add(Me.ListBox3)
            Me.TabPage3.Controls.Add(Me.btnSaveList3)
            Me.TabPage3.Controls.Add(Me.btnUnlock3)
            Me.TabPage3.Controls.Add(Me.btnLoadList3)
            Me.TabPage3.Controls.Add(Me.btnClearList3)
            Me.TabPage3.Controls.Add(Me.btnLock3)
            Me.TabPage3.Controls.Add(Me.btnCopy3)
            Me.TabPage3.Controls.Add(Me.btnDelItems3)
            Me.TabPage3.Controls.Add(Me.btnDel3)
            Me.TabPage3.Controls.Add(Me.btnAddCustom3)
            Me.TabPage3.Location = New System.Drawing.Point(4, 22)
            Me.TabPage3.Name = "TabPage3"
            Me.TabPage3.Size = New System.Drawing.Size(591, 662)
            Me.TabPage3.TabIndex = 2
            Me.TabPage3.Text = "Master Calibration"
            Me.TabPage3.UseVisualStyleBackColor = True
            '
            'ListBox2
            '
            Me.ListBox2.FormattingEnabled = True
            Me.ListBox2.Location = New System.Drawing.Point(7, 116)
            Me.ListBox2.Name = "ListBox2"
            Me.ListBox2.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
            Me.ListBox2.Size = New System.Drawing.Size(575, 537)
            Me.ListBox2.TabIndex = 15
            '
            'btnSaveList2
            '
            Me.btnSaveList2.Location = New System.Drawing.Point(341, 7)
            Me.btnSaveList2.Name = "btnSaveList2"
            Me.btnSaveList2.Size = New System.Drawing.Size(40, 47)
            Me.btnSaveList2.TabIndex = 23
            Me.btnSaveList2.Text = "Save List"
            Me.btnSaveList2.UseVisualStyleBackColor = True
            '
            'btnUnlock2
            '
            Me.btnUnlock2.Location = New System.Drawing.Point(531, 6)
            Me.btnUnlock2.Name = "btnUnlock2"
            Me.btnUnlock2.Size = New System.Drawing.Size(51, 47)
            Me.btnUnlock2.TabIndex = 30
            Me.btnUnlock2.Text = "Unlock"
            Me.btnUnlock2.UseVisualStyleBackColor = True
            '
            'btnLoadList2
            '
            Me.btnLoadList2.Location = New System.Drawing.Point(387, 7)
            Me.btnLoadList2.Name = "btnLoadList2"
            Me.btnLoadList2.Size = New System.Drawing.Size(40, 47)
            Me.btnLoadList2.TabIndex = 24
            Me.btnLoadList2.Text = "Load List"
            Me.btnLoadList2.UseVisualStyleBackColor = True
            '
            'btnClearList2
            '
            Me.btnClearList2.Location = New System.Drawing.Point(295, 7)
            Me.btnClearList2.Name = "btnClearList2"
            Me.btnClearList2.Size = New System.Drawing.Size(40, 47)
            Me.btnClearList2.TabIndex = 22
            Me.btnClearList2.Text = "Clear List"
            Me.btnClearList2.UseVisualStyleBackColor = True
            '
            'btnLock2
            '
            Me.btnLock2.Location = New System.Drawing.Point(485, 6)
            Me.btnLock2.Name = "btnLock2"
            Me.btnLock2.Size = New System.Drawing.Size(40, 47)
            Me.btnLock2.TabIndex = 28
            Me.btnLock2.Text = "Lock"
            Me.btnLock2.UseVisualStyleBackColor = True
            '
            'btnCopy2
            '
            Me.btnCopy2.BackColor = System.Drawing.Color.AntiqueWhite
            Me.btnCopy2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.btnCopy2.ForeColor = System.Drawing.Color.Blue
            Me.btnCopy2.Location = New System.Drawing.Point(72, 60)
            Me.btnCopy2.Name = "btnCopy2"
            Me.btnCopy2.Size = New System.Drawing.Size(59, 47)
            Me.btnCopy2.TabIndex = 25
            Me.btnCopy2.Text = "COPY !"
            Me.btnCopy2.UseVisualStyleBackColor = False
            '
            'btnDelItems2
            '
            Me.btnDelItems2.Location = New System.Drawing.Point(249, 7)
            Me.btnDelItems2.Name = "btnDelItems2"
            Me.btnDelItems2.Size = New System.Drawing.Size(40, 47)
            Me.btnDelItems2.TabIndex = 21
            Me.btnDelItems2.Text = "Del Items"
            Me.btnDelItems2.UseVisualStyleBackColor = True
            '
            'btnDel2
            '
            Me.btnDel2.BackColor = System.Drawing.Color.AntiqueWhite
            Me.btnDel2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.btnDel2.ForeColor = System.Drawing.Color.Crimson
            Me.btnDel2.Location = New System.Drawing.Point(7, 60)
            Me.btnDel2.Name = "btnDel2"
            Me.btnDel2.Size = New System.Drawing.Size(59, 47)
            Me.btnDel2.TabIndex = 26
            Me.btnDel2.Text = "DEL !"
            Me.btnDel2.UseVisualStyleBackColor = False
            '
            'btnAddCustom2
            '
            Me.btnAddCustom2.Location = New System.Drawing.Point(191, 7)
            Me.btnAddCustom2.Name = "btnAddCustom2"
            Me.btnAddCustom2.Size = New System.Drawing.Size(52, 47)
            Me.btnAddCustom2.TabIndex = 20
            Me.btnAddCustom2.Text = "Add Custom"
            Me.btnAddCustom2.UseVisualStyleBackColor = True
            '
            'Label2
            '
            Me.Label2.AutoSize = True
            Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label2.Location = New System.Drawing.Point(528, 77)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(0, 13)
            Me.Label2.TabIndex = 27
            '
            'ListBox3
            '
            Me.ListBox3.FormattingEnabled = True
            Me.ListBox3.Location = New System.Drawing.Point(7, 116)
            Me.ListBox3.Name = "ListBox3"
            Me.ListBox3.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended
            Me.ListBox3.Size = New System.Drawing.Size(575, 537)
            Me.ListBox3.TabIndex = 31
            '
            'btnSaveList3
            '
            Me.btnSaveList3.Location = New System.Drawing.Point(341, 7)
            Me.btnSaveList3.Name = "btnSaveList3"
            Me.btnSaveList3.Size = New System.Drawing.Size(40, 47)
            Me.btnSaveList3.TabIndex = 39
            Me.btnSaveList3.Text = "Save List"
            Me.btnSaveList3.UseVisualStyleBackColor = True
            '
            'btnUnlock3
            '
            Me.btnUnlock3.Location = New System.Drawing.Point(531, 6)
            Me.btnUnlock3.Name = "btnUnlock3"
            Me.btnUnlock3.Size = New System.Drawing.Size(51, 47)
            Me.btnUnlock3.TabIndex = 45
            Me.btnUnlock3.Text = "Unlock"
            Me.btnUnlock3.UseVisualStyleBackColor = True
            '
            'btnLoadList3
            '
            Me.btnLoadList3.Location = New System.Drawing.Point(387, 7)
            Me.btnLoadList3.Name = "btnLoadList3"
            Me.btnLoadList3.Size = New System.Drawing.Size(40, 47)
            Me.btnLoadList3.TabIndex = 40
            Me.btnLoadList3.Text = "Load List"
            Me.btnLoadList3.UseVisualStyleBackColor = True
            '
            'btnClearList3
            '
            Me.btnClearList3.Location = New System.Drawing.Point(295, 7)
            Me.btnClearList3.Name = "btnClearList3"
            Me.btnClearList3.Size = New System.Drawing.Size(40, 47)
            Me.btnClearList3.TabIndex = 38
            Me.btnClearList3.Text = "Clear List"
            Me.btnClearList3.UseVisualStyleBackColor = True
            '
            'btnLock3
            '
            Me.btnLock3.Location = New System.Drawing.Point(485, 6)
            Me.btnLock3.Name = "btnLock3"
            Me.btnLock3.Size = New System.Drawing.Size(40, 47)
            Me.btnLock3.TabIndex = 43
            Me.btnLock3.Text = "Lock"
            Me.btnLock3.UseVisualStyleBackColor = True
            '
            'btnCopy3
            '
            Me.btnCopy3.BackColor = System.Drawing.Color.AntiqueWhite
            Me.btnCopy3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.btnCopy3.ForeColor = System.Drawing.Color.Blue
            Me.btnCopy3.Location = New System.Drawing.Point(72, 60)
            Me.btnCopy3.Name = "btnCopy3"
            Me.btnCopy3.Size = New System.Drawing.Size(59, 47)
            Me.btnCopy3.TabIndex = 41
            Me.btnCopy3.Text = "COPY !"
            Me.btnCopy3.UseVisualStyleBackColor = False
            '
            'btnDelItems3
            '
            Me.btnDelItems3.Location = New System.Drawing.Point(249, 7)
            Me.btnDelItems3.Name = "btnDelItems3"
            Me.btnDelItems3.Size = New System.Drawing.Size(40, 47)
            Me.btnDelItems3.TabIndex = 37
            Me.btnDelItems3.Text = "Del Items"
            Me.btnDelItems3.UseVisualStyleBackColor = True
            '
            'btnDel3
            '
            Me.btnDel3.BackColor = System.Drawing.Color.AntiqueWhite
            Me.btnDel3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.btnDel3.ForeColor = System.Drawing.Color.Crimson
            Me.btnDel3.Location = New System.Drawing.Point(7, 60)
            Me.btnDel3.Name = "btnDel3"
            Me.btnDel3.Size = New System.Drawing.Size(59, 47)
            Me.btnDel3.TabIndex = 42
            Me.btnDel3.Text = "DEL !"
            Me.btnDel3.UseVisualStyleBackColor = False
            '
            'btnAddCustom3
            '
            Me.btnAddCustom3.Location = New System.Drawing.Point(191, 7)
            Me.btnAddCustom3.Name = "btnAddCustom3"
            Me.btnAddCustom3.Size = New System.Drawing.Size(52, 47)
            Me.btnAddCustom3.TabIndex = 36
            Me.btnAddCustom3.Text = "Add Custom"
            Me.btnAddCustom3.UseVisualStyleBackColor = True
            '
            'Label3
            '
            Me.Label3.AutoSize = True
            Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.Label3.Location = New System.Drawing.Point(528, 77)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New System.Drawing.Size(0, 13)
            Me.Label3.TabIndex = 46
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(622, 724)
            Me.Controls.Add(Me.TabControl1)
            Me.Controls.Add(Me.menuStrip1)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
            Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
            Me.MainMenuStrip = Me.menuStrip1
            Me.MaximizeBox = False
            Me.Name = "MainForm"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "CopyMaster"
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
        Friend WithEvents btnAddRFD As Windows.Forms.Button
        Friend WithEvents btnLockSameSN As Windows.Forms.Button
        Friend WithEvents btnUnlock As Windows.Forms.Button
        Friend WithEvents btnLock As Windows.Forms.Button
        Friend WithEvents btnDel As Windows.Forms.Button
        Friend WithEvents TabControl1 As Windows.Forms.TabControl
        Friend WithEvents TabPage1 As Windows.Forms.TabPage
        Friend WithEvents TabPage2 As Windows.Forms.TabPage
        Friend WithEvents ListBox2 As Windows.Forms.ListBox
        Friend WithEvents btnSaveList2 As Windows.Forms.Button
        Friend WithEvents btnUnlock2 As Windows.Forms.Button
        Friend WithEvents btnLoadList2 As Windows.Forms.Button
        Friend WithEvents btnClearList2 As Windows.Forms.Button
        Friend WithEvents btnLock2 As Windows.Forms.Button
        Friend WithEvents btnCopy2 As Windows.Forms.Button
        Friend WithEvents btnDelItems2 As Windows.Forms.Button
        Friend WithEvents btnDel2 As Windows.Forms.Button
        Friend WithEvents btnAddCustom2 As Windows.Forms.Button
        Friend WithEvents Label2 As Windows.Forms.Label
        Friend WithEvents TabPage3 As Windows.Forms.TabPage
        Friend WithEvents ListBox3 As Windows.Forms.ListBox
        Friend WithEvents btnSaveList3 As Windows.Forms.Button
        Friend WithEvents btnUnlock3 As Windows.Forms.Button
        Friend WithEvents btnLoadList3 As Windows.Forms.Button
        Friend WithEvents btnClearList3 As Windows.Forms.Button
        Friend WithEvents btnLock3 As Windows.Forms.Button
        Friend WithEvents btnCopy3 As Windows.Forms.Button
        Friend WithEvents btnDelItems3 As Windows.Forms.Button
        Friend WithEvents btnDel3 As Windows.Forms.Button
        Friend WithEvents btnAddCustom3 As Windows.Forms.Button
        Friend WithEvents Label3 As Windows.Forms.Label
    End Class
End Namespace
