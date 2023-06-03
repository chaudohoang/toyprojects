Imports System.Drawing
Imports System.IO

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

    Private Sub cmdCopyMaster_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdCopyMaster.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "CopyMaster.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdRenameVNTT_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdRenameVNTT.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "RenameVNTT.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub TabPage2_Enter(sender As Object, e As EventArgs) Handles TabPage2.Enter

        Dim DevMode As New List(Of String)
        Dim DevModeFile As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\DevMode.csv"
        If Not IO.File.Exists(DevModeFile) Then
            chkSkipPOI.Checked = False
            chkSkipRFD.Checked = False
            chkSkipRFDS.Checked = False
            chkSkipLCC.Checked = False
            chkSkipRM.Checked = False
            chkSkipFL.Checked = False
            chkSkipFLDLGDNPOCB41.Checked = False
            chkSkipSaveToDB.Checked = False
            Exit Sub
        Else
            DevMode = IO.File.ReadAllLines(DevModeFile).ToList
        End If
        For Each item As String In DevMode
            Dim skipProperty As String = ""
            Dim skipValue As String = ""
            Try
                skipProperty = item.Split(",")(0)
                skipValue = item.Split(",")(1)
                Select Case skipProperty
                    Case "SkipPOI"
                        If skipValue.ToLower = "true" Then chkSkipPOI.Checked = True
                    Case "SkipRFD"
                        If skipValue.ToLower = "true" Then chkSkipRFD.Checked = True
                    Case "SkipRFDS"
                        If skipValue.ToLower = "true" Then chkSkipRFDS.Checked = True
                    Case "SkipLCC"
                        If skipValue.ToLower = "true" Then chkSkipLCC.Checked = True
                    Case "SkipRM"
                        If skipValue.ToLower = "true" Then chkSkipRM.Checked = True
                    Case "SkipFL"
                        If skipValue.ToLower = "true" Then chkSkipFL.Checked = True
                    Case "SkipFLDLGDNPOCB41"
                        If skipValue.ToLower = "true" Then chkSkipFLDLGDNPOCB41.Checked = True
                    Case "SkipSaveToDB"
                        If skipValue.ToLower = "true" Then chkSkipSaveToDB.Checked = True


                    Case Else

                End Select
            Catch ex As Exception
            End Try

        Next
    End Sub

    Private Sub chkSkipPOI_CheckedChanged(sender As Object, e As EventArgs) Handles chkSkipPOI.CheckedChanged
        Dim DevMode As New List(Of String)
        Dim DevModeFile As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\DevMode.csv"

        Dim skipProperty = "SkipPOI"
        Dim skipValue = If(chkSkipPOI.Checked = True, "true", "false")
        Dim line = skipProperty + "," + skipValue

        Dim propertyExisted As Boolean = False
        If Not IO.File.Exists(DevModeFile) Then
            File.WriteAllText(DevModeFile, line)
        Else
            DevMode = IO.File.ReadAllLines(DevModeFile).ToList
            For index = 0 To DevMode.Count - 1
                If DevMode(index).Contains(skipProperty) Then
                    DevMode(index) = line
                    propertyExisted = True
                    Exit For
                End If
            Next
        End If

        If Not propertyExisted Then
            DevMode.Add(line)
        End If

        File.WriteAllLines(DevModeFile, DevMode)

    End Sub

    Private Sub chkSkipRFD_CheckedChanged(sender As Object, e As EventArgs) Handles chkSkipRFD.CheckedChanged
        Dim DevMode As New List(Of String)
        Dim DevModeFile As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\DevMode.csv"

        Dim skipProperty = "SkipRFD"
        Dim skipValue = If(chkSkipRFD.Checked = True, "true", "false")
        Dim line = skipProperty + "," + skipValue

        Dim propertyExisted As Boolean = False
        If Not IO.File.Exists(DevModeFile) Then
            File.WriteAllText(DevModeFile, line)
        Else
            DevMode = IO.File.ReadAllLines(DevModeFile).ToList
            For index = 0 To DevMode.Count - 1
                If DevMode(index).Contains(skipProperty) Then
                    DevMode(index) = line
                    propertyExisted = True
                    Exit For
                End If
            Next
        End If

        If Not propertyExisted Then
            DevMode.Add(line)
        End If

        File.WriteAllLines(DevModeFile, DevMode)
    End Sub

    Private Sub chkSkipRFDS_CheckedChanged(sender As Object, e As EventArgs) Handles chkSkipRFDS.CheckedChanged
        Dim DevMode As New List(Of String)
        Dim DevModeFile As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\DevMode.csv"

        Dim skipProperty = "SkipRFDS"
        Dim skipValue = If(chkSkipRFDS.Checked = True, "true", "false")
        Dim line = skipProperty + "," + skipValue

        Dim propertyExisted As Boolean = False
        If Not IO.File.Exists(DevModeFile) Then
            File.WriteAllText(DevModeFile, line)
        Else
            DevMode = IO.File.ReadAllLines(DevModeFile).ToList
            For index = 0 To DevMode.Count - 1
                If DevMode(index).Contains(skipProperty) Then
                    DevMode(index) = line
                    propertyExisted = True
                    Exit For
                End If
            Next
        End If

        If Not propertyExisted Then
            DevMode.Add(line)
        End If

        File.WriteAllLines(DevModeFile, DevMode)
    End Sub

    Private Sub chkSkipLCC_CheckedChanged(sender As Object, e As EventArgs) Handles chkSkipLCC.CheckedChanged
        Dim DevMode As New List(Of String)
        Dim DevModeFile As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\DevMode.csv"

        Dim skipProperty = "SkipLCC"
        Dim skipValue = If(chkSkipLCC.Checked = True, "true", "false")
        Dim line = skipProperty + "," + skipValue

        Dim propertyExisted As Boolean = False
        If Not IO.File.Exists(DevModeFile) Then
            File.WriteAllText(DevModeFile, line)
        Else
            DevMode = IO.File.ReadAllLines(DevModeFile).ToList
            For index = 0 To DevMode.Count - 1
                If DevMode(index).Contains(skipProperty) Then
                    DevMode(index) = line
                    propertyExisted = True
                    Exit For
                End If
            Next
        End If

        If Not propertyExisted Then
            DevMode.Add(line)
        End If

        File.WriteAllLines(DevModeFile, DevMode)
    End Sub

    Private Sub chkSkipRM_CheckedChanged(sender As Object, e As EventArgs) Handles chkSkipRM.CheckedChanged
        Dim DevMode As New List(Of String)
        Dim DevModeFile As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\DevMode.csv"

        Dim skipProperty = "SkipRM"
        Dim skipValue = If(chkSkipRM.Checked = True, "true", "false")
        Dim line = skipProperty + "," + skipValue

        Dim propertyExisted As Boolean = False
        If Not IO.File.Exists(DevModeFile) Then
            File.WriteAllText(DevModeFile, line)
        Else
            DevMode = IO.File.ReadAllLines(DevModeFile).ToList
            For index = 0 To DevMode.Count - 1
                If DevMode(index).Contains(skipProperty) Then
                    DevMode(index) = line
                    propertyExisted = True
                    Exit For
                End If
            Next
        End If

        If Not propertyExisted Then
            DevMode.Add(line)
        End If

        File.WriteAllLines(DevModeFile, DevMode)
    End Sub

    Private Sub chkSkipFL_CheckedChanged(sender As Object, e As EventArgs) Handles chkSkipFL.CheckedChanged
        Dim DevMode As New List(Of String)
        Dim DevModeFile As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\DevMode.csv"

        Dim skipProperty = "SkipFL"
        Dim skipValue = If(chkSkipFL.Checked = True, "true", "false")
        Dim line = skipProperty + "," + skipValue

        Dim propertyExisted As Boolean = False
        If Not IO.File.Exists(DevModeFile) Then
            File.WriteAllText(DevModeFile, line)
        Else
            DevMode = IO.File.ReadAllLines(DevModeFile).ToList
            For index = 0 To DevMode.Count - 1
                If DevMode(index).Contains(skipProperty) Then
                    DevMode(index) = line
                    propertyExisted = True
                    Exit For
                End If
            Next
        End If

        If Not propertyExisted Then
            DevMode.Add(line)
        End If

        File.WriteAllLines(DevModeFile, DevMode)
    End Sub

    Private Sub chkSkipFLDLGDNPOCB41_CheckedChanged(sender As Object, e As EventArgs) Handles chkSkipFLDLGDNPOCB41.CheckedChanged
        Dim DevMode As New List(Of String)
        Dim DevModeFile As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\DevMode.csv"

        Dim skipProperty = "SkipFLDLGDNPOCB41"
        Dim skipValue = If(chkSkipFLDLGDNPOCB41.Checked = True, "true", "false")
        Dim line = skipProperty + "," + skipValue

        Dim propertyExisted As Boolean = False
        If Not IO.File.Exists(DevModeFile) Then
            File.WriteAllText(DevModeFile, line)
        Else
            DevMode = IO.File.ReadAllLines(DevModeFile).ToList
            For index = 0 To DevMode.Count - 1
                If DevMode(index).Contains(skipProperty) Then
                    DevMode(index) = line
                    propertyExisted = True
                    Exit For
                End If
            Next
        End If

        If Not propertyExisted Then
            DevMode.Add(line)
        End If

        File.WriteAllLines(DevModeFile, DevMode)
    End Sub

    Private Sub chkSkipSaveToDB_CheckedChanged(sender As Object, e As EventArgs) Handles chkSkipSaveToDB.CheckedChanged
        Dim DevMode As New List(Of String)
        Dim DevModeFile As String = "C:\Radiant Vision Systems Data\TrueTest\AppData\DevMode.csv"

        Dim skipProperty = "SkipSaveToDB"
        Dim skipValue = If(chkSkipSaveToDB.Checked = True, "true", "false")
        Dim line = skipProperty + "," + skipValue

        Dim propertyExisted As Boolean = False
        If Not IO.File.Exists(DevModeFile) Then
            File.WriteAllText(DevModeFile, line)
        Else
            DevMode = IO.File.ReadAllLines(DevModeFile).ToList
            For index = 0 To DevMode.Count - 1
                If DevMode(index).Contains(skipProperty) Then
                    DevMode(index) = line
                    propertyExisted = True
                    Exit For
                End If
            Next
        End If

        If Not propertyExisted Then
            DevMode.Add(line)
        End If

        File.WriteAllLines(DevModeFile, DevMode)
    End Sub

    Private Sub cmdReloadSequence_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdReloadSequence.LinkClicked
        TrueTest.SequenceSet(TrueTest.Sequence.XMLFilePathName)
    End Sub

    Private Sub cmdDove2p0Simulator_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdDove2p0Simulator.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "Dove2p0.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdEmu2p0Simulator_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdEmu2p0Simulator.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "EMU2p0.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdEmu2p1Simulator_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdEmu2p1Simulator.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "EMU2p1.exe")
        Process.Start(startinfo)
    End Sub

    Private Sub cmdFolderLock_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles cmdFolderLock.LinkClicked
        startinfo.WorkingDirectory = exePath
        startinfo.FileName = System.IO.Path.Combine(exePath, "FolderLock.exe")
        Process.Start(startinfo)
    End Sub
End Class
