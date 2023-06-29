using System;
using System.ComponentModel;
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
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.Arguments = "\"" + apppath + "\"";
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
            string filepath = @"Apps\ImageJ\ImageJ.exe";
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

        private void cmdOpenWPSExcel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"Apps\WPSOffice\et.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
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
            if ((cbTightVNCPath.Text == @"C:\Setup\Program") && File.Exists(@"Tools\Installer\tightvncCSetupProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncCSetupProgram.exe");
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

        private void cmdInstallIrfanview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\iview457_x64_setup.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\iview457_x64_setup.exe");
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
            if (File.Exists(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2.exe");
            }
        }

        private void cmdInstallNPP_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\npp.7.8.1.Installer.x64.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\npp.7.8.1.Installer.x64.exe");
            }
        }

        private void cmdInstallDotnet48_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Big Installers\ndp48-x86-xx64-allos-enu.exe"))
            {
                ExecuteAsAdmin(@"Big Installers\ndp48-x86-xx64-allos-enu.exe");
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


        private void cmdSetTTPermission_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\updatePermission.bat"))
            {
                ExecuteAsAdmin(@"Tools\updatePermission.bat");
            }
        }


        private void cmdInstallWireshark_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Big Installers\Wireshark-win64-3.4.3.exe"))
            {
                ExecuteAsAdmin(@"Big Installers\Wireshark-win64-3.4.3.exe");
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
            if (cbFixture.Text == @"Dooone" && File.Exists(@"Tools\pinDoooneCameraPCFolders.bat") && File.Exists(@"Tools\pinDoooneCameraPCFolders.ps1"))
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

            if (File.Exists(@"Tools\ShareCSetupProgram.bat") && cbProgramFolder.Text == @"C:\Setup\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareCSetupProgram.bat");
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

            if ((cbTightVNCPath.Text == @"C:\Setup\Program") && File.Exists(@"Tools\Installer\tightvncCSetupProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncCSetupProgram.exe");
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

            if (File.Exists(@"Tools\Autostart_Camera\Task_Create_StartProcess.bat"))
            {
                ExecuteAsAdmin(@"Tools\Autostart_Camera\Task_Create_StartProcess.bat");
            }

            if (File.Exists(@"Tools\Autostart_Camera\Task_Create_KillProcess.bat"))
            {
                ExecuteAsAdmin(@"Tools\Autostart_Camera\Task_Create_KillProcess.bat");
            }

            if (File.Exists(@"Tools\Installer\Visual C++ 2012\VC2012_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\Visual C++ 2012\VC2012_silent_install.bat");
            }

            if (File.Exists(@"Tools\Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Tools\Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat");
            }

            if (File.Exists(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2.exe");
            }

            if (File.Exists(@"Tools\Installer\iview457_x64_setup.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\iview457_x64_setup.exe");
            }

            if (File.Exists(@"Tools\Installer\npp.7.8.1.Installer.x64.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\npp.7.8.1.Installer.x64.exe");
            }

            if (File.Exists(@"Big Installers\ndp48-x86-xx64-allos-enu.exe") && chkdotnetinstall.Checked)
            {
                ExecuteAsAdmin(@"Big Installers\ndp48-x86-xx64-allos-enu.exe");
            }

            if (File.Exists(@"Big Installers\MATLAB_Runtime_R2021b_Update_2_win64.exe") && chkmatlabinstall.Checked)
            {
                ExecuteAsAdmin(@"Big Installers\MATLAB_Runtime_R2021b_Update_2_win64.exe");
            }

            if (File.Exists(@"Big Installers\MCR_R2017b_win64_installer.exe") && chkmatlabinstall2017b.Checked)
            {
                ExecuteAsAdmin(@"Big Installers\MCR_R2017b_win64_installer.exe");
            }

            if (File.Exists(@"Big Installers\MATLAB_Runtime_R2022b_Update_5_win64.exe") && chkmatlabinstall2022b.Checked)
            {
                ExecuteAsAdmin(@"Big Installers\MATLAB_Runtime_R2022b_Update_5_win64.exe");
            }

            if (File.Exists(@"Apps\ImageJ\pinImageJtoTaskbar.bat"))
            {
                Process.Start(@"Apps\ImageJ\pinImageJtoTaskbar.bat");
            }

            if (File.Exists(@"Tools\SetSequence\pinSetSequenceToTaskbar.bat"))
            {
                Process.Start(@"Tools\SetSequence\pinSetSequenceToTaskbar.bat");
            }

            if (File.Exists(@"Tools\SetSequence\pinSetSeqxToTaskbar.bat"))
            {
                Process.Start(@"Tools\SetSequence\pinSetSeqxToTaskbar.bat");
            }

            if (File.Exists(@"Tools\FFC_Database_Template\pinFFCDBGenerateToTaskbar.bat"))
            {
                Process.Start(@"Tools\FFC_Database_Template\pinFFCDBGenerateToTaskbar.bat");
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

            if (File.Exists(@"Tools\ShareCSetupProgram.bat") && cbProgramFolder.Text == @"C:\Setup\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareCSetupProgram.bat");
            }

            else if (File.Exists(@"Tools\ShareDProgram.bat") && cbProgramFolder.Text == @"D:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareDProgram.bat");
            }

            else if (File.Exists(@"Tools\ShareEProgram.bat") && cbProgramFolder.Text == @"E:\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareEProgram.bat");
            }

            if ((cbTightVNCPath.Text == @"C:\Setup\Program") && File.Exists(@"Tools\Installer\tightvncCSetupProgram.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\tightvncCSetupProgram.exe");
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

            if (File.Exists(@"Tools\Autostart_Fixture\Task_Create_StartProcess.bat"))
            {
                ExecuteAsAdmin(@"Tools\Autostart_Fixture\Task_Create_StartProcess.bat");
            }

            if (File.Exists(@"Tools\Autostart_Fixture\Task_Create_KillProcess.bat"))
            {
                ExecuteAsAdmin(@"Tools\Autostart_Fixture\Task_Create_KillProcess.bat");
            }

            if (File.Exists(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\KDiff3-64bit-Setup_0.9.98-2.exe");
            }

            if (File.Exists(@"Tools\Installer\iview457_x64_setup.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\iview457_x64_setup.exe");
            }

            if (File.Exists(@"Tools\Installer\npp.7.8.1.Installer.x64.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\npp.7.8.1.Installer.x64.exe");
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
            if (File.Exists(@"Tools\ShareCSetupProgram.bat") && cbProgramFolder.Text == @"C:\Setup\Program")
            {
                ExecuteAsAdmin(@"Tools\ShareCSetupProgram.bat");
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
            String[] exes = { };
            try
            {                
            exes = Directory.GetFiles(@"TrueTest Installers", "*.EXE", SearchOption.AllDirectories)
            .Select(fileName => Path.GetFileName(fileName))
            .Where(fileName => Path.GetFileNameWithoutExtension(fileName).StartsWith("TrueTest"))
            .ToArray();
            }
            catch (Exception)
            {
            }

            foreach (string file in exes)
            {
                cbTrueTestInstallerList.Items.Add(file);
            }
        }
        private void cmdInstallTrueTest_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {            
            if (File.Exists(@"TrueTest Installers\" + cbTrueTestInstallerList.Text))
            {
                DialogResult dialogResult = MessageBox.Show("Backup TT First ?", "Backup TT", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    
                    if (File.Exists(@"Tools\BackupCurrentTT.exe"))
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = @"Tools\BackupCurrentTT.exe";
                        p.EnableRaisingEvents = true;
                        p.Start();
                        p.WaitForExit();
                    }
                }                
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


        private void cmdUseButterfly_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\SetSequence\SetSeqx.exe"))
            {
                Process.Start(@"Tools\SetSequence\SetSeqx.exe");
            }
        }

        private void cmdUseScript_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\SetSequence\Set_all_sequences.vbs"))
            {
                Process.Start(@"Tools\SetSequence\Set_all_sequences.vbs");
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

        private void cmdAutostartTaskCamera_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Autostart_Camera\Task_Create_StartProcess.bat"))
            {
                ExecuteAsAdmin(@"Tools\Autostart_Camera\Task_Create_StartProcess.bat");
            }

            if (File.Exists(@"Tools\Autostart_Camera\Task_Create_KillProcess.bat"))
            {
                ExecuteAsAdmin(@"Tools\Autostart_Camera\Task_Create_KillProcess.bat");
            }
        }

        private void cmdAutostartTaskFixture_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Autostart_Fixture\Task_Create_StartProcess.bat"))
            {
                ExecuteAsAdmin(@"Tools\Autostart_Fixture\Task_Create_StartProcess.bat");
            }

            if (File.Exists(@"Tools\Autostart_Fixture\Task_Create_KillProcess.bat"))
            {
                ExecuteAsAdmin(@"Tools\Autostart_Fixture\Task_Create_KillProcess.bat");
            }
        }

        private void cmdRemoteTools_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = Path.GetFullPath(@"Tools\RemoteTools\RemoteTools.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

        private void cmdPinRegularApps_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Apps\ImageJ\pinImageJtoTaskbar.bat"))
            {
                Process.Start(@"Apps\ImageJ\pinImageJtoTaskbar.bat");
            }

            if (File.Exists(@"Tools\SetSequence\pinSetSequencetoTaskbar.bat"))
            {
                Process.Start(@"Tools\SetSequence\pinSetSequencetoTaskbar.bat");
            }

            if (File.Exists(@"Tools\SetSequence\pinSetSeqxToTaskbar.bat"))
            {
                Process.Start(@"Tools\SetSequence\pinSetSeqxToTaskbar.bat");
            }

            if (File.Exists(@"Tools\FFC_Database_Template\pinFFCDBGenerateToTaskbar.bat"))
            {
                Process.Start(@"Tools\FFC_Database_Template\pinFFCDBGenerateToTaskbar.bat");
            }
        }

		private void cmdInstallMatlab2017b_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            if (File.Exists(@"Big Installers\MCR_R2017b_win64_installer.exe"))
            {
                ExecuteAsAdmin(@"Big Installers\MCR_R2017b_win64_installer.exe");
            }
        }

        private void cmdInstallMatlab_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Big Installers\MATLAB_Runtime_R2021b_Update_2_win64.exe"))
            {
                ExecuteAsAdmin(@"Big Installers\MATLAB_Runtime_R2021b_Update_2_win64.exe");
            }
        }

		private void cmdOpenLINQPad_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            string filepath = Path.GetFullPath(@"Tools\LINQPad7\LINQPad7.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

		private void cmdOpenTCPServer_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            string filepath = Path.GetFullPath(@"Tools\TCPClientServer\TCPServer.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

		private void cmdOpenTCPClient_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            string filepath = Path.GetFullPath(@"Tools\TCPClientServer\TCPClient.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

		private void cmdOpenCopywhiz_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            string filepath = Path.GetFullPath(@"Tools\Copywhiz\Copywhiz.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

		private void cmdCheckSequence_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            string filepath = Path.GetFullPath(@"Tools\SequenceCheck\SequenceCheck.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

		private void cmdOpenWorklog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            string filepath = Path.GetFullPath(@"Tools\RVSWorklog\RVSWorklog.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

		private void cmdOpenAutoClose_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            string filepath = Path.GetFullPath(@"Tools\AutoClose\AutoClose.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

		private void cmdFTPUploader_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
            string filepath = Path.GetFullPath(@"Tools\FTPUploaderVB\FTPUploaderVB.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

		private void cmdRestartTTDove2p0_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string filepath = Path.GetFullPath(@"Tools\RestartTTDove2p0\RestartTTDove2p0.exe");
			if (File.Exists(filepath))
			{

				ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
				startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
				Process.Start(startInfo);

			}
		}

		private void cmdShareX_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string filepath = Path.GetFullPath(@"Tools\ShareX\RunShareX.exe");
			if (File.Exists(filepath))
			{

				ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
				startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
				Process.Start(startInfo);

			}
		}

		private void cmdMakeProcessFile2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string filepath = Path.GetFullPath(@"Tools\MakeProcessFile2.exe");
			if (File.Exists(filepath))
			{

				ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
				startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
				Process.Start(startInfo);

			}
		}

        private void cmdEmu2p1SequenceConvert_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = Path.GetFullPath(@"Tools\Emu2p1SequenceConvert\Emu2p1SequenceConvert.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

        private void cmdCopyMaster_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = Path.GetFullPath(@"Tools\CopyMaster\CopyMaster.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

        private void cmdRenameVNTT_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\RenameVNTT.exe"))
            {
                Process.Start(@"Tools\RenameVNTT.exe");
            }
        }

        private void cmdInstallMatlab2022b_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Big Installers\MATLAB_Runtime_R2022b_Update_5_win64.exe"))
            {
                ExecuteAsAdmin(@"Big Installers\MATLAB_Runtime_R2022b_Update_5_win64.exe");
            }
        }

        private void cmdOpenGooil1Tablet_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\gooil1tablet.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenGooil2Tablet_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\gooil2tablet.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenGooil3Tablet_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\gooil3tablet.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenGooil4Tablet_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"C:\Windows\gooil4tablet.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdInstallWinMerge_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Tools\Installer\WinMerge-2.16.20-x64-Setup.exe"))
            {
                ExecuteAsAdmin(@"Tools\Installer\WinMerge-2.16.20-x64-Setup.exe");
            }
        }

        private void cmdFolderLock_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = Path.GetFullPath(@"Tools\FolderLock\FolderLock.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }

        private void cmdCheckUSBLicenseCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"Apps\WPSOffice\et.exe";
            string lcfilepath = @"Tools\License Code Combined List.xlsx";
            if (File.Exists(filepath) && File.Exists(lcfilepath))
            {
                Process.Start(filepath, "\"" + lcfilepath + "\"");
            }
        }

        private void cmdSmartSniffer_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = Path.GetFullPath(@"Tools\smsniff-x64\smsniff.exe");
            if (File.Exists(filepath))
            {

                ProcessStartInfo startInfo = new ProcessStartInfo(filepath);
                startInfo.WorkingDirectory = Path.GetDirectoryName(filepath);
                Process.Start(startInfo);

            }
        }
    }
}
