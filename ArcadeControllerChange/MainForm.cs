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

namespace ArcadeControllerChange
{
	public partial class MainForm : Form
	{
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

		private void ArcadeControllerChange_Resize(object sender, EventArgs e)
		{
			if (minimizedToTrayToolStripMenuItem.Checked == true && this.WindowState == FormWindowState.Minimized)
			{
				Hide();
			}
		}

		private void ArcadeControllerChange_Load(object sender, EventArgs e)
		{
			SetVersionInfo();
		}
		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}
		private void exit2ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

        private void button1_Click(object sender, EventArgs e)
        {
			if (textBox1.Text !="")
			{
                string[] lines = textBox1.Lines;
                for (int i = 0; i <= lines.GetUpperBound(0); i++)
                {
                    lines[i] = lines[i].Replace("\"", "");
                    if (lines[i]!="" && File.Exists(lines[i]))
					{
                        string text = File.ReadAllText(lines[i]);
                        string outputFile = lines[i].Replace("default-template", "default");

                        if (comboBox1.Text != "" && comboBox2.Text !="")
						{
                            text = text.Replace(comboBox1.Text, comboBox2.Text);
                        }
                        if (comboBox3.Text != "" && comboBox4.Text != "")
                        {
                            text = text.Replace(comboBox3.Text, comboBox4.Text);
                        }
                        if (comboBox5.Text != "" && comboBox6.Text != "")
                        {
                            text = text.Replace(comboBox5.Text, comboBox6.Text);
                        }
                        if (comboBox7.Text != "" && comboBox8.Text != "")
                        {
                            text = text.Replace(comboBox7.Text, comboBox8.Text);
                        }
						if (File.Exists(outputFile))
						{
                            File.SetAttributes(outputFile, FileAttributes.Normal);
                            File.Delete(outputFile);
						}
                        File.WriteAllText(outputFile, text);
                        File.SetAttributes(outputFile, FileAttributes.ReadOnly);
                    }
                }
            }
        }
    }
}
