using System;
using System.Drawing;
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
                    abelAPI.SendRingingEvent(abelAPI.FindEventForCommand(bell.ToString()));
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
            // Don't send commands to Abel when typing in a text box
            if (!(ActiveControl is TextBox))
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
                if (abelAPI.IsValidAbelKeystroke((char)e.KeyCode))
                    return abelAPI.FindEventForKeystroke((char)e.KeyCode);
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

    internal class CustomTextBox : System.Windows.Forms.TextBox
    {
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Prevent main keypress event firing to avoid sending keypresses to Abel when editing a text box
            
            // Do some rudimentary processing
            // TODO: Ideally work out how to invoke the default processing
            char data = (char)keyData;
            if (data >= '0' && data <= '9' || data >= 'A' && data <= 'Z')
            {
                // Add character if valid
                // TOOD: Add this where the cursor is.
                Text += data;
                SelectionStart = Text.Length;
                SelectionLength = 0;
            }
            if (keyData == Keys.Back)
            {
                if (SelectionLength > 0)
                {
                    // Remove selection
                    Text = Text.Remove(SelectionStart, SelectionLength);
                    SelectionStart = Text.Length;
                    SelectionLength = 0;
                }
                else if (Text.Length > 0)
                {
                    // Remove last character
                    Text = Text.Remove(Text.Length - 1, 1);
                    SelectionStart = Text.Length;
                    SelectionLength = 0;
                }
            }

            // Tell form that we've processed the keystroke here
            return true;
        }
    }
}