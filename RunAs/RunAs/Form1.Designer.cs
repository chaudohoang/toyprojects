namespace RunAs
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.txtPass = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.txtDomain = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radioButtonSameLocation = new System.Windows.Forms.RadioButton();
            this.txtOtherLocation = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.radioButtonOtherLocation = new System.Windows.Forms.RadioButton();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBoxPCFilter = new System.Windows.Forms.CheckBox();
            this.btnPCFilter = new System.Windows.Forms.Button();
            this.checkBoxHash = new System.Windows.Forms.CheckBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.txtHash = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnHelp = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnExport);
            this.groupBox1.Controls.Add(this.btnImport);
            this.groupBox1.Controls.Add(this.txtPass);
            this.groupBox1.Controls.Add(this.txtUser);
            this.groupBox1.Controls.Add(this.txtDomain);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(10, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(321, 104);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Credentials :";
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(229, 57);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(81, 35);
            this.btnExport.TabIndex = 5;
            this.btnExport.Text = "Export...";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(229, 19);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(81, 35);
            this.btnImport.TabIndex = 4;
            this.btnImport.Text = "Import...";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // txtPass
            // 
            this.txtPass.Location = new System.Drawing.Point(73, 72);
            this.txtPass.Name = "txtPass";
            this.txtPass.PasswordChar = '*';
            this.txtPass.Size = new System.Drawing.Size(141, 20);
            this.txtPass.TabIndex = 3;
            // 
            // txtUser
            // 
            this.txtUser.Location = new System.Drawing.Point(73, 45);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(141, 20);
            this.txtUser.TabIndex = 2;
            // 
            // txtDomain
            // 
            this.txtDomain.Location = new System.Drawing.Point(73, 19);
            this.txtDomain.Name = "txtDomain";
            this.txtDomain.Size = new System.Drawing.Size(141, 20);
            this.txtDomain.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Password :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Username :";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Domain :";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radioButtonSameLocation);
            this.groupBox2.Controls.Add(this.txtOtherLocation);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.radioButtonOtherLocation);
            this.groupBox2.Controls.Add(this.btnGenerate);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.checkBoxPCFilter);
            this.groupBox2.Controls.Add(this.btnPCFilter);
            this.groupBox2.Controls.Add(this.checkBoxHash);
            this.groupBox2.Controls.Add(this.btnRun);
            this.groupBox2.Controls.Add(this.txtHash);
            this.groupBox2.Controls.Add(this.btnBrowse);
            this.groupBox2.Controls.Add(this.txtFilePath);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Location = new System.Drawing.Point(10, 114);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(321, 230);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Application :";
            // 
            // radioButtonSameLocation
            // 
            this.radioButtonSameLocation.AutoSize = true;
            this.radioButtonSameLocation.Checked = true;
            this.radioButtonSameLocation.Location = new System.Drawing.Point(11, 177);
            this.radioButtonSameLocation.Name = "radioButtonSameLocation";
            this.radioButtonSameLocation.Size = new System.Drawing.Size(163, 17);
            this.radioButtonSameLocation.TabIndex = 19;
            this.radioButtonSameLocation.TabStop = true;
            this.radioButtonSameLocation.Text = "Same folder with application :";
            this.radioButtonSameLocation.UseVisualStyleBackColor = true;
            this.radioButtonSameLocation.CheckedChanged += new System.EventHandler(this.radioButtonSameLocation_CheckedChanged);
            // 
            // txtOtherLocation
            // 
            this.txtOtherLocation.BackColor = System.Drawing.SystemColors.Control;
            this.txtOtherLocation.Location = new System.Drawing.Point(105, 200);
            this.txtOtherLocation.Name = "txtOtherLocation";
            this.txtOtherLocation.ReadOnly = true;
            this.txtOtherLocation.Size = new System.Drawing.Size(137, 20);
            this.txtOtherLocation.TabIndex = 18;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 156);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(94, 13);
            this.label6.TabIndex = 17;
            this.label6.Text = "Save launcher to :";
            // 
            // radioButtonOtherLocation
            // 
            this.radioButtonOtherLocation.AutoSize = true;
            this.radioButtonOtherLocation.Location = new System.Drawing.Point(11, 200);
            this.radioButtonOtherLocation.Name = "radioButtonOtherLocation";
            this.radioButtonOtherLocation.Size = new System.Drawing.Size(97, 17);
            this.radioButtonOtherLocation.TabIndex = 16;
            this.radioButtonOtherLocation.Text = "Other location :";
            this.radioButtonOtherLocation.UseVisualStyleBackColor = true;
            this.radioButtonOtherLocation.CheckedChanged += new System.EventHandler(this.radioButtonOtherLocation_CheckedChanged);
            // 
            // btnGenerate
            // 
            this.btnGenerate.ForeColor = System.Drawing.Color.DarkBlue;
            this.btnGenerate.Location = new System.Drawing.Point(250, 181);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(60, 43);
            this.btnGenerate.TabIndex = 14;
            this.btnGenerate.Text = "Generate launcher";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 64);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(123, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "Generate launcher with :";
            // 
            // checkBoxPCFilter
            // 
            this.checkBoxPCFilter.AutoSize = true;
            this.checkBoxPCFilter.Location = new System.Drawing.Point(11, 131);
            this.checkBoxPCFilter.Name = "checkBoxPCFilter";
            this.checkBoxPCFilter.Size = new System.Drawing.Size(83, 17);
            this.checkBoxPCFilter.TabIndex = 11;
            this.checkBoxPCFilter.Text = "PC list filter :";
            this.checkBoxPCFilter.UseVisualStyleBackColor = true;
            this.checkBoxPCFilter.CheckedChanged += new System.EventHandler(this.checkBoxPCFilter_CheckedChanged);
            // 
            // btnPCFilter
            // 
            this.btnPCFilter.Enabled = false;
            this.btnPCFilter.Location = new System.Drawing.Point(204, 131);
            this.btnPCFilter.Name = "btnPCFilter";
            this.btnPCFilter.Size = new System.Drawing.Size(106, 23);
            this.btnPCFilter.TabIndex = 12;
            this.btnPCFilter.Text = "Edit PC list";
            this.btnPCFilter.UseVisualStyleBackColor = true;
            this.btnPCFilter.Click += new System.EventHandler(this.btnPCFilter_Click);
            // 
            // checkBoxHash
            // 
            this.checkBoxHash.AutoSize = true;
            this.checkBoxHash.Location = new System.Drawing.Point(11, 83);
            this.checkBoxHash.Name = "checkBoxHash";
            this.checkBoxHash.Size = new System.Drawing.Size(178, 17);
            this.checkBoxHash.TabIndex = 8;
            this.checkBoxHash.Text = "Calculated SHA256 from target :";
            this.checkBoxHash.UseVisualStyleBackColor = true;
            this.checkBoxHash.CheckedChanged += new System.EventHandler(this.checkBoxHash_CheckedChanged);
            // 
            // btnRun
            // 
            this.btnRun.ForeColor = System.Drawing.Color.DarkRed;
            this.btnRun.Location = new System.Drawing.Point(248, 30);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(62, 23);
            this.btnRun.TabIndex = 13;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // txtHash
            // 
            this.txtHash.BackColor = System.Drawing.SystemColors.Control;
            this.txtHash.Location = new System.Drawing.Point(9, 105);
            this.txtHash.Name = "txtHash";
            this.txtHash.ReadOnly = true;
            this.txtHash.Size = new System.Drawing.Size(301, 20);
            this.txtHash.TabIndex = 9;
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(188, 30);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(54, 23);
            this.btnBrowse.TabIndex = 7;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // txtFilePath
            // 
            this.txtFilePath.Location = new System.Drawing.Point(11, 32);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(171, 20);
            this.txtFilePath.TabIndex = 6;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 16);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(44, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Target :";
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(10, 350);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(41, 22);
            this.btnHelp.TabIndex = 15;
            this.btnHelp.Text = "Help";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(340, 381);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "RunAs";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtPass;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.TextBox txtDomain;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.TextBox txtHash;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.CheckBox checkBoxHash;
        private System.Windows.Forms.CheckBox checkBoxPCFilter;
        private System.Windows.Forms.Button btnPCFilter;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton radioButtonOtherLocation;
        private System.Windows.Forms.TextBox txtOtherLocation;
        private System.Windows.Forms.RadioButton radioButtonSameLocation;
    }
}

