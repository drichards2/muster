using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public bool ReceiveNotification(Tuple<DateTime, RingingEvent, bool> input)
        {
            Debug.WriteLine($"{input.Item1.ToString("hh:mm:ss.ffff")}, {input.Item2}");
            return true;
        }

        public AbelAPI simulator;

        public Func<RingingEvent, bool> SendBellStrike { get; set; }

        public Task Start()
        {
            bool isHS = true;
            while (true)
            {
                for (int idxBell = 0; idxBell < numBells; idxBell++)
                {
                    int bell = idxBell;
                    if (shouldRing[bell])
                    {
                        RingingEvent ringingEvent = simulator.FindEventForCommand((bell+1).ToString());
                        SendBellStrike(ringingEvent);
                        Debug.WriteLine($"Ringing bell {idxBell+1} at {DateTime.Now}");
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
