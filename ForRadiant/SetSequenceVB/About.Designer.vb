
Imports System

Namespace SetSequence
	Partial Class About
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
			Dim resources As ComponentModel.ComponentResourceManager = New ComponentModel.ComponentResourceManager(GetType(About))
			label1 = New Windows.Forms.Label()
			pictureBox1 = New Windows.Forms.PictureBox()
			CType(pictureBox1, ComponentModel.ISupportInitialize).BeginInit()
			SuspendLayout()
			' 
			' label1
			' 
			label1.AutoSize = True
			label1.Font = New Drawing.Font("Microsoft Sans Serif", 12.0F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, 0)
			label1.Location = New Drawing.Point(12, 9)
			label1.Name = "label1"
			label1.Size = New Drawing.Size(548, 20)
			label1.TabIndex = 0
			label1.Text = "This software is also private software, but it belongs to you, whoever you are !"
			' 
			' pictureBox1
			' 
			pictureBox1.Image = CType(resources.GetObject("pictureBox1.Image"), Drawing.Image)
			pictureBox1.Location = New Drawing.Point(16, 32)
			pictureBox1.Name = "pictureBox1"
			pictureBox1.Size = New Drawing.Size(1252, 284)
			pictureBox1.TabIndex = 1
			pictureBox1.TabStop = False
			' 
			' About
			' 
			AutoScaleDimensions = New Drawing.SizeF(6.0F, 13.0F)
			AutoScaleMode = Windows.Forms.AutoScaleMode.Font
			ClientSize = New Drawing.Size(1284, 328)
			Controls.Add(pictureBox1)
			Controls.Add(label1)
			Icon = CType(resources.GetObject("$this.Icon"), Drawing.Icon)
			Name = "About"
			Text = "Private Software"
			CType(pictureBox1, ComponentModel.ISupportInitialize).EndInit()
			ResumeLayout(False)
			PerformLayout()

		End Sub

#End Region

		Private label1 As Windows.Forms.Label
		Private pictureBox1 As Windows.Forms.PictureBox
	End Class
End Namespace
