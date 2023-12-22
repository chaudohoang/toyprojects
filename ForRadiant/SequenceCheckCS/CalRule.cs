using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace SequenceCheckCS
{

    public partial class CalRule
    {
        public SqlCeConnection conn;
        public SqlCeCommand cmdColorCalibration = new SqlCeCommand();
        public SqlCeDataAdapter daColorCalibration = new SqlCeDataAdapter();
        public DataSet dsColorCalibration = new DataSet();
        public DataTable dtColorCalibration = new DataTable();
        public SqlCeCommand cmdFlatFieldCalibration = new SqlCeCommand();
        public SqlCeDataAdapter daFlatFieldCalibration = new SqlCeDataAdapter();
        public DataSet dsFlatFieldCalibration = new DataSet();
        public DataTable dtFlatFieldCalibration = new DataTable();
        public SqlCeCommand cmdImgScaleCalibration = new SqlCeCommand();
        public SqlCeDataAdapter daImgScaleCalibration = new SqlCeDataAdapter();
        public DataSet dsImgScaleCalibration = new DataSet();
        public DataTable dtImgScaleCalibration = new DataTable();
        public string CameraSN = "";
        public XmlNode node3;
        public XmlNodeList nodes3;
        public object xmlDoc3 = new XmlDocument();
        public string exePath = My.MyProject.Application.Info.DirectoryPath;

        public CalRule()
        {

            // This call is required by the designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.

        }

        private void getCurrentCameraSN()
        {

            if (File.Exists(@"C:\Radiant Vision Systems Data\TrueTest\UserData\CameraSN.txt"))
            {
                var fileloaded = default(bool);
                while (!fileloaded)
                {
                    try
                    {
                        CameraSN = File.ReadAllText(@"C:\Radiant Vision Systems Data\TrueTest\UserData\CameraSN.txt");
                        fileloaded = true;
                    }
                    catch (Exception ex)
                    {
                        fileloaded = false;
                    }
                }

            }

        }

        public object GetConnect(object CameraSN)
        {
            string calFile1 = Path.Combine(@"C:\Radiant Vision Systems Data\Camera Data\Calibration Files", Conversions.ToString(Operators.AddObject(CameraSN, "_CalibrationDB.calx")));
            string calFile2 = Path.Combine(@"C:\Radiant Vision Systems Data\Camera Data\Calibration Files", Conversions.ToString(Operators.AddObject(Operators.AddObject("0", CameraSN), "_CalibrationDB.calx")));
            if (File.Exists(calFile1))
            {
                conn = new SqlCeConnection("Data Source=" + calFile1 + ";Max Database Size=4091");
            }
            else if (File.Exists(calFile2))
            {
                conn = new SqlCeConnection("Data Source=" + calFile2 + ";Max Database Size=4091");
            }
            else
            {
                conn = new SqlCeConnection(@"Data Source=C:\Radiant Vision Systems Data\Camera Data\Calibration Files\PM Calibration Demo Camera.calx;Max Database Size=4091");
            }

            return conn;
        }
        private void btnUseLastModified3_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence"))
            {
                var latestfile = new DirectoryInfo(@"C:\Radiant Vision Systems Data\TrueTest\Sequence").GetFiles().OrderByDescending(o => o.LastWriteTime).FirstOrDefault();
                txtFile3.Text = latestfile.FullName;
            }
        }

        private void btnBrowse3_Click(object sender, EventArgs e)
        {
            using (var frm = new OpenFileDialog())
            {
                if (frm.ShowDialog(this) == DialogResult.OK)
                {
                    txtFile3.Text = frm.FileName;
                }
            }
        }

        private void ShowColorCalRefs()
        {

            try
            {
                dsColorCalibration.Clear();
                conn = (SqlCeConnection)GetConnect(CameraSN);
                cmdColorCalibration = conn.CreateCommand();
                cmdColorCalibration.CommandText = "SELECT ColorCalibrationID, Description FROM ColorCalibrations";

                daColorCalibration.SelectCommand = cmdColorCalibration;
                daColorCalibration.Fill(dsColorCalibration, "ColorCalibrations");
                var newNoneRow = dsColorCalibration.Tables["ColorCalibrations"].NewRow();
                newNoneRow[0] = "0";
                newNoneRow[1] = "(None)";
                dsColorCalibration.Tables["ColorCalibrations"].Rows.InsertAt(newNoneRow, 0);
                var newFactoryRow = dsColorCalibration.Tables["ColorCalibrations"].NewRow();
                newFactoryRow[0] = "-1";
                newFactoryRow[1] = "Factory";
                dsColorCalibration.Tables["ColorCalibrations"].Rows.InsertAt(newFactoryRow, 0);
                ColorCalDataGridView1.DataSource = dsColorCalibration;
                ColorCalDataGridView1.DataMember = "ColorCalibrations";
                ColorCalDataGridView1.ReadOnly = true;
                ColorCalDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                ColorCalDataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                ColorCalDataGridView1.AllowUserToResizeColumns = true;
            }

            catch (Exception ex)
            {
                Interaction.MsgBox("Error: " + ex.Source + ": " + ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!");
            }
        }
        private void ShowFlatFieldCalRefs()
        {

            try
            {
                dsFlatFieldCalibration.Clear();
                conn = (SqlCeConnection)GetConnect(CameraSN);
                cmdFlatFieldCalibration = conn.CreateCommand();
                cmdFlatFieldCalibration.CommandText = "SELECT CalibrationID, CalibrationDesc FROM FlatFieldCalibration";

                daFlatFieldCalibration.SelectCommand = cmdFlatFieldCalibration;
                daFlatFieldCalibration.Fill(dsFlatFieldCalibration, "FlatFieldCalibration");
                var newNoneRow = dsFlatFieldCalibration.Tables["FlatFieldCalibration"].NewRow();
                newNoneRow[0] = "0";
                newNoneRow[1] = "(None)";
                dsFlatFieldCalibration.Tables["FlatFieldCalibration"].Rows.InsertAt(newNoneRow, 0);
                var newFactoryRow = dsFlatFieldCalibration.Tables["FlatFieldCalibration"].NewRow();
                newFactoryRow[0] = "-1";
                newFactoryRow[1] = "Factory";
                dsFlatFieldCalibration.Tables["FlatFieldCalibration"].Rows.InsertAt(newFactoryRow, 0);
                FlatFieldCalDataGridView1.DataSource = dsFlatFieldCalibration;
                FlatFieldCalDataGridView1.DataMember = "FlatFieldCalibration";
                FlatFieldCalDataGridView1.ReadOnly = true;
                FlatFieldCalDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                FlatFieldCalDataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                FlatFieldCalDataGridView1.AllowUserToResizeColumns = true;
            }

            catch (Exception ex)
            {
                Interaction.MsgBox("Error: " + ex.Source + ": " + ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!");
            }
        }
        private void ShowImgScaleCalRefs()
        {

            try
            {
                dsImgScaleCalibration.Clear();
                conn = (SqlCeConnection)GetConnect(CameraSN);
                cmdImgScaleCalibration = conn.CreateCommand();
                cmdImgScaleCalibration.CommandText = "SELECT ImageScalingCalibrationID, ImageScalingCalibrationDesc FROM ImageScalingCalibration";

                daImgScaleCalibration.SelectCommand = cmdImgScaleCalibration;
                daImgScaleCalibration.Fill(dsImgScaleCalibration, "ImageScalingCalibration");
                var newNoneRow = dsImgScaleCalibration.Tables["ImageScalingCalibration"].NewRow();
                newNoneRow[0] = "0";
                newNoneRow[1] = "(None)";
                dsImgScaleCalibration.Tables["ImageScalingCalibration"].Rows.InsertAt(newNoneRow, 0);
                var newFactoryRow = dsImgScaleCalibration.Tables["ImageScalingCalibration"].NewRow();
                newFactoryRow[0] = "-1";
                newFactoryRow[1] = "Factory";
                dsImgScaleCalibration.Tables["ImageScalingCalibration"].Rows.InsertAt(newFactoryRow, 0);
                ImgScaleCalDataGridView1.DataSource = dsImgScaleCalibration;
                ImgScaleCalDataGridView1.DataMember = "ImageScalingCalibration";
                ImgScaleCalDataGridView1.ReadOnly = true;
                ImgScaleCalDataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                ImgScaleCalDataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                ImgScaleCalDataGridView1.AllowUserToResizeColumns = true;
            }

            catch (Exception ex)
            {
                Interaction.MsgBox("Error: " + ex.Source + ": " + ex.Message, MsgBoxStyle.OkOnly, "Connection Error !!");
            }
        }

        private void CalRule_Load(object sender, EventArgs e)
        {
            txtFile3.Text = My.MyProject.Forms.MainForm.txtFile3.Text;
            getCurrentCameraSN();
        }

        private void txtFile3_TextChanged(object sender, EventArgs e)
        {
            ReloadColorCalRule();
            ShowColorCalRefs();
            ReloadFlatFieldCalRule();
            ShowFlatFieldCalRefs();
            ReloadImgScaleCalRule();
            ShowImgScaleCalRefs();
        }
        private void ReloadColorCalRule()
        {
            if (!Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules"))
            {
                Directory.CreateDirectory(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules");
            }
            string ColorCalRuleFilaName = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_colorcal.txt";
            string ColorCalRuleFilePath = Path.Combine(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", ColorCalRuleFilaName);

            string[] Fields;
            Fields = "Step,ColorCalibrationID".Split(',');
            int Cols = Fields.GetLength(0);
            var dt = new DataTable();
            for (int index = 0, loopTo = Cols - 1; index <= loopTo; index++)
                dt.Columns.Add(Fields[index]);
            if (File.Exists(ColorCalRuleFilePath))
            {
                string[] Lines = File.ReadAllLines(ColorCalRuleFilePath);
                DataRow dr;
                for (int index = 0, loopTo1 = Lines.GetLength(0) - 1; index <= loopTo1; index++)
                {
                    Fields = Lines[index].Split(',');
                    dr = dt.NewRow();
                    for (int f = 0, loopTo2 = Cols - 1; f <= loopTo2; f++)

                        dr[f] = Fields[f];
                    dt.Rows.Add(dr);
                }

            }

            ColorCalDataGridView2.DataSource = dt;
            ColorCalDataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ColorCalDataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            ColorCalDataGridView2.AllowUserToResizeColumns = true;
        }
        private void ReloadFlatFieldCalRule()
        {
            if (!Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules"))
            {
                Directory.CreateDirectory(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules");
            }
            string FlatFieldCalRuleFilaName = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_flatfieldcal.txt";
            string FlatFieldCalRuleFilePath = Path.Combine(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", FlatFieldCalRuleFilaName);

            string[] Fields;
            Fields = "Step,CalibrationID".Split(',');
            int Cols = Fields.GetLength(0);
            var dt = new DataTable();
            for (int index = 0, loopTo = Cols - 1; index <= loopTo; index++)
                dt.Columns.Add(Fields[index]);
            if (File.Exists(FlatFieldCalRuleFilePath))
            {
                string[] Lines = File.ReadAllLines(FlatFieldCalRuleFilePath);
                DataRow dr;
                for (int index = 0, loopTo1 = Lines.GetLength(0) - 1; index <= loopTo1; index++)
                {
                    Fields = Lines[index].Split(',');
                    dr = dt.NewRow();
                    for (int f = 0, loopTo2 = Cols - 1; f <= loopTo2; f++)

                        dr[f] = Fields[f];
                    dt.Rows.Add(dr);
                }

            }

            FlatFieldCalDataGridView2.DataSource = dt;
            FlatFieldCalDataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            FlatFieldCalDataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            FlatFieldCalDataGridView2.AllowUserToResizeColumns = true;
        }
        private void ReloadImgScaleCalRule()
        {
            if (!Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules"))
            {
                Directory.CreateDirectory(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules");
            }
            string ImgScaleCalRuleFilaName = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_imgscalecal.txt";
            string ImgScaleCalRuleFilePath = Path.Combine(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", ImgScaleCalRuleFilaName);

            string[] Fields;
            Fields = "Step,ImageScalingCalibrationID".Split(',');
            int Cols = Fields.GetLength(0);
            var dt = new DataTable();
            for (int index = 0, loopTo = Cols - 1; index <= loopTo; index++)
                dt.Columns.Add(Fields[index]);
            if (File.Exists(ImgScaleCalRuleFilePath))
            {
                string[] Lines = File.ReadAllLines(ImgScaleCalRuleFilePath);
                DataRow dr;
                for (int index = 0, loopTo1 = Lines.GetLength(0) - 1; index <= loopTo1; index++)
                {
                    Fields = Lines[index].Split(',');
                    dr = dt.NewRow();
                    for (int f = 0, loopTo2 = Cols - 1; f <= loopTo2; f++)

                        dr[f] = Fields[f];
                    dt.Rows.Add(dr);
                }

            }

            ImgScaleCalDataGridView2.DataSource = dt;
            ImgScaleCalDataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            ImgScaleCalDataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            ImgScaleCalDataGridView2.AllowUserToResizeColumns = true;
        }

        private void btnColorCalSaveCalRules_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules"))
            {
                Directory.CreateDirectory(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules");
            }
            string ColorCalRuleFilaName = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_colorcal.txt";
            string ColorCalRuleFilePath = Path.Combine(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", ColorCalRuleFilaName);
            var rows = from row in ColorCalDataGridView2.Rows.Cast<DataGridViewRow>()
                       where !row.IsNewRow
                       select Array.ConvertAll(row.Cells.Cast<DataGridViewCell>().ToArray(), c => c.Value is not null ? c.Value.ToString() : "");

            var rowsData = new List<string>();
            var stepData = new List<string>();
            var duplicateRows = new List<string>();
            foreach (var r in rows)
            {
                string rowdata = string.Join(",", r);
                string rowstep = r[0];
                if (stepData.Contains(rowstep))
                {
                    duplicateRows.Add(rowstep);
                }
                stepData.Add(rowstep);
                rowsData.Add(rowdata);
            }
            string saveContent = "";
            string duplicateContent = "";
            if (rowsData.Count > 0)
            {
                saveContent = string.Join(Constants.vbCrLf, rowsData);
            }
            if (duplicateRows.Count > 0)
            {
                duplicateContent = string.Join(",", duplicateRows);
            }

            var result = MessageBox.Show("Confirm saving ?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                if (!string.IsNullOrEmpty(duplicateContent))
                {
                    MessageBox.Show("Check duplicate rules : " + duplicateContent);
                }

                else if (string.IsNullOrEmpty(saveContent))
                {
                    var result2 = MessageBox.Show("Save empty rules ?", "Confirm saving", MessageBoxButtons.YesNo);
                    if (result2 == DialogResult.Yes)
                    {
                        File.WriteAllText(ColorCalRuleFilePath, saveContent);
                    }
                }

                else
                {
                    File.WriteAllText(ColorCalRuleFilePath, saveContent);
                }
            }


        }

        private void btnColorCalUseSampleRuleMono_Click(object sender, EventArgs e)
        {
            string[] Fields;
            Fields = "Step,ColorCalibrationID".Split(',');
            int Cols = Fields.GetLength(0);
            var dt = new DataTable();
            for (int index = 0, loopTo = Cols - 1; index <= loopTo; index++)
                dt.Columns.Add(Fields[index]);
            dt.Rows.Add("G255", "1");
            dt.Rows.Add("R255", "1");
            dt.Rows.Add("B255", "1");
            dt.Rows.Add("g36", "1");
            dt.Rows.Add("g32", "1");
            dt.Rows.Add("g216", "1");
            dt.Rows.Add("g192", "1");
            dt.Rows.Add("r36", "1");
            dt.Rows.Add("r32", "1");
            dt.Rows.Add("r216", "1");
            dt.Rows.Add("r192", "1");
            dt.Rows.Add("b36", "1");
            dt.Rows.Add("b32", "1");
            dt.Rows.Add("b216", "1");
            dt.Rows.Add("b192", "1");

            var result = MessageBox.Show("This will clear current rules ?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                ColorCalDataGridView2.DataSource = dt;
            }

        }

        private void bntColorCalUseSampleRuleColor_Click(object sender, EventArgs e)
        {
            string[] Fields;
            Fields = "Step,ColorCalibrationID".Split(',');
            int Cols = Fields.GetLength(0);
            var dt = new DataTable();
            for (int index = 0, loopTo = Cols - 1; index <= loopTo; index++)
                dt.Columns.Add(Fields[index]);
            dt.Rows.Add("G255", "1");
            dt.Rows.Add("R255", "1");
            dt.Rows.Add("B255", "1");
            dt.Rows.Add("W10", "1");
            dt.Rows.Add("W12", "1");
            dt.Rows.Add("W216", "1");
            dt.Rows.Add("W8", "2");
            dt.Rows.Add("W23", "3");
            dt.Rows.Add("W31", "3");
            dt.Rows.Add("W42", "3");

            var result = MessageBox.Show("This will clear current rules ?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                ColorCalDataGridView2.DataSource = dt;
            }
        }

        private void btnFlatFieldCalUseSampleRuleMono_Click(object sender, EventArgs e)
        {
            string[] Fields;
            Fields = "Step,CalibrationID".Split(',');
            int Cols = Fields.GetLength(0);
            var dt = new DataTable();
            for (int index = 0, loopTo = Cols - 1; index <= loopTo; index++)
                dt.Columns.Add(Fields[index]);
            dt.Rows.Add("G255", "-1");
            dt.Rows.Add("R255", "-1");
            dt.Rows.Add("B255", "-1");
            dt.Rows.Add("g36", "-1");
            dt.Rows.Add("g32", "-1");
            dt.Rows.Add("g216", "-1");
            dt.Rows.Add("g192", "-1");
            dt.Rows.Add("r36", "-1");
            dt.Rows.Add("r32", "-1");
            dt.Rows.Add("r216", "-1");
            dt.Rows.Add("r192", "-1");
            dt.Rows.Add("b36", "-1");
            dt.Rows.Add("b32", "-1");
            dt.Rows.Add("b216", "-1");
            dt.Rows.Add("b192", "-1");

            var result = MessageBox.Show("This will clear current rules ?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                FlatFieldCalDataGridView2.DataSource = dt;
            }
        }

        private void btnImgScaleCalUseSampleRuleMono_Click(object sender, EventArgs e)
        {
            string[] Fields;
            Fields = "Step,ImageScalingCaibrationlID".Split(',');
            int Cols = Fields.GetLength(0);
            var dt = new DataTable();
            for (int index = 0, loopTo = Cols - 1; index <= loopTo; index++)
                dt.Columns.Add(Fields[index]);
            dt.Rows.Add("G255", "-1");
            dt.Rows.Add("R255", "-1");
            dt.Rows.Add("B255", "-1");
            dt.Rows.Add("g36", "-1");
            dt.Rows.Add("g32", "-1");
            dt.Rows.Add("g216", "-1");
            dt.Rows.Add("g192", "-1");
            dt.Rows.Add("r36", "-1");
            dt.Rows.Add("r32", "-1");
            dt.Rows.Add("r216", "-1");
            dt.Rows.Add("r192", "-1");
            dt.Rows.Add("b36", "-1");
            dt.Rows.Add("b32", "-1");
            dt.Rows.Add("b216", "-1");
            dt.Rows.Add("b192", "-1");

            var result = MessageBox.Show("This will clear current rules ?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                ImgScaleCalDataGridView2.DataSource = dt;
            }
        }

        private void btnFlatFieldCalSaveCalRules_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules"))
            {
                Directory.CreateDirectory(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules");
            }
            string FlatFieldCalRuleFilaName = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_flatfieldcal.txt";
            string FlatFieldCalRuleFilePath = Path.Combine(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", FlatFieldCalRuleFilaName);
            var rows = from row in FlatFieldCalDataGridView2.Rows.Cast<DataGridViewRow>()
                       where !row.IsNewRow
                       select Array.ConvertAll(row.Cells.Cast<DataGridViewCell>().ToArray(), c => c.Value is not null ? c.Value.ToString() : "");

            var rowsData = new List<string>();
            var stepData = new List<string>();
            var duplicateRows = new List<string>();
            foreach (var r in rows)
            {
                string rowdata = string.Join(",", r);
                string rowstep = r[0];
                if (stepData.Contains(rowstep))
                {
                    duplicateRows.Add(rowstep);
                }
                stepData.Add(rowstep);
                rowsData.Add(rowdata);
            }
            string saveContent = "";
            string duplicateContent = "";
            if (rowsData.Count > 0)
            {
                saveContent = string.Join(Constants.vbCrLf, rowsData);
            }
            if (duplicateRows.Count > 0)
            {
                duplicateContent = string.Join(",", duplicateRows);
            }

            var result = MessageBox.Show("Confirm saving ?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                if (!string.IsNullOrEmpty(duplicateContent))
                {
                    MessageBox.Show("Check duplicate rules : " + duplicateContent);
                }

                else if (string.IsNullOrEmpty(saveContent))
                {
                    var result2 = MessageBox.Show("Save empty rules ?", "Confirm saving", MessageBoxButtons.YesNo);
                    if (result2 == DialogResult.Yes)
                    {
                        File.WriteAllText(FlatFieldCalRuleFilePath, saveContent);
                    }
                }

                else
                {
                    File.WriteAllText(FlatFieldCalRuleFilePath, saveContent);
                }
            }
        }

        private void btnImgScaleCalSaveCalRules_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules"))
            {
                Directory.CreateDirectory(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules");
            }
            string ImgScaleCalRuleFilaName = Path.GetFileNameWithoutExtension(txtFile3.Text) + "_imgscalecal.txt";
            string ImgScaleCalRuleFilePath = Path.Combine(@"C:\Radiant Vision Systems Data\TrueTest\Sequence\Calibration Rules", ImgScaleCalRuleFilaName);
            var rows = from row in ImgScaleCalDataGridView2.Rows.Cast<DataGridViewRow>()
                       where !row.IsNewRow
                       select Array.ConvertAll(row.Cells.Cast<DataGridViewCell>().ToArray(), c => c.Value is not null ? c.Value.ToString() : "");

            var rowsData = new List<string>();
            var stepData = new List<string>();
            var duplicateRows = new List<string>();
            foreach (var r in rows)
            {
                string rowdata = string.Join(",", r);
                string rowstep = r[0];
                if (stepData.Contains(rowstep))
                {
                    duplicateRows.Add(rowstep);
                }
                stepData.Add(rowstep);
                rowsData.Add(rowdata);
            }
            string saveContent = "";
            string duplicateContent = "";
            if (rowsData.Count > 0)
            {
                saveContent = string.Join(Constants.vbCrLf, rowsData);
            }
            if (duplicateRows.Count > 0)
            {
                duplicateContent = string.Join(",", duplicateRows);
            }

            var result = MessageBox.Show("Confirm saving ?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                if (!string.IsNullOrEmpty(duplicateContent))
                {
                    MessageBox.Show("Check duplicate rules : " + duplicateContent);
                }

                else if (string.IsNullOrEmpty(saveContent))
                {
                    var result2 = MessageBox.Show("Save empty rules ?", "Confirm saving", MessageBoxButtons.YesNo);
                    if (result2 == DialogResult.Yes)
                    {
                        File.WriteAllText(ImgScaleCalRuleFilePath, saveContent);
                    }
                }

                else
                {
                    File.WriteAllText(ImgScaleCalRuleFilePath, saveContent);
                }
            }
        }

        private void btnColorCalReloadCalRules_Click(object sender, EventArgs e)
        {
            ReloadColorCalRule();
        }

        private void btnFlatFieldCalReloadCalRules_Click(object sender, EventArgs e)
        {
            ReloadFlatFieldCalRule();
        }

        private void btnImgScaleCalReloadCalRules_Click(object sender, EventArgs e)
        {
            ReloadImgScaleCalRule();
        }

        private void bntImgScaleCalUseSampleRuleColor_Click(object sender, EventArgs e)
        {
            string[] Fields;
            Fields = "Step,ImageScalingCalibrationID".Split(',');
            int Cols = Fields.GetLength(0);
            var dt = new DataTable();
            for (int index = 0, loopTo = Cols - 1; index <= loopTo; index++)
                dt.Columns.Add(Fields[index]);
            dt.Rows.Add("G255", "-1");
            dt.Rows.Add("R255", "-1");
            dt.Rows.Add("B255", "-1");
            dt.Rows.Add("W10", "-1");
            dt.Rows.Add("W12", "-1");
            dt.Rows.Add("W216", "-1");
            dt.Rows.Add("W8", "-1");
            dt.Rows.Add("W23", "-1");
            dt.Rows.Add("W31", "-1");
            dt.Rows.Add("W42", "-1");

            var result = MessageBox.Show("This will clear current rules ?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                ImgScaleCalDataGridView2.DataSource = dt;
            }
        }

        private void bntFlatFieldCalUseSampleRuleColor_Click(object sender, EventArgs e)
        {
            string[] Fields;
            Fields = "Step,CalibrationID".Split(',');
            int Cols = Fields.GetLength(0);
            var dt = new DataTable();
            for (int index = 0, loopTo = Cols - 1; index <= loopTo; index++)
                dt.Columns.Add(Fields[index]);
            dt.Rows.Add("G255", "-1");
            dt.Rows.Add("R255", "-1");
            dt.Rows.Add("B255", "-1");
            dt.Rows.Add("W10", "-1");
            dt.Rows.Add("W12", "-1");
            dt.Rows.Add("W216", "-1");
            dt.Rows.Add("W8", "-1");
            dt.Rows.Add("W23", "-1");
            dt.Rows.Add("W31", "-1");
            dt.Rows.Add("W42", "-1");

            var result = MessageBox.Show("This will clear current rules ?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                FlatFieldCalDataGridView2.DataSource = dt;
            }
        }
    }
}