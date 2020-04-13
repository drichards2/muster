using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Muster
{
    public class RingingEvent
    {
        private string Name;
        private char Message;

        public RingingEvent(string name, char message)
        {
            Name = name;
            Message = message;
        }

        public override string ToString()
        {
            return Name;
        }

        public char ToByte()
        {
            return Message;
        }
    }
}
