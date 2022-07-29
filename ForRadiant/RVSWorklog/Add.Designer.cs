
namespace RVSWorklog
{
	partial class Add
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Add));
			this.label1 = new System.Windows.Forms.Label();
			this.txtDate = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtLog = new System.Windows.Forms.TextBox();
			this.btnSave = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.cbxChanel = new System.Windows.Forms.ComboBox();
			this.label4 = new System.Windows.Forms.Label();
			this.cbxLine = new System.Windows.Forms.ComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.cbxType = new System.Windows.Forms.ComboBox();
			this.label6 = new System.Windows.Forms.Label();
			this.cbxStation = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(39, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Date : ";
			// 
			// txtDate
			// 
			this.txtDate.Location = new System.Drawing.Point(57, 6);
			this.txtDate.Name = "txtDate";
			this.txtDate.ReadOnly = true;
			this.txtDate.Size = new System.Drawing.Size(131, 20);
			this.txtDate.TabIndex = 1;
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(34, 13);
			this.label2.TabIndex = 10;
			this.label2.Text = "Log : ";
			// 
			// txtLog
			// 
			this.txtLog.Location = new System.Drawing.Point(57, 31);
			this.txtLog.Multiline = true;
			this.txtLog.Name = "txtLog";
			this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtLog.Size = new System.Drawing.Size(562, 311);
			this.txtLog.TabIndex = 11;
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(4, 301);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(47, 41);
			this.btnSave.TabIndex = 12;
			this.btnSave.Text = "Save";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			this.btnSave.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Add_KeyDown);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(319, 9);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(49, 13);
			this.label3.TabIndex = 4;
			this.label3.Text = "Station : ";
			// 
			// cbxChanel
			// 
			this.cbxChanel.FormattingEnabled = true;
			this.cbxChanel.Items.AddRange(new object[] {
            "",
            "1",
            "2",
            "3",
            "4"});
			this.cbxChanel.Location = new System.Drawing.Point(474, 6);
			this.cbxChanel.Name = "cbxChanel";
			this.cbxChanel.Size = new System.Drawing.Size(39, 21);
			this.cbxChanel.TabIndex = 7;
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(419, 9);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(49, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "Chanel : ";
			// 
			// cbxLine
			// 
			this.cbxLine.FormattingEnabled = true;
			this.cbxLine.Items.AddRange(new object[] {
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
            "X1080 GIB",
            "H13F PGIB",
            "H13F MGIB",
            "H24F PGIB",
            "H24F MGIB",
            "H25F PGIB",
            "H25F MGIB"});
			this.cbxLine.Location = new System.Drawing.Point(236, 6);
			this.cbxLine.Name = "cbxLine";
			this.cbxLine.Size = new System.Drawing.Size(77, 21);
			this.cbxLine.TabIndex = 3;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(194, 9);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(36, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "Line : ";
			// 
			// cbxType
			// 
			this.cbxType.FormattingEnabled = true;
			this.cbxType.Items.AddRange(new object[] {
            "",
            "Mono",
            "Color"});
			this.cbxType.Location = new System.Drawing.Point(565, 6);
			this.cbxType.Name = "cbxType";
			this.cbxType.Size = new System.Drawing.Size(54, 21);
			this.cbxType.TabIndex = 9;
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(519, 9);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(40, 13);
			this.label6.TabIndex = 8;
			this.label6.Text = "Type : ";
			// 
			// cbxStation
			// 
			this.cbxStation.FormattingEnabled = true;
			this.cbxStation.Items.AddRange(new object[] {
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
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20"});
			this.cbxStation.Location = new System.Drawing.Point(374, 6);
			this.cbxStation.Name = "cbxStation";
			this.cbxStation.Size = new System.Drawing.Size(39, 21);
			this.cbxStation.TabIndex = 5;
			// 
			// Add
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(627, 350);
			this.Controls.Add(this.cbxStation);
			this.Controls.Add(this.cbxType);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.cbxLine);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.cbxChanel);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.txtLog);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.txtDate);
			this.Controls.Add(this.label1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.Name = "Add";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Add";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Add_KeyDown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox txtDate;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtLog;
		private System.Windows.Forms.Button btnSave;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cbxChanel;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox cbxLine;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cbxType;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.ComboBox cbxStation;
	}
}