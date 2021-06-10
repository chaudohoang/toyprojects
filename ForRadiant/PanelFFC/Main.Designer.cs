
namespace PanelFFC
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.btnIllunisCF = new System.Windows.Forms.Button();
            this.btnIllunisDJ = new System.Windows.Forms.Button();
            this.btnIllunisEP = new System.Windows.Forms.Button();
            this.btnRadiantCF = new System.Windows.Forms.Button();
            this.btnX10801panel = new System.Windows.Forms.Button();
            this.btnX10805panel = new System.Windows.Forms.Button();
            this.btnRgbFFC = new System.Windows.Forms.Button();
            this.btnGrayFFC = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnIllunisCF
            // 
            this.btnIllunisCF.Location = new System.Drawing.Point(12, 12);
            this.btnIllunisCF.Name = "btnIllunisCF";
            this.btnIllunisCF.Size = new System.Drawing.Size(75, 23);
            this.btnIllunisCF.TabIndex = 0;
            this.btnIllunisCF.Text = "Illunis CF";
            this.btnIllunisCF.UseVisualStyleBackColor = true;
            this.btnIllunisCF.Click += new System.EventHandler(this.btnIllunisCF_Click);
            // 
            // btnIllunisDJ
            // 
            this.btnIllunisDJ.Location = new System.Drawing.Point(93, 12);
            this.btnIllunisDJ.Name = "btnIllunisDJ";
            this.btnIllunisDJ.Size = new System.Drawing.Size(75, 23);
            this.btnIllunisDJ.TabIndex = 1;
            this.btnIllunisDJ.Text = "Illunis DJ";
            this.btnIllunisDJ.UseVisualStyleBackColor = true;
            this.btnIllunisDJ.Click += new System.EventHandler(this.btnIllunisDJ_Click);
            // 
            // btnIllunisEP
            // 
            this.btnIllunisEP.Location = new System.Drawing.Point(174, 12);
            this.btnIllunisEP.Name = "btnIllunisEP";
            this.btnIllunisEP.Size = new System.Drawing.Size(75, 23);
            this.btnIllunisEP.TabIndex = 2;
            this.btnIllunisEP.Text = "Illunis EP";
            this.btnIllunisEP.UseVisualStyleBackColor = true;
            this.btnIllunisEP.Click += new System.EventHandler(this.btnIllunisEP_Click);
            // 
            // btnRadiantCF
            // 
            this.btnRadiantCF.Location = new System.Drawing.Point(12, 41);
            this.btnRadiantCF.Name = "btnRadiantCF";
            this.btnRadiantCF.Size = new System.Drawing.Size(75, 23);
            this.btnRadiantCF.TabIndex = 3;
            this.btnRadiantCF.Text = "Radiant CF";
            this.btnRadiantCF.UseVisualStyleBackColor = true;
            this.btnRadiantCF.Click += new System.EventHandler(this.btnRadiantCF_Click);
            // 
            // btnX10801panel
            // 
            this.btnX10801panel.Location = new System.Drawing.Point(93, 41);
            this.btnX10801panel.Name = "btnX10801panel";
            this.btnX10801panel.Size = new System.Drawing.Size(75, 23);
            this.btnX10801panel.TabIndex = 4;
            this.btnX10801panel.Text = "X1080_1";
            this.btnX10801panel.UseVisualStyleBackColor = true;
            this.btnX10801panel.Click += new System.EventHandler(this.btnX10801panel_Click);
            // 
            // btnX10805panel
            // 
            this.btnX10805panel.Location = new System.Drawing.Point(174, 41);
            this.btnX10805panel.Name = "btnX10805panel";
            this.btnX10805panel.Size = new System.Drawing.Size(75, 23);
            this.btnX10805panel.TabIndex = 5;
            this.btnX10805panel.Text = "X1080_5";
            this.btnX10805panel.UseVisualStyleBackColor = true;
            this.btnX10805panel.Click += new System.EventHandler(this.btnX10805panel_Click);
            // 
            // btnRgbFFC
            // 
            this.btnRgbFFC.Location = new System.Drawing.Point(12, 70);
            this.btnRgbFFC.Name = "btnRgbFFC";
            this.btnRgbFFC.Size = new System.Drawing.Size(75, 23);
            this.btnRgbFFC.TabIndex = 3;
            this.btnRgbFFC.Text = "Rgb FFC";
            this.btnRgbFFC.UseVisualStyleBackColor = true;
            this.btnRgbFFC.Click += new System.EventHandler(this.btnRgbFFC_Click);
            // 
            // btnGrayFFC
            // 
            this.btnGrayFFC.Location = new System.Drawing.Point(93, 70);
            this.btnGrayFFC.Name = "btnGrayFFC";
            this.btnGrayFFC.Size = new System.Drawing.Size(75, 23);
            this.btnGrayFFC.TabIndex = 0;
            this.btnGrayFFC.Text = "Gray FFC";
            this.btnGrayFFC.UseVisualStyleBackColor = true;
            this.btnGrayFFC.Click += new System.EventHandler(this.btnGrayFFC_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 102);
            this.Controls.Add(this.btnX10805panel);
            this.Controls.Add(this.btnX10801panel);
            this.Controls.Add(this.btnRgbFFC);
            this.Controls.Add(this.btnRadiantCF);
            this.Controls.Add(this.btnIllunisEP);
            this.Controls.Add(this.btnIllunisDJ);
            this.Controls.Add(this.btnGrayFFC);
            this.Controls.Add(this.btnIllunisCF);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Main";
            this.Text = "Panel FFC";
            this.Load += new System.EventHandler(this.Main_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnIllunisCF;
        private System.Windows.Forms.Button btnIllunisDJ;
        private System.Windows.Forms.Button btnIllunisEP;
        private System.Windows.Forms.Button btnRadiantCF;
        private System.Windows.Forms.Button btnX10801panel;
        private System.Windows.Forms.Button btnX10805panel;
        private System.Windows.Forms.Button btnRgbFFC;
        private System.Windows.Forms.Button btnGrayFFC;
    }
}

