<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class InitForm
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
        Me.txtMonth = New System.Windows.Forms.TextBox()
        Me.txtDay = New System.Windows.Forms.TextBox()
        Me.txtCompMode = New System.Windows.Forms.TextBox()
        Me.txtDefaultFolder = New System.Windows.Forms.TextBox()
        Me.txtModel = New System.Windows.Forms.TextBox()
        Me.txtPID = New System.Windows.Forms.TextBox()
        Me.txtINIFile = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.BtnOK = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'txtMonth
        '
        Me.txtMonth.Location = New System.Drawing.Point(95, 12)
        Me.txtMonth.Name = "txtMonth"
        Me.txtMonth.Size = New System.Drawing.Size(607, 20)
        Me.txtMonth.TabIndex = 0
        '
        'txtDay
        '
        Me.txtDay.Location = New System.Drawing.Point(95, 38)
        Me.txtDay.Name = "txtDay"
        Me.txtDay.Size = New System.Drawing.Size(607, 20)
        Me.txtDay.TabIndex = 1
        '
        'txtCompMode
        '
        Me.txtCompMode.Location = New System.Drawing.Point(95, 64)
        Me.txtCompMode.Name = "txtCompMode"
        Me.txtCompMode.Size = New System.Drawing.Size(607, 20)
        Me.txtCompMode.TabIndex = 2
        '
        'txtDefaultFolder
        '
        Me.txtDefaultFolder.Location = New System.Drawing.Point(95, 90)
        Me.txtDefaultFolder.Name = "txtDefaultFolder"
        Me.txtDefaultFolder.Size = New System.Drawing.Size(607, 20)
        Me.txtDefaultFolder.TabIndex = 3
        '
        'txtModel
        '
        Me.txtModel.Location = New System.Drawing.Point(95, 116)
        Me.txtModel.Name = "txtModel"
        Me.txtModel.Size = New System.Drawing.Size(607, 20)
        Me.txtModel.TabIndex = 4
        '
        'txtPID
        '
        Me.txtPID.Location = New System.Drawing.Point(95, 142)
        Me.txtPID.Name = "txtPID"
        Me.txtPID.Size = New System.Drawing.Size(607, 20)
        Me.txtPID.TabIndex = 5
        '
        'txtINIFile
        '
        Me.txtINIFile.Location = New System.Drawing.Point(95, 168)
        Me.txtINIFile.Name = "txtINIFile"
        Me.txtINIFile.Size = New System.Drawing.Size(607, 20)
        Me.txtINIFile.TabIndex = 6
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 15)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(37, 13)
        Me.Label1.TabIndex = 7
        Me.Label1.Text = "Month"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 41)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(26, 13)
        Me.Label2.TabIndex = 8
        Me.Label2.Text = "Day"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 67)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(64, 13)
        Me.Label3.TabIndex = 9
        Me.Label3.Text = "Comp Mode"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 93)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(73, 13)
        Me.Label4.TabIndex = 10
        Me.Label4.Text = "Default Folder"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(12, 119)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(36, 13)
        Me.Label5.TabIndex = 11
        Me.Label5.Text = "Model"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(12, 145)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(25, 13)
        Me.Label6.TabIndex = 12
        Me.Label6.Text = "PID"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(12, 171)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(37, 13)
        Me.Label7.TabIndex = 13
        Me.Label7.Text = "Ini File"
        '
        'BtnOK
        '
        Me.BtnOK.Location = New System.Drawing.Point(716, 12)
        Me.BtnOK.Name = "BtnOK"
        Me.BtnOK.Size = New System.Drawing.Size(72, 176)
        Me.BtnOK.TabIndex = 14
        Me.BtnOK.Text = "OK"
        Me.BtnOK.UseVisualStyleBackColor = True
        '
        'InitForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(800, 201)
        Me.Controls.Add(Me.BtnOK)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtINIFile)
        Me.Controls.Add(Me.txtPID)
        Me.Controls.Add(Me.txtModel)
        Me.Controls.Add(Me.txtDefaultFolder)
        Me.Controls.Add(Me.txtCompMode)
        Me.Controls.Add(Me.txtDay)
        Me.Controls.Add(Me.txtMonth)
        Me.Name = "InitForm"
        Me.Text = "InitForm"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents txtMonth As TextBox
    Friend WithEvents txtDay As TextBox
    Friend WithEvents txtCompMode As TextBox
    Friend WithEvents txtDefaultFolder As TextBox
    Friend WithEvents txtModel As TextBox
    Friend WithEvents txtPID As TextBox
    Friend WithEvents txtINIFile As TextBox
    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents Label6 As Label
    Friend WithEvents Label7 As Label
    Friend WithEvents BtnOK As Button
End Class
