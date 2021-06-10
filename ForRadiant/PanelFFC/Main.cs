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


namespace PanelFFC
{
    public partial class Main : Form
    {
        public Main()
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

        private void btnIllunisCF_Click(object sender, EventArgs e)
        {
            Form illuniscf = new PanelFFCIllunisCF();
            illuniscf.Show();
        }

        private void btnIllunisDJ_Click(object sender, EventArgs e)
        {
            Form illunisdj = new PanelFFCIllunisDJ();
            illunisdj.Show();
        }

        private void btnIllunisEP_Click(object sender, EventArgs e)
        {
            Form illunisep = new PanelFFCIllunisEP();
            illunisep.Show();
        }

        private void btnRadiantCF_Click(object sender, EventArgs e)
        {
            Form radiantcf = new PanelFFCRadiantCF();
            radiantcf.Show();
        }

        private void btnX10801panel_Click(object sender, EventArgs e)
        {
            Form x1080_1 = new PanelFFCX10801panel();
            x1080_1.Show();
        }

        private void btnX10805panel_Click(object sender, EventArgs e)
        {
            Form x1080_5 = new PanelFFCX10805panel();
            x1080_5.Show();
        }

        private void btnRgbFFC_Click(object sender, EventArgs e)
        {
            Form rgbffc = new PanelFFCRGB();
            rgbffc.Show();
        }

        private void btnGrayFFC_Click(object sender, EventArgs e)
        {
            Form grayffc = new PanelFFCGRAY();
            grayffc.Show();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }
    }
}
