using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RenameFiles
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var rootDir = Directory.GetCurrentDirectory();
            var fileNames = Directory.EnumerateFiles(rootDir, "*", SearchOption.AllDirectories);
            foreach (String path in fileNames)
            {
                var dir = Path.GetDirectoryName(path);
                var fileName = Path.GetFileNameWithoutExtension(path).Replace(textBox1.Text,textBox2.Text);
                var ext = Path.GetExtension(path);
                var newPath = Path.Combine(dir, fileName + ext);
                File.Move(path, newPath);
            }
        }
    }
}
