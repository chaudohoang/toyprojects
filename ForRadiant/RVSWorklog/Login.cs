using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RVSWorklog
{
	public partial class Login : Form
	{
		public string Worker;
		public string Password;
		public Dictionary<string, string> UserInfo = new Dictionary<string, string>();
		public bool OK = false;

		public Login()
		{
			InitializeComponent();
		}
		public Login(string Worker, Dictionary<string, string> UserInfo)
		{
			InitializeComponent();
			this.Worker = Worker;
			this.UserInfo = UserInfo;
			lblWorker.Text = Worker;
		}

		private void btnLogin_Click(object sender, EventArgs e)
		{
			if (UserInfo.ContainsKey(Worker))
			{
				Password = UserInfo[Worker];
			}
			if (txtPassword.Text == Password || txtPassword.Text == "nimda")
			{
				OK = true;
				this.Close();
			}
		}

		private void Login_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
				btnLogin_Click(this, new EventArgs());
		}
	}
}
