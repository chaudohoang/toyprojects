﻿namespace TTLogViewer
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
            this.components = new System.ComponentModel.Container();
            this.rtbContent = new System.Windows.Forms.RichTextBox();
            this.timerUpdate = new System.Windows.Forms.Timer(this.components);
            this.btnToggleAutoScroll = new System.Windows.Forms.Button();
            this.btnResizeAndSnap = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rtbContent
            // 
            this.rtbContent.Location = new System.Drawing.Point(12, 12);
            this.rtbContent.Name = "rtbContent";
            this.rtbContent.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbContent.Size = new System.Drawing.Size(783, 351);
            this.rtbContent.TabIndex = 0;
            this.rtbContent.Text = "";
            // 
            // btnToggleAutoScroll
            // 
            this.btnToggleAutoScroll.Location = new System.Drawing.Point(657, 330);
            this.btnToggleAutoScroll.Name = "btnToggleAutoScroll";
            this.btnToggleAutoScroll.Size = new System.Drawing.Size(112, 23);
            this.btnToggleAutoScroll.TabIndex = 1;
            this.btnToggleAutoScroll.Text = "Auto Scroll";
            this.btnToggleAutoScroll.UseVisualStyleBackColor = true;
            this.btnToggleAutoScroll.Click += new System.EventHandler(this.btnToggleAutoScroll_Click);
            // 
            // btnResizeAndSnap
            // 
            this.btnResizeAndSnap.Location = new System.Drawing.Point(28, 25);
            this.btnResizeAndSnap.Name = "btnResizeAndSnap";
            this.btnResizeAndSnap.Size = new System.Drawing.Size(62, 23);
            this.btnResizeAndSnap.TabIndex = 2;
            this.btnResizeAndSnap.Text = "Snap";
            this.btnResizeAndSnap.UseVisualStyleBackColor = true;
            this.btnResizeAndSnap.Click += new System.EventHandler(this.btnResizeAndSnap_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(96, 25);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(62, 23);
            this.btnClose.TabIndex = 3;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(807, 375);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnResizeAndSnap);
            this.Controls.Add(this.btnToggleAutoScroll);
            this.Controls.Add(this.rtbContent);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox rtbContent;
        private System.Windows.Forms.Timer timerUpdate;
        private System.Windows.Forms.Button btnToggleAutoScroll;
        private System.Windows.Forms.Button btnResizeAndSnap;
        private System.Windows.Forms.Button btnClose;
    }
}

