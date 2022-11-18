using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Reflection;
using System.IO;

namespace AutoClose
{
	public partial class AutoClose : Form
	{
		public string apppath;
		public string appdir;
		public string killlistpath;
		private bool allowVisible = true;

		[DllImport("User32.dll")]
		private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

		internal struct LASTINPUTINFO
		{
			public uint cbSize;

			public uint dwTime;
		}

		public AutoClose()
		{
			apppath = Assembly.GetExecutingAssembly().Location;
			appdir = Path.GetDirectoryName(apppath);
			killlistpath = Path.Combine(appdir, "KillList.txt");
			InitializeComponent();
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == NativeMethods.WM_SHOWME)
			{
				ShowMe();
			}
			base.WndProc(ref m);
		}
		private void ShowMe()
		{
			Show();
			if (WindowState == FormWindowState.Minimized)
			{
				WindowState = FormWindowState.Normal;
			}
			// get our current "TopMost" value (ours will always be false though)
			bool top = TopMost;
			// make our form jump to the top of everything
			TopMost = true;
			// set it back to whatever it was
			TopMost = top;
		}

		private void SetVersionInfo()
		{
			Version versionInfo = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			DateTime startDate = new DateTime(2000, 1, 1);
			int diffDays = versionInfo.Build;
			DateTime computedDate = startDate.AddDays(diffDays);
			string lastBuilt = computedDate.ToShortDateString();
			//this.Text = string.Format("{0} - {1} ({2})",
			//            this.Text, versionInfo.ToString(), lastBuilt);
			this.Text = string.Format("{0} - {1}",
						this.Text, versionInfo.ToString());
		}

		protected override void SetVisibleCore(bool value)
		{
			if (!allowVisible)
			{
				value = false;
				if (!this.IsHandleCreated) CreateHandle();
			}
			base.SetVisibleCore(value);
		}

		public static uint GetIdleTime()
		{
			LASTINPUTINFO LastUserAction = new LASTINPUTINFO();
			LastUserAction.cbSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(LastUserAction);
			GetLastInputInfo(ref LastUserAction);
			return ((uint)Environment.TickCount - LastUserAction.dwTime);
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			SetVersionInfo();
			ReloadList();
			timer1.Start();
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			try
			{
				if (GetIdleTime() > Int32.Parse(txtInterval.Text) * 1000)
				{
					var processes = txtProgramList.Text.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var process in processes)
					{
						EndTask(process);
					}
				}
			}
			catch (Exception)
			{
				
			}
				
		}
		private static void EndTask(string taskname)
		{
			foreach (Process process in Process.GetProcessesByName(taskname))
			{
				process.Kill();
			}
		}
		private void ReloadList()
		{
			try
			{
				txtProgramList.Text = "";
				if (File.Exists(killlistpath))
				{
					txtProgramList.Text = File.ReadAllText(killlistpath);
				}
				else MessageBox.Show("KillList.txt not existed !");
			}
			catch (Exception)
			{

			}
			
		}

		private void btnSaveList_Click(object sender, EventArgs e)
		{
			try
			{
				File.WriteAllText(killlistpath, txtProgramList.Text);
				MessageBox.Show("Saved to " + killlistpath);
			}
			catch (Exception)
			{

			}
			
		}

		private void btnReloadList_Click(object sender, EventArgs e)
		{
			try
			{
				ReloadList();
				MessageBox.Show("Loaded from " + killlistpath);
			}
			catch (Exception)
			{

			}
			
		}

		private void AutoClose_Resize(object sender, EventArgs e)
		{
			if (chkHideTaskbarIcon.Checked == true && this.WindowState == FormWindowState.Minimized)
			{
				Hide();
				ShowInTaskbar = false;
			}
		}

		private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			allowVisible = true;
			Show();
			Activate();
			ShowInTaskbar = true;
			this.WindowState = FormWindowState.Normal;
		}

		private void exitToolStripMenuItem_Click_1(object sender, EventArgs e)
		{
			Application.Exit();
		}
	}
}
