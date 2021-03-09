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
    public partial class MainForm : Form
    {
        public MainForm()
        {
            
            InitializeComponent();
        }

        private void btnSetsequence_Click(object sender, EventArgs e)
        {
            SetSequence form = new SetSequence();
            form.Show();
        }

        private void btnSetupPC_Click(object sender, EventArgs e)
        {
            SetupPC form = new SetupPC();
            form.Show();
        }

        private void btnCreateFFC_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"FFC_Database_Template\Generabe FFC Database.vbs"))
            {
                Process.Start(@"FFC_Database_Template\Generabe FFC Database.vbs");

            }
        }

        public void ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        
    }
}
