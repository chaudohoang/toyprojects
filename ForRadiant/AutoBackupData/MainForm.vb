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
Imports System.Collections.Concurrent

Namespace AutoDeleteData
    Partial Public Class MainForm
        Inherits Form
        Public apppath As String
        Public appdir As String
        Public settingPath As String
        Private allowVisible As Boolean = True

        Private logPath As String = ""
        Private maxLogLines As Integer = 200
        Private backupSettingsFile As String = "C:\Radiant Vision Systems Data\TrueTest\UserData\AutoBackup\backup_settings.txt"
        Private logLines As New List(Of String)
        Private watchers As New Dictionary(Of String, FileSystemWatcher)() ' Use Dictionary instead of List
        ' Dictionary to store excluded folders per source path
        Private excludedPaths As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)
        ' Dictionary to store excluded files per source path
        Private excludedFiles As New Dictionary(Of String, List(Of String))(StringComparer.OrdinalIgnoreCase)

        Private monitoring As Boolean = False ' Monitoring state
        Private processedFiles As New ConcurrentDictionary(Of String, DateTime)()
        Private processedDirectories As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Private lastCopiedFiles As New Dictionary(Of String, DateTime)
        Private logLock As New Object()

        Public Sub New()
            apppath = Assembly.GetExecutingAssembly().Location
            appdir = Path.GetDirectoryName(apppath)
            settingPath = Path.Combine(appdir, "settingAutoBackupData.txt")
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

        Protected Overrides Sub SetVisibleCore(ByVal value As Boolean)
            If Not allowVisible Then
                value = False
                If Not IsHandleCreated Then CreateHandle()
            End If
            MyBase.SetVisibleCore(value)
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


        Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
            SetVersionInfo()
            LoadSettings()
            LoadBackupSettings()
            SetLogFilePath()

            If startMinimizedToolStripMenuItem.Checked = True Then
                Me.WindowState = FormWindowState.Minimized
            End If

            If MonitorAutomaticallyToolStripMenuItem.Checked = True Then
                startMonitor()
            End If

        End Sub

        Private Sub SaveBackupSettings()
            Try
                Using writer As New StreamWriter(backupSettingsFile, False)
                    For Each row As DataGridViewRow In dataGridView1.Rows
                        If Not row.IsNewRow Then
                            Dim values = {
                        If(row.Cells(0).Value, ""),
                        If(row.Cells(1).Value, ""),
                        If(row.Cells(2).Value, ""),
                        If(row.Cells(3).Value, "")
                    }
                            writer.WriteLine(String.Join("|", values))
                        End If
                    Next
                End Using

            Catch ex As Exception
                MessageBox.Show("Error saving settings: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub

        Private Sub LoadBackupSettings()
            If Not File.Exists(backupSettingsFile) Then Return
            dataGridView1.Rows.Clear()

            For Each line In File.ReadAllLines(backupSettingsFile)
                ' Split using comma `,`
                Dim parts = line.Split("|"c)

                ' Ensure at least 3 values are present (avoid incomplete rows)
                If parts.Length < 3 Then Continue For

                ' Create an array with exactly 4 values (fill missing ones with empty strings)
                Dim fixedParts(3) As String
                For i As Integer = 0 To Math.Min(parts.Length - 1, 3)
                    fixedParts(i) = parts(i)
                Next

                ' Add row to DataGridView
                dataGridView1.Rows.Add(fixedParts)
            Next
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
                notifyIcon1.BalloonTipText = "AutoBackupData still running and minimized to tray"
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





        Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
            ' Check if the close reason is user closing the form
            If e.CloseReason = CloseReason.UserClosing Then
                ' Minimize the form instead of closing
                e.Cancel = True
                Me.WindowState = FormWindowState.Minimized
            End If
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
            btnSaveSettings.Invoke(Sub()
                                       btnSaveSettings.Enabled = False
                                   End Sub)
            btnReloadSettings.Invoke(Sub()
                                         btnReloadSettings.Enabled = False
                                     End Sub)

            dataGridView1.ReadOnly = True

            If monitoring Then Exit Sub ' Prevent multiple starts
            monitoring = True
            AppendLog("Monitoring started.")
            Try
                ' Dispose of existing watchers
                For Each watcher In watchers.Values
                    watcher.EnableRaisingEvents = False
                    watcher.Dispose()
                Next
                watchers.Clear()
                excludedPaths.Clear() ' Clear old exclusions
                excludedFiles.Clear()

                ' Loop through DataGridView rows
                For Each row As DataGridViewRow In dataGridView1.Rows
                    If row.IsNewRow Then Continue For

                    Dim sourceFolder = row.Cells(0).Value?.ToString()
                    If String.IsNullOrWhiteSpace(sourceFolder) OrElse Not Directory.Exists(sourceFolder) Then Continue For

                    ' Extract excluded folders (Third cell)
                    Dim excludedFolders As String() = row.Cells(2).Value?.ToString()?.Split(","c).Select(Function(f) f.Trim()).ToArray()

                    ' Extract excluded files (Fourth cell)
                    Dim excludedFileNames As String() = row.Cells(3).Value?.ToString()?.Split(","c).Select(Function(f) f.Trim()).ToArray()

                    ' Store folder exclusions
                    If excludedFolders IsNot Nothing AndAlso excludedFolders.Length > 0 Then
                        excludedPaths(sourceFolder) = excludedFolders.ToList()
                    End If

                    ' Store file exclusions
                    If excludedFileNames IsNot Nothing AndAlso excludedFileNames.Length > 0 Then
                        excludedFiles(sourceFolder) = excludedFileNames.ToList()
                    End If

                    ' Create a new watcher if not already added
                    If Not watchers.ContainsKey(sourceFolder) Then
                        Dim watcher As New FileSystemWatcher(sourceFolder) With {
                            .IncludeSubdirectories = True,
                            .EnableRaisingEvents = True,
                            .NotifyFilter = NotifyFilters.FileName Or NotifyFilters.DirectoryName Or NotifyFilters.LastWrite
                        }

                        AddHandler watcher.Created, AddressOf OnFileCreated
                        AddHandler watcher.Changed, AddressOf OnFileChanged

                        watchers.Add(sourceFolder, watcher)
                    End If
                Next
            Catch ex As Exception
                AppendLog($"Error setting up file watchers: {ex.Message}")
            Finally
                If Not monitoring Then watchers.Clear()
            End Try


        End Sub

        Private Sub SetLogFilePath()
            Dim currentDate As String = Now.ToString("yyyyMMdd")

            Dim logFolder As String
            If Directory.Exists(txtLogpath.Text) Then
                logFolder = txtLogpath.Text
            Else
                logFolder = appdir & "\log"
            End If

            ' Ensure log folder exists
            If Not Directory.Exists(logFolder) Then
                Directory.CreateDirectory(logFolder)
            End If

            ' Set the log file path
            logPath = Path.Combine(logFolder, "autobackuplog_" & currentDate & ".txt")
        End Sub

        Private Sub AppendLog(message As String)
            ' Ensure the log path is updated before writing
            SetLogFilePath()

            Dim attempts As Integer = 0
            Dim success As Boolean = False

            Do While attempts < 5 AndAlso Not success
                Try
                    ' Try to write the log entry
                    File.AppendAllText(logPath, $"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}")
                    success = True ' If successful, exit loop
                Catch ex As IOException
                    attempts += 1
                    If attempts >= 5 Then
                        ' If max attempts reached, log to Debug or Console
                        Debug.WriteLine($"Failed to write log: {ex.Message}")
                    Else
                        ' Wait 200ms before retrying
                        Threading.Thread.Sleep(200)
                    End If
                End Try
            Loop
        End Sub


        Private Sub DeleteAllLogs()
            Dim logFolder As String

            ' Determine the log folder path
            If Directory.Exists(txtLogpath.Text) Then
                logFolder = txtLogpath.Text
            Else
                logFolder = appdir & "\log"
            End If

            ' Ensure the log folder exists before attempting to delete files
            If Directory.Exists(logFolder) Then
                Try
                    ' Get all log files matching the naming pattern
                    Dim logFiles = Directory.GetFiles(logFolder, "autobackuplog_*.txt")

                    ' Delete each log file
                    For Each file In logFiles
                        IO.File.Delete(file)
                    Next

                    'MessageBox.Show("All log files have been deleted successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Catch ex As Exception
                    'MessageBox.Show("Error deleting log files: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Try
            Else
                'MessageBox.Show("Log folder does not exist.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        End Sub




        Private Sub stopMonitor()


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
            btnSaveSettings.Invoke(Sub()
                                       btnSaveSettings.Enabled = True
                                   End Sub)
            btnReloadSettings.Invoke(Sub()
                                         btnReloadSettings.Enabled = True
                                     End Sub)


            dataGridView1.ReadOnly = False

            If Not monitoring Then Exit Sub ' Prevent stopping when already stopped

            AppendLog("Stopping monitoring...")
            For Each key In watchers.Keys.ToList() ' Ensure keys don't change during iteration
                Dim watcher = watchers(key)
                If watcher IsNot Nothing Then
                    AppendLog($"Disposing watcher for: {key}")
                    ' Remove handlers explicitly
                    RemoveHandler watcher.Created, AddressOf OnFileCreated
                    RemoveHandler watcher.Changed, AddressOf OnFileChanged

                    ' Dispose the watcher to ensure it stops listening to events
                    Try
                        watcher.Dispose()
                    Catch ex As Exception
                        AppendLog($"Error disposing watcher for {key}: {ex.Message}")
                    End Try
                End If
            Next

            watchers.Clear() ' Ensure no remaining watchers
            monitoring = False
            AppendLog("Monitoring stopped.")

        End Sub

        Private Async Sub OnFileCreated(sender As Object, e As FileSystemEventArgs)
            Await Task.Delay(500) ' Allow time for changes before processing
            ProcessFileChange(e)
        End Sub
        Private Async Sub OnFileChanged(sender As Object, e As FileSystemEventArgs)
            ' Ignore changes if the file was just copied
            If lastCopiedFiles.ContainsKey(e.FullPath) AndAlso
       (DateTime.Now - lastCopiedFiles(e.FullPath)).TotalSeconds < 2 Then Exit Sub
            Await Task.Delay(500)
            ProcessFileChange(e)
        End Sub

        Private Function ShouldExclude(filePath As String) As Boolean
            ' Extract directory and filename from the given file path
            Dim fileDirectory As String = Path.GetDirectoryName(filePath)
            Dim fileName As String = Path.GetFileName(filePath)

            ' Loop through each source path in the exclusions dictionary
            For Each Source In excludedPaths.Keys
                ' Ensure we only check exclusions for matching source folders
                If filePath.StartsWith(Source, StringComparison.OrdinalIgnoreCase) Then

                    ' **Check folder exclusions** (Check if any excluded folder name is part of the path)
                    If excludedPaths.ContainsKey(Source) Then
                        For Each excludedFolder In excludedPaths(Source)
                            If fileDirectory.IndexOf(excludedFolder, StringComparison.OrdinalIgnoreCase) >= 0 Then
                                AppendLog($"Excluded (Folder): {filePath}")
                                Return True
                            End If
                        Next
                    End If

                    ' **Check file exclusions** (Check if filename contains any excluded keyword)
                    If excludedFiles.ContainsKey(Source) Then
                        If excludedFiles(Source).Any(Function(excludedFile) fileName.IndexOf(excludedFile, StringComparison.OrdinalIgnoreCase) >= 0) Then
                            AppendLog($"Excluded (File): {filePath}")
                            Return True
                        End If
                    End If
                End If
            Next

            ' If not excluded, allow processing
            Return False
        End Function

        Private Sub ProcessFileChange(e As FileSystemEventArgs)
            If e Is Nothing OrElse String.IsNullOrWhiteSpace(e.FullPath) Then
                AppendLog("Error: FileSystemEventArgs is null or empty.")
                Return
            End If

            Dim filePath As String = e.FullPath

            ' Ignore duplicate events within a short time window (e.g., 2 seconds)
            Dim key As String = filePath ' Track by file path only
            Dim lastProcessed As DateTime = DateTime.MinValue ' Initialize default value

            ' Check if this file was recently processed
            If processedFiles.TryGetValue(key, lastProcessed) Then
                If (DateTime.Now - lastProcessed).TotalMilliseconds < 2000 Then
                    'AppendLog($"Skipping duplicate event for: {filePath}")
                    Exit Sub
                End If
            End If

            processedFiles(key) = DateTime.Now ' Update last processed time

            ' Ensure DataGridView is initialized
            If dataGridView1 Is Nothing OrElse dataGridView1.Rows Is Nothing Then
                AppendLog("Error: DataGridView1 is not initialized.")
                Return
            End If

            ' Iterate through each backup rule in DataGridView
            For Each row As DataGridViewRow In dataGridView1.Rows
                If Not row.IsNewRow Then
                    Dim sourcePath As String = row.Cells(0).Value?.ToString()
                    Dim destinationPath As String = row.Cells(1).Value?.ToString()

                    If String.IsNullOrWhiteSpace(sourcePath) OrElse String.IsNullOrWhiteSpace(destinationPath) Then
                        'AppendLog("Skipping row: source or destination path is empty.")
                        Continue For
                    End If

                    If e.FullPath.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase) Then
                        Dim relativePath As String = e.FullPath.Substring(sourcePath.Length).TrimStart("\"c)
                        Dim destFile As String = Path.Combine(destinationPath, relativePath)

                        Try


                            ' Handle directories separately (prevent duplicate processing)
                            If Directory.Exists(e.FullPath) Then

                                Dim excludedFolderList As List(Of String) = excludedPaths(sourcePath)

                                Dim shouldExcludeFolder As Boolean = excludedFolderList.Any(Function(excluded) e.FullPath.IndexOf(excluded, StringComparison.OrdinalIgnoreCase) >= 0)

                                If shouldExcludeFolder Then
                                    AppendLog($"Skipped directory (excluded): {e.FullPath}")
                                ElseIf Not processedDirectories.Contains(e.FullPath) Then
                                    processedDirectories.Add(e.FullPath)
                                    ' Ensure destination folder exists
                                    Dim destDir As String = Path.GetDirectoryName(destFile)
                                    If Not Directory.Exists(destDir) Then Directory.CreateDirectory(destDir)
                                    DirectoryCopy(e.FullPath, destFile)
                                    AppendLog($"Copied directory: {e.FullPath} -> {destFile}")
                                Else
                                    'AppendLog($"Skipping duplicate directory copy: {e.FullPath}")
                                End If
                            Else

                                Dim excludedFileList As List(Of String) = excludedFiles(sourcePath)
                                Dim excludedFolderList As List(Of String) = excludedPaths(sourcePath)

                                ' Check if e.FullPath contains any excluded pattern
                                Dim shouldExcludeFile As Boolean = excludedFileList.Any(Function(excluded) e.FullPath.IndexOf(excluded, StringComparison.OrdinalIgnoreCase) >= 0) _
                                OrElse excludedFolderList.Any(Function(excluded) e.FullPath.IndexOf(excluded, StringComparison.OrdinalIgnoreCase) >= 0)

                                If shouldExcludeFile Then
                                    AppendLog($"Skipped file (excluded): {e.FullPath}")
                                ElseIf Not File.Exists(destFile) OrElse File.GetLastWriteTime(e.FullPath) > File.GetLastWriteTime(destFile) Then
                                    ' Use the retry function instead of File.Copy
                                    ' Ensure destination folder exists
                                    Dim destDir As String = Path.GetDirectoryName(destFile)
                                    If Not Directory.Exists(destDir) Then Directory.CreateDirectory(destDir)
                                    CopyFileWithRetry(e.FullPath, destFile)
                                ElseIf Not processedFiles.ContainsKey($"Skipped-{filePath}") Then

                                    'AppendLog($"Skipped: {e.FullPath} (No changes detected)")
                                    processedFiles($"Skipped-{filePath}") = DateTime.Now ' Prevent excessive logs
                                End If

                            End If
                        Catch ex As Exception
                            AppendLog($"Error copying {e.FullPath}: {ex.Message}")
                        End Try
                    End If
                End If
            Next
        End Sub
        Private Sub CopyFileWithRetry(sourcePath As String, destPath As String)
            Dim maxRetries As Integer = 5
            Dim retryDelay As Integer = 1000 ' 1 second delay

            For attempt As Integer = 1 To maxRetries
                Try
                    ' Attempt to copy the file only if necessary
                    If Not File.Exists(destPath) OrElse File.GetLastWriteTime(sourcePath) > File.GetLastWriteTime(destPath) Then

                        ' Ensure the destination directory exists ONLY if the file is being copied
                        Dim destDir As String = Path.GetDirectoryName(destPath)
                        If Not Directory.Exists(destDir) Then
                            Directory.CreateDirectory(destDir)
                        End If

                        ' Copy the file
                        File.Copy(sourcePath, destPath, True)
                        AppendLog($"Copied: {sourcePath} -> {destPath}")
                    End If

                    Exit Sub
                Catch ex As IOException
                    If attempt < maxRetries Then
                        AppendLog($"Retrying copy ({attempt}/{maxRetries}): {sourcePath}")
                        Thread.Sleep(retryDelay)
                    Else
                        AppendLog($"Error copying {sourcePath}: {ex.Message}")
                    End If
                End Try
            Next
        End Sub

        Private Sub DirectoryCopy(sourceDir As String, destDir As String)
            Try
                ' Create the destination directory if it doesn't exist
                If Not Directory.Exists(destDir) Then
                    Directory.CreateDirectory(destDir)
                End If

                ' Copy all files
                For Each file As String In Directory.GetFiles(sourceDir)
                    Dim fileName As String = Path.GetFileName(file)
                    Dim destFile As String = Path.Combine(destDir, fileName)
                    IO.File.Copy(file, destFile, True)
                Next

                ' Copy all subdirectories recursively
                For Each subDir As String In Directory.GetDirectories(sourceDir)
                    Dim subDirName As String = Path.GetFileName(subDir)
                    Dim newDestDir As String = Path.Combine(destDir, subDirName)
                    DirectoryCopy(subDir, newDestDir)
                Next

            Catch ex As Exception
                AppendLog($"Error copying directory {sourceDir} -> {destDir}: {ex.Message}")
            End Try
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

        Private Sub btnClearLogs_Click(sender As Object, e As EventArgs) Handles btnClearLogs.Click
            DeleteAllLogs()

        End Sub

        Private Sub btnSaveSettings_Click(sender As Object, e As EventArgs) Handles btnSaveSettings.Click
            SaveBackupSettings()
        End Sub

        Private Sub btnReloadSettings_Click(sender As Object, e As EventArgs) Handles btnReloadSettings.Click
            LoadBackupSettings()
        End Sub

        Private Sub btnViewLog_Click(sender As Object, e As EventArgs) Handles btnViewLog.Click

            Try
                    Process.Start(logpath)
                Catch ex As Exception
                    ' Optional: Handle/log the exception
                End Try

        End Sub
    End Class
End Namespace
