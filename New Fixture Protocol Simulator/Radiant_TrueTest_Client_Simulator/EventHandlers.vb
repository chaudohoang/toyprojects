' =============================================================================
' EventHandlers.vb — Radiant_TrueTest_Client_Simulator
'
' Purpose : All button-click handlers and file-loading logic for the UI.
'           Also defines CommandItem (dropdown display wrapper) and the
'           mByteData / mAutoByteCount state fields.
' =============================================================================

Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports System.IO
Imports System.Drawing
Imports Microsoft.VisualBasic

Partial Public Class ClientSimulatorForm

    ' ── ByteData state fields ─────────────────────────────────────────────────
    ' Byte payload to attach to the next sent packet (always goes last in packet)
    Private mByteData As Byte() = Nothing
    ' Tracks the byte-count string auto-added to lbxDataItems on file load,
    ' so it can be cleanly removed when the file is cleared or replaced.
    Private mAutoByteCount As String = ""

    ' ── CommandItem ───────────────────────────────────────────────────────────
    ' Wraps a CommandId so the dropdown shows "CHGPTN  —  ChangePattern"
    ' (wire string first so it matches what arrives on the network).
    Private Class CommandItem
        Public ReadOnly Id         As CommandId
        Public ReadOnly WireString As String

        Public Sub New(id As CommandId)
            Me.Id         = id
            Me.WireString = TcpPacket.GetCommandString(id)
        End Sub

        Public Overrides Function ToString() As String
            Return WireString & "  —  " & Id.ToString()
        End Function
    End Class

    Private Function BuildCommandItems() As List(Of CommandItem)
        Dim items As New List(Of CommandItem)
        For Each cmd As CommandId In [Enum].GetValues(GetType(CommandId))
            items.Add(New CommandItem(cmd))
        Next
        Return items
    End Function

    ' ── Connection handlers ───────────────────────────────────────────────────

    Private Sub OnConnectClick(sender As Object, e As EventArgs)
        Dim port As Integer
        If Not Integer.TryParse(txtPort.Text.Trim(), port) Then
            MessageBox.Show("Please enter a valid port number.", "Invalid Port",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)
            Return
        End If
        ConnectToServer(txtIP.Text.Trim(), port)   '→ Client.vb
    End Sub

    Private Sub OnDisconnectClick(sender As Object, e As EventArgs)
        Disconnect()   '→ Client.vb
    End Sub

    Private Sub OnFormClosing(sender As Object, e As FormClosingEventArgs)
        Disconnect()   '→ Client.vb
    End Sub

    ' ── ASCII Data list handlers ──────────────────────────────────────────────

    Private Sub OnAddDataClick(sender As Object, e As EventArgs)
        Dim txt As String = txtDataEntry.Text.Trim()
        If txt.Length > 0 Then
            lbxDataItems.Items.Add(txt)
            txtDataEntry.Clear()
            txtDataEntry.Focus()
        End If
    End Sub

    Private Sub OnRemoveDataClick(sender As Object, e As EventArgs)
        If lbxDataItems.SelectedIndex >= 0 Then
            lbxDataItems.Items.RemoveAt(lbxDataItems.SelectedIndex)
        End If
    End Sub

    ' ── ByteData file handlers ────────────────────────────────────────────────

    Private Sub OnLoadByteDataClick(sender As Object, e As EventArgs)
        Using ofd As New OpenFileDialog With {.Title = "Select Data File", .Filter = "All Files (*.*)|*.*"}
            If ofd.ShowDialog() <> DialogResult.OK Then Return
            Try
                Select Case cbxByteDataFormat.SelectedIndex

                    Case 0  ' ── Hex text, one byte per line  (Gamma File.hex / POCBDATA.hex) ──
                        '  Each line contains exactly one two-character hex value, e.g. "A3".
                        '  Handles both LF and CRLF line endings.
                        Dim lines() As String = File.ReadAllText(ofd.FileName) _
                                                    .Split(New Char() {Chr(10)},
                                                           StringSplitOptions.RemoveEmptyEntries)
                        Dim result As New List(Of Byte)
                        For Each line As String In lines
                            Dim t As String = line.Trim()
                            If t.Length > 0 Then
                                result.Add(CByte(Convert.ToUInt32(t, 16)))
                            End If
                        Next
                        mByteData = result.ToArray()

                    Case 1  ' ── Hex text, space-separated  (OTP File Type 1.txt / OTP File Type 2.txt) ──
                        '  OTP Type 1 — single line:   0x6900,0xF0,0x03,0x12,0x0B,0x45
                        '  OTP Type 2 — multi-line:    0x00 0x3A 0x3E 0x42 ...
                        '  OTP files always contain 0x prefixes — reject if none found
                        '  so a Gamma file loaded in this mode gives an error instead of
                        '  silently producing wrong data.
                        Dim s As String = File.ReadAllText(ofd.FileName)
                        If Not s.ToLower().Contains("0x") Then
                            Throw New InvalidOperationException("No 0x prefix found.")
                        End If
                        s = s.Replace(vbCrLf, " ").Replace(vbCr, " ").Replace(vbLf, " ")
                        s = s.Replace("0x6900,", "")   ' strip OTP-specific header token
                        s = s.Replace(",", " ").Replace("0x", "").Trim()
                        While s.Contains("  ") : s = s.Replace("  ", " ") : End While
                        Dim result As New List(Of Byte)
                        Dim i As Integer = 0
                        While i + 1 < s.Length
                            If s(i) = " "c Then i += 1 : Continue While
                            result.Add(CByte(Convert.ToInt32(s.Substring(i, 2), 16)))
                            i += 3   ' 2 hex chars + 1 space separator
                        End While
                        mByteData = result.ToArray()

                End Select

                ' Remove the previous auto-added byte count if the user is replacing the file
                If mAutoByteCount <> "" Then
                    Dim oldIdx As Integer = lbxDataItems.Items.IndexOf(mAutoByteCount)
                    If oldIdx >= 0 Then lbxDataItems.Items.RemoveAt(oldIdx)
                End If

                ' Show file info and add byte count as the last data field (visible in list)
                lblByteDataInfo.Text      = Path.GetFileName(ofd.FileName) &
                                            "  (" & mByteData.Length & " bytes)  —  byte count auto-appended on send"
                lblByteDataInfo.ForeColor = Color.DarkGreen
                mAutoByteCount            = CStr(mByteData.Length)
                lbxDataItems.Items.Add(mAutoByteCount)

            Catch ex As Exception
                mByteData                 = Nothing
                lblByteDataInfo.Text      = "Incorrect file type — please check the File Format selection."
                lblByteDataInfo.ForeColor = Color.Red
            End Try
        End Using
    End Sub

    Private Sub OnClearByteDataClick(sender As Object, e As EventArgs)
        ' Remove the auto-added byte count from the data list
        If mAutoByteCount <> "" Then
            Dim idx As Integer = lbxDataItems.Items.IndexOf(mAutoByteCount)
            If idx >= 0 Then lbxDataItems.Items.RemoveAt(idx)
            mAutoByteCount = ""
        End If
        mByteData                 = Nothing
        lblByteDataInfo.Text      = "No ByteData loaded."
        lblByteDataInfo.ForeColor = Color.Gray
    End Sub

    ' ── Send handler ─────────────────────────────────────────────────────────

    Private Sub OnSendClick(sender As Object, e As EventArgs)
        Dim cmd As CommandId = DirectCast(cbxCommand.SelectedItem, CommandItem).Id

        ' Collect every item in the data listbox as ordered ASCII fields.
        ' The byte count (if a file is loaded) is already the last item —
        ' it was added automatically by OnLoadByteDataClick.
        Dim dataList As New List(Of String)
        For Each item As Object In lbxDataItems.Items
            dataList.Add(item.ToString())
        Next

        SendPacket(cmd,
                   If(dataList.Count > 0, dataList, Nothing),
                   mByteData)   ' → Client.vb
    End Sub

End Class
