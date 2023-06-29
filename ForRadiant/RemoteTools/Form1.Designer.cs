namespace RemoteTools
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnFileShare = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.btnRemoteControl = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnPing = new System.Windows.Forms.Button();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(332, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // btnFileShare
            // 
            this.btnFileShare.Location = new System.Drawing.Point(137, 415);
            this.btnFileShare.Name = "btnFileShare";
            this.btnFileShare.Size = new System.Drawing.Size(119, 23);
            this.btnFileShare.TabIndex = 2;
            this.btnFileShare.Text = "File Share";
            this.btnFileShare.UseVisualStyleBackColor = true;
            this.btnFileShare.Click += new System.EventHandler(this.btnFileShare_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 38);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.Size = new System.Drawing.Size(367, 371);
            this.dataGridView1.TabIndex = 4;
            // 
            // btnRemoteControl
            // 
            this.btnRemoteControl.Location = new System.Drawing.Point(262, 415);
            this.btnRemoteControl.Name = "btnRemoteControl";
            this.btnRemoteControl.Size = new System.Drawing.Size(119, 23);
            this.btnRemoteControl.TabIndex = 5;
            this.btnRemoteControl.Text = "Remote Control";
            this.btnRemoteControl.UseVisualStyleBackColor = true;
            this.btnRemoteControl.Click += new System.EventHandler(this.btnRemoteControl_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(351, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(28, 20);
            this.button1.TabIndex = 6;
            this.button1.Text = "X";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnPing
            // 
            this.btnPing.Location = new System.Drawing.Point(12, 415);
            this.btnPing.Name = "btnPing";
            this.btnPing.Size = new System.Drawing.Size(119, 23);
            this.btnPing.TabIndex = 2;
            this.btnPing.Text = "Ping";
            this.btnPing.UseVisualStyleBackColor = true;
            this.btnPing.Click += new System.EventHandler(this.btnPing_Click);
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.SystemColors.Menu;
            this.richTextBox1.Location = new System.Drawing.Point(12, 444);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(367, 159);
            this.richTextBox1.TabIndex = 7;
            this.richTextBox1.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 612);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnRemoteControl);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnPing);
            this.Controls.Add(this.btnFileShare);
            this.Controls.Add(this.textBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Remote Tools";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnFileShare;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Button btnRemoteControl;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnPing;
        private System.Windows.Forms.RichTextBox richTextBox1;
    }
}

