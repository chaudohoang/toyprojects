using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace CopySequenceStep
{
    public partial class Form1 : Form
    {
        private string sourceFilePath;
        private string destinationFilePath;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnSourceSeqBrowse_Click(object sender, EventArgs e)
        {
            BrowseFile(txtSourceSeq, ref sourceFilePath);

            // After opening the source sequence file, dynamically show checkboxes for each <SequenceItem>
            DisplaySequenceItems();
        }

        private void btnDestinationSeqBrowse_Click(object sender, EventArgs e)
        {
            BrowseFile(txtDestinationSeq, ref destinationFilePath);

            // After opening the source sequence file, dynamically show checkboxes for each <SequenceItem>
            DisplaySequenceItemsDestination();
        }

        private void DisplaySequenceItems()
    {
        // Clear existing checkboxes in flpSequenceItemsSource
        flpSequenceItemsSource.Controls.Clear();

        if (string.IsNullOrEmpty(sourceFilePath))
        {
            MessageBox.Show("Please select a source sequence file first.");
            return;
        }

        try
        {
            // Load the XML document
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(sourceFilePath);

            // Find the <Items> element
            XmlNode itemsNode = xmlDoc.SelectSingleNode("//Items");

            if (itemsNode != null)
            {
                // Iterate through <SequenceItem> elements
                foreach (XmlNode sequenceItemNode in itemsNode.SelectNodes("SequenceItem"))
                {
                    // Get the Name and UserName attribute values
                    string patternSetupName = sequenceItemNode.SelectSingleNode("PatternSetupName")?.InnerText;
                    string userName = sequenceItemNode.SelectSingleNode("Analysis/UserName")?.InnerText;

                    if (!string.IsNullOrEmpty(patternSetupName) && !string.IsNullOrEmpty(userName))
                    {
                        // Create a checkbox for each <SequenceItem>
                        CheckBox checkBox = new CheckBox();
                        checkBox.Text = $"{patternSetupName} - {userName}";
                        checkBox.AutoSize = true;
                        checkBox.CheckedChanged += CheckBox_CheckedChanged; // Attach event handler

                        // Add the checkbox to the source flow layout panel
                        flpSequenceItemsSource.Controls.Add(checkBox);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error while displaying sequence items: {ex.Message}");
        }
    }

    private void CheckBox_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox checkBox = (CheckBox)sender;

        if (checkBox.Checked)
        {
            // Add the checkbox text to lstSequenceItemsDestination
            lstSequenceItemsDestination.Items.Add(checkBox.Text);
        }
        else
        {
            // Remove the checkbox text from lstSequenceItemsDestination
            lstSequenceItemsDestination.Items.Remove(checkBox.Text);
        }
    }

        private void DisplaySequenceItemsDestination()
        {
            // Clear existing items in lstSequenceItemsDestination
            lstSequenceItemsDestination.Items.Clear();

            if (string.IsNullOrEmpty(destinationFilePath))
            {
                MessageBox.Show("Please select a destination sequence file first.");
                return;
            }

            try
            {
                // Load the XML document
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(destinationFilePath);

                // Find the <Items> element
                XmlNode itemsNode = xmlDoc.SelectSingleNode("//Items");

                if (itemsNode != null)
                {
                    // Iterate through <SequenceItem> elements
                    foreach (XmlNode sequenceItemNode in itemsNode.SelectNodes("SequenceItem"))
                    {
                        // Get the Name and UserName attribute values
                        string patternSetupName = sequenceItemNode.SelectSingleNode("PatternSetupName")?.InnerText;
                        string userName = sequenceItemNode.SelectSingleNode("Analysis/UserName")?.InnerText;

                        if (!string.IsNullOrEmpty(patternSetupName) && !string.IsNullOrEmpty(userName))
                        {
                            // Combine pattern setup name and user name into a single string
                            string labelText = $"{patternSetupName} - {userName}";

                            // Add the label text to the destination ListBox
                            lstSequenceItemsDestination.Items.Add(labelText);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while displaying sequence items in destination: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void BrowseFile(TextBox textBox, ref string filepath)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Sequence Files|*.seqxc";
            openFileDialog.Title = "Select Sequence File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox.Text = openFileDialog.FileName;
                filepath = openFileDialog.FileName; // Save the file path for later use
            }
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            // Get the selected item index
            int selectedIndex = lstSequenceItemsDestination.SelectedIndex;

            // Check if there is a selected item and it's not the first item
            if (selectedIndex > 0)
            {
                // Get the text of the selected item
                string selectedText = lstSequenceItemsDestination.SelectedItem.ToString();

                // Remove the selected item
                lstSequenceItemsDestination.Items.RemoveAt(selectedIndex);

                // Insert the item at the previous index
                lstSequenceItemsDestination.Items.Insert(selectedIndex - 1, selectedText);

                // Select the moved item
                lstSequenceItemsDestination.SelectedIndex = selectedIndex - 1;
            }
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            // Get the selected item index
            int selectedIndex = lstSequenceItemsDestination.SelectedIndex;

            // Check if there is a selected item and it's not the last item
            if (selectedIndex >= 0 && selectedIndex < lstSequenceItemsDestination.Items.Count - 1)
            {
                // Get the text of the selected item
                string selectedText = lstSequenceItemsDestination.SelectedItem.ToString();

                // Remove the selected item
                lstSequenceItemsDestination.Items.RemoveAt(selectedIndex);

                // Insert the item at the next index
                lstSequenceItemsDestination.Items.Insert(selectedIndex + 1, selectedText);

                // Select the moved item
                lstSequenceItemsDestination.SelectedIndex = selectedIndex + 1;
            }
        }

        // Modify the btnCopy_Click event handler
        private void btnCopy_Click(object sender, EventArgs e)
        {
            // Check if the destination file is selected
            if (string.IsNullOrEmpty(destinationFilePath))
            {
                MessageBox.Show("Please select a destination sequence file.");
                return;
            }

            // Check if the source file is opened
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                // If source file is not opened, skip replace functions and only run sorting function
                SortSequenceItems(destinationFilePath);

                // Uncheck all checkboxes in flpSequenceItemsSource
                UncheckAllCheckboxes();
                DisplaySequenceItemsDestination();

                // Display success message
                MessageBox.Show("SequenceItem elements sorted successfully.");
                return;
            }

            // Find the checked checkboxes in flpSequenceItemsSource and copy their PatternSetup
            List<(string patternSetupName, string userName)> checkedPatternSetups = GetCheckedPatternSetups();

            foreach ((string patternSetupName, string userName) in checkedPatternSetups)
            {
                // Replace PatternSetup
                ReplacePatternSetup(sourceFilePath, destinationFilePath, patternSetupName);
            }

            foreach ((string patternSetupName, string userName) in checkedPatternSetups)
            {
                // Replace SequenceItem
                ReplaceSequenceItem(sourceFilePath, destinationFilePath, patternSetupName, userName);
            }

            // Sort the copied SequenceItems based on PatternSetupName and UserName
            SortSequenceItems(destinationFilePath);

            // Uncheck all checkboxes in flpSequenceItemsSource
            UncheckAllCheckboxes();
            DisplaySequenceItemsDestination();

            // Display success message
            MessageBox.Show("PatternSetup and SequenceItem elements copied and sorted successfully.");
        }

        private void UncheckAllCheckboxes()
        {
            foreach (Control control in flpSequenceItemsSource.Controls)
            {
                if (control is CheckBox checkBox)
                {
                    checkBox.Checked = false;
                }
            }
        }

        private void SortSequenceItems(string destinationFilePath)
        {
            try
            {
                // Store the order of items in lstSequenceItemsDestination
                List<string> sequenceItemOrder = new List<string>();
                foreach (string itemText in lstSequenceItemsDestination.Items)
                {
                    sequenceItemOrder.Add(itemText);
                }

                // Load the destination XML document
                XmlDocument destinationXmlDoc = new XmlDocument();
                destinationXmlDoc.Load(destinationFilePath);

                // Find the <Items> element in the destination XML document
                XmlNode itemsNode = destinationXmlDoc.SelectSingleNode("//Items");

                if (itemsNode != null)
                {

                    // Re-add the SequenceItem nodes in the original order
                    foreach (string itemText in sequenceItemOrder)
                    {
                        // Extract PatternSetupName and UserName from the item text
                        (string patternSetupName, string userName) = ExtractPatternSetupAndUserName(itemText);

                        try
                        {
                            // Find the corresponding SequenceItem nodes in the destination XML document
                            string xpathQuery = $"//SequenceItem[PatternSetupName='{patternSetupName}' and Analysis/UserName='{userName}']";
                            XmlNodeList sequenceItemNodes = destinationXmlDoc.SelectNodes(xpathQuery);

                            // Remove existing<SequenceItem> elements with the same PatternSetupName and UserName
                            foreach (XmlNode existingNode in sequenceItemNodes)
                            {
                                existingNode.ParentNode.RemoveChild(existingNode);
                            }

                            if (sequenceItemNodes != null && sequenceItemNodes.Count > 0)
                            {
                                // Append the original SequenceItem nodes
                                foreach (XmlNode node in sequenceItemNodes)
                                {
                                    XmlNode importedNode = destinationXmlDoc.ImportNode(node, true);
                                    itemsNode.AppendChild(importedNode);
                                }
                            }
                            else
                            {
                                MessageBox.Show($"No SequenceItem nodes found for PatternSetupName '{patternSetupName}' and UserName '{userName}'.");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error while processing SequenceItem nodes: {ex.Message}");
                        }
                    }

                    destinationXmlDoc.Save(destinationFilePath);

                    // Fix empty elements after saving changes
                    FixEmptyElements(destinationFilePath);

                    // Display success message
                    //MessageBox.Show("SequenceItem elements sorted successfully.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while sorting SequenceItem elements: {ex.Message}");
            }
        }

        static void SaveXmlDocument(XmlDocument xmlDoc, string filePath)
        {
            // Save the modified XML document with single-line empty elements
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                NewLineHandling = NewLineHandling.None
            };

            using (XmlWriter writer = XmlWriter.Create(filePath, settings))
            {
                xmlDoc.Save(writer);
            }
        }

        private List<(string patternSetupName, string userName)> GetCheckedPatternSetups()
        {
            List<(string patternSetupName, string userName)> checkedPatternSetups = new List<(string patternSetupName, string userName)>();

            foreach (CheckBox checkBox in flpSequenceItemsSource.Controls)
            {
                if (checkBox.Checked)
                {
                    // Extract patternSetupName and userName from checkbox text
                    (string patternSetupName, string userName) = ExtractPatternSetupAndUserName(checkBox.Text);
                    if (!string.IsNullOrEmpty(patternSetupName))
                    {
                        checkedPatternSetups.Add((patternSetupName, userName));
                    }
                }
            }

            return checkedPatternSetups;
        }


        private (string patternSetupName, string userName) ExtractPatternSetupAndUserName(string text)
        {
            // Initialize variables to store the extracted values
            string patternSetupName = string.Empty;
            string userName = string.Empty;

            // Find the index of the hyphen
            int hyphenIndex = text.IndexOf('-');

            // If the hyphen is found and there is text before and after it
            if (hyphenIndex != -1 && hyphenIndex < text.Length - 1)
            {
                // Extract the patternSetupName (text before the hyphen) and trim any leading or trailing spaces
                patternSetupName = text.Substring(0, hyphenIndex).Trim();

                // Extract the userName (text after the hyphen) and trim any leading or trailing spaces
                userName = text.Substring(hyphenIndex + 1).Trim();
            }

            return (patternSetupName, userName);
        }


        private void ReplacePatternSetup(string sourceFilePath, string destinationFilePath, string itemName)
        {
            try
            {
                // Load the source XML document
                XmlDocument sourceXmlDoc = new XmlDocument();
                sourceXmlDoc.Load(sourceFilePath);

                // Load the destination XML document
                XmlDocument destinationXmlDoc = new XmlDocument();
                destinationXmlDoc.Load(destinationFilePath);

                // Find the <PatternSetup> element in the source XML document with the specified <Name>
                XmlNode sourcePatternSetupNode = sourceXmlDoc.SelectSingleNode($"//PatternSetup[Name='{itemName}']");

                if (sourcePatternSetupNode != null)
                {
                    // Import the <PatternSetup> node into the destination document
                    XmlNode importedNode = destinationXmlDoc.ImportNode(sourcePatternSetupNode, true);

                    // Find the <PatternSetupList> element in the destination XML document
                    XmlNode patternSetupListNode = destinationXmlDoc.SelectSingleNode("//PatternSetupList");

                    if (patternSetupListNode != null)
                    {
                        // Remove existing PatternSetup node with the same name, if it exists
                        XmlNode existingPatternSetupNode = patternSetupListNode.SelectSingleNode($"PatternSetup[Name='{itemName}']");
                        if (existingPatternSetupNode != null)
                        {
                            patternSetupListNode.RemoveChild(existingPatternSetupNode);
                        }

                        // Append the imported <PatternSetup> node as a child of <PatternSetupList>
                        patternSetupListNode.AppendChild(importedNode);

                        destinationXmlDoc.Save(destinationFilePath);

                        // Fix empty elements after saving changes
                        FixEmptyElements(destinationFilePath);

                    }
                    else
                    {
                        MessageBox.Show("No <PatternSetupList> element found in destination XML file.");
                    }

                    //MessageBox.Show("PatternSetup elements copied successfully.");

                }
                else
                {
                    MessageBox.Show($"No <PatternSetup> element with Name '{itemName}' found in source XML file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while copying PatternSetup element: {ex.Message}");
            }
        }


        private void ReplaceSequenceItem(string sourceFilePath, string destinationFilePath, string patternSetupName, string userName)
        {
            try
            {
                // Load the source XML document
                XmlDocument sourceXmlDoc = new XmlDocument();
                sourceXmlDoc.Load(sourceFilePath);

                // Load the destination XML document
                XmlDocument destinationXmlDoc = new XmlDocument();
                destinationXmlDoc.Load(destinationFilePath);

                // Find the existing <SequenceItem> elements in the destination XML document with the same PatternSetupName and UserName
                XmlNodeList existingSequenceItemNodes = destinationXmlDoc.SelectNodes($"//SequenceItem[PatternSetupName='{patternSetupName}' and Analysis/UserName='{userName}']");

                // Remove existing <SequenceItem> elements with the same PatternSetupName and UserName
                foreach (XmlNode existingNode in existingSequenceItemNodes)
                {
                    existingNode.ParentNode.RemoveChild(existingNode);
                }

                // Find the <SequenceItem> element in the source XML document with the specified <PatternSetupName> and <UserName>
                XmlNodeList sourceSequenceItemNodes = sourceXmlDoc.SelectNodes($"//SequenceItem[PatternSetupName='{patternSetupName}' and Analysis/UserName='{userName}']");

                if (sourceSequenceItemNodes != null && sourceSequenceItemNodes.Count > 0)
                {
                    // Import each matching <SequenceItem> node into the destination document
                    foreach (XmlNode sourceSequenceItemNode in sourceSequenceItemNodes)
                    {
                        XmlNode importedNode = destinationXmlDoc.ImportNode(sourceSequenceItemNode, true);

                        // Find the <Items> element in the destination XML document
                        XmlNode itemsNode = destinationXmlDoc.SelectSingleNode("//Items");

                        if (itemsNode != null)
                        {
                            // Append the imported <SequenceItem> node as a child of <Items>
                            itemsNode.AppendChild(importedNode);

                            destinationXmlDoc.Save(destinationFilePath);

                            // Fix empty elements after saving changes
                            FixEmptyElements(destinationFilePath);
                        }
                        else
                        {
                            MessageBox.Show("No <Items> element found in destination XML file.");
                        }
                    }

                    //MessageBox.Show("SequenceItem elements copied successfully.");
                }
                else
                {
                    MessageBox.Show($"No <SequenceItem> element with PatternSetupName '{patternSetupName}' and UserName '{userName}' found in source XML file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while replacing SequenceItem elements: {ex.Message}");
            }
        }

        private void FixEmptyElements(string filePath)
        {
            try
            {
                // Load the XML document
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filePath);

                // Traverse through all elements
                foreach (XmlNode node in xmlDoc.SelectNodes("//*"))
                {
                    // Check if the element is empty (has no child nodes)
                    if (node.ChildNodes.Count == 0 && node.InnerText.Trim() == "")
                    {
                        // Add an empty text node
                        node.InnerText = "";
                    }
                }

                // Save the modified XML document with single-line empty elements
                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "\t",
                    NewLineHandling = NewLineHandling.None
                };

                using (XmlWriter writer = XmlWriter.Create(filePath, settings))
                {
                    xmlDoc.Save(writer);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while fixing empty elements: {ex.Message}");
            }
        }
    }
}
