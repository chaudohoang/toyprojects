using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RVSWorklog
{
	public partial class Admin : Form
	{
		public static string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
		public static string appdir = Path.GetDirectoryName(apppath);
		public string logPath;
		public string logDir;
		public string[] filePaths;
		public int lstDatePreviousIndex =-1;
		public Admin()
		{
			InitializeComponent();
		}

		private void btnRefresh_Click(object sender, EventArgs e)
		{
			ReloadFiles();
			RefreshDateWithFilter();
			RefreshWorkerWithFilter();
		}

		private void Admin_Load(object sender, EventArgs e)
		{
			ReloadFiles();
			RefreshDateWithFilter();

		}
		private void ReloadFiles()
		{
			logDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\RVSWorklog";
			filePaths = Directory.GetFiles(logDir, "*.txt",
										 SearchOption.TopDirectoryOnly);
		}

		private void txtDateSearch_TextChanged(object sender, EventArgs e)
		{
			RefreshDateWithFilter();
			RefreshWorkerWithFilter();
		}

		private void RefreshDateWithFilter()
		{
			txtWorklog.Text = "";
			lstDate.Items.Clear();
			foreach (string item in filePaths)
			{
				string date = Path.GetFileName(item).Split('_')[0];
				if (!lstDate.Items.Contains(date) && item.Contains(txtDateSearch.Text))
				{
					lstDate.Items.Add(date);
				}

			}
		}


		private void RefreshWorkerWithFilter()
		{
			txtWorklog.Text = "";
			lstWorker.Items.Clear();
			foreach (string item in filePaths)
			{
				string file = Path.GetFileName(item);
				if (lstDate.SelectedItem != null && file.Contains(lstDate.SelectedItem.ToString()) && file.Contains(txtWorkerSearch.Text))
				{
					file = file.Replace(".txt", "").Split('_')[1];
					lstWorker.Items.Add(file);
				}
			}
		}

		private void lstDate_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (lstDate.SelectedIndex != lstDatePreviousIndex || lstWorker.SelectedItem==null)
			{
				RefreshWorkerWithFilter();
			}
			lstDatePreviousIndex = lstDate.SelectedIndex;
		}

		private void lstWorker_SelectedIndexChanged(object sender, EventArgs e)
		{
			txtWorklog.Text = "";
			if (lstWorker.SelectedItem != null)
			{
				string file = Path.Combine(logDir, lstDate.SelectedItem.ToString()+"_"+lstWorker.SelectedItem.ToString()+".txt");
				txtWorklog.Text = File.ReadAllText(file);
				logPath = file;
			}
			
		}

		private void txtWorkerSearch_TextChanged(object sender, EventArgs e)
		{
			RefreshWorkerWithFilter();
		}

		private void btnExportAll_Click(object sender, EventArgs e)
		{
			string dummyFileName = "Save Here";

			SaveFileDialog sf = new SaveFileDialog();
			// Feed the dummy name to the save dialog
			sf.FileName = dummyFileName;
			sf.CheckFileExists = false;
			sf.Filter = "Directory | directory";

			if (sf.ShowDialog() == DialogResult.OK)
			{
				// Now here's our save folder
				string savePath = Path.GetDirectoryName(sf.FileName);
				foreach (string item in filePaths)
				{
					string dest = Path.Combine(savePath, Path.GetFileName(item));
					File.Copy(item, dest, true);

				}
				// Do whatever
			}

			
		}

		private void btnExportDay_Click(object sender, EventArgs e)
		{
			string dummyFileName = "Save Here";

			SaveFileDialog sf = new SaveFileDialog();
			// Feed the dummy name to the save dialog
			sf.FileName = dummyFileName;
			sf.CheckFileExists = false;
			sf.Filter = "Directory | directory";

			if (sf.ShowDialog() == DialogResult.OK)
			{
				// Now here's our save folder
				string savePath = Path.GetDirectoryName(sf.FileName);
				foreach (string item in filePaths)
				{
					string file = Path.GetFileName(item);
					if (lstDate.SelectedItem != null && file.Contains(lstDate.SelectedItem.ToString()) && file.Contains(txtWorkerSearch.Text))
					{
						string dest = Path.Combine(savePath, Path.GetFileName(item));
						File.Copy(item, dest, true);
					}
				}
				// Do whatever
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (txtWorklog.Text == "")
			{
				MessageBox.Show("Empty Log !!!");
			}
			else
			{
				SaveFileDialog savefile = new SaveFileDialog();
				// set a default file name
				savefile.FileName = Path.GetFileName(logPath);
				// set filters - this can be done in properties as well
				savefile.Filter = "Text files (*.txt)|*.txt";
				savefile.InitialDirectory = appdir;

				if (savefile.ShowDialog() == DialogResult.OK)
				{
					using (StreamWriter sw = new StreamWriter(savefile.FileName))
						sw.Write(txtWorklog.Text);
				}
			}
		}
	}
}
