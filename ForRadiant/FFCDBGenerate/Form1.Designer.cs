
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
			this.cbxNoOfPanels = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.cbxPanelNumberList = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(38, 26);
			this.label1.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(77, 37);
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
			this.cbLine.Location = new System.Drawing.Point(184, 17);
			this.cbLine.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.cbLine.Name = "cbLine";
			this.cbLine.Size = new System.Drawing.Size(185, 45);
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
			this.cbStation.Location = new System.Drawing.Point(184, 94);
			this.cbStation.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.cbStation.Name = "cbStation";
			this.cbStation.Size = new System.Drawing.Size(185, 45);
			this.cbStation.TabIndex = 2;
			this.cbStation.Text = "1";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(38, 102);
			this.label2.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(117, 37);
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
			this.cbChannel.Location = new System.Drawing.Point(184, 171);
			this.cbChannel.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.cbChannel.Name = "cbChannel";
			this.cbChannel.Size = new System.Drawing.Size(185, 45);
			this.cbChannel.TabIndex = 3;
			this.cbChannel.Text = "1";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(38, 179);
			this.label3.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(136, 37);
			this.label3.TabIndex = 4;
			this.label3.Text = "Channel";
			// 
			// cbSaveTo
			// 
			this.cbSaveTo.FormattingEnabled = true;
			this.cbSaveTo.Items.AddRange(new object[] {
            "D:\\Program\\RVS\\Log",
            "E:\\Program\\RVS\\Log"});
			this.cbSaveTo.Location = new System.Drawing.Point(184, 410);
			this.cbSaveTo.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.cbSaveTo.Name = "cbSaveTo";
			this.cbSaveTo.Size = new System.Drawing.Size(666, 45);
			this.cbSaveTo.TabIndex = 6;
			this.cbSaveTo.Text = "E:\\Program\\RVS\\Log";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(38, 418);
			this.label4.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(135, 37);
			this.label4.TabIndex = 6;
			this.label4.Text = "Save To";
			// 
			// chkOpenOutput
			// 
			this.chkOpenOutput.AutoSize = true;
			this.chkOpenOutput.Checked = true;
			this.chkOpenOutput.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkOpenOutput.Location = new System.Drawing.Point(48, 487);
			this.chkOpenOutput.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.chkOpenOutput.Name = "chkOpenOutput";
			this.chkOpenOutput.Size = new System.Drawing.Size(559, 41);
			this.chkOpenOutput.TabIndex = 7;
			this.chkOpenOutput.Text = "Open generated folder when finished";
			this.chkOpenOutput.UseVisualStyleBackColor = true;
			// 
			// btnGenerate
			// 
			this.btnGenerate.Location = new System.Drawing.Point(396, 14);
			this.btnGenerate.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.btnGenerate.Name = "btnGenerate";
			this.btnGenerate.Size = new System.Drawing.Size(374, 213);
			this.btnGenerate.TabIndex = 8;
			this.btnGenerate.Text = "Generate";
			this.btnGenerate.UseVisualStyleBackColor = true;
			this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
			// 
			// cbxNoOfPanels
			// 
			this.cbxNoOfPanels.FormattingEnabled = true;
			this.cbxNoOfPanels.Items.AddRange(new object[] {
            "3",
            "4",
            "5",
            "6",
            "7",
            "8"});
			this.cbxNoOfPanels.Location = new System.Drawing.Point(345, 248);
			this.cbxNoOfPanels.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.cbxNoOfPanels.Name = "cbxNoOfPanels";
			this.cbxNoOfPanels.Size = new System.Drawing.Size(185, 45);
			this.cbxNoOfPanels.TabIndex = 4;
			this.cbxNoOfPanels.Text = "5";
			this.cbxNoOfPanels.SelectedIndexChanged += new System.EventHandler(this.cbxNoOfPanels_SelectedIndexChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(38, 256);
			this.label5.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(271, 37);
			this.label5.TabIndex = 11;
			this.label5.Text = "How many panels";
			// 
			// cbxPanelNumberList
			// 
			this.cbxPanelNumberList.FormattingEnabled = true;
			this.cbxPanelNumberList.Location = new System.Drawing.Point(345, 324);
			this.cbxPanelNumberList.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
			this.cbxPanelNumberList.Name = "cbxPanelNumberList";
			this.cbxPanelNumberList.Size = new System.Drawing.Size(504, 45);
			this.cbxPanelNumberList.TabIndex = 5;
			this.cbxPanelNumberList.Text = "1,2,3,4,5";
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(38, 333);
			this.label6.Margin = new System.Windows.Forms.Padding(10, 0, 10, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(238, 37);
			this.label6.TabIndex = 13;
			this.label6.Text = "Panel Numbers";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(19F, 37F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1488, 569);
			this.Controls.Add(this.cbxPanelNumberList);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.cbxNoOfPanels);
			this.Controls.Add(this.label5);
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
			this.Margin = new System.Windows.Forms.Padding(10, 9, 10, 9);
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
		private System.Windows.Forms.ComboBox cbxNoOfPanels;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cbxPanelNumberList;
		private System.Windows.Forms.Label label6;
	}
}

