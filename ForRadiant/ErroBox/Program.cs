// Copyright (C) 2014-2015, phamtuan Research Inc.
//  
// All rights are reserved. Reproduction or transmission in whole or in part, in any form or by
// any means, electronic, mechanical or otherwise, is prohibited without the prior written
// consent of the copyright owner.
// ---------------------------------------------------------------------------------

#region

using System;
using System.Windows.Forms;

#endregion

namespace ErrorBox
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmDemo());
        }
    }
}