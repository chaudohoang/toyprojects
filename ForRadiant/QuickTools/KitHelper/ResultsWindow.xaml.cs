using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace KitHelper
{
    // Copy results, one tab each. Unlike the confirmation (a modal dialog: nothing else may happen
    // until OK / Cancel), a result is only something to read, so it must not hold up the next
    // copy: this window is not modal, never takes the focus when a result arrives, and a second
    // result becomes another tab instead of another window. Closing the last tab closes it.
    // It belongs to the panel (Owner): centred on it, shown and hidden with it (MainWindow.ShowNear
    // / HidePanel), no taskbar button of its own - the K icon or the tray brings both back. It covers
    // the copy buttons, so opening a folder pops up the confirmation of its suggested copy at once
    // (MainWindow.OfferSuggestedCopy). Docking it beside the panel (1.10.8) was tried and rejected.
    public partial class ResultsWindow : Window
    {
        public static ResultsWindow Current { get; private set; }

        sealed class Tab
        {
            public Border Header;
            public UIElement Body;
            public CopyResultWindow Source;      // never shown; it built Body and handles its buttons
            public Brush Mood;
        }

        readonly List<Tab> tabs = new List<Tab>();
        Tab selected;

        ResultsWindow()
        {
            InitializeComponent();
            KeyDown += (s, e) => { if (e.Key == Key.Escape && selected != null) CloseTab(selected); };
            Closed += (s, e) => { if (Current == this) Current = null; foreach (var t in tabs) t.Source.Close(); };
            // Clicking away from the results hides the pair, as clicking away from the panel does.
            Deactivated += (s, e) => (Owner as MainWindow)?.AutoHide();
            // Grows with its content (SizeToContent): keep it inside the screen it is on.
            SizeChanged += (s, e) => KeepOnScreen();
        }

        // With the panel: docked to it (it may have moved since).
        public void ShowWithPanel(MainWindow panel)
        {
            if (!IsVisible) Show();
            UpdateLayout();                      // real size first, then centre
            CenterOn(panel);
            KeepOnScreen();
        }

        void CenterOn(Window panel)
        {
            Left = panel.Left + (panel.ActualWidth - ActualWidth) / 2;
            Top = panel.Top + Math.Max(0, (panel.ActualHeight - ActualHeight) / 2);
        }

        void KeepOnScreen()
        {
            var h = new WindowInteropHelper(this).Handle;
            var src = PresentationSource.FromVisual(this);
            if (h == IntPtr.Zero || src?.CompositionTarget == null) return;
            var px = System.Windows.Forms.Screen.FromHandle(h).WorkingArea;
            var toDip = src.CompositionTarget.TransformFromDevice;
            var tl = toDip.Transform(new Point(px.Left, px.Top));
            var br = toDip.Transform(new Point(px.Right, px.Bottom));
            if (Top + ActualHeight > br.Y) Top = Math.Max(tl.Y, br.Y - ActualHeight);
            if (Left + ActualWidth > br.X) Left = Math.Max(tl.X, br.X - ActualWidth);
            if (Top < tl.Y) Top = tl.Y;
            if (Left < tl.X) Left = tl.X;
        }

        // r: a result-mode CopyResultWindow (not shown). panel: its owner; shown too if it was hidden
        // meanwhile, since the two always appear together.
        public static void Add(MainWindow panel, CopyResultWindow r, string title, bool failed)
        {
            bool first = Current == null;
            if (Current == null) Current = new ResultsWindow { Owner = panel };
            Current.AddTab(r, title, failed);
            if (!panel.IsVisible) panel.ShowNear(null);      // shows the results with it
            else if (first || !Current.IsVisible) Current.ShowWithPanel(panel);   // ShowActivated=False: the panel keeps the focus
        }

        void AddTab(CopyResultWindow r, string title, bool failed)
        {
            var tab = new Tab { Source = r, Mood = (Brush)FindResource(failed ? "Err" : "Ok") };
            tab.Body = r.TakeContent(() => CloseTab(tab));

            // Fixed width, like a browser tab: the title is cut, the full one is in the tooltip.
            var dot = new Ellipse { Width = 8, Height = 8, Fill = tab.Mood, Margin = new Thickness(0, 0, 6, 0), VerticalAlignment = VerticalAlignment.Center };
            var text = new TextBlock { Text = ShortTitle(title), TextTrimming = TextTrimming.CharacterEllipsis, VerticalAlignment = VerticalAlignment.Center };
            var close = new Button { Content = "✕", Style = (Style)FindResource("ChromeButton"), Margin = new Thickness(4, 0, 0, 0),
                                     Padding = new Thickness(4, 0, 4, 0), FontSize = 10, ToolTip = "Close this result" };
            close.Click += (s, e) => { CloseTab(tab); e.Handled = true; };
            var row = new DockPanel();
            DockPanel.SetDock(dot, Dock.Left);
            DockPanel.SetDock(close, Dock.Right);
            row.Children.Add(dot); row.Children.Add(close); row.Children.Add(text);
            tab.Header = new Border
            {
                Child = row, Width = TabWidth, CornerRadius = new CornerRadius(6), Padding = new Thickness(8, 3, 2, 3), Margin = new Thickness(0, 0, 4, 6),
                BorderThickness = new Thickness(1), Cursor = Cursors.Hand, ToolTip = $"{title}  ({DateTime.Now:HH:mm:ss})",
            };
            tab.Header.MouseLeftButtonUp += (s, e) => Select(tab);
            tabs.Add(tab);
            Tabs.Children.Add(tab.Header);
            Select(tab);
        }

        void Select(Tab tab)
        {
            selected = tab;
            Body.Content = tab.Body;
            foreach (var t in tabs)
            {
                bool on = t == tab;
                t.Header.Background = on ? (Brush)FindResource("Line") : Brushes.Transparent;
                t.Header.BorderBrush = on ? t.Mood : (Brush)FindResource("Line");
                ((TextBlock)((DockPanel)t.Header.Child).Children[2]).FontWeight = on ? FontWeights.SemiBold : FontWeights.Normal;
            }
            // Scroll the row so the selected tab is in view (after layout: a new tab has no position yet).
            Dispatcher.BeginInvoke(new Action(() => tab.Header.BringIntoView()), System.Windows.Threading.DispatcherPriority.Loaded);
            int failed = tabs.Count(t => t.Mood == FindResource("Err"));
            TxtCount.Text = tabs.Count == 1 ? "" : $"{tabs.Count} results" + (failed > 0 ? $", {failed} with errors" : "");
        }

        void CloseTab(Tab tab)
        {
            int i = tabs.IndexOf(tab);
            if (i < 0) return;
            tabs.RemoveAt(i);
            Tabs.Children.Remove(tab.Header);
            tab.Source.Close();
            if (tabs.Count == 0) { Close(); return; }
            if (selected == tab) Select(tabs[Math.Min(i, tabs.Count - 1)]);
            else Select(selected);
        }

        const double TabWidth = 150;

        // "Copy all → POCB4.2" → "POCB4.2"; "Copy package X (3 folders)" → "Package X (3 folders)".
        static string ShortTitle(string title) =>
            title.StartsWith("Copy all → ") ? title.Substring("Copy all → ".Length)
            : title.StartsWith("Copy package ") ? "Package " + title.Substring("Copy package ".Length)
            : title.StartsWith("Copy to ") ? title.Substring("Copy to ".Length)
            : title.StartsWith("Copy kits from ") ? "Kits from " + title.Substring("Copy kits from ".Length)
            : title;

        void ScrollLeft_Click(object sender, RoutedEventArgs e) => TabScroller.ScrollToHorizontalOffset(TabScroller.HorizontalOffset - TabWidth * 2);
        void ScrollRight_Click(object sender, RoutedEventArgs e) => TabScroller.ScrollToHorizontalOffset(TabScroller.HorizontalOffset + TabWidth * 2);

        void TabScroller_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            TabScroller.ScrollToHorizontalOffset(TabScroller.HorizontalOffset - e.Delta / 2.0);
            e.Handled = true;
        }

        // The arrows show only when some tabs are out of view, each only when it can move.
        void TabScroller_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            bool overflow = TabScroller.ExtentWidth > TabScroller.ViewportWidth + 1;
            BtnLeft.Visibility = BtnRight.Visibility = overflow ? Visibility.Visible : Visibility.Collapsed;
            BtnLeft.IsEnabled = TabScroller.HorizontalOffset > 0.5;
            BtnRight.IsEnabled = TabScroller.HorizontalOffset < TabScroller.ScrollableWidth - 0.5;
        }

        void CloseAll_Click(object sender, RoutedEventArgs e) => Close();
        void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    }
}
