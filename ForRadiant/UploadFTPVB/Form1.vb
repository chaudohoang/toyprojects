Imports System.IO
Imports System.Reflection
Public Class Form1
    Private Sub btnUpload_Click(sender As Object, e As EventArgs) Handles btnUpload.Click
        Dim apppath As String = System.Reflection.Assembly.GetExecutingAssembly().Location
        Dim appdir As String = Path.GetDirectoryName(apppath)

        Dim uploadlist As New List(Of String)
        Dim subfolder As String
        Dim destfolder As String
        Dim destfile As String
        If txtSource.Text = "Current Folder" Then
            uploadlist = GetFilesRecursive(appdir)
            For Each sourcefile In uploadlist
                subfolder = Path.GetDirectoryName(sourcefile).Replace(appdir, "").Replace("\", "")
                destfolder = "ftp://" & txtHost.Text & "/" & txtDest.Text & subfolder
                If subfolder = "" Then
                    destfile = destfolder & Path.GetFileName(sourcefile)
                Else
                    destfile = destfolder & "\" & Path.GetFileName(sourcefile)
                End If
                CreateFTPFolder(txtDest.Text & subfolder)
                UploadFTP(destfile, sourcefile)
            Next
        Else
            uploadlist = GetFilesRecursive(txtSource.Text)
            For Each sourcefile In uploadlist
                subfolder = Path.GetDirectoryName(sourcefile).Replace(txtSource.Text, "").Replace("\", "")
                destfolder = "ftp://" & txtHost.Text & "/" & txtDest.Text & subfolder
                If subfolder = "" Then
                    destfile = destfolder & Path.GetFileName(sourcefile)
                Else
                    destfile = destfolder & "\" & Path.GetFileName(sourcefile)
                End If
                CreateFTPFolder(txtDest.Text & subfolder)
                UploadFTP(destfile, sourcefile)
            Next
        End If
        ' Loop through and display each path.


    End Sub

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

    Private Sub CreateFTPFolder(path As String)
        Dim RequestFolderCreate As Net.FtpWebRequest = CType(System.Net.FtpWebRequest.Create("ftp://" & txtHost.Text & "/" & path), System.Net.FtpWebRequest)
        RequestFolderCreate.Credentials = New System.Net.NetworkCredential(txtUsername.Text, txtPassword.Text)
        RequestFolderCreate.Method = System.Net.WebRequestMethods.Ftp.MakeDirectory

        Try
            Using response As System.Net.FtpWebResponse = DirectCast(RequestFolderCreate.GetResponse(), System.Net.FtpWebResponse)

            End Using

        Catch ex As System.Net.WebException

        End Try
    End Sub

    Public Sub UploadFTP(ByVal _UploadPath As String, ByVal _FileName As String)
        Dim _FileInfo As New System.IO.FileInfo(_FileName)
        Dim _FtpWebRequest As System.Net.FtpWebRequest = CType(System.Net.FtpWebRequest.Create(New Uri(_UploadPath)), System.Net.FtpWebRequest)
        _FtpWebRequest.Credentials = New Net.NetworkCredential(txtUsername.Text, txtPassword.Text)
        _FtpWebRequest.KeepAlive = False
        _FtpWebRequest.Timeout = 200000
        _FtpWebRequest.Method = System.Net.WebRequestMethods.Ftp.UploadFile
        _FtpWebRequest.UseBinary = True
        _FtpWebRequest.ContentLength = _FileInfo.Length
        Dim buffLength As Integer = 2048
        Dim buff(buffLength - 1) As Byte
        Dim _FileStream As System.IO.FileStream = _FileInfo.OpenRead()
        Try
            Dim _Stream As System.IO.Stream = _FtpWebRequest.GetRequestStream()
            Dim contentLen As Integer = _FileStream.Read(buff, 0, buffLength)
            Do While contentLen <> 0
                _Stream.Write(buff, 0, contentLen)
                contentLen = _FileStream.Read(buff, 0, buffLength)
            Loop
            _Stream.Close()
            _Stream.Dispose()
            _FileStream.Close()
            _FileStream.Dispose()
            txtLog.Text += "Uploaded " + _FileName + " To: " + _UploadPath + Environment.NewLine
        Catch ex As Exception
            txtLog.Text += "Upload Error: " + ex.Message
        End Try
    End Sub

End Class
