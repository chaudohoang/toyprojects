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

namespace IPListBuilder
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void cbIPList1_DropDown(object sender, EventArgs e)
        {
            cbIPList1.Items.Clear();
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");
            foreach (string file in files)
            {
                cbIPList1.Items.Add(file);
            }
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

        private void cbIPList3_DropDown(object sender, EventArgs e)
        {
            cbIPList3.Items.Clear();
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.csv");
            foreach (string file in files)
            {
                cbIPList3.Items.Add(file);
            }
        }

        private void btnGenerate1_Click(object sender, EventArgs e)
        {
            if (File.Exists(cbIPList1.Text))
            {
                string[] lines = File.ReadAllLines(cbIPList1.Text);
                string filename = Path.GetFileNameWithoutExtension(cbIPList1.Text);
                List<string> newfile = new List<string>();
                newfile.Add("<?xml version=\"1.0\" encoding=\"UTF - 8\"?>");
                newfile.Add("<Advanced_IP_scanner>");
                
                foreach (string line in lines)
                {
                    string newline = "   <row status=\"alive\" ip=\"" + line.Split(',')[1] + "\" has_http=\"0\" is_http8080=\"0\" has_https=\"0\" has_ftp=\"0\" has_rdp=\"0\" sIsDNSScan=\"0\" alias=\"" + line.Split(',')[0] + "\" expanded=\"0\"></row>";
                    newfile.Add(newline);
                }

                newfile.Add("</Advanced_IP_scanner>");
                File.WriteAllLines(filename+".xml", newfile);
                MessageBox.Show("Generated: " + Path.GetFullPath(filename + ".xml"));
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

        private void btnGenerate3_Click(object sender, EventArgs e)
        {
            if (File.Exists(cbIPList3.Text))
            {
                string[] lines = File.ReadAllLines(cbIPList3.Text);
                foreach (string line in lines)
                {
                    string path = @"VNC addresses\" + line.Split(',')[0] + ".vnc";
                    if (!Directory.Exists(@"VNC addresses"))
                    {
                        Directory.CreateDirectory(@"VNC addresses");
                    }
                    string contents = "ConnMethod=tcp" + Environment.NewLine+ "FriendlyName=" + line.Split(',')[0] + Environment.NewLine + "Host=" + line.Split(',')[1] + Environment.NewLine + "RelativePtr=0";
                    File.WriteAllText(path, contents);
                }
                MessageBox.Show("Generated: " + Path.GetFullPath(@"VNC addresses"));

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
    }
}
