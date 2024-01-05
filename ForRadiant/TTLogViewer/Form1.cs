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

            // Attach event handlers
            Load += Form1_Load;
            MouseDown += Form1_MouseDown;
            MouseMove += Form1_MouseMove;
            rtbContent.VScroll += rtbContent_VScroll;
            btnToggleAutoScroll.Click += btnToggleAutoScroll_Click;

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

        private bool isSnapped = false;
        private Size originalSize;

        private void btnResizeAndSnap_Click(object sender, EventArgs e)
        {
            if (isSnapped)
            {
                // Restore the original size and location
                Size = originalSize;

                // Update the button text
                btnResizeAndSnap.Text = "Snap";
            }
            else
            {
                // Save the original size and location
                originalSize = Size;

                // Set the new size for the form (adjust width and height as needed)
                Size = new Size(150, 75);

                // Update the button text
                btnResizeAndSnap.Text = "Restore";
            }

            // Toggle the snapped state
            isSnapped = !isSnapped;
        }

        private void LoadFileContent()
        {
            try
            {
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
                                    return;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("File not found.");
                            return;
                        }
                    }
                    catch (IOException)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }

                MessageBox.Show("Error loading file after multiple attempts.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading file: {ex.Message}");
            }
        }

        private void UpdateFileContent()
        {
            try
            {
                if (autoScrollEnabled)
                {
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

                                        while (!streamReader.EndOfStream)
                                        {
                                            string newLine = streamReader.ReadLine();

                                            if (!string.IsNullOrEmpty(newLine))
                                            {
                                                if (fileStream.Position > currentLength)
                                                {
                                                    rtbContent.AppendText(newLine + Environment.NewLine);
                                                    rtbContent.ScrollToCaret();
                                                }
                                            }
                                        }

                                        return;
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show("File not found.");
                                return;
                            }
                        }
                        catch (IOException)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }

                    MessageBox.Show("Error updating file content after multiple attempts.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating file content: {ex.Message}");
            }
        }

        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            Invoke(new Action(() => UpdateFileContent()));
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            lastMousePosition = new Point(e.X, e.Y);
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int deltaX = e.X - lastMousePosition.X;
                int deltaY = e.Y - lastMousePosition.Y;
                Location = new Point(Location.X + deltaX, Location.Y + deltaY);
            }
        }


        private void rtbContent_VScroll(object sender, EventArgs e)
        {
            // Check if the user is at the bottom of the RichTextBox
            autoScrollEnabled = rtbContent.GetPositionFromCharIndex(rtbContent.TextLength - 1).Y <= rtbContent.Height;

            // Update the auto-scroll button state based on autoScrollEnabled
            btnToggleAutoScroll.Text = autoScrollEnabled ? "Auto Scroll: ON" : "Auto Scroll: OFF";
        }

        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Changed || e.ChangeType == WatcherChangeTypes.Created)
            {
                Invoke(new Action(() => UpdateFileContent()));
            }
        }

        private void btnToggleAutoScroll_Click(object sender, EventArgs e)
        {
            autoScrollEnabled = !autoScrollEnabled;
            btnToggleAutoScroll.Text = autoScrollEnabled ? "Auto Scroll: ON" : "Auto Scroll: OFF";
        }
    }
}
