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
        Me.BtnClear = New System.Windows.Forms.Button()
        Me.BtnInit = New System.Windows.Forms.Button()
        Me.BtnMap = New System.Windows.Forms.Button()
        Me.BtnCrop = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'BtnClear
        '
        Me.BtnClear.Location = New System.Drawing.Point(12, 12)
        Me.BtnClear.Name = "BtnClear"
        Me.BtnClear.Size = New System.Drawing.Size(75, 23)
        Me.BtnClear.TabIndex = 0
        Me.BtnClear.Text = "Clear"
        Me.BtnClear.UseVisualStyleBackColor = True
        '
        'BtnInit
        '
        Me.BtnInit.Location = New System.Drawing.Point(12, 41)
        Me.BtnInit.Name = "BtnInit"
        Me.BtnInit.Size = New System.Drawing.Size(75, 23)
        Me.BtnInit.TabIndex = 1
        Me.BtnInit.Text = "Init"
        Me.BtnInit.UseVisualStyleBackColor = True
        '
        'BtnMap
        '
        Me.BtnMap.Location = New System.Drawing.Point(12, 70)
        Me.BtnMap.Name = "BtnMap"
        Me.BtnMap.Size = New System.Drawing.Size(75, 23)
        Me.BtnMap.TabIndex = 2
        Me.BtnMap.Text = "Map"
        Me.BtnMap.UseVisualStyleBackColor = True
        '
        'BtnCrop
        '
        Me.BtnCrop.Location = New System.Drawing.Point(12, 99)
        Me.BtnCrop.Name = "BtnCrop"
        Me.BtnCrop.Size = New System.Drawing.Size(75, 23)
        Me.BtnCrop.TabIndex = 3
        Me.BtnCrop.Text = "Crop"
        Me.BtnCrop.UseVisualStyleBackColor = True
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.BtnCrop)
        Me.Controls.Add(Me.BtnMap)
        Me.Controls.Add(Me.BtnInit)
        Me.Controls.Add(Me.BtnClear)
        Me.Name = "MainForm"
        Me.Text = "LgdcropSimulator"
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents BtnClear As Button
    Friend WithEvents BtnInit As Button
    Friend WithEvents BtnMap As Button
    Friend WithEvents BtnCrop As Button
End Class
