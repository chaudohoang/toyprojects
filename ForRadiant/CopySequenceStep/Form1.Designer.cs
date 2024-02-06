namespace CopySequenceStep
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.flpSequenceItemsSource = new System.Windows.Forms.FlowLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.btnSourceSeqBrowse = new System.Windows.Forms.Button();
            this.txtSourceSeq = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lstSequenceItemsDestination = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnDestinationSeqBrowse = new System.Windows.Forms.Button();
            this.txtDestinationSeq = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnMoveUp = new System.Windows.Forms.Button();
            this.btnCopy = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.flpSequenceItemsSource);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnSourceSeqBrowse);
            this.panel1.Controls.Add(this.txtSourceSeq);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(546, 528);
            this.panel1.TabIndex = 0;
            // 
            // flpSequenceItemsSource
            // 
            this.flpSequenceItemsSource.AutoScroll = true;
            this.flpSequenceItemsSource.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpSequenceItemsSource.Location = new System.Drawing.Point(6, 47);
            this.flpSequenceItemsSource.Name = "flpSequenceItemsSource";
            this.flpSequenceItemsSource.Size = new System.Drawing.Size(537, 478);
            this.flpSequenceItemsSource.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 31);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(254, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "List of source sequence steps, check step to copy : ";
            // 
            // btnSourceSeqBrowse
            // 
            this.btnSourceSeqBrowse.Location = new System.Drawing.Point(468, 3);
            this.btnSourceSeqBrowse.Name = "btnSourceSeqBrowse";
            this.btnSourceSeqBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnSourceSeqBrowse.TabIndex = 2;
            this.btnSourceSeqBrowse.Text = "Browse";
            this.btnSourceSeqBrowse.UseVisualStyleBackColor = true;
            this.btnSourceSeqBrowse.Click += new System.EventHandler(this.btnSourceSeqBrowse_Click);
            // 
            // txtSourceSeq
            // 
            this.txtSourceSeq.Location = new System.Drawing.Point(130, 5);
            this.txtSourceSeq.Name = "txtSourceSeq";
            this.txtSourceSeq.Size = new System.Drawing.Size(332, 20);
            this.txtSourceSeq.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(102, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Source Sequence : ";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lstSequenceItemsDestination);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.btnDestinationSeqBrowse);
            this.panel2.Controls.Add(this.txtDestinationSeq);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Location = new System.Drawing.Point(564, 12);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(546, 528);
            this.panel2.TabIndex = 1;
            // 
            // lstSequenceItemsDestination
            // 
            this.lstSequenceItemsDestination.AllowDrop = true;
            this.lstSequenceItemsDestination.FormattingEnabled = true;
            this.lstSequenceItemsDestination.Location = new System.Drawing.Point(6, 47);
            this.lstSequenceItemsDestination.Name = "lstSequenceItemsDestination";
            this.lstSequenceItemsDestination.Size = new System.Drawing.Size(537, 472);
            this.lstSequenceItemsDestination.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 31);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(126, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "List of destination steps : ";
            // 
            // btnDestinationSeqBrowse
            // 
            this.btnDestinationSeqBrowse.Location = new System.Drawing.Point(468, 3);
            this.btnDestinationSeqBrowse.Name = "btnDestinationSeqBrowse";
            this.btnDestinationSeqBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnDestinationSeqBrowse.TabIndex = 2;
            this.btnDestinationSeqBrowse.Text = "Browse";
            this.btnDestinationSeqBrowse.UseVisualStyleBackColor = true;
            this.btnDestinationSeqBrowse.Click += new System.EventHandler(this.btnDestinationSeqBrowse_Click);
            // 
            // txtDestinationSeq
            // 
            this.txtDestinationSeq.Location = new System.Drawing.Point(130, 5);
            this.txtDestinationSeq.Name = "txtDestinationSeq";
            this.txtDestinationSeq.Size = new System.Drawing.Size(332, 20);
            this.txtDestinationSeq.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(121, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Destination Sequence : ";
            // 
            // btnMoveUp
            // 
            this.btnMoveUp.Location = new System.Drawing.Point(1116, 59);
            this.btnMoveUp.Name = "btnMoveUp";
            this.btnMoveUp.Size = new System.Drawing.Size(66, 39);
            this.btnMoveUp.TabIndex = 2;
            this.btnMoveUp.Text = "Move Item Up";
            this.btnMoveUp.UseVisualStyleBackColor = true;
            this.btnMoveUp.Click += new System.EventHandler(this.btnMoveUp_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Location = new System.Drawing.Point(1116, 149);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(66, 47);
            this.btnCopy.TabIndex = 4;
            this.btnCopy.Text = "Start Copy";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Location = new System.Drawing.Point(1116, 104);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(66, 39);
            this.btnMoveDown.TabIndex = 5;
            this.btnMoveDown.Text = "Move Item Down";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1190, 551);
            this.Controls.Add(this.btnMoveDown);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.btnMoveUp);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Copy Sequence Step";
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnSourceSeqBrowse;
        private System.Windows.Forms.TextBox txtSourceSeq;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.FlowLayoutPanel flpSequenceItemsSource;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnDestinationSeqBrowse;
        private System.Windows.Forms.TextBox txtDestinationSeq;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox lstSequenceItemsDestination;
        private System.Windows.Forms.Button btnMoveUp;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Button btnMoveDown;
    }
}

