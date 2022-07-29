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
		public MainForm()
		{
			InitializeComponent();
			UserInfo.Add("Hwang JunSeok", "joon");
			UserInfo.Add("Kim Kang Hyun", "ryan");
			UserInfo.Add("Kwon Kwang Ho", "miguel");
			UserInfo.Add("Kim Sun Do", "sun");
			UserInfo.Add("Yoon Sung Mo", "jimmy");
			UserInfo.Add("Kwon Hyeon Tae", "ray");
			UserInfo.Add("Gwon Hyuk Lyong", "ethan");
			UserInfo.Add("Kim SuRyong", "victor");
			UserInfo.Add("Min KyeongWon", "kyle");
			UserInfo.Add("Đỗ Hoàng Châu", "charlie");
			UserInfo.Add("Đào Như Quyền", "power");
			UserInfo.Add("Lê Hoàng Minh", "lucas");
			UserInfo.Add("Nguyễn Huy Hoàng", "andy");
			UserInfo.Add("Nguyễn Sơn Tùng", "tom");
			UserInfo.Add("Nguyễn Tiến Tùng", "stephen");
			UserInfo.Add("Phan Thanh Trung", "junie");
			UserInfo.Add("Nguyễn Quốc Trọng", "thomas");
			UserInfo.Add("Nguyễn Văn Hải", "tony");
			UserInfo.Add("Phạm Văn Tâm", "jason");
			UserInfo.Add("Vũ Công Hùng", "roy");
			UserInfo.Add("Nguyễn Khánh Tùng", "jaden");
			UserInfo.Add("Nguyễn Việt Thiện", "ken");
			UserInfo.Add("Trần Quang Ngọc", "peter");
			UserInfo.Add("Bùi Hải Phong", "joe");
			UserInfo.Add("Bùi Duy Kiên", "erik");
			UserInfo.Add("Đoàn Xuân Hiệp", "max");
		}


		private void cbxWorker_SelectedIndexChanged(object sender, EventArgs e)
		{
			Login login = new Login(cbxWorker.Text, UserInfo);
			login.ShowDialog();
			if (login.OK)
			{
				logPath = Path.Combine(appdir + "\\log", DateTime.Now.ToString("yyyyMMdd")+"_" +cbxWorker.Text + ".txt");
				txtWorklog.Text = "";
				if (File.Exists(logPath))
				{
					txtWorklog.Text = File.ReadAllText(logPath);
				}				
				btnAdd.Enabled = true;
			}
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			Add add = new Add();
			add.ShowDialog();
			if (add.saved)
			{
				logPath = Path.Combine(appdir + "\\log", add.date+ "_" + cbxWorker.Text + ".txt");
				txtWorklog.Text = "";
				if (!File.Exists(logPath))
				{
					// Create a file to write to.
					Directory.CreateDirectory(appdir + "\\log");
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

		private void MainForm_Load(object sender, EventArgs e)
		{
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

		private void dateFilter_CloseUp(object sender, EventArgs e)
		{
			logPath = Path.Combine(appdir + "\\log", dateFilter.Value.ToString("yyyyMMdd") + "_" + cbxWorker.Text + ".txt");
			txtWorklog.Text = "";
			if (File.Exists(logPath))
			{
				txtWorklog.Text = File.ReadAllText(logPath);
			}
		}
	}
}
