using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RVSWorklog
{
	public partial class MainForm : Form
	{
		public Dictionary<string, string> UserInfo = new Dictionary<string, string>();
		public static string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
		public static string appdir = Path.GetDirectoryName(apppath);
		public string logPath;
		public string logDir;
		public string[] filePaths;
		public int lstDatePreviousIndex = -1;
		public Admin admin = null;
		public MainForm()
		{
			InitializeComponent();
			UserInfo.Add("admin", "nimda");
			UserInfo.Add("Hwang JunSeok", "joon9");
			UserInfo.Add("Kim Kang Hyun", "ryan1");
			UserInfo.Add("Kwon Kwang Ho", "miguel10");
			UserInfo.Add("Kim Sun Do", "sun24");
			UserInfo.Add("Yoon Sung Mo", "jimmy2");
			UserInfo.Add("Kwon Hyeon Tae", "ray23");
			UserInfo.Add("Gwon Hyuk Lyong", "ethan11");
			UserInfo.Add("Kim SuRyong", "victor25");
			UserInfo.Add("Min KyeongWon", "kyle3");
			UserInfo.Add("Đỗ Hoàng Châu", "charlie12");
			UserInfo.Add("Đào Như Quyền", "power22");
			UserInfo.Add("Lê Hoàng Minh", "lucas13");
			UserInfo.Add("Nguyễn Huy Hoàng", "andy4");
			UserInfo.Add("Nguyễn Sơn Tùng", "tom21");
			UserInfo.Add("Nguyễn Tiến Tùng", "stephen14");
			UserInfo.Add("Phan Thanh Trung", "junie20");
			UserInfo.Add("Nguyễn Quốc Trọng", "thomas5");
			UserInfo.Add("Nguyễn Văn Hải", "tony15");
			UserInfo.Add("Phạm Văn Tâm", "jason19");
			UserInfo.Add("Vũ Công Hùng", "roy6");
			UserInfo.Add("Nguyễn Khánh Tùng", "jaden16");
			UserInfo.Add("Nguyễn Việt Thiện", "ken7");
			UserInfo.Add("Trần Quang Ngọc", "peter18");
			UserInfo.Add("Bùi Hải Phong", "joe17");
			UserInfo.Add("Bùi Duy Kiên", "erik8");
			UserInfo.Add("Đoàn Xuân Hiệp", "max26");
		}


		private void cbxWorker_SelectedIndexChanged(object sender, EventArgs e)
		{
			Login login = new Login(cbxWorker.Text, UserInfo);
			login.ShowDialog();
			if (login.OK)
			{
				lblStatus.Text = cbxWorker.Text + " logged in.";
				//logPath = Path.Combine(appdir + "\\log", DateTime.Now.ToString("yyyyMMdd")+"_" +cbxWorker.Text + ".txt");
				logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\RVSWorklog", DateTime.Now.ToString("yyyyMMdd") + "_" + cbxWorker.Text + ".txt");
				txtWorklog.Text = "";
				if (File.Exists(logPath))
				{					
					txtWorklog.Text = File.ReadAllText(logPath);
				}
				RefreshDateWithFilter();
				string date = Path.GetFileName(logPath).Split('_')[0];
				int dateIndex = lstDate.Items.IndexOf(date);
                if (dateIndex != -1)
                {
					lstDate.SetSelected(dateIndex, true);
				}				
				btnAdd.Enabled = true;
				btnExportAll.Enabled = true;
				btnExportCurrent.Enabled = true;
				txtDateSearch.ReadOnly = false;
				lstDate.Enabled = true;
			}
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			Add add = new Add();
			add.ShowDialog();
			if (add.saved)
			{
				//logPath = Path.Combine(appdir + "\\log", add.date+ "_" + cbxWorker.Text + ".txt");
				logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\RVSWorklog", add.date + "_" + cbxWorker.Text + ".txt");
				txtWorklog.Text = "";
				if (!File.Exists(logPath))
				{
					// Create a file to write to.
					//Directory.CreateDirectory(appdir + "\\log");
					Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\RVSWorklog");
					using (StreamWriter sw = File.CreateText(logPath))
					{

					}

				}		

				File.AppendAllText(logPath, add.datetime + " " + (add.line != "" ? add.line + "#" : "") + (add.station != "" ? add.station + "-" : "") + (add.chanel != "" ? add.chanel + "-" : "") + (add.type != "" ? add.type : ""));
				File.AppendAllText(logPath, Environment.NewLine + "Worker : " + cbxWorker.Text);
				File.AppendAllText(logPath, Environment.NewLine + add.log);
				File.AppendAllText(logPath, Environment.NewLine);
				File.AppendAllText(logPath, Environment.NewLine);

				txtWorklog.Text = File.ReadAllText(logPath);
			}
		}
		private void MainForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				btnAdd_Click(this, new EventArgs());
		}

		private void txtWorklog_TextChanged(object sender, EventArgs e)
		{
			txtWorklog.SelectionStart = txtWorklog.Text.Length;
			txtWorklog.ScrollToCaret();
			txtWorklog.Refresh();
		}

		private void ReloadFiles()
		{
			logDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\RVSWorklog";
            if (!Directory.Exists(logDir))
            {
				Directory.CreateDirectory(logDir);
            }
			filePaths = Directory.GetFiles(logDir, "*.txt",
										 SearchOption.TopDirectoryOnly);
		}
		private void MainForm_Load(object sender, EventArgs e)
		{
			lblToday.Text = DateTime.Now.ToShortDateString();
			ReloadFiles();
			SetVersionInfo();
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

		private void btnAdmin_Click(object sender, EventArgs e)
		{
			Login login = new Login("Admin", UserInfo);
			login.ShowDialog();
			
			if (login.OK)
			{

				if (admin == null)
				{
					admin = new Admin();
					admin.FormClosed += delegate { admin = null; };
					admin.Show();
				}
				else
				{
					admin.Focus();
				}
				

			}
		}

		private void btnExportCurrent_Click(object sender, EventArgs e)
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

		private void btnExportAll_Click(object sender, EventArgs e)
		{
			ReloadFiles();
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
					if (item.Contains(cbxWorker.Text))
					{
						string dest = Path.Combine(savePath, Path.GetFileName(item));
						File.Copy(item, dest, true);
					}			

				}
				// Do whatever
			}
		}

		private void RefreshDateWithFilter()
        {
			lstDate.Items.Clear();
			foreach (string item in filePaths)
			{
				string date = Path.GetFileName(item).Split('_')[0];
				if (!lstDate.Items.Contains(date) && item.Contains(txtDateSearch.Text) && item.Contains(cbxWorker.Text))
				{
					lstDate.Items.Add(date);
				}

			}
		}
		private void RefreshLogtWithDate()
        {
			txtWorklog.Text = "";
			foreach (string item in filePaths)
			{
				if (File.Exists(item) && lstDate.SelectedItem != null && item.Contains(lstDate.SelectedItem.ToString()) && item.Contains(cbxWorker.Text) )
				{
					txtWorklog.Text = File.ReadAllText(item);
				}
			}
		}
        private void txtDateSearch_TextChanged(object sender, EventArgs e)
        {
			RefreshDateWithFilter();
			RefreshLogtWithDate();
		}

        private void lstDate_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (lstDate.SelectedIndex != lstDatePreviousIndex || txtWorklog.Text =="")
			{
				RefreshLogtWithDate();
			}
			lstDatePreviousIndex = lstDate.SelectedIndex;
		}
    }
}
