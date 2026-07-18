using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinSCP;

namespace FTPUPloaderCS
{
    public partial class MainForm : Form
    {
        public string apppath;
        public string appdir;
        public string settingPath;
        public bool IsUploading;
        public Task uploadTask;
        public CancellationTokenSource TasksCancellationTokenSource = new CancellationTokenSource();
        public Stopwatch sw;
        private bool allowVisible = true;

        public MainForm()
        {
            apppath = Assembly.GetExecutingAssembly().Location;
            appdir = Path.GetDirectoryName(apppath);
            settingPath = Path.Combine(appdir, "settingFTPUploader.txt");
            InitializeComponent();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SHOWME)
            {
                ShowMe();
            }
            base.WndProc(ref m);
        }

        private void ShowMe()
        {
            Show();
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
            }
            bool top = TopMost;
            TopMost = true;
            TopMost = top;
        }

        private void SetVersionInfo()
        {
            Version versionInfo = Assembly.GetExecutingAssembly().GetName().Version;
            DateTime startDate = new DateTime(2000, 1, 1);
            int diffDays = versionInfo.Build;
            DateTime computedDate = startDate.AddDays(diffDays);
            Text = string.Format("{0} - {1}", Text, versionInfo.ToString());
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!allowVisible)
            {
                value = false;
                if (!IsHandleCreated)
                {
                    CreateHandle();
                }
            }
            base.SetVisibleCore(value);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            SetVersionInfo();
            LoadSettings();
            RestartTask();
            if (startMinimizedToolStripMenuItem.Checked)
            {
                WindowState = FormWindowState.Minimized;
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("dh.chau@radiantvs.com");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void exit2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (minimizedToTrayToolStripMenuItem.Checked && WindowState == FormWindowState.Minimized)
            {
                Hide();
                ShowInTaskbar = false;
                notifyIcon1.BalloonTipText = "FTPUploader still running and minimized to tray";
                notifyIcon1.ShowBalloonTip(100);
            }
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            allowVisible = true;
            Show();
            Activate();
            ShowInTaskbar = true;
            WindowState = FormWindowState.Normal;
        }

        public void Upload(string InfoFile)
        {
            if (!File.Exists(InfoFile))
            {
                return;
            }

            bool uploaded = false;
            string logContent = string.Empty;
            string[] lines = File.ReadAllLines(InfoFile);
            string host = lines[0];
            string username = lines[1];
            string password = lines[2];
            string exePath = lines[3];
            string sessionLogPath = lines[4];
            string succeedLogPath = lines[5];
            string failLogPath = lines[6];
            string sourceFile = lines[7];
            string destFile = lines[8];
            string sourceIndexFile = lines[10];
            string sourceHostFile = lines[13];
            string totalFileCount = lines[15];
            string channelIndex = lines[16];
            string PID = Path.GetFileNameWithoutExtension(sourceIndexFile);

            string failCountPath = Path.Combine(txtUploadListPath.Text, "Fail Count", Path.GetFileName(InfoFile));
            string summaryLogPath = Path.Combine(txtUploadListPath.Text, "Log", DateTime.Now.ToString("yyyyMMdd") + "_summary.csv");

            Random m_Rnd = new Random();
            Color tempcolor = lblFileUploadStatus.ForeColor;
            while (lblFileUploadStatus.ForeColor == tempcolor)
            {
                lblFileUploadStatus.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255));
            }

            if (!Directory.Exists(Path.GetDirectoryName(succeedLogPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(succeedLogPath));
            }
            if (!Directory.Exists(Path.GetDirectoryName(failLogPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(failLogPath));
            }

            lblFileStatus.Invoke((Action)(() => lblFileStatus.Text = "Uploading " + sourceFile + " ..."));

            try
            {
                if (TasksCancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                if (sourceFile == sourceIndexFile || sourceFile == sourceHostFile)
                {
                    return;
                }

                var sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Ftp,
                    HostName = host,
                    UserName = username,
                    Password = password,
                    TimeoutInMilliseconds = 10000
                };

                using (var session = new Session())
                {
                    session.ExecutablePath = exePath;
                    if (!Directory.Exists(Path.GetDirectoryName(sessionLogPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(sessionLogPath));
                    }
                    session.SessionLogPath = sessionLogPath;
                    session.Open(sessionOptions);

                    var transferOptions = new TransferOptions
                    {
                        TransferMode = TransferMode.Binary
                    };

                    TransferOperationResult transferResult = session.PutFiles(sourceFile, destFile, false, transferOptions);
                    transferResult.Check();
                }

                uploaded = true;
                lblFileUploadStatus.Invoke((Action)(() => lblFileUploadStatus.Text = "Succeeded "));

                logContent = Text + "\t" + DateTime.Now.ToString("HH:mm:ss.fff") + "\tUpload succeeded " + sourceFile + " to: " + "ftp://" + host + destFile + Environment.NewLine;
                File.AppendAllText(succeedLogPath, logContent);
                if (TasksCancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                File.AppendAllText(sourceIndexFile, destFile + "@" + channelIndex + Environment.NewLine);
                File.AppendAllText(sourceHostFile, destFile + "@" + channelIndex + Environment.NewLine);
                int uploadedCount = File.ReadAllLines(sourceHostFile).Length;
                if (uploadedCount == int.Parse(totalFileCount))
                {
                    CreateIndexAndHostQueue(InfoFile);
                }
            }
            catch (Exception e)
            {
                lblFileUploadStatus.Invoke((Action)(() => lblFileUploadStatus.Text = "Failed "));
                logContent = Text + "\t" + DateTime.Now.ToString("HH:mm:ss.fff") + "\tUpload failed with exception : " + e.Message + sourceFile + " to: " + "ftp://" + host + destFile + Environment.NewLine;
                File.AppendAllText(failLogPath, logContent);

                if (!Directory.Exists(Path.GetDirectoryName(failCountPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(failCountPath));
                }
                int failCount = 0;
                int failRetry = 0;
                if (!File.Exists(failCountPath))
                {
                    failCount = 1;
                    File.WriteAllText(failCountPath, "1");
                }
                else
                {
                    string[] failLines = File.ReadAllLines(failCountPath);
                    failCount = int.Parse(failLines[0]);
                    failCount += 1;
                    File.WriteAllText(failCountPath, failCount.ToString());
                }

                if (!int.TryParse(txtMaximumFailRetry.Text, out failRetry))
                {
                    failRetry = 0;
                }
                if (failCount >= failRetry)
                {
                    if (TasksCancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    File.AppendAllText(sourceIndexFile, destFile + "@" + channelIndex + " - failed" + Environment.NewLine);
                    File.AppendAllText(sourceHostFile, destFile + "@" + channelIndex + " - failed" + Environment.NewLine);

                    logContent = Text + "\t" + DateTime.Now.ToString("HH:mm:ss.fff") + "\tMaximum fail count reached (" + failCount.ToString() + "/" + failRetry.ToString() + "), deleting queue: " + sourceFile + " to: " + "ftp://" + host + destFile + Environment.NewLine;
                    File.AppendAllText(failLogPath, logContent);

                    if (backupQueueAfterUploadToolStripMenuItem.Checked)
                    {
                        BackupInfoFile(InfoFile, "Backedup Failed Queue");
                    }
                    File.Delete(InfoFile);
                    File.Delete(failCountPath);
                    UpdateSummaryLogFail(summaryLogPath, PID, destFile, e.Message);
                }
            }

            if (uploaded)
            {
                if (backupQueueAfterUploadToolStripMenuItem.Checked)
                {
                    BackupInfoFile(InfoFile, "Backedup Succeed Queue");
                }
                File.Delete(InfoFile);
                UpdateSummaryLogSucceed(summaryLogPath, PID, destFile);
                if (File.Exists(failCountPath))
                {
                    File.Delete(failCountPath);
                }
            }
        }

        public void UploadIndexAndHost(string InfoFile)
        {
            if (!File.Exists(InfoFile))
            {
                return;
            }

            bool uploaded = false;
            string logContent = string.Empty;
            string[] lines = File.ReadAllLines(InfoFile);
            string host = lines[0];
            string username = lines[1];
            string password = lines[2];
            string exePath = lines[3];
            string sessionLogPath = lines[4];
            string succeedLogPath = lines[5];
            string failLogPath = lines[6];
            string sourceFile = lines[7];
            string destFile = lines[8];

            string failCountPath = Path.Combine(txtUploadListPath.Text, "Fail Count", "IndexHost", Path.GetFileName(InfoFile));

            Random m_Rnd = new Random();
            Color tempcolor = lblFileUploadStatus.ForeColor;
            while (lblFileUploadStatus.ForeColor == tempcolor)
            {
                lblFileUploadStatus.ForeColor = Color.FromArgb(255, m_Rnd.Next(0, 255), m_Rnd.Next(0, 255), m_Rnd.Next(0, 255));
            }

            if (!Directory.Exists(Path.GetDirectoryName(succeedLogPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(succeedLogPath));
            }
            if (!Directory.Exists(Path.GetDirectoryName(failLogPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(failLogPath));
            }

            lblFileStatus.Invoke((Action)(() => lblFileStatus.Text = "Uploading " + sourceFile + " ..."));
            try
            {
                if (TasksCancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }

                var sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Ftp,
                    HostName = host,
                    UserName = username,
                    Password = password,
                    TimeoutInMilliseconds = 20000
                };

                using (var session = new Session())
                {
                    session.ExecutablePath = exePath;
                    if (!Directory.Exists(Path.GetDirectoryName(sessionLogPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(sessionLogPath));
                    }
                    session.SessionLogPath = sessionLogPath;
                    session.Open(sessionOptions);

                    var transferOptions = new TransferOptions
                    {
                        TransferMode = TransferMode.Binary
                    };

                    TransferOperationResult transferResult = session.PutFiles(sourceFile, destFile, false, transferOptions);
                    transferResult.Check();
                }

                uploaded = true;
                lblFileUploadStatus.Invoke((Action)(() => lblFileUploadStatus.Text = "Succeeded "));
                logContent = DateTime.Now.ToString("HH:mm:ss.fff") + "\tUpload succeeded " + sourceFile + " to: " + "ftp://" + host + destFile + Environment.NewLine;
                File.AppendAllText(succeedLogPath, logContent);
            }
            catch (Exception e)
            {
                lblFileUploadStatus.Invoke((Action)(() => lblFileUploadStatus.Text = "Failed "));
                logContent = DateTime.Now.ToString("HH:mm:ss.fff") + "\tUpload failed with exception : " + e.Message + sourceFile + " to: " + "ftp://" + host + destFile + Environment.NewLine;
                File.AppendAllText(failLogPath, logContent);

                if (!Directory.Exists(Path.GetDirectoryName(failCountPath)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(failCountPath));
                }
                int failCount = 0;
                int failRetry = 0;
                if (!File.Exists(failCountPath))
                {
                    failCount = 1;
                    File.WriteAllText(failCountPath, "1");
                }
                else
                {
                    string[] failLines = File.ReadAllLines(failCountPath);
                    failCount = int.Parse(failLines[0]);
                    failCount += 1;
                    File.WriteAllText(failCountPath, failCount.ToString());
                }

                if (!int.TryParse(txtMaximumFailRetry.Text, out failRetry))
                {
                    failRetry = 0;
                }
                if (failCount >= failRetry)
                {
                    logContent = DateTime.Now.ToString("HH:mm:ss.fff") + "\tMaximum fail count reached (" + failCount.ToString() + "/" + failRetry.ToString() + "), deleting queue: " + sourceFile + " to: " + "ftp://" + host + destFile + Environment.NewLine;
                    File.AppendAllText(failLogPath, logContent);
                    if (backupQueueAfterUploadToolStripMenuItem.Checked)
                    {
                        BackupInfoFile(InfoFile, "Backedup Failed Queue\\IndexHost");
                    }
                    File.Delete(InfoFile);
                    File.Delete(failCountPath);
                }
                return;
            }

            if (uploaded)
            {
                if (backupQueueAfterUploadToolStripMenuItem.Checked)
                {
                    BackupInfoFile(InfoFile, "Backedup Succeed Queue\\IndexHost");
                }
                File.Delete(InfoFile);
                if (File.Exists(failCountPath))
                {
                    File.Delete(failCountPath);
                }
            }
        }

        private void CreateIndexAndHostQueue(string InfoFile)
        {
            if (!File.Exists(InfoFile))
            {
                return;
            }
            string[] lines = File.ReadAllLines(InfoFile);
            string OutputIndexInfoFile = lines[9];
            string OutputHostInfoFile = lines[12];
            string sourceIndexFile = lines[10];
            string sourceHostFile = lines[13];

            if (TasksCancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            RemoveFailedLines(sourceIndexFile);
            RemoveFailedLines(sourceHostFile);

            if (!File.Exists(OutputIndexInfoFile) && OutputIndexInfoFile != InfoFile)
            {
                lines[7] = lines[10];
                lines[8] = lines[11];
                File.WriteAllLines(OutputIndexInfoFile, lines);
            }
            UploadIndexAndHost(OutputIndexInfoFile);

            if (!File.Exists(OutputHostInfoFile) && OutputHostInfoFile != InfoFile)
            {
                lines[7] = lines[13];
                lines[8] = lines[14];
                File.WriteAllLines(OutputHostInfoFile, lines);
            }
            UploadIndexAndHost(OutputHostInfoFile);
        }

        private void UpdateSummaryLogFail(string summaryLogFile, string PID, string destFile, string failMessage)
        {
            if (!File.Exists(summaryLogFile))
            {
                return;
            }

            string failedFileName = string.Empty;
            string failedFileReason = string.Empty;
            int spacePos = failMessage.IndexOf('.');
            if (spacePos >= 0)
            {
                failMessage = failMessage.Substring(0, spacePos);
            }

            string fileNameLower = Path.GetFileNameWithoutExtension(destFile).ToLower();
            if (fileNameLower.Contains("otp"))
            {
                failedFileName = "OTP";
                failedFileReason = "OTP_X";
            }
            else if (fileNameLower.Contains("gamma"))
            {
                failedFileName = "GAMMA";
                failedFileReason = "GAMMA_X";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("1g1o") && fileNameLower.Contains("map"))
            {
                failedFileName = "HEX_MAP";
                failedFileReason = "HEX_MAP_X";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("1g1o") && fileNameLower.Contains("rcb"))
            {
                failedFileName = "HEX_RCB";
                failedFileReason = "HEX_RCB_X";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("1g1o"))
            {
                failedFileName = "HEX";
                failedFileReason = "HEX_X";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("1st"))
            {
                failedFileName = "HEX_1ST";
                failedFileReason = "HEX_1ST_X";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("2nd"))
            {
                failedFileName = "HEX_2ND";
                failedFileReason = "HEX_2ND_X";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("3rd"))
            {
                failedFileName = "HEX_3RD";
                failedFileReason = "HEX_3RD_X";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("4th"))
            {
                failedFileName = "HEX_4TH";
                failedFileReason = "HEX_4TH_X";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("5th"))
            {
                failedFileName = "HEX_5TH";
                failedFileReason = "HEX_5TH_X";
            }
            else if (Path.GetFileNameWithoutExtension(destFile).Contains("step2_03"))
            {
                failedFileName = Path.GetFileNameWithoutExtension(destFile).Replace("step2_03_", "").Replace("_imgY_Crop", "");
                failedFileReason = failedFileName + "_X";
            }
            else
            {
                failedFileName = Path.GetFileNameWithoutExtension(destFile).Replace("_imgY_Crop", "");
                failedFileReason = failedFileName + "_X";
            }

            string[] lines = File.ReadAllLines(summaryLogFile);
            string[] columnHeader = lines[0].Split(',');
            int failedFileIndex = 1;
            int failedReasonIndex = 1;
            bool columnAndFileNameMatch = false;
            bool columnAndFileReasonMatch = false;
            for (int index = 0; index < columnHeader.Length; index++)
            {
                if (columnHeader[index] == failedFileName.ToUpper())
                {
                    failedFileIndex = index;
                    columnAndFileNameMatch = true;
                    break;
                }
            }
            for (int index = 0; index < columnHeader.Length; index++)
            {
                if (columnHeader[index] == failedFileReason.ToUpper())
                {
                    failedReasonIndex = index;
                    columnAndFileReasonMatch = true;
                    break;
                }
            }
            if (columnAndFileNameMatch)
            {
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] newlineList = lines[i].Split(',');
                    if (newlineList.Contains(PID))
                    {
                        newlineList[failedFileIndex] = "X";
                    }
                    lines[i] = string.Join(",", newlineList);
                }
                File.WriteAllLines(summaryLogFile, lines);
            }
            if (columnAndFileReasonMatch)
            {
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] newlineList = lines[i].Split(',');
                    if (newlineList.Contains(PID))
                    {
                        newlineList[failedReasonIndex] = failMessage;
                    }
                    lines[i] = string.Join(",", newlineList);
                }
                File.WriteAllLines(summaryLogFile, lines);
            }
        }

        private void UpdateSummaryLogSucceed(string summaryLogFile, string PID, string destFile)
        {
            if (!File.Exists(summaryLogFile))
            {
                return;
            }

            string succeededFileName;
            string fileNameLower = Path.GetFileNameWithoutExtension(destFile).ToLower();
            if (fileNameLower.Contains("otp"))
            {
                succeededFileName = "OTP";
            }
            else if (fileNameLower.Contains("gamma"))
            {
                succeededFileName = "GAMMA";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("1g1o") && fileNameLower.Contains("map"))
            {
                succeededFileName = "HEX_MAP";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("1g1o") && fileNameLower.Contains("rcb"))
            {
                succeededFileName = "HEX_RCB";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("1g1o"))
            {
                succeededFileName = "HEX";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("1st"))
            {
                succeededFileName = "HEX_1ST";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("2nd"))
            {
                succeededFileName = "HEX_2ND";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("3rd"))
            {
                succeededFileName = "HEX_3RD";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("4th"))
            {
                succeededFileName = "HEX_4TH";
            }
            else if (fileNameLower.Contains("nypucdata") && fileNameLower.Contains("5th"))
            {
                succeededFileName = "HEX_5TH";
            }
            else if (Path.GetFileNameWithoutExtension(destFile).Contains("step2_03"))
            {
                succeededFileName = Path.GetFileNameWithoutExtension(destFile).Replace("step2_03_", "").Replace("_imgY_Crop", "");
            }
            else
            {
                succeededFileName = Path.GetFileNameWithoutExtension(destFile).Replace("_imgY_Crop", "");
            }

            string[] lines = File.ReadAllLines(summaryLogFile);
            string[] columnHeader = lines[0].Split(',');
            bool columnAndFileMatch = false;
            int succeededFileIndex = 1;
            for (int index = 0; index < columnHeader.Length; index++)
            {
                if (columnHeader[index] == succeededFileName.ToUpper())
                {
                    succeededFileIndex = index;
                    columnAndFileMatch = true;
                    break;
                }
            }
            if (columnAndFileMatch)
            {
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] newlineList = lines[i].Split(',');
                    if (newlineList.Contains(PID))
                    {
                        newlineList[succeededFileIndex] = "O";
                    }
                    lines[i] = string.Join(",", newlineList);
                }
                File.WriteAllLines(summaryLogFile, lines);
            }
        }

        private void UploadAll()
        {
            if (!Directory.Exists(txtUploadListPath.Text))
            {
                try
                {
                    Directory.CreateDirectory(txtUploadListPath.Text);
                }
                catch (Exception)
                {
                    lblFileStatus.Invoke((Action)(() => lblFileStatus.Text = "Queue Folder not existed, try again ..."));
                    return;
                }
            }

            while (!TasksCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    lblStatus.Invoke((Action)(() => lblStatus.Text = "Uploading files ..."));

                    string root = txtUploadListPath.Text;
                    int maximumUpload = int.Parse(txtMaximumUpload.Text);
                    IEnumerable<string> uploadList = Directory.EnumerateFiles(root, "*.txt")
                        .OrderByDescending(x => File.GetCreationTime(x))
                        .Take(maximumUpload);

                    foreach (string info in uploadList)
                    {
                        if (TasksCancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                        Upload(info);
                    }

                    lblStatus.Invoke((Action)(() => lblStatus.Text = "Uploading finished !"));
                }
                catch (Exception ex)
                {
                    lblStatus.Invoke((Action)(() => lblStatus.Text = "Error uploading : " + ex.Message));
                }
                finally
                {
                    lblStatus.Invoke((Action)(() => lblStatus.Text = "Reset timer for uploading ."));
                }

                int checkTime;
                if (!int.TryParse(txtInterval.Text, out checkTime))
                {
                    checkTime = 1;
                }

                sw = new Stopwatch();
                sw.Start();
                while (sw.ElapsedMilliseconds < 1000 * checkTime)
                {
                    if (TasksCancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    Thread.Sleep(100);
                }
            }
        }

        private void RestartTask()
        {
            TasksCancellationTokenSource = new CancellationTokenSource();
            uploadTask = new Task(new Action(UploadAll), TasksCancellationTokenSource.Token);
            uploadTask.Start();
            cmdStartUpload.Enabled = false;
            cmdStopUpload.Enabled = true;
            txtInterval.Enabled = false;
            txtMaximumUpload.Enabled = false;
            txtUploadListPath.Enabled = false;
            txtMaximumFailRetry.Enabled = false;
        }

        private void StopTask()
        {
            if (TasksCancellationTokenSource != null)
            {
                TasksCancellationTokenSource.Cancel();
            }
            if (uploadTask != null)
            {
                if (uploadTask.Status != TaskStatus.RanToCompletion)
                {
                    Stopwatch wait = new Stopwatch();
                    wait.Start();
                    while (uploadTask.Status != TaskStatus.RanToCompletion)
                    {
                        if (uploadTask.Status == TaskStatus.Canceled || uploadTask.Status == TaskStatus.Faulted)
                        {
                            break;
                        }
                        if (wait.ElapsedMilliseconds > 1000)
                        {
                            break;
                        }
                    }
                    wait.Stop();
                }
                if (uploadTask.IsCompleted || uploadTask.IsCanceled || uploadTask.IsFaulted)
                {
                    uploadTask.Dispose();
                }
                uploadTask = null;
            }
            cmdStartUpload.Enabled = true;
            cmdStopUpload.Enabled = false;
            txtInterval.Enabled = true;
            txtMaximumUpload.Enabled = true;
            txtUploadListPath.Enabled = true;
            txtMaximumFailRetry.Enabled = true;
        }

        private void cmdStartUpload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RestartTask();
        }

        private void cmdStopUpload_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StopTask();
        }

        private void SaveSettings()
        {
            var settings = new Dictionary<string, string>();
            settings.Add("startminimized", startMinimizedToolStripMenuItem.Checked ? "true" : "false");
            settings.Add("minimizedtotray", minimizedToTrayToolStripMenuItem.Checked ? "true" : "false");

            string settingContent = string.Empty;
            foreach (string k in settings.Keys)
            {
                settingContent += k + "=" + settings[k] + Environment.NewLine;
            }
            try
            {
                File.WriteAllText(settingPath, settingContent);
            }
            catch (Exception)
            {
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(settingPath))
                {
                    string[] settings = File.ReadAllLines(settingPath);
                    foreach (string line in settings)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length < 2)
                        {
                            continue;
                        }
                        string setting = parts[0];
                        string value = parts[1];
                        if (setting == "startminimized")
                        {
                            startMinimizedToolStripMenuItem.Checked = value == "true";
                        }
                        else if (setting == "minimizedtotray")
                        {
                            minimizedToTrayToolStripMenuItem.Checked = value == "true";
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        private void RemoveFailedLines(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            string[] allLines = File.ReadAllLines(filePath);
            string[] cleanedLines = allLines.Where(line => !line.Contains(" - failed")).ToArray();
            if (cleanedLines.Length != allLines.Length)
            {
                File.WriteAllLines(filePath, cleanedLines);
            }
        }

        private void BackupInfoFile(string infoFilePath, string backupFolder)
        {
            if (!File.Exists(infoFilePath))
            {
                return;
            }

            try
            {
                string backupPath = Path.Combine(txtUploadListPath.Text, backupFolder);
                if (!Directory.Exists(backupPath))
                {
                    Directory.CreateDirectory(backupPath);
                }

                string backupFileName = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff") + "_" + Path.GetFileName(infoFilePath);
                string fullBackupPath = Path.Combine(backupPath, backupFileName);
                File.Copy(infoFilePath, fullBackupPath, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Backup failed: " + ex.Message);
            }
        }

    }

    internal static class NativeMethods
    {
        public const int HWND_BROADCAST = 0xFFFF;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }
}
