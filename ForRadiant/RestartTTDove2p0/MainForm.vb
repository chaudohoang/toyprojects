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

Namespace RestartTT
	Partial Public Class MainForm
		Inherits Form
		Public apppath As String
		Public appdir As String
		Public settingPath As String
		Public checkLastLineTask As Tasks.Task
		Public monitorTask As Tasks.Task
		Public selfRestartTask As Tasks.Task
		Public checkLastLineTasksCancellationTokenSource As New CancellationTokenSource
		Public monitorTaskTasksCancellationTokenSource As New CancellationTokenSource
		Public RunCount As Integer = 0
		Public offset As Long = 0
		Public checkingLastLine As Boolean = False
		Private allowVisible As Boolean = True
		Public today As String = Now.ToString("yyyyMMdd")
		Public logPath As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\" + today + " Operation Log.txt"
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
			settingPath = Path.Combine(appdir, "setting.txt")
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
			If startMinimizedToolStripMenuItem.Checked = True Then
				Me.WindowState = FormWindowState.Minimized
			End If
			StartMonitor()
			selfRestartTask = New Tasks.Task(New Action(Sub() CheckForSelfRestart()))
			selfRestartTask.Start()
		End Sub
		Private Sub CheckForSelfRestart()
			Dim startTime As DateTime = DateTime.Now
			Dim tomorrow As DateTime

			tomorrow = startTime + New TimeSpan(1, 0, 0, 0, 0)
			Dim year = tomorrow.Year
			Dim month = tomorrow.Month
			Dim day = tomorrow.Day
			Dim endTime = New DateTime(year, month, day, 0, 0, 0)
			Dim duration = endTime - startTime
			Dim durationInSeconds = duration.TotalSeconds
			sw = New Stopwatch
			sw.Start()
			While sw.ElapsedMilliseconds <= durationInSeconds * 1000
				System.Threading.Thread.Sleep(100)
			End While
			Application.Restart()
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

		Private Sub StartMonitor()

			checkLastLineTasksCancellationTokenSource = New CancellationTokenSource
			checkLastLineTask = New Tasks.Task(New Action(Sub() GetLastLine()), checkLastLineTasksCancellationTokenSource.Token)
			checkLastLineTask.Start()
			monitorTaskTasksCancellationTokenSource = New CancellationTokenSource
			monitorTask = New Tasks.Task(New Action(Sub() ReadFromFile()), monitorTaskTasksCancellationTokenSource.Token)
			monitorTask.Start()

		End Sub
		Private Sub ReCheckLastLine()
			If checkLastLineTasksCancellationTokenSource IsNot Nothing Then
				checkLastLineTasksCancellationTokenSource.Cancel()
			End If
			If checkLastLineTask IsNot Nothing Then
				If checkLastLineTask.Status <> TaskStatus.RanToCompletion Then
					'Wait a little longer
					Dim sw As New Stopwatch
					sw.Start()
					Do Until checkLastLineTask.Status = TaskStatus.RanToCompletion
						If checkLastLineTask.Status = TaskStatus.Canceled Then Exit Do
						If checkLastLineTask.Status = TaskStatus.Faulted Then Exit Do
						If sw.ElapsedMilliseconds > 1000 Then Exit Do
					Loop
					sw.Stop()
				End If
				If checkLastLineTask.IsCompleted OrElse checkLastLineTask.IsCanceled OrElse checkLastLineTask.IsFaulted Then
					checkLastLineTask.Dispose()
				End If
				checkLastLineTask = Nothing
			End If
			If chkResetCount.Checked Then
				RunCount = 0
				lblRunCount.Invoke(Sub()
									   lblRunCount.Text = RunCount.ToString
								   End Sub)
			End If
			checkLastLineTasksCancellationTokenSource = New CancellationTokenSource
			checkLastLineTask = New Tasks.Task(New Action(Sub() GetLastLine()), checkLastLineTasksCancellationTokenSource.Token)
			checkLastLineTask.Start()
		End Sub
		Private Sub GetLastLine()
			checkingLastLine = True
			If checkLastLineTasksCancellationTokenSource.IsCancellationRequested Then
				Exit Sub
			End If
			If Not File.Exists(logPath) Then
				File.WriteAllText(logPath, "")
			End If
			Dim fsw As New FileSystemWatcher
			fsw.Path = "C:\Radiant Vision Systems Data\TrueTest\AppData"
			fsw.Filter = "*.*"

			Dim file__1 As FileStream = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)

			Dim reader As New StreamReader(file__1)
			Do
				If checkLastLineTasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				reader.ReadLine()
				If reader.EndOfStream Then
					offset = file__1.Position
					checkingLastLine = False
					Exit Sub
				End If
			Loop While Not reader.EndOfStream
		End Sub
		Private Sub ReadFromFile()
			While checkingLastLine
				If monitorTaskTasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				Thread.Sleep(5)
				If Not checkingLastLine Then
					Exit While
				End If
			End While
			RunCount = 0
			Dim fsw As New FileSystemWatcher
			fsw.Path = "C:\Radiant Vision Systems Data\TrueTest\AppData"
			fsw.Filter = "*.*"
			If monitorTaskTasksCancellationTokenSource.IsCancellationRequested Then
				Exit Sub
			End If
			Dim file__1 As FileStream = File.Open(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)

			Dim reader As New StreamReader(file__1)
			While Not monitorTaskTasksCancellationTokenSource.IsCancellationRequested
				If monitorTaskTasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				fsw.WaitForChanged(WatcherChangeTypes.Changed)

				file__1.Seek(offset, SeekOrigin.Begin)

				If Not reader.EndOfStream Then
					Do
						If monitorTaskTasksCancellationTokenSource.IsCancellationRequested Then
							Exit Sub
						End If
						Dim checkString As String = reader.ReadLine()
						If checkString.Contains("Sent : RESULT") Then
							RunCount += 1
							lblRunCount.Invoke(Sub()
												   lblRunCount.Text = RunCount.ToString
											   End Sub)
						End If
						If RunCount.ToString = txtRunCount.Text Then
							Try
								Thread.Sleep(3000)
								RunCount = 0
								lblRunCount.Invoke(Sub()
													   lblRunCount.Text = RunCount.ToString
												   End Sub)
								For Each process As Process In Process.GetProcessesByName("TrueTest")
									process.Kill()
									Process.Start("TrueTest")
								Next
							Catch ex As Exception

							End Try
						End If

					Loop While Not reader.EndOfStream

					offset = file__1.Position

				End If
			End While
		End Sub
		Private Sub StopMonitor()

			If checkLastLineTasksCancellationTokenSource IsNot Nothing Then
				checkLastLineTasksCancellationTokenSource.Cancel()
			End If
			If monitorTaskTasksCancellationTokenSource IsNot Nothing Then
				monitorTaskTasksCancellationTokenSource.Cancel()
			End If
			If checkLastLineTask IsNot Nothing Then
				If checkLastLineTask.Status <> TaskStatus.RanToCompletion Then
					'Wait a little longer
					Dim sw As New Stopwatch
					sw.Start()
					Do Until checkLastLineTask.Status = TaskStatus.RanToCompletion
						If checkLastLineTask.Status = TaskStatus.Canceled Then Exit Do
						If checkLastLineTask.Status = TaskStatus.Faulted Then Exit Do
						If sw.ElapsedMilliseconds > 1000 Then Exit Do
					Loop
					sw.Stop()
				End If
				If checkLastLineTask.IsCompleted OrElse checkLastLineTask.IsCanceled OrElse checkLastLineTask.IsFaulted Then
					checkLastLineTask.Dispose()
				End If
				checkLastLineTask = Nothing
			End If

			If monitorTask IsNot Nothing Then
				If monitorTask.Status <> TaskStatus.RanToCompletion Then
					'Wait a little longer
					Dim sw As New Stopwatch
					sw.Start()
					Do Until monitorTask.Status = TaskStatus.RanToCompletion
						If monitorTask.Status = TaskStatus.Canceled Then Exit Do
						If monitorTask.Status = TaskStatus.Faulted Then Exit Do
						If sw.ElapsedMilliseconds > 1000 Then Exit Do
					Loop
					sw.Stop()
				End If
				If monitorTask.IsCompleted OrElse monitorTask.IsCanceled OrElse monitorTask.IsFaulted Then
					monitorTask.Dispose()
				End If
				monitorTask = Nothing
			End If

		End Sub

		Private Sub cmdSaveSettings_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdSaveSettings.LinkClicked
			SaveSettings()
			ReCheckLastLine()
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
			settings.Add("waitsec", txtWait.Text)
			If chkResetCount.Checked = True Then
				settings.Add("resetcountonsave", "true")
			Else
				settings.Add("resetcountonsave", "false")
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
						ElseIf setting = "numruns" Then
							txtRunCount.Text = value
						ElseIf setting = "waitsec" Then
							txtWait.Text = value
						ElseIf setting = "resetcountonsave" Then
							chkResetCount.Checked = If(value = "true", True, False)
						End If
					Next
				End If
			Catch ex As Exception

			End Try
		End Sub

		Private Sub cmdStopUpload_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
			StopMonitor()

		End Sub

	End Class
End Namespace
