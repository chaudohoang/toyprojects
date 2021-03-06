﻿namespace Puzzle
{
    partial class StartForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.imgBrowseBtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.restartBtn = new System.Windows.Forms.Button();
            this.startBtn = new System.Windows.Forms.Button();
            this.printPreviewControl1 = new System.Windows.Forms.PrintPreviewControl();
            this.squaresNrComboBox = new System.Windows.Forms.ComboBox();
            this.gameBoxPuzzle = new System.Windows.Forms.GroupBox();
            this.hintBtn = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(20, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 20);
            this.label1.TabIndex = 1;
            this.label1.Text = "Squares";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(24, 62);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 20);
            this.label2.TabIndex = 2;
            this.label2.Text = "Image";
            // 
            // imgBrowseBtn
            // 
            this.imgBrowseBtn.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.imgBrowseBtn.Location = new System.Drawing.Point(115, 56);
            this.imgBrowseBtn.Margin = new System.Windows.Forms.Padding(2);
            this.imgBrowseBtn.Name = "imgBrowseBtn";
            this.imgBrowseBtn.Size = new System.Drawing.Size(112, 34);
            this.imgBrowseBtn.TabIndex = 3;
            this.imgBrowseBtn.Text = "Choose Image";
            this.imgBrowseBtn.UseVisualStyleBackColor = true;
            this.imgBrowseBtn.Click += new System.EventHandler(this.imgBrowseBtn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.hintBtn);
            this.groupBox1.Controls.Add(this.restartBtn);
            this.groupBox1.Controls.Add(this.startBtn);
            this.groupBox1.Controls.Add(this.printPreviewControl1);
            this.groupBox1.Controls.Add(this.squaresNrComboBox);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.imgBrowseBtn);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(9, 10);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(569, 105);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Setup";
            // 
            // restartBtn
            // 
            this.restartBtn.Font = new System.Drawing.Font("Consolas", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.restartBtn.Location = new System.Drawing.Point(255, 57);
            this.restartBtn.Margin = new System.Windows.Forms.Padding(2);
            this.restartBtn.Name = "restartBtn";
            this.restartBtn.Size = new System.Drawing.Size(146, 42);
            this.restartBtn.TabIndex = 8;
            this.restartBtn.TabStop = false;
            this.restartBtn.Text = "Restart";
            this.restartBtn.UseVisualStyleBackColor = true;
            // 
            // startBtn
            // 
            this.startBtn.Font = new System.Drawing.Font("Consolas", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.startBtn.Location = new System.Drawing.Point(255, 13);
            this.startBtn.Margin = new System.Windows.Forms.Padding(2);
            this.startBtn.Name = "startBtn";
            this.startBtn.Size = new System.Drawing.Size(146, 42);
            this.startBtn.TabIndex = 7;
            this.startBtn.TabStop = false;
            this.startBtn.Text = "Start";
            this.startBtn.UseVisualStyleBackColor = true;
            this.startBtn.Click += new System.EventHandler(this.startBtn_Click);
            // 
            // printPreviewControl1
            // 
            this.printPreviewControl1.Location = new System.Drawing.Point(347, 144);
            this.printPreviewControl1.Margin = new System.Windows.Forms.Padding(2);
            this.printPreviewControl1.Name = "printPreviewControl1";
            this.printPreviewControl1.Size = new System.Drawing.Size(75, 81);
            this.printPreviewControl1.TabIndex = 6;
            // 
            // squaresNrComboBox
            // 
            this.squaresNrComboBox.FormattingEnabled = true;
            this.squaresNrComboBox.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.squaresNrComboBox.Location = new System.Drawing.Point(115, 22);
            this.squaresNrComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.squaresNrComboBox.Name = "squaresNrComboBox";
            this.squaresNrComboBox.Size = new System.Drawing.Size(113, 25);
            this.squaresNrComboBox.TabIndex = 4;
            // 
            // gameBoxPuzzle
            // 
            this.gameBoxPuzzle.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gameBoxPuzzle.Location = new System.Drawing.Point(9, 119);
            this.gameBoxPuzzle.Margin = new System.Windows.Forms.Padding(2);
            this.gameBoxPuzzle.Name = "gameBoxPuzzle";
            this.gameBoxPuzzle.Padding = new System.Windows.Forms.Padding(2);
            this.gameBoxPuzzle.Size = new System.Drawing.Size(1440, 810);
            this.gameBoxPuzzle.TabIndex = 5;
            this.gameBoxPuzzle.TabStop = false;
            this.gameBoxPuzzle.Text = "Game";
            // 
            // hintBtn
            // 
            this.hintBtn.Font = new System.Drawing.Font("Consolas", 13.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hintBtn.Location = new System.Drawing.Point(405, 13);
            this.hintBtn.Margin = new System.Windows.Forms.Padding(2);
            this.hintBtn.Name = "hintBtn";
            this.hintBtn.Size = new System.Drawing.Size(146, 86);
            this.hintBtn.TabIndex = 9;
            this.hintBtn.TabStop = false;
            this.hintBtn.Text = "Show Hint";
            this.hintBtn.UseVisualStyleBackColor = true;
            // 
            // StartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1459, 937);
            this.Controls.Add(this.gameBoxPuzzle);
            this.Controls.Add(this.groupBox1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "StartForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Puzzle";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button imgBrowseBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox squaresNrComboBox;
        private System.Windows.Forms.GroupBox gameBoxPuzzle;
        private System.Windows.Forms.Button startBtn;
        private System.Windows.Forms.PrintPreviewControl printPreviewControl1;
        private System.Windows.Forms.Button restartBtn;
        private System.Windows.Forms.Button hintBtn;
    }
}

