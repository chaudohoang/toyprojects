
namespace EZAE
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
            this.btnSetsequence = new System.Windows.Forms.Button();
            this.btnSetupPC = new System.Windows.Forms.Button();
            this.btnCreateFFC = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSetsequence
            // 
            this.btnSetsequence.Location = new System.Drawing.Point(4, 5);
            this.btnSetsequence.Name = "btnSetsequence";
            this.btnSetsequence.Size = new System.Drawing.Size(105, 76);
            this.btnSetsequence.TabIndex = 0;
            this.btnSetsequence.Text = "Set Sequence";
            this.btnSetsequence.UseVisualStyleBackColor = true;
            this.btnSetsequence.Click += new System.EventHandler(this.btnSetsequence_Click);
            // 
            // btnSetupPC
            // 
            this.btnSetupPC.Location = new System.Drawing.Point(115, 5);
            this.btnSetupPC.Name = "btnSetupPC";
            this.btnSetupPC.Size = new System.Drawing.Size(105, 76);
            this.btnSetupPC.TabIndex = 1;
            this.btnSetupPC.Text = "Setup PC";
            this.btnSetupPC.UseVisualStyleBackColor = true;
            this.btnSetupPC.Click += new System.EventHandler(this.btnSetupPC_Click);
            // 
            // btnCreateFFC
            // 
            this.btnCreateFFC.Location = new System.Drawing.Point(4, 87);
            this.btnCreateFFC.Name = "btnCreateFFC";
            this.btnCreateFFC.Size = new System.Drawing.Size(105, 76);
            this.btnCreateFFC.TabIndex = 2;
            this.btnCreateFFC.Text = "Create FFC Databases";
            this.btnCreateFFC.UseVisualStyleBackColor = true;
            this.btnCreateFFC.Click += new System.EventHandler(this.btnCreateFFC_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(490, 270);
            this.Controls.Add(this.btnCreateFFC);
            this.Controls.Add(this.btnSetupPC);
            this.Controls.Add(this.btnSetsequence);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSetsequence;
        private System.Windows.Forms.Button btnSetupPC;
        private System.Windows.Forms.Button btnCreateFFC;
    }
}

