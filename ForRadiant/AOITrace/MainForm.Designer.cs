namespace AOITrace
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
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dateTimePickerFilter = new System.Windows.Forms.DateTimePicker();
            this.btnBrowseResultFolder = new System.Windows.Forms.Button();
            this.lstResultFiles = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtResultFolderPath = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.chkShowBefore = new System.Windows.Forms.CheckBox();
            this.chkShowAfter = new System.Windows.Forms.CheckBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
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
            this.txtMonoLogFolders.Location = new System.Drawing.Point(380, 35);
            this.txtMonoLogFolders.Multiline = true;
            this.txtMonoLogFolders.Name = "txtMonoLogFolders";
            this.txtMonoLogFolders.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMonoLogFolders.Size = new System.Drawing.Size(362, 526);
            this.txtMonoLogFolders.TabIndex = 6;
            this.txtMonoLogFolders.Text = "D:\\Trace Spot Mono Camera\\Station 1 Mono CSV\r\nD:\\Trace Spot Mono Camera\\Station 2" +
    " Mono CSV";
            // 
            // txtColorLogFolders
            // 
            this.txtColorLogFolders.Location = new System.Drawing.Point(12, 35);
            this.txtColorLogFolders.Multiline = true;
            this.txtColorLogFolders.Name = "txtColorLogFolders";
            this.txtColorLogFolders.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtColorLogFolders.Size = new System.Drawing.Size(362, 526);
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
            this.txtOutputCsvPath.Size = new System.Drawing.Size(627, 20);
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
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1131, 619);
            this.tabControl1.TabIndex = 9;
            // 
            // tabPage1
            // 
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
            // tabPage2
            // 
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
            // dateTimePickerFilter
            // 
            this.dateTimePickerFilter.Location = new System.Drawing.Point(227, 32);
            this.dateTimePickerFilter.Name = "dateTimePickerFilter";
            this.dateTimePickerFilter.Size = new System.Drawing.Size(200, 20);
            this.dateTimePickerFilter.TabIndex = 12;
            this.dateTimePickerFilter.ValueChanged += new System.EventHandler(this.dateTimePickerFilter_ValueChanged);
            // 
            // btnBrowseResultFolder
            // 
            this.btnBrowseResultFolder.Location = new System.Drawing.Point(661, 4);
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
            this.lstResultFiles.Location = new System.Drawing.Point(9, 58);
            this.lstResultFiles.Name = "lstResultFiles";
            this.lstResultFiles.ScrollAlwaysVisible = true;
            this.lstResultFiles.Size = new System.Drawing.Size(727, 524);
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
            this.txtResultFolderPath.Size = new System.Drawing.Size(546, 20);
            this.txtResultFolderPath.TabIndex = 9;
            this.txtResultFolderPath.Text = "D:\\Trace Spot Mono Camera\\Match CSV";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 38);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(215, 13);
            this.label6.TabIndex = 14;
            this.label6.Text = "List of Result Files, double click to open files";
            // 
            // chkShowBefore
            // 
            this.chkShowBefore.AutoSize = true;
            this.chkShowBefore.Location = new System.Drawing.Point(433, 35);
            this.chkShowBefore.Name = "chkShowBefore";
            this.chkShowBefore.Size = new System.Drawing.Size(137, 17);
            this.chkShowBefore.TabIndex = 15;
            this.chkShowBefore.Text = "Show Files Before Date";
            this.chkShowBefore.UseVisualStyleBackColor = true;
            this.chkShowBefore.CheckedChanged += new System.EventHandler(this.chkShowBefore_CheckedChanged);
            // 
            // chkShowAfter
            // 
            this.chkShowAfter.AutoSize = true;
            this.chkShowAfter.Location = new System.Drawing.Point(576, 35);
            this.chkShowAfter.Name = "chkShowAfter";
            this.chkShowAfter.Size = new System.Drawing.Size(128, 17);
            this.chkShowAfter.TabIndex = 16;
            this.chkShowAfter.Text = "Show Files After Date";
            this.chkShowAfter.UseVisualStyleBackColor = true;
            this.chkShowAfter.CheckedChanged += new System.EventHandler(this.chkShowAfter_CheckedChanged);
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(1123, 593);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Run Schedule";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1155, 643);
            this.Controls.Add(this.tabControl1);
            this.Name = "MainForm";
            this.Text = "AOITrace";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

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
    }
}

