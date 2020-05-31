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
    internal class AbelAPI
    {
        /// <summary>   Handle of the Abel process. </summary>
        private IntPtr AbelHandle;

        /// <summary>   The ringing commands. </summary>
        private Dictionary<string, char> RingingCommands = new Dictionary<string, char>
        {
            {"Go", 'S' },
            {"Bob", 'T' },
            {"Single", 'U' },
            {"ThatsAll", 'V' },
            {"Rounds", 'W' },
            {"Stand", 'X' },
            {"ResetBells", 'Y' }
        };

        /// <summary>   Number of bells. </summary>
        public const int numberOfBells = 16;
        /// <summary>   The required version. </summary>
        public static int[] RequiredVersion = { 3, 10, 2 };

        /// <summary>   Default constructor. </summary>
        public AbelAPI()
        {
            int command = 0;
            for (int i = 0; i < numberOfBells; i++)
            {
                // Skip F and J
                if ((char)('A' + command) == 'F' || (char)('A' + command) == 'J')
                    command++;

                RingingCommands.Add((i + 1).ToString(), (char)('A' + command++));
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if Abel process is connected. </summary>
        ///
        /// <returns>   True if Abel is connected, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsAbelConnected()
        {
            return AbelHandle != IntPtr.Zero;
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
        public void FindAbel()
        {
            var foundHandle = IntPtr.Zero;
            // Inspired by the Abel connection in Graham John's Handbell Manager (https://github.com/GACJ/handbellmanager)
            Process[] currentProcesses = Process.GetProcesses();
            foreach (Process p in currentProcesses)
            {
                if (Convert.ToString(p.ProcessName).ToUpper() == "ABEL3")
                {
                    foundHandle = p.MainWindowHandle;
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

            if (foundHandle != AbelHandle)
            {
                AbelHandle = foundHandle;
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
            if (AbelHandle != null)
            {
                PostMessage(AbelHandle, WM_CHAR, keyStroke, 0);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sends a ringing event. </summary>
        ///
        /// <param name="evt">  The event. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SendRingingEvent(RingingEvent evt)
        {
            if (IsValidAbelCommand(evt))
            {
                SendKeystroke(evt.ToChar());
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if 'ringingEvent' is a valid simulator command. </summary>
        ///
        /// <param name="ringingEvent"> The ringing event. </param>
        ///
        /// <returns>   True if valid abel command, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsValidAbelCommand(RingingEvent ringingEvent)
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
            if (IsValidAbelKeystroke(keyStroke))
            {
                SendKeystroke(keyStroke);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if 'key' is a valid simulator keystroke. </summary>
        ///
        /// <param name="key">  The key. </param>
        ///
        /// <returns>   True if valid abel keystroke, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsValidAbelKeystroke(char key)
        {
            return RingingCommands.ContainsValue(key);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Searches for the corresponding ringing event for a command. </summary>
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
        /// <summary>   Searches for the corresponding ringing event for a keystroke. </summary>
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