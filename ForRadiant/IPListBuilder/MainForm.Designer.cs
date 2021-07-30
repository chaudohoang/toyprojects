
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
            this.tabFreeFileSync = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.cbIPList4 = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.cbSource1 = new System.Windows.Forms.ComboBox();
            this.cbExtendedDestination = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cbIdleTime1 = new System.Windows.Forms.ComboBox();
            this.btnGenerate5 = new System.Windows.Forms.Button();
            this.tabIPonly = new System.Windows.Forms.TabPage();
            this.btnGenerate4 = new System.Windows.Forms.Button();
            this.tabCopyWhiz = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.cbIPList2 = new System.Windows.Forms.ComboBox();
            this.btnGenerate2 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.cbExtendedPath = new System.Windows.Forms.ComboBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabFreeFileSync.SuspendLayout();
            this.tabIPonly.SuspendLayout();
            this.tabCopyWhiz.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabFreeFileSync
            // 
            this.tabFreeFileSync.Controls.Add(this.btnGenerate5);
            this.tabFreeFileSync.Controls.Add(this.cbIdleTime1);
            this.tabFreeFileSync.Controls.Add(this.label8);
            this.tabFreeFileSync.Controls.Add(this.cbExtendedDestination);
            this.tabFreeFileSync.Controls.Add(this.cbSource1);
            this.tabFreeFileSync.Controls.Add(this.label7);
            this.tabFreeFileSync.Controls.Add(this.label6);
            this.tabFreeFileSync.Controls.Add(this.cbIPList4);
            this.tabFreeFileSync.Controls.Add(this.label5);
            this.tabFreeFileSync.Location = new System.Drawing.Point(4, 22);
            this.tabFreeFileSync.Name = "tabFreeFileSync";
            this.tabFreeFileSync.Size = new System.Drawing.Size(437, 118);
            this.tabFreeFileSync.TabIndex = 4;
            this.tabFreeFileSync.Text = "FreeFileSync";
            this.tabFreeFileSync.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(36, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "IP List";
            // 
            // cbIPList4
            // 
            this.cbIPList4.FormattingEnabled = true;
            this.cbIPList4.Location = new System.Drawing.Point(48, 6);
            this.cbIPList4.Name = "cbIPList4";
            this.cbIPList4.Size = new System.Drawing.Size(378, 21);
            this.cbIPList4.TabIndex = 7;
            this.cbIPList4.DropDown += new System.EventHandler(this.cbIPList4_DropDown);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(41, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Source";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 63);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(108, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Extended Destination";
            // 
            // cbSource1
            // 
            this.cbSource1.FormattingEnabled = true;
            this.cbSource1.Items.AddRange(new object[] {
            "",
            "C:\\Users\\Administrator\\RVS Sync\\RVS\\Tools",
            "D:\\RVS Sync\\RVS\\Tools",
            "\\\\127.0.0.1\\Program\\RVS\\Tools",
            "C:\\Users\\Administrator\\RVS Sync\\RVS\\TrueTest Installers",
            "D:\\RVS Sync\\RVS\\TrueTest Installers",
            "\\\\127.0.0.1\\Program\\RVS\\TrueTest Installers"});
            this.cbSource1.Location = new System.Drawing.Point(53, 33);
            this.cbSource1.Name = "cbSource1";
            this.cbSource1.Size = new System.Drawing.Size(258, 21);
            this.cbSource1.TabIndex = 9;
            this.cbSource1.Text = "C:\\Users\\Administrator\\RVS Sync\\RVS\\Tools";
            // 
            // cbExtendedDestination
            // 
            this.cbExtendedDestination.FormattingEnabled = true;
            this.cbExtendedDestination.Items.AddRange(new object[] {
            "",
            "\\Program\\RVS\\Tools",
            "\\Program\\RVS\\TrueTest Installers"});
            this.cbExtendedDestination.Location = new System.Drawing.Point(120, 60);
            this.cbExtendedDestination.Name = "cbExtendedDestination";
            this.cbExtendedDestination.Size = new System.Drawing.Size(191, 21);
            this.cbExtendedDestination.TabIndex = 9;
            this.cbExtendedDestination.Text = "\\Program\\RVS\\Tools";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 90);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 13);
            this.label8.TabIndex = 12;
            this.label8.Text = "Idle Time";
            // 
            // cbIdleTime1
            // 
            this.cbIdleTime1.FormattingEnabled = true;
            this.cbIdleTime1.Items.AddRange(new object[] {
            "",
            "0",
            "10"});
            this.cbIdleTime1.Location = new System.Drawing.Point(62, 87);
            this.cbIdleTime1.Name = "cbIdleTime1";
            this.cbIdleTime1.Size = new System.Drawing.Size(249, 21);
            this.cbIdleTime1.TabIndex = 13;
            this.cbIdleTime1.Text = "0";
            // 
            // btnGenerate5
            // 
            this.btnGenerate5.Location = new System.Drawing.Point(320, 33);
            this.btnGenerate5.Name = "btnGenerate5";
            this.btnGenerate5.Size = new System.Drawing.Size(106, 79);
            this.btnGenerate5.TabIndex = 14;
            this.btnGenerate5.Text = "Generate";
            this.btnGenerate5.UseVisualStyleBackColor = true;
            this.btnGenerate5.Click += new System.EventHandler(this.btnGenerate5_Click);
            // 
            // tabIPonly
            // 
            this.tabIPonly.Controls.Add(this.btnGenerate4);
            this.tabIPonly.Location = new System.Drawing.Point(4, 22);
            this.tabIPonly.Name = "tabIPonly";
            this.tabIPonly.Padding = new System.Windows.Forms.Padding(3);
            this.tabIPonly.Size = new System.Drawing.Size(437, 118);
            this.tabIPonly.TabIndex = 3;
            this.tabIPonly.Text = "IP only";
            this.tabIPonly.UseVisualStyleBackColor = true;
            // 
            // btnGenerate4
            // 
            this.btnGenerate4.Location = new System.Drawing.Point(6, 6);
            this.btnGenerate4.Name = "btnGenerate4";
            this.btnGenerate4.Size = new System.Drawing.Size(425, 106);
            this.btnGenerate4.TabIndex = 0;
            this.btnGenerate4.Text = "Generate";
            this.btnGenerate4.UseVisualStyleBackColor = true;
            this.btnGenerate4.Click += new System.EventHandler(this.btnGenerate4_Click);
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
            this.tabCopyWhiz.Size = new System.Drawing.Size(437, 118);
            this.tabCopyWhiz.TabIndex = 1;
            this.tabCopyWhiz.Text = "CopyWhiz";
            this.tabCopyWhiz.UseVisualStyleBackColor = true;
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
            // cbIPList2
            // 
            this.cbIPList2.FormattingEnabled = true;
            this.cbIPList2.Location = new System.Drawing.Point(48, 6);
            this.cbIPList2.Name = "cbIPList2";
            this.cbIPList2.Size = new System.Drawing.Size(378, 21);
            this.cbIPList2.TabIndex = 3;
            this.cbIPList2.DropDown += new System.EventHandler(this.cbIPList2_DropDown);
            // 
            // btnGenerate2
            // 
            this.btnGenerate2.Location = new System.Drawing.Point(9, 60);
            this.btnGenerate2.Name = "btnGenerate2";
            this.btnGenerate2.Size = new System.Drawing.Size(417, 52);
            this.btnGenerate2.TabIndex = 4;
            this.btnGenerate2.Text = "Generate";
            this.btnGenerate2.UseVisualStyleBackColor = true;
            this.btnGenerate2.Click += new System.EventHandler(this.btnGenerate2_Click);
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
            // cbExtendedPath
            // 
            this.cbExtendedPath.FormattingEnabled = true;
            this.cbExtendedPath.Location = new System.Drawing.Point(89, 33);
            this.cbExtendedPath.Name = "cbExtendedPath";
            this.cbExtendedPath.Size = new System.Drawing.Size(337, 21);
            this.cbExtendedPath.TabIndex = 6;
            this.cbExtendedPath.Text = "\\Program\\RVS\\Tools";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabCopyWhiz);
            this.tabControl1.Controls.Add(this.tabIPonly);
            this.tabControl1.Controls.Add(this.tabFreeFileSync);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(445, 144);
            this.tabControl1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(551, 164);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "IP List Builder";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabFreeFileSync.ResumeLayout(false);
            this.tabFreeFileSync.PerformLayout();
            this.tabIPonly.ResumeLayout(false);
            this.tabCopyWhiz.ResumeLayout(false);
            this.tabCopyWhiz.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabFreeFileSync;
        private System.Windows.Forms.Button btnGenerate5;
        private System.Windows.Forms.ComboBox cbIdleTime1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cbExtendedDestination;
        private System.Windows.Forms.ComboBox cbSource1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cbIPList4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabPage tabIPonly;
        private System.Windows.Forms.Button btnGenerate4;
        private System.Windows.Forms.TabPage tabCopyWhiz;
        private System.Windows.Forms.ComboBox cbExtendedPath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnGenerate2;
        private System.Windows.Forms.ComboBox cbIPList2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl tabControl1;
    }
}

