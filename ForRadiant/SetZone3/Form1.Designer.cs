namespace SetZone3
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
            this.btnSelectFiles = new System.Windows.Forms.Button();
            this.btnBlockFiles = new System.Windows.Forms.Button();
            this.btnUnblockFiles = new System.Windows.Forms.Button();
            this.lstFiles = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // btnSelectFiles
            // 
            this.btnSelectFiles.Location = new System.Drawing.Point(12, 12);
            this.btnSelectFiles.Name = "btnSelectFiles";
            this.btnSelectFiles.Size = new System.Drawing.Size(75, 23);
            this.btnSelectFiles.TabIndex = 0;
            this.btnSelectFiles.Text = "Select Files";
            this.btnSelectFiles.UseVisualStyleBackColor = true;
            this.btnSelectFiles.Click += new System.EventHandler(this.btnSelectFiles_Click);
            // 
            // btnBlockFiles
            // 
            this.btnBlockFiles.Location = new System.Drawing.Point(93, 12);
            this.btnBlockFiles.Name = "btnBlockFiles";
            this.btnBlockFiles.Size = new System.Drawing.Size(75, 23);
            this.btnBlockFiles.TabIndex = 1;
            this.btnBlockFiles.Text = "Set";
            this.btnBlockFiles.UseVisualStyleBackColor = true;
            this.btnBlockFiles.Click += new System.EventHandler(this.btnBlockFiles_Click);
            // 
            // btnUnblockFiles
            // 
            this.btnUnblockFiles.Location = new System.Drawing.Point(174, 12);
            this.btnUnblockFiles.Name = "btnUnblockFiles";
            this.btnUnblockFiles.Size = new System.Drawing.Size(75, 23);
            this.btnUnblockFiles.TabIndex = 2;
            this.btnUnblockFiles.Text = "Unset";
            this.btnUnblockFiles.UseVisualStyleBackColor = true;
            this.btnUnblockFiles.Click += new System.EventHandler(this.btnUnblockFiles_Click);
            // 
            // lstFiles
            // 
            this.lstFiles.FormattingEnabled = true;
            this.lstFiles.Location = new System.Drawing.Point(12, 41);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(670, 355);
            this.lstFiles.TabIndex = 3;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(693, 408);
            this.Controls.Add(this.lstFiles);
            this.Controls.Add(this.btnUnblockFiles);
            this.Controls.Add(this.btnBlockFiles);
            this.Controls.Add(this.btnSelectFiles);
            this.Name = "Form1";
            this.Text = "SetZone3";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSelectFiles;
        private System.Windows.Forms.Button btnBlockFiles;
        private System.Windows.Forms.Button btnUnblockFiles;
        private System.Windows.Forms.ListBox lstFiles;
    }
}

