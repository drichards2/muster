using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Abel
{
    internal class AbelAPI
    {
        private IntPtr AbelHandle;

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

                    string ChildWindow = "AfxMDIFrame140s";
                    string GrandchildWindow = "AfxFrameOrView140s";

                    foundHandle = FindWindowEx(foundHandle, IntPtr.Zero, ChildWindow, "");
                    foundHandle = FindWindowEx(foundHandle, IntPtr.Zero, GrandchildWindow, "");
                    if (foundHandle != IntPtr.Zero)
                        break;
                }
            }

            if (foundHandle != AbelHandle)
            {
                AbelHandle = foundHandle;
            }
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

        public static List<char> SpecifyValidAbelCommands()
        {
            List<char> validKeys = new List<char>();

            /*            // Return A-Y except F and J
                        for (char i = 'A'; i <= 'Y'; i++)
                            if (i != 'F' && i != 'J')
                                validKeys.Add(i);
            */

            // Return A-W
            for (char i = 'A'; i <= 'W'; i++)
                validKeys.Add(i);

            return validKeys;
        }
    }
}