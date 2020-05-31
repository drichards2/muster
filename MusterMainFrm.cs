////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	MusterMainFrm.cs
//
// summary:	Implements the main Muster Windows Form
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Muster
{
    /// <summary>   Muster. </summary>
    public partial class Muster : Form
    {
        /// <summary>   The logger. </summary>
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>   The simulator. </summary>
        private SimulatorAPI simulator
        {
            get { return simulators[indexSimulator]; }
        }

        /// <summary>   List of simulators. </summary>
        private List<SimulatorAPI> simulators = new List<SimulatorAPI>() { new AbelAPI(), new BeltowerAPI() };

        /// <summary>   Index of the active simulator. </summary>
        private int indexSimulator = 0;

        /// <summary>   Manager for peer connection. </summary>
        private readonly PeerConnectionManager peerConnectionManager;

        /// <summary>   Enable keep-alive messages. </summary>
        private const bool EnableKeepAlives = true;

        /// <summary>   Custom key mapping dictionary. </summary>
        private Dictionary<Keys, Keys> CustomKeyMappings = new Dictionary<Keys, Keys>();
        /// <summary>   Filename of the custom key map file. </summary>
        private const string CustomKeyMapFileName = "KeyConfig.txt";

        /// <summary>   Default constructor. </summary>
        public Muster()
        {
            InitializeComponent();

            // Add in a callback to stop editing a text box when user clicks away
            foreach (Control control in this.Controls)
            {
                if (!(control is TextBox))
                    control.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Muster_MouseDown);
            }

            logger.Info("Starting up");

            NameInput.Text = Environment.UserName;

            peerConnectionManager = new PeerConnectionManager()
            {
                EnableKeepAlives = EnableKeepAlives,
                simulator = simulator,
                bandDetails = bandDetails,
                bandIDDisplay = bandID,
            };

            if (EnableKeepAlives)
                keepAlive.Start();

            FindSimulator();
            ToggleKeyInfo();

            RHBell.SelectedIndex = 0;

            Task.Run(() =>
            {
                var bellOrder = new int[12] { 6, 7, 5, 8, 4, 9, 3, 10, 2, 11, 1, 12 };
                foreach (var bell in bellOrder)
                {
                    simulator.SendRingingEvent(simulator.FindEventForCommand(bell.ToString()));
                    Thread.Sleep(150);
                }
            });
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by MakeNewBand for click events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async void MakeNewBand_Click(object sender, EventArgs e)
        {
            await peerConnectionManager.CreateNewBand();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by JoinBand for click events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async void JoinBand_Click(object sender, EventArgs e)
        {
            await peerConnectionManager.JoinBandRequest(NameInput.Text, LocationInput.Text);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by Connect for click events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async void Connect_Click(object sender, EventArgs e)
        {
            await peerConnectionManager.SendConnectRequest();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by Disconnect for click events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async void Disconnect_Click(object sender, EventArgs e)
        {
            await peerConnectionManager.LeaveBand();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by Test for click events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Test_Click(object sender, EventArgs e)
        {
            peerConnectionManager.TestConnection();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by Muster for key down events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Key event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Muster_KeyDown(object sender, KeyEventArgs e)
        {
            ProcessKeystroke(e);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Process the keystroke. </summary>
        ///
        /// <param name="e">    Key event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Applies the mapping from keypress to ringing event. </summary>
        ///
        /// <param name="e">    Key event information. </param>
        ///
        /// <returns>   A RingingEvent. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private RingingEvent ApplyMapping(KeyEventArgs e)
        {
            if (AdvancedMode.Checked)
            {
                Keys mappedKey = CustomMapKeypress(e.KeyCode);
                return simulator.FindEventForKey(mappedKey.ToString());
            }

            RingingEvent res = null;
            switch (e.KeyCode)
            {
                case Keys.F: // LH bell
                    res = simulator.FindEventForCommand((LHBell.SelectedIndex + 1).ToString());
                    break;
                case Keys.J: // RH bell
                    res = simulator.FindEventForCommand((RHBell.SelectedIndex + 1).ToString());
                    break;
                case Keys.G: // Go
                    res = simulator.FindEventForCommand("Go");
                    break;
                case Keys.A: // Bob
                    res = simulator.FindEventForCommand("Bob");
                    break;
                case Keys.OemSemicolon: // Single
                    res = simulator.FindEventForCommand("Single");
                    break;
                case Keys.T: // That's all
                    res = simulator.FindEventForCommand("ThatsAll");
                    break;
                case Keys.R: // Rounds
                    res = simulator.FindEventForCommand("Rounds");
                    break;
                case Keys.Q: // Stand
                    res = simulator.FindEventForCommand("Stand");
                    break;
                case Keys.F4: // Reset all bells
                    res = simulator.FindEventForCommand("ResetBells");
                    break;
            }

            return res;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Apply custom mapping to keypress. </summary>
        ///
        /// <param name="e">    The Keys to process. </param>
        ///
        /// <returns>   The Keys. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private Keys CustomMapKeypress(Keys e)
        {
            // Override only if user has explicitly specified this in a configuration file
            if (CustomKeyMappings.ContainsKey(e))
            {
                e = CustomKeyMappings[e];
            }
            return e;
        }

        /// <summary>   Searches for the simulator and update status text. </summary>
        private void FindSimulator()
        {
            for (int i = 0; i < simulators.Count; i++)
            {
                var isFound = simulators[i].FindInstance();
                if (isFound)
                {
                    // Come up with better approach if more simulators needed
                    if (indexSimulator != i)
                    {
                        indexSimulator = i;
                        ToggleKeyInfo();
                        AdvancedMode.Checked = false;
                    }

                    indexSimulator = i;
                    peerConnectionManager.simulator = simulator;

                    continue;
                };
            }

            if (simulator.IsConnected())
            {
                simConnectLabel.Text = "Simulator status:\nConnected to " + simulator.Name;
                simConnectLabel.ForeColor = Color.CadetBlue;
                simConnectLabel.Font = new Font(simConnectLabel.Font, FontStyle.Regular);
            }
            else
            {
                simConnectLabel.Text = "Simulator status:\nNot connected";
                simConnectLabel.ForeColor = Color.DarkOrange;
                simConnectLabel.Font = new Font(simConnectLabel.Font, FontStyle.Bold);
            }
        }

        /// <summary>   Show the relevant key information. </summary>
        private void ToggleKeyInfo()
        {
            if (indexSimulator == 0)
            {
                KeyInfo_Abel.Visible = true;
                KeyInfo_Beltower.Visible = false;
            }
            else
            {
                KeyInfo_Abel.Visible = false;
                KeyInfo_Beltower.Visible = true;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by a timer. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void SimConnect_Tick(object sender, EventArgs e)
        {
            FindSimulator();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by About for click events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void About_Click(object sender, EventArgs e)
        {
            var ver = getRunningVersion();
            MessageBox.Show("Facilitates ringing on Abel with other ringers over the internet. \n" +
                "Visit https://muster.norfolk-st.co.uk/ for more information.\n" +
                "Written by Dave Richards and Jonathan Agg.", $"Muster (version { ver.ToString()})");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets running version of Muster. </summary>
        ///
        /// <returns>   The running version. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by RHBell for selected index changed events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void RHBell_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Set LH bell to one more than selected RH bell
            var idx = RHBell.SelectedIndex + 1;
            if (idx >= LHBell.Items.Count)
                idx = 0;
            LHBell.SelectedIndex = idx;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Suppress key events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Key event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Suppress_KeyEvent(object sender, KeyEventArgs e)
        {
            // Prevent key presses changing the selected bell
            e.SuppressKeyPress = true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Suppress key press events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Key press event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Suppress_KeyPressEvent(object sender, KeyPressEventArgs e)
        {
            // Prevent key presses changing the selected bell
            e.Handled = true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Callback when Advanced Mode checkbox changes. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void AdvancedMode_CheckedChanged(object sender, EventArgs e)
        {
            LHBell.Enabled = !AdvancedMode.Checked;
            RHBell.Enabled = !AdvancedMode.Checked;
            KeyInfo_Abel.Visible = indexSimulator == 0 && !AdvancedMode.Checked;
            KeyInfo_Beltower.Visible = indexSimulator == 1 && !AdvancedMode.Checked;

            if (AdvancedMode.Checked)
                UpdateCustomKeyPressConfiguration();
        }

        /// <summary>   Updates the custom key press configuration. </summary>
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
                    if (tokens.Length == 2 && Enum.TryParse(tokens[0], false, out Keys key) && Enum.TryParse(tokens[1], false, out Keys value))
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by keep-alive timer. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void keepAlive_Tick(object sender, EventArgs e)
        {
            peerConnectionManager.keepAlive_Tick();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by textBox when in focus during typing. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void textBox_Enter(object sender, EventArgs e)
        {
            // Only process keystrokes in the text box, to stop also sending keypresses to Abel
            KeyDown -= new System.Windows.Forms.KeyEventHandler(this.Muster_KeyDown);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by textBox when typing stops. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void textBox_Validated(object sender, EventArgs e)
        {
            // Start sending keypresses to Abel again
            KeyDown += new System.Windows.Forms.KeyEventHandler(this.Muster_KeyDown);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Event handler. Called by Muster for mouse down events. </summary>
        ///
        /// <param name="sender">   Source of the event. </param>
        /// <param name="e">        Mouse event information. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void Muster_MouseDown(object sender, MouseEventArgs e)
        {
            // When user clicks away from a text box, move the focus away from the text box
            // to another (arbitrarily chosen) component
            if (ActiveControl is TextBox)
                label1.Focus();
        }
    }
}