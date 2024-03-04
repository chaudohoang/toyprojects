using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAccountSwitch
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
            string CSVFilePathName = @"accounts.csv";
            string[] Lines = File.ReadAllLines(CSVFilePathName);
            string[] Fields;
			//Fields = Lines[0].Split(new char[] { ',' });
			Fields = "Description,ID,Password".Split(new char[] { ',' });
			int Cols = Fields.GetLength(0);
            DataTable dt = new DataTable();

            //1st row must be column names; force lower case to ensure matching later on.
            for (int i = 0; i < Cols; i++)
                dt.Columns.Add(Fields[i], typeof(string));
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

		private void btnSave_Click(object sender, EventArgs e)
		{
            System.IO.StreamWriter strWri = new System.IO.StreamWriter("accounts.csv");

            for (int i = 0; i < dataGridView1.Rows.Count-1; i++)
            {
                string strRowVal = "";
                for (int j = 0; j < dataGridView1.Rows[i].Cells.Count; j++)
                {
                    if (strRowVal == "")
                    {
                        strRowVal = dataGridView1.Rows[i].Cells[j].Value.ToString();
                    }
                    else
                    {
                        strRowVal = strRowVal + "," + dataGridView1.Rows[i].Cells[j].Value.ToString();
                    }
                }
                strWri.WriteLine(strRowVal);
            }
            strWri.Close();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string ID, Password;
            ProcessStartInfo startinfo = new ProcessStartInfo();
            string steamExePath = txtSteamExePath.Text.Trim(); // Trim to remove leading and trailing whitespaces
            if (!string.IsNullOrEmpty(steamExePath))
            {
                if (dataGridView1.CurrentCell != null)
                {
                    int rowindex = dataGridView1.CurrentCell.RowIndex;
                    ID = dataGridView1.Rows[rowindex].Cells[1].Value.ToString();
                    Password = dataGridView1.Rows[rowindex].Cells[2].Value.ToString();
                    startinfo.FileName = "cmd.exe";
                    startinfo.Arguments = "/c taskkill.exe /F /IM steam.exe && start \"\" \"" + steamExePath + "\" -noreactlogin -login " + ID + " " + Password + " && timeout 2 >nul && move nul 2>&0";
                }

                try
                {
                    Process.Start(startinfo);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Please provide the path to steam.exe.");
            }
        }


        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
		{
            if (e.ColumnIndex == 2 && e.Value != null)
            {
                e.Value = new String('*', e.Value.ToString().Length);
            }
        }
	}
}
