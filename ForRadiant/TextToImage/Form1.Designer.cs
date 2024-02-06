namespace TextToImage
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
            this.pictureBoxIn = new System.Windows.Forms.PictureBox();
            this.PastePictureButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.ResizeRatioTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ConvertToStringButton = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.PasteTextButton = new System.Windows.Forms.Button();
            this.ConvertToImageButton = new System.Windows.Forms.Button();
            this.ClearInputButton = new System.Windows.Forms.Button();
            this.textBoxIn = new System.Windows.Forms.TextBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.pictureBoxOut = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIn)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOut)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBoxIn
            // 
            this.pictureBoxIn.Location = new System.Drawing.Point(6, 35);
            this.pictureBoxIn.Name = "pictureBoxIn";
            this.pictureBoxIn.Size = new System.Drawing.Size(874, 439);
            this.pictureBoxIn.TabIndex = 0;
            this.pictureBoxIn.TabStop = false;
            // 
            // PastePictureButton
            // 
            this.PastePictureButton.Location = new System.Drawing.Point(6, 6);
            this.PastePictureButton.Name = "PastePictureButton";
            this.PastePictureButton.Size = new System.Drawing.Size(75, 23);
            this.PastePictureButton.TabIndex = 1;
            this.PastePictureButton.Text = "Paste";
            this.PastePictureButton.UseVisualStyleBackColor = true;
            this.PastePictureButton.Click += new System.EventHandler(this.PasteButton_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(87, 6);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(75, 23);
            this.ClearButton.TabIndex = 2;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(894, 506);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.ResizeRatioTextBox);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Controls.Add(this.ConvertToStringButton);
            this.tabPage1.Controls.Add(this.PastePictureButton);
            this.tabPage1.Controls.Add(this.ClearButton);
            this.tabPage1.Controls.Add(this.pictureBoxIn);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(886, 480);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Image Input";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(375, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "%";
            // 
            // ResizeRatioTextBox
            // 
            this.ResizeRatioTextBox.Location = new System.Drawing.Point(328, 8);
            this.ResizeRatioTextBox.Name = "ResizeRatioTextBox";
            this.ResizeRatioTextBox.Size = new System.Drawing.Size(41, 20);
            this.ResizeRatioTextBox.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(249, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "With Resize : ";
            // 
            // ConvertToStringButton
            // 
            this.ConvertToStringButton.Location = new System.Drawing.Point(168, 6);
            this.ConvertToStringButton.Name = "ConvertToStringButton";
            this.ConvertToStringButton.Size = new System.Drawing.Size(75, 23);
            this.ConvertToStringButton.TabIndex = 3;
            this.ConvertToStringButton.Text = "Convert";
            this.ConvertToStringButton.UseVisualStyleBackColor = true;
            this.ConvertToStringButton.Click += new System.EventHandler(this.ConvertButton_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(886, 480);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Text Output";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.PasteTextButton);
            this.tabPage3.Controls.Add(this.ConvertToImageButton);
            this.tabPage3.Controls.Add(this.ClearInputButton);
            this.tabPage3.Controls.Add(this.textBoxIn);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Size = new System.Drawing.Size(886, 480);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Text Input";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // PasteTextButton
            // 
            this.PasteTextButton.Location = new System.Drawing.Point(6, 11);
            this.PasteTextButton.Name = "PasteTextButton";
            this.PasteTextButton.Size = new System.Drawing.Size(75, 23);
            this.PasteTextButton.TabIndex = 6;
            this.PasteTextButton.Text = "Paste";
            this.PasteTextButton.UseVisualStyleBackColor = true;
            this.PasteTextButton.Click += new System.EventHandler(this.PasteTextButton_Click);
            // 
            // ConvertToImageButton
            // 
            this.ConvertToImageButton.Location = new System.Drawing.Point(168, 11);
            this.ConvertToImageButton.Name = "ConvertToImageButton";
            this.ConvertToImageButton.Size = new System.Drawing.Size(75, 23);
            this.ConvertToImageButton.TabIndex = 5;
            this.ConvertToImageButton.Text = "Convert";
            this.ConvertToImageButton.UseVisualStyleBackColor = true;
            this.ConvertToImageButton.Click += new System.EventHandler(this.ConvertToImageBtn_Click);
            // 
            // ClearInputButton
            // 
            this.ClearInputButton.Location = new System.Drawing.Point(87, 11);
            this.ClearInputButton.Name = "ClearInputButton";
            this.ClearInputButton.Size = new System.Drawing.Size(75, 23);
            this.ClearInputButton.TabIndex = 4;
            this.ClearInputButton.Text = "Clear";
            this.ClearInputButton.UseVisualStyleBackColor = true;
            this.ClearInputButton.Click += new System.EventHandler(this.ClearInputButton_Click);
            // 
            // textBoxIn
            // 
            this.textBoxIn.Location = new System.Drawing.Point(6, 40);
            this.textBoxIn.Multiline = true;
            this.textBoxIn.Name = "textBoxIn";
            this.textBoxIn.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxIn.Size = new System.Drawing.Size(874, 434);
            this.textBoxIn.TabIndex = 1;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.pictureBoxOut);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(886, 480);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Image Output";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // pictureBoxOut
            // 
            this.pictureBoxOut.Location = new System.Drawing.Point(3, 3);
            this.pictureBoxOut.Name = "pictureBoxOut";
            this.pictureBoxOut.Size = new System.Drawing.Size(880, 474);
            this.pictureBoxOut.TabIndex = 1;
            this.pictureBoxOut.TabStop = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(918, 530);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "TextToImage";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxIn)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            this.tabPage4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOut)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBoxIn;
        private System.Windows.Forms.Button PastePictureButton;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TextBox textBoxIn;
        private System.Windows.Forms.PictureBox pictureBoxOut;
        private System.Windows.Forms.Button ConvertToStringButton;
        private System.Windows.Forms.Button ConvertToImageButton;
        private System.Windows.Forms.Button ClearInputButton;
        private System.Windows.Forms.Button PasteTextButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ResizeRatioTextBox;
    }
}

