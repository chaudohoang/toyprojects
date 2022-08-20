using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OneTimeRunner
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void SetVersionInfo()
        {
            Version versionInfo = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            DateTime startDate = new DateTime(2000, 1, 1);
            int diffDays = versionInfo.Build;
            DateTime computedDate = startDate.AddDays(diffDays);
            string lastBuilt = computedDate.ToShortDateString();
            //this.Text = string.Format("{0} - {1} ({2})",
            //            this.Text, versionInfo.ToString(), lastBuilt);
            this.Text = string.Format("{0} - {1}",
                        this.Text, versionInfo.ToString());
        }
        private bool allowVisible = true;

        protected override void SetVisibleCore(bool value)
        {
            if (!allowVisible)
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        }


        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            allowVisible = true;
            Show();
            Activate();
            this.WindowState = FormWindowState.Normal;
        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            // get the file's extension 
            string strFileExt = Path.GetExtension(e.FullPath).ToLower();

            // filter file types 
            if (comFileType.Text=="*.*")
            {
                //notifyFileChange(e.FullPath, e.ChangeType.ToString(), ToolTipIcon.Info);
            }
            else if (Regex.IsMatch(strFileExt, comFileType.Text, RegexOptions.IgnoreCase))
            {
                //notifyFileChange(e.FullPath, e.ChangeType.ToString(), ToolTipIcon.Info);
            }
        }

        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
        {
            // get the file's extension 
            string strFileExt = Path.GetExtension(e.FullPath).ToLower();

            // filter file types 
            if (comFileType.Text == "*.*")
            {
                //notifyFileChange(e.FullPath, e.ChangeType.ToString(), ToolTipIcon.Info);
                processFile(e.FullPath, int.Parse(comKillProcessTimeout.Text));
            }
            else if (Regex.IsMatch(strFileExt, comFileType.Text, RegexOptions.IgnoreCase))
            {
                //notifyFileChange(e.FullPath, e.ChangeType.ToString(), ToolTipIcon.Info);
                processFile(e.FullPath, int.Parse(comKillProcessTimeout.Text));
            }
        }

        private void fileSystemWatcher1_Deleted(object sender, FileSystemEventArgs e)
        {
            // get the file's extension 
            string strFileExt = Path.GetExtension(e.FullPath).ToLower();

            // filter file types
            if (comFileType.Text == "*.*")
            {
                //notifyFileChange(e.FullPath, e.ChangeType.ToString(), ToolTipIcon.Info);
            }
            else if (Regex.IsMatch(strFileExt, comFileType.Text, RegexOptions.IgnoreCase))
            {
                //notifyFileChange(e.FullPath, e.ChangeType.ToString(), ToolTipIcon.Info);
            }
        }

        private void fileSystemWatcher1_Renamed(object sender, RenamedEventArgs e)
        {
            // get the file's extension 
            string strFileExt = Path.GetExtension(e.FullPath).ToLower();

            // filter file types 
            if(comFileType.Text == "*.*")
            {
                //notifyFileChange(e.FullPath, e.ChangeType.ToString(), ToolTipIcon.Info);
            }
            else if (Regex.IsMatch(strFileExt, comFileType.Text, RegexOptions.IgnoreCase))
            {
                //notifyFileChange(e.FullPath, e.ChangeType.ToString(), ToolTipIcon.Info);
            }
        }

        private void comWatchFolder_SelectedValueChanged(object sender, EventArgs e)
        {
            if (Directory.Exists(comWatchFolder.Text))
            {
                fileSystemWatcher1.Path = comWatchFolder.Text;
                notifyIcon1.BalloonTipTitle = "Watch folder changed.";
                notifyIcon1.BalloonTipText = "Now watching folder " + comWatchFolder.Text;
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
                notifyIcon1.ShowBalloonTip(30000);
            }
            else
            {
                notifyIcon1.BalloonTipTitle = "Watch folder not changed.";
                notifyIcon1.BalloonTipText = comWatchFolder.Text + " does not exsits";
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Warning;
                notifyIcon1.ShowBalloonTip(30000);
            }
        }

        private void comWatchFolder_DropDown(object sender, EventArgs e)
        {
            comWatchFolder.Items.Clear();
            List<string> lineOfContents = File.ReadAllLines(@"watchfolderlist.txt").Distinct().ToList();
            foreach (var line in lineOfContents)
            {
                comWatchFolder.Items.Add(line);
            }
        }

        private void comFileType_DropDown(object sender, EventArgs e)
        {
            comFileType.Items.Clear();
            List<string> lineOfContents = File.ReadAllLines(@"watchfiletypelist.txt").Distinct().ToList();
            foreach (var line in lineOfContents)
            {
                comFileType.Items.Add(line);
            }
        }
        public void notifyFileChange(string inputfile, string changetype, ToolTipIcon icon)
        {
            notifyIcon1.BalloonTipTitle = "File " + changetype;
            notifyIcon1.BalloonTipText = inputfile +" "+ changetype;
            notifyIcon1.BalloonTipIcon = icon;
            notifyIcon1.ShowBalloonTip(30000);
        }
        public void processFile(string inputfile, int waittime)
        {
            Process process = new Process();
            process.StartInfo.FileName = inputfile;
            var tryrun = Int32.Parse(comRetryCount.Text);

            while (true)
            {
                try
                {
                    process.Start();
                    break; // success!
                }
                catch
                {
                    if (--tryrun == 0)
                        throw;
                    Thread.Sleep(1000);
                }
            }
            
            process.WaitForExit(1000 * waittime);
            if (!process.HasExited && process.Responding)
            {
                process.Kill();
            }
            if (chkDelAfterRun.Checked == true)
            {
                var trykill = Int32.Parse(comRetryCount.Text);
                while (true)
                {
                    try
                    {
                        File.Delete(inputfile);
                        break; // success!
                    }
                    catch
                    {
                        if (--trykill == 0)
                            throw;
                        Thread.Sleep(1000);
                    }
                }
            }
           
        }
    
    }
}
    