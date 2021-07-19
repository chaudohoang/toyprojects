Imports System.IO
Imports System.Net
Imports System.Reflection
Public Class Form1
    Private Sub btnUpload_Click(sender As Object, e As EventArgs) Handles btnUpload.Click
        btnUpload.Enabled = False
        Dim apppath As String = Assembly.GetExecutingAssembly().Location
        Dim appdir As String = Path.GetDirectoryName(apppath)

        Dim uploadlist As New List(Of String)
        Dim subfolder As String
        Dim destfolder As String
        Dim destfile As String
        If cbxSource.Text = "Current Folder" Then
            uploadlist = GetFilesRecursive(appdir)
            uploadlist.Remove(apppath)
            For Each sourcefile In uploadlist
                subfolder = Path.GetDirectoryName(sourcefile).Replace(appdir, "")
                destfolder = "ftp://" & cbxHost.Text & "/" & txtDest.Text & subfolder
                If subfolder = "" Then
                    destfile = destfolder & Path.GetFileName(sourcefile)
                Else
                    destfile = destfolder & "\" & Path.GetFileName(sourcefile)
                End If
                CreateFTPFolder(txtDest.Text & subfolder)
                UploadFTP(destfile, sourcefile)
            Next
        Else
            uploadlist = GetFilesRecursive(cbxSource.Text)
            For Each sourcefile In uploadlist
                subfolder = Path.GetDirectoryName(sourcefile).Replace(cbxSource.Text, "")
                destfolder = "ftp://" & cbxHost.Text & "/" & txtDest.Text & subfolder
                If subfolder = "" Then
                    destfile = destfolder & Path.GetFileName(sourcefile)
                Else
                    destfile = destfolder & "\" & Path.GetFileName(sourcefile)
                End If
                CreateFTPFolder(txtDest.Text & subfolder)
                UploadFTP(destfile, sourcefile)
            Next
        End If
        btnUpload.Enabled = True
    End Sub

    Public Function FTPFolderExists(directory As String) As Boolean
        Dim directoryExists__1 As Boolean

        Dim request = DirectCast(WebRequest.Create(directory), FtpWebRequest)
        request.Method = WebRequestMethods.Ftp.ListDirectory
        request.Credentials = New NetworkCredential(cbxUsername.Text, cbxPassword.Text)

        Try
            Using request.GetResponse()
                directoryExists__1 = True
            End Using
        Catch generatedExceptionName As WebException
            directoryExists__1 = False
        End Try

        Return directoryExists__1
    End Function

    Public Shared Function GetFilesRecursive(ByVal initial As String) As List(Of String)
        ' This list stores the results.
        Dim result As New List(Of String)

        ' This stack stores the directories to process.
        Dim stack As New Stack(Of String)

        ' Add the initial directory
        stack.Push(initial)

        ' Continue processing for each stacked directory
        Do While (stack.Count > 0)
            ' Get top directory string
            Dim dir As String = stack.Pop
            Try
                ' Add all immediate file paths
                result.AddRange(Directory.GetFiles(dir, "*.*"))

                ' Loop through all subdirectories and add them to the stack.
                Dim directoryName As String
                For Each directoryName In Directory.GetDirectories(dir)
                    stack.Push(directoryName)
                Next

            Catch ex As Exception
            End Try
        Loop

        ' Return the list
        Return result
    End Function

    Private Sub CreateFTPFolder(inputpath As String)
        inputpath = inputpath.Replace("\", "/").Replace("//", "/")
        Dim subfolders As List(Of String) = inputpath.Split("/").ToList()
        Dim uploadpath As String = "ftp://" & cbxHost.Text

        Dim RequestFolderCreate As Net.FtpWebRequest
        For i = 0 To subfolders.Count - 1
            uploadpath = uploadpath & "/" & subfolders(i)
            Dim uploadpathExist As Boolean = FTPFolderExists(uploadpath)
            If Not uploadpathExist Then
                RequestFolderCreate = CType(WebRequest.Create(uploadpath), FtpWebRequest)
                RequestFolderCreate.Credentials = New NetworkCredential(cbxUsername.Text, cbxPassword.Text)
                RequestFolderCreate.Method = WebRequestMethods.Ftp.MakeDirectory
                Try
                    Using response As FtpWebResponse = DirectCast(RequestFolderCreate.GetResponse(), FtpWebResponse)

                    End Using

                Catch ex As WebException
                    cbxLog.Items.Add("Create FTP Folder Error: " + ex.Message)
                End Try
            Else
                Continue For
            End If
        Next
    End Sub

    Public Sub UploadFTP(ByVal _UploadPath As String, ByVal _FileName As String)
        Dim _FileInfo As New FileInfo(_FileName)
        Dim _FtpWebRequest As FtpWebRequest = CType(FtpWebRequest.Create(New Uri(_UploadPath)), FtpWebRequest)
        _FtpWebRequest.Credentials = New Net.NetworkCredential(cbxUsername.Text, cbxPassword.Text)
        _FtpWebRequest.KeepAlive = False
        _FtpWebRequest.Timeout = 200000
        _FtpWebRequest.Method = WebRequestMethods.Ftp.UploadFile
        _FtpWebRequest.UseBinary = True
        _FtpWebRequest.ContentLength = _FileInfo.Length
        Dim buffLength As Integer = 2048
        Dim buff(buffLength - 1) As Byte
        Dim _FileStream As FileStream = _FileInfo.OpenRead()
        Try
            Dim _Stream As Stream = _FtpWebRequest.GetRequestStream()
            Dim contentLen As Integer = _FileStream.Read(buff, 0, buffLength)
            Do While contentLen <> 0
                _Stream.Write(buff, 0, contentLen)
                contentLen = _FileStream.Read(buff, 0, buffLength)
            Loop
            _Stream.Close()
            _Stream.Dispose()
            _FileStream.Close()
            _FileStream.Dispose()
            cbxLog.Items.Add("Uploaded " + _FileName + " To: " + _UploadPath)
        Catch ex As Exception
            cbxLog.Items.Add("Upload Error: " + ex.Message)
        End Try
    End Sub

    Private Sub btnClearlog_Click(sender As Object, e As EventArgs) Handles btnClearlog.Click
        cbxLog.Items.Clear()
    End Sub

    Private Sub cbxSource_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbxSource.SelectedIndexChanged
        If cbxSource.SelectedIndex = 2 Then
            Using frm = New FolderBrowserDialog
                If frm.ShowDialog(Me) = DialogResult.OK Then

                    cbxSource.Text = frm.SelectedPath

                End If
            End Using
        End If
    End Sub

    Private Sub SetVersionInfo()

        Dim ass As System.Reflection.Assembly = System.Reflection.Assembly.GetExecutingAssembly()
        Dim ver As System.Version = ass.GetName().Version
        Dim startDate As DateTime = New Date(2000, 1, 1)
        Dim diffDays As Integer = ver.Build
        Dim computedDate As DateTime = startDate.AddDays(diffDays)
        Dim lastBuilt As String = computedDate.ToShortDateString()
        'Me.Text = (Me.Text & " " & ver.Major & "." & ver.Minor & "." & ver.Build & "." & ver.Revision & " (" & lastBuilt & ")")
        Me.Text = (Me.Text & " " & ver.Major & "." & ver.Minor & "." & ver.Build & "." & ver.Revision)

    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetVersionInfo()
    End Sub
End Class
