' =============================================================================
' Protocol.vb — Fixture_Company_Server_Simulator
'
' Purpose : Shared protocol definitions used by both Server.vb and UI.vb.
'           Contains:
'             • CommandId  — all 52 command identifiers
'             • Endness    — byte-order flag used by TcpPacket
'             • TcpPacket  — encode / decode one packet on the wire
'
' ┌─ WIRE FORMAT ─────────────────────────────────────────────────────────────┐
' │  [0–3]    4 bytes   Total size, uint32 Little-Endian                      │
' │  [4–7]    4 bytes   Checksum = sum(bytes 0–3), stored Little-Endian       │
' │  [8+]     N bytes   ASCII payload  "COMMAND DATA1 DATA2 … DATAn"          │
' │  [8+N]    1 byte    0x00  null terminator                                 │
' │  [8+N+1]  M bytes   Raw ByteData  (optional, always at the very end)      │
' └───────────────────────────────────────────────────────────────────────────┘
' =============================================================================

Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Net.Sockets
Imports System.Text

' =============================================================================
' CommandId  —  all 52 commands in the simulator protocol.
' =============================================================================
Public Enum CommandId
    Start          = 0    ' "START"          Fixture → TrueTest: Fixture initiates new panel cycle
    Ends           = 1    ' "END"            Fixture → TrueTest: End the current test sequence
    ModelChange    = 2    ' "MODELCHG"       Fixture → TrueTest: Report panel model name
    ChangePattern  = 3    ' "CHGPTN"         TrueTest → Fixture: Request a display test-pattern change
    CorrectionData = 4    ' "POCBDATA"       TrueTest → Fixture: Deliver POCB pixel correction data
    Result         = 5    ' "RESULT"         TrueTest → Fixture: Final pass/fail result for the panel
    PGOn           = 6    ' "PGON"           TrueTest → Fixture: Turn the pattern generator ON
    PGOff          = 7    ' "PGOFF"          TrueTest → Fixture: Turn the pattern generator OFF
    Unknown        = 8    ' (unrecognised command)
    ACK            = 9    ' "ACK"            Either direction:   Acknowledge the previous packet
    NAK            = 10   ' "NAK"            Either direction:   Negative-acknowledge (error / reject)
    PING           = 11   ' "PING"           Either direction:   Keep-alive / heartbeat
    ChgPtnDone     = 12   ' "CHGPTNDONE"     TrueTest → Fixture: Confirm pattern change is done
    NONE           = 13   ' "NONE"           No-op placeholder
    ModeChangeLPM  = 14   ' "CHGLPM"         TrueTest → Fixture: Request switch to Low Power Mode
    CHGRCB         = 15   ' "CHGRCB"         Fixture → TrueTest: Request switch to RCB mode
    POCBGAMMA      = 16   ' "POCBGAMMA"      Fixture → TrueTest: Send gamma correction data
    CAMALARM       = 17   ' "CAMALARM"       TrueTest → Fixture: Report camera alarm condition
    GATEDATA       = 18   ' "GATEDATA"       Fixture → TrueTest: Send gate-driver data
    FFCSTART       = 19   ' "FFCSTART"       Fixture → TrueTest: Start flat-field correction
    FFCDONE        = 20   ' "FFCDONE"        TrueTest → Fixture: Flat-field correction complete
    POCBPATH       = 21   ' "POCBPATH"       TrueTest → Fixture: Provide POCB correction file path(s)
    STAINSTART     = 22   ' "STAINSTART"     Fixture → TrueTest: Start stain measurement
    STAINDONE      = 23   ' "STAINDONE"      TrueTest → Fixture: Stain measurement complete
    CHGBND         = 24   ' "CHGBND"         TrueTest → Fixture: Change band / frequency setting
    FTPUPLOAD      = 25   ' "FTPUPLOAD"      Fixture → TrueTest: Send FTP upload info (host, user, password) to TrueTest
    GETVER         = 26   ' "GETVER"         Fixture → TrueTest: Request TrueTest to report its software version
    DGMA           = 27   ' "DGMA"           Fixture → TrueTest: Send digital-gamma data
    PUCVER         = 28   ' "PUCVER"         TrueTest → Fixture: Report PUC version (PUC = customer POCB processing program)
    GETSEQUENCE    = 29   ' "GETSEQUENCE"    Fixture → TrueTest: Request TrueTest to report the current test sequence name
    HOTCOLDPIXEL   = 30   ' "HOTCOLDPIXEL"   TrueTest → Fixture: Hot / cold pixel detection result
    INTENSITY      = 31   ' "INTENSITY"      TrueTest → Fixture: Intensity measurement result
    MOIRE          = 32   ' "MOIRE"          TrueTest → Fixture: Moiré pattern detection result
    PWMSIGNAL      = 33   ' "PWMSIGNAL"      TrueTest → Fixture: PWM signal measurement result
    AFTERSTART     = 34   ' "AFTERSTART"     Fixture → TrueTest: Start after-image measurement
    AFTERDONE      = 35   ' "AFTERDONE"      TrueTest → Fixture: After-image measurement complete
    DARKPOINTSTART = 36   ' "DARKPOINTSTART" Fixture → TrueTest: Start dark-point measurement
    DARKPOINTDONE  = 37   ' "DARKPOINTDONE"  TrueTest → Fixture: Dark-point measurement complete
    DARKPOINTDATA  = 38   ' "DARKPOINTDATA"  TrueTest → Fixture: Dark-point measurement data
    TEMPLATE       = 39   ' "TEMPLATE"       Fixture → TrueTest: Notify TrueTest to start template-related flow
    CHGPUC         = 40   ' "CHGPUC"         TrueTest → Fixture: Request fixture to change POCB (PUC) program status ON / OFF
    BMPDATA        = 41   ' "BMPDATA"        TrueTest → Fixture: Deliver bitmap image data
    SEQCHECK       = 42   ' "SEQCHECK"       Fixture → TrueTest: Request TrueTest to verify the current test sequence
    SEQCHECKDONE   = 43   ' "SEQCHECKDONE"   TrueTest → Fixture: Sequence-check complete
    TABLETRESULT   = 44   ' "TABLETRESULT"   TrueTest → Fixture: Tablet-mode test result
    RESTART        = 45   ' "RESTART"        Fixture → TrueTest: Trigger restart-related function in TrueTest
    TIME           = 46   ' "TIME"           Fixture → TrueTest: Request TrueTest to sync time with fixture
    CAMSTOP        = 47   ' "CAMSTOP"        Fixture → TrueTest: Request TrueTest to stop camera capture
    VNTTCheck      = 48   ' "VNTTCheck"      TrueTest → Fixture: Report VNTT calibration data to the fixture
    VNTTset        = 49   ' "VNTTset"        Fixture → TrueTest: Request TrueTest to update VNTT calibration files
    CHGRGB         = 50   ' "CHGRGB"         TrueTest → Fixture: Request fixture to change display pattern to specific RGB colour
    VNTTCOPY       = 51   ' "VNTTCOPY"       Fixture → TrueTest: Request TrueTest to copy VNTT configuration files
