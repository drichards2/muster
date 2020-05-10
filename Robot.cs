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
        public Dictionary<int, RingingEvent> ringingCommands = new Dictionary<int, RingingEvent>();

        public double interbellGapIdeal = 200;
        public double HSGRatio = 0.9F;

        public Func<RingingEvent, bool> SendBellStrike { get; set; }
        public Func<bool, bool> NotifyRobotStopped { get; set; }
        public RingingEvent[] BellStrikes;
        public RingingEvent[] ConductingCommands;

        public static int HISTORY_SIZE = 10;
        public double gain = 0.1F;

        private DateTime lastStrike = DateTime.MinValue;
        private List<double> prevHumanGaps = new List<double>(HISTORY_SIZE);
        private int strikeCount = 0;

        private CancellationTokenSource stopRobot = new CancellationTokenSource();
        private bool StartChanges = false;
        private double interbellGap = 240;

        public bool ReceiveNotification(Tuple<DateTime, RingingEvent, bool> input)
        {
            var strikeTime = input.Item1;

            if (input.Item2.ToString().Equals("Go"))
            {
                StartChanges = true;
            }
            if (input.Item2.ToString().Equals("Stand"))
            {
                Stop();
            }

            bool isBell = input.Item2.ToChar() <= 'P';
            if (isBell)
            {
                int bell = int.Parse(input.Item2.ToString());
                if (bell <= numBells && !shouldRing[bell - 1] && lastStrike != DateTime.MinValue)
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
                        logger.Debug($"Change: {change}, new interbellgap: {interbellGap}");
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
            if (!File.Exists(filename))
            {
                MessageBox.Show("Expecting a file called 'rows.txt' in same directory 'Muster.exe'.");
                return;
            }

            bellOrder.Clear();
            ringingCommands.Clear();
            using (StreamReader sr = new StreamReader(filename))
            {
                // Read first line which specfies the number of bells
                string lineBells = sr.ReadLine();
                numBells = int.Parse(lineBells);
                if (lineBells.Length > 2)
                {
                    MessageBox.Show("First line needs to specify number of bells.");
                    return;
                }

                // Read bells robot should ring
                shouldRing = new bool[numBells];
                string robotBells = sr.ReadLine();
                for (int i = 0; i < robotBells.Length; i++)
                    shouldRing[i] = robotBells[i] == '1';
                if (robotBells.Length != numBells)
                {
                    MessageBox.Show("Second line needs to specify whether each bell is to be rung by the robot.");
                    return;
                }

                // Read third line which specifies the peal speed in minutes
                string pealSpeed = sr.ReadLine();
                int pealSpeedMinutes = int.Parse(pealSpeed);
                if (pealSpeed.Length > 3)
                {
                    MessageBox.Show("Third line needs to specify peal speed in minutes.");
                    return;
                }
                interbellGapIdeal = 1000 * pealSpeedMinutes * 60.0 / (5000 / 2) / (2 * numBells + 1);

                // Read fourth line which specifies the "gain"
                string gainStr = sr.ReadLine();
                gain = double.Parse(gainStr);
                gain = Math.Max(Math.Min(gain, 1), 0);
                if (pealSpeed.Length > 5)
                {
                    MessageBox.Show("Fourth line needs to specify how much the robot speed will adjust (0=none, 1=lots).");
                    return;
                }

                // Read the rest of the file as changes
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    foreach (char c in line)
                    {
                        if (c >= '1' && c <= '9')
                            bellOrder.Add(c - '0');
                        else if (c == '0')
                            bellOrder.Add(10);
                        else if (c == 'E' || c == 'e')
                            bellOrder.Add(11);
                        else if (c == 'T' || c == 't')
                            bellOrder.Add(12);
                        else if (c >= 'A' && c <= 'D')
                            bellOrder.Add(c - 'A' + 13);
                        else if (c >= 'a' && c <= 'd')
                            bellOrder.Add(c - 'a' + 13);
                        else if (c == '-')
                            // Position the call at the start of the backstroke
                            //    Based on Abel's output where call is shown after the lead end (the HS)
                            ringingCommands.Add(bellOrder.Count - 2 * numBells, ConductingCommands[0]);
                        else if (c == 's')
                            ringingCommands.Add(bellOrder.Count - 2 * numBells, ConductingCommands[1]);
                    }
                }
            }
            MessageBox.Show($"Loaded in {bellOrder.Count / numBells} rows on {numBells} bells.");
        }

        public async Task Start()
        {
            interbellGap = interbellGapIdeal;
            StartChanges = false;

            stopRobot.Dispose();
            stopRobot = new CancellationTokenSource();

            bool isHS = true;
            bool go = false;
            int index = 0;

            while (!stopRobot.IsCancellationRequested)
            {
                for (int idxBell = 0; idxBell < numBells; idxBell++)
                {
                    int bell = go ? bellOrder[index++] : idxBell + 1;
                    if (shouldRing[bell - 1])
                    {
                        SendBellStrike(BellStrikes[bell - 1]);
                        logger.Debug("Ringing bell " + bell + " at  " + DateTime.Now);
                    }

                    if (ringingCommands.ContainsKey(index))
                    {
                        SendBellStrike(ringingCommands[index]);
                        logger.Debug("Sending command " + ringingCommands[index] + " at strike " + index);
                    }
                    await Task.Delay((int)interbellGap);
                }

                if (!isHS)
                    await Task.Delay((int)(HSGRatio * interbellGap));

                // Start at the handstroke after "Go" command is received
                go = !go && !isHS && StartChanges || go;

                isHS = !isHS;

                index %= bellOrder.Count;
            }
        }

        public void Stop()
        {
            stopRobot.Cancel();
            NotifyRobotStopped(true);
            prevHumanGaps.Clear();
        }
    }
}
