﻿Imports System

Namespace SetSequenceVB
	Partial Class MainForm
		''' <summary>
		''' Required designer variable.
		''' </summary>
		Private components As ComponentModel.IContainer = Nothing

		''' <summary>
		''' Clean up any resources being used.
		''' </summary>
		''' <paramname="disposing">true if managed resources should be disposed; otherwise, false.</param>
		Protected Overrides Sub Dispose(ByVal disposing As Boolean)
			If disposing AndAlso components IsNot Nothing Then
				components.Dispose()
			End If
			MyBase.Dispose(disposing)
		End Sub

#Region "Windows Form Designer generated code"

		''' <summary>
		''' Required method for Designer support - do not modify
		''' the contents of this method with the code editor.
		''' </summary>
		Private Sub InitializeComponent()
			Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
			Me.cmdBrowseSequence = New System.Windows.Forms.LinkLabel()
			Me.lblAddtionalTarget = New System.Windows.Forms.Label()
			Me.cmdUseLastModifiedSequence = New System.Windows.Forms.LinkLabel()
			Me.cbCameraRotation = New System.Windows.Forms.ComboBox()
			Me.btnApply = New System.Windows.Forms.Button()
			Me.lblSubframe = New System.Windows.Forms.Label()
			Me.lblCameraRotatioin = New System.Windows.Forms.Label()
			Me.cbFocusDistance = New System.Windows.Forms.ComboBox()
			Me.cbSubframe = New System.Windows.Forms.ComboBox()
			Me.lblCalibrationIDs = New System.Windows.Forms.Label()
			Me.lblFocusDistance = New System.Windows.Forms.Label()
			Me.cbCalBox = New System.Windows.Forms.ComboBox()
			Me.label1 = New System.Windows.Forms.Label()
			Me.cbFNumber = New System.Windows.Forms.ComboBox()
			Me.lblFNumber = New System.Windows.Forms.Label()
			Me.lblAbout = New System.Windows.Forms.Label()
			Me.lblSequencePath = New System.Windows.Forms.Label()
			Me.txtAdditionalSequence = New System.Windows.Forms.TextBox()
			Me.btnBrowseAdditional = New System.Windows.Forms.Button()
			Me.lblTargetSequence = New System.Windows.Forms.Label()
			Me.SuspendLayout()
			'
			'cmdBrowseSequence
			'
			Me.cmdBrowseSequence.AutoSize = True
			Me.cmdBrowseSequence.Location = New System.Drawing.Point(155, 9)
			Me.cmdBrowseSequence.Name = "cmdBrowseSequence"
			Me.cmdBrowseSequence.Size = New System.Drawing.Size(92, 13)
			Me.cmdBrowseSequence.TabIndex = 2
			Me.cmdBrowseSequence.TabStop = True
			Me.cmdBrowseSequence.Text = "Browse sequence"
			'
			'lblAddtionalTarget
			'
			Me.lblAddtionalTarget.AutoSize = True
			Me.lblAddtionalTarget.Location = New System.Drawing.Point(12, 55)
			Me.lblAddtionalTarget.Name = "lblAddtionalTarget"
			Me.lblAddtionalTarget.Size = New System.Drawing.Size(148, 13)
			Me.lblAddtionalTarget.TabIndex = 64
			Me.lblAddtionalTarget.Text = "Additional Target Sequence : "
			'
			'cmdUseLastModifiedSequence
			'
			Me.cmdUseLastModifiedSequence.AutoSize = True
			Me.cmdUseLastModifiedSequence.Location = New System.Drawing.Point(12, 9)
			Me.cmdUseLastModifiedSequence.Name = "cmdUseLastModifiedSequence"
			Me.cmdUseLastModifiedSequence.Size = New System.Drawing.Size(137, 13)
			Me.cmdUseLastModifiedSequence.TabIndex = 1
			Me.cmdUseLastModifiedSequence.TabStop = True
			Me.cmdUseLastModifiedSequence.Text = "Use last modified sequence"
			'
			'cbCameraRotation
			'
			Me.cbCameraRotation.FormattingEnabled = True
			Me.cbCameraRotation.Items.AddRange(New Object() {"", "Copy from first step", "None", "Clockwise90", "Rotate180", "Counterclockwise90"})
			Me.cbCameraRotation.Location = New System.Drawing.Point(99, 232)
			Me.cbCameraRotation.Name = "cbCameraRotation"
			Me.cbCameraRotation.Size = New System.Drawing.Size(148, 21)
			Me.cbCameraRotation.TabIndex = 6
			Me.cbCameraRotation.Text = "Copy from first step"
			'
			'btnApply
			'
			Me.btnApply.Location = New System.Drawing.Point(253, 151)
			Me.btnApply.Name = "btnApply"
			Me.btnApply.Size = New System.Drawing.Size(91, 129)
			Me.btnApply.TabIndex = 8
			Me.btnApply.Text = "Apply"
			Me.btnApply.UseVisualStyleBackColor = True
			'
			'lblSubframe
			'
			Me.lblSubframe.AutoSize = True
			Me.lblSubframe.Location = New System.Drawing.Point(12, 154)
			Me.lblSubframe.Name = "lblSubframe"
			Me.lblSubframe.Size = New System.Drawing.Size(52, 13)
			Me.lblSubframe.TabIndex = 59
			Me.lblSubframe.Text = "Subframe"
			'
			'lblCameraRotatioin
			'
			Me.lblCameraRotatioin.AutoSize = True
			Me.lblCameraRotatioin.Location = New System.Drawing.Point(12, 235)
			Me.lblCameraRotatioin.Name = "lblCameraRotatioin"
			Me.lblCameraRotatioin.Size = New System.Drawing.Size(86, 13)
			Me.lblCameraRotatioin.TabIndex = 61
			Me.lblCameraRotatioin.Text = "Camera Rotation"
			'
			'cbFocusDistance
			'
			Me.cbFocusDistance.FormattingEnabled = True
			Me.cbFocusDistance.Items.AddRange(New Object() {"", "Copy from first step"})
			Me.cbFocusDistance.Location = New System.Drawing.Point(99, 178)
			Me.cbFocusDistance.Name = "cbFocusDistance"
			Me.cbFocusDistance.Size = New System.Drawing.Size(148, 21)
			Me.cbFocusDistance.TabIndex = 4
			Me.cbFocusDistance.Text = "Copy from first step"
			'
			'cbSubframe
			'
			Me.cbSubframe.FormattingEnabled = True
			Me.cbSubframe.Items.AddRange(New Object() {"", "Copy from first step", "800,450,2784,5676", "700,250,2984,6076", "1000,1300,4576,1784"})
			Me.cbSubframe.Location = New System.Drawing.Point(99, 151)
			Me.cbSubframe.Name = "cbSubframe"
			Me.cbSubframe.Size = New System.Drawing.Size(148, 21)
			Me.cbSubframe.TabIndex = 3
			Me.cbSubframe.Text = "Copy from first step"
			'
			'lblCalibrationIDs
			'
			Me.lblCalibrationIDs.AutoSize = True
			Me.lblCalibrationIDs.Location = New System.Drawing.Point(12, 262)
			Me.lblCalibrationIDs.Name = "lblCalibrationIDs"
			Me.lblCalibrationIDs.Size = New System.Drawing.Size(75, 13)
			Me.lblCalibrationIDs.TabIndex = 62
			Me.lblCalibrationIDs.Text = "Calibration IDs"
			'
			'lblFocusDistance
			'
			Me.lblFocusDistance.AutoSize = True
			Me.lblFocusDistance.Location = New System.Drawing.Point(12, 181)
			Me.lblFocusDistance.Name = "lblFocusDistance"
			Me.lblFocusDistance.Size = New System.Drawing.Size(81, 13)
			Me.lblFocusDistance.TabIndex = 60
			Me.lblFocusDistance.Text = "Focus Distance"
			'
			'cbCalBox
			'
			Me.cbCalBox.FormattingEnabled = True
			Me.cbCalBox.Items.AddRange(New Object() {"", "Copy from first step", "1,1,1,1"})
			Me.cbCalBox.Location = New System.Drawing.Point(99, 259)
			Me.cbCalBox.Name = "cbCalBox"
			Me.cbCalBox.Size = New System.Drawing.Size(148, 21)
			Me.cbCalBox.TabIndex = 7
			Me.cbCalBox.Text = "Copy from first step"
			'
			'label1
			'
			Me.label1.AutoSize = True
			Me.label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 36.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
			Me.label1.Location = New System.Drawing.Point(366, 165)
			Me.label1.Name = "label1"
			Me.label1.Size = New System.Drawing.Size(0, 55)
			Me.label1.TabIndex = 72
			'
			'cbFNumber
			'
			Me.cbFNumber.FormattingEnabled = True
			Me.cbFNumber.Items.AddRange(New Object() {"", "Copy from first step", "8.0"})
			Me.cbFNumber.Location = New System.Drawing.Point(99, 205)
			Me.cbFNumber.Name = "cbFNumber"
			Me.cbFNumber.Size = New System.Drawing.Size(148, 21)
			Me.cbFNumber.TabIndex = 5
			Me.cbFNumber.Text = "Copy from first step"
			'
			'lblFNumber
			'
			Me.lblFNumber.AutoSize = True
			Me.lblFNumber.Location = New System.Drawing.Point(12, 208)
			Me.lblFNumber.Name = "lblFNumber"
			Me.lblFNumber.Size = New System.Drawing.Size(53, 13)
			Me.lblFNumber.TabIndex = 73
			Me.lblFNumber.Text = "F-Number"
			'
			'lblAbout
			'
			Me.lblAbout.AutoSize = True
			Me.lblAbout.Location = New System.Drawing.Point(571, 9)
			Me.lblAbout.Name = "lblAbout"
			Me.lblAbout.Size = New System.Drawing.Size(16, 13)
			Me.lblAbout.TabIndex = 74
			Me.lblAbout.Text = "..."
			'
			'lblSequencePath
			'
			Me.lblSequencePath.AutoSize = True
			Me.lblSequencePath.Location = New System.Drawing.Point(119, 33)
			Me.lblSequencePath.Name = "lblSequencePath"
			Me.lblSequencePath.Size = New System.Drawing.Size(0, 13)
			Me.lblSequencePath.TabIndex = 75
			'
			'txtAdditionalSequence
			'
			Me.txtAdditionalSequence.AllowDrop = True
			Me.txtAdditionalSequence.Location = New System.Drawing.Point(17, 71)
			Me.txtAdditionalSequence.Multiline = True
			Me.txtAdditionalSequence.Name = "txtAdditionalSequence"
			Me.txtAdditionalSequence.ScrollBars = System.Windows.Forms.ScrollBars.Both
			Me.txtAdditionalSequence.Size = New System.Drawing.Size(491, 73)
			Me.txtAdditionalSequence.TabIndex = 76
			'
			'btnBrowseAdditional
			'
			Me.btnBrowseAdditional.Location = New System.Drawing.Point(514, 71)
			Me.btnBrowseAdditional.Name = "btnBrowseAdditional"
			Me.btnBrowseAdditional.Size = New System.Drawing.Size(70, 73)
			Me.btnBrowseAdditional.TabIndex = 77
			Me.btnBrowseAdditional.Text = "Browse"
			Me.btnBrowseAdditional.UseVisualStyleBackColor = True
			'
			'lblTargetSequence
			'
			Me.lblTargetSequence.AutoSize = True
			Me.lblTargetSequence.Location = New System.Drawing.Point(14, 33)
			Me.lblTargetSequence.Name = "lblTargetSequence"
			Me.lblTargetSequence.Size = New System.Drawing.Size(99, 13)
			Me.lblTargetSequence.TabIndex = 75
			Me.lblTargetSequence.Text = "Target Sequence : "
			'
			'MainForm
			'
			Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
			Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
			Me.ClientSize = New System.Drawing.Size(596, 287)
			Me.Controls.Add(Me.btnBrowseAdditional)
			Me.Controls.Add(Me.txtAdditionalSequence)
			Me.Controls.Add(Me.lblTargetSequence)
			Me.Controls.Add(Me.lblSequencePath)
			Me.Controls.Add(Me.lblAbout)
			Me.Controls.Add(Me.cbFNumber)
			Me.Controls.Add(Me.lblFNumber)
			Me.Controls.Add(Me.label1)
			Me.Controls.Add(Me.cmdBrowseSequence)
			Me.Controls.Add(Me.lblAddtionalTarget)
			Me.Controls.Add(Me.cmdUseLastModifiedSequence)
			Me.Controls.Add(Me.cbCameraRotation)
			Me.Controls.Add(Me.btnApply)
			Me.Controls.Add(Me.lblSubframe)
			Me.Controls.Add(Me.lblCameraRotatioin)
			Me.Controls.Add(Me.cbFocusDistance)
			Me.Controls.Add(Me.cbSubframe)
			Me.Controls.Add(Me.lblCalibrationIDs)
			Me.Controls.Add(Me.lblFocusDistance)
			Me.Controls.Add(Me.cbCalBox)
			Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
			Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
			Me.MaximizeBox = False
			Me.Name = "MainForm"
			Me.Text = "SetSequenceVB"
			Me.ResumeLayout(False)
			Me.PerformLayout()

		End Sub

#End Region

		Friend WithEvents cmdBrowseSequence As Windows.Forms.LinkLabel
		Friend WithEvents lblAddtionalTarget As Windows.Forms.Label
		Friend WithEvents cmdUseLastModifiedSequence As Windows.Forms.LinkLabel
		Friend WithEvents cbCameraRotation As Windows.Forms.ComboBox
		Friend WithEvents btnApply As Windows.Forms.Button
		Friend WithEvents lblSubframe As Windows.Forms.Label
		Friend WithEvents lblCameraRotatioin As Windows.Forms.Label
		Friend WithEvents cbFocusDistance As Windows.Forms.ComboBox
		Friend WithEvents cbSubframe As Windows.Forms.ComboBox
		Friend WithEvents lblCalibrationIDs As Windows.Forms.Label
		Friend WithEvents lblFocusDistance As Windows.Forms.Label
		Friend WithEvents cbCalBox As Windows.Forms.ComboBox
		Friend WithEvents label1 As Windows.Forms.Label
		Friend WithEvents cbFNumber As Windows.Forms.ComboBox
		Friend WithEvents lblFNumber As Windows.Forms.Label
		Friend WithEvents lblAbout As Windows.Forms.Label
		Friend WithEvents lblSequencePath As Windows.Forms.Label
		Friend WithEvents txtAdditionalSequence As Windows.Forms.TextBox
		Friend WithEvents btnBrowseAdditional As Windows.Forms.Button
		Friend WithEvents lblTargetSequence As Windows.Forms.Label
	End Class
End Namespace
