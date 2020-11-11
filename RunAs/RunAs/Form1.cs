using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Diagnostics;

namespace RunAs
{
    public partial class Form1 : Form
    {
        //private string filepath, domain, user, pass, hash, code, launcherfilepath;
        //private bool pathIsNetwork;
        string launchercode;
        TargetFile targetfile = new TargetFile();


        public Form1()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

        }


        private void btnBrowse_Click(object sender, EventArgs e)
        {

            var OF = new OpenFileDialog
            {
                InitialDirectory = Application.StartupPath,
                Filter = "All files (*) | *.*"
            };

            if (OF.ShowDialog() == DialogResult.OK)
            {
                OF.FilterIndex = 0;
                OF.RestoreDirectory = true;
                txtFilePath.Text = (OF.FileName);

            }

        }


        private void btnHelp_Click(object sender, EventArgs e)
        {
            Process.Start("readme.txt");
        }

        private void checkBoxHash_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxHash.Checked)
            {
                targetfile.FilePath = ConvertString.FormatFilePath(txtFilePath.Text);
                if (File.Exists(targetfile.FilePath))

                    txtHash.Text = ConvertString.calcSHA256(targetfile.FilePath);
                else
                {
                    DialogResult dialogResult = MessageBox.Show("Calculate from other file ?", "Cannot find target to calculate . . .", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        OpenFileDialog OF = new OpenFileDialog();



                        OF.InitialDirectory = Application.StartupPath;
                        OF.Filter = "All files (*) | *.*";


                        if (OF.ShowDialog() == DialogResult.OK)
                        {
                            OF.FilterIndex = 0;
                            OF.RestoreDirectory = true;
                            txtHash.Text = ConvertString.calcSHA256(OF.FileName);


                        }
                        else checkBoxHash.Checked = false;
                    }
                    else checkBoxHash.Checked = false;
                }

            }
            if (checkBoxHash.Checked == false) txtHash.Text = "";
        }

        private void checkBoxPCFilter_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxPCFilter.Checked == false)
            {

                btnPCFilter.Enabled = false;

            }
            else
            {

                btnPCFilter.Enabled = true;
            }
        }

        private void btnPCFilter_Click(object sender, EventArgs e)
        {
            Process.Start("pclist.ini");
        }

        private void radioButtonSameLocation_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButtonSameLocation.Checked == true)


                txtOtherLocation.Text = "";


        }

        private void radioButtonOtherLocation_CheckedChanged(object sender, EventArgs e)
        {
            
            if (radioButtonOtherLocation.Checked == true)
            {
                
                targetfile.FilePath = ConvertString.FormatFilePath(txtFilePath.Text);
                SaveFileDialog SF = new SaveFileDialog();
                // set a default file name
                SF.FileName = System.IO.Path.GetFileNameWithoutExtension(targetfile.FilePath) + " Launcher.exe";

                // set filters - this can be done in properties as well
                SF.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
                if (File.Exists(targetfile.FilePath))
                    SF.InitialDirectory = System.IO.Path.GetDirectoryName(targetfile.FilePath);
                SF.RestoreDirectory = true;

                if (SF.ShowDialog() == DialogResult.OK)
                {

                    txtOtherLocation.Text = SF.FileName;
                    
                }
                else
                {

                    txtOtherLocation.Text = "";
                    radioButtonSameLocation.Checked = true;
                }

            }

        }
        private void btnRun_Click(object sender, EventArgs e)
        {
            targetfile.FilePath = ConvertString.FormatFilePath(txtFilePath.Text);
            targetfile.PathIsNetWork = ConvertString.IsNetworkPath(targetfile.FilePath);
            if (txtDomain.Text == null || txtDomain.Text == "") targetfile.Domain = "";
                else targetfile.Domain = txtDomain.Text;
            if (txtUser.Text == null || txtUser.Text=="") targetfile.User = "";
                else targetfile.User = txtUser.Text;
            if (txtPass.Text == null || txtPass.Text == "") targetfile.Pass = "";
                else targetfile.Pass = txtPass.Text;
            //Run test
            if (File.Exists(targetfile.FilePath))

                try
                {
                    if (targetfile.PathIsNetWork)
                    {
                        string tempfilepath = Path.GetTempPath() + Path.GetFileName(targetfile.FilePath);
                        if (!File.Exists(tempfilepath)) File.Copy(targetfile.FilePath, tempfilepath);
                        targetfile.FilePath = tempfilepath;
                    }
                    Win32.LaunchCommand1(targetfile.FilePath, targetfile.Domain, targetfile.User, targetfile.Pass);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("LaunchCommand error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }


            else MessageBox.Show("Application not found or path is not absolute . . .", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            var OF = new OpenFileDialog
            {
                InitialDirectory = Application.StartupPath,
                Filter = "Credentials files (*.creds)|*.creds|All files (*.*)|*.*"
            };

            if (OF.ShowDialog() == DialogResult.OK)
            {
                OF.FilterIndex = 0;
                OF.RestoreDirectory = true;
                string[] lines = File.ReadAllLines(OF.FileName);

                // Read the file and display it line by line.  
                txtDomain.Text = ConvertString.DecodeFrom64(lines[0]);
                txtUser.Text = ConvertString.DecodeFrom64(lines[1]);
                txtPass.Text = ConvertString.DecodeFrom64(lines[2]);
            }
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            targetfile.FilePath = ConvertString.FormatFilePath(txtFilePath.Text);
            targetfile.PathIsNetWork = ConvertString.IsNetworkPath(targetfile.FilePath);
            if (txtDomain.Text == null || txtDomain.Text == "") targetfile.Domain = "";
            else targetfile.Domain = txtDomain.Text;
            if (txtUser.Text == null || txtUser.Text == "") targetfile.User = "";
            else targetfile.User = txtUser.Text;
            if (txtPass.Text == null || txtPass.Text == "") targetfile.Pass = "";
            else targetfile.Pass = txtPass.Text;
            if (checkBoxHash.Checked) targetfile.Hash = txtHash.Text;
            else targetfile.Hash = "";

            if (radioButtonSameLocation.Checked)
                if (File.Exists(targetfile.FilePath)) targetfile.LauncherFilePath = System.IO.Path.ChangeExtension(targetfile.FilePath, null) + " Launcher.exe";

                else
                {

                    MessageBox.Show("Cannot find save path, please choose other location . . .", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            if (radioButtonOtherLocation.Checked)
                targetfile.LauncherFilePath = txtOtherLocation.Text;
            //Create launcher
            var csc = new CSharpCodeProvider(new Dictionary<string, string>() { { "CompilerVersion", "v4.0" } });
            var parameters = new CompilerParameters(new[] { "mscorlib.dll", "System.Core.dll" }, targetfile.LauncherFilePath, true);
            parameters.GenerateExecutable = true;
            parameters.GenerateInMemory = true;

            string[] pclist = File.ReadAllLines(@"pclist.ini");
            string pclistitems = String.Join("\r\n", pclist);

            string[] lines = File.ReadAllLines(@"template.bin");
            for (int i = lines.Length - 1; i >= 0; i--)
            {
                lines[i] = ConvertString.DecodeFrom64(lines[i]);

            }


            if ((String.IsNullOrWhiteSpace(pclistitems) || checkBoxPCFilter.Checked == false) && (targetfile.Hash == null || targetfile.Hash == "" || checkBoxHash.Checked == false))
                for (int i = lines.Length - 1; i >= 0; i--)

                {


                    if (lines[i].Contains("hash"))
                        lines[i] = "";
                    if (lines[i].Contains("allowed"))
                        lines[i] = "";
                    if (lines[i].Contains("pcitems"))
                        lines[i] = "";

                    if (lines[i].Contains("Incorrect"))
                        lines[i] = "";


                }

            else if ((String.IsNullOrWhiteSpace(pclistitems) || checkBoxPCFilter.Checked == false) && !(targetfile.Hash == null || targetfile.Hash == "" || checkBoxHash.Checked == false))
                for (int i = lines.Length - 1; i >= 0; i--)

                {
                    if (lines[i].Contains("pcitems"))
                        lines[i] = "";
                    if (lines[i].Contains("if (hash == \"hashholder\" && allowed == true)"))
                        lines[i] = "if (hash == \"hashholder\")";
                    if (lines[i].Contains("allowed"))
                        lines[i] = "";


                }

            else if ((targetfile.Hash == null || targetfile.Hash == "" || checkBoxHash.Checked == false) && !(String.IsNullOrWhiteSpace(pclistitems) || checkBoxPCFilter.Checked == false))
                for (int i = lines.Length - 1; i >= 0; i--)

                {
                    if (lines[i].Contains("if (hash == \"hashholder\" && allowed == true)"))
                        lines[i] = "if (allowed == true)";
                    if (lines[i].Contains("hash"))
                        lines[i] = "";

                }



            launchercode = String.Join("\r\n", lines);


            launchercode = launchercode.Replace(
                      "nameholder",
                       targetfile.FilePath
                      );


            launchercode = launchercode.Replace(
                      "domainholder",
                      targetfile.Domain
                      );

            launchercode = launchercode.Replace(
                    "userholder",
                    targetfile.User
                    );

            launchercode = launchercode.Replace(
                   "passholder",
                   targetfile.Pass
                   );
            launchercode = launchercode.Replace(
                    "hashholder",
                    targetfile.Hash
                    );
            launchercode = launchercode.Replace(
                    "pcitemsholder",
                    pclistitems
                    );


            File.WriteAllText("launchercode.cs", launchercode);


            CompilerResults results = csc.CompileAssemblyFromSource(parameters, launchercode);
            results.Errors.Cast<CompilerError>().ToList().ForEach(error => Console.WriteLine(error.ErrorText));
            if (targetfile.LauncherFilePath != null && targetfile.LauncherFilePath != "")
            {
                Clipboard.SetText(targetfile.LauncherFilePath);
                MessageBox.Show(targetfile.LauncherFilePath + " created.\n\nPath copied to Clipboard !", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


        }



        private void btnExport_Click(object sender, EventArgs e)
        {
            SaveFileDialog SF = new SaveFileDialog();
            // set a default file name
            SF.FileName = "credentials.creds";
            // set filters - this can be done in properties as well
            SF.Filter = "Credentials files (*.creds)|*.creds|All files (*.*)|*.*";
            SF.InitialDirectory = Application.StartupPath;
            SF.RestoreDirectory = true;

            if (SF.ShowDialog() == DialogResult.OK)
            {
            if (txtDomain.Text == null || txtDomain.Text == "") targetfile.Domain = "";
            if (txtUser.Text == null || txtUser.Text=="") targetfile.User = "";
            if (txtPass.Text == null || txtPass.Text == "") targetfile.Pass = "";
                string lines = ConvertString.EncodeTo64(targetfile.Domain) + "\r\n" + ConvertString.EncodeTo64(targetfile.User) + "\r\n" + ConvertString.EncodeTo64(targetfile.Pass);
                using (StreamWriter sw = new StreamWriter(SF.FileName))
                    sw.WriteLine(lines);

            }
        }


    }
}
