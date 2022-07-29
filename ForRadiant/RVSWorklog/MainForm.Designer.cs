
namespace RVSWorklog
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
			this.cbxWorker = new System.Windows.Forms.ComboBox();
			this.label2 = new System.Windows.Forms.Label();
			this.txtWorklog = new System.Windows.Forms.TextBox();
			this.btnAdd = new System.Windows.Forms.Button();
			this.lblStatus = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.dateFilter = new System.Windows.Forms.DateTimePicker();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(51, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "Worker : ";
			// 
			// cbxWorker
			// 
			this.cbxWorker.FormattingEnabled = true;
			this.cbxWorker.Items.AddRange(new object[] {
            "Hwang JunSeok",
            "Kim Kang Hyun",
            "Kwon Kwang Ho",
            "Kim Sun Do",
            "Yoon Sung Mo",
            "Kwon Hyeon Tae",
            "Gwon Hyuk Lyong",
            "Kim SuRyong",
            "Min KyeongWon",
            "Đỗ Hoàng Châu",
            "Đào Như Quyền",
            "Lê Hoàng Minh",
            "Nguyễn Huy Hoàng",
            "Nguyễn Sơn Tùng",
            "Nguyễn Tiến Tùng",
            "Phan Thanh Trung",
            "Nguyễn Quốc Trọng",
            "Nguyễn Văn Hải",
            "Phạm Văn Tâm",
            "Vũ Công Hùng",
            "Nguyễn Khánh Tùng",
            "Nguyễn Việt Thiện",
            "Trần Quang Ngọc",
            "Bùi Hải Phong",
            "Bùi Duy Kiên",
            "Đoàn Xuân Hiệp"});
			this.cbxWorker.Location = new System.Drawing.Point(81, 6);
			this.cbxWorker.Name = "cbxWorker";
			this.cbxWorker.Size = new System.Drawing.Size(301, 21);
			this.cbxWorker.TabIndex = 1;
			this.cbxWorker.SelectedIndexChanged += new System.EventHandler(this.cbxWorker_SelectedIndexChanged);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 39);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(63, 13);
			this.label2.TabIndex = 2;
			this.label2.Text = "Work Log : ";
			// 
			// txtWorklog
			// 
			this.txtWorklog.Location = new System.Drawing.Point(81, 59);
			this.txtWorklog.Multiline = true;
			this.txtWorklog.Name = "txtWorklog";
			this.txtWorklog.ReadOnly = true;
			this.txtWorklog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtWorklog.Size = new System.Drawing.Size(707, 379);
			this.txtWorklog.TabIndex = 5;
			this.txtWorklog.TextChanged += new System.EventHandler(this.txtWorklog_TextChanged);
			// 
			// btnAdd
			// 
			this.btnAdd.Enabled = false;
			this.btnAdd.Location = new System.Drawing.Point(15, 80);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(51, 61);
			this.btnAdd.TabIndex = 6;
			this.btnAdd.Text = "Add Log";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			this.btnAdd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
			// 
			// lblStatus
			// 
			this.lblStatus.AutoSize = true;
			this.lblStatus.Location = new System.Drawing.Point(388, 9);
			this.lblStatus.Name = "lblStatus";
			this.lblStatus.Size = new System.Drawing.Size(0, 13);
			this.lblStatus.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(81, 39);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(61, 13);
			this.label3.TabIndex = 3;
			this.label3.Text = "Date filter : ";
			// 
			// dateFilter
			// 
			this.dateFilter.CustomFormat = "";
			this.dateFilter.Format = System.Windows.Forms.DateTimePickerFormat.Short;
			this.dateFilter.Location = new System.Drawing.Point(148, 33);
			this.dateFilter.Name = "dateFilter";
			this.dateFilter.Size = new System.Drawing.Size(103, 20);
			this.dateFilter.TabIndex = 4;
			this.dateFilter.CloseUp += new System.EventHandler(this.dateFilter_CloseUp);
			// 
			// MainForm
			// 
			this.AcceptButton = this.btnAdd;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.dateFilter);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lblStatus);
			this.Controls.Add(this.btnAdd);
			this.Controls.Add(this.txtWorklog);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.cbxWorker);
			this.Controls.Add(this.label1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "RVSWorklog";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyDown);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox cbxWorker;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtWorklog;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.Label lblStatus;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.DateTimePicker dateFilter;
	}
}

