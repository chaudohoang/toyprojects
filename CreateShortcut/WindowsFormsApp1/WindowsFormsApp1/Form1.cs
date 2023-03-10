using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                button1.Enabled = false;
                string path = textBox1.Text;
                WalkSubDirectories(path);
                Random m_Rnd = new Random();
                Color tempcolor;
                tempcolor = label2.ForeColor;
                while (label2.ForeColor == tempcolor)
                {
                    label2.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255));
                }
                label2.Text = "Done!";
                button1.Enabled = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            
        }

        public void WalkSubDirectories(string root)
        {
            string[] folders = Directory.GetDirectories(root);
            foreach (string folder in folders)
            {
                string[] files = Directory.GetFiles(folder,"*.exe");
                foreach (string file in files)
                {
                    
                }
            }
        }
    }
}
