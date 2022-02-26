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
using System.IO;
using System.Diagnostics;

namespace PanelFFC
{
    public partial class Main : Form
    {
        public Main()
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

       

        private void Main_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
            RefreshItems();

        }

		private void btnRefresh_Click(object sender, EventArgs e)
		{
            RefreshItems();
        }

        private void RefreshItems()
		{
            try
            {
                string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appdir = Path.GetDirectoryName(apppath);
                String[] dlls =
                Directory.GetFiles(appdir, "*.exe", SearchOption.AllDirectories)
                .Select(fileName => Path.GetFileName(fileName))
                .Where(fileName => Path.GetFileNameWithoutExtension(fileName).StartsWith("PanelFFC"))
                .ToArray();
                listBox1.Items.Clear();
                foreach (string file in dlls)
                {
                    listBox1.Items.Add(file);
                }
                listBox1.Items.Remove("PanelFFC.exe");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

		private void btnStart_Click(object sender, EventArgs e)
		{
            string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appdir = Path.GetDirectoryName(apppath)+"\\"+"Apps";
            if (File.Exists(appdir+"\\"+listBox1.SelectedItem))
            {
                Process.Start(appdir + "\\" + listBox1.SelectedItem);
            }
        }
	}
}
