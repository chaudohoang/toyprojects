Imports System.IO
Imports System.Text
Imports System.Windows.Forms
Imports System.Collections.Concurrent
Imports System.Threading
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports AutoCopyData.AutoCopyData

Public Class MainForm
    Private logFile As String = "backup_log.txt"
    Private maxLogLines As Integer = 200
    Private logFolder As String = "logs"
    Private settingsFile As String = "C:\Radiant Vision Systems Data\TrueTest\UserData\AutoBackup\settings.txt"
    Private logLines As New List(Of String)
    Private watchers As New Dictionary(Of String, FileSystemWatcher)() ' Use Dictionary instead of List
    Private monitoring As Boolean = False ' Monitoring state
    Private processedFiles As New ConcurrentDictionary(Of String, DateTime)()
    Private processedDirectories As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
    Private lastCopiedFiles As New Dictionary(Of String, DateTime)
    Private logLock As New Object()

    ' Entry point for the application (MainForm_Load)
    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetVersionInfo()
        LoadSettings()
        ToggleMonitoring()
    End Sub

    ' Existing method that handles other application logic
    Private Sub SetVersionInfo()
        Dim versionInfo As Version = Assembly.GetExecutingAssembly().GetName().Version
        Dim startDate As Date = New DateTime(2000, 1, 1)
        Dim diffDays = versionInfo.Build
        Dim computedDate = startDate.AddDays(diffDays)
        Dim lastBuilt As String = computedDate.ToShortDateString()
        Text = String.Format("{0} - {1}", Text, versionInfo.ToString())
    End Sub

    Private Sub LoadSettings()
        If Not File.Exists(settingsFile) Then Return
        DataGridView1.Rows.Clear()

        For Each line In File.ReadAllLines(settingsFile)
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
            DataGridView1.Rows.Add(fixedParts)
        Next
    End Sub
    Private Sub SaveSettings()
        Try
            Using writer As New StreamWriter(settingsFile, False)
                For Each row As DataGridViewRow In DataGridView1.Rows
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
            ' Only restart watchers if monitoring is active
            If monitoring Then SetupFileWatchers()
        Catch ex As Exception
            MessageBox.Show("Error saving settings: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub ToggleMonitoring()
        If monitoring Then
            ' Stop Monitoring
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
            BtnRun.Text = "Run"
            LblStatus.Text = "Status: Stopped"
            LblStatus.ForeColor = Color.Red
            AppendLog("Monitoring stopped.")
        Else
            ' Start Monitoring
            SetupFileWatchers() ' Ensure unique watchers only

            monitoring = True
            BtnRun.Text = "Stop"
            LblStatus.Text = "Status: Running"
            LblStatus.ForeColor = Color.Green
            AppendLog("Monitoring started.")
        End If
    End Sub

    Private Sub SetupFileWatchers()
        Try
            For Each watcher In watchers.Values
                watcher.EnableRaisingEvents = False
                watcher.Dispose()
            Next
            watchers.Clear()

            For Each row As DataGridViewRow In DataGridView1.Rows
                If row.IsNewRow Then Continue For
                Dim sourceFolder = row.Cells(0).Value?.ToString()
                If String.IsNullOrWhiteSpace(sourceFolder) OrElse Not Directory.Exists(sourceFolder) Then Continue For

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
            ' Ensure watchers dictionary is cleared to avoid memory leaks
            If Not monitoring Then watchers.Clear()
        End Try
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
                AppendLog($"Skipping duplicate event for: {filePath}")
                Exit Sub
            End If
        End If

        processedFiles(key) = DateTime.Now ' Update last processed time

        ' Ensure DataGridView is initialized
        If DataGridView1 Is Nothing OrElse DataGridView1.Rows Is Nothing Then
            AppendLog("Error: DataGridView1 is not initialized.")
            Return
        End If

        ' Iterate through each backup rule in DataGridView
        For Each row As DataGridViewRow In DataGridView1.Rows
            If Not row.IsNewRow Then
                Dim sourcePath As String = row.Cells(0).Value?.ToString()
                Dim destinationPath As String = row.Cells(1).Value?.ToString()

                If String.IsNullOrWhiteSpace(sourcePath) OrElse String.IsNullOrWhiteSpace(destinationPath) Then
                    AppendLog("Skipping row: source or destination path is empty.")
                    Continue For
                End If

                If e.FullPath.StartsWith(sourcePath, StringComparison.OrdinalIgnoreCase) Then
                    Dim relativePath As String = e.FullPath.Substring(sourcePath.Length).TrimStart("\"c)
                    Dim destFile As String = Path.Combine(destinationPath, relativePath)

                    Try
                        ' Ensure destination folder exists
                        Dim destDir As String = Path.GetDirectoryName(destFile)
                        If Not Directory.Exists(destDir) Then Directory.CreateDirectory(destDir)

                        ' Handle directories separately (prevent duplicate processing)
                        If Directory.Exists(e.FullPath) Then
                            If Not processedDirectories.Contains(e.FullPath) Then
                                processedDirectories.Add(e.FullPath) ' Mark directory as processed
                                DirectoryCopy(e.FullPath, destFile)
                                AppendLog($"Copied directory: {e.FullPath} -> {destFile}")
                            Else
                                AppendLog($"Skipping duplicate directory copy: {e.FullPath}")
                            End If
                        Else
                            If Not File.Exists(destFile) OrElse File.GetLastWriteTime(e.FullPath) > File.GetLastWriteTime(destFile) Then
                                ' Use the retry function instead of File.Copy
                                CopyFileWithRetry(e.FullPath, destFile)
                            ElseIf Not processedFiles.ContainsKey($"Skipped-{filePath}") Then

                                AppendLog($"Skipped: {e.FullPath} (No changes detected)")
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
                ' Ensure the destination directory exists
                Dim destDir As String = Path.GetDirectoryName(destPath)
                If Not Directory.Exists(destDir) Then
                    Directory.CreateDirectory(destDir)
                End If

                ' Attempt to copy the file
                File.Copy(sourcePath, destPath, True)
                AppendLog($"Copied: {sourcePath} -> {destPath}")
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
    Private Sub UpdateLogSize()
        If File.Exists(logFile) Then
            Dim fileSize As Long = New FileInfo(logFile).Length
            LabelLogSize.Text = $"Log Size: {fileSize / 1024:N2} KB"

            If fileSize > (1024 * 1024) Then
                Dim archiveFile As String = logFile.Replace(".txt", $"_{DateTime.Now:HHmmss}.txt")
                Dim success As Boolean = False

                For attempt As Integer = 1 To 5
                    Try
                        File.Move(logFile, archiveFile)
                        success = True
                        Exit For
                    Catch ex As IOException When attempt < 5
                        Threading.Thread.Sleep(500)
                    End Try
                Next

                If success Then AppendLog("Log rotated due to size limit.")
            End If
        Else
            LabelLogSize.Text = "Log Size: 0 KB"
        End If
    End Sub
    Private Sub AppendLog(message As String)
        Dim logText As String = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}"
        logFile = Path.Combine(logFolder, $"backup_{DateTime.Now:yyyy-MM-dd}.txt")

        ' Check if the directory exists, and create it if it doesn't
        If Not Directory.Exists(logFolder) Then
            Directory.CreateDirectory(logFolder)
        End If

        SyncLock logLock
            logLines.Add(logText)
            Dim success As Boolean = False
            For attempt As Integer = 1 To 5
                Try
                    Using writer As New StreamWriter(logFile, True, Encoding.UTF8, 4096)
                        If logLines.Count > 0 Then
                            For Each log In logLines
                                writer.WriteLine(log)
                            Next
                            logLines.Clear()
                        End If
                        writer.WriteLine(logText)
                    End Using
                    success = True
                    Exit For
                Catch ex As IOException When attempt < 5
                    Threading.Thread.Sleep(500)
                End Try
            Next
            If Not success Then logLines.Add(logText)
        End SyncLock

        If TextBoxLog.InvokeRequired Then
            TextBoxLog.Invoke(Sub() AppendLogToTextBox(logText))
        Else
            AppendLogToTextBox(logText)
        End If
    End Sub
    Private Sub AppendLogToTextBox(message As String)
        If TextBoxLog.Lines.Length >= maxLogLines Then
            TextBoxLog.Text = String.Join(Environment.NewLine, TextBoxLog.Lines.Skip(1)) ' Remove oldest log
        End If

        TextBoxLog.AppendText(message & Environment.NewLine)
    End Sub
    Private Sub ButtonClearLog_Click(sender As Object, e As EventArgs) Handles ButtonClearLog.Click
        If Directory.Exists(logFolder) Then
            For Each file As String In Directory.GetFiles(logFolder, "backup_*.txt")
                IO.File.Delete(file)
            Next
        End If
        logLines.Clear()
        TextBoxLog.Clear()
        UpdateLogSize()
    End Sub

    Private Sub ButtonOpenLogFolder_Click(sender As Object, e As EventArgs) Handles ButtonOpenLogFolder.Click
        Process.Start("explorer.exe", logFolder)
    End Sub
    Private Sub ButtonMinimizeToTray_Click(sender As Object, e As EventArgs) Handles ButtonMinimizeToTray.Click
        NotifyIcon1.Visible = True
        Me.Hide()
    End Sub
    Private Sub NotifyIcon1_DoubleClick(sender As Object, e As EventArgs) Handles NotifyIcon1.DoubleClick
        Me.Show()
        Me.WindowState = FormWindowState.Normal
        Me.BringToFront()
        Me.Activate()
        NotifyIcon1.Visible = False
    End Sub

    Private Sub MainForm_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        NotifyIcon1.Visible = False

    End Sub

    ' Save Settings Button Click
    Private Sub btnSaveList_Click(sender As Object, e As EventArgs) Handles btnSaveList.Click
        SaveSettings()
    End Sub
    ' Load Settings Button Click
    Private Sub btnLoadList_Click(sender As Object, e As EventArgs) Handles btnLoadList.Click
        LoadSettings()
    End Sub
    Private Sub BtnRun_Click(sender As Object, e As EventArgs) Handles BtnRun.Click
        ToggleMonitoring()
    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click
        SaveSettings()
        Application.Exit()
    End Sub
End Class
