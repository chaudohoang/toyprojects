using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Windows.Forms;

namespace FileLock
{
    public partial class Form1 : Form
    {
        private const string fileListFileName = "filelocklist.txt";
        private const string logFileName = "filelocklog.txt";

        public Form1()
        {
            InitializeComponent();
            LoadFileList(); // Load file list when the form loads
        }

        private void LoadFileList()
        {
            if (File.Exists(fileListFileName))
            {
                string[] fileLines = File.ReadAllLines(fileListFileName);
                foreach (string line in fileLines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        FilesTextBox.AppendText(line + Environment.NewLine);
                    }
                }
            }
        }

        private void SaveFileList()
        {
            File.WriteAllLines(fileListFileName, FilesTextBox.Lines);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveFileList(); // Save file list when the form is closing
        }

        private void LockButton_Click(object sender, EventArgs e)
        {
            Password passwordForm = new Password();
            passwordForm.ShowDialog();

            if (!passwordForm.IsPasswordCorrect)
            {
                WriteToLog("Incorrect password. Operation canceled.");
                return;
            }

            LockUnlockFiles(true);
        }

        private void UnlockButton_Click(object sender, EventArgs e)
        {
            Password passwordForm = new Password();
            passwordForm.ShowDialog();

            if (!passwordForm.IsPasswordCorrect)
            {
                WriteToLog("Incorrect password. Operation canceled.");
                return;
            }

            LockUnlockFiles(false);
        }

        private void LockUnlockFiles(bool lockFiles)
        {
            string[] files = FilesTextBox.Lines;

            foreach (string filePath in files)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        continue; // Skip empty lines
                    }

                    if (!File.Exists(filePath))
                    {
                        WriteToLog($"File '{Path.GetFileName(filePath)}' does not exist.");
                        continue; // Skip processing this file
                    }

                    if (IsFileLocked(filePath) && lockFiles)
                    {
                        WriteToLog($"File '{Path.GetFileName(filePath)}' is already locked.");
                        continue; // Skip locking this file
                    }

                    if (!IsFileLocked(filePath) && !lockFiles)
                    {
                        WriteToLog($"File '{Path.GetFileName(filePath)}' is already unlocked.");
                        continue; // Skip unlocking this file
                    }

                    FileSecurity fileSecurity = File.GetAccessControl(filePath);

                    if (lockFiles)
                    {
                        // Lock the file
                        FileSystemAccessRule rule = new FileSystemAccessRule(
                            new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                            FileSystemRights.FullControl,
                            AccessControlType.Deny);
                        fileSecurity.AddAccessRule(rule);
                    }
                    else
                    {
                        // Unlock the file
                        FileSystemAccessRule rule = new FileSystemAccessRule(
                            new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                            FileSystemRights.FullControl,
                            AccessControlType.Deny);
                        fileSecurity.RemoveAccessRule(rule);
                    }

                    File.SetAccessControl(filePath, fileSecurity);
                }
                catch (Exception ex)
                {
                    WriteToLog($"Error locking/unlocking file '{Path.GetFileName(filePath)}': {ex.Message}");
                }
            }
        }

        private bool IsFileLocked(string filePath)
        {
            try
            {
                FileSecurity fileSecurity = File.GetAccessControl(filePath);
                AuthorizationRuleCollection rules = fileSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));

                foreach (FileSystemAccessRule rule in rules)
                {
                    if (rule.AccessControlType == AccessControlType.Deny &&
                        rule.FileSystemRights == FileSystemRights.FullControl &&
                        rule.IdentityReference == new SecurityIdentifier(WellKnownSidType.WorldSid, null))
                    {
                        return true; // The file is locked
                    }
                }

                return false; // The file is not locked
            }
            catch (Exception)
            {
                return true; // Assume the file is locked if an exception occurs
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
                    FilesTextBox.AppendText(file + Environment.NewLine);
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
