
namespace EZAE
{
    partial class SetupPC
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
            this.btnInstallTightVNC = new System.Windows.Forms.Button();
            this.btnRemoveTightVNC = new System.Windows.Forms.Button();
            this.txtTightVNCPath = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // btnInstallTightVNC
            // 
            this.btnInstallTightVNC.Location = new System.Drawing.Point(3, 2);
            this.btnInstallTightVNC.Name = "btnInstallTightVNC";
            this.btnInstallTightVNC.Size = new System.Drawing.Size(105, 28);
            this.btnInstallTightVNC.TabIndex = 1;
            this.btnInstallTightVNC.Text = "Install TightVNC to";
            this.btnInstallTightVNC.UseVisualStyleBackColor = true;
            this.btnInstallTightVNC.Click += new System.EventHandler(this.btnInstallTightVNC_Click);
            // 
            // btnRemoveTightVNC
            // 
            this.btnRemoveTightVNC.Location = new System.Drawing.Point(208, 7);
            this.btnRemoveTightVNC.Name = "btnRemoveTightVNC";
            this.btnRemoveTightVNC.Size = new System.Drawing.Size(105, 23);
            this.btnRemoveTightVNC.TabIndex = 2;
            this.btnRemoveTightVNC.Text = "Remove TightVNC";
            this.btnRemoveTightVNC.UseVisualStyleBackColor = true;
            this.btnRemoveTightVNC.Click += new System.EventHandler(this.btnRemoveTightVNC_Click);
            // 
            // txtTightVNCPath
            // 
            this.txtTightVNCPath.FormattingEnabled = true;
            this.txtTightVNCPath.Items.AddRange(new object[] {
            "D:\\Program",
            "C:\\Program"});
            this.txtTightVNCPath.Location = new System.Drawing.Point(114, 7);
            this.txtTightVNCPath.Name = "txtTightVNCPath";
            this.txtTightVNCPath.Size = new System.Drawing.Size(88, 21);
            this.txtTightVNCPath.TabIndex = 3;
            this.txtTightVNCPath.Text = "D:\\Program";
            // 
            // SetupPC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(469, 263);
            this.Controls.Add(this.txtTightVNCPath);
            this.Controls.Add(this.btnRemoveTightVNC);
            this.Controls.Add(this.btnInstallTightVNC);
            this.Name = "SetupPC";
            this.Text = "SetupPC";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnInstallTightVNC;
        private System.Windows.Forms.Button btnRemoveTightVNC;
        private System.Windows.Forms.ComboBox txtTightVNCPath;
    }
}