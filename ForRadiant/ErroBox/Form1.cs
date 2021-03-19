// Copyright (C) 2014-2015, phamtuan Research Inc.
//  
// All rights are reserved. Reproduction or transmission in whole or in part, in any form or by
// any means, electronic, mechanical or otherwise, is prohibited without the prior written
// consent of the copyright owner.
// ---------------------------------------------------------------------------------

#region

using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ErrorBox;

#endregion

namespace ErrorBox
{
    public partial class frmDemo : Form
    {
       
        public frmDemo()
        {

            InitializeComponent();
     
        }

        private void btnLoad_Click(object sender, EventArgs e)
        {
            ErrorBox.Show("Are you sure you want to exit?", "Error", ErrorBox.Buttons.OK, ErrorBox.Icon.Error,ErrorBox.AnimateStyle.FadeIn);
           
            //DialogResult result = ErrorBox.Show("Are you sure you want to exit?", "Exit", ErrorBox.Buttons.YesNoCancel,
            //    ErrorBox.Icon.Shield, ErrorBox.AnimateStyle.FadeIn);

            //if (result == DialogResult.Yes)
            //{
            //    MessageBox.Show("Exiting now");
            //}
        }

        

       
    }
}