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
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtModel = New System.Windows.Forms.TextBox()
        Me.btnRefresh = New System.Windows.Forms.Button()
        Me.lblErrorCode = New System.Windows.Forms.Label()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'BtnClear
        '
        Me.BtnClear.Location = New System.Drawing.Point(15, 36)
        Me.BtnClear.Name = "BtnClear"
        Me.BtnClear.Size = New System.Drawing.Size(102, 23)
        Me.BtnClear.TabIndex = 0
        Me.BtnClear.Text = "Clear"
        Me.BtnClear.UseVisualStyleBackColor = True
        '
        'BtnInit
        '
        Me.BtnInit.Location = New System.Drawing.Point(15, 65)
        Me.BtnInit.Name = "BtnInit"
        Me.BtnInit.Size = New System.Drawing.Size(102, 23)
        Me.BtnInit.TabIndex = 1
        Me.BtnInit.Text = "Init"
        Me.BtnInit.UseVisualStyleBackColor = True
        '
        'BtnMap
        '
        Me.BtnMap.Location = New System.Drawing.Point(15, 94)
        Me.BtnMap.Name = "BtnMap"
        Me.BtnMap.Size = New System.Drawing.Size(102, 23)
        Me.BtnMap.TabIndex = 2
        Me.BtnMap.Text = "Map"
        Me.BtnMap.UseVisualStyleBackColor = True
        '
        'BtnCrop
        '
        Me.BtnCrop.Location = New System.Drawing.Point(15, 123)
        Me.BtnCrop.Name = "BtnCrop"
        Me.BtnCrop.Size = New System.Drawing.Size(102, 23)
        Me.BtnCrop.TabIndex = 3
        Me.BtnCrop.Text = "Crop"
        Me.BtnCrop.UseVisualStyleBackColor = True
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(36, 13)
        Me.Label1.TabIndex = 4
        Me.Label1.Text = "Model"
        '
        'txtModel
        '
        Me.txtModel.Location = New System.Drawing.Point(54, 6)
        Me.txtModel.Name = "txtModel"
        Me.txtModel.Size = New System.Drawing.Size(63, 20)
        Me.txtModel.TabIndex = 5
        '
        'btnRefresh
        '
        Me.btnRefresh.Location = New System.Drawing.Point(123, 4)
        Me.btnRefresh.Name = "btnRefresh"
        Me.btnRefresh.Size = New System.Drawing.Size(75, 23)
        Me.btnRefresh.TabIndex = 6
        Me.btnRefresh.Text = "Refresh"
        Me.btnRefresh.UseVisualStyleBackColor = True
        '
        'lblErrorCode
        '
        Me.lblErrorCode.AutoSize = True
        Me.lblErrorCode.Font = New System.Drawing.Font("Microsoft Sans Serif", 72.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblErrorCode.Location = New System.Drawing.Point(156, 65)
        Me.lblErrorCode.Name = "lblErrorCode"
        Me.lblErrorCode.Size = New System.Drawing.Size(0, 108)
        Me.lblErrorCode.TabIndex = 7
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatus.ForeColor = System.Drawing.Color.Navy
        Me.lblStatus.Location = New System.Drawing.Point(11, 191)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(0, 20)
        Me.lblStatus.TabIndex = 8
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label2.ForeColor = System.Drawing.Color.Red
        Me.Label2.Location = New System.Drawing.Point(153, 36)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(111, 20)
        Me.Label2.TabIndex = 9
        Me.Label2.Text = "Error Code : "
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(392, 267)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.lblErrorCode)
        Me.Controls.Add(Me.btnRefresh)
        Me.Controls.Add(Me.txtModel)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.BtnCrop)
        Me.Controls.Add(Me.BtnMap)
        Me.Controls.Add(Me.BtnInit)
        Me.Controls.Add(Me.BtnClear)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Name = "MainForm"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "LgdcropSimulator_Net"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents BtnClear As Button
    Friend WithEvents BtnInit As Button
    Friend WithEvents BtnMap As Button
    Friend WithEvents BtnCrop As Button
    Friend WithEvents Label1 As Label
    Friend WithEvents txtModel As TextBox
    Friend WithEvents btnRefresh As Button
    Friend WithEvents lblErrorCode As Label
    Friend WithEvents lblStatus As Label
    Friend WithEvents Label2 As Label
End Class
