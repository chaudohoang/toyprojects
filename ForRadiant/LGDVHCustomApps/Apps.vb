Imports System.Drawing
Imports System.IO
Public Class Apps
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

    Dim exePath As String = My.Application.Info.DirectoryPath
    Dim startinfo As System.Diagnostics.ProcessStartInfo = New System.Diagnostics.ProcessStartInfo()

    Private Sub cmdSetSequence_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdSetSequence.LinkClicked

        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "SetSequence.exe")
        If File.Exists(startinfo.FileName) Then
            Process.Start(startinfo)
        End If

    End Sub

    Private Sub cmdButterfly_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdButterfly.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "SetSeqx.exe")
        If File.Exists(startinfo.FileName) Then
            Process.Start(startinfo)
        End If
    End Sub

    Private Sub cmdFFCDBGenerate_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdFFCDBGenerate.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "FFCDBGenerate.exe")
        If File.Exists(startinfo.FileName) Then
            Process.Start(startinfo)
        End If
    End Sub

    Private Sub cmdCheckSequence_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdCheckSequence.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "SequenceCheck.exe")
        If File.Exists(startinfo.FileName) Then
            Process.Start(startinfo)
        End If
    End Sub

    Private Sub Apps_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        SetVersionInfo()
    End Sub
End Class
