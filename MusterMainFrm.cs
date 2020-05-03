﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Muster
{
    public partial class Muster : Form
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly AbelAPI abelAPI = new AbelAPI();
        private readonly PeerConnectionManager peerConnectionManager;
        private Robot robot = new Robot();

        private const bool EnableKeepAlives = true;

        private Dictionary<Keys, Keys> CustomKeyMappings = new Dictionary<Keys, Keys>();
        private const string CustomKeyMapFileName = "KeyConfig.txt";

        public Muster()
        {
            InitializeComponent();

            logger.Info("Starting up");

            NameInput.Text = Environment.UserName;

            FindAbel();

            peerConnectionManager = new PeerConnectionManager()
            {
                EnableKeepAlives = EnableKeepAlives,
                simulator = abelAPI,
                bandDetails = bandDetails,
                bandIDDisplay = bandID,
            };

            if (EnableKeepAlives)
                keepAlive.Start();

            robot = new Robot()
            {
                SendBellStrike = peerConnectionManager.SendAndRingKeyStroke,
                BellStrikes = new RingingEvent[AbelAPI.numberOfBells]
            };
            for (int i = 0; i < AbelAPI.numberOfBells; i++)
            {
                robot.BellStrikes[i] = abelAPI.FindEventForCommand((i + 1).ToString());
            }

            peerConnectionManager.NotifyBellStrike = robot.ReceiveNotification;

            RHBell.SelectedIndex = 2;
        }

        private async void MakeNewBand_Click(object sender, EventArgs e)
        {
            await peerConnectionManager.CreateNewBand();
        }

        private async void JoinBand_Click(object sender, EventArgs e)
        {
            await peerConnectionManager.JoinBandRequest(NameInput.Text, LocationInput.Text);
        }

        private async void Connect_Click(object sender, EventArgs e)
        {
            await peerConnectionManager.SendConnectRequest();
        }

        private async void Disconnect_Click(object sender, EventArgs e)
        {
            await peerConnectionManager.LeaveBand();
        }

        private void Test_Click(object sender, EventArgs e)
        {
            peerConnectionManager.TestConnection();
        }

        private void Muster_KeyDown(object sender, KeyEventArgs e)
        {
            ProcessKeystroke(e);
        }

        private void ProcessKeystroke(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                logger.Debug($"Key press ignored: {e.KeyCode}");
                e.SuppressKeyPress = true;
                return;
            }

            RingingEvent evt = ApplyMapping(e);

            if (evt != null)
            {
                logger.Debug("Key press: {in} -> {out}", e.KeyCode, evt);
                peerConnectionManager.SendAndRingKeyStroke(evt);
            }
            else
            {
                logger.Debug($"Key press ignored: {e.KeyCode}");
            }
        }

        private RingingEvent ApplyMapping(KeyEventArgs e)
        {
            if (AdvancedMode.Checked)
            {
                Keys mappedKey = CustomMapKeypress(e.KeyCode);
                if (abelAPI.IsValidAbelKeystroke((char)mappedKey))
                    return abelAPI.FindEventForKeystroke((char)mappedKey);
                else
                    return null;
            }

            RingingEvent res = null;
            switch (e.KeyCode)
            {
                case Keys.F: // LH bell
                    res = abelAPI.FindEventForCommand((LHBell.SelectedIndex + 1).ToString());
                    break;
                case Keys.J: // RH bell
                    res = abelAPI.FindEventForCommand((RHBell.SelectedIndex + 1).ToString());
                    break;
                case Keys.G: // Go
                    res = abelAPI.FindEventForCommand("Go");
                    break;
                case Keys.A: // Bob
                    res = abelAPI.FindEventForCommand("Bob");
                    break;
                case Keys.OemSemicolon: // Single
                    res = abelAPI.FindEventForCommand("Single");
                    break;
                case Keys.T: // That's all
                    res = abelAPI.FindEventForCommand("ThatsAll");
                    break;
                case Keys.R: // Rounds
                    res = abelAPI.FindEventForCommand("Rounds");
                    break;
                case Keys.Q: // Stand
                    res = abelAPI.FindEventForCommand("Stand");
                    break;
                case Keys.F4: // Reset all bells
                    res = abelAPI.FindEventForCommand("ResetBells");
                    break;
            }

            return res;
        }

        private Keys CustomMapKeypress(Keys e)
        {
            // Override only if user has explicitly specified this in a configuration file
            if (CustomKeyMappings.ContainsKey(e))
            {
                e = CustomKeyMappings[e];
            }
            return e;
        }

        private void FindAbel()
        {
            abelAPI.FindAbel();

            if (abelAPI.IsAbelConnected())
            {
                abelConnectLabel.Text = "Abel status:\nConnected";
                abelConnectLabel.ForeColor = Color.CadetBlue;
                abelConnectLabel.Font = new Font(abelConnectLabel.Font, FontStyle.Regular);
            }
            else
            {
                abelConnectLabel.Text = "Abel status:\nNot connected";
                abelConnectLabel.ForeColor = Color.DarkOrange;
                abelConnectLabel.Font = new Font(abelConnectLabel.Font, FontStyle.Bold);
            }
        }

        private void AbelConnect_Tick(object sender, EventArgs e)
        {
            FindAbel();
        }

        private void About_Click(object sender, EventArgs e)
        {
            var ver = getRunningVersion();
            MessageBox.Show("Facilitates ringing on Abel with other ringers over the internet. \n" +
                "Visit https://muster.norfolk-st.co.uk/ for more information.\n" +
                "Written by Dave Richards and Jonathan Agg.", $"Muster (version { ver.ToString()})");
        }

        private Version getRunningVersion()
        {
            try
            {
                return System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion;
            }
            catch (Exception)
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            }
        }

        private void RHBell_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set LH bell to one more than selected RH bell
            var idx = RHBell.SelectedIndex + 1;
            if (idx >= LHBell.Items.Count)
                idx = 0;
            LHBell.SelectedIndex = idx;
        }

        private void Suppress_KeyEvent(object sender, KeyEventArgs e)
        {
            // Prevent key presses changing the selected bell
            e.SuppressKeyPress = true;
        }
        private void Suppress_KeyPressEvent(object sender, KeyPressEventArgs e)
        {
            // Prevent key presses changing the selected bell
            e.Handled = true;
        }

        private void AdvancedMode_CheckedChanged(object sender, EventArgs e)
        {
            LHBell.Enabled = !AdvancedMode.Checked;
            RHBell.Enabled = !AdvancedMode.Checked;
            KeyInfo.Visible = !AdvancedMode.Checked;

            if (AdvancedMode.Checked)
                UpdateCustomKeyPressConfiguration();
        }

        private void UpdateCustomKeyPressConfiguration()
        {
            // Read in tab-separated pairs of overrides in specified file
            // 'A\tB' will send the character 'B' to Abel when 'A' is pressed
            // Other keys e.g. 'B' and 'C' will be unaffected.

            CustomKeyMappings.Clear();

            if (File.Exists(CustomKeyMapFileName))
            {
                var mapping = File.ReadAllLines(CustomKeyMapFileName);
                foreach (string map in mapping)
                {
                    var tokens = map.Split('\t');
                    Keys key, value;
                    if (tokens.Length == 2 && Enum.TryParse(tokens[0], false, out key) && Enum.TryParse(tokens[1], false, out value))
                    {
                        if (CustomKeyMappings.ContainsKey(key))
                            CustomKeyMappings.Remove(key);
                        CustomKeyMappings.Add(key, value);
                    }
                    else
                        logger.Debug("Ignoring keymapping: " + map);
                }

                string message = "Applying the following key overrides specified in " + CustomKeyMapFileName + ":\n";
                foreach (KeyValuePair<Keys, Keys> kvp in CustomKeyMappings)
                    message += String.Format("{0} -> {1}\n", kvp.Key, kvp.Value);
                MessageBox.Show(message);
            }
        }

        private void keepAlive_Tick(object sender, EventArgs e)
        {
            peerConnectionManager.keepAlive_Tick();
        }

        private void enableRobot_CheckedChanged(object sender, EventArgs e)
        {
            if (enableRobot.Checked)
                Task.Run(async () => { await robot.Start(); });
            else
                robot.Stop();
        }

        private void configureRobot_Click(object sender, EventArgs e)
        {
            string fileName = "C:\\Users\\jagg\\source\\repos\\muster\\rows.txt";
            logger.Debug("Reading in robot configuration: " + fileName); 
            robot.LoadRows(fileName);
        }
    }
}