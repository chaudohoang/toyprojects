using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomPopup
{
    public partial class Testform : Form
    {
        public Testform()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ErrorBox.Show("hhhhhhhhhhhhhhhhhhhhhhhh");
        }
    }
}
