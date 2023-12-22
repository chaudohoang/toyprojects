using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace SequenceCheckCS
{


    public partial class MainForm
    {

        private SqlCeConnection conn;
        private string CameraSN;
        private string exePath = My.MyProject.Application.Info.DirectoryPath;

        public MainForm()
        {
            InitializeComponent();
        }
        private void SetVersionInfo()
        {

            var ass = System.Reflection.Assembly.GetExecutingAssembly();
            var ver = ass.GetName().Version;
            var startDate = new DateTime(2000, 1, 1);
            int diffDays = ver.Build;
            var computedDate = startDate.AddDays(diffDays);
            string lastBuilt = computedDate.ToShortDateString();
            // Me.Text = (Me.Text & " " & ver.Major & "." & ver.Minor & "." & ver.Build & "." & ver.Revision & " (" & lastBuilt & ")")
            Text = Text + " " + ver.Major + "." + ver.Minor + "." + ver.Build + "." + ver.Revision;

        }


        private void Form1_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                txtFile1.Text = latestfile.FullName;
                txtFile3.Text = latestfile.FullName;
                string defaultMasterSequence = latestfile.DirectoryName + @"\Master\" + latestfile.Name;
                txtFile2.Text = defaultMasterSequence;
            }
        }

        private void txtFile1_DragOver(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void txtFile1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            txtFile1.Text = files[0];
        }

        private void txtFile2_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            txtFile2.Text = files[0];
        }

        private void txtFile2_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            btnCompare.Enabled = false;
            getCurrentCameraSN();
            CheckForMatchingSequenceParameters(txtFile1.Text, txtFile2.Text);
            CommLogUpdateText(Constants.vbCrLf);
            CompareCalSettings(txtFile1.Text, txtFile2.Text);
            CommLogUpdateText(Constants.vbCrLf);
            btnCompare.Enabled = true;
        }

        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            using (var frm = new OpenFileDialog())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    txtFile1.Text = frm.FileName;
                }
            }
        }

        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            using (var frm = new OpenFileDialog())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    txtFile2.Text = frm.FileName;
                }
            }
        }

        public void CheckForMatchingSequenceParameters(string file1FullPath, string file2FullPath)
        {
            var timeLogString = new List<string>();
            string tempString = "";
            var sw = new Stopwatch();
            sw.Start();
            bool equal = true;
            var log = new List<string>();

            var sw2 = new Stopwatch();
            sw2.Start();

            if (!File.Exists(file1FullPath))
            {
                equal = false;
                CommLogUpdateText("Parameters Check : Sequence 1 is not existed !!!");
                return;
            }
            else if (!File.Exists(file2FullPath))
            {
                equal = false;
                CommLogUpdateText("Parameters Check : Sequence 2 is not existed !!!");

                return;
            }

            else if ((file1FullPath ?? "") == (file2FullPath ?? ""))
            {
                equal = false;
                CommLogUpdateText("Parameters Check : Sequence 1 and Sequence 2 is the same file !!!");
                return;
            }
            sw2.Stop();
            timeLogString.Add("Check sequence files existence : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            sw2.Restart();
            List<string> ignoreList;
            ignoreList = (from s in cbxIgnoreList.Text.Split(',')
                          select s).ToList();

            sw2.Stop();
            timeLogString.Add("Get ignore parameters : " + sw2.ElapsedMilliseconds.ToString() + "ms");
            if (!string.IsNullOrEmpty(cbxIgnoreList.Text))
                timeLogString.Add(cbxIgnoreList.Text);

            var sequence1AnaList = new List<string>();
            var sequence2AnaList = new List<string>();

            XmlNode node1;
            XmlNodeList nodes1;

            XmlNode node2;
            XmlNodeList nodes2;

            var xmlDoc1 = new XmlDocument();
            var xmlDoc2 = new XmlDocument();
            var fileloaded = default(bool);
            while (!fileloaded)
            {
                try
                {
                    xmlDoc1.Load(file1FullPath);
                    xmlDoc2.Load(file2FullPath);
                    fileloaded = true;
                }
                catch (Exception ex)
                {
                    fileloaded = false;
                }
            }

            nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");
            nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");

            sw2.Restart();
            // conpare sequence analysis list
            for (int i1 = 0, loopTo = nodes1.Count - 1; i1 <= loopTo; i1++)
            {
                if (nodes1[i1].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequence1AnaList.Add(nodes1[i1].SelectSingleNode("PatternSetupName").InnerText + "_" + nodes1[i1].SelectSingleNode("Analysis/UserName").InnerText);
                }
            }
            sw2.Stop();
            timeLogString.Add("Get sequence 1 analysis list : " + sw2.ElapsedMilliseconds.ToString() + "ms");
            timeLogString.Add("Sequence 1 analysis : " + string.Join(",", sequence1AnaList));

            sw2.Restart();
            for (int i2 = 0, loopTo1 = nodes2.Count - 1; i2 <= loopTo1; i2++)
            {
                if (nodes2[i2].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequence2AnaList.Add(nodes2[i2].SelectSingleNode("PatternSetupName").InnerText + "_" + nodes2[i2].SelectSingleNode("Analysis/UserName").InnerText);
                }
            }

            sw2.Stop();
            timeLogString.Add("Get sequence 2 analysis list : " + sw2.ElapsedMilliseconds.ToString() + "ms");
            timeLogString.Add("Sequence 2 analysis : " + string.Join(",", sequence2AnaList));


            sw2.Restart();

            if ((string.Join(",", sequence1AnaList) ?? "") != (string.Join(",", sequence2AnaList) ?? ""))
            {
                equal = false;
                CommLogUpdateText("Parameters Check : Analysis list does not match !!!");
                CommLogUpdateText("Parameters Check : Sequence 1 analyses : " + string.Join(",", sequence1AnaList));
                CommLogUpdateText("Parameters Check : Sequence 2 analyses : " + string.Join(",", sequence2AnaList));
                return;
            }
            sw2.Stop();
            timeLogString.Add("Check if 2 sequences having same number of analysis : " + sw2.ElapsedMilliseconds.ToString() + "ms" + Environment.NewLine);

            int SequenceItemCount;
            if (nodes1.Count < nodes2.Count)
            {
                SequenceItemCount = nodes1.Count;
            }
            else
            {
                SequenceItemCount = nodes2.Count;
            }
            for (int index = 0, loopTo2 = SequenceItemCount - 1; index <= loopTo2; index++)
            {
                if (!(nodes1[index].SelectSingleNode("Selected").InnerText.ToLower() == "true") | !(nodes2[index].SelectSingleNode("Selected").InnerText.ToLower() == "true"))
                {
                    continue;
                }
                node1 = nodes1[index].SelectSingleNode("Analysis");
                node2 = nodes2[index].SelectSingleNode("Analysis");

                string seq1AnalysisName = nodes1[index].SelectSingleNode("PatternSetupName").InnerText + "_" + nodes1[index].SelectSingleNode("Analysis/UserName").InnerText;
                string seq2AnalysisName = nodes2[index].SelectSingleNode("PatternSetupName").InnerText + "_" + nodes2[index].SelectSingleNode("Analysis/UserName").InnerText;
                timeLogString.Add("Checking step : " + seq1AnalysisName);
                sw2.Restart();
                // Remove items in ignoreList

                foreach (string item in ignoreList)
                {
                    foreach (XmlNode childNode1 in node1.ChildNodes)
                    {
                        if ((childNode1.Name.ToLower() ?? "") == (item.ToLower() ?? ""))
                        {
                            node1.RemoveChild(childNode1);
                            tempString = tempString + childNode1.Name + ",";
                        }
                    }
                }
                sw2.Stop();
                timeLogString.Add("Sequence 1 remove ignored parameters in Analysis " + seq1AnalysisName + " and removed " + (tempString.Length == 0 ? "nothing" : tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString() + "ms");
                tempString = "";
                sw2.Restart();
                foreach (string item in ignoreList)
                {
                    foreach (XmlNode childNode2 in node2.ChildNodes)
                    {
                        if ((childNode2.Name.ToLower() ?? "") == (item.ToLower() ?? ""))
                        {
                            node2.RemoveChild(childNode2);
                            tempString = tempString + childNode2.Name + ",";
                        }
                    }
                }
                sw2.Stop();
                timeLogString.Add("Sequence 2 remove ignored parameters in Analysis " + seq2AnalysisName + " and removed " + (tempString.Length == 0 ? "nothing" : tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString() + "ms");
                tempString = "";
                sw2.Restart();
                for (int childindex = node1.ChildNodes.Count - 1; childindex >= 0; childindex -= 1)
                {
                    if (node2.SelectSingleNode(node1.ChildNodes[childindex].Name) is null)
                    {
                        tempString = tempString + node1.ChildNodes[childindex].Name + ",";
                        node1.RemoveChild(node1.ChildNodes[childindex]);
                    }
                }
                sw2.Stop();
                bool seq2NeedSorting;
                seq2NeedSorting = tempString.Length == 0 ? false : true;
                timeLogString.Add("Sequence 1 removing extra element in Analysis " + seq1AnalysisName + " and removed " + (tempString.Length == 0 ? "nothing" : tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString() + "ms");
                tempString = "";

                sw2.Restart();
                for (int childIndex = node2.ChildNodes.Count - 1; childIndex >= 0; childIndex -= 1)
                {
                    if (node1.SelectSingleNode(node2.ChildNodes[childIndex].Name) is null)
                    {
                        tempString = tempString + node2.ChildNodes[childIndex].Name + ",";
                        node2.RemoveChild(node2.ChildNodes[childIndex]);
                    }
                }
                sw2.Stop();
                bool seq1NeedSorting;
                seq1NeedSorting = tempString.Length == 0 ? false : true;
                timeLogString.Add("Sequence 2 removing extra element in Analysis " + seq2AnalysisName + " and removed " + (tempString.Length == 0 ? "nothing" : tempString.Trim().Remove(tempString.Length - 1)) + " : " + sw2.ElapsedMilliseconds.ToString() + "ms");
                tempString = "";

                sw2.Restart();

                for (int childIndex = 0, loopTo3 = node1.ChildNodes.Count - 1; childIndex <= loopTo3; childIndex++)
                {
                    if (node1.ChildNodes[childIndex].Name == "FilterList" && node1.ChildNodes[childIndex].SelectSingleNode("FilterList") is not null && node2.ChildNodes[childIndex].SelectSingleNode("FilterList") is not null)
                    {

                        var FLXmlNode1 = node1.ChildNodes[childIndex].SelectSingleNode("FilterList");
                        var FLXmlNode2 = node2.ChildNodes[childIndex].SelectSingleNode("FilterList");

                        for (int FLChildIndex = FLXmlNode1.ChildNodes.Count - 1; FLChildIndex >= 0; FLChildIndex -= 1)
                        {
                            if (FLXmlNode1.ChildNodes[FLChildIndex].SelectSingleNode("Selected").InnerText.ToLower() == "false")
                                continue;
                            for (int FLGrandChildIndex = FLXmlNode1.ChildNodes[FLChildIndex].ChildNodes.Count - 1; FLGrandChildIndex >= 0; FLGrandChildIndex -= 1)
                            {
                                if (FLXmlNode1.ChildNodes[FLChildIndex].ChildNodes[FLGrandChildIndex].Name == "ElapsedMilliseconds")
                                {
                                    FLXmlNode1.ChildNodes[FLChildIndex].RemoveChild(FLXmlNode1.ChildNodes[FLChildIndex].ChildNodes[FLGrandChildIndex]);
                                }
                            }
                        }

                        for (int FLChildIndex = FLXmlNode2.ChildNodes.Count - 1; FLChildIndex >= 0; FLChildIndex -= 1)
                        {
                            if (FLXmlNode2.ChildNodes[FLChildIndex].SelectSingleNode("Selected").InnerText.ToLower() == "false")
                                continue;
                            for (int FLGrandChildIndex = FLXmlNode2.ChildNodes[FLChildIndex].ChildNodes.Count - 1; FLGrandChildIndex >= 0; FLGrandChildIndex -= 1)
                            {
                                if (FLXmlNode2.ChildNodes[FLChildIndex].ChildNodes[FLGrandChildIndex].Name == "ElapsedMilliseconds")
                                {
                                    FLXmlNode2.ChildNodes[FLChildIndex].RemoveChild(FLXmlNode2.ChildNodes[FLChildIndex].ChildNodes[FLGrandChildIndex]);
                                }
                            }
                        }

                        for (int FLChildIndex = 0, loopTo4 = FLXmlNode1.ChildNodes.Count - 1; FLChildIndex <= loopTo4; FLChildIndex++)
                        {
                            if (FLXmlNode1.ChildNodes[FLChildIndex].SelectSingleNode("Selected").InnerText.ToLower() == "false")
                                continue;
                            for (int FLGrandChildIndex = FLXmlNode1.ChildNodes[FLChildIndex].ChildNodes.Count - 1; FLGrandChildIndex >= 0; FLGrandChildIndex -= 1)
                            {
                                if ((FLXmlNode1.ChildNodes[FLChildIndex].ChildNodes[FLGrandChildIndex].InnerText.ToLower() ?? "") != (FLXmlNode2.ChildNodes[FLChildIndex].ChildNodes[FLGrandChildIndex].InnerText.ToLower() ?? ""))
                                {
                                    equal = false;
                                    log.Add("Step : " + seq1AnalysisName + ", FilterList Parameter : " + FLXmlNode1.ChildNodes[FLChildIndex].Name + "/" + FLXmlNode1.ChildNodes[FLChildIndex].ChildNodes[FLGrandChildIndex].Name + ", Sequence 1 Value : " + FLXmlNode1.ChildNodes[FLChildIndex].ChildNodes[FLGrandChildIndex].InnerText + ", Sequence 2 Value : " + FLXmlNode2.ChildNodes[FLChildIndex].ChildNodes[FLGrandChildIndex].InnerText);
                                }
                            }
                        }
                    }

                    else if ((node1.ChildNodes[childIndex].InnerText.ToLower() ?? "") != (node2.ChildNodes[childIndex].InnerText.ToLower() ?? ""))
                    {
                        equal = false;
                        log.Add("Step : " + seq1AnalysisName + ", Parameter : " + node1.ChildNodes[childIndex].Name + ", Sequence 1 Value : " + node1.ChildNodes[childIndex].InnerText + ", Sequence 2 Value : " + node2.ChildNodes[childIndex].InnerText);
                    }

                    tempString = tempString + node1.ChildNodes[childIndex].Name + ",";
                }
                sw2.Stop();

                timeLogString.Add("Comparing done for analysis step : " + seq1AnalysisName + " : " + sw2.ElapsedMilliseconds.ToString() + "ms");
                timeLogString.Add("Compared " + node1.ChildNodes.Count.ToString() + " parameters : " + (tempString.Length == 0 ? "nothing" : tempString.Trim().Remove(tempString.Length - 1)) + Environment.NewLine);
                tempString = "";
            }

            if (equal)
            {
                CommLogUpdateText("NO PARAMETER DIFFERENCE !!!");
            }
            else
            {
                CommLogUpdateText("FOUND PARAMETER DIFFERENCE !!!");
            }

            sw.Stop();
            timeLogString.Add("Total time : " + sw.ElapsedMilliseconds.ToString() + "ms");

            for (int i = 0, loopTo5 = log.Count - 1; i <= loopTo5; i++)
                CommLogUpdateText(log[i]);

            CommLogUpdateText("Compare Parameters Time : " + (sw.ElapsedMilliseconds / 1000d).ToString() + "s");
            // File.WriteAllLines(Path.Combine(Path.GetTempPath(), "Parameter Compare Time.txt"), timeLogString)
            // Process.Start("notepad.exe", Path.Combine(Path.GetTempPath(), "Parameter Compare Time.txt"))

        }

        public void SortElements(XmlNode node)
        {
            bool changed = true;

            while (changed)
            {
                changed = false;

                for (int i = 1, loopTo = node.ChildNodes.Count - 1; i <= loopTo; i++)
                {

                    if (string.Compare(node.ChildNodes[i].Name, node.ChildNodes[i - 1].Name, true) < 0)
                    {
                        node.InsertBefore(node.ChildNodes[i], node.ChildNodes[i - 1]);
                        changed = true;
                    }
                }
            }
        }

        private delegate void UpdateCommLogDelegate(string text);
        private void CommLogUpdateText(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new UpdateCommLogDelegate(CommLogUpdateText), new object[] { text });
                return;
            }
            if ((text ?? "") != Constants.vbCrLf)
            {
                ListBox1.Items.Add(DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + " : " + text);
            }
            else
            {
                ListBox1.Items.Add(text);
            }
            ListBox1.TopIndex = ListBox1.Items.Count - 1;
        }

        private void btnClearlog_Click(object sender, EventArgs e)
        {
            ListBox1.Items.Clear();
        }

        private void CommLogUpdateText2(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new UpdateCommLogDelegate(CommLogUpdateText2), new object[] { text });
                return;
            }
            if ((text ?? "") != Constants.vbCrLf)
            {
                ListBox2.Items.Add(DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + " : " + text);
            }
            else
            {
                ListBox2.Items.Add(text);
            }
            ListBox2.TopIndex = ListBox2.Items.Count - 1;
        }

        private void btnClearlog2_Click(object sender, EventArgs e)
        {
            ListBox2.Items.Clear();
        }
        private void CommLogUpdateText3(string text)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new UpdateCommLogDelegate(CommLogUpdateText3), new object[] { text });
                return;
            }
            if ((text ?? "") != Constants.vbCrLf)
            {
                ListBox3.Items.Add(DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + " : " + text);
            }
            else
            {
                ListBox3.Items.Add(text);
            }
            ListBox3.TopIndex = ListBox3.Items.Count - 1;
        }

        private void btnClearlog3_Click(object sender, EventArgs e)
        {
            ListBox3.Items.Clear();
        }

        private void btnBrowse3_Click(object sender, EventArgs e)
        {
            using (var frm = new OpenFileDialog())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    txtFile3.Text = frm.FileName;
                }
            }
        }

        private void txtFile3_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            txtFile3.Text = files[0];
        }

        private void txtFile3_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            btnCheck.Enabled = false;
            getCurrentCameraSN2();
            CheckSequence(txtFile3.Text);
            if (!string.IsNullOrEmpty(txtAdditionalSequence.Text))
            {
                try
                {
                    string[] additionalTargets = txtAdditionalSequence.Text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string item in additionalTargets)
                    {
                        CommLogUpdateText2(Constants.vbCrLf);
                        CheckSequence(item);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            if (ListBox2.Items.Count > 0 && (ListBox2.Items[ListBox2.Items.Count - 1].ToString() ?? "") != Constants.vbCrLf)
            {
                CommLogUpdateText2(Constants.vbCrLf);
            }
            btnCheck.Enabled = true;

        }

        public void CheckSequence(string InputSequence)
        {
            bool subframeMatch = true;
            bool CalIsNONE = false;
            bool ColorCalNG = false;
            bool FlatFieldCalNG = false;
            bool ImgScaleCalNG = false;
            var logSubframe = new List<string>();
            var logCalNone = new List<string>();
            var logColorCal = new List<string>();
            var logFlatFieldCal = new List<string>();
            var logImgScaleCal = new List<string>();
            var sequenceAnaList = new List<string>();
            var demuraStepList = new List<string>();
            XmlNode node3;
            XmlNodeList nodes3;
            var xmlDoc3 = new XmlDocument();
            string SN = "";
            if (!Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules"))
            {
                Directory.CreateDirectory(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules");
            }
            string ColorCalRuleFilaName = Path.GetFileNameWithoutExtension(InputSequence) + "_colorcal.txt";
            string ColorCalRuleFilePath = Path.Combine(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", ColorCalRuleFilaName);

            var ColorCalRulesDict = new Dictionary<string, string>();

            if (File.Exists(ColorCalRuleFilePath))
            {
                string[] CalRuleContent = File.ReadAllLines(ColorCalRuleFilePath);
                foreach (string line in CalRuleContent)
                {
                    string StepName = line.Split(',')[0];
                    string CalID = line.Split(',')[1];
                    ColorCalRulesDict.Add(StepName, CalID);
                }

            }

            string FlatFieldCalRuleFilaName = Path.GetFileNameWithoutExtension(InputSequence) + "_flatfieldcal.txt";
            string FlatFieldCalRuleFilePath = Path.Combine(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", FlatFieldCalRuleFilaName);

            var FlatFieldCalRulesDict = new Dictionary<string, string>();

            if (File.Exists(FlatFieldCalRuleFilePath))
            {
                string[] CalRuleContent = File.ReadAllLines(FlatFieldCalRuleFilePath);
                foreach (string line in CalRuleContent)
                {
                    string StepName = line.Split(',')[0];
                    string CalID = line.Split(',')[1];
                    FlatFieldCalRulesDict.Add(StepName, CalID);
                }

            }

            string ImgScaleCalRuleFilaName = Path.GetFileNameWithoutExtension(InputSequence) + "_imgscalecal.txt";
            string ImgScaleCalRuleFilePath = Path.Combine(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", ImgScaleCalRuleFilaName);

            var ImgScaleCalRulesDict = new Dictionary<string, string>();

            if (File.Exists(ImgScaleCalRuleFilePath))
            {
                string[] CalRuleContent = File.ReadAllLines(ImgScaleCalRuleFilePath);
                foreach (string line in CalRuleContent)
                {
                    string StepName = line.Split(',')[0];
                    string CalID = line.Split(',')[1];
                    ImgScaleCalRulesDict.Add(StepName, CalID);
                }

            }

            xmlDoc3.Load(InputSequence);
            nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");
            for (int i = 0, loopTo = nodes3.Count - 1; i <= loopTo; i++)
            {
                if (nodes3[i].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequenceAnaList.Add(nodes3[i].SelectSingleNode("PatternSetupName").InnerText);
                }
                if (nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDCustomerAnalysis") | nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDNCustomerAnalysis") && !nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDNPOCB4p1"))
                {
                    demuraStepList.Add(nodes3[i].SelectSingleNode("PatternSetupName").InnerText);
                }
            }
            nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");

            for (int index = 0, loopTo1 = nodes3.Count - 1; index <= loopTo1; index++)
            {
                if (demuraStepList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                    continue;
                if (!string.IsNullOrEmpty(cbxSubframe.Text) && sequenceAnaList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                {
                    node3 = nodes3[index].SelectSingleNode("CameraSettingsList");
                    if (cbCameraSNStyle2.SelectedIndex == 0)
                    {
                        foreach (XmlNode childNode in node3.ChildNodes)
                        {
                            if ((childNode.SelectSingleNode("SerialNumber").InnerText ?? "") != (CameraSN ?? ""))
                            {
                                node3.RemoveChild(childNode);
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode childNode in node3.ChildNodes)
                        {
                            var lastChild = node3.LastChild.Clone();
                            node3.RemoveAll();
                            node3.AppendChild(lastChild);
                        }
                    }
                    try
                    {
                        SN = "";
                        SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    }
                    catch (Exception ex)
                    {
                    }
                    if (string.IsNullOrEmpty(SN))
                    {
                        break;
                    }

                    string subframe = node3.SelectSingleNode("CameraSettings/SubFrameRegion").InnerText;
                    string StepName = nodes3[index].SelectSingleNode("Name").InnerText;
                    if ((subframe ?? "") != (cbxSubframe.Text ?? ""))
                    {
                        subframeMatch = false;
                        logSubframe.Add("SN : " + SN + " , Step : " + StepName + ", Subframe : " + subframe);
                    }

                }

            }

            for (int index = 0, loopTo2 = nodes3.Count - 1; index <= loopTo2; index++)
            {
                if (demuraStepList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                    continue;
                if (chkCalNone.Checked == true && sequenceAnaList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                {
                    node3 = nodes3[index].SelectSingleNode("CameraSettingsList");

                    if (cbCameraSNStyle2.SelectedIndex == 0)
                    {
                        foreach (XmlNode childNode in node3.ChildNodes)
                        {
                            if ((childNode.SelectSingleNode("SerialNumber").InnerText ?? "") != (CameraSN ?? ""))
                            {
                                node3.RemoveChild(childNode);
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode childNode in node3.ChildNodes)
                        {
                            var lastChild = node3.LastChild.Clone();
                            node3.RemoveAll();
                            node3.AppendChild(lastChild);
                        }
                    }
                    try
                    {
                        SN = "";
                        SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    }
                    catch (Exception ex)
                    {
                    }

                    if (string.IsNullOrEmpty(SN))
                    {
                        break;
                    }

                    string CCID = node3.SelectSingleNode("CameraSettings/ColorCalID").InnerText;
                    string IMCID = node3.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText;
                    string FFID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText;
                    string StepName = nodes3[index].SelectSingleNode("Name").InnerText;
                    string log = "";
                    if (Conversions.ToDouble(CCID) == 0d)
                    {
                        CalIsNONE = true;
                        log += " , ColorCalID : " + CCID;
                    }
                    if (Conversions.ToDouble(IMCID) == 0d)
                    {
                        CalIsNONE = true;
                        log += " , ImageScalingID : " + IMCID;
                    }
                    if (Conversions.ToDouble(FFID) == 0d)
                    {
                        CalIsNONE = true;
                        log += " , FlatFieldID : " + FFID;
                    }
                    if (!string.IsNullOrEmpty(log))
                    {
                        logCalNone.Add("SN : " + SN + " , Step : " + StepName + log);
                    }
                }

            }

            for (int index = 0, loopTo3 = nodes3.Count - 1; index <= loopTo3; index++)
            {
                if (demuraStepList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                    continue;
                if (chkColorCalSettings.Checked == true && sequenceAnaList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                {
                    node3 = nodes3[index].SelectSingleNode("CameraSettingsList");
                    if (cbCameraSNStyle2.SelectedIndex == 0)
                    {
                        foreach (XmlNode childNode in node3.ChildNodes)
                        {
                            if ((childNode.SelectSingleNode("SerialNumber").InnerText ?? "") != (CameraSN ?? ""))
                            {
                                node3.RemoveChild(childNode);
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode childNode in node3.ChildNodes)
                        {
                            var lastChild = node3.LastChild.Clone();
                            node3.RemoveAll();
                            node3.AppendChild(lastChild);
                        }
                    }
                    try
                    {
                        SN = "";
                        SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    }
                    catch (Exception ex)
                    {
                    }

                    if (string.IsNullOrEmpty(SN))
                    {
                        break;
                    }

                    string CCID = node3.SelectSingleNode("CameraSettings/ColorCalID").InnerText;
                    string StepName = nodes3[index].SelectSingleNode("Name").InnerText;
                    string log = "";
                    if (ColorCalRulesDict.ContainsKey(StepName) && (ColorCalRulesDict[StepName] ?? "") != (CCID ?? ""))
                    {
                        ColorCalNG = true;
                        log += " , ColorCalID : " + CCID + " , Correct ColorCalID : " + ColorCalRulesDict[StepName];
                    }
                    else if (!ColorCalRulesDict.ContainsKey(StepName))
                    {
                        ColorCalNG = true;
                        log += " , ColorCalID : " + CCID + " , This step has no calibration rule ";
                    }

                    if (!string.IsNullOrEmpty(log))
                    {
                        logColorCal.Add("SN : " + SN + " , Step : " + StepName + log);
                    }
                }

            }

            for (int index = 0, loopTo4 = nodes3.Count - 1; index <= loopTo4; index++)
            {
                if (demuraStepList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                    continue;
                if (chkFlatFieldCalSettings.Checked == true && sequenceAnaList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                {
                    node3 = nodes3[index].SelectSingleNode("CameraSettingsList");

                    if (cbCameraSNStyle2.SelectedIndex == 0)
                    {
                        foreach (XmlNode childNode in node3.ChildNodes)
                        {
                            if ((childNode.SelectSingleNode("SerialNumber").InnerText ?? "") != (CameraSN ?? ""))
                            {
                                node3.RemoveChild(childNode);
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode childNode in node3.ChildNodes)
                        {
                            var lastChild = node3.LastChild.Clone();
                            node3.RemoveAll();
                            node3.AppendChild(lastChild);
                        }
                    }
                    try
                    {
                        SN = "";
                        SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    }
                    catch (Exception ex)
                    {
                    }
                    if (string.IsNullOrEmpty(SN))
                    {
                        break;
                    }

                    string FFID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText;
                    string StepName = nodes3[index].SelectSingleNode("Name").InnerText;
                    string log = "";
                    if (FlatFieldCalRulesDict.ContainsKey(StepName) && (FlatFieldCalRulesDict[StepName] ?? "") != (FFID ?? ""))
                    {
                        FlatFieldCalNG = true;
                        log += " , FlatFieldID : " + FFID + " , Correct FlatFieldID : " + FlatFieldCalRulesDict[StepName];
                    }
                    else if (!FlatFieldCalRulesDict.ContainsKey(StepName))
                    {
                        FlatFieldCalNG = true;
                        log += " , FlatFieldID : " + FFID + " , This step has no calibration rule ";
                    }

                    if (!string.IsNullOrEmpty(log))
                    {
                        logFlatFieldCal.Add("SN : " + SN + " , Step : " + StepName + log);
                    }
                }

            }

            for (int index = 0, loopTo5 = nodes3.Count - 1; index <= loopTo5; index++)
            {
                // If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
                if (chkImgScaleCalSettings.Checked == true && sequenceAnaList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                {
                    node3 = nodes3[index].SelectSingleNode("CameraSettingsList");
                    if (cbCameraSNStyle2.SelectedIndex == 0)
                    {
                        foreach (XmlNode childNode in node3.ChildNodes)
                        {
                            if ((childNode.SelectSingleNode("SerialNumber").InnerText ?? "") != (CameraSN ?? ""))
                            {
                                node3.RemoveChild(childNode);
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode childNode in node3.ChildNodes)
                        {
                            var lastChild = node3.LastChild.Clone();
                            node3.RemoveAll();
                            node3.AppendChild(lastChild);
                        }
                    }
                    try
                    {
                        SN = "";
                        SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    }
                    catch (Exception ex)
                    {
                    }

                    if (string.IsNullOrEmpty(SN))
                    {
                        break;
                    }

                    string ISCID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText;
                    string StepName = nodes3[index].SelectSingleNode("Name").InnerText;

                    string log = "";
                    if (ImgScaleCalRulesDict.ContainsKey(StepName) && (ImgScaleCalRulesDict[StepName] ?? "") != (ISCID ?? ""))
                    {
                        ImgScaleCalNG = true;
                        log += " , ImageScalingCalibration : " + ISCID + " , Correct ImageScalingCalibration : " + ImgScaleCalRulesDict[StepName];
                    }
                    else if (!ImgScaleCalRulesDict.ContainsKey(StepName))
                    {
                        ImgScaleCalNG = true;
                        log += " , ImageScalingCalibration : " + ISCID + " , This step has no calibration rule ";
                    }

                    if (!string.IsNullOrEmpty(log))
                    {
                        logImgScaleCal.Add("SN : " + SN + " , Step : " + StepName + log);
                    }
                }

            }

            if (!string.IsNullOrEmpty(cbxSubframe.Text) | chkCalNone.Checked == true | chkColorCalSettings.Checked == true | chkFlatFieldCalSettings.Checked == true | chkImgScaleCalSettings.Checked == true)
            {
                CommLogUpdateText2("Local Camera SN : " + CameraSN);
                CommLogUpdateText2("Sequence File : " + InputSequence);
                CommLogUpdateText2("Sequence Camera SN : " + SN);
                if (string.IsNullOrEmpty(SN))
                {
                    CommLogUpdateText2("Sequence is not set with local Camera, cannot check with local Camera !");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(cbxSubframe.Text) && subframeMatch)
            {
                CommLogUpdateText2("SUBFRAME MATCH ALL !!!");
            }
            else if (!string.IsNullOrEmpty(cbxSubframe.Text) && !subframeMatch)
            {
                CommLogUpdateText2("SUBFRAME MISMATCH DETECTED !!!");
            }
            else
            {

            }

            for (int i = 0, loopTo6 = logSubframe.Count - 1; i <= loopTo6; i++)
                CommLogUpdateText2(logSubframe[i]);

            if (chkCalNone.Checked == true && !CalIsNONE)
            {
                CommLogUpdateText2("CALIBRATION ALL SET !!!");
            }
            else if (chkCalNone.Checked == true && CalIsNONE)
            {
                CommLogUpdateText2("CALIBRATION NONE DETECTED !!!");
            }
            else
            {
            }

            for (int i = 0, loopTo7 = logCalNone.Count - 1; i <= loopTo7; i++)
                CommLogUpdateText2(logCalNone[i]);

            if (chkColorCalSettings.Checked == true && !ColorCalNG)
            {
                CommLogUpdateText2("COLOR CALIBRATION ALL OK !!!");
            }
            else if (chkColorCalSettings.Checked == true && ColorCalNG)
            {
                CommLogUpdateText2("NG COLOR CALIBRATION DETECTED !!!");
            }
            else
            {
            }

            for (int i = 0, loopTo8 = logColorCal.Count - 1; i <= loopTo8; i++)
                CommLogUpdateText2(logColorCal[i]);

            if (chkFlatFieldCalSettings.Checked == true && !FlatFieldCalNG)
            {
                CommLogUpdateText2("FLAT FIELD CALIBRATION ALL OK !!!");
            }
            else if (chkFlatFieldCalSettings.Checked == true && FlatFieldCalNG)
            {
                CommLogUpdateText2("NG FLAT FIELD CALIBRATION DETECTED !!!");
            }
            else
            {
            }

            for (int i = 0, loopTo9 = logFlatFieldCal.Count - 1; i <= loopTo9; i++)
                CommLogUpdateText2(logFlatFieldCal[i]);

            if (chkImgScaleCalSettings.Checked == true && !ImgScaleCalNG)
            {
                CommLogUpdateText2("IMAGE SCALING CALIBRATION ALL OK !!!");
            }
            else if (chkImgScaleCalSettings.Checked == true && ImgScaleCalNG)
            {
                CommLogUpdateText2("NG IMAGE SCALING CALIBRATION DETECTED !!!");
            }
            else
            {
            }

            for (int i = 0, loopTo10 = logImgScaleCal.Count - 1; i <= loopTo10; i++)
                CommLogUpdateText2(logImgScaleCal[i]);


        }


        private void btnShowSettings_Click(object sender, EventArgs e)
        {
            btnShowSettings.Enabled = false;
            ShowMeasSettings(txtFile3.Text);
            CommLogUpdateText2(Constants.vbCrLf);
            btnShowSettings.Enabled = true;
        }

        public void ShowMeasSettings(string InputSequence)
        {
            var sequenceAnaList = new List<string>();
            var demuraStepList = new List<string>();
            XmlNode node3;
            XmlNodeList nodes3;
            var xmlDoc3 = new XmlDocument();
            xmlDoc3.Load(InputSequence);
            nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");

            for (int i = 0, loopTo = nodes3.Count - 1; i <= loopTo; i++)
            {
                if (nodes3[i].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequenceAnaList.Add(nodes3[i].SelectSingleNode("PatternSetupName").InnerText);
                }
                if (nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDCustomerAnalysis") | nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDNCustomerAnalysis"))
                {
                    demuraStepList.Add(nodes3[i].SelectSingleNode("PatternSetupName").InnerText);
                }
            }
            nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
            CommLogUpdateText2("Sequence File : " + InputSequence);
            CommLogUpdateText2("ALL SETTINGS :");
            for (int index = 0, loopTo1 = nodes3.Count - 1; index <= loopTo1; index++)
            {
                // If demuraStepList.Contains(nodes3(index).SelectSingleNode("Name").InnerText) Then Continue For
                if (sequenceAnaList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                {
                    string FocusDistance = nodes3[index].SelectSingleNode("LensDistance").InnerText;
                    string FNumber = nodes3[index].SelectSingleNode("LensfStop").InnerText;
                    switch (FNumber ?? "")
                    {
                        case "2":
                            {
                                FNumber = "2.0";
                                break;
                            }
                        case "2.5":
                            {
                                FNumber = "2.3";
                                break;
                            }
                        case "3":
                            {
                                FNumber = "2.8";
                                break;
                            }
                        case "3.5":
                            {
                                FNumber = "4.0";
                                break;
                            }
                        case "4":
                            {
                                FNumber = "3.3";
                                break;
                            }
                        case "4.5":
                            {
                                FNumber = "4.7";
                                break;
                            }
                        case "5":
                            {
                                FNumber = "5.6";
                                break;
                            }
                        case "5.5":
                            {
                                FNumber = "6.7";
                                break;
                            }
                        case "6":
                            {
                                FNumber = "8.0";
                                break;
                            }
                        case "6.5":
                            {
                                FNumber = "9.5";
                                break;
                            }
                        case "7":
                            {
                                FNumber = "11";
                                break;
                            }
                        case "7.5":
                            {
                                FNumber = "13";
                                break;
                            }
                        case "8":
                            {
                                FNumber = "16";
                                break;
                            }
                        case "8.5":
                            {
                                FNumber = "19";
                                break;
                            }
                        case "9":
                            {
                                FNumber = "22";
                                break;
                            }

                        default:
                            {
                                break;
                            }

                    }
                    string CameraRotation = nodes3[index].SelectSingleNode("CameraRotation").InnerText;
                    node3 = nodes3[index].SelectSingleNode("CameraSettingsList");
                    foreach (XmlNode childNode in node3.ChildNodes)
                    {
                        var lastChild = node3.LastChild.Clone();
                        node3.RemoveAll();
                        node3.AppendChild(lastChild);
                    }
                    string subframe = node3.SelectSingleNode("CameraSettings/SubFrameRegion").InnerText;
                    string CCID = node3.SelectSingleNode("CameraSettings/ColorCalID").InnerText;
                    string IMCID = node3.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText;
                    string FFID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText;
                    string SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    CommLogUpdateText2("SN : " + SN + " , Step : " + nodes3[index].SelectSingleNode("Name").InnerText + " , Focus : " + FocusDistance + " , F-number : " + FNumber + " , Rotation : " + CameraRotation + " , Subframe : " + subframe + " , ColorCalID : " + CCID + " , ImageScalingID : " + IMCID + " , FlatFieldID : " + FFID);
                }
            }
        }

        public void ShowColorCalSettings(string InputSequence)
        {
            SqlCeConnection conn;
            var cmdCalibration = new SqlCeCommand();
            var daCalibration = new SqlCeDataAdapter();
            var dsCalibration = new DataSet();
            var dtCalibration = new DataTable();
            string SN = "";
            var sequenceAnaList = new List<string>();
            var demuraStepList = new List<string>();
            XmlNode node3;
            XmlNodeList nodes3;
            var xmlDoc3 = new XmlDocument();
            xmlDoc3.Load(InputSequence);
            nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");
            for (int i = 0, loopTo = nodes3.Count - 1; i <= loopTo; i++)
            {
                if (nodes3[i].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequenceAnaList.Add(nodes3[i].SelectSingleNode("PatternSetupName").InnerText);
                }
                if (nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDCustomerAnalysis") | nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDNCustomerAnalysis"))
                {
                    demuraStepList.Add(nodes3[i].SelectSingleNode("PatternSetupName").InnerText);
                }
            }
            nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
            CommLogUpdateText2("Sequence File : " + InputSequence);
            CommLogUpdateText2("COLOR CALIBRATION SETTINGS :");
            for (int index = 0, loopTo1 = nodes3.Count - 1; index <= loopTo1; index++)
            {
                if (demuraStepList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                    continue;
                if (sequenceAnaList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                {

                    node3 = nodes3[index].SelectSingleNode("CameraSettingsList");
                    foreach (XmlNode childNode in node3.ChildNodes)
                    {
                        var lastChild = node3.LastChild.Clone();
                        node3.RemoveAll();
                        node3.AppendChild(lastChild);
                    }

                    string CCID = node3.SelectSingleNode("CameraSettings/ColorCalID").InnerText;
                    SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    CommLogUpdateText2("SN : " + SN + " , Step : " + nodes3[index].SelectSingleNode("Name").InnerText + " , ColorCalID : " + CCID);
                }
            }
            CommLogUpdateText2("COLOR CALIBRATION REFERENCES :");
            try
            {
                conn = (SqlCeConnection)GetConnect(SN);
                cmdCalibration = conn.CreateCommand();
                cmdCalibration.CommandText = "SELECT ColorCalibrationID, Description FROM ColorCalibrations";

                daCalibration.SelectCommand = cmdCalibration;
                daCalibration.Fill(dsCalibration, "ColorCalibrations");
                if (dsCalibration.Tables["ColorCalibrations"].Rows.Count == 0)
                {
                    CommLogUpdateText2("SN : " + SN + " : No user created calibrations ");
                }
                else
                {
                    foreach (DataRow row in dsCalibration.Tables["ColorCalibrations"].Rows)
                        CommLogUpdateText2(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("SN : " + SN + " , ColorCalID : ", row["ColorCalibrationID"]), " , Description : "), row["Description"])));
                }
            }

            catch (Exception ex)
            {
                Interaction.MsgBox("Error: " + ex.Source + ": " + ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!");
            }


        }
        public void ShowFlatFieldCalRefs(string InputSequence)
        {
            SqlCeConnection conn;
            var cmdCalibration = new SqlCeCommand();
            var daCalibration = new SqlCeDataAdapter();
            var dsCalibration = new DataSet();
            var dtCalibration = new DataTable();
            string SN = "";
            var sequenceAnaList = new List<string>();
            var demuraStepList = new List<string>();
            XmlNode node3;
            XmlNodeList nodes3;
            var xmlDoc3 = new XmlDocument();
            xmlDoc3.Load(InputSequence);
            nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");
            for (int i = 0, loopTo = nodes3.Count - 1; i <= loopTo; i++)
            {
                if (nodes3[i].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequenceAnaList.Add(nodes3[i].SelectSingleNode("PatternSetupName").InnerText);
                }
                if (nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDCustomerAnalysis") | nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDNCustomerAnalysis"))
                {
                    demuraStepList.Add(nodes3[i].SelectSingleNode("PatternSetupName").InnerText);
                }
            }
            nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
            CommLogUpdateText2("Sequence File : " + InputSequence);
            CommLogUpdateText2("FLAT FIELD CALIBRATION SETTINGS :");
            for (int index = 0, loopTo1 = nodes3.Count - 1; index <= loopTo1; index++)
            {
                if (demuraStepList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                    continue;
                if (sequenceAnaList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                {

                    node3 = nodes3[index].SelectSingleNode("CameraSettingsList");
                    foreach (XmlNode childNode in node3.ChildNodes)
                    {
                        var lastChild = node3.LastChild.Clone();
                        node3.RemoveAll();
                        node3.AppendChild(lastChild);
                    }

                    string FFID = node3.SelectSingleNode("CameraSettings/FlatFieldID").InnerText;
                    SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    CommLogUpdateText2("SN : " + SN + " , Step : " + nodes3[index].SelectSingleNode("Name").InnerText + " , FlatFieldID : " + FFID);
                }
            }
            CommLogUpdateText2("FLAT FIELD CALIBRATION REFERENCES :");
            try
            {
                conn = (SqlCeConnection)GetConnect(SN);
                cmdCalibration = conn.CreateCommand();
                cmdCalibration.CommandText = "SELECT CalibrationID, CalibrationDesc FROM FlatFieldCalibration";

                daCalibration.SelectCommand = cmdCalibration;
                daCalibration.Fill(dsCalibration, "FlatFieldCalibration");

                if (dsCalibration.Tables["FlatFieldCalibration"].Rows.Count == 0)
                {
                    CommLogUpdateText2("SN : " + SN + " : No user created calibrations ");
                }
                else
                {
                    foreach (DataRow row in dsCalibration.Tables["FlatFieldCalibration"].Rows)
                        CommLogUpdateText2(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("SN : " + SN + " , FlatFieldID : ", row["CalibrationID"]), " , Description : "), row["CalibrationDesc"])));
                }
            }


            catch (Exception ex)
            {
                Interaction.MsgBox("Error: " + ex.Source + ": " + ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!");
            }


        }
        public void ShowImgScaleCalRefs(string InputSequence)
        {
            SqlCeConnection conn;
            var cmdCalibration = new SqlCeCommand();
            var daCalibration = new SqlCeDataAdapter();
            var dsCalibration = new DataSet();
            var dtCalibration = new DataTable();
            string SN = "";
            var sequenceAnaList = new List<string>();
            var demuraStepList = new List<string>();
            XmlNode node3;
            XmlNodeList nodes3;
            var xmlDoc3 = new XmlDocument();
            xmlDoc3.Load(InputSequence);
            nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");
            for (int i = 0, loopTo = nodes3.Count - 1; i <= loopTo; i++)
            {
                if (nodes3[i].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequenceAnaList.Add(nodes3[i].SelectSingleNode("PatternSetupName").InnerText);
                }
                if (nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDCustomerAnalysis") | nodes3[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDNCustomerAnalysis"))
                {
                    demuraStepList.Add(nodes3[i].SelectSingleNode("PatternSetupName").InnerText);
                }
            }
            nodes3 = xmlDoc3.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
            CommLogUpdateText2("Sequence File : " + InputSequence);
            CommLogUpdateText2("IMG SCALING CALIBRATION SETTINGS :");
            for (int index = 0, loopTo1 = nodes3.Count - 1; index <= loopTo1; index++)
            {
                if (demuraStepList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                    continue;
                if (sequenceAnaList.Contains(nodes3[index].SelectSingleNode("Name").InnerText))
                {

                    node3 = nodes3[index].SelectSingleNode("CameraSettingsList");
                    foreach (XmlNode childNode in node3.ChildNodes)
                    {
                        var lastChild = node3.LastChild.Clone();
                        node3.RemoveAll();
                        node3.AppendChild(lastChild);
                    }

                    string ISCID = node3.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText;
                    SN = node3.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    CommLogUpdateText2("SN : " + SN + " , Step : " + nodes3[index].SelectSingleNode("Name").InnerText + " , ImageScalingCalibrationID : " + ISCID);
                }
            }
            CommLogUpdateText2("IMG SCALING CALIBRATION REFERENCES :");
            try
            {
                conn = (SqlCeConnection)GetConnect(SN);
                cmdCalibration = conn.CreateCommand();
                cmdCalibration.CommandText = "SELECT ImageScalingCalibrationID, ImageScalingCalibrationDesc FROM ImageScalingCalibration";

                daCalibration.SelectCommand = cmdCalibration;
                daCalibration.Fill(dsCalibration, "ImageScalingCalibration");

                if (dsCalibration.Tables["ImageScalingCalibration"].Rows.Count == 0)
                {
                    CommLogUpdateText2("SN : " + SN + " : No user created calibrations ");
                }
                else
                {
                    foreach (DataRow row in dsCalibration.Tables["ImageScalingCalibration"].Rows)

                        CommLogUpdateText2(Conversions.ToString(Operators.ConcatenateObject(Operators.ConcatenateObject(Operators.ConcatenateObject("SN : " + SN + " , ImageScalingCalibrationID : ", row["ImageScalingCalibrationID"]), " , Description : "), row["ImageScalingCalibrationDesc"])));
                }
            }

            catch (Exception ex)
            {
                Interaction.MsgBox("Error: " + ex.Source + ": " + ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!");
            }


        }

        private void btnUseLastModified1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                txtFile1.Text = latestfile.FullName;
            }
        }

        private void btnUseDefaultMaster_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                string defaultMasterSequence = latestfile.DirectoryName + @"\Master\" + latestfile.Name;
                txtFile2.Text = defaultMasterSequence;
            }
        }

        private void btnUseLastModified3_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                txtFile3.Text = latestfile.FullName;
            }
        }

        private void btnExportAnalysesCompareLog_Click(object sender, EventArgs e)
        {
            string logPath = Path.Combine(exePath, DateTime.Now.ToString("yyyyMMddHHmmss") + "_Compare.txt");
            if (ListBox1.Items.Count == 0)
            {
                MessageBox.Show("Empty Log !!!");
            }
            else
            {
                var savefile = new SaveFileDialog();
                // set a default file name
                savefile.FileName = Path.GetFileName(logPath);
                // set filters - this can be done in properties as well
                savefile.Filter = "Text files (*.txt)|*.txt";
                savefile.InitialDirectory = exePath;

                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    using (var sw = new StreamWriter(savefile.FileName))
                    {
                        sw.WriteLine("Sequence 1 : " + txtFile1.Text);
                        sw.WriteLine("Sequence 2 : " + txtFile2.Text);
                        sw.WriteLine("Ignore List : " + cbxIgnoreList.Text);

                        for (int index = 0, loopTo = ListBox1.Items.Count - 1; index <= loopTo; index++)
                            sw.WriteLine(ListBox1.Items[index]);
                    }
                    Process.Start("notepad", savefile.FileName);
                }

            }
        }

        private void btnExportMeasurementsCompareLog_Click(object sender, EventArgs e)
        {
            string logPath = Path.Combine(exePath, DateTime.Now.ToString("yyyyMMddHHmmss") + "_Measurement.txt");
            if (ListBox2.Items.Count == 0)
            {
                MessageBox.Show("Empty Log !!!");
            }
            else
            {
                var savefile = new SaveFileDialog();
                // set a default file name
                savefile.FileName = Path.GetFileName(logPath);
                // set filters - this can be done in properties as well
                savefile.Filter = "Text files (*.txt)|*.txt";
                savefile.InitialDirectory = exePath;

                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    using (var sw = new StreamWriter(savefile.FileName))
                    {
                        sw.WriteLine("Sequence : " + txtFile3.Text);
                        sw.WriteLine("Subframe : " + cbxSubframe.Text);
                        sw.WriteLine("Check Calibration NONE : " + (chkCalNone.Checked ? "yes" : "no"));

                        for (int index = 0, loopTo = ListBox2.Items.Count - 1; index <= loopTo; index++)
                            sw.WriteLine(ListBox2.Items[index]);
                    }
                    Process.Start("notepad", savefile.FileName);
                }

            }
        }

        private void btnExportOtherCompareLog_Click(object sender, EventArgs e)
        {
            string logPath = Path.Combine(exePath, DateTime.Now.ToString("yyyyMMddHHmmss") + "_Other.txt");
            if (ListBox3.Items.Count == 0)
            {
                MessageBox.Show("Empty Log !!!");
            }
            else
            {
                var savefile = new SaveFileDialog();
                // set a default file name
                savefile.FileName = Path.GetFileName(logPath);
                // set filters - this can be done in properties as well
                savefile.Filter = "Text files (*.txt)|*.txt";
                savefile.InitialDirectory = exePath;

                if (savefile.ShowDialog() == DialogResult.OK)
                {
                    using (var sw = new StreamWriter(savefile.FileName))
                    {

                        for (int index = 0, loopTo = ListBox3.Items.Count - 1; index <= loopTo; index++)
                            sw.WriteLine(ListBox3.Items[index]);
                    }
                    Process.Start("notepad", savefile.FileName);
                }

            }
        }

        private void btnShowCalSettings_Click(object sender, EventArgs e)
        {
            btnShowCalSettings.Enabled = false;
            ShowColorCalSettings(txtFile3.Text);
            ShowFlatFieldCalRefs(txtFile3.Text);
            ShowImgScaleCalRefs(txtFile3.Text);
            CommLogUpdateText2(Constants.vbCrLf);
            btnShowCalSettings.Enabled = true;

        }

        private void btnEditCalRule_Click(object sender, EventArgs e)
        {
            My.MyProject.Forms.CalRule.Show();
        }

        private void getCurrentCameraSN()
        {

            if (File.Exists(@"C:\Radiant Vision Systems Data\TrueTest\UserData\CameraSN.txt"))
            {
                var fileloaded = default(bool);
                while (!fileloaded)
                {
                    try
                    {
                        CameraSN = File.ReadAllText(@"C:\Radiant Vision Systems Data\TrueTest\UserData\CameraSN.txt");
                        fileloaded = true;
                    }
                    catch (Exception ex)
                    {
                        fileloaded = false;
                    }
                }

            }
            // CommLogUpdateText("Local Camera SN : " + CameraSN)

        }

        private void getCurrentCameraSN2()
        {

            if (File.Exists(@"C:\Radiant Vision Systems Data\TrueTest\UserData\CameraSN.txt"))
            {
                var fileloaded = default(bool);
                while (!fileloaded)
                {
                    try
                    {
                        CameraSN = File.ReadAllText(@"C:\Radiant Vision Systems Data\TrueTest\UserData\CameraSN.txt");
                        fileloaded = true;
                    }
                    catch (Exception ex)
                    {
                        fileloaded = false;
                    }
                }

            }
            // CommLogUpdateText2("Local Camera SN : " + CameraSN)

        }

        public void CompareCalSettings(string file1FullPath, string file2FullPath)
        {
            if (!chkCompareCAL.Checked)
            {
                CommLogUpdateText("SKIPPED CAL COMPARISION !!!");
                return;
            }
            var timeLogString = new List<string>();
            string tempString = "";
            var sw = new Stopwatch();
            sw.Start();
            bool equal = true;
            var log = new List<string>();
            var sw2 = new Stopwatch();
            sw2.Start();

            CommLogUpdateText("Local Camera SN : " + CameraSN);

            if (!File.Exists(file1FullPath))
            {
                equal = false;
                CommLogUpdateText("Calibrations Check : Sequence 1 is not existed !!!");
                return;
            }
            else if (!File.Exists(file2FullPath))
            {
                equal = false;
                CommLogUpdateText("Calibrations Check : Sequence 2 is not existed !!!");

                return;
            }
            else if ((file1FullPath ?? "") == (file2FullPath ?? ""))
            {
                equal = false;
                CommLogUpdateText("Calibrations Check : Sequence 1 and Sequence 2 is the same file !!!");
                return;
            }
            sw2.Stop();
            timeLogString.Add("Check sequence files existence : " + sw2.ElapsedMilliseconds.ToString() + "ms");
            string SN1 = "";
            string SN2 = "";

            var ColorCalSetting1 = new List<string>();
            var ColorCalSetting2 = new List<string>();
            var ImgScaleSetting1 = new List<string>();
            var ImgScaleSetting2 = new List<string>();
            var FFCSetting1 = new List<string>();
            var FFCSetting2 = new List<string>();

            var sequence1AnaList = new List<string>();
            var sequence2AnaList = new List<string>();

            var demuraStepList1 = new List<string>();
            var demuraStepList2 = new List<string>();

            XmlNode node1;
            XmlNodeList nodes1;

            XmlNode node2;
            XmlNodeList nodes2;

            var xmlDoc1 = new XmlDocument();
            var xmlDoc2 = new XmlDocument();
            xmlDoc1.Load(file1FullPath);
            xmlDoc2.Load(file2FullPath);

            nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");
            nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");
            sw2.Restart();

            for (int i = 0, loopTo = nodes1.Count - 1; i <= loopTo; i++)
            {
                if (nodes1[i].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequence1AnaList.Add(nodes1[i].SelectSingleNode("PatternSetupName").InnerText);
                }
                if (nodes1[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDCustomerAnalysis") | nodes1[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDNCustomerAnalysis") && !nodes1[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDNPOCB4p1"))
                {
                    demuraStepList1.Add(nodes1[i].SelectSingleNode("PatternSetupName").InnerText);
                }
            }
            sequence1AnaList = sequence1AnaList.Distinct().ToList();
            sw2.Stop();
            timeLogString.Add("Get sequence 1 analysis list and demura step list (to skip demura step) : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            sw2.Restart();
            nodes1 = xmlDoc1.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
            for (int index = 0, loopTo1 = nodes1.Count - 1; index <= loopTo1; index++)
            {

                if (sequence1AnaList.Contains(nodes1[index].SelectSingleNode("Name").InnerText))
                {
                    if (demuraStepList1.Contains(nodes1[index].SelectSingleNode("Name").InnerText))
                        continue;
                    node1 = nodes1[index].SelectSingleNode("CameraSettingsList");
                    if (cbCameraSNStyle.SelectedIndex == 0)
                    {
                        foreach (XmlNode childNode in node1.ChildNodes)
                        {
                            if ((childNode.SelectSingleNode("SerialNumber").InnerText ?? "") != (CameraSN ?? ""))
                            {
                                node1.RemoveChild(childNode);
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode childNode in node1.ChildNodes)
                        {
                            var lastChild = node1.LastChild.Clone();
                            node1.RemoveAll();
                            node1.AppendChild(lastChild);
                        }
                    }
                    try
                    {
                        SN1 = "";
                        SN1 = node1.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    }
                    catch (Exception ex)
                    {
                    }
                    if (string.IsNullOrEmpty(SN1))
                    {
                        break;
                    }

                    string CCID = node1.SelectSingleNode("CameraSettings/ColorCalID").InnerText;
                    string IMCID = node1.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText;
                    string FFID = node1.SelectSingleNode("CameraSettings/FlatFieldID").InnerText;
                    ColorCalSetting1.Add(SN1 + "," + nodes1[index].SelectSingleNode("Name").InnerText + "," + CCID);
                    ImgScaleSetting1.Add(SN1 + "," + nodes1[index].SelectSingleNode("Name").InnerText + "," + IMCID);
                    FFCSetting1.Add(SN1 + "," + nodes1[index].SelectSingleNode("Name").InnerText + "," + FFID);
                    // log.Add("SN : " + SN1 + " , Step : " + nodes1(index).SelectSingleNode("Name").InnerText + " , ColorCalID : " + CCID + " , ImageScalingID : " + IMCID + " , FlatFieldID : " + FFID)
                }
            }
            sw2.Stop();
            timeLogString.Add("Get sequence 1 Serial Number : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            sw2.Restart();

            for (int i = 0, loopTo2 = nodes2.Count - 1; i <= loopTo2; i++)
            {
                if (nodes2[i].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequence2AnaList.Add(nodes2[i].SelectSingleNode("PatternSetupName").InnerText);
                }
                if (nodes2[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDCustomerAnalysis") | nodes2[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDNCustomerAnalysis") && !nodes2[i].SelectSingleNode("Analysis").Attributes["xsi:type"].Value.Contains("DemuraLGDNPOCB4p1"))
                {
                    demuraStepList2.Add(nodes2[i].SelectSingleNode("PatternSetupName").InnerText);
                }
            }
            sequence2AnaList = sequence2AnaList.Distinct().ToList();
            sw2.Stop();
            timeLogString.Add("Get sequence 2 analysis list and demura step list (to skip demura step) : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            sw2.Restart();
            nodes2 = xmlDoc2.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");
            for (int index = 0, loopTo3 = nodes2.Count - 1; index <= loopTo3; index++)
            {
                if (sequence2AnaList.Contains(nodes2[index].SelectSingleNode("Name").InnerText))
                {
                    if (demuraStepList2.Contains(nodes2[index].SelectSingleNode("Name").InnerText))
                        continue;
                    node2 = nodes2[index].SelectSingleNode("CameraSettingsList");
                    if (cbCameraSNStyle.SelectedIndex == 0)
                    {
                        foreach (XmlNode childNode in node2.ChildNodes)
                        {
                            if ((childNode.SelectSingleNode("SerialNumber").InnerText ?? "") != (CameraSN ?? ""))
                            {
                                node2.RemoveChild(childNode);
                            }
                        }
                    }
                    else
                    {
                        foreach (XmlNode childNode in node2.ChildNodes)
                        {
                            var lastChild = node2.LastChild.Clone();
                            node2.RemoveAll();
                            node2.AppendChild(lastChild);
                        }
                    }
                    try
                    {
                        SN2 = "";
                        SN2 = node2.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                    }
                    catch (Exception ex)
                    {
                    }
                    if (string.IsNullOrEmpty(SN2))
                    {
                        break;
                    }
                    string CCID = node2.SelectSingleNode("CameraSettings/ColorCalID").InnerText;
                    string IMCID = node2.SelectSingleNode("CameraSettings/ImageScalingCalibrationID").InnerText;
                    string FFID = node2.SelectSingleNode("CameraSettings/FlatFieldID").InnerText;
                    ColorCalSetting2.Add(SN2 + "," + nodes2[index].SelectSingleNode("Name").InnerText + "," + CCID);
                    ImgScaleSetting2.Add(SN2 + "," + nodes2[index].SelectSingleNode("Name").InnerText + "," + IMCID);
                    FFCSetting2.Add(SN2 + "," + nodes2[index].SelectSingleNode("Name").InnerText + "," + FFID);
                    // log.Add("SN : " + SN2 + " , Step : " + nodes1(index).SelectSingleNode("Name").InnerText + " , ColorCalID : " + CCID + " , ImageScalingID : " + IMCID + " , FlatFieldID : " + FFID)
                }
            }
            sw2.Stop();
            timeLogString.Add("Get sequence 2 Serial Number : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            if (string.IsNullOrEmpty(SN1))
            {
                CommLogUpdateText("Sequence 1 Camera SN : " + SN1);
                CommLogUpdateText("Sequence 2 Camera SN : " + SN2);
                CommLogUpdateText("Running Sequence is copied but not set calibraion !");
                return;
            }
            if (string.IsNullOrEmpty(SN2))
            {
                CommLogUpdateText("Sequence 1 Camera SN : " + SN1);
                CommLogUpdateText("Sequence 2 Camera SN : " + SN2);
                CommLogUpdateText("Calibration Sequence is copied but not set calibraion !");
                return;
            }

            if (!string.IsNullOrEmpty(SN1) && !string.IsNullOrEmpty(SN2) && (SN1 ?? "") != (SN2 ?? ""))
            {
                CommLogUpdateText("Sequence 1 Camera SN : " + SN1);
                CommLogUpdateText("Sequence 2 Camera SN : " + SN2);
                CommLogUpdateText("2 sequences are set from different cameras !");
                return;
            }

            var colorCalRef1 = new Dictionary<string, string>();
            var colorCalRef2 = new Dictionary<string, string>();

            sw2.Restart();
            GetColorCalRef(file1FullPath, ref colorCalRef1);
            GetColorCalRef(file2FullPath, ref colorCalRef2);
            sw2.Stop();
            timeLogString.Add("Get color calibrations from cal files : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            var imgScaleRef1 = new Dictionary<string, string>();
            var imgScaleRef2 = new Dictionary<string, string>();

            sw2.Restart();
            GetIMGScaleCalRef(file1FullPath, ref imgScaleRef1);
            GetIMGScaleCalRef(file2FullPath, ref imgScaleRef2);
            sw2.Stop();
            timeLogString.Add("Get image scaling calibrations from cal files : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            var flatFieldRef1 = new Dictionary<string, string>();
            var flatFieldRef2 = new Dictionary<string, string>();

            sw2.Restart();
            GetFFCCalRef(file1FullPath, ref flatFieldRef1);
            GetFFCCalRef(file2FullPath, ref flatFieldRef2);
            sw2.Stop();
            timeLogString.Add("Get flat field calibrations from cal files : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            sw2.Restart();
            for (int index = 0, loopTo4 = ColorCalSetting1.Count - 1; index <= loopTo4; index++)
            {
                if (ColorCalSetting1[index].Split(',').Reverse().ElementAtOrDefault(0) == "0")
                {
                    equal = false;
                    log.Add("Step : " + ColorCalSetting1[index].Split(',')[1] + ", Sequence 1 Color Cal is (None)");
                }
                else if ((ColorCalSetting1[index] ?? "") != (ColorCalSetting2[index] ?? ""))
                {
                    equal = false;
                    log.Add("Step : " + ColorCalSetting1[index].Split(',')[1] + ", Sequence 1 Color Cal : " + colorCalRef1[ColorCalSetting1[index].Split(',')[2]] + ", Sequence 2 Color Cal : " + colorCalRef2[ColorCalSetting2[index].Split(',')[2]]);
                }
            }
            sw2.Stop();
            timeLogString.Add("Done checking color calibrations : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            sw2.Restart();
            for (int index = 0, loopTo5 = ImgScaleSetting1.Count - 1; index <= loopTo5; index++)
            {
                if (ImgScaleSetting1[index].Split(',').Reverse().ElementAtOrDefault(0) == "0")
                {
                    equal = false;
                    log.Add("Step : " + ImgScaleSetting1[index].Split(',')[1] + ", Sequence 1 Img Scale Cal is (None)");
                }
                else if ((ImgScaleSetting1[index] ?? "") != (ImgScaleSetting2[index] ?? ""))
                {
                    equal = false;
                    log.Add("Step : " + ImgScaleSetting1[index].Split(',')[1] + ", Sequence 1 Img Scale Cal : " + imgScaleRef1[ImgScaleSetting1[index].Split(',')[2]] + ", Sequence 2 Img Scale Cal : " + imgScaleRef2[ImgScaleSetting2[index].Split(',')[2]]);
                }
            }
            sw2.Stop();
            timeLogString.Add("Done checking image scaling calibrations : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            sw2.Restart();
            for (int index = 0, loopTo6 = FFCSetting1.Count - 1; index <= loopTo6; index++)
            {
                if (FFCSetting1[index].Split(',').Reverse().ElementAtOrDefault(0) == "0")
                {
                    equal = false;
                    log.Add("Step : " + FFCSetting1[index].Split(',')[1] + ", Sequence 1 FFC Cal is (None)");
                }
                else if ((FFCSetting1[index] ?? "") != (FFCSetting2[index] ?? ""))
                {
                    equal = false;
                    log.Add("Step : " + FFCSetting1[index].Split(',')[1] + ", Sequence 1 FFC Cal : " + flatFieldRef1[FFCSetting1[index].Split(',')[2]] + ", Sequence 2 FFC Cal : " + flatFieldRef2[FFCSetting2[index].Split(',')[2]]);
                }
            }
            sw2.Stop();
            timeLogString.Add("Done checking flat field calibrations : " + sw2.ElapsedMilliseconds.ToString() + "ms");

            CommLogUpdateText("Sequence 1 Camera SN : " + SN1);
            CommLogUpdateText("Sequence 2 Camera SN : " + SN2);

            if (equal)
            {
                CommLogUpdateText("NO CAL DIFFERENCE !!!");
            }
            else
            {
                CommLogUpdateText("FOUND CAL DIFFERENCE !!!");
            }

            sw.Stop();
            timeLogString.Add("Total time : " + sw.ElapsedMilliseconds.ToString() + "ms");

            for (int i = 0, loopTo7 = log.Count - 1; i <= loopTo7; i++)
                CommLogUpdateText(log[i]);

            CommLogUpdateText("Compare Calibrations Time : " + (sw.ElapsedMilliseconds / 1000d).ToString() + "s");
            // File.WriteAllLines(Path.Combine(Path.GetTempPath(), "Calibration Compare Time.txt"), timeLogString)
            // Process.Start("notepad.exe", Path.Combine(Path.GetTempPath(), "Calibration Compare Time.txt"))

        }

        public void GetColorCalRef(string SequencePath, ref Dictionary<string, string> RefDict)
        {
            SqlCeConnection conn;
            var cmdColorCalibration = new SqlCeCommand();
            var daColorCalibration = new SqlCeDataAdapter();
            var dsColorCalibration = new DataSet();
            string SN = "";
            var sequenceAnaList = new List<string>();
            var xmlDoc = new XmlDocument();
            XmlNode node;
            XmlNodeList nodes;
            xmlDoc.Load(SequencePath);
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");
            for (int i = 0, loopTo = nodes.Count - 1; i <= loopTo; i++)
            {
                if (nodes[i].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequenceAnaList.Add(nodes[i].SelectSingleNode("PatternSetupName").InnerText);
                }
            }
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");

            for (int index = 0, loopTo1 = nodes.Count - 1; index <= loopTo1; index++)
            {
                if (sequenceAnaList.Contains(nodes[index].SelectSingleNode("Name").InnerText))
                {

                    node = nodes[index].SelectSingleNode("CameraSettingsList");
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        var lastChild = node.LastChild.Clone();
                        node.RemoveAll();
                        node.AppendChild(lastChild);
                    }
                    SN = node.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                }
            }
            try
            {
                dsColorCalibration.Clear();
                conn = (SqlCeConnection)GetConnect(SN);
                cmdColorCalibration = conn.CreateCommand();
                cmdColorCalibration.CommandText = "SELECT ColorCalibrationID, Description FROM ColorCalibrations";
                daColorCalibration.SelectCommand = cmdColorCalibration;
                daColorCalibration.Fill(dsColorCalibration, "ColorCalibrations");
                var newNoneRow = dsColorCalibration.Tables["ColorCalibrations"].NewRow();
                newNoneRow[0] = "0";
                newNoneRow[1] = "(None)";
                dsColorCalibration.Tables["ColorCalibrations"].Rows.InsertAt(newNoneRow, 0);
                var newFactoryRow = dsColorCalibration.Tables["ColorCalibrations"].NewRow();
                newFactoryRow[0] = "-1";
                newFactoryRow[1] = "Factory";
                dsColorCalibration.Tables["ColorCalibrations"].Rows.InsertAt(newFactoryRow, 0);
            }
            catch (Exception ex)
            {
                CommLogUpdateText("Error: " + ex.Source + ": " + ex.Message + "Connection Error !!");
                return;
            }

            foreach (DataRow Row in dsColorCalibration.Tables["ColorCalibrations"].Rows)
                RefDict.Add(Conversions.ToString(Row[0]), Conversions.ToString(Row[1]));
        }

        public void GetIMGScaleCalRef(string SequencePath, ref Dictionary<string, string> RefDict)
        {
            SqlCeConnection conn;
            var cmdImgScaleCalibration = new SqlCeCommand();
            var daImgScaleCalibration = new SqlCeDataAdapter();
            var dsImgScaleCalibration = new DataSet();
            string SN = "";
            var sequenceAnaList = new List<string>();
            var xmlDoc = new XmlDocument();
            XmlNode node;
            XmlNodeList nodes;
            xmlDoc.Load(SequencePath);
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");
            for (int i = 0, loopTo = nodes.Count - 1; i <= loopTo; i++)
            {
                if (nodes[i].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequenceAnaList.Add(nodes[i].SelectSingleNode("PatternSetupName").InnerText);
                }
            }
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");

            for (int index = 0, loopTo1 = nodes.Count - 1; index <= loopTo1; index++)
            {
                if (sequenceAnaList.Contains(nodes[index].SelectSingleNode("Name").InnerText))
                {

                    node = nodes[index].SelectSingleNode("CameraSettingsList");
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        var lastChild = node.LastChild.Clone();
                        node.RemoveAll();
                        node.AppendChild(lastChild);
                    }
                    SN = node.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                }
            }
            try
            {
                dsImgScaleCalibration.Clear();
                conn = (SqlCeConnection)GetConnect(SN);
                cmdImgScaleCalibration = conn.CreateCommand();
                cmdImgScaleCalibration.CommandText = "SELECT ImageScalingCalibrationID, ImageScalingCalibrationDesc FROM ImageScalingCalibration";
                daImgScaleCalibration.SelectCommand = cmdImgScaleCalibration;
                daImgScaleCalibration.Fill(dsImgScaleCalibration, "ImageScalingCalibration");
                var newNoneRow = dsImgScaleCalibration.Tables["ImageScalingCalibration"].NewRow();
                newNoneRow[0] = "0";
                newNoneRow[1] = "(None)";
                dsImgScaleCalibration.Tables["ImageScalingCalibration"].Rows.InsertAt(newNoneRow, 0);
                var newFactoryRow = dsImgScaleCalibration.Tables["ImageScalingCalibration"].NewRow();
                newFactoryRow[0] = "-1";
                newFactoryRow[1] = "Factory";
                dsImgScaleCalibration.Tables["ImageScalingCalibration"].Rows.InsertAt(newFactoryRow, 0);
            }
            catch (Exception ex)
            {
                CommLogUpdateText("Error: " + ex.Source + ": " + ex.Message + "Connection Error !!");
                return;
            }

            foreach (DataRow Row in dsImgScaleCalibration.Tables["ImageScalingCalibration"].Rows)
                RefDict.Add(Conversions.ToString(Row[0]), Conversions.ToString(Row[1]));
        }

        public void GetFFCCalRef(string SequencePath, ref Dictionary<string, string> RefDict)
        {
            SqlCeConnection conn;
            var cmdFlatFieldCalibration = new SqlCeCommand();
            var daImgScaleCalibration = new SqlCeDataAdapter();
            var dsFlatFieldCalibration = new DataSet();
            string SN = "";
            var sequenceAnaList = new List<string>();
            var xmlDoc = new XmlDocument();
            XmlNode node;
            XmlNodeList nodes;
            xmlDoc.Load(SequencePath);
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/Items/SequenceItem");
            for (int i = 0, loopTo = nodes.Count - 1; i <= loopTo; i++)
            {
                if (nodes[i].SelectSingleNode("Selected").InnerText.ToLower() == "true")
                {
                    sequenceAnaList.Add(nodes[i].SelectSingleNode("PatternSetupName").InnerText);
                }
            }
            nodes = xmlDoc.DocumentElement.SelectNodes("/Sequence/PatternSetupList/PatternSetup");

            for (int index = 0, loopTo1 = nodes.Count - 1; index <= loopTo1; index++)
            {
                if (sequenceAnaList.Contains(nodes[index].SelectSingleNode("Name").InnerText))
                {

                    node = nodes[index].SelectSingleNode("CameraSettingsList");
                    foreach (XmlNode childNode in node.ChildNodes)
                    {
                        var lastChild = node.LastChild.Clone();
                        node.RemoveAll();
                        node.AppendChild(lastChild);
                    }
                    SN = node.SelectSingleNode("CameraSettings/SerialNumber").InnerText;
                }
            }
            try
            {
                dsFlatFieldCalibration.Clear();
                conn = (SqlCeConnection)GetConnect(SN);
                cmdFlatFieldCalibration = conn.CreateCommand();
                cmdFlatFieldCalibration.CommandText = "SELECT CalibrationID, CalibrationDesc FROM FlatFieldCalibration";
                daImgScaleCalibration.SelectCommand = cmdFlatFieldCalibration;
                daImgScaleCalibration.Fill(dsFlatFieldCalibration, "FlatFieldCalibration");
                var newNoneRow = dsFlatFieldCalibration.Tables["FlatFieldCalibration"].NewRow();
                newNoneRow[0] = "0";
                newNoneRow[1] = "(None)";
                dsFlatFieldCalibration.Tables["FlatFieldCalibration"].Rows.InsertAt(newNoneRow, 0);
                var newFactoryRow = dsFlatFieldCalibration.Tables["FlatFieldCalibration"].NewRow();
                newFactoryRow[0] = "-1";
                newFactoryRow[1] = "Factory";
                dsFlatFieldCalibration.Tables["FlatFieldCalibration"].Rows.InsertAt(newFactoryRow, 0);
            }
            catch (Exception ex)
            {
                CommLogUpdateText("Error: " + ex.Source + ": " + ex.Message + "Connection Error !!");
                return;
            }

            foreach (DataRow Row in dsFlatFieldCalibration.Tables["FlatFieldCalibration"].Rows)
                RefDict.Add(Conversions.ToString(Row[0]), Conversions.ToString(Row[1]));
        }

        public object GetConnect(object CameraSN)
        {
            SqlCeConnection conn;
            string calFile1 = Path.Combine(@"C:\Radiant Vision Systems Data\Camera Data\Calibration Files", Conversions.ToString(Operators.AddObject(CameraSN, "_CalibrationDB.calx")));
            string calFile2 = Path.Combine(@"C:\Radiant Vision Systems Data\Camera Data\Calibration Files", Conversions.ToString(Operators.AddObject(Operators.AddObject("0", CameraSN), "_CalibrationDB.calx")));
            if (File.Exists(calFile1))
            {
                conn = new SqlCeConnection("Data Source=" + calFile1 + ";Max Database Size=4091");
            }
            else if (File.Exists(calFile2))
            {
                conn = new SqlCeConnection("Data Source=" + calFile2 + ";Max Database Size=4091");
            }
            else
            {
                conn = new SqlCeConnection(@"Data Source=C:\Radiant Vision Systems Data\Camera Data\Calibration Files\PM Calibration Demo Camera.calx;Max Database Size=4091");
            }

            return conn;
        }
        private void CompareAppDataFiles()
        {
            var sw = new Stopwatch();
            sw.Start();
            string AppDataFolder = @"C:\Radiant Vision Systems Data\TrueTest\AppData";
            string MasterAppDataFolder = @"C:\Radiant Vision Systems Data\TrueTest\Master AppData";
            if (!Directory.Exists(MasterAppDataFolder))
            {
                Directory.CreateDirectory(MasterAppDataFolder);
            }
            var files = Directory.EnumerateFiles(MasterAppDataFolder);
            if (files.Count() == 0)
            {
                CommLogUpdateText3("No files in Master AppData folder to compare ! ");
                return;
            }
            var runningFileMissing = default(bool);
            var ngFiles = new List<string>();
            var okFiles = new List<string>();
            if (!File.Exists(Path.Combine(My.MyProject.Application.Info.DirectoryPath, "WinMergeU.exe")))
            {
                MessageBox.Show(Path.Combine(My.MyProject.Application.Info.DirectoryPath, "WinMergeU.exe") + " not existed, copy WinMergeU.exe to same folder and try again !");
                return;
            }
            foreach (string masterFile in files)
            {
                string filename = Path.GetFileName(masterFile);
                string runningFile = Path.Combine(AppDataFolder, filename);
                if (!File.Exists(runningFile))
                {
                    runningFileMissing = true;
                    CommLogUpdateText3("NG AppData file : " + runningFile + " is not existed or deleted !");
                    continue;
                }
                if (new[] { "xml", "csv", "txt" }.Contains(Path.GetExtension(masterFile).Remove(0, 1)))
                {
                    var compareProcess = new Process();
                    var startinfo = new ProcessStartInfo();
                    startinfo.FileName = Path.Combine(My.MyProject.Application.Info.DirectoryPath, "WinMergeU.exe");
                    startinfo.Arguments = "-noninteractive -minimize -enableexitcode -cfg Settings/DiffContextV2=0 " + Conversions.ToString('"') + runningFile + Conversions.ToString('"') + " " + Conversions.ToString('"') + masterFile + Conversions.ToString('"');

                    compareProcess = Process.Start(startinfo);
                    if (compareProcess.WaitForExit(15000))
                    {
                        int ExitCode = compareProcess.ExitCode;
                        if (ExitCode != 0)
                        {
                            ngFiles.Add(runningFile);
                        }
                        else
                        {
                            okFiles.Add(runningFile);
                        }
                    }
                    else
                    {
                        CommLogUpdateText3("Timed out comparing appdata files");
                    }
                }

                else if (new[] { "sdf" }.Contains(Path.GetExtension(masterFile).Remove(0, 1)))
                {
                    var masterInfo = new FileInfo(masterFile);
                    var runningInfo = new FileInfo(runningFile);
                    long masterSize = masterInfo.Length;
                    long runningSize = runningInfo.Length;
                    if (masterSize != runningSize)
                    {
                        ngFiles.Add(runningFile);
                    }
                    else
                    {
                        okFiles.Add(runningFile);
                    }
                }

            }
            if (okFiles.Count > 0)
            {
                foreach (string item in okFiles)
                    CommLogUpdateText3("OK AppData file : " + item);
            }
            if (ngFiles.Count > 0 | runningFileMissing)
            {
                foreach (string item in ngFiles)
                    CommLogUpdateText3("NG AppData file : " + item);
                CommLogUpdateText3("Checking Appdata File Finished, NG");
            }
            else
            {
                CommLogUpdateText3("Checking Appdata File Finished, OK");
            }
            sw.Stop();
            CommLogUpdateText3("Elapsed time : " + (sw.ElapsedMilliseconds / 1000d).ToString() + " seconds.");
        }

        private void btnCompareAppdata_Click(object sender, EventArgs e)
        {
            CompareAppDataFiles();
        }

        private void btnBrowseAdditional_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = @"C:\Radiant Vision Systems Data\TrueTest\Sequence";
                dialog.Multiselect = true;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string[] files = dialog.FileNames;

                    foreach (string file in files)
                        txtAdditionalSequence.Text += file + Constants.vbCrLf;
                }
            }
        }

        private void txtAdditionalSequence_DragDrop(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files is not null && files.Length != 0)
            {
                foreach (string file in files)
                    txtAdditionalSequence.Text += file + Constants.vbCrLf;
            }
        }

        private void txtAdditionalSequence_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void btnClearAdditional_Click(object sender, EventArgs e)
        {
            txtAdditionalSequence.Text = "";
        }
    }
}