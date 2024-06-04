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
Imports System.Text.RegularExpressions
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Runtime.InteropServices.ComTypes

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
            LoadAllMonitorList()
            LoadExcludedFileNames()
            LoadExcludedFolderNames()
            LoadCreationSetting()
            If MonitorAutomaticallyToolStripMenuItem.Checked = True Then
                startMonitor()
            End If
        End Sub

        Private Sub aboutToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles aboutToolStripMenuItem.Click
            MessageBox.Show("dh.chau@radiantvs.com")
        End Sub

        Private Sub exitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles exitToolStripMenuItem.Click
            SaveSettings()
            Application.Exit()
        End Sub

        Private Sub exit2ToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles exit2ToolStripMenuItem.Click
            SaveSettings()
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
            Try
                Using writer As New StreamWriter(settingPath)
                    ' Save control settings
                    For Each ctrl As Control In Me.Controls
                        If TypeOf ctrl Is Windows.Forms.TextBox Then
                            writer.WriteLine($"{ctrl.Name}={ctrl.Text}")
                        ElseIf TypeOf ctrl Is CheckBox Then
                            Dim checkBox As CheckBox = DirectCast(ctrl, CheckBox)
                            writer.WriteLine($"{ctrl.Name}={checkBox.Checked}")
                        End If
                    Next

                    ' Save ToolStripMenuItem settings
                    writer.WriteLine($"startminimized={startMinimizedToolStripMenuItem.Checked}")
                    writer.WriteLine($"minimizedtotray={minimizedToTrayToolStripMenuItem.Checked}")
                    writer.WriteLine($"monitorautomatically={MonitorAutomaticallyToolStripMenuItem.Checked}")
                End Using
            Catch ex As Exception
                MessageBox.Show("Error saving settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
        Private Sub LoadSettings()
            If File.Exists(settingPath) Then
                Try
                    Dim settings() As String = File.ReadAllLines(settingPath)
                    For Each line As String In settings
                        Dim parts As String() = line.Split("="c)
                        If parts.Length = 2 Then
                            Dim setting As String = parts(0).Trim()
                            Dim value As String = parts(1).Trim()

                            ' Check for ToolStripMenuItem settings
                            If setting = "startminimized" Then
                                startMinimizedToolStripMenuItem.Checked = (value.ToLower() = "true")
                            ElseIf setting = "minimizedtotray" Then
                                minimizedToTrayToolStripMenuItem.Checked = (value.ToLower() = "true")
                            ElseIf setting = "monitorautomatically" Then
                                MonitorAutomaticallyToolStripMenuItem.Checked = (value.ToLower() = "true")
                            Else
                                ' Check for control settings
                                Dim ctrl As Control = Me.Controls.Find(setting, True).FirstOrDefault()
                                If ctrl IsNot Nothing Then
                                    If TypeOf ctrl Is Windows.Forms.TextBox Then
                                        ctrl.Text = value
                                    ElseIf TypeOf ctrl Is CheckBox Then
                                        Dim checkBox As CheckBox = DirectCast(ctrl, CheckBox)
                                        checkBox.Checked = Boolean.Parse(value)
                                    End If
                                End If
                            End If
                        End If
                    Next
                Catch ex As Exception
                    MessageBox.Show("Error loading settings: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            End If
        End Sub

        Private Function InlineAssignHelper(Of T)(ByRef target As T, ByVal value As T) As T
            target = value
            Return value
        End Function

        Private Sub LoadAllMonitorList()
            Dim CSVFilePathName = "C:\Radiant Vision Systems Data\TrueTest\UserData\foldermonitorlist.csv"
            Dim Lines() As String
            If File.Exists(CSVFilePathName) Then
                Try
                    Lines = File.ReadAllLines(CSVFilePathName).Where(Function(arg) Not String.IsNullOrWhiteSpace(arg)).ToArray
                Catch ex As Exception
                    MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\foldermonitorlist.csv is in use, cannot load saved list")
                    Lines = New String() {
                        "D:\Program\RVS\InputLog,3600,3,",
                        "D:\Program\RVS\UploadQueue\Log,3600,3,",
                        "D:\Program\RVS\TrueTestWatcherLog,3600,3,",
                        "D:\Program\RVS\AutoDeleteDataLog,3600,3,",
                        "D:\POCB\Hex,3600,3,",
                        "E:\POCB\Hex,3600,3,",
                        "F:\POCB\Hex,3600,3,",
                        "G:\POCB\Hex,3600,3,",
                        "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3,",
                        "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3,",
                        "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3,",
                        "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3,",
                        "D:\Test,3600,3,"}
                End Try

            Else
                Lines = New String() {
                    "D:\Program\RVS\InputLog,3600,3,",
                    "D:\Program\RVS\UploadQueue\Log,3600,3,",
                    "D:\Program\RVS\TrueTestWatcherLog,3600,3,",
                    "D:\Program\RVS\AutoDeleteDataLog,3600,3,",
                    "D:\POCB\Hex,3600,3,",
                    "E:\POCB\Hex,3600,3,",
                    "F:\POCB\Hex,3600,3,",
                    "G:\POCB\Hex,3600,3,",
                    "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3,",
                    "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3,",
                    "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3,",
                    "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3,",
                    "D:\Test,3600,3,"}
            End If

            Dim Fields() = "Delete Path, Delete Wait (s), Delete Period (days), Next Run Countdown (s)".Split(",")
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
            dataGridView1.Columns(3).ReadOnly = True

            Dim CSVFilePathName2 = "C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist.csv"
            Dim Lines2() As String
            If File.Exists(CSVFilePathName2) Then
                Try
                    Lines2 = File.ReadAllLines(CSVFilePathName2).Where(Function(arg) Not String.IsNullOrWhiteSpace(arg)).ToArray
                Catch ex As Exception
                    MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist.csv is in use, cannot load saved list")
                    Lines2 = New String() {
                        "C,60,30,",
                        "D,60,30,",
                        "E,60,30,",
                        "F,60,30,",
                        "G,60,30,",
                        "H,60,30,"
                        }
                End Try

            Else
                Lines2 = New String() {
                    "C,60,30,",
                    "D,60,30,",
                    "E,60,30,",
                    "F,60,30,",
                    "G,60,30,",
                    "H,60,30,"
                    }
            End If

            Dim Fields2() = "Disk drive, Check Wait (s), Minimum space to start delete (GB), Next Run Countdown (s)".Split(",")
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
            dataGridView2.Columns(3).ReadOnly = True

            Dim CSVFilePathName3 = "C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist2.csv"
            Dim Lines3() As String
            If File.Exists(CSVFilePathName3) Then
                Try
                    Lines3 = File.ReadAllLines(CSVFilePathName3).Where(Function(arg) Not String.IsNullOrWhiteSpace(arg)).ToArray
                Catch ex As Exception
                    MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist2.csv is in use, cannot load saved list")
                    Lines3 = New String() {
                        "C:\POCB\Hex",
                        "D:\POCB\Hex",
                        "E:\POCB\Hex",
                        "F:\POCB\Hex",
                        "G:\POCB\Hex",
                        "H:\POCB\Hex",
                        "C:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                        "C:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                        "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                        "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                        "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                        "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                        "F:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                        "F:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                        "G:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                        "G:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                        "H:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                        "H:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2"
                        }
                End Try

            Else
                Lines3 = New String() {
                    "C:\POCB\Hex",
                    "D:\POCB\Hex",
                    "E:\POCB\Hex",
                    "F:\POCB\Hex",
                    "G:\POCB\Hex",
                    "H:\POCB\Hex",
                    "C:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                    "C:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                    "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                    "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                    "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                    "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                    "F:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                    "F:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                    "G:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                    "G:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                    "H:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                    "H:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2"
                    }
            End If

            Dim Fields3() = "Delete Path".Split(",")
            Dim Cols3 = Fields3.GetLength(0)
            Dim dt3 = New DataTable
            For index = 0 To Cols3 - 1
                dt3.Columns.Add(Fields3(index))
            Next

            Dim Row3 As DataRow
            For index = 0 To Lines3.GetLength(0) - 1
                Fields3 = Lines3(index).Split(",")
                Row3 = dt3.NewRow
                For f = 0 To Cols3 - 1
                    Row3(f) = Fields3(f)
                Next
                dt3.Rows.Add(Row3)
            Next
            dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
            dataGridView3.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
            dataGridView3.AllowUserToResizeColumns = True
            If dataGridView3.RowCount = 0 Then
                dataGridView3.DataSource = dt3
            End If

        End Sub

        Private Sub LoadFolderMonitorList()
            Dim CSVFilePathName = "C:\Radiant Vision Systems Data\TrueTest\UserData\foldermonitorlist.csv"
            Dim Lines() As String
            If File.Exists(CSVFilePathName) Then
                Try
                    Lines = File.ReadAllLines(CSVFilePathName).Where(Function(arg) Not String.IsNullOrWhiteSpace(arg)).ToArray
                Catch ex As Exception
                    MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\foldermonitorlist.csv is in use, cannot load saved list")
                    Lines = New String() {
                        "D:\Program\RVS\InputLog,3600,3,",
                        "D:\Program\RVS\UploadQueue\Log,3600,3,",
                        "D:\Program\RVS\TrueTestWatcherLog,3600,3,",
                        "D:\Program\RVS\AutoDeleteDataLog,3600,3,",
                        "D:\POCB\Hex,3600,3,",
                        "E:\POCB\Hex,3600,3,",
                        "F:\POCB\Hex,3600,3,",
                        "G:\POCB\Hex,3600,3,",
                        "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3,",
                        "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3,",
                        "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3,",
                        "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3,",
                        "D:\Test,3600,3,"}
                End Try

            Else
                Lines = New String() {
                        "D:\Program\RVS\InputLog,3600,3,",
                        "D:\Program\RVS\UploadQueue\Log,3600,3,",
                        "D:\Program\RVS\TrueTestWatcherLog,3600,3,",
                        "D:\Program\RVS\AutoDeleteDataLog,3600,3,",
                        "D:\POCB\Hex,3600,3,",
                        "E:\POCB\Hex,3600,3,",
                        "F:\POCB\Hex,3600,3,",
                        "G:\POCB\Hex,3600,3,",
                        "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3,",
                        "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3,",
                        "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1,3600,3,",
                        "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2,3600,3,",
                        "D:\Test,3600,3,"}
            End If

            Dim Fields() = "Delete Path, Delete Wait (s), Delete Period (days), Next Run Countdown (s)".Split(",")
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
            dataGridView1.Columns(3).ReadOnly = True

        End Sub

        Private Sub LoadDiskMonitiorist()

            Dim CSVFilePathName2 = "C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist.csv"
            Dim Lines2() As String
            If File.Exists(CSVFilePathName2) Then
                Try
                    Lines2 = File.ReadAllLines(CSVFilePathName2).Where(Function(arg) Not String.IsNullOrWhiteSpace(arg)).ToArray
                Catch ex As Exception
                    MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist.csv is in use, cannot load saved list")
                    Lines2 = New String() {
                        "C,60,30,",
                        "D,60,30,",
                        "E,60,30,",
                        "F,60,30,",
                        "G,60,30,",
                        "H,60,30,"
                        }
                End Try

            Else
                Lines2 = New String() {
                    "C,60,30,",
                    "D,60,30,",
                    "E,60,30,",
                    "F,60,30,",
                    "G,60,30,",
                    "H,60,30,"
                    }
            End If

            Dim Fields2() = "Disk drive, Check Wait (s), Minimum space to start delete (GB), Next Run Countdown (s)".Split(",")
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
            dataGridView2.Columns(3).ReadOnly = True

            Dim CSVFilePathName3 = "C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist2.csv"
            Dim Lines3() As String
            If File.Exists(CSVFilePathName3) Then
                Try
                    Lines3 = File.ReadAllLines(CSVFilePathName3).Where(Function(arg) Not String.IsNullOrWhiteSpace(arg)).ToArray
                Catch ex As Exception
                    MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist2.csv is in use, cannot load saved list")
                    Lines3 = New String() {
                        "D:\POCB\Hex",
                        "E:\POCB\Hex",
                        "F:\POCB\Hex",
                        "G:\POCB\Hex",
                        "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                        "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                        "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                        "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                        "F:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                        "F:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                        "G:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                        "G:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2"
                        }
                End Try

            Else
                Lines3 = New String() {
                    "D:\POCB\Hex",
                    "E:\POCB\Hex",
                    "F:\POCB\Hex",
                    "G:\POCB\Hex",
                    "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                    "D:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                    "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                    "E:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                    "F:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                    "F:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2",
                    "G:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB1",
                    "G:\Radiant Vision Systems Data\TrueTest\UserData\AutoGenerated\DB2"
                    }
            End If

            Dim Fields3() = "Delete Path".Split(",")
            Dim Cols3 = Fields3.GetLength(0)
            Dim dt3 = New DataTable
            For index = 0 To Cols3 - 1
                dt3.Columns.Add(Fields3(index))
            Next

            Dim Row3 As DataRow
            For index = 0 To Lines3.GetLength(0) - 1
                Fields3 = Lines3(index).Split(",")
                Row3 = dt3.NewRow
                For f = 0 To Cols3 - 1
                    Row3(f) = Fields3(f)
                Next
                dt3.Rows.Add(Row3)
            Next
            dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells
            dataGridView3.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None
            dataGridView3.AllowUserToResizeColumns = True
            If dataGridView3.RowCount = 0 Then
                dataGridView3.DataSource = dt3
            End If
        End Sub

        Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
            ' Check if the close reason is user closing the form
            If e.CloseReason = CloseReason.UserClosing Then
                ' Minimize the form instead of closing
                e.Cancel = True
                Me.WindowState = FormWindowState.Minimized
            End If
        End Sub

        Private Sub DeleteCurrentWithSize(foldersToDelete As List(Of String), TargetAvailableFreeSpaceInGB As Double, logpath As String)

            ' Recursively delete files in all subdirectories and delete empty directories
            DeleteOldestFilesAndSubfolders(foldersToDelete, TargetAvailableFreeSpaceInGB, logpath)

        End Sub

        Private Function GetDirectorySize(ByVal folderPath As String) As Long
            Dim folderInfo As New IO.DirectoryInfo(folderPath)
            Dim fileSize As Long = 0

            For Each file In folderInfo.GetFiles()
                fileSize += file.Length
            Next

            For Each subfolder In folderInfo.GetDirectories()
                fileSize += GetDirectorySize(subfolder.FullName)
            Next

            Return fileSize
        End Function


        Private Sub DeleteCurrentAtPeriod(path As String, period As Integer, logpath As String)

            WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Deleting " + path + Environment.NewLine)

            If Directory.Exists(path) Then
                Dim directory As New IO.DirectoryInfo(path)

                ' Recursively delete files in all subdirectories and delete empty directories
                DeleteFilesAndFolders(directory, period, logpath, path)


            Else
                WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Not existed " + path + Environment.NewLine)
            End If

            WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)

        End Sub

        Function ShouldExcludeFile(file As FileInfo, exludeFileNameList As List(Of String), logpath As String) As Boolean
            ' Check if the checkbox is checked
            If chkSkipFileLastCreationTime.Checked Then
                ' Parse the value from txtFileLastCreationTimeToSkip
                Dim minutesToSkip As Integer
                If Integer.TryParse(txtFileLastCreationTimeToSkip.Text.Trim(), minutesToSkip) Then
                    ' Check if the file's creation time is within the specified minutes range
                    If (DateTime.Now - file.CreationTime).TotalMinutes <= minutesToSkip Then
                        ' Log a message indicating that the file is excluded based on creation time
                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"File '{file.Name}' is excluded based on creation time specified in txtFileLastCreationTimeToSkip." + Environment.NewLine)
                        Return True
                    End If
                Else
                    ' Log an error message if the minutes value is invalid
                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Invalid minutes value specified in txtFileLastCreationTimeToSkip. Expected integer value." + Environment.NewLine)
                End If
            End If

            ' Continue with the existing logic for excluding files based on the pattern list
            Return exludeFileNameList.Any(Function(pattern) New Regex("^" & Regex.Escape(pattern).Replace("\*", ".*").Replace("\?", ".") & "$", RegexOptions.IgnoreCase).IsMatch(file.Name))
        End Function

        Function ShouldExcludeFolder(ByVal folder As DirectoryInfo, exludeFolderNameList As List(Of String), logpath As String) As Boolean
            ' Check if the checkbox is checked
            If chkSkipFolderLastCreationTime.Checked Then
                ' Parse the value from txtFolderLastCreationTimeToSkip
                Dim timeToSkipStr As String = txtFolderLastCreationTimeToSkip.Text.Trim()
                ' Try to parse the minutes value from the text box
                Dim minutesToSkip As Integer
                If Integer.TryParse(timeToSkipStr, minutesToSkip) Then
                    ' Check if the folder's creation time is within the specified minutes range
                    If (DateTime.Now - folder.CreationTime).TotalMinutes <= minutesToSkip Then
                        ' Log a message indicating that the folder is excluded based on creation time
                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Folder '{folder.Name}' is excluded based on creation time specified in txtFolderLastCreationTimeToSkip." + Environment.NewLine)
                        Return True
                    End If
                Else
                    ' Log an error message if the minutes value is invalid
                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Invalid minutes value specified in txtFolderLastCreationTimeToSkip. Expected integer value." + Environment.NewLine)
                End If
            End If

            ' Continue with the existing logic for excluding folders based on the pattern list
            Return exludeFolderNameList.Any(Function(pattern) New Regex("^" & Regex.Escape(pattern).Replace("\*", ".*").Replace("\?", ".") & "$", RegexOptions.IgnoreCase).IsMatch(folder.Name))
        End Function

        Sub DeleteFilesAndFolders(ByVal parentDirectory As DirectoryInfo, ByVal period As Integer, ByVal logpath As String, ByVal topLevelDirectoryPath As String)

            Dim excludedFolderNames As New List(Of String)(txtExcludeFolderNames.Text.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))

            If ShouldExcludeFolder(parentDirectory, excludedFolderNames, logpath) Then
                Return ' Skip deletion and recursion if parent folder matches the name
            End If

            Dim excludedFileNames As New List(Of String)(txtExcludeFileNames.Text.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))

            ' Delete files in the current directory
            For Each file As FileInfo In parentDirectory.GetFiles()

                If ShouldExcludeFile(file, excludedFileNames, logpath) Then
                    Continue For ' Skip deletion of excluded files
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

            ' Recursively delete files in subdirectories
            For Each directory As DirectoryInfo In parentDirectory.GetDirectories()
                DeleteFilesAndFolders(directory, period, logpath, topLevelDirectoryPath)
            Next

            ' Delete the parent directory if it's empty and not the top level directory
            If parentDirectory.FullName <> topLevelDirectoryPath AndAlso parentDirectory.GetFiles().Length = 0 AndAlso parentDirectory.GetDirectories().Length = 0 Then
                ' Check if the checkbox for skipping folder last creation time is checked
                If chkSkipFolderLastCreationTime.Checked Then
                    ' Parse the value from txtFolderLastCreationTimeToSkip
                    Dim minutesToSkip As Integer
                    If Integer.TryParse(txtFolderLastCreationTimeToSkip.Text.Trim(), minutesToSkip) Then
                        ' Check if the parent directory's creation time is within the specified minutes range
                        Dim directoryCreationTime As DateTime = Directory.GetCreationTime(parentDirectory.FullName)
                        If (DateTime.Now - directoryCreationTime).TotalMinutes <= minutesToSkip Then
                            ' Log a message indicating that the directory deletion is skipped based on creation time
                            WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Skipped deleting empty directory '{parentDirectory.FullName}' based on creation time specified in txtFolderLastCreationTimeToSkip." + Environment.NewLine)
                            Exit Sub ' Skip deletion
                        End If
                    Else
                        ' Log an error message if the minutes value is invalid
                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Invalid minutes value specified in txtFolderLastCreationTimeToSkip. Expected integer value." + Environment.NewLine)
                    End If
                End If

                If (Now - parentDirectory.CreationTime).Days > period Then
                    ' Delete the parent directory if it meets the deletion criteria
                    Try
                        parentDirectory.Delete()
                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Deleted " + parentDirectory.FullName + Environment.NewLine)
                    Catch ex As Exception
                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot delete " + parentDirectory.FullName + ", exception : " + ex.Message + Environment.NewLine)
                    End Try
                End If

            End If

        End Sub

        Sub DeleteOldestFilesAndSubfolders(ByVal foldersToDelete As List(Of String), ByVal targetSizeGB As Double, ByVal logpath As String)

            Dim excludedFolderNames As New List(Of String)(txtExcludeFolderNames.Text.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))
            Dim excludedFilesNames As New List(Of String)(txtExcludeFileNames.Text.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries))

            Dim allFiles As New List(Of FileSystemInfo)
            Dim emptiedFolders As New List(Of DirectoryInfo)

            ' Collect files and folders from all specified folders
            For Each folderPath As String In foldersToDelete
                Try
                    Dim directory As New DirectoryInfo(folderPath)
                    If directory.Exists Then
                        allFiles.AddRange(GetAllFilesAndFolders(directory))
                    Else
                        WriteLog(logpath, $"Directory not found: {folderPath}")
                    End If
                Catch ex As Exception
                    ' Log the exception or handle it as needed
                    WriteLog(logpath, $"Error processing folder {folderPath}: {ex.Message}")
                End Try
            Next

            ' Sort files and folders by creation time in ascending order (oldest first)
            allFiles.Sort(Function(x, y) GetCreationTime(x).CompareTo(GetCreationTime(y)))

            Dim driveRoot As String = Path.GetPathRoot(foldersToDelete(0))
            Dim driveInfo As DriveInfo = New DriveInfo(driveRoot)
            Dim freeSpaceGB As Double = driveInfo.AvailableFreeSpace / 1024 / 1024 / 1024 ' Convert bytes to GB

            If freeSpaceGB >= targetSizeGB Then
                WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Free space on disk already meets or exceeds the target size." + Environment.NewLine)
                Return
            End If

            ' Delete files and track emptied folders
            For Each item As FileSystemInfo In allFiles
                Try
                    If TypeOf item Is FileInfo Then
                        Dim file As FileInfo = DirectCast(item, FileInfo)
                        If ShouldStopDeleting(file, excludedFolderNames, excludedFilesNames, logpath) Then
                            WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"File {file.FullName} matches one of the exclude criteria. Stopping deletion." + Environment.NewLine)
                            Continue For
                        End If
                        file.Delete()
                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Deleted file: {file.FullName}" + Environment.NewLine)
                    ElseIf TypeOf item Is DirectoryInfo Then
                        Dim directory As DirectoryInfo = DirectCast(item, DirectoryInfo)
                        If DirectoryIsEmpty(directory) AndAlso Not IsFolderInPathExcluded(directory, excludedFolderNames) Then
                            ' Check if the checkbox is checked
                            If chkSkipFolderLastCreationTime.Checked Then
                                ' Parse the value from txtFolderLastCreationTimeToSkip as an integer
                                Dim minutesToSkip As Integer
                                If Integer.TryParse(txtFolderLastCreationTimeToSkip.Text, minutesToSkip) Then
                                    ' Check if the folder was created within the specified time frame
                                    If (DateTime.Now - directory.CreationTime).TotalMinutes > minutesToSkip Then
                                        directory.Delete()
                                        emptiedFolders.Add(directory) ' Track emptied folders
                                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Deleted empty folder: {directory.FullName}" + Environment.NewLine)
                                    Else
                                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Skipped deleting empty folder created in the last {minutesToSkip} minutes: {directory.FullName}" + Environment.NewLine)
                                    End If
                                Else
                                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Invalid value in txtFolderLastCreationTimeToSkip. Skipping deletion based on creation time." + Environment.NewLine)
                                End If
                            Else
                                ' If the checkbox is not checked, proceed with deletion without considering creation time
                                directory.Delete()
                                emptiedFolders.Add(directory) ' Track emptied folders
                                WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Deleted empty folder: {directory.FullName}" + Environment.NewLine)
                            End If
                        End If
                    End If

                    freeSpaceGB = New DriveInfo(driveRoot).AvailableFreeSpace / 1024 / 1024 / 1024
                    If freeSpaceGB >= targetSizeGB Then
                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Target available size reached. Stopping deletion." + Environment.NewLine)
                        Return
                    End If
                Catch ex As Exception
                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Failed to delete: {item.FullName}. {ex.Message}" + Environment.NewLine)
                End Try
            Next

            ' Delete emptied folders and their empty parent folders recursively
            For Each folder As DirectoryInfo In emptiedFolders
                DeleteEmptyParentFolders(folder, excludedFolderNames, foldersToDelete, logpath)
            Next

        End Sub

        Sub DeleteEmptyParentFolders(ByVal folder As DirectoryInfo, ByVal foldersToDelete As List(Of String), ByVal excludeFolders As List(Of String), logpath As String)
            If folder Is Nothing Then Return

            ' Stop deleting if parent folder is one of the top-level folders or matches any of the exclude folders
            If foldersToDelete.Contains(folder.FullName) OrElse excludeFolders.Contains(folder.Name) Then Return

            ' Check if the checkbox is checked
            If chkSkipFolderLastCreationTime.Checked Then
                ' Parse the value from txtFolderLastCreationTimeToSkip as an integer
                Dim minutesToSkip As Integer
                If Integer.TryParse(txtFolderLastCreationTimeToSkip.Text, minutesToSkip) Then
                    ' Skip deleting if the folder was created within the specified time frame
                    If (DateTime.Now - folder.CreationTime).TotalMinutes <= minutesToSkip Then
                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Skipped deleting empty parent folder created in the last {minutesToSkip} minutes: {folder.FullName}" + Environment.NewLine)
                        Return
                    End If
                Else
                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Invalid value in txtFolderLastCreationTimeToSkip. Skipping deletion based on creation time." + Environment.NewLine)
                End If
            End If

            If DirectoryIsEmpty(folder.Parent) Then
                Try
                    folder.Parent.Delete()
                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Deleted empty parent folder: {folder.Parent.FullName}" + Environment.NewLine)
                    DeleteEmptyParentFolders(folder.Parent, foldersToDelete, excludeFolders, logpath) ' Recursively check and delete empty parent folders
                Catch ex As Exception
                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Failed to delete empty parent folder: {folder.Parent.FullName}. {ex.Message}" + Environment.NewLine)
                End Try
            End If
        End Sub

        Function GetAllFilesAndFolders(ByVal directory As DirectoryInfo) As List(Of FileSystemInfo)
            Dim allFilesAndFolders As New List(Of FileSystemInfo)

            ' Add files and folders in the current directory
            allFilesAndFolders.AddRange(directory.GetFiles())
            allFilesAndFolders.AddRange(directory.GetDirectories())

            ' Recursively add files and folders from subdirectories
            For Each subdirectory As DirectoryInfo In directory.GetDirectories()
                allFilesAndFolders.AddRange(GetAllFilesAndFolders(subdirectory))
            Next

            Return allFilesAndFolders
        End Function

        Function DirectoryIsEmpty(ByVal directory As DirectoryInfo) As Boolean
            Return (directory.GetFiles().Length = 0 AndAlso directory.GetDirectories().Length = 0)
        End Function

        Function GetCreationTime(ByVal item As FileSystemInfo) As DateTime
            If TypeOf item Is FileInfo Then
                Return DirectCast(item, FileInfo).CreationTime
            ElseIf TypeOf item Is DirectoryInfo Then
                Return DirectCast(item, DirectoryInfo).CreationTime
            End If
            Return DateTime.MinValue
        End Function

        Function ShouldStopDeleting(ByVal file As FileInfo, ByVal excludeFolders As List(Of String), ByVal excludeFileNames As List(Of String), ByVal logpath As String) As Boolean

            If excludeFileNames.Any(Function(pattern) New Regex("^" & Regex.Escape(pattern).Replace("\*", ".*").Replace("\?", ".") & "$", RegexOptions.IgnoreCase).IsMatch(file.Name)) Then Return True
            If IsFolderInPathExcluded(file.Directory, excludeFolders) Then Return True

            ' Check if the checkbox is checked
            If chkSkipFileLastCreationTime.Checked Then
                ' Parse the value from txtFileLastCreationTimeToSkip as an integer
                Dim minutesToSkip As Integer
                If Integer.TryParse(txtFileLastCreationTimeToSkip.Text.Trim, minutesToSkip) Then
                    ' Skip files created within the specified time frame
                    If (DateTime.Now - file.CreationTime).TotalMinutes <= minutesToSkip Then Return True
                Else
                    ' Log an error if the value in txtFileLastCreationTimeToSkip is invalid
                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Invalid value in txtFileLastCreationTimeToSkip. Skipping deletion based on creation time." + Environment.NewLine)
                End If
            End If

            Return False
        End Function

        Function IsFolderInPathExcluded(ByVal directory As DirectoryInfo, ByVal excludeFolders As List(Of String)) As Boolean
            If directory Is Nothing Then Return False
            If excludeFolders.Any(Function(pattern) New Regex("^" & Regex.Escape(pattern).Replace("\*", ".*").Replace("\?", ".") & "$", RegexOptions.IgnoreCase).IsMatch(directory.Name)) Then Return True
            Return IsFolderInPathExcluded(directory.Parent, excludeFolders)
        End Function


        Private Sub CheckDiskSpace(driveLetter As String, minimumGB As Double, logpath As String)

            WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Checking " + driveLetter + Environment.NewLine)

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
                        If dataGridView3 IsNot Nothing Then
                            Dim foundFolderToDelete As Boolean
                            Dim foldersToDelete As New List(Of String)
                            For i = 0 To dataGridView3.RowCount - 1
                                Dim rowIndex As Integer = dataGridView3.Rows(i).Cells(0).RowIndex
                                If String.IsNullOrEmpty(dataGridView3.Rows(rowIndex).Cells(0).Value) Then Continue For
                                Dim path As String = dataGridView3.Rows(rowIndex).Cells(0).Value.ToString()
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

                                ' Collect folders to be processed
                                foldersToDelete.Add(path)
                                WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Added  {path} to delete list " + Environment.NewLine)

                            Next

                            WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Nothing to delete in Drive {driveLetter} " + Environment.NewLine)

                            Dim deletelogpath As String = logpath
                            Dim DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentWithSize(foldersToDelete, minimumGB, deletelogpath)))
                            DeleteTask.Start()


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

        End Sub

        Private Sub DeleteCurrentAtPeriodWithWait(currentRowIndex As Integer, path As String, period As Integer, wait As Integer, logpath As String)

            While True
                If DeleteTaskTasksCancellationTokenSource.IsCancellationRequested Then
                    Exit Sub
                End If
                Dim sw = New Stopwatch
                sw.Start()
                While sw.ElapsedMilliseconds < 1000 * wait
                    If DeleteTaskTasksCancellationTokenSource.IsCancellationRequested Then

                        Exit Sub
                    End If
                    System.Threading.Thread.Sleep(100)
                    dataGridView1.Invoke(Sub()
                                             dataGridView1.Rows(currentRowIndex).Cells(3).Value = Math.Floor(((wait + 1) - sw.ElapsedMilliseconds / 1000)).ToString
                                         End Sub)
                End While

                Dim Deleting As Boolean = False
                If Not Deleting Then

                    Deleting = True
                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Deleting " + path + Environment.NewLine)

                    If Directory.Exists(path) Then
                        Dim directory As New IO.DirectoryInfo(path)

                        ' Recursively delete files in all subdirectories and delete empty directories
                        DeleteFilesAndFolders(directory, period, logpath, path)

                    Else
                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Not existed " + path + Environment.NewLine)
                    End If

                    Deleting = False
                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Deleting " + path + Environment.NewLine)

                Else
                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Still Deleting " + path + " ,start again after " + wait.ToString + " seconds" + Environment.NewLine)

                End If

            End While


        End Sub

        Private Sub CheckDiskSpaceWithWait(currentRowIndex As Integer, driveLetter As String, minimumGB As Double, wait As Integer, logpath As String)

            While True
                If CheckDiskTaskTasksCancellationTokenSource.IsCancellationRequested Then
                    Exit Sub
                End If
                Dim sw = New Stopwatch
                sw.Start()
                While sw.ElapsedMilliseconds < 1000 * wait
                    If CheckDiskTaskTasksCancellationTokenSource.IsCancellationRequested Then

                        Exit Sub
                    End If
                    System.Threading.Thread.Sleep(100)
                    dataGridView2.Invoke(Sub()
                                             dataGridView2.Rows(currentRowIndex).Cells(3).Value = Math.Floor(((wait + 1) - sw.ElapsedMilliseconds / 1000)).ToString
                                         End Sub)
                End While

                Dim Checking As Boolean = False

                If Not Checking Then

                    Checking = True

                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Start Checking " + driveLetter + Environment.NewLine)

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
                                If dataGridView3 IsNot Nothing Then
                                    Dim foldersToDelete As New List(Of String)
                                    Dim foundFolderToDelete As Boolean
                                    For i = 0 To dataGridView3.RowCount - 1
                                        Dim rowIndex As Integer = dataGridView3.Rows(i).Cells(0).RowIndex
                                        If String.IsNullOrEmpty(dataGridView3.Rows(rowIndex).Cells(0).Value) Then Continue For
                                        Dim path As String = dataGridView3.Rows(rowIndex).Cells(0).Value.ToString()
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

                                        ' Collect folders to be processed
                                        foldersToDelete.Add(path)
                                        WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + $"Added  {path} to delete list " + Environment.NewLine)
                                    Next

                                    Dim deletelogpath As String = logpath
                                    Dim DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentWithSize(foldersToDelete, minimumGB, deletelogpath)))
                                    DeleteTask.Start()


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

                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Finish Checking " + driveLetter + Environment.NewLine)

                Else

                    WriteLog(logpath, Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Still Checking " + driveLetter + " ,start again after " + wait.ToString + " seconds" + Environment.NewLine)

                End If

            End While


        End Sub

        Private Sub cmdStartMonitor_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStartMonitor.LinkClicked
            startMonitor()
        End Sub

        Private Sub cmdStopMonitor_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdStopMonitor.LinkClicked
            stopMonitor()
        End Sub

        Private Sub startMonitor()
            If Not Directory.Exists(txtLogpath.Text) Then
                Try
                    Directory.CreateDirectory(txtLogpath.Text)
                Catch ex As Exception
                    MessageBox.Show(Now.ToString("yyyyMMdd HH:mm:ss") + " : " + "Cannot create log folder, use application folder instead !" + Environment.NewLine)
                End Try
            End If
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
                    Dim logpath As String = ""
                    If Directory.Exists(txtLogpath.Text) Then
                        logpath = txtLogpath.Text + "\autodeletedatalog_" + path.Replace("\", "-").Replace(":", "") + "_" + Now.ToString("yyyyMMdd") + ".txt"
                    Else
                        logpath = appdir + "\Log\autodeletedatalog_" + path.Replace("\", "-").Replace(":", "") + "_" + Now.ToString("yyyyMMdd") + ".txt"

                    End If

                    DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentAtPeriodWithWait(rowIndex, path, period, waitTime, logpath)), DeleteTaskTasksCancellationTokenSource.Token)
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
                    If Not Int32.TryParse(dataGridView2.Rows(rowIndex).Cells(1).Value, waitTime) Then
                        waitTime = 60
                    End If
                    Dim minimumGB As Integer = dataGridView2.Rows(rowIndex).Cells(2).Value
                    Dim logpath As String = ""
                    If Directory.Exists(txtLogpath.Text) Then
                        logpath = txtLogpath.Text + "\autocheckdisklog_" + driveLetter + "_" + Now.ToString("yyyyMMdd") + ".txt"
                    Else
                        logpath = appdir + "\Log\autocheckdisklog_" + driveLetter + "_" + Now.ToString("yyyyMMdd") + ".txt"
                    End If
                    CheckDiskTask = New Tasks.Task(New Action(Sub() CheckDiskSpaceWithWait(rowIndex, driveLetter, minimumGB, waitTime, logpath)), CheckDiskTaskTasksCancellationTokenSource.Token)
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
            txtLogpath.Invoke(Sub()
                                  txtLogpath.Enabled = False
                              End Sub)
            btnSaveList1.Invoke(Sub()
                                    btnSaveList1.Enabled = False
                                End Sub)
            btnReloadList1.Invoke(Sub()
                                      btnReloadList1.Enabled = False
                                  End Sub)
            btnDelSelectedFolder.Invoke(Sub()
                                            btnDelSelectedFolder.Enabled = False
                                        End Sub)
            btnDelAllFolders.Invoke(Sub()
                                        btnDelAllFolders.Enabled = False
                                    End Sub)
            dataGridView1.ReadOnly = True

            dataGridView2.ReadOnly = True

            dataGridView3.ReadOnly = True

            txtExcludeFileNames.Enabled = False
            txtExcludeFolderNames.Enabled = False

            btnSaveList2.Invoke(Sub()
                                    btnSaveList2.Enabled = False
                                End Sub)
            btnReloadList2.Invoke(Sub()
                                      btnReloadList2.Enabled = False
                                  End Sub)
            btnCheckSelectedDiskFreeSpace.Invoke(Sub()
                                                     btnCheckSelectedDiskFreeSpace.Enabled = False
                                                 End Sub)
            btnCheckAllDiskFreeSpace.Invoke(Sub()
                                                btnCheckAllDiskFreeSpace.Enabled = False
                                            End Sub)

            btnEnableEditExcludeFileList.Invoke(Sub()
                                                    btnEnableEditExcludeFileList.Enabled = False
                                                End Sub)

            btnSaveList3.Invoke(Sub()
                                    btnSaveList3.Enabled = False
                                End Sub)

            btnReloadList3.Invoke(Sub()
                                      btnReloadList3.Enabled = False
                                  End Sub)

            btnEnableEditExcludeFolderList.Invoke(Sub()
                                                      btnEnableEditExcludeFolderList.Enabled = False
                                                  End Sub)

            btnSaveList4.Invoke(Sub()
                                    btnSaveList4.Enabled = False
                                End Sub)

            btnReloadList4.Invoke(Sub()
                                      btnReloadList4.Enabled = False
                                  End Sub)

            chkMonitorDisk.Enabled = False

            chkMonitorFolders.Enabled = False

            chkSkipFileLastCreationTime.Enabled = False

            chkSkipFolderLastCreationTime.Enabled = False

            txtFileLastCreationTimeToSkip.Enabled = False

            txtFolderLastCreationTimeToSkip.Enabled = False

            SaveAllListToolStripMenuItem.Enabled = False

            ReloadAllListToolStripMenuItem.Enabled = False

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
            If CheckDiskTask IsNot Nothing Then
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

            If dataGridView1 IsNot Nothing Then
                For i = 0 To dataGridView1.RowCount - 1

                    Dim rowIndex As Integer = dataGridView1.Rows(i).Cells(0).RowIndex
                    dataGridView1.Invoke(Sub()
                                             dataGridView1.Rows(rowIndex).Cells(3).Value = ""
                                         End Sub)
                Next

            End If
            If dataGridView2 IsNot Nothing Then

                For i = 0 To dataGridView2.RowCount - 1

                    Dim rowIndex As Integer = dataGridView2.Rows(i).Cells(0).RowIndex
                    dataGridView2.Invoke(Sub()
                                             dataGridView2.Rows(rowIndex).Cells(3).Value = ""
                                         End Sub)
                Next

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
            txtLogpath.Invoke(Sub()
                                  txtLogpath.Enabled = True
                              End Sub)
            btnSaveList1.Invoke(Sub()
                                    btnSaveList1.Enabled = True
                                End Sub)
            btnReloadList1.Invoke(Sub()
                                      btnReloadList1.Enabled = True
                                  End Sub)
            btnDelSelectedFolder.Invoke(Sub()
                                            btnDelSelectedFolder.Enabled = True
                                        End Sub)
            btnDelAllFolders.Invoke(Sub()
                                        btnDelAllFolders.Enabled = True
                                    End Sub)

            dataGridView1.ReadOnly = False

            dataGridView2.ReadOnly = False

            dataGridView3.ReadOnly = False

            dataGridView1.Columns(3).ReadOnly = True

            dataGridView2.Columns(3).ReadOnly = True

            txtExcludeFileNames.Enabled = True

            txtExcludeFolderNames.Enabled = True

            btnSaveList2.Invoke(Sub()
                                    btnSaveList2.Enabled = True
                                End Sub)
            btnReloadList2.Invoke(Sub()
                                      btnReloadList2.Enabled = True
                                  End Sub)
            btnCheckSelectedDiskFreeSpace.Invoke(Sub()
                                                     btnCheckSelectedDiskFreeSpace.Enabled = True
                                                 End Sub)
            btnCheckAllDiskFreeSpace.Invoke(Sub()
                                                btnCheckAllDiskFreeSpace.Enabled = True
                                            End Sub)

            btnEnableEditExcludeFileList.Invoke(Sub()
                                                    btnEnableEditExcludeFileList.Enabled = True
                                                End Sub)

            btnSaveList3.Invoke(Sub()
                                    btnSaveList3.Enabled = True
                                End Sub)

            btnReloadList3.Invoke(Sub()
                                      btnReloadList3.Enabled = True
                                  End Sub)

            btnEnableEditExcludeFolderList.Invoke(Sub()
                                                      btnEnableEditExcludeFolderList.Enabled = True
                                                  End Sub)

            btnSaveList4.Invoke(Sub()
                                    btnSaveList4.Enabled = True
                                End Sub)

            btnReloadList4.Invoke(Sub()
                                      btnReloadList4.Enabled = True
                                  End Sub)

            chkMonitorDisk.Enabled = True

            chkMonitorFolders.Enabled = True

            chkSkipFileLastCreationTime.Enabled = True

            chkSkipFolderLastCreationTime.Enabled = True

            txtFileLastCreationTimeToSkip.Enabled = True

            txtFolderLastCreationTimeToSkip.Enabled = True

            SaveAllListToolStripMenuItem.Enabled = True

            ReloadAllListToolStripMenuItem.Enabled = True

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

        Private Sub SaveAllListToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles SaveAllListToolStripMenuItem.Click
            SaveAllMonitorList()
            SaveExcludedFileNames()
            SaveExcludedFolderNames()
            SaveCreationSetting()
        End Sub

        Private Sub ReloadAllListToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ReloadAllListToolStripMenuItem.Click
            LoadAllMonitorList()
            LoadExcludedFileNames()
            LoadExcludedFolderNames()
            LoadCreationSetting()
        End Sub

        Private Sub WriteLog(file As String, content As String)
            If Not Directory.Exists(Path.GetDirectoryName(file)) Then
                Directory.CreateDirectory(Path.GetDirectoryName(file))
            End If
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

        Private Sub btnSaveList1_Click(sender As Object, e As EventArgs) Handles btnSaveList1.Click
            SaveFolderMonitorLIst()
        End Sub

        Private Sub btnSaveList2_Click(sender As Object, e As EventArgs) Handles btnSaveList2.Click
            SaveDiskMonitorList()
        End Sub

        Private Sub btnDelSelectedFolder_Click(sender As Object, e As EventArgs) Handles btnDelSelectedFolder.Click
            If dataGridView1 IsNot Nothing Then
                Dim rowIndex As Integer = dataGridView1.CurrentCell.RowIndex
                Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
                Dim period As Integer = dataGridView1.Rows(rowIndex).Cells(2).Value
                Dim logpath As String = ""
                If Directory.Exists(txtLogpath.Text) Then
                    logpath = txtLogpath.Text + "\autodeletedatalog_" + path.Replace("\", "-").Replace(":", "") + "_" + Now.ToString("yyyyMMdd") + ".txt"
                Else
                    logpath = appdir + "\Log\autodeletedatalog_" + path.Replace("\", "-").Replace(":", "") + "_" + Now.ToString("yyyyMMdd") + ".txt"
                End If
                Dim DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentAtPeriod(path, period, logpath)))
                DeleteTask.Start()
            End If
        End Sub

        Private Sub btnDelAllFolders_Click(sender As Object, e As EventArgs) Handles btnDelAllFolders.Click
            If dataGridView1 IsNot Nothing Then

                For i = 0 To dataGridView1.RowCount - 1

                    Dim rowIndex As Integer = dataGridView1.Rows(i).Cells(0).RowIndex
                    If String.IsNullOrEmpty(dataGridView1.Rows(rowIndex).Cells(0).Value) Then Continue For
                    Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
                    Dim period As Integer = dataGridView1.Rows(rowIndex).Cells(2).Value
                    Dim logpath As String = ""
                    If Directory.Exists(txtLogpath.Text) Then
                        logpath = txtLogpath.Text + "\autodeletedatalog_" + path.Replace("\", "-").Replace(":", "") + "_" + Now.ToString("yyyyMMdd") + ".txt"
                    Else
                        logpath = appdir + "\Log\autodeletedatalog_" + path.Replace("\", "-").Replace(":", "") + "_" + Now.ToString("yyyyMMdd") + ".txt"
                    End If
                    Dim DeleteTask = New Tasks.Task(New Action(Sub() DeleteCurrentAtPeriod(path, period, logpath)))
                    DeleteTask.Start()
                Next

            End If
        End Sub

        Private Sub btnCheckSelectedDiskFreeSpace_Click(sender As Object, e As EventArgs) Handles btnCheckSelectedDiskFreeSpace.Click
            If dataGridView2 IsNot Nothing Then
                Dim rowIndex As Integer = dataGridView2.CurrentCell.RowIndex
                Dim driveLetter As String = dataGridView2.Rows(rowIndex).Cells(0).Value.ToString()
                Dim minimumGB As Integer = dataGridView2.Rows(rowIndex).Cells(2).Value
                Dim logpath As String = ""
                If Directory.Exists(txtLogpath.Text) Then
                    logpath = txtLogpath.Text + "\autocheckdisklog_" + driveLetter + "_" + Now.ToString("yyyyMMdd") + ".txt"
                Else
                    logpath = appdir + "\Log\autocheckdisklog_" + driveLetter + "_" + Now.ToString("yyyyMMdd") + ".txt"
                End If
                Dim CheckDiskTask = New Tasks.Task(New Action(Sub() CheckDiskSpace(driveLetter, minimumGB, logpath)))
                CheckDiskTask.Start()
            End If
        End Sub

        Private Sub btnCheckAllDiskFreeSpace_Click(sender As Object, e As EventArgs) Handles btnCheckAllDiskFreeSpace.Click
            If dataGridView2 IsNot Nothing Then

                For i = 0 To dataGridView2.RowCount - 1

                    Dim rowIndex As Integer = dataGridView2.Rows(i).Cells(0).RowIndex
                    If String.IsNullOrEmpty(dataGridView2.Rows(rowIndex).Cells(0).Value) Then Continue For
                    Dim driveLetter As String = dataGridView2.Rows(rowIndex).Cells(0).Value.ToString()
                    Dim minimumGB As Integer = dataGridView2.Rows(rowIndex).Cells(2).Value
                    Dim logpath As String = ""
                    If Directory.Exists(txtLogpath.Text) Then
                        logpath = txtLogpath.Text + "\autocheckdisklog_" + driveLetter + "_" + Now.ToString("yyyyMMdd") + ".txt"
                    Else
                        logpath = appdir + "\Log\autocheckdisklog_" + driveLetter + "_" + Now.ToString("yyyyMMdd") + ".txt"
                    End If
                    CheckDiskTask = New Tasks.Task(New Action(Sub() CheckDiskSpace(driveLetter, minimumGB, logpath)))
                    CheckDiskTask.Start()
                Next

            End If
        End Sub

        Private Sub btnViewSelectedDiskLog_Click(sender As Object, e As EventArgs) Handles btnViewSelectedDiskLog.Click
            If dataGridView2 IsNot Nothing Then
                Dim rowIndex As Integer = dataGridView2.CurrentCell.RowIndex
                Dim driveLetter As String = dataGridView2.Rows(rowIndex).Cells(0).Value.ToString()
                Dim logpath As String = ""
                If Directory.Exists(txtLogpath.Text) Then
                    logpath = txtLogpath.Text + "\autocheckdisklog_" + driveLetter + "_" + Now.ToString("yyyyMMdd") + ".txt"
                Else
                    logpath = appdir + "\Log\autocheckdisklog_" + driveLetter + "_" + Now.ToString("yyyyMMdd") + ".txt"
                End If
                Try
                    Process.Start(logpath)
                Catch ex As Exception
                End Try

            End If
        End Sub

        Private Sub ClearAllLogsToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ClearAllLogsToolStripMenuItem.Click

            Dim logList As IEnumerable(Of String)
            If Directory.Exists(txtLogpath.Text) Then
                logList = IO.Directory.EnumerateFiles(txtLogpath.Text, "autodeletedatalog*.txt")
            Else
                logList = IO.Directory.EnumerateFiles(appdir, "autodeletedatalog*.txt")
            End If

            For Each log As String In logList
                Try
                    IO.File.Delete(log)
                Catch ex As Exception

                End Try
            Next

            Dim logList2 As IEnumerable(Of String)
            If Directory.Exists(txtLogpath.Text) Then
                logList2 = IO.Directory.EnumerateFiles(txtLogpath.Text, "autocheckdisklog*.txt")
            Else
                logList2 = IO.Directory.EnumerateFiles(appdir, "autocheckdisklog*.txt")
            End If

            For Each log As String In logList2
                Try
                    IO.File.Delete(log)
                Catch ex As Exception

                End Try
            Next
        End Sub

        Private Sub btnViewSelectedFolderLog_Click(sender As Object, e As EventArgs) Handles btnViewSelectedFolderLog.Click
            If dataGridView1 IsNot Nothing Then
                Dim rowIndex As Integer = dataGridView1.CurrentCell.RowIndex
                Dim path As String = dataGridView1.Rows(rowIndex).Cells(0).Value.ToString()
                Dim logpath As String = ""
                If Directory.Exists(txtLogpath.Text) Then
                    logpath = txtLogpath.Text + "\autodeletedatalog_" + path.Replace("\", "-").Replace(":", "") + "_" + Now.ToString("yyyyMMdd") + ".txt"
                Else
                    logpath = appdir + "\Log\autodeletedatalog_" + path.Replace("\", "-").Replace(":", "") + "_" + Now.ToString("yyyyMMdd") + ".txt"
                End If
                Try
                    Process.Start(logpath)
                Catch ex As Exception
                End Try

            End If
        End Sub

        Private Sub btnClearLogs_Click(sender As Object, e As EventArgs) Handles btnClearLogs.Click

            Dim logList As IEnumerable(Of String)
            If Directory.Exists(txtLogpath.Text) Then
                logList = IO.Directory.EnumerateFiles(txtLogpath.Text, "autodeletedatalog*.txt")
            Else
                logList = IO.Directory.EnumerateFiles(appdir, "autodeletedatalog*.txt")
            End If

            For Each log As String In logList
                Try
                    IO.File.Delete(log)
                Catch ex As Exception

                End Try
            Next
        End Sub

        Private Sub btnClearLogs2_Click(sender As Object, e As EventArgs) Handles btnClearLogs2.Click

            Dim logList2 As IEnumerable(Of String)
            If Directory.Exists(txtLogpath.Text) Then
                logList2 = IO.Directory.EnumerateFiles(txtLogpath.Text, "autocheckdisklog*.txt")
            Else
                logList2 = IO.Directory.EnumerateFiles(appdir, "autocheckdisklog*.txt")
            End If

            For Each log As String In logList2
                Try
                    IO.File.Delete(log)
                Catch ex As Exception

                End Try
            Next
        End Sub

        Private Sub btnReloadList1_Click(sender As Object, e As EventArgs) Handles btnReloadList1.Click
            LoadFolderMonitorList()
        End Sub

        Private Sub btnReloadList2_Click(sender As Object, e As EventArgs) Handles btnReloadList2.Click
            LoadDiskMonitiorist()
        End Sub

        Private Sub SaveAllMonitorList()
            Dim rows = From row As DataGridViewRow In dataGridView1.Rows.Cast(Of DataGridViewRow)()
                       Where Not row.IsNewRow
                       Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))
            Try
                Using sw As New IO.StreamWriter("C:\Radiant Vision Systems Data\TrueTest\UserData\foldermonitorlist.csv")
                    For Each r In rows
                        'Dim skip As Boolean = False
                        'For Each item As String In r
                        '	If String.IsNullOrEmpty(item) Or String.IsNullOrWhiteSpace(item) Then
                        '		skip = True
                        '		Exit For
                        '	End If
                        'Next
                        'If skip Then Continue For
                        sw.WriteLine(String.Join(",", r))
                    Next
                End Using
            Catch ex As Exception
                MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\foldermonitorlist.csv is in use, cannot save list")
            End Try

            Dim rows2 = From row As DataGridViewRow In dataGridView2.Rows.Cast(Of DataGridViewRow)()
                        Where Not row.IsNewRow
                        Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))
            Try
                Using sw As New IO.StreamWriter("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist.csv")
                    For Each r In rows2
                        'Dim skip As Boolean = False
                        'For Each item As String In r
                        '	If String.IsNullOrEmpty(item) Or String.IsNullOrWhiteSpace(item) Then
                        '		skip = True
                        '		Exit For
                        '	End If
                        'Next
                        'If skip Then Continue For
                        sw.WriteLine(String.Join(",", r))
                    Next
                End Using
            Catch ex As Exception
                MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist.csv is in use, cannot save list")
            End Try

            Dim rows3 = From row As DataGridViewRow In dataGridView3.Rows.Cast(Of DataGridViewRow)()
                        Where Not row.IsNewRow
                        Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))
            Try
                Using sw As New IO.StreamWriter("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist2.csv")
                    For Each r In rows3
                        'Dim skip As Boolean = False
                        'For Each item As String In r
                        '	If String.IsNullOrEmpty(item) Or String.IsNullOrWhiteSpace(item) Then
                        '		skip = True
                        '		Exit For
                        '	End If
                        'Next
                        'If skip Then Continue For
                        sw.WriteLine(String.Join(",", r))
                    Next
                End Using
            Catch ex As Exception
                MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist2.csv is in use, cannot save list")
            End Try
        End Sub

        Private Sub SaveFolderMonitorLIst()
            Dim rows = From row As DataGridViewRow In dataGridView1.Rows.Cast(Of DataGridViewRow)()
                       Where Not row.IsNewRow
                       Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))
            Try
                Using sw As New IO.StreamWriter("C:\Radiant Vision Systems Data\TrueTest\UserData\foldermonitorlist.csv")
                    For Each r In rows
                        'Dim skip As Boolean = False
                        'For Each item As String In r
                        '	If String.IsNullOrEmpty(item) Or String.IsNullOrWhiteSpace(item) Then
                        '		skip = True
                        '		Exit For
                        '	End If
                        'Next
                        'If skip Then Continue For
                        sw.WriteLine(String.Join(",", r))
                    Next
                End Using
            Catch ex As Exception
                MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\foldermonitorlist.csv is in use, cannot save list")
            End Try
        End Sub

        Private Sub SaveDiskMonitorList()
            Dim rows2 = From row As DataGridViewRow In dataGridView2.Rows.Cast(Of DataGridViewRow)()
                        Where Not row.IsNewRow
                        Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))
            Try
                Using sw As New IO.StreamWriter("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist.csv")
                    For Each r In rows2
                        'Dim skip As Boolean = False
                        'For Each item As String In r
                        '	If String.IsNullOrEmpty(item) Or String.IsNullOrWhiteSpace(item) Then
                        '		skip = True
                        '		Exit For
                        '	End If
                        'Next
                        'If skip Then Continue For
                        sw.WriteLine(String.Join(",", r))
                    Next
                End Using
            Catch ex As Exception
                MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist.csv is in use, cannot save list")
            End Try

            Dim rows3 = From row As DataGridViewRow In dataGridView3.Rows.Cast(Of DataGridViewRow)()
                        Where Not row.IsNewRow
                        Select Array.ConvertAll(row.Cells.Cast(Of DataGridViewCell).ToArray, Function(c) If(c.Value IsNot Nothing, c.Value.ToString, ""))
            Try
                Using sw As New IO.StreamWriter("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist2.csv")
                    For Each r In rows3
                        'Dim skip As Boolean = False
                        'For Each item As String In r
                        '	If String.IsNullOrEmpty(item) Or String.IsNullOrWhiteSpace(item) Then
                        '		skip = True
                        '		Exit For
                        '	End If
                        'Next
                        'If skip Then Continue For
                        sw.WriteLine(String.Join(",", r))
                    Next
                End Using
            Catch ex As Exception
                MessageBox.Show("C:\Radiant Vision Systems Data\TrueTest\UserData\diskmonitorlist2.csv is in use, cannot save list")
            End Try
        End Sub

        Sub SaveExcludedFileNames()
            Try
                ' Write the text from the text box to the specified file
                File.WriteAllText("C:\Radiant Vision Systems Data\TrueTest\UserData\autodelete_exclude_files.txt", txtExcludeFileNames.Text)
            Catch ex As Exception
                ' Handle the exception if writing fails
                MessageBox.Show("Error writing text box lines to file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
        Sub SaveExcludedFolderNames()
            Try
                ' Write the text from the text box to the specified file
                File.WriteAllText("C:\Radiant Vision Systems Data\TrueTest\UserData\autodelete_exclude_folders.txt", txtExcludeFolderNames.Text)
            Catch ex As Exception
                ' Handle the exception if writing fails
                MessageBox.Show("Error writing text box lines to file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
        Private Sub btnSaveList3_Click(sender As Object, e As EventArgs) Handles btnSaveList3.Click
            SaveExcludedFileNames()
        End Sub

        Sub LoadExcludedFileNames()
            If File.Exists("C:\Radiant Vision Systems Data\TrueTest\UserData\autodelete_exclude_files.txt") Then
                Try
                    ' Read all lines from the file and set them as the text of the text box
                    txtExcludeFileNames.Text = File.ReadAllText("C:\Radiant Vision Systems Data\TrueTest\UserData\autodelete_exclude_files.txt")
                Catch ex As Exception
                    ' Handle the exception if reading fails
                    MessageBox.Show("Error loading lines from file to text box: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            Else

            End If
        End Sub
        Sub LoadExcludedFolderNames()
            If File.Exists("C:\Radiant Vision Systems Data\TrueTest\UserData\autodelete_exclude_folders.txt") Then
                Try
                    ' Read all lines from the file and set them as the text of the text box
                    txtExcludeFolderNames.Text = File.ReadAllText("C:\Radiant Vision Systems Data\TrueTest\UserData\autodelete_exclude_folders.txt")
                Catch ex As Exception
                    ' Handle the exception if reading fails
                    MessageBox.Show("Error loading lines from file to text box: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            Else

            End If
        End Sub
        Private Sub btnReloadList3_Click(sender As Object, e As EventArgs) Handles btnReloadList3.Click
            LoadExcludedFileNames()
        End Sub

        Private Sub btnEnableEditExcludFileList_Click(sender As Object, e As EventArgs) Handles btnEnableEditExcludeFileList.Click
            ' Toggle the read-only state of the TextBox
            txtExcludeFileNames.ReadOnly = Not txtExcludeFileNames.ReadOnly

            ' Change the button text based on the read-only state of the TextBox
            If txtExcludeFileNames.ReadOnly Then
                btnEnableEditExcludeFileList.Text = "Edit List"
            Else
                btnEnableEditExcludeFileList.Text = "Done Editing"
            End If
        End Sub

        Private Sub btnEnableEditExcludeFolderList_Click(sender As Object, e As EventArgs) Handles btnEnableEditExcludeFolderList.Click
            ' Toggle the read-only state of the TextBox
            txtExcludeFolderNames.ReadOnly = Not txtExcludeFolderNames.ReadOnly

            ' Change the button text based on the read-only state of the TextBox
            If txtExcludeFolderNames.ReadOnly Then
                btnEnableEditExcludeFolderList.Text = "Edit List"
            Else
                btnEnableEditExcludeFolderList.Text = "Done Editing"
            End If
        End Sub

        Private Sub btnSaveList4_Click(sender As Object, e As EventArgs) Handles btnSaveList4.Click
            SaveExcludedFolderNames()
        End Sub

        Private Sub btnReloadList4_Click(sender As Object, e As EventArgs) Handles btnReloadList4.Click
            LoadExcludedFolderNames()
        End Sub

        Sub SaveCreationSetting()
            Try
                ' Define the path for the settings file
                Dim settingsFilePath As String = "C:\Radiant Vision Systems Data\TrueTest\UserData\autodelete_creation_settings.txt"

                ' Initialize the content string
                Dim content As New StringBuilder()

                ' Check and add the SkipFileLastCreationTime setting
                content.AppendLine($"SkipFileLastCreationTime={If(chkSkipFileLastCreationTime.Checked, "true", "false")}")

                ' Check and add the SkipFolderLastCreationTime setting
                content.AppendLine($"SkipFolderLastCreationTime={If(chkSkipFolderLastCreationTime.Checked, "true", "false")}")

                ' Add the FileLastCreationTime setting
                content.AppendLine($"FileLastCreationTime={txtFileLastCreationTimeToSkip.Text.Trim()}")

                ' Add the FolderLastCreationTime setting
                content.AppendLine($"FolderLastCreationTime={txtFolderLastCreationTimeToSkip.Text.Trim()}")

                ' Write the content to the settings file
                File.WriteAllText(settingsFilePath, content.ToString())
            Catch ex As Exception
                ' Handle the exception if writing fails
                MessageBox.Show("Error writing settings to file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub



        Private Sub btnSaveTimeCreation_Click(sender As Object, e As EventArgs) Handles btnSaveTimeCreation.Click
            SaveCreationSetting()
        End Sub
        Sub LoadCreationSetting()
            ' Define the path for the settings file
            Dim settingsFilePath As String = "C:\Radiant Vision Systems Data\TrueTest\UserData\autodelete_creation_settings.txt"

            If File.Exists(settingsFilePath) Then
                Try
                    ' Read all lines from the file
                    Dim lines As String() = File.ReadAllLines(settingsFilePath)

                    ' Loop through each line
                    For Each line As String In lines
                        ' Split the line into key and value
                        Dim parts As String() = line.Split("="c)

                        ' Check if the line contains SkipFileLastCreationTime setting
                        If parts(0) = "SkipFileLastCreationTime" Then
                            ' Set the checkbox state based on the value
                            chkSkipFileLastCreationTime.Checked = parts(1).ToLower() = "true"
                        End If

                        ' Check if the line contains SkipFolderLastCreationTime setting
                        If parts(0) = "SkipFolderLastCreationTime" Then
                            ' Set the checkbox state based on the value
                            chkSkipFolderLastCreationTime.Checked = parts(1).ToLower() = "true"
                        End If

                        ' Check if the line contains FileLastCreationTime setting
                        If parts(0) = "FileLastCreationTime" Then
                            ' Set the text box text based on the value
                            txtFileLastCreationTimeToSkip.Text = parts(1)
                        End If

                        ' Check if the line contains FolderLastCreationTime setting
                        If parts(0) = "FolderLastCreationTime" Then
                            ' Set the text box text based on the value
                            txtFolderLastCreationTimeToSkip.Text = parts(1)
                        End If
                    Next
                Catch ex As Exception
                    ' Handle the exception if reading fails
                    MessageBox.Show("Error loading settings from file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            Else
                ' If the settings file doesn't exist, do nothing
            End If
        End Sub


        Private Sub btnLoadTimeCreation_Click(sender As Object, e As EventArgs) Handles btnLoadTimeCreation.Click
            LoadCreationSetting()
        End Sub
    End Class
End Namespace
