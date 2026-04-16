' =============================================================================
' Server.vb — Fixture_Company_Server_Simulator
'
' Purpose : TCP server networking — listen, accept, receive, send.
'           Protocol definitions (CommandId, TcpPacket) are in Protocol.vb.
'           UI controls are declared in UI.vb.
'           Button handlers are in EventHandlers.vb.
'           All four files are the same Partial Class.
'
' ┌─ COMMUNICATION FLOW ──────────────────────────────────────────────────────┐
' │   Radiant TrueTest (CLIENT)  ──── TCP connect ────►  Fixture (SERVER)    │
' │                               ◄──► packets ──────────  (this app)        │
' │   TrueTest connects TO the fixture. The fixture listens passively.        │
' └───────────────────────────────────────────────────────────────────────────┘
' =============================================================================

Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Net
Imports System.Net.Sockets
Imports System.Threading
Imports System.Windows.Forms

Partial Public Class ServerSimulatorForm

#Region "Fields"

    Private mServer       As TcpListener    ' Listens for incoming TCP connections
    Private mClient       As TcpClient      ' The one connected TrueTest client
    Private mStream       As NetworkStream  ' Read/write stream for that client
    Private listenThread  As Thread         ' Blocks on AcceptTcpClient
    Private recvThread    As Thread         ' Reads incoming packets
    Private mConnected    As Boolean = False
    Private mConnectionId As Integer = 0   ' Incremented on each new session

#End Region

#Region "Server Control"

    ''' <summary>Start listening on the given IP and port.</summary>
    Public Sub StartListening(ip As String, port As Integer)
        Try
            mServer = New TcpListener(IPAddress.Parse(ip), port)
            mServer.Server.SetSocketOption(SocketOptionLevel.Socket,
                                           SocketOptionName.ReuseAddress, True)
            listenThread = New Thread(AddressOf ListenForClient) With {.IsBackground = True}
            listenThread.Start()
            SetListeningState()
            Log("Server started.  Listening on " & ip & ":" & port.ToString())
        Catch ex As Exception
            Log("ERROR starting server: " & ex.Message)
        End Try
    End Sub

    ''' <summary>Close all connections and stop the listener.</summary>
    Public Sub StopServer()
        mConnectionId += 1   ' Invalidates any running threads
        mConnected = False
        Try
            If mStream IsNot Nothing Then mStream.Close() : mStream = Nothing
        Catch
        End Try
        Try
            If mClient IsNot Nothing Then mClient.Close() : mClient = Nothing
        Catch
        End Try
        Try
            If mServer IsNot Nothing Then mServer.Stop() : mServer = Nothing
        Catch
        End Try
        Log("Server stopped.")
        SetConnectedState(False)
    End Sub

#End Region

#Region "Background Threads"

    ''' <summary>
    ''' Blocks on AcceptTcpClient until TrueTest connects,
    ''' then starts the ReceiveLoop for that session.
    ''' </summary>
    Private Sub ListenForClient()
        Try
            If mServer IsNot Nothing Then mServer.Start()
            Log("Waiting for TrueTest client to connect…")

            mClient    = mServer.AcceptTcpClient()
            mStream    = mClient.GetStream()
            mConnected = True
            mConnectionId += 1
            Dim myId As Integer = mConnectionId

            Dim ep As IPEndPoint = CType(mClient.Client.RemoteEndPoint, IPEndPoint)
            Log("TrueTest client connected from " & ep.ToString())
            SetConnectedState(True)

            recvThread = New Thread(Sub() ReceiveLoop(myId)) With {.IsBackground = True}
            recvThread.Start()
        Catch ex As Exception
            If mConnected Then Log("Listen error: " & ex.Message)
        End Try
    End Sub

    ''' <summary>
    ''' Reads packets from TrueTest and logs them.
    ''' No automatic responses — use the Packet Builder to reply manually.
    ''' When the client disconnects, automatically returns to Listening state.
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
                Log("TrueTest client disconnected.")
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

        ' Return to listening state if this is still the active session
        If connectionId = mConnectionId Then
            RestartListening()
        End If
    End Sub

    ''' <summary>
    ''' Called when the active client disconnects.
    ''' Cleans up the dead socket and immediately waits for the next connection
    ''' without stopping the TcpListener (avoids port-rebinding issues).
    ''' </summary>
    Private Sub RestartListening()
        mConnected = False
        Try
            If mStream IsNot Nothing Then mStream.Close() : mStream = Nothing
        Catch
        End Try
        Try
            If mClient IsNot Nothing Then mClient.Close() : mClient = Nothing
        Catch
        End Try
        Log("Waiting for next client connection…")
        SetListeningState()
        listenThread = New Thread(AddressOf ListenForClient) With {.IsBackground = True}
        listenThread.Start()
    End Sub

#End Region

#Region "Send"

    ''' <summary>Build and transmit one packet to the connected TrueTest client.</summary>
    Public Sub SendPacket(cmd      As CommandId,
                          data     As List(Of String),
                          byteData As Byte())
        If mClient Is Nothing OrElse Not mConnected Then
            Log("Cannot send — no client is currently connected.")
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
    ''' Thread-safe — safe to call from ReceiveLoop or any background thread.
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

End Class   ' end of Partial Class ServerSimulatorForm
