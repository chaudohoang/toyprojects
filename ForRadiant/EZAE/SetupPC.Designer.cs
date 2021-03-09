
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
            this.cbTightVNCPath = new System.Windows.Forms.ComboBox();
            this.btnCopyDISettings = new System.Windows.Forms.Button();
            this.cbDIList = new System.Windows.Forms.ComboBox();
            this.btnInstallVC12 = new System.Windows.Forms.Button();
            this.btnInstallVC1519 = new System.Windows.Forms.Button();
            this.btnInstallIrfanview = new System.Windows.Forms.Button();
            this.btnInstallKdiff = new System.Windows.Forms.Button();
            this.btnInstallNPP = new System.Windows.Forms.Button();
            this.btnInstallDotnet = new System.Windows.Forms.Button();
            this.btnInstallMatlab = new System.Windows.Forms.Button();
            this.btnShareC = new System.Windows.Forms.Button();
            this.btnShareD = new System.Windows.Forms.Button();
            this.btnShareE = new System.Windows.Forms.Button();
            this.btnShareRVSData = new System.Windows.Forms.Button();
            this.btnCreateOTPandResult = new System.Windows.Forms.Button();
            this.btnShareDProgram = new System.Windows.Forms.Button();
            this.btnPinFolders = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
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
            this.btnRemoveTightVNC.Location = new System.Drawing.Point(249, 7);
            this.btnRemoveTightVNC.Name = "btnRemoveTightVNC";
            this.btnRemoveTightVNC.Size = new System.Drawing.Size(105, 23);
            this.btnRemoveTightVNC.TabIndex = 2;
            this.btnRemoveTightVNC.Text = "Remove TightVNC";
            this.btnRemoveTightVNC.UseVisualStyleBackColor = true;
            this.btnRemoveTightVNC.Click += new System.EventHandler(this.btnRemoveTightVNC_Click);
            // 
            // cbTightVNCPath
            // 
            this.cbTightVNCPath.FormattingEnabled = true;
            this.cbTightVNCPath.Items.AddRange(new object[] {
            "D:\\Program",
            "C:\\Program"});
            this.cbTightVNCPath.Location = new System.Drawing.Point(114, 7);
            this.cbTightVNCPath.Name = "cbTightVNCPath";
            this.cbTightVNCPath.Size = new System.Drawing.Size(129, 21);
            this.cbTightVNCPath.TabIndex = 3;
            this.cbTightVNCPath.Text = "D:\\Program";
            // 
            // btnCopyDISettings
            // 
            this.btnCopyDISettings.Location = new System.Drawing.Point(3, 36);
            this.btnCopyDISettings.Name = "btnCopyDISettings";
            this.btnCopyDISettings.Size = new System.Drawing.Size(105, 28);
            this.btnCopyDISettings.TabIndex = 4;
            this.btnCopyDISettings.Text = "Copy DISettings";
            this.btnCopyDISettings.UseVisualStyleBackColor = true;
            this.btnCopyDISettings.Click += new System.EventHandler(this.btnCopyDISettings_Click);
            // 
            // cbDIList
            // 
            this.cbDIList.FormattingEnabled = true;
            this.cbDIList.Items.AddRange(new object[] {
            "Dove2p0 CH1",
            "Dove2p0 CH2",
            "Dove2p0 CH3",
            "Dove2p0 CH4",
            "Emu2p0 CH1",
            "Emu2p0 CH2",
            "Emu2p0 CH3",
            "Emu2p0 CH4"});
            this.cbDIList.Location = new System.Drawing.Point(114, 41);
            this.cbDIList.Name = "cbDIList";
            this.cbDIList.Size = new System.Drawing.Size(129, 21);
            this.cbDIList.TabIndex = 5;
            // 
            // btnInstallVC12
            // 
            this.btnInstallVC12.Location = new System.Drawing.Point(3, 70);
            this.btnInstallVC12.Name = "btnInstallVC12";
            this.btnInstallVC12.Size = new System.Drawing.Size(105, 28);
            this.btnInstallVC12.TabIndex = 6;
            this.btnInstallVC12.Text = "Install VC++2012";
            this.btnInstallVC12.UseVisualStyleBackColor = true;
            // 
            // btnInstallVC1519
            // 
            this.btnInstallVC1519.Location = new System.Drawing.Point(114, 70);
            this.btnInstallVC1519.Name = "btnInstallVC1519";
            this.btnInstallVC1519.Size = new System.Drawing.Size(129, 28);
            this.btnInstallVC1519.TabIndex = 7;
            this.btnInstallVC1519.Text = "Install VC++2015-2019";
            this.btnInstallVC1519.UseVisualStyleBackColor = true;
            // 
            // btnInstallIrfanview
            // 
            this.btnInstallIrfanview.Location = new System.Drawing.Point(249, 36);
            this.btnInstallIrfanview.Name = "btnInstallIrfanview";
            this.btnInstallIrfanview.Size = new System.Drawing.Size(105, 28);
            this.btnInstallIrfanview.TabIndex = 8;
            this.btnInstallIrfanview.Text = "Install Irfanview";
            this.btnInstallIrfanview.UseVisualStyleBackColor = true;
            this.btnInstallIrfanview.Click += new System.EventHandler(this.btnInstallIrfanview_Click);
            // 
            // btnInstallKdiff
            // 
            this.btnInstallKdiff.Location = new System.Drawing.Point(249, 70);
            this.btnInstallKdiff.Name = "btnInstallKdiff";
            this.btnInstallKdiff.Size = new System.Drawing.Size(105, 28);
            this.btnInstallKdiff.TabIndex = 9;
            this.btnInstallKdiff.Text = "Install Kdiff";
            this.btnInstallKdiff.UseVisualStyleBackColor = true;
            // 
            // btnInstallNPP
            // 
            this.btnInstallNPP.Location = new System.Drawing.Point(3, 104);
            this.btnInstallNPP.Name = "btnInstallNPP";
            this.btnInstallNPP.Size = new System.Drawing.Size(105, 28);
            this.btnInstallNPP.TabIndex = 10;
            this.btnInstallNPP.Text = "Install Notepad++";
            this.btnInstallNPP.UseVisualStyleBackColor = true;
            // 
            // btnInstallDotnet
            // 
            this.btnInstallDotnet.Location = new System.Drawing.Point(114, 104);
            this.btnInstallDotnet.Name = "btnInstallDotnet";
            this.btnInstallDotnet.Size = new System.Drawing.Size(129, 28);
            this.btnInstallDotnet.TabIndex = 11;
            this.btnInstallDotnet.Text = "Install Dotnet 4.8";
            this.btnInstallDotnet.UseVisualStyleBackColor = true;
            // 
            // btnInstallMatlab
            // 
            this.btnInstallMatlab.Location = new System.Drawing.Point(249, 104);
            this.btnInstallMatlab.Name = "btnInstallMatlab";
            this.btnInstallMatlab.Size = new System.Drawing.Size(105, 28);
            this.btnInstallMatlab.TabIndex = 12;
            this.btnInstallMatlab.Text = "Install Matlab";
            this.btnInstallMatlab.UseVisualStyleBackColor = true;
            // 
            // btnShareC
            // 
            this.btnShareC.Location = new System.Drawing.Point(3, 138);
            this.btnShareC.Name = "btnShareC";
            this.btnShareC.Size = new System.Drawing.Size(105, 28);
            this.btnShareC.TabIndex = 13;
            this.btnShareC.Text = "ShareCDrive";
            this.btnShareC.UseVisualStyleBackColor = true;
            // 
            // btnShareD
            // 
            this.btnShareD.Location = new System.Drawing.Point(114, 138);
            this.btnShareD.Name = "btnShareD";
            this.btnShareD.Size = new System.Drawing.Size(129, 28);
            this.btnShareD.TabIndex = 14;
            this.btnShareD.Text = "ShareDDrive";
            this.btnShareD.UseVisualStyleBackColor = true;
            // 
            // btnShareE
            // 
            this.btnShareE.Location = new System.Drawing.Point(249, 138);
            this.btnShareE.Name = "btnShareE";
            this.btnShareE.Size = new System.Drawing.Size(105, 28);
            this.btnShareE.TabIndex = 15;
            this.btnShareE.Text = "ShareEDrive";
            this.btnShareE.UseVisualStyleBackColor = true;
            // 
            // btnShareRVSData
            // 
            this.btnShareRVSData.Location = new System.Drawing.Point(3, 172);
            this.btnShareRVSData.Name = "btnShareRVSData";
            this.btnShareRVSData.Size = new System.Drawing.Size(105, 28);
            this.btnShareRVSData.TabIndex = 16;
            this.btnShareRVSData.Text = "ShareRVSData";
            this.btnShareRVSData.UseVisualStyleBackColor = true;
            // 
            // btnCreateOTPandResult
            // 
            this.btnCreateOTPandResult.Location = new System.Drawing.Point(114, 172);
            this.btnCreateOTPandResult.Name = "btnCreateOTPandResult";
            this.btnCreateOTPandResult.Size = new System.Drawing.Size(129, 28);
            this.btnCreateOTPandResult.TabIndex = 17;
            this.btnCreateOTPandResult.Text = "Create OTP and Result";
            this.btnCreateOTPandResult.UseVisualStyleBackColor = true;
            // 
            // btnShareDProgram
            // 
            this.btnShareDProgram.Location = new System.Drawing.Point(249, 172);
            this.btnShareDProgram.Name = "btnShareDProgram";
            this.btnShareDProgram.Size = new System.Drawing.Size(105, 28);
            this.btnShareDProgram.TabIndex = 18;
            this.btnShareDProgram.Text = "ShareDProgram";
            this.btnShareDProgram.UseVisualStyleBackColor = true;
            // 
            // btnPinFolders
            // 
            this.btnPinFolders.Location = new System.Drawing.Point(3, 206);
            this.btnPinFolders.Name = "btnPinFolders";
            this.btnPinFolders.Size = new System.Drawing.Size(105, 28);
            this.btnPinFolders.TabIndex = 19;
            this.btnPinFolders.Text = "Pin Folders";
            this.btnPinFolders.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(114, 206);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(129, 28);
            this.button1.TabIndex = 20;
            this.button1.Text = "TT Full Permisison";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // SetupPC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(469, 263);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnPinFolders);
            this.Controls.Add(this.btnShareDProgram);
            this.Controls.Add(this.btnCreateOTPandResult);
            this.Controls.Add(this.btnShareRVSData);
            this.Controls.Add(this.btnShareE);
            this.Controls.Add(this.btnShareD);
            this.Controls.Add(this.btnShareC);
            this.Controls.Add(this.btnInstallMatlab);
            this.Controls.Add(this.btnInstallDotnet);
            this.Controls.Add(this.btnInstallNPP);
            this.Controls.Add(this.btnInstallKdiff);
            this.Controls.Add(this.btnInstallIrfanview);
            this.Controls.Add(this.btnInstallVC1519);
            this.Controls.Add(this.btnInstallVC12);
            this.Controls.Add(this.cbDIList);
            this.Controls.Add(this.btnCopyDISettings);
            this.Controls.Add(this.cbTightVNCPath);
            this.Controls.Add(this.btnRemoveTightVNC);
            this.Controls.Add(this.btnInstallTightVNC);
            this.Name = "SetupPC";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "SetupPC";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnInstallTightVNC;
        private System.Windows.Forms.Button btnRemoveTightVNC;
        private System.Windows.Forms.ComboBox cbTightVNCPath;
        private System.Windows.Forms.Button btnCopyDISettings;
        private System.Windows.Forms.ComboBox cbDIList;
        private System.Windows.Forms.Button btnInstallVC12;
        private System.Windows.Forms.Button btnInstallVC1519;
        private System.Windows.Forms.Button btnInstallIrfanview;
        private System.Windows.Forms.Button btnInstallKdiff;
        private System.Windows.Forms.Button btnInstallNPP;
        private System.Windows.Forms.Button btnInstallDotnet;
        private System.Windows.Forms.Button btnInstallMatlab;
        private System.Windows.Forms.Button btnShareC;
        private System.Windows.Forms.Button btnShareD;
        private System.Windows.Forms.Button btnShareE;
        private System.Windows.Forms.Button btnShareRVSData;
        private System.Windows.Forms.Button btnCreateOTPandResult;
        private System.Windows.Forms.Button btnShareDProgram;
        private System.Windows.Forms.Button btnPinFolders;
        private System.Windows.Forms.Button button1;
    }
}