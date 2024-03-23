using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Globalization;
using System.Timers;
using Timer = System.Threading.Timer;

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
        private System.Timers.Timer restartTimer; // Timer for auto-restart functionality
        private System.Timers.Timer logCheckTimer; // Timer for log check functionality
        private volatile bool stopRequested = false;
        private string settingsFilePath = "settings.txt";

        public Form1()
        {
            InitializeComponent();
            logFolder = LogFolderTextBox.Text;
            UpdateToday(); // Initialize today variable
            initialDetectionDone = new bool[4];
            logFilePath = Path.Combine(Application.StartupPath, "log.txt");

            // Start monitoring when the application starts
            StartMonitoring();

            // Set up timer for auto-restart
            SetupRestartTimer();

            // Set up timer for periodic log file checks
            SetupLogCheckTimer();
        }

        private void UpdateToday()
        {
            today = DateTime.Now.ToString("yyyyMMdd");
        }

        private void StartMonitoring()
        {
            // Update UI
            lblStatus.Text = "Status: Monitoring";
            StartButton.Enabled = false;
            StopButton.Enabled = true;

            LogFolderTextBox.Enabled = false;
            LogStringTextBox.Enabled = false;
            ActionDelayTextBox.Enabled = false;
            Ch1Ch2FinishActionBox.Enabled = false;
            Ch3Ch4FinishActionBox.Enabled = false;
            RecheckLogFileTimeTextBox.Enabled = false;

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

        private void SetupRestartTimer()
        {
            // Calculate time until next day 12:01 AM
            TimeSpan timeUntilRestart = DateTime.Today.AddDays(1) - DateTime.Now;

            // Set up timer
            restartTimer = new System.Timers.Timer(); ;
            restartTimer.Interval = (int)timeUntilRestart.TotalMilliseconds;
            restartTimer.Elapsed += RestartTimer_Elapsed;
            restartTimer.Start();

        }

        private void RestartTimer_Elapsed(object sender, EventArgs e)
        {
            // Restart the application
            LogToFile("New day detected. Restarting application...");
            Application.Restart();
        }

        private void SetupLogCheckTimer()
        {
            // Create a timer to periodically check for log files
            logCheckTimer = new System.Timers.Timer();
            int intervalInSeconds;
            if (!int.TryParse(RecheckLogFileTimeTextBox.Text, out intervalInSeconds))
            {
                // Default to 1 second if parsing fails
                intervalInSeconds = 1800;
            }

            // Convert seconds to milliseconds for timer interval
            logCheckTimer.Interval = intervalInSeconds * 1000;
            logCheckTimer.Interval = TimeSpan.FromSeconds(10).TotalMilliseconds; // Adjust the interval as needed
            logCheckTimer.Elapsed += LogCheckTimer_Elapsed;
            logCheckTimer.Start();
        }

        private void LogCheckTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            // Check for log files and start monitoring if necessary
            CheckAndStartMonitoring();
        }

        private void CheckAndStartMonitoring()
        {
            logFileNames = GetLogFileNames(logFolder);

            if (logFileNames.Length == 0)
            {
                // No log files found, start monitoring again
                LogToFile("No log files found. Starting monitoring...");
                StartMonitoring();
            }
            else
            {
                LogToFile("Log files found. Monitoring continues...");
            }
        }



        private void StartButton_Click(object sender, EventArgs e)
        {
            StartMonitoring();
        }

        private void StopMonitoring()
        {
            // Update UI
            lblStatus.Text = "Status: Stopped...";
            StartButton.Enabled = true;
            StopButton.Enabled = false;

            LogFolderTextBox.Enabled = true;
            LogStringTextBox.Enabled = true;
            ActionDelayTextBox.Enabled = true;
            Ch1Ch2FinishActionBox.Enabled = true;
            Ch3Ch4FinishActionBox.Enabled = true;
            RecheckLogFileTimeTextBox.Enabled = true;

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

        private void StopButton_Click(object sender, EventArgs e)
        {
            StopMonitoring();
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

                                    string finishActionCh1Ch2 = string.Empty;
                                    string finishActionCh3Ch4 = string.Empty;

                                    Invoke((MethodInvoker)delegate
                                    {
                                        finishActionCh1Ch2 = Ch1Ch2FinishActionBox.Text;
                                        finishActionCh3Ch4 = Ch3Ch4FinishActionBox.Text;
                                    });

                                    string processName = string.Empty;

                                    // Set the process name based on the selected finish actions
                                    if (channelName == "Ch1 or Ch2")
                                    {
                                        if (finishActionCh1Ch2 == "Run Ch1 Ch2")
                                        {
                                            processName = "RunCh1Ch2.exe";
                                        }
                                        else if (finishActionCh1Ch2 == "Run Ch3 Ch4")
                                        {
                                            processName = "RunCh3Ch4.exe";
                                        }
                                        else if (finishActionCh1Ch2 == "Run Any Ch")
                                        {
                                            processName = "RunAnyCh.exe";
                                        }
                                    }
                                    else if (channelName == "Ch3 or Ch4")
                                    {
                                        if (finishActionCh3Ch4 == "Run Ch1 Ch2")
                                        {
                                            processName = "RunCh1Ch2.exe";
                                        }
                                        else if (finishActionCh3Ch4 == "Run Ch3 Ch4")
                                        {
                                            processName = "RunCh3Ch4.exe";
                                        }
                                        else if (finishActionCh3Ch4 == "Run Any Ch")
                                        {
                                            processName = "RunAnyCh.exe";
                                        }
                                    }


                                    if (string.IsNullOrEmpty(finishActionCh1Ch2) && string.IsNullOrEmpty(finishActionCh3Ch4))
                                    {
                                        LogToFile("No specific action selected for finish actions.");
                                        waitingForOccurrence = false;
                                        continue; // Skip starting any process
                                    }

                                    string appPath = Path.Combine(Application.StartupPath, processName);

                                    LogToFile($"Starting {processName} based on occurrence in {channelName}.");
                                    Invoke((MethodInvoker)delegate
                                    {
                                        lblLastAction.Text = $"Last action : {DateTime.Now}: Starting {processName} based on occurrence in {channelName}.\"";
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
            SaveSettings();
            // Check if monitoringThreads is not null
            if (monitoringThreads != null)
            {
                // Set stopRequested flag to true to request stop monitoring
                stopRequested = true;

                // Wait for all monitoring threads to finish, but with a timeout
                DateTime endTime = DateTime.Now.AddSeconds(10); // Set a timeout of 10 seconds
                foreach (var thread in monitoringThreads)
                {
                    if (thread != null && !thread.Join((int)(endTime - DateTime.Now).TotalMilliseconds))
                    {
                        // If the thread does not finish within the timeout, abort it
                        thread.Abort();
                    }
                }
            }
            

            // Dispose of other resources if needed
        }
        // Method to save settings to a file
        private void SaveSettings()
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(settingsFilePath))
                {
                    // Iterate through form controls and save their values
                    foreach (Control control in Controls)
                    {
                        if (control is TextBox textBox)
                        {
                            writer.WriteLine($"{textBox.Name}:{textBox.Text}");
                        }
                        if (control is ComboBox comboBox)
                        {
                            writer.WriteLine($"{comboBox.Name}:{comboBox.Text}");
                        }
                        // Add more conditions for other types of controls as needed
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingsFilePath))
                {
                    Dictionary<string, string> settings = new Dictionary<string, string>();

                    // Read settings from the file
                    foreach (string line in File.ReadLines(settingsFilePath))
                    {
                        string[] parts = line.Split(':');
                        if (parts.Length == 2)
                        {
                            settings[parts[0]] = parts[1];
                        }
                    }

                    // Set form control values based on settings
                    foreach (Control control in Controls)
                    {
                        if (control is TextBox textBox && settings.ContainsKey(textBox.Name))
                        {
                            textBox.Text = settings[textBox.Name];
                        }
                        // Add more conditions for other types of controls as needed
                    }

                    foreach (Control control in Controls)
                    {
                        if (control is ComboBox comboBox && settings.ContainsKey(comboBox.Name))
                        {
                            comboBox.Text = settings[comboBox.Name];
                        }
                        // Add more conditions for other types of controls as needed
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSettings();
        }
    }
}

