Imports System
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.IO
Imports WinSCP
Imports System.Collections.Generic
Imports System.Diagnostics
Imports Microsoft.VisualBasic
Imports System.Linq
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Runtime.InteropServices.ComTypes

Namespace FTPUploaderVB
	Partial Public Class MainForm
		Inherits Form
		Public apppath As String
		Public appdir As String
		Public settingPath As String
		Public IsUploading As Boolean
		Public uploadTask As Tasks.Task
		Public TasksCancellationTokenSource As New CancellationTokenSource
		Public sw As Stopwatch
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
			settingPath = Path.Combine(appdir, "settingFTPUploader.txt")
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
			LoadSettings()
			RestartTask()
			If startMinimizedToolStripMenuItem.Checked = True Then
				Me.WindowState = FormWindowState.Minimized
			End If
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
				ShowInTaskbar = False
				notifyIcon1.BalloonTipText = "FTPUploader still running and minimized to tray"
				notifyIcon1.ShowBalloonTip(100)
			End If
		End Sub
		Private Sub notifyIcon1_DoubleClick(sender As Object, e As EventArgs) Handles notifyIcon1.DoubleClick
			allowVisible = True
			Show()
			Activate()
			ShowInTaskbar = True
			WindowState = FormWindowState.Normal
		End Sub
		Public Sub Upload(InfoFile As String)
			If Not File.Exists(InfoFile) Then
				Exit Sub
			End If

			Dim uploaded = False
			Dim logContent As String = ""
			Dim lines = File.ReadAllLines(InfoFile)
			Dim host = lines(0)
			Dim username = lines(1)
			Dim password = lines(2)
			Dim exePath = lines(3)
			Dim sessionLogPath = lines(4)
			Dim succeedLogPath = lines(5)
			Dim failLogPath = lines(6)
			Dim sourceFile = lines(7)
			Dim destFile = lines(8)
			Dim sourceIndexFile = lines(10)
			Dim sourceHostFile = lines(13)
			Dim totalFileCount = lines(15)
			Dim channelIndex = lines(16)
			Dim PID = Path.GetFileNameWithoutExtension(sourceIndexFile)

			Dim failCountPath = txtUploadListPath.Text + "\Fail Count\" + Path.GetFileName(InfoFile)
			Dim summaryLogPath = txtUploadListPath.Text + "\Log\" + Now.ToString("yyyyMMdd") + "_summary.csv"

			Static m_Rnd As New Random
			Dim tempcolor As Color
			tempcolor = lblFileUploadStatus.ForeColor
			Do While lblFileUploadStatus.ForeColor = tempcolor
				lblFileUploadStatus.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255))
			Loop

			If Not Directory.Exists(Path.GetDirectoryName(succeedLogPath)) Then
				Directory.CreateDirectory(Path.GetDirectoryName(succeedLogPath))
			End If
			If Not Directory.Exists(Path.GetDirectoryName(failLogPath)) Then
				Directory.CreateDirectory(Path.GetDirectoryName(failLogPath))
			End If

			lblFileStatus.Invoke(Sub()
									 lblFileStatus.Text = "Uploading " + sourceFile + " ..."
								 End Sub)


			Try
				If TasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				If sourceFile = sourceIndexFile Or sourceFile = sourceHostFile Then
					Exit Sub
				End If

				' Setup session options
				Dim sessionOptions As New SessionOptions
				With sessionOptions
					.Protocol = Protocol.Ftp
					.HostName = host
					.UserName = username
					.Password = password
					.TimeoutInMilliseconds = 10000
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
				lblFileUploadStatus.Invoke(Sub()
											   lblFileUploadStatus.Text = "Succeeded "
										   End Sub)

				logContent = Me.Text + vbTab + Now.ToString("HH:mm:ss.fff") + vbTab + "Upload succeeded " + sourceFile + " to: " + "ftp://" + host + destFile + System.Environment.NewLine
				File.AppendAllText(succeedLogPath, logContent)
				If TasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				File.AppendAllText(sourceIndexFile, destFile + "@" + channelIndex + System.Environment.NewLine)
				File.AppendAllText(sourceHostFile, destFile + "@" + channelIndex + System.Environment.NewLine)
				Dim uploadedCount As Integer = File.ReadAllLines(sourceHostFile).Length
				If uploadedCount = Int32.Parse(totalFileCount) Then
					CreateIndexAndHostQueue(InfoFile)
				End If

			Catch e As Exception
				lblFileUploadStatus.Invoke(Sub()
											   lblFileUploadStatus.Text = "Failed "
										   End Sub)

				logContent = Me.Text + vbTab + Now.ToString("HH:mm:ss.fff") + vbTab + "Upload failed with exception : " + e.Message + sourceFile + " to: " + "ftp://" + host + destFile + System.Environment.NewLine
				File.AppendAllText(failLogPath, logContent)
				If Not Directory.Exists(Path.GetDirectoryName(failCountPath)) Then
					Directory.CreateDirectory(Path.GetDirectoryName(failCountPath))
				End If
				Dim failCount As Integer = 0
				Dim failRetry As Integer = 0
				If Not File.Exists(failCountPath) Then
					failCount = 1
					File.WriteAllText(failCountPath, "1")
				Else
					Dim failLines = File.ReadAllLines(failCountPath)
					failCount = CInt(failLines(0))
					failCount += 1
					File.WriteAllText(failCountPath, failCount.ToString)

				End If

				If Not Int32.TryParse(txtMaximumFailRetry.Text, failRetry) Then
					failRetry = 0
				End If
				If failCount >= failRetry Then
					File.Delete(InfoFile)
					File.Delete(failCountPath)
					UpdateSummaryLogFail(summaryLogPath, PID, destFile, e.Message)
				End If

			End Try
			If uploaded Then
				File.Delete(InfoFile)
				UpdateSummaryLogSucceed(summaryLogPath, PID, destFile)
				If File.Exists(failCountPath) Then
					File.Delete(failCountPath)
				End If
			End If
		End Sub

		Private Sub UploadIndexAndHost(InfoFile As String)
			If Not File.Exists(InfoFile) Then
				Exit Sub
			End If

			Dim uploaded = False
			Dim logContent As String = ""
			Dim lines = File.ReadAllLines(InfoFile)
			Dim host = lines(0)
			Dim username = lines(1)
			Dim password = lines(2)
			Dim exePath = lines(3)
			Dim sessionLogPath = lines(4)
			Dim succeedLogPath = lines(5)
			Dim failLogPath = lines(6)
			Dim sourceFile = lines(7)
			Dim destFile = lines(8)

			Static m_Rnd As New Random
			Dim tempcolor As Color
			tempcolor = lblFileUploadStatus.ForeColor
			Do While lblFileUploadStatus.ForeColor = tempcolor
				lblFileUploadStatus.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255))
			Loop

			If Not Directory.Exists(Path.GetDirectoryName(succeedLogPath)) Then
				Directory.CreateDirectory(Path.GetDirectoryName(succeedLogPath))
			End If
			If Not Directory.Exists(Path.GetDirectoryName(failLogPath)) Then
				Directory.CreateDirectory(Path.GetDirectoryName(failLogPath))
			End If


			lblFileStatus.Invoke(Sub()
									 lblFileStatus.Text = "Uploading " + sourceFile + " ..."
								 End Sub)


			Try
				If TasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
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
				lblFileUploadStatus.Invoke(Sub()
											   lblFileUploadStatus.Text = "Succeeded "
										   End Sub)

				logContent = Now.ToString("HH:mm:ss.fff") + vbTab + "Upload succeeded " + sourceFile + " to: " + "ftp://" + host + destFile + System.Environment.NewLine
				File.AppendAllText(succeedLogPath, logContent)

			Catch e As Exception
				lblFileUploadStatus.Invoke(Sub()
											   lblFileUploadStatus.Text = "Failed "
										   End Sub)

				logContent = Now.ToString("HH:mm:ss.fff") + vbTab + "Upload failed with exception : " + e.Message + sourceFile + " to: " + "ftp://" + host + destFile + System.Environment.NewLine
				File.AppendAllText(failLogPath, logContent)
			End Try

			File.Delete(InfoFile)

		End Sub

		Private Sub CreateIndexAndHostQueue(InfoFile As String)
			If Not File.Exists(InfoFile) Then
				Exit Sub
			End If
			Dim lines = File.ReadAllLines(InfoFile)

			Dim OutputIndexInfoFile = lines(9)
			Dim OutputHostInfoFile = lines(12)
			If TasksCancellationTokenSource.IsCancellationRequested Then
				Exit Sub
			End If
			If Not File.Exists(OutputIndexInfoFile) AndAlso OutputIndexInfoFile <> InfoFile Then
				lines(7) = lines(10)
				lines(8) = lines(11)
				File.WriteAllLines(OutputIndexInfoFile, lines)
			End If
			UploadIndexAndHost(OutputIndexInfoFile)
			If Not File.Exists(OutputHostInfoFile) AndAlso OutputHostInfoFile <> InfoFile Then
				lines(7) = lines(13)
				lines(8) = lines(14)
				File.WriteAllLines(OutputHostInfoFile, lines)
			End If
			UploadIndexAndHost(OutputHostInfoFile)
		End Sub

		Private Sub UpdateSummaryLogFail(summaryLogFile As String, PID As String, destFile As String, failMessage As String)
			If Not File.Exists(summaryLogFile) Then
				Exit Sub
			End If
			Dim failedFileName As String = ""
			Dim failedFileReason As String = ""
			Dim spacePos As Integer = failMessage.IndexOf(".")
			failMessage = failMessage.Substring(0, spacePos)

			If Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("otp") Then
				failedFileName = "OTP"
				failedFileReason = "OTP_X"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("gamma") Then
				failedFileName = "GAMMA"
				failedFileReason = "GAMMA_X"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("1g1o") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("map") Then
				failedFileName = "HEX_MAP"
				failedFileReason = "HEX_MAP_X"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("1g1o") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("rcb") Then
				failedFileName = "HEX_RCB"
				failedFileReason = "HEX_RCB_X"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("1g1o") Then
				failedFileName = "HEX"
				failedFileReason = "HEX_X"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("1st") Then
				failedFileName = "HEX_1ST"
				failedFileReason = "HEX_1ST_X"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("2nd") Then
				failedFileName = "HEX_2ND"
				failedFileReason = "HEX_2ND_X"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("3rd") Then
				failedFileName = "HEX_3RD"
				failedFileReason = "HEX_3RD_X"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("4th") Then
				failedFileName = "HEX_4TH"
				failedFileReason = "HEX_4TH_X"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("5th") Then
				failedFileName = "HEX_5TH"
				failedFileReason = "HEX_5TH_X"
			ElseIf Path.GetFileNameWithoutExtension(destFile).Contains("step2_03") Then
				failedFileName = Path.GetFileNameWithoutExtension(destFile).Replace("step2_03_", "").Replace("_imgY_Crop", "")
				failedFileReason = failedFileName + "_X"
			Else
				failedFileName = Path.GetFileNameWithoutExtension(destFile).Replace("_imgY_Crop", "")
				failedFileReason = failedFileName + "_X"
			End If
			Dim lines = File.ReadAllLines(summaryLogFile)
			Dim columnHeader = lines(0).Split(",")
			Dim failedFileIndex As Integer = 1
			Dim failedReasonIndex As Integer = 1
			Dim columnAndFileNameMatch As Boolean = False
			Dim columnAndFileReasonMatch As Boolean = False
			For index = 0 To columnHeader.Count - 1
				If columnHeader(index) = failedFileName.ToUpper Then
					failedFileIndex = index
					columnAndFileNameMatch = True
					Exit For
				End If

			Next
			For index = 0 To columnHeader.Count - 1

				If columnHeader(index) = failedFileReason.ToUpper Then
					failedReasonIndex = index
					columnAndFileReasonMatch = True
					Exit For
				End If
			Next
			If columnAndFileNameMatch Then
				For i = 1 To lines.Count - 1
					Dim newlineList = lines(i).Split(",")
					If newlineList.Contains(PID) Then
						newlineList(failedFileIndex) = "X"
					End If
					Dim newLine As String = String.Join(",", newlineList)
					lines(i) = newLine
				Next
				File.WriteAllLines(summaryLogFile, lines)
			End If
			If columnAndFileReasonMatch Then
				For i = 1 To lines.Count - 1
					Dim newlineList = lines(i).Split(",")
					If newlineList.Contains(PID) Then
						newlineList(failedReasonIndex) = failMessage
					End If
					Dim newLine As String = String.Join(",", newlineList)
					lines(i) = newLine
				Next
				File.WriteAllLines(summaryLogFile, lines)
			End If

		End Sub

		Private Sub UpdateSummaryLogSucceed(summaryLogFile As String, PID As String, destFile As String)
			If Not File.Exists(summaryLogFile) Then
				Exit Sub
			End If
			Dim succeededFileName As String = ""

			If Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("otp") Then
				succeededFileName = "OTP"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("gamma") Then
				succeededFileName = "GAMMA"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("1g1o") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("map") Then
				succeededFileName = "HEX_MAP"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("1g1o") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("rcb") Then
				succeededFileName = "HEX_RCB"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("1g1o") Then
				succeededFileName = "HEX"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("1st") Then
				succeededFileName = "HEX_1ST"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("2nd") Then
				succeededFileName = "HEX_2ND"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("3rd") Then
				succeededFileName = "HEX_3RD"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("4th") Then
				succeededFileName = "HEX_4TH"
			ElseIf Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("nypucdata") AndAlso Path.GetFileNameWithoutExtension(destFile).ToLower.Contains("5th") Then
				succeededFileName = "HEX_5TH"
			ElseIf Path.GetFileNameWithoutExtension(destFile).Contains("step2_03") Then
				succeededFileName = Path.GetFileNameWithoutExtension(destFile).Replace("step2_03_", "").Replace("_imgY_Crop", "")
			Else
				succeededFileName = Path.GetFileNameWithoutExtension(destFile).Replace("_imgY_Crop", "")
			End If
			Dim lines = File.ReadAllLines(summaryLogFile)
			Dim columnHeader = lines(0).Split(",")
			Dim columnAndFileMatch As Boolean = False
			Dim succeededFileIndex As Integer = 1
			For index = 0 To columnHeader.Count - 1
				If columnHeader(index) = succeededFileName.ToUpper Then
					succeededFileIndex = index
					columnAndFileMatch = True
					Exit For
				End If
			Next
			If columnAndFileMatch Then
				For i = 1 To lines.Count - 1
					Dim newlineList = lines(i).Split(",")
					If newlineList.Contains(PID) Then
						newlineList(succeededFileIndex) = "O"
					End If
					Dim newLine As String = String.Join(",", newlineList)
					lines(i) = newLine
				Next
				File.WriteAllLines(summaryLogFile, lines)
			End If


		End Sub

		Private Sub UploadAll()

			If Not Directory.Exists(txtUploadListPath.Text) Then
				Try
					Directory.CreateDirectory(txtUploadListPath.Text)
				Catch ex As Exception
					lblFileStatus.Invoke(Sub()
											 lblFileStatus.Text = "Queue Folder not existed, try again ..."
										 End Sub)
					Exit Sub
				End Try
			End If


			While Not TasksCancellationTokenSource.IsCancellationRequested

				Try
					lblStatus.Invoke(Sub()
										 lblStatus.Text = "Uploading files ..."
									 End Sub)

					Dim root As String = txtUploadListPath.Text
					Dim maximumUpload As Integer = Int32.Parse(txtMaximumUpload.Text)
					Dim uploadList As IEnumerable(Of String) = IO.Directory.EnumerateFiles(root, "*.txt") _
															.OrderByDescending(Of Date)(Function(x As String) IO.File.GetCreationTime(x)) _
															.Take(maximumUpload)

					For Each info As String In uploadList
						If TasksCancellationTokenSource.IsCancellationRequested Then

							Exit Sub
						End If
						Upload(info)

					Next

					lblStatus.Invoke(Sub()
										 lblStatus.Text = "Uploading finished !"
									 End Sub)

				Catch ex As Exception
					lblStatus.Invoke(Sub()
										 lblStatus.Text = "Error uploading : " + ex.Message
									 End Sub)

				Finally
					lblStatus.Invoke(Sub()
										 lblStatus.Text = "Reset timer for uploading ."
									 End Sub)

				End Try
				Dim checkTime As New Integer
				If Not Int32.TryParse(txtInterval.Text, checkTime) Then
					checkTime = 1
				End If

				sw = New Stopwatch
				sw.Start()
				While sw.ElapsedMilliseconds < 1000 * checkTime
					If TasksCancellationTokenSource.IsCancellationRequested Then

						Exit Sub
					End If
					System.Threading.Thread.Sleep(100)
				End While
			End While

		End Sub

		Private Sub RestartTask()
			TasksCancellationTokenSource = New CancellationTokenSource
			uploadTask = New Tasks.Task(New Action(Sub() UploadAll()), TasksCancellationTokenSource.Token)
			uploadTask.Start()
			cmdStartUpload.Enabled = False
			cmdStopUpload.Enabled = True
			txtInterval.Enabled = False
			txtMaximumUpload.Enabled = False
			txtUploadListPath.Enabled = False
			txtMaximumFailRetry.Enabled = False
		End Sub
		Private Sub StopTask()
			If TasksCancellationTokenSource IsNot Nothing Then
				TasksCancellationTokenSource.Cancel()
			End If
			If uploadTask IsNot Nothing Then
				If uploadTask.Status <> TaskStatus.RanToCompletion Then
					'Wait a little longer
					Dim sw As New Stopwatch
					sw.Start()
					Do Until uploadTask.Status = TaskStatus.RanToCompletion
						If uploadTask.Status = TaskStatus.Canceled Then Exit Do
						If uploadTask.Status = TaskStatus.Faulted Then Exit Do
						If sw.ElapsedMilliseconds > 1000 Then Exit Do
					Loop
					sw.Stop()
				End If
				If uploadTask.IsCompleted OrElse uploadTask.IsCanceled OrElse uploadTask.IsFaulted Then
					uploadTask.Dispose()
				End If
				uploadTask = Nothing
			End If
			cmdStartUpload.Enabled = True
			cmdStopUpload.Enabled = False
			txtInterval.Enabled = True
			txtMaximumUpload.Enabled = True
			txtUploadListPath.Enabled = True
			txtMaximumFailRetry.Enabled = True
		End Sub

		Private Sub cmdStartUpload_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStartUpload.LinkClicked
			RestartTask()
		End Sub

		Private Sub cmdStopUpload_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStopUpload.LinkClicked
			StopTask()

		End Sub

		Private Sub SaveSettings()
			Dim settings As New Dictionary(Of String, String)
			If startMinimizedToolStripMenuItem.Checked Then
				settings.Add("startminimized", "true")
			Else
				settings.Add("startminimized", "false")
			End If
			If minimizedToTrayToolStripMenuItem.Checked Then
				settings.Add("minimizedtotray", "true")
			Else
				settings.Add("minimizedtotray", "false")
			End If

			Dim settingContent As String = ""
			Dim keys() As String = settings.Keys.ToArray
			For Each k As String In keys
				settingContent += k + "=" + settings(k) + Environment.NewLine
			Next
			Try
				File.WriteAllText(settingPath, settingContent)
			Catch ex As Exception

			End Try
		End Sub

		Private Sub LoadSettings()
			Try
				If File.Exists(settingPath) Then
					Dim settings() As String = File.ReadAllLines(settingPath)
					For Each line As String In settings
						Dim setting As String = line.Split("=")(0)
						Dim value As String = line.Split("=")(1)
						If setting = "startminimized" Then
							startMinimizedToolStripMenuItem.Checked = If(value = "true", True, False)
						ElseIf setting = "minimizedtotray" Then
							minimizedToTrayToolStripMenuItem.Checked = If(value = "true", True, False)
						End If
					Next
				End If
			Catch ex As Exception

			End Try
		End Sub

		Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
			SaveSettings()
		End Sub
	End Class
End Namespace
