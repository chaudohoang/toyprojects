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
	public partial class WriteLog : Form
	{
		public string log;
		public string date;
		public string datetime;
		public string line;
		public string station;
		public string chanel;
		public string type;
		public bool saved = false;
		public WriteLog()
		{
			InitializeComponent();
			lblToday.Text = DateTime.Now.ToString();
			txtLog.Text = "Things to do : " + Environment.NewLine + "Issue : " + Environment.NewLine + "Troubleshooting : " + Environment.NewLine + "Status : ";
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			saved = true;
			date = DateTime.Now.ToString("yyyyMMdd");
			datetime = lblToday.Text;
			log = txtLog.Text;
			line = cbxLine.Text;
			station = cbxStation.Text;
			chanel = cbxChanel.Text;
			type = cbxType.Text;
			this.Close();
		}

		private void Add_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Modifiers == Keys.Control && e.KeyCode == Keys.S)
				btnSave_Click(this, new EventArgs());
			if (e.KeyCode == Keys.Escape)
			{
				this.Close();
			}
		}

	}
}