End Enum

Public Enum Endness
    Big    = 1   ' Most-significant byte first
    Little = 2   ' Least-significant byte first
End Enum


' =============================================================================
' TcpPacket  —  encode / decode one TCP packet in the simulator protocol.
' =============================================================================
Public Class TcpPacket

    Public ReadOnly Command        As CommandId
    Public ReadOnly Data           As List(Of String)
    Public ReadOnly ByteDataLength As Integer

    Private mByteData As Byte()

    ' ── Receive constructor ──────────────────────────────────────────────────
    Public Sub New(buffer As Byte(), bytesRead As Integer)
        Command = CommandId.Unknown
        Data    = New List(Of String)

        If buffer Is Nothing OrElse bytesRead < 10 Then Return

        Try
            Dim msgSize  As Integer = CInt(ReadInt(buffer, 0, 4, Endness.Little))
            Dim checksum As Long    = ReadInt(buffer, 4, 4, Endness.Little)
            If msgSize <= 0 OrElse checksum <= 0 Then Return

            Dim nullPos As Integer = 8
            While nullPos < bytesRead AndAlso buffer(nullPos) <> &H0
                nullPos += 1
            End While

            Dim payloadLen As Integer = nullPos - 8
            If payloadLen <= 0 Then Return

            Dim payloadBytes(payloadLen - 1) As Byte
            Array.Copy(buffer, 8, payloadBytes, 0, payloadLen)
            Dim payload As String = Encoding.ASCII.GetString(payloadBytes)

            Dim tokens() As String = payload.Split(New Char() {" "c},
                                                    StringSplitOptions.RemoveEmptyEntries)
            If tokens.Length = 0 Then Return

            Command = ParseCommandString(tokens(0).Trim())
            For i As Integer = 1 To tokens.Length - 1
                Data.Add(tokens(i).Trim())
            Next

            Dim bdStart As Integer = nullPos + 1
            ByteDataLength = If(bdStart < bytesRead, bytesRead - bdStart, 0)

        Catch
            Command = CommandId.Unknown
        End Try
    End Sub

    ' ── Send constructor ─────────────────────────────────────────────────────
    Public Sub New(cmd      As CommandId,
                   Optional data     As List(Of String) = Nothing,
                   Optional byteData As Byte()          = Nothing)
        Command        = cmd
        Me.Data        = If(data, New List(Of String))
        mByteData      = byteData
        ByteDataLength = If(byteData IsNot Nothing, byteData.Length, 0)
    End Sub

    ' ── Send ─────────────────────────────────────────────────────────────────
    ''' <summary>
    ''' Serialise and write this packet to the given NetworkStream.
    ''' Packet layout:
    '''   [0–3]  size LE  =  4 + 4 + len(payload) + 1 [+ len(ByteData)]
    '''   [4–7]  checksum LE  =  sum of the 4 size bytes
    '''   [8+]   ASCII payload  "COMMAND DATA1 DATA2 … DATAn"
    '''          0x00 null terminator
    '''          optional raw ByteData (always at the very end)
    ''' </summary>
    Public Function Send(stream As NetworkStream) As Boolean
        Try
            Dim payloadStr As String = GetCommandString(Command)
            If Data IsNot Nothing Then
                For Each s As String In Data
                    payloadStr &= " " & s
                Next
            End If
            Dim payloadBytes As Byte() = Encoding.ASCII.GetBytes(payloadStr)

            Dim msgSize As Integer = 4 + 4 + payloadBytes.Length + 1
            If mByteData IsNot Nothing Then msgSize += mByteData.Length

            Dim packet As New List(Of Byte)
            packet.AddRange(WriteInt(msgSize, Endness.Little))
            packet.AddRange(WriteInt(SumBytes(packet.ToArray(), 0, 4), Endness.Little))
            packet.AddRange(payloadBytes)
            packet.Add(&H0)
            If mByteData IsNot Nothing AndAlso mByteData.Length > 0 Then
                packet.AddRange(mByteData)
            End If

            stream.Write(packet.ToArray(), 0, packet.Count)
            Return True
        Catch
            Return False
        End Try
    End Function

    ' ── Command string mappings ──────────────────────────────────────────────

    Private Shared Function ParseCommandString(s As String) As CommandId
        Select Case s.ToUpper().Trim()
            Case "START"           : Return CommandId.Start
            Case "END"             : Return CommandId.Ends
            Case "MODELCHG"        : Return CommandId.ModelChange
            Case "CHGPTN"          : Return CommandId.ChangePattern
            Case "POCBDATA"        : Return CommandId.CorrectionData
            Case "RESULT"          : Return CommandId.Result
            Case "PGON"            : Return CommandId.PGOn
            Case "PGOFF"           : Return CommandId.PGOff
            Case "ACK"             : Return CommandId.ACK
            Case "NAK"             : Return CommandId.NAK
            Case "PING"            : Return CommandId.PING
            Case "CHGPTNDONE"      : Return CommandId.ChgPtnDone
            Case "NONE"            : Return CommandId.NONE
            Case "CHGLPM"          : Return CommandId.ModeChangeLPM
            Case "CHGRCB"          : Return CommandId.CHGRCB
            Case "POCBGAMMA"       : Return CommandId.POCBGAMMA
            Case "CAMALARM"        : Return CommandId.CAMALARM
            Case "GATEDATA"        : Return CommandId.GATEDATA
            Case "FFCSTART"        : Return CommandId.FFCSTART
            Case "FFCDONE"         : Return CommandId.FFCDONE
            Case "POCBPATH"        : Return CommandId.POCBPATH
            Case "STAINSTART"      : Return CommandId.STAINSTART
            Case "STAINDONE"       : Return CommandId.STAINDONE
            Case "CHGBND"          : Return CommandId.CHGBND
            Case "FTPUPLOAD"       : Return CommandId.FTPUPLOAD
            Case "GETVER"          : Return CommandId.GETVER
            Case "DGMA"            : Return CommandId.DGMA
            Case "PUCVER"          : Return CommandId.PUCVER
            Case "GETSEQUENCE"     : Return CommandId.GETSEQUENCE
            Case "HOTCOLDPIXEL"    : Return CommandId.HOTCOLDPIXEL
            Case "INTENSITY"       : Return CommandId.INTENSITY
            Case "MOIRE"           : Return CommandId.MOIRE
            Case "PWMSIGNAL"       : Return CommandId.PWMSIGNAL
            Case "AFTERSTART"      : Return CommandId.AFTERSTART
            Case "AFTERDONE"       : Return CommandId.AFTERDONE
            Case "DARKPOINTSTART"  : Return CommandId.DARKPOINTSTART
            Case "DARKPOINTDONE"   : Return CommandId.DARKPOINTDONE
            Case "DARKPOINTDATA"   : Return CommandId.DARKPOINTDATA
            Case "TEMPLATE"        : Return CommandId.TEMPLATE
            Case "CHGPUC"          : Return CommandId.CHGPUC
            Case "BMPDATA"         : Return CommandId.BMPDATA
            Case "SEQCHECK"        : Return CommandId.SEQCHECK
            Case "SEQCHECKDONE"    : Return CommandId.SEQCHECKDONE
            Case "TABLETRESULT"    : Return CommandId.TABLETRESULT
            Case "RESTART"         : Return CommandId.RESTART
            Case "TIME"            : Return CommandId.TIME
            Case "CAMSTOP"         : Return CommandId.CAMSTOP
            Case "VNTTCHECK"       : Return CommandId.VNTTCheck
            Case "VNTTSET"         : Return CommandId.VNTTset
            Case "CHGRGB"          : Return CommandId.CHGRGB
            Case "VNTTCOPY"        : Return CommandId.VNTTCOPY
            Case Else              : Return CommandId.Unknown
        End Select
    End Function

    Public Shared Function GetCommandString(cmd As CommandId) As String
        Select Case cmd
            Case CommandId.Start          : Return "START"
            Case CommandId.Ends           : Return "END"
            Case CommandId.ModelChange    : Return "MODELCHG"
            Case CommandId.ChangePattern  : Return "CHGPTN"
            Case CommandId.CorrectionData : Return "POCBDATA"
            Case CommandId.Result         : Return "RESULT"
            Case CommandId.PGOn           : Return "PGON"
            Case CommandId.PGOff          : Return "PGOFF"
            Case CommandId.ACK            : Return "ACK"
            Case CommandId.NAK            : Return "NAK"
            Case CommandId.PING           : Return "PING"
            Case CommandId.ChgPtnDone     : Return "CHGPTNDONE"
            Case CommandId.NONE           : Return "NONE"
            Case CommandId.ModeChangeLPM  : Return "CHGLPM"
            Case CommandId.CHGRCB         : Return "CHGRCB"
            Case CommandId.POCBGAMMA      : Return "POCBGAMMA"
            Case CommandId.CAMALARM       : Return "CAMALARM"
            Case CommandId.GATEDATA       : Return "GATEDATA"
            Case CommandId.FFCSTART       : Return "FFCSTART"
            Case CommandId.FFCDONE        : Return "FFCDONE"
            Case CommandId.POCBPATH       : Return "POCBPATH"
            Case CommandId.STAINSTART     : Return "STAINSTART"
            Case CommandId.STAINDONE      : Return "STAINDONE"
            Case CommandId.CHGBND         : Return "CHGBND"
            Case CommandId.FTPUPLOAD      : Return "FTPUPLOAD"
            Case CommandId.GETVER         : Return "GETVER"
            Case CommandId.DGMA           : Return "DGMA"
            Case CommandId.PUCVER         : Return "PUCVER"
            Case CommandId.GETSEQUENCE    : Return "GETSEQUENCE"
            Case CommandId.HOTCOLDPIXEL   : Return "HOTCOLDPIXEL"
            Case CommandId.INTENSITY      : Return "INTENSITY"
            Case CommandId.MOIRE          : Return "MOIRE"
            Case CommandId.PWMSIGNAL      : Return "PWMSIGNAL"
            Case CommandId.AFTERSTART     : Return "AFTERSTART"
            Case CommandId.AFTERDONE      : Return "AFTERDONE"
            Case CommandId.DARKPOINTSTART : Return "DARKPOINTSTART"
            Case CommandId.DARKPOINTDONE  : Return "DARKPOINTDONE"
            Case CommandId.DARKPOINTDATA  : Return "DARKPOINTDATA"
            Case CommandId.TEMPLATE       : Return "TEMPLATE"
            Case CommandId.CHGPUC         : Return "CHGPUC"
            Case CommandId.BMPDATA        : Return "BMPDATA"
            Case CommandId.SEQCHECK       : Return "SEQCHECK"
            Case CommandId.SEQCHECKDONE   : Return "SEQCHECKDONE"
            Case CommandId.TABLETRESULT   : Return "TABLETRESULT"
            Case CommandId.RESTART        : Return "RESTART"
            Case CommandId.TIME           : Return "TIME"
            Case CommandId.CAMSTOP        : Return "CAMSTOP"
            Case CommandId.VNTTCheck      : Return "VNTTCheck"
            Case CommandId.VNTTset        : Return "VNTTset"
            Case CommandId.CHGRGB         : Return "CHGRGB"
            Case CommandId.VNTTCOPY       : Return "VNTTCOPY"
            Case Else                     : Return "Unknown"
        End Select
    End Function

    ' ── Byte helpers ─────────────────────────────────────────────────────────

    Private Shared Function ReadInt(buffer As Byte(), offset As Integer,
                                    count  As Integer, order  As Endness) As Long
        Dim result As Long = 0
        For i As Integer = 0 To count - 1
            Dim shift As Integer = If(order = Endness.Little, i, count - 1 - i)
            result = result Or (CLng(buffer(offset + i)) << (8 * shift))
        Next
        Return result
    End Function

    Private Shared Function WriteInt(value As Long, order As Endness) As Byte()
        Dim output(3) As Byte
        Dim v As Long = value
        For i As Integer = 0 To 3
            output(i) = CByte(v And &HFF)
            v >>= 8
        Next
        If order = Endness.Big Then Array.Reverse(output)
        Return output
    End Function

    Private Shared Function SumBytes(buffer As Byte(),
                                     offset As Integer,
                                     count  As Integer) As Long
        Dim total As Long = 0
        For i As Integer = offset To offset + count - 1
            total += CLng(buffer(i))
        Next
        Return total
    End Function

End Class
