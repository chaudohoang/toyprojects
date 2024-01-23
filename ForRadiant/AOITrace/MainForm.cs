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
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnProcess_Click(object sender, EventArgs e)
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
                    List<ColorRecordFile> colorRecords = ReadCsvFile<ColorRecordFile, ColorRecordFileMap>(colorCsv);

                    foreach (var monoLogFolder in monoLogFolders)
                    {
                        List<string> monoCsvs = Directory.GetFiles(monoLogFolder, "*.csv").ToList();

                        foreach (var monoCsv in monoCsvs)
                        {
                            List<MonoRecordFile> monoRecords = ReadCsvFile<MonoRecordFile, MonoRecordFileMap>(monoCsv);

                            foreach (var colorRecord in colorRecords)
                            {
                                if (colorRecord.Description.IndexOf("spot", StringComparison.OrdinalIgnoreCase) != -1 && !string.IsNullOrEmpty(colorRecord.DefectInfo))
                                {
                                    var matchingRecord = monoRecords.FirstOrDefault(monoRecord => monoRecord.PID.Equals(colorRecord.PID, StringComparison.OrdinalIgnoreCase));

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
