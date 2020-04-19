using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Muster
{
    class Robot
    {
        public int numBells = 6;
        public List<bool> shouldRing = new List<bool>{ true, true, false, false, true, true };
        public int interbellGap = 200;
        public float HSGRatio = 0.7F;

        public List<int> bellOrder = new List<int>();

        public bool ReceiveNotification(Tuple<DateTime, RingingEvent, bool> input)
        {
            Debug.WriteLine($"{input.Item1.ToString("hh:mm:ss.ffff")}, {input.Item2}");
            return true;
        }

        public AbelAPI simulator;

        public void LoadRows(string filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                // Read the stream to a string, and write the string to the console.
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    foreach (char c in line)
                        bellOrder.Add(c - '0');
                }
            }
        }

        public Func<RingingEvent, bool> SendBellStrike { get; set; }

        public async Task Start()
        {
            bool isHS = true;
            int index = 0;
            while (index < bellOrder.Count - numBells + 1)
            {
                for (int idxBell = 0; idxBell < numBells; idxBell++)
                {
                    int bell = bellOrder[index++];
                    if (shouldRing[bell - 1])
                    {
                        RingingEvent ringingEvent = simulator.FindEventForCommand((bell).ToString());
                        SendBellStrike(ringingEvent);
                        Debug.WriteLine($"Ringing bell {bell} at {DateTime.Now}");
                    }
                    Thread.Sleep(interbellGap);
                }

                if (!isHS)
                    Thread.Sleep((int)(HSGRatio * interbellGap));

                isHS = !isHS;
            }
        }
    }
}
