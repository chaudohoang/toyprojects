// PasswordForm.cs
using System;
using System.Windows.Forms;

namespace FileLock
{
    public partial class Password : Form
    {
        private const string CorrectPassword = "rvsk1234"; // Change this to your desired password

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

    }
}
