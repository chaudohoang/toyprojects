using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Effects;

namespace QuickLauncher
{
    // A one-line text prompt in the panel's style. Built in code: it is just a box and two buttons.
    static class InputWindow
    {
        public static string Ask(Window owner, string title, string prompt, string value)
        {
            var box = new TextBox { Text = value, Style = (Style)Application.Current.FindResource("Field"), Margin = new Thickness(0, 8, 0, 12) };
            var ok = new Button { Content = "OK", IsDefault = true, MinWidth = 80, Style = (Style)Application.Current.FindResource("PrimaryButton") };
            var cancel = new Button { Content = "Cancel", IsCancel = true, MinWidth = 80, Margin = new Thickness(8, 0, 0, 0) };
            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttons.Children.Add(ok);
            buttons.Children.Add(cancel);

            var body = new StackPanel { Margin = new Thickness(16, 12, 16, 14) };
            body.Children.Add(new TextBlock { Text = title, FontSize = 14, FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 6) });
            body.Children.Add(new TextBlock { Text = prompt, TextWrapping = TextWrapping.Wrap, Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("Dim") });
            body.Children.Add(box);
            body.Children.Add(buttons);

            var w = new Window
            {
                Owner = owner, Width = 440, SizeToContent = SizeToContent.Height,
                WindowStyle = WindowStyle.None, AllowsTransparency = true, Background = System.Windows.Media.Brushes.Transparent,
                ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.CenterOwner,
                FontFamily = new System.Windows.Media.FontFamily("Segoe UI"), FontSize = 12.5,
                Foreground = (System.Windows.Media.Brush)Application.Current.FindResource("Fg"),
                Content = new Border
                {
                    CornerRadius = new CornerRadius(10), Margin = new Thickness(10), BorderThickness = new Thickness(1),
                    Background = (System.Windows.Media.Brush)Application.Current.FindResource("Bg"),
                    BorderBrush = (System.Windows.Media.Brush)Application.Current.FindResource("Line"),
                    Effect = new DropShadowEffect { BlurRadius = 16, ShadowDepth = 2, Opacity = 0.5 },
                    Child = body,
                },
            };
            w.MouseLeftButtonDown += (s, e) => { if (e.ButtonState == MouseButtonState.Pressed) w.DragMove(); };
            ok.Click += (s, e) => w.DialogResult = true;
            w.Loaded += (s, e) => { box.Focus(); box.SelectAll(); };
            return w.ShowDialog() == true ? box.Text : null;
        }
    }
}
