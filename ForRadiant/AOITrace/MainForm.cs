using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public MainForm()
        {
            InitializeComponent();
            processRunTimer = new Timer();
            processRunTimer.Interval = 1000; // Update every second
            processRunTimer.Tick += ProcessRunTimer_Tick;
            countdownTimer = new Timer();
        }

        private void CompareLog()
        {
            List<string> colorLogFolders = new List<string>(txtColorLogFolders.Lines);
            List<string> monoLogFolders = new List<string>(txtMonoLogFolders.Lines);
            string outputCsvFilePath = txtOutputCsvPath.Text + "\\Matched_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

            List<OutputRecord> matchingRecords = new List<OutputRecord>();

            foreach (var colorLogFolder in colorLogFolders)
            {
                List<string> colorCsvs = Directory.GetFiles(colorLogFolder, "*.csv").ToList();

                foreach (var colorCsv in colorCsvs)
                {
                    FileInfo fileInfo = new FileInfo(colorCsv);

                    // Check if the file was created today, and exclude it
                    if (fileInfo.CreationTime.Date == DateTime.Now.Date)
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

                            // Check if the file was created today, and exclude it
                            if (monoFileInfo.CreationTime.Date == DateTime.Now.Date)
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

            WriteCsvFile(outputCsvFilePath, matchingRecords);
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            CompareLog();
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
            UpdateListBoxBasedOnDate();
        }

        private void lstResultFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstResultFiles.SelectedItem != null)
            {
                string selectedCsvFile = lstResultFiles.SelectedItem.ToString();
                System.Diagnostics.Process.Start(selectedCsvFile);
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
            processRunTimer.Tick += ProcessRunTimer_Tick;

            processRunTimer.Start();

            // Set up the countdown timer
            countdownTimer.Interval = 1000; // Update every second
            countdownTimer.Tick += (sender, args) =>
            {
                timeUntilScheduled = scheduledTime - DateTime.Now;
                UpdateCountdownLabel(timeUntilScheduled);
            };

            countdownTimer.Start();
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
        }



        private void ProcessRunTimer_Tick(object sender, EventArgs e)
        {
            // Perform the process run logic here
            CompareLog();

            // Stop the current timer
            StopTimer(processRunTimer);

            // Schedule the next run for the same time on the next day
            StartProcessRunTimer();
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


    
}
