using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;


namespace MakeSequenceFOI
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
        private void button1_Click(object sender, EventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "C:\\Radiant Vision Systems Data\\TrueTest\\Sequence";
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    textBox1.Text = filePath;


                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }
    }
}
