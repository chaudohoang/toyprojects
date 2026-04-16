' =============================================================================
' Client.vb — Radiant_TrueTest_Client_Simulator
'
' Purpose : TCP client networking — connect, retry, receive, send.
'           Protocol definitions (CommandId, TcpPacket) are in Protocol.vb.
'           UI controls are declared in UI.vb.
'           Button handlers are in EventHandlers.vb.
'           All four files are the same Partial Class.
'
' ┌─ COMMUNICATION FLOW ──────────────────────────────────────────────────────┐
' │   Radiant TrueTest (CLIENT)  ──── TCP connect ────►  Fixture (SERVER)    │
' │         (this app)            ◄──► packets ──────────                     │
' │   This app dials OUT to the fixture's IP + Port.                          │
' └───────────────────────────────────────────────────────────────────────────┘
' =============================================================================

Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Net.Sockets
Imports System.Threading
Imports System.Windows.Forms

Partial Public Class ClientSimulatorForm

#Region "Fields"

    Private mClient       As TcpClient      ' Connection to the fixture server
    Private mStream       As NetworkStream  ' Read/write stream for that connection
    Private connectThread As Thread         ' Performs the TCP connect with retry
    Private recvThread    As Thread         ' Reads incoming packets
    Private mConnected    As Boolean = False
    Private mCancelling   As Boolean = False ' Set by Disconnect() to stop the retry loop
    Private mConnectionId As Integer = 0    ' Incremented on each new session
    Private mLastIP       As String  = ""   ' Saved so RestartRetrying can reconnect
    Private mLastPort     As Integer = 0

#End Region

#Region "Client Control"

    ''' <summary>Begin connecting to the fixture server. Retries automatically every 2 s.</summary>
    Public Sub ConnectToServer(ip As String, port As Integer)
        mLastIP   = ip
        mLastPort = port
        Log("Connecting to " & ip & ":" & port.ToString() & " …")
        connectThread = New Thread(Sub() DoConnect(ip, port)) With {.IsBackground = True}
        connectThread.Start()
    End Sub

    ''' <summary>Close the connection and stop any active retry loop.</summary>
    Public Sub Disconnect()
        mConnectionId += 1   ' Prevents a dying ReceiveLoop from calling RestartRetrying
        mCancelling = True   ' Stops the retry loop in DoConnect
        mConnected  = False
        Try
            If mStream IsNot Nothing Then mStream.Close() : mStream = Nothing
        Catch
        End Try
        Try
            If mClient IsNot Nothing Then mClient.Close() : mClient = Nothing
        Catch
        End Try
        Log("Disconnected.")
        SetConnectedState(False)
    End Sub

#End Region

