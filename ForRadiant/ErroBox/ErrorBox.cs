// Copyright (C) 2014-2015, phamtuan Research Inc.
//  
// All rights are reserved. Reproduction or transmission in whole or in part, in any form or by
// any means, electronic, mechanical or otherwise, is prohibited without the prior written
// consent of the copyright owner.
// ---------------------------------------------------------------------------------

#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

#endregion

namespace ErrorBox
{
    internal class ErrorBox : Form
    {
        // Bóng đổ
        private const int CS_DROPSHADOW = 0x00020000;

        private static ErrorBox _errorBox;

        // Header, Footer và Icon
        private Panel _plHeader = new Panel();
        private Label _lblTitle;
        private Panel _plFooter = new Panel();
        private Panel _plIcon = new Panel();
        private PictureBox _picIcon = new PictureBox();

        // THông điệp
        private Label _lblMessage;

        // Panel
        private FlowLayoutPanel _flpButtons = new FlowLayoutPanel();

        // List các button
        private List<Button> _buttonCollection = new List<Button>();

        // Kết quả
        private static DialogResult _buttonResult;

        // Timer hiệu ứng
        private static Timer _timer;

        // Phát tiếng Beep
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool MessageBeep(uint type);

        private ErrorBox()
        {
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.BackColor = Color.Red;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new System.Windows.Forms.Padding(3);
            this.Width = 400;

            // Header
            _lblTitle = new Label();
            _lblTitle.ForeColor = Color.White;
            _lblTitle.Font = new System.Drawing.Font("Segoe UI", 20);
            _lblTitle.Dock = DockStyle.Top;
            _lblTitle.Height = 60;

            // Thông điệp
            _lblMessage = new Label();
            _lblMessage.ForeColor = Color.White;
            _lblMessage.Font = new System.Drawing.Font("Segoe UI", 20);
            _lblMessage.Dock = DockStyle.Fill;

            _flpButtons.FlowDirection = FlowDirection.RightToLeft;
            _flpButtons.Dock = DockStyle.Fill;

            _plHeader.Dock = DockStyle.Fill;
            _plHeader.Padding = new Padding(20);
            _plHeader.Controls.Add(_lblMessage);
            _plHeader.Controls.Add(_lblTitle);

            _plFooter.Dock = DockStyle.Bottom;
            _plFooter.Padding = new Padding(20);
            _plFooter.BackColor = Color.Red;
            _plFooter.Height = 80;
            _plFooter.Controls.Add(_flpButtons);

            _picIcon.Width = 32;
            _picIcon.Height = 32;
            _picIcon.Location = new Point(30, 50);

            _plIcon.Dock = DockStyle.Left;
            _plIcon.Padding = new Padding(20);
            _plIcon.Width = 70;
            _plIcon.Controls.Add(_picIcon);

            // Add controls vào form
            this.Controls.Add(_plHeader);
            this.Controls.Add(_plIcon);
            this.Controls.Add(_plFooter);
        }

        public static void Show(string message)
        {
            _errorBox = new ErrorBox();
            _errorBox._lblMessage.Text = message;
            _errorBox.ShowDialog();
            MessageBeep(0);
        }

        public static void Show(string message, string title)
        {
            _errorBox = new ErrorBox();
            _errorBox._lblMessage.Text = message;
            _errorBox._lblTitle.Text = title;
            _errorBox.Size = ErrorBox.MessageSize(message);
            _errorBox.ShowDialog();
            MessageBeep(0);
            
        }

        public static DialogResult Show(string message, string title, Buttons buttons)
        {
            _errorBox = new ErrorBox();
            _errorBox._lblMessage.Text = message;
            _errorBox._lblTitle.Text = title;
            _errorBox._plIcon.Hide();

            ErrorBox.InitButtons(buttons);

            _errorBox.Size = ErrorBox.MessageSize(message);
            _errorBox.ShowDialog();
            MessageBeep(0);
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, Buttons buttons, Icon icon)
        {
            _errorBox = new ErrorBox();
            _errorBox._lblMessage.Text = message;
            _errorBox._lblTitle.Text = title;

            ErrorBox.InitButtons(buttons);
            ErrorBox.InitIcon(icon);

            _errorBox.Size = ErrorBox.MessageSize(message);
            _errorBox.ShowDialog();
            MessageBeep(0);
            return _buttonResult;
        }

