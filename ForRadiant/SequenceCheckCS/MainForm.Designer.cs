using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SequenceCheckCS
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class MainForm : Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is not null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            TabPage3 = new TabPage();
            btnExportOtherCompareLog = new Button();
            btnExportOtherCompareLog.Click += new EventHandler(btnExportOtherCompareLog_Click);
            btnClearlog3 = new Button();
            btnClearlog3.Click += new EventHandler(btnClearlog3_Click);
            ListBox3 = new ListBox();
            Label8 = new Label();
            btnCompareAppdata = new Button();
            btnCompareAppdata.Click += new EventHandler(btnCompareAppdata_Click);
            TabPage2 = new TabPage();
            cbCameraSNStyle2 = new ComboBox();
            btnClearAdditional = new Button();
            btnClearAdditional.Click += new EventHandler(btnClearAdditional_Click);
            btnBrowseAdditional = new Button();
            btnBrowseAdditional.Click += new EventHandler(btnBrowseAdditional_Click);
            txtAdditionalSequence = new TextBox();
            txtAdditionalSequence.DragDrop += new DragEventHandler(txtAdditionalSequence_DragDrop);
            txtAdditionalSequence.DragEnter += new DragEventHandler(txtAdditionalSequence_DragEnter);
            lblAddtionalTarget = new Label();
            chkImgScaleCalSettings = new CheckBox();
            chkFlatFieldCalSettings = new CheckBox();
            btnEditCalRule = new Button();
            btnEditCalRule.Click += new EventHandler(btnEditCalRule_Click);
            chkColorCalSettings = new CheckBox();
            btnShowCalSettings = new Button();
            btnShowCalSettings.Click += new EventHandler(btnShowCalSettings_Click);
            btnExportMeasurementsCompareLog = new Button();
            btnExportMeasurementsCompareLog.Click += new EventHandler(btnExportMeasurementsCompareLog_Click);
            btnUseLastModified3 = new Button();
            btnUseLastModified3.Click += new EventHandler(btnUseLastModified3_Click);
            btnShowSettings = new Button();
            btnShowSettings.Click += new EventHandler(btnShowSettings_Click);
            btnClearlog2 = new Button();
            btnClearlog2.Click += new EventHandler(btnClearlog2_Click);
            btnCheck = new Button();
            btnCheck.Click += new EventHandler(btnCheck_Click);
            chkCalNone = new CheckBox();
            cbxSubframe = new ComboBox();
            Label7 = new Label();
            ListBox2 = new ListBox();
            Label6 = new Label();
            Label5 = new Label();
            txtFile3 = new TextBox();
            txtFile3.DragDrop += new DragEventHandler(txtFile3_DragDrop);
            txtFile3.DragOver += new DragEventHandler(txtFile3_DragOver);
            btnBrowse3 = new Button();
            btnBrowse3.Click += new EventHandler(btnBrowse3_Click);
            TabPage1 = new TabPage();
            cbCameraSNStyle = new ComboBox();
            chkCompareCAL = new CheckBox();
            btnExportAnalysesCompareLog = new Button();
            btnExportAnalysesCompareLog.Click += new EventHandler(btnExportAnalysesCompareLog_Click);
            btnUseDefaultMaster = new Button();
            btnUseDefaultMaster.Click += new EventHandler(btnUseDefaultMaster_Click);
            btnUseLastModified1 = new Button();
            btnUseLastModified1.Click += new EventHandler(btnUseLastModified1_Click);
            cbxIgnoreList = new ComboBox();
            Label2 = new Label();
            Label1 = new Label();
            btnCompare = new Button();
            btnCompare.Click += new EventHandler(btnCompare_Click);
            btnClearlog = new Button();
            btnClearlog.Click += new EventHandler(btnClearlog_Click);
            txtFile2 = new TextBox();
            txtFile2.DragDrop += new DragEventHandler(txtFile2_DragDrop);
            txtFile2.DragOver += new DragEventHandler(txtFile2_DragOver);
            txtFile1 = new TextBox();
            txtFile1.DragOver += new DragEventHandler(txtFile1_DragOver);
            txtFile1.DragDrop += new DragEventHandler(txtFile1_DragDrop);
            ListBox1 = new ListBox();
            btnBrowse2 = new Button();
            btnBrowse2.Click += new EventHandler(btnBrowse2_Click);
            Label3 = new Label();
            btnBrowse1 = new Button();
            btnBrowse1.Click += new EventHandler(btnBrowse1_Click);
            Label4 = new Label();
            TabControl1 = new TabControl();
            TabPage3.SuspendLayout();
            TabPage2.SuspendLayout();
            TabPage1.SuspendLayout();
            TabControl1.SuspendLayout();
            SuspendLayout();
            // 
            // TabPage3
            // 
            TabPage3.Controls.Add(btnExportOtherCompareLog);
            TabPage3.Controls.Add(btnClearlog3);
            TabPage3.Controls.Add(ListBox3);
            TabPage3.Controls.Add(Label8);
            TabPage3.Controls.Add(btnCompareAppdata);
            TabPage3.Location = new Point(4, 22);
            TabPage3.Name = "TabPage3";
            TabPage3.Padding = new Padding(3);
            TabPage3.Size = new Size(951, 562);
            TabPage3.TabIndex = 2;
            TabPage3.Text = "Other Compare";
            TabPage3.UseVisualStyleBackColor = true;
            // 
            // btnExportOtherCompareLog
            // 
            btnExportOtherCompareLog.Location = new Point(6, 298);
            btnExportOtherCompareLog.Name = "btnExportOtherCompareLog";
            btnExportOtherCompareLog.Size = new Size(65, 79);
            btnExportOtherCompareLog.TabIndex = 17;
            btnExportOtherCompareLog.Text = "Export log";
            btnExportOtherCompareLog.UseVisualStyleBackColor = true;
            // 
            // btnClearlog3
            // 
            btnClearlog3.Location = new Point(6, 213);
            btnClearlog3.Name = "btnClearlog3";
            btnClearlog3.Size = new Size(65, 79);
            btnClearlog3.TabIndex = 16;
            btnClearlog3.Text = "Clear log";
            btnClearlog3.UseVisualStyleBackColor = true;
            // 
            // ListBox3
            // 
            ListBox3.FormattingEnabled = true;
            ListBox3.HorizontalScrollbar = true;
            ListBox3.Location = new Point(77, 175);
            ListBox3.Name = "ListBox3";
            ListBox3.ScrollAlwaysVisible = true;
            ListBox3.Size = new Size(868, 381);
            ListBox3.TabIndex = 14;
            // 
            // Label8
            // 
            Label8.AutoSize = true;
            Label8.Location = new Point(12, 175);
            Label8.Name = "Label8";
            Label8.Size = new Size(25, 13);
            Label8.TabIndex = 15;
            Label8.Text = "Log";
            // 
            // btnCompareAppdata
            // 
            btnCompareAppdata.Location = new Point(6, 6);
            btnCompareAppdata.Name = "btnCompareAppdata";
            btnCompareAppdata.Size = new Size(113, 26);
            btnCompareAppdata.TabIndex = 0;
            btnCompareAppdata.Text = "Compare AppData";
            btnCompareAppdata.UseVisualStyleBackColor = true;
            // 
            // TabPage2
            // 
            TabPage2.Controls.Add(cbCameraSNStyle2);
            TabPage2.Controls.Add(btnClearAdditional);
            TabPage2.Controls.Add(btnBrowseAdditional);
            TabPage2.Controls.Add(txtAdditionalSequence);
            TabPage2.Controls.Add(lblAddtionalTarget);
            TabPage2.Controls.Add(chkImgScaleCalSettings);
            TabPage2.Controls.Add(chkFlatFieldCalSettings);
            TabPage2.Controls.Add(btnEditCalRule);
            TabPage2.Controls.Add(chkColorCalSettings);
            TabPage2.Controls.Add(btnShowCalSettings);
            TabPage2.Controls.Add(btnExportMeasurementsCompareLog);
            TabPage2.Controls.Add(btnUseLastModified3);
            TabPage2.Controls.Add(btnShowSettings);
            TabPage2.Controls.Add(btnClearlog2);
            TabPage2.Controls.Add(btnCheck);
            TabPage2.Controls.Add(chkCalNone);
            TabPage2.Controls.Add(cbxSubframe);
            TabPage2.Controls.Add(Label7);
            TabPage2.Controls.Add(ListBox2);
            TabPage2.Controls.Add(Label6);
            TabPage2.Controls.Add(Label5);
            TabPage2.Controls.Add(txtFile3);
            TabPage2.Controls.Add(btnBrowse3);
            TabPage2.Location = new Point(4, 22);
            TabPage2.Name = "TabPage2";
            TabPage2.Padding = new Padding(3);
            TabPage2.Size = new Size(951, 562);
            TabPage2.TabIndex = 1;
            TabPage2.Text = "Check Sequence Measurement";
            TabPage2.UseVisualStyleBackColor = true;
            // 
            // cbCameraSNStyle2
            // 
            cbCameraSNStyle2.FormattingEnabled = true;
            cbCameraSNStyle2.Items.AddRange(new object[] { "Use local Camera SN", "Use sequence last saved Camera SN" });
            cbCameraSNStyle2.Location = new Point(337, 140);
            cbCameraSNStyle2.Name = "cbCameraSNStyle2";
            cbCameraSNStyle2.Size = new Size(170, 21);
            cbCameraSNStyle2.TabIndex = 81;
            cbCameraSNStyle2.Text = "Use local Camera SN";
            // 
            // btnClearAdditional
            // 
            btnClearAdditional.Location = new Point(754, 49);
            btnClearAdditional.Name = "btnClearAdditional";
            btnClearAdditional.Size = new Size(76, 58);
            btnClearAdditional.TabIndex = 80;
            btnClearAdditional.Text = "Clear";
            btnClearAdditional.UseVisualStyleBackColor = true;
            // 
            // btnBrowseAdditional
            // 
            btnBrowseAdditional.Location = new Point(672, 49);
            btnBrowseAdditional.Name = "btnBrowseAdditional";
            btnBrowseAdditional.Size = new Size(76, 58);
            btnBrowseAdditional.TabIndex = 80;
            btnBrowseAdditional.Text = "Browse";
            btnBrowseAdditional.UseVisualStyleBackColor = true;
            // 
            // txtAdditionalSequence
            // 
            txtAdditionalSequence.AllowDrop = true;
            txtAdditionalSequence.Location = new Point(6, 49);
            txtAdditionalSequence.Multiline = true;
            txtAdditionalSequence.Name = "txtAdditionalSequence";
            txtAdditionalSequence.ScrollBars = ScrollBars.Both;
            txtAdditionalSequence.Size = new Size(660, 60);
            txtAdditionalSequence.TabIndex = 79;
            // 
            // lblAddtionalTarget
            // 
            lblAddtionalTarget.AutoSize = true;
            lblAddtionalTarget.Location = new Point(6, 33);
            lblAddtionalTarget.Name = "lblAddtionalTarget";
            lblAddtionalTarget.Size = new Size(148, 13);
            lblAddtionalTarget.TabIndex = 78;
            lblAddtionalTarget.Text = "Additional Target Sequence : ";
            // 
            // chkImgScaleCalSettings
            // 
            chkImgScaleCalSettings.AutoSize = true;
            chkImgScaleCalSettings.Location = new Point(513, 153);
            chkImgScaleCalSettings.Name = "chkImgScaleCalSettings";
            chkImgScaleCalSettings.Size = new Size(163, 17);
            chkImgScaleCalSettings.TabIndex = 27;
            chkImgScaleCalSettings.Text = "Check ImgScale Cal Settings";
            chkImgScaleCalSettings.UseVisualStyleBackColor = true;
            // 
            // chkFlatFieldCalSettings
            // 
            chkFlatFieldCalSettings.AutoSize = true;
            chkFlatFieldCalSettings.Location = new Point(513, 135);
            chkFlatFieldCalSettings.Name = "chkFlatFieldCalSettings";
            chkFlatFieldCalSettings.Size = new Size(158, 17);
            chkFlatFieldCalSettings.TabIndex = 26;
            chkFlatFieldCalSettings.Text = "Check FlatField Cal Settings";
            chkFlatFieldCalSettings.UseVisualStyleBackColor = true;
            // 
            // btnEditCalRule
            // 
            btnEditCalRule.Location = new Point(669, 113);
            btnEditCalRule.Name = "btnEditCalRule";
            btnEditCalRule.Size = new Size(90, 23);
            btnEditCalRule.TabIndex = 25;
            btnEditCalRule.Text = "Edit cal rules";
            btnEditCalRule.UseVisualStyleBackColor = true;
            // 
            // chkColorCalSettings
            // 
            chkColorCalSettings.AutoSize = true;
            chkColorCalSettings.Location = new Point(513, 117);
            chkColorCalSettings.Name = "chkColorCalSettings";
            chkColorCalSettings.Size = new Size(143, 17);
            chkColorCalSettings.TabIndex = 24;
            chkColorCalSettings.Text = "Check Color Cal Settings";
            chkColorCalSettings.UseVisualStyleBackColor = true;
            // 
            // btnShowCalSettings
            // 
            btnShowCalSettings.Location = new Point(846, 142);
            btnShowCalSettings.Name = "btnShowCalSettings";
            btnShowCalSettings.Size = new Size(99, 23);
            btnShowCalSettings.TabIndex = 23;
            btnShowCalSettings.Text = "Show cal settings";
            btnShowCalSettings.UseVisualStyleBackColor = true;
            // 
            // btnExportMeasurementsCompareLog
            // 
            btnExportMeasurementsCompareLog.Location = new Point(6, 298);
            btnExportMeasurementsCompareLog.Name = "btnExportMeasurementsCompareLog";
            btnExportMeasurementsCompareLog.Size = new Size(65, 79);
            btnExportMeasurementsCompareLog.TabIndex = 18;
            btnExportMeasurementsCompareLog.Text = "Export log";
            btnExportMeasurementsCompareLog.UseVisualStyleBackColor = true;
            // 
            // btnUseLastModified3
            // 
            btnUseLastModified3.Location = new Point(672, 8);
            btnUseLastModified3.Name = "btnUseLastModified3";
            btnUseLastModified3.Size = new Size(87, 23);
            btnUseLastModified3.TabIndex = 17;
            btnUseLastModified3.Text = "Last Modified";
            btnUseLastModified3.UseVisualStyleBackColor = true;
            // 
            // btnShowSettings
            // 
            btnShowSettings.Location = new Point(846, 113);
            btnShowSettings.Name = "btnShowSettings";
            btnShowSettings.Size = new Size(99, 23);
            btnShowSettings.TabIndex = 16;
            btnShowSettings.Text = "Show all settings";
            btnShowSettings.UseVisualStyleBackColor = true;
            // 
            // btnClearlog2
            // 
            btnClearlog2.Location = new Point(6, 213);
            btnClearlog2.Name = "btnClearlog2";
            btnClearlog2.Size = new Size(65, 79);
            btnClearlog2.TabIndex = 15;
            btnClearlog2.Text = "Clear log";
            btnClearlog2.UseVisualStyleBackColor = true;
            // 
            // btnCheck
            // 
            btnCheck.Location = new Point(846, 8);
            btnCheck.Name = "btnCheck";
            btnCheck.Size = new Size(99, 23);
            btnCheck.TabIndex = 14;
            btnCheck.Text = "Check";
            btnCheck.UseVisualStyleBackColor = true;
            // 
            // chkCalNone
            // 
            chkCalNone.AutoSize = true;
            chkCalNone.Location = new Point(364, 117);
            chkCalNone.Name = "chkCalNone";
            chkCalNone.Size = new Size(143, 17);
            chkCalNone.TabIndex = 13;
            chkCalNone.Text = "Check Calibration NONE";
            chkCalNone.UseVisualStyleBackColor = true;
            // 
            // cbxSubframe
            // 
            cbxSubframe.FormattingEnabled = true;
            cbxSubframe.Items.AddRange(new object[] { "", "800,450,2784,5676", "700,250,2984,6076", "1000,1300,4576,1784" });
            cbxSubframe.Location = new Point(77, 115);
            cbxSubframe.Name = "cbxSubframe";
            cbxSubframe.Size = new Size(281, 21);
            cbxSubframe.TabIndex = 12;
            // 
            // Label7
            // 
            Label7.AutoSize = true;
            Label7.Location = new Point(6, 118);
            Label7.Name = "Label7";
            Label7.Size = new Size(52, 13);
            Label7.TabIndex = 11;
            Label7.Text = "Subframe";
            // 
            // ListBox2
            // 
            ListBox2.FormattingEnabled = true;
            ListBox2.HorizontalScrollbar = true;
            ListBox2.Location = new Point(77, 175);
            ListBox2.Name = "ListBox2";
            ListBox2.ScrollAlwaysVisible = true;
            ListBox2.Size = new Size(868, 381);
            ListBox2.TabIndex = 9;
            // 
            // Label6
            // 
            Label6.AutoSize = true;
            Label6.Location = new Point(12, 175);
            Label6.Name = "Label6";
            Label6.Size = new Size(25, 13);
            Label6.TabIndex = 10;
            Label6.Text = "Log";
            // 
            // Label5
            // 
            Label5.AutoSize = true;
            Label5.Location = new Point(6, 13);
            Label5.Name = "Label5";
            Label5.Size = new Size(56, 13);
            Label5.TabIndex = 3;
            Label5.Text = "Sequence";
            // 
            // txtFile3
            // 
            txtFile3.AllowDrop = true;
            txtFile3.Location = new Point(77, 10);
            txtFile3.Name = "txtFile3";
            txtFile3.ScrollBars = ScrollBars.Vertical;
            txtFile3.Size = new Size(589, 20);
            txtFile3.TabIndex = 4;
            // 
            // btnBrowse3
            // 
            btnBrowse3.Location = new Point(765, 8);
            btnBrowse3.Name = "btnBrowse3";
            btnBrowse3.Size = new Size(75, 23);
            btnBrowse3.TabIndex = 5;
            btnBrowse3.Text = "Browse";
            btnBrowse3.UseVisualStyleBackColor = true;
            // 
            // TabPage1
            // 
            TabPage1.Controls.Add(cbCameraSNStyle);
            TabPage1.Controls.Add(chkCompareCAL);
            TabPage1.Controls.Add(btnExportAnalysesCompareLog);
            TabPage1.Controls.Add(btnUseDefaultMaster);
            TabPage1.Controls.Add(btnUseLastModified1);
            TabPage1.Controls.Add(cbxIgnoreList);
            TabPage1.Controls.Add(Label2);
            TabPage1.Controls.Add(Label1);
            TabPage1.Controls.Add(btnCompare);
            TabPage1.Controls.Add(btnClearlog);
            TabPage1.Controls.Add(txtFile2);
            TabPage1.Controls.Add(txtFile1);
            TabPage1.Controls.Add(ListBox1);
            TabPage1.Controls.Add(btnBrowse2);
            TabPage1.Controls.Add(Label3);
            TabPage1.Controls.Add(btnBrowse1);
            TabPage1.Controls.Add(Label4);
            TabPage1.Location = new Point(4, 22);
            TabPage1.Name = "TabPage1";
            TabPage1.Padding = new Padding(3);
            TabPage1.Size = new Size(951, 562);
            TabPage1.TabIndex = 0;
            TabPage1.Text = "Compare Sequence";
            TabPage1.UseVisualStyleBackColor = true;
            // 
            // cbCameraSNStyle
            // 
            cbCameraSNStyle.FormattingEnabled = true;
            cbCameraSNStyle.Items.AddRange(new object[] { "Use local Camera SN", "Use sequence last saved Camera SN" });
            cbCameraSNStyle.Location = new Point(681, 65);
            cbCameraSNStyle.Name = "cbCameraSNStyle";
            cbCameraSNStyle.Size = new Size(170, 21);
            cbCameraSNStyle.TabIndex = 15;
            cbCameraSNStyle.Text = "Use local Camera SN";
            // 
            // chkCompareCAL
            // 
            chkCompareCAL.AutoSize = true;
            chkCompareCAL.Location = new Point(857, 67);
            chkCompareCAL.Name = "chkCompareCAL";
            chkCompareCAL.Size = new Size(91, 17);
            chkCompareCAL.TabIndex = 14;
            chkCompareCAL.Text = "Compare CAL";
            chkCompareCAL.UseVisualStyleBackColor = true;
            // 
            // btnExportAnalysesCompareLog
            // 
            btnExportAnalysesCompareLog.Location = new Point(6, 298);
            btnExportAnalysesCompareLog.Name = "btnExportAnalysesCompareLog";
            btnExportAnalysesCompareLog.Size = new Size(65, 79);
            btnExportAnalysesCompareLog.TabIndex = 13;
            btnExportAnalysesCompareLog.Text = "Export log";
            btnExportAnalysesCompareLog.UseVisualStyleBackColor = true;
            // 
            // btnUseDefaultMaster
            // 
            btnUseDefaultMaster.Location = new Point(681, 37);
            btnUseDefaultMaster.Name = "btnUseDefaultMaster";
            btnUseDefaultMaster.Size = new Size(89, 23);
            btnUseDefaultMaster.TabIndex = 12;
            btnUseDefaultMaster.Text = "Default Master";
            btnUseDefaultMaster.UseVisualStyleBackColor = true;
            // 
            // btnUseLastModified1
            // 
            btnUseLastModified1.Location = new Point(681, 9);
            btnUseLastModified1.Name = "btnUseLastModified1";
            btnUseLastModified1.Size = new Size(89, 23);
            btnUseLastModified1.TabIndex = 11;
            btnUseLastModified1.Text = "Last Modified";
            btnUseLastModified1.UseVisualStyleBackColor = true;
            // 
            // cbxIgnoreList
            // 
            cbxIgnoreList.FormattingEnabled = true;
            cbxIgnoreList.Items.AddRange(new object[] { "", "Notes,DllOutputPath,DLLDefFolder" });
            cbxIgnoreList.Location = new Point(77, 65);
            cbxIgnoreList.Name = "cbxIgnoreList";
            cbxIgnoreList.Size = new Size(598, 21);
            cbxIgnoreList.TabIndex = 10;
            cbxIgnoreList.Text = "Notes,DllOutputPath,DLLDefFolder";
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Location = new Point(6, 42);
            Label2.Name = "Label2";
            Label2.Size = new Size(65, 13);
            Label2.TabIndex = 3;
            Label2.Text = "Sequence 2";
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Location = new Point(6, 14);
            Label1.Name = "Label1";
            Label1.Size = new Size(65, 13);
            Label1.TabIndex = 0;
            Label1.Text = "Sequence 1";
            // 
            // btnCompare
            // 
            btnCompare.Location = new Point(857, 9);
            btnCompare.Name = "btnCompare";
            btnCompare.Size = new Size(88, 51);
            btnCompare.TabIndex = 6;
            btnCompare.Text = "Compare";
            btnCompare.UseVisualStyleBackColor = true;
            // 
            // btnClearlog
            // 
            btnClearlog.Location = new Point(6, 213);
            btnClearlog.Name = "btnClearlog";
            btnClearlog.Size = new Size(65, 79);
            btnClearlog.TabIndex = 9;
            btnClearlog.Text = "Clear log";
            btnClearlog.UseVisualStyleBackColor = true;
            // 
            // txtFile2
            // 
            txtFile2.AllowDrop = true;
            txtFile2.Location = new Point(77, 39);
            txtFile2.Name = "txtFile2";
            txtFile2.ScrollBars = ScrollBars.Vertical;
            txtFile2.Size = new Size(598, 20);
            txtFile2.TabIndex = 4;
            // 
            // txtFile1
            // 
            txtFile1.AllowDrop = true;
            txtFile1.Location = new Point(77, 11);
            txtFile1.Name = "txtFile1";
            txtFile1.ScrollBars = ScrollBars.Vertical;
            txtFile1.Size = new Size(598, 20);
            txtFile1.TabIndex = 1;
            // 
            // ListBox1
            // 
            ListBox1.FormattingEnabled = true;
            ListBox1.HorizontalScrollbar = true;
            ListBox1.Location = new Point(77, 175);
            ListBox1.Name = "ListBox1";
            ListBox1.ScrollAlwaysVisible = true;
            ListBox1.Size = new Size(868, 381);
            ListBox1.TabIndex = 7;
            // 
            // btnBrowse2
            // 
            btnBrowse2.Location = new Point(776, 37);
            btnBrowse2.Name = "btnBrowse2";
            btnBrowse2.Size = new Size(75, 23);
            btnBrowse2.TabIndex = 5;
            btnBrowse2.Text = "Browse";
            btnBrowse2.UseVisualStyleBackColor = true;
            // 
            // Label3
            // 
            Label3.AutoSize = true;
            Label3.Location = new Point(12, 175);
            Label3.Name = "Label3";
            Label3.Size = new Size(25, 13);
            Label3.TabIndex = 8;
            Label3.Text = "Log";
            // 
            // btnBrowse1
            // 
            btnBrowse1.Location = new Point(776, 9);
            btnBrowse1.Name = "btnBrowse1";
            btnBrowse1.Size = new Size(75, 23);
            btnBrowse1.TabIndex = 2;
            btnBrowse1.Text = "Browse";
            btnBrowse1.UseVisualStyleBackColor = true;
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Location = new Point(6, 68);
            Label4.Name = "Label4";
            Label4.Size = new Size(56, 13);
            Label4.TabIndex = 3;
            Label4.Text = "Ignore List";
            // 
            // TabControl1
            // 
            TabControl1.Controls.Add(TabPage1);
            TabControl1.Controls.Add(TabPage2);
            TabControl1.Controls.Add(TabPage3);
            TabControl1.Location = new Point(12, 12);
            TabControl1.Name = "TabControl1";
            TabControl1.SelectedIndex = 0;
            TabControl1.Size = new Size(959, 588);
            TabControl1.TabIndex = 10;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(6.0f, 13.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(978, 605);
            Controls.Add(TabControl1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            Name = "MainForm";
            Text = "Sequence Check";
            TabPage3.ResumeLayout(false);
            TabPage3.PerformLayout();
            TabPage2.ResumeLayout(false);
            TabPage2.PerformLayout();
            TabPage1.ResumeLayout(false);
            TabPage1.PerformLayout();
            TabControl1.ResumeLayout(false);
            Load += new EventHandler(Form1_Load);
            ResumeLayout(false);

        }

        internal TabPage TabPage3;
        internal Button btnCompareAppdata;
        internal TabPage TabPage2;
        internal CheckBox chkImgScaleCalSettings;
        internal CheckBox chkFlatFieldCalSettings;
        internal Button btnEditCalRule;
        internal CheckBox chkColorCalSettings;
        internal Button btnShowCalSettings;
        internal Button btnExportMeasurementsCompareLog;
        internal Button btnUseLastModified3;
        internal Button btnShowSettings;
        internal Button btnClearlog2;
        internal Button btnCheck;
        internal CheckBox chkCalNone;
        internal ComboBox cbxSubframe;
        internal Label Label7;
        internal ListBox ListBox2;
        internal Label Label6;
        internal Label Label5;
        internal TextBox txtFile3;
        internal Button btnBrowse3;
        internal TabPage TabPage1;
        internal Button btnExportAnalysesCompareLog;
        internal Button btnUseDefaultMaster;
        internal Button btnUseLastModified1;
        internal ComboBox cbxIgnoreList;
        internal Label Label2;
        internal Label Label1;
        internal Button btnCompare;
        internal Button btnClearlog;
        internal TextBox txtFile2;
        internal TextBox txtFile1;
        internal ListBox ListBox1;
        internal Button btnBrowse2;
        internal Label Label3;
        internal Button btnBrowse1;
        internal Label Label4;
        internal TabControl TabControl1;
        internal Button btnExportOtherCompareLog;
        internal Button btnClearlog3;
        internal ListBox ListBox3;
        internal Label Label8;
        internal CheckBox chkCompareCAL;
        private Button btnBrowseAdditional;
        private TextBox txtAdditionalSequence;
        private Label lblAddtionalTarget;
        private Button btnClearAdditional;
        internal ComboBox cbCameraSNStyle;
        internal ComboBox cbCameraSNStyle2;
    }
}