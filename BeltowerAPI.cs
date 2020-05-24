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
    /// <summary>   An Beltower a pi. </summary>
    internal class BeltowerAPI
    {
        /// <summary>   Handle of the Beltower. </summary>
        private IntPtr BeltowerHandle;

        /// <summary>   The ringing commands. </summary>
        private Dictionary<string, char> RingingCommandToChar = new Dictionary<string, char>
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

        /// <summary>   Number of bells. </summary>
        public const int numberOfBells = 16;
        /// <summary>   The required version. </summary>
        public static int[] RequiredVersion = { 1, 0, 0 };

        /// <summary>   Default constructor. </summary>
        public BeltowerAPI()
        {
            for (int i = 0; i < 9; i++)
            {
                RingingCommandToChar.Add((i + 1).ToString(), (char)(i + '1'));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if this  is Beltower connected. </summary>
        ///
        /// <returns>   True if Beltower connected, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsConnected()
        {
            return BeltowerHandle != IntPtr.Zero;
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
        public void FindInstance()
        {
            var foundHandle = IntPtr.Zero;
            // Inspired by the Beltower connection in Graham John's Handbell Manager (https://github.com/GACJ/handbellmanager)
            Process[] currentProcesses = Process.GetProcesses();
            foreach (Process p in currentProcesses)
            {
                if (Convert.ToString(p.ProcessName).ToUpper() == "BELTOW95" || Convert.ToString(p.ProcessName).ToUpper() == "BELTUTOR")
                {
                    foundHandle = p.MainWindowHandle;
                    // var version = p.MainModule.FileVersionInfo.FileVersion;
                    var IsCompatible = true; // CheckCompatibility(version);

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

                    if (IsCompatible && foundHandle != IntPtr.Zero)
                        break;
                }
            }

            if (foundHandle != BeltowerHandle)
            {
                BeltowerHandle = foundHandle;
            }
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
        /// <summary>   Posts a message. </summary>
        ///
        /// <param name="hWnd">     The window. </param>
        /// <param name="Msg">      The message. </param>
        /// <param name="wParam">   The parameter. </param>
        /// <param name="lParam">   The parameter. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        /// <summary>   The windows message keydown. </summary>
        const int WM_KEYDOWN = 0x100;
        /// <summary>   The windows message keyup. </summary>
        const int WM_KEYUP = 0x101;
        /// <summary>   The windows message character. </summary>
        const int WM_CHAR = 0x102;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sends a keystroke. </summary>
        ///
        /// <param name="keyStroke">    The key stroke. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SendKeystroke(char keyStroke)
        {
            if (BeltowerHandle != null)
            {
                PostMessage(BeltowerHandle, WM_CHAR, keyStroke, 0);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sends a ringing event. </summary>
        ///
        /// <param name="evt">  The event. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SendRingingEvent(RingingEvent evt)
        {
            if (IsValidCommand(evt))
            {
                SendKeystroke(evt.ToChar());
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if 'ringingEvent' is valid Beltower command. </summary>
        ///
        /// <param name="ringingEvent"> The ringing event. </param>
        ///
        /// <returns>   True if valid Beltower command, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsValidCommand(RingingEvent ringingEvent)
        {
            return RingingCommands.ContainsKey(ringingEvent.ToString());
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Ring bell. </summary>
        ///
        /// <param name="keyStroke">    The key stroke. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void RingBell(char keyStroke)
        {
            if (IsValidKeystroke(keyStroke))
            {
                SendKeystroke(keyStroke);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if 'key' is valid Beltower keystroke. </summary>
        ///
        /// <param name="key">  The key. </param>
        ///
        /// <returns>   True if valid Beltower keystroke, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsValidKeystroke(char key)
        {
            return RingingCommands.ContainsValue(key);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Searches for the first event for command. </summary>
        ///
        /// <param name="command">  The command. </param>
        ///
        /// <returns>   The found event for command. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public RingingEvent FindEventForCommand(string command)
        {
            if (RingingCommands.ContainsKey(command))
            {
                return new RingingEvent(command, RingingCommands[command]);
            }
            else
                return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Searches for the first event for keystroke. </summary>
        ///
        /// <param name="keyStroke">    The key stroke. </param>
        ///
        /// <returns>   The found event for keystroke. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public RingingEvent FindEventForKeystroke(char keyStroke)
        {
            var reversed = RingingCommands.ToDictionary(x => x.Value, x => x.Key);
            if (reversed.ContainsKey(keyStroke))
            {
                return new RingingEvent(reversed[keyStroke], keyStroke);
            }
            else
                return null;
        }
    }
}