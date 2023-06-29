using IWshRuntimeLibrary;
using System;
using System.Drawing;
using System.IO;
using System.Linq.Expressions;
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
                string name = Path.GetFileName(folder);
                string[] files = Directory.GetFiles(folder,"*.exe");
                int i = 0;
                foreach (string file in files)
                {
                    if (file.ToLower().Contains("unins"))
                    {
                        continue;
                    }
                    object shDesktop = (object)"Desktop";
                    WshShell shell = new WshShell();
                    string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\Minigames\"+name + (i == 0 ? "" : "_"+i.ToString()) + ".lnk";
                    if (!Directory.Exists((string)shell.SpecialFolders.Item(ref shDesktop) + @"\Minigames"))
                    {
                        Directory.CreateDirectory((string)shell.SpecialFolders.Item(ref shDesktop) + @"\Minigames");
                    }
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
                    shortcut.Description = name;
                    shortcut.TargetPath = file;
                    shortcut.Save();
                    i++;
                }
            }
        }
    }
}
