using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FolderLock
{
    public partial class Password : Form
    {
        public string password = "";
        public Password()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool passwordEntered = false;
            this.password = textBox1.Text;
            if (this.password == "")
            {
                MessageBox.Show("Password cannot be blank");
            }
            else passwordEntered = true;

            if (passwordEntered == true)
            {
                this.Close();
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(this, new EventArgs());
            }

            if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }


        }
    }
}
