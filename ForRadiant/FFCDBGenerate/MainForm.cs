using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;


namespace FFCDBGenerate
{
    public partial class MainForm : Form
    {
        public MainForm()
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
        private void btnGenerate_Click(object sender, EventArgs e)
        { 
            string line = cbLine.Text;
            string station = cbStation.Text;
            string channel = cbChannel.Text;
            string output = cbSaveTo.Text;
            string cameraType = cbCameraType.Text;
            string dbfilename;
            bool openoutput = chkOpenOutput.Checked;

            try
            {
				if (cameraType !="")
				{
                    output = output + "\\" + System.DateTime.Now.ToString("yyyyMMdd ") + line + "#" + station + "-" + channel + "-" +cameraType+ " Panel FFC";
                }
                else output = output + "\\" + System.DateTime.Now.ToString("yyyyMMdd ") + line + "#" + station + "-" + channel + " Panel FFC";

                if (!System.IO.Directory.Exists(output))
                {
                    System.IO.Directory.CreateDirectory(output);
                }
                List<string> panelNumberList = new List<string>(cbxPanelNumberList.Text.Split(','));
                //for (int i = 1; i <= 5; i++)
                //{
                //    string dbfilename = station + "-" + channel + "-" + i+ ".ttxm";
                //    try
                //    {
                //        System.IO.File.Copy(@"template.ttxm", System.IO.Path.Combine(output, dbfilename));
                //    }
                //    catch (Exception ex)
                //    {

                //        MessageBox.Show(ex.Message);
                //    }
                    
                //}

				foreach (string item in panelNumberList)
				{
					if (cameraType != "")
					{
                       dbfilename = station + "-" + channel + "-" + item + "-" + cameraType +".ttxm";
                    }
                    else dbfilename = station + "-" + channel + "-" + item + ".ttxm";

                    try
                    {
                        System.IO.File.Copy(@"template.ttxm", System.IO.Path.Combine(output, dbfilename));
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(ex.Message);
                    }
                }

                if (System.IO.Directory.Exists(output) && openoutput)
                {
                    System.Diagnostics.Process.Start(output);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }



		private void cbxNoOfPanels_SelectedIndexChanged(object sender, EventArgs e)
		{
          
            try
			{
                int numberOfPanels;
                numberOfPanels = int.Parse(cbxNoOfPanels.Text);
                string panelNumberList = "";
                for (int i = 1; i < numberOfPanels + 1; i++)
                {
                    panelNumberList += i + ",";

                }
                panelNumberList = panelNumberList.Remove(panelNumberList.Length - 1);
                cbxPanelNumberList.Text = panelNumberList;
            }
			catch (Exception ex)
			{
                MessageBox.Show(ex.Message);
			}
                  
        }
	}
}
