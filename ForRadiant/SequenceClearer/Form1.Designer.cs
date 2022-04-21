
namespace SequenceCleaner
{
	partial class Form1
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.cmdBrowseSequence = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.txtSequenceFilePath = new System.Windows.Forms.TextBox();
			this.btnClean = new System.Windows.Forms.Button();
			this.chkBackup = new System.Windows.Forms.CheckBox();
			this.lblDone = new System.Windows.Forms.Label();
			this.chkClearCCD = new System.Windows.Forms.CheckBox();
			this.chkClearCosineCorrection = new System.Windows.Forms.CheckBox();
			this.chkClearMaskBitmap = new System.Windows.Forms.CheckBox();
			this.chkSaveToDBFalse = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// cmdBrowseSequence
			// 
			this.cmdBrowseSequence.AutoSize = true;
			this.cmdBrowseSequence.Location = new System.Drawing.Point(12, 9);
			this.cmdBrowseSequence.Name = "cmdBrowseSequence";
			this.cmdBrowseSequence.Size = new System.Drawing.Size(92, 13);
			this.cmdBrowseSequence.TabIndex = 67;
			this.cmdBrowseSequence.TabStop = true;
			this.cmdBrowseSequence.Text = "Browse sequence";
			this.cmdBrowseSequence.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.cmdBrowseSequence_LinkClicked);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 32);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(97, 13);
			this.label1.TabIndex = 68;
			this.label1.Text = "Target sequence : ";
			// 
			// txtSequenceFilePath
			// 
			this.txtSequenceFilePath.Location = new System.Drawing.Point(115, 29);
			this.txtSequenceFilePath.Name = "txtSequenceFilePath";
			this.txtSequenceFilePath.Size = new System.Drawing.Size(401, 20);
			this.txtSequenceFilePath.TabIndex = 69;
			// 
			// btnClean
			// 
			this.btnClean.Location = new System.Drawing.Point(15, 57);
			this.btnClean.Name = "btnClean";
			this.btnClean.Size = new System.Drawing.Size(75, 113);
			this.btnClean.TabIndex = 70;
			this.btnClean.Text = "Clear";
			this.btnClean.UseVisualStyleBackColor = true;
			this.btnClean.Click += new System.EventHandler(this.btnClean_Click);
			// 
			// chkBackup
			// 
			this.chkBackup.AutoSize = true;
			this.chkBackup.Checked = true;
			this.chkBackup.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkBackup.Location = new System.Drawing.Point(96, 61);
			this.chkBackup.Name = "chkBackup";
			this.chkBackup.Size = new System.Drawing.Size(82, 17);
			this.chkBackup.TabIndex = 71;
			this.chkBackup.Text = "Backup first";
			this.chkBackup.UseVisualStyleBackColor = true;
			// 
			// lblDone
			// 
			this.lblDone.AutoSize = true;
			this.lblDone.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblDone.Location = new System.Drawing.Point(377, 107);
			this.lblDone.Name = "lblDone";
			this.lblDone.Size = new System.Drawing.Size(0, 31);
			this.lblDone.TabIndex = 72;
			// 
			// chkClearCCD
			// 
			this.chkClearCCD.AutoSize = true;
			this.chkClearCCD.Checked = true;
			this.chkClearCCD.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkClearCCD.Location = new System.Drawing.Point(96, 84);
			this.chkClearCCD.Name = "chkClearCCD";
			this.chkClearCCD.Size = new System.Drawing.Size(126, 17);
			this.chkClearCCD.TabIndex = 73;
			this.chkClearCCD.Text = "Clear CCD Parameter";
			this.chkClearCCD.UseVisualStyleBackColor = true;
			// 
			// chkClearCosineCorrection
			// 
			this.chkClearCosineCorrection.AutoSize = true;
			this.chkClearCosineCorrection.Checked = true;
			this.chkClearCosineCorrection.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkClearCosineCorrection.Location = new System.Drawing.Point(96, 107);
			this.chkClearCosineCorrection.Name = "chkClearCosineCorrection";
			this.chkClearCosineCorrection.Size = new System.Drawing.Size(187, 17);
			this.chkClearCosineCorrection.TabIndex = 74;
			this.chkClearCosineCorrection.Text = "Clear Cosine Correction Parameter";
			this.chkClearCosineCorrection.UseVisualStyleBackColor = true;
			// 
			// chkClearMaskBitmap
			// 
			this.chkClearMaskBitmap.AutoSize = true;
			this.chkClearMaskBitmap.Checked = true;
			this.chkClearMaskBitmap.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkClearMaskBitmap.Location = new System.Drawing.Point(96, 130);
			this.chkClearMaskBitmap.Name = "chkClearMaskBitmap";
			this.chkClearMaskBitmap.Size = new System.Drawing.Size(165, 17);
			this.chkClearMaskBitmap.TabIndex = 75;
			this.chkClearMaskBitmap.Text = "Clear Mask Bitmap Parameter";
			this.chkClearMaskBitmap.UseVisualStyleBackColor = true;
			// 
			// chkSaveToDBFalse
			// 
			this.chkSaveToDBFalse.AutoSize = true;
			this.chkSaveToDBFalse.Checked = true;
			this.chkSaveToDBFalse.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkSaveToDBFalse.Location = new System.Drawing.Point(96, 153);
			this.chkSaveToDBFalse.Name = "chkSaveToDBFalse";
			this.chkSaveToDBFalse.Size = new System.Drawing.Size(156, 17);
			this.chkSaveToDBFalse.TabIndex = 76;
			this.chkSaveToDBFalse.Text = "Save To Database -> False";
			this.chkSaveToDBFalse.UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(530, 178);
			this.Controls.Add(this.chkSaveToDBFalse);
			this.Controls.Add(this.chkClearMaskBitmap);
			this.Controls.Add(this.chkClearCosineCorrection);
			this.Controls.Add(this.chkClearCCD);
			this.Controls.Add(this.lblDone);
			this.Controls.Add(this.chkBackup);
			this.Controls.Add(this.btnClean);
			this.Controls.Add(this.txtSequenceFilePath);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.cmdBrowseSequence);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Form1";
			this.Text = "Sequence Clearer";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.LinkLabel cmdBrowseSequence;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtSequenceFilePath;
		private System.Windows.Forms.Button btnClean;
		private System.Windows.Forms.CheckBox chkBackup;
		private System.Windows.Forms.Label lblDone;
		private System.Windows.Forms.CheckBox chkClearCCD;
		private System.Windows.Forms.CheckBox chkClearCosineCorrection;
		private System.Windows.Forms.CheckBox chkClearMaskBitmap;
		private System.Windows.Forms.CheckBox chkSaveToDBFalse;
	}
}

