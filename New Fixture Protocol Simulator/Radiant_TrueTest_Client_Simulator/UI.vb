' =============================================================================
' UI.vb — Radiant_TrueTest_Client_Simulator
'
' Purpose : Declares all WinForms controls and builds the UI entirely in code.
'
' Files in this project:
'   UI.vb             — control layout and state helpers  (this file)
'   EventHandlers.vb  — button click handlers and file loading logic
'   Client.vb         — TCP networking (connect, receive, send)
'   Protocol.vb       — CommandId enum, TcpPacket class
' =============================================================================

Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Windows.Forms
Imports System.Drawing

Partial Public Class ClientSimulatorForm
    Inherits Form

    ' ── Control declarations ──────────────────────────────────────────────────
    Private txtIP             As TextBox
    Private txtPort           As TextBox
    Private btnConnect        As Button
    Private btnDisconnect     As Button
    Private lblStatus         As Label
    Private cbxCommand        As ComboBox
    Private lbxDataItems      As ListBox
    Private txtDataEntry      As TextBox
    Private btnAddData        As Button
    Private btnRemoveData     As Button
    Private btnClearData      As Button
    Private cbxByteDataFormat As ComboBox
    Private lblByteDataInfo   As Label
    Private btnLoadByteData   As Button
    Private btnClearByteData  As Button
    Private btnSend           As Button
    Private lbxLog            As ListBox   ' Written to by Log() in Client.vb

    ' ── Constructor ──────────────────────────────────────────────────────────
    Public Sub New()
        BuildUI()
    End Sub

    Private Sub BuildUI()
        Me.Text          = "Radiant TrueTest  —  Client Simulator"
        Me.Size          = New Size(980, 700)
        Me.MinimumSize   = New Size(820, 580)
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Font          = New Font("Segoe UI", 9)
        CheckForIllegalCrossThreadCalls = False

        ' ── TOP BAR ───────────────────────────────────────────────────────────
        Dim pnlConn As New Panel With {
            .Dock = DockStyle.Top, .Height = 52,
            .BackColor = Color.FromArgb(235, 235, 235), .Padding = New Padding(10, 0, 10, 0)
        }
        Dim lblIP As New Label With {.Text = "Server IP:", .AutoSize = True, .Location = New Point(10, 17)}
        txtIP = New TextBox With {.Text = "127.0.0.1", .Width = 115, .Location = New Point(78, 14)}
        Dim lblPort As New Label With {.Text = "Port:", .AutoSize = True, .Location = New Point(205, 17)}
        txtPort = New TextBox With {.Text = "5000", .Width = 58, .Location = New Point(240, 14)}
        btnConnect = New Button With {
            .Text = "Connect", .Width = 118, .Height = 26, .Location = New Point(313, 12),
            .BackColor = Color.FromArgb(0, 120, 215), .ForeColor = Color.White, .FlatStyle = FlatStyle.Flat
        }
        btnConnect.FlatAppearance.BorderSize = 0
        btnDisconnect = New Button With {
            .Text = "Disconnect", .Width = 90, .Height = 26, .Location = New Point(440, 12),
            .BackColor = Color.FromArgb(196, 55, 55), .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat, .Enabled = False
        }
        btnDisconnect.FlatAppearance.BorderSize = 0
        lblStatus = New Label With {
            .Text = "●  Disconnected", .AutoSize = True, .Location = New Point(545, 17),
            .ForeColor = Color.Gray, .Font = New Font("Segoe UI", 9, FontStyle.Bold)
        }
        pnlConn.Controls.AddRange({lblIP, txtIP, lblPort, txtPort, btnConnect, btnDisconnect, lblStatus})

        ' ── SPLIT ─────────────────────────────────────────────────────────────
        Dim split As New SplitContainer With {.Dock = DockStyle.Fill}

        ' ── LEFT : Packet Builder ─────────────────────────────────────────────
        Dim pnlLeft As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(14, 10, 10, 10)}
        Dim lblHeader As New Label With {
            .Text = "PACKET BUILDER", .AutoSize = True, .Location = New Point(14, 10),
            .ForeColor = Color.Gray, .Font = New Font("Segoe UI", 8, FontStyle.Bold)
        }
        Dim lblCmd As New Label With {.Text = "Command:", .AutoSize = True, .Location = New Point(14, 34)}
        cbxCommand = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Width = 240, .DropDownWidth = 360, .Location = New Point(14, 52),
            .DataSource = BuildCommandItems()
        }
        Dim lblData As New Label With {
            .Text = "ASCII Data Fields  (appended after the command, in this order):",
            .AutoSize = True, .Location = New Point(14, 88)
        }
        lbxDataItems = New ListBox With {
            .Location = New Point(14, 108), .Width = 345, .Height = 112, .SelectionMode = SelectionMode.One
        }
        txtDataEntry  = New TextBox With {.Location = New Point(14, 226), .Width = 225}
        btnAddData    = New Button With {.Text = "Add",      .Location = New Point(246, 224), .Width = 50,  .Height = 23}
        btnRemoveData = New Button With {.Text = "Remove",   .Location = New Point(302, 224), .Width = 60,  .Height = 23}
        btnClearData  = New Button With {.Text = "Clear All",.Location = New Point(14, 256),  .Width = 80,  .Height = 23}
        Dim sep1 As New Label With {.BorderStyle = BorderStyle.Fixed3D, .Location = New Point(14, 292), .Width = 350, .Height = 2}
        Dim lblBD  As New Label With {.Text = "Raw ByteData  (always appended LAST in the packet):", .AutoSize = True, .Location = New Point(14, 302)}
        Dim lblFmt As New Label With {.Text = "File format:", .AutoSize = True, .Location = New Point(14, 322)}
        cbxByteDataFormat = New ComboBox With {
            .DropDownStyle = ComboBoxStyle.DropDownList,
            .Width = 300, .DropDownWidth = 430, .Location = New Point(14, 338)
        }
        cbxByteDataFormat.Items.AddRange({
            "Hex text, one byte per line  (Gamma File.hex  /  POCBDATA.hex)",
            "Hex text, space-separated  (OTP File Type 1.txt  /  OTP File Type 2.txt)"
        })
        cbxByteDataFormat.SelectedIndex = 0
        lblByteDataInfo = New Label With {
            .Text = "No ByteData loaded.", .AutoSize = True,
            .MaximumSize = New Size(350, 0), .Location = New Point(14, 368), .ForeColor = Color.Gray
        }
        btnLoadByteData  = New Button With {.Text = "Load from File…",.Location = New Point(14, 414),  .Width = 118, .Height = 24}
        btnClearByteData = New Button With {.Text = "Clear ByteData", .Location = New Point(140, 414), .Width = 112, .Height = 24}
        Dim sep2 As New Label With {.BorderStyle = BorderStyle.Fixed3D, .Location = New Point(14, 452), .Width = 350, .Height = 2}
        btnSend = New Button With {
            .Text = "▶  SEND PACKET", .Location = New Point(14, 462), .Width = 165, .Height = 36,
            .BackColor = Color.FromArgb(14, 140, 60), .ForeColor = Color.White,
            .FlatStyle = FlatStyle.Flat, .Enabled = False, .Font = New Font("Segoe UI", 9, FontStyle.Bold)
        }
        btnSend.FlatAppearance.BorderSize = 0
        pnlLeft.Controls.AddRange({
            lblHeader, lblCmd, cbxCommand, lblData, lbxDataItems,
            txtDataEntry, btnAddData, btnRemoveData, btnClearData,
            sep1, lblBD, lblFmt, cbxByteDataFormat,
            lblByteDataInfo, btnLoadByteData, btnClearByteData, sep2, btnSend
        })
        split.Panel1.Controls.Add(pnlLeft)

        ' ── RIGHT : Communication Log ─────────────────────────────────────────
        Dim pnlRight As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(10)}
        Dim lblLogHeader As New Label With {
            .Text = "COMMUNICATION LOG", .AutoSize = True, .Location = New Point(10, 10),
            .ForeColor = Color.Gray, .Font = New Font("Segoe UI", 8, FontStyle.Bold)
        }
        lbxLog = New ListBox With {
            .Location = New Point(10, 30), .Font = New Font("Consolas", 8.25F),
            .Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right,
            .ScrollAlwaysVisible = True, .HorizontalScrollbar = True
        }
        Dim btnClearLog As New Button With {
            .Text = "Clear Log", .Width = 80, .Height = 24,
            .Anchor = AnchorStyles.Bottom Or AnchorStyles.Right
        }
        pnlRight.Controls.AddRange({lblLogHeader, lbxLog, btnClearLog})
        AddHandler pnlRight.Resize, Sub(s, ev)
            lbxLog.Size          = New Size(pnlRight.ClientSize.Width - 20, pnlRight.ClientSize.Height - 64)
            btnClearLog.Location = New Point(pnlRight.ClientSize.Width - 90, pnlRight.ClientSize.Height - 30)
        End Sub
        AddHandler btnClearLog.Click, Sub(s, ev) lbxLog.Items.Clear()
        split.Panel2.Controls.Add(pnlRight)

        Me.Controls.Add(split)
        Me.Controls.Add(pnlConn)

        ' ── Wire events (handlers in EventHandlers.vb) ────────────────────────
        AddHandler btnConnect.Click,       AddressOf OnConnectClick
        AddHandler btnDisconnect.Click,    AddressOf OnDisconnectClick
        AddHandler btnAddData.Click,       AddressOf OnAddDataClick
        AddHandler btnRemoveData.Click,    AddressOf OnRemoveDataClick
        AddHandler btnClearData.Click,     Sub(s, ev) lbxDataItems.Items.Clear()
        AddHandler btnLoadByteData.Click,  AddressOf OnLoadByteDataClick
        AddHandler btnClearByteData.Click, AddressOf OnClearByteDataClick
        AddHandler btnSend.Click,          AddressOf OnSendClick
        AddHandler Me.FormClosing,         AddressOf OnFormClosing
        AddHandler Me.Load, Sub(s, ev)
                                split.Panel1MinSize    = 310
                                split.Panel2MinSize    = 220
                                split.SplitterDistance = 390
                            End Sub
        AddHandler txtDataEntry.KeyDown, Sub(s, ev)
            If ev.KeyCode = Keys.Enter Then OnAddDataClick(s, ev)
        End Sub
    End Sub

    ' ── State helpers (called from Server.vb) ─────────────────────────────────

    Public Sub SetRetryingState()
        If InvokeRequired Then BeginInvoke(Sub() SetRetryingState()) : Return
        btnConnect.Enabled    = False : btnDisconnect.Enabled = True : btnSend.Enabled = False
        txtIP.Enabled         = False : txtPort.Enabled = False
        lblStatus.Text        = "●  Retrying…" : lblStatus.ForeColor = Color.DarkOrange
    End Sub

    Public Sub SetConnectedState(connected As Boolean)
        If InvokeRequired Then BeginInvoke(Sub() SetConnectedState(connected)) : Return
        btnConnect.Enabled    = Not connected : btnDisconnect.Enabled = connected : btnSend.Enabled = connected
        txtIP.Enabled         = Not connected : txtPort.Enabled = Not connected
        lblStatus.Text        = If(connected, "●  Connected", "●  Disconnected")
        lblStatus.ForeColor   = If(connected, Color.DarkGreen, Color.Gray)
    End Sub

End Class
