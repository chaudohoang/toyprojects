using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EZAE
{
    public partial class SetupPC : Form
    {
        public SetupPC()
        {
            InitializeComponent();
        }

        public void ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        private void btnInstallTightVNC_Click(object sender, EventArgs e)
        {
            if (txtTightVNCPath.Text=="D:\\Program")
            {
                ExecuteAsAdmin(@"Installer\tightvncDProgram.exe");
            }
            else if (txtTightVNCPath.Text == "D:\\Program")
            {
                ExecuteAsAdmin(@"Installer\tightvncCProgram.exe");
            }
            else
            {

            }
        }

        private void btnRemoveTightVNC_Click(object sender, EventArgs e)
        {
            if (File.Exists("D:\\Program\\TightVNC\\Remove TightVNC.bat"))
            {
                ExecuteAsAdmin("D:\\Program\\TightVNC\\Remove TightVNC.bat");
            }
            else if (File.Exists("C:\\Program\\TightVNC\\Remove TightVNC.bat"))
            {
                ExecuteAsAdmin("C:\\Program\\TightVNC\\Remove TightVNC.bat");
            }
            else
            {

            }
        }
    }
}
