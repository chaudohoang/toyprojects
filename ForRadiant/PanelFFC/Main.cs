using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PanelFFC
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
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
    }
}
