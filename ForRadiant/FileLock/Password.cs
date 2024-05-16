// PasswordForm.cs
using System;
using System.Windows.Forms;

namespace FileHide
{
    public partial class Password : Form
    {
        private const string CorrectPassword = "lgd1234"; // Change this to your desired password

        public bool IsPasswordCorrect { get; private set; }

        public Password()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            IsPasswordCorrect = textBox1.Text == CorrectPassword;
            Close();
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
