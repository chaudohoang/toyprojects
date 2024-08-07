﻿using Microsoft.VisualBasic.FileIO;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.IO;
using System.Data.OleDb;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using System.ComponentModel;
using System.Drawing;
using System.Collections.Generic;
using System.Threading;
using System.Text;

namespace RemoteTools
{
    public partial class Form1 : Form
    {
        static BackgroundWorker bw;
        string IP;
        public Form1()
        {
            InitializeComponent();
            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            richTextBox1.HideSelection = false;
        }

        private void SetVersionInfo()
        {
            Version versionInfo = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime startDate = new DateTime(2000, 1, 1);
            int diffDays = versionInfo.Build;
            DateTime computedDate = startDate.AddDays(diffDays);
            string lastBuilt = computedDate.ToShortDateString();
            //this.Text = string.Format("{0} - {1} ({2})",
            //            this.Text, versionInfo.ToString(), lastBuilt);
            this.Text = string.Format("{0} - {1}",
                        this.Text, versionInfo.ToString());
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
            string CSVFilePathName = @"All.csv";
            string[] Lines = File.ReadAllLines(CSVFilePathName);
            string[] Fields;
            //Fields = Lines[0].Split(new char[] { ',' });
            Fields = "PC,IP".Split(new char[] { ',' });
            int Cols = Fields.GetLength(0);
            DataTable dt = new DataTable();

            //1st row must be column names; force lower case to ensure matching later on.
            for (int i = 0; i < Cols; i++)
                dt.Columns.Add(Fields[i].ToLower(), typeof(string));
            DataRow Row;
            for (int i = 0; i < Lines.GetLength(0); i++)
            {
                Fields = Lines[i].Split(new char[] { ',' });
                Row = dt.NewRow();
                for (int f = 0; f < Cols; f++)
                    Row[f] = Fields[f];
                dt.Rows.Add(Row);
            }
            dataGridView1.DataSource = dt;

        }
        private void btnFileShare_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell != null)
            {
                int rowindex = dataGridView1.CurrentCell.RowIndex;
                int columnindex = 1;
                IP = "\\\\" +dataGridView1.Rows[rowindex].Cells[columnindex].Value.ToString();
            }
            else
            {
                IP = "\\\\" + textBox1.Text;
            }

            try
            {
                Process.Start(IP);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void btnRemoteControl_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell != null)
            {
                int rowindex = dataGridView1.CurrentCell.RowIndex;
                int columnindex = 1;
                IP = dataGridView1.Rows[rowindex].Cells[columnindex].Value.ToString();
            }
            else
            {
                IP = textBox1.Text;
            }

            try
            {
                Process.Start(@"tvnviewer.exe", IP);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

            BindingSource bs = new BindingSource();
            bs.DataSource = dataGridView1.DataSource;
            bs.Filter = "pc like '%" + textBox1.Text + "%'";
            dataGridView1.DataSource = bs;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void btnPing_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell != null)
            {
                int rowindex = dataGridView1.CurrentCell.RowIndex;
                int columnindex = 1;
                IP =  dataGridView1.Rows[rowindex].Cells[columnindex].Value.ToString();
            }
            else
            {
                IP =  textBox1.Text;
            }

            richTextBox1.Text = "";

            foreach (var process in Process.GetProcessesByName("PING"))
            {
                process.Kill();
            }
            foreach (var process in Process.GetProcessesByName("conhost"))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception)
                {
                    
                }
                
            }

            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.DoWork += cmd_DoWork;
            
            if (bw.IsBusy)
                bw.CancelAsync();
            while (bw.IsBusy)
                System.Threading.Thread.Sleep(100);

            bw.RunWorkerAsync();

        }

        private void cmd_DoWork(object sender, DoWorkEventArgs e)
        {
            Process p = Process.Start(new ProcessStartInfo("ping") { Arguments = @" " + IP + " -n 30", RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true });
            if (p != null)
            {                
                p.OutputDataReceived += ((s, ev) =>
                {
                    string sData = ev.Data;
                    sData += "\r\n";

                    if (this.richTextBox1.InvokeRequired)
                    {
                        this.richTextBox1.BeginInvoke((MethodInvoker)delegate () {
                            Random r = new Random();
                            Color tempColor = new Color();
                            tempColor = richTextBox1.SelectionColor;
                            do
                            {
                                richTextBox1.SelectionColor = Color.FromArgb(255, r.Next(0, 255), r.Next(0, 255), r.Next(0, 255));
                            } while (richTextBox1.SelectionColor == tempColor);

                            this.richTextBox1.AppendText(sData); ; ;
                        });
                    }
                    else
                    {
                        Random r = new Random();
                        Color tempColor = new Color();
                        tempColor = richTextBox1.SelectionColor;
                        do
                        {
                            richTextBox1.SelectionColor = Color.FromArgb(255, r.Next(0, 255), r.Next(0, 255), r.Next(0, 255));
                        } while (richTextBox1.SelectionColor == tempColor);
                        this.richTextBox1.AppendText(sData);
                    }

                    System.Threading.Thread.Sleep(10);
                });
                p.BeginOutputReadLine();
            }
                       
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var process in Process.GetProcessesByName("PING"))
            {
                process.Kill();
            }
            foreach (var process in Process.GetProcessesByName("conhost"))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception)
                {

                }

            }
        }

        private void btnSaveList_Click(object sender, EventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                // Build the header using column names from the DataGridView
                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    sb.Append(column.HeaderText + ',');
                }
                sb.AppendLine();

                // Build the rows
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        // Convert cell.Value to string, handle DBNull.Value if necessary
                        string cellValue = (cell.Value ?? string.Empty).ToString();

                        // Replace any commas in cell value with a space (or handle differently if needed)
                        cellValue = cellValue.Replace(",", " ");

                        sb.Append(cellValue + ',');
                    }
                    sb.AppendLine();
                }

                // Write to file
                File.WriteAllText("All.csv", sb.ToString());
                MessageBox.Show("CSV file saved successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving CSV file: " + ex.Message);
            }
        }
    }
}
