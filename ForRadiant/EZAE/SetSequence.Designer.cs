
namespace EZAE
{
    partial class SetSequence
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
            this.lblSubframe = new System.Windows.Forms.Label();
            this.subframeBox = new System.Windows.Forms.ComboBox();
            this.btnAddSubframe = new System.Windows.Forms.Button();
            this.btnDelSubframe = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblSubframe
            // 
            this.lblSubframe.AutoSize = true;
            this.lblSubframe.Location = new System.Drawing.Point(12, 9);
            this.lblSubframe.Name = "lblSubframe";
            this.lblSubframe.Size = new System.Drawing.Size(52, 13);
            this.lblSubframe.TabIndex = 0;
            this.lblSubframe.Text = "Subframe";
            // 
            // subframeBox
            // 
            this.subframeBox.FormattingEnabled = true;
            this.subframeBox.Location = new System.Drawing.Point(70, 6);
            this.subframeBox.Name = "subframeBox";
            this.subframeBox.Size = new System.Drawing.Size(152, 21);
            this.subframeBox.TabIndex = 1;
            // 
            // btnAddSubframe
            // 
            this.btnAddSubframe.Location = new System.Drawing.Point(228, 4);
            this.btnAddSubframe.Name = "btnAddSubframe";
            this.btnAddSubframe.Size = new System.Drawing.Size(37, 23);
            this.btnAddSubframe.TabIndex = 2;
            this.btnAddSubframe.Text = "Add";
            this.btnAddSubframe.UseVisualStyleBackColor = true;
            this.btnAddSubframe.Click += new System.EventHandler(this.btnAddSubframe_Click);
            // 
            // btnDelSubframe
            // 
            this.btnDelSubframe.Location = new System.Drawing.Point(271, 4);
            this.btnDelSubframe.Name = "btnDelSubframe";
            this.btnDelSubframe.Size = new System.Drawing.Size(37, 23);
            this.btnDelSubframe.TabIndex = 2;
            this.btnDelSubframe.Text = "Del";
            this.btnDelSubframe.UseVisualStyleBackColor = true;
            this.btnDelSubframe.Click += new System.EventHandler(this.btnDelSubframe_Click);
            // 
            // SetSequence
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnDelSubframe);
            this.Controls.Add(this.btnAddSubframe);
            this.Controls.Add(this.subframeBox);
            this.Controls.Add(this.lblSubframe);
            this.Name = "SetSequence";
            this.Text = "SetSequence";
            this.Load += new System.EventHandler(this.FormLoad);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblSubframe;
        private System.Windows.Forms.ComboBox subframeBox;
        private System.Windows.Forms.Button btnAddSubframe;
        private System.Windows.Forms.Button btnDelSubframe;
    }
}