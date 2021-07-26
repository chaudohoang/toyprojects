using Microsoft.VisualBasic.FileIO;
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using System.IO;
using System.Data.OleDb;
using System.Globalization;

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
            for (int i = 1; i < Lines.GetLength(0); i++)
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
            string IP;
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
            string IP;
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


    }
}
