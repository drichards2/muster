////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	RingingEvent.cs
//
// summary:	Implements the ringing event class
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Muster
{
    /// <summary>   A ringing event. </summary>
    public class RingingEvent
    {
        /// <summary>   The name. </summary>
        private string Name;
        /// <summary>   The message. </summary>
        private char Message;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Constructor. </summary>
        ///
        /// <param name="name">     The name. </param>
        /// <param name="message">  The message. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public RingingEvent(string name, char message)
        {
            Name = name;
            Message = message;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Returns a string that represents the current object. </summary>
        ///
        /// <returns>   A string that represents the current object. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public override string ToString()
        {
            return Name;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Converts this  to a character. </summary>
        ///
        /// <returns>   This  as a char. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public char ToChar()
        {
            return Message;
        }
    }
}
