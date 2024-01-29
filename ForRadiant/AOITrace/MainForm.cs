using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CsvHelper;
using CsvHelper.Configuration;

namespace AOITrace
{
    public partial class MainForm : Form
    {
        // Declare processRunTimer as a class-level field
        private Timer processRunTimer;
        private Timer countdownTimer;
        private Logger logger; // Logger instance
        private int newAOITracesCount = 0;
        private string unreadResultFilePath;
        private FileSystemWatcher fileWatcher;

        public MainForm()
        {
            InitializeComponent();
            processRunTimer = new Timer();
            countdownTimer = new Timer();
            cmbFileSelection.Items.AddRange(new string[] { "Check Only Summary Log Files Created On Previous Day", "Check All Summary Log Files" });
            cmbFileSelection.SelectedIndex = 0; // Default selection
            cmbTimeUnit.Items.AddRange(new string[] { "Seconds", "Minutes", "Hours" });
            cmbTimeUnit.SelectedIndex = 0; // Default to Seconds
            // Create an instance of Logger
            logger = new Logger(txtLogs, this, txtLogLimit, btnSetLogLimit); // Assuming txtLogs is a RichTextBox
            // Inside the MainForm constructor, after initializing components
            unreadResultFilePath = Path.Combine(Environment.CurrentDirectory, "UnreadResult.txt");
            // Initialize FileSystemWatcher
            InitializeFileSystemWatcher();
        }

        private void InitializeFileSystemWatcher()
        {
            fileWatcher = new FileSystemWatcher();
            fileWatcher.Path = Environment.CurrentDirectory;
            fileWatcher.Filter = "UnreadResult.txt";
            fileWatcher.NotifyFilter = NotifyFilters.LastWrite;
            fileWatcher.Changed += FileWatcher_Changed;

            // Check if the form has been created before enabling FileSystemWatcher
            if (IsHandleCreated)
            {
                fileWatcher.EnableRaisingEvents = true;

                // Initial update of lblNotification
                UpdateNotificationLabel();
            }
            else
            {
                // If the handle hasn't been created yet, wait for the form to load
                Load += (sender, e) =>
                {
                    fileWatcher.EnableRaisingEvents = true;

                    // Initial update of lblNotification
                    UpdateNotificationLabel();
                };
            }
        }
        private void UpdateNotificationLabel()
        {
            // Check if the form has been created before invoking UI updates
            if (IsHandleCreated)
            {
                // Use Invoke to update the UI from the UI thread
                Invoke((MethodInvoker)delegate
                {
                    // Read the lines from UnreadResult.txt
                    string[] unreadResultLines = File.Exists(unreadResultFilePath) ? File.ReadAllLines(unreadResultFilePath) : new string[0];

                    // Update lblNotification based on the number of lines
                    lblNotification.Text = $"{unreadResultLines.Length} Unread Result";

                    // Change text color to red when the number of lines is not equal to 0
                    lblNotification.ForeColor = unreadResultLines.Length != 0 ? Color.Red : SystemColors.ControlText;

                    // Update the count of newAOITracesCount
                    newAOITracesCount = unreadResultLines.Length;
                });
            }
        }


        private void FileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            // Check if the form has been created before invoking UI updates
            if (IsHandleCreated)
            {
                // Use Invoke to update the UI from the UI thread
                Invoke((MethodInvoker)delegate
                {
                    // Handle file changes, update lblNotification
                    UpdateNotificationLabel();

                    // Update the UnreadResult ListBox
                    UpdateUnreadResultListBox();
                });
            }
        }


