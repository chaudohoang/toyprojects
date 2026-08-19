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
        ' window sitting in a busy state with no explanation.
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnDomainCrash
        Dim app As New Application()
        AddHandler app.DispatcherUnhandledException, AddressOf OnDispatcherCrash
        app.ShutdownMode = ShutdownMode.OnMainWindowClose
        Dim w As New RecoveryWindow()
        app.Run(w)
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
    Private chkForce As CheckBox
    Private chkSkipMissing As CheckBox
    Private chkReconstruct As CheckBox
    Private numRetry As ComboBox
    Private dg As DataGrid
    Private txtLog As TextBox
    Private bar As ProgressBar
    Private lblStatus As TextBlock

    Private panels As New List(Of Program.Panel)()
    Private rows As New ObservableCollection(Of PanelRow)()
    Private busy As Boolean = False

    Public Sub New()
        Title = "FTP Recovery - stalled queue repair"
        Width = 1180
        Height = 800
        MinWidth = 900
        MinHeight = 560
        WindowStartupLocation = WindowStartupLocation.CenterScreen
        WindowState = WindowState.Maximized
        Content = BuildUi()
        txtRoot.Text = Program.DefaultQueueRoot()
        StartLogTimer()
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
        btnScan = MakeButton("Start Scan", AddressOf OnScan, True)
        btnUploadOne = MakeButton("Upload this PID", AddressOf OnUploadOne, False)
        btnUploadAll = MakeButton("Upload ALL panels", AddressOf OnUploadAll, False)
        btnStop = MakeButton("Stop", AddressOf OnStop, False)
        buttons.Children.Add(btnScan)
        buttons.Children.Add(btnUploadOne)
        buttons.Children.Add(btnUploadAll)
        buttons.Children.Add(btnStop)
        Grid.SetColumn(buttons, 0)

        Dim opts As New WrapPanel() With {.HorizontalAlignment = HorizontalAlignment.Right}
        chkReconstruct = MakeCheck("Reconstruct from disk")
        chkForce = MakeCheck("Force incomplete")
        chkSkipMissing = MakeCheck("Skip missing source")

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

        opts.Children.Add(chkReconstruct)
        opts.Children.Add(chkForce)
        opts.Children.Add(chkSkipMissing)
        opts.Children.Add(lblRetry)
        opts.Children.Add(numRetry)
        Grid.SetColumn(opts, 1)

        outer.Children.Add(buttons)
        outer.Children.Add(opts)
        Grid.SetRow(outer, 1)
        Return outer
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
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = New GridLength(1, GridUnitType.Star)})
        g.ColumnDefinitions.Add(New ColumnDefinition() With {.Width = GridLength.Auto})

        bar = New ProgressBar() With {.Height = 18, .Minimum = 0, .Maximum = 1, .Value = 0}
        Grid.SetColumn(bar, 0)

        lblStatus = New TextBlock() With {
            .Text = "Idle",
            .VerticalAlignment = VerticalAlignment.Center,
            .Margin = New Thickness(10, 0, 0, 0),
            .MinWidth = 160,
            .TextTrimming = TextTrimming.CharacterEllipsis}
        Grid.SetColumn(lblStatus, 1)

        g.Children.Add(bar)
        g.Children.Add(lblStatus)
        Grid.SetRow(g, 3)
        Return g
    End Function

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

    Private Sub FlushLog(sender As Object, e As EventArgs)
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
            txtLog.AppendText(chunk)
            txtLog.ScrollToEnd()
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
        btnStop.IsEnabled = state
        ' Options are read once at the start of a run; letting them change mid-run
        ' would mean half the panels used different rules.
        chkReconstruct.IsEnabled = Not state
        chkForce.IsEnabled = Not state
        chkSkipMissing.IsEnabled = Not state
        numRetry.IsEnabled = Not state
        ' Deliberately NOT setting a wait cursor. The window stays fully
        ' responsive during a run, and a spinning cursor reads as "frozen" -
        ' the progress bar, status text and streaming log are the honest signal.
    End Sub

    Private Function HasUploadableSelection() As Boolean
        Dim r = TryCast(dg.SelectedItem, PanelRow)
        Return r IsNot Nothing AndAlso r.CanUpload
    End Function

    Private Sub ApplySettings(execute As Boolean)
        Program.QueueRoot = txtRoot.Text.TrimEnd("\"c)
        Program.DoExecute = execute
        Program.ForceIncomplete = chkForce.IsChecked.GetValueOrDefault()
        Program.SkipMissingSource = chkSkipMissing.IsChecked.GetValueOrDefault()
        Program.Reconstruct = chkReconstruct.IsChecked.GetValueOrDefault()
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
        If Not Directory.Exists(txtRoot.Text) Then
            MessageBox.Show(Me, "Queue folder not found:" & vbCrLf & txtRoot.Text,
                            "Scan", MessageBoxButton.OK, MessageBoxImage.Warning)
            Return
        End If

        SetBusy(True)
        rows.Clear()
        txtLog.Clear()
        ' An explicit Scan starts a fresh session - drop the completed-panel history.
        Program.ClearOutcomes()
        ApplySettings(False)
        Program.ResetRun()

        Dim found As New List(Of Program.Panel)()

        Task.Run(Sub()
                     Try
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
                        End Try
                    End Sub))
            End Sub)
    End Sub

    Private Sub FillGrid()
        rows.Clear()
        Dim ready As Integer = 0
        Dim pink = New SolidColorBrush(Color.FromRgb(255, 235, 235))
        Dim blue = New SolidColorBrush(Color.FromRgb(233, 243, 255))

        Dim green = New SolidColorBrush(Color.FromRgb(232, 245, 233))
        Dim amber = New SolidColorBrush(Color.FromRgb(255, 244, 224))

        Dim live As New List(Of PanelRow)()
        Dim liveKeys As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For i = 0 To panels.Count - 1
            Dim p = panels(i)
            liveKeys.Add(p.Key)
            Dim st = Program.Classify(p)
            Dim r As New PanelRow() With {
                .Idx = i,
                .PID = p.PID,
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
                r.RowBrush = pink
            Else
                ready += 1
                If st.NewCount = 0 AndAlso st.RetryCount = 0 Then r.RowBrush = blue
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

        For Each r In live.OrderBy(Function(x) x.PID)
            rows.Add(r)
        Next
        For Each r In done.OrderBy(Function(x) x.PID)
            rows.Add(r)
        Next

        AppendLog("")
        AppendLog(ready.ToString() & " of " & panels.Count.ToString() & " pending panel(s) can complete.")
        If done.Count > 0 Then
            AppendLog(done.Count.ToString() & " completed panel(s) kept below the pending ones.")
        End If
        AppendLog("Review the table, then use Upload per row or Upload ALL.")
    End Sub

    Private Sub OnGridSelection(sender As Object, e As SelectionChangedEventArgs)
        Dim r = TryCast(dg.SelectedItem, PanelRow)
        If Not busy Then btnUploadOne.IsEnabled = (r IsNot Nothing AndAlso r.CanUpload)
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
                                                  End Sub))
            End Sub)
    End Sub

End Class
