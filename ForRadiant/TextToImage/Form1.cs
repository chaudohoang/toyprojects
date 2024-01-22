using RVSSnipper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using TextBox = System.Windows.Forms.TextBox;

namespace TextToImage
{
    public partial class MainForm : Form
    {
        private const int ChunkSize = 20000;
        private int initialTextBoxLeftSpacing;
        private int initialTextBoxTopSpacing;

        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            // Load the image from the clipboard when the form is loaded
            initialTextBoxLeftSpacing = textBoxIn.Left;
            initialTextBoxTopSpacing = textBoxIn.Top;
            await LoadImageFromClipboardAsync();
        }

        private async Task LoadImageFromClipboardAsync()
        {
            if (Clipboard.ContainsImage())
            {
                // Get the image from the clipboard
                Image clipboardImage = Clipboard.GetImage();

                // Update pictureBoxIn
                pictureBoxIn.Image = clipboardImage;
                pictureBoxIn.SizeMode = PictureBoxSizeMode.Zoom;

                // Update pictureBoxOut
                pictureBoxOut.SizeMode = PictureBoxSizeMode.Zoom;
            }
        }

        private string ImageToBase64(Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Use the Jpeg encoder for the MemoryStream
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg); // You can adjust the format based on your requirements
                byte[] imageBytes = ms.ToArray();

