﻿namespace AutoClickOnLog
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
            this.lblStatus = new System.Windows.Forms.Label();
            this.StopButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.LogFolderTextBox = new System.Windows.Forms.TextBox();
            this.StartButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.LogStringTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ActionDelayTextBox = new System.Windows.Forms.TextBox();
            this.ViewlogButton = new System.Windows.Forms.Button();
            this.ClearlogButton = new System.Windows.Forms.Button();
            this.lblLastAction = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.Ch1Ch2FinishActionBox = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.Ch3Ch4FinishActionBox = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.RecheckLogFileTimeTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(174, 17);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(46, 13);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "Status : ";
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(93, 12);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 23);
            this.StopButton.TabIndex = 8;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Mlog Folder";
            // 
            // LogFolderTextBox
            // 
            this.LogFolderTextBox.Location = new System.Drawing.Point(99, 46);
            this.LogFolderTextBox.Name = "LogFolderTextBox";
            this.LogFolderTextBox.Size = new System.Drawing.Size(475, 20);
            this.LogFolderTextBox.TabIndex = 6;
            this.LogFolderTextBox.Text = "C:\\Users\\dh.chau\\Downloads\\Compressed\\GOOIL LOG";
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(12, 12);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(75, 23);
            this.StartButton.TabIndex = 5;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(55, 13);
            this.label3.TabIndex = 10;
            this.label3.Text = "Log String";
            // 
            // LogStringTextBox
            // 
            this.LogStringTextBox.Location = new System.Drawing.Point(99, 80);
            this.LogStringTextBox.Name = "LogStringTextBox";
            this.LogStringTextBox.Size = new System.Drawing.Size(475, 20);
            this.LogStringTextBox.TabIndex = 11;
            this.LogStringTextBox.Text = "Inspection Complete";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 216);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Action Delay (s)";
            // 
            // ActionDelayTextBox
            // 
            this.ActionDelayTextBox.Location = new System.Drawing.Point(104, 213);
            this.ActionDelayTextBox.Name = "ActionDelayTextBox";
            this.ActionDelayTextBox.Size = new System.Drawing.Size(475, 20);
            this.ActionDelayTextBox.TabIndex = 14;
            this.ActionDelayTextBox.Text = "15";
            // 
            // ViewlogButton
            // 
            this.ViewlogButton.Location = new System.Drawing.Point(442, 12);
            this.ViewlogButton.Name = "ViewlogButton";
            this.ViewlogButton.Size = new System.Drawing.Size(56, 23);
            this.ViewlogButton.TabIndex = 16;
            this.ViewlogButton.Text = "View log";
            this.ViewlogButton.UseVisualStyleBackColor = true;
            this.ViewlogButton.Click += new System.EventHandler(this.ViewlogButton_Click);
            // 
            // ClearlogButton
            // 
            this.ClearlogButton.Location = new System.Drawing.Point(504, 12);
            this.ClearlogButton.Name = "ClearlogButton";
            this.ClearlogButton.Size = new System.Drawing.Size(70, 23);
            this.ClearlogButton.TabIndex = 17;
            this.ClearlogButton.Text = "Clear log";
            this.ClearlogButton.UseVisualStyleBackColor = true;
            this.ClearlogButton.Click += new System.EventHandler(this.ClearlogButton_Click);
            // 
            // lblLastAction
            // 
            this.lblLastAction.AutoSize = true;
            this.lblLastAction.Location = new System.Drawing.Point(17, 257);
            this.lblLastAction.Name = "lblLastAction";
            this.lblLastAction.Size = new System.Drawing.Size(69, 13);
            this.lblLastAction.TabIndex = 18;
            this.lblLastAction.Text = "Last Action : ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(21, 151);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "Ch1 Ch2 Finish Action : ";
            // 
            // Ch1Ch2FinishActionBox
            // 
            this.Ch1Ch2FinishActionBox.FormattingEnabled = true;
            this.Ch1Ch2FinishActionBox.Items.AddRange(new object[] {
            "Run Ch1 Ch2",
            "Run Ch3 Ch4",
            "Run Any Ch"});
            this.Ch1Ch2FinishActionBox.Location = new System.Drawing.Point(147, 148);
            this.Ch1Ch2FinishActionBox.Name = "Ch1Ch2FinishActionBox";
            this.Ch1Ch2FinishActionBox.Size = new System.Drawing.Size(430, 21);
            this.Ch1Ch2FinishActionBox.TabIndex = 20;
            this.Ch1Ch2FinishActionBox.Text = "Run Ch3 Ch4";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(21, 182);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(120, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Ch3 Ch4 Finish Action : ";
            // 
            // Ch3Ch4FinishActionBox
            // 
            this.Ch3Ch4FinishActionBox.FormattingEnabled = true;
            this.Ch3Ch4FinishActionBox.Items.AddRange(new object[] {
            "Run Ch1 Ch2",
            "Run Ch3 Ch4",
            "Run Any Ch"});
            this.Ch3Ch4FinishActionBox.Location = new System.Drawing.Point(147, 179);
            this.Ch3Ch4FinishActionBox.Name = "Ch3Ch4FinishActionBox";
            this.Ch3Ch4FinishActionBox.Size = new System.Drawing.Size(430, 21);
            this.Ch3Ch4FinishActionBox.TabIndex = 22;
            this.Ch3Ch4FinishActionBox.Text = "Run Ch1 Ch2";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(21, 119);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(148, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "Recheck Log Files Interval (s)";
            // 
            // RecheckLogFileTimeTextBox
            // 
            this.RecheckLogFileTimeTextBox.Location = new System.Drawing.Point(177, 116);
            this.RecheckLogFileTimeTextBox.Name = "RecheckLogFileTimeTextBox";
            this.RecheckLogFileTimeTextBox.Size = new System.Drawing.Size(397, 20);
            this.RecheckLogFileTimeTextBox.TabIndex = 24;
            this.RecheckLogFileTimeTextBox.Text = "300";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 315);
            this.Controls.Add(this.RecheckLogFileTimeTextBox);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.Ch3Ch4FinishActionBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.Ch1Ch2FinishActionBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.lblLastAction);
            this.Controls.Add(this.ClearlogButton);
            this.Controls.Add(this.ViewlogButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ActionDelayTextBox);
            this.Controls.Add(this.LogStringTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LogFolderTextBox);
            this.Controls.Add(this.StartButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "AutoClickOnLog";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox LogFolderTextBox;
        private System.Windows.Forms.Button StartButton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox LogStringTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox ActionDelayTextBox;
        private System.Windows.Forms.Button ViewlogButton;
        private System.Windows.Forms.Button ClearlogButton;
        private System.Windows.Forms.Label lblLastAction;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox Ch1Ch2FinishActionBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox Ch3Ch4FinishActionBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox RecheckLogFileTimeTextBox;
    }
}

