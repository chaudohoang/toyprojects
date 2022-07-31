
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
            this.btnExportAll = new System.Windows.Forms.Button();
            this.btnAdmin = new System.Windows.Forms.Button();
            this.btnExportCurrent = new System.Windows.Forms.Button();
            this.txtDateSearch = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lstDate = new System.Windows.Forms.ListBox();
            this.lblToday = new System.Windows.Forms.Label();
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
            this.cbxWorker.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbxWorker.FormattingEnabled = true;
            this.cbxWorker.Items.AddRange(new object[] {
            "Bùi Duy Kiên",
            "Bùi Hải Phong",
            "Đào Như Quyền",
            "Đỗ Hoàng Châu",
            "Đoàn Xuân Hiệp",
            "Gwon Hyuk Lyong",
            "Hwang JunSeok",
            "Kim Kang Hyun",
            "Kim Sun Do",
            "Kim SuRyong",
            "Kwon Hyeon Tae",
            "Kwon Kwang Ho",
            "Lê Hoàng Minh",
            "Min KyeongWon",
            "Nguyễn Huy Hoàng",
            "Nguyễn Khánh Tùng",
            "Nguyễn Quốc Trọng",
            "Nguyễn Sơn Tùng",
            "Nguyễn Tiến Tùng",
            "Nguyễn Văn Hải",
            "Nguyễn Việt Thiện",
            "Phạm Văn Tâm",
            "Phan Thanh Trung",
            "Trần Quang Ngọc",
            "Vũ Công Hùng",
            "Yoon Sung Mo"});
            this.cbxWorker.Location = new System.Drawing.Point(81, 6);
            this.cbxWorker.Name = "cbxWorker";
            this.cbxWorker.Size = new System.Drawing.Size(301, 21);
            this.cbxWorker.Sorted = true;
            this.cbxWorker.TabIndex = 1;
            this.cbxWorker.SelectedIndexChanged += new System.EventHandler(this.cbxWorker_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 36);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Work Log : ";
            // 
            // txtWorklog
            // 
            this.txtWorklog.Location = new System.Drawing.Point(239, 61);
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
            this.btnAdd.Text = "Write Log";
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
            this.label3.Location = new System.Drawing.Point(236, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Today : ";
            // 
            // btnExportAll
            // 
            this.btnExportAll.Enabled = false;
            this.btnExportAll.Location = new System.Drawing.Point(15, 147);
            this.btnExportAll.Name = "btnExportAll";
            this.btnExportAll.Size = new System.Drawing.Size(51, 61);
            this.btnExportAll.TabIndex = 7;
            this.btnExportAll.Text = "Export All Log";
            this.btnExportAll.UseVisualStyleBackColor = true;
            this.btnExportAll.Click += new System.EventHandler(this.btnExportAll_Click);
            // 
            // btnAdmin
            // 
            this.btnAdmin.Location = new System.Drawing.Point(891, 9);
            this.btnAdmin.Name = "btnAdmin";
            this.btnAdmin.Size = new System.Drawing.Size(53, 23);
            this.btnAdmin.TabIndex = 8;
            this.btnAdmin.Text = "Admin";
            this.btnAdmin.UseVisualStyleBackColor = true;
            this.btnAdmin.Click += new System.EventHandler(this.btnAdmin_Click);
            // 
            // btnExportCurrent
            // 
            this.btnExportCurrent.Enabled = false;
            this.btnExportCurrent.Location = new System.Drawing.Point(15, 214);
            this.btnExportCurrent.Name = "btnExportCurrent";
            this.btnExportCurrent.Size = new System.Drawing.Size(51, 61);
            this.btnExportCurrent.TabIndex = 9;
            this.btnExportCurrent.Text = "Export Current";
            this.btnExportCurrent.UseVisualStyleBackColor = true;
            this.btnExportCurrent.Click += new System.EventHandler(this.btnExportCurrent_Click);
            // 
            // txtDateSearch
            // 
            this.txtDateSearch.Location = new System.Drawing.Point(160, 33);
            this.txtDateSearch.Name = "txtDateSearch";
            this.txtDateSearch.ReadOnly = true;
            this.txtDateSearch.Size = new System.Drawing.Size(70, 20);
            this.txtDateSearch.TabIndex = 19;
            this.txtDateSearch.TextChanged += new System.EventHandler(this.txtDateSearch_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(81, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Date Search : ";
            // 
            // lstDate
            // 
            this.lstDate.Enabled = false;
            this.lstDate.FormattingEnabled = true;
            this.lstDate.Location = new System.Drawing.Point(81, 59);
            this.lstDate.Name = "lstDate";
            this.lstDate.ScrollAlwaysVisible = true;
            this.lstDate.Size = new System.Drawing.Size(152, 381);
            this.lstDate.Sorted = true;
            this.lstDate.TabIndex = 17;
            this.lstDate.SelectedIndexChanged += new System.EventHandler(this.lstDate_SelectedIndexChanged);
            // 
            // lblToday
            // 
            this.lblToday.AutoSize = true;
            this.lblToday.Location = new System.Drawing.Point(288, 36);
            this.lblToday.Name = "lblToday";
            this.lblToday.Size = new System.Drawing.Size(0, 13);
            this.lblToday.TabIndex = 20;
            // 
            // MainForm
            // 
            this.AcceptButton = this.btnAdd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(956, 450);
            this.Controls.Add(this.lblToday);
            this.Controls.Add(this.txtDateSearch);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lstDate);
            this.Controls.Add(this.btnExportCurrent);
            this.Controls.Add(this.btnAdmin);
            this.Controls.Add(this.btnExportAll);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtWorklog);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbxWorker);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
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
		private System.Windows.Forms.Button btnExportAll;
		private System.Windows.Forms.Button btnAdmin;
		private System.Windows.Forms.Button btnExportCurrent;
        private System.Windows.Forms.TextBox txtDateSearch;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lstDate;
        private System.Windows.Forms.Label lblToday;
    }
}

