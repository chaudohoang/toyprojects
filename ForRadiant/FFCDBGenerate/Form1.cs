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
        private void btnGenerate_Click(object sender, EventArgs e)
        { 
            string line = cbLine.Text;
            string station = cbStation.Text;
            string channel = cbChannel.Text;
            string output = cbSaveTo.Text;
            bool openoutput = chkOpenOutput.Checked;

            try
            {
                output = output + "\\" + System.DateTime.Now.ToString("yyyyMMdd ") + line + "#" + station + "-" + channel + " Panel FFC";
                if (!System.IO.Directory.Exists(output))
                {
                    System.IO.Directory.CreateDirectory(output);
                }
                for (int i = 1; i <= 5; i++)
                {
                    string dbfilename = station + "-" + channel + "-" + i+ ".ttxm";
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
    }
}
