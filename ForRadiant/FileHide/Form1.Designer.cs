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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ExtensionsTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.FoldersTextBox = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // FilesTextBox
            // 
            this.FilesTextBox.AllowDrop = true;
            this.FilesTextBox.Location = new System.Drawing.Point(6, 19);
            this.FilesTextBox.Multiline = true;
            this.FilesTextBox.Name = "FilesTextBox";
            this.FilesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.FilesTextBox.Size = new System.Drawing.Size(625, 375);
            this.FilesTextBox.TabIndex = 1;
            this.FilesTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.FilesTextBox_DragDrop);
            this.FilesTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.FilesTextBox_DragEnter);
            // 
            // HideButton
            // 
            this.HideButton.Location = new System.Drawing.Point(663, 28);
            this.HideButton.Name = "HideButton";
            this.HideButton.Size = new System.Drawing.Size(60, 61);
            this.HideButton.TabIndex = 0;
            this.HideButton.Text = "Hide";
            this.HideButton.UseVisualStyleBackColor = true;
            this.HideButton.Click += new System.EventHandler(this.HideButton_Click);
            // 
            // UnhideButton
            // 
            this.UnhideButton.Location = new System.Drawing.Point(729, 28);
            this.UnhideButton.Name = "UnhideButton";
            this.UnhideButton.Size = new System.Drawing.Size(60, 61);
            this.UnhideButton.TabIndex = 2;
            this.UnhideButton.Text = "Unhide";
            this.UnhideButton.UseVisualStyleBackColor = true;
            this.UnhideButton.Click += new System.EventHandler(this.UnhideButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Type or drop files to below text box :";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(645, 426);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.FilesTextBox);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(637, 400);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Hide Files";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.ExtensionsTextBox);
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.label2);
            this.tabPage2.Controls.Add(this.FoldersTextBox);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(637, 400);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Hide Files inside Folders";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // ExtensionsTextBox
            // 
            this.ExtensionsTextBox.Location = new System.Drawing.Point(216, 374);
            this.ExtensionsTextBox.Name = "ExtensionsTextBox";
            this.ExtensionsTextBox.Size = new System.Drawing.Size(415, 20);
            this.ExtensionsTextBox.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 377);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(204, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Files extension to hide (comma seperated)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(190, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Type or drop folders to below text box :";
            // 
            // FoldersTextBox
            // 
            this.FoldersTextBox.AllowDrop = true;
            this.FoldersTextBox.Location = new System.Drawing.Point(6, 19);
            this.FoldersTextBox.Multiline = true;
            this.FoldersTextBox.Name = "FoldersTextBox";
            this.FoldersTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.FoldersTextBox.Size = new System.Drawing.Size(625, 349);
            this.FoldersTextBox.TabIndex = 4;
            this.FoldersTextBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.FoldersTextBox_DragDrop);
            this.FoldersTextBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.FoldersTextBox_DragEnter);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.UnhideButton);
            this.Controls.Add(this.HideButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "File Hide";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox FilesTextBox;
        private System.Windows.Forms.Button HideButton;
        private System.Windows.Forms.Button UnhideButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox FoldersTextBox;
        private System.Windows.Forms.TextBox ExtensionsTextBox;
        private System.Windows.Forms.Label label3;
    }
}

