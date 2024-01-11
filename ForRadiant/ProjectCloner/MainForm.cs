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

namespace ProjectCloner
{
	public partial class MainForm : Form
	{
        private readonly FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

        public string apppath;
		public string appdir;
		private bool allowVisible = true;

		[DllImport("User32.dll")]
		private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

		internal struct LASTINPUTINFO
		{
			public uint cbSize;

			public uint dwTime;
		}

		public MainForm()
		{
			apppath = Assembly.GetExecutingAssembly().Location;
			appdir = Path.GetDirectoryName(apppath);
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

		private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			allowVisible = true;
			Show();		
			Activate();
			this.WindowState = FormWindowState.Normal;
		}

		private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show("dh.chau@radiantvs.com");
		}

		private void ProjectCloner_Resize(object sender, EventArgs e)
		{
			if (minimizedToTrayToolStripMenuItem.Checked == true && this.WindowState == FormWindowState.Minimized)
			{
				Hide();
			}
		}

		private void ProjectCloner_Load(object sender, EventArgs e)
		{
			SetVersionInfo();
        }

        private void TextBox_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void TextBox_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                ((TextBox)sender).Text = files.FirstOrDefault();
            }
        }

        private void browseInputFolderButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                cppConsoleInputFolderTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void browseOriginalFolderButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                cppConsoleOriginalFolderTextBox.Text = folderBrowserDialog.SelectedPath;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
		private void exit2ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

        private void runButton_Click(object sender, EventArgs e)
        {
            // Call the method to clone the C++ project
			string result = CloneCppProject(cppConsoleInputFolderTextBox.Text, cppConsoleOriginalFolderTextBox.Text);

            MessageBox.Show("Successfully cloned C++ project to : " + result);
        }

        private string CloneCppProject(string inputPath, string originalPath)
        {
            // Call the original CloneCppProject method with the provided paths
			return CloneCppConsoleProject.CloneCppProject(originalPath, inputPath);
        }
    }
}
