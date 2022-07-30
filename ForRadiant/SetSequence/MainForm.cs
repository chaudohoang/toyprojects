using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SetSequence
{
    public partial class MainForm : Form
    {
        private string subframeregion = "";
        private string lensdistance = "";
        private string fnumber = "";
        private string colorcalID = "";
        private string imagescalingID = "";
        private string flatfieldID = "";
        private string cameraRotation = "";

        public MainForm()
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

        private void cmdUseLastModifiedSequence_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles("*.*", SearchOption.AllDirectories).OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                lblSequencePath.Text = latestfile.FullName;
            }
        }

        private void cmdBrowseSequence_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = @"C:\Radiant Vision Systems Data\TrueTest\Sequence";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    lblSequencePath.Text = dialog.FileName;
                }
            }
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            btnApply.Enabled = false;

            GetValues(lblSequencePath.Text,cbSubframe.Text,cbCalBox.Text,cbFocusDistance.Text,cbFNumber.Text,cbCameraRotation.Text);
            SetValues(lblSequencePath.Text);

            
            try
            {
                string[] additionalTargets;
                additionalTargets = txtAdditionalSequence.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in additionalTargets)
                {
                    SetValues(item);
                }
            }
            catch (Exception)
            {
            }


            Random m_Rnd = new Random();
            Color tempcolor;
            tempcolor = label1.ForeColor;
            while (label1.ForeColor == tempcolor)
            {
                label1.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255));
            }
            label1.Text = "Done!";
            btnApply.Enabled = true;
        }


        private void MainForm_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                lblSequencePath.Text = latestfile.FullName;
            }
        }

        private void btnBrowseAdditional_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = @"C:\Radiant Vision Systems Data\TrueTest\Sequence";
                dialog.Multiselect = true;
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string[] files = dialog.FileNames;
                    foreach (string file in files)
                    {
                        txtAdditionalSequence.Text += file + "\r\n";
                    }
                }
            }
        }

        private void txtAdditionalSequence_DragDrop(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (files != null && files.Length != 0)
            {
                foreach (string file in files)
                {
                    txtAdditionalSequence.Text += file + "\r\n";
                }
            }
        }

        private void txtAdditionalSequence_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void lblAbout_DoubleClick(object sender, EventArgs e)
        {
            Form AboutForm = new About();
            AboutForm.ShowDialog();
        }

        private void GetValues(string xmlFile, string subframeSetting, string calSetting, string focusSetting, string fNumberSetting, string rotationSetting)
        {
            XmlNode node;
            XmlNodeList nodes;

            var xmlDoc = new XmlDocument();
            if (File.Exists(@xmlFile))
            {
                xmlDoc.Load(xmlFile);

                if (subframeSetting == "Copy from first step")
                {
                    node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName");
                    string firstPatternName = node.InnerText;
                    nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
                    for (int index = 0; index <= nodes.Count - 1; index++)
                    {
                        if (nodes[index].SelectSingleNode("Name").InnerText == firstPatternName)
                        {
                            subframeregion = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/SubFrameRegion").InnerText;
                        }
                    }
                }
                else
                {
                    subframeregion = subframeSetting;
                }

                if (calSetting == "Copy from first step")
                {
                    node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName");
                    string firstPatternName = node.InnerText;
                    nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
                    for (int index = 0; index <= nodes.Count - 1; index++)
                    {
                        if (nodes[index].SelectSingleNode("Name").InnerText == firstPatternName)
                        {
                            colorcalID = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/ColorCalID").InnerText;
                            imagescalingID = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/ImageScalingCalibrationID").InnerText;
                            flatfieldID = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/FlatFieldID").InnerText;
                        }
                    }
                }

                else
                {
                    string[] values = calSetting.Split(',');
                    colorcalID = values[1];
                    imagescalingID = values[2];
                    flatfieldID = values[3];
                }

                if (focusSetting == "Copy from first step")
                {
                    node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName");
                    string firstPatternName = node.InnerText;
                    nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
                    for (int index = 0; index <= nodes.Count - 1; index++)
                    {
                        if (nodes[index].SelectSingleNode("Name").InnerText == firstPatternName)
                        {
                            lensdistance = nodes[index].SelectSingleNode("LensDistance").InnerText;
                        }
                    }
                }
                else
                {
                    lensdistance = focusSetting;
                }

                if (fNumberSetting == "Copy from first step")
                {
                    node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName");
                    string firstPatternName = node.InnerText;
                    nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
                    for (int index = 0; index <= nodes.Count - 1; index++)
                    {
                        if (nodes[index].SelectSingleNode("Name").InnerText == firstPatternName)
                        {
                            fnumber = nodes[index].SelectSingleNode("LensfStop").InnerText;
                        }
                    }
                }
                else
                {
                    switch (fNumberSetting)
                    {
                        case ("8.0"):
                            fnumber = "6";

                            break;

                        default:
                            fnumber = cbFNumber.Text;
                            break;
                    }

                }

                if (rotationSetting == "Copy from first step")
                {
                    node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName");
                    string firstPatternName = node.InnerText;
                    nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
                    for (int index = 0; index <= nodes.Count - 1; index++)
                    {
                        if (nodes[index].SelectSingleNode("Name").InnerText == firstPatternName)
                        {
                            cameraRotation = nodes[index].SelectSingleNode("CameraRotation").InnerText;
                        }
                    }
                }
                else
                {
                    cameraRotation = rotationSetting;
                }

            }

        }

        private void SetValues(string xmlFile)
		{
            XmlNodeList nodes;

            var xmlDoc = new XmlDocument();
            if (File.Exists(@xmlFile))
            {
                xmlDoc.Load(xmlFile);
                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/LensDistance");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (lensdistance != "")
                    {
                        nodes[index].InnerText = lensdistance;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/LensfStop");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (fnumber != "")
                    {
                        nodes[index].InnerText = fnumber;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (subframeregion != "")
                    {
                        nodes[index].InnerText = subframeregion;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/ColorCalID");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (colorcalID != "")
                    {
                        nodes[index].InnerText = colorcalID;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/ImageScalingCalibrationID");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (imagescalingID != "")
                    {
                        nodes[index].InnerText = imagescalingID;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/FlatFieldID");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (flatfieldID != "")
                    {
                        nodes[index].InnerText = flatfieldID;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraRotation");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (cameraRotation != "")
                    {
                        nodes[index].InnerText = cameraRotation;
                    }
                }

                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };
                XmlWriter writer = XmlWriter.Create(xmlFile, settings);
                xmlDoc.Save(writer);
                if (writer != null)
                    writer.Close();


            }
        }
    }

}
