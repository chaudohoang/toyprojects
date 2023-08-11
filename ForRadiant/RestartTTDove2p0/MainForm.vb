Imports System
Imports System.Windows.Forms
Imports System.Runtime.InteropServices
Imports System.Reflection
Imports System.IO
Imports System.Collections.Generic
Imports System.Diagnostics
Imports Microsoft.VisualBasic
Imports System.Linq
Imports System.Drawing
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Text

Namespace RestartTT
	Partial Public Class MainForm
		Inherits Form
		Public apppath As String
		Public appdir As String
		Public settingPath As String
		Public monitorResultMessageTask As Tasks.Task
		Public monitorRestartMessageTask As Tasks.Task
		Public monitorOnInitializeMessageTask As Tasks.Task
		Public monitorResultMessageTaskTasksCancellationTokenSource As New CancellationTokenSource
		Public monitorRestartMessageTaskTasksCancellationTokenSource As New CancellationTokenSource
		Public monitorOnInitializeMessageTaskTasksCancellationTokenSource As New CancellationTokenSource
		Public ResultMessageCount As Integer = 0
		Public IgnoreExistedResult As Boolean
		Public IgnoreExistedRestart As Boolean
		Public IgnoreExistedOnInitialize As Boolean
		Public monitorResultMessageRunning As Boolean
		Public monitorRestartMessageRunning As Boolean
		Public monitorOnInitializeMessageRunning As Boolean
		Private allowVisible As Boolean = True
		Public today As String = Now.ToString("yyyyMMdd")
		Public logPath As String = ""
		Public sw As Stopwatch

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
			settingPath = Path.Combine(appdir, "settingRestartTTDove2p0.txt")
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
			logPath = "C:\Radiant Vision Systems Data\TrueTest\AppData\" + today + " Operation Log.txt"
			lblTodayLogPath.Text = logPath
			If Not File.Exists(logPath) Then
				File.WriteAllText(logPath, "")
			End If
			SetVersionInfo()
			LoadSettings()
			If startMinimizedToolStripMenuItem.Checked = True Then
				Me.WindowState = FormWindowState.Minimized
			End If
			Dim checkChangedDayLogTask = New Tasks.Task(New Action(Sub() CheckForChangeDayLog()))
			checkChangedDayLogTask.Start()
			StartMonitorOnInitializeMessage()
			monitorOnInitializeMessageRunning = True
		End Sub
		Public Sub CheckForChangeDayLog()
			Dim startTime As DateTime = DateTime.Now
			Dim tomorrow As DateTime
			Dim newToday As String
			tomorrow = startTime + New TimeSpan(1, 0, 0, 0, 0)
			Dim year = tomorrow.Year
			Dim month = tomorrow.Month
			Dim day = tomorrow.Day
			Dim endTime = New DateTime(year, month, day, 0, 0, 0)
			Dim duration = endTime - startTime
			Dim durationInSeconds = duration.TotalSeconds

			While True
				newToday = Now.ToString("yyyyMMdd")
				If newToday <> today Then
					today = newToday
					logPath = "C:\Radiant Vision Systems Data\TrueTest\AppData\" + today + " Operation Log.txt"
					lblTodayLogPath.Invoke(Sub()
											   lblTodayLogPath.Text = logPath
										   End Sub)
					If monitorResultMessageRunning Then
						monitorResultMessageRunning = False
						StopMonitorResultMessage()
						StartMonitorResultMessage()
						monitorResultMessageRunning = True
					End If
					If monitorRestartMessageRunning Then
						monitorRestartMessageRunning = False
						StopMonitorRestartMessage()
						StartMonitorRestartMessage()
						monitorRestartMessageRunning = True
					End If
					If monitorOnInitializeMessageRunning Then
						monitorOnInitializeMessageRunning = False
						StopMonitorOnInitializeMessage()
						StartMonitorOnInitializeMessage()
						monitorOnInitializeMessageRunning = True
					End If
				End If
				System.Threading.Thread.Sleep(1000)
			End While

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
				notifyIcon1.BalloonTipText = "RestartTTDove2p0 still running and minimized to tray"
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

		Private Sub StartMonitorResultMessage()
			IgnoreExistedResult = IgnoreExistingResultMessage()
			today = Now.ToString("yyyyMMdd")
			logPath = "C:\Radiant Vision Systems Data\TrueTest\AppData\" + today + " Operation Log.txt"
			File.AppendAllText("log.txt", "today = " + today + Environment.NewLine + "logPath = " + logPath + Environment.NewLine)
			monitorResultMessageTaskTasksCancellationTokenSource = New CancellationTokenSource
			monitorResultMessageTask = New Tasks.Task(New Action(Sub() MonitorTailOfFileForResultMessage()), monitorResultMessageTaskTasksCancellationTokenSource.Token)
			monitorResultMessageTask.Start()

			lblResultMonitoringStatus.Invoke(Sub()
												 lblResultMonitoringStatus.Text = "Status : Monitoring ..."
											 End Sub)
			cmdStartMonitorResultMessage.Invoke(Sub()
													cmdStartMonitorResultMessage.Enabled = False
												End Sub)
			cmdStopMonitorResultMessage.Invoke(Sub()
												   cmdStopMonitorResultMessage.Enabled = True
											   End Sub)

		End Sub

		Private Sub StopMonitorResultMessage()

			If monitorResultMessageTaskTasksCancellationTokenSource IsNot Nothing Then
				monitorResultMessageTaskTasksCancellationTokenSource.Cancel()
			End If
			If monitorResultMessageTask IsNot Nothing Then
				If monitorResultMessageTask.Status <> TaskStatus.RanToCompletion Then
					'Wait a little longer
					Dim sw As New Stopwatch
					sw.Start()
					Do Until monitorResultMessageTask.Status = TaskStatus.RanToCompletion
						If monitorResultMessageTask.Status = TaskStatus.Canceled Then Exit Do
						If monitorResultMessageTask.Status = TaskStatus.Faulted Then Exit Do
						If sw.ElapsedMilliseconds > 1000 Then Exit Do
					Loop
					sw.Stop()
				End If
				If monitorResultMessageTask.IsCompleted OrElse monitorResultMessageTask.IsCanceled OrElse monitorResultMessageTask.IsFaulted Then
					monitorResultMessageTask.Dispose()
				End If
				monitorResultMessageTask = Nothing
			End If
			lblResultMonitoringStatus.Invoke(Sub()
												 lblResultMonitoringStatus.Text = "Status : Stopped ..."
											 End Sub)
			cmdStartMonitorResultMessage.Invoke(Sub()
													cmdStartMonitorResultMessage.Enabled = True
												End Sub)
			cmdStopMonitorResultMessage.Invoke(Sub()
												   cmdStopMonitorResultMessage.Enabled = False
											   End Sub)

		End Sub

		Private Sub StartMonitorRestartMessage()
			IgnoreExistedRestart = IgnoreExistingRestartMessage()
			today = Now.ToString("yyyyMMdd")
			logPath = "C:\Radiant Vision Systems Data\TrueTest\AppData\" + today + " Operation Log.txt"
			File.AppendAllText("log.txt", "today = " + today + Environment.NewLine + "logPath = " + logPath + Environment.NewLine)
			monitorRestartMessageTaskTasksCancellationTokenSource = New CancellationTokenSource
			monitorRestartMessageTask = New Tasks.Task(New Action(Sub() MonitorTailOfFileForRestartMessage()), monitorRestartMessageTaskTasksCancellationTokenSource.Token)
			monitorRestartMessageTask.Start()
			lblRestartMonitoringStatus.Invoke(Sub()
												  lblRestartMonitoringStatus.Text = "Status : Monitoring ..."
											  End Sub)
			cmdStartMonitorRestartMessage.Invoke(Sub()
													 cmdStartMonitorRestartMessage.Enabled = False
												 End Sub)
			cmdStopMonitorRestartMessage.Invoke(Sub()
													cmdStopMonitorRestartMessage.Enabled = True
												End Sub)

		End Sub

		Private Sub StartMonitorOnInitializeMessage()
			IgnoreExistedOnInitialize = IgnoreExistingOnInitializeMessage()
			today = Now.ToString("yyyyMMdd")
			logPath = "C:\Radiant Vision Systems Data\TrueTest\AppData\" + today + " Operation Log.txt"
			File.AppendAllText("log.txt", "today = " + today + Environment.NewLine + "logPath = " + logPath + Environment.NewLine)
			monitorOnInitializeMessageTaskTasksCancellationTokenSource = New CancellationTokenSource
			monitorOnInitializeMessageTask = New Tasks.Task(New Action(Sub() MonitorTailOfFileForOnInitializeMessage()), monitorOnInitializeMessageTaskTasksCancellationTokenSource.Token)
			monitorOnInitializeMessageTask.Start()
			lblOnInitializeMonitoringStatus.Invoke(Sub()
													   lblOnInitializeMonitoringStatus.Text = "Status : Monitoring ..."
												   End Sub)
			cmdStartMonitorOnInitializeMessage.Invoke(Sub()
														  cmdStartMonitorOnInitializeMessage.Enabled = False
													  End Sub)
			cmdStopMonitorOnInitializeMessage.Invoke(Sub()
														 cmdStopMonitorOnInitializeMessage.Enabled = True
													 End Sub)

		End Sub

		Private Sub StopMonitorOnInitializeMessage()

			If monitorOnInitializeMessageTaskTasksCancellationTokenSource IsNot Nothing Then
				monitorOnInitializeMessageTaskTasksCancellationTokenSource.Cancel()
			End If
			If monitorOnInitializeMessageTask IsNot Nothing Then
				If monitorOnInitializeMessageTask.Status <> TaskStatus.RanToCompletion Then
					'Wait a little longer
					Dim sw As New Stopwatch
					sw.Start()
					Do Until monitorOnInitializeMessageTask.Status = TaskStatus.RanToCompletion
						If monitorOnInitializeMessageTask.Status = TaskStatus.Canceled Then Exit Do
						If monitorOnInitializeMessageTask.Status = TaskStatus.Faulted Then Exit Do
						If sw.ElapsedMilliseconds > 1000 Then Exit Do
					Loop
					sw.Stop()
				End If
				If monitorOnInitializeMessageTask.IsCompleted OrElse monitorOnInitializeMessageTask.IsCanceled OrElse monitorOnInitializeMessageTask.IsFaulted Then
					monitorOnInitializeMessageTask.Dispose()
				End If
				monitorOnInitializeMessageTask = Nothing
			End If
			lblOnInitializeMonitoringStatus.Invoke(Sub()
													   lblOnInitializeMonitoringStatus.Text = "Status : Stopped ..."
												   End Sub)
			cmdStartMonitorOnInitializeMessage.Invoke(Sub()
														  cmdStartMonitorOnInitializeMessage.Enabled = True
													  End Sub)
			cmdStopMonitorOnInitializeMessage.Invoke(Sub()
														 cmdStopMonitorOnInitializeMessage.Enabled = False
													 End Sub)

		End Sub

		Private Sub StopMonitorRestartMessage()

			If monitorRestartMessageTaskTasksCancellationTokenSource IsNot Nothing Then
				monitorRestartMessageTaskTasksCancellationTokenSource.Cancel()
			End If
			If monitorRestartMessageTask IsNot Nothing Then
				If monitorRestartMessageTask.Status <> TaskStatus.RanToCompletion Then
					'Wait a little longer
					Dim sw As New Stopwatch
					sw.Start()
					Do Until monitorRestartMessageTask.Status = TaskStatus.RanToCompletion
						If monitorRestartMessageTask.Status = TaskStatus.Canceled Then Exit Do
						If monitorRestartMessageTask.Status = TaskStatus.Faulted Then Exit Do
						If sw.ElapsedMilliseconds > 1000 Then Exit Do
					Loop
					sw.Stop()
				End If
				If monitorRestartMessageTask.IsCompleted OrElse monitorRestartMessageTask.IsCanceled OrElse monitorRestartMessageTask.IsFaulted Then
					monitorRestartMessageTask.Dispose()
				End If
				monitorRestartMessageTask = Nothing
			End If
			lblRestartMonitoringStatus.Invoke(Sub()
												  lblRestartMonitoringStatus.Text = "Status : Stopped ..."
											  End Sub)
			cmdStartMonitorRestartMessage.Invoke(Sub()
													 cmdStartMonitorRestartMessage.Enabled = True
												 End Sub)
			cmdStopMonitorRestartMessage.Invoke(Sub()
													cmdStopMonitorRestartMessage.Enabled = False
												End Sub)

		End Sub

		Public Sub MonitorTailOfFileForResultMessage()
			If Not File.Exists(logPath) Then
				File.AppendAllText(logPath, "")
			End If
			Dim initialFileSize = New FileInfo(logPath).Length
			Dim lastReadLength = initialFileSize - 1024
			If lastReadLength < 0 Then lastReadLength = 0

			While True
				If monitorResultMessageTaskTasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				Try
					Dim fileSize = New FileInfo(logPath).Length

					If fileSize > lastReadLength Then

						Using fs = New FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
							fs.Seek(lastReadLength, SeekOrigin.Begin)
							Dim buffer = New Byte(1023) {}

							While True
								If monitorResultMessageTaskTasksCancellationTokenSource.IsCancellationRequested Then
									Exit Sub
								End If
								Dim bytesRead = fs.Read(buffer, 0, buffer.Length)
								lastReadLength += bytesRead
								If bytesRead = 0 Then Exit While
								Dim text = ASCIIEncoding.ASCII.GetString(buffer, 0, bytesRead)

								If text.Contains("Sent : RESULT") And IgnoreExistedResult Then
									IgnoreExistedResult = False
								ElseIf text.Contains("Sent : RESULT") And Not IgnoreExistedResult Then
									ResultMessageCount += 1
									lblRunCount.Invoke(Sub()
														   lblRunCount.Text = ResultMessageCount.ToString
													   End Sub)
								End If

								If ResultMessageCount.ToString = txtRunCount.Text Then
									Dim waitTime As New Integer
									If Not Int32.TryParse(txtWaitResult.Text, waitTime) Then
										waitTime = 1
									End If
									Thread.Sleep(waitTime * 1000)
									Try

										ResultMessageCount = 0
										lblRunCount.Invoke(Sub()
															   lblRunCount.Text = ResultMessageCount.ToString
														   End Sub)
										For Each process As Process In Process.GetProcessesByName("TrueTest")
											process.Kill()
											Process.Start("TrueTest")
										Next
									Catch ex As Exception

									End Try
								End If
								'File.AppendAllText("log.txt", text)
							End While
						End Using
					End If

				Catch
				End Try

				Thread.Sleep(1000)
			End While
		End Sub

		Public Function IgnoreExistingResultMessage() As Boolean
			Dim ignore As Boolean
			If Not File.Exists(logPath) Then
				File.AppendAllText(logPath, "")
			End If
			Dim initialFileSize = New FileInfo(logPath).Length
			Dim lastReadLength = initialFileSize - 1024
			If lastReadLength < 0 Then lastReadLength = 0

			Try
				Dim fileSize = New FileInfo(logPath).Length

				If fileSize > lastReadLength Then

					Using fs = New FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
						fs.Seek(lastReadLength, SeekOrigin.Begin)
						Dim buffer = New Byte(1023) {}

						Dim bytesRead = fs.Read(buffer, 0, buffer.Length)
						lastReadLength += bytesRead
						If bytesRead = 0 Then
							ignore = False
						End If
						Dim text = ASCIIEncoding.ASCII.GetString(buffer, 0, bytesRead)

						If text.Contains("Sent : RESULT") Then
							ignore = True

						End If

					End Using
				End If

			Catch
			End Try
			Return ignore
		End Function

		Public Function IgnoreExistingRestartMessage() As Boolean
			Dim ignore As Boolean
			If Not File.Exists(logPath) Then
				File.AppendAllText(logPath, "")
			End If
			Dim initialFileSize = New FileInfo(logPath).Length
			Dim lastReadLength = initialFileSize - 1024
			If lastReadLength < 0 Then lastReadLength = 0

			Try
				Dim fileSize = New FileInfo(logPath).Length

				If fileSize > lastReadLength Then

					Using fs = New FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
						fs.Seek(lastReadLength, SeekOrigin.Begin)
						Dim buffer = New Byte(1023) {}

						Dim bytesRead = fs.Read(buffer, 0, buffer.Length)
						lastReadLength += bytesRead
						If bytesRead = 0 Then
							ignore = False
						End If
						Dim text = ASCIIEncoding.ASCII.GetString(buffer, 0, bytesRead)

						If text.Contains("Sent RESTART response") Then
							ignore = True

						End If

					End Using
				End If

			Catch
			End Try
			Return ignore
		End Function

		Public Function IgnoreExistingOnInitializeMessage() As Boolean
			Dim ignore As Boolean
			If Not File.Exists(logPath) Then
				File.AppendAllText(logPath, "")
			End If
			Dim initialFileSize = New FileInfo(logPath).Length
			Dim lastReadLength = initialFileSize - 1024
			If lastReadLength < 0 Then lastReadLength = 0

			Try
				Dim fileSize = New FileInfo(logPath).Length

				If fileSize > lastReadLength Then

					Using fs = New FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
						fs.Seek(lastReadLength, SeekOrigin.Begin)
						Dim buffer = New Byte(1023) {}

						Dim bytesRead = fs.Read(buffer, 0, buffer.Length)
						lastReadLength += bytesRead
						If bytesRead = 0 Then
							ignore = False
						End If
						Dim text = ASCIIEncoding.ASCII.GetString(buffer, 0, bytesRead)

						If text.Contains("On initialize #2") Then
							ignore = True

						End If

					End Using
				End If

			Catch
			End Try
			Return ignore
		End Function

		Public Sub MonitorTailOfFileForRestartMessage()
			If Not File.Exists(logPath) Then
				File.AppendAllText(logPath, "")
			End If
			Dim initialFileSize = New FileInfo(logPath).Length
			Dim lastReadLength = initialFileSize - 1024
			If lastReadLength < 0 Then lastReadLength = 0

			While True
				If monitorRestartMessageTaskTasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				Try
					Dim fileSize = New FileInfo(logPath).Length

					If fileSize > lastReadLength Then

						Using fs = New FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
							fs.Seek(lastReadLength, SeekOrigin.Begin)
							Dim buffer = New Byte(1023) {}

							While True
								If monitorRestartMessageTaskTasksCancellationTokenSource.IsCancellationRequested Then
									Exit Sub
								End If
								Dim bytesRead = fs.Read(buffer, 0, buffer.Length)
								lastReadLength += bytesRead
								If bytesRead = 0 Then Exit While
								Dim text = ASCIIEncoding.ASCII.GetString(buffer, 0, bytesRead)

								If text.Contains("Sent RESTART response") And IgnoreExistedRestart Then
									IgnoreExistedRestart = False
								ElseIf text.Contains("Sent RESTART response") And Not IgnoreExistedRestart Then
									Dim waitTime As New Integer
									If Not Int32.TryParse(txtWaitRestart.Text, waitTime) Then
										waitTime = 1
									End If
									Thread.Sleep(waitTime * 1000)
									Try
										For Each process As Process In Process.GetProcessesByName("TrueTest")
											process.Kill()
											Process.Start("TrueTest")
										Next
									Catch ex As Exception

									End Try
								End If

								'File.AppendAllText("log.txt", text)
							End While
						End Using
					End If

				Catch
				End Try

				Thread.Sleep(1000)
			End While
		End Sub

		Public Sub MonitorTailOfFileForOnInitializeMessage()
			If Not File.Exists(logPath) Then
				File.AppendAllText(logPath, "")
			End If
			Dim initialFileSize = New FileInfo(logPath).Length
			Dim lastReadLength = initialFileSize - 1024
			If lastReadLength < 0 Then lastReadLength = 0

			While True
				If monitorOnInitializeMessageTaskTasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				Try
					Dim fileSize = New FileInfo(logPath).Length

					If fileSize > lastReadLength Then

						Using fs = New FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
							fs.Seek(lastReadLength, SeekOrigin.Begin)
							Dim buffer = New Byte(1023) {}

							While True
								If monitorOnInitializeMessageTaskTasksCancellationTokenSource.IsCancellationRequested Then
									Exit Sub
								End If
								Dim bytesRead = fs.Read(buffer, 0, buffer.Length)
								lastReadLength += bytesRead
								If bytesRead = 0 Then Exit While
								Dim text = ASCIIEncoding.ASCII.GetString(buffer, 0, bytesRead)

								If text.Contains("On initialize #2") And IgnoreExistedOnInitialize Then
									IgnoreExistedOnInitialize = False
								ElseIf text.Contains("On initialize #2") And Not IgnoreExistedOnInitialize Then
									Dim waitTime As New Integer
									If Not Int32.TryParse(txtWaitOnInitialize.Text, waitTime) Then
										waitTime = 1
									End If
									Thread.Sleep(waitTime * 1000)
									Try
										For Each process As Process In Process.GetProcessesByName("TrueTest")
											process.Kill()
											Process.Start("TrueTest")
										Next
									Catch ex As Exception

									End Try
								End If

								'File.AppendAllText("log.txt", text)
							End While
						End Using
					End If

				Catch
				End Try

				Thread.Sleep(1000)
			End While
		End Sub

		Private Sub cmdSaveSettings_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdSaveSettings.LinkClicked
			SaveSettings()
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
			settings.Add("numruns", txtRunCount.Text)
			settings.Add("waitsec", txtWaitResult.Text)
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
						ElseIf setting = "numruns" Then
							txtRunCount.Text = value
						ElseIf setting = "waitsec" Then
							txtWaitResult.Text = value
						End If
					Next
				End If
			Catch ex As Exception

			End Try
		End Sub

		Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
			SaveSettings()
		End Sub

		Private Sub cmdResetRunCount_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdResetRunCount.LinkClicked
			ResultMessageCount = 0
			lblRunCount.Invoke(Sub()
								   lblRunCount.Text = ResultMessageCount.ToString
							   End Sub)
		End Sub

		Private Sub cmdRestartTTNowResult_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdRestartTTNowResult.LinkClicked
			Try
				For Each process As Process In Process.GetProcessesByName("TrueTest")
					process.Kill()
					Process.Start("TrueTest")
				Next
			Catch ex As Exception

			End Try
		End Sub

		Private Sub cmdRestartNowRestart_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdRestartNowRestart.LinkClicked
			Try
				For Each process As Process In Process.GetProcessesByName("TrueTest")
					process.Kill()
					Process.Start("TrueTest")
				Next
			Catch ex As Exception

			End Try
		End Sub

		Private Sub cmdStartMonitorResultMessage_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStartMonitorResultMessage.LinkClicked
			StartMonitorResultMessage()
			monitorResultMessageRunning = True
		End Sub

		Private Sub cmdStopMonitorResultMessage_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStopMonitorResultMessage.LinkClicked
			StopMonitorResultMessage()
			monitorResultMessageRunning = False
		End Sub

		Private Sub cmdStartMonitorRestartMessage_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStartMonitorRestartMessage.LinkClicked
			StartMonitorRestartMessage()
			monitorRestartMessageRunning = True
		End Sub

		Private Sub cmdStopMonitorRestartMessage_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStopMonitorRestartMessage.LinkClicked
			StopMonitorRestartMessage()
			monitorRestartMessageRunning = False
		End Sub

		Private Sub cmdRestartNowOnInitialize_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdRestartNowOnInitialize.LinkClicked
			Try
				For Each process As Process In Process.GetProcessesByName("TrueTest")
					process.Kill()
					Process.Start("TrueTest")
				Next
			Catch ex As Exception

			End Try
		End Sub

		Private Sub cmdStartMonitorOnInitializeMessage_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStartMonitorOnInitializeMessage.LinkClicked
			StartMonitorOnInitializeMessage()
			monitorOnInitializeMessageRunning = True
		End Sub

		Private Sub cmdStopMonitorOnInitializeMessage_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStopMonitorOnInitializeMessage.LinkClicked
			StopMonitorOnInitializeMessage()
			monitorOnInitializeMessageRunning = False
		End Sub

	End Class
End Namespace