        public static DialogResult Show(string message, string title, Buttons buttons, Icon icon, AnimateStyle style)
        {
            _errorBox = new ErrorBox();
            _errorBox._lblMessage.Text = message;
            _errorBox._lblTitle.Text = title;
            _errorBox.Height = 0;

            ErrorBox.InitButtons(buttons);
            ErrorBox.InitIcon(icon);

            _timer = new Timer();
            Size formSize = ErrorBox.MessageSize(message);

            switch (style)
            {
                case ErrorBox.AnimateStyle.SlideDown:
                    _errorBox.Size = new Size(formSize.Width, 0);
                    _timer.Interval = 1;
                    _timer.Tag = new AnimateerrorBox(formSize, style);
                    break;

                case ErrorBox.AnimateStyle.FadeIn:
                    _errorBox.Size = formSize;
                    _errorBox.Opacity = 0;
                    _timer.Interval = 20;
                    _timer.Tag = new AnimateerrorBox(formSize, style);
                    break;

                case ErrorBox.AnimateStyle.ZoomIn:
                    _errorBox.Size = new Size(formSize.Width + 100, formSize.Height + 100);
                    _timer.Tag = new AnimateerrorBox(formSize, style);
                    _timer.Interval = 1;
                    break;
            }

            _timer.Tick += timer_Tick;
            _timer.Start();

            _errorBox.ShowDialog();
            MessageBeep(0);
            return _buttonResult;
        }

        private static void timer_Tick(object sender, EventArgs e)
        {
            Timer timer = (Timer) sender;
            AnimateerrorBox animate = (AnimateerrorBox) timer.Tag;

            switch (animate.Style)
            {
                case ErrorBox.AnimateStyle.SlideDown:
                    if (_errorBox.Height < animate.FormSize.Height)
                    {
                        _errorBox.Height += 17;
                        _errorBox.Invalidate();
                    }
                    else
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                    break;

                case ErrorBox.AnimateStyle.FadeIn:
                    if (_errorBox.Opacity < 1)
                    {
                        _errorBox.Opacity += 0.1;
                        _errorBox.Invalidate();
                    }
                    else
                    {
                        _timer.Stop();
                        _timer.Dispose();
                    }
                    break;

                case ErrorBox.AnimateStyle.ZoomIn:
                    if (_errorBox.Width > animate.FormSize.Width)
                    {
                        _errorBox.Width -= 17;
                        _errorBox.Invalidate();
                    }
                    if (_errorBox.Height > animate.FormSize.Height)
                    {
                        _errorBox.Height -= 17;
                        _errorBox.Invalidate();
                    }
                    break;
            }
        }

        private static void InitButtons(Buttons buttons)
        {
            switch (buttons)
            {
                case ErrorBox.Buttons.AbortRetryIgnore:
                    _errorBox.InitAbortRetryIgnoreButtons();
                    break;

                case ErrorBox.Buttons.OK:
                    _errorBox.InitOKButton();
                    break;

                case ErrorBox.Buttons.OKCancel:
                    _errorBox.InitOKCancelButtons();
                    break;

                case ErrorBox.Buttons.RetryCancel:
                    _errorBox.InitRetryCancelButtons();
                    break;

                case ErrorBox.Buttons.YesNo:
                    _errorBox.InitYesNoButtons();
                    break;

                case ErrorBox.Buttons.YesNoCancel:
                    _errorBox.InitYesNoCancelButtons();
                    break;
            }

            foreach (Button btn in _errorBox._buttonCollection)
            {
                btn.ForeColor = Color.White;
                btn.Font = new System.Drawing.Font("Segoe UI", 8);
                btn.Padding = new Padding(3);
                btn.FlatStyle = FlatStyle.Flat;
                btn.Height = 30;
                btn.FlatAppearance.BorderColor = Color.White;

                _errorBox._flpButtons.Controls.Add(btn);
            }
        }

        private static void InitIcon(Icon icon)
        {
            switch (icon)
            {
                case ErrorBox.Icon.Application:
                    _errorBox._picIcon.Image = SystemIcons.Application.ToBitmap();
                    break;

                case ErrorBox.Icon.Exclamation:
                    _errorBox._picIcon.Image = SystemIcons.Exclamation.ToBitmap();
                    break;

                case ErrorBox.Icon.Error:
                    _errorBox._picIcon.Image = SystemIcons.Error.ToBitmap();
                    break;

                case ErrorBox.Icon.Info:
                    _errorBox._picIcon.Image = SystemIcons.Information.ToBitmap();
                    break;

                case ErrorBox.Icon.Question:
                    _errorBox._picIcon.Image = SystemIcons.Question.ToBitmap();
                    break;

                case ErrorBox.Icon.Shield:
                    _errorBox._picIcon.Image = SystemIcons.Shield.ToBitmap();
                    break;

                case ErrorBox.Icon.Warning:
                    _errorBox._picIcon.Image = SystemIcons.Warning.ToBitmap();
                    break;
            }
        }

