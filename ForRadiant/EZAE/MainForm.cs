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
            string filepath = @"FFC_Database_Template\Generabe FFC Database.vbs";
            if (File.Exists(filepath));
            {
                Process.Start(filepath);
            }
        }
 
        private void btnEngineeringMode_Click(object sender, EventArgs e)
        {
            string filepath = @"Tools\EngineerMode.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
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

        private void btnCheckLC_Click(object sender, EventArgs e)
        {
            string filepath = @"WPSOffice\et.exe";
            string lcfilepath = @"License Code Combined List.xlsx";
            if ( File.Exists(filepath) && File.Exists(lcfilepath) ) 
            {
                Process.Start(filepath, "\"" + lcfilepath + "\"");
            }
            
        }

        private void btnOpenImageJ_Click(object sender, EventArgs e)
        {
            string filepath = @"ImageJ\ImageJ.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnOpenLanSearch_Click(object sender, EventArgs e)
        {
            string filepath = @"lansearchpro_portable\lansearc.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnOpenMTFCenter_Click(object sender, EventArgs e)
        {
            string filepath = @"MTF_Center\MTF.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnOpenUpdates_Click(object sender, EventArgs e)
        {
            string folderpath = @"Updates";
            if (Directory.Exists(folderpath))
            {
                Process.Start(folderpath);
            }
        }

        private void btnOpenTools_Click(object sender, EventArgs e)
        {
            string folderpath = @"Tools";
            if (Directory.Exists(folderpath))
            {
                Process.Start(folderpath);
            }
        }
    }
}
