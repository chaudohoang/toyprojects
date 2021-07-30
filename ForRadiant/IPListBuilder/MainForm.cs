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


namespace IPListBuilder
{
    public partial class MainForm : Form
    {
        public MainForm()
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

        private void cbIPList2_DropDown(object sender, EventArgs e)
        {
            cbIPList2.Items.Clear();
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");
            foreach (string file in files)
            {
                cbIPList2.Items.Add(file);
            }
        }

        private void btnGenerate2_Click(object sender, EventArgs e)
        {
            if (File.Exists(cbIPList2.Text))
            {
                string[] lines = File.ReadAllLines(cbIPList2.Text);
                string filename = Path.GetFileNameWithoutExtension(cbIPList2.Text);
                List<string> newfile = new List<string>();
                newfile.Add("<?xml version=\"1.0\"?>");
                newfile.Add("<Copywhiz Action=\"Copy\" ShowLog=\"Never\" LogContent=\"1\" RunSilently=\"False\">");
                newfile.Add("   <Filter IncludeFilesMask=\"\" ExcludeFilesMask=\"\" IncludeFoldersMask=\"\" ExcludeFoldersMask=\"\" DateFilter=\"0\" FilterBySize=\"False\" CopyAllFilesToSingleFolder=\"False\" CreateFolderStructureOnly=\"False\" SkipEmptyFolders=\"False\" CreateFullFolderHierarchy=\"False\"/>");
                newfile.Add("   <Source SourceFolder=\"\"></Source>");
                newfile.Add("   <FileTransfer FileAlreadyExistsAction=\"1\" FileAlreadyExistsSkipIdentical=\"True\" FileAlreadyExistsSkipNewer=\"True\" FileAlreadyExistsRenameSuffix=\"\">");
                newfile.Add("       <Destinations>");
                foreach (string line in lines)
                {
                    string newline = "          <FileFolder Path=\"\\\\" + line.Split(',')[1] + cbExtendedPath.Text + "\"/>";
                    newfile.Add(newline);
                }
                newfile.Add("       </Destinations>");
                newfile.Add("   </FileTransfer>");
                newfile.Add("</Copywhiz>");
                File.WriteAllLines(filename + ".czml", newfile);
                MessageBox.Show("Generated: " + Path.GetFullPath(filename + ".czml"));
            }
        }
        private void btnGenerate4_Click(object sender, EventArgs e)
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");
            List<string> result = new List<string>();
            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file);
                List<string> newfile = new List<string>();
                foreach (string line in lines)
                {
                    newfile.Add(line.Split(',')[1]);
                }
                string filename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file), System.IO.Path.GetFileNameWithoutExtension(file)+".txt");
                result.Add(filename);
                File.WriteAllLines(filename, newfile);
            }
            MessageBox.Show("Generated: "+ Environment.NewLine +string.Join(Environment.NewLine, result));
            
        }

        private void cbIPList4_DropDown(object sender, EventArgs e)
        {
            cbIPList4.Items.Clear();
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");
            foreach (string file in files)
            {
                cbIPList4.Items.Add(file);
            }
        }

        private void btnGenerate5_Click(object sender, EventArgs e)
        {
            if (File.Exists(cbIPList4.Text))
            {
                if (!Directory.Exists(@"Batch Job"))
                {
                    Directory.CreateDirectory(@"Batch Job");
                }

                string[] lines = File.ReadAllLines(cbIPList4.Text);
                string iplist = Path.GetFileNameWithoutExtension(cbIPList4.Text);
                string sourcefolder = Path.GetFileName(cbSource1.Text);
                string filename = iplist + "_" + sourcefolder;
                List<string> batchfile = new List<string>();
                batchfile.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                batchfile.Add("<FreeFileSync XmlType=\"BATCH\" XmlFormat=\"17\">");
                batchfile.Add("    <Compare>");
                batchfile.Add("        <Variant>TimeAndSize</Variant>");
                batchfile.Add("        <Symlinks>Exclude</Symlinks>");
                batchfile.Add("        <IgnoreTimeShift/>");
                batchfile.Add("    </Compare>");
                batchfile.Add("    <Synchronize>");
                batchfile.Add("        <Variant>Mirror</Variant>");
                batchfile.Add("        <DetectMovedFiles>false</DetectMovedFiles>");
                batchfile.Add("        <DeletionPolicy>RecycleBin</DeletionPolicy>");
                batchfile.Add("        <VersioningFolder Style=\"Replace\"/>");
                batchfile.Add("    </Synchronize>");
                batchfile.Add("    <Filter>");
                batchfile.Add("        <Include>");
                batchfile.Add("            <Item>*</Item>");
                batchfile.Add("        </Include>");
                batchfile.Add("        <Exclude>");
                batchfile.Add("            <Item>\\System Volume Information\\</Item>");
                batchfile.Add("            <Item>\\$Recycle.Bin\\</Item>");
                batchfile.Add("            <Item>\\RECYCLE?\\</Item>");
                batchfile.Add("            <Item>*\\thumbs.db</Item>");
                batchfile.Add("        </Exclude>");
                batchfile.Add("        <TimeSpan Type=\"None\">0</TimeSpan>");
                batchfile.Add("        <SizeMin Unit=\"None\">0</SizeMin>");
                batchfile.Add("        <SizeMax Unit=\"None\">0</SizeMax>");
                batchfile.Add("    </Filter>");
                batchfile.Add("    <FolderPairs>");
                foreach (string line in lines)
                {
                    batchfile.Add("        <Pair>");
                    batchfile.Add("            <Left>" + cbSource1.Text + "</Left>");
                    batchfile.Add("            <Right>\\\\" + line.Split(',')[1] + cbExtendedDestination.Text + "</Right>");
                    batchfile.Add("        </Pair>");
                }
                batchfile.Add("    </FolderPairs>");
                batchfile.Add("    <Errors Ignore=\"true\" Retry=\"0\" Delay=\"5\"/>");
                batchfile.Add("    <PostSyncCommand Condition=\"Completion\"/>");
                batchfile.Add("    <LogFolder/>");
                batchfile.Add("    <EmailNotification Condition=\"Always\"/>");
                batchfile.Add("    <Batch>");
                batchfile.Add("        <ProgressDialog Minimized=\"true\" AutoClose=\"true\"/>");
                batchfile.Add("        <ErrorDialog>Show</ErrorDialog>");
                batchfile.Add("        <PostSyncAction>None</PostSyncAction>");
                batchfile.Add("    </Batch>");
                batchfile.Add("</FreeFileSync>");
                File.WriteAllLines(@"Batch Job\\" + filename + ".ffs_batch", batchfile);

                List<string> realfile = new List<string>();
                realfile.Add("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
                realfile.Add("<FreeFileSync XmlType=\"REAL\" XmlFormat=\"2\">");
                realfile.Add("    <Directories>");
                realfile.Add("        <Item>" + cbSource1.Text +"</Item>");
                realfile.Add("    </Directories>");
                realfile.Add("    <Delay>" + cbIdleTime1.Text +"</Delay>");
                if (cbSource1.Text.StartsWith("\\\\127.0.0.1"))
                {
                    realfile.Add("    <Commandline>\"C:\\Program Files\\FreeFileSync\\FreeFileSync.exe\" " + "\"" + "\\\\127.0.0.1\\Program\\RVS\\Tools" + "\\CopyScript\\IPlist\\Batch Job\\" + filename + ".ffs_batch" + "\" &amp; exit 0" + "</Commandline>");
                }
                else
                {
                    realfile.Add("    <Commandline>\"C:\\Program Files\\FreeFileSync\\FreeFileSync.exe\" " + "\"" + Path.GetFullPath(@"Batch Job\\" + filename + ".ffs_batch") + "\" &amp; exit 0" + "</Commandline>");
                }
                realfile.Add("</FreeFileSync>");
                File.WriteAllLines(@"Batch Job\\" + filename + ".ffs_real", realfile);

                List<string> taskfile = new List<string>();
                taskfile.Add("schtasks /create /TR \"'C:\\Program Files\\FreeFileSync\\RealTimeSync.exe' '%~dp0" + filename + ".ffs_real" + "'\" /TN \"" + filename + "\" /SC ONLOGON /RL HIGHEST /F");
                File.WriteAllLines(@"Batch Job\\" + filename + "_StartupTask.bat", taskfile);

                MessageBox.Show("Generated: " + Path.GetFullPath(@"Batch Job\\" + filename + ".ffs_batch") +Environment.NewLine + Path.GetFullPath(@"Batch Job\\" + filename + ".ffs_real")+ Environment.NewLine + Path.GetFullPath(@"Batch Job\\" + filename + "_StartupTask.bat"));
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }
    }
}
