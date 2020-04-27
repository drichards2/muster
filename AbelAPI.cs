using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Muster
{
    internal class AbelAPI
    {
        private IntPtr AbelHandle;

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

        public const int numberOfBells = 16;
        public static int[] RequiredVersion = { 3, 10, 2 };

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

        public bool IsAbelConnected()
        {
            return AbelHandle != IntPtr.Zero;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

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

        private bool CheckCompatibility(string version)
        {
            // Check compatibility based on the first three parts of the version number
            string[] verParts = version.Split('.');
            return int.Parse(verParts[0]) > RequiredVersion[0] ||
                int.Parse(verParts[0]) == RequiredVersion[0] && int.Parse(verParts[1]) > RequiredVersion[1] ||
                int.Parse(verParts[0]) == RequiredVersion[0] && int.Parse(verParts[1]) == RequiredVersion[1] && int.Parse(verParts[2]) >= RequiredVersion[2];
        }

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_CHAR = 0x102;

        public void SendKeystroke(char keyStroke)
        {
            if (AbelHandle != null)
            {
                PostMessage(AbelHandle, WM_CHAR, keyStroke, 0);
            }
        }

        public void SendRingingEvent(RingingEvent evt)
        {
            if (IsValidAbelCommand(evt))
            {
                SendKeystroke(evt.ToChar());
            }
        }

        public bool IsValidAbelCommand(RingingEvent ringingEvent)
        {
            return RingingCommands.ContainsKey(ringingEvent.ToString());
        }

        public void RingBell(char keyStroke)
        {
            if (IsValidAbelKeystroke(keyStroke))
            {
                SendKeystroke(keyStroke);
            }
        }

        public bool IsValidAbelKeystroke(char key)
        {
            return RingingCommands.ContainsValue(key);
        }

        public RingingEvent FindEventForCommand(string command)
        {
            if (RingingCommands.ContainsKey(command))
            {
                return new RingingEvent(command, RingingCommands[command]);
            }
            else
                return null;
        }

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