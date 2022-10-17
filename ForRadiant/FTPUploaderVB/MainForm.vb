Imports System
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.IO
Imports WinSCP
Imports System.Collections.Generic
Imports System.Diagnostics
Imports Microsoft.VisualBasic

Namespace FTPUploaderVB
	Partial Public Class MainForm
		Inherits Form
		Public apppath As String
		Public appdir As String
		Public IsUploading As Boolean
		Public uploadProcess As Process
		Public startinfo As System.Diagnostics.ProcessStartInfo
		Public seconds As Integer = 1   ' change 5 into the count of minutes
		Public timer As System.Timers.Timer
		Private allowVisible As Boolean = True

		<DllImport("User32.dll")>
		Private Shared Function GetLastInputInfo(ByRef plii As MainForm.LASTINPUTINFO) As Boolean
		End Function

		Friend Structure LASTINPUTINFO
			Public cbSize As UInteger

			Public dwTime As UInteger
		End Structure

		Public Sub New()
			apppath = Assembly.GetExecutingAssembly().Location
			appdir = Path.GetDirectoryName(apppath)
			InitializeComponent()
		End Sub

		Protected Overrides Sub WndProc(ByRef m As Message)
			If m.Msg = NativeMethods.WM_SHOWME Then
				ShowMe()
			End If
			MyBase.WndProc(m)
		End Sub
		Private Sub ShowMe()
			Show()
			If WindowState = FormWindowState.Minimized Then
				WindowState = FormWindowState.Normal
			End If
			' get our current "TopMost" value (ours will always be false though)
			Dim top = TopMost
			' make our form jump to the top of everything
			TopMost = True
			' set it back to whatever it was
			TopMost = top
		End Sub

		Private Sub SetVersionInfo()
			Dim versionInfo As Version = Assembly.GetExecutingAssembly().GetName().Version
			Dim startDate As Date = New DateTime(2000, 1, 1)
			Dim diffDays = versionInfo.Build
			Dim computedDate = startDate.AddDays(diffDays)
			Dim lastBuilt As String = computedDate.ToShortDateString()
			'this.Text = string.Format("{0} - {1} ({2})",
			'            this.Text, versionInfo.ToString(), lastBuilt);
			Text = String.Format("{0} - {1}", Text, versionInfo.ToString())
		End Sub

		Protected Overrides Sub SetVisibleCore(ByVal value As Boolean)
			If Not allowVisible Then
				value = False
				If Not IsHandleCreated Then CreateHandle()
			End If
			MyBase.SetVisibleCore(value)
		End Sub
		Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
			SetVersionInfo()
			EnableAutoUpload()
		End Sub

		Private Sub aboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles aboutToolStripMenuItem.Click
			MessageBox.Show("dh.chau@radiantvs.com")
		End Sub

		Private Sub exitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles exitToolStripMenuItem.Click
			Application.Exit()
		End Sub

		Private Sub exit2ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles exit2ToolStripMenuItem.Click
			Application.Exit()
		End Sub

		Private Sub MainForm_Resize(sender As Object, e As EventArgs) Handles MyBase.Resize
			If minimizedToTrayToolStripMenuItem.Checked = True AndAlso WindowState = FormWindowState.Minimized Then
				Hide()
			End If
		End Sub
		Private Sub notifyIcon1_DoubleClick(sender As Object, e As EventArgs) Handles notifyIcon1.DoubleClick
			allowVisible = True
			Show()
			Activate()
			WindowState = FormWindowState.Normal
		End Sub

		Public Function Upload(InfoFile As String) As Boolean
			Dim uploaded = False
			Dim uploadLog As String = ""
			Dim lines = File.ReadAllLines(InfoFile)
			Dim host = lines(0)
			Dim username = lines(1)
			Dim password = lines(2)
			Dim exePath = lines(3)
			Dim sessionLogPath = lines(4)
			Dim operationLogPath = lines(5)
			Dim sourceFile = lines(6)
			Dim destFile = lines(7)
			Try
				' Setup session options
				Dim sessionOptions As New SessionOptions
				With sessionOptions
					.Protocol = Protocol.Ftp
					.HostName = host
					.UserName = username
					.Password = password
					.TimeoutInMilliseconds = 20000
				End With

				Using session As New Session
					session.ExecutablePath = exePath
					If Not Directory.Exists(Path.GetDirectoryName(sessionLogPath)) Then
						Directory.CreateDirectory(Path.GetDirectoryName(sessionLogPath))
					End If
					session.SessionLogPath = sessionLogPath
					' Connect
					session.Open(sessionOptions)

					' Upload files
					Dim transferOptions As New TransferOptions
					transferOptions.TransferMode = TransferMode.Binary

					Dim transferResult As TransferOperationResult
					transferResult =
					session.PutFiles(sourceFile, destFile, False, transferOptions)

					' Throw on any error
					transferResult.Check()

					' Print results
					For Each transfer In transferResult.Transfers
					Next
				End Using
				uploaded = True
				uploadLog = "Upload succeeded " + sourceFile + " to: " + "ftp://" + host + destFile + System.Environment.NewLine
				File.AppendAllText(operationLogPath, uploadLog)
			Catch e As Exception
				uploadLog = "Upload failed with exception : " + e.Message + sourceFile + " to: " + "ftp://" + host + destFile + System.Environment.NewLine
				File.AppendAllText(operationLogPath, uploadLog)
			End Try
			If uploaded Then
				File.Delete(InfoFile)
			End If
			Return uploaded
		End Function

		Private Sub UploadToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles UploadToolStripMenuItem.Click
			UploadAll()
		End Sub

		Private Sub UploadAll()
			If IsUploading Then
				Exit Sub
			End If
			lblStatus.Invoke(Sub()
								 lblStatus.Text = "Uploading files ..."
							 End Sub)
			IsUploading = True
			Dim count As Integer = 1

			If Not Directory.Exists(txtUploadListPath.Text) Then
				lblStatus.Invoke(Sub()
									 lblStatus.Text = "Queue Folder not existed, try again ..."
								 End Sub)
				IsUploading = False
				Exit Sub
			End If

			Dim uploadList As IEnumerable(Of String) = IO.Directory.EnumerateFiles(txtUploadListPath.Text, "*.txt")

			For Each info As String In uploadList
				If count > Int32.Parse(txtMaximumUpload.Text) Then
					lblStatus.Invoke(Sub()
										 lblStatus.Text = "Uploading finished !"
									 End Sub)
					IsUploading = False
					Exit Sub
				End If
				Upload(info)
				count += 1
			Next
			lblStatus.Invoke(Sub()
								 lblStatus.Text = "Uploading finished !"
							 End Sub)
			IsUploading = False
		End Sub
		Private Sub AutoUploadTaskToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles AutoUploadTaskToolStripMenuItem.Click
			EnableAutoUpload()
		End Sub
		Private Sub EnableAutoUpload()
			lblAutoUpload.ForeColor = System.Drawing.Color.Green
			lblAutoUpload.Text = "Auto Upload Started"
			seconds = 1000 * Int32.Parse(txtInterval.Text)   ' change 5 into the count of minutes
			If timer IsNot Nothing Then
				timer.Stop()
				timer = Nothing
			End If
			timer = New System.Timers.Timer(seconds)
			RemoveHandler timer.Elapsed, AddressOf timer_Elapsed
			AddHandler timer.Elapsed, AddressOf timer_Elapsed
			timer.Start()
		End Sub
		Private Sub timer_Elapsed(sender As Object, e As System.Timers.ElapsedEventArgs)
			UploadAll()
		End Sub

		Private Sub StopAutoUploadToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles StopAutoUploadToolStripMenuItem.Click
			DisableAutoUpload()
		End Sub
		Private Sub DisableAutoUpload()
			lblAutoUpload.ForeColor = System.Drawing.Color.Red
			lblAutoUpload.Text = "Auto Upload Stopped"
			If timer IsNot Nothing Then
				timer.Stop()
				timer = Nothing
			End If
		End Sub
	End Class
End Namespace
