<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.RunButton = New System.Windows.Forms.Button()
        Me.inputTextBox = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'RunButton
        '
        Me.RunButton.ForeColor = System.Drawing.SystemColors.MenuHighlight
        Me.RunButton.Location = New System.Drawing.Point(12, 84)
        Me.RunButton.Name = "RunButton"
        Me.RunButton.Size = New System.Drawing.Size(598, 29)
        Me.RunButton.TabIndex = 4
        Me.RunButton.Text = "Run"
        Me.RunButton.UseVisualStyleBackColor = True
        '
        'inputTextBox
        '
        Me.inputTextBox.AllowDrop = True
        Me.inputTextBox.Location = New System.Drawing.Point(89, 8)
        Me.inputTextBox.Multiline = True
        Me.inputTextBox.Name = "inputTextBox"
        Me.inputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.inputTextBox.Size = New System.Drawing.Size(521, 66)
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
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(622, 122)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.inputTextBox)
        Me.Controls.Add(Me.RunButton)
        Me.Name = "Form1"
        Me.Text = "CSVAverage"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents RunButton As Button
    Friend WithEvents inputTextBox As TextBox
    Friend WithEvents Label1 As Label
End Class
