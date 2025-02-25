<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me.DataGridView1 = New System.Windows.Forms.DataGridView()
        Me.SourceFolder = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.DestinationFolder = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ExcludedFolders = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.ExcludedFiles = New System.Windows.Forms.DataGridViewTextBoxColumn()
        Me.TextBoxLog = New System.Windows.Forms.TextBox()
        Me.LabelLogSize = New System.Windows.Forms.Label()
        Me.ButtonClearLog = New System.Windows.Forms.Button()
        Me.ButtonOpenLogFolder = New System.Windows.Forms.Button()
        Me.ButtonMinimizeToTray = New System.Windows.Forms.Button()
        Me.NotifyIcon1 = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.BtnRun = New System.Windows.Forms.Button()
        Me.LblStatus = New System.Windows.Forms.Label()
        Me.btnSaveList = New System.Windows.Forms.Button()
        Me.btnLoadList = New System.Windows.Forms.Button()
        Me.ContextMenuStrip1 = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.ExitToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.ContextMenuStrip1.SuspendLayout()
        Me.SuspendLayout()
        '
        'DataGridView1
        '
        Me.DataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill
        Me.DataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.DataGridView1.Columns.AddRange(New System.Windows.Forms.DataGridViewColumn() {Me.SourceFolder, Me.DestinationFolder, Me.ExcludedFolders, Me.ExcludedFiles})
        Me.DataGridView1.Location = New System.Drawing.Point(12, 12)
        Me.DataGridView1.Name = "DataGridView1"
        Me.DataGridView1.Size = New System.Drawing.Size(898, 195)
        Me.DataGridView1.TabIndex = 0
        '
        'SourceFolder
        '
        Me.SourceFolder.HeaderText = "Source Folder"
        Me.SourceFolder.Name = "SourceFolder"
        '
        'DestinationFolder
        '
        Me.DestinationFolder.HeaderText = "Destination Folder"
        Me.DestinationFolder.Name = "DestinationFolder"
        '
        'ExcludedFolders
        '
        Me.ExcludedFolders.HeaderText = "Excluded Folders"
        Me.ExcludedFolders.Name = "ExcludedFolders"
        '
        'ExcludedFiles
        '
        Me.ExcludedFiles.HeaderText = "Excluded Files"
        Me.ExcludedFiles.Name = "ExcludedFiles"
        '
        'TextBoxLog
        '
        Me.TextBoxLog.Location = New System.Drawing.Point(12, 221)
        Me.TextBoxLog.Multiline = True
        Me.TextBoxLog.Name = "TextBoxLog"
        Me.TextBoxLog.ReadOnly = True
        Me.TextBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.TextBoxLog.Size = New System.Drawing.Size(898, 264)
        Me.TextBoxLog.TabIndex = 1
        Me.TextBoxLog.WordWrap = False
        '
        'LabelLogSize
        '
        Me.LabelLogSize.AutoSize = True
        Me.LabelLogSize.Location = New System.Drawing.Point(916, 194)
        Me.LabelLogSize.Name = "LabelLogSize"
        Me.LabelLogSize.Size = New System.Drawing.Size(104, 13)
        Me.LabelLogSize.TabIndex = 2
        Me.LabelLogSize.Text = "Total Log Size: 0 KB"
        '
        'ButtonClearLog
        '
        Me.ButtonClearLog.Location = New System.Drawing.Point(921, 70)
        Me.ButtonClearLog.Name = "ButtonClearLog"
        Me.ButtonClearLog.Size = New System.Drawing.Size(99, 23)
        Me.ButtonClearLog.TabIndex = 3
        Me.ButtonClearLog.Text = "Clear Logs"
        Me.ButtonClearLog.UseVisualStyleBackColor = True
        '
        'ButtonOpenLogFolder
        '
        Me.ButtonOpenLogFolder.Location = New System.Drawing.Point(921, 99)
        Me.ButtonOpenLogFolder.Name = "ButtonOpenLogFolder"
        Me.ButtonOpenLogFolder.Size = New System.Drawing.Size(99, 23)
        Me.ButtonOpenLogFolder.TabIndex = 4
        Me.ButtonOpenLogFolder.Text = "Open Log Folder"
        Me.ButtonOpenLogFolder.UseVisualStyleBackColor = True
        '
        'ButtonMinimizeToTray
        '
        Me.ButtonMinimizeToTray.Location = New System.Drawing.Point(921, 41)
        Me.ButtonMinimizeToTray.Name = "ButtonMinimizeToTray"
        Me.ButtonMinimizeToTray.Size = New System.Drawing.Size(99, 23)
        Me.ButtonMinimizeToTray.TabIndex = 5
        Me.ButtonMinimizeToTray.Text = "Minimize to Tray"
        Me.ButtonMinimizeToTray.UseVisualStyleBackColor = True
        '
        'NotifyIcon1
        '
        Me.NotifyIcon1.ContextMenuStrip = Me.ContextMenuStrip1
        Me.NotifyIcon1.Icon = CType(resources.GetObject("NotifyIcon1.Icon"), System.Drawing.Icon)
        Me.NotifyIcon1.Text = "AutoCopyData"
        Me.NotifyIcon1.Visible = True
        '
        'BtnRun
        '
        Me.BtnRun.Location = New System.Drawing.Point(921, 12)
        Me.BtnRun.Name = "BtnRun"
        Me.BtnRun.Size = New System.Drawing.Size(99, 23)
        Me.BtnRun.TabIndex = 6
        Me.BtnRun.Text = "Run"
        Me.BtnRun.UseVisualStyleBackColor = True
        '
        'LblStatus
        '
        Me.LblStatus.AutoSize = True
        Me.LblStatus.Location = New System.Drawing.Point(937, 221)
        Me.LblStatus.Name = "LblStatus"
        Me.LblStatus.Size = New System.Drawing.Size(83, 13)
        Me.LblStatus.TabIndex = 7
        Me.LblStatus.Text = "Status: Stopped"
        '
        'btnSaveList
        '
        Me.btnSaveList.Location = New System.Drawing.Point(921, 128)
        Me.btnSaveList.Name = "btnSaveList"
        Me.btnSaveList.Size = New System.Drawing.Size(99, 23)
        Me.btnSaveList.TabIndex = 4
        Me.btnSaveList.Text = "Save Settings"
        Me.btnSaveList.UseVisualStyleBackColor = True
        '
        'btnLoadList
        '
        Me.btnLoadList.Location = New System.Drawing.Point(921, 157)
        Me.btnLoadList.Name = "btnLoadList"
        Me.btnLoadList.Size = New System.Drawing.Size(99, 23)
        Me.btnLoadList.TabIndex = 4
        Me.btnLoadList.Text = "Load Settings"
        Me.btnLoadList.UseVisualStyleBackColor = True
        '
        'ContextMenuStrip1
        '
        Me.ContextMenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ExitToolStripMenuItem})
        Me.ContextMenuStrip1.Name = "ContextMenuStrip1"
        Me.ContextMenuStrip1.Size = New System.Drawing.Size(93, 26)
        '
        'ExitToolStripMenuItem
        '
        Me.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem"
        Me.ExitToolStripMenuItem.Size = New System.Drawing.Size(92, 22)
        Me.ExitToolStripMenuItem.Text = "Exit"
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1032, 497)
        Me.Controls.Add(Me.LblStatus)
        Me.Controls.Add(Me.BtnRun)
        Me.Controls.Add(Me.ButtonMinimizeToTray)
        Me.Controls.Add(Me.btnLoadList)
        Me.Controls.Add(Me.btnSaveList)
        Me.Controls.Add(Me.ButtonOpenLogFolder)
        Me.Controls.Add(Me.ButtonClearLog)
        Me.Controls.Add(Me.LabelLogSize)
        Me.Controls.Add(Me.TextBoxLog)
        Me.Controls.Add(Me.DataGridView1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "MainForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "AutoCopyData"
        CType(Me.DataGridView1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ContextMenuStrip1.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents DataGridView1 As DataGridView
    Friend WithEvents SourceFolder As DataGridViewTextBoxColumn
    Friend WithEvents DestinationFolder As DataGridViewTextBoxColumn
    Friend WithEvents ExcludedFolders As DataGridViewTextBoxColumn
    Friend WithEvents ExcludedFiles As DataGridViewTextBoxColumn
    Friend WithEvents TextBoxLog As TextBox
    Friend WithEvents LabelLogSize As Label
    Friend WithEvents ButtonClearLog As Button
    Friend WithEvents ButtonOpenLogFolder As Button
    Friend WithEvents ButtonMinimizeToTray As Button
    Friend WithEvents NotifyIcon1 As NotifyIcon
    Friend WithEvents BtnRun As Button
    Friend WithEvents LblStatus As Label
    Friend WithEvents btnSaveList As Button
    Friend WithEvents btnLoadList As Button
    Friend WithEvents ContextMenuStrip1 As ContextMenuStrip
    Friend WithEvents ExitToolStripMenuItem As ToolStripMenuItem
End Class