        private void CompareLog()
        {
            try
            {
                logger.LogInfo("Comparison started.");
                List<string> colorLogFolders = new List<string>();
                List<string> monoLogFolders = new List<string>();
                string outputCsvFilePath = string.Empty;
                // Use Invoke to access UI controls on the UI thread
                txtColorLogFolders.Invoke((MethodInvoker)delegate
                {
                    colorLogFolders = new List<string>(txtColorLogFolders.Lines);
                });

                txtMonoLogFolders.Invoke((MethodInvoker)delegate
                {
                    monoLogFolders = new List<string>(txtMonoLogFolders.Lines);
                });

                txtOutputCsvPath.Invoke((MethodInvoker)delegate
                {
                    outputCsvFilePath = txtOutputCsvPath.Text + "\\Matched_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";
                });

                List<OutputRecord> matchingRecords = new List<OutputRecord>();

                foreach (var colorLogFolder in colorLogFolders)
                {
                    logger.LogInfo($"Processing color log folder: {colorLogFolder}");
                    List<string> colorCsvs = Directory.GetFiles(colorLogFolder, "*.csv").ToList();

                    foreach (var colorCsv in colorCsvs)
                    {
                        FileInfo fileInfo = new FileInfo(colorCsv);

                        // Check based on ComboBox selection
                        if (ShouldExcludeFile(fileInfo))
                        {
                            continue;
                        }

                        List<ColorRecordFile> colorRecords = ReadCsvFile<ColorRecordFile, ColorRecordFileMap>(colorCsv);

                        foreach (var monoLogFolder in monoLogFolders)
                        {
                            List<string> monoCsvs = Directory.GetFiles(monoLogFolder, "*.csv").ToList();

                            foreach (var monoCsv in monoCsvs)
                            {
                                FileInfo monoFileInfo = new FileInfo(monoCsv);

                                // Check based on ComboBox selection
                                if (ShouldExcludeFile(monoFileInfo))
                                {
                                    continue;
                                }

                                List<MonoRecordFile> monoRecords = ReadCsvFile<MonoRecordFile, MonoRecordFileMap>(monoCsv);

                                foreach (var colorRecord in colorRecords)
                                {
                                    if (colorRecord.Description.IndexOf("spot", StringComparison.OrdinalIgnoreCase) != -1 &&
                                        !string.IsNullOrEmpty(colorRecord.DefectInfo))
                                    {
                                        var matchingRecord = monoRecords.FirstOrDefault(monoRecord =>
                                            monoRecord.PID.Equals(colorRecord.PID, StringComparison.OrdinalIgnoreCase));

                                        if (matchingRecord != null)
                                        {
                                            matchingRecords.Add(new OutputRecord
                                            {
                                                PID = colorRecord.PID,
                                                MonoStation = matchingRecord.EQPID,
                                                ColorStation = colorRecord.EQPID,
                                                MonoChannel = matchingRecord.CH,
                                                ColorChannel = colorRecord.CH,
                                                DefectInfo = colorRecord.DefectInfo
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Check if there are matching records before creating the result file
                if (matchingRecords.Any())
                {
                    // Use Invoke to access UI controls on the UI thread
                    txtOutputCsvPath.Invoke((MethodInvoker)delegate
                    {
                        WriteCsvFile(outputCsvFilePath, matchingRecords);
                        logger.LogMatchingRecordNotification(outputCsvFilePath);

                        // Check if the outputCsvFilePath is already in UnreadResult.txt
                        if (!File.Exists(unreadResultFilePath) || !File.ReadLines(unreadResultFilePath).Contains(outputCsvFilePath))
                        {
                            // Write the outputCsvFilePath to UnreadResult.txt
                            using (StreamWriter writer = File.AppendText(unreadResultFilePath))
                            {
                                // Format the line with file path and number of matching records
                                string line = $"{outputCsvFilePath}, {matchingRecords.Count} matching record(s)";
                                writer.WriteLine(line);
                            }
                        }
                        else
                        {
                            // Optionally inform the user or log that the file path already exists
                            Console.WriteLine($"{outputCsvFilePath} already exists in UnreadResult.txt");
                        }
                    });

                }
                else
                {
                    logger.LogInfo($"No matching records found. No result file created.");
                }
                logger.LogInfo("Comparison completed.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError($"Unauthorized access error: {ex.Message}");
            }
            catch (IOException ex)
            {
                logger.LogError($"I/O error during comparison: {ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error during comparison: {ex.Message}");
            }
        }

        private bool ShouldExcludeFile(FileInfo fileInfo)
        {
            bool result = false;

            // Use Invoke to access UI controls on the UI thread
            cmbFileSelection.Invoke((MethodInvoker)delegate
            {
                // Check based on ComboBox selection
                switch (cmbFileSelection.SelectedItem.ToString())
                {
                    case "Check Only Summary Log Files Created On Previous Day":
                        result = fileInfo.CreationTime.Date != DateTime.Now.Date.AddDays(-1);
                        break;
                    // Add more cases for additional options if needed
                    default:
                        result = false; // Default behavior: include all files
                        break;
                }
            });

            return result;
        }

        private async void btnProcess_Click(object sender, EventArgs e)
        {
            // Disable the button to prevent multiple clicks during the operation
            btnProcess.Enabled = false;

            try
            {
                // Start the heavy operation in the background
                await Task.Run(() => CompareLog());

                // Update UI or perform any other post-operation tasks if needed
            }
            finally
            {
                // Enable the button after the operation completes (either successfully or with an exception)
                btnProcess.Enabled = true;
            }
        }

        static List<T> ReadCsvFile<T, TMap>(string filePath)
            where T : class
            where TMap : ClassMap<T>
        {
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { HasHeaderRecord = true, PrepareHeaderForMatch = args => args.Header.ToLower() }))
            {
                csv.Context.RegisterClassMap<TMap>();
                return csv.GetRecords<T>().ToList();
            }
        }

        static void WriteCsvFile<T>(string filePath, IEnumerable<T> records)
        {
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(records);
            }
        }

        private void btnLoadColorLogFolders_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtColorLogFolders.Lines = File.ReadAllLines(openFileDialog.FileName).ToArray();
                }
            }
        }

        private void btnSaveColorLogFolders_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllLines(saveFileDialog.FileName, txtColorLogFolders.Lines);
                }
            }
        }

        private void btnLoadMonoLogFolders_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    txtMonoLogFolders.Lines = File.ReadAllLines(openFileDialog.FileName).ToArray();
                }
            }
        }

        private void btnSaveMonoLogFolders_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllLines(saveFileDialog.FileName, txtMonoLogFolders.Lines);
                }
            }
        }

        private void btnBrowseResultFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = folderBrowserDialog.SelectedPath;
                    txtResultFolderPath.Text = selectedFolder;
                    UpdateListBoxBasedOnDate();

                }
            }
        }

        private void tabPage2_Enter(object sender, EventArgs e)
        {
          
            // Handle file changes, update lblNotification
            UpdateListBoxBasedOnDate();

            // Update the UnreadResult ListBox
            UpdateUnreadResultListBox();                
            
        }

        private void lstResultFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstResultFiles.SelectedItem != null)
            {
                string selectedCsvFile = lstResultFiles.SelectedItem.ToString();
                System.Diagnostics.Process.Start(selectedCsvFile);

                // Check if the file path exists in UnreadResult.txt
                if (File.Exists(unreadResultFilePath))
                {
                    // Read all lines from UnreadResult.txt
                    string[] lines = File.ReadAllLines(unreadResultFilePath);

                    // Check if the selectedCsvFile is in the lines
                    string lineToRemove = lines.FirstOrDefault(line => line.StartsWith(selectedCsvFile, StringComparison.OrdinalIgnoreCase));

                    if (lineToRemove != null)
                    {
                        // Remove the line from the array
                        lines = lines.Where(line => line != lineToRemove).ToArray();

                        // Write the updated lines back to UnreadResult.txt
                        File.WriteAllLines(unreadResultFilePath, lines);
                        UpdateNotificationLabel();

                        // Inform the user
                        logger.LogInfo($"Removed '{selectedCsvFile}' from UnreadResult.txt");
                    }
                    else
                    {
                        // Inform the user that the file path was not found in UnreadResult.txt
                        logger.LogInfo($"'{selectedCsvFile}' not found in UnreadResult.txt");
                    }
                }
                else
                {
                    // Inform the user that UnreadResult.txt does not exist
                    logger.LogInfo("UnreadResult.txt not found");
                }
            }
        }


        private void dateTimePickerFilter_ValueChanged(object sender, EventArgs e)
        {
            // Update the ListBox based on the selected date
            UpdateListBoxBasedOnDate();
        }

        private void UpdateListBoxBasedOnDate()
        {
            if (Directory.Exists(txtResultFolderPath.Text))
            {                
                // Get the selected date
                DateTime selectedDate = dateTimePickerFilter.Value.Date;

                // Get all CSV files in the selected folder
                string[] csvFiles = Directory.GetFiles(txtResultFolderPath.Text, "Matched_*.csv");

                // Filter CSV files based on the selected date in the file name
                csvFiles = csvFiles.Where(file =>
                {
                    // Extract the date part from the file name
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);
                    if (fileNameWithoutExtension != null && fileNameWithoutExtension.StartsWith("Matched_"))
                    {
                        string dateString = fileNameWithoutExtension.Substring("Matched_".Length);
                        if (DateTime.TryParseExact(dateString, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime fileDate))
                        {
                            if (!chkShowBefore.Checked && !chkShowAfter.Checked)
                            {
                                // Include the file if its date is on the selected date
                                return fileDate.Date == selectedDate;
                            }
                            else if (chkShowBefore.Checked && !chkShowAfter.Checked)
                            {
                                // Include the file if its date is on or before the selected date
                                return fileDate.Date <= selectedDate;
                            }
                            else if (!chkShowBefore.Checked && chkShowAfter.Checked)
                            {
                                // Include the file if its date is on or after the selected date
                                return fileDate.Date >= selectedDate;
                            }
                            else if (chkShowBefore.Checked && chkShowAfter.Checked)
                            {
                                // Show all the files
                                return true;
                            }

                        }
                    }
                    return false;
                }).ToArray();

                // Sort CSV files by date (newest on top)
                Array.Sort(csvFiles, (a, b) => File.GetLastWriteTime(b).CompareTo(File.GetLastWriteTime(a)));

                // Update the ListBox
                lstResultFiles.Items.Clear();
                lstResultFiles.Items.AddRange(csvFiles);
            }
        }

        private void UpdateUnreadResultListBox()
        {
            // Check if the form has been created before invoking UI updates
            if (IsHandleCreated)
            {
                // Use Invoke to update the UI from the UI thread
                Invoke((MethodInvoker)delegate
                {
                    // Read the lines from UnreadResult.txt
                    string[] unreadResultLines = File.Exists(unreadResultFilePath) ? File.ReadAllLines(unreadResultFilePath) : new string[0];

                    // Update the ListBox
                    lstUnreadResultFiles.Items.Clear();
                    lstUnreadResultFiles.Items.AddRange(unreadResultLines);
                });
            }
        }

        private void chkShowBefore_CheckedChanged(object sender, EventArgs e)
        {
            UpdateListBoxBasedOnDate();
        }

        private void chkShowAfter_CheckedChanged(object sender, EventArgs e)
        {
            UpdateListBoxBasedOnDate();
        }

        private void btnScheduleProcessRun_Click(object sender, EventArgs e)
        {
            if (btnScheduleProcessRun.Text == "Run")
            {
                // Start the process run timer
                StartProcessRunTimer();
            }
            else
            {
                // Stop the process run timer
                StopProcessRunTimer();
            }
        }

        private void StartProcessRunTimer()
        {
            DateTime scheduledTime = DateTime.Now.Date + dateTimePickerProcessRun.Value.TimeOfDay;

            if (scheduledTime <= DateTime.Now)
            {
                scheduledTime = scheduledTime.AddDays(1);
            }

            TimeSpan timeUntilScheduled = scheduledTime - DateTime.Now;

            UpdateCountdownLabel(timeUntilScheduled);

            btnScheduleProcessRun.Text = "Stop";
            processRunTimer.Interval = (int)timeUntilScheduled.TotalMilliseconds;

            // Set up an event handler for the timer tick
            processRunTimer.Tick -= ProcessRunTimer_Tick; // Remove the event handler to prevent multiple registrations
            processRunTimer.Tick += ProcessRunTimer_Tick;

            processRunTimer.Start();

            // Set up the countdown timer
            countdownTimer.Interval = 1000; // Update every second
            countdownTimer.Tick -= (sender, args) =>
            {
                timeUntilScheduled = scheduledTime - DateTime.Now;
                UpdateCountdownLabel(timeUntilScheduled);
            };

            countdownTimer.Tick += (sender, args) =>
            {
                timeUntilScheduled = scheduledTime - DateTime.Now;
                UpdateCountdownLabel(timeUntilScheduled);
            };

            countdownTimer.Start();

            // Log the start of the schedule
            logger.LogInfo("Scheduled process run started.");

        }


        private void UpdateCountdownLabel(TimeSpan remainingTime)
        {
            // Update the label with the remaining time in hours, minutes, and seconds
            lblCountdown.Text = $"Next Run: {remainingTime.Hours:D2}h {remainingTime.Minutes:D2}m {remainingTime.Seconds:D2}s";
        }

        private void StopProcessRunTimer()
        {
            btnScheduleProcessRun.Text = "Run";
            StopTimer(processRunTimer);

            // Stop the countdown timer
            StopTimer(countdownTimer);

            lblCountdown.Text = string.Empty;

            // Log the stop of the schedule
            logger.LogInfo("Scheduled process run stopped.");
        }



        private async void ProcessRunTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Perform the process run logic asynchronously
                await Task.Run(() => CompareLog());

                // Schedule the next run for the same time on the next day
                StartProcessRunTimer();

                // Log the completion of the process run
                logger.LogInfo("Scheduled process run completed.");
            }
            catch (Exception ex)
            {
                // Log the exception details
                logger.LogError($"An error occurred during scheduled process run: {ex.Message}");
            }
        }

        private void ScheduleNextRun()
        {
            // Set the next run time to the selected time on the same day
            DateTime nextRunTime = DateTime.Now.Date + dateTimePickerProcessRun.Value.TimeOfDay;

            // Calculate the time difference until the next run
            TimeSpan timeUntilNextRun = nextRunTime - DateTime.Now;

            // Ensure the next run time is in the future
            if (timeUntilNextRun.TotalSeconds > 0)
            {
                // Set the interval to the time difference
                processRunTimer.Interval = (int)timeUntilNextRun.TotalMilliseconds;
                processRunTimer.Start();
            }
            else
            {
                // If the next run time is not in the future, stop the timer (no automatic rescheduling)
                StopTimer(processRunTimer);
            }
        }

        private void StopTimer(Timer timer)
        {
            timer.Stop();            
        }

        private void btnBrowseOutputFolder_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFolder = folderBrowserDialog.SelectedPath;
                    txtOutputCsvPath.Text = selectedFolder;

                }
            }
        }

        private void btnClearLogs_Click(object sender, EventArgs e)
        {
            txtLogs.Text = "";
        }

        private void btnSetCurrentTime_Click(object sender, EventArgs e)
        {
            // Set the time picker value to the current time
            dateTimePickerProcessRun.Value = DateTime.Now;
        }

        private void btnAddTime_Click(object sender, EventArgs e)
        {
            // Parse the user input for the time to add
            if (int.TryParse(txtTimeToAdd.Text, out int timeToAdd))
            {
                // Determine the time unit based on the selected item in the combobox
                string selectedTimeUnit = cmbTimeUnit.SelectedItem.ToString();

                // Add the specified time to the current value of dateTimePickerProcessRun
                DateTime newDateTime = dateTimePickerProcessRun.Value;

                switch (selectedTimeUnit)
                {
                    case "Seconds":
                        newDateTime = newDateTime.AddSeconds(timeToAdd);
                        break;
                    case "Minutes":
                        newDateTime = newDateTime.AddMinutes(timeToAdd);
                        break;
                    case "Hours":
                        newDateTime = newDateTime.AddHours(timeToAdd);
                        break;
                        // Add more cases for additional time units if needed
                }

                // Set the updated value to dateTimePickerProcessRun
                dateTimePickerProcessRun.Value = newDateTime;
            }
            else
            {
                // Handle invalid input (non-numeric) if needed
                MessageBox.Show("Invalid input. Please enter a numeric value for time.");
            }
        }

        private void lstUnreadResultFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstUnreadResultFiles.SelectedItem != null)
            {
                string selectedCsvFile = lstUnreadResultFiles.SelectedItem.ToString();

                // Extract the file path from the comma-separated line
                string[] lineParts = selectedCsvFile.Split(',');
                if (lineParts.Length > 0)
                {
                    string filePath = lineParts[0].Trim();

                    // Start the process using the corrected file path
                    System.Diagnostics.Process.Start(filePath);

                    // Check if the file path exists in UnreadResult.txt
                    if (File.Exists(unreadResultFilePath))
                    {
                        // Read all lines from UnreadResult.txt
                        string[] lines = File.ReadAllLines(unreadResultFilePath);

                        // Check if the selectedCsvFile is in the lines
                        string lineToRemove = lines.FirstOrDefault(line => line.StartsWith(selectedCsvFile, StringComparison.OrdinalIgnoreCase));

                        if (lineToRemove != null)
                        {
                            // Remove the line from the array
                            lines = lines.Where(line => line != lineToRemove).ToArray();

                            // Write the updated lines back to UnreadResult.txt
                            File.WriteAllLines(unreadResultFilePath, lines);
                            UpdateNotificationLabel();

                            // Inform the user
                            logger.LogInfo($"Removed '{selectedCsvFile}' from UnreadResult.txt");
                        }
                        else
                        {
                            // Inform the user that the file path was not found in UnreadResult.txt
                            logger.LogInfo($"'{selectedCsvFile}' not found in UnreadResult.txt");
                        }
                    }
                    else
                    {
                        // Inform the user that UnreadResult.txt does not exist
                        logger.LogInfo("UnreadResult.txt not found");
                    }
                }
                else
                {
                    // Handle the case where the line doesn't contain a valid file path
                    logger.LogInfo($"Invalid format in UnreadResult.txt: '{selectedCsvFile}'");
                }
            }
        }

        
    }

    class ColorRecordFile
    {
        public string PID { get; set; }
        public string CH { get; set; }
        public string EQPID { get; set; }
        public string Description { get; set; }
        public string DefectInfo { get; set; }
    }

    class MonoRecordFile
    {
        public string PID { get; set; }
        public string CH { get; set; }
        public string EQPID { get; set; }
    }

    class OutputRecord
    {
        public string PID { get; set; }
        public string MonoStation { get; set; }
        public string MonoChannel { get; set; }
        public string ColorStation { get; set; }
        public string ColorChannel { get; set; }
        public string DefectInfo { get; set; }
    }

    class ColorRecordFileMap : ClassMap<ColorRecordFile>
    {
        public ColorRecordFileMap()
        {
            Map(m => m.PID).Name("PID");
            Map(m => m.CH).Name("CH");
            Map(m => m.EQPID).Name("EQP ID");
            Map(m => m.Description).Name("Description");
            Map(m => m.DefectInfo).Name("Defect Info");
        }
    }

    class MonoRecordFileMap : ClassMap<MonoRecordFile>
    {
        public MonoRecordFileMap()
        {
            Map(m => m.PID).Name("PID");
            Map(m => m.CH).Name("CH");
            Map(m => m.EQPID).Name("EQP ID");
        }
    }

    class Logger
    {
        private RichTextBox logRichTextBox;
        private Form mainForm;
        private int maxLogLines = 200;

        private TextBox txtLogLimit;
        private Button btnSetLogLimit;

        public Logger(RichTextBox richTextBox, Form form, TextBox logLimitTextBox, Button setLogLimitButton)
        {
            logRichTextBox = richTextBox;
            mainForm = form;
            txtLogLimit = logLimitTextBox;
            btnSetLogLimit = setLogLimitButton;

            // Disable the text box by default
            txtLogLimit.Enabled = false;

            // Attach the button click event
            btnSetLogLimit.Click += BtnSetLogLimit_Click;
        }

        public void LogInfo(string message)
        {
            Log($"[INFO] {DateTime.Now}: {message}", Color.Black);
        }

        public void LogError(string message)
        {
            Log($"[ERROR] {DateTime.Now}: {message}", Color.Red);
        }

        public void LogMatchingRecordNotification(string message)
        {
            Log($"[MATCH] {DateTime.Now}: {message}", Color.Red); // Assuming you want to highlight matching records in red
        }

        private void Log(string message, Color color)
        {
            mainForm.Invoke((MethodInvoker)delegate
            {
                // Set the color for the entire log message
                logRichTextBox.SelectionColor = color;

                // Append the message to the RichTextBox
                logRichTextBox.AppendText(message + Environment.NewLine);

                // Reset the color to default
                logRichTextBox.SelectionColor = logRichTextBox.ForeColor;

                // Trim excess lines if the log exceeds the maximum
                TrimExcessLogLines();
            });
        }

        private void TrimExcessLogLines()
        {
            if (logRichTextBox.Lines.Length > maxLogLines)
            {
                // Get the lines beyond the maximum limit
                string[] remainingLines = logRichTextBox.Lines.Skip(logRichTextBox.Lines.Length - maxLogLines).ToArray();

                // Set the text with the remaining lines
                logRichTextBox.Lines = remainingLines;
            }
        }

        private void BtnSetLogLimit_Click(object sender, EventArgs e)
        {
            if (txtLogLimit.Enabled)
            {
                // Attempt to set the log limit
                if (int.TryParse(txtLogLimit.Text, out int newLimit) && newLimit > 0)
                {
                    // Update the log limit
                    maxLogLines = newLimit;

                    // Trim excess lines based on the new limit
                    TrimExcessLogLines();

                    // Disable the text box
                    txtLogLimit.Enabled = false;

                    // Change button text back to "Set"
                    btnSetLogLimit.Text = "Set Log Limit";
                }
                else
                {
                    // Invalid input, display a message or handle accordingly
                    MessageBox.Show("Invalid log limit. Please enter a positive integer.");
                }
            }
            else
            {
                // Enable the text box
                txtLogLimit.Enabled = true;

                // Change button text to "Save"
                btnSetLogLimit.Text = "Save";
            }
        }
    }



}
