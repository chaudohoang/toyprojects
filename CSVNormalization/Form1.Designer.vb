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
        Me.InputTextBox = New System.Windows.Forms.TextBox()
        Me.OutputTextBox = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.RunButton = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 26)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(61, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Input File(s)"
        '
        'InputTextBox
        '
        Me.InputTextBox.AllowDrop = True
        Me.InputTextBox.Location = New System.Drawing.Point(89, 23)
        Me.InputTextBox.Multiline = True
        Me.InputTextBox.Name = "InputTextBox"
        Me.InputTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.InputTextBox.Size = New System.Drawing.Size(522, 75)
        Me.InputTextBox.TabIndex = 1
        '
        'OutputTextBox
        '
        Me.OutputTextBox.Location = New System.Drawing.Point(89, 104)
        Me.OutputTextBox.Name = "OutputTextBox"
        Me.OutputTextBox.Size = New System.Drawing.Size(522, 20)
        Me.OutputTextBox.TabIndex = 3
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 107)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(71, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Output Folder"
        '
        'RunButton
        '
        Me.RunButton.Location = New System.Drawing.Point(15, 146)
        Me.RunButton.Name = "RunButton"
        Me.RunButton.Size = New System.Drawing.Size(596, 47)
        Me.RunButton.TabIndex = 4
        Me.RunButton.Text = "Run"
        Me.RunButton.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(623, 208)
        Me.Controls.Add(Me.RunButton)
        Me.Controls.Add(Me.OutputTextBox)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.InputTextBox)
        Me.Controls.Add(Me.Label1)
        Me.Name = "Form1"
        Me.Text = "CSV Normalize"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents InputTextBox As TextBox
    Friend WithEvents OutputTextBox As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents RunButton As Button
End Class
