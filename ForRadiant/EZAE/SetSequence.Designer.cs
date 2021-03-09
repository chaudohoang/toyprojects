
namespace EZAE
{
    partial class SetSequence
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
            this.lblSubframe = new System.Windows.Forms.Label();
            this.cbSubframe = new System.Windows.Forms.ComboBox();
            this.btnSaveSubframe = new System.Windows.Forms.Button();
            this.btnDelSubframe = new System.Windows.Forms.Button();
            this.cbFocusDistance = new System.Windows.Forms.ComboBox();
            this.lblFocusDistance = new System.Windows.Forms.Label();
            this.lblCameraRotatioin = new System.Windows.Forms.Label();
            this.cbCameraRotation = new System.Windows.Forms.ComboBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.cbCalBox = new System.Windows.Forms.ComboBox();
            this.lblCalibrationIDs = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.lblSequence = new System.Windows.Forms.Label();
            this.lblSequenceFileName = new System.Windows.Forms.Label();
            this.btnUseLastModified = new System.Windows.Forms.Button();
            this.btnBrowseSequence = new System.Windows.Forms.Button();
            this.lblSequencePath = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblSubframe
            // 
            this.lblSubframe.AutoSize = true;
            this.lblSubframe.Location = new System.Drawing.Point(2, 61);
            this.lblSubframe.Name = "lblSubframe";
            this.lblSubframe.Size = new System.Drawing.Size(52, 13);
            this.lblSubframe.TabIndex = 0;
            this.lblSubframe.Text = "Subframe";
            // 
            // cbSubframe
            // 
            this.cbSubframe.FormattingEnabled = true;
            this.cbSubframe.Location = new System.Drawing.Point(89, 58);
            this.cbSubframe.Name = "cbSubframe";
            this.cbSubframe.Size = new System.Drawing.Size(152, 21);
            this.cbSubframe.TabIndex = 1;
            // 
            // btnSaveSubframe
            // 
            this.btnSaveSubframe.Location = new System.Drawing.Point(247, 56);
            this.btnSaveSubframe.Name = "btnSaveSubframe";
            this.btnSaveSubframe.Size = new System.Drawing.Size(40, 23);
            this.btnSaveSubframe.TabIndex = 2;
            this.btnSaveSubframe.Text = "Save";
            this.btnSaveSubframe.UseVisualStyleBackColor = true;
            this.btnSaveSubframe.Click += new System.EventHandler(this.btnSaveSubframe_Click);
            // 
            // btnDelSubframe
            // 
            this.btnDelSubframe.Location = new System.Drawing.Point(293, 56);
            this.btnDelSubframe.Name = "btnDelSubframe";
            this.btnDelSubframe.Size = new System.Drawing.Size(40, 23);
            this.btnDelSubframe.TabIndex = 2;
            this.btnDelSubframe.Text = "Del";
            this.btnDelSubframe.UseVisualStyleBackColor = true;
            this.btnDelSubframe.Click += new System.EventHandler(this.btnDelSubframe_Click);
            // 
            // cbFocusDistance
            // 
            this.cbFocusDistance.FormattingEnabled = true;
            this.cbFocusDistance.Items.AddRange(new object[] {
            ""});
            this.cbFocusDistance.Location = new System.Drawing.Point(89, 85);
            this.cbFocusDistance.Name = "cbFocusDistance";
            this.cbFocusDistance.Size = new System.Drawing.Size(152, 21);
            this.cbFocusDistance.TabIndex = 4;
            // 
            // lblFocusDistance
            // 
            this.lblFocusDistance.AutoSize = true;
            this.lblFocusDistance.Location = new System.Drawing.Point(2, 88);
            this.lblFocusDistance.Name = "lblFocusDistance";
            this.lblFocusDistance.Size = new System.Drawing.Size(81, 13);
            this.lblFocusDistance.TabIndex = 3;
            this.lblFocusDistance.Text = "Focus Distance";
            // 
            // lblCameraRotatioin
            // 
            this.lblCameraRotatioin.AutoSize = true;
            this.lblCameraRotatioin.Location = new System.Drawing.Point(2, 115);
            this.lblCameraRotatioin.Name = "lblCameraRotatioin";
            this.lblCameraRotatioin.Size = new System.Drawing.Size(86, 13);
            this.lblCameraRotatioin.TabIndex = 5;
            this.lblCameraRotatioin.Text = "Camera Rotation";
            // 
            // cbCameraRotation
            // 
            this.cbCameraRotation.FormattingEnabled = true;
            this.cbCameraRotation.Items.AddRange(new object[] {
            "",
            "None",
            "Clockwise90",
            "Rotate180",
            "Counterclockwise90"});
            this.cbCameraRotation.Location = new System.Drawing.Point(89, 112);
            this.cbCameraRotation.Name = "cbCameraRotation";
            this.cbCameraRotation.Size = new System.Drawing.Size(152, 21);
            this.cbCameraRotation.TabIndex = 6;
            // 
            // cbCalBox
            // 
            this.cbCalBox.FormattingEnabled = true;
            this.cbCalBox.Items.AddRange(new object[] {
            "Copy from first step",
            "1,1,1,1"});
            this.cbCalBox.Location = new System.Drawing.Point(89, 139);
            this.cbCalBox.Name = "cbCalBox";
            this.cbCalBox.Size = new System.Drawing.Size(152, 21);
            this.cbCalBox.TabIndex = 8;
            this.cbCalBox.Text = "Copy from first step";
            // 
            // lblCalibrationIDs
            // 
            this.lblCalibrationIDs.AutoSize = true;
            this.lblCalibrationIDs.Location = new System.Drawing.Point(2, 142);
            this.lblCalibrationIDs.Name = "lblCalibrationIDs";
            this.lblCalibrationIDs.Size = new System.Drawing.Size(75, 13);
            this.lblCalibrationIDs.TabIndex = 7;
            this.lblCalibrationIDs.Text = "Calibration IDs";
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(247, 85);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(86, 75);
            this.btnApply.TabIndex = 9;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lblSequence
            // 
            this.lblSequence.AutoSize = true;
            this.lblSequence.Location = new System.Drawing.Point(2, 7);
            this.lblSequence.Name = "lblSequence";
            this.lblSequence.Size = new System.Drawing.Size(56, 13);
            this.lblSequence.TabIndex = 10;
            this.lblSequence.Text = "Sequence";
            // 
            // lblSequenceFileName
            // 
            this.lblSequenceFileName.AutoSize = true;
            this.lblSequenceFileName.Location = new System.Drawing.Point(88, 32);
            this.lblSequenceFileName.Name = "lblSequenceFileName";
            this.lblSequenceFileName.Size = new System.Drawing.Size(0, 13);
            this.lblSequenceFileName.TabIndex = 11;
            // 
            // btnUseLastModified
            // 
            this.btnUseLastModified.Location = new System.Drawing.Point(84, 2);
            this.btnUseLastModified.Name = "btnUseLastModified";
            this.btnUseLastModified.Size = new System.Drawing.Size(141, 23);
            this.btnUseLastModified.TabIndex = 2;
            this.btnUseLastModified.Text = "Use last modified";
            this.btnUseLastModified.UseVisualStyleBackColor = true;
            this.btnUseLastModified.Click += new System.EventHandler(this.btnUseLastModified_Click);
            // 
            // btnBrowseSequence
            // 
            this.btnBrowseSequence.Location = new System.Drawing.Point(231, 2);
            this.btnBrowseSequence.Name = "btnBrowseSequence";
            this.btnBrowseSequence.Size = new System.Drawing.Size(102, 23);
            this.btnBrowseSequence.TabIndex = 2;
            this.btnBrowseSequence.Text = "Browse";
            this.btnBrowseSequence.UseVisualStyleBackColor = true;
            this.btnBrowseSequence.Click += new System.EventHandler(this.btnBrowseSequence_Click);
            // 
            // lblSequencePath
            // 
            this.lblSequencePath.AutoSize = true;
            this.lblSequencePath.Location = new System.Drawing.Point(2, 32);
            this.lblSequencePath.Name = "lblSequencePath";
            this.lblSequencePath.Size = new System.Drawing.Size(81, 13);
            this.lblSequencePath.TabIndex = 12;
            this.lblSequencePath.Text = "Sequence Path";
            // 
            // SetSequence
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(339, 167);
            this.Controls.Add(this.lblSequencePath);
            this.Controls.Add(this.lblSequenceFileName);
            this.Controls.Add(this.lblSequence);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.cbCalBox);
            this.Controls.Add(this.lblCalibrationIDs);
            this.Controls.Add(this.cbCameraRotation);
            this.Controls.Add(this.lblCameraRotatioin);
            this.Controls.Add(this.cbFocusDistance);
            this.Controls.Add(this.lblFocusDistance);
            this.Controls.Add(this.btnBrowseSequence);
            this.Controls.Add(this.btnUseLastModified);
            this.Controls.Add(this.btnDelSubframe);
            this.Controls.Add(this.btnSaveSubframe);
            this.Controls.Add(this.cbSubframe);
            this.Controls.Add(this.lblSubframe);
            this.Name = "SetSequence";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SetSequence";
            this.Load += new System.EventHandler(this.FormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSubframe;
        private System.Windows.Forms.ComboBox cbSubframe;
        private System.Windows.Forms.Button btnSaveSubframe;
        private System.Windows.Forms.Button btnDelSubframe;
        private System.Windows.Forms.ComboBox cbFocusDistance;
        private System.Windows.Forms.Label lblFocusDistance;
        private System.Windows.Forms.Label lblCameraRotatioin;
        private System.Windows.Forms.ComboBox cbCameraRotation;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ComboBox cbCalBox;
        private System.Windows.Forms.Label lblCalibrationIDs;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label lblSequence;
        private System.Windows.Forms.Label lblSequenceFileName;
        private System.Windows.Forms.Button btnUseLastModified;
        private System.Windows.Forms.Button btnBrowseSequence;
        private System.Windows.Forms.Label lblSequencePath;
    }
}