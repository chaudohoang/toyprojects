<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
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
		Me.SuspendLayout()
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(12, 9)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(65, 13)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Sequence 1"
		'
		'txtFile1
		'
		Me.txtFile1.AllowDrop = True
		Me.txtFile1.Location = New System.Drawing.Point(83, 6)
		Me.txtFile1.Name = "txtFile1"
		Me.txtFile1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtFile1.Size = New System.Drawing.Size(598, 20)
		Me.txtFile1.TabIndex = 1
		'
		'btnBrowse1
		'
		Me.btnBrowse1.Location = New System.Drawing.Point(687, 4)
		Me.btnBrowse1.Name = "btnBrowse1"
		Me.btnBrowse1.Size = New System.Drawing.Size(75, 23)
		Me.btnBrowse1.TabIndex = 2
		Me.btnBrowse1.Text = "Browse"
		Me.btnBrowse1.UseVisualStyleBackColor = True
		'
		'btnBrowse2
		'
		Me.btnBrowse2.Location = New System.Drawing.Point(687, 32)
		Me.btnBrowse2.Name = "btnBrowse2"
		Me.btnBrowse2.Size = New System.Drawing.Size(75, 23)
		Me.btnBrowse2.TabIndex = 5
		Me.btnBrowse2.Text = "Browse"
		Me.btnBrowse2.UseVisualStyleBackColor = True
		'
		'txtFile2
		'
		Me.txtFile2.AllowDrop = True
		Me.txtFile2.Location = New System.Drawing.Point(83, 34)
		Me.txtFile2.Name = "txtFile2"
		Me.txtFile2.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtFile2.Size = New System.Drawing.Size(598, 20)
		Me.txtFile2.TabIndex = 4
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(12, 37)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(65, 13)
		Me.Label2.TabIndex = 3
		Me.Label2.Text = "Sequence 2"
		'
		'btnCompare
		'
		Me.btnCompare.Location = New System.Drawing.Point(768, 4)
		Me.btnCompare.Name = "btnCompare"
		Me.btnCompare.Size = New System.Drawing.Size(99, 23)
		Me.btnCompare.TabIndex = 6
		Me.btnCompare.Text = "Compare"
		Me.btnCompare.UseVisualStyleBackColor = True
		'
		'ListBox1
		'
		Me.ListBox1.FormattingEnabled = True
		Me.ListBox1.HorizontalScrollbar = True
		Me.ListBox1.Location = New System.Drawing.Point(83, 61)
		Me.ListBox1.Name = "ListBox1"
		Me.ListBox1.ScrollAlwaysVisible = True
		Me.ListBox1.Size = New System.Drawing.Size(784, 134)
		Me.ListBox1.TabIndex = 7
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(12, 61)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(25, 13)
		Me.Label3.TabIndex = 8
		Me.Label3.Text = "Log"
		'
		'btnClearlog
		'
		Me.btnClearlog.Location = New System.Drawing.Point(768, 32)
		Me.btnClearlog.Name = "btnClearlog"
		Me.btnClearlog.Size = New System.Drawing.Size(99, 23)
		Me.btnClearlog.TabIndex = 9
		Me.btnClearlog.Text = "Clear log"
		Me.btnClearlog.UseVisualStyleBackColor = True
		'
		'Form1
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(885, 203)
		Me.Controls.Add(Me.btnClearlog)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.ListBox1)
		Me.Controls.Add(Me.btnCompare)
		Me.Controls.Add(Me.btnBrowse2)
		Me.Controls.Add(Me.txtFile2)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.btnBrowse1)
		Me.Controls.Add(Me.txtFile1)
		Me.Controls.Add(Me.Label1)
		Me.Name = "Form1"
		Me.Text = "Sequence Compare"
		Me.ResumeLayout(False)
		Me.PerformLayout()

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
End Class
