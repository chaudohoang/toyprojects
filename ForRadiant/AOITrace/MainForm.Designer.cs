﻿namespace AOITrace
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.label1 = new System.Windows.Forms.Label();
            this.txtMonoLogFolders = new System.Windows.Forms.TextBox();
            this.txtColorLogFolders = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtOutputCsvPath = new System.Windows.Forms.TextBox();
            this.btnProcess = new System.Windows.Forms.Button();
            this.btnLoadColorLogFolders = new System.Windows.Forms.Button();
            this.btnSaveColorLogFolders = new System.Windows.Forms.Button();
            this.btnLoadMonoLogFolders = new System.Windows.Forms.Button();
            this.btnSaveMonoLogFolders = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.btnBrowseOutputFolder = new System.Windows.Forms.Button();
            this.cmbFileSelection = new System.Windows.Forms.ComboBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.lstUnreadResultFiles = new System.Windows.Forms.ListBox();
            this.label7 = new System.Windows.Forms.Label();
            this.chkShowAfter = new System.Windows.Forms.CheckBox();
            this.chkShowBefore = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.dateTimePickerFilter = new System.Windows.Forms.DateTimePicker();
            this.btnBrowseResultFolder = new System.Windows.Forms.Button();
            this.lstResultFiles = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtResultFolderPath = new System.Windows.Forms.TextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.cmbTimeUnit = new System.Windows.Forms.ComboBox();
            this.txtTimeToAdd = new System.Windows.Forms.TextBox();
            this.btnAddTime = new System.Windows.Forms.Button();
            this.btnSetCurrentTime = new System.Windows.Forms.Button();
            this.lblCountdown = new System.Windows.Forms.Label();
            this.btnScheduleProcessRun = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.dateTimePickerProcessRun = new System.Windows.Forms.DateTimePicker();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.txtLogs = new System.Windows.Forms.RichTextBox();
            this.btnClearLogs = new System.Windows.Forms.Button();
            this.lblNotification = new System.Windows.Forms.Label();
            this.btnSetLogLimit = new System.Windows.Forms.Button();
            this.txtLogLimit = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(371, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(85, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Mono Log Paths";
            // 
            // txtMonoLogFolders
            // 
            this.txtMonoLogFolders.Location = new System.Drawing.Point(380, 62);
            this.txtMonoLogFolders.Multiline = true;
            this.txtMonoLogFolders.Name = "txtMonoLogFolders";
            this.txtMonoLogFolders.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMonoLogFolders.Size = new System.Drawing.Size(362, 499);
            this.txtMonoLogFolders.TabIndex = 6;
            this.txtMonoLogFolders.Text = "D:\\Trace Spot Mono Camera\\Station 1 Mono CSV\r\nD:\\Trace Spot Mono Camera\\Station 2" +
    " Mono CSV";
            // 
            // txtColorLogFolders
            // 
            this.txtColorLogFolders.Location = new System.Drawing.Point(12, 66);
            this.txtColorLogFolders.Multiline = true;
            this.txtColorLogFolders.Name = "txtColorLogFolders";
            this.txtColorLogFolders.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtColorLogFolders.Size = new System.Drawing.Size(362, 495);
            this.txtColorLogFolders.TabIndex = 3;
            this.txtColorLogFolders.Text = "D:\\Trace Spot Mono Camera\\Station 1 Color CSV\r\nD:\\Trace Spot Mono Camera\\Station " +
    "2 Color CSV";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Color Log Paths";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 567);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Result Output Path";
            // 
            // txtOutputCsvPath
            // 
            this.txtOutputCsvPath.Location = new System.Drawing.Point(115, 567);
            this.txtOutputCsvPath.Name = "txtOutputCsvPath";
            this.txtOutputCsvPath.Size = new System.Drawing.Size(546, 20);
            this.txtOutputCsvPath.TabIndex = 7;
            this.txtOutputCsvPath.Text = "D:\\Trace Spot Mono Camera\\Match CSV";
            // 
            // btnProcess
            // 
            this.btnProcess.Location = new System.Drawing.Point(748, 35);
            this.btnProcess.Name = "btnProcess";
            this.btnProcess.Size = new System.Drawing.Size(74, 63);
            this.btnProcess.TabIndex = 8;
            this.btnProcess.Text = "Process";
            this.btnProcess.UseVisualStyleBackColor = true;
            this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
            // 
            // btnLoadColorLogFolders
            // 
            this.btnLoadColorLogFolders.Location = new System.Drawing.Point(94, 6);
            this.btnLoadColorLogFolders.Name = "btnLoadColorLogFolders";
            this.btnLoadColorLogFolders.Size = new System.Drawing.Size(75, 23);
            this.btnLoadColorLogFolders.TabIndex = 1;
            this.btnLoadColorLogFolders.Text = "Load";
            this.btnLoadColorLogFolders.UseVisualStyleBackColor = true;
            this.btnLoadColorLogFolders.Click += new System.EventHandler(this.btnLoadColorLogFolders_Click);
            // 
            // btnSaveColorLogFolders
            // 
            this.btnSaveColorLogFolders.Location = new System.Drawing.Point(175, 6);
            this.btnSaveColorLogFolders.Name = "btnSaveColorLogFolders";
            this.btnSaveColorLogFolders.Size = new System.Drawing.Size(75, 23);
            this.btnSaveColorLogFolders.TabIndex = 2;
            this.btnSaveColorLogFolders.Text = "Save";
            this.btnSaveColorLogFolders.UseVisualStyleBackColor = true;
            this.btnSaveColorLogFolders.Click += new System.EventHandler(this.btnSaveColorLogFolders_Click);
            // 
            // btnLoadMonoLogFolders
            // 
            this.btnLoadMonoLogFolders.Location = new System.Drawing.Point(462, 6);
            this.btnLoadMonoLogFolders.Name = "btnLoadMonoLogFolders";
            this.btnLoadMonoLogFolders.Size = new System.Drawing.Size(75, 23);
            this.btnLoadMonoLogFolders.TabIndex = 4;
            this.btnLoadMonoLogFolders.Text = "Load";
            this.btnLoadMonoLogFolders.UseVisualStyleBackColor = true;
            this.btnLoadMonoLogFolders.Click += new System.EventHandler(this.btnLoadMonoLogFolders_Click);
            // 
            // btnSaveMonoLogFolders
            // 
            this.btnSaveMonoLogFolders.Location = new System.Drawing.Point(543, 6);
            this.btnSaveMonoLogFolders.Name = "btnSaveMonoLogFolders";
            this.btnSaveMonoLogFolders.Size = new System.Drawing.Size(75, 23);
            this.btnSaveMonoLogFolders.TabIndex = 5;
            this.btnSaveMonoLogFolders.Text = "Save";
            this.btnSaveMonoLogFolders.UseVisualStyleBackColor = true;
            this.btnSaveMonoLogFolders.Click += new System.EventHandler(this.btnSaveMonoLogFolders_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(12, 37);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1131, 619);
            this.tabControl1.TabIndex = 9;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.btnBrowseOutputFolder);
            this.tabPage1.Controls.Add(this.cmbFileSelection);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.btnSaveMonoLogFolders);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.btnLoadMonoLogFolders);
            this.tabPage1.Controls.Add(this.txtMonoLogFolders);
            this.tabPage1.Controls.Add(this.btnSaveColorLogFolders);
            this.tabPage1.Controls.Add(this.txtColorLogFolders);
            this.tabPage1.Controls.Add(this.btnLoadColorLogFolders);
            this.tabPage1.Controls.Add(this.label3);
            this.tabPage1.Controls.Add(this.btnProcess);
            this.tabPage1.Controls.Add(this.txtOutputCsvPath);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1123, 593);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Trace";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // btnBrowseOutputFolder
            // 
            this.btnBrowseOutputFolder.Location = new System.Drawing.Point(667, 565);
            this.btnBrowseOutputFolder.Name = "btnBrowseOutputFolder";
            this.btnBrowseOutputFolder.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseOutputFolder.TabIndex = 12;
            this.btnBrowseOutputFolder.Text = "Browse";
            this.btnBrowseOutputFolder.UseVisualStyleBackColor = true;
            this.btnBrowseOutputFolder.Click += new System.EventHandler(this.btnBrowseOutputFolder_Click);
            // 
            // cmbFileSelection
            // 
            this.cmbFileSelection.Location = new System.Drawing.Point(15, 35);
            this.cmbFileSelection.Name = "cmbFileSelection";
            this.cmbFileSelection.Size = new System.Drawing.Size(727, 21);
            this.cmbFileSelection.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.lstUnreadResultFiles);
            this.tabPage2.Controls.Add(this.label7);
            this.tabPage2.Controls.Add(this.chkShowAfter);
            this.tabPage2.Controls.Add(this.chkShowBefore);
            this.tabPage2.Controls.Add(this.label6);
            this.tabPage2.Controls.Add(this.dateTimePickerFilter);
            this.tabPage2.Controls.Add(this.btnBrowseResultFolder);
            this.tabPage2.Controls.Add(this.lstResultFiles);
            this.tabPage2.Controls.Add(this.label4);
            this.tabPage2.Controls.Add(this.txtResultFolderPath);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1123, 593);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Result";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.tabPage2.Enter += new System.EventHandler(this.tabPage2_Enter);
            // 
            // lstUnreadResultFiles
            // 
            this.lstUnreadResultFiles.FormattingEnabled = true;
            this.lstUnreadResultFiles.Location = new System.Drawing.Point(9, 55);
            this.lstUnreadResultFiles.Name = "lstUnreadResultFiles";
            this.lstUnreadResultFiles.ScrollAlwaysVisible = true;
            this.lstUnreadResultFiles.Size = new System.Drawing.Size(744, 225);
            this.lstUnreadResultFiles.TabIndex = 18;
            this.lstUnreadResultFiles.DoubleClick += new System.EventHandler(this.lstUnreadResultFiles_DoubleClick);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(17, 39);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(253, 13);
            this.label7.TabIndex = 17;
            this.label7.Text = "List of Unread Result Files, double click to open files";
            // 
            // chkShowAfter
            // 
            this.chkShowAfter.AutoSize = true;
            this.chkShowAfter.Location = new System.Drawing.Point(625, 303);
            this.chkShowAfter.Name = "chkShowAfter";
            this.chkShowAfter.Size = new System.Drawing.Size(128, 17);
            this.chkShowAfter.TabIndex = 16;
            this.chkShowAfter.Text = "Show Files After Date";
            this.chkShowAfter.UseVisualStyleBackColor = true;
            this.chkShowAfter.CheckedChanged += new System.EventHandler(this.chkShowAfter_CheckedChanged);
            // 
            // chkShowBefore
            // 
            this.chkShowBefore.AutoSize = true;
            this.chkShowBefore.Location = new System.Drawing.Point(482, 303);
            this.chkShowBefore.Name = "chkShowBefore";
            this.chkShowBefore.Size = new System.Drawing.Size(137, 17);
            this.chkShowBefore.TabIndex = 15;
            this.chkShowBefore.Text = "Show Files Before Date";
            this.chkShowBefore.UseVisualStyleBackColor = true;
            this.chkShowBefore.CheckedChanged += new System.EventHandler(this.chkShowBefore_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 304);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(255, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "List of Result Files by Date, double click to open files";
            // 
            // dateTimePickerFilter
            // 
            this.dateTimePickerFilter.Location = new System.Drawing.Point(276, 300);
            this.dateTimePickerFilter.Name = "dateTimePickerFilter";
            this.dateTimePickerFilter.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerFilter.TabIndex = 12;
            this.dateTimePickerFilter.ValueChanged += new System.EventHandler(this.dateTimePickerFilter_ValueChanged);
            // 
            // btnBrowseResultFolder
            // 
            this.btnBrowseResultFolder.Location = new System.Drawing.Point(678, 4);
            this.btnBrowseResultFolder.Name = "btnBrowseResultFolder";
            this.btnBrowseResultFolder.Size = new System.Drawing.Size(75, 23);
            this.btnBrowseResultFolder.TabIndex = 11;
            this.btnBrowseResultFolder.Text = "Browse";
            this.btnBrowseResultFolder.UseVisualStyleBackColor = true;
            this.btnBrowseResultFolder.Click += new System.EventHandler(this.btnBrowseResultFolder_Click);
            // 
            // lstResultFiles
            // 
            this.lstResultFiles.FormattingEnabled = true;
            this.lstResultFiles.Location = new System.Drawing.Point(9, 331);
            this.lstResultFiles.Name = "lstResultFiles";
            this.lstResultFiles.ScrollAlwaysVisible = true;
            this.lstResultFiles.Size = new System.Drawing.Size(744, 251);
            this.lstResultFiles.TabIndex = 10;
            this.lstResultFiles.DoubleClick += new System.EventHandler(this.lstResultFiles_DoubleClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Result Output Path";
            // 
            // txtResultFolderPath
            // 
            this.txtResultFolderPath.Location = new System.Drawing.Point(109, 6);
            this.txtResultFolderPath.Name = "txtResultFolderPath";
            this.txtResultFolderPath.Size = new System.Drawing.Size(563, 20);
            this.txtResultFolderPath.TabIndex = 9;
            this.txtResultFolderPath.Text = "D:\\Trace Spot Mono Camera\\Match CSV";
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.cmbTimeUnit);
            this.tabPage3.Controls.Add(this.txtTimeToAdd);
            this.tabPage3.Controls.Add(this.btnAddTime);
            this.tabPage3.Controls.Add(this.btnSetCurrentTime);
            this.tabPage3.Controls.Add(this.lblCountdown);
            this.tabPage3.Controls.Add(this.btnScheduleProcessRun);
            this.tabPage3.Controls.Add(this.label5);
            this.tabPage3.Controls.Add(this.dateTimePickerProcessRun);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1123, 593);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Run Schedule";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // cmbTimeUnit
            // 
            this.cmbTimeUnit.FormattingEnabled = true;
            this.cmbTimeUnit.Location = new System.Drawing.Point(214, 51);
            this.cmbTimeUnit.Name = "cmbTimeUnit";
            this.cmbTimeUnit.Size = new System.Drawing.Size(83, 21);
            this.cmbTimeUnit.TabIndex = 8;
            // 
            // txtTimeToAdd
            // 
            this.txtTimeToAdd.Location = new System.Drawing.Point(177, 51);
            this.txtTimeToAdd.Name = "txtTimeToAdd";
            this.txtTimeToAdd.Size = new System.Drawing.Size(31, 20);
            this.txtTimeToAdd.TabIndex = 7;
            this.txtTimeToAdd.Text = "10";
            // 
            // btnAddTime
            // 
            this.btnAddTime.Location = new System.Drawing.Point(130, 49);
            this.btnAddTime.Name = "btnAddTime";
            this.btnAddTime.Size = new System.Drawing.Size(41, 23);
            this.btnAddTime.TabIndex = 6;
            this.btnAddTime.Text = "Add";
            this.btnAddTime.UseVisualStyleBackColor = true;
            this.btnAddTime.Click += new System.EventHandler(this.btnAddTime_Click);
            // 
            // btnSetCurrentTime
            // 
            this.btnSetCurrentTime.Location = new System.Drawing.Point(30, 49);
            this.btnSetCurrentTime.Name = "btnSetCurrentTime";
            this.btnSetCurrentTime.Size = new System.Drawing.Size(94, 23);
            this.btnSetCurrentTime.TabIndex = 5;
            this.btnSetCurrentTime.Text = "Set Current Time";
            this.btnSetCurrentTime.UseVisualStyleBackColor = true;
            this.btnSetCurrentTime.Click += new System.EventHandler(this.btnSetCurrentTime_Click);
            // 
            // lblCountdown
            // 
            this.lblCountdown.AutoSize = true;
            this.lblCountdown.Location = new System.Drawing.Point(432, 17);
            this.lblCountdown.Name = "lblCountdown";
            this.lblCountdown.Size = new System.Drawing.Size(0, 13);
            this.lblCountdown.TabIndex = 4;
            // 
            // btnScheduleProcessRun
            // 
            this.btnScheduleProcessRun.Location = new System.Drawing.Point(336, 12);
            this.btnScheduleProcessRun.Name = "btnScheduleProcessRun";
            this.btnScheduleProcessRun.Size = new System.Drawing.Size(75, 23);
            this.btnScheduleProcessRun.TabIndex = 2;
            this.btnScheduleProcessRun.Text = "Run";
            this.btnScheduleProcessRun.UseVisualStyleBackColor = true;
            this.btnScheduleProcessRun.Click += new System.EventHandler(this.btnScheduleProcessRun_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "AutoRun Schedule";
            // 
            // dateTimePickerProcessRun
            // 
            this.dateTimePickerProcessRun.Format = System.Windows.Forms.DateTimePickerFormat.Time;
            this.dateTimePickerProcessRun.Location = new System.Drawing.Point(130, 15);
            this.dateTimePickerProcessRun.Name = "dateTimePickerProcessRun";
            this.dateTimePickerProcessRun.ShowUpDown = true;
            this.dateTimePickerProcessRun.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerProcessRun.TabIndex = 0;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.label8);
            this.tabPage4.Controls.Add(this.txtLogLimit);
            this.tabPage4.Controls.Add(this.btnSetLogLimit);
            this.tabPage4.Controls.Add(this.txtLogs);
            this.tabPage4.Controls.Add(this.btnClearLogs);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(1123, 593);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Logs";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // txtLogs
            // 
            this.txtLogs.Location = new System.Drawing.Point(3, 32);
            this.txtLogs.Name = "txtLogs";
            this.txtLogs.Size = new System.Drawing.Size(1117, 558);
            this.txtLogs.TabIndex = 2;
            this.txtLogs.Text = "";
            // 
            // btnClearLogs
            // 
            this.btnClearLogs.Location = new System.Drawing.Point(1045, 4);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(75, 23);
            this.btnClearLogs.TabIndex = 1;
            this.btnClearLogs.Text = "Clear Logs";
            this.btnClearLogs.UseVisualStyleBackColor = true;
            this.btnClearLogs.Click += new System.EventHandler(this.btnClearLogs_Click);
            // 
            // lblNotification
            // 
            this.lblNotification.AutoSize = true;
            this.lblNotification.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNotification.Location = new System.Drawing.Point(16, 9);
            this.lblNotification.Name = "lblNotification";
            this.lblNotification.Size = new System.Drawing.Size(154, 24);
            this.lblNotification.TabIndex = 10;
            this.lblNotification.Text = "0 Unread Result !";
            // 
            // btnSetLogLimit
            // 
            this.btnSetLogLimit.Location = new System.Drawing.Point(867, 4);
            this.btnSetLogLimit.Name = "btnSetLogLimit";
            this.btnSetLogLimit.Size = new System.Drawing.Size(86, 23);
            this.btnSetLogLimit.TabIndex = 3;
            this.btnSetLogLimit.Text = "Set Log Limit";
            this.btnSetLogLimit.UseVisualStyleBackColor = true;
            // 
            // txtLogLimit
            // 
            this.txtLogLimit.Location = new System.Drawing.Point(959, 6);
            this.txtLogLimit.Name = "txtLogLimit";
            this.txtLogLimit.Size = new System.Drawing.Size(42, 20);
            this.txtLogLimit.TabIndex = 4;
            this.txtLogLimit.Text = "200";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(1007, 9);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(32, 13);
            this.label8.TabIndex = 5;
            this.label8.Text = "Lines";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1155, 659);
            this.Controls.Add(this.lblNotification);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "AOITrace";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            this.tabPage4.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtMonoLogFolders;
        private System.Windows.Forms.TextBox txtColorLogFolders;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtOutputCsvPath;
        private System.Windows.Forms.Button btnProcess;
        private System.Windows.Forms.Button btnLoadColorLogFolders;
        private System.Windows.Forms.Button btnSaveColorLogFolders;
        private System.Windows.Forms.Button btnLoadMonoLogFolders;
        private System.Windows.Forms.Button btnSaveMonoLogFolders;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtResultFolderPath;
        private System.Windows.Forms.Button btnBrowseResultFolder;
        private System.Windows.Forms.ListBox lstResultFiles;
        private System.Windows.Forms.DateTimePicker dateTimePickerFilter;
        private System.Windows.Forms.CheckBox chkShowAfter;
        private System.Windows.Forms.CheckBox chkShowBefore;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnScheduleProcessRun;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.DateTimePicker dateTimePickerProcessRun;
        private System.Windows.Forms.Label lblCountdown;
        private System.Windows.Forms.ComboBox cmbFileSelection;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.Button btnBrowseOutputFolder;
        private System.Windows.Forms.Button btnClearLogs;
        private System.Windows.Forms.Button btnSetCurrentTime;
        private System.Windows.Forms.ComboBox cmbTimeUnit;
        private System.Windows.Forms.TextBox txtTimeToAdd;
        private System.Windows.Forms.Button btnAddTime;
        private System.Windows.Forms.RichTextBox txtLogs;
        private System.Windows.Forms.Label lblNotification;
        private System.Windows.Forms.ListBox lstUnreadResultFiles;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtLogLimit;
        private System.Windows.Forms.Button btnSetLogLimit;
    }
}

