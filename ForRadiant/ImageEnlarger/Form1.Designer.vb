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
		Me.RunButton = New System.Windows.Forms.Button()
		Me.inputTextBox = New System.Windows.Forms.TextBox()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.Label5 = New System.Windows.Forms.Label()
		Me.outputTextBox = New System.Windows.Forms.TextBox()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.txtOldHeight = New System.Windows.Forms.TextBox()
		Me.txtOldWidth = New System.Windows.Forms.TextBox()
		Me.txtNewHeight = New System.Windows.Forms.TextBox()
		Me.txtNewWidth = New System.Windows.Forms.TextBox()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.Label6 = New System.Windows.Forms.Label()
		Me.SuspendLayout()
		'
		'RunButton
		'
		Me.RunButton.ForeColor = System.Drawing.SystemColors.MenuHighlight
		Me.RunButton.Location = New System.Drawing.Point(371, 62)
		Me.RunButton.Name = "RunButton"
		Me.RunButton.Size = New System.Drawing.Size(296, 46)
		Me.RunButton.TabIndex = 4
		Me.RunButton.Text = "Run"
		Me.RunButton.UseVisualStyleBackColor = True
		'
		'inputTextBox
		'
		Me.inputTextBox.AllowDrop = True
		Me.inputTextBox.Location = New System.Drawing.Point(89, 8)
		Me.inputTextBox.Name = "inputTextBox"
		Me.inputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.inputTextBox.Size = New System.Drawing.Size(578, 20)
		Me.inputTextBox.TabIndex = 5
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(12, 11)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(55, 13)
		Me.Label1.TabIndex = 6
		Me.Label1.Text = "Input Files"
		'
		'Label5
		'
		Me.Label5.AutoSize = True
		Me.Label5.Location = New System.Drawing.Point(12, 37)
		Me.Label5.Name = "Label5"
		Me.Label5.Size = New System.Drawing.Size(71, 13)
		Me.Label5.TabIndex = 18
		Me.Label5.Text = "Output Folder"
		'
		'outputTextBox
		'
		Me.outputTextBox.Location = New System.Drawing.Point(89, 34)
		Me.outputTextBox.Name = "outputTextBox"
		Me.outputTextBox.Size = New System.Drawing.Size(578, 20)
		Me.outputTextBox.TabIndex = 17
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(12, 91)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(57, 13)
		Me.Label2.TabIndex = 20
		Me.Label2.Text = "Old Height"
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(12, 65)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(54, 13)
		Me.Label3.TabIndex = 19
		Me.Label3.Text = "Old Width"
		'
		'txtOldHeight
		'
		Me.txtOldHeight.Location = New System.Drawing.Point(89, 88)
		Me.txtOldHeight.Name = "txtOldHeight"
		Me.txtOldHeight.Size = New System.Drawing.Size(68, 20)
		Me.txtOldHeight.TabIndex = 22
		'
		'txtOldWidth
		'
		Me.txtOldWidth.AllowDrop = True
		Me.txtOldWidth.Location = New System.Drawing.Point(89, 62)
		Me.txtOldWidth.Name = "txtOldWidth"
		Me.txtOldWidth.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtOldWidth.Size = New System.Drawing.Size(68, 20)
		Me.txtOldWidth.TabIndex = 21
		'
		'txtNewHeight
		'
		Me.txtNewHeight.Location = New System.Drawing.Point(268, 88)
		Me.txtNewHeight.Name = "txtNewHeight"
		Me.txtNewHeight.Size = New System.Drawing.Size(68, 20)
		Me.txtNewHeight.TabIndex = 26
		Me.txtNewHeight.Text = "2532"
		'
		'txtNewWidth
		'
		Me.txtNewWidth.AllowDrop = True
		Me.txtNewWidth.Location = New System.Drawing.Point(268, 62)
		Me.txtNewWidth.Name = "txtNewWidth"
		Me.txtNewWidth.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
		Me.txtNewWidth.Size = New System.Drawing.Size(68, 20)
		Me.txtNewWidth.TabIndex = 25
		Me.txtNewWidth.Text = "1170"
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.Location = New System.Drawing.Point(185, 91)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(63, 13)
		Me.Label4.TabIndex = 24
		Me.Label4.Text = "New Height"
		'
		'Label6
		'
		Me.Label6.AutoSize = True
		Me.Label6.Location = New System.Drawing.Point(185, 65)
		Me.Label6.Name = "Label6"
		Me.Label6.Size = New System.Drawing.Size(60, 13)
		Me.Label6.TabIndex = 23
		Me.Label6.Text = "New Width"
		'
		'Form1
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(679, 192)
		Me.Controls.Add(Me.txtNewHeight)
		Me.Controls.Add(Me.txtNewWidth)
		Me.Controls.Add(Me.Label4)
		Me.Controls.Add(Me.Label6)
		Me.Controls.Add(Me.txtOldHeight)
		Me.Controls.Add(Me.txtOldWidth)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.Label5)
		Me.Controls.Add(Me.outputTextBox)
		Me.Controls.Add(Me.Label1)
		Me.Controls.Add(Me.inputTextBox)
		Me.Controls.Add(Me.RunButton)
		Me.Name = "Form1"
		Me.Text = "Image Enlarger"
		Me.TopMost = True
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub
	Friend WithEvents RunButton As Button
    Friend WithEvents inputTextBox As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents outputTextBox As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents txtOldHeight As TextBox
    Friend WithEvents txtOldWidth As TextBox
	Friend WithEvents txtNewHeight As TextBox
	Friend WithEvents txtNewWidth As TextBox
	Friend WithEvents Label4 As Label
	Friend WithEvents Label6 As Label
End Class
