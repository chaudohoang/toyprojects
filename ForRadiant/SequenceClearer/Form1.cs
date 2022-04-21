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
using System.Xml;

namespace SequenceCleaner
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

		private void cmdBrowseSequence_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.InitialDirectory = @"C:\Radiant Vision Systems Data\TrueTest\Sequence";
				if (dialog.ShowDialog(this) == DialogResult.OK)
				{
					txtSequenceFilePath.Text = dialog.FileName;
				}
			}
		}

		private void btnClean_Click(object sender, EventArgs e)
		{
			if (chkBackup.Checked)
			{
				string path = System.IO.Path.GetDirectoryName(txtSequenceFilePath.Text);
				string date = System.DateTime.Now.ToString("yyyyMMdd");
				string backupfilename = System.IO.Path.GetFileNameWithoutExtension(txtSequenceFilePath.Text) + "_" + date + ".seqxc" ;
				string backfupfilepath = System.IO.Path.Combine(path, backupfilename);
				if (!System.IO.File.Exists(backfupfilepath))
				{
					System.IO.File.Copy(txtSequenceFilePath.Text, backfupfilepath);
					MessageBox.Show("Sequence backed up to : " + backfupfilepath);
				}				
			}

            XmlNode node;
            XmlNodeList nodes;

            var xmlDoc = new XmlDocument();
            if (File.Exists(txtSequenceFilePath.Text))
            {
                xmlDoc.Load(txtSequenceFilePath.Text);

				if (chkClearCCD.Checked)
				{
					nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem/Analysis/RemoveCCDLines");
					for (int index = 0; index <= nodes.Count - 1; index++)
					{
						nodes[index].InnerText = "";
					}
				}

				if (chkClearCosineCorrection.Checked)
				{
					nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem/Analysis/CosineCorrection");
					for (int index = 0; index <= nodes.Count - 1; index++)
					{
						nodes[index].InnerText = "";
					}
				}

				if (chkClearMaskBitmap.Checked)
				{
					nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem/Analysis/BitmapMaskFileName");
					for (int index = 0; index <= nodes.Count - 1; index++)
					{
						nodes[index].InnerText = "";
					}
				}

				if (chkSaveToDBFalse.Checked)
				{
					nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem/Analysis/SaveToDatabase");
					for (int index = 0; index <= nodes.Count - 1; index++)
					{
						nodes[index].InnerText = "False";
					}
				}          

				XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                XmlWriter writer = XmlWriter.Create(txtSequenceFilePath.Text, settings);
                xmlDoc.Save(writer);
                if (writer != null)
                    writer.Close();
			
			Random m_Rnd = new Random();
			Color tempcolor;
			tempcolor = lblDone.ForeColor;
			while (lblDone.ForeColor == tempcolor)
			{
				lblDone.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255));
			}
			lblDone.Text = "Done!";
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			SetVersionInfo();
		}
	}
}
