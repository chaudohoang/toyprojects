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
Imports System.Data
Imports System.Linq.Expressions
Imports System.CodeDom

Namespace AutoDeleteData
	Partial Public Class MainForm
		Inherits Form
		Public apppath As String
		Public appdir As String
		Public settingPath As String
		Private allowVisible As Boolean = True
		Public DeleteTask As Tasks.Task
		Public DeleteTaskTasksCancellationTokenSource As New CancellationTokenSource
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
			settingPath = Path.Combine(appdir, "settingAutoDeleteData.txt")
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
			LoadDeleteList()
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
				notifyIcon1.BalloonTipText = "AutoDeleteData still running and minimized to tray"
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


		Private Sub cmdSaveSettings_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs)
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

		Private Sub LoadDeleteList()
			Dim CSVFilePathName = "C:\Radiant Vision Systems Data\TrueTest\UserData\deletelist.csv"
			Dim Lines() As String
			If File.Exists(CSVFilePathName) Then
				Try
					Lines = File.ReadAllLines(CSVFilePathName).Where(Function(arg) Not String.IsNullOrWhiteSpace(arg)).ToArray
				Catch ex As Exception
					MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\deletelist.csv is in use, cannot load saved list")
					Lines = New String() {"D:\Program\RVS\InputLog,3600,3",
						"D:\Program\RVS\UploadQueue\Log,3600,3",
						"D:\Program\RVS\TrueTestWatcherLog,3600,3",
						"D:\POCB\Hex,3600,3",
						"E:\POCB\Hex,3600,3",
						"E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3",
						"E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3",
						"D:\Test,3600,3"}
				End Try

			Else
				Lines = New String() {"D:\Program\RVS\InputLog,3600,3",
						"D:\Program\RVS\UploadQueue\Log,3600,3",
						"D:\Program\RVS\TrueTestWatcherLog,3600,3",
						"D:\POCB\Hex,3600,3",
						"E:\POCB\Hex,3600,3",
						"E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3",
						"E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3",
						"D:\Test,3600,3"}
			End If

			Dim Fields() = "Delete Path, Delete Wait (s), Delete Period (days)".Split(",")
			Dim Cols = Fields.GetLength(0)
			Dim dt = New DataTable
			For index = 0 To Cols - 1
				dt.Columns.Add(Fields(index))
			Next

			Dim Row As DataRow
			For index = 0 To Lines.GetLength(0) - 1
				Fields = Lines(index).Split(",")
				Row = dt.NewRow
				For f = 0 To Cols - 1
					Row(f) = Fields(f)
				Next
				dt.Rows.Add(Row)
			Next
			dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
			dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
			dataGridView1.AllowUserToResizeColumns = True
			If dataGridView1.RowCount = 0 Then
				dataGridView1.DataSource = dt
			End If

		End Sub

		Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
			SaveSettings()
		End Sub

		Private Sub btnDeleteSelectedNow_Click(sender As Object, e As EventArgs) Handles btnDeleteSelectedNow.Click

			If dataGridView1 IsNot Nothing Then
				Dim rowIndex As Integer = dataGridView1.CurrentCell.RowIndex
				Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
				Dim period As Integer = dataGridView1.Rows(rowIndex).Cells(2).Value
				Dim waitTime As New Integer
				If Not Int32.TryParse(dataGridView1.Rows(rowIndex).Cells(1).Value, waitTime) Then
					waitTime = 60
				End If
				Dim logpath As String = "autodeletedatalog" + (rowIndex + 1).ToString + ".txt"
				Dim DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrent(path, period, waitTime, logpath)))
				DeleteTask.Start()
			End If

		End Sub

		Private Sub btnSaveList_Click(sender As Object, e As EventArgs) Handles btnSaveList.Click
			Dim rows = From row As DataGridViewRow In dataGridView1.Rows.Cast(Of DataGridViewRow)()
					   Where Not row.IsNewRow
					   Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))
			Try
				Using sw As New IO.StreamWriter("C:\Radiant Vision Systems Data\TrueTest\UserData\deletelist.csv")
					For Each r In rows
						Dim skip As Boolean = False
						For Each item As String In r
							If String.IsNullOrEmpty(item) Or String.IsNullOrWhiteSpace(item) Then
								skip = True
								Exit For
							End If
						Next
						If skip Then Continue For
						sw.WriteLine(String.Join(",", r))
					Next
				End Using
			Catch ex As Exception
				MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\deletelist.csv is in use, cannot save list")
			End Try

		End Sub
		Private Sub DeleteCurrent(path As String, period As Integer, wait As Integer, logpath As String)

			Try
				File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Deleting " + path + Environment.NewLine)
			Catch ex As Exception

			End Try

			Try
				richTextBox1.Invoke(Sub()
										richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Deleting " + path + Environment.NewLine)
										richTextBox1.Focus()
									End Sub)
			Catch ex As Exception

			End Try

			If Directory.Exists(path) Then
				Dim directory As New IO.DirectoryInfo(path)
				For Each file As IO.FileInfo In directory.GetFiles
					If (Now - file.CreationTime).Days > period Then
						Try
							file.Delete()
							IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + file.FullName + Environment.NewLine)

						Catch ex As Exception
							IO.File.AppendAllText("autodeletedatalog.txt", Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + file.FullName + ", exception : " + ex.Message + Environment.NewLine)
						End Try

					End If
				Next

				If path.Contains("POCB\HEX") Or path.Contains("D:\Test") Then
					Dim subFolders As New List(Of String)
					GetLevel3SubFolders(path, subFolders)

					For Each dir As String In subFolders
						Dim dirInfo As New IO.DirectoryInfo(dir)
						If (Now - dirInfo.CreationTime).Days > period Then
							Try
								dirInfo.Delete(True)
								IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + dirInfo.FullName + Environment.NewLine)
							Catch ex As Exception
								IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + dirInfo.FullName + ", exception : " + ex.Message + Environment.NewLine)

							End Try

						End If
					Next
				Else
					For Each dir As IO.DirectoryInfo In directory.GetDirectories()
						If (Now - dir.CreationTime).Days > period Then
							Try
								dir.Delete(True)
								IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + dir.FullName + Environment.NewLine)
							Catch ex As Exception
								IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + dir.FullName + ", exception : " + ex.Message + Environment.NewLine)

							End Try

						End If
					Next
				End If

			End If

			Try
				File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)

			Catch ex As Exception

			End Try

			Try
				richTextBox1.Invoke(Sub()
										richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)
										richTextBox1.Focus()
									End Sub)
			Catch ex As Exception

			End Try


		End Sub

		Private Sub DeleteCurrentWithWait(path As String, period As Integer, wait As Integer, logpath As String)

			While True
				If DeleteTaskTasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				Dim Deleting As Boolean = False
				If Not Deleting Then

					Deleting = True
					Try
						File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Deleting " + path + Environment.NewLine)
					Catch ex As Exception

					End Try
					Try

					Catch ex As Exception
						richTextBox1.Invoke(Sub()
												richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Deleting " + path + Environment.NewLine)
												richTextBox1.Focus()
											End Sub)
					End Try

					If Directory.Exists(path) Then
						Dim directory As New IO.DirectoryInfo(path)
						For Each file As IO.FileInfo In directory.GetFiles
							If DeleteTaskTasksCancellationTokenSource.IsCancellationRequested Then
								Exit Sub
							End If
							If (Now - file.CreationTime).Days > period Then
								Try
									file.Delete()
									IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + file.FullName + Environment.NewLine)
								Catch ex As Exception
									IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + file.FullName + ", exception : " + ex.Message + Environment.NewLine)
								End Try

							End If
						Next

						If path.Contains("POCB\HEX") Or path.Contains("D:\Test") Then
							If DeleteTaskTasksCancellationTokenSource.IsCancellationRequested Then
								Exit Sub
							End If
							Dim subFolders As New List(Of String)
							GetLevel3SubFolders(path, subFolders)

							For Each dir As String In subFolders
								Dim dirInfo As New IO.DirectoryInfo(dir)
								If (Now - dirInfo.CreationTime).Days > period Then
									Try
										dirInfo.Delete(True)
										IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + dirInfo.FullName + Environment.NewLine)
									Catch ex As Exception
										IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + dirInfo.FullName + ", exception : " + ex.Message + Environment.NewLine)

									End Try

								End If
							Next
						Else
							For Each dir As IO.DirectoryInfo In directory.GetDirectories
								If DeleteTaskTasksCancellationTokenSource.IsCancellationRequested Then
									Exit Sub
								End If
								If (Now - dir.CreationTime).Days > period Then
									Try
										dir.Delete(True)
										IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + dir.FullName + Environment.NewLine)
									Catch ex As Exception
										IO.File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + dir.FullName + ", exception : " + ex.Message + Environment.NewLine)
									End Try

								End If
							Next
						End If

					End If

					Deleting = False
					Try
						File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)
					Catch ex As Exception

					End Try
					Try
						richTextBox1.Invoke(Sub()
												richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)
												richTextBox1.Focus()
											End Sub)
					Catch ex As Exception

					End Try

				Else
					Try
						File.AppendAllText(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Still Deleting " + path + " ,start again after " + wait.ToString + " seconds" + Environment.NewLine)
					Catch ex As Exception

					End Try
					Try
						richTextBox1.Invoke(Sub()
												richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Still Deleting " + path + " ,start again after " + wait.ToString + " seconds" + Environment.NewLine)
												richTextBox1.Focus()
											End Sub)
					Catch ex As Exception

					End Try


				End If

				sw = New Stopwatch
				sw.Start()
				While sw.ElapsedMilliseconds < 1000 * wait
					If DeleteTaskTasksCancellationTokenSource.IsCancellationRequested Then

						Exit Sub
					End If
					System.Threading.Thread.Sleep(100)
				End While
			End While


		End Sub

		Private Sub btnDeleteAllNow_Click(sender As Object, e As EventArgs) Handles btnDeleteAllNow.Click
			If dataGridView1 IsNot Nothing Then

				For i = 0 To dataGridView1.RowCount - 1

					Dim rowIndex As Integer = dataGridView1.Rows(i).Cells(0).RowIndex
					If String.IsNullOrEmpty(dataGridView1.Rows(rowIndex).Cells(0).Value) Then Continue For
					Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
					Dim period As Integer = dataGridView1.Rows(rowIndex).Cells(2).Value
					Dim waitTime As New Integer
					If Not Int32.TryParse(dataGridView1.Rows(rowIndex).Cells(1).Value, waitTime) Then
						waitTime = 60
					End If
					Dim logpath As String = "autodeletedatalog" + (i + 1).ToString + ".txt"
					Dim DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrent(path, period, waitTime, logpath)))
					DeleteTask.Start()
				Next

			End If
		End Sub


		Private Sub cmdStartMonitor_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStartMonitor.LinkClicked
			startMonitor()
		End Sub

		Private Sub cmdStopMonitor_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStopMonitor.LinkClicked
			stopMonitor()
		End Sub

		Private Sub startMonitor()

			If dataGridView1 IsNot Nothing Then
				DeleteTaskTasksCancellationTokenSource = New CancellationTokenSource
				For i = 0 To dataGridView1.RowCount - 1

					Dim rowIndex As Integer = dataGridView1.Rows(i).Cells(0).RowIndex
					If String.IsNullOrEmpty(dataGridView1.Rows(rowIndex).Cells(0).Value) Then Continue For
					Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
					Dim waitTime As New Integer
					If Not Int32.TryParse(dataGridView1.Rows(rowIndex).Cells(1).Value, waitTime) Then
						waitTime = 60
					End If
					Dim period As Integer = dataGridView1.Rows(rowIndex).Cells(2).Value
					Dim logpath As String = "autodeletedatalog" + (i + 1).ToString + ".txt"
					DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentWithWait(path, period, waitTime, logpath)), DeleteTaskTasksCancellationTokenSource.Token)
					DeleteTask.Start()
				Next
				lblMonitoringStatus.Invoke(Sub()
											   lblMonitoringStatus.Text = "Status : Monitoring ..."
										   End Sub)
				cmdStartMonitor.Invoke(Sub()
										   cmdStartMonitor.Enabled = False
									   End Sub)
				cmdStopMonitor.Invoke(Sub()
										  cmdStopMonitor.Enabled = True
									  End Sub)
			End If

		End Sub

		Private Sub stopMonitor()
			If DeleteTaskTasksCancellationTokenSource IsNot Nothing Then
				DeleteTaskTasksCancellationTokenSource.Cancel()
			End If
			If DeleteTask IsNot Nothing Then
				If DeleteTask.Status <> TaskStatus.RanToCompletion Then
					'Wait a little longer
					Dim sw As New Stopwatch
					sw.Start()
					Do Until DeleteTask.Status = TaskStatus.RanToCompletion
						If DeleteTask.Status = TaskStatus.Canceled Then Exit Do
						If DeleteTask.Status = TaskStatus.Faulted Then Exit Do
						If sw.ElapsedMilliseconds > 1000 Then Exit Do
					Loop
					sw.Stop()
				End If
				If DeleteTask.IsCompleted OrElse DeleteTask.IsCanceled OrElse DeleteTask.IsFaulted Then
					DeleteTask.Dispose()
				End If
				DeleteTask = Nothing
			End If
			lblMonitoringStatus.Invoke(Sub()
										   lblMonitoringStatus.Text = "Status : Stopped ..."
									   End Sub)
			cmdStartMonitor.Invoke(Sub()
									   cmdStartMonitor.Enabled = True
								   End Sub)
			cmdStopMonitor.Invoke(Sub()
									  cmdStopMonitor.Enabled = False
								  End Sub)
		End Sub

		Private Sub btnReloadList_Click(sender As Object, e As EventArgs) Handles btnReloadList.Click
			LoadDeleteList()
		End Sub

		Private Sub btnLogClear_Click(sender As Object, e As EventArgs) Handles btnLogClear.Click
			richTextBox1.Clear()
			Dim logList As IEnumerable(Of String) = IO.Directory.EnumerateFiles(appdir, "autodeletedatalog*.txt")

			For Each log As String In logList
				Try
					IO.File.Delete(log)
				Catch ex As Exception

				End Try
			Next

		End Sub

		Private Sub btnVewItemLog_Click(sender As Object, e As EventArgs) Handles btnVewItemLog.Click
			If dataGridView1 IsNot Nothing Then
				Dim rowIndex As Integer = dataGridView1.CurrentCell.RowIndex
				Dim logpath As String = "autodeletedatalog" + (rowIndex + 1).ToString + ".txt"
				Try
					Process.Start(logpath)
				Catch ex As Exception
				End Try

			End If
		End Sub
		Sub GetLevel3SubFolders(ByVal currentPath As String, ByRef subFolders As List(Of String), Optional ByVal currentDepth As Integer = 0)
			' Check if we've reached level 2 depth
			If currentDepth = 3 Then
				' Add the current folder to the list
				subFolders.Add(currentPath)
				Return
			End If

			' Get all subdirectories in the current directory
			Dim subDirs As String() = Directory.GetDirectories(currentPath)

			' Recursively call the function for each subdirectory
			For Each subDir In subDirs
				GetLevel3SubFolders(subDir, subFolders, currentDepth + 1)
			Next
		End Sub

	End Class
End Namespace
