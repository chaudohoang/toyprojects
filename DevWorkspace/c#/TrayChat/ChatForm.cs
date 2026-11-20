using System.Runtime.InteropServices;

namespace TrayChat;

sealed class ChatForm : Form
{
    readonly Messenger messenger;
    readonly Peer peer;
    readonly RichTextBox history = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, BorderStyle = BorderStyle.None,
        BackColor = SystemColors.Window, Font = new Font("Segoe UI", 10f),
    };
    readonly TextBox input = new() { Dock = DockStyle.Fill, Multiline = true, Font = new Font("Segoe UI", 10f) };
    readonly Button sendButton = new() { Dock = DockStyle.Right, Width = 80, Text = "Send" };

    public ChatForm(Messenger messenger, Peer peer)
    {
        this.messenger = messenger;
        this.peer = peer;

        Text = $"Chat - {peer.Name}";
        Icon = TrayApp.AppIcon;
        Size = new Size(440, 520);
        MinimumSize = new Size(300, 300);
        StartPosition = FormStartPosition.CenterScreen;

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 72, Padding = new Padding(6) };
        bottom.Controls.Add(input);
        bottom.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 6 });
        bottom.Controls.Add(sendButton);
        var historyHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8) , BackColor = SystemColors.Window };
        historyHost.Controls.Add(history);
        Controls.Add(historyHost);
        Controls.Add(bottom);

        sendButton.Click += (_, _) => SendCurrent();
        input.KeyDown += (_, e) =>
        {
            // Enter sends, Shift+Enter inserts a newline.
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                SendCurrent();
            }
        };
        Shown += (_, _) => input.Focus();
    }

    public void Receive(string text)
    {
        Append(peer.Name, text, Color.SeaGreen);
        if (!ContainsFocus) Flash();
    }

    async void SendCurrent()
    {
        var text = input.Text.Trim();
        if (text.Length == 0) return;
        input.Clear();
        Append("Me", text, Color.SteelBlue);
        try
        {
            await messenger.SendAsync(messenger.Find(peer.Id) ?? peer, text);
        }
        catch (Exception ex)
        {
            Append("Not delivered", ex is TimeoutException ? "Peer did not respond." : ex.Message, Color.Firebrick);
        }
    }

    void Append(string who, string text, Color color)
    {
        history.SelectionStart = history.TextLength;
        history.SelectionLength = 0;
        history.SelectionColor = color;
        history.SelectionFont = new Font(history.Font, FontStyle.Bold);
        history.AppendText($"{who}  {DateTime.Now:HH:mm}\n");
        history.SelectionColor = history.ForeColor;
        history.SelectionFont = history.Font;
        history.AppendText(text.ReplaceLineEndings("\n") + "\n\n");
        history.ScrollToCaret();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Keep the conversation around; the tray menu re-opens the same window.
        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            Hide();
        }
        base.OnFormClosing(e);
    }

    void Flash()
    {
        var info = new FLASHWINFO
        {
            cbSize = (uint)Marshal.SizeOf<FLASHWINFO>(),
            hwnd = Handle,
            dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
        };
        FlashWindowEx(ref info);
    }

    const uint FLASHW_ALL = 3, FLASHW_TIMERNOFG = 12;

    [StructLayout(LayoutKind.Sequential)]
    struct FLASHWINFO
    {
        public uint cbSize;
        public IntPtr hwnd;
        public uint dwFlags;
        public uint uCount;
        public uint dwTimeout;
    }

    [DllImport("user32.dll")]
    static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
}
