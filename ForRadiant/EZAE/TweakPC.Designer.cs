
namespace EZAE
{
    partial class TweakPC
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TweakPC));
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
            this.btnGivePermission = new System.Windows.Forms.Button();
            this.btnInstallWireshark = new System.Windows.Forms.Button();
            this.btnShareCProgram = new System.Windows.Forms.Button();
            this.btnShareEProgram = new System.Windows.Forms.Button();
            this.btnCopyAllToLocalProgram = new System.Windows.Forms.Button();
            this.btnUACOff = new System.Windows.Forms.Button();
            this.btnPasswordSharingOff = new System.Windows.Forms.Button();
            this.btnFirewallOff = new System.Windows.Forms.Button();
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
            "C:\\Program",
            "E:\\Program"});
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
            this.btnInstallVC12.Click += new System.EventHandler(this.btnInstallVC12_Click);
            // 
            // btnInstallVC1519
            // 
            this.btnInstallVC1519.Location = new System.Drawing.Point(114, 70);
            this.btnInstallVC1519.Name = "btnInstallVC1519";
            this.btnInstallVC1519.Size = new System.Drawing.Size(129, 28);
            this.btnInstallVC1519.TabIndex = 7;
            this.btnInstallVC1519.Text = "Install VC++2015-2019";
            this.btnInstallVC1519.UseVisualStyleBackColor = true;
            this.btnInstallVC1519.Click += new System.EventHandler(this.btnInstallVC1519_Click);
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
            this.btnInstallKdiff.Click += new System.EventHandler(this.btnInstallKdiff_Click);
            // 
            // btnInstallNPP
            // 
            this.btnInstallNPP.Location = new System.Drawing.Point(3, 104);
            this.btnInstallNPP.Name = "btnInstallNPP";
            this.btnInstallNPP.Size = new System.Drawing.Size(105, 28);
            this.btnInstallNPP.TabIndex = 10;
            this.btnInstallNPP.Text = "Install Notepad++";
            this.btnInstallNPP.UseVisualStyleBackColor = true;
            this.btnInstallNPP.Click += new System.EventHandler(this.btnInstallNPP_Click);
            // 
            // btnInstallDotnet
            // 
            this.btnInstallDotnet.Location = new System.Drawing.Point(114, 104);
            this.btnInstallDotnet.Name = "btnInstallDotnet";
            this.btnInstallDotnet.Size = new System.Drawing.Size(129, 28);
            this.btnInstallDotnet.TabIndex = 11;
            this.btnInstallDotnet.Text = "Install Dotnet 4.8";
            this.btnInstallDotnet.UseVisualStyleBackColor = true;
            this.btnInstallDotnet.Click += new System.EventHandler(this.btnInstallDotnet_Click);
            // 
            // btnInstallMatlab
            // 
            this.btnInstallMatlab.Location = new System.Drawing.Point(249, 104);
            this.btnInstallMatlab.Name = "btnInstallMatlab";
            this.btnInstallMatlab.Size = new System.Drawing.Size(105, 28);
            this.btnInstallMatlab.TabIndex = 12;
            this.btnInstallMatlab.Text = "Install Matlab";
            this.btnInstallMatlab.UseVisualStyleBackColor = true;
            this.btnInstallMatlab.Click += new System.EventHandler(this.btnInstallMatlab_Click);
            // 
            // btnShareC
            // 
            this.btnShareC.Location = new System.Drawing.Point(3, 138);
            this.btnShareC.Name = "btnShareC";
            this.btnShareC.Size = new System.Drawing.Size(105, 28);
            this.btnShareC.TabIndex = 13;
            this.btnShareC.Text = "ShareCDrive";
            this.btnShareC.UseVisualStyleBackColor = true;
            this.btnShareC.Click += new System.EventHandler(this.btnShareC_Click);
            // 
            // btnShareD
            // 
            this.btnShareD.Location = new System.Drawing.Point(114, 138);
            this.btnShareD.Name = "btnShareD";
            this.btnShareD.Size = new System.Drawing.Size(129, 28);
            this.btnShareD.TabIndex = 14;
            this.btnShareD.Text = "ShareDDrive";
            this.btnShareD.UseVisualStyleBackColor = true;
            this.btnShareD.Click += new System.EventHandler(this.btnShareD_Click);
            // 
            // btnShareE
            // 
            this.btnShareE.Location = new System.Drawing.Point(249, 138);
            this.btnShareE.Name = "btnShareE";
            this.btnShareE.Size = new System.Drawing.Size(105, 28);
            this.btnShareE.TabIndex = 15;
            this.btnShareE.Text = "ShareEDrive";
            this.btnShareE.UseVisualStyleBackColor = true;
            this.btnShareE.Click += new System.EventHandler(this.btnShareE_Click);
            // 
            // btnShareRVSData
            // 
            this.btnShareRVSData.Location = new System.Drawing.Point(3, 172);
            this.btnShareRVSData.Name = "btnShareRVSData";
            this.btnShareRVSData.Size = new System.Drawing.Size(105, 28);
            this.btnShareRVSData.TabIndex = 16;
            this.btnShareRVSData.Text = "ShareRVSData";
            this.btnShareRVSData.UseVisualStyleBackColor = true;
            this.btnShareRVSData.Click += new System.EventHandler(this.btnShareRVSData_Click);
            // 
            // btnCreateOTPandResult
            // 
            this.btnCreateOTPandResult.Location = new System.Drawing.Point(114, 172);
            this.btnCreateOTPandResult.Name = "btnCreateOTPandResult";
            this.btnCreateOTPandResult.Size = new System.Drawing.Size(129, 28);
            this.btnCreateOTPandResult.TabIndex = 17;
            this.btnCreateOTPandResult.Text = "Create OTP and Result";
            this.btnCreateOTPandResult.UseVisualStyleBackColor = true;
            this.btnCreateOTPandResult.Click += new System.EventHandler(this.btnCreateOTPandResult_Click);
            // 
            // btnShareDProgram
            // 
            this.btnShareDProgram.Location = new System.Drawing.Point(114, 206);
            this.btnShareDProgram.Name = "btnShareDProgram";
            this.btnShareDProgram.Size = new System.Drawing.Size(129, 28);
            this.btnShareDProgram.TabIndex = 18;
            this.btnShareDProgram.Text = "ShareDProgram";
            this.btnShareDProgram.UseVisualStyleBackColor = true;
            this.btnShareDProgram.Click += new System.EventHandler(this.btnShareDProgram_Click);
            // 
            // btnPinFolders
            // 
            this.btnPinFolders.Location = new System.Drawing.Point(249, 172);
            this.btnPinFolders.Name = "btnPinFolders";
            this.btnPinFolders.Size = new System.Drawing.Size(105, 28);
            this.btnPinFolders.TabIndex = 19;
            this.btnPinFolders.Text = "Pin Folders";
            this.btnPinFolders.UseVisualStyleBackColor = true;
            this.btnPinFolders.Click += new System.EventHandler(this.btnPinFolders_Click);
            // 
            // btnGivePermission
            // 
            this.btnGivePermission.Location = new System.Drawing.Point(3, 240);
            this.btnGivePermission.Name = "btnGivePermission";
            this.btnGivePermission.Size = new System.Drawing.Size(105, 28);
            this.btnGivePermission.TabIndex = 20;
            this.btnGivePermission.Text = "TT Full Permisison";
            this.btnGivePermission.UseVisualStyleBackColor = true;
            this.btnGivePermission.Click += new System.EventHandler(this.btnGivePermission_Click);
            // 
            // btnInstallWireshark
            // 
            this.btnInstallWireshark.Location = new System.Drawing.Point(249, 240);
            this.btnInstallWireshark.Name = "btnInstallWireshark";
            this.btnInstallWireshark.Size = new System.Drawing.Size(105, 28);
            this.btnInstallWireshark.TabIndex = 21;
            this.btnInstallWireshark.Text = "Install Wireshark";
            this.btnInstallWireshark.UseVisualStyleBackColor = true;
            this.btnInstallWireshark.Click += new System.EventHandler(this.btnInstallWireshark_Click);
            // 
            // btnShareCProgram
            // 
            this.btnShareCProgram.Location = new System.Drawing.Point(3, 206);
            this.btnShareCProgram.Name = "btnShareCProgram";
            this.btnShareCProgram.Size = new System.Drawing.Size(105, 28);
            this.btnShareCProgram.TabIndex = 22;
            this.btnShareCProgram.Text = "ShareCProgram";
            this.btnShareCProgram.UseVisualStyleBackColor = true;
            this.btnShareCProgram.Click += new System.EventHandler(this.btnShareCProgram_Click);
            // 
            // btnShareEProgram
            // 
            this.btnShareEProgram.Location = new System.Drawing.Point(249, 206);
            this.btnShareEProgram.Name = "btnShareEProgram";
            this.btnShareEProgram.Size = new System.Drawing.Size(105, 28);
            this.btnShareEProgram.TabIndex = 23;
            this.btnShareEProgram.Text = "ShareEProgram";
            this.btnShareEProgram.UseVisualStyleBackColor = true;
            this.btnShareEProgram.Click += new System.EventHandler(this.btnShareEProgram_Click);
            // 
            // btnCopyAllToLocalProgram
            // 
            this.btnCopyAllToLocalProgram.Location = new System.Drawing.Point(114, 240);
            this.btnCopyAllToLocalProgram.Name = "btnCopyAllToLocalProgram";
            this.btnCopyAllToLocalProgram.Size = new System.Drawing.Size(129, 28);
            this.btnCopyAllToLocalProgram.TabIndex = 24;
            this.btnCopyAllToLocalProgram.Text = "Copy All To Local";
            this.btnCopyAllToLocalProgram.UseVisualStyleBackColor = true;
            this.btnCopyAllToLocalProgram.Click += new System.EventHandler(this.btnCopyAllToLocalProgram_Click);
            // 
            // btnUACOff
            // 
            this.btnUACOff.Location = new System.Drawing.Point(3, 274);
            this.btnUACOff.Name = "btnUACOff";
            this.btnUACOff.Size = new System.Drawing.Size(105, 28);
            this.btnUACOff.TabIndex = 25;
            this.btnUACOff.Text = "UAC Off";
            this.btnUACOff.UseVisualStyleBackColor = true;
            this.btnUACOff.Click += new System.EventHandler(this.btnUACOff_Click);
            // 
            // btnPasswordSharingOff
            // 
            this.btnPasswordSharingOff.Location = new System.Drawing.Point(114, 274);
            this.btnPasswordSharingOff.Name = "btnPasswordSharingOff";
            this.btnPasswordSharingOff.Size = new System.Drawing.Size(129, 28);
            this.btnPasswordSharingOff.TabIndex = 26;
            this.btnPasswordSharingOff.Text = "Password Sharing Off";
            this.btnPasswordSharingOff.UseVisualStyleBackColor = true;
            this.btnPasswordSharingOff.Click += new System.EventHandler(this.btnPasswordSharingOff_Click);
            // 
            // btnFirewallOff
            // 
            this.btnFirewallOff.Location = new System.Drawing.Point(249, 274);
            this.btnFirewallOff.Name = "btnFirewallOff";
            this.btnFirewallOff.Size = new System.Drawing.Size(105, 28);
            this.btnFirewallOff.TabIndex = 27;
            this.btnFirewallOff.Text = "Firewall Off";
            this.btnFirewallOff.UseVisualStyleBackColor = true;
            this.btnFirewallOff.Click += new System.EventHandler(this.btnFirewallOff_Click);
            // 
            // TweakPC
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(441, 333);
            this.Controls.Add(this.btnFirewallOff);
            this.Controls.Add(this.btnPasswordSharingOff);
            this.Controls.Add(this.btnUACOff);
            this.Controls.Add(this.btnCopyAllToLocalProgram);
            this.Controls.Add(this.btnShareEProgram);
            this.Controls.Add(this.btnShareCProgram);
            this.Controls.Add(this.btnInstallWireshark);
            this.Controls.Add(this.btnGivePermission);
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
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TweakPC";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Tweak PC";
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
        private System.Windows.Forms.Button btnGivePermission;
        private System.Windows.Forms.Button btnInstallWireshark;
        private System.Windows.Forms.Button btnShareCProgram;
        private System.Windows.Forms.Button btnShareEProgram;
        private System.Windows.Forms.Button btnCopyAllToLocalProgram;
        private System.Windows.Forms.Button btnUACOff;
        private System.Windows.Forms.Button btnPasswordSharingOff;
        private System.Windows.Forms.Button btnFirewallOff;
    }
}