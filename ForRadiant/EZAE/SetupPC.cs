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
            if (File.Exists(fileName))
            {
                proc.StartInfo.FileName = fileName;
                proc.StartInfo.UseShellExecute = true;
                proc.StartInfo.Verb = "runas";
                proc.Start();
            }
        }

        private void btnInstallTightVNC_Click(object sender, EventArgs e)
        {
            if (cbTightVNCPath.Text==@"D:\Program")
            {
                ExecuteAsAdmin(@"Installer\tightvncDProgram.exe");
            }
            else if (cbTightVNCPath.Text == @"D:\Program")
            {
                ExecuteAsAdmin(@"Installer\tightvncCProgram.exe");
            }
        }

        private void btnRemoveTightVNC_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"D:\Program\TightVNC\Remove TightVNC.bat"))
            {
                ExecuteAsAdmin(@"D:\Program\TightVNC\Remove TightVNC.bat");
            }
            else if (File.Exists(@"C:\Program\TightVNC\Remove TightVNC.bat"))
            {
                ExecuteAsAdmin(@"C:\Program\TightVNC\Remove TightVNC.bat");
            }
        }

        private void btnCopyDISettings_Click(object sender, EventArgs e)
        {
            switch (cbDIList.Text)
            {
                case "Dove2p0 CH1":
                    if (File.Exists(@"Installer\DISettings\CopyDI DoooneCH1.vbs"))
                    {
                        Process.Start(@"Installer\DISettings\CopyDI DoooneCH1.vbs");
                    }
                    break;
                case "Dove2p0 CH2":
                    if (File.Exists(@"Installer\DISettings\CopyDI DoooneCH2.vbs"))
                    {
                        Process.Start(@"Installer\DISettings\CopyDI DoooneCH2.vbs");
                    }
                    break;
                case "Dove2p0 CH3":
                    if (File.Exists(@"Installer\DISettings\CopyDI DoooneCH3.vbs"))
                    {
                        Process.Start(@"Installer\DISettings\CopyDI DoooneCH3.vbs");
                    }
                    break;
                case "Dove2p0 CH4":
                    if (File.Exists(@"Installer\DISettings\CopyDI DoooneCH4.vbs"))
                    {
                        Process.Start(@"Installer\DISettings\CopyDI DoooneCH4.vbs");
                    }
                    break;
                case "Emu2p0 CH1":
                    if (File.Exists(@"Installer\DISettings\CopyDI GooilCH1.vbs"))
                    {
                        Process.Start(@"Installer\DISettings\CopyDI GooilCH1.vbs");
                    }
                    break;
                case "Emu2p0 CH2":
                    if (File.Exists(@"Installer\DISettings\CopyDI GooilCH2.vbs"))
                    {
                        Process.Start(@"Installer\DISettings\CopyDI GooilCH2.vbs");
                    }
                    break;
                case "Emu2p0 CH3":
                    if (File.Exists(@"Installer\DISettings\CopyDI GooilCH3.vbs"))
                    {
                        Process.Start(@"Installer\DISettings\CopyDI GooilCH3.vbs");
                    }
                    break;
                case "Emu2p0 CH4":
                    if (File.Exists(@"Installer\DISettings\CopyDI GooilCH4.vbs"))
                    {
                        Process.Start(@"Installer\DISettings\CopyDI GooilCH4.vbs");
                    }
                    break;
                default:
                    break;
            }
        }

        private void btnInstallIrfanview_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Installer\iview457_x64_setup silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\iview457_x64_setup silent.bat");
            }
        }
    }
}
