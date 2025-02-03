<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CropForm
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
        Me.BtnOK = New System.Windows.Forms.Button()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtCropImage = New System.Windows.Forms.TextBox()
        Me.txtColorIdx = New System.Windows.Forms.TextBox()
        Me.txtPatternIdx = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.txtResolution = New System.Windows.Forms.TextBox()
        Me.SuspendLayout()
        '
        'BtnOK
        '
        Me.BtnOK.Location = New System.Drawing.Point(716, 12)
        Me.BtnOK.Name = "BtnOK"
        Me.BtnOK.Size = New System.Drawing.Size(72, 72)
        Me.BtnOK.TabIndex = 29
        Me.BtnOK.Text = "OK"
        Me.BtnOK.UseVisualStyleBackColor = True
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 67)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(61, 13)
        Me.Label3.TabIndex = 24
        Me.Label3.Text = "Crop Image"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(13, 15)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(60, 13)
        Me.Label2.TabIndex = 23
        Me.Label2.Text = "Color Index"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 41)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(70, 13)
        Me.Label1.TabIndex = 22
        Me.Label1.Text = "Pattern Index"
        '
        'txtCropImage
        '
        Me.txtCropImage.Location = New System.Drawing.Point(95, 64)
        Me.txtCropImage.Name = "txtCropImage"
        Me.txtCropImage.Size = New System.Drawing.Size(607, 20)
        Me.txtCropImage.TabIndex = 17
        '
        'txtColorIdx
        '
        Me.txtColorIdx.Location = New System.Drawing.Point(95, 12)
        Me.txtColorIdx.Name = "txtColorIdx"
        Me.txtColorIdx.Size = New System.Drawing.Size(607, 20)
        Me.txtColorIdx.TabIndex = 16
        '
        'txtPatternIdx
        '
        Me.txtPatternIdx.Location = New System.Drawing.Point(95, 38)
        Me.txtPatternIdx.Name = "txtPatternIdx"
        Me.txtPatternIdx.Size = New System.Drawing.Size(607, 20)
        Me.txtPatternIdx.TabIndex = 15
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 93)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(57, 13)
        Me.Label4.TabIndex = 31
        Me.Label4.Text = "Resolution"
        '
        'txtResolution
        '
        Me.txtResolution.Location = New System.Drawing.Point(95, 90)
        Me.txtResolution.Name = "txtResolution"
        Me.txtResolution.Size = New System.Drawing.Size(607, 20)
        Me.txtResolution.TabIndex = 30
        '
        'CropForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 121)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtResolution)
        Me.Controls.Add(Me.BtnOK)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtCropImage)
        Me.Controls.Add(Me.txtColorIdx)
        Me.Controls.Add(Me.txtPatternIdx)
        Me.Name = "CropForm"
        Me.Text = "CropForm"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents BtnOK As Button
    Friend WithEvents Label3 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents txtCropImage As TextBox
    Friend WithEvents txtColorIdx As TextBox
    Friend WithEvents txtPatternIdx As TextBox
    Friend WithEvents Label4 As Label
    Friend WithEvents txtResolution As TextBox
End Class
