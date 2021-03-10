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

        private void btnInstallVC12_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Installer\Visual C++ 2012\VC2012_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Installer\Visual C++ 2012\VC2012_silent_install.bat");
            }
        }

        private void btnInstallVC1519_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat");
            }
        }

        private void btnInstallKdiff_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat");
            }
        }

        private void btnInstallNPP_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Installer\npp.7.8.1.Installer.x64 silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\npp.7.8.1.Installer.x64 silent.bat");
            }
        }

        private void btnInstallDotnet_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Installer\ndp48-x86-x64-allos-enu.exe"))
            {
                ExecuteAsAdmin(@"Installer\ndp48-x86-x64-allos-enu.exe");
            }
        }

        private void btnInstallMatlab_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Installer\MCR_R2017b_win64_installer.exe"))
            {
                ExecuteAsAdmin(@"Installer\MCR_R2017b_win64_installer.exe");
            }
        }

        private void btnShareC_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\ShareC.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareC.bat");
            }
        }

        private void btnShareD_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\ShareD.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareD.bat");
            }
        }

        private void btnShareE_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\ShareE.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareE.bat");
            }
        }

        private void btnShareRVSData_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\ShareRadiantVisionSystemsData.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareRadiantVisionSystemsData.bat");
            }
        }

        private void btnCreateOTPandResult_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\CreateOTPandResult.bat"))
            {
                ExecuteAsAdmin(@"Tools\CreateOTPandResult.bat");
            }
        }

        private void btnShareDProgram_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\ShareDProgram.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareDProgram.bat");
            }
        }

        private void btnPinFolders_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\pinFolders.bat"))
            {
                ExecuteAsAdmin(@"Tools\pinFolders.bat");
            }
        }

        private void btnGivePermission_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\updatePermission.bat"))
            {
                ExecuteAsAdmin(@"Tools\updatePermission.bat");
            }
        }
    }
}
