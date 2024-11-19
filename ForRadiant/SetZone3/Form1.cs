using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SetZone3
{
    public partial class Form1 : Form
    {
        // Declare the CreateFile method
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        private const uint GENERIC_WRITE = 0x40000000;
        private const uint OPEN_ALWAYS = 4;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSelectFiles_Click(object sender, EventArgs e)
        {
            // Open file dialog for selecting files
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.Title = "Select Files to Block";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in openFileDialog.FileNames)
                    {
                        if (!lstFiles.Items.Contains(file))
                        {
                            lstFiles.Items.Add(file);
                        }
                    }
                }
            }
        }

        private void btnBlockFiles_Click(object sender, EventArgs e)
        {
            if (lstFiles.Items.Count == 0)
            {
                MessageBox.Show("Please select at least one file to block.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (string filePath in lstFiles.Items)
            {
                try
                {
                    // Validate file existence
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show($"File does not exist: {filePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }

                    // Properly format the ADS path
                    string adsPath = @"\\?\" + filePath + ":Zone.Identifier";
                    Console.WriteLine($"Processing file: {filePath}");
                    Console.WriteLine($"ADS Path: {adsPath}");

                    // Use Windows API to create or open the alternate data stream
                    IntPtr handle = CreateFile(adsPath, GENERIC_WRITE, 0, IntPtr.Zero, OPEN_ALWAYS, 0, IntPtr.Zero);
                    if (handle == IntPtr.Zero || handle.ToInt64() == -1)
                    {
                        throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                    }

                    using (FileStream fs = new FileStream(handle, FileAccess.Write))
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        writer.WriteLine("[ZoneTransfer]");
                        writer.WriteLine("ZoneId=3"); // Internet zone
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to block file: {filePath}\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            MessageBox.Show("Selected files are now blocked.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }



        private void btnUnblockFiles_Click(object sender, EventArgs e)
        {
            if (lstFiles.Items.Count == 0)
            {
                MessageBox.Show("Please select at least one file to unblock.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            foreach (string filePath in lstFiles.Items)
            {
                try
                {
                    // Validate file existence
                    if (!File.Exists(filePath))
                    {
                        MessageBox.Show($"File does not exist: {filePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        continue;
                    }

                    // Construct the ADS path for Zone.Identifier
                    string adsPath = @"\\?\" + filePath + ":Zone.Identifier";

                    // Check if the ADS exists
                    if (File.Exists(adsPath))
                    {
                        // Delete the Zone.Identifier ADS
                        File.Delete(adsPath);
                        Console.WriteLine($"Unblocked file: {filePath}");
                    }
                    else
                    {
                        Console.WriteLine($"No Zone.Identifier found for file: {filePath}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to unblock file: {filePath}\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            MessageBox.Show("Selected files are now unblocked.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

    }
}
