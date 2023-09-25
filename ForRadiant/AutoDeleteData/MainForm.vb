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
		Public CheckDiskTask As Tasks.Task
		Public DeleteTaskTasksCancellationTokenSource As New CancellationTokenSource
		Public CheckDiskTaskTasksCancellationTokenSource As New CancellationTokenSource
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
						"F:\POCB\Hex,3600,3",
						"G:\POCB\Hex,3600,3",
						"D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3",
						"D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3",
						"E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3",
						"E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3",
						"D:\Test,3600,3"}
				End Try

			Else
				Lines = New String() {"D:\Program\RVS\InputLog,3600,3",
						"D:\Program\RVS\UploadQueue\Log,3600,3",
						"D:\Program\RVS\TrueTestWatcherLog,3600,3",
						"D:\POCB\Hex,3600,3",
						"E:\POCB\Hex,3600,3",
						"F:\POCB\Hex,3600,3",
						"G:\POCB\Hex,3600,3",
						"D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3",
						"D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3",
						"E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3",
						"E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3",
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

			Dim CSVFilePathName2 = "C:\Radiant Vision Systems Data\TrueTest\UserData\deletelist2.csv"
			Dim Lines2() As String
			If File.Exists(CSVFilePathName2) Then
				Try
					Lines2 = File.ReadAllLines(CSVFilePathName2).Where(Function(arg) Not String.IsNullOrWhiteSpace(arg)).ToArray
				Catch ex As Exception
					MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\deletelist2.csv is in use, cannot load saved list")
					Lines2 = New String() {"C,60,30,20,20",
						"D,60,30,20,20",
						"E,60,30,20,20",
						"F,60,30,20,20",
						"G,60,30,20,20",
						"H,60,30,20,20"
						}
				End Try

			Else
				Lines2 = New String() {"C,60,30,20,20",
						"D,60,30,20,20",
						"E,60,30,20,20",
						"F,60,30,20,20",
						"G,60,30,20,20",
						"H,60,30,20,20"
						}
			End If

			Dim Fields2() = "Disk drive, Check Wait (s), Minimum space to start delete (GB), Maximum files to delete, Maximum folders to delete".Split(",")
			Dim Cols2 = Fields2.GetLength(0)
			Dim dt2 = New DataTable
			For index = 0 To Cols2 - 1
				dt2.Columns.Add(Fields2(index))
			Next

			Dim Row2 As DataRow
			For index = 0 To Lines2.GetLength(0) - 1
				Fields2 = Lines2(index).Split(",")
				Row2 = dt2.NewRow
				For f = 0 To Cols2 - 1
					Row2(f) = Fields2(f)
				Next
				dt2.Rows.Add(Row2)
			Next
			dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
			dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
			dataGridView2.AllowUserToResizeColumns = True
			If dataGridView2.RowCount = 0 Then
				dataGridView2.DataSource = dt2
			End If

		End Sub

		Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
			SaveSettings()
		End Sub

		Private Sub DeleteCurrentWithMaximum(path As String, maximumFilesToDelete As Integer, maximumFoldersToDelete As Integer, ByRef filesDeleted As Integer, ByRef foldersDeleted As Integer, logpath As String)
			Dim deletedSomeFiles As Boolean
			Dim deletedSomeFolders As Boolean

			WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Deleting " + path + Environment.NewLine)


			Try
				richTextBox1.Invoke(Sub()
										richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Deleting " + path + Environment.NewLine)
										richTextBox1.Focus()
									End Sub)
			Catch ex As Exception

			End Try

			If Directory.Exists(path) Then
				Dim directory As New IO.DirectoryInfo(path)
				Dim files() As FileInfo = directory.GetFiles().OrderByDescending(Function(f) f.CreationTime).ToArray

				For Each file As IO.FileInfo In files
					If filesDeleted >= maximumFilesToDelete Then
						WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Maximum Files Deleted, Exit Deleting Files" + Environment.NewLine)
						Exit For
					End If
					Try
						file.Delete()
						deletedSomeFiles = True
						filesDeleted += 1
						WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + file.FullName + Environment.NewLine)
						WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Files Deleted = " + filesDeleted.ToString + Environment.NewLine)
					Catch ex As Exception
						WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + file.FullName + ", exception : " + ex.Message + Environment.NewLine)
					End Try

				Next
				If Not deletedSomeFiles Then
					WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "No Files deleted in " + path + Environment.NewLine)
				End If

				If path.ToUpper.Contains("POCB\HEX") Or path.Contains("D:\Test") Then
					Dim subFolders As New List(Of String)
					GetLevel3SubFolders(path, subFolders)

					For Each dir As String In subFolders
						If foldersDeleted >= maximumFoldersToDelete Then
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Maximum Folders Deleted, Exit Deleting Folders" + Environment.NewLine)
							Exit For
						End If
						Dim dirInfo As New IO.DirectoryInfo(dir)

						Try
							dirInfo.Delete(True)
							deletedSomeFolders = True
							foldersDeleted += 1
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + dirInfo.FullName + Environment.NewLine)
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Folders Deleted = " + foldersDeleted.ToString + Environment.NewLine)
						Catch ex As Exception
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + dirInfo.FullName + ", exception : " + ex.Message + Environment.NewLine)

						End Try

					Next
				Else
					For Each dir As IO.DirectoryInfo In directory.GetDirectories().OrderByDescending(Function(d) d.CreationTime).ToArray.Take(maximumFoldersToDelete)
						If foldersDeleted >= maximumFoldersToDelete Then
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Maximum Folders Deleted, Exit Deleting Folders" + Environment.NewLine)
							Exit For
						End If
						Try
							dir.Delete(True)
							foldersDeleted += 1
							deletedSomeFolders = True
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + dir.FullName + Environment.NewLine)
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Folders Deleted = " + foldersDeleted.ToString + Environment.NewLine)
						Catch ex As Exception
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + dir.FullName + ", exception : " + ex.Message + Environment.NewLine)

						End Try

					Next
				End If
				If Not deletedSomeFolders Then
					WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "No Folders deleted in " + path + Environment.NewLine)
				End If
			End If

			WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)

			Try
				richTextBox1.Invoke(Sub()
										richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)
										richTextBox1.Focus()
									End Sub)
			Catch ex As Exception

			End Try


		End Sub

		Private Sub DeleteCurrentAtPeriod(path As String, period As Integer, logpath As String)

			WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Deleting " + path + Environment.NewLine)

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
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + file.FullName + Environment.NewLine)

						Catch ex As Exception
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + file.FullName + ", exception : " + ex.Message + Environment.NewLine)
						End Try

					End If
				Next

				If path.ToUpper.Contains("POCB\HEX") Or path.Contains("D:\Test") Then
					Dim subFolders As New List(Of String)
					GetLevel3SubFolders(path, subFolders)

					For Each dir As String In subFolders
						Dim dirInfo As New IO.DirectoryInfo(dir)
						If (Now - dirInfo.CreationTime).Days > period Then
							Try
								dirInfo.Delete(True)
								WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + dirInfo.FullName + Environment.NewLine)
							Catch ex As Exception
								WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + dirInfo.FullName + ", exception : " + ex.Message + Environment.NewLine)

							End Try

						End If
					Next
				Else
					For Each dir As IO.DirectoryInfo In directory.GetDirectories()
						If (Now - dir.CreationTime).Days > period Then
							Try
								dir.Delete(True)
								WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + dir.FullName + Environment.NewLine)
							Catch ex As Exception
								WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + dir.FullName + ", exception : " + ex.Message + Environment.NewLine)

							End Try

						End If
					Next
				End If

			End If


			WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)

			Try
				richTextBox1.Invoke(Sub()
										richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)
										richTextBox1.Focus()
									End Sub)
			Catch ex As Exception

			End Try


		End Sub

		Private Sub CheckDiskSpace(driveLetter As String, minimumGB As Double, maximumFilesToDelete As Integer, maximumFoldersToDelete As Integer, logpath As String)
			Dim filesDeleted As Integer = 0
			Dim foldersDeleted As Integer = 0

			WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Checking " + driveLetter + Environment.NewLine)


			Try
				richTextBox1.Invoke(Sub()
										richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Checking " + driveLetter + Environment.NewLine)
										richTextBox1.Focus()
									End Sub)
			Catch ex As Exception

			End Try

			Try
				Dim driveInfo As New DriveInfo(driveLetter)

				If driveInfo.IsReady Then
					Dim availableSpaceInBytes As Long = driveInfo.AvailableFreeSpace
					Dim availableSpaceInMB As Double = CDbl(availableSpaceInBytes) / (1024 * 1024)
					Dim availableSpaceInGB As Double = availableSpaceInMB / 1024
					WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Drive {driveLetter} has : {availableSpaceInGB:F2} GB Available Space" + Environment.NewLine)
					If availableSpaceInGB < minimumGB Then
						WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Drive {driveLetter} has less than : {minimumGB:F2} GB Available Space" + Environment.NewLine)
						WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Start Deleting old files in Drive {driveLetter} " + Environment.NewLine)
						If dataGridView1 IsNot Nothing Then
							Dim foundFolderToDelete As Boolean
							For i = 0 To dataGridView1.RowCount - 1

								Dim rowIndex As Integer = dataGridView1.Rows(i).Cells(0).RowIndex
								If String.IsNullOrEmpty(dataGridView1.Rows(rowIndex).Cells(0).Value) Then Continue For
								Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
								Dim deleteDriveLetter As String = ""
								Try
									deleteDriveLetter = IO.Path.GetPathRoot(path).Replace(":\", "")
								Catch ex As Exception
								End Try
								If deleteDriveLetter <> driveLetter Then
									Continue For
								Else
									foundFolderToDelete = True
								End If
								Dim period As Integer = dataGridView1.Rows(rowIndex).Cells(2).Value
								Dim deletelogpath As String = logpath
								Dim DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentWithMaximum(path, maximumFilesToDelete, maximumFoldersToDelete, filesDeleted, foldersDeleted, deletelogpath)))
								DeleteTask.Start()
							Next
							If Not foundFolderToDelete Then
								WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Nothing to delete in Drive {driveLetter} " + Environment.NewLine)
							End If
						End If

						WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Finish Deleting old files in Drive {driveLetter} " + Environment.NewLine)
					End If
				Else
					WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Drive {driveLetter} is not ready." + Environment.NewLine)
				End If

			Catch ex As Exception
				WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"An error occurred: {ex.Message}" + Environment.NewLine)
			End Try


			WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Checking " + driveLetter + Environment.NewLine)

			Try
				richTextBox1.Invoke(Sub()
										richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Checking " + driveLetter + Environment.NewLine)
										richTextBox1.Focus()
									End Sub)
			Catch ex As Exception

			End Try


		End Sub

		Private Sub DeleteCurrentAtPeriodWithWait(path As String, period As Integer, wait As Integer, logpath As String)

			While True
				If DeleteTaskTasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				Dim Deleting As Boolean = False
				If Not Deleting Then

					Deleting = True
					WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Deleting " + path + Environment.NewLine)
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
									WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + file.FullName + Environment.NewLine)
								Catch ex As Exception
									WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + file.FullName + ", exception : " + ex.Message + Environment.NewLine)
								End Try

							End If
						Next

						If path.ToUpper.Contains("POCB\HEX") Or path.Contains("D:\Test") Then
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
										WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + dirInfo.FullName + Environment.NewLine)
									Catch ex As Exception
										WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + dirInfo.FullName + ", exception : " + ex.Message + Environment.NewLine)

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
										WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + dir.FullName + Environment.NewLine)
									Catch ex As Exception
										WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + dir.FullName + ", exception : " + ex.Message + Environment.NewLine)
									End Try

								End If
							Next
						End If

					End If

					Deleting = False
					WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)
					Try
						richTextBox1.Invoke(Sub()
												richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)
												richTextBox1.Focus()
											End Sub)
					Catch ex As Exception

					End Try

				Else
					WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Still Deleting " + path + " ,start again after " + wait.ToString + " seconds" + Environment.NewLine)
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

		Private Sub CheckDiskSpaceWithWait(driveLetter As String, waitTime As Integer, minimumGB As Double, maximumFilesToDelete As Integer, maximumFoldersToDelete As Integer, wait As Integer, logpath As String)

			While True
				If CheckDiskTaskTasksCancellationTokenSource.IsCancellationRequested Then
					Exit Sub
				End If
				Dim Checking As Boolean = False
				Dim filesDeleted As Integer = 0
				Dim foldersDeleted As Integer = 0
				If Not Checking Then

					Checking = True

					WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Checking " + driveLetter + Environment.NewLine)

					Try

					Catch ex As Exception
						richTextBox1.Invoke(Sub()
												richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Checking " + driveLetter + Environment.NewLine)
												richTextBox1.Focus()
											End Sub)
					End Try

					Try
						Dim driveInfo As New DriveInfo(driveLetter)

						If driveInfo.IsReady Then
							Dim availableSpaceInBytes As Long = driveInfo.AvailableFreeSpace
							Dim availableSpaceInMB As Double = CDbl(availableSpaceInBytes) / (1024 * 1024)
							Dim availableSpaceInGB As Double = availableSpaceInMB / 1024
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Drive {driveLetter} has : {availableSpaceInGB:F2} GB Available Space" + Environment.NewLine)
							If availableSpaceInGB < minimumGB Then
								WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Drive {driveLetter} has less than : {minimumGB:F2} GB Available Space" + Environment.NewLine)
								WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Start Deleting old files in Drive {driveLetter} " + Environment.NewLine)
								If dataGridView1 IsNot Nothing Then
									Dim foundFolderToDelete As Boolean
									For i = 0 To dataGridView1.RowCount - 1

										Dim rowIndex As Integer = dataGridView1.Rows(i).Cells(0).RowIndex
										If String.IsNullOrEmpty(dataGridView1.Rows(rowIndex).Cells(0).Value) Then Continue For
										Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
										Dim deleteDriveLetter As String = ""
										Try
											deleteDriveLetter = IO.Path.GetPathRoot(path).Replace(":\", "")
										Catch ex As Exception
										End Try
										If deleteDriveLetter <> driveLetter Then
											Continue For
										Else
											foundFolderToDelete = True
										End If
										Dim period As Integer = dataGridView1.Rows(rowIndex).Cells(2).Value
										Dim deletelogpath As String = logpath
										Dim DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentWithMaximum(path, maximumFilesToDelete, maximumFoldersToDelete, filesDeleted, foldersDeleted, deletelogpath)))
										DeleteTask.Start()
									Next
									If Not foundFolderToDelete Then
										WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Nothing to delete in Drive {driveLetter} " + Environment.NewLine)
									End If
								End If

								WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Finish Deleting old files in Drive {driveLetter} " + Environment.NewLine)

							End If
						Else
							WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Drive {driveLetter} is not ready." + Environment.NewLine)
						End If

					Catch ex As Exception
						WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"An error occurred: {ex.Message}" + Environment.NewLine)
					End Try

					Checking = False
					filesDeleted = 0
					foldersDeleted = 0

					WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Checking " + driveLetter + Environment.NewLine)

					Try
						richTextBox1.Invoke(Sub()
												richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Checking " + driveLetter + Environment.NewLine)
												richTextBox1.Focus()
											End Sub)
					Catch ex As Exception

					End Try

				Else

					WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Still Checking " + driveLetter + " ,start again after " + wait.ToString + " seconds" + Environment.NewLine)

					Try
						richTextBox1.Invoke(Sub()
												richTextBox1.AppendText(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Still Checking " + driveLetter + " ,start again after " + wait.ToString + " seconds" + Environment.NewLine)
												richTextBox1.Focus()
											End Sub)
					Catch ex As Exception

					End Try


				End If

				sw = New Stopwatch
				sw.Start()
				While sw.ElapsedMilliseconds < 1000 * wait
					If CheckDiskTaskTasksCancellationTokenSource.IsCancellationRequested Then

						Exit Sub
					End If
					System.Threading.Thread.Sleep(100)
				End While
			End While


		End Sub




		Private Sub cmdStartMonitor_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStartMonitor.LinkClicked
			startMonitor()
		End Sub

		Private Sub cmdStopMonitor_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStopMonitor.LinkClicked
			stopMonitor()
		End Sub

		Private Sub startMonitor()

			If dataGridView1 IsNot Nothing AndAlso chkMonitorFolders.Checked = True Then
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
					Dim logpath As String = "autodeletedatalog " + path.Replace("\", "-").Replace(":", "") + ".txt"
					DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentAtPeriodWithWait(path, period, waitTime, logpath)), DeleteTaskTasksCancellationTokenSource.Token)
					DeleteTask.Start()
				Next

			End If
			If dataGridView2 IsNot Nothing AndAlso chkMonitorDisk.Checked = True Then
				CheckDiskTaskTasksCancellationTokenSource = New CancellationTokenSource
				For i = 0 To dataGridView2.RowCount - 1

					Dim rowIndex As Integer = dataGridView2.Rows(i).Cells(0).RowIndex
					If String.IsNullOrEmpty(dataGridView2.Rows(rowIndex).Cells(0).Value) Then Continue For
					Dim driveLetter As String = dataGridView2.Rows(rowIndex).Cells(0).Value.ToString()
					Dim waitTime As New Integer
					If Not Int32.TryParse(dataGridView1.Rows(rowIndex).Cells(1).Value, waitTime) Then
						waitTime = 60
					End If
					Dim minimumGB As Integer = dataGridView2.Rows(rowIndex).Cells(2).Value
					Dim maximumFilesToDelete As Integer = dataGridView2.Rows(rowIndex).Cells(3).Value
					Dim maximumFoldersToDelete As Integer = dataGridView2.Rows(rowIndex).Cells(4).Value
					Dim logpath As String = "autocheckdisklog " + driveLetter + ".txt"
					CheckDiskTask = New Tasks.Task(New Action(Sub() CheckDiskSpaceWithWait(driveLetter, waitTime, minimumGB, maximumFilesToDelete, maximumFoldersToDelete, waitTime, logpath)), CheckDiskTaskTasksCancellationTokenSource.Token)
					CheckDiskTask.Start()
				Next

			End If

			lblMonitoringStatus.Invoke(Sub()
										   lblMonitoringStatus.Text = "Status : Monitoring ..."
									   End Sub)
			cmdStartMonitor.Invoke(Sub()
									   cmdStartMonitor.Enabled = False
								   End Sub)
			cmdStopMonitor.Invoke(Sub()
									  cmdStopMonitor.Enabled = True
								  End Sub)


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

			If CheckDiskTaskTasksCancellationTokenSource IsNot Nothing Then
				CheckDiskTaskTasksCancellationTokenSource.Cancel()
			End If
			If CheckDiskTaskTasksCancellationTokenSource IsNot Nothing Then
				If CheckDiskTask.Status <> TaskStatus.RanToCompletion Then
					'Wait a little longer
					Dim sw As New Stopwatch
					sw.Start()
					Do Until CheckDiskTask.Status = TaskStatus.RanToCompletion
						If CheckDiskTask.Status = TaskStatus.Canceled Then Exit Do
						If CheckDiskTask.Status = TaskStatus.Faulted Then Exit Do
						If sw.ElapsedMilliseconds > 1000 Then Exit Do
					Loop
					sw.Stop()
				End If
				If CheckDiskTask.IsCompleted OrElse CheckDiskTask.IsCanceled OrElse CheckDiskTask.IsFaulted Then
					CheckDiskTask.Dispose()
				End If
				CheckDiskTask = Nothing
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


		Private Sub DeleteSelectedFolderToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteSelectedFolderToolStripMenuItem.Click
			If dataGridView1 IsNot Nothing Then
				Dim rowIndex As Integer = dataGridView1.CurrentCell.RowIndex
				Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
				Dim period As Integer = dataGridView1.Rows(rowIndex).Cells(2).Value
				Dim logpath As String = "autodeletedatalog " + path.Replace("\", "-").Replace(":", "") + ".txt"
				Dim DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentAtPeriod(path, period, logpath)))
				DeleteTask.Start()
			End If
		End Sub

		Private Sub DeleteAllFoldersToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles DeleteAllFoldersToolStripMenuItem.Click
			If dataGridView1 IsNot Nothing Then

				For i = 0 To dataGridView1.RowCount - 1

					Dim rowIndex As Integer = dataGridView1.Rows(i).Cells(0).RowIndex
					If String.IsNullOrEmpty(dataGridView1.Rows(rowIndex).Cells(0).Value) Then Continue For
					Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
					Dim period As Integer = dataGridView1.Rows(rowIndex).Cells(2).Value
					Dim logpath As String = "autodeletedatalog " + path.Replace("\", "-").Replace(":", "") + ".txt"
					Dim DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentAtPeriod(path, period, logpath)))
					DeleteTask.Start()
				Next

			End If

		End Sub

		Private Sub CheckSelectedDiskFreespaceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CheckSelectedDiskFreespaceToolStripMenuItem.Click
			If dataGridView2 IsNot Nothing Then
				Dim rowIndex As Integer = dataGridView2.CurrentCell.RowIndex
				Dim driveLetter As String = dataGridView2.Rows(rowIndex).Cells(0).Value.ToString()
				Dim minimumGB As Integer = dataGridView2.Rows(rowIndex).Cells(2).Value
				Dim maximumFilesToDelete As Integer = dataGridView2.Rows(rowIndex).Cells(3).Value
				Dim maximumFoldersToDelete As Integer = dataGridView2.Rows(rowIndex).Cells(4).Value
				Dim logpath As String = "autocheckdisklog " + driveLetter + ".txt"
				Dim CheckDiskTask = New Tasks.Task(New Action(Sub() CheckDiskSpace(driveLetter, minimumGB, maximumFilesToDelete, maximumFoldersToDelete, logpath)))
				CheckDiskTask.Start()
			End If
		End Sub

		Private Sub CheckAllDisksFreepsaceToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles CheckAllDisksFreepsaceToolStripMenuItem.Click
			If dataGridView2 IsNot Nothing Then

				For i = 0 To dataGridView2.RowCount - 1

					Dim rowIndex As Integer = dataGridView2.Rows(i).Cells(0).RowIndex
					If String.IsNullOrEmpty(dataGridView2.Rows(rowIndex).Cells(0).Value) Then Continue For
					Dim driveLetter As String = dataGridView2.Rows(rowIndex).Cells(0).Value.ToString()
					Dim minimumGB As Integer = dataGridView2.Rows(rowIndex).Cells(2).Value
					Dim maximumFilesToDelete As Integer = dataGridView2.Rows(rowIndex).Cells(3).Value
					Dim maximumFoldersToDelete As Integer = dataGridView2.Rows(rowIndex).Cells(4).Value
					Dim logpath As String = "autocheckdisklog " + driveLetter + ".txt"
					CheckDiskTask = New Tasks.Task(New Action(Sub() CheckDiskSpace(driveLetter, minimumGB, maximumFilesToDelete, maximumFoldersToDelete, logpath)))
					CheckDiskTask.Start()
				Next

			End If
		End Sub

		Private Sub SaveListToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveListToolStripMenuItem.Click
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

			Dim rows2 = From row As DataGridViewRow In dataGridView2.Rows.Cast(Of DataGridViewRow)()
						Where Not row.IsNewRow
						Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))
			Try
				Using sw As New IO.StreamWriter("C:\Radiant Vision Systems Data\TrueTest\UserData\deletelist2.csv")
					For Each r In rows2
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
				MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\deletelist2.csv is in use, cannot save list")
			End Try
		End Sub

		Private Sub ReloadListToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReloadListToolStripMenuItem.Click
			LoadDeleteList()
		End Sub

		Private Sub ViewSelectedFolderLogToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ViewSelectedFolderLogToolStripMenuItem.Click
			If dataGridView1 IsNot Nothing Then
				Dim rowIndex As Integer = dataGridView1.CurrentCell.RowIndex
				Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
				Dim logpath As String = "autodeletedatalog " + path.Replace("\", "-").Replace(":", "") + ".txt"
				Try
					Process.Start(logpath)
				Catch ex As Exception
				End Try

			End If
		End Sub

		Private Sub ViewSelectedDiskLogToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ViewSelectedDiskLogToolStripMenuItem.Click
			If dataGridView2 IsNot Nothing Then
				Dim rowIndex As Integer = dataGridView2.CurrentCell.RowIndex
				Dim driverLetter As String = dataGridView2.Rows(rowIndex).Cells(0).Value.ToString()
				Dim logpath As String = "autocheckdisklog " + driverLetter + ".txt"
				Try
					Process.Start(logpath)
				Catch ex As Exception
				End Try

			End If
		End Sub

		Private Sub ClearLogsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ClearLogsToolStripMenuItem.Click
			richTextBox1.Clear()
			Dim logList As IEnumerable(Of String) = IO.Directory.EnumerateFiles(appdir, "autodeletedatalog*.txt")

			For Each log As String In logList
				Try
					IO.File.Delete(log)
				Catch ex As Exception

				End Try
			Next

			Dim logList2 As IEnumerable(Of String) = IO.Directory.EnumerateFiles(appdir, "autocheckdisklog*.txt")

			For Each log As String In logList2
				Try
					IO.File.Delete(log)
				Catch ex As Exception

				End Try
			Next
		End Sub

		Private Sub WriteLog(file As String, content As String)
			Dim fileloaded As Boolean
			While Not fileloaded
				Try
					IO.File.AppendAllText(file, content)
					fileloaded = True
				Catch ex As Exception
					fileloaded = False
				End Try
			End While
		End Sub

	End Class
End Namespace
