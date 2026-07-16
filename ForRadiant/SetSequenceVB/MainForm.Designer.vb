Imports System

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
            Me.Label2 = New System.Windows.Forms.Label()
            Me.cbDemosaicAlgorithm = New System.Windows.Forms.ComboBox()
            Me.SuspendLayout()
            '
            'cmdBrowseSequence
            '
            Me.cmdBrowseSequence.AutoSize = True
            Me.cmdBrowseSequence.Location = New System.Drawing.Point(207, 11)
            Me.cmdBrowseSequence.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.cmdBrowseSequence.Name = "cmdBrowseSequence"
            Me.cmdBrowseSequence.Size = New System.Drawing.Size(115, 16)
            Me.cmdBrowseSequence.TabIndex = 2
            Me.cmdBrowseSequence.TabStop = True
            Me.cmdBrowseSequence.Text = "Browse sequence"
            '
            'lblAddtionalTarget
            '
            Me.lblAddtionalTarget.AutoSize = True
            Me.lblAddtionalTarget.Location = New System.Drawing.Point(16, 68)
            Me.lblAddtionalTarget.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.lblAddtionalTarget.Name = "lblAddtionalTarget"
            Me.lblAddtionalTarget.Size = New System.Drawing.Size(184, 16)
            Me.lblAddtionalTarget.TabIndex = 64
            Me.lblAddtionalTarget.Text = "Additional Target Sequence : "
            '
            'cmdUseLastModifiedSequence
            '
            Me.cmdUseLastModifiedSequence.AutoSize = True
            Me.cmdUseLastModifiedSequence.Location = New System.Drawing.Point(16, 11)
            Me.cmdUseLastModifiedSequence.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.cmdUseLastModifiedSequence.Name = "cmdUseLastModifiedSequence"
            Me.cmdUseLastModifiedSequence.Size = New System.Drawing.Size(174, 16)
            Me.cmdUseLastModifiedSequence.TabIndex = 1
            Me.cmdUseLastModifiedSequence.TabStop = True
            Me.cmdUseLastModifiedSequence.Text = "Use last modified sequence"
            '
            'cbCameraRotation
            '
            Me.cbCameraRotation.FormattingEnabled = True
            Me.cbCameraRotation.Items.AddRange(New Object() {"", "Copy from first step", "None", "Clockwise90", "Rotate180", "Counterclockwise90"})
            Me.cbCameraRotation.Location = New System.Drawing.Point(149, 286)
            Me.cbCameraRotation.Margin = New System.Windows.Forms.Padding(4)
            Me.cbCameraRotation.Name = "cbCameraRotation"
            Me.cbCameraRotation.Size = New System.Drawing.Size(196, 24)
            Me.cbCameraRotation.TabIndex = 6
            Me.cbCameraRotation.Text = "Copy from first step"
            '
            'btnApply
            '
            Me.btnApply.Location = New System.Drawing.Point(359, 186)
            Me.btnApply.Margin = New System.Windows.Forms.Padding(4)
            Me.btnApply.Name = "btnApply"
            Me.btnApply.Size = New System.Drawing.Size(121, 192)
            Me.btnApply.TabIndex = 8
            Me.btnApply.Text = "Apply"
            Me.btnApply.UseVisualStyleBackColor = True
            '
            'lblSubframe
            '
            Me.lblSubframe.AutoSize = True
            Me.lblSubframe.Location = New System.Drawing.Point(16, 190)
            Me.lblSubframe.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.lblSubframe.Name = "lblSubframe"
            Me.lblSubframe.Size = New System.Drawing.Size(65, 16)
            Me.lblSubframe.TabIndex = 59
            Me.lblSubframe.Text = "Subframe"
            '
            'lblCameraRotatioin
            '
            Me.lblCameraRotatioin.AutoSize = True
            Me.lblCameraRotatioin.Location = New System.Drawing.Point(16, 289)
            Me.lblCameraRotatioin.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.lblCameraRotatioin.Name = "lblCameraRotatioin"
            Me.lblCameraRotatioin.Size = New System.Drawing.Size(108, 16)
            Me.lblCameraRotatioin.TabIndex = 61
            Me.lblCameraRotatioin.Text = "Camera Rotation"
            '
            'cbFocusDistance
            '
            Me.cbFocusDistance.FormattingEnabled = True
            Me.cbFocusDistance.Items.AddRange(New Object() {"", "Copy from first step"})
            Me.cbFocusDistance.Location = New System.Drawing.Point(149, 219)
            Me.cbFocusDistance.Margin = New System.Windows.Forms.Padding(4)
            Me.cbFocusDistance.Name = "cbFocusDistance"
            Me.cbFocusDistance.Size = New System.Drawing.Size(196, 24)
            Me.cbFocusDistance.TabIndex = 4
            Me.cbFocusDistance.Text = "Copy from first step"
            '
            'cbSubframe
            '
            Me.cbSubframe.FormattingEnabled = True
            Me.cbSubframe.Items.AddRange(New Object() {"", "Copy from first step", "800,450,2784,5676", "700,250,2984,6076", "1000,1300,4576,1784", "2400,1200,5800,11700", "2200,600,6300,12900", "1048,1096,8510,11922"})
            Me.cbSubframe.Location = New System.Drawing.Point(149, 186)
            Me.cbSubframe.Margin = New System.Windows.Forms.Padding(4)
            Me.cbSubframe.Name = "cbSubframe"
            Me.cbSubframe.Size = New System.Drawing.Size(196, 24)
            Me.cbSubframe.TabIndex = 3
            Me.cbSubframe.Text = "Copy from first step"
            '
            'lblCalibrationIDs
            '
            Me.lblCalibrationIDs.AutoSize = True
            Me.lblCalibrationIDs.Location = New System.Drawing.Point(16, 322)
            Me.lblCalibrationIDs.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.lblCalibrationIDs.Name = "lblCalibrationIDs"
            Me.lblCalibrationIDs.Size = New System.Drawing.Size(94, 16)
            Me.lblCalibrationIDs.TabIndex = 62
            Me.lblCalibrationIDs.Text = "Calibration IDs"
            '
            'lblFocusDistance
            '
            Me.lblFocusDistance.AutoSize = True
            Me.lblFocusDistance.Location = New System.Drawing.Point(16, 223)
            Me.lblFocusDistance.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.lblFocusDistance.Name = "lblFocusDistance"
            Me.lblFocusDistance.Size = New System.Drawing.Size(100, 16)
            Me.lblFocusDistance.TabIndex = 60
            Me.lblFocusDistance.Text = "Focus Distance"
            '
            'cbCalBox
            '
            Me.cbCalBox.FormattingEnabled = True
            Me.cbCalBox.Items.AddRange(New Object() {"", "Copy from first step", "1,1,1,1"})
            Me.cbCalBox.Location = New System.Drawing.Point(149, 319)
            Me.cbCalBox.Margin = New System.Windows.Forms.Padding(4)
            Me.cbCalBox.Name = "cbCalBox"
            Me.cbCalBox.Size = New System.Drawing.Size(196, 24)
            Me.cbCalBox.TabIndex = 7
            Me.cbCalBox.Text = "Copy from first step"
            '
            'label1
            '
            Me.label1.AutoSize = True
            Me.label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 36.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
            Me.label1.Location = New System.Drawing.Point(488, 203)
            Me.label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.label1.Name = "label1"
            Me.label1.Size = New System.Drawing.Size(0, 69)
            Me.label1.TabIndex = 72
            '
            'cbFNumber
            '
            Me.cbFNumber.FormattingEnabled = True
            Me.cbFNumber.Items.AddRange(New Object() {"", "Copy from first step", "8.0"})
            Me.cbFNumber.Location = New System.Drawing.Point(149, 252)
            Me.cbFNumber.Margin = New System.Windows.Forms.Padding(4)
            Me.cbFNumber.Name = "cbFNumber"
            Me.cbFNumber.Size = New System.Drawing.Size(196, 24)
            Me.cbFNumber.TabIndex = 5
            Me.cbFNumber.Text = "Copy from first step"
            '
            'lblFNumber
            '
            Me.lblFNumber.AutoSize = True
            Me.lblFNumber.Location = New System.Drawing.Point(16, 256)
            Me.lblFNumber.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.lblFNumber.Name = "lblFNumber"
            Me.lblFNumber.Size = New System.Drawing.Size(67, 16)
            Me.lblFNumber.TabIndex = 73
            Me.lblFNumber.Text = "F-Number"
            '
            'lblAbout
            '
            Me.lblAbout.AutoSize = True
            Me.lblAbout.Location = New System.Drawing.Point(761, 11)
            Me.lblAbout.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.lblAbout.Name = "lblAbout"
            Me.lblAbout.Size = New System.Drawing.Size(16, 16)
            Me.lblAbout.TabIndex = 74
            Me.lblAbout.Text = "..."
            '
            'lblSequencePath
            '
            Me.lblSequencePath.AutoSize = True
            Me.lblSequencePath.Location = New System.Drawing.Point(159, 41)
            Me.lblSequencePath.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.lblSequencePath.Name = "lblSequencePath"
            Me.lblSequencePath.Size = New System.Drawing.Size(0, 16)
            Me.lblSequencePath.TabIndex = 75
            '
            'txtAdditionalSequence
            '
            Me.txtAdditionalSequence.AllowDrop = True
            Me.txtAdditionalSequence.Location = New System.Drawing.Point(23, 87)
            Me.txtAdditionalSequence.Margin = New System.Windows.Forms.Padding(4)
            Me.txtAdditionalSequence.Multiline = True
            Me.txtAdditionalSequence.Name = "txtAdditionalSequence"
            Me.txtAdditionalSequence.ScrollBars = System.Windows.Forms.ScrollBars.Both
            Me.txtAdditionalSequence.Size = New System.Drawing.Size(653, 89)
            Me.txtAdditionalSequence.TabIndex = 76
            '
            'btnBrowseAdditional
            '
            Me.btnBrowseAdditional.Location = New System.Drawing.Point(685, 87)
            Me.btnBrowseAdditional.Margin = New System.Windows.Forms.Padding(4)
            Me.btnBrowseAdditional.Name = "btnBrowseAdditional"
            Me.btnBrowseAdditional.Size = New System.Drawing.Size(93, 90)
            Me.btnBrowseAdditional.TabIndex = 77
            Me.btnBrowseAdditional.Text = "Browse"
            Me.btnBrowseAdditional.UseVisualStyleBackColor = True
            '
            'lblTargetSequence
            '
            Me.lblTargetSequence.AutoSize = True
            Me.lblTargetSequence.Location = New System.Drawing.Point(19, 41)
            Me.lblTargetSequence.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.lblTargetSequence.Name = "lblTargetSequence"
            Me.lblTargetSequence.Size = New System.Drawing.Size(121, 16)
            Me.lblTargetSequence.TabIndex = 75
            Me.lblTargetSequence.Text = "Target Sequence : "
            '
            'Label2
            '
            Me.Label2.AutoSize = True
            Me.Label2.Location = New System.Drawing.Point(16, 356)
            Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
            Me.Label2.Name = "Label2"
            Me.Label2.Size = New System.Drawing.Size(121, 16)
            Me.Label2.TabIndex = 79
            Me.Label2.Text = "Demosaic Algoritm"
            '
            'cbDemosaicAlgorithm
            '
            Me.cbDemosaicAlgorithm.FormattingEnabled = True
            Me.cbDemosaicAlgorithm.Items.AddRange(New Object() {"", "Copy from first step", "Quarter Resolution"})
            Me.cbDemosaicAlgorithm.Location = New System.Drawing.Point(149, 352)
            Me.cbDemosaicAlgorithm.Margin = New System.Windows.Forms.Padding(4)
            Me.cbDemosaicAlgorithm.Name = "cbDemosaicAlgorithm"
            Me.cbDemosaicAlgorithm.Size = New System.Drawing.Size(196, 24)
            Me.cbDemosaicAlgorithm.TabIndex = 78
            Me.cbDemosaicAlgorithm.Text = "Copy from first step"
            '
            'MainForm
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(8.0!, 16.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.ClientSize = New System.Drawing.Size(795, 385)
            Me.Controls.Add(Me.Label2)
            Me.Controls.Add(Me.cbDemosaicAlgorithm)
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
            Me.Margin = New System.Windows.Forms.Padding(4)
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
        Friend WithEvents Label2 As Windows.Forms.Label
        Friend WithEvents cbDemosaicAlgorithm As Windows.Forms.ComboBox
    End Class
End Namespace
