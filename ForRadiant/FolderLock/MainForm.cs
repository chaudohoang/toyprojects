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
using System.Security.AccessControl;
using System.Security.Principal;


namespace FolderLock
{
	public partial class MainForm : Form
	{
		public string apppath;
		public string appdir;
		public string password = "";
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

		private void TemplateApp_Resize(object sender, EventArgs e)
		{
			if (minimizedToTrayToolStripMenuItem.Checked == true && this.WindowState == FormWindowState.Minimized)
			{
				Hide();
			}
		}

		private void TemplateApp_Load(object sender, EventArgs e)
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

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                comboBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void bntLock_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(comboBox1.Text))
            {
                MessageBox.Show("Path not existed !");
                return;
            }
            try
			{
				string folderPath = comboBox1.Text;
				string adminUserName = Environment.UserName;// getting your adminUserName

				SecurityIdentifier cu = WindowsIdentity.GetCurrent().User;
				DirectorySecurity ds = Directory.GetAccessControl(folderPath);

				ds.SetOwner(cu);
				FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);
				ds.AddAccessRule(fsa);
				Directory.SetAccessControl(folderPath, ds);
				MessageBox.Show("Locked");
			}
			catch (UnauthorizedAccessException)
			{
                MessageBox.Show("Already Locked");
            }
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
        }

        private void btnUnlock_Click(object sender, EventArgs e)
        {
			if (!Directory.Exists(comboBox1.Text))
			{
                MessageBox.Show("Path not existed !");
				return;
            }
			bool alreadyUnlocked = true;
			try
			{
                string folderPath = comboBox1.Text;
                string adminUserName = Environment.UserName;// getting your adminUserName
                DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);
				AuthorizationRuleCollection fSecurity = ds.GetAccessRules(true, true, Type.GetType("System.Security.Principal.NTAccount"));
				foreach (AccessRule myacc in fSecurity)
				{
					if (myacc.AccessControlType==AccessControlType.Deny)
					{
						alreadyUnlocked = false;
						break;
					}
				}
            }
			catch (Exception ex)
			{
                MessageBox.Show(ex.Message);
            }

			if (alreadyUnlocked)
			{
                MessageBox.Show("Already UnLocked");
            }
			else
			{
				Password passwordForm = new Password();
				passwordForm.ShowDialog();
				password = passwordForm.password;
				if (password.ToLower() == "rvskorea1234")
				{
					try
					{
						string folderPath = comboBox1.Text;
						string adminUserName = Environment.UserName;// getting your adminUserName
						DirectorySecurity ds = Directory.GetAccessControl(folderPath);
						FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);

						ds.RemoveAccessRule(fsa);
						Directory.SetAccessControl(folderPath, ds);
						MessageBox.Show("UnLocked");
						password = "";
					}
					catch (Exception ex)
					{
						MessageBox.Show(ex.Message);
					}
				}
				else if (password == "")
				{

				}
				else MessageBox.Show("Incorrect password, try again !");
            }            
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void bntLock2_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox2.Text))
            {
                MessageBox.Show("Path not existed !");
                return;
            }
            try
            {
                string folderPath = textBox2.Text;
                string adminUserName = Environment.UserName;// getting your adminUserName

                SecurityIdentifier cu = WindowsIdentity.GetCurrent().User;
                DirectorySecurity ds = Directory.GetAccessControl(folderPath);

                ds.SetOwner(cu);
                FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);
                ds.AddAccessRule(fsa);
                Directory.SetAccessControl(folderPath, ds);
                MessageBox.Show("Locked");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Already Locked");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUnlock2_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox2.Text))
            {
                MessageBox.Show("Path not existed !");
                return;
            }
            bool alreadyUnlocked = true;
            try
            {
                string folderPath = textBox2.Text;
                string adminUserName = Environment.UserName;// getting your adminUserName
                DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);
                AuthorizationRuleCollection fSecurity = ds.GetAccessRules(true, true, Type.GetType("System.Security.Principal.NTAccount"));
                foreach (AccessRule myacc in fSecurity)
                {
                    if (myacc.AccessControlType == AccessControlType.Deny)
                    {
                        alreadyUnlocked = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (alreadyUnlocked)
            {
                MessageBox.Show("Already UnLocked");
            }
            else
            {
                Password passwordForm = new Password();
                passwordForm.ShowDialog();
                password = passwordForm.password;
                if (password.ToLower() == "rvskorea1234")
                {
                    try
                    {
                        string folderPath = textBox2.Text;
                        string adminUserName = Environment.UserName;// getting your adminUserName
                        DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                        FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);

                        ds.RemoveAccessRule(fsa);
                        Directory.SetAccessControl(folderPath, ds);
                        MessageBox.Show("UnLocked");
                        password = "";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else if (password == "")
                {

                }
                else MessageBox.Show("Incorrect password, try again !");
            }
        }

        private void btnBrowse3_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void bntLock3_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox3.Text))
            {
                MessageBox.Show("Path not existed !");
                return;
            }
            try
            {
                string folderPath = textBox3.Text;
                string adminUserName = Environment.UserName;// getting your adminUserName

                SecurityIdentifier cu = WindowsIdentity.GetCurrent().User;
                DirectorySecurity ds = Directory.GetAccessControl(folderPath);

                ds.SetOwner(cu);
                FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);
                ds.AddAccessRule(fsa);
                Directory.SetAccessControl(folderPath, ds);
                MessageBox.Show("Locked");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Already Locked");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUnlock3_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox3.Text))
            {
                MessageBox.Show("Path not existed !");
                return;
            }
            bool alreadyUnlocked = true;
            try
            {
                string folderPath = textBox3.Text;
                string adminUserName = Environment.UserName;// getting your adminUserName
                DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);
                AuthorizationRuleCollection fSecurity = ds.GetAccessRules(true, true, Type.GetType("System.Security.Principal.NTAccount"));
                foreach (AccessRule myacc in fSecurity)
                {
                    if (myacc.AccessControlType == AccessControlType.Deny)
                    {
                        alreadyUnlocked = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (alreadyUnlocked)
            {
                MessageBox.Show("Already UnLocked");
            }
            else
            {
                Password passwordForm = new Password();
                passwordForm.ShowDialog();
                password = passwordForm.password;
                if (password.ToLower() == "rvskorea1234")
                {
                    try
                    {
                        string folderPath = textBox3.Text;
                        string adminUserName = Environment.UserName;// getting your adminUserName
                        DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                        FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);

                        ds.RemoveAccessRule(fsa);
                        Directory.SetAccessControl(folderPath, ds);
                        MessageBox.Show("UnLocked");
                        password = "";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else if (password == "")
                {

                }
                else MessageBox.Show("Incorrect password, try again !");
            }
        }

        private void btnBrowse4_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBox4.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void bntLock4_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox4.Text))
            {
                MessageBox.Show("Path not existed !");
                return;
            }
            try
            {
                string folderPath = textBox4.Text;
                string adminUserName = Environment.UserName;// getting your adminUserName

                SecurityIdentifier cu = WindowsIdentity.GetCurrent().User;
                DirectorySecurity ds = Directory.GetAccessControl(folderPath);

                ds.SetOwner(cu);
                FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);
                ds.AddAccessRule(fsa);
                Directory.SetAccessControl(folderPath, ds);
                MessageBox.Show("Locked");
            }
            catch (UnauthorizedAccessException)
            {
                MessageBox.Show("Already Locked");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUnlock4_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox4.Text))
            {
                MessageBox.Show("Path not existed !");
                return;
            }
            bool alreadyUnlocked = true;
            try
            {
                string folderPath = textBox4.Text;
                string adminUserName = Environment.UserName;// getting your adminUserName
                DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);
                AuthorizationRuleCollection fSecurity = ds.GetAccessRules(true, true, Type.GetType("System.Security.Principal.NTAccount"));
                foreach (AccessRule myacc in fSecurity)
                {
                    if (myacc.AccessControlType == AccessControlType.Deny)
                    {
                        alreadyUnlocked = false;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            if (alreadyUnlocked)
            {
                MessageBox.Show("Already UnLocked");
            }
            else
            {
                Password passwordForm = new Password();
                passwordForm.ShowDialog();
                password = passwordForm.password;
                if (password.ToLower() == "rvskorea1234")
                {
                    try
                    {
                        string folderPath = textBox4.Text;
                        string adminUserName = Environment.UserName;// getting your adminUserName
                        DirectorySecurity ds = Directory.GetAccessControl(folderPath);
                        FileSystemAccessRule fsa = new FileSystemAccessRule(adminUserName, FileSystemRights.FullControl, AccessControlType.Deny);

                        ds.RemoveAccessRule(fsa);
                        Directory.SetAccessControl(folderPath, ds);
                        MessageBox.Show("UnLocked");
                        password = "";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                else if (password == "")
                {

                }
                else MessageBox.Show("Incorrect password, try again !");
            }
        }
    }
}
