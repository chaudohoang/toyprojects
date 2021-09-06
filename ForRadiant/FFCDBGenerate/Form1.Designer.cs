
namespace FFCDBGenerate
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
            this.label1 = new System.Windows.Forms.Label();
            this.cbLine = new System.Windows.Forms.ComboBox();
            this.cbStation = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbChannel = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbSaveTo = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkOpenOutput = new System.Windows.Forms.CheckBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(27, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Line";
            // 
            // cbLine
            // 
            this.cbLine.FormattingEnabled = true;
            this.cbLine.Items.AddRange(new object[] {
            "",
            "301",
            "302",
            "303",
            "304",
            "305",
            "306",
            "401",
            "402",
            "403",
            "404",
            "405",
            "406",
            "501",
            "502",
            "503",
            "504",
            "505",
            "506",
            "X1080",
            "H12F MGIB",
            "H12F PGIB",
            "H13F MGIB",
            "H13F PGIB",
            "H24F MGIB",
            "H24F PGIB",
            "H25F MGIB",
            "H25F PGIB"});
            this.cbLine.Location = new System.Drawing.Point(58, 6);
            this.cbLine.Name = "cbLine";
            this.cbLine.Size = new System.Drawing.Size(61, 21);
            this.cbLine.TabIndex = 1;
            this.cbLine.Text = "504";
            // 
            // cbStation
            // 
            this.cbStation.FormattingEnabled = true;
            this.cbStation.Items.AddRange(new object[] {
            "",
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12"});
            this.cbStation.Location = new System.Drawing.Point(58, 33);
            this.cbStation.Name = "cbStation";
            this.cbStation.Size = new System.Drawing.Size(61, 21);
            this.cbStation.TabIndex = 3;
            this.cbStation.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Station";
            // 
            // cbChannel
            // 
            this.cbChannel.FormattingEnabled = true;
            this.cbChannel.Items.AddRange(new object[] {
            "",
            "1",
            "2",
            "3",
            "4"});
            this.cbChannel.Location = new System.Drawing.Point(58, 60);
            this.cbChannel.Name = "cbChannel";
            this.cbChannel.Size = new System.Drawing.Size(61, 21);
            this.cbChannel.TabIndex = 5;
            this.cbChannel.Text = "1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Channel";
            // 
            // cbSaveTo
            // 
            this.cbSaveTo.FormattingEnabled = true;
            this.cbSaveTo.Items.AddRange(new object[] {
            "",
            "D:\\Program\\RVS\\Log",
            "E:\\Program\\RVS\\Log"});
            this.cbSaveTo.Location = new System.Drawing.Point(58, 87);
            this.cbSaveTo.Name = "cbSaveTo";
            this.cbSaveTo.Size = new System.Drawing.Size(185, 21);
            this.cbSaveTo.TabIndex = 7;
            this.cbSaveTo.Text = "E:\\Program\\RVS\\Log";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 90);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(48, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Save To";
            // 
            // chkOpenOutput
            // 
            this.chkOpenOutput.AutoSize = true;
            this.chkOpenOutput.Checked = true;
            this.chkOpenOutput.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOpenOutput.Location = new System.Drawing.Point(15, 114);
            this.chkOpenOutput.Name = "chkOpenOutput";
            this.chkOpenOutput.Size = new System.Drawing.Size(200, 17);
            this.chkOpenOutput.TabIndex = 9;
            this.chkOpenOutput.Text = "Open generated folder when finished";
            this.chkOpenOutput.UseVisualStyleBackColor = true;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(125, 6);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(118, 75);
            this.btnGenerate.TabIndex = 10;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(470, 140);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.chkOpenOutput);
            this.Controls.Add(this.cbSaveTo);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbChannel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cbStation);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbLine);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "FFCDBGenerate";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbLine;
        private System.Windows.Forms.ComboBox cbStation;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbChannel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbSaveTo;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkOpenOutput;
        private System.Windows.Forms.Button btnGenerate;
    }
}