                // Convert the byte array to Base64 string
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }

        private List<string> ChunkString(string str, int chunkSize)
        {
            return Enumerable.Range(0, (int)Math.Ceiling((double)str.Length / chunkSize))
                .Select(i => str.Substring(i * chunkSize, Math.Min(chunkSize, str.Length - i * chunkSize)))
                .ToList();
        }

        private async Task CreateTextBoxesAsync(List<string> chunks)
        {
            // Clear existing controls in tabPage2
            tabPage2.Controls.Clear();

            // Create a TableLayoutPanel to organize text boxes, labels, and buttons
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel();
            tableLayoutPanel.Dock = DockStyle.Fill;

            // Calculate the number of rows needed
            int numRows = (int)Math.Ceiling((double)chunks.Count / 2);

            // Add rows and columns to the TableLayoutPanel
            for (int row = 0; row < numRows; row++)
            {
                tableLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

                for (int col = 0; col < 2; col++) // Two columns for label and TextBox
                {
                    tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
                }
            }

            // Create dynamic text boxes with labels and copy buttons
            int textBoxIndex = 0;
            for (int row = 0; row < numRows; row++)
            {
                for (int col = 0; col < 2; col++) // Two columns for label and TextBox
                {
                    if (textBoxIndex < chunks.Count)
                    {
                        // Create a panel to hold the label, Copy button, and TextBox
                        Panel panel = new Panel();
                        panel.Dock = DockStyle.Fill;

                        // Create a label indicating the order of creation
                        Label label = new Label();
                        label.Dock = DockStyle.Top; // Place label above the TextBox
                        label.TextAlign = ContentAlignment.MiddleCenter;
                        label.Text = $"Box {textBoxIndex + 1}";

                        // Create a "Copy" button
                        System.Windows.Forms.Button copyButton = new System.Windows.Forms.Button();
                        copyButton.Text = "Copy";
                        copyButton.Size = new Size(50, 20); // Set the size of the button
                        copyButton.Dock = DockStyle.Left; // Place button to the left of the label
                        copyButton.Tag = textBoxIndex; // Attach the index to the button for identification
                        copyButton.Click += CopyButton_Click; // Attach the event handler for button click

                        // Create a TextBox
                        TextBox textBox = new TextBox();
                        textBox.Multiline = true;
                        textBox.ScrollBars = ScrollBars.Vertical;
                        textBox.Dock = DockStyle.Fill;
                        textBox.Name = $"textBox{textBoxIndex}"; // Set the unique name for identification
                        textBox.Text = chunks[textBoxIndex++];

                        // Add the Label, Copy Button, and TextBox to the Panel
                        panel.Controls.Add(copyButton);
                        panel.Controls.Add(label);
                        panel.Controls.Add(textBox);

                        // Add the Panel to the TableLayoutPanel
                        tableLayoutPanel.Controls.Add(panel, col, row);
                    }
                }
            }

            // Add the TableLayoutPanel to tabPage2
            tabPage2.Controls.Add(tableLayoutPanel);
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.Button button && button.Tag is int textBoxIndex)
            {
                // Find the associated TextBox using the index
                Control[] textBoxControls = tabPage2.Controls.Find($"textBox{textBoxIndex}", true);

                if (textBoxControls.Length > 0 && textBoxControls[0] is TextBox textBox)
                {
                    // Copy the content of the associated TextBox to the clipboard
                    Clipboard.SetText(textBox.Text);
                    // Print a message to the Output window to indicate that the copy operation is successful
                    Console.WriteLine($"Copied content of TextBox {textBoxIndex} to clipboard.");
                }
            }
        }

        private async void ConvertButton_Click(object sender, EventArgs e)
        {

            using (var passwordForm = new PasswordForm())
            {
                if (passwordForm.ShowDialog() == DialogResult.OK)
                {
                    await ConvertImageToBase64Async();
                }
                else
                {
                    // Password is incorrect or dialog was canceled
                    MessageBox.Show("Incorrect password or canceled.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
        }

        private async Task ConvertImageToBase64Async()
        {
            // Check if there is an image in the PictureBox
            if (pictureBoxIn.Image != null)
            {
                // Resize the image with a percentage (adjust the percentage as needed)
                double resizePercentage = GetPercentageFromString(ResizeRatioTextBox.Text); // Resize to 50% of the original size

                Image resizedImage = ResizeImage(pictureBoxIn.Image, resizePercentage);

                // Convert the resized image to Base64 string
                string base64String = ImageToBase64(resizedImage);

                // Divide the Base64 string into chunks
                List<string> chunks = ChunkString(base64String, ChunkSize);

                // Create dynamic text boxes for each chunk in tabPage2
                await CreateTextBoxesAsync(chunks);
            }
        }

        private Image ResizeImage(Image image, double percentage)
        {
            int newWidth = (int)(image.Width * percentage);
            int newHeight = (int)(image.Height * percentage);

            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(image, 0, 0, newWidth, newHeight);
            }
            return resizedImage;
        }

        private double GetPercentageFromString(string percentageText)
        {
            // Remove the percentage sign and trim any whitespace
            string cleanedText = percentageText.Trim();

            // Try to parse the cleaned text to a double
            if (double.TryParse(cleanedText, out double percentage))
            {
                // Divide by 100 to get the decimal representation of the percentage
                return percentage / 100.0;
            }

            // Return a default value or handle the error as needed
            return 1.0; // Default to 100% if parsing fails
        }

        private void PasteButton_Click(object sender, EventArgs e)
        {
            // Load the image from the clipboard when the "Paste" button is clicked
            LoadImageFromClipboardAsync();
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            // Clear the image and text boxes when the "Clear" button is clicked
            pictureBoxIn.Image = null;
            tabPage2.Controls.Clear();
        }

        private void resizeAll()
        {
            AdjustPictureInSize();
            AdjustTextBoxesSOutize();
            AdjustTextBoxInSize();
            AdjustPictureOutSize();
            // Resize the TabControl and its contained TabPage
            tabControl1.Size = new Size(ClientSize.Width - 25, ClientSize.Height - 25);
            tabPage1.Size = new Size(tabControl1.Size.Width - 10, tabControl1.Size.Height - 10);
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            // Resize the image and text boxes when the form is resized
            resizeAll();
        }

        private void AdjustTextBoxInSize()
        {
            // Calculate the new left and top positions based on the initial spacing
            int newLeft = initialTextBoxLeftSpacing + 5;
            int newTop = initialTextBoxTopSpacing + 40;

            // Adjust the size of the TextBox
            textBoxIn.Size = new Size(tabPage3.Size.Width - 15, tabPage3.Size.Height - 45);

            // Snap the TextBox to the calculated position
            textBoxIn.Location = new Point(newLeft, newTop);

        }

        private void AdjustPictureOutSize()
        {
            if (pictureBoxOut.Image != null)
            {
                // Calculate the new size to maintain the aspect ratio
                int newWidth = (int)(pictureBoxOut.Image.Width * (float)ClientSize.Height / pictureBoxOut.Image.Height);
                int newHeight = ClientSize.Height;

                // Set the new size and ensure the image is fully visible
                pictureBoxOut.Size = new Size(newWidth, newHeight);
                pictureBoxOut.SizeMode = PictureBoxSizeMode.Zoom;
            }

        }


        private void AdjustPictureInSize()
        {
            if (pictureBoxIn.Image != null)
            {
                // Calculate the new size to maintain the aspect ratio
                int newWidth = (int)(pictureBoxIn.Image.Width * (float)ClientSize.Height / pictureBoxIn.Image.Height);
                int newHeight = ClientSize.Height;

                // Set the new size and ensure the image is fully visible
                pictureBoxIn.Size = new Size(newWidth, newHeight);
                pictureBoxIn.SizeMode = PictureBoxSizeMode.Zoom;
            }


        }

        private void AdjustTextBoxesSOutize()
        {
            // Adjust the size of text boxes in tabPage2
            foreach (Control control in tabPage2.Controls)
            {
                if (control is TextBox)
                {
                    TextBox textBox = (TextBox)control;
                    textBox.Size = new Size(tabControl1.Size.Width / 2 - 20, tabControl1.Size.Height / 2 - 20);
                }
            }
        }

        private Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                return Image.FromStream(ms);
            }
        }

        private void ConvertToImageBtn_Click(object sender, EventArgs e)
        {

            using (var passwordForm = new PasswordForm())
            {
                if (passwordForm.ShowDialog() == DialogResult.OK)
                {
                    // Get the Base64 string from the TextBox
                    string base64String = textBoxIn.Text.Trim();

                    // Check if the Base64 string is not empty
                    if (!string.IsNullOrEmpty(base64String))
                    {
                        try
                        {
                            // Convert Base64 string to Image
                            Image convertedImage = Base64ToImage(base64String);

                            // Display the converted image in the PictureBox
                            pictureBoxOut.Image = convertedImage;
                        }
                        catch (Exception ex)
                        {
                            // Handle any exceptions (e.g., invalid Base64 string)
                            MessageBox.Show($"Error converting Base64 to Image: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        // Display a message if the Base64 string is empty
                        MessageBox.Show("Please enter a Base64 string.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    // Password is incorrect or dialog was canceled
                    MessageBox.Show("Incorrect password or canceled.", "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            
        }

        private void ClearInputButton_Click(object sender, EventArgs e)
        {
            textBoxIn.Clear();
        }

        private void PasteTextButton_Click(object sender, EventArgs e)
        {
            textBoxIn.AppendText(Clipboard.GetText());
        }
    }
}
