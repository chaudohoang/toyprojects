Imports System.Drawing

Public Class Apps
    Dim exePath As String = System.IO.Path.Combine(My.Application.Info.DirectoryPath, "LGDVHCustomApp\")
    Dim startinfo As System.Diagnostics.ProcessStartInfo = New System.Diagnostics.ProcessStartInfo()

    Private Sub cmdSetSequence_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdSetSequence.LinkClicked

        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "SetSequence.exe")
        Process.Start(startinfo)

    End Sub

    Private Sub cmdButterfly_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdButterfly.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "SetSeqx.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdFFCDBGenerate_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdFFCDBGenerate.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "FFCDBGenerate.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdCheckSequence_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdCheckSequence.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "SequenceCheck.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdFTPUploader_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdFTPUploader.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "FTPUploaderVB.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdEmu2p1SequenceConvert_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdEmu2p1SequenceConvert.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "Emu2p1SequenceConvert.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdBackupTT_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdBackupTT.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "BackupCurrentTT.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdCopyMasterAppData_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdCopyMasterAppData.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "CopyMasterAppData.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdRenameVNTT_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdRenameVNTT.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "RenameVNTT.exe")
        Process.Start(startinfo)
    End Sub
End Class
