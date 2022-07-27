<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class Apps
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
		Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Apps))
		Me.TabPage3 = New System.Windows.Forms.TabPage()
		Me.TabPage2 = New System.Windows.Forms.TabPage()
		Me.TabPage1 = New System.Windows.Forms.TabPage()
		Me.cmdCheckSequence = New System.Windows.Forms.LinkLabel()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.cmdFFCDBGenerate = New System.Windows.Forms.LinkLabel()
		Me.cmdSetSequence = New System.Windows.Forms.LinkLabel()
		Me.cmdButterfly = New System.Windows.Forms.LinkLabel()
		Me.TabControl1 = New System.Windows.Forms.TabControl()
		Me.TabPage1.SuspendLayout()
		Me.TabControl1.SuspendLayout()
		Me.SuspendLayout()
		'
		'TabPage3
		'
		Me.TabPage3.Location = New System.Drawing.Point(4, 22)
		Me.TabPage3.Name = "TabPage3"
		Me.TabPage3.Size = New System.Drawing.Size(511, 265)
		Me.TabPage3.TabIndex = 2
		Me.TabPage3.Text = "TabPage3"
		Me.TabPage3.UseVisualStyleBackColor = True
		'
		'TabPage2
		'
		Me.TabPage2.Location = New System.Drawing.Point(4, 22)
		Me.TabPage2.Name = "TabPage2"
		Me.TabPage2.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage2.Size = New System.Drawing.Size(511, 265)
		Me.TabPage2.TabIndex = 1
		Me.TabPage2.Text = "TabPage2"
		Me.TabPage2.UseVisualStyleBackColor = True
		'
		'TabPage1
		'
		Me.TabPage1.Controls.Add(Me.cmdCheckSequence)
		Me.TabPage1.Controls.Add(Me.Label1)
		Me.TabPage1.Controls.Add(Me.cmdFFCDBGenerate)
		Me.TabPage1.Controls.Add(Me.cmdSetSequence)
		Me.TabPage1.Controls.Add(Me.cmdButterfly)
		Me.TabPage1.Location = New System.Drawing.Point(4, 22)
		Me.TabPage1.Name = "TabPage1"
		Me.TabPage1.Padding = New System.Windows.Forms.Padding(3)
		Me.TabPage1.Size = New System.Drawing.Size(511, 265)
		Me.TabPage1.TabIndex = 0
		Me.TabPage1.Text = "TabPage1"
		Me.TabPage1.UseVisualStyleBackColor = True
		'
		'cmdCheckSequence
		'
		Me.cmdCheckSequence.AutoSize = True
		Me.cmdCheckSequence.Location = New System.Drawing.Point(226, 16)
		Me.cmdCheckSequence.Name = "cmdCheckSequence"
		Me.cmdCheckSequence.Size = New System.Drawing.Size(90, 13)
		Me.cmdCheckSequence.TabIndex = 3
		Me.cmdCheckSequence.TabStop = True
		Me.cmdCheckSequence.Text = "Check Sequence"
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(6, 3)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(33, 13)
		Me.Label1.TabIndex = 2
		Me.Label1.Text = "Tools"
		'
		'cmdFFCDBGenerate
		'
		Me.cmdFFCDBGenerate.AutoSize = True
		Me.cmdFFCDBGenerate.Location = New System.Drawing.Point(135, 16)
		Me.cmdFFCDBGenerate.Name = "cmdFFCDBGenerate"
		Me.cmdFFCDBGenerate.Size = New System.Drawing.Size(85, 13)
		Me.cmdFFCDBGenerate.TabIndex = 1
		Me.cmdFFCDBGenerate.TabStop = True
		Me.cmdFFCDBGenerate.Text = "FFCDBGenerate"
		'
		'cmdSetSequence
		'
		Me.cmdSetSequence.AutoSize = True
		Me.cmdSetSequence.Location = New System.Drawing.Point(6, 16)
		Me.cmdSetSequence.Name = "cmdSetSequence"
		Me.cmdSetSequence.Size = New System.Drawing.Size(72, 13)
		Me.cmdSetSequence.TabIndex = 0
		Me.cmdSetSequence.TabStop = True
		Me.cmdSetSequence.Text = "SetSequence"
		'
		'cmdButterfly
		'
		Me.cmdButterfly.AutoSize = True
		Me.cmdButterfly.Location = New System.Drawing.Point(84, 16)
		Me.cmdButterfly.Name = "cmdButterfly"
		Me.cmdButterfly.Size = New System.Drawing.Size(45, 13)
		Me.cmdButterfly.TabIndex = 1
		Me.cmdButterfly.TabStop = True
		Me.cmdButterfly.Text = "Butterfly"
		'
		'TabControl1
		'
		Me.TabControl1.Controls.Add(Me.TabPage1)
		Me.TabControl1.Controls.Add(Me.TabPage2)
		Me.TabControl1.Controls.Add(Me.TabPage3)
		Me.TabControl1.Location = New System.Drawing.Point(12, 12)
		Me.TabControl1.Name = "TabControl1"
		Me.TabControl1.SelectedIndex = 0
		Me.TabControl1.Size = New System.Drawing.Size(519, 291)
		Me.TabControl1.TabIndex = 2
		'
		'Apps
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(543, 315)
		Me.Controls.Add(Me.TabControl1)
		Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
		Me.Name = "Apps"
		Me.Text = "Apps"
		Me.TabPage1.ResumeLayout(False)
		Me.TabPage1.PerformLayout()
		Me.TabControl1.ResumeLayout(False)
		Me.ResumeLayout(False)

	End Sub

	Friend WithEvents TabPage3 As TabPage
	Friend WithEvents TabPage2 As TabPage
	Friend WithEvents TabPage1 As TabPage
	Friend WithEvents Label1 As Label
	Friend WithEvents cmdFFCDBGenerate As LinkLabel
	Friend WithEvents cmdSetSequence As LinkLabel
	Friend WithEvents cmdButterfly As LinkLabel
	Friend WithEvents TabControl1 As TabControl
	Friend WithEvents cmdCheckSequence As LinkLabel
End Class
