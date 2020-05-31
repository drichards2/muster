////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	AbelAPI.cs
//
// summary:	Implements the Abel API class
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Muster
{
    /// <summary>   An Abel API. </summary>
    internal class AbelAPI : SimulatorAPI
    {
        /// <summary>   Abel specific map from ringing events to characters. </summary>
        private Dictionary<string, char> Abel_RingingCommandToChar = new Dictionary<string, char>
        {
            {"Go", 'S' },
            {"Bob", 'T' },
            {"Single", 'U' },
            {"ThatsAll", 'V' },
            {"Rounds", 'W' },
            {"Stand", 'X' },
            {"ResetBells", 'Y' }
        };

        /// <summary>   The required version. </summary>
        public static int[] RequiredVersion = { 3, 10, 2 };

        /// <summary>   Default constructor. </summary>
        public AbelAPI()
        {
            Name = "Abel";

            // Map ringing events to A-Y missing out F and J
            // Add bells 1-16
            int command = 0;
            for (int i = 0; i < numberOfBells; i++)
            {
                // Skip F and J
                if ((char)('A' + command) == 'F' || (char)('A' + command) == 'J')
                    command++;

                RingingCommandToChar.Add((i + 1).ToString(), (char)('A' + command++));
            }

            // Add remaining commands
            foreach (string key in Abel_RingingCommandToChar.Keys)
                RingingCommandToChar.Add(key, Abel_RingingCommandToChar[key]);

            // For all these commands, key.ToString()[0] matches the desired character.
            // Therefore, there's no need to add keys to KeypressToRingingCommand.
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Searches for the window. </summary>
        ///
        /// <param name="hWndParent">       The window parent. </param>
        /// <param name="hWndChildAfter">   The window child after. </param>
        /// <param name="lpszClass">        The class. </param>
        /// <param name="lpszWindow">       The window. </param>
        ///
        /// <returns>   The found window. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>   Searches for an Abel process. </summary>
        public override bool FindInstance()
        {
            var foundHandle = IntPtr.Zero;
            // Inspired by the Abel connection in Graham John's Handbell Manager (https://github.com/GACJ/handbellmanager)
            Process[] currentProcesses = Process.GetProcesses();
            foreach (Process p in currentProcesses)
            {
                if (Convert.ToString(p.ProcessName).ToUpper() == "ABEL3")
                {
                    foundHandle = p.MainWindowHandle;
                    if (foundHandle == IntPtr.Zero)
                        break;
                    var version = p.MainModule.FileVersionInfo.FileVersion;
                    var IsCompatible = CheckCompatibility(version);

                    string ChildWindow = "AfxMDIFrame140s";
                    string GrandchildWindow = "AfxFrameOrView140s";

                    foundHandle = FindWindowEx(foundHandle, IntPtr.Zero, ChildWindow, "");
                    foundHandle = FindWindowEx(foundHandle, IntPtr.Zero, GrandchildWindow, "");

                    if (IsCompatible && foundHandle != IntPtr.Zero)
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
        /// <summary>   Check compatibility. </summary>
        ///
        /// <param name="version">  The version. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool CheckCompatibility(string version)
        {
            // Check compatibility based on the first three parts of the version number
            string[] verParts = version.Split('.');
            return int.Parse(verParts[0]) > RequiredVersion[0] ||
                int.Parse(verParts[0]) == RequiredVersion[0] && int.Parse(verParts[1]) > RequiredVersion[1] ||
                int.Parse(verParts[0]) == RequiredVersion[0] && int.Parse(verParts[1]) == RequiredVersion[1] && int.Parse(verParts[2]) >= RequiredVersion[2];
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
                PostMessage(SimulatorHandle, WM_CHAR, keyStroke, 0);
            }
        }
    }
}