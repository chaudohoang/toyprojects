using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SetSequence
{
    public partial class CalRule : Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components != null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            this.btnUseLastModified3 = new System.Windows.Forms.Button();
            this.Label5 = new System.Windows.Forms.Label();
            this.txtFile3 = new System.Windows.Forms.TextBox();
            this.btnBrowse3 = new System.Windows.Forms.Button();
            this.TabControl1 = new System.Windows.Forms.TabControl();
            this.TabPage1 = new System.Windows.Forms.TabPage();
            this.btnColorCalReloadCalRules = new System.Windows.Forms.Button();
            this.bntColorCalUseSampleRuleColor = new System.Windows.Forms.Button();
            this.btnColorCalUseSampleRuleMono = new System.Windows.Forms.Button();
            this.ColorCalDataGridView2 = new System.Windows.Forms.DataGridView();
            this.Label1 = new System.Windows.Forms.Label();
            this.ColorCalDataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnColorCalSaveCalRules = new System.Windows.Forms.Button();
            this.Label2 = new System.Windows.Forms.Label();
            this.TabPage2 = new System.Windows.Forms.TabPage();
            this.btnFlatFieldCalReloadCalRules = new System.Windows.Forms.Button();
            this.bntFlatFieldCalUseSampleRuleColor = new System.Windows.Forms.Button();
            this.btnFlatFieldCalUseSampleRuleMono = new System.Windows.Forms.Button();
            this.FlatFieldCalDataGridView2 = new System.Windows.Forms.DataGridView();
            this.Label3 = new System.Windows.Forms.Label();
            this.FlatFieldCalDataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnFlatFieldCalSaveCalRules = new System.Windows.Forms.Button();
            this.Label4 = new System.Windows.Forms.Label();
            this.TabPage3 = new System.Windows.Forms.TabPage();
            this.btnImgScaleCalReloadCalRules = new System.Windows.Forms.Button();
            this.bntImgScaleCalUseSampleRuleColor = new System.Windows.Forms.Button();
            this.btnImgScaleCalUseSampleRuleMono = new System.Windows.Forms.Button();
            this.ImgScaleCalDataGridView2 = new System.Windows.Forms.DataGridView();
            this.Label6 = new System.Windows.Forms.Label();
            this.ImgScaleCalDataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnImgScaleCalSaveCalRules = new System.Windows.Forms.Button();
            this.Label7 = new System.Windows.Forms.Label();
            this.TabControl1.SuspendLayout();
            this.TabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColorCalDataGridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColorCalDataGridView1)).BeginInit();
            this.TabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FlatFieldCalDataGridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FlatFieldCalDataGridView1)).BeginInit();
            this.TabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImgScaleCalDataGridView2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgScaleCalDataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnUseLastModified3
            // 
            this.btnUseLastModified3.Location = new System.Drawing.Point(671, 10);
            this.btnUseLastModified3.Name = "btnUseLastModified3";
            this.btnUseLastModified3.Size = new System.Drawing.Size(87, 23);
            this.btnUseLastModified3.TabIndex = 21;
            this.btnUseLastModified3.Text = "Last Modified";
            this.btnUseLastModified3.UseVisualStyleBackColor = true;
            this.btnUseLastModified3.Click += new System.EventHandler(this.btnUseLastModified3_Click);
            // 
            // Label5
            // 
            this.Label5.AutoSize = true;
            this.Label5.Location = new System.Drawing.Point(5, 15);
            this.Label5.Name = "Label5";
            this.Label5.Size = new System.Drawing.Size(56, 13);
            this.Label5.TabIndex = 18;
            this.Label5.Text = "Sequence";
            // 
            // txtFile3
            // 
            this.txtFile3.AllowDrop = true;
            this.txtFile3.Location = new System.Drawing.Point(76, 12);
            this.txtFile3.Name = "txtFile3";
            this.txtFile3.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtFile3.Size = new System.Drawing.Size(589, 20);
            this.txtFile3.TabIndex = 19;
            this.txtFile3.TextChanged += new System.EventHandler(this.txtFile3_TextChanged);
            // 
            // btnBrowse3
            // 
            this.btnBrowse3.Location = new System.Drawing.Point(764, 10);
            this.btnBrowse3.Name = "btnBrowse3";
            this.btnBrowse3.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse3.TabIndex = 20;
            this.btnBrowse3.Text = "Browse";
            this.btnBrowse3.UseVisualStyleBackColor = true;
            this.btnBrowse3.Click += new System.EventHandler(this.btnBrowse3_Click);
            // 
            // TabControl1
            // 
            this.TabControl1.Controls.Add(this.TabPage1);
            this.TabControl1.Controls.Add(this.TabPage2);
            this.TabControl1.Controls.Add(this.TabPage3);
            this.TabControl1.Location = new System.Drawing.Point(8, 38);
            this.TabControl1.Name = "TabControl1";
            this.TabControl1.SelectedIndex = 0;
            this.TabControl1.Size = new System.Drawing.Size(857, 493);
            this.TabControl1.TabIndex = 22;
            // 
            // TabPage1
            // 
            this.TabPage1.Controls.Add(this.btnColorCalReloadCalRules);
            this.TabPage1.Controls.Add(this.bntColorCalUseSampleRuleColor);
            this.TabPage1.Controls.Add(this.btnColorCalUseSampleRuleMono);
            this.TabPage1.Controls.Add(this.ColorCalDataGridView2);
            this.TabPage1.Controls.Add(this.Label1);
            this.TabPage1.Controls.Add(this.ColorCalDataGridView1);
            this.TabPage1.Controls.Add(this.btnColorCalSaveCalRules);
            this.TabPage1.Controls.Add(this.Label2);
            this.TabPage1.Location = new System.Drawing.Point(4, 22);
            this.TabPage1.Name = "TabPage1";
            this.TabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage1.Size = new System.Drawing.Size(849, 467);
            this.TabPage1.TabIndex = 0;
            this.TabPage1.Text = "Color Calibration";
            this.TabPage1.UseVisualStyleBackColor = true;
            // 
            // btnColorCalReloadCalRules
            // 
            this.btnColorCalReloadCalRules.Location = new System.Drawing.Point(764, 28);
            this.btnColorCalReloadCalRules.Name = "btnColorCalReloadCalRules";
            this.btnColorCalReloadCalRules.Size = new System.Drawing.Size(73, 23);
            this.btnColorCalReloadCalRules.TabIndex = 41;
            this.btnColorCalReloadCalRules.Text = "Reload";
            this.btnColorCalReloadCalRules.UseVisualStyleBackColor = true;
            this.btnColorCalReloadCalRules.Click += new System.EventHandler(this.btnColorCalReloadCalRules_Click);
            // 
            // bntColorCalUseSampleRuleColor
            // 
            this.bntColorCalUseSampleRuleColor.Location = new System.Drawing.Point(558, 28);
            this.bntColorCalUseSampleRuleColor.Name = "bntColorCalUseSampleRuleColor";
            this.bntColorCalUseSampleRuleColor.Size = new System.Drawing.Size(132, 23);
            this.bntColorCalUseSampleRuleColor.TabIndex = 40;
            this.bntColorCalUseSampleRuleColor.Text = "Use Sample Rule Color";
            this.bntColorCalUseSampleRuleColor.UseVisualStyleBackColor = true;
            this.bntColorCalUseSampleRuleColor.Click += new System.EventHandler(this.bntColorCalUseSampleRuleColor_Click);
            // 
            // btnColorCalUseSampleRuleMono
            // 
            this.btnColorCalUseSampleRuleMono.Location = new System.Drawing.Point(420, 28);
            this.btnColorCalUseSampleRuleMono.Name = "btnColorCalUseSampleRuleMono";
            this.btnColorCalUseSampleRuleMono.Size = new System.Drawing.Size(132, 23);
            this.btnColorCalUseSampleRuleMono.TabIndex = 39;
            this.btnColorCalUseSampleRuleMono.Text = "Use Sample Rule Mono";
            this.btnColorCalUseSampleRuleMono.UseVisualStyleBackColor = true;
            this.btnColorCalUseSampleRuleMono.Click += new System.EventHandler(this.btnColorCalUseSampleRuleMono_Click);
            // 
            // ColorCalDataGridView2
            // 
            this.ColorCalDataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ColorCalDataGridView2.Location = new System.Drawing.Point(420, 57);
            this.ColorCalDataGridView2.Name = "ColorCalDataGridView2";
            this.ColorCalDataGridView2.Size = new System.Drawing.Size(417, 402);
            this.ColorCalDataGridView2.TabIndex = 38;
            // 
            // Label1
            // 
            this.Label1.AutoSize = true;
            this.Label1.Location = new System.Drawing.Point(12, 7);
            this.Label1.Name = "Label1";
            this.Label1.Size = new System.Drawing.Size(114, 13);
            this.Label1.TabIndex = 37;
            this.Label1.Text = "Calibration References";
            // 
            // ColorCalDataGridView1
            // 
            this.ColorCalDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ColorCalDataGridView1.Location = new System.Drawing.Point(12, 57);
            this.ColorCalDataGridView1.Name = "ColorCalDataGridView1";
            this.ColorCalDataGridView1.Size = new System.Drawing.Size(402, 402);
            this.ColorCalDataGridView1.TabIndex = 36;
            // 
            // btnColorCalSaveCalRules
            // 
            this.btnColorCalSaveCalRules.Location = new System.Drawing.Point(696, 28);
            this.btnColorCalSaveCalRules.Name = "btnColorCalSaveCalRules";
            this.btnColorCalSaveCalRules.Size = new System.Drawing.Size(62, 23);
            this.btnColorCalSaveCalRules.TabIndex = 35;
            this.btnColorCalSaveCalRules.Text = "Save Calibration Rules";
            this.btnColorCalSaveCalRules.UseVisualStyleBackColor = true;
            this.btnColorCalSaveCalRules.Click += new System.EventHandler(this.btnColorCalSaveCalRules_Click);
            // 
            // Label2
            // 
            this.Label2.AutoSize = true;
            this.Label2.Location = new System.Drawing.Point(421, 7);
            this.Label2.Name = "Label2";
            this.Label2.Size = new System.Drawing.Size(175, 13);
            this.Label2.TabIndex = 34;
            this.Label2.Text = "Calibration Rule (Step,CalibrationID)";
            // 
            // TabPage2
            // 
            this.TabPage2.Controls.Add(this.btnFlatFieldCalReloadCalRules);
            this.TabPage2.Controls.Add(this.bntFlatFieldCalUseSampleRuleColor);
            this.TabPage2.Controls.Add(this.btnFlatFieldCalUseSampleRuleMono);
            this.TabPage2.Controls.Add(this.FlatFieldCalDataGridView2);
            this.TabPage2.Controls.Add(this.Label3);
            this.TabPage2.Controls.Add(this.FlatFieldCalDataGridView1);
            this.TabPage2.Controls.Add(this.btnFlatFieldCalSaveCalRules);
            this.TabPage2.Controls.Add(this.Label4);
            this.TabPage2.Location = new System.Drawing.Point(4, 22);
            this.TabPage2.Name = "TabPage2";
            this.TabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage2.Size = new System.Drawing.Size(849, 467);
            this.TabPage2.TabIndex = 1;
            this.TabPage2.Text = "Flat Field Calibration";
            this.TabPage2.UseVisualStyleBackColor = true;
            // 
            // btnFlatFieldCalReloadCalRules
            // 
            this.btnFlatFieldCalReloadCalRules.Location = new System.Drawing.Point(764, 28);
            this.btnFlatFieldCalReloadCalRules.Name = "btnFlatFieldCalReloadCalRules";
            this.btnFlatFieldCalReloadCalRules.Size = new System.Drawing.Size(73, 23);
            this.btnFlatFieldCalReloadCalRules.TabIndex = 49;
            this.btnFlatFieldCalReloadCalRules.Text = "Reload";
            this.btnFlatFieldCalReloadCalRules.UseVisualStyleBackColor = true;
            this.btnFlatFieldCalReloadCalRules.Click += new System.EventHandler(this.btnFlatFieldCalReloadCalRules_Click);
            // 
            // bntFlatFieldCalUseSampleRuleColor
            // 
            this.bntFlatFieldCalUseSampleRuleColor.Location = new System.Drawing.Point(558, 28);
            this.bntFlatFieldCalUseSampleRuleColor.Name = "bntFlatFieldCalUseSampleRuleColor";
            this.bntFlatFieldCalUseSampleRuleColor.Size = new System.Drawing.Size(132, 23);
            this.bntFlatFieldCalUseSampleRuleColor.TabIndex = 48;
            this.bntFlatFieldCalUseSampleRuleColor.Text = "Use Sample Rule Color";
            this.bntFlatFieldCalUseSampleRuleColor.UseVisualStyleBackColor = true;
            this.bntFlatFieldCalUseSampleRuleColor.Click += new System.EventHandler(this.bntFlatFieldCalUseSampleRuleColor_Click);
            // 
            // btnFlatFieldCalUseSampleRuleMono
            // 
            this.btnFlatFieldCalUseSampleRuleMono.Location = new System.Drawing.Point(420, 28);
            this.btnFlatFieldCalUseSampleRuleMono.Name = "btnFlatFieldCalUseSampleRuleMono";
            this.btnFlatFieldCalUseSampleRuleMono.Size = new System.Drawing.Size(132, 23);
            this.btnFlatFieldCalUseSampleRuleMono.TabIndex = 47;
            this.btnFlatFieldCalUseSampleRuleMono.Text = "Use Sample Rule Mono";
            this.btnFlatFieldCalUseSampleRuleMono.UseVisualStyleBackColor = true;
            this.btnFlatFieldCalUseSampleRuleMono.Click += new System.EventHandler(this.btnFlatFieldCalUseSampleRuleMono_Click);
            // 
            // FlatFieldCalDataGridView2
            // 
            this.FlatFieldCalDataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.FlatFieldCalDataGridView2.Location = new System.Drawing.Point(420, 57);
            this.FlatFieldCalDataGridView2.Name = "FlatFieldCalDataGridView2";
            this.FlatFieldCalDataGridView2.Size = new System.Drawing.Size(417, 402);
            this.FlatFieldCalDataGridView2.TabIndex = 46;
            // 
            // Label3
            // 
            this.Label3.AutoSize = true;
            this.Label3.Location = new System.Drawing.Point(12, 7);
            this.Label3.Name = "Label3";
            this.Label3.Size = new System.Drawing.Size(114, 13);
            this.Label3.TabIndex = 45;
            this.Label3.Text = "Calibration References";
            // 
            // FlatFieldCalDataGridView1
            // 
            this.FlatFieldCalDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.FlatFieldCalDataGridView1.Location = new System.Drawing.Point(12, 57);
            this.FlatFieldCalDataGridView1.Name = "FlatFieldCalDataGridView1";
            this.FlatFieldCalDataGridView1.Size = new System.Drawing.Size(402, 402);
            this.FlatFieldCalDataGridView1.TabIndex = 44;
            // 
            // btnFlatFieldCalSaveCalRules
            // 
            this.btnFlatFieldCalSaveCalRules.Location = new System.Drawing.Point(696, 28);
            this.btnFlatFieldCalSaveCalRules.Name = "btnFlatFieldCalSaveCalRules";
            this.btnFlatFieldCalSaveCalRules.Size = new System.Drawing.Size(62, 23);
            this.btnFlatFieldCalSaveCalRules.TabIndex = 43;
            this.btnFlatFieldCalSaveCalRules.Text = "Save Calibration Rules";
            this.btnFlatFieldCalSaveCalRules.UseVisualStyleBackColor = true;
            this.btnFlatFieldCalSaveCalRules.Click += new System.EventHandler(this.btnFlatFieldCalSaveCalRules_Click);
            // 
            // Label4
            // 
            this.Label4.AutoSize = true;
            this.Label4.Location = new System.Drawing.Point(421, 7);
            this.Label4.Name = "Label4";
            this.Label4.Size = new System.Drawing.Size(175, 13);
            this.Label4.TabIndex = 42;
            this.Label4.Text = "Calibration Rule (Step,CalibrationID)";
            // 
            // TabPage3
            // 
            this.TabPage3.Controls.Add(this.btnImgScaleCalReloadCalRules);
            this.TabPage3.Controls.Add(this.bntImgScaleCalUseSampleRuleColor);
            this.TabPage3.Controls.Add(this.btnImgScaleCalUseSampleRuleMono);
            this.TabPage3.Controls.Add(this.ImgScaleCalDataGridView2);
            this.TabPage3.Controls.Add(this.Label6);
            this.TabPage3.Controls.Add(this.ImgScaleCalDataGridView1);
            this.TabPage3.Controls.Add(this.btnImgScaleCalSaveCalRules);
            this.TabPage3.Controls.Add(this.Label7);
            this.TabPage3.Location = new System.Drawing.Point(4, 22);
            this.TabPage3.Name = "TabPage3";
            this.TabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.TabPage3.Size = new System.Drawing.Size(849, 467);
            this.TabPage3.TabIndex = 2;
            this.TabPage3.Text = "Image Scaling Calibration";
            this.TabPage3.UseVisualStyleBackColor = true;
            // 
            // btnImgScaleCalReloadCalRules
            // 
            this.btnImgScaleCalReloadCalRules.Location = new System.Drawing.Point(764, 28);
            this.btnImgScaleCalReloadCalRules.Name = "btnImgScaleCalReloadCalRules";
            this.btnImgScaleCalReloadCalRules.Size = new System.Drawing.Size(73, 23);
            this.btnImgScaleCalReloadCalRules.TabIndex = 49;
            this.btnImgScaleCalReloadCalRules.Text = "Reload";
            this.btnImgScaleCalReloadCalRules.UseVisualStyleBackColor = true;
            this.btnImgScaleCalReloadCalRules.Click += new System.EventHandler(this.btnImgScaleCalReloadCalRules_Click);
            // 
            // bntImgScaleCalUseSampleRuleColor
            // 
            this.bntImgScaleCalUseSampleRuleColor.Location = new System.Drawing.Point(558, 28);
            this.bntImgScaleCalUseSampleRuleColor.Name = "bntImgScaleCalUseSampleRuleColor";
            this.bntImgScaleCalUseSampleRuleColor.Size = new System.Drawing.Size(132, 23);
            this.bntImgScaleCalUseSampleRuleColor.TabIndex = 48;
            this.bntImgScaleCalUseSampleRuleColor.Text = "Use Sample Rule Color";
            this.bntImgScaleCalUseSampleRuleColor.UseVisualStyleBackColor = true;
            this.bntImgScaleCalUseSampleRuleColor.Click += new System.EventHandler(this.bntImgScaleCalUseSampleRuleColor_Click);
            // 
            // btnImgScaleCalUseSampleRuleMono
            // 
            this.btnImgScaleCalUseSampleRuleMono.Location = new System.Drawing.Point(420, 28);
            this.btnImgScaleCalUseSampleRuleMono.Name = "btnImgScaleCalUseSampleRuleMono";
            this.btnImgScaleCalUseSampleRuleMono.Size = new System.Drawing.Size(132, 23);
            this.btnImgScaleCalUseSampleRuleMono.TabIndex = 47;
            this.btnImgScaleCalUseSampleRuleMono.Text = "Use Sample Rule Mono";
            this.btnImgScaleCalUseSampleRuleMono.UseVisualStyleBackColor = true;
            this.btnImgScaleCalUseSampleRuleMono.Click += new System.EventHandler(this.btnImgScaleCalUseSampleRuleMono_Click);
            // 
            // ImgScaleCalDataGridView2
            // 
            this.ImgScaleCalDataGridView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ImgScaleCalDataGridView2.Location = new System.Drawing.Point(420, 57);
            this.ImgScaleCalDataGridView2.Name = "ImgScaleCalDataGridView2";
            this.ImgScaleCalDataGridView2.Size = new System.Drawing.Size(417, 402);
            this.ImgScaleCalDataGridView2.TabIndex = 46;
            // 
            // Label6
            // 
            this.Label6.AutoSize = true;
            this.Label6.Location = new System.Drawing.Point(12, 7);
            this.Label6.Name = "Label6";
            this.Label6.Size = new System.Drawing.Size(114, 13);
            this.Label6.TabIndex = 45;
            this.Label6.Text = "Calibration References";
            // 
            // ImgScaleCalDataGridView1
            // 
            this.ImgScaleCalDataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ImgScaleCalDataGridView1.Location = new System.Drawing.Point(12, 57);
            this.ImgScaleCalDataGridView1.Name = "ImgScaleCalDataGridView1";
            this.ImgScaleCalDataGridView1.Size = new System.Drawing.Size(402, 402);
            this.ImgScaleCalDataGridView1.TabIndex = 44;
            // 
            // btnImgScaleCalSaveCalRules
            // 
            this.btnImgScaleCalSaveCalRules.Location = new System.Drawing.Point(696, 28);
            this.btnImgScaleCalSaveCalRules.Name = "btnImgScaleCalSaveCalRules";
            this.btnImgScaleCalSaveCalRules.Size = new System.Drawing.Size(62, 23);
            this.btnImgScaleCalSaveCalRules.TabIndex = 43;
            this.btnImgScaleCalSaveCalRules.Text = "Save Calibration Rules";
            this.btnImgScaleCalSaveCalRules.UseVisualStyleBackColor = true;
            this.btnImgScaleCalSaveCalRules.Click += new System.EventHandler(this.btnImgScaleCalSaveCalRules_Click);
            // 
            // Label7
            // 
            this.Label7.AutoSize = true;
            this.Label7.Location = new System.Drawing.Point(421, 7);
            this.Label7.Name = "Label7";
            this.Label7.Size = new System.Drawing.Size(175, 13);
            this.Label7.TabIndex = 42;
            this.Label7.Text = "Calibration Rule (Step,CalibrationID)";
            // 
            // CalRule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(877, 543);
            this.Controls.Add(this.TabControl1);
            this.Controls.Add(this.btnUseLastModified3);
            this.Controls.Add(this.Label5);
            this.Controls.Add(this.txtFile3);
            this.Controls.Add(this.btnBrowse3);
            this.Name = "CalRule";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Calibration Rules";
            this.Load += new System.EventHandler(this.CalRule_Load);
            this.TabControl1.ResumeLayout(false);
            this.TabPage1.ResumeLayout(false);
            this.TabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ColorCalDataGridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ColorCalDataGridView1)).EndInit();
            this.TabPage2.ResumeLayout(false);
            this.TabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.FlatFieldCalDataGridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FlatFieldCalDataGridView1)).EndInit();
            this.TabPage3.ResumeLayout(false);
            this.TabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ImgScaleCalDataGridView2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ImgScaleCalDataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        internal Button btnUseLastModified3;
        internal Label Label5;
        internal TextBox txtFile3;
        internal Button btnBrowse3;
        internal TabControl TabControl1;
        internal TabPage TabPage1;
        internal Button btnColorCalReloadCalRules;
        internal Button bntColorCalUseSampleRuleColor;
        internal Button btnColorCalUseSampleRuleMono;
        internal DataGridView ColorCalDataGridView2;
        internal Label Label1;
        internal DataGridView ColorCalDataGridView1;
        internal Button btnColorCalSaveCalRules;
        internal Label Label2;
        internal TabPage TabPage2;
        internal TabPage TabPage3;
        internal Button btnFlatFieldCalReloadCalRules;
        internal Button bntFlatFieldCalUseSampleRuleColor;
        internal Button btnFlatFieldCalUseSampleRuleMono;
        internal DataGridView FlatFieldCalDataGridView2;
        internal Label Label3;
        internal DataGridView FlatFieldCalDataGridView1;
        internal Button btnFlatFieldCalSaveCalRules;
        internal Label Label4;
        internal Button btnImgScaleCalReloadCalRules;
        internal Button bntImgScaleCalUseSampleRuleColor;
        internal Button btnImgScaleCalUseSampleRuleMono;
        internal DataGridView ImgScaleCalDataGridView2;
        internal Label Label6;
        internal DataGridView ImgScaleCalDataGridView1;
        internal Button btnImgScaleCalSaveCalRules;
        internal Label Label7;
    }
}