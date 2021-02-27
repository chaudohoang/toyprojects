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

namespace EZAE
{
    public partial class SetSequence : Form
    {
        public SetSequence()
        {
            InitializeComponent();
        }

        private void FormLoad(object sender, EventArgs e)
        {
            LoadSubframe();
        }

        public void LoadSubframe()
        {
            subframeBox.Items.Clear();
            string path = "subframe.txt";
            List<string> lines = new List<string>(File.ReadAllLines(path));
            foreach (string line in lines)
            {

            }
            StreamReader sr = new StreamReader(path);
            while (sr.Peek() >= 0)
            {
                subframeBox.Items.Add(sr.ReadLine());
            }
            subframeBox.SelectedIndex = -1;
            sr.Close();
        }

        private void btnAddSubframe_Click(object sender, EventArgs e)
        {
            string path = "subframe.txt";
            StreamWriter sw = new StreamWriter(path, true);
            if (subframeBox.Text != "")
            {
                sw.WriteLine(subframeBox.Text);
            }
            sw.Close();
            LoadSubframe();
          
        }

        private void btnDelSubframe_Click(object sender, EventArgs e)
        {
            string path = "subframe.txt";
            string[] lines1 = File.ReadAllLines(path);
            

        }
    }
}
