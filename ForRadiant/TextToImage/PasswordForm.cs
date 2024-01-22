using System;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace RVSSnipper
{
    public partial class PasswordForm : Form
    {
        public string EnteredPassword { get; private set; }

        public PasswordForm()
        {
            InitializeComponent();  
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            TrySubmitPassword();
        }

        private void TrySubmitPassword()

        {
            // Check if the entered password is correct
            if (IsPasswordCorrect())
            {
                // Set the DialogResult to OK
                DialogResult = DialogResult.OK;
                // Close the form
                Close();
            }
            else
            {
                // Display an error message or handle incorrect password
                MessageBox.Show("Incorrect password. Please try again.");
            }
        }

        private bool IsPasswordCorrect()
        {
            // Implement your password validation logic here
            // For example, compare the entered password with the correct one
            string correctPassword = "rvsk"+DateTime.Now.ToString("HHmm");
            return textBox1.Text == correctPassword;
        }

        private void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key is pressed
            if (e.KeyCode == Keys.Enter)
            {
                // Try to submit the password
                TrySubmitPassword();
            }
            // Check if the Esc key is pressed
            else if (e.KeyCode == Keys.Escape)
            {
                // Set DialogResult to Cancel
                DialogResult = DialogResult.Cancel;
                // Close the form
                Close();
            }
        }
    }
}
