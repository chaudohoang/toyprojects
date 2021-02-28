<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class PanelFFC
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(PanelFFC))
        Me.Label20 = New System.Windows.Forms.Label()
        Me.Label26 = New System.Windows.Forms.Label()
        Me.txtType = New System.Windows.Forms.ComboBox()
        Me.txtCsvPrefix = New System.Windows.Forms.ComboBox()
        Me.Label27 = New System.Windows.Forms.Label()
        Me.txtCSVSubfix = New System.Windows.Forms.ComboBox()
        Me.Label29 = New System.Windows.Forms.Label()
        Me.txtXMLSubfix = New System.Windows.Forms.ComboBox()
        Me.Label30 = New System.Windows.Forms.Label()
        Me.txtXMLPrefix = New System.Windows.Forms.ComboBox()
        Me.Label31 = New System.Windows.Forms.Label()
        Me.txtpanelcount = New System.Windows.Forms.ComboBox()
        Me.Label32 = New System.Windows.Forms.Label()
        Me.btnRun = New System.Windows.Forms.Button()
        Me.Label33 = New System.Windows.Forms.Label()
        Me.txtinputpath = New System.Windows.Forms.ComboBox()
        Me.txtoutputpath = New System.Windows.Forms.ComboBox()
        Me.Label39 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtCameraSN = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'Label20
        '
        Me.Label20.AutoSize = True
        Me.Label20.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label20.Location = New System.Drawing.Point(33, 185)
        Me.Label20.Name = "Label20"
        Me.Label20.Size = New System.Drawing.Size(0, 24)
        Me.Label20.TabIndex = 34
        '
        'Label26
        '
        Me.Label26.AutoSize = True
        Me.Label26.Location = New System.Drawing.Point(14, 70)
        Me.Label26.Name = "Label26"
        Me.Label26.Size = New System.Drawing.Size(31, 13)
        Me.Label26.TabIndex = 38
        Me.Label26.Text = "Type"
        '
        'txtType
        '
        Me.txtType.FormattingEnabled = True
        Me.txtType.Items.AddRange(New Object() {"", "RGB", "GRAY"})
        Me.txtType.Location = New System.Drawing.Point(77, 67)
        Me.txtType.Name = "txtType"
        Me.txtType.Size = New System.Drawing.Size(121, 21)
        Me.txtType.TabIndex = 39
        Me.txtType.Text = "RGB"
        '
        'txtCsvPrefix
        '
        Me.txtCsvPrefix.FormattingEnabled = True
        Me.txtCsvPrefix.Items.AddRange(New Object() {"", "CF", "DJ", "EP"})
        Me.txtCsvPrefix.Location = New System.Drawing.Point(77, 94)
        Me.txtCsvPrefix.Name = "txtCsvPrefix"
        Me.txtCsvPrefix.Size = New System.Drawing.Size(121, 21)
        Me.txtCsvPrefix.TabIndex = 41
        Me.txtCsvPrefix.Text = "CF"
        '
        'Label27
        '
        Me.Label27.AutoSize = True
        Me.Label27.Location = New System.Drawing.Point(14, 97)
        Me.Label27.Name = "Label27"
        Me.Label27.Size = New System.Drawing.Size(57, 13)
        Me.Label27.TabIndex = 40
        Me.Label27.Text = "CSV Prefix"
        '
        'txtCSVSubfix
        '
        Me.txtCSVSubfix.FormattingEnabled = True
        Me.txtCSVSubfix.Items.AddRange(New Object() {"", "R,G,B", "1,2,3,4,5,6"})
        Me.txtCSVSubfix.Location = New System.Drawing.Point(77, 121)
        Me.txtCSVSubfix.Name = "txtCSVSubfix"
        Me.txtCSVSubfix.Size = New System.Drawing.Size(121, 21)
        Me.txtCSVSubfix.TabIndex = 43
        Me.txtCSVSubfix.Text = "R,G,B"
        '
        'Label29
        '
        Me.Label29.AutoSize = True
        Me.Label29.Location = New System.Drawing.Point(14, 124)
        Me.Label29.Name = "Label29"
        Me.Label29.Size = New System.Drawing.Size(60, 13)
        Me.Label29.TabIndex = 42
        Me.Label29.Text = "CSV Subfix"
        '
        'txtXMLSubfix
        '
        Me.txtXMLSubfix.FormattingEnabled = True
        Me.txtXMLSubfix.Items.AddRange(New Object() {"", "Illunis", "1,2,3", "1,2,3,4,5,6"})
        Me.txtXMLSubfix.Location = New System.Drawing.Point(267, 94)
        Me.txtXMLSubfix.Name = "txtXMLSubfix"
        Me.txtXMLSubfix.Size = New System.Drawing.Size(121, 21)
        Me.txtXMLSubfix.TabIndex = 47
        Me.txtXMLSubfix.Text = "1,2,3"
        '
        'Label30
        '
        Me.Label30.AutoSize = True
        Me.Label30.Location = New System.Drawing.Point(204, 97)
        Me.Label30.Name = "Label30"
        Me.Label30.Size = New System.Drawing.Size(61, 13)
        Me.Label30.TabIndex = 46
        Me.Label30.Text = "XML Subfix"
        '
        'txtXMLPrefix
        '
        Me.txtXMLPrefix.FormattingEnabled = True
        Me.txtXMLPrefix.Items.AddRange(New Object() {"", "CF", "DJ", "EP"})
        Me.txtXMLPrefix.Location = New System.Drawing.Point(267, 67)
        Me.txtXMLPrefix.Name = "txtXMLPrefix"
        Me.txtXMLPrefix.Size = New System.Drawing.Size(121, 21)
        Me.txtXMLPrefix.TabIndex = 45
        Me.txtXMLPrefix.Text = "CF"
        '
        'Label31
        '
        Me.Label31.AutoSize = True
        Me.Label31.Location = New System.Drawing.Point(204, 70)
        Me.Label31.Name = "Label31"
        Me.Label31.Size = New System.Drawing.Size(58, 13)
        Me.Label31.TabIndex = 44
        Me.Label31.Text = "XML Prefix"
        '
        'txtpanelcount
        '
        Me.txtpanelcount.FormattingEnabled = True
        Me.txtpanelcount.Items.AddRange(New Object() {"", "1", "2", "3", "4", "5", "6", "7", "8", "9"})
        Me.txtpanelcount.Location = New System.Drawing.Point(267, 121)
        Me.txtpanelcount.Name = "txtpanelcount"
        Me.txtpanelcount.Size = New System.Drawing.Size(121, 21)
        Me.txtpanelcount.TabIndex = 49
        Me.txtpanelcount.Text = "5"
        '
        'Label32
        '
        Me.Label32.AutoSize = True
        Me.Label32.Location = New System.Drawing.Point(204, 124)
        Me.Label32.Name = "Label32"
        Me.Label32.Size = New System.Drawing.Size(65, 13)
        Me.Label32.TabIndex = 48
        Me.Label32.Text = "Panel Count"
        '
        'btnRun
        '
        Me.btnRun.Location = New System.Drawing.Point(207, 148)
        Me.btnRun.Name = "btnRun"
        Me.btnRun.Size = New System.Drawing.Size(181, 21)
        Me.btnRun.TabIndex = 50
        Me.btnRun.Text = "RUN"
        Me.btnRun.UseVisualStyleBackColor = True
        '
        'Label33
        '
        Me.Label33.AutoSize = True
        Me.Label33.Location = New System.Drawing.Point(14, 13)
        Me.Label33.Name = "Label33"
        Me.Label33.Size = New System.Drawing.Size(63, 13)
        Me.Label33.TabIndex = 42
        Me.Label33.Text = "Input Folder"
        '
        'txtinputpath
        '
        Me.txtinputpath.FormattingEnabled = True
        Me.txtinputpath.Location = New System.Drawing.Point(77, 10)
        Me.txtinputpath.Name = "txtinputpath"
        Me.txtinputpath.Size = New System.Drawing.Size(311, 21)
        Me.txtinputpath.TabIndex = 43
        '
        'txtoutputpath
        '
        Me.txtoutputpath.FormattingEnabled = True
        Me.txtoutputpath.Location = New System.Drawing.Point(77, 37)
        Me.txtoutputpath.Name = "txtoutputpath"
        Me.txtoutputpath.Size = New System.Drawing.Size(311, 21)
        Me.txtoutputpath.TabIndex = 43
        '
        'Label39
        '
        Me.Label39.AutoSize = True
        Me.Label39.Location = New System.Drawing.Point(14, 40)
        Me.Label39.Name = "Label39"
        Me.Label39.Size = New System.Drawing.Size(71, 13)
        Me.Label39.TabIndex = 42
        Me.Label39.Text = "Output Folder"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(14, 151)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(61, 13)
        Me.Label1.TabIndex = 42
        Me.Label1.Text = "Camera SN"
        '
        'txtCameraSN
        '
        Me.txtCameraSN.FormattingEnabled = True
        Me.txtCameraSN.Location = New System.Drawing.Point(77, 148)
        Me.txtCameraSN.Name = "txtCameraSN"
        Me.txtCameraSN.Size = New System.Drawing.Size(121, 21)
        Me.txtCameraSN.TabIndex = 43
        Me.txtCameraSN.Text = "123456"
        '
        'PanelFFC
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(402, 219)
        Me.Controls.Add(Me.btnRun)
        Me.Controls.Add(Me.txtpanelcount)
        Me.Controls.Add(Me.Label32)
        Me.Controls.Add(Me.txtXMLSubfix)
        Me.Controls.Add(Me.Label30)
        Me.Controls.Add(Me.txtXMLPrefix)
        Me.Controls.Add(Me.Label31)
        Me.Controls.Add(Me.txtoutputpath)
        Me.Controls.Add(Me.txtinputpath)
        Me.Controls.Add(Me.Label39)
        Me.Controls.Add(Me.Label33)
        Me.Controls.Add(Me.txtCameraSN)
        Me.Controls.Add(Me.txtCSVSubfix)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Label29)
        Me.Controls.Add(Me.txtCsvPrefix)
        Me.Controls.Add(Me.Label27)
        Me.Controls.Add(Me.txtType)
        Me.Controls.Add(Me.Label26)
        Me.Controls.Add(Me.Label20)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "PanelFFC"
        Me.Text = "PanelFFC"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label20 As Label
    Friend WithEvents Label26 As Label
    Friend WithEvents txtType As ComboBox
    Friend WithEvents txtCsvPrefix As ComboBox
    Friend WithEvents Label27 As Label
    Friend WithEvents txtCSVSubfix As ComboBox
    Friend WithEvents Label29 As Label
    Friend WithEvents txtXMLSubfix As ComboBox
    Friend WithEvents Label30 As Label
    Friend WithEvents txtXMLPrefix As ComboBox
    Friend WithEvents Label31 As Label
    Friend WithEvents txtpanelcount As ComboBox
    Friend WithEvents Label32 As Label
    Friend WithEvents btnRun As Button
    Friend WithEvents Label33 As Label
    Friend WithEvents txtinputpath As ComboBox
    Friend WithEvents txtoutputpath As ComboBox
    Friend WithEvents Label39 As Label
    Friend WithEvents Label1 As Label
    Friend WithEvents txtCameraSN As ComboBox
End Class
