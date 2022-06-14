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
			this.lblSequencePath = new System.Windows.Forms.Label();
			this.cmdUseLastModifiedSequence = new System.Windows.Forms.LinkLabel();
			this.cbCameraRotation = new System.Windows.Forms.ComboBox();
			this.btnApply = new System.Windows.Forms.Button();
			this.lblSubframe = new System.Windows.Forms.Label();
			this.lblCameraRotatioin = new System.Windows.Forms.Label();
			this.lblSequenceFileName = new System.Windows.Forms.Label();
			this.cbFocusDistance = new System.Windows.Forms.ComboBox();
			this.cbSubframe = new System.Windows.Forms.ComboBox();
			this.lblCalibrationIDs = new System.Windows.Forms.Label();
			this.lblFocusDistance = new System.Windows.Forms.Label();
			this.cbCalBox = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.cbFNumber = new System.Windows.Forms.ComboBox();
			this.lblFNumber = new System.Windows.Forms.Label();
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
			// lblSequencePath
			// 
			this.lblSequencePath.AutoSize = true;
			this.lblSequencePath.Location = new System.Drawing.Point(12, 31);
			this.lblSequencePath.Name = "lblSequencePath";
			this.lblSequencePath.Size = new System.Drawing.Size(99, 13);
			this.lblSequencePath.TabIndex = 64;
			this.lblSequencePath.Text = "Target Sequence : ";
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
			this.cbCameraRotation.Location = new System.Drawing.Point(99, 132);
			this.cbCameraRotation.Name = "cbCameraRotation";
			this.cbCameraRotation.Size = new System.Drawing.Size(148, 21);
			this.cbCameraRotation.TabIndex = 6;
			this.cbCameraRotation.Text = "Copy from first step";
			// 
			// btnApply
			// 
			this.btnApply.Location = new System.Drawing.Point(253, 51);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(91, 129);
			this.btnApply.TabIndex = 8;
			this.btnApply.Text = "Apply";
			this.btnApply.UseVisualStyleBackColor = true;
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// lblSubframe
			// 
			this.lblSubframe.AutoSize = true;
			this.lblSubframe.Location = new System.Drawing.Point(12, 54);
			this.lblSubframe.Name = "lblSubframe";
			this.lblSubframe.Size = new System.Drawing.Size(52, 13);
			this.lblSubframe.TabIndex = 59;
			this.lblSubframe.Text = "Subframe";
			// 
			// lblCameraRotatioin
			// 
			this.lblCameraRotatioin.AutoSize = true;
			this.lblCameraRotatioin.Location = new System.Drawing.Point(12, 135);
			this.lblCameraRotatioin.Name = "lblCameraRotatioin";
			this.lblCameraRotatioin.Size = new System.Drawing.Size(86, 13);
			this.lblCameraRotatioin.TabIndex = 61;
			this.lblCameraRotatioin.Text = "Camera Rotation";
			// 
			// lblSequenceFileName
			// 
			this.lblSequenceFileName.AutoSize = true;
			this.lblSequenceFileName.Location = new System.Drawing.Point(117, 31);
			this.lblSequenceFileName.Name = "lblSequenceFileName";
			this.lblSequenceFileName.Size = new System.Drawing.Size(0, 13);
			this.lblSequenceFileName.TabIndex = 63;
			// 
			// cbFocusDistance
			// 
			this.cbFocusDistance.FormattingEnabled = true;
			this.cbFocusDistance.Items.AddRange(new object[] {
            "",
            "Copy from first step"});
			this.cbFocusDistance.Location = new System.Drawing.Point(99, 78);
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
			this.cbSubframe.Location = new System.Drawing.Point(99, 51);
			this.cbSubframe.Name = "cbSubframe";
			this.cbSubframe.Size = new System.Drawing.Size(148, 21);
			this.cbSubframe.TabIndex = 3;
			this.cbSubframe.Text = "Copy from first step";
			// 
			// lblCalibrationIDs
			// 
			this.lblCalibrationIDs.AutoSize = true;
			this.lblCalibrationIDs.Location = new System.Drawing.Point(12, 162);
			this.lblCalibrationIDs.Name = "lblCalibrationIDs";
			this.lblCalibrationIDs.Size = new System.Drawing.Size(75, 13);
			this.lblCalibrationIDs.TabIndex = 62;
			this.lblCalibrationIDs.Text = "Calibration IDs";
			// 
			// lblFocusDistance
			// 
			this.lblFocusDistance.AutoSize = true;
			this.lblFocusDistance.Location = new System.Drawing.Point(12, 81);
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
			this.cbCalBox.Location = new System.Drawing.Point(99, 159);
			this.cbCalBox.Name = "cbCalBox";
			this.cbCalBox.Size = new System.Drawing.Size(148, 21);
			this.cbCalBox.TabIndex = 7;
			this.cbCalBox.Text = "Copy from first step";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.label1.Location = new System.Drawing.Point(366, 65);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(0, 55);
			this.label1.TabIndex = 72;
			// 
			// cbFNumber
			// 
			this.cbFNumber.FormattingEnabled = true;
			this.cbFNumber.Items.AddRange(new object[] {
            "",
            "Copy from first step",
            "8"});
			this.cbFNumber.Location = new System.Drawing.Point(99, 105);
			this.cbFNumber.Name = "cbFNumber";
			this.cbFNumber.Size = new System.Drawing.Size(148, 21);
			this.cbFNumber.TabIndex = 5;
			this.cbFNumber.Text = "Copy from first step";
			// 
			// lblFNumber
			// 
			this.lblFNumber.AutoSize = true;
			this.lblFNumber.Location = new System.Drawing.Point(12, 108);
			this.lblFNumber.Name = "lblFNumber";
			this.lblFNumber.Size = new System.Drawing.Size(53, 13);
			this.lblFNumber.TabIndex = 73;
			this.lblFNumber.Text = "F Number";
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(596, 191);
			this.Controls.Add(this.cbFNumber);
			this.Controls.Add(this.lblFNumber);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cmdBrowseSequence);
			this.Controls.Add(this.lblSequencePath);
			this.Controls.Add(this.cmdUseLastModifiedSequence);
			this.Controls.Add(this.cbCameraRotation);
			this.Controls.Add(this.btnApply);
			this.Controls.Add(this.lblSubframe);
			this.Controls.Add(this.lblCameraRotatioin);
			this.Controls.Add(this.lblSequenceFileName);
			this.Controls.Add(this.cbFocusDistance);
			this.Controls.Add(this.cbSubframe);
			this.Controls.Add(this.lblCalibrationIDs);
			this.Controls.Add(this.lblFocusDistance);
			this.Controls.Add(this.cbCalBox);
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
        private System.Windows.Forms.Label lblSequencePath;
        private System.Windows.Forms.LinkLabel cmdUseLastModifiedSequence;
        private System.Windows.Forms.ComboBox cbCameraRotation;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label lblSubframe;
        private System.Windows.Forms.Label lblCameraRotatioin;
        private System.Windows.Forms.Label lblSequenceFileName;
        private System.Windows.Forms.ComboBox cbFocusDistance;
        private System.Windows.Forms.ComboBox cbSubframe;
        private System.Windows.Forms.Label lblCalibrationIDs;
        private System.Windows.Forms.Label lblFocusDistance;
        private System.Windows.Forms.ComboBox cbCalBox;
        private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cbFNumber;
		private System.Windows.Forms.Label lblFNumber;
	}
}

