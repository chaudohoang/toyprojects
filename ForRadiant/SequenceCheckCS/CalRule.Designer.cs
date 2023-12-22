using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace SequenceCheckCS
{
    [Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
    public partial class CalRule : Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is not null)
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
            btnUseLastModified3 = new Button();
            btnUseLastModified3.Click += new EventHandler(btnUseLastModified3_Click);
            Label5 = new Label();
            txtFile3 = new TextBox();
            txtFile3.TextChanged += new EventHandler(txtFile3_TextChanged);
            btnBrowse3 = new Button();
            btnBrowse3.Click += new EventHandler(btnBrowse3_Click);
            TabControl1 = new TabControl();
            TabPage1 = new TabPage();
            btnColorCalReloadCalRules = new Button();
            btnColorCalReloadCalRules.Click += new EventHandler(btnColorCalReloadCalRules_Click);
            bntColorCalUseSampleRuleColor = new Button();
            bntColorCalUseSampleRuleColor.Click += new EventHandler(bntColorCalUseSampleRuleColor_Click);
            btnColorCalUseSampleRuleMono = new Button();
            btnColorCalUseSampleRuleMono.Click += new EventHandler(btnColorCalUseSampleRuleMono_Click);
            ColorCalDataGridView2 = new DataGridView();
            Label1 = new Label();
            ColorCalDataGridView1 = new DataGridView();
            btnColorCalSaveCalRules = new Button();
            btnColorCalSaveCalRules.Click += new EventHandler(btnColorCalSaveCalRules_Click);
            Label2 = new Label();
            TabPage2 = new TabPage();
            btnFlatFieldCalReloadCalRules = new Button();
            btnFlatFieldCalReloadCalRules.Click += new EventHandler(btnFlatFieldCalReloadCalRules_Click);
            bntFlatFieldCalUseSampleRuleColor = new Button();
            bntFlatFieldCalUseSampleRuleColor.Click += new EventHandler(bntFlatFieldCalUseSampleRuleColor_Click);
            btnFlatFieldCalUseSampleRuleMono = new Button();
            btnFlatFieldCalUseSampleRuleMono.Click += new EventHandler(btnFlatFieldCalUseSampleRuleMono_Click);
            FlatFieldCalDataGridView2 = new DataGridView();
            Label3 = new Label();
            FlatFieldCalDataGridView1 = new DataGridView();
            btnFlatFieldCalSaveCalRules = new Button();
            btnFlatFieldCalSaveCalRules.Click += new EventHandler(btnFlatFieldCalSaveCalRules_Click);
            Label4 = new Label();
            TabPage3 = new TabPage();
            btnImgScaleCalReloadCalRules = new Button();
            btnImgScaleCalReloadCalRules.Click += new EventHandler(btnImgScaleCalReloadCalRules_Click);
            bntImgScaleCalUseSampleRuleColor = new Button();
            bntImgScaleCalUseSampleRuleColor.Click += new EventHandler(bntImgScaleCalUseSampleRuleColor_Click);
            btnImgScaleCalUseSampleRuleMono = new Button();
            btnImgScaleCalUseSampleRuleMono.Click += new EventHandler(btnImgScaleCalUseSampleRuleMono_Click);
            ImgScaleCalDataGridView2 = new DataGridView();
            Label6 = new Label();
            ImgScaleCalDataGridView1 = new DataGridView();
            btnImgScaleCalSaveCalRules = new Button();
            btnImgScaleCalSaveCalRules.Click += new EventHandler(btnImgScaleCalSaveCalRules_Click);
            Label7 = new Label();
            TabControl1.SuspendLayout();
            TabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ColorCalDataGridView2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ColorCalDataGridView1).BeginInit();
            TabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)FlatFieldCalDataGridView2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)FlatFieldCalDataGridView1).BeginInit();
            TabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)ImgScaleCalDataGridView2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)ImgScaleCalDataGridView1).BeginInit();
            SuspendLayout();
            // 
            // btnUseLastModified3
            // 
            btnUseLastModified3.Location = new Point(671, 10);
            btnUseLastModified3.Name = "btnUseLastModified3";
            btnUseLastModified3.Size = new Size(87, 23);
            btnUseLastModified3.TabIndex = 21;
            btnUseLastModified3.Text = "Last Modified";
            btnUseLastModified3.UseVisualStyleBackColor = true;
            // 
            // Label5
            // 
            Label5.AutoSize = true;
            Label5.Location = new Point(5, 15);
            Label5.Name = "Label5";
            Label5.Size = new Size(56, 13);
            Label5.TabIndex = 18;
            Label5.Text = "Sequence";
            // 
            // txtFile3
            // 
            txtFile3.AllowDrop = true;
            txtFile3.Location = new Point(76, 12);
            txtFile3.Name = "txtFile3";
            txtFile3.ScrollBars = ScrollBars.Vertical;
            txtFile3.Size = new Size(589, 20);
            txtFile3.TabIndex = 19;
            // 
            // btnBrowse3
            // 
            btnBrowse3.Location = new Point(764, 10);
            btnBrowse3.Name = "btnBrowse3";
            btnBrowse3.Size = new Size(75, 23);
            btnBrowse3.TabIndex = 20;
            btnBrowse3.Text = "Browse";
            btnBrowse3.UseVisualStyleBackColor = true;
            // 
            // TabControl1
            // 
            TabControl1.Controls.Add(TabPage1);
            TabControl1.Controls.Add(TabPage2);
            TabControl1.Controls.Add(TabPage3);
            TabControl1.Location = new Point(8, 38);
            TabControl1.Name = "TabControl1";
            TabControl1.SelectedIndex = 0;
            TabControl1.Size = new Size(857, 493);
            TabControl1.TabIndex = 22;
            // 
            // TabPage1
            // 
            TabPage1.Controls.Add(btnColorCalReloadCalRules);
            TabPage1.Controls.Add(bntColorCalUseSampleRuleColor);
            TabPage1.Controls.Add(btnColorCalUseSampleRuleMono);
            TabPage1.Controls.Add(ColorCalDataGridView2);
            TabPage1.Controls.Add(Label1);
            TabPage1.Controls.Add(ColorCalDataGridView1);
            TabPage1.Controls.Add(btnColorCalSaveCalRules);
            TabPage1.Controls.Add(Label2);
            TabPage1.Location = new Point(4, 22);
            TabPage1.Name = "TabPage1";
            TabPage1.Padding = new Padding(3);
            TabPage1.Size = new Size(849, 467);
            TabPage1.TabIndex = 0;
            TabPage1.Text = "Color Calibration";
            TabPage1.UseVisualStyleBackColor = true;
            // 
            // btnColorCalReloadCalRules
            // 
            btnColorCalReloadCalRules.Location = new Point(764, 28);
            btnColorCalReloadCalRules.Name = "btnColorCalReloadCalRules";
            btnColorCalReloadCalRules.Size = new Size(73, 23);
            btnColorCalReloadCalRules.TabIndex = 41;
            btnColorCalReloadCalRules.Text = "Reload";
            btnColorCalReloadCalRules.UseVisualStyleBackColor = true;
            // 
            // bntColorCalUseSampleRuleColor
            // 
            bntColorCalUseSampleRuleColor.Location = new Point(558, 28);
            bntColorCalUseSampleRuleColor.Name = "bntColorCalUseSampleRuleColor";
            bntColorCalUseSampleRuleColor.Size = new Size(132, 23);
            bntColorCalUseSampleRuleColor.TabIndex = 40;
            bntColorCalUseSampleRuleColor.Text = "Use Sample Rule Color";
            bntColorCalUseSampleRuleColor.UseVisualStyleBackColor = true;
            // 
            // btnColorCalUseSampleRuleMono
            // 
            btnColorCalUseSampleRuleMono.Location = new Point(420, 28);
            btnColorCalUseSampleRuleMono.Name = "btnColorCalUseSampleRuleMono";
            btnColorCalUseSampleRuleMono.Size = new Size(132, 23);
            btnColorCalUseSampleRuleMono.TabIndex = 39;
            btnColorCalUseSampleRuleMono.Text = "Use Sample Rule Mono";
            btnColorCalUseSampleRuleMono.UseVisualStyleBackColor = true;
            // 
            // ColorCalDataGridView2
            // 
            ColorCalDataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ColorCalDataGridView2.Location = new Point(420, 57);
            ColorCalDataGridView2.Name = "ColorCalDataGridView2";
            ColorCalDataGridView2.Size = new Size(417, 402);
            ColorCalDataGridView2.TabIndex = 38;
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Location = new Point(12, 7);
            Label1.Name = "Label1";
            Label1.Size = new Size(114, 13);
            Label1.TabIndex = 37;
            Label1.Text = "Calibration References";
            // 
            // ColorCalDataGridView1
            // 
            ColorCalDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ColorCalDataGridView1.Location = new Point(12, 57);
            ColorCalDataGridView1.Name = "ColorCalDataGridView1";
            ColorCalDataGridView1.Size = new Size(402, 402);
            ColorCalDataGridView1.TabIndex = 36;
            // 
            // btnColorCalSaveCalRules
            // 
            btnColorCalSaveCalRules.Location = new Point(696, 28);
            btnColorCalSaveCalRules.Name = "btnColorCalSaveCalRules";
            btnColorCalSaveCalRules.Size = new Size(62, 23);
            btnColorCalSaveCalRules.TabIndex = 35;
            btnColorCalSaveCalRules.Text = "Save Calibration Rules";
            btnColorCalSaveCalRules.UseVisualStyleBackColor = true;
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Location = new Point(421, 7);
            Label2.Name = "Label2";
            Label2.Size = new Size(175, 13);
            Label2.TabIndex = 34;
            Label2.Text = "Calibration Rule (Step,CalibrationID)";
            // 
            // TabPage2
            // 
            TabPage2.Controls.Add(btnFlatFieldCalReloadCalRules);
            TabPage2.Controls.Add(bntFlatFieldCalUseSampleRuleColor);
            TabPage2.Controls.Add(btnFlatFieldCalUseSampleRuleMono);
            TabPage2.Controls.Add(FlatFieldCalDataGridView2);
            TabPage2.Controls.Add(Label3);
            TabPage2.Controls.Add(FlatFieldCalDataGridView1);
            TabPage2.Controls.Add(btnFlatFieldCalSaveCalRules);
            TabPage2.Controls.Add(Label4);
            TabPage2.Location = new Point(4, 22);
            TabPage2.Name = "TabPage2";
            TabPage2.Padding = new Padding(3);
            TabPage2.Size = new Size(849, 467);
            TabPage2.TabIndex = 1;
            TabPage2.Text = "Flat Field Calibration";
            TabPage2.UseVisualStyleBackColor = true;
            // 
            // btnFlatFieldCalReloadCalRules
            // 
            btnFlatFieldCalReloadCalRules.Location = new Point(764, 28);
            btnFlatFieldCalReloadCalRules.Name = "btnFlatFieldCalReloadCalRules";
            btnFlatFieldCalReloadCalRules.Size = new Size(73, 23);
            btnFlatFieldCalReloadCalRules.TabIndex = 49;
            btnFlatFieldCalReloadCalRules.Text = "Reload";
            btnFlatFieldCalReloadCalRules.UseVisualStyleBackColor = true;
            // 
            // bntFlatFieldCalUseSampleRuleColor
            // 
            bntFlatFieldCalUseSampleRuleColor.Location = new Point(558, 28);
            bntFlatFieldCalUseSampleRuleColor.Name = "bntFlatFieldCalUseSampleRuleColor";
            bntFlatFieldCalUseSampleRuleColor.Size = new Size(132, 23);
            bntFlatFieldCalUseSampleRuleColor.TabIndex = 48;
            bntFlatFieldCalUseSampleRuleColor.Text = "Use Sample Rule Color";
            bntFlatFieldCalUseSampleRuleColor.UseVisualStyleBackColor = true;
            // 
            // btnFlatFieldCalUseSampleRuleMono
            // 
            btnFlatFieldCalUseSampleRuleMono.Location = new Point(420, 28);
            btnFlatFieldCalUseSampleRuleMono.Name = "btnFlatFieldCalUseSampleRuleMono";
            btnFlatFieldCalUseSampleRuleMono.Size = new Size(132, 23);
            btnFlatFieldCalUseSampleRuleMono.TabIndex = 47;
            btnFlatFieldCalUseSampleRuleMono.Text = "Use Sample Rule Mono";
            btnFlatFieldCalUseSampleRuleMono.UseVisualStyleBackColor = true;
            // 
            // FlatFieldCalDataGridView2
            // 
            FlatFieldCalDataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            FlatFieldCalDataGridView2.Location = new Point(420, 57);
            FlatFieldCalDataGridView2.Name = "FlatFieldCalDataGridView2";
            FlatFieldCalDataGridView2.Size = new Size(417, 402);
            FlatFieldCalDataGridView2.TabIndex = 46;
            // 
            // Label3
            // 
            Label3.AutoSize = true;
            Label3.Location = new Point(12, 7);
            Label3.Name = "Label3";
            Label3.Size = new Size(114, 13);
            Label3.TabIndex = 45;
            Label3.Text = "Calibration References";
            // 
            // FlatFieldCalDataGridView1
            // 
            FlatFieldCalDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            FlatFieldCalDataGridView1.Location = new Point(12, 57);
            FlatFieldCalDataGridView1.Name = "FlatFieldCalDataGridView1";
            FlatFieldCalDataGridView1.Size = new Size(402, 402);
            FlatFieldCalDataGridView1.TabIndex = 44;
            // 
            // btnFlatFieldCalSaveCalRules
            // 
            btnFlatFieldCalSaveCalRules.Location = new Point(696, 28);
            btnFlatFieldCalSaveCalRules.Name = "btnFlatFieldCalSaveCalRules";
            btnFlatFieldCalSaveCalRules.Size = new Size(62, 23);
            btnFlatFieldCalSaveCalRules.TabIndex = 43;
            btnFlatFieldCalSaveCalRules.Text = "Save Calibration Rules";
            btnFlatFieldCalSaveCalRules.UseVisualStyleBackColor = true;
            // 
            // Label4
            // 
            Label4.AutoSize = true;
            Label4.Location = new Point(421, 7);
            Label4.Name = "Label4";
            Label4.Size = new Size(175, 13);
            Label4.TabIndex = 42;
            Label4.Text = "Calibration Rule (Step,CalibrationID)";
            // 
            // TabPage3
            // 
            TabPage3.Controls.Add(btnImgScaleCalReloadCalRules);
            TabPage3.Controls.Add(bntImgScaleCalUseSampleRuleColor);
            TabPage3.Controls.Add(btnImgScaleCalUseSampleRuleMono);
            TabPage3.Controls.Add(ImgScaleCalDataGridView2);
            TabPage3.Controls.Add(Label6);
            TabPage3.Controls.Add(ImgScaleCalDataGridView1);
            TabPage3.Controls.Add(btnImgScaleCalSaveCalRules);
            TabPage3.Controls.Add(Label7);
            TabPage3.Location = new Point(4, 22);
            TabPage3.Name = "TabPage3";
            TabPage3.Padding = new Padding(3);
            TabPage3.Size = new Size(849, 467);
            TabPage3.TabIndex = 2;
            TabPage3.Text = "Image Scaling Calibration";
            TabPage3.UseVisualStyleBackColor = true;
            // 
            // btnImgScaleCalReloadCalRules
            // 
            btnImgScaleCalReloadCalRules.Location = new Point(764, 28);
            btnImgScaleCalReloadCalRules.Name = "btnImgScaleCalReloadCalRules";
            btnImgScaleCalReloadCalRules.Size = new Size(73, 23);
            btnImgScaleCalReloadCalRules.TabIndex = 49;
            btnImgScaleCalReloadCalRules.Text = "Reload";
            btnImgScaleCalReloadCalRules.UseVisualStyleBackColor = true;
            // 
            // bntImgScaleCalUseSampleRuleColor
            // 
            bntImgScaleCalUseSampleRuleColor.Location = new Point(558, 28);
            bntImgScaleCalUseSampleRuleColor.Name = "bntImgScaleCalUseSampleRuleColor";
            bntImgScaleCalUseSampleRuleColor.Size = new Size(132, 23);
            bntImgScaleCalUseSampleRuleColor.TabIndex = 48;
            bntImgScaleCalUseSampleRuleColor.Text = "Use Sample Rule Color";
            bntImgScaleCalUseSampleRuleColor.UseVisualStyleBackColor = true;
            // 
            // btnImgScaleCalUseSampleRuleMono
            // 
            btnImgScaleCalUseSampleRuleMono.Location = new Point(420, 28);
            btnImgScaleCalUseSampleRuleMono.Name = "btnImgScaleCalUseSampleRuleMono";
            btnImgScaleCalUseSampleRuleMono.Size = new Size(132, 23);
            btnImgScaleCalUseSampleRuleMono.TabIndex = 47;
            btnImgScaleCalUseSampleRuleMono.Text = "Use Sample Rule Mono";
            btnImgScaleCalUseSampleRuleMono.UseVisualStyleBackColor = true;
            // 
            // ImgScaleCalDataGridView2
            // 
            ImgScaleCalDataGridView2.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ImgScaleCalDataGridView2.Location = new Point(420, 57);
            ImgScaleCalDataGridView2.Name = "ImgScaleCalDataGridView2";
            ImgScaleCalDataGridView2.Size = new Size(417, 402);
            ImgScaleCalDataGridView2.TabIndex = 46;
            // 
            // Label6
            // 
            Label6.AutoSize = true;
            Label6.Location = new Point(12, 7);
            Label6.Name = "Label6";
            Label6.Size = new Size(114, 13);
            Label6.TabIndex = 45;
            Label6.Text = "Calibration References";
            // 
            // ImgScaleCalDataGridView1
            // 
            ImgScaleCalDataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            ImgScaleCalDataGridView1.Location = new Point(12, 57);
            ImgScaleCalDataGridView1.Name = "ImgScaleCalDataGridView1";
            ImgScaleCalDataGridView1.Size = new Size(402, 402);
            ImgScaleCalDataGridView1.TabIndex = 44;
            // 
            // btnImgScaleCalSaveCalRules
            // 
            btnImgScaleCalSaveCalRules.Location = new Point(696, 28);
            btnImgScaleCalSaveCalRules.Name = "btnImgScaleCalSaveCalRules";
            btnImgScaleCalSaveCalRules.Size = new Size(62, 23);
            btnImgScaleCalSaveCalRules.TabIndex = 43;
            btnImgScaleCalSaveCalRules.Text = "Save Calibration Rules";
            btnImgScaleCalSaveCalRules.UseVisualStyleBackColor = true;
            // 
            // Label7
            // 
            Label7.AutoSize = true;
            Label7.Location = new Point(421, 7);
            Label7.Name = "Label7";
            Label7.Size = new Size(175, 13);
            Label7.TabIndex = 42;
            Label7.Text = "Calibration Rule (Step,CalibrationID)";
            // 
            // CalRule
            // 
            AutoScaleDimensions = new SizeF(6.0f, 13.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(877, 543);
            Controls.Add(TabControl1);
            Controls.Add(btnUseLastModified3);
            Controls.Add(Label5);
            Controls.Add(txtFile3);
            Controls.Add(btnBrowse3);
            Name = "CalRule";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Calibration Rules";
            TabControl1.ResumeLayout(false);
            TabPage1.ResumeLayout(false);
            TabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ColorCalDataGridView2).EndInit();
            ((System.ComponentModel.ISupportInitialize)ColorCalDataGridView1).EndInit();
            TabPage2.ResumeLayout(false);
            TabPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)FlatFieldCalDataGridView2).EndInit();
            ((System.ComponentModel.ISupportInitialize)FlatFieldCalDataGridView1).EndInit();
            TabPage3.ResumeLayout(false);
            TabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)ImgScaleCalDataGridView2).EndInit();
            ((System.ComponentModel.ISupportInitialize)ImgScaleCalDataGridView1).EndInit();
            Load += new EventHandler(CalRule_Load);
            ResumeLayout(false);
            PerformLayout();

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