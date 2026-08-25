Option Strict On

' =============================================================================
' FTPRecoveryWpf - WPF front end over the FTPRecovery engine.
'
' Replaces the WinForms GUI. Built entirely in code (no XAML, no designer) so it
' still compiles with vbc from build.bat.
'
' Layout is done with Grid/DockPanel and star sizing rather than absolute pixel
' positions, which is what caused labels to overflow in the WinForms version.
' Row resizing is disabled; column resizing and sorting are allowed.
' =============================================================================

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Linq
Imports System.Net.NetworkInformation
Imports System.Threading.Tasks
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Media

Module WpfProgram

    <STAThread>
    Sub Main()
        ' Anything that escapes must land in a file - a silent swallow leaves the
        ' window sitting in a busy state with no explanation, and a silent EXIT
        ' leaves no explanation at all.
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnDomainCrash
        ' Exceptions inside Task.Run are captured by the Task, not thrown - without
        ' this they vanish completely.
        AddHandler TaskScheduler.UnobservedTaskException, AddressOf OnTaskCrash
        AddHandler AppDomain.CurrentDomain.ProcessExit, AddressOf OnProcessExit

        CrashLog("start", "FTPRecoveryGUI started, pid " &
                 System.Diagnostics.Process.GetCurrentProcess().Id.ToString())

        Dim app As New Application()
        AddHandler app.DispatcherUnhandledException, AddressOf OnDispatcherCrash
        app.ShutdownMode = ShutdownMode.OnMainWindowClose
        Dim w As New RecoveryWindow()
        app.Run(w)
    End Sub

    Private Sub OnTaskCrash(sender As Object, e As UnobservedTaskExceptionEventArgs)
        CrashLog("Task", e.Exception.ToString())
        e.SetObserved()
    End Sub

    Private Sub OnProcessExit(sender As Object, e As EventArgs)
        CrashLog("exit", "process exiting normally")
    End Sub

    Friend Sub CrashLog(context As String, text As String)
        Try
            Dim dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Log\Recovery")
            If Not Directory.Exists(dir) Then Directory.CreateDirectory(dir)
            File.AppendAllText(Path.Combine(dir, "gui_error.log"),
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") & "  [" & context & "]" &
                Environment.NewLine & text & Environment.NewLine & Environment.NewLine)
        Catch
        End Try
    End Sub

    Private Sub OnDomainCrash(sender As Object, e As UnhandledExceptionEventArgs)
        CrashLog("AppDomain", Convert.ToString(e.ExceptionObject))
    End Sub

    Private Sub OnDispatcherCrash(sender As Object, e As System.Windows.Threading.DispatcherUnhandledExceptionEventArgs)
        CrashLog("Dispatcher", e.Exception.ToString())
        e.Handled = True
    End Sub

End Module

' One dg row. Plain properties are enough - the collection is rebuilt on each
' scan rather than mutated in place.
Public Class PanelRow
    Public Property Idx As Integer
    Public Property PID As String = ""
    Public Property MadeAt As String = ""
    Public Property Total As Integer
    Public Property HostNow As Integer
    Public Property DoneCount As Integer
    Public Property RetryCount As Integer
    Public Property NewFiles As Integer
    Public Property Rebuilt As Integer
    Public Property Projected As String = ""
    Public Property Verdict As String = ""
    Public Property RowBrush As Brush = Brushes.Transparent
    ' False for a completed panel kept on screen as a record - its queue files are
    ' gone, so there is nothing left to upload.
    Public Property CanUpload As Boolean = True
    Public Property IsDone As Boolean = False
End Class

Public Class RecoveryWindow
    Inherits Window

    Private txtRoot As TextBox
    Private btnBrowse As Button
    Private btnScan As Button
    Private btnUploadOne As Button
    Private btnUploadAll As Button
    Private btnStop As Button
    Private btnAutoRun As Button
    Private chkForce As CheckBox
    Private chkSkipMissing As CheckBox
    Private chkRetryAll As CheckBox
    Private chkVerbose As CheckBox
    Private chkAutoRepeat As CheckBox
    Private cboRepeatEvery As ComboBox
    ' Set while a repeat is scheduled, so Stop and a manual Scan can break the loop.
    Private autoRepeatTimer As System.Windows.Threading.DispatcherTimer = Nothing
    Private autoRepeatRound As Integer = 0
    ' True when the scan now running was started by the repeat loop, so the upload
    ' should follow automatically once it finishes.
    Private autoRepeatUploadAfterScan As Boolean = False
    Private chkReconstruct As CheckBox
    Private cboHost As ComboBox
    Private cboPerSession As ComboBox
    Private cboStall As ComboBox
    Private numRetry As ComboBox
    Private dg As DataGrid
    Private txtLog As TextBox
    Private bar As ProgressBar
    Private lblStatus As TextBlock
    Private lblSession As TextBlock

    ' Live reachability of the FTP servers this queue is configured for. Hosts are
    ' read from line 0 of the queue files, not hardcoded, so the strip is correct
    ' on any machine. A stalled upload is very often a network problem, so it
    ' should be visible without leaving the window.
    '
    ' Always monitored, in this order. Any additional host named by the queue files
    ' (line 0) is appended, so a machine pointed at a different server still shows it.
    Private ReadOnly EXTRA_PING_HOSTS As String() = {"127.0.0.1", "10.119.211.173", "10.119.211.174"}

    Private pingHosts As New List(Of String)()
    Private pingPanel As StackPanel = Nothing
    Private pingLabels As New List(Of TextBlock)()
    Private pingTimer As System.Windows.Threading.DispatcherTimer = Nothing
    Private pingBusy As Boolean = False

    Private panels As New List(Of Program.Panel)()
    Private rows As New ObservableCollection(Of PanelRow)()
    Private busy As Boolean = False

    Public Sub New()
        Title = "FTP Recovery - stalled queue repair      [build " & Program.BuildStamp() & "]"
        Width = 1180
        Height = 800
        MinWidth = 900
        MinHeight = 560
        WindowStartupLocation = WindowStartupLocation.CenterScreen
        WindowState = WindowState.Maximized
        Content = BuildUi()
        txtRoot.Text = Program.DefaultQueueRoot()
        StartLogTimer()
        SetPingHosts(New List(Of String)())     ' extras only until the queue is read
        RefreshPingHosts()
        StartPingTimer()
        AddHandler Me.Closing, AddressOf OnWindowClosing
    End Sub

    ' Closing the window kills the upload thread with it. That is survivable - the
    ' next scan works the state out from disk - but it should be deliberate, and it
    ' must be recorded, or a mid-run close looks exactly like a crash afterwards.
    Private Sub OnWindowClosing(sender As Object, e As ComponentModel.CancelEventArgs)
        If busy Then
            Dim r = MessageBox.Show(Me,
                "An upload is still running." & vbCrLf & vbCrLf &
                "Closing now stops it immediately. Nothing is lost - files already " &
                "sent are recorded, and the queue files that were not sent stay put - " &
                "but the panel being worked on will not have its index/host sent." &
                vbCrLf & vbCrLf & "Close anyway?",
                "Upload in progress", MessageBoxButton.YesNo, MessageBoxImage.Warning)
            If r <> MessageBoxResult.Yes Then
                e.Cancel = True
                Return
            End If
            Program.CancelRequested = True
            WpfProgram.CrashLog("close", "window closed by the user DURING an upload")
            ' The engine's file log is written as it goes, but close the writers so
            ' the last lines are on disk rather than sitting in a buffer.
            Try
                Program.CloseSessionPublic()
                Program.CloseLogsPublic()
            Catch
            End Try
        Else
            WpfProgram.CrashLog("close", "window closed by the user (idle)")
        End If
    End Sub

    ' =====================================================================
    ' UI construction
    ' =====================================================================

    Private Function BuildUi() As UIElement
        Dim root As New Grid() With {.Margin = New Thickness(10)}
        ' 0 path, 1 actions, 2 content (star), 3 status
        root.RowDefinitions.Add(New RowDefinition() With {.Height = GridLength.Auto})
        root.RowDefinitions.Add(New RowDefinition() With {.Height = GridLength.Auto})
        root.RowDefinitions.Add(New RowDefinition() With {.Height = New GridLength(1, GridUnitType.Star), .MinHeight = 200})
        root.RowDefinitions.Add(New RowDefinition() With {.Height = GridLength.Auto})

        root.Children.Add(BuildPathRow())
        root.Children.Add(BuildActionRow())
        root.Children.Add(BuildContent())
        root.Children.Add(BuildStatusBar())
        Return root
    End Function

    ' Table on the left, log on the right, draggable splitter between them.
    Private Function BuildContent() As UIElement
        Dim g As New Grid()
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(3, GridUnitType.Star), .MinWidth = 420})
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(2, GridUnitType.Star), .MinWidth = 220})

        Dim gridPart = BuildGrid()
        Grid.SetColumn(gridPart, 0)

        Dim split As New GridSplitter() With {
            .Width = 6,
            .HorizontalAlignment = HorizontalAlignment.Center,
            .VerticalAlignment = VerticalAlignment.Stretch,
            .ResizeBehavior = GridResizeBehavior.PreviousAndNext,
            .Background = Brushes.Transparent,
            .Cursor = Input.Cursors.SizeWE}
        Grid.SetColumn(split, 1)

        Dim logPart = BuildLog()
        Grid.SetColumn(logPart, 2)

        g.Children.Add(gridPart)
        g.Children.Add(split)
        g.Children.Add(logPart)
        Grid.SetRow(g, 2)
        Return g
    End Function

    Private Function BuildPathRow() As UIElement
        Dim g As New Grid() With {.Margin = New Thickness(0, 0, 0, 8)}
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})

        Dim lbl As New TextBlock() With {
            .Text = "Queue folder:",
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(0, 0, 8, 0)}
        Grid.SetColumn(lbl, 0)

        txtRoot = New TextBox() With {.VerticalContentAlignment = VerticalAlignment.Center, .Padding = New Thickness(4, 3, 4, 3)}
        Grid.SetColumn(txtRoot, 1)

        btnBrowse = New Button() With {
            .Content = "Browse...", .Padding = New Thickness(12, 3, 12, 3),
            .Margin = New Thickness(8, 0, 0, 0)}
        AddHandler btnBrowse.Click, AddressOf OnBrowse
        Grid.SetColumn(btnBrowse, 2)

        g.Children.Add(lbl)
        g.Children.Add(txtRoot)
        g.Children.Add(btnBrowse)
        Grid.SetRow(g, 0)
        Return g
    End Function

    ' Buttons on the left, options on the right. WrapPanel means a narrow window
    ' reflows instead of clipping the option labels.
    Private Function BuildActionRow() As UIElement
        Dim outer As New Grid() With {.Margin = New Thickness(0, 0, 0, 8)}
        outer.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})
        outer.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})

        Dim buttons As New WrapPanel()
        ' The one-click unattended cycle goes first and is styled to stand out - it
        ' is what most runs will use.
        btnAutoRun = MakeButton("AUTO: scan + upload, repeat", AddressOf OnAutoRun, True)
        btnAutoRun.FontWeight = FontWeights.Bold
        btnAutoRun.Background = New SolidColorBrush(Color.FromRgb(214, 234, 255))
        btnAutoRun.BorderBrush = New SolidColorBrush(Color.FromRgb(90, 140, 200))
        btnAutoRun.BorderThickness = New Thickness(1.5)
        btnAutoRun.Padding = New Thickness(14, 4, 14, 4)
        btnAutoRun.Margin = New Thickness(0, 0, 14, 0)

        btnScan = MakeButton("Start Scan", AddressOf OnScan, True)
        btnUploadOne = MakeButton("Upload this PID", AddressOf OnUploadOne, False)
        btnUploadAll = MakeButton("Upload ALL panels", AddressOf OnUploadAll, False)
        btnStop = MakeButton("Stop", AddressOf OnStop, False)
        buttons.Children.Add(btnAutoRun)
        buttons.Children.Add(btnScan)
        buttons.Children.Add(btnUploadOne)
        buttons.Children.Add(btnUploadAll)
        buttons.Children.Add(btnStop)
        Grid.SetColumn(buttons, 0)

        Dim opts As New WrapPanel() With {.HorizontalAlignment = HorizontalAlignment.Right}
        chkReconstruct = MakeCheck("Reconstruct from disk")
        ' On by default: without it, panels whose upload instructions were lost are
        ' reported as unfixable when the images are usually still on disk. It only
        ' ever adds files that pass the allow/deny rules, so it is safe to leave on.
        chkReconstruct.IsChecked = True
        chkForce = MakeCheck("Force incomplete")
        chkSkipMissing = MakeCheck("Skip missing source")
        chkRetryAll = MakeCheck("Keep retrying if server down")
        chkRetryAll.IsChecked = True
        ' Untick to stop the per-file lines. Panel headers, warnings and results
        ' still appear - much easier to follow when files go past several a second.
        chkVerbose = MakeCheck("Log every file")
        chkVerbose.IsChecked = True
        ' Takes effect immediately, even mid-run - it only changes what is printed.
        AddHandler chkVerbose.Checked,
            Sub() Program.VerboseFileLog = True
        AddHandler chkVerbose.Unchecked,
            Sub()
                Program.VerboseFileLog = False
                AppendLog(">>> per-file logging off - panel results still shown.")
            End Sub
        ' All three change what the table says, so all three re-classify.
        For Each cb In New CheckBox() {chkReconstruct, chkForce, chkSkipMissing}
            AddHandler cb.Checked, AddressOf OnOptionToggled
            AddHandler cb.Unchecked, AddressOf OnOptionToggled
        Next

        ' Send to a specific server instead of the one named in the queue files.
        ' "Auto" keeps the queue's own host, which is the normal case.
        Dim lblHost As New TextBlock() With {
            .Text = "Send to:", .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(12, 0, 6, 0)}
        cboHost = New ComboBox() With {
            .Width = 165, .VerticalContentAlignment = VerticalAlignment.Center}
        cboHost.Items.Add("Auto (from queue)")
        cboHost.Items.Add("10.119.211.173")
        cboHost.Items.Add("10.119.211.174")
        cboHost.SelectedIndex = 0
        AddHandler cboHost.SelectionChanged, AddressOf OnHostOverrideChanged

        ' How many files to send down one FTP session before opening a fresh one.
        ' LGD asked for 100; too many rapid logins is what their server refuses.
        Dim lblPer As New TextBlock() With {
            .Text = "Files/session:", .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(12, 0, 6, 0)}
        cboPerSession = New ComboBox() With {
            .Width = 95, .VerticalContentAlignment = VerticalAlignment.Center}
        For Each v In New String() {"50", "100", "200", "500", "No limit"}
            cboPerSession.Items.Add(v)
        Next
        cboPerSession.SelectedIndex = 1          ' 100, as LGD asked

        ' Scan and upload again after each run finishes. Useful for a large backlog
        ' and for SERVER-OFFLINE panels, which need nothing but another attempt.
        chkAutoRepeat = MakeCheck("Repeat automatically")
        cboRepeatEvery = New ComboBox() With {
            .Width = 95, .VerticalContentAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(6, 0, 0, 0)}
        For Each v In New String() {"at once", "1 min", "5 min", "15 min", "30 min"}
            cboRepeatEvery.Items.Add(v)
        Next
        cboRepeatEvery.SelectedIndex = 1          ' 1 min

        Dim lblStall As New TextBlock() With {
            .Text = "Stall:", .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(12, 0, 6, 0)}
        cboStall = New ComboBox() With {
            .Width = 85, .VerticalContentAlignment = VerticalAlignment.Center}
        For Each v In New String() {"30s", "60s", "90s", "180s", "No limit"}
            cboStall.Items.Add(v)
        Next
        cboStall.SelectedIndex = 0          ' 30s

        Dim lblRetry As New TextBlock() With {
            .Text = "Retries:", .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(12, 0, 6, 0)}
        numRetry = New ComboBox() With {
            .Width = 55, .VerticalContentAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(0, 0, 0, 0)}
        For v = 1 To 10
            numRetry.Items.Add(v.ToString())
        Next
        numRetry.SelectedIndex = 2   ' 3 attempts

        ' Each label and its dropdown go in together. Added separately, the
        ' WrapPanel could break the line between them and clip the label.
        opts.Children.Add(chkReconstruct)
        opts.Children.Add(chkForce)
        opts.Children.Add(chkSkipMissing)
        opts.Children.Add(chkRetryAll)
        opts.Children.Add(chkVerbose)
        opts.Children.Add(Pair(chkAutoRepeat, cboRepeatEvery))
        opts.Children.Add(Pair(lblHost, cboHost))
        opts.Children.Add(Pair(lblPer, cboPerSession))
        opts.Children.Add(Pair(lblStall, cboStall))
        opts.Children.Add(Pair(lblRetry, numRetry))
        Grid.SetColumn(opts, 1)

        outer.Children.Add(buttons)
        outer.Children.Add(opts)
        Grid.SetRow(outer, 1)
        Return outer
    End Function

    ' Keeps a caption and its control on the same line, with breathing room from
    ' whatever comes before it.
    Private Function Pair(caption As UIElement, ctrl As UIElement) As UIElement
        Dim sp As New StackPanel() With {
            .Orientation = Orientation.Horizontal,
            .Margin = New Thickness(14, 0, 0, 0),
            .VerticalAlignment = VerticalAlignment.Center}
        sp.Children.Add(caption)
        sp.Children.Add(ctrl)
        Return sp
    End Function

    Private Function MakeButton(text As String, handler As RoutedEventHandler, enabled As Boolean) As Button
        Dim b As New Button() With {
            .Content = text,
            .Padding = New Thickness(14, 5, 14, 5),
            .Margin = New Thickness(0, 0, 8, 0),
            .IsEnabled = enabled,
            .MinWidth = 90}
        AddHandler b.Click, handler
        Return b
    End Function

    Private Function MakeCheck(text As String) As CheckBox
        Return New CheckBox() With {
            .Content = text,
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(12, 0, 0, 0)}
    End Function

    Private Function BuildGrid() As UIElement
        dg = New DataGrid() With {
            .AutoGenerateColumns = False,
            .IsReadOnly = True,
            .CanUserAddRows = False,
            .CanUserDeleteRows = False,
            .CanUserResizeRows = False,
            .CanUserResizeColumns = True,
            .CanUserSortColumns = True,
            .SelectionMode = DataGridSelectionMode.Single,
            .SelectionUnit = DataGridSelectionUnit.FullRow,
            .HeadersVisibility = DataGridHeadersVisibility.Column,
            .GridLinesVisibility = DataGridGridLinesVisibility.Horizontal,
            .HorizontalGridLinesBrush = New SolidColorBrush(Color.FromRgb(225, 228, 232)),
            .RowHeaderWidth = 0,
            .EnableRowVirtualization = True,
            .ItemsSource = rows}

        ' Per-row background driven by the row object, so verdict colouring
        ' survives sorting - unlike setting it on a row index.
        Dim rowStyle As New Style(GetType(DataGridRow))
        rowStyle.Setters.Add(New Setter(DataGridRow.BackgroundProperty, New Binding("RowBrush")))
        dg.RowStyle = rowStyle

        AddCol("PID", "PID", 2, True)
        ' When the panel was made. The list runs oldest first, so this is how the
        ' operator can see that the top row really is the front of the queue.
        AddCol("MadeAt", "Panel date", 0, False)
        AddCol("Total", "Total", 0, False)
        AddCol("HostNow", "Host now", 0, False)
        AddCol("DoneCount", "Done", 0, False)
        AddCol("RetryCount", "Retry", 0, False)
        AddCol("NewFiles", "New", 0, False)
        AddCol("Rebuilt", "Rebuilt", 0, False)
        AddCol("Projected", "Projected", 0, False)
        AddCol("Verdict", "Verdict", 3, True)
        AddUploadColumn()

        AddHandler dg.SelectionChanged, AddressOf OnGridSelection
        Return dg
    End Function

    ' star > 0 gives a proportional column; otherwise size to header+content so
    ' numbers never clip.
    Private Sub AddCol(path As String, header As String, star As Integer, wrap As Boolean)
        Dim c As New DataGridTextColumn() With {
            .Header = header,
            .Binding = New Binding(path)}
        If star > 0 Then
            c.Width = New DataGridLength(star, DataGridLengthUnitType.Star)
        Else
            c.Width = New DataGridLength(1, DataGridLengthUnitType.Auto)
        End If
        Dim st As New Style(GetType(TextBlock))
        st.Setters.Add(New Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center))
        st.Setters.Add(New Setter(TextBlock.MarginProperty, New Thickness(6, 0, 6, 0)))
        If wrap Then
            st.Setters.Add(New Setter(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis))
        Else
            st.Setters.Add(New Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right))
        End If
        c.ElementStyle = st
        dg.Columns.Add(c)
    End Sub

    Private Sub AddUploadColumn()
        Dim f As New FrameworkElementFactory(GetType(Button))
        f.SetValue(Button.ContentProperty, "Upload")
        f.SetValue(Button.PaddingProperty, New Thickness(10, 2, 10, 2))
        f.SetValue(Button.MarginProperty, New Thickness(2))
        f.SetBinding(Button.IsEnabledProperty, New Binding("CanUpload"))
        f.AddHandler(ButtonBase.ClickEvent, New RoutedEventHandler(AddressOf OnRowUpload))

        Dim tmpl As New DataTemplate()
        tmpl.VisualTree = f

        ' Auto sizing measures the header (empty) rather than the templated
        ' button, which collapsed the column. Fixed width instead.
        dg.Columns.Add(New DataGridTemplateColumn() With {
            .Header = "",
            .CellTemplate = tmpl,
            .Width = New DataGridLength(92),
            .MinWidth = 92,
            .CanUserResize = False})
    End Sub

    Private Function BuildLog() As UIElement
        txtLog = New TextBox() With {
            .IsReadOnly = True,
            .TextWrapping = TextWrapping.NoWrap,
            .VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            .HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            .FontFamily = New FontFamily("Consolas"),
            .FontSize = 11.5,
            .Background = Brushes.White,
            .Padding = New Thickness(4)}
        Return txtLog
    End Function

    Private Function BuildStatusBar() As UIElement
        Dim g As New Grid() With {.Margin = New Thickness(0, 8, 0, 0)}
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})

        Dim pings = BuildPingStrip()
        Grid.SetColumn(pings, 0)

        ' Fixed readout of the current FTP session - in the log it scrolls away
        ' within a second, and it is the thing to watch when the customer's server
        ' is refusing connections.
        lblSession = New TextBlock() With {
            .Text = "no FTP session",
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(16, 0, 0, 0),
            .FontFamily = New FontFamily("Consolas"),
            .FontSize = 11.5,
            .Foreground = Brushes.Gray}
        Grid.SetColumn(lblSession, 1)

        bar = New ProgressBar() With {.Height = 18, .Minimum = 0, .Maximum = 1, .Value = 0,
                                      .Margin = New Thickness(12, 0, 0, 0)}
        Grid.SetColumn(bar, 2)

        lblStatus = New TextBlock() With {
            .Text = "Idle",
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(10, 0, 0, 0),
            .MinWidth = 160,
            .TextTrimming = TextTrimming.CharacterEllipsis}
        Grid.SetColumn(lblStatus, 3)

        g.Children.Add(pings)
        g.Children.Add(lblSession)
        g.Children.Add(bar)
        g.Children.Add(lblStatus)
        Grid.SetRow(g, 3)
        Return g
    End Function

    Private Function BuildPingStrip() As UIElement
        pingPanel = New StackPanel() With {.Orientation = Orientation.Horizontal,
                                           .VerticalAlignment = VerticalAlignment.Center}
        Return pingPanel
    End Function

    ' Rebuild the strip for a given set of hosts. Called at startup and after every
    ' scan, so changing the queue folder retargets the monitor automatically.
    Private Sub SetPingHosts(hosts As List(Of String))
        Dim merged As New List(Of String)()
        Dim seen As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        ' Fixed list first, in the order given, then anything extra the queue names.
        For Each h In EXTRA_PING_HOSTS
            If h <> "" AndAlso seen.Add(h) Then merged.Add(h)
        Next
        For Each h In hosts
            If h <> "" AndAlso seen.Add(h) Then merged.Add(h)
        Next

        ' Nothing changed - don't rebuild and lose the current readings.
        If merged.Count = pingHosts.Count AndAlso
           merged.SequenceEqual(pingHosts, StringComparer.OrdinalIgnoreCase) Then Exit Sub

        pingHosts = merged
        pingPanel.Children.Clear()
        pingLabels.Clear()

        If pingHosts.Count = 0 Then
            pingPanel.Children.Add(New TextBlock() With {
                .Text = "no FTP host found in the queue",
                .Foreground = Brushes.Gray,
                .FontFamily = New FontFamily("Consolas"),
                .FontSize = 11.5,
                .VerticalAlignment = VerticalAlignment.Center})
            Exit Sub
        End If

        For Each h In pingHosts
            Dim t As New TextBlock() With {
                .Text = h & "  ...",
                .Margin = New Thickness(0, 0, 16, 0),
                .VerticalAlignment = VerticalAlignment.Center,
                .FontFamily = New FontFamily("Consolas"),
                .FontSize = 11.5,
                .Foreground = Brushes.Gray}
            pingLabels.Add(t)
            pingPanel.Children.Add(t)
        Next
    End Sub

    ' Look at the queue folder for the hosts to watch, off the UI thread.
    Private Sub RefreshPingHosts()
        Dim rootPath = txtRoot.Text.TrimEnd("\"c)
        Task.Run(Sub()
                     Dim hosts = Program.HostsInQueue(rootPath)
                     Dispatcher.BeginInvoke(New Action(Sub() SetPingHosts(hosts)))
                 End Sub)
    End Sub

    ' =====================================================================
    ' Ping
    ' =====================================================================

    Private Sub StartPingTimer()
        PingTick(Nothing, Nothing)                     ' first reading immediately
        pingTimer = New System.Windows.Threading.DispatcherTimer()
        ' 30s, not 3s. Each check is a real TCP connect to port 21, so at 3s with
        ' three hosts this was 1,200 connections per hour to each FTP server, all
        ' day, whether or not anything was being uploaded. The customer's server
        ' counts those.
        pingTimer.Interval = TimeSpan.FromSeconds(30)
        AddHandler pingTimer.Tick, AddressOf PingTick
        pingTimer.Start()
    End Sub

    Private Sub PingTick(sender As Object, e As EventArgs)
        If pingBusy Then Return                        ' don't stack slow rounds

        ' Do not probe while uploading. The upload itself is the proof of
        ' reachability, and adding a connection every 30s to a server that is
        ' already busy receiving files is exactly the load the customer objected
        ' to. The session readout beside this strip shows the real state anyway.
        If busy Then
            ' Grey the readings so they don't look live while probing is paused.
            For Each t In pingLabels
                t.Foreground = Brushes.Silver
            Next
            Return
        End If

        Dim targets = pingHosts.ToList()               ' snapshot: the list can be rebuilt
        If targets.Count = 0 Then Return
        pingBusy = True

        Task.Run(Sub()
                     For i = 0 To targets.Count - 1
                         Dim idx = i
                         ' ICMP ping, NOT a connect to port 21. A TCP check would
                         ' be a better test of "can we actually upload", but it is
                         ' another connection to the FTP server every time it runs,
                         ' and the customer's server counts those. Ping touches no
                         ' FTP port at all.
                         '
                         ' Caveat: a network can block ping while allowing FTP, so
                         ' red here does not always mean uploads will fail.
                         Dim label As String
                         Dim colour As Brush
                         Try
                             Using pg As New Ping()
                                 Dim r = pg.Send(targets(idx), 1500)
                                 If r IsNot Nothing AndAlso r.Status = IPStatus.Success Then
                                     label = r.RoundtripTime.ToString() & " ms"
                                     colour = If(r.RoundtripTime > 200,
                                                 CType(Brushes.DarkOrange, Brush),
                                                 CType(Brushes.Green, Brush))
                                 Else
                                     label = "no reply"
                                     colour = Brushes.Red
                                 End If
                             End Using
                         Catch
                             label = "unreachable"
                             colour = Brushes.Red
                         End Try

                         Dim text = targets(idx) & "  " & label
                         Dispatcher.BeginInvoke(New Action(
                             Sub()
                                 ' The strip may have been rebuilt mid-round.
                                 If idx < pingLabels.Count AndAlso
                                    String.Equals(pingHosts(idx), targets(idx), StringComparison.OrdinalIgnoreCase) Then
                                     pingLabels(idx).Text = text
                                     pingLabels(idx).Foreground = colour
                                 End If
                             End Sub))
                     Next
                 End Sub).ContinueWith(Sub() pingBusy = False)
    End Sub

    ' =====================================================================
    ' Dispatcher-safe UI updates
    ' =====================================================================

    ' The engine logs a line per file. Marshalling each one to the UI thread
    ' individually floods the dispatcher queue and starves input, which is what
    ' made the window unresponsive during a 12,000-file run. Instead the worker
    ' appends to a buffer and a timer flushes it a few times a second.
    Private ReadOnly logBuffer As New Queue(Of String)()
    Private ReadOnly logLock As New Object()
    Private logTimer As System.Windows.Threading.DispatcherTimer = Nothing
    Private logDroppedNotice As Boolean = False

    Private Sub AppendLog(text As String)
        SyncLock logLock
            ' Cap the buffer: if the FTP server is down, failures arrive faster
            ' than anyone can read them. Keep the newest.
            If logBuffer.Count > 4000 Then
                logBuffer.Dequeue()
                logDroppedNotice = True
            End If
            logBuffer.Enqueue(text)
        End SyncLock
    End Sub

    Private Sub StartLogTimer()
        If logTimer IsNot Nothing Then Return
        logTimer = New System.Windows.Threading.DispatcherTimer()
        logTimer.Interval = TimeSpan.FromMilliseconds(250)
        AddHandler logTimer.Tick, AddressOf FlushLog
        logTimer.Start()
    End Sub

    ' Live FTP session readout. Green while a session is open, and it shows how
    ' many files have gone down it - so "are we opening too many sessions?" is
    ' answerable at a glance instead of by scrolling the log.
    Private Sub UpdateSessionLabel()
        If lblSession Is Nothing Then Return
        Dim n = Program.SessionNumber
        If n = 0 Then
            lblSession.Text = "no FTP session"
            lblSession.Foreground = Brushes.Gray
            Return
        End If
        Dim used = Program.FilesThisSession
        Dim cap = Program.FilesPerSession
        Dim txt = "FTP session #" & n.ToString() & "  " &
                  used.ToString() & If(cap > 0, " / " & cap.ToString(), "") & " file(s)"

        ' Live throughput. Files/sec is the number that matters for an ETA; MB/sec
        ' shows whether the link or the per-file overhead is the limit.
        If Program.UploadStart <> DateTime.MinValue Then
            Dim secs = DateTime.Now.Subtract(Program.UploadStart).TotalSeconds
            If secs >= 1 Then
                Dim fps = Program.nUploaded / secs
                Dim mbps = (Program.UploadedBytes / 1048576.0) / secs
                txt &= "   |   " & fps.ToString("0.0") & " files/s  " &
                       mbps.ToString("0.00") & " MB/s"
            End If
        End If

        lblSession.Text = txt
        lblSession.Foreground = If(busy, CType(Brushes.Green, Brush), CType(Brushes.Gray, Brush))
    End Sub

    Private Sub FlushLog(sender As Object, e As EventArgs)
        ' Before the early-out below: the rate must keep ticking even when no new
        ' log lines are arriving, e.g. during one slow file.
        UpdateSessionLabel()
        Dim chunk As String = ""
        Dim dropped As Boolean = False
        SyncLock logLock
            If logBuffer.Count = 0 AndAlso Not logDroppedNotice Then Return
            Dim sb As New Text.StringBuilder()
            While logBuffer.Count > 0
                sb.AppendLine(logBuffer.Dequeue())
            End While
            chunk = sb.ToString()
            dropped = logDroppedNotice
            logDroppedNotice = False
        End SyncLock

        If txtLog.Text.Length > 400000 Then txtLog.Clear()
        If dropped Then txtLog.AppendText("... (older lines dropped - see the log file for the full trace)" & Environment.NewLine)
        If chunk <> "" Then
            ' Only follow the tail if the view is already at the bottom. Scroll up
            ' to read and it stays put; scroll back down and it resumes following.
            Dim atBottom = (txtLog.VerticalOffset + txtLog.ViewportHeight) >= (txtLog.ExtentHeight - 4)
            txtLog.AppendText(chunk)
            If atBottom Then txtLog.ScrollToEnd()
        End If
    End Sub

    Private Sub SetStatus(text As String)
        If Not Dispatcher.CheckAccess() Then
            Dispatcher.BeginInvoke(New Action(Of String)(AddressOf SetStatus), text)
            Return
        End If
        lblStatus.Text = text
    End Sub

    Private Sub SetProgress(value As Integer, max As Integer)
        If Not Dispatcher.CheckAccess() Then
            Dispatcher.BeginInvoke(New Action(Of Integer, Integer)(AddressOf SetProgress), value, max)
            Return
        End If
        bar.Maximum = Math.Max(1, max)
        bar.Value = Math.Min(value, bar.Maximum)
    End Sub

    Private Sub SetBusy(state As Boolean)
        If Not Dispatcher.CheckAccess() Then
            Dispatcher.BeginInvoke(New Action(Of Boolean)(AddressOf SetBusy), state)
            Return
        End If
        busy = state
        btnScan.IsEnabled = Not state
        btnBrowse.IsEnabled = Not state
        txtRoot.IsEnabled = Not state
        btnUploadAll.IsEnabled = (Not state) AndAlso panels.Count > 0
        btnUploadOne.IsEnabled = (Not state) AndAlso HasUploadableSelection()
        dg.IsEnabled = Not state
        ' Stop stays available while a repeat is pending, not just while a run is in
        ' progress - otherwise the log says "Press Stop to end" next to a greyed
        ' out button and the only way out is unticking the box.
        btnStop.IsEnabled = state OrElse (autoRepeatTimer IsNot Nothing)
        btnAutoRun.IsEnabled = Not state
        ' Options are read once at the start of a run; letting them change mid-run
        ' would mean half the panels used different rules.
        chkReconstruct.IsEnabled = Not state
        chkForce.IsEnabled = Not state
        chkSkipMissing.IsEnabled = Not state
        chkRetryAll.IsEnabled = Not state
        ' Verbose is safe to change mid-run - it only affects what is printed.
        chkVerbose.IsEnabled = True
        ' Safe to change mid-run: it is only read when a pass finishes.
        chkAutoRepeat.IsEnabled = True
        cboRepeatEvery.IsEnabled = True
        cboHost.IsEnabled = Not state
        cboPerSession.IsEnabled = Not state
        cboStall.IsEnabled = Not state
        numRetry.IsEnabled = Not state
        ' Deliberately NOT setting a wait cursor. The window stays fully
        ' responsive during a run, and a spinning cursor reads as "frozen" -
        ' the progress bar, status text and streaming log are the honest signal.
    End Sub

    Private Function HasUploadableSelection() As Boolean
        Dim r = TryCast(dg.SelectedItem, PanelRow)
        Return r IsNot Nothing AndAlso r.CanUpload
    End Function

    ' Changing the destination server does not alter the table - only where the
    ' files go - so no re-classify is needed. It is loud in the log instead,
    ' because sending to the wrong server is not something to do by accident.
    Private Sub OnHostOverrideChanged(sender As Object, e As SelectionChangedEventArgs)
        If cboHost Is Nothing OrElse busy Then Return
        AppendLog("")
        If cboHost.SelectedIndex > 0 Then
            AppendLog(">>> SEND TO: " & Convert.ToString(cboHost.SelectedItem) &
                      "  - the server named in the queue files will be IGNORED.")
        Else
            AppendLog(">>> SEND TO: Auto - each panel goes to the server named in its own queue file.")
        End If
    End Sub

    Private Sub ApplySettings(execute As Boolean)
        Program.QueueRoot = txtRoot.Text.TrimEnd("\"c)
        Program.DoExecute = execute
        Program.ForceIncomplete = chkForce.IsChecked.GetValueOrDefault()
        Program.SkipMissingSource = chkSkipMissing.IsChecked.GetValueOrDefault()
        Program.RetryEveryFileWhenDown = chkRetryAll.IsChecked.GetValueOrDefault()
        Program.VerboseFileLog = chkVerbose.IsChecked.GetValueOrDefault()
        Program.Reconstruct = chkReconstruct.IsChecked.GetValueOrDefault()
        ' Index 0 is "Auto (from queue)" - anything else is a literal host.
        Program.HostOverride = If(cboHost.SelectedIndex > 0,
                                  Convert.ToString(cboHost.SelectedItem), "")
        Dim per As Integer = 0
        Integer.TryParse(Convert.ToString(cboPerSession.SelectedItem), per)   ' "No limit" -> 0
        Program.FilesPerSession = per
        Dim stall As Integer = 0
        Integer.TryParse(Convert.ToString(cboStall.SelectedItem).Replace("s", ""), stall)
        Program.StallTimeoutSeconds = stall      ' "No limit" -> 0
        Dim r As Integer = 3
        Integer.TryParse(CStr(numRetry.SelectedItem), r)
        Program.MaxRetry = r
        Program.OnlyPid = ""
        Program.LogSink = AddressOf AppendLog
    End Sub

    ' =====================================================================
    ' Scan
    ' =====================================================================

    Private Sub OnBrowse(sender As Object, e As RoutedEventArgs)
        Using dlg As New System.Windows.Forms.FolderBrowserDialog()
            dlg.Description = "Select the FTPUploaderVB upload queue folder"
            If Directory.Exists(txtRoot.Text) Then dlg.SelectedPath = txtRoot.Text
            If dlg.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                txtRoot.Text = dlg.SelectedPath
            End If
        End Using
    End Sub

    Private Sub OnScan(sender As Object, e As RoutedEventArgs)
        ' A scan the user started means they have taken over - drop any pending
        ' repeat. A scan the repeat loop started must not cancel its own loop.
        If Not autoRepeatUploadAfterScan Then CancelRepeat("")
        If Not Directory.Exists(txtRoot.Text) Then
            MessageBox.Show(Me, "Queue folder not found:" & vbCrLf & txtRoot.Text,
                            "Scan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        SetBusy(True)
        rows.Clear()
        txtLog.Clear()
        RefreshPingHosts()          ' the folder may have changed since last time
        ' An explicit Scan starts a fresh session - drop the completed-panel history.
        Program.ClearOutcomes()
        ApplySettings(False)
        Program.ResetRun()

        Dim found As New List(Of Program.Panel)()

        Task.Run(Sub()
                     Try
                         AppendLog("FTP Recovery  [build " & Program.BuildStamp() & "]")
                         AppendLog("Scanning " & Program.QueueRoot & " ...")
                         Dim entries = Program.ScanQueueFiles()
                         AppendLog("Parsed " & entries.Count.ToString() & " queue file(s).")
                         found = Program.BuildPanels(entries)
                         AppendLog("Grouped into " & found.Count.ToString() & " panel(s).")
                     Catch ex As Exception
                         AppendLog("SCAN FAILED: " & ex.Message)
                         WpfProgram.CrashLog("Scan", ex.ToString())
                     End Try
                 End Sub).ContinueWith(
            Sub()
                Dispatcher.BeginInvoke(New Action(
                    Sub()
                        ' Whatever happens, the UI must come back out of the busy
                        ' state - otherwise the window looks frozen with no clue why.
                        Try
                            panels = found
                            FillGrid()
                        Catch ex As Exception
                            AppendLog("GRID FAILED: " & ex.Message)
                            WpfProgram.CrashLog("FillGrid", ex.ToString())
                        Finally
                            SetBusy(False)
                            SetStatus(panels.Count.ToString() & " panel(s)")
                            AfterScanForRepeat()
                        End Try
                    End Sub))
            End Sub)
    End Sub

    ' Start the unattended cycle from one button: scan, upload all, scan again,
    ' repeat until Stop. Ticks the repeat option so the state of the window matches
    ' what is actually happening.
    Private Sub OnAutoRun(sender As Object, e As RoutedEventArgs)
        If busy Then Return
        If Not Directory.Exists(txtRoot.Text) Then
            MessageBox.Show(Me, "Queue folder not found:" & vbCrLf & txtRoot.Text,
                            "Auto run", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If
        chkAutoRepeat.IsChecked = True
        autoRepeatRound = 0
        AppendLog("")
        AppendLog(">>> AUTO RUN started - scan, upload, repeat. Press Stop to end.")
        If chkForce.IsChecked.GetValueOrDefault() Then
            AppendLog("    CAUTION: 'Force incomplete' is on. A panel that TrueTest is still")
            AppendLog("    writing looks incomplete, and Force will finalise it short rather")
            AppendLog("    than waiting for the rest of its images. On a live machine, untick")
            AppendLog("    Force and let repeated passes pick panels up as they finish.")
        End If
        ' Tell the scan-completion hook to carry straight on into the upload.
        autoRepeatUploadAfterScan = True
        OnScan(Nothing, Nothing)
    End Sub

    ' A scan started by the repeat loop has finished. Upload if there is anything
    ' to upload, otherwise go straight back to waiting for the next round.
    Private Sub AfterScanForRepeat()
        If Not autoRepeatUploadAfterScan Then Return
        autoRepeatUploadAfterScan = False
        If Not chkAutoRepeat.IsChecked.GetValueOrDefault() Then Return
        If Program.CancelRequested Then
            CancelRepeat("stopped by the user")
            Return
        End If

        Dim canDo = panels.Where(Function(p)
                                     Dim st = Program.Classify(p)
                                     Return st.CanComplete OrElse st.WillForce
                                 End Function).Count()
        If canDo > 0 Then
            AppendLog(">>> auto repeat: uploading " & canDo.ToString() & " panel(s) ...")
            OnUploadAll(Nothing, Nothing)
        Else
            ' Nothing uploadable - schedule the next scan rather than stopping.
            MaybeScheduleRepeat()
        End If
    End Sub

    Private Sub FillGrid()
        rows.Clear()
        Dim ready As Integer = 0
        Dim pink = New SolidColorBrush(Color.FromRgb(255, 235, 235))
        Dim blue = New SolidColorBrush(Color.FromRgb(233, 243, 255))

        Dim green = New SolidColorBrush(Color.FromRgb(232, 245, 233))
        Dim amber = New SolidColorBrush(Color.FromRgb(255, 244, 224))
        ' Ready, but only because entries were rebuilt from disk - worth telling
        ' apart from a panel whose queue was complete all along.
        Dim lilac = New SolidColorBrush(Color.FromRgb(243, 236, 255))
        ' Will be sent, but short, because Force is on - not blocked, but not clean.
        Dim orange = New SolidColorBrush(Color.FromRgb(255, 226, 196))

        Dim live As New List(Of PanelRow)()
        Dim liveKeys As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For i = 0 To panels.Count - 1
            Dim p = panels(i)
            liveKeys.Add(p.Key)
            Dim st = Program.Classify(p)
            Dim r As New PanelRow() With {
                .Idx = i,
                .PID = p.PID,
                .MadeAt = Program.PanelStampDisplay(p),
                .Total = p.Total,
                .HostNow = st.HostNow,
                .DoneCount = st.DoneCount,
                .RetryCount = st.RetryCount,
                .NewFiles = st.NewCount,
                .Rebuilt = st.Rebuilt,
                .Projected = st.Projected.ToString() & " / " & p.Total.ToString(),
                .Verdict = st.Verdict,
                .CanUpload = True}
            If Not st.CanComplete Then
                ' Force turns "blocked" into "will send, short" - colour it as an
                ' outcome to check rather than an obstacle.
                r.RowBrush = If(st.WillForce, orange, pink)
            Else
                ready += 1
                If st.Rebuilt > 0 Then
                    r.RowBrush = lilac          ' ready only thanks to reconstruction
                ElseIf st.NewCount = 0 AndAlso st.RetryCount = 0 Then
                    r.RowBrush = blue
                End If
            End If
            live.Add(r)
        Next

        ' Panels already processed no longer have queue files, so a scan cannot see
        ' them. Keep them on screen with their result rather than letting them
        ' silently disappear - the grid then reads as a record of the session.
        Dim done As New List(Of PanelRow)()
        For Each kv In Program.Outcomes
            If liveKeys.Contains(kv.Key) Then Continue For
            Dim o = kv.Value
            done.Add(New PanelRow() With {
                .Idx = -1,
                .PID = o.PID,
                .MadeAt = o.MadeAt,
                .Total = o.Total,
                .HostNow = o.HostAfter,
                .DoneCount = o.Uploaded,
                .RetryCount = 0,
                .NewFiles = 0,
                .Rebuilt = o.Rebuilt,
                .Projected = o.HostAfter.ToString() & " / " & o.Total.ToString(),
                .Verdict = o.Result,
                .CanUpload = False,
                .IsDone = True,
                .RowBrush = If(o.Result.StartsWith("INDEX+HOST SENT"), green, amber)})
        Next

        ' Pending rows keep the order the engine will upload them in - oldest panel
        ' first - so the table reads top-to-bottom as the run will proceed.
        ' Re-sorting by PID here would show a different order from what happens.
        For Each r In live
            rows.Add(r)
        Next
        For Each r In done.OrderBy(Function(x) x.PID)
            rows.Add(r)
        Next

        AppendLog("")
        AppendLog(ready.ToString() & " of " & panels.Count.ToString() & " pending panel(s) can complete.")
        Dim viaRebuild = live.Where(Function(r) r.Rebuilt > 0).Count()
        If viaRebuild > 0 Then
            AppendLog(viaRebuild.ToString() & " of those depend on rebuilt entries (shown in lilac).")
        End If
        Dim forced = live.Where(Function(r) r.Verdict.StartsWith("FORCED")).Count()
        If forced > 0 Then
            AppendLog(forced.ToString() & " panel(s) will be FORCED and sent short (shown in orange).")
        End If
        If done.Count > 0 Then
            AppendLog(done.Count.ToString() & " completed panel(s) kept below the pending ones.")
        End If
        AppendLog("Review the table, then use Upload per row or Upload ALL.")
    End Sub

    Private Sub OnGridSelection(sender As Object, e As SelectionChangedEventArgs)
        Dim r = TryCast(dg.SelectedItem, PanelRow)
        If Not busy Then btnUploadOne.IsEnabled = (r IsNot Nothing AndAlso r.CanUpload)
    End Sub

    ' Any option change alters what the table should say, so re-classify the panels
    ' already in memory. No disk re-scan of the queue is needed - only the
    ' interpretation changes - but switching Reconstruct ON does need a look at the
    ' source folders, so that part runs off the UI thread.
    Private Sub OnOptionToggled(sender As Object, e As RoutedEventArgs)
        If busy OrElse panels.Count = 0 Then Return

        Dim wantReconstruct = chkReconstruct.IsChecked.GetValueOrDefault()
        ApplySettings(False)                       ' pushes all four settings + log sink

        Dim needsWork = wantReconstruct AndAlso panels.Any(Function(p) Not p.ReconstructApplied)
        If Not needsWork Then
            AppendLog("")
            AppendLog(">>> Options changed - table re-checked.")
            Program.LogOptionNotes()
            FillGrid()
            Return
        End If

        SetBusy(True)
        AppendLog("")
        AppendLog(">>> 'Reconstruct from disk' switched on - looking for forgotten files ...")
        Task.Run(Sub()
                     Try
                         For Each p In panels
                             Program.EnsureReconstructed(p)
                         Next
                     Catch ex As Exception
                         AppendLog("Reconstruct failed: " & ex.Message)
                         WpfProgram.CrashLog("OptionToggle", ex.ToString())
                     End Try
                 End Sub).ContinueWith(
            Sub()
                Dispatcher.BeginInvoke(New Action(
                    Sub()
                        Try
                            FillGrid()
                        Finally
                            SetBusy(False)
                            SetStatus(panels.Count.ToString() & " panel(s)")
                        End Try
                    End Sub))
            End Sub)
    End Sub

    ' =====================================================================
    ' Upload
    ' =====================================================================

    ' The button lives in a cell template, so its DataContext is the row object.
    ' Using that instead of a row index keeps it correct after sorting.
    Private Sub OnRowUpload(sender As Object, e As RoutedEventArgs)
        If busy Then Return
        Dim b = TryCast(sender, Button)
        If b Is Nothing Then Return
        Dim r = TryCast(b.DataContext, PanelRow)
        If r Is Nothing Then Return
        If r.Idx < 0 OrElse r.Idx >= panels.Count Then Return
        UploadPanels(New List(Of Program.Panel) From {panels(r.Idx)}, False)
    End Sub

    Private Sub OnUploadOne(sender As Object, e As RoutedEventArgs)
        Dim r = TryCast(dg.SelectedItem, PanelRow)
        If r Is Nothing Then Return
        If r.Idx < 0 OrElse r.Idx >= panels.Count Then Return
        UploadPanels(New List(Of Program.Panel) From {panels(r.Idx)}, False)
    End Sub

    Private Sub OnUploadAll(sender As Object, e As RoutedEventArgs)
        If panels.Count = 0 Then Return
        UploadPanels(panels.ToList(), True)
    End Sub

    Private Sub OnStop(sender As Object, e As RoutedEventArgs)
        Program.CancelRequested = True
        ' Stop must break the repeat loop too, or the next pass would start moments
        ' after the user asked it to halt. Untick the box as well, so the window
        ' does not claim it is still repeating.
        CancelRepeat("stopped by the user")
        autoRepeatUploadAfterScan = False
        If chkAutoRepeat IsNot Nothing Then chkAutoRepeat.IsChecked = False
        SetStatus("stopping...")
        AppendLog("")
        AppendLog(">>> STOP requested - finishing the current file then halting.")
    End Sub

    Private Sub UploadPanels(targets As List(Of Program.Panel), isAll As Boolean)
        Dim what = If(isAll, targets.Count.ToString() & " panel(s)", "PID " & targets(0).PID)

        SetBusy(True)
        ApplySettings(True)
        Program.ResetRun()
        Program.OpenLogsPublic()
        AppendLog("")
        AppendLog(">>> Uploading " & what & "  (FTPUploaderVB must be stopped - both " &
                  "processes append to the same host file with no locking)")
        Program.LogOptionNotes()

        Task.Run(Sub()
                     Try
                         Dim n As Integer = 0
                         For Each p In targets
                             If Program.CancelRequested Then Exit For
                             Program.ProcessPanel(p)
                             n += 1
                             SetProgress(n, targets.Count)
                             SetStatus(n.ToString() & " / " & targets.Count.ToString())
                         Next
                     Catch ex As Exception
                         AppendLog("UPLOAD FAILED: " & ex.ToString())
                     Finally
                         Program.CloseSessionPublic()
                         AppendLog("")
                         AppendLog("================ SUMMARY ================")
                         AppendLog(Program.SummaryText())
                         Program.CloseLogsPublic()
                     End Try
                 End Sub).ContinueWith(
            Sub()
                Dispatcher.BeginInvoke(New Action(AddressOf RefreshAfterUpload))
            End Sub)
    End Sub

    ' Queue files have been consumed - rebuild the model so the dg is truthful.
    ' The scan touches thousands of files, so it must not run on the UI thread.
    Private Sub RefreshAfterUpload()
        SetStatus("re-scanning...")
        Program.LogSink = AddressOf AppendLog
        Program.DoExecute = False
        Dim found As New List(Of Program.Panel)()

        Task.Run(Sub()
                     Try
                         Dim entries = Program.ScanQueueFiles()
                         found = Program.BuildPanels(entries)
                     Catch ex As Exception
                         AppendLog("Re-scan failed: " & ex.Message)
                     End Try
                 End Sub).ContinueWith(
            Sub()
                Dispatcher.BeginInvoke(New Action(Sub()
                                                      panels = found
                                                      FillGrid()
                                                      Program.CancelRequested = False
                                                      SetBusy(False)
                                                      SetProgress(0, 1)
                                                      SetStatus(panels.Count.ToString() & " panel(s) remaining")
                                                      MaybeScheduleRepeat()
                                                  End Sub))
            End Sub)
    End Sub

    ' ---------------------------------------------------------------------
    ' Auto repeat
    '
    ' Scan + Upload ALL, over and over, until the user presses Stop. A round with
    ' nothing to upload is not a reason to give up - new panels arrive and an
    ' unreachable server comes back - so it waits and scans again instead.
    ' ---------------------------------------------------------------------
    Private Sub CancelRepeat(reason As String)
        If autoRepeatTimer IsNot Nothing Then
            autoRepeatTimer.Stop()
            autoRepeatTimer = Nothing
            If reason <> "" Then AppendLog(">>> auto repeat stopped - " & reason)
        End If
        autoRepeatRound = 0
        ' No pending repeat any more, so Stop only applies to a live run.
        If btnStop IsNot Nothing AndAlso Not busy Then btnStop.IsEnabled = False
    End Sub

    Private Sub MaybeScheduleRepeat()
        If chkAutoRepeat Is Nothing OrElse Not chkAutoRepeat.IsChecked.GetValueOrDefault() Then
            CancelRepeat("")
            Return
        End If
        If Program.CancelRequested Then
            CancelRepeat("stopped by the user")
            Return
        End If

        ' Keep going until the user presses Stop. Nothing to upload right now is not
        ' a reason to give up - new panels arrive, and a server that was unreachable
        ' comes back. It just means waiting for the next scan.
        Dim canDo = panels.Where(Function(p)
                                     Dim st = Program.Classify(p)
                                     Return st.CanComplete OrElse st.WillForce
                                 End Function).Count()

        Dim mins As Integer = 0
        Dim sel = Convert.ToString(cboRepeatEvery.SelectedItem)
        Integer.TryParse(sel.Replace(" min", ""), mins)      ' "at once" -> 0

        ' "at once" is for working through a backlog. With nothing to do it would
        ' re-scan every couple of seconds, and a scan reads every queue file in the
        ' folder - so idle rounds always wait at least a minute.
        Dim wait As TimeSpan
        If canDo > 0 Then
            wait = If(mins > 0, TimeSpan.FromMinutes(mins), TimeSpan.FromSeconds(2))
        Else
            wait = TimeSpan.FromMinutes(Math.Max(1, mins))
        End If

        autoRepeatRound += 1
        AppendLog("")
        If canDo > 0 Then
            AppendLog(">>> auto repeat: " & canDo.ToString() & " panel(s) uploadable - next pass in " &
                      FriendlyWait(wait) & "   (round " & autoRepeatRound.ToString() & ")")
        Else
            AppendLog(">>> auto repeat: nothing to upload right now - scanning again in " &
                      FriendlyWait(wait) & ".  Press Stop to end.")
        End If

        If autoRepeatTimer IsNot Nothing Then autoRepeatTimer.Stop()
        autoRepeatTimer = New System.Windows.Threading.DispatcherTimer()
        autoRepeatTimer.Interval = wait
        AddHandler autoRepeatTimer.Tick,
            Sub()
                autoRepeatTimer.Stop()
                autoRepeatTimer = Nothing
                If busy Then Return                       ' something else started
                If Not chkAutoRepeat.IsChecked.GetValueOrDefault() Then Return
                ' Always scan first - the folder may have gained panels since the
                ' last pass, or lost the reason a panel was blocked.
                autoRepeatUploadAfterScan = True
                OnScan(Nothing, Nothing)
            End Sub
        autoRepeatTimer.Start()
        ' SetBusy already ran before this timer existed, so enable Stop here.
        btnStop.IsEnabled = True
    End Sub

    Private Function FriendlyWait(t As TimeSpan) As String
        If t.TotalSeconds < 60 Then Return CInt(t.TotalSeconds).ToString() & "s"
        Return CInt(t.TotalMinutes).ToString() & " min"
    End Function

End Class
