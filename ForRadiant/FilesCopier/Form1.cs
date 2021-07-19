using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilesCopier
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
            List<string> sourcelist = GetFilesRecursive(comboBox1.Text);
            List<string> destlist = new List<string>();
            for (int i = 0; i < sourcelist.Count; i++)
            {
                destlist.Add(sourcelist[i].Replace(comboBox1.Text, comboBox2.Text));
            }
            for (int i = 0; i < sourcelist.Count; i++)
            {
                string source = sourcelist[i];
                string dest = destlist[i];
                if (!Directory.Exists(Path.GetDirectoryName(dest)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(dest));
                }
                CustomFileCopier copier = new CustomFileCopier();
                copier.SourceFilePath = source;
                copier.DestFilePath = dest;
                copier.Copy();
            }
            
        }

        public static List<string> GetFilesRecursive(string initial)
        {
            // This list stores the results.
            List<string> result = new List<string>();

            // This stack stores the directories to process.
            Stack<string> stack = new Stack<string>();

            // Add the initial directory
            stack.Push(initial);

            // Continue processing for each stacked directory
            while ((stack.Count > 0))
            {
                // Get top directory string
                string dir = stack.Pop();
                try
                {
                    // Add all immediate file paths
                    result.AddRange(Directory.GetFiles(dir, "*.*"));
                    foreach (var directoryName in Directory.GetDirectories(dir))
                        stack.Push(directoryName);
                }
                catch (Exception ex)
                {
                }
            }

            // Return the list
            return result;
        }

        public delegate void ProgressChangeDelegate(double Persentage, ref bool Cancel);
        public delegate void Completedelegate();

        class CustomFileCopier
        {
            public CustomFileCopier()
            {
                OnProgressChanged += delegate { };
                OnComplete += delegate { };
            }
            public CustomFileCopier(string Source, string Dest)
            {
                this.SourceFilePath = Source;
                this.DestFilePath = Dest;

                OnProgressChanged += delegate { };
                OnComplete += delegate { };
            }

            public void Copy()
            {
                byte[] buffer = new byte[1024 * 1024]; // 1MB buffer
                bool cancelFlag = false;

                using (FileStream source = new FileStream(SourceFilePath, FileMode.Open, FileAccess.Read))
                {
                    long fileLength = source.Length;
                    using (FileStream dest = new FileStream(DestFilePath, FileMode.CreateNew, FileAccess.Write))
                    {
                        long totalBytes = 0;
                        int currentBlockSize = 0;

                        while ((currentBlockSize = source.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            totalBytes += currentBlockSize;
                            double persentage = (double)totalBytes * 100.0 / fileLength;

                            dest.Write(buffer, 0, currentBlockSize);

                            cancelFlag = false;
                            OnProgressChanged(persentage, ref cancelFlag);

                            if (cancelFlag == true)
                            {
                                // Delete dest file here
                                break;
                            }
                        }
                    }
                }

                OnComplete();
            }

            public string SourceFilePath { get; set; }
            public string DestFilePath { get; set; }

            public event ProgressChangeDelegate OnProgressChanged;
            public event Completedelegate OnComplete;
        }

        private void progressBar1_ParentChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
        }
    }
}
