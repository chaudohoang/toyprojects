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
			BuildStatusStrip()
		End Sub

		' ===================================================================
		' Extra controls, built in code so the designer file is untouched.
		'
		' A strip along the bottom of the form: FTP server reachability, how many
		' files share one session, when to give up on a stalled transfer, and which
		' server to send to.
		' ===================================================================
		Private lblPingA As Label
		Private lblPingB As Label
		Private lblSpeed As Label
		Private cboPerSession As ComboBox
		Private cboStall As ComboBox
		Private cboHost As ComboBox
		Private pingTimer As System.Windows.Forms.Timer
		Private speedTimer As System.Windows.Forms.Timer
		Private pingBusy As Boolean = False
		Private ReadOnly PingHosts As String() = {"10.119.211.173", "10.119.211.174"}

		' Live throughput. Counted here rather than derived from the logs so the
		' figure is available while a run is in progress.
		Private UploadedFiles As Long = 0
		Private UploadedBytes As Long = 0
		Private UploadClock As Stopwatch = Nothing

		Private Sub BuildStatusStrip()
			Dim strip As New Panel With {
				.Dock = DockStyle.Bottom,
				.Height = 30,
				.Padding = New Padding(6, 4, 6, 4)
			}

			lblPingA = New Label With {.AutoSize = True, .Location = New Point(6, 7),
									   .ForeColor = Color.Gray, .Text = PingHosts(0) & " ..."}
			lblPingB = New Label With {.AutoSize = True, .Location = New Point(170, 7),
									   .ForeColor = Color.Gray, .Text = PingHosts(1) & " ..."}

			Dim lblSend As New Label With {.AutoSize = True, .Location = New Point(340, 7), .Text = "Send to:"}
			cboHost = New ComboBox With {.Location = New Point(395, 3), .Width = 150,
										 .DropDownStyle = ComboBoxStyle.DropDownList}
			cboHost.Items.AddRange(New Object() {"Auto (from queue)", PingHosts(0), PingHosts(1)})
			cboHost.SelectedIndex = 0

			Dim lblPer As New Label With {.AutoSize = True, .Location = New Point(560, 7), .Text = "Files/session:"}
			cboPerSession = New ComboBox With {.Location = New Point(645, 3), .Width = 85,
											   .DropDownStyle = ComboBoxStyle.DropDownList}
			cboPerSession.Items.AddRange(New Object() {"50", "100", "200", "500", "No limit"})
			cboPerSession.SelectedIndex = 1                 ' 100

			Dim lblStall As New Label With {.AutoSize = True, .Location = New Point(745, 7), .Text = "Stall:"}
			cboStall = New ComboBox With {.Location = New Point(785, 3), .Width = 85,
										  .DropDownStyle = ComboBoxStyle.DropDownList}
			cboStall.Items.AddRange(New Object() {"30s", "60s", "90s", "180s", "No limit"})
			cboStall.SelectedIndex = 0                      ' 30s

			lblSpeed = New Label With {.AutoSize = True, .Location = New Point(880, 7),
									   .ForeColor = Color.Gray, .Text = "idle"}

			strip.Controls.AddRange(New Control() {lblPingA, lblPingB, lblSend, cboHost,
												   lblPer, cboPerSession, lblStall, cboStall,
												   lblSpeed})
			Controls.Add(strip)

			' ICMP, not a connect to port 21 - a status light must not add
			' connections to the FTP server it is reporting on.
			pingTimer = New System.Windows.Forms.Timer With {.Interval = 30000}
			AddHandler pingTimer.Tick, AddressOf PingTick
			pingTimer.Start()
			PingTick(Nothing, Nothing)

			speedTimer = New System.Windows.Forms.Timer With {.Interval = 1000}
			AddHandler speedTimer.Tick, AddressOf SpeedTick
			speedTimer.Start()
		End Sub

		' files/s says how fast the queue is draining; MB/s says whether the link or
		' the per-file overhead is the limit. A rate that collapses is usually the
		' first sign something is stuck.
		Private Sub SpeedTick(sender As Object, e As EventArgs)
			If lblSpeed Is Nothing Then Exit Sub
			If UploadClock Is Nothing OrElse UploadedFiles = 0 Then
				lblSpeed.Text = "idle"
				lblSpeed.ForeColor = Color.Gray
				Exit Sub
			End If
			Dim secs = Math.Max(1.0R, UploadClock.Elapsed.TotalSeconds)
			Dim fps = UploadedFiles / secs
			Dim mbps = (UploadedBytes / 1048576.0R) / secs
			Dim cap = FilesPerSessionSetting()
			' Session number plus how far through its file budget it is: the answer
			' to "are we opening too many sessions?" at a glance.
			lblSpeed.Text = "session #" & SessionNumber.ToString() & " (" &
							FilesThisSession.ToString() &
							If(cap > 0, "/" & cap.ToString(), "") & ")   " &
							fps.ToString("0.0") & " files/s   " &
							mbps.ToString("0.00") & " MB/s"
			lblSpeed.ForeColor = Color.Black
		End Sub

		Private Sub PingTick(sender As Object, e As EventArgs)
			If pingBusy Then Exit Sub
			pingBusy = True
			Task.Run(Sub()
						 For i = 0 To PingHosts.Length - 1
							 Dim idx = i
							 Dim text As String
							 Dim colour As Color
							 Try
								 Using p As New Net.NetworkInformation.Ping()
									 Dim r = p.Send(PingHosts(idx), 1500)
									 If r IsNot Nothing AndAlso r.Status = Net.NetworkInformation.IPStatus.Success Then
										 text = PingHosts(idx) & "  " & r.RoundtripTime.ToString() & " ms"
										 colour = If(r.RoundtripTime > 200, Color.DarkOrange, Color.Green)
									 Else
										 text = PingHosts(idx) & "  no reply"
										 colour = Color.Red
									 End If
								 End Using
							 Catch
								 text = PingHosts(idx) & "  unreachable"
								 colour = Color.Red
							 End Try
							 Dim lbl = If(idx = 0, lblPingA, lblPingB)
							 Try
								 lbl.Invoke(Sub()
												lbl.Text = text
												lbl.ForeColor = colour
											End Sub)
							 Catch
							 End Try
						 Next
					 End Sub).ContinueWith(Sub() pingBusy = False)
		End Sub

		' Chosen server, or the one from the queue file when set to Auto.
		Private Function EffectiveHost(queueHost As String) As String
			Try
				If cboHost IsNot Nothing AndAlso cboHost.SelectedIndex > 0 Then
					Return Convert.ToString(cboHost.SelectedItem)
				End If
			Catch
			End Try
			Return queueHost
		End Function

		Private Function FilesPerSessionSetting() As Integer
			Try
				Dim n As Integer
				If cboPerSession IsNot Nothing AndAlso
				   Integer.TryParse(Convert.ToString(cboPerSession.SelectedItem), n) Then
					Return n
				End If
			Catch
			End Try
			Return 0                                        ' "No limit"
		End Function

		Private Function StallSecondsSetting() As Integer
			Try
				Dim n As Integer
				If cboStall IsNot Nothing AndAlso
				   Integer.TryParse(Convert.ToString(cboStall.SelectedItem).Replace("s", ""), n) Then
					Return n
				End If
			Catch
			End Try
			Return 0                                        ' "No limit"
		End Function

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
		' ===================================================================
		' Shared FTP session
		'
		' Previously every file opened its own WinSCP session: connect, login,
		' handshake, STOR, quit - about two logins per file once retries are
		' counted. That is why an HDD machine could never keep up while an SSD
		' machine managed 99%: most of the time went on connection setup, not
		' transferring. The customer also asked us to stop opening so many
		' sessions, since a burst of logins gets queued and then refused.
		'
		' One session is now reused across files and recycled every
		' FILES_PER_SESSION uploads.
		' ===================================================================
		Private CurSession As Session = Nothing
		Private CurHost As String = ""
		Private CurUser As String = ""
		Private FilesThisSession As Integer = 0
		Private SessionNumber As Integer = 0
		Private LastSessionUse As DateTime = DateTime.MinValue
		' A session left idle longer than this is replaced rather than used. The
		' server or a firewall will have dropped it, and discovering that on the
		' next upload costs a failed file.
		Private Const SESSION_IDLE_SECONDS As Integer = 120
		Private Const FILES_PER_SESSION As Integer = 100

		Private Function GetSession(host As String, username As String, password As String,
									exePath As String, sessionLogPath As String) As Session
			Dim useHost = EffectiveHost(host)
			Dim perSession = FilesPerSessionSetting()
			If CurSession IsNot Nothing AndAlso perSession > 0 AndAlso FilesThisSession >= perSession Then
				CloseSession()
			End If
			If CurSession IsNot Nothing AndAlso LastSessionUse <> DateTime.MinValue _
			   AndAlso DateTime.Now.Subtract(LastSessionUse).TotalSeconds > SESSION_IDLE_SECONDS Then
				CloseSession()
			End If
			If CurSession IsNot Nothing AndAlso CurSession.Opened _
			   AndAlso String.Equals(CurHost, useHost, StringComparison.OrdinalIgnoreCase) _
			   AndAlso String.Equals(CurUser, username, StringComparison.OrdinalIgnoreCase) Then
				LastSessionUse = DateTime.Now
				Return CurSession
			End If

			CloseSession()

			Dim sessionOptions As New SessionOptions
			With sessionOptions
				.Protocol = Protocol.Ftp
				.HostName = useHost
				.UserName = username
				.Password = password
				.TimeoutInMilliseconds = 20000
			End With

			Dim s As New Session
			s.ExecutablePath = exePath
			Try
				If Not Directory.Exists(Path.GetDirectoryName(sessionLogPath)) Then
					Directory.CreateDirectory(Path.GetDirectoryName(sessionLogPath))
				End If
				s.SessionLogPath = sessionLogPath
			Catch
			End Try
			s.Open(sessionOptions)

			CurSession = s
			CurHost = useHost
			CurUser = username
			FilesThisSession = 0
			SessionNumber += 1
			LastSessionUse = DateTime.Now
			LogSession("session #" & SessionNumber.ToString() & " opened to " & useHost &
					   " as " & username &
					   If(perSession > 0, "  (limit " & perSession.ToString() & " file(s))",
										  "  (no limit)"))
			Return s
		End Function

		' One line per session, so afterwards you can see how many files each login
		' actually carried - the number the customer cares about.
		Private Sub LogSession(text As String)
			Try
				' Uploading runs on a background task, so the textbox cannot be read
				' directly from here.
				Dim root As String = ""
				If txtUploadListPath.InvokeRequired Then
					txtUploadListPath.Invoke(Sub() root = txtUploadListPath.Text)
				Else
					root = txtUploadListPath.Text
				End If
				If root = "" Then Exit Sub

				Dim dir = Path.Combine(root, "Log")
				If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)
				Dim f = Path.Combine(dir, Now.ToString("yyyyMMdd") & "_session.log")
				File.AppendAllText(f, Now.ToString("HH:mm:ss.fff") & vbTab & text &
									  System.Environment.NewLine)
			Catch
			End Try
		End Sub

		Private Sub CloseSession()
			If CurSession IsNot Nothing Then
				LogSession("session #" & SessionNumber.ToString() & " closed after " &
						   FilesThisSession.ToString() & " file(s) to " & CurHost)
				Try
					CurSession.Dispose()
				Catch
				End Try
				CurSession = Nothing
				CurHost = ""
				CurUser = ""
				FilesThisSession = 0
			End If
		End Sub

		' Errors that will give the same answer however many times we try.
		Private Function IsPermanentError(msg As String) As Boolean
			If msg Is Nothing Then Return False
			Dim m = msg.ToLower()
			For Each s In New String() {"does not exist", "cannot find the file",
										"cannot find the path", "no such file",
										"permission denied", "access is denied",
										"550"}
				If m.Contains(s) Then Return True
			Next
			Return False
		End Function

		' Does the error mean the connection died rather than this file being bad?
		' It matters: during an outage EVERY file fails, and treating those as file
		' failures marks good images failed and deletes their queue files. That is
		' how panels ended up with nothing left to send.
		Private Function LooksLikeConnectionError(msg As String) As Boolean
			If msg Is Nothing Then Return False
			Dim m = msg.ToLower()
			For Each s In New String() {"connection", "timed out", "timeout", "network",
										"refused", "unreachable", "closed", "reset",
										"lost", "disconnect", "not logged in", "421"}
				If m.Contains(s) Then Return True
			Next
			Return False
		End Function

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

				' A missing source file will not appear by trying again - retrying it
				' five times just burns five seconds per file. Fail it once, now.
				If Not File.Exists(sourceFile) Then
					Throw New Exception("Source file missing on disk: " & sourceFile)
				End If

				Dim fileBytes As Long = 0
				Try
					fileBytes = New FileInfo(sourceFile).Length
				Catch
				End Try

				' ---------------------------------------------------------------
				' All retries for THIS file happen here, before moving to the next
				' one. Previously a failure just moved on and the file waited for a
				' later cycle - and because work was picked newest-first, a file
				' that fell outside the window was never looked at again. That is
				' what let a backlog build up and never drain.
				' ---------------------------------------------------------------
				Dim maxAttempts As Integer = 0
				If Not Int32.TryParse(txtMaximumFailRetry.Text, maxAttempts) Then
					maxAttempts = 3
				End If
				If maxAttempts < 1 Then maxAttempts = 1

				Dim lastError As String = ""
				Dim connectionProblem As Boolean = False

				For attempt = 1 To maxAttempts
					If TasksCancellationTokenSource.IsCancellationRequested Then
						Exit Sub
					End If
					Dim openingSession As Boolean = True
					Dim stalled As Boolean = False
					Dim watchdog As System.Threading.Timer = Nothing
					Dim activeSession As Session = Nothing
					Try
						Dim session = GetSession(host, username, password, exePath, sessionLogPath)
						openingSession = False
						activeSession = session

						' When a server disappears mid-transfer there is no reset -
						' the socket just goes quiet, and Windows can sit on it for
						' minutes. Session.Abort() is the supported way to break out.
						Dim stallSecs = StallSecondsSetting()
						If stallSecs > 0 Then
							watchdog = New System.Threading.Timer(
								Sub(o)
									If activeSession IsNot Nothing AndAlso Not stalled Then
										stalled = True
										Try
											activeSession.Abort()
										Catch
										End Try
									End If
								End Sub, Nothing, stallSecs * 1000, System.Threading.Timeout.Infinite)
						End If

						Dim transferOptions As New TransferOptions
						transferOptions.TransferMode = TransferMode.Binary

						Dim transferResult As TransferOperationResult
						transferResult = session.PutFiles(sourceFile, destFile, False, transferOptions)

						' Throw on any error
						transferResult.Check()

						FilesThisSession += 1
						' Clock starts at the first successful upload, so idle time
						' between cycles does not drag the average down.
						If UploadClock Is Nothing Then UploadClock = Stopwatch.StartNew()
						UploadedFiles += 1
						UploadedBytes += fileBytes
						uploaded = True
						connectionProblem = False
						Exit For
					Catch exUp As Exception
						lastError = exUp.Message
						connectionProblem = openingSession OrElse LooksLikeConnectionError(lastError)
						If stalled Then
							' We aborted it because the server stopped responding.
							' That is the connection, not the file.
							connectionProblem = True
							lastError = "no response for " & StallSecondsSetting().ToString() &
										"s, transfer aborted"
						End If

						logContent = Me.Text + vbTab + Now.ToString("HH:mm:ss.fff") + vbTab +
									 "Attempt " + attempt.ToString + "/" + maxAttempts.ToString +
									 " failed : " + lastError + " " + sourceFile +
									 System.Environment.NewLine
						File.AppendAllText(failLogPath, logContent)

						' Only throw the session away when the connection itself is
						' suspect. Reconnecting after a file-level error just adds
						' another login for the server to queue.
						If connectionProblem Then CloseSession()

						' Some failures cannot be fixed by trying again - a missing
						' file, a refused permission, a dest folder that does not
						' exist. Stop early instead of repeating the same error.
						If Not connectionProblem AndAlso IsPermanentError(lastError) Then
							Exit For
						End If
						If attempt < maxAttempts Then System.Threading.Thread.Sleep(1000)
					Finally
						If watchdog IsNot Nothing Then watchdog.Dispose()
					End Try
				Next

				If Not uploaded Then
					Throw New Exception(If(connectionProblem, "CONNECTION: ", "") & lastError)
				End If
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
				Dim needTotal As Integer = 0
				' >= not = : if the count ever drifts past the total (a duplicate
				' line, a restored backup) an exact match never happens again and
				' the panel can never complete.
				If Int32.TryParse(totalFileCount, needTotal) AndAlso needTotal > 0 _
				   AndAlso uploadedCount >= needTotal Then
					CreateIndexAndHostQueue(InfoFile)
				End If

			Catch e As Exception
				lblFileUploadStatus.Invoke(Sub()
											   lblFileUploadStatus.Text = "Failed "
										   End Sub)

				logContent = Me.Text + vbTab + Now.ToString("HH:mm:ss.fff") + vbTab + "Upload failed with exception : " + e.Message + sourceFile + " to: " + "ftp://" + host + destFile + System.Environment.NewLine
				File.AppendAllText(failLogPath, logContent)

				' The server was unreachable, so nothing is wrong with this file.
				' Leave the queue file alone, write no " - failed" marker and do not
				' count it against the retry budget: an outage would otherwise burn
				' through every panel and mark thousands of good images as failed,
				' leaving the customer with manifests missing files that were never
				' actually attempted.
				If e.Message.StartsWith("CONNECTION: ") Then
					logContent = Me.Text + vbTab + Now.ToString("HH:mm:ss.fff") + vbTab +
								 "Server unreachable - queue kept for a later cycle: " + sourceFile +
								 System.Environment.NewLine
					File.AppendAllText(failLogPath, logContent)
					Exit Sub
				End If

				' All retries for this file have already been used, above. There is
				' no longer a fail count spread across cycles - a file that has
				' failed every attempt with the server reachable is a bad file, so
				' it is marked now rather than being picked up again later.
				If TasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If

				' Append to index and host files with "- failed"
				File.AppendAllText(sourceIndexFile, destFile + "@" + channelIndex + " - failed" + System.Environment.NewLine)
				File.AppendAllText(sourceHostFile, destFile + "@" + channelIndex + " - failed" + System.Environment.NewLine)

				logContent = Me.Text + vbTab + Now.ToString("HH:mm:ss.fff") + vbTab + "All retries used, deleting queue: " + sourceFile + " to: " + "ftp://" + host + destFile + System.Environment.NewLine
				File.AppendAllText(failLogPath, logContent)

				' The panel may have just reached its total on THIS line. The check
				' only existed in the success path, so if the last file of a panel
				' failed, the index and host were never sent and the panel stayed
				' stuck for good. Checked here too, and with >= rather than =, so a
				' count that has drifted past the total still completes.
				Dim failedCount As Integer = File.ReadAllLines(sourceHostFile).Length
				Dim wantTotal As Integer = 0
				If Int32.TryParse(totalFileCount, wantTotal) AndAlso wantTotal > 0 _
				   AndAlso failedCount >= wantTotal Then
					CreateIndexAndHostQueue(InfoFile)
				End If

				' Backup before deleting - FAILED
				If backupQueueAfterUploadToolStripMenuItem.Checked Then
					BackupInfoFile(InfoFile, "Backedup Failed Queue")
				End If
				File.Delete(InfoFile)
				If File.Exists(failCountPath) Then
					File.Delete(failCountPath)
				End If
				UpdateSummaryLogFail(summaryLogPath, PID, destFile, e.Message)

			End Try
			If uploaded Then
				' Backup before deleting - SUCCEEDED
				If backupQueueAfterUploadToolStripMenuItem.Checked Then
					BackupInfoFile(InfoFile, "Backedup Succeed Queue")
				End If
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

			Dim failCountPath = txtUploadListPath.Text + "\Fail Count\IndexHost\" + Path.GetFileName(InfoFile)

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

				' Same shared session as the data files - no separate login just to
				' send the index or the host file.
				Dim session = GetSession(host, username, password, exePath, sessionLogPath)
				Dim transferOptions As New TransferOptions
				transferOptions.TransferMode = TransferMode.Binary
				Dim transferResult As TransferOperationResult
				transferResult = session.PutFiles(sourceFile, destFile, False, transferOptions)
				' Throw on any error
				transferResult.Check()
				FilesThisSession += 1
				If UploadClock Is Nothing Then UploadClock = Stopwatch.StartNew()
				UploadedFiles += 1
				Try
					UploadedBytes += New FileInfo(sourceFile).Length
				Catch
				End Try
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

				' Fail count mechanism
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
					' Log when max fail count reached
					logContent = Now.ToString("HH:mm:ss.fff") + vbTab + "Maximum fail count reached (" + failCount.ToString + "/" + failRetry.ToString + "), deleting queue: " + sourceFile + " to: " + "ftp://" + host + destFile + System.Environment.NewLine
					File.AppendAllText(failLogPath, logContent)

					' Backup before deleting - FAILED
					If backupQueueAfterUploadToolStripMenuItem.Checked Then
						BackupInfoFile(InfoFile, "Backedup Failed Queue\IndexHost")
					End If
					File.Delete(InfoFile)
					File.Delete(failCountPath)
				End If
				Exit Sub ' Don't delete if not reached max retry
			End Try

			If uploaded Then
				' Backup before deleting - SUCCEEDED
				If backupQueueAfterUploadToolStripMenuItem.Checked Then
					BackupInfoFile(InfoFile, "Backedup Succeed Queue\IndexHost")
				End If
				File.Delete(InfoFile)
				If File.Exists(failCountPath) Then
					File.Delete(failCountPath)
				End If
			End If
		End Sub

		Private Sub CreateIndexAndHostQueue(InfoFile As String)
			If Not File.Exists(InfoFile) Then
				Exit Sub
			End If
			Dim lines = File.ReadAllLines(InfoFile)
			Dim OutputIndexInfoFile = lines(9)
			Dim OutputHostInfoFile = lines(12)
			Dim sourceIndexFile = lines(10)
			Dim sourceHostFile = lines(13)

			If TasksCancellationTokenSource.IsCancellationRequested Then
				Exit Sub
			End If

			' Clean the source files by removing lines with " - failed" before uploading
			RemoveFailedLines(sourceIndexFile)
			RemoveFailedLines(sourceHostFile)

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
			' IndexOf returns -1 when the message has no full stop, and Substring
			' then throws - from inside a Catch block, which aborted the rest of
			' the batch for that cycle.
			If spacePos > 0 Then
				failMessage = failMessage.Substring(0, spacePos)
			End If

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
					' Deliberately NOT closing the session here. This runs at the end
					' of every polling cycle, and a cycle may carry only a handful of
					' files - closing here meant one login per cycle, which is what
					' the shared session was supposed to avoid. The session is now
					' kept across cycles and closed only when it hits the file limit,
					' when the connection breaks, or after being idle (see GetSession).
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
			' The session is kept across cycles now, so it has to be closed here -
			' otherwise a login stays open on the server after Stop.
			CloseSession()
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


		Private Sub RemoveFailedLines(filePath As String)
			If Not File.Exists(filePath) Then
				Exit Sub
			End If

			Dim allLines = File.ReadAllLines(filePath)
			Dim cleanedLines = allLines.Where(Function(line) Not line.Contains(" - failed")).ToArray()

			' Only write back if there were changes
			If cleanedLines.Length <> allLines.Length Then
				File.WriteAllLines(filePath, cleanedLines)
			End If
		End Sub

		Private Sub BackupInfoFile(infoFilePath As String, backupFolder As String)
			If Not File.Exists(infoFilePath) Then
				Exit Sub
			End If

			Try
				Dim backupPath = Path.Combine(txtUploadListPath.Text, backupFolder)
				If Not Directory.Exists(backupPath) Then
					Directory.CreateDirectory(backupPath)
				End If

				Dim backupFileName = Now.ToString("yyyyMMdd_HHmmss_fff") + "_" + Path.GetFileName(infoFilePath)
				Dim fullBackupPath = Path.Combine(backupPath, backupFileName)
				File.Copy(infoFilePath, fullBackupPath, True)
			Catch ex As Exception
				' Log backup failure but don't stop the process
				Debug.WriteLine("Backup failed: " + ex.Message)
			End Try
		End Sub

	End Class
End Namespace
