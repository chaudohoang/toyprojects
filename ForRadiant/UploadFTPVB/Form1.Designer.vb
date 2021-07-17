<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(Form1))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnUpload = New System.Windows.Forms.Button()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.cbxSource = New System.Windows.Forms.ComboBox()
        Me.txtDest = New System.Windows.Forms.ComboBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.cbxLog = New System.Windows.Forms.ListBox()
        Me.btnClearlog = New System.Windows.Forms.Button()
        Me.cbxHost = New System.Windows.Forms.ComboBox()
        Me.cbxUsername = New System.Windows.Forms.ComboBox()
        Me.cbxPassword = New System.Windows.Forms.ComboBox()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(12, 9)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(29, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Host"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(12, 35)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(55, 13)
        Me.Label2.TabIndex = 2
        Me.Label2.Text = "Username"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(12, 61)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(53, 13)
        Me.Label3.TabIndex = 4
        Me.Label3.Text = "Password"
        '
        'btnUpload
        '
        Me.btnUpload.Location = New System.Drawing.Point(312, 6)
        Me.btnUpload.Name = "btnUpload"
        Me.btnUpload.Size = New System.Drawing.Size(74, 73)
        Me.btnUpload.TabIndex = 6
        Me.btnUpload.Text = "Upload"
        Me.btnUpload.UseVisualStyleBackColor = True
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(12, 115)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(60, 13)
        Me.Label4.TabIndex = 8
        Me.Label4.Text = "Destination"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(12, 88)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(41, 13)
        Me.Label5.TabIndex = 7
        Me.Label5.Text = "Source"
        '
        'cbxSource
        '
        Me.cbxSource.FormattingEnabled = True
        Me.cbxSource.Items.AddRange(New Object() {"Current Folder", "C:\POCB\INPUT", "Choose Folder"})
        Me.cbxSource.Location = New System.Drawing.Point(78, 85)
        Me.cbxSource.Name = "cbxSource"
        Me.cbxSource.Size = New System.Drawing.Size(379, 21)
        Me.cbxSource.TabIndex = 9
        Me.cbxSource.Text = "Current Folder"
        '
        'txtDest
        '
        Me.txtDest.FormattingEnabled = True
        Me.txtDest.Items.AddRange(New Object() {"HN_DATA\POCB\TEST\SUBFOLDER\"})
        Me.txtDest.Location = New System.Drawing.Point(78, 112)
        Me.txtDest.Name = "txtDest"
        Me.txtDest.Size = New System.Drawing.Size(379, 21)
        Me.txtDest.TabIndex = 10
        Me.txtDest.Text = "HN_DATA\POCB\TEST\SUBFOLDER\"
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(12, 142)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(25, 13)
        Me.Label6.TabIndex = 11
        Me.Label6.Text = "Log"
        '
        'cbxLog
        '
        Me.cbxLog.FormattingEnabled = True
        Me.cbxLog.HorizontalScrollbar = True
        Me.cbxLog.Location = New System.Drawing.Point(12, 163)
        Me.cbxLog.Name = "cbxLog"
        Me.cbxLog.ScrollAlwaysVisible = True
        Me.cbxLog.Size = New System.Drawing.Size(445, 381)
        Me.cbxLog.TabIndex = 12
        '
        'btnClearlog
        '
        Me.btnClearlog.Location = New System.Drawing.Point(392, 6)
        Me.btnClearlog.Name = "btnClearlog"
        Me.btnClearlog.Size = New System.Drawing.Size(66, 73)
        Me.btnClearlog.TabIndex = 13
        Me.btnClearlog.Text = "Clear log"
        Me.btnClearlog.UseVisualStyleBackColor = True
        '
        'cbxHost
        '
        Me.cbxHost.FormattingEnabled = True
        Me.cbxHost.Items.AddRange(New Object() {"127.0.0.1", "10.119.211.105", "10.119.211.32"})
        Me.cbxHost.Location = New System.Drawing.Point(78, 6)
        Me.cbxHost.Name = "cbxHost"
        Me.cbxHost.Size = New System.Drawing.Size(228, 21)
        Me.cbxHost.TabIndex = 14
        Me.cbxHost.Text = "127.0.0.1"
        '
        'cbxUsername
        '
        Me.cbxUsername.FormattingEnabled = True
        Me.cbxUsername.Items.AddRange(New Object() {"admin", "edc_adm01", "eqp_ftp01"})
        Me.cbxUsername.Location = New System.Drawing.Point(78, 32)
        Me.cbxUsername.Name = "cbxUsername"
        Me.cbxUsername.Size = New System.Drawing.Size(228, 21)
        Me.cbxUsername.TabIndex = 15
        Me.cbxUsername.Text = "admin"
        '
        'cbxPassword
        '
        Me.cbxPassword.FormattingEnabled = True
        Me.cbxPassword.Items.AddRange(New Object() {"admin", "!lgdedc00", "1q2w3e4!"})
        Me.cbxPassword.Location = New System.Drawing.Point(78, 58)
        Me.cbxPassword.Name = "cbxPassword"
        Me.cbxPassword.Size = New System.Drawing.Size(228, 21)
        Me.cbxPassword.TabIndex = 16
        Me.cbxPassword.Text = "admin"
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(470, 560)
        Me.Controls.Add(Me.cbxPassword)
        Me.Controls.Add(Me.cbxUsername)
        Me.Controls.Add(Me.cbxHost)
        Me.Controls.Add(Me.btnClearlog)
        Me.Controls.Add(Me.cbxLog)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txtDest)
        Me.Controls.Add(Me.cbxSource)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.btnUpload)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "Form1"
        Me.Text = "UploadFTP"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents Label1 As Label
    Friend WithEvents Label2 As Label
    Friend WithEvents Label3 As Label
    Friend WithEvents btnUpload As Button
    Friend WithEvents Label4 As Label
    Friend WithEvents Label5 As Label
    Friend WithEvents cbxSource As ComboBox
    Friend WithEvents txtDest As ComboBox
    Friend WithEvents Label6 As Label
    Friend WithEvents cbxLog As ListBox
    Friend WithEvents btnClearlog As Button
    Friend WithEvents cbxHost As ComboBox
    Friend WithEvents cbxUsername As ComboBox
    Friend WithEvents cbxPassword As ComboBox
End Class
