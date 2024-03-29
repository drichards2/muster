﻿////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	MusterPgm.cs
//
// summary:	Implements the Muster pgm class
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Muster
{
    /// <summary>   A Muster pgm. </summary>
    static class MusterPgm
    {
        /// <summary>   The main entry point for the application. </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Muster());
        }
    }
}
