using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace EZAE
{
    public partial class SetSequence : Form
    {
        public SetSequence()
        {
            InitializeComponent();
        }

        private void FormLoad(object sender, EventArgs e)
        {
            LoadSubframe();
            var latestfile = new DirectoryInfo("C:\\Radiant Vision Systems Data\\TrueTest\\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
            sequenceName.Text = latestfile.FullName;
        }

        public void LoadSubframe()
        {
            subframeBox.Items.Clear();
            string path = "subframe.txt";
            List<string> lines = new List<string>(File.ReadAllLines(path));
            foreach (string line in lines)
            {
                subframeBox.Items.Add(line);
            }

        }

        private void btnAddSubframe_Click(object sender, EventArgs e)
        {
            string path = "subframe.txt";
            List<string> lines = new List<string>(File.ReadAllLines(path));
            if (subframeBox.Text != "")
                lines.Add(subframeBox.Text);
            File.WriteAllLines(path, lines.Distinct());
            LoadSubframe();
          
        }

        private void btnDelSubframe_Click(object sender, EventArgs e)
        {
            string path = "subframe.txt";
            File.WriteAllLines(path, File.ReadLines(path).Where(l => l != subframeBox.Text).ToList().Distinct());
            LoadSubframe();

        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            XmlNode node;
            XmlNodeList nodes;

            var xmlDoc = new XmlDocument();
            xmlDoc.Load(sequenceName.Text);

            string x, y, width, height, lensdistance, colorcalID, imagescalingID, flatfieldID, cameraRotation;

            x = y = width = height = lensdistance = colorcalID = imagescalingID = flatfieldID = cameraRotation= "";
            
            if (subframeBox.Text != "")
            {
                string[] values = subframeBox.Text.Split(',');
                x = values[0];
                y = values[1];
                width = values[2];
                height = values[3];
            }

            if (calBox.Text != "Copy from first step")
            {
                string[] values = calBox.Text.Split(',');
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

            if (focusBox.Text != "")
            {
                lensdistance = focusBox.Text;
            }

            if (rotationBox.Text != "")
            {
                cameraRotation = rotationBox.Text;
            }

            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup/LensDistance");
            for (int index = 0; index < nodes.Count - 1; index++)
            {
                if (lensdistance !="")
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
            

            xmlDoc.Save(sequenceName.Text);
        }

        private void btnUseLastModified_Click(object sender, EventArgs e)
        {
            var latestfile = new DirectoryInfo("C:\\Radiant Vision Systems Data\\TrueTest\\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
            sequenceName.Text = latestfile.FullName;
            this.Controls.Remove(subframeBox);
            this.Controls.Add(subframeBox);
        }

        private void btnBrowseSequence_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = "C:\\Radiant Vision Systems Data\\TrueTest\\Sequence";
                if (dialog.ShowDialog(this)==DialogResult.OK)
                {
                    sequenceName.Text = dialog.FileName;
                }
            }
            this.Controls.Remove(subframeBox);
            this.Controls.Add(subframeBox);
        }
    }
}
