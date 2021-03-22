
namespace IPListBuilder
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabIPScanner = new System.Windows.Forms.TabPage();
            this.btnGenerate1 = new System.Windows.Forms.Button();
            this.cbIPList1 = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabCopyWhiz = new System.Windows.Forms.TabPage();
            this.cbExtendedPath = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnGenerate2 = new System.Windows.Forms.Button();
            this.cbIPList2 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabRealVNC = new System.Windows.Forms.TabPage();
            this.btnGenerate3 = new System.Windows.Forms.Button();
            this.cbIPList3 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabIPonly = new System.Windows.Forms.TabPage();
            this.btnGenerate4 = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabIPScanner.SuspendLayout();
            this.tabCopyWhiz.SuspendLayout();
            this.tabRealVNC.SuspendLayout();
            this.tabIPonly.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabIPScanner);
            this.tabControl1.Controls.Add(this.tabCopyWhiz);
            this.tabControl1.Controls.Add(this.tabRealVNC);
            this.tabControl1.Controls.Add(this.tabIPonly);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(445, 126);
            this.tabControl1.TabIndex = 0;
            // 
            // tabIPScanner
            // 
            this.tabIPScanner.Controls.Add(this.btnGenerate1);
            this.tabIPScanner.Controls.Add(this.cbIPList1);
            this.tabIPScanner.Controls.Add(this.label1);
            this.tabIPScanner.Location = new System.Drawing.Point(4, 22);
            this.tabIPScanner.Name = "tabIPScanner";
            this.tabIPScanner.Padding = new System.Windows.Forms.Padding(3);
            this.tabIPScanner.Size = new System.Drawing.Size(437, 100);
            this.tabIPScanner.TabIndex = 0;
            this.tabIPScanner.Text = "Advance IP Scanner";
            this.tabIPScanner.UseVisualStyleBackColor = true;
            // 
            // btnGenerate1
            // 
            this.btnGenerate1.Location = new System.Drawing.Point(9, 33);
            this.btnGenerate1.Name = "btnGenerate1";
            this.btnGenerate1.Size = new System.Drawing.Size(417, 61);
            this.btnGenerate1.TabIndex = 2;
            this.btnGenerate1.Text = "Generate";
            this.btnGenerate1.UseVisualStyleBackColor = true;
            this.btnGenerate1.Click += new System.EventHandler(this.btnGenerate1_Click);
            // 
            // cbIPList1
            // 
            this.cbIPList1.FormattingEnabled = true;
            this.cbIPList1.Location = new System.Drawing.Point(48, 6);
            this.cbIPList1.Name = "cbIPList1";
            this.cbIPList1.Size = new System.Drawing.Size(378, 21);
            this.cbIPList1.TabIndex = 1;
            this.cbIPList1.DropDown += new System.EventHandler(this.cbIPList1_DropDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP List";
            // 
            // tabCopyWhiz
            // 
            this.tabCopyWhiz.Controls.Add(this.cbExtendedPath);
            this.tabCopyWhiz.Controls.Add(this.label4);
            this.tabCopyWhiz.Controls.Add(this.btnGenerate2);
            this.tabCopyWhiz.Controls.Add(this.cbIPList2);
            this.tabCopyWhiz.Controls.Add(this.label2);
            this.tabCopyWhiz.Location = new System.Drawing.Point(4, 22);
            this.tabCopyWhiz.Name = "tabCopyWhiz";
            this.tabCopyWhiz.Padding = new System.Windows.Forms.Padding(3);
            this.tabCopyWhiz.Size = new System.Drawing.Size(437, 100);
            this.tabCopyWhiz.TabIndex = 1;
            this.tabCopyWhiz.Text = "CopyWhiz";
            this.tabCopyWhiz.UseVisualStyleBackColor = true;
            // 
            // cbExtendedPath
            // 
            this.cbExtendedPath.FormattingEnabled = true;
            this.cbExtendedPath.Location = new System.Drawing.Point(89, 33);
            this.cbExtendedPath.Name = "cbExtendedPath";
            this.cbExtendedPath.Size = new System.Drawing.Size(337, 21);
            this.cbExtendedPath.TabIndex = 6;
            this.cbExtendedPath.Text = "\\Program\\RVS";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 36);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "Extended Path";
            // 
            // btnGenerate2
            // 
            this.btnGenerate2.Location = new System.Drawing.Point(9, 60);
            this.btnGenerate2.Name = "btnGenerate2";
            this.btnGenerate2.Size = new System.Drawing.Size(417, 31);
            this.btnGenerate2.TabIndex = 4;
            this.btnGenerate2.Text = "Generate";
            this.btnGenerate2.UseVisualStyleBackColor = true;
            this.btnGenerate2.Click += new System.EventHandler(this.btnGenerate2_Click);
            // 
            // cbIPList2
            // 
            this.cbIPList2.FormattingEnabled = true;
            this.cbIPList2.Location = new System.Drawing.Point(48, 6);
            this.cbIPList2.Name = "cbIPList2";
            this.cbIPList2.Size = new System.Drawing.Size(378, 21);
            this.cbIPList2.TabIndex = 3;
            this.cbIPList2.DropDown += new System.EventHandler(this.cbIPList2_DropDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "IP List";
            // 
            // tabRealVNC
            // 
            this.tabRealVNC.Controls.Add(this.btnGenerate3);
            this.tabRealVNC.Controls.Add(this.cbIPList3);
            this.tabRealVNC.Controls.Add(this.label3);
            this.tabRealVNC.Location = new System.Drawing.Point(4, 22);
            this.tabRealVNC.Name = "tabRealVNC";
            this.tabRealVNC.Size = new System.Drawing.Size(437, 100);
            this.tabRealVNC.TabIndex = 2;
            this.tabRealVNC.Text = "Real VNC";
            this.tabRealVNC.UseVisualStyleBackColor = true;
            // 
            // btnGenerate3
            // 
            this.btnGenerate3.Location = new System.Drawing.Point(9, 33);
            this.btnGenerate3.Name = "btnGenerate3";
            this.btnGenerate3.Size = new System.Drawing.Size(417, 61);
            this.btnGenerate3.TabIndex = 6;
            this.btnGenerate3.Text = "Generate";
            this.btnGenerate3.UseVisualStyleBackColor = true;
            this.btnGenerate3.Click += new System.EventHandler(this.btnGenerate3_Click);
            // 
            // cbIPList3
            // 
            this.cbIPList3.FormattingEnabled = true;
            this.cbIPList3.Location = new System.Drawing.Point(48, 6);
            this.cbIPList3.Name = "cbIPList3";
            this.cbIPList3.Size = new System.Drawing.Size(378, 21);
            this.cbIPList3.TabIndex = 5;
            this.cbIPList3.DropDown += new System.EventHandler(this.cbIPList3_DropDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "IP List";
            // 
            // tabIPonly
            // 
            this.tabIPonly.Controls.Add(this.btnGenerate4);
            this.tabIPonly.Location = new System.Drawing.Point(4, 22);
            this.tabIPonly.Name = "tabIPonly";
            this.tabIPonly.Padding = new System.Windows.Forms.Padding(3);
            this.tabIPonly.Size = new System.Drawing.Size(437, 100);
            this.tabIPonly.TabIndex = 3;
            this.tabIPonly.Text = "IP only";
            this.tabIPonly.UseVisualStyleBackColor = true;
            // 
            // btnGenerate4
            // 
            this.btnGenerate4.Location = new System.Drawing.Point(6, 6);
            this.btnGenerate4.Name = "btnGenerate4";
            this.btnGenerate4.Size = new System.Drawing.Size(425, 88);
            this.btnGenerate4.TabIndex = 0;
            this.btnGenerate4.Text = "Generate";
            this.btnGenerate4.UseVisualStyleBackColor = true;
            this.btnGenerate4.Click += new System.EventHandler(this.btnGenerate4_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 146);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "IP List Builder";
            this.tabControl1.ResumeLayout(false);
            this.tabIPScanner.ResumeLayout(false);
            this.tabIPScanner.PerformLayout();
            this.tabCopyWhiz.ResumeLayout(false);
            this.tabCopyWhiz.PerformLayout();
            this.tabRealVNC.ResumeLayout(false);
            this.tabRealVNC.PerformLayout();
            this.tabIPonly.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabIPScanner;
        private System.Windows.Forms.TabPage tabCopyWhiz;
        private System.Windows.Forms.TabPage tabRealVNC;
        private System.Windows.Forms.ComboBox cbIPList1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbIPList2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cbIPList3;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnGenerate1;
        private System.Windows.Forms.Button btnGenerate2;
        private System.Windows.Forms.Button btnGenerate3;
        private System.Windows.Forms.ComboBox cbExtendedPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabPage tabIPonly;
        private System.Windows.Forms.Button btnGenerate4;
    }
}

