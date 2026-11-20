using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KitHelper
{
    // A question in Kit Helper's own style, replacing MessageBox. Buttons are listed left to
    // right; the first is the primary (Enter) one. Ask returns the clicked button's index, or -1
    // when the window is closed (✕ / Esc).
    public partial class AskWindow : Window
    {
        public int Choice { get; private set; } = -1;

        AskWindow(string title, string message, string detail, string[] buttons)
        {
            InitializeComponent();
            KeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };
            TxtTitle.Text = title;
            CopyResultWindow.SetMood(this, "Warn", "QUESTION");
            TxtMessage.Text = message;
            TxtDetail.Text = detail ?? "";
            TxtDetail.Visibility = string.IsNullOrEmpty(detail) ? Visibility.Collapsed : Visibility.Visible;
            for (int i = 0; i < buttons.Length; i++)
            {
                int index = i;
                var b = new Button { Content = buttons[i], MinWidth = 90 };
                if (i == 0) { b.Style = (Style)FindResource("PrimaryButton"); b.IsDefault = true; }
                b.Click += (s, e) => { Choice = index; DialogResult = true; };
                // Primary on the right, as in the copy windows.
                Buttons.Children.Insert(0, b);
            }
        }

        public static int Ask(Window owner, string title, string message, string detail, params string[] buttons)
        {
            var w = new AskWindow(title, message, detail, buttons) { Owner = owner };
            w.ShowDialog();
            return w.Choice;
        }

        void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