        private void InitAbortRetryIgnoreButtons()
        {
            Button btnAbort = new Button();
            btnAbort.Text = "Abort";
            btnAbort.Click += ButtonClick;

            Button btnRetry = new Button();
            btnRetry.Text = "Retry";
            btnRetry.Click += ButtonClick;

            Button btnIgnore = new Button();
            btnIgnore.Text = "Ignore";
            btnIgnore.Click += ButtonClick;

            this._buttonCollection.Add(btnAbort);
            this._buttonCollection.Add(btnRetry);
            this._buttonCollection.Add(btnIgnore);
        }

        private void InitOKButton()
        {
            Button btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Click += ButtonClick;

            this._buttonCollection.Add(btnOK);
        }

        private void InitOKCancelButtons()
        {
            Button btnOK = new Button();
            btnOK.Text = "OK";
            btnOK.Click += ButtonClick;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;


            this._buttonCollection.Add(btnOK);
            this._buttonCollection.Add(btnCancel);
        }

        private void InitRetryCancelButtons()
        {
            Button btnRetry = new Button();
            btnRetry.Text = "OK";
            btnRetry.Click += ButtonClick;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;


            this._buttonCollection.Add(btnRetry);
            this._buttonCollection.Add(btnCancel);
        }

        private void InitYesNoButtons()
        {
            Button btnYes = new Button();
            btnYes.Text = "Yes";
            btnYes.Click += ButtonClick;

            Button btnNo = new Button();
            btnNo.Text = "No";
            btnNo.Click += ButtonClick;


            this._buttonCollection.Add(btnYes);
            this._buttonCollection.Add(btnNo);
        }

        private void InitYesNoCancelButtons()
        {
            Button btnYes = new Button();
            btnYes.Text = "Abort";
            btnYes.Click += ButtonClick;

            Button btnNo = new Button();
            btnNo.Text = "Retry";
            btnNo.Click += ButtonClick;

            Button btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Click += ButtonClick;

            this._buttonCollection.Add(btnYes);
            this._buttonCollection.Add(btnNo);
            this._buttonCollection.Add(btnCancel);
        }

        private static void ButtonClick(object sender, EventArgs e)
        {
            Button btn = (Button) sender;

            switch (btn.Text)
            {
                case "Abort":
                    _buttonResult = DialogResult.Abort;
                    break;

                case "Retry":
                    _buttonResult = DialogResult.Retry;
                    break;

                case "Ignore":
                    _buttonResult = DialogResult.Ignore;
                    break;

                case "OK":
                    _buttonResult = DialogResult.OK;
                    break;

                case "Cancel":
                    _buttonResult = DialogResult.Cancel;
                    break;

                case "Yes":
                    _buttonResult = DialogResult.Yes;
                    break;

                case "No":
                    _buttonResult = DialogResult.No;
                    break;
            }

            _errorBox.Dispose();
        }

        private static Size MessageSize(string message)
        {
            Graphics g = _errorBox.CreateGraphics();
            int width = Screen.PrimaryScreen.Bounds.Width;
            int height = 460;

            //SizeF size = g.MeasureString(message, new System.Drawing.Font("Segoe UI", 10));

            //if (message.Length < 150)
            //{
            //    if ((int) size.Width > 350)
            //    {
            //        width = (int) size.Width;
            //    }
            //}
            //else
            //{
            //    string[] groups = (from Match m in Regex.Matches(message, ".{1,180}") select m.Value).ToArray();
            //    int lines = groups.Length + 1;
            //    width = 700;
            //    height += (int) (size.Height + 10)*lines;
            //}
            return new Size(width, height);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle rect = new Rectangle(new Point(0, 0), new Size(this.Width - 1, this.Height - 1));
            Pen pen = new Pen(Color.FromArgb(0, 151, 251));

            g.DrawRectangle(pen, rect);
        }

        public enum Buttons
        {
            AbortRetryIgnore = 1,
            OK = 2,
            OKCancel = 3,
            RetryCancel = 4,
            YesNo = 5,
            YesNoCancel = 6
        }

        public enum Icon
        {
            Application = 1,
            Exclamation = 2,
            Error = 3,
            Warning = 4,
            Info = 5,
            Question = 6,
            Shield = 7,
            Search = 8
        }

        public enum AnimateStyle
        {
            SlideDown = 1,
            FadeIn = 2,
            ZoomIn = 3
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ErrorBox
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "ErrorBox";
            this.ResumeLayout(false);

        }

    }

    internal class AnimateerrorBox
    {
        public Size FormSize;
        public ErrorBox.AnimateStyle Style;

        public AnimateerrorBox(Size formSize, ErrorBox.AnimateStyle style)
        {
            this.FormSize = formSize;
            this.Style = style;
        }
    }


}