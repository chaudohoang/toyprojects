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
            //Timer timer = new Timer();
            //timer.Interval = (1 * 1000); // 10 secs
            //timer.Tick += new EventHandler(timer_Tick);
            //timer.Start();
            RefreshItems();

        }
        private void timer_Tick(object sender, EventArgs e)
        {
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
                string newApps = appdir + "\\Apps";
                string oldApps = appdir + "\\Apps\\old";
                String[] dllsNew =
                Directory.GetFiles(newApps, "*.exe", SearchOption.TopDirectoryOnly)
                .Select(fileName => Path.GetFileName(fileName))
                .Where(fileName => Path.GetFileNameWithoutExtension(fileName).StartsWith("PanelFFC"))
                .ToArray();
                String[] dllsOld =
                Directory.GetFiles(oldApps, "*.exe", SearchOption.TopDirectoryOnly)
                .Select(fileName => Path.GetFileName(fileName))
                .Where(fileName => Path.GetFileNameWithoutExtension(fileName).StartsWith("PanelFFC"))
                .ToArray();
                listBox1.Items.Clear();
                foreach (string file in dllsNew)
                {
                    listBox1.Items.Add(file);
                }
                listBox1.Items.Remove("PanelFFC.exe");
                listBox2.Items.Clear();
                foreach (string file in dllsOld)
                {
                    listBox2.Items.Add(file);
                }
                listBox2.Items.Remove("PanelFFC.exe");
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
        }

        private void StartNew()
		{
            string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appdir = Path.GetDirectoryName(apppath) + "\\Apps";
            if (File.Exists(appdir + "\\" + listBox1.SelectedItem))
            {
                Process.Start(appdir + "\\" + listBox1.SelectedItem);
            }
        }
        private void StartOld()
        {
            string apppath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string appdir = Path.GetDirectoryName(apppath) + "\\Apps\\old";
            if (File.Exists(appdir + "\\" + listBox2.SelectedItem))
            {
                Process.Start(appdir + "\\" + listBox2.SelectedItem);
            }
        }

		private void listBox1_DoubleClick(object sender, EventArgs e)
		{
            StartNew();
		}

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            StartOld();
        }

        private void Main_Click(object sender, EventArgs e)
        {
            RefreshItems();
        }

    }
}
