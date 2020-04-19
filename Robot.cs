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
        public List<bool> shouldRing = new List<bool> { true, true, false, false, true, true };
        public double interbellGap = 200;
        public double HSGRatio = 0.9F;

        public List<int> bellOrder = new List<int>();

        private DateTime lastStrike = DateTime.MinValue;

        public static int HISTORY_SIZE = 5;
        public double gain = 0.8F;

        private List<double> prevHumanGaps = new List<double>(HISTORY_SIZE);
        private int strikeCount = 0;

        public bool ReceiveNotification(Tuple<DateTime, RingingEvent, bool> input)
        {
            var strikeTime = input.Item1;

            bool isBell = input.Item2.ToChar() <= 'P';
            if (isBell)
            {
                int bell = int.Parse(input.Item2.ToString());
                if (!shouldRing[bell - 1] && lastStrike != DateTime.MinValue)
                {
                    var delta = strikeTime - lastStrike;
                    double gap = delta.TotalMilliseconds;

                    // Account for handstroke-gap
                    if (strikeCount == 0)
                        gap /= (1 + HSGRatio);

                    // Don't include large mistakes
                    //    This doesn't ever trigger as it needs knowledge of the expected strike order
                    if (gap < 2 * interbellGap)
                        prevHumanGaps.Insert(0, gap);

                    double average = prevHumanGaps.Average();
                    var change = gain * (average - interbellGap);
                    change = Math.Min(50, change);
                    change = Math.Max(-50, change);

                    if (Math.Abs(change) < 10)
                        change = 0;

                    interbellGap += change;
                    Debug.WriteLine($"Change: {change}, interbellgap: {interbellGap}");

                    if (prevHumanGaps.Count == HISTORY_SIZE)
                    {
                        prevHumanGaps.RemoveAt(prevHumanGaps.Count - 1);
                    }
                }
            }

            lastStrike = strikeTime;
            strikeCount = (strikeCount + 1) % (2 * numBells);
            return true;
        }

    public AbelAPI simulator;

    public void LoadRows(string filename)
    {
        bellOrder.Clear();
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
        while (true)
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
                Thread.Sleep((int)interbellGap);
            }

            if (!isHS)
                Thread.Sleep((int)(HSGRatio * interbellGap));

            isHS = !isHS;

            index %= bellOrder.Count;
        }
    }
}
}
