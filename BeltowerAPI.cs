////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	BeltowerAPI.cs
//
// summary:	Implements the Beltower a pi class
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Muster
{
    /// <summary>   A Beltower API. </summary>
    internal class BeltowerAPI : SimulatorAPI
    {
        /// <summary>   Beltower map from ringing commands to characters. </summary>
        private Dictionary<string, char> BelTower_RingingCommandToChar = new Dictionary<string, char>
        {
            {"10", '0' },
            {"11", '-' },
            {"12", '=' },
            {"13", '[' },
            {"14", ']' },
            {"15", '\'' },
            {"16", '#' },
            {"Bob", 'B' },
            {"Single", 'S' },
            {"Go", 'G' },
            {"ThatsAll", 'T' },
            {"Rounds", 'R' },
            {"Stand", 'X' }
        };

        /// <summary>   Beltower map from keypresses to ringing commands. </summary>
        private Dictionary<string, string> BelTower_KeypressToRingingCommand = new Dictionary<string, string>
        {
            // Specify any mappings that are needed, where Key.ToString()[0] is not the correct char.
            {"D1", "1"},
            {"D2", "2"},
            {"D3", "3"},
            {"D4", "4"},
            {"D5", "5"},
            {"D6", "6"},
            {"D7", "7"},
            {"D8", "8"},
            {"D9", "9"},
            {"D0", "10"},
            {"OemMinus", "11"},
            {"Oemplus", "12"},
            {"OemOpenBrackets", "13"},
            {"Oem6", "14"},
            {"Oemtilde", "15"},
            {"Oem7", "16"},
        };

        /// <summary>   Default constructor. </summary>
        public BeltowerAPI()
        {
            Name = "Beltower";

            for (int i = 0; i < 9; i++)
            {
                RingingCommandToChar.Add((i + 1).ToString(), (char)(i + '1'));
            }

            foreach (string key in BelTower_RingingCommandToChar.Keys)
                RingingCommandToChar.Add(key, BelTower_RingingCommandToChar[key]);

            foreach (string key in BelTower_KeypressToRingingCommand.Keys)
                KeypressToRingingCommand.Add(key, BelTower_KeypressToRingingCommand[key]);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Searches for the first window exception. </summary>
        ///
        /// <param name="hWndParent">       The window parent. </param>
        /// <param name="hWndChildAfter">   The window child after. </param>
        /// <param name="lpszClass">        The class. </param>
        /// <param name="lpszWindow">       The window. </param>
        ///
        /// <returns>   The found window exception. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        /// <summary>   Searches for the first Beltower. </summary>
        public override bool FindInstance()
        {
            var foundHandle = IntPtr.Zero;
            // Inspired by the Beltower connection in Graham John's Handbell Manager (https://github.com/GACJ/handbellmanager)
            Process[] currentProcesses = Process.GetProcesses();
            foreach (Process p in currentProcesses)
            {
                if (Convert.ToString(p.ProcessName).ToUpper() == "BELTOW95" || Convert.ToString(p.ProcessName).ToUpper() == "BELTUTOR")
                {
                    foundHandle = p.MainWindowHandle;

                    while (true)
                    {
                        StringBuilder name = new StringBuilder(256);
                        GetClassName(foundHandle, name, 256);
                        if (name.ToString() == "ThunderRT6MDIForm")
                            break;
                        uint GW_HWNDPREV = 3;
                        foundHandle = GetWindow(foundHandle, GW_HWNDPREV);
                    }

                    string ChildWindow = "MDIClient";
                    string GrandchildWindow = "ThunderRT6FormDC";

                    foundHandle = FindWindowEx(foundHandle, IntPtr.Zero, ChildWindow, "");
                    foundHandle = FindWindowEx(foundHandle, IntPtr.Zero, GrandchildWindow, "Changes");

                    if (foundHandle != IntPtr.Zero)
                        break;
                }
            }

            if (foundHandle != SimulatorHandle)
            {
                SimulatorHandle = foundHandle;
            }

            return SimulatorHandle != IntPtr.Zero;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sends a keystroke. </summary>
        ///
        /// <param name="keyStroke">    The key stroke. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected override void SendKeystroke(char keyStroke)
        {
            if (SimulatorHandle != null)
            {
                if (keyStroke != 'X')
                    PostMessage(SimulatorHandle, WM_CHAR, keyStroke, 0);
                else
                    PostMessage(SimulatorHandle, WM_KEYDOWN, keyStroke, 0);
            }
        }
    }
}