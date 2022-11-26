<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class CalRef
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
		Me.Label1 = New System.Windows.Forms.Label()
		Me.txtColorCalID = New System.Windows.Forms.TextBox()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.txtColorCalDescription = New System.Windows.Forms.TextBox()
		Me.CalReferenceBox = New System.Windows.Forms.ListBox()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.btnAddColorCalRef = New System.Windows.Forms.Button()
		Me.Label3 = New System.Windows.Forms.Label()
		Me.CalRuleBox = New System.Windows.Forms.ListBox()
		Me.btnDelCalRef = New System.Windows.Forms.Button()
		Me.btnDelCalRefAll = New System.Windows.Forms.Button()
		Me.btnCalRefSave = New System.Windows.Forms.Button()
		Me.btnCalRuleSave = New System.Windows.Forms.Button()
		Me.btnDelCalRuleAll = New System.Windows.Forms.Button()
		Me.btnDelCalRule = New System.Windows.Forms.Button()
		Me.btnAddColorCalRule = New System.Windows.Forms.Button()
		Me.txtStepCalID = New System.Windows.Forms.TextBox()
		Me.Label5 = New System.Windows.Forms.Label()
		Me.txtStep = New System.Windows.Forms.TextBox()
		Me.Label6 = New System.Windows.Forms.Label()
		Me.SuspendLayout()
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(12, 9)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(57, 13)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "ColorCalID"
		'
		'txtColorCalID
		'
		Me.txtColorCalID.Location = New System.Drawing.Point(95, 6)
		Me.txtColorCalID.Name = "txtColorCalID"
		Me.txtColorCalID.Size = New System.Drawing.Size(213, 20)
		Me.txtColorCalID.TabIndex = 1
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(12, 35)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(60, 13)
		Me.Label2.TabIndex = 2
		Me.Label2.Text = "Description"
		'
		'txtColorCalDescription
		'
		Me.txtColorCalDescription.Location = New System.Drawing.Point(95, 32)
		Me.txtColorCalDescription.Name = "txtColorCalDescription"
		Me.txtColorCalDescription.Size = New System.Drawing.Size(213, 20)
		Me.txtColorCalDescription.TabIndex = 3
		'
		'CalReferenceBox
		'
		Me.CalReferenceBox.FormattingEnabled = True
		Me.CalReferenceBox.Location = New System.Drawing.Point(401, 22)
		Me.CalReferenceBox.Name = "CalReferenceBox"
		Me.CalReferenceBox.ScrollAlwaysVisible = True
		Me.CalReferenceBox.Size = New System.Drawing.Size(318, 134)
		Me.CalReferenceBox.TabIndex = 6
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.Location = New System.Drawing.Point(401, 6)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(109, 13)
		Me.Label4.TabIndex = 7
		Me.Label4.Text = "Calibration Reference"
		'
		'btnAddColorCalRef
		'
		Me.btnAddColorCalRef.Location = New System.Drawing.Point(314, 6)
		Me.btnAddColorCalRef.Name = "btnAddColorCalRef"
		Me.btnAddColorCalRef.Size = New System.Drawing.Size(81, 48)
		Me.btnAddColorCalRef.TabIndex = 8
		Me.btnAddColorCalRef.Text = "Add ColorCal Ref"
		Me.btnAddColorCalRef.UseVisualStyleBackColor = True
		'
		'Label3
		'
		Me.Label3.AutoSize = True
		Me.Label3.Location = New System.Drawing.Point(398, 159)
		Me.Label3.Name = "Label3"
		Me.Label3.Size = New System.Drawing.Size(81, 13)
		Me.Label3.TabIndex = 9
		Me.Label3.Text = "Calibration Rule"
		'
		'CalRuleBox
		'
		Me.CalRuleBox.FormattingEnabled = True
		Me.CalRuleBox.Location = New System.Drawing.Point(401, 175)
		Me.CalRuleBox.Name = "CalRuleBox"
		Me.CalRuleBox.ScrollAlwaysVisible = True
		Me.CalRuleBox.Size = New System.Drawing.Size(318, 134)
		Me.CalRuleBox.TabIndex = 10
		'
		'btnDelCalRef
		'
		Me.btnDelCalRef.Location = New System.Drawing.Point(725, 22)
		Me.btnDelCalRef.Name = "btnDelCalRef"
		Me.btnDelCalRef.Size = New System.Drawing.Size(63, 40)
		Me.btnDelCalRef.TabIndex = 11
		Me.btnDelCalRef.Text = "Del Selected"
		Me.btnDelCalRef.UseVisualStyleBackColor = True
		'
		'btnDelCalRefAll
		'
		Me.btnDelCalRefAll.Location = New System.Drawing.Point(725, 68)
		Me.btnDelCalRefAll.Name = "btnDelCalRefAll"
		Me.btnDelCalRefAll.Size = New System.Drawing.Size(63, 40)
		Me.btnDelCalRefAll.TabIndex = 12
		Me.btnDelCalRefAll.Text = "Del All"
		Me.btnDelCalRefAll.UseVisualStyleBackColor = True
		'
		'btnCalRefSave
		'
		Me.btnCalRefSave.Location = New System.Drawing.Point(725, 114)
		Me.btnCalRefSave.Name = "btnCalRefSave"
		Me.btnCalRefSave.Size = New System.Drawing.Size(63, 40)
		Me.btnCalRefSave.TabIndex = 13
		Me.btnCalRefSave.Text = "Save"
		Me.btnCalRefSave.UseVisualStyleBackColor = True
		'
		'btnCalRuleSave
		'
		Me.btnCalRuleSave.Location = New System.Drawing.Point(725, 267)
		Me.btnCalRuleSave.Name = "btnCalRuleSave"
		Me.btnCalRuleSave.Size = New System.Drawing.Size(63, 40)
		Me.btnCalRuleSave.TabIndex = 16
		Me.btnCalRuleSave.Text = "Save"
		Me.btnCalRuleSave.UseVisualStyleBackColor = True
		'
		'btnDelCalRuleAll
		'
		Me.btnDelCalRuleAll.Location = New System.Drawing.Point(725, 221)
		Me.btnDelCalRuleAll.Name = "btnDelCalRuleAll"
		Me.btnDelCalRuleAll.Size = New System.Drawing.Size(63, 40)
		Me.btnDelCalRuleAll.TabIndex = 15
		Me.btnDelCalRuleAll.Text = "Del All"
		Me.btnDelCalRuleAll.UseVisualStyleBackColor = True
		'
		'btnDelCalRule
		'
		Me.btnDelCalRule.Location = New System.Drawing.Point(725, 175)
		Me.btnDelCalRule.Name = "btnDelCalRule"
		Me.btnDelCalRule.Size = New System.Drawing.Size(63, 40)
		Me.btnDelCalRule.TabIndex = 14
		Me.btnDelCalRule.Text = "Del Selected"
		Me.btnDelCalRule.UseVisualStyleBackColor = True
		'
		'btnAddColorCalRule
		'
		Me.btnAddColorCalRule.Location = New System.Drawing.Point(314, 175)
		Me.btnAddColorCalRule.Name = "btnAddColorCalRule"
		Me.btnAddColorCalRule.Size = New System.Drawing.Size(81, 48)
		Me.btnAddColorCalRule.TabIndex = 21
		Me.btnAddColorCalRule.Text = "Add Calibration Rule"
		Me.btnAddColorCalRule.UseVisualStyleBackColor = True
		'
		'txtStepCalID
		'
		Me.txtStepCalID.Location = New System.Drawing.Point(95, 201)
		Me.txtStepCalID.Name = "txtStepCalID"
		Me.txtStepCalID.Size = New System.Drawing.Size(213, 20)
		Me.txtStepCalID.TabIndex = 20
		'
		'Label5
		'
		Me.Label5.AutoSize = True
		Me.Label5.Location = New System.Drawing.Point(12, 204)
		Me.Label5.Name = "Label5"
		Me.Label5.Size = New System.Drawing.Size(57, 13)
		Me.Label5.TabIndex = 19
		Me.Label5.Text = "ColorCalID"
		'
		'txtStep
		'
		Me.txtStep.Location = New System.Drawing.Point(95, 175)
		Me.txtStep.Name = "txtStep"
		Me.txtStep.Size = New System.Drawing.Size(213, 20)
		Me.txtStep.TabIndex = 18
		'
		'Label6
		'
		Me.Label6.AutoSize = True
		Me.Label6.Location = New System.Drawing.Point(12, 178)
		Me.Label6.Name = "Label6"
		Me.Label6.Size = New System.Drawing.Size(29, 13)
		Me.Label6.TabIndex = 17
		Me.Label6.Text = "Step"
		'
		'CalRef
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(800, 319)
		Me.Controls.Add(Me.btnAddColorCalRule)
		Me.Controls.Add(Me.txtStepCalID)
		Me.Controls.Add(Me.Label5)
		Me.Controls.Add(Me.txtStep)
		Me.Controls.Add(Me.Label6)
		Me.Controls.Add(Me.btnCalRuleSave)
		Me.Controls.Add(Me.btnDelCalRuleAll)
		Me.Controls.Add(Me.btnDelCalRule)
		Me.Controls.Add(Me.btnCalRefSave)
		Me.Controls.Add(Me.btnDelCalRefAll)
		Me.Controls.Add(Me.btnDelCalRef)
		Me.Controls.Add(Me.CalRuleBox)
		Me.Controls.Add(Me.Label3)
		Me.Controls.Add(Me.btnAddColorCalRef)
		Me.Controls.Add(Me.Label4)
		Me.Controls.Add(Me.CalReferenceBox)
		Me.Controls.Add(Me.txtColorCalDescription)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.txtColorCalID)
		Me.Controls.Add(Me.Label1)
		Me.Name = "CalRef"
		Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
		Me.Text = "CalRef"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents Label1 As Label
	Friend WithEvents txtColorCalID As TextBox
	Friend WithEvents Label2 As Label
	Friend WithEvents txtColorCalDescription As TextBox
	Friend WithEvents CalReferenceBox As ListBox
	Friend WithEvents Label4 As Label
	Friend WithEvents btnAddColorCalRef As Button
	Friend WithEvents Label3 As Label
	Friend WithEvents CalRuleBox As ListBox
	Friend WithEvents btnDelCalRef As Button
	Friend WithEvents btnDelCalRefAll As Button
	Friend WithEvents btnCalRefSave As Button
	Friend WithEvents btnCalRuleSave As Button
	Friend WithEvents btnDelCalRuleAll As Button
	Friend WithEvents btnDelCalRule As Button
	Friend WithEvents btnAddColorCalRule As Button
	Friend WithEvents txtStepCalID As TextBox
	Friend WithEvents Label5 As Label
	Friend WithEvents txtStep As TextBox
	Friend WithEvents Label6 As Label
End Class
