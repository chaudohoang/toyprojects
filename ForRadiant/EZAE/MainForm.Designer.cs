
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
            this.btnCheckLC = new System.Windows.Forms.Button();
            this.btnOpenImageJ = new System.Windows.Forms.Button();
            this.btnOpenLanSearch = new System.Windows.Forms.Button();
            this.btnOpenMTFCenter = new System.Windows.Forms.Button();
            this.btnOpenTools = new System.Windows.Forms.Button();
            this.btnOpenUpdates = new System.Windows.Forms.Button();
            this.btnEngineeringMode = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSetsequence
            // 
            this.btnSetsequence.Location = new System.Drawing.Point(4, 5);
            this.btnSetsequence.Name = "btnSetsequence";
            this.btnSetsequence.Size = new System.Drawing.Size(128, 23);
            this.btnSetsequence.TabIndex = 0;
            this.btnSetsequence.Text = "Set Sequence";
            this.btnSetsequence.UseVisualStyleBackColor = true;
            this.btnSetsequence.Click += new System.EventHandler(this.btnSetsequence_Click);
            // 
            // btnSetupPC
            // 
            this.btnSetupPC.Location = new System.Drawing.Point(138, 5);
            this.btnSetupPC.Name = "btnSetupPC";
            this.btnSetupPC.Size = new System.Drawing.Size(128, 23);
            this.btnSetupPC.TabIndex = 1;
            this.btnSetupPC.Text = "Setup PC";
            this.btnSetupPC.UseVisualStyleBackColor = true;
            this.btnSetupPC.Click += new System.EventHandler(this.btnSetupPC_Click);
            // 
            // btnCreateFFC
            // 
            this.btnCreateFFC.Location = new System.Drawing.Point(272, 5);
            this.btnCreateFFC.Name = "btnCreateFFC";
            this.btnCreateFFC.Size = new System.Drawing.Size(128, 23);
            this.btnCreateFFC.TabIndex = 2;
            this.btnCreateFFC.Text = "Create FFC Databases";
            this.btnCreateFFC.UseVisualStyleBackColor = true;
            this.btnCreateFFC.Click += new System.EventHandler(this.btnCreateFFC_Click);
            // 
            // btnCheckLC
            // 
            this.btnCheckLC.Location = new System.Drawing.Point(4, 34);
            this.btnCheckLC.Name = "btnCheckLC";
            this.btnCheckLC.Size = new System.Drawing.Size(128, 23);
            this.btnCheckLC.TabIndex = 3;
            this.btnCheckLC.Text = "Check License Code";
            this.btnCheckLC.UseVisualStyleBackColor = true;
            // 
            // btnOpenImageJ
            // 
            this.btnOpenImageJ.Location = new System.Drawing.Point(138, 34);
            this.btnOpenImageJ.Name = "btnOpenImageJ";
            this.btnOpenImageJ.Size = new System.Drawing.Size(128, 23);
            this.btnOpenImageJ.TabIndex = 4;
            this.btnOpenImageJ.Text = "Open ImageJ";
            this.btnOpenImageJ.UseVisualStyleBackColor = true;
            // 
            // btnOpenLanSearch
            // 
            this.btnOpenLanSearch.Location = new System.Drawing.Point(272, 34);
            this.btnOpenLanSearch.Name = "btnOpenLanSearch";
            this.btnOpenLanSearch.Size = new System.Drawing.Size(128, 23);
            this.btnOpenLanSearch.TabIndex = 5;
            this.btnOpenLanSearch.Text = "Open Lan Search";
            this.btnOpenLanSearch.UseVisualStyleBackColor = true;
            // 
            // btnOpenMTFCenter
            // 
            this.btnOpenMTFCenter.Location = new System.Drawing.Point(138, 63);
            this.btnOpenMTFCenter.Name = "btnOpenMTFCenter";
            this.btnOpenMTFCenter.Size = new System.Drawing.Size(128, 23);
            this.btnOpenMTFCenter.TabIndex = 7;
            this.btnOpenMTFCenter.Text = "Open MTF Center";
            this.btnOpenMTFCenter.UseVisualStyleBackColor = true;
            // 
            // btnOpenTools
            // 
            this.btnOpenTools.Location = new System.Drawing.Point(272, 63);
            this.btnOpenTools.Name = "btnOpenTools";
            this.btnOpenTools.Size = new System.Drawing.Size(128, 23);
            this.btnOpenTools.TabIndex = 8;
            this.btnOpenTools.Text = "Open Tools";
            this.btnOpenTools.UseVisualStyleBackColor = true;
            // 
            // btnOpenUpdates
            // 
            this.btnOpenUpdates.Location = new System.Drawing.Point(4, 63);
            this.btnOpenUpdates.Name = "btnOpenUpdates";
            this.btnOpenUpdates.Size = new System.Drawing.Size(128, 23);
            this.btnOpenUpdates.TabIndex = 9;
            this.btnOpenUpdates.Text = "Open Updates";
            this.btnOpenUpdates.UseVisualStyleBackColor = true;
            // 
            // btnEngineeringMode
            // 
            this.btnEngineeringMode.Location = new System.Drawing.Point(4, 92);
            this.btnEngineeringMode.Name = "btnEngineeringMode";
            this.btnEngineeringMode.Size = new System.Drawing.Size(396, 36);
            this.btnEngineeringMode.TabIndex = 10;
            this.btnEngineeringMode.Text = "Engineering Mode";
            this.btnEngineeringMode.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(513, 270);
            this.Controls.Add(this.btnEngineeringMode);
            this.Controls.Add(this.btnOpenUpdates);
            this.Controls.Add(this.btnOpenTools);
            this.Controls.Add(this.btnOpenMTFCenter);
            this.Controls.Add(this.btnOpenLanSearch);
            this.Controls.Add(this.btnOpenImageJ);
            this.Controls.Add(this.btnCheckLC);
            this.Controls.Add(this.btnCreateFFC);
            this.Controls.Add(this.btnSetupPC);
            this.Controls.Add(this.btnSetsequence);
            this.HelpButton = true;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EZAE";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSetsequence;
        private System.Windows.Forms.Button btnSetupPC;
        private System.Windows.Forms.Button btnCreateFFC;
        private System.Windows.Forms.Button btnCheckLC;
        private System.Windows.Forms.Button btnOpenImageJ;
        private System.Windows.Forms.Button btnOpenLanSearch;
        private System.Windows.Forms.Button btnOpenMTFCenter;
        private System.Windows.Forms.Button btnOpenTools;
        private System.Windows.Forms.Button btnOpenUpdates;
        private System.Windows.Forms.Button btnEngineeringMode;
    }
}

