using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileEncryptandDecrypt
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var OF = new OpenFileDialog
            {
                InitialDirectory = Application.StartupPath,
                Filter = "All files (*) | *.*"
            };

            if (OF.ShowDialog() == DialogResult.OK)
            {
                OF.FilterIndex = 0;
                OF.RestoreDirectory = true;
                textBox1.Text = (OF.FileName);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var OF = new OpenFileDialog
            {
                InitialDirectory = Application.StartupPath,
                Filter = "All files (*) | *.*"
            };

            if (OF.ShowDialog() == DialogResult.OK)
            {
                OF.FilterIndex = 0;
                OF.RestoreDirectory = true;
                textBox2.Text = (OF.FileName);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            StringManip.EncodeFileTo64(textBox1.Text, System.IO.Path.ChangeExtension(textBox1.Text, null) + "_encrypted" + System.IO.Path.GetExtension(textBox1.Text));
            MessageBox.Show("Encrypted file to " + System.IO.Path.ChangeExtension(textBox1.Text, null) + "_encrypted" + System.IO.Path.GetExtension(textBox1.Text), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

        }

        private void button3_Click(object sender, EventArgs e)
        {
            StringManip.DecodeFileFrom64(textBox2.Text, System.IO.Path.ChangeExtension(textBox2.Text, null) + "_decrypted" + System.IO.Path.GetExtension(textBox2.Text));
            MessageBox.Show("Decrypted file to " + System.IO.Path.ChangeExtension(textBox2.Text, null) + "_decrypted" + System.IO.Path.GetExtension(textBox2.Text), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
