using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Muster
{
    public partial class Muster : Form
    {
        private const int numberOfBells = 12;
        private const int MAX_PEERS = 6;

        private readonly MusterAPI api = new MusterAPI();
        private List<Socket> peerSockets = new List<Socket>(MAX_PEERS);
        private List<Task> peerListeners = new List<Task>(MAX_PEERS);
        private List<CancellationTokenSource> peerCancellation = new List<CancellationTokenSource>(MAX_PEERS);

        private IntPtr AbelHandle;

        private string userID;

        private static readonly HttpClient client = new HttpClient();
        //private string endpointAddress = "http://virtserver.swaggerhub.com/drichards2/muster/1.0.0/";
        //private string endpointAddress = "http://localhost:5000/v1/";
        private string endpointAddress = "https://muster.norfolk-st.co.uk/v1/";

        public Muster()
        {
            InitializeComponent();
            userID = GenerateRandomString();
            Debug.WriteLine("Generated user ID: " + userID);

            api.APIEndpoint = endpointAddress;
            NameInput.Text = Environment.UserName;

            //TODO: Add "Find Abel" button in case it's not launched before this app is started
            FindAbel();

  /*          for(int i = 0; i < numberOfBells; i++)
            {
                RingBell(i);
                System.Threading.Thread.Sleep(230);
            }
*/
        }

        private async void MakeNewBand_Click(object sender, EventArgs e)
        {
            var newBandID = await api.CreateBand();
            bandID.Text = newBandID;
            connectionList.Rows.Clear();
        }


        private async void JoinBand_Click(object sender, EventArgs e)
        {
            var member = new MusterAPI.Member
            {
                id = userID,
                name = NameInput.Text,
                location = LocationInput.Text
            };
            var didSucceed = await api.SendJoinBandRequest(bandID.Text, member);

            await GetTheBandBackTogether();
        }


        private async Task GetTheBandBackTogether()
        {
            connectionList.Rows.Clear();
            var band = await api.GetBand(bandID.Text);

            /*
            if (band != null)
                foreach (var member in band.members)
                    if (member.id != userID)
                        connectionList.Rows.Add(member.name, member.location, member.address, member.port, "Disconnected");
            */
        }


        private void ContactServer_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("Doing nothing.");
            // Resend UDP message to server in case of emergency
            // TODO: Only allow this to be used when user has already joined a band
            // TODO: Probably remove this button
            // SendUDPMessagesToServer();
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            SetupPeerSockets();
        }

        private void SendUDPMessagesToServer()
        {
            DisconnectAll();

            Console.WriteLine($"Requesting to connect {connectionList.Rows.Count} peers");
            if (connectionList.Rows.Count - 1 > MAX_PEERS)
            {
                MessageBox.Show($"Can't connect {connectionList.Rows.Count - 1} peers - {MAX_PEERS} is the maximum");
                return;
            }

            foreach (DataGridViewRow row in connectionList.Rows)
            {
                if (row.IsNewRow)
                    continue;

                row.Cells[4].Value = "Not connected";
                
                byte[] data = Encoding.ASCII.GetBytes($"{bandID.Text}:{userID}");
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(holePunchIP.Text), int.Parse(holePunchPort.Text));

                var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                var sent = _socket.SendTo(data,  endPoint);
                if (sent != data.Length)
                {
                    MessageBox.Show("Something's gone wrong.");
                    Debug.WriteLine("Error sending UDP message to server.");
                }

                // TODO: Listen for reply from server

                peerSockets.Add(_socket);

                if (sent == data.Length)
                    row.Cells[4].Value = "Set port";
                else
                    row.Cells[4].Value = "Broken";
            }
        }

        private void SetupPeerSockets()
        {
            SendUDPMessagesToServer();
            
            if ( (connectionList.Rows.Count-1) != peerSockets.Count)
            {
                MessageBox.Show("There seems to be a mismatch between open sockets and peers requested. Abort abort.");
                return;
            }
            
            for (int connectRows = 0; connectRows < connectionList.Rows.Count; connectRows++)
            {
                var row = connectionList.Rows[connectRows];
                if (row.IsNewRow)
                    continue;

                if (IPAddress.TryParse(row.Cells[2].Value.ToString(), out var ipAddr) &&
                    int.TryParse(row.Cells[3].Value.ToString(), out var port))
                {
                    var _socket = peerSockets[connectRows];
                    _socket.Connect(ipAddr, port);
                    row.Cells[4].Value = "Connecting";

                    var ctokenSource = new CancellationTokenSource();
                    peerCancellation.Add(ctokenSource);

                    var runParameters = new ListenerTask.ListenerConfig
                    {
                        cancellationToken = ctokenSource.Token,
                        peerChannel = connectRows,
                        srcSocket = peerSockets[connectRows],
                        BellStrikeEvent = BellStrike,
                        EchoBackEvent = SocketEcho
                    };

                    var listenerTask = new Task(() => {
                        runParameters.srcSocket.ReceiveTimeout = 5000;

                        const int BLOCK_SIZE = 1024;
                        byte[] buffer = new byte[BLOCK_SIZE];
                        while (!runParameters.cancellationToken.IsCancellationRequested)
                        {
                            try
                            {
                                // Blocks until a message returns on this socket from a remote host.
                                var bytesReceived = runParameters.srcSocket.Receive(buffer); 
                                Debug.WriteLine("Received message " + String.Join("", buffer));

                                for (int i = 0; i < bytesReceived; i++)
                                {
                                    if (buffer[i] >= 'A' && buffer[i] < 'A' + numberOfBells)
                                    {
                                        runParameters.BellStrikeEvent?.Invoke(buffer[i] - 'A');
                                    }
                                    else if (buffer[i] == '?')
                                    {
                                        runParameters.srcSocket.Send(new byte[] { (byte)'#' });
                                    }
                                    else if (buffer[i] == '#')
                                    {
                                    runParameters.EchoBackEvent?.Invoke(runParameters.peerChannel);
                                    }
                                }
                            }
                            catch (SocketException se)
                            {
                                // Probably OK?
                            }
                         }
                        });
                    listenerTask.Start();
                }
            }
        }

        private void SocketEcho(int peerChannel)
        {
            Debug.WriteLine($"Received echo request: {peerChannel}");
            connectionList.Rows[peerChannel].Cells[4].Value = "Connected";
        }

        private void BellStrike(int bell)
        {
            RingBell(bell);
        }

        private void Disconnect_Click(object sender, EventArgs e)
        {
            DisconnectAll();
        }

        private void Test_Click(object sender, EventArgs e)
        {
            TestConnection();
        }

        private void TestConnection()
        {
            foreach (DataGridViewRow row in connectionList.Rows)
            {
                if (row.IsNewRow)
                    continue;

                row.Cells[4].Value = "Waiting for reply";
            }
            foreach (var sock in peerSockets)
            {
                sock.Send(new byte[] { (byte)'?' });
            }
        }

        private void ClosePeerSockets()
        {
            foreach (var peerSocket in peerSockets)
            {
                peerSocket?.Dispose();
            }
            peerSockets.Clear();
        }

        private void DisconnectAll()
        {
            foreach (var cancellationToken in peerCancellation)
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
            }

            foreach (var peerListener in peerListeners)
            {
                peerListener.Wait();
                peerListener.Dispose();
            }

            ClosePeerSockets();

            foreach (DataGridViewRow row in connectionList.Rows)
                if (!row.IsNewRow)
                    row.Cells[4].Value = "Disconnected";
        }

        private void Muster_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine($"New key: {e.KeyValue}");
            int bellNumber = e.KeyValue - 'A';

            if ((e.KeyValue >= 'A' && e.KeyValue < 'A' + numberOfBells) || (e.KeyValue == '?'))
            {
                var txBytes = Encoding.ASCII.GetBytes($"{bandID.Text}!{e.KeyCode}");
                foreach (var _socket in peerSockets)
                {
                    Debug.WriteLine($"Sending message to: {_socket.RemoteEndPoint.ToString()}");
                    Task.Factory.StartNew(() => { _socket.Send(txBytes); });
                }
            }

            RingBell(bellNumber);
        }

        private void RingBell(int bellNumber)
        {
            if (bellNumber >= 0 && bellNumber < numberOfBells)
            {
                SendKeystroke(bellNumber);
            }
        }

        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        const int WM_KEYDOWN = 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_CHAR = 0x102;

        private void SendKeystroke(int bell)
        {
            if (AbelHandle != null)
            {
                PostMessage(AbelHandle, WM_CHAR, 'A' + bell, 0);
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, string lpszClass, string lpszWindow);

        private void FindAbel()
        {
            Process[] currentProcesses = Process.GetProcesses();
            foreach (Process p in currentProcesses)
            {
                if (Convert.ToString(p.ProcessName).ToUpper() == "ABEL3")
                {
                    AbelHandle = p.MainWindowHandle;

                    string ChildWindow = "AfxMDIFrame140s";
                    string GrandchildWindow = "AfxFrameOrView140s";

                    AbelHandle = FindWindowEx(AbelHandle, IntPtr.Zero, ChildWindow, "");
                    AbelHandle = FindWindowEx(AbelHandle, IntPtr.Zero, GrandchildWindow, "");
                }
            }
        }

        public static string GenerateRandomString()
        {
			var stringLength = 10;
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[stringLength];
            var random = new Random();

            for (int i = 0; i < stringLength; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = new String(stringChars);
            return finalString;
        }
    }

}

