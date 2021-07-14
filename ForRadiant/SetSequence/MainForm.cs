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
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                lblSequenceFileName.Text = latestfile.FullName;
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

                if (cbSubframe.Text == "Copy from first step")
                {
                    node = xmlDoc.DocumentElement.SelectSingleNode("/Sequence/Items/SequenceItem/PatternSetupName");
                    string firstPatternName = node.InnerText;
                    nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
                    for (int index = 0; index <= nodes.Count - 1; index++)
                    {
                        if (nodes[index].SelectSingleNode("Name").InnerText == firstPatternName)
                        {
                            x = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/SubFrameRegion/X").InnerText;
                            y = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/SubFrameRegion/Y").InnerText;
                            width = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/SubFrameRegion/Width").InnerText;
                            height = nodes[index].SelectSingleNode("CameraSettingsList/CameraSettings/SubFrameRegion/Height").InnerText;
                        }
                    }
                }
                else
                {
                    string[] values = cbSubframe.Text.Split(',');
                    x = values[0];
                    y = values[1];
                    width = values[2];
                    height = values[3];
                }

                if (cbCalBox.Text == "Copy from first step")
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
                    string[] values = cbCalBox.Text.Split(',');
                    colorcalID = values[1];
                    imagescalingID = values[2];
                    flatfieldID = values[3];
                }

                if (cbFocusDistance.Text == "Copy from first step")
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
                    lensdistance = cbFocusDistance.Text;
                }

                if (cbCameraRotation.Text == "Copy from first step")
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
                    cameraRotation = cbCameraRotation.Text;
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/LensDistance");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (lensdistance != "")
                    {
                        nodes[index].InnerText = lensdistance;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/X");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (x != "")
                    {
                        nodes[index].InnerText = x;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Location/X");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (x != "")
                    {
                        nodes[index].InnerText = x;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Y");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (y != "")
                    {
                        nodes[index].InnerText = y;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Location/Y");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (y != "")
                    {
                        nodes[index].InnerText = y;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Width");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (width != "")
                    {
                        nodes[index].InnerText = width;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Size/Width");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (width != "")
                    {
                        nodes[index].InnerText = width;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Height");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (height != "")
                    {
                        nodes[index].InnerText = height;
                    }
                }

                nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/CameraSettingsList/CameraSettings/SubFrameRegion/Size/Height");
                for (int index = 0; index <= nodes.Count - 1; index++)
                {
                    if (height != "")
                    {
                        nodes[index].InnerText = height;
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
                XmlWriter writer = XmlWriter.Create(lblSequenceFileName.Text, settings);
                xmlDoc.Save(writer);
                if (writer != null)
                    writer.Close();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                lblSequenceFileName.Text = latestfile.FullName;
            }
        }
    }
}
