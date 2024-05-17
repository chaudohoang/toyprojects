namespace FileHide
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
            this.FilesTextBox = new System.Windows.Forms.TextBox();
            this.HideButton = new System.Windows.Forms.Button();
            this.UnhideButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.FoldersTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ExtensionsTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.RemoveEmptyLinesButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // FilesTextBox
            // 
            this.FilesTextBox.AllowDrop = true;
            this.FilesTextBox.Location = new System.Drawing.Point(12, 38);
            this.FilesTextBox.Multiline = true;
            this.FilesTextBox.Name = "FilesTextBox";
            this.FilesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.FilesTextBox.Size = new System.Drawing.Size(625, 223);
            this.FilesTextBox.TabIndex = 1;
            this.FilesTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.FilesTextBox_DragDrop);
            this.FilesTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilesTextBox_DragEnter);
            // 
            // HideButton
            // 
            this.HideButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideButton.Location = new System.Drawing.Point(643, 38);
            this.HideButton.Name = "HideButton";
            this.HideButton.Size = new System.Drawing.Size(124, 83);
            this.HideButton.TabIndex = 0;
            this.HideButton.Text = "Hide";
            this.HideButton.UseVisualStyleBackColor = true;
            this.HideButton.Click += new System.EventHandler(this.HideButton_Click);
            // 
            // UnhideButton
            // 
            this.UnhideButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.UnhideButton.Location = new System.Drawing.Point(643, 127);
            this.UnhideButton.Name = "UnhideButton";
            this.UnhideButton.Size = new System.Drawing.Size(124, 83);
            this.UnhideButton.TabIndex = 2;
            this.UnhideButton.Text = "Unhide";
            this.UnhideButton.UseVisualStyleBackColor = true;
            this.UnhideButton.Click += new System.EventHandler(this.UnhideButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Type or drop files to below text box :";
            // 
            // FoldersTextBox
            // 
            this.FoldersTextBox.AllowDrop = true;
            this.FoldersTextBox.Location = new System.Drawing.Point(12, 293);
            this.FoldersTextBox.Multiline = true;
            this.FoldersTextBox.Name = "FoldersTextBox";
            this.FoldersTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.FoldersTextBox.Size = new System.Drawing.Size(625, 223);
            this.FoldersTextBox.TabIndex = 4;
            this.FoldersTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.FoldersTextBox_DragDrop);
            this.FoldersTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.FoldersTextBox_DragEnter);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 277);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(190, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Type or drop folders to below text box :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 525);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(204, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Files extension to hide (comma seperated)";
            // 
            // ExtensionsTextBox
            // 
            this.ExtensionsTextBox.Location = new System.Drawing.Point(222, 522);
            this.ExtensionsTextBox.Name = "ExtensionsTextBox";
            this.ExtensionsTextBox.Size = new System.Drawing.Size(415, 20);
            this.ExtensionsTextBox.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Hide Files :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 264);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(127, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Hide Files Inside Folders :";
            // 
            // RemoveEmptyLinesButton
            // 
            this.RemoveEmptyLinesButton.Location = new System.Drawing.Point(643, 216);
            this.RemoveEmptyLinesButton.Name = "RemoveEmptyLinesButton";
            this.RemoveEmptyLinesButton.Size = new System.Drawing.Size(121, 23);
            this.RemoveEmptyLinesButton.TabIndex = 10;
            this.RemoveEmptyLinesButton.Text = "Cleanup empty lines";
            this.RemoveEmptyLinesButton.UseVisualStyleBackColor = true;
            this.RemoveEmptyLinesButton.Click += new System.EventHandler(this.RemoveEmptyLinesButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(776, 552);
            this.Controls.Add(this.RemoveEmptyLinesButton);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ExtensionsTextBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.FoldersTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.FilesTextBox);
            this.Controls.Add(this.UnhideButton);
            this.Controls.Add(this.HideButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "File Hide";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox FilesTextBox;
        private System.Windows.Forms.Button HideButton;
        private System.Windows.Forms.Button UnhideButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox FoldersTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox ExtensionsTextBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button RemoveEmptyLinesButton;
    }
}

