using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Muster
{
    class Robot
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        // An example:
        public int numBells = 8;
        public bool[] shouldRing = { true, true, false, false, true, true, false, false };
        public List<int> bellOrder = new List<int>(8) { 1, 2, 3, 4, 5, 6, 7, 8 };

        public double interbellGap = 200;
        public double HSGRatio = 0.9F;

        public Func<RingingEvent, bool> SendBellStrike { get; set; }
        public RingingEvent[] BellStrikes;

        public static int HISTORY_SIZE = 5;
        public double gain = 0.8F;

        private DateTime lastStrike = DateTime.MinValue;
        private List<double> prevHumanGaps = new List<double>(HISTORY_SIZE);
        private int strikeCount = 0;

        private CancellationTokenSource stopRobot = new CancellationTokenSource();

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
                    //    TODO: This doesn't ever trigger as it needs knowledge of the expected strike order
                    if (gap < 2 * interbellGap)
                        prevHumanGaps.Insert(0, gap);

                    // Start updating once buffer is filled up
                    if (prevHumanGaps.Count == HISTORY_SIZE)
                    {
                        double average = prevHumanGaps.Average();
                        var change = gain * (average - interbellGap);
                        change = Math.Min(50, change);
                        change = Math.Max(-50, change);

                        if (Math.Abs(change) < 10)
                            change = 0;

                        interbellGap += change;
                        Debug.WriteLine($"Change: {change}, interbellgap: {interbellGap}");
                    }

                    // Update buffer
                    if (prevHumanGaps.Count == HISTORY_SIZE)
                    {
                        prevHumanGaps.RemoveAt(prevHumanGaps.Count - 1);
                    }
                }
            }

            lastStrike = strikeTime;
            strikeCount++;
            strikeCount %= 2 * numBells;
            return true;
        }

        public void LoadRows(string filename)
        {
            bellOrder.Clear();
            using (StreamReader sr = new StreamReader(filename))
            {
                // Read first line which specfies the number of bells
                string lineBells = sr.ReadLine();
                numBells = int.Parse(lineBells);
                if (lineBells.Length > 2)
                    MessageBox.Show("First line needs to specify number of bells.");

                // Read bells robot should ring
                shouldRing = new bool[numBells];
                string robotBells = sr.ReadLine();
                for (int i = 0; i < robotBells.Length; i++)
                    shouldRing[i] = robotBells[i] == '1';
                if (robotBells.Length != numBells)
                    MessageBox.Show("Second line needs to specify whether each bell is to be rung by the robot.");

                // Read the stream to a string, and write the string to the console.
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    foreach (char c in line)
                        bellOrder.Add(c - '0');
                }
            }
        }

        public async Task Start()
        {
            //TODO: Move this setting to the configuration somehow.
            //  Need to be ensure user is able to reset this to the default if it's diverged
            interbellGap = 300 - (numBells - 6) * 10;

            stopRobot.Dispose();
            stopRobot = new CancellationTokenSource();

            bool isHS = true;
            int index = 0;
            while (!stopRobot.IsCancellationRequested)
            {
                for (int idxBell = 0; idxBell < numBells; idxBell++)
                {
                    int bell = bellOrder[index++];
                    if (shouldRing[bell - 1])
                    {
                        SendBellStrike(BellStrikes[bell - 1]);
                        logger.Debug("Ringing bell " + bell + " at  " + DateTime.Now);
                    }
                    await Task.Delay((int)interbellGap);
                }

                if (!isHS)
                    await Task.Delay((int)(HSGRatio * interbellGap));

                isHS = !isHS;

                index %= bellOrder.Count;
            }
        }

        public void Stop()
        {
            stopRobot.Cancel();
        }
    }
}
