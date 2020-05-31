////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	SimulatorAPI.cs
//
// summary:	Implements the Simulator API class
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Muster
{
    /// <summary>   A simulator API. </summary>
    internal abstract class SimulatorAPI
    {
        /// <summary>   Simulator name. </summary>
        public string Name = "";

        /// <summary>   Number of bells. </summary>
        public const int numberOfBells = 16;

        /// <summary>   Handle of the simulator process. </summary>
        protected IntPtr SimulatorHandle = IntPtr.Zero;

        /// <summary>   Map from ringing events to characters. </summary>
        protected Dictionary<string, char> RingingCommandToChar = new Dictionary<string, char>();

        /// <summary>   Map from keypress to ringing command where required. </summary>
        protected Dictionary<string, string> KeypressToRingingCommand = new Dictionary<string, string>();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if simulator process is connected. </summary>
        ///
        /// <returns>   True if simulator is connected, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsConnected()
        {
            return SimulatorHandle != IntPtr.Zero;
        }

        /// <summary>   Searches for the simulator process. </summary>
        public abstract bool FindInstance();

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
        protected static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        /// <summary>   The Windows message keydown. </summary>
        protected const int WM_KEYDOWN = 0x100;
        /// <summary>   The Windows message keyup. </summary>
        protected const int WM_KEYUP = 0x101;
        /// <summary>   The Windows message character. </summary>
        protected const int WM_CHAR = 0x102;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sends a keystroke. </summary>
        ///
        /// <param name="keyStroke">    The key stroke. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected abstract void SendKeystroke(char keyStroke);

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
        /// <summary>   Query if 'ringingEvent' is a valid simulator command. </summary>
        ///
        /// <param name="ringingEvent"> The ringing event. </param>
        ///
        /// <returns>   True if a valid simulator command, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsValidCommand(RingingEvent ringingEvent)
        {
            return RingingCommandToChar.ContainsKey(ringingEvent.ToString());
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
        /// <summary>   Query if 'key' is a valid simulator keystroke. </summary>
        ///
        /// <param name="key">  The key. </param>
        ///
        /// <returns>   True if a valid simulator keystroke, false if not. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsValidKeystroke(char key)
        {
            return RingingCommandToChar.ContainsValue(key);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Map a command to a ringing event. </summary>
        ///
        /// <param name="command">  The command. </param>
        ///
        /// <returns>   The ringing event for the command. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public RingingEvent FindEventForCommand(string command)
        {
            if (RingingCommandToChar.ContainsKey(command))
            {
                return new RingingEvent(command, RingingCommandToChar[command]);
            }
            else
                return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Map a character to a ringing event. </summary>
        ///
        /// <param name="keyChar">    The character. </param>
        ///
        /// <returns>   The ringing event for the character. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public RingingEvent FindEventForChar(char keyChar)
        {
            var reversed = RingingCommandToChar.ToDictionary(x => x.Value, x => x.Key);
            if (reversed.ContainsKey(keyChar))
            {
                return new RingingEvent(reversed[keyChar], keyChar);
            }
            else
                return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Map a keypress to a ringing event. </summary>
        ///
        /// <param name="keyStroke">    The keypress. </param>
        ///
        /// <returns>   The ringing event for the keypress. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public RingingEvent FindEventForKey(string keyCode)
        {
            if (KeypressToRingingCommand.ContainsKey(keyCode))
            {
                // The char sent over the network does not match the key directly
                //   Apply mapping
                var command = KeypressToRingingCommand[keyCode];
                return FindEventForCommand(command);
            }
            else if (keyCode.Length == 1)
            {
                // The char sent over the network matches the key press.
                return FindEventForChar(keyCode[0]);
            }
            else
                return null;
        }
    }
}