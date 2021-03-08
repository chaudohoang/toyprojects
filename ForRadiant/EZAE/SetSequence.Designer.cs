
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
            this.subframeBox = new System.Windows.Forms.ComboBox();
            this.btnAddSubframe = new System.Windows.Forms.Button();
            this.btnDelSubframe = new System.Windows.Forms.Button();
            this.focusBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.rotationBox = new System.Windows.Forms.ComboBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.calBox = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnApply = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.sequenceName = new System.Windows.Forms.Label();
            this.btnUseLastModified = new System.Windows.Forms.Button();
            this.btnBrowseSequence = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
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
            // subframeBox
            // 
            this.subframeBox.FormattingEnabled = true;
            this.subframeBox.Location = new System.Drawing.Point(89, 58);
            this.subframeBox.Name = "subframeBox";
            this.subframeBox.Size = new System.Drawing.Size(152, 21);
            this.subframeBox.TabIndex = 1;
            // 
            // btnAddSubframe
            // 
            this.btnAddSubframe.Location = new System.Drawing.Point(247, 56);
            this.btnAddSubframe.Name = "btnAddSubframe";
            this.btnAddSubframe.Size = new System.Drawing.Size(40, 23);
            this.btnAddSubframe.TabIndex = 2;
            this.btnAddSubframe.Text = "Save";
            this.btnAddSubframe.UseVisualStyleBackColor = true;
            this.btnAddSubframe.Click += new System.EventHandler(this.btnAddSubframe_Click);
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
            // focusBox
            // 
            this.focusBox.FormattingEnabled = true;
            this.focusBox.Items.AddRange(new object[] {
            ""});
            this.focusBox.Location = new System.Drawing.Point(89, 85);
            this.focusBox.Name = "focusBox";
            this.focusBox.Size = new System.Drawing.Size(152, 21);
            this.focusBox.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(2, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Focus Distance";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(2, 115);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Camera Rotation";
            // 
            // rotationBox
            // 
            this.rotationBox.FormattingEnabled = true;
            this.rotationBox.Items.AddRange(new object[] {
            "",
            "None",
            "Clockwise90",
            "Rotate180",
            "Counterclockwise90"});
            this.rotationBox.Location = new System.Drawing.Point(89, 112);
            this.rotationBox.Name = "rotationBox";
            this.rotationBox.Size = new System.Drawing.Size(152, 21);
            this.rotationBox.TabIndex = 6;
            // 
            // calBox
            // 
            this.calBox.FormattingEnabled = true;
            this.calBox.Items.AddRange(new object[] {
            "Copy from first step",
            "1,1,1,1"});
            this.calBox.Location = new System.Drawing.Point(89, 139);
            this.calBox.Name = "calBox";
            this.calBox.Size = new System.Drawing.Size(152, 21);
            this.calBox.TabIndex = 8;
            this.calBox.Text = "Copy from first step";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Calibration IDs";
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
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(2, 7);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Sequence";
            // 
            // sequenceName
            // 
            this.sequenceName.AutoSize = true;
            this.sequenceName.Location = new System.Drawing.Point(88, 32);
            this.sequenceName.Name = "sequenceName";
            this.sequenceName.Size = new System.Drawing.Size(0, 13);
            this.sequenceName.TabIndex = 11;
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
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(2, 32);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Sequence Path";
            // 
            // SetSequence
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(341, 167);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.sequenceName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.calBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.rotationBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.focusBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnBrowseSequence);
            this.Controls.Add(this.btnUseLastModified);
            this.Controls.Add(this.btnDelSubframe);
            this.Controls.Add(this.btnAddSubframe);
            this.Controls.Add(this.subframeBox);
            this.Controls.Add(this.lblSubframe);
            this.Name = "SetSequence";
            this.Text = "SetSequence";
            this.Load += new System.EventHandler(this.FormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSubframe;
        private System.Windows.Forms.ComboBox subframeBox;
        private System.Windows.Forms.Button btnAddSubframe;
        private System.Windows.Forms.Button btnDelSubframe;
        private System.Windows.Forms.ComboBox focusBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox rotationBox;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.ComboBox calBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label sequenceName;
        private System.Windows.Forms.Button btnUseLastModified;
        private System.Windows.Forms.Button btnBrowseSequence;
        private System.Windows.Forms.Label label5;
    }
}