using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;

namespace AutoClickOnLog
{
    public partial class Form1 : Form
    {
        private string logFolder; // Update this with the path to your log folder
        private string today;
        private string[] logFileNames; // Array to store target log file names
        private bool[] initialDetectionDone; // Flag to indicate whether initial detection has been done for each log file
        private Thread[] monitoringThreads; // Array to store monitoring threads
        private bool waitingForOccurrence = false; // Flag to indicate if waiting for occurrence
        private DateTime lastOccurrenceTime; // Time of the last occurrence
        private string logFilePath; // Path to log file

        public Form1()
        {
            InitializeComponent();
            logFolder = LogFolderTextBox.Text;
            UpdateToday(); // Initialize today variable
            initialDetectionDone = new bool[4];
            logFilePath = Path.Combine(Application.StartupPath, "log.txt");
        }
        private void UpdateToday()
        {
            today = DateTime.Now.ToString("yyyyMMdd");
        }


        private volatile bool stopRequested = false;

        private void StartButton_Click(object sender, EventArgs e)
        {
            // Update UI
            lblStatus.Text = "Status: Monitoring";
            StartButton.Enabled = false;
            StopButton.Enabled = true;

            LogFolderTextBox.Enabled = false;
            LogStringTextBox.Enabled = false;
            ActionDelayTextBox.Enabled = false;

            // Set target log file names
            logFileNames = GetLogFileNames(logFolder);

            // Reset initial detection flags
            Array.Clear(initialDetectionDone, 0, initialDetectionDone.Length);

            // Start monitoring threads for each log file
            monitoringThreads = new Thread[logFileNames.Length]; // Create a new array for threads
            stopRequested = false; // Reset stopRequested flag
            for (int i = 0; i < logFileNames.Length; i++)
            {
                int index = i; // Capture index variable for thread lambda
                Thread monitoringThread = new Thread(() => MonitorLogFile(index));
                monitoringThread.Start();
                monitoringThreads[index] = monitoringThread; // Store the reference to the thread
            }
        }


        private void StopButton_Click(object sender, EventArgs e)
        {
            // Update UI
            lblStatus.Text = "Status: Stopped...";
            StartButton.Enabled = true;
            StopButton.Enabled = false;

            LogFolderTextBox.Enabled = true;
            LogStringTextBox.Enabled = true;
            ActionDelayTextBox.Enabled = true;

            // Request stop for monitoring threads
            stopRequested = true;

            // Wait for all monitoring threads to finish
            foreach (var thread in monitoringThreads)
            {
                thread.Join(); // Wait for the thread to finish
            }

            // Reset monitoring threads array
            monitoringThreads = new Thread[logFileNames.Length];
        }

        private string[] GetLogFileNames(string folder)
        {
            string[] allFiles = Directory.GetFiles(folder, $"{today}_*.log");
            List<string> targetFiles = new List<string>();

            foreach (string file in allFiles)
            {
                string fileName = Path.GetFileName(file);
                if (fileName.EndsWith("Mlog_Ch1.log") ||
                    fileName.EndsWith("Mlog_Ch2.log") ||
                    fileName.EndsWith("Mlog_Ch3.log") ||
                    fileName.EndsWith("Mlog_Ch4.log"))
                {
                    targetFiles.Add(fileName);
                }
            }

            return targetFiles.ToArray();
        }

        private void MonitorLogFile(int index)
        {
            // Check if it's past midnight, update the today variable if needed
            if (DateTime.Now.Date != DateTime.ParseExact(today, "yyyyMMdd", CultureInfo.InvariantCulture))
            {
                UpdateToday();
            }


            string logFileName = logFileNames[index];
            string logFilePath = Path.Combine(logFolder, logFileName);

            if (!File.Exists(logFilePath))
            {
                File.AppendAllText(logFilePath, "");
            }

            long lastReadLength = new FileInfo(logFilePath).Length - 1024;
            if (lastReadLength < 0) lastReadLength = 0;

            while (!stopRequested)
            {
                try
                {
                    long fileSize = new FileInfo(logFilePath).Length;

                    if (fileSize > lastReadLength)
                    {
                        using (FileStream fs = new FileStream(logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            fs.Seek(lastReadLength, SeekOrigin.Begin);
                            byte[] buffer = new byte[1024];

                            while (true)
                            {
                                int bytesRead = fs.Read(buffer, 0, buffer.Length);
                                lastReadLength += bytesRead;
                                if (bytesRead == 0) break;
                                string text = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                                if (!initialDetectionDone[index])
                                {
                                    // Skip initial detection
                                    initialDetectionDone[index] = true;
                                    continue;
                                }

                                if (text.Contains(LogStringTextBox.Text) && !waitingForOccurrence)
                                {
                                    waitingForOccurrence = true;
                                    lastOccurrenceTime = DateTime.Now;

                                    int waitTime;
                                    if (!int.TryParse(ActionDelayTextBox.Text, out waitTime))
                                    {
                                        waitTime = 1;
                                    }
                                    Thread.Sleep(waitTime * 1000);

                                    string channelName = logFileName.EndsWith("Mlog_Ch1.log") || logFileName.EndsWith("Mlog_Ch2.log") ? "Ch1 or Ch2" : "Ch3 or Ch4";
                                    string oppositeChannelName = logFileName.EndsWith("Mlog_Ch1.log") || logFileName.EndsWith("Mlog_Ch2.log") ? "Ch3 or Ch4" : "Ch1 or Ch2";
                                    string processName = logFileName.EndsWith("Mlog_Ch1.log") || logFileName.EndsWith("Mlog_Ch2.log") ? "RunCh3Ch4.exe" : "RunCh1Ch2.exe";
                                    string appPath = Path.Combine(Application.StartupPath, processName);

                                    LogToFile($"Starting {processName} based on occurrence in {channelName}.");
                                    Invoke((MethodInvoker)delegate
                                    {
                                        lblLastAction.Text = $"Last action : {DateTime.Now}: Finished {channelName}, Start {oppositeChannelName}.\"";
                                    });


                                    try
                                    {
                                        Process.Start(appPath);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogToFile($"Error starting {processName}: {ex.Message}");
                                    }

                                    waitingForOccurrence = false;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogToFile($"Error monitoring log file: {ex.Message}");
                }

                Thread.Sleep(1000);
            }
        }

        private void LogToFile(string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing to log file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearlogButton_Click(object sender, EventArgs e)
        {
            try
            {
                File.WriteAllText(logFilePath, "");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error clearing to log file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ViewlogButton_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(logFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Check if monitoringThreads is not null
            //
            if (monitoringThreads != null)
            {
                // Set stopRequested flag to true to request stop monitoring
                stopRequested = true;

                // Wait for all monitoring threads to finish, but with a timeout
                DateTime endTime = DateTime.Now.AddSeconds(10); // Set a timeout of 10 seconds
                foreach (var thread in monitoringThreads)
                {
                    if (!thread.Join((int)(endTime - DateTime.Now).TotalMilliseconds))
                    {
                        // If the thread does not finish within the timeout, abort it
                        thread.Abort();
                    }
                }
            }

            // Dispose of other resources if needed
        }

    }
}
