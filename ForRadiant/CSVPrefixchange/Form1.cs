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
using System.Reflection;

namespace CSVPrefixchange
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void SetVersionInfo()
        {
            Version versionInfo = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime startDate = new DateTime(2000, 1, 1);
            int diffDays = versionInfo.Build;
            DateTime computedDate = startDate.AddDays(diffDays);
            string lastBuilt = computedDate.ToShortDateString();
            //this.Text = string.Format("{0} - {1} ({2})",
            //            this.Text, versionInfo.ToString(), lastBuilt);
            this.Text = string.Format("{0} - {1}",
                        this.Text, versionInfo.ToString());
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            btnRun.Enabled = false;
            string path = txtPath.Text;
            DirectoryInfo parentdir = new DirectoryInfo(path);
            WalkDirectoryTree(parentdir);
            
            btnRun.Enabled = true;
        }

        public void WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            FileInfo[] files = null;
            DirectoryInfo[] subDirs = null;

            try
            {
                files = root.GetFiles("*.*");
            }

            catch (UnauthorizedAccessException e)
            {
                MessageBox.Show(e.Message);
              
            }

            catch (DirectoryNotFoundException e)
            {
                MessageBox.Show(e.Message);
            }

            if (files != null)
            {
                foreach (FileInfo fi in files)
                {
       
                    string oldstring = txtFrom.Text;
                    string newstring = txtTo.Text;
                    string text = File.ReadAllText(fi.FullName);
                    text = text.Replace(oldstring, newstring);
                    File.WriteAllText(fi.FullName, text);
                    string newf = fi.FullName.Replace(oldstring, newstring);

                    File.Move(fi.FullName, newf);

                }

            }

            //recursive
            subDirs = root.GetDirectories();
            foreach (DirectoryInfo dirInfo in subDirs)
            {

                WalkDirectoryTree(dirInfo);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }
    }
}
