namespace FileLock
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
            this.FilesTextBox = new System.Windows.Forms.TextBox();
            this.LockButton = new System.Windows.Forms.Button();
            this.UnlockButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // FilesTextBox
            // 
            this.FilesTextBox.AllowDrop = true;
            this.FilesTextBox.Location = new System.Drawing.Point(12, 28);
            this.FilesTextBox.Multiline = true;
            this.FilesTextBox.Name = "FilesTextBox";
            this.FilesTextBox.Size = new System.Drawing.Size(645, 410);
            this.FilesTextBox.TabIndex = 1;
            this.FilesTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.FilesTextBox_DragDrop);
            this.FilesTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilesTextBox_DragEnter);
            // 
            // LockButton
            // 
            this.LockButton.Location = new System.Drawing.Point(663, 28);
            this.LockButton.Name = "LockButton";
            this.LockButton.Size = new System.Drawing.Size(60, 61);
            this.LockButton.TabIndex = 0;
            this.LockButton.Text = "Lock";
            this.LockButton.UseVisualStyleBackColor = true;
            this.LockButton.Click += new System.EventHandler(this.LockButton_Click);
            // 
            // UnlockButton
            // 
            this.UnlockButton.Location = new System.Drawing.Point(729, 28);
            this.UnlockButton.Name = "UnlockButton";
            this.UnlockButton.Size = new System.Drawing.Size(60, 61);
            this.UnlockButton.TabIndex = 2;
            this.UnlockButton.Text = "Unlock";
            this.UnlockButton.UseVisualStyleBackColor = true;
            this.UnlockButton.Click += new System.EventHandler(this.UnlockButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Type or drop files to below text box :";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.UnlockButton);
            this.Controls.Add(this.FilesTextBox);
            this.Controls.Add(this.LockButton);
            this.Name = "Form1";
            this.Text = "File Lock";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox FilesTextBox;
        private System.Windows.Forms.Button LockButton;
        private System.Windows.Forms.Button UnlockButton;
        private System.Windows.Forms.Label label1;
    }
}

