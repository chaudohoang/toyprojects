
namespace RVSWorklog
{
	partial class Admin
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Admin));
			this.txtWorklog = new System.Windows.Forms.TextBox();
			this.btnRefresh = new System.Windows.Forms.Button();
			this.lstDate = new System.Windows.Forms.ListBox();
			this.lstWorker = new System.Windows.Forms.ListBox();
			this.txtWorkerSearch = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtDateSearch = new System.Windows.Forms.TextBox();
			this.btnExportDay = new System.Windows.Forms.Button();
			this.btnExportAll = new System.Windows.Forms.Button();
			this.btnExportCurrent = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// txtWorklog
			// 
			this.txtWorklog.Location = new System.Drawing.Point(383, 33);
			this.txtWorklog.Multiline = true;
			this.txtWorklog.Name = "txtWorklog";
			this.txtWorklog.ReadOnly = true;
			this.txtWorklog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtWorklog.Size = new System.Drawing.Size(707, 381);
			this.txtWorklog.TabIndex = 6;
			// 
			// btnRefresh
			// 
			this.btnRefresh.Location = new System.Drawing.Point(659, 5);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(79, 23);
			this.btnRefresh.TabIndex = 9;
			this.btnRefresh.Text = "Refresh";
			this.btnRefresh.UseVisualStyleBackColor = true;
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// lstDate
			// 
			this.lstDate.FormattingEnabled = true;
			this.lstDate.Location = new System.Drawing.Point(12, 33);
			this.lstDate.Name = "lstDate";
			this.lstDate.ScrollAlwaysVisible = true;
			this.lstDate.Size = new System.Drawing.Size(152, 381);
			this.lstDate.TabIndex = 10;
			this.lstDate.SelectedIndexChanged += new System.EventHandler(this.lstDate_SelectedIndexChanged);
			// 
			// lstWorker
			// 
			this.lstWorker.FormattingEnabled = true;
			this.lstWorker.Location = new System.Drawing.Point(170, 33);
			this.lstWorker.Name = "lstWorker";
			this.lstWorker.ScrollAlwaysVisible = true;
			this.lstWorker.Size = new System.Drawing.Size(207, 381);
			this.lstWorker.TabIndex = 11;
			this.lstWorker.SelectedIndexChanged += new System.EventHandler(this.lstWorker_SelectedIndexChanged);
			// 
			// txtWorkerSearch
			// 
			this.txtWorkerSearch.Location = new System.Drawing.Point(264, 7);
			this.txtWorkerSearch.Name = "txtWorkerSearch";
			this.txtWorkerSearch.Size = new System.Drawing.Size(110, 20);
			this.txtWorkerSearch.TabIndex = 12;
			this.txtWorkerSearch.TextChanged += new System.EventHandler(this.txtWorkerSearch_TextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(170, 14);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(88, 13);
			this.label1.TabIndex = 14;
			this.label1.Text = "Worker Search : ";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(12, 14);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(76, 13);
			this.label2.TabIndex = 15;
			this.label2.Text = "Date Search : ";
			// 
			// txtDateSearch
			// 
			this.txtDateSearch.Location = new System.Drawing.Point(94, 7);
			this.txtDateSearch.Name = "txtDateSearch";
			this.txtDateSearch.Size = new System.Drawing.Size(70, 20);
			this.txtDateSearch.TabIndex = 16;
			this.txtDateSearch.TextChanged += new System.EventHandler(this.txtDateSearch_TextChanged);
			// 
			// btnExportDay
			// 
			this.btnExportDay.Location = new System.Drawing.Point(471, 5);
			this.btnExportDay.Name = "btnExportDay";
			this.btnExportDay.Size = new System.Drawing.Size(88, 23);
			this.btnExportDay.TabIndex = 18;
			this.btnExportDay.Text = "Export Day Log";
			this.btnExportDay.UseVisualStyleBackColor = true;
			this.btnExportDay.Click += new System.EventHandler(this.btnExportDay_Click);
			// 
			// btnExportAll
			// 
			this.btnExportAll.Location = new System.Drawing.Point(383, 5);
			this.btnExportAll.Name = "btnExportAll";
			this.btnExportAll.Size = new System.Drawing.Size(82, 23);
			this.btnExportAll.TabIndex = 19;
			this.btnExportAll.Text = "Export All Log";
			this.btnExportAll.UseVisualStyleBackColor = true;
			this.btnExportAll.Click += new System.EventHandler(this.btnExportAll_Click);
			// 
			// btnExportCurrent
			// 
			this.btnExportCurrent.Location = new System.Drawing.Point(565, 5);
			this.btnExportCurrent.Name = "btnExportCurrent";
			this.btnExportCurrent.Size = new System.Drawing.Size(88, 23);
			this.btnExportCurrent.TabIndex = 20;
			this.btnExportCurrent.Text = "Export Current Log";
			this.btnExportCurrent.UseVisualStyleBackColor = true;
			this.btnExportCurrent.Click += new System.EventHandler(this.button1_Click);
			// 
			// Admin
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1102, 426);
			this.Controls.Add(this.btnExportCurrent);
			this.Controls.Add(this.btnExportAll);
			this.Controls.Add(this.btnExportDay);
			this.Controls.Add(this.txtDateSearch);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtWorkerSearch);
			this.Controls.Add(this.lstWorker);
			this.Controls.Add(this.lstDate);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.txtWorklog);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Admin";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Admin";
			this.Load += new System.EventHandler(this.Admin_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox txtWorklog;
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.ListBox lstDate;
		private System.Windows.Forms.ListBox lstWorker;
		private System.Windows.Forms.TextBox txtWorkerSearch;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox txtDateSearch;
		private System.Windows.Forms.Button btnExportDay;
		private System.Windows.Forms.Button btnExportAll;
		private System.Windows.Forms.Button btnExportCurrent;
	}
}