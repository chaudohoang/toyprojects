﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
		Me.Label1 = New System.Windows.Forms.Label()
		Me.txtFile1 = New System.Windows.Forms.TextBox()
		Me.btnBrowse1 = New System.Windows.Forms.Button()
		Me.btnBrowse2 = New System.Windows.Forms.Button()
		Me.txtFile2 = New System.Windows.Forms.TextBox()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.btnCompare = New System.Windows.Forms.Button()
		Me.ListBox1 = New System.Windows.Forms.ListBox()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.btnClearlog = New System.Windows.Forms.Button()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.TabControl1 = New System.Windows.Forms.TabControl()
		Me.TabPage1 = New System.Windows.Forms.TabPage()
		Me.btnUseDefaultMaster = New System.Windows.Forms.Button()
		Me.btnUseLastModified1 = New System.Windows.Forms.Button()
		Me.cbxIgnoreList = New System.Windows.Forms.ComboBox()
		Me.TabPage2 = New System.Windows.Forms.TabPage()
		Me.btnUseLastModified3 = New System.Windows.Forms.Button()
		Me.btnShowSettings = New System.Windows.Forms.Button()
		Me.btnClearlog2 = New System.Windows.Forms.Button()
		Me.btnCheck = New System.Windows.Forms.Button()
		Me.chkCalNone = New System.Windows.Forms.CheckBox()
		Me.cbxSubframe = New System.Windows.Forms.ComboBox()
		Me.Label7 = New System.Windows.Forms.Label()
		Me.ListBox2 = New System.Windows.Forms.ListBox()
		Me.Label6 = New System.Windows.Forms.Label()
		Me.Label5 = New System.Windows.Forms.Label()
		Me.txtFile3 = New System.Windows.Forms.TextBox()
		Me.btnBrowse3 = New System.Windows.Forms.Button()
		Me.TabControl1.SuspendLayout()
		Me.TabPage1.SuspendLayout()
		Me.TabPage2.SuspendLayout()
		Me.SuspendLayout()
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(6, 14)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(65, 13)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Sequence 1"
		'
		'txtFile1
		'
		Me.txtFile1.AllowDrop = True
		Me.txtFile1.Location = New System.Drawing.Point(77, 11)
		Me.txtFile1.Name = "txtFile1"
		Me.txtFile1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtFile1.Size = New System.Drawing.Size(598, 20)
		Me.txtFile1.TabIndex = 1
		'
		'btnBrowse1
		'
		Me.btnBrowse1.Location = New System.Drawing.Point(776, 9)
		Me.btnBrowse1.Name = "btnBrowse1"
		Me.btnBrowse1.Size = New System.Drawing.Size(75, 23)
		Me.btnBrowse1.TabIndex = 2
		Me.btnBrowse1.Text = "Browse"
		Me.btnBrowse1.UseVisualStyleBackColor = True
		'
		'btnBrowse2
		'
		Me.btnBrowse2.Location = New System.Drawing.Point(776, 37)
		Me.btnBrowse2.Name = "btnBrowse2"
		Me.btnBrowse2.Size = New System.Drawing.Size(75, 23)
		Me.btnBrowse2.TabIndex = 5
		Me.btnBrowse2.Text = "Browse"
		Me.btnBrowse2.UseVisualStyleBackColor = True
		'
		'txtFile2
		'
		Me.txtFile2.AllowDrop = True
		Me.txtFile2.Location = New System.Drawing.Point(77, 39)
		Me.txtFile2.Name = "txtFile2"
		Me.txtFile2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtFile2.Size = New System.Drawing.Size(598, 20)
		Me.txtFile2.TabIndex = 4
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(6, 42)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(65, 13)
		Me.Label2.TabIndex = 3
		Me.Label2.Text = "Sequence 2"
		'
		'btnCompare
		'
		Me.btnCompare.Location = New System.Drawing.Point(857, 9)
		Me.btnCompare.Name = "btnCompare"
		Me.btnCompare.Size = New System.Drawing.Size(88, 51)
		Me.btnCompare.TabIndex = 6
		Me.btnCompare.Text = "Compare"
		Me.btnCompare.UseVisualStyleBackColor = True
		'
		'ListBox1
		'
		Me.ListBox1.FormattingEnabled = True
		Me.ListBox1.HorizontalScrollbar = True
		Me.ListBox1.Location = New System.Drawing.Point(77, 97)
		Me.ListBox1.Name = "ListBox1"
		Me.ListBox1.ScrollAlwaysVisible = True
		Me.ListBox1.Size = New System.Drawing.Size(868, 381)
		Me.ListBox1.TabIndex = 7
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(12, 97)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(25, 13)
		Me.Label3.TabIndex = 8
		Me.Label3.Text = "Log"
		'
		'btnClearlog
		'
		Me.btnClearlog.Location = New System.Drawing.Point(6, 135)
		Me.btnClearlog.Name = "btnClearlog"
		Me.btnClearlog.Size = New System.Drawing.Size(65, 79)
		Me.btnClearlog.TabIndex = 9
		Me.btnClearlog.Text = "Clear log"
		Me.btnClearlog.UseVisualStyleBackColor = True
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.Location = New System.Drawing.Point(6, 68)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(56, 13)
		Me.Label4.TabIndex = 3
		Me.Label4.Text = "Ignore List"
		'
		'TabControl1
		'
		Me.TabControl1.Controls.Add(Me.TabPage1)
		Me.TabControl1.Controls.Add(Me.TabPage2)
		Me.TabControl1.Location = New System.Drawing.Point(12, 12)
		Me.TabControl1.Name = "TabControl1"
		Me.TabControl1.SelectedIndex = 0
		Me.TabControl1.Size = New System.Drawing.Size(959, 511)
		Me.TabControl1.TabIndex = 10
		'
		'TabPage1
		'
		Me.TabPage1.Controls.Add(Me.btnUseDefaultMaster)
		Me.TabPage1.Controls.Add(Me.btnUseLastModified1)
		Me.TabPage1.Controls.Add(Me.cbxIgnoreList)
		Me.TabPage1.Controls.Add(Me.Label2)
		Me.TabPage1.Controls.Add(Me.Label1)
		Me.TabPage1.Controls.Add(Me.btnCompare)
		Me.TabPage1.Controls.Add(Me.btnClearlog)
		Me.TabPage1.Controls.Add(Me.txtFile2)
		Me.TabPage1.Controls.Add(Me.txtFile1)
		Me.TabPage1.Controls.Add(Me.ListBox1)
		Me.TabPage1.Controls.Add(Me.btnBrowse2)
		Me.TabPage1.Controls.Add(Me.Label3)
		Me.TabPage1.Controls.Add(Me.btnBrowse1)
		Me.TabPage1.Controls.Add(Me.Label4)
		Me.TabPage1.Location = New System.Drawing.Point(4, 22)
		Me.TabPage1.Name = "TabPage1"
		Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage1.Size = New System.Drawing.Size(951, 485)
		Me.TabPage1.TabIndex = 0
		Me.TabPage1.Text = "Compare Sequence Analyses"
		Me.TabPage1.UseVisualStyleBackColor = True
		'
		'btnUseDefaultMaster
		'
		Me.btnUseDefaultMaster.Location = New System.Drawing.Point(681, 37)
		Me.btnUseDefaultMaster.Name = "btnUseDefaultMaster"
		Me.btnUseDefaultMaster.Size = New System.Drawing.Size(89, 23)
		Me.btnUseDefaultMaster.TabIndex = 12
		Me.btnUseDefaultMaster.Text = "Default Master"
		Me.btnUseDefaultMaster.UseVisualStyleBackColor = True
		'
		'btnUseLastModified1
		'
		Me.btnUseLastModified1.Location = New System.Drawing.Point(681, 9)
		Me.btnUseLastModified1.Name = "btnUseLastModified1"
		Me.btnUseLastModified1.Size = New System.Drawing.Size(89, 23)
		Me.btnUseLastModified1.TabIndex = 11
		Me.btnUseLastModified1.Text = "Last Modified"
		Me.btnUseLastModified1.UseVisualStyleBackColor = True
		'
		'cbxIgnoreList
		'
		Me.cbxIgnoreList.FormattingEnabled = True
		Me.cbxIgnoreList.Items.AddRange(New Object() {"", "Notes,DllOutputPath,DllDefFolder"})
		Me.cbxIgnoreList.Location = New System.Drawing.Point(77, 65)
		Me.cbxIgnoreList.Name = "cbxIgnoreList"
		Me.cbxIgnoreList.Size = New System.Drawing.Size(598, 21)
		Me.cbxIgnoreList.TabIndex = 10
		'
		'TabPage2
		'
		Me.TabPage2.Controls.Add(Me.btnUseLastModified3)
		Me.TabPage2.Controls.Add(Me.btnShowSettings)
		Me.TabPage2.Controls.Add(Me.btnClearlog2)
		Me.TabPage2.Controls.Add(Me.btnCheck)
		Me.TabPage2.Controls.Add(Me.chkCalNone)
		Me.TabPage2.Controls.Add(Me.cbxSubframe)
		Me.TabPage2.Controls.Add(Me.Label7)
		Me.TabPage2.Controls.Add(Me.ListBox2)
		Me.TabPage2.Controls.Add(Me.Label6)
		Me.TabPage2.Controls.Add(Me.Label5)
		Me.TabPage2.Controls.Add(Me.txtFile3)
		Me.TabPage2.Controls.Add(Me.btnBrowse3)
		Me.TabPage2.Location = New System.Drawing.Point(4, 22)
		Me.TabPage2.Name = "TabPage2"
		Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage2.Size = New System.Drawing.Size(951, 485)
		Me.TabPage2.TabIndex = 1
		Me.TabPage2.Text = "Check Sequence Measurement"
		Me.TabPage2.UseVisualStyleBackColor = True
		'
		'btnUseLastModified3
		'
		Me.btnUseLastModified3.Location = New System.Drawing.Point(672, 8)
		Me.btnUseLastModified3.Name = "btnUseLastModified3"
		Me.btnUseLastModified3.Size = New System.Drawing.Size(87, 23)
		Me.btnUseLastModified3.TabIndex = 17
		Me.btnUseLastModified3.Text = "Last Modified"
		Me.btnUseLastModified3.UseVisualStyleBackColor = True
		'
		'btnShowSettings
		'
		Me.btnShowSettings.Location = New System.Drawing.Point(846, 35)
		Me.btnShowSettings.Name = "btnShowSettings"
		Me.btnShowSettings.Size = New System.Drawing.Size(99, 23)
		Me.btnShowSettings.TabIndex = 16
		Me.btnShowSettings.Text = "Show all settings"
		Me.btnShowSettings.UseVisualStyleBackColor = True
		'
		'btnClearlog2
		'
		Me.btnClearlog2.Location = New System.Drawing.Point(6, 135)
		Me.btnClearlog2.Name = "btnClearlog2"
		Me.btnClearlog2.Size = New System.Drawing.Size(65, 79)
		Me.btnClearlog2.TabIndex = 15
		Me.btnClearlog2.Text = "Clear log"
		Me.btnClearlog2.UseVisualStyleBackColor = True
		'
		'btnCheck
		'
		Me.btnCheck.Location = New System.Drawing.Point(846, 8)
		Me.btnCheck.Name = "btnCheck"
		Me.btnCheck.Size = New System.Drawing.Size(99, 23)
		Me.btnCheck.TabIndex = 14
		Me.btnCheck.Text = "Check"
		Me.btnCheck.UseVisualStyleBackColor = True
		'
		'chkCalNone
		'
		Me.chkCalNone.AutoSize = True
		Me.chkCalNone.Location = New System.Drawing.Point(364, 39)
		Me.chkCalNone.Name = "chkCalNone"
		Me.chkCalNone.Size = New System.Drawing.Size(143, 17)
		Me.chkCalNone.TabIndex = 13
		Me.chkCalNone.Text = "Check Calibration NONE"
		Me.chkCalNone.UseVisualStyleBackColor = True
		'
		'cbxSubframe
		'
		Me.cbxSubframe.FormattingEnabled = True
		Me.cbxSubframe.Items.AddRange(New Object() {"", "800,450,2784,5676", "700,250,2984,6076", "1000,1300,4576,1784"})
		Me.cbxSubframe.Location = New System.Drawing.Point(77, 37)
		Me.cbxSubframe.Name = "cbxSubframe"
		Me.cbxSubframe.Size = New System.Drawing.Size(281, 21)
		Me.cbxSubframe.TabIndex = 12
		'
		'Label7
		'
		Me.Label7.AutoSize = True
		Me.Label7.Location = New System.Drawing.Point(6, 40)
		Me.Label7.Name = "Label7"
		Me.Label7.Size = New System.Drawing.Size(52, 13)
		Me.Label7.TabIndex = 11
		Me.Label7.Text = "Subframe"
		'
		'ListBox2
		'
		Me.ListBox2.FormattingEnabled = True
		Me.ListBox2.HorizontalScrollbar = True
		Me.ListBox2.Location = New System.Drawing.Point(77, 97)
		Me.ListBox2.Name = "ListBox2"
		Me.ListBox2.ScrollAlwaysVisible = True
		Me.ListBox2.Size = New System.Drawing.Size(868, 381)
		Me.ListBox2.TabIndex = 9
		'
		'Label6
		'
		Me.Label6.AutoSize = True
		Me.Label6.Location = New System.Drawing.Point(12, 97)
		Me.Label6.Name = "Label6"
		Me.Label6.Size = New System.Drawing.Size(25, 13)
		Me.Label6.TabIndex = 10
		Me.Label6.Text = "Log"
		'
		'Label5
		'
		Me.Label5.AutoSize = True
		Me.Label5.Location = New System.Drawing.Point(6, 13)
		Me.Label5.Name = "Label5"
		Me.Label5.Size = New System.Drawing.Size(56, 13)
		Me.Label5.TabIndex = 3
		Me.Label5.Text = "Sequence"
		'
		'txtFile3
		'
		Me.txtFile3.AllowDrop = True
		Me.txtFile3.Location = New System.Drawing.Point(77, 10)
		Me.txtFile3.Name = "txtFile3"
		Me.txtFile3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtFile3.Size = New System.Drawing.Size(589, 20)
		Me.txtFile3.TabIndex = 4
		'
		'btnBrowse3
		'
		Me.btnBrowse3.Location = New System.Drawing.Point(765, 8)
		Me.btnBrowse3.Name = "btnBrowse3"
		Me.btnBrowse3.Size = New System.Drawing.Size(75, 23)
		Me.btnBrowse3.TabIndex = 5
		Me.btnBrowse3.Text = "Browse"
		Me.btnBrowse3.UseVisualStyleBackColor = True
		'
		'Form1
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(978, 529)
		Me.Controls.Add(Me.TabControl1)
		Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.MaximizeBox = False
		Me.Name = "Form1"
		Me.Text = "Sequence Check"
		Me.TabControl1.ResumeLayout(False)
		Me.TabPage1.ResumeLayout(False)
		Me.TabPage1.PerformLayout()
		Me.TabPage2.ResumeLayout(False)
		Me.TabPage2.PerformLayout()
		Me.ResumeLayout(False)

	End Sub

	Friend WithEvents Label1 As Label
    Friend WithEvents txtFile1 As TextBox
    Friend WithEvents btnBrowse1 As Button
	Friend WithEvents btnBrowse2 As Button
	Friend WithEvents txtFile2 As TextBox
	Friend WithEvents Label2 As Label
	Friend WithEvents btnCompare As Button
	Friend WithEvents ListBox1 As ListBox
	Friend WithEvents Label3 As Label
	Friend WithEvents btnClearlog As Button
	Friend WithEvents Label4 As Label
	Friend WithEvents TabControl1 As TabControl
	Friend WithEvents TabPage1 As TabPage
	Friend WithEvents TabPage2 As TabPage
	Friend WithEvents btnCheck As Button
	Friend WithEvents chkCalNone As CheckBox
	Friend WithEvents cbxSubframe As ComboBox
	Friend WithEvents Label7 As Label
	Friend WithEvents ListBox2 As ListBox
	Friend WithEvents Label6 As Label
	Friend WithEvents Label5 As Label
	Friend WithEvents txtFile3 As TextBox
	Friend WithEvents btnBrowse3 As Button
	Friend WithEvents cbxIgnoreList As ComboBox
	Friend WithEvents btnClearlog2 As Button
	Friend WithEvents btnShowSettings As Button
	Friend WithEvents btnUseDefaultMaster As Button
	Friend WithEvents btnUseLastModified1 As Button
	Friend WithEvents btnUseLastModified3 As Button
End Class