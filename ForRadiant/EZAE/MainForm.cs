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
using System.Xml;

namespace EZAE
{
    public partial class MainForm : Form
    {
        public MainForm()
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


        private void btnApply_Click(object sender, EventArgs e)
        {
            XmlNode node;
            XmlNodeList nodes;

            var xmlDoc = new XmlDocument();
            if (File.Exists(@lblSequenceFileName.Text))
            {
                xmlDoc.Load(lblSequenceFileName.Text);

                string x, y, width, height, lensdistance, colorcalID, imagescalingID, flatfieldID, cameraRotation;

                x = y = width = height = lensdistance = colorcalID = imagescalingID = flatfieldID = cameraRotation = "";

                if (cbSubframe.Text != "")
                {
                    string[] values = cbSubframe.Text.Split(',');
                    x = values[0];
                    y = values[1];
                    width = values[2];
                    height = values[3];
                }

                if (cbCalBox.Text != "Copy from first step")
                {
                    string[] values = cbCalBox.Text.Split(',');
                    colorcalID = values[1];
                    imagescalingID = values[2];
                    flatfieldID = values[3];
                }

                else
                {
                    node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName");
                    string firstPatternName = node.InnerText;
                    nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
                    for (int index = 0; index < nodes.Count - 1; index++)
                    {
                        if (nodes[index].SelectSingleNode("Name").InnerText == firstPatternName)
                        {
                            colorcalID = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/ColorCalID").InnerText;
                            imagescalingID = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/ImageScalingCalibrationID").InnerText;
                            flatfieldID = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/FlatFieldID").InnerText;
                        }
                    }
                }

                if (cbFocusDistance.Text != "")
                {
                    lensdistance = cbFocusDistance.Text;
                }

                if (cbCameraRotation.Text != "")
                {
                    cameraRotation = cbCameraRotation.Text;
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/LensDistance");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (lensdistance != "")
                    {
                        nodes[index].InnerText = lensdistance;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/X");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (x != "")
                    {
                        nodes[index].InnerText = x;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Location/X");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (x != "")
                    {
                        nodes[index].InnerText = x;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Y");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (y != "")
                    {
                        nodes[index].InnerText = y;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Location/Y");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (y != "")
                    {
                        nodes[index].InnerText = y;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Width");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (width != "")
                    {
                        nodes[index].InnerText = width;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Size/Width");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (width != "")
                    {
                        nodes[index].InnerText = width;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Height");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (height != "")
                    {
                        nodes[index].InnerText = height;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Size/Height");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (height != "")
                    {
                        nodes[index].InnerText = height;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/ColorCalID");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (colorcalID != "")
                    {
                        nodes[index].InnerText = colorcalID;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/ImageScalingCalibrationID");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (imagescalingID != "")
                    {
                        nodes[index].InnerText = imagescalingID;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/FlatFieldID");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (flatfieldID != "")
                    {
                        nodes[index].InnerText = flatfieldID;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraRotation");
                for (int index = 0; index < nodes.Count - 1; index++)
                {
                    if (cameraRotation != "")
                    {
                        nodes[index].InnerText = cameraRotation;
                    }
                }
                xmlDoc.Save(lblSequenceFileName.Text);
            }
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


        private void cmdPinEngineerMode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"pinEngineerModeToTaskBar.bat";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenImageJ_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"ImageJ\ImageJ.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenLanSearch_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"lansearchpro_portable\lansearc.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void cmdOpenUpdates_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string folderpath = @"Updates";
            if (Directory.Exists(folderpath))
            {
                Process.Start(folderpath);
            }
        }

        private void cmdOpenMTFCenter_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"MTF_Center\MTF.exe";
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
            string filepath = @"WPSOffice\et.exe";
            string lcfilepath = @"License Code Combined List.xlsx";
            if (File.Exists(filepath) && File.Exists(lcfilepath))
            {
                Process.Start(filepath, "\"" + lcfilepath + "\"");
            }
        }

        private void cmdCreateFFC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"FFC_Database_Template\Generabe FFC Database.vbs";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
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
            if ( (cbTightVNCPath.Text == @"C:\Program") && File.Exists(@"Installer\tightvncCProgram.exe") )
            {
                ExecuteAsAdmin(@"Installer\tightvncCProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"D:\Program") && File.Exists(@"Installer\tightvncDProgram.exe"))
            {
                ExecuteAsAdmin(@"Installer\tightvncDProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"E:\Program") && File.Exists(@"Installer\tightvncEProgram.exe"))
            {
                ExecuteAsAdmin(@"Installer\tightvncEProgram.exe");
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
        }

        private void cmdCopyDISettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
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

        private void cmdInstallIrfanview_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Installer\iview457_x64_setup silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\iview457_x64_setup silent.bat");
            }
        }

        private void cmdInstallVC12_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Installer\Visual C++ 2012\VC2012_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Installer\Visual C++ 2012\VC2012_silent_install.bat");
            }
        }

        private void cmdInstallVC1519_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat");
            }
        }

        private void cmdInstallKdiff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat");
            }
        }

        private void cmdInstallNPP_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Installer\npp.7.8.1.Installer.x64 silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\npp.7.8.1.Installer.x64 silent.bat");
            }
        }

        private void cmdInstallDotnet48_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Installer\ndp48-x86-x64-allos-enu.exe"))
            {
                ExecuteAsAdmin(@"Installer\ndp48-x86-x64-allos-enu.exe");
            }
        }

        private void cmdInstallMatlab_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Installer\MCR_R2017b_win64_installer.exe"))
            {
                ExecuteAsAdmin(@"Installer\MCR_R2017b_win64_installer.exe");
            }
        }

        private void cmdShareC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"ShareC.bat"))
            {
                ExecuteAsAdmin(@"ShareC.bat");
            }
        }

        private void cmdShareD_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"ShareD.bat"))
            {
                ExecuteAsAdmin(@"ShareD.bat");
            }
        }

        private void cmdShareE_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"ShareE.bat"))
            {
                ExecuteAsAdmin(@"ShareE.bat");
            }
        }

        private void cmdShareRVSData_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"ShareRadiantVisionSystemsData.bat"))
            {
                ExecuteAsAdmin(@"ShareRadiantVisionSystemsData.bat");
            }
        }

        private void cmdShareCProgram_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if ( File.Exists(@"ShareCProgram.bat") && cbProgramFolder.Text == @"C:\Program" )
            {
                ExecuteAsAdmin(@"ShareCProgram.bat");
            }
        }

        private void cmdShareDProgram_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"ShareDProgram.bat"))
            {
                ExecuteAsAdmin(@"ShareDProgram.bat");
            }
        }

        private void cmdShareEProgram_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"ShareEProgram.bat"))
            {
                ExecuteAsAdmin(@"ShareEProgram.bat");
            }
        }

        private void cmdCreateOTPandResult_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Installer\CopyOTP.vbs"))
            {
                Process.Start(@"Installer\CopyOTP.vbs");
            }

        }

        private void cmdSetTTPermission_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"updatePermission.bat"))
            {
                ExecuteAsAdmin(@"updatePermission.bat");
            }
        }

       
        private void cmdInstallWireshark_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Installer\Wireshark-win64-3.4.3.exe"))
            {
                ExecuteAsAdmin(@"Installer\Wireshark-win64-3.4.3.exe");
            }
        }

        private void cmdPinFixturePcFolders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (cbFixture.Text == @"Dooone" && File.Exists(@"pinDoooneFixturePCFolders.bat") && File.Exists(@"pinDoooneFixturePCFolders.ps1"))
            {
                ExecuteAsAdmin(@"pinDoooneFixturePCFolders.bat");
            }
            else if (cbFixture.Text == @"Gooil" && File.Exists(@"pinGooilFixturePCFolders.bat") && File.Exists(@"pinGooilFixturePCFolders.ps1"))
            {
                ExecuteAsAdmin(@"pinGooilFixturePCFolders.bat");
            }
        }

        private void cmdPinCameraPcFolders_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if ( cbFixture.Text==@"Dooone" && File.Exists(@"pinDoooneCameraPCFolders.bat") && File.Exists(@"pinDoooneCameraPCFolders.ps1"))
            {
                ExecuteAsAdmin(@"pinDoooneCameraPCFolders.bat");
            }
            else if (cbFixture.Text == @"Gooil" && File.Exists(@"pinGooilCameraPCFolders.bat") && File.Exists(@"pinGooilCameraPCFolders.ps1"))
            {
                ExecuteAsAdmin(@"pinGooilCameraPCFolders.bat");
            }

        }

        private void cmdUACOff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"disableUAC.bat"))
            {
                ExecuteAsAdmin(@"disableUAC.bat");
            }
        }

        private void cmdFirewallOff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"FirewallOff.bat"))
            {
                ExecuteAsAdmin(@"FirewallOff.bat");
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

        private void cmdCopyRVStolocal_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"CopyScript\copyFilesToLocal.vbs"))
            {
                Process.Start(@"CopyScript\copyFilesToLocal.vbs");
            }

            if (File.Exists(@"CopyScript\copyFoldesToLocal.vbs"))
            {
                Process.Start(@"CopyScript\copyFoldesToLocal.vbs");
            }
        }

        private void cmdPasswordSharingOff_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"PasswordSharingOff.bat"))
            {
                ExecuteAsAdmin(@"PasswordSharingOff.bat");
            }
        }

        private void cmdOneclickSetupCameraPC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"disableUAC.bat"))
            {
                ExecuteAsAdmin(@"disableUAC.bat");
            }

            if (File.Exists(@"PasswordSharingOff.bat"))
            {
                ExecuteAsAdmin(@"PasswordSharingOff.bat");
            }

            if (File.Exists(@"FirewallOff.bat"))
            {
                ExecuteAsAdmin(@"FirewallOff.bat");
            }

            if (File.Exists(@"ShareC.bat"))
            {
                ExecuteAsAdmin(@"ShareC.bat");
            }

            if (File.Exists(@"ShareD.bat"))
            {
                ExecuteAsAdmin(@"ShareD.bat");
            }

            if (File.Exists(@"ShareE.bat"))
            {
                ExecuteAsAdmin(@"ShareE.bat");
            }

            if (File.Exists(@"ShareCProgram.bat") && cbProgramFolder.Text == @"C:\Program")
            {
                ExecuteAsAdmin(@"ShareCProgram.bat");
            }

            else if (File.Exists(@"ShareDProgram.bat") && cbProgramFolder.Text == @"D:\Program")
            {
                ExecuteAsAdmin(@"ShareDProgram.bat");
            }

            else if (File.Exists(@"ShareEProgram.bat") && cbProgramFolder.Text == @"E:\Program")
            {
                ExecuteAsAdmin(@"ShareEProgram.bat");
            }

            if (File.Exists(@"ShareRadiantVisionSystemsData.bat"))
            {
                ExecuteAsAdmin(@"ShareRadiantVisionSystemsData.bat");
            }

            if ((cbTightVNCPath.Text == @"C:\Program") && File.Exists(@"Installer\tightvncCProgram.exe"))
            {
                ExecuteAsAdmin(@"Installer\tightvncCProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"D:\Program") && File.Exists(@"Installer\tightvncDProgram.exe"))
            {
                ExecuteAsAdmin(@"Installer\tightvncDProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"E:\Program") && File.Exists(@"Installer\tightvncEProgram.exe"))
            {
                ExecuteAsAdmin(@"Installer\tightvncEProgram.exe");
            }

            if (cbFixture.Text == @"Dooone" && File.Exists(@"pinDoooneFixturePCFolders.bat") && File.Exists(@"pinDoooneFixturePCFolders.ps1"))
            {
                ExecuteAsAdmin(@"pinDoooneFixturePCFolders.bat");
            }
            else if (cbFixture.Text == @"Gooil" && File.Exists(@"pinGooilFixturePCFolders.bat") && File.Exists(@"pinGooilFixturePCFolders.ps1"))
            {
                ExecuteAsAdmin(@"pinGooilFixturePCFolders.bat");
            }

            if (File.Exists(@"Installer\Visual C++ 2012\VC2012_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Installer\Visual C++ 2012\VC2012_silent_install.bat");
            }

            if (File.Exists(@"Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat");
            }

            if (File.Exists(@"Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat");
            }

            if (File.Exists(@"Installer\iview457_x64_setup silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\iview457_x64_setup silent.bat");
            }

            if (File.Exists(@"Installer\npp.7.8.1.Installer.x64 silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\npp.7.8.1.Installer.x64 silent.bat");
            }

            if (File.Exists(@"Installer\ndp48-x86-x64-allos-enu.exe"))
            {
                ExecuteAsAdmin(@"Installer\ndp48-x86-x64-allos-enu.exe");
            }

            if (File.Exists(@"Installer\MCR_R2017b_win64_installer.exe"))
            {
                ExecuteAsAdmin(@"Installer\MCR_R2017b_win64_installer.exe");
            }
        }

        private void cmdOneclickSetupFixturePC_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"disableUAC.bat"))
            {
                ExecuteAsAdmin(@"disableUAC.bat");
            }

            if (File.Exists(@"PasswordSharingOff.bat"))
            {
                ExecuteAsAdmin(@"PasswordSharingOff.bat");
            }

            if (File.Exists(@"FirewallOff.bat"))
            {
                ExecuteAsAdmin(@"FirewallOff.bat");
            }

            if (File.Exists(@"ShareC.bat"))
            {
                ExecuteAsAdmin(@"ShareC.bat");
            }

            if (File.Exists(@"ShareD.bat"))
            {
                ExecuteAsAdmin(@"ShareD.bat");
            }

            if (File.Exists(@"ShareE.bat"))
            {
                ExecuteAsAdmin(@"ShareE.bat");
            }

            if (File.Exists(@"ShareCProgram.bat") && cbProgramFolder.Text == @"C:\Program")
            {
                ExecuteAsAdmin(@"ShareCProgram.bat");
            }

            else if (File.Exists(@"ShareDProgram.bat") && cbProgramFolder.Text == @"D:\Program")
            {
                ExecuteAsAdmin(@"ShareDProgram.bat");
            }

            else if (File.Exists(@"ShareEProgram.bat") && cbProgramFolder.Text == @"E:\Program")
            {
                ExecuteAsAdmin(@"ShareEProgram.bat");
            }

            if ((cbTightVNCPath.Text == @"C:\Program") && File.Exists(@"Installer\tightvncCProgram.exe"))
            {
                ExecuteAsAdmin(@"Installer\tightvncCProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"D:\Program") && File.Exists(@"Installer\tightvncDProgram.exe"))
            {
                ExecuteAsAdmin(@"Installer\tightvncDProgram.exe");
            }
            else if ((cbTightVNCPath.Text == @"E:\Program") && File.Exists(@"Installer\tightvncEProgram.exe"))
            {
                ExecuteAsAdmin(@"Installer\tightvncEProgram.exe");
            }

            if (cbFixture.Text == @"Dooone" && File.Exists(@"pinDoooneCameraPCFolders.bat") && File.Exists(@"pinDoooneCameraPCFolders.ps1"))
            {
                ExecuteAsAdmin(@"pinDoooneCameraPCFolders.bat");
            }
            else if (cbFixture.Text == @"Gooil" && File.Exists(@"pinGooilCameraPCFolders.bat") && File.Exists(@"pinGooilCameraPCFolders.ps1"))
            {
                ExecuteAsAdmin(@"pinGooilCameraPCFolders.bat");
            }

            if (File.Exists(@"Installer\Visual C++ 2012\VC2012_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Installer\Visual C++ 2012\VC2012_silent_install.bat");
            }

            if (File.Exists(@"Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat"))
            {
                ExecuteAsAdmin(@"Installer\Visual C++ 2015-2019\VC2015-2019_silent_install.bat");
            }

            if (File.Exists(@"Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\KDiff3-64bit-Setup_0.9.98-2 silent.bat");
            }

            if (File.Exists(@"Installer\iview457_x64_setup silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\iview457_x64_setup silent.bat");
            }

            if (File.Exists(@"Installer\npp.7.8.1.Installer.x64 silent.bat"))
            {
                ExecuteAsAdmin(@"Installer\npp.7.8.1.Installer.x64 silent.bat");
            }
        }

        private void cmdOneclickSetupTrueTestSettings_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"Installer\CopyOTP.vbs"))
            {
                Process.Start(@"Installer\CopyOTP.vbs");
            }

            if (File.Exists(@"updatePermission.bat"))
            {
                ExecuteAsAdmin(@"updatePermission.bat");
            }

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

        private void cmdBackupCurrentTT_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"backupCurrentTT.vbs"))
            {
                Process.Start(@"backupCurrentTT.vbs");
            }
        }

        private void cmdShareProgram_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (File.Exists(@"ShareCProgram.bat") && cbProgramFolder.Text == @"C:\Program")
            {
                ExecuteAsAdmin(@"ShareCProgram.bat");
            }

            else if (File.Exists(@"ShareDProgram.bat") && cbProgramFolder.Text == @"D:\Program")
            {
                ExecuteAsAdmin(@"ShareDProgram.bat");
            }

            else if (File.Exists(@"ShareEProgram.bat") && cbProgramFolder.Text == @"E:\Program")
            {
                ExecuteAsAdmin(@"ShareEProgram.bat");
            }
        }

        private void cmdOpenEngineerMode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string filepath = @"EngineerMode.exe";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }
        private void cbTrueTestInstallerList_DropDown(object sender, EventArgs e)
        {
            cbTrueTestInstallerList.Items.Clear();
            String[] exes =
            Directory.GetFiles(@"TrueTest Setup", "*.EXE", SearchOption.AllDirectories)
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
            if (File.Exists(@"backupCurrentTT.vbs"))
            {
                Process.Start(@"backupCurrentTT.vbs");
            }
            if (File.Exists(@"TrueTest Setup\" + cbTrueTestInstallerList.Text))
            {
                ExecuteAsAdmin(@"TrueTest Setup\" + cbTrueTestInstallerList.Text);
            }
        }
    }
}
