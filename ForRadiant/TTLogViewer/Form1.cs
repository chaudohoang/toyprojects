using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace TTLogViewer
{
    public partial class Form1 : Form
    {
        private string filePath = "C:\\Radiant Vision Systems Data\\TrueTest\\AppData\\" + DateTime.Now.ToString("yyyyMMdd") + " Operation Log.txt";
        private Point lastMousePosition;
        private bool autoScrollEnabled = true;
        private FileSystemWatcher fileWatcher;

        public Form1()
        {
            InitializeComponent();
            InitializeForm();
            SetupFileWatcher();
            timerUpdate.Start();
        }

        private void InitializeForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;

            // Make the background transparent
            TransparencyKey = BackColor;
            BackColor = Color.LimeGreen; // Change to your desired color

            // Attach event handlers
            Load += Form1_Load;
            MouseDown += Form1_MouseDown;
            MouseMove += Form1_MouseMove;
            rtbContent.VScroll += rtbContent_VScroll;
            Paint += Form1_Paint;
        }

        private void SetupFileWatcher()
        {
            fileWatcher = new FileSystemWatcher(Path.GetDirectoryName(filePath), Path.GetFileName(filePath));
            fileWatcher.Changed += FileWatcher_Changed;
            fileWatcher.EnableRaisingEvents = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadFileContent();
        }

        private void LoadFileContent()
        {
            try
            {
                // Try to open and read the file with a retry mechanism
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                using (StreamReader streamReader = new StreamReader(fileStream))
                                {
                                    string fileContent = streamReader.ReadToEnd();
                                    rtbContent.Text = fileContent;
                                    rtbContent.SelectionStart = rtbContent.Text.Length;
                                    rtbContent.ScrollToCaret();
                                    return; // Exit the loop if successful
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("File not found.");
                            return; // Exit the loop if file not found
                        }
                    }
                    catch (IOException)
                    {
                        // Retry after a short delay
                        System.Threading.Thread.Sleep(100);
                    }
                }

                // If the loop completes without success, show an error message
                MessageBox.Show("Error loading file after multiple attempts.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}");
            }
        }

        private bool scrollToEndRequested = false;
        private bool userScrolled = false;

        private void UpdateFileContent()
        {
            try
            {
                if (autoScrollEnabled)
                {
                    // Try to open and read the file with a retry mechanism
                    for (int i = 0; i < 3; i++)
                    {
                        try
                        {
                            if (File.Exists(filePath))
                            {
                                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                                {
                                    using (StreamReader streamReader = new StreamReader(fileStream))
                                    {
                                        int currentLength = rtbContent.Text.Length;
                                        int linesToRead = 100; // Adjust the number of lines to read initially
                                        int linesRead = 0;

                                        // Read the file line by line
                                        while (linesRead < linesToRead && !streamReader.EndOfStream)
                                        {
                                            string newLine = streamReader.ReadLine();

                                            if (!string.IsNullOrEmpty(newLine))
                                            {
                                                // Append only the new lines
                                                if (fileStream.Position > currentLength)
                                                {
                                                    rtbContent.AppendText(newLine + Environment.NewLine);
                                                    linesRead++;
                                                }
                                            }
                                        }

                                        // If it's the first time loading or user manually scrolled, scroll to the end
                                        if (currentLength == 0 || userScrolled)
                                        {
                                            rtbContent.SelectionStart = rtbContent.Text.Length;
                                            rtbContent.ScrollToCaret();
                                            userScrolled = false; // Reset the flag after auto-scrolling
                                        }

                                        // Check if the user manually clicked "Scroll to End" button
                                        if (scrollToEndRequested)
                                        {
                                            rtbContent.SelectionStart = rtbContent.Text.Length;
                                            rtbContent.ScrollToCaret();
                                            scrollToEndRequested = false;
                                        }

                                        return; // Exit the loop if successful
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("File not found.");
                                return; // Exit the loop if file not found
                            }
                        }
                        catch (IOException)
                        {
                            // Retry after a short delay
                            System.Threading.Thread.Sleep(100);
                        }
                    }

                    // If the loop completes without success, show an error message
                    MessageBox.Show("Error updating file content after multiple attempts.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating file content: {ex.Message}");
            }
        }

        private void rtbContent_TextChanged(object sender, EventArgs e)
        {
            // Check if the user manually scrolled
            userScrolled = rtbContent.SelectionStart != rtbContent.Text.Length;
        }

        private void btnLive_Click(object sender, EventArgs e)
        {
            // Set the flag to indicate the user requested to scroll to the end
            scrollToEndRequested = true;
        }


        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            // Invoke the update on the UI thread
            Invoke(new Action(() => UpdateFileContent()));
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // Capture the current mouse position when the user clicks on the form
            lastMousePosition = new Point(e.X, e.Y);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            // Move the form based on the difference in mouse position
            if (e.Button == MouseButtons.Left)
            {
                int deltaX = e.X - lastMousePosition.X;
                int deltaY = e.Y - lastMousePosition.Y;
                Location = new Point(Location.X + deltaX, Location.Y + deltaY);
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // Draw a green border around the form
            e.Graphics.DrawRectangle(new Pen(Color.Green, 2), 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }

        private void rtbContent_VScroll(object sender, EventArgs e)
        {
            // Check if the user is at the bottom of the RichTextBox
            autoScrollEnabled = rtbContent.GetPositionFromCharIndex(rtbContent.TextLength - 1).Y <= rtbContent.Height;
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                // Invoke the update on the UI thread
                Invoke(new Action(() => UpdateFileContent()));
            }
        }

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int LWA_ALPHA = 0x2;
    }
}
