using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FTPIndexPathGenerator
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
        private void button1_Click(object sender, EventArgs e)
        {
            int nSeed = 7919; // nSeed = 7919
            int LayerSize;
            int LayerCount = 1;
            string PID = textBox1.Text;
            int nVlue = 0;
            long lTmp;
            string path;
            char[] cPanlID = PID.ToCharArray();

            // Hash For higher directory -> num = 0
            // Hash For lower directory -> num = 1
            //int num = 0;

            for (int i = 0; i < PID.Length; i++)
            {
                char ctemp = cPanlID[i];
                int num = 0x00ff;
                int temp = 0x00ff & cPanlID[i];
                nVlue = ((nVlue * 0x00ff) + (0x00ff & cPanlID[i])) % nSeed;
            }

            if (nSeed <= 157)
            {
                LayerSize = nSeed;
            }
            else
            {
                LayerCount = 2;
                LayerSize = Convert.ToInt32(nSeed / Math.Pow(nSeed, 0.5));
            }

            
            if (LayerCount == 1)
            {
                lTmp = Convert.ToInt32(nVlue / LayerSize - 0.49999);
                path = lTmp.ToString("00000000");
            }
            else
            {
                //DongA logic
                //double dTemp;
                //dTemp = nVlue / LayerSize - 0.49999 ;
                //path = dTemp.ToString("00000000");

                lTmp = Convert.ToInt32(nVlue / LayerSize - 0.49999);
                path = lTmp.ToString("00000000");
                lTmp = nVlue % LayerSize;
                path = path + "\\" + lTmp.ToString("00000000");
            }

            textBox2.Text = path;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }
    }
}
