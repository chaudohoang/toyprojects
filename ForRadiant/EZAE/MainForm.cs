using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;


namespace EZAE
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appdir = Path.GetDirectoryName(apppath);
            string toolsdir = appdir + "\\Tools";
            string pintotaskbarpath = Path.Combine(toolsdir, "pttb.exe");

            if (File.Exists(pintotaskbarpath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(pintotaskbarpath);
                startInfo.Arguments = "\"" +apppath+ "\"";
                Process.Start(startInfo);

            }

            if (File.Exists(@"Tools\addhosttointranet.bat"))
            {

                ExecuteAsAdmin(@"Tools\addhosttointranet.bat");

            }


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

        public void ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }


        private void tabSetSequence_Enter(object sender, EventArgs e)
        {
          
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                lblSequenceFileName.Text = latestfile.FullName;
            }
        }

        private void tabSetupPC_Enter(object sender, EventArgs e)
        {
         
        }

        private void tabControl_Enter(object sender, EventArgs e)
        {
        
        }


        private void cmdOpenImageJ_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"Tools\ImageJ\ImageJ.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenLanSearch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"Tools\lansearchpro_portable\lansearc.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenUpdates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string folderpath = @"Tools\Updates";
            if (Directory.Exists(folderpath))
            {
                Process.Start(folderpath);
            }
        }

        private void cmdOpenMTFCenter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"Tools\MTF_Center\MTF.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenTools_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string folderpath = @"Tools";
            if (Directory.Exists(folderpath))
            {
                Process.Start(folderpath);
            }
        }

        private void cmdCheckLC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"Tools\WPSOffice\et.exe";
            string lcfilepath = @"Tools\License Code Combined List.xlsx";
            if (File.Exists(filepath) && File.Exists(lcfilepath))
            {
                Process.Start(filepath, "\"" + lcfilepath + "\"");
            }
        }

        private void cmdCreateFFC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = Path.GetFullPath(@"Tools\FFC_Database_Template\FFCDBGenerate.exe");
            if (File.Exists(filepath))
            {
 
                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

        private void cmdOpenDooone1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\dooone1.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenDooone2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\dooone2.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenDooone3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\dooone3.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenDooone4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\dooone4.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenGooil1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\gooil1.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenGooil2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\gooil2.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenGooil3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\gooil3.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

 

        private void cmdOpenGooil4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\gooil4.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdInstallTightVNC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if ( (cbTightVNCPath.Text == @"C:\Program") && File.Exists(@"Tools\Installer\tightvncCProgram.exe") )
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncCProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"D:\Program") && File.Exists(@"Tools\Installer\tightvncDProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncDProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"E:\Program") && File.Exists(@"Tools\Installer\tightvncEProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncEProgram.exe");
            }
        }

        private void cmdRemoveTightVNC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"D:\Program\TightVNC\Remove TightVNC.bat"))
            {
                ExecuteAsAdmin(@"D:\Program\TightVNC\Remove TightVNC.bat");
            }
            else if (File.Exists(@"C:\Program\TightVNC\Remove TightVNC.bat"))
            {
                ExecuteAsAdmin(@"C:\Program\TightVNC\Remove TightVNC.bat");
            }
            else if (File.Exists(@"E:\Program\TightVNC\Remove TightVNC.bat"))
            {
                ExecuteAsAdmin(@"E:\Program\TightVNC\Remove TightVNC.bat");
            }
        }

        private void cmdCopyDISettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            switch (cbDIList.Text)
            {
                case "Dove2p0 CH1":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI DoooneCH1.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI DoooneCH1.vbs");
                    }
                    break;
                case "Dove2p0 CH2":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI DoooneCH2.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI DoooneCH2.vbs");
                    }
                    break;
                case "Dove2p0 CH3":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI DoooneCH3.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI DoooneCH3.vbs");
                    }
                    break;
                case "Dove2p0 CH4":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI DoooneCH4.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI DoooneCH4.vbs");
                    }
                    break;
                case "Emu2p0 CH1":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI GooilCH1.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI GooilCH1.vbs");
                    }
                    break;
                case "Emu2p0 CH2":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI GooilCH2.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI GooilCH2.vbs");
                    }
                    break;
                case "Emu2p0 CH3":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI GooilCH3.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI GooilCH3.vbs");
                    }
                    break;
                case "Emu2p0 CH4":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI GooilCH4.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI GooilCH4.vbs");
                    }
                    break;
                default:
                    break;
            }
        }

        private void cmdInstallIrfanview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\iview457_x64_setup silent.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\iview457_x64_setup silent.bat");
            }
        }

        private void cmdInstallVC12_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\Visual C++ 2012\VC2012_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\Visual C++ 2012\VC2012_silent_install.bat");
            }
        }

        private void cmdInstallVC1519_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat");
            }
        }

        private void cmdInstallKdiff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat");
            }
        }

        private void cmdInstallNPP_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\npp.7.8.1.Installer.x64 silent.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\npp.7.8.1.Installer.x64 silent.bat");
            }
        }

        private void cmdInstallDotnet48_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\ndp48-x86-x64-allos-enu.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\ndp48-x86-x64-allos-enu.exe");
            }
        }

        private void cmdInstallMatlab_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\MCR_R2017b_win64_installer.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\MCR_R2017b_win64_installer.exe");
            }
        }

        private void cmdShareC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\ShareC.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareC.bat");
            }
        }

        private void cmdShareD_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\ShareD.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareD.bat");
            }
        }

        private void cmdShareE_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\ShareE.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareE.bat");
            }
        }

        private void cmdShareRVSData_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\ShareRadiantVisionSystemsData.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareRadiantVisionSystemsData.bat");
            }
        }


        private void cmdCreateOTPandResult_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\CopyOTP.vbs"))
            {
                Process.Start(@"Tools\Installer\CopyOTP.vbs");
            }

        }

        private void cmdSetTTPermission_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\updatePermission.bat"))
            {
                ExecuteAsAdmin(@"Tools\updatePermission.bat");
            }
        }

       
        private void cmdInstallWireshark_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\Wireshark-win64-3.4.3.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\Wireshark-win64-3.4.3.exe");
            }
        }

        private void cmdPinFixturePcFolders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (cbFixture.Text == @"Dooone" && File.Exists(@"Tools\pinDoooneFixturePCFolders.bat") && File.Exists(@"Tools\pinDoooneFixturePCFolders.ps1"))
            {
                ExecuteAsAdmin(@"Tools\pinDoooneFixturePCFolders.bat");
            }
            else if (cbFixture.Text == @"Gooil" && File.Exists(@"Tools\pinGooilFixturePCFolders.bat") && File.Exists(@"Tools\pinGooilFixturePCFolders.ps1"))
            {
                ExecuteAsAdmin(@"Tools\pinGooilFixturePCFolders.bat");
            }
        }

        private void cmdPinCameraPcFolders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if ( cbFixture.Text==@"Dooone" && File.Exists(@"Tools\pinDoooneCameraPCFolders.bat") && File.Exists(@"Tools\pinDoooneCameraPCFolders.ps1"))
            {
                ExecuteAsAdmin(@"Tools\pinDoooneCameraPCFolders.bat");
            }
            else if (cbFixture.Text == @"Gooil" && File.Exists(@"Tools\pinGooilCameraPCFolders.bat") && File.Exists(@"Tools\pinGooilCameraPCFolders.ps1"))
            {
                ExecuteAsAdmin(@"Tools\pinGooilCameraPCFolders.bat");
            }

        }

        private void cmdUACOff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\disableUAC.bat"))
            {
                ExecuteAsAdmin(@"Tools\disableUAC.bat");
            }
        }

        private void cmdFirewallOff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\FirewallOff.bat"))
            {
                ExecuteAsAdmin(@"Tools\FirewallOff.bat");
            }
        }

        private void cmdBrowseSequence_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = @"C:\Radiant Vision Systems Data\TrueTest\Sequence";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    lblSequenceFileName.Text = dialog.FileName;
                }
            }
        }

        private void cmdUseLastModifiedSequence_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                lblSequenceFileName.Text = latestfile.FullName;
            }
        }

        private void cmdPasswordSharingOff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\PasswordSharingOff.bat"))
            {
                ExecuteAsAdmin(@"Tools\PasswordSharingOff.bat");
            }
        }

        private void cmdOneclickSetupCameraPC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\disableUAC.bat"))
            {
                ExecuteAsAdmin(@"Tools\disableUAC.bat");
            }

            if (File.Exists(@"Tools\changePW1.bat"))
            {
                ExecuteAsAdmin(@"Tools\changePW1.bat");
            }

            if (File.Exists(@"Tools\PasswordSharingOff.bat"))
            {
                ExecuteAsAdmin(@"Tools\PasswordSharingOff.bat");
            }

            if (File.Exists(@"Tools\FirewallOff.bat"))
            {
                ExecuteAsAdmin(@"Tools\FirewallOff.bat");
            }

            if (File.Exists(@"Tools\ShareC.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareC.bat");
            }

            if (File.Exists(@"Tools\ShareD.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareD.bat");
            }

            if (File.Exists(@"Tools\ShareE.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareE.bat");
            }

            if (File.Exists(@"Tools\ShareCProgram.bat") && cbProgramFolder.Text == @"C:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareCProgram.bat");
            }

            else if (File.Exists(@"Tools\ShareDProgram.bat") && cbProgramFolder.Text == @"D:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareDProgram.bat");
            }

            else if (File.Exists(@"Tools\ShareEProgram.bat") && cbProgramFolder.Text == @"E:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareEProgram.bat");
            }

            if (File.Exists(@"Tools\ShareRadiantVisionSystemsData.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareRadiantVisionSystemsData.bat");
            }

            if (File.Exists(@"Tools\SharePOCB.bat"))
            {
                ExecuteAsAdmin(@"Tools\SharePOCB.bat");
            }

            if ((cbTightVNCPath.Text == @"C:\Program") && File.Exists(@"Tools\Installer\tightvncCProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncCProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"D:\Program") && File.Exists(@"Tools\Installer\tightvncDProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncDProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"E:\Program") && File.Exists(@"Tools\Installer\tightvncEProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncEProgram.exe");
            }

            if (cbFixture.Text == @"Dooone" && File.Exists(@"Tools\pinDoooneFixturePCFolders.bat") && File.Exists(@"Tools\pinDoooneFixturePCFolders.ps1"))
            {
                ExecuteAsAdmin(@"Tools\pinDoooneFixturePCFolders.bat");
            }
            else if (cbFixture.Text == @"Gooil" && File.Exists(@"Tools\pinGooilFixturePCFolders.bat") && File.Exists(@"Tools\pinGooilFixturePCFolders.ps1"))
            {
                ExecuteAsAdmin(@"Tools\pinGooilFixturePCFolders.bat");
            }

            if (File.Exists(@"Tools\Installer\Visual C++ 2012\VC2012_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\Visual C++ 2012\VC2012_silent_install.bat");
            }

            if (File.Exists(@"Tools\Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat");
            }

            if (File.Exists(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat");
            }

            if (File.Exists(@"Tools\Installer\iview457_x64_setup silent.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\iview457_x64_setup silent.bat");
            }

            if (File.Exists(@"Tools\Installer\npp.7.8.1.Installer.x64 silent.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\npp.7.8.1.Installer.x64 silent.bat");
            }

            if (File.Exists(@"Tools\Installer\ndp48-x86-x64-allos-enu.exe") && chkdotnetinstall.Checked)
            {
                ExecuteAsAdmin(@"Tools\Installer\ndp48-x86-x64-allos-enu.exe");
            }

            if (File.Exists(@"Tools\Installer\MCR_R2017b_win64_installer.exe") && chkmatlabinstall.Checked)
            {
                ExecuteAsAdmin(@"Tools\Installer\MCR_R2017b_win64_installer.exe");
            }

            if (File.Exists(@"Tools\Installer\MCR_R2018a_win64_installer.exe") && chkmatlab2018ainstall.Checked)
            {
                ExecuteAsAdmin(@"Tools\Installer\MCR_R2018a_win64_installer.exe");
            }

        }

        private void cmdOneclickSetupFixturePC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\disableUAC.bat"))
            {
                ExecuteAsAdmin(@"Tools\disableUAC.bat");
            }

            if (File.Exists(@"Tools\changePW1.bat"))
            {
                ExecuteAsAdmin(@"Tools\changePW1.bat");
            }

            if (File.Exists(@"Tools\PasswordSharingOff.bat"))
            {
                ExecuteAsAdmin(@"Tools\PasswordSharingOff.bat");
            }

            if (File.Exists(@"Tools\FirewallOff.bat"))
            {
                ExecuteAsAdmin(@"Tools\FirewallOff.bat");
            }

            if (File.Exists(@"Tools\ShareC.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareC.bat");
            }

            if (File.Exists(@"Tools\ShareD.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareD.bat");
            }

            if (File.Exists(@"Tools\ShareE.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareE.bat");
            }

            if (File.Exists(@"Tools\ShareCProgram.bat") && cbProgramFolder.Text == @"C:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareCProgram.bat");
            }

            else if (File.Exists(@"Tools\ShareDProgram.bat") && cbProgramFolder.Text == @"D:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareDProgram.bat");
            }

            else if (File.Exists(@"Tools\ShareEProgram.bat") && cbProgramFolder.Text == @"E:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareEProgram.bat");
            }

            if ((cbTightVNCPath.Text == @"C:\Program") && File.Exists(@"Tools\Installer\tightvncCProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncCProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"D:\Program") && File.Exists(@"Tools\Installer\tightvncDProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncDProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"E:\Program") && File.Exists(@"Tools\Installer\tightvncEProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncEProgram.exe");
            }

            if (cbFixture.Text == @"Dooone" && File.Exists(@"Tools\pinDoooneCameraPCFolders.bat") && File.Exists(@"Tools\pinDoooneCameraPCFolders.ps1"))
            {
                ExecuteAsAdmin(@"Tools\pinDoooneCameraPCFolders.bat");
            }
            else if (cbFixture.Text == @"Gooil" && File.Exists(@"Tools\pinGooilCameraPCFolders.bat") && File.Exists(@"Tools\pinGooilCameraPCFolders.ps1"))
            {
                ExecuteAsAdmin(@"Tools\pinGooilCameraPCFolders.bat");
            }

            if (File.Exists(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat");
            }

            if (File.Exists(@"Tools\Installer\iview457_x64_setup silent.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\iview457_x64_setup silent.bat");
            }

            if (File.Exists(@"Tools\Installer\npp.7.8.1.Installer.x64 silent.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\npp.7.8.1.Installer.x64 silent.bat");
            }
        }

        private void cmdOneclickSetupTrueTestSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\CopyOTP.vbs"))
            {
                Process.Start(@"Tools\Installer\CopyOTP.vbs");
            }

            if (File.Exists(@"Tools\Installer\CopySequence.vbs"))
            {
                Process.Start(@"Tools\Installer\CopySequence.vbs");
            }

            if (File.Exists(@"Tools\updatePermission.bat"))
            {
                ExecuteAsAdmin(@"Tools\updatePermission.bat");
            }

            switch (cbDIList.Text)
            {
                case "Dove2p0 CH1":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI DoooneCH1.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI DoooneCH1.vbs");
                    }
                    break;
                case "Dove2p0 CH2":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI DoooneCH2.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI DoooneCH2.vbs");
                    }
                    break;
                case "Dove2p0 CH3":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI DoooneCH3.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI DoooneCH3.vbs");
                    }
                    break;
                case "Dove2p0 CH4":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI DoooneCH4.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI DoooneCH4.vbs");
                    }
                    break;
                case "Emu2p0 CH1":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI GooilCH1.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI GooilCH1.vbs");
                    }
                    break;
                case "Emu2p0 CH2":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI GooilCH2.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI GooilCH2.vbs");
                    }
                    break;
                case "Emu2p0 CH3":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI GooilCH3.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI GooilCH3.vbs");
                    }
                    break;
                case "Emu2p0 CH4":
                    if (File.Exists(@"Tools\Installer\DISettings\CopyDI GooilCH4.vbs"))
                    {
                        Process.Start(@"Tools\Installer\DISettings\CopyDI GooilCH4.vbs");
                    }
                    break;
                default:
                    break;
            }
        }

        private void cmdBackupCurrentTT_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\BackupCurrentTT.exe"))
            {
                Process.Start(@"Tools\BackupCurrentTT.exe");
            }
        }

        private void cmdShareProgram_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\ShareCProgram.bat") && cbProgramFolder.Text == @"C:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareCProgram.bat");
            }

            else if (File.Exists(@"Tools\ShareDProgram.bat") && cbProgramFolder.Text == @"D:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareDProgram.bat");
            }

            else if (File.Exists(@"Tools\ShareEProgram.bat") && cbProgramFolder.Text == @"E:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareEProgram.bat");
            }
        }

        private void cmdOpenEngineerMode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"Tools\EngineerMode.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }
        private void cbTrueTestInstallerList_DropDown(object sender, EventArgs e)
        {
            cbTrueTestInstallerList.Items.Clear();
            String[] exes =
            Directory.GetFiles(@"TrueTest Installers", "*.EXE", SearchOption.AllDirectories)
            .Select(fileName => Path.GetFileName(fileName))
            .Where(fileName => Path.GetFileNameWithoutExtension(fileName).StartsWith("TrueTest"))
            .ToArray();

            foreach (string file in exes)
            {
                cbTrueTestInstallerList.Items.Add(file);
            }
        }
        private void cmdInstallTrueTest_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\BackupCurrentTT.exe"))
            {
                Process.Start(@"Tools\BackupCurrentTT.exe");
            }
            if (File.Exists(@"TrueTest Installers\" + cbTrueTestInstallerList.Text))
            {
                ExecuteAsAdmin(@"TrueTest Installers\" + cbTrueTestInstallerList.Text);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }

        private void cmdSharePOCB_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\SharePOCB.bat"))
            {
                ExecuteAsAdmin(@"Tools\SharePOCB.bat");
            }
        }

        private void cmdChangePW1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\changePW1.bat"))
            {
                ExecuteAsAdmin(@"Tools\changePW1.bat");
            }

        }

        private void cmdInstallMatlab2018a_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\MCR_R2018a_win64_installer.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\MCR_R2018a_win64_installer.exe");
            }
        }

        private void cmdUseButterfly_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\SetSequence\SetSeqx.exe"))
            {
                Process.Start(@"Tools\SetSequence\SetSeqx.exe");
            }
        }

        private void cmdUseScript_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\SetSequence\\Set_all_sequences.vbs"))
            {
                Process.Start(@"Tools\SetSequence\Set_all_sequences.vbs");
            }
        }

        private void cmdCopySequence_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\CopySequence.vbs"))
            {
                Process.Start(@"Tools\Installer\CopySequence.vbs");
            }
        }

        private void cmdSetSequence_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = Path.GetFullPath(@"Tools\SetSequence\SetSequence.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

        private void cbCopyWhizItem_DropDown(object sender, EventArgs e)
        {
            cbCopyWhizItem.Items.Clear();
            String[] czml =
            Directory.GetFiles(@"Tools/CopyWhiz", "*.czml", SearchOption.AllDirectories)
            .Select(fileName => Path.GetFileName(fileName))
            .ToArray();

            foreach (string file in czml)
            {
                cbCopyWhizItem.Items.Add(file);
            }
        }

        private void cmdRunCopyWhiz_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

            if ((cbCopyWhizItem.Text != "") && File.Exists(@"Tools\CopyWhiz\CopywhizCopy.exe"))
            {
                string filepath = Path.GetFullPath(@"Tools\CopyWhiz\CopywhizCopy.exe");
                string argumentpath = Path.Combine(Path.GetDirectoryName(filepath), cbCopyWhizItem.Text);
                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.Arguments = "\"" + argumentpath + "\"";
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                
                Process.Start(startInfo);
            }
        }
    }
}
