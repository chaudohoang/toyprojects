using Microsoft.VisualBasic.FileIO;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;
using System.Reflection;

namespace RemoteTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
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
            var path = @"All.csv";
            using (TextFieldParser csvParser = new TextFieldParser(path))
            {
                csvParser.CommentTokens = new string[] { "#" };
                csvParser.SetDelimiters(new string[] { "," });
                csvParser.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvParser.ReadLine();

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    string[] fields = csvParser.ReadFields();
                    string PC = fields[0];
                    string IP = fields[1];
                    dataGridView1.Rows.Add(PC, IP);
                }
            }

        }
        private void btnFileShare_Click(object sender, EventArgs e)
        {
            int rowindex = dataGridView1.CurrentCell.RowIndex;
            int columnindex = 1;

            string IP = dataGridView1.Rows[rowindex].Cells[columnindex].Value.ToString();
            if (textBox1.Text != "")
            {
                try
                {
                    Process.Start("\\\\" + textBox1.Text);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }
            else
            { 
                Process.Start("\\\\"+IP);
            }
        }

        private void btnRemoteControl_Click(object sender, EventArgs e)
        {
            int rowindex = dataGridView1.CurrentCell.RowIndex;
            int columnindex = 1;


            string IP = dataGridView1.Rows[rowindex].Cells[columnindex].Value.ToString();
            if (textBox1.Text != "")
            {
                try
                {
                    Process.Start(@"tvnviewer.exe", textBox1.Text);
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                Process.Start(@"tvnviewer.exe", IP);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
            BindingSource bs = new BindingSource();
            bs.DataSource = dataGridView1.DataSource;
            bs.Filter = string.Format("CONVERT(" + dataGridView1.Columns[1].DataPropertyName + ", System.String) like '%" + textBox1.Text.Replace("'", "''") + "%'");
            dataGridView1.DataSource = bs;
            dataGridView1.Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }
    }
}
