using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace FileHide
{
    public partial class Form1 : Form
    {
        private const string controlValuesFileName = "settings.txt";
        private const string logFileName = "filelocklog.txt";

        public Form1()
        {
            InitializeComponent();
            LoadControlValues(); // Load control values when the form loads

            // Add DragEnter and DragDrop events to FoldersTextBox
            FoldersTextBox.DragEnter += new DragEventHandler(FoldersTextBox_DragEnter);
            FoldersTextBox.DragDrop += new DragEventHandler(FoldersTextBox_DragDrop);
        }

        private void LoadControlValues()
        {
            if (File.Exists(controlValuesFileName))
            {
                string[] lines = File.ReadAllLines(controlValuesFileName);
                FilesTextBox.Clear();
                FoldersTextBox.Clear();
                ExtensionsTextBox.Clear();

                bool readingFiles = false;
                bool readingFolders = false;
                bool readingExtensions = false;

                foreach (string line in lines)
                {
                    if (line == "[Files]")
                    {
                        readingFiles = true;
                        readingFolders = false;
                        readingExtensions = false;
                        continue;
                    }
                    if (line == "[Folders]")
                    {
                        readingFiles = false;
                        readingFolders = true;
                        readingExtensions = false;
                        continue;
                    }
                    if (line == "[Extensions]")
                    {
                        readingFiles = false;
                        readingFolders = false;
                        readingExtensions = true;
                        continue;
                    }

                    if (readingFiles)
                    {
                        FilesTextBox.AppendText(line + Environment.NewLine);
                    }
                    if (readingFolders)
                    {
                        FoldersTextBox.AppendText(line + Environment.NewLine);
                    }
                    if (readingExtensions)
                    {
                        ExtensionsTextBox.AppendText(line);
                    }
                }
            }
        }

        private void SaveControlValues()
        {
            using (StreamWriter sw = new StreamWriter(controlValuesFileName))
            {
                sw.WriteLine("[Files]");
                foreach (string line in FilesTextBox.Lines)
                {
                    sw.WriteLine(line);
                }

                sw.WriteLine("[Folders]");
                foreach (string line in FoldersTextBox.Lines)
                {
                    sw.WriteLine(line);
                }

                sw.WriteLine("[Extensions]");
                sw.WriteLine(ExtensionsTextBox.Text);
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveControlValues(); // Save control values when the form is closing
        }

        private void HideButton_Click(object sender, EventArgs e)
        {
            Password passwordForm = new Password();
            passwordForm.ShowDialog();

            if (!passwordForm.IsPasswordCorrect)
            {
                WriteToLog("Incorrect password. Operation canceled.");
                return;
            }

            HideUnhideFiles(true);
        }

        private void UnhideButton_Click(object sender, EventArgs e)
        {
            Password passwordForm = new Password();
            passwordForm.ShowDialog();

            if (!passwordForm.IsPasswordCorrect)
            {
                WriteToLog("Incorrect password. Operation canceled.");
                return;
            }

            HideUnhideFiles(false);
        }

        private void HideUnhideFiles(bool hideFiles)
        {
            string[] filePaths = FilesTextBox.Lines;
            string[] folderPaths = FoldersTextBox.Lines;
            string[] extensions = ExtensionsTextBox.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                          .Select(ext => ext.Trim().ToLower()).ToArray();

            bool includeAllFiles = extensions.Contains("*");

            // Process individual files
            foreach (string filePath in filePaths)
            {
                ProcessFile(filePath, hideFiles);
            }

            // Process folders
            foreach (string folderPath in folderPaths)
            {
                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                {
                    WriteToLog($"Folder '{folderPath}' does not exist.");
                    continue;
                }

                ProcessFolder(folderPath, extensions, includeAllFiles, hideFiles);
            }
        }

        private void ProcessFile(string filePath, bool hideFiles)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                {
                    WriteToLog($"File '{filePath}' does not exist.");
                    return;
                }

                FileAttributes attributes = File.GetAttributes(filePath);

                if (hideFiles)
                {
                    // Hide the file
                    attributes |= FileAttributes.Hidden | FileAttributes.System;
                    File.SetAttributes(filePath, attributes);
                    WriteToLog($"File '{filePath}' is now hidden.");
                }
                else
                {
                    // Unhide the file
                    attributes &= ~FileAttributes.Hidden;
                    attributes &= ~FileAttributes.System;
                    File.SetAttributes(filePath, attributes);
                    WriteToLog($"File '{filePath}' is now visible.");
                }
            }
            catch (Exception ex)
            {
                WriteToLog($"Error processing file '{filePath}': {ex.Message}");
            }
        }

        private void ProcessFolder(string folderPath, string[] extensions, bool includeAllFiles, bool hideFiles)
        {
            try
            {
                if (includeAllFiles)
                {
                    extensions = new string[] { "*.*" };
                }

                foreach (string extension in extensions)
                {
                    string searchPattern = includeAllFiles ? "*.*" : $"*.{extension}";
                    string[] files = Directory.GetFiles(folderPath, searchPattern, SearchOption.AllDirectories);

                    foreach (string file in files)
                    {
                        ProcessFile(file, hideFiles);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToLog($"Error processing folder '{folderPath}': {ex.Message}");
            }
        }

        private void FilesTextBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FilesTextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (!FilesTextBox.Lines.Contains(file))
                    {
                        FilesTextBox.AppendText(file + Environment.NewLine);
                    }
                }
            }
        }

        private void FoldersTextBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void FoldersTextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] folders = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string folder in folders)
                {
                    if (!FoldersTextBox.Lines.Contains(folder))
                    {
                        FoldersTextBox.AppendText(folder + Environment.NewLine);
                    }
                }
            }
        }

        private void WriteToLog(string message)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(logFileName))
                {
                    sw.WriteLine($"{DateTime.Now}: {message}");
                }
            }
            catch (Exception ex)
            {
                // If logging fails, you might want to handle this accordingly
                MessageBox.Show($"Error writing to log file: {ex.Message}");
            }
        }
    }
}
