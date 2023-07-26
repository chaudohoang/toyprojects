
Imports System

Namespace TrueTestWatcher
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
            Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
            Me.notifyIcon1 = New System.Windows.Forms.NotifyIcon()
            Me.contextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip()
            Me.exit2ToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.menuStrip1 = New System.Windows.Forms.MenuStrip()
            Me.commandToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.exitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.settingsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.minimizedToTrayToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.helpToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.aboutToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.btnExportAnalysesCompareLog = New System.Windows.Forms.Button()
            Me.btnClearlog = New System.Windows.Forms.Button()
            Me.ListBox1 = New System.Windows.Forms.ListBox()
            Me.Label3 = New System.Windows.Forms.Label()
            Me.TabControl1 = New System.Windows.Forms.TabControl()
            Me.TabPage1 = New System.Windows.Forms.TabPage()
            Me.TabPage2 = New System.Windows.Forms.TabPage()
            Me.cbxIgnoreList = New System.Windows.Forms.ComboBox()
            Me.Label4 = New System.Windows.Forms.Label()
            Me.chkCompareAppdataFiles = New System.Windows.Forms.CheckBox()
            Me.chkCompareSequenceCalibrations = New System.Windows.Forms.CheckBox()
            Me.chkCompareSequenceParameters = New System.Windows.Forms.CheckBox()
            Me.ManualCheckToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.StartMinimizedToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
            Me.contextMenuStrip1.SuspendLayout()
            Me.menuStrip1.SuspendLayout()
            Me.TabControl1.SuspendLayout()
            Me.TabPage1.SuspendLayout()
            Me.TabPage2.SuspendLayout()
            Me.SuspendLayout()
            '
            'notifyIcon1
            '
            Me.notifyIcon1.ContextMenuStrip = Me.contextMenuStrip1
            Me.notifyIcon1.Icon = CType(resources.GetObject("notifyIcon1.Icon"), System.Drawing.Icon)
            Me.notifyIcon1.Text = "TrueTestWatcher"
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
            Me.menuStrip1.Size = New System.Drawing.Size(969, 24)
            Me.menuStrip1.TabIndex = 1
            Me.menuStrip1.Text = "menuStrip1"
            '
            'commandToolStripMenuItem
            '
            Me.commandToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ManualCheckToolStripMenuItem, Me.exitToolStripMenuItem})
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
            Me.settingsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.minimizedToTrayToolStripMenuItem, Me.StartMinimizedToolStripMenuItem})
            Me.settingsToolStripMenuItem.Name = "settingsToolStripMenuItem"
            Me.settingsToolStripMenuItem.Size = New System.Drawing.Size(61, 20)
            Me.settingsToolStripMenuItem.Text = "Settings"
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
            'btnExportAnalysesCompareLog
            '
            Me.btnExportAnalysesCompareLog.Location = New System.Drawing.Point(5, 129)
            Me.btnExportAnalysesCompareLog.Name = "btnExportAnalysesCompareLog"
            Me.btnExportAnalysesCompareLog.Size = New System.Drawing.Size(65, 79)
            Me.btnExportAnalysesCompareLog.TabIndex = 17
            Me.btnExportAnalysesCompareLog.Text = "Export log"
            Me.btnExportAnalysesCompareLog.UseVisualStyleBackColor = True
            '
            'btnClearlog
            '
            Me.btnClearlog.Location = New System.Drawing.Point(5, 44)
            Me.btnClearlog.Name = "btnClearlog"
            Me.btnClearlog.Size = New System.Drawing.Size(65, 79)
            Me.btnClearlog.TabIndex = 16
            Me.btnClearlog.Text = "Clear log"
            Me.btnClearlog.UseVisualStyleBackColor = True
            '
            'ListBox1
            '
            Me.ListBox1.FormattingEnabled = True
            Me.ListBox1.HorizontalScrollbar = True
            Me.ListBox1.Location = New System.Drawing.Point(76, 6)
            Me.ListBox1.Name = "ListBox1"
            Me.ListBox1.ScrollAlwaysVisible = True
            Me.ListBox1.Size = New System.Drawing.Size(868, 381)
            Me.ListBox1.TabIndex = 14
            '
            'Label3
            '
            Me.Label3.AutoSize = True
            Me.Label3.Location = New System.Drawing.Point(11, 6)
            Me.Label3.Name = "Label3"
            Me.Label3.Size = New System.Drawing.Size(25, 13)
            Me.Label3.TabIndex = 15
            Me.Label3.Text = "Log"
            '
            'TabControl1
            '
            Me.TabControl1.Controls.Add(Me.TabPage1)
            Me.TabControl1.Controls.Add(Me.TabPage2)
            Me.TabControl1.Location = New System.Drawing.Point(12, 27)
            Me.TabControl1.Name = "TabControl1"
            Me.TabControl1.SelectedIndex = 0
            Me.TabControl1.Size = New System.Drawing.Size(957, 418)
            Me.TabControl1.TabIndex = 20
            '
            'TabPage1
            '
            Me.TabPage1.Controls.Add(Me.ListBox1)
            Me.TabPage1.Controls.Add(Me.Label3)
            Me.TabPage1.Controls.Add(Me.btnClearlog)
            Me.TabPage1.Controls.Add(Me.btnExportAnalysesCompareLog)
            Me.TabPage1.Location = New System.Drawing.Point(4, 22)
            Me.TabPage1.Name = "TabPage1"
            Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPage1.Size = New System.Drawing.Size(949, 392)
            Me.TabPage1.TabIndex = 0
            Me.TabPage1.Text = "Main"
            Me.TabPage1.UseVisualStyleBackColor = True
            '
            'TabPage2
            '
            Me.TabPage2.Controls.Add(Me.cbxIgnoreList)
            Me.TabPage2.Controls.Add(Me.Label4)
            Me.TabPage2.Controls.Add(Me.chkCompareAppdataFiles)
            Me.TabPage2.Controls.Add(Me.chkCompareSequenceCalibrations)
            Me.TabPage2.Controls.Add(Me.chkCompareSequenceParameters)
            Me.TabPage2.Location = New System.Drawing.Point(4, 22)
            Me.TabPage2.Name = "TabPage2"
            Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
            Me.TabPage2.Size = New System.Drawing.Size(949, 392)
            Me.TabPage2.TabIndex = 1
            Me.TabPage2.Text = "Compare Settings"
            Me.TabPage2.UseVisualStyleBackColor = True
            '
            'cbxIgnoreList
            '
            Me.cbxIgnoreList.FormattingEnabled = True
            Me.cbxIgnoreList.Items.AddRange(New Object() {"", "Notes,DllOutputPath,DLLDefFolder"})
            Me.cbxIgnoreList.Location = New System.Drawing.Point(68, 72)
            Me.cbxIgnoreList.Name = "cbxIgnoreList"
            Me.cbxIgnoreList.Size = New System.Drawing.Size(598, 21)
            Me.cbxIgnoreList.TabIndex = 29
            Me.cbxIgnoreList.Text = "Notes,DllOutputPath,DLLDefFolder"
            '
            'Label4
            '
            Me.Label4.AutoSize = True
            Me.Label4.Location = New System.Drawing.Point(6, 75)
            Me.Label4.Name = "Label4"
            Me.Label4.Size = New System.Drawing.Size(56, 13)
            Me.Label4.TabIndex = 28
            Me.Label4.Text = "Ignore List"
            '
            'chkCompareAppdataFiles
            '
            Me.chkCompareAppdataFiles.AutoSize = True
            Me.chkCompareAppdataFiles.Checked = True
            Me.chkCompareAppdataFiles.CheckState = System.Windows.Forms.CheckState.Checked
            Me.chkCompareAppdataFiles.Location = New System.Drawing.Point(6, 52)
            Me.chkCompareAppdataFiles.Name = "chkCompareAppdataFiles"
            Me.chkCompareAppdataFiles.Size = New System.Drawing.Size(135, 17)
            Me.chkCompareAppdataFiles.TabIndex = 27
            Me.chkCompareAppdataFiles.Text = "Compare Appdata Files"
            Me.chkCompareAppdataFiles.UseVisualStyleBackColor = True
            '
            'chkCompareSequenceCalibrations
            '
            Me.chkCompareSequenceCalibrations.AutoSize = True
            Me.chkCompareSequenceCalibrations.Checked = True
            Me.chkCompareSequenceCalibrations.CheckState = System.Windows.Forms.CheckState.Checked
            Me.chkCompareSequenceCalibrations.Location = New System.Drawing.Point(6, 29)
            Me.chkCompareSequenceCalibrations.Name = "chkCompareSequenceCalibrations"
            Me.chkCompareSequenceCalibrations.Size = New System.Drawing.Size(177, 17)
            Me.chkCompareSequenceCalibrations.TabIndex = 26
            Me.chkCompareSequenceCalibrations.Text = "Compare Sequence Calibrations"
            Me.chkCompareSequenceCalibrations.UseVisualStyleBackColor = True
            '
            'chkCompareSequenceParameters
            '
            Me.chkCompareSequenceParameters.AutoSize = True
            Me.chkCompareSequenceParameters.Checked = True
            Me.chkCompareSequenceParameters.CheckState = System.Windows.Forms.CheckState.Checked
            Me.chkCompareSequenceParameters.Location = New System.Drawing.Point(6, 6)
            Me.chkCompareSequenceParameters.Name = "chkCompareSequenceParameters"
            Me.chkCompareSequenceParameters.Size = New System.Drawing.Size(176, 17)
            Me.chkCompareSequenceParameters.TabIndex = 25
            Me.chkCompareSequenceParameters.Text = "Compare Sequence Parameters"
            Me.chkCompareSequenceParameters.UseVisualStyleBackColor = True
            '
            'ManualCheckToolStripMenuItem
            '
            Me.ManualCheckToolStripMenuItem.Name = "ManualCheckToolStripMenuItem"
            Me.ManualCheckToolStripMenuItem.Size = New System.Drawing.Size(180, 22)
            Me.ManualCheckToolStripMenuItem.Text = "Manual Check"
            '
            'StartMinimizedToolStripMenuItem
            '
            Me.StartMinimizedToolStripMenuItem.Checked = True
            Me.StartMinimizedToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked
            Me.StartMinimizedToolStripMenuItem.Name = "StartMinimizedToolStripMenuItem"
            Me.StartMinimizedToolStripMenuItem.Size = New System.Drawing.Size(226, 22)
            Me.StartMinimizedToolStripMenuItem.Text = "Start Minimized"
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(969, 443)
            Me.Controls.Add(Me.TabControl1)
            Me.Controls.Add(Me.menuStrip1)
            Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
            Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
            Me.MainMenuStrip = Me.menuStrip1
            Me.MaximizeBox = False
            Me.Name = "MainForm"
            Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
            Me.Text = "TrueTestWatcher"
            Me.contextMenuStrip1.ResumeLayout(False)
            Me.menuStrip1.ResumeLayout(False)
            Me.menuStrip1.PerformLayout()
            Me.TabControl1.ResumeLayout(False)
            Me.TabPage1.ResumeLayout(False)
            Me.TabPage1.PerformLayout()
            Me.TabPage2.ResumeLayout(False)
            Me.TabPage2.PerformLayout()
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
        Friend WithEvents btnExportAnalysesCompareLog As Windows.Forms.Button
        Friend WithEvents btnClearlog As Windows.Forms.Button
        Friend WithEvents ListBox1 As Windows.Forms.ListBox
        Friend WithEvents Label3 As Windows.Forms.Label
        Friend WithEvents TabControl1 As Windows.Forms.TabControl
        Friend WithEvents TabPage1 As Windows.Forms.TabPage
        Friend WithEvents TabPage2 As Windows.Forms.TabPage
        Friend WithEvents chkCompareAppdataFiles As Windows.Forms.CheckBox
        Friend WithEvents chkCompareSequenceCalibrations As Windows.Forms.CheckBox
        Friend WithEvents chkCompareSequenceParameters As Windows.Forms.CheckBox
        Friend WithEvents cbxIgnoreList As Windows.Forms.ComboBox
        Friend WithEvents Label4 As Windows.Forms.Label
        Friend WithEvents ManualCheckToolStripMenuItem As Windows.Forms.ToolStripMenuItem
        Friend WithEvents StartMinimizedToolStripMenuItem As Windows.Forms.ToolStripMenuItem
    End Class
End Namespace
