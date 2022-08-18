
namespace UltraViewerBook
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
			this.btnRemoteControl = new System.Windows.Forms.Button();
			this.dataGridView1 = new System.Windows.Forms.DataGridView();
			this.btnSave = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
			this.SuspendLayout();
			// 
			// btnRemoteControl
			// 
			this.btnRemoteControl.Location = new System.Drawing.Point(199, 389);
			this.btnRemoteControl.Name = "btnRemoteControl";
			this.btnRemoteControl.Size = new System.Drawing.Size(180, 23);
			this.btnRemoteControl.TabIndex = 10;
			this.btnRemoteControl.Text = "Remote Control";
			this.btnRemoteControl.UseVisualStyleBackColor = true;
			this.btnRemoteControl.Click += new System.EventHandler(this.btnRemoteControl_Click);
			// 
			// dataGridView1
			// 
			this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.dataGridView1.Location = new System.Drawing.Point(12, 12);
			this.dataGridView1.Name = "dataGridView1";
			this.dataGridView1.Size = new System.Drawing.Size(367, 371);
			this.dataGridView1.TabIndex = 9;
			this.dataGridView1.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dataGridView1_CellFormatting);
			// 
			// btnSave
			// 
			this.btnSave.Location = new System.Drawing.Point(13, 389);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(180, 23);
			this.btnSave.TabIndex = 8;
			this.btnSave.Text = "Save Contacts";
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(392, 423);
			this.Controls.Add(this.btnRemoteControl);
			this.Controls.Add(this.dataGridView1);
			this.Controls.Add(this.btnSave);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "Form1";
			this.Text = "UltraViewerBook";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
		private System.Windows.Forms.Button btnRemoteControl;
		private System.Windows.Forms.DataGridView dataGridView1;
		private System.Windows.Forms.Button btnSave;
	}
}

