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

        private void btnCreateFFC_Click(object sender, EventArgs e)
        {
            string filepath = @"FFC_Database_Template\Generabe FFC Database.vbs";
            if (File.Exists(filepath));
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

        private void btnPinEngineerMode_Click(object sender, EventArgs e)
        {
            string filepath = @"Tools\pinEngineerModeToTaskBar.bat";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnDooone1_Click(object sender, EventArgs e)
        {
            string filepath = @"C:\Windows\dooone1.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnDooone2_Click(object sender, EventArgs e)
        {
            string filepath = @"C:\Windows\dooone2.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnDooone3_Click(object sender, EventArgs e)
        {
            string filepath = @"C:\Windows\dooone3.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnDooone4_Click(object sender, EventArgs e)
        {
            string filepath = @"C:\Windows\dooone4.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnGooil1_Click(object sender, EventArgs e)
        {
            string filepath = @"C:\Windows\gooil1.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnGooil2_Click(object sender, EventArgs e)
        {
            string filepath = @"C:\Windows\gooil2.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnGooil3_Click(object sender, EventArgs e)
        {
            string filepath = @"C:\Windows\gooil3.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
        }

        private void btnGooil4_Click(object sender, EventArgs e)
        {
            string filepath = @"C:\Windows\gooil4.lnk";
            if (File.Exists(filepath))
            {
                Process.Start(filepath);
            }
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

        private void btnUseLastModified_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                lblSequenceFileName.Text = latestfile.FullName;
            }
            this.Controls.Remove(cbSubframe);
            this.Controls.Add(cbSubframe);
        }

        private void btnBrowseSequence_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = @"C:\Radiant Vision Systems Data\TrueTest\Sequence";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    lblSequenceFileName.Text = dialog.FileName;
                }
            }
            this.Controls.Remove(cbSubframe);
            this.Controls.Add(cbSubframe);
        }

        private void btnInstallTightVNC_Click(object sender, EventArgs e)
        {
            if (cbTightVNCPath.Text == @"D:\Program")
            {
                ExecuteAsAdmin(@"Installer\tightvncDProgram.exe");
            }
            else if (cbTightVNCPath.Text == @"D:\Program")
            {
                ExecuteAsAdmin(@"Installer\tightvncCProgram.exe");
            }
            else if (cbTightVNCPath.Text == @"E:\Program")
            {
                ExecuteAsAdmin(@"Installer\tightvncEProgram.exe");
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

        private void btnInstallWireshark_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Installer\Wireshark-win64-3.4.3.exe"))
            {
                ExecuteAsAdmin(@"Installer\Wireshark-win64-3.4.3.exe");
            }
        }

        private void btnShareCProgram_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\ShareCProgram.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareCProgram.bat");
            }
        }

        private void btnShareEProgram_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\ShareEProgram.bat"))
            {
                ExecuteAsAdmin(@"Tools\ShareEProgram.bat");
            }
        }

        private void btnCopyAllToLocalProgram_Click(object sender, EventArgs e)
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

        private void btnUACOff_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\disableUAC.bat"))
            {
                ExecuteAsAdmin(@"Tools\disableUAC.bat");
            }
        }

        private void btnPasswordSharingOff_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\PasswordSharingOff.bat"))
            {
                ExecuteAsAdmin(@"Tools\PasswordSharingOff.bat");
            }
        }

        private void btnFirewallOff_Click(object sender, EventArgs e)
        {
            if (File.Exists(@"Tools\FirewallOff.bat"))
            {
                ExecuteAsAdmin(@"Tools\FirewallOff.bat");
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
    }
}