#Region "Background Threads"

    ''' <summary>
    ''' Runs on a background thread.
    ''' Keeps retrying every 2 seconds until connected or Disconnect() is called.
    ''' </summary>
    Private Sub DoConnect(ip As String, port As Integer)
        mCancelling = False
        Dim attempt As Integer = 0
        SetRetryingState()

        Do
            attempt += 1
            Log("Attempt " & attempt & "  —  connecting to " & ip & ":" & port & " …")

            Try
                mClient = New TcpClient()
                mClient.Connect(ip, port)   ' Blocks; throws if refused

                mStream    = mClient.GetStream()
                mConnected = True
                mConnectionId += 1
                Dim myId As Integer = mConnectionId

                Log("Connected to fixture server at " & ip & ":" & port.ToString() &
                    "  (after " & attempt & If(attempt = 1, " attempt", " attempts") & ")")
                SetConnectedState(True)

                recvThread = New Thread(Sub() ReceiveLoop(myId)) With {.IsBackground = True}
                recvThread.Start()
                Return

            Catch ex As Exception
                If mCancelling Then
                    Log("Connection cancelled by user.")
                    SetConnectedState(False)
                    Return
                End If

                Log("Attempt " & attempt & " failed  (" & ex.Message & ")  —  retrying in 2 s…")

                ' Wait in small slices so Disconnect() is picked up within 100 ms
                Dim waited As Integer = 0
                While waited < 2000 AndAlso Not mCancelling
                    Thread.Sleep(100)
                    waited += 100
                End While

                If mCancelling Then
                    Log("Connection cancelled by user.")
                    SetConnectedState(False)
                    Return
                End If
            End Try
        Loop
    End Sub

    ''' <summary>
    ''' Reads packets from the fixture server and logs them.
    ''' No automatic responses — use the Packet Builder to reply manually.
    ''' When the server disconnects, automatically resumes retrying.
    ''' </summary>
    Private Sub ReceiveLoop(connectionId As Integer)
        Dim buffer(1024 * 1024 - 1) As Byte
        Dim bytesRead As Integer

        Do
            bytesRead = 0
            Try
                bytesRead = mStream.Read(buffer, 0, buffer.Length)
            Catch
                Exit Do
            End Try

            If bytesRead = 0 Then
                Log("Fixture server closed the connection.")
                Exit Do
            End If

            Dim pkt As New TcpPacket(buffer, bytesRead)

            Dim line As String = ">>> RECEIVED  [" & pkt.Command.ToString() &
                                 "  /  wire: " & TcpPacket.GetCommandString(pkt.Command) & "]"
            If pkt.Data IsNot Nothing AndAlso pkt.Data.Count > 0 Then
                line &= "    Data: " & String.Join("  |  ", pkt.Data)
            End If
            If pkt.ByteDataLength > 0 Then
                line &= "    + " & pkt.ByteDataLength & " bytes of raw ByteData"
            End If
            Log(line)
        Loop

        ' Resume retrying if this is still the active session
        If connectionId = mConnectionId Then
            RestartRetrying()
        End If
    End Sub

    ''' <summary>
    ''' Called when the server closes the connection.
    ''' Cleans up the dead socket and resumes the retry loop automatically.
    ''' </summary>
    Private Sub RestartRetrying()
        mConnected = False
        Try
            If mStream IsNot Nothing Then mStream.Close() : mStream = Nothing
        Catch
        End Try
        Try
            If mClient IsNot Nothing Then mClient.Close() : mClient = Nothing
        Catch
        End Try
        Log("Server closed connection.  Retrying…")
        connectThread = New Thread(Sub() DoConnect(mLastIP, mLastPort)) With {.IsBackground = True}
        connectThread.Start()
    End Sub

#End Region

#Region "Send"

    ''' <summary>Build and transmit one packet to the connected fixture server.</summary>
    Public Sub SendPacket(cmd      As CommandId,
                          data     As List(Of String),
                          byteData As Byte())
        If mClient Is Nothing OrElse Not mConnected Then
            Log("Cannot send — not connected to any server.")
            Return
        End If

        Dim pkt As New TcpPacket(cmd, data, byteData)
        Dim ok  As Boolean = pkt.Send(mStream)

        If ok Then
            Dim line As String = "<<< SENT  [" & cmd.ToString() &
                                 "  /  wire: " & TcpPacket.GetCommandString(cmd) & "]"
            If data IsNot Nothing AndAlso data.Count > 0 Then
                line &= "    Data: " & String.Join("  |  ", data)
            End If
            If byteData IsNot Nothing AndAlso byteData.Length > 0 Then
                line &= "    + " & byteData.Length & " bytes of raw ByteData"
            End If
            Log(line)
        Else
            Log("SEND FAILED for [" & cmd.ToString() & "]")
        End If
    End Sub

#End Region

#Region "Logging"

    Private Delegate Sub LogDelegate(text As String)

    ''' <summary>
    ''' Append a timestamped entry to the log.
    ''' Thread-safe — safe to call from any background thread.
    ''' </summary>
    Public Sub Log(text As String)
        Dim ts As String = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")
        If InvokeRequired Then
            BeginInvoke(New LogDelegate(AddressOf Log), text)
            Return
        End If
        lbxLog.Items.Add("[" & ts & "]  " & text)
        lbxLog.TopIndex = lbxLog.Items.Count - 1
    End Sub

#End Region

End Class   ' end of Partial Class ClientSimulatorForm
