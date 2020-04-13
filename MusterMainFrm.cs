using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Muster
{
    public partial class Muster : Form
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Abel.AbelAPI abelAPI = new Abel.AbelAPI();
        private readonly PeerConnectionManager peerConnectionManager;

        private const bool EnableKeepAlives = true;

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

            RHBell.SelectedIndex = 0;

            Task.Run(() =>
            {
                var bellOrder = new int[12] { 6, 7, 5, 8, 4, 9, 3, 10, 2, 11, 1, 12 };
                foreach (var bell in bellOrder)
                {
                    abelAPI.RingBell(abelAPI.FindKeyStrokeForBell(bell));
                    Thread.Sleep(150);
                }
            });
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

            Keys key = ApplyMapping(e);

            if (key != Keys.None)
            {
                char keyStroke = (char)key;
                logger.Debug($"Key press: {e.KeyCode} -> {keyStroke}");
                peerConnectionManager.SendAndRingKeyStroke(keyStroke);
            }
            else
            {
                logger.Debug($"Key press ignored: {e.KeyCode}");
            }
        }

        private Keys ApplyMapping(KeyEventArgs e)
        {
            if (AdvancedMode.Checked)
            {
                if (abelAPI.IsValidAbelCommand((char)e.KeyCode))
                    return e.KeyCode;
                else
                    return Keys.None;
            }

            Keys res = Keys.None;
            switch (e.KeyCode)
            {
                case Keys.F: // LH bell
                    res = (Keys)abelAPI.FindKeyStrokeForBell(LHBell.SelectedIndex + 1);
                    break;
                case Keys.J: // RH bell
                    res = (Keys)abelAPI.FindKeyStrokeForBell(RHBell.SelectedIndex + 1);
                    break;
                /*                case Keys.G: // Go
                                    res = Keys.S;
                                    break;
                                case Keys.A: // Bob
                                    res = Keys.T;
                                    break;
                                case Keys.OemSemicolon: // Single
                                    res = Keys.U;
                                    break;
                                case Keys.T: // That's all
                                    res = Keys.V;
                                    break;
                                case Keys.R: // Rounds
                                    res = Keys.W;
                                    break;
                                case Keys.Q: // Stand
                                    res = Keys.X;
                                    break;
                                case Keys.F4: // Reset all bells
                                    res = Keys.Y;
                                    break;
                */
                case Keys.G: // Go
                    res = Keys.Q;
                    break;
                case Keys.A: // Bob
                    res = Keys.R;
                    break;
                case Keys.OemSemicolon: // Single
                    res = Keys.S;
                    break;
                case Keys.T: // That's all
                    res = Keys.T;
                    break;
                case Keys.R: // Rounds
                    res = Keys.U;
                    break;
                case Keys.Q: // Stand
                    res = Keys.V;
                    break;
                case Keys.F4: // Reset all bells
                    res = Keys.W;
                    break;

            }

            return res;
        }

        private void FindAbel()
        {
            abelAPI.FindAbel();

            if (abelAPI.IsAbelConnected())
            {
                abelConnectLabel.Text = "Abel: Connected";
                abelConnectLabel.ForeColor = Color.CadetBlue;
                abelConnectLabel.Font = new Font(abelConnectLabel.Font, FontStyle.Regular);
            }
            else
            {
                abelConnectLabel.Text = "Abel: Not connected";
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
        }

        private void keepAlive_Tick(object sender, EventArgs e)
        {
            peerConnectionManager.keepAlive_Tick();
        }
    }
}