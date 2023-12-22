namespace SetSequence
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
            this.cmdBrowseSequence = new System.Windows.Forms.LinkLabel();
            this.lblAddtionalTarget = new System.Windows.Forms.Label();
            this.cmdUseLastModifiedSequence = new System.Windows.Forms.LinkLabel();
            this.cbCameraRotation = new System.Windows.Forms.ComboBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.lblSubframe = new System.Windows.Forms.Label();
            this.lblCameraRotatioin = new System.Windows.Forms.Label();
            this.cbFocusDistance = new System.Windows.Forms.ComboBox();
            this.cbSubframe = new System.Windows.Forms.ComboBox();
            this.lblFocusDistance = new System.Windows.Forms.Label();
            this.cbCalBox = new System.Windows.Forms.ComboBox();
            this.cbFNumber = new System.Windows.Forms.ComboBox();
            this.lblFNumber = new System.Windows.Forms.Label();
            this.lblAbout = new System.Windows.Forms.Label();
            this.lblSequencePath = new System.Windows.Forms.Label();
            this.txtAdditionalSequence = new System.Windows.Forms.TextBox();
            this.btnBrowseAdditional = new System.Windows.Forms.Button();
            this.lblTargetSequence = new System.Windows.Forms.Label();
            this.btnClearAdditional = new System.Windows.Forms.Button();
            this.btnSetCalRule = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cbCalibrationIDStyle = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cmdBrowseSequence
            // 
            this.cmdBrowseSequence.AutoSize = true;
            this.cmdBrowseSequence.Location = new System.Drawing.Point(155, 9);
            this.cmdBrowseSequence.Name = "cmdBrowseSequence";
            this.cmdBrowseSequence.Size = new System.Drawing.Size(92, 13);
            this.cmdBrowseSequence.TabIndex = 2;
            this.cmdBrowseSequence.TabStop = true;
            this.cmdBrowseSequence.Text = "Browse sequence";
            this.cmdBrowseSequence.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.cmdBrowseSequence_LinkClicked);
            // 
            // lblAddtionalTarget
            // 
            this.lblAddtionalTarget.AutoSize = true;
            this.lblAddtionalTarget.Location = new System.Drawing.Point(12, 55);
            this.lblAddtionalTarget.Name = "lblAddtionalTarget";
            this.lblAddtionalTarget.Size = new System.Drawing.Size(148, 13);
            this.lblAddtionalTarget.TabIndex = 64;
            this.lblAddtionalTarget.Text = "Additional Target Sequence : ";
            // 
            // cmdUseLastModifiedSequence
            // 
            this.cmdUseLastModifiedSequence.AutoSize = true;
            this.cmdUseLastModifiedSequence.Location = new System.Drawing.Point(12, 9);
            this.cmdUseLastModifiedSequence.Name = "cmdUseLastModifiedSequence";
            this.cmdUseLastModifiedSequence.Size = new System.Drawing.Size(137, 13);
            this.cmdUseLastModifiedSequence.TabIndex = 1;
            this.cmdUseLastModifiedSequence.TabStop = true;
            this.cmdUseLastModifiedSequence.Text = "Use last modified sequence";
            this.cmdUseLastModifiedSequence.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.cmdUseLastModifiedSequence_LinkClicked);
            // 
            // cbCameraRotation
            // 
            this.cbCameraRotation.FormattingEnabled = true;
            this.cbCameraRotation.Items.AddRange(new object[] {
            "",
            "Copy from first step",
            "None",
            "Clockwise90",
            "Rotate180",
            "Counterclockwise90"});
            this.cbCameraRotation.Location = new System.Drawing.Point(101, 227);
            this.cbCameraRotation.Name = "cbCameraRotation";
            this.cbCameraRotation.Size = new System.Drawing.Size(148, 21);
            this.cbCameraRotation.TabIndex = 6;
            this.cbCameraRotation.Text = "Copy from first step";
            // 
            // btnApply
            // 
            this.btnApply.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApply.ForeColor = System.Drawing.Color.Blue;
            this.btnApply.Location = new System.Drawing.Point(255, 145);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(244, 103);
            this.btnApply.TabIndex = 8;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lblSubframe
            // 
            this.lblSubframe.AutoSize = true;
            this.lblSubframe.Location = new System.Drawing.Point(14, 149);
            this.lblSubframe.Name = "lblSubframe";
            this.lblSubframe.Size = new System.Drawing.Size(52, 13);
            this.lblSubframe.TabIndex = 59;
            this.lblSubframe.Text = "Subframe";
            // 
            // lblCameraRotatioin
            // 
            this.lblCameraRotatioin.AutoSize = true;
            this.lblCameraRotatioin.Location = new System.Drawing.Point(14, 230);
            this.lblCameraRotatioin.Name = "lblCameraRotatioin";
            this.lblCameraRotatioin.Size = new System.Drawing.Size(86, 13);
            this.lblCameraRotatioin.TabIndex = 61;
            this.lblCameraRotatioin.Text = "Camera Rotation";
            // 
            // cbFocusDistance
            // 
            this.cbFocusDistance.FormattingEnabled = true;
            this.cbFocusDistance.Items.AddRange(new object[] {
            "",
            "Copy from first step"});
            this.cbFocusDistance.Location = new System.Drawing.Point(101, 173);
            this.cbFocusDistance.Name = "cbFocusDistance";
            this.cbFocusDistance.Size = new System.Drawing.Size(148, 21);
            this.cbFocusDistance.TabIndex = 4;
            this.cbFocusDistance.Text = "Copy from first step";
            // 
            // cbSubframe
            // 
            this.cbSubframe.FormattingEnabled = true;
            this.cbSubframe.Items.AddRange(new object[] {
            "",
            "Copy from first step",
            "800,450,2784,5676",
            "700,250,2984,6076",
            "1000,1300,4576,1784"});
            this.cbSubframe.Location = new System.Drawing.Point(101, 146);
            this.cbSubframe.Name = "cbSubframe";
            this.cbSubframe.Size = new System.Drawing.Size(148, 21);
            this.cbSubframe.TabIndex = 3;
            this.cbSubframe.Text = "Copy from first step";
            // 
            // lblFocusDistance
            // 
            this.lblFocusDistance.AutoSize = true;
            this.lblFocusDistance.Location = new System.Drawing.Point(14, 176);
            this.lblFocusDistance.Name = "lblFocusDistance";
            this.lblFocusDistance.Size = new System.Drawing.Size(81, 13);
            this.lblFocusDistance.TabIndex = 60;
            this.lblFocusDistance.Text = "Focus Distance";
            // 
            // cbCalBox
            // 
            this.cbCalBox.FormattingEnabled = true;
            this.cbCalBox.Items.AddRange(new object[] {
            "",
            "Copy from first step",
            "1,1,1,1"});
            this.cbCalBox.Location = new System.Drawing.Point(255, 254);
            this.cbCalBox.Name = "cbCalBox";
            this.cbCalBox.Size = new System.Drawing.Size(148, 21);
            this.cbCalBox.TabIndex = 7;
            this.cbCalBox.Text = "Copy from first step";
            // 
            // cbFNumber
            // 
            this.cbFNumber.FormattingEnabled = true;
            this.cbFNumber.Items.AddRange(new object[] {
            "",
            "Copy from first step",
            "8.0"});
            this.cbFNumber.Location = new System.Drawing.Point(101, 200);
            this.cbFNumber.Name = "cbFNumber";
            this.cbFNumber.Size = new System.Drawing.Size(148, 21);
            this.cbFNumber.TabIndex = 5;
            this.cbFNumber.Text = "Copy from first step";
            // 
            // lblFNumber
            // 
            this.lblFNumber.AutoSize = true;
            this.lblFNumber.Location = new System.Drawing.Point(14, 203);
            this.lblFNumber.Name = "lblFNumber";
            this.lblFNumber.Size = new System.Drawing.Size(53, 13);
            this.lblFNumber.TabIndex = 73;
            this.lblFNumber.Text = "F-Number";
            // 
            // lblAbout
            // 
            this.lblAbout.AutoSize = true;
            this.lblAbout.Location = new System.Drawing.Point(1187, 9);
            this.lblAbout.Name = "lblAbout";
            this.lblAbout.Size = new System.Drawing.Size(16, 13);
            this.lblAbout.TabIndex = 74;
            this.lblAbout.Text = "...";
            this.lblAbout.DoubleClick += new System.EventHandler(this.lblAbout_DoubleClick);
            // 
            // lblSequencePath
            // 
            this.lblSequencePath.AutoSize = true;
            this.lblSequencePath.Location = new System.Drawing.Point(119, 33);
            this.lblSequencePath.Name = "lblSequencePath";
            this.lblSequencePath.Size = new System.Drawing.Size(0, 13);
            this.lblSequencePath.TabIndex = 75;
            // 
            // txtAdditionalSequence
            // 
            this.txtAdditionalSequence.AllowDrop = true;
            this.txtAdditionalSequence.Location = new System.Drawing.Point(17, 71);
            this.txtAdditionalSequence.Multiline = true;
            this.txtAdditionalSequence.Name = "txtAdditionalSequence";
            this.txtAdditionalSequence.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtAdditionalSequence.Size = new System.Drawing.Size(491, 73);
            this.txtAdditionalSequence.TabIndex = 76;
            this.txtAdditionalSequence.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtAdditionalSequence_DragDrop);
            this.txtAdditionalSequence.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtAdditionalSequence_DragEnter);
            // 
            // btnBrowseAdditional
            // 
            this.btnBrowseAdditional.Location = new System.Drawing.Point(514, 71);
            this.btnBrowseAdditional.Name = "btnBrowseAdditional";
            this.btnBrowseAdditional.Size = new System.Drawing.Size(70, 73);
            this.btnBrowseAdditional.TabIndex = 77;
            this.btnBrowseAdditional.Text = "Browse";
            this.btnBrowseAdditional.UseVisualStyleBackColor = true;
            this.btnBrowseAdditional.Click += new System.EventHandler(this.btnBrowseAdditional_Click);
            // 
            // lblTargetSequence
            // 
            this.lblTargetSequence.AutoSize = true;
            this.lblTargetSequence.Location = new System.Drawing.Point(14, 33);
            this.lblTargetSequence.Name = "lblTargetSequence";
            this.lblTargetSequence.Size = new System.Drawing.Size(99, 13);
            this.lblTargetSequence.TabIndex = 75;
            this.lblTargetSequence.Text = "Target Sequence : ";
            // 
            // btnClearAdditional
            // 
            this.btnClearAdditional.Location = new System.Drawing.Point(590, 71);
            this.btnClearAdditional.Name = "btnClearAdditional";
            this.btnClearAdditional.Size = new System.Drawing.Size(70, 73);
            this.btnClearAdditional.TabIndex = 77;
            this.btnClearAdditional.Text = "Clear";
            this.btnClearAdditional.UseVisualStyleBackColor = true;
            this.btnClearAdditional.Click += new System.EventHandler(this.btnClearAdditional_Click);
            // 
            // btnSetCalRule
            // 
            this.btnSetCalRule.Enabled = false;
            this.btnSetCalRule.Location = new System.Drawing.Point(409, 252);
            this.btnSetCalRule.Name = "btnSetCalRule";
            this.btnSetCalRule.Size = new System.Drawing.Size(90, 23);
            this.btnSetCalRule.TabIndex = 80;
            this.btnSetCalRule.Text = "Set Cal Rule";
            this.btnSetCalRule.UseVisualStyleBackColor = true;
            this.btnSetCalRule.Click += new System.EventHandler(this.btnSetCalRule_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 257);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 79;
            this.label4.Text = "Cal ID Style";
            // 
            // cbCalibrationIDStyle
            // 
            this.cbCalibrationIDStyle.FormattingEnabled = true;
            this.cbCalibrationIDStyle.Items.AddRange(new object[] {
            "Same for all steps",
            "Different each steps"});
            this.cbCalibrationIDStyle.Location = new System.Drawing.Point(101, 254);
            this.cbCalibrationIDStyle.Name = "cbCalibrationIDStyle";
            this.cbCalibrationIDStyle.Size = new System.Drawing.Size(148, 21);
            this.cbCalibrationIDStyle.TabIndex = 79;
            this.cbCalibrationIDStyle.Text = "Same for all steps";
            this.cbCalibrationIDStyle.SelectedIndexChanged += new System.EventHandler(this.cbCalibrationIDStyle_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(508, 188);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 55);
            this.label1.TabIndex = 72;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(671, 288);
            this.Controls.Add(this.btnSetCalRule);
            this.Controls.Add(this.cbCalibrationIDStyle);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbCalBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmdUseLastModifiedSequence);
            this.Controls.Add(this.lblFocusDistance);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.lblAbout);
            this.Controls.Add(this.btnClearAdditional);
            this.Controls.Add(this.cbSubframe);
            this.Controls.Add(this.cmdBrowseSequence);
            this.Controls.Add(this.cbFocusDistance);
            this.Controls.Add(this.lblAddtionalTarget);
            this.Controls.Add(this.lblCameraRotatioin);
            this.Controls.Add(this.btnBrowseAdditional);
            this.Controls.Add(this.cbFNumber);
            this.Controls.Add(this.lblSequencePath);
            this.Controls.Add(this.lblSubframe);
            this.Controls.Add(this.lblFNumber);
            this.Controls.Add(this.lblTargetSequence);
            this.Controls.Add(this.txtAdditionalSequence);
            this.Controls.Add(this.cbCameraRotation);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "SetSequence";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.LinkLabel cmdBrowseSequence;
        private System.Windows.Forms.Label lblAddtionalTarget;
        private System.Windows.Forms.LinkLabel cmdUseLastModifiedSequence;
        private System.Windows.Forms.ComboBox cbCameraRotation;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label lblSubframe;
        private System.Windows.Forms.Label lblCameraRotatioin;
        private System.Windows.Forms.ComboBox cbFocusDistance;
        private System.Windows.Forms.ComboBox cbSubframe;
        private System.Windows.Forms.Label lblFocusDistance;
        private System.Windows.Forms.ComboBox cbCalBox;
		private System.Windows.Forms.ComboBox cbFNumber;
		private System.Windows.Forms.Label lblFNumber;
		private System.Windows.Forms.Label lblAbout;
		private System.Windows.Forms.Label lblSequencePath;
		private System.Windows.Forms.TextBox txtAdditionalSequence;
		private System.Windows.Forms.Button btnBrowseAdditional;
		private System.Windows.Forms.Label lblTargetSequence;
        private System.Windows.Forms.Button btnClearAdditional;
        internal System.Windows.Forms.Button btnSetCalRule;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbCalibrationIDStyle;
        private System.Windows.Forms.Label label1;
    }
}

