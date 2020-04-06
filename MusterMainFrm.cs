using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private const int numberOfBells = 12;
        private const int MAX_PEERS = 6;
        private const int UDP_BLOCK_SIZE = 1024;

        private readonly MusterAPIExtended api = new MusterAPIExtended();
        private List<Socket> peerSockets = new List<Socket>(MAX_PEERS);
        private List<Task> peerListeners = new List<Task>(MAX_PEERS);
        private List<CancellationTokenSource> peerCancellation = new List<CancellationTokenSource>(MAX_PEERS);

        private IntPtr AbelHandle;
        private IPEndPoint UdpEndPoint => UdpEndPointResolver.Result;
        private Task<IPEndPoint> UdpEndPointResolver;

        private string clientId;
        private MusterAPI.Band currentBand;
        private List<MusterAPI.Endpoint> peerEndpoints = new List<MusterAPI.Endpoint>(MAX_PEERS);
        private UDPDiscoveryService localUDPDiscoveryService = new UDPDiscoveryService();

        private static readonly HttpClient client = new HttpClient();
        //private string endpointAddress = "http://virtserver.swaggerhub.com/drichards2/muster/1.0.0/";
        private string endpointAddress = "https://muster.norfolk-st.co.uk/v1/";

        public Muster()
        {
            InitializeComponent();

            logger.Info("Starting up");

            api.APIEndpoint = endpointAddress;
            NameInput.Text = Environment.UserName;

            UdpEndPointResolver = api.GetUDPEndPoint();

            FindAbel();

            DisplayVersionInformation();

            var bellOrder = new int[12] { 6, 7, 5, 8, 4, 9, 3, 10, 2, 11, 1, 12 };
            for (int i = 0; i < numberOfBells; i++)
            {
                RingBell(bellOrder[i] - 1);
                System.Threading.Thread.Sleep(150);
            }
        }

        private async void MakeNewBand_Click(object sender, EventArgs e)
        {
            var newBandID = await api.CreateBand();
            bandID.Text = newBandID;

            clientId = null;
            bandDetails.Rows.Clear();
            localUDPDiscoveryService.ClearLocalClients();
        }


        private async void JoinBand_Click(object sender, EventArgs e)
        {
            var shouldJoin = clientId == null || currentBand == null;
            if (shouldJoin)
            {
                var member = new MusterAPI.Member
                {
                    id = null,
                    name = NameInput.Text,
                    location = LocationInput.Text
                };
                clientId = await api.SendJoinBandRequest(bandID.Text, member);

                if (clientId == null)
                {
                    MessageBox.Show("Could not join band. Check the band ID is correct and try clicking 'Join/refresh band' again.");
                    return;
                }
            }

            if (clientId != null)
            {
                bool timeToConnect = false;
                while (!timeToConnect)
                {
                    currentBand = await GetTheBandBackTogether();

                    bandDetails.Rows.Clear();
                    foreach (var peer in currentBand.members)
                    {
                        bandDetails.Rows.Add(peer.name, peer.location, "Waiting to start");
                    }

                    timeToConnect = await api.ConnectionPhaseAnyResponse(bandID.Text, MusterAPIExtended.ConnectionPhases.CONNECT);
                    await Task.Delay(1000); // don't block GUI
                }

                currentBand = await GetTheBandBackTogether();

                bandDetails.Rows.Clear();
                foreach (var peer in currentBand.members)
                {
                    var status = peer.id != clientId ? "" : "Ready";
                    bandDetails.Rows.Add(peer.name, peer.location, status);
                }

                SetupPeerSockets();
            }
        }

        private async Task<MusterAPI.Band> GetTheBandBackTogether()
        {
            return await api.GetBand(bandID.Text);
        }

        private async void Connect_Click(object sender, EventArgs e)
        {
            var success = await api.SetConnectionStatus(bandID.Text, MusterAPIExtended.ConnectionPhases.CONNECT, clientId);
            if (!success)
            {
                MessageBox.Show("Try clicking 'Connect' again.");
                logger.Error("Could not set connection status.");
            }
        }

        private async void SendUDPMessagesToServer()
        {
            DisconnectAll();

            int numPeers = currentBand.members.Length - 1;
            logger.Error("Requesting to connect {numPeers} peers", numPeers);
            if (numPeers > MAX_PEERS)
            {
                MessageBox.Show($"Can't connect {numPeers} peers - {MAX_PEERS} is the maximum");
                return;
            }

            // Find local IP address for broadcasting to local network
            string _localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                _localIP = endPoint.Address.ToString();
            }

            foreach (var peer in currentBand.members)
                if (peer.id != clientId)
                {
                    byte[] data = Encoding.ASCII.GetBytes($"{bandID.Text}:{clientId}:{peer.id}");

                    var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    var sent = _socket.SendTo(data, UdpEndPoint);
                    if (sent != data.Length)
                    {
                        logger.Debug("Problem sending UDP to server, {bytes_expected} expected, {bytes_transmitted} transmitted", data.Length, sent);
                        MessageBox.Show("Error connecting to server. Try clicking 'Join/refresh band' again.");
                    }

                    byte[] buffer = new byte[UDP_BLOCK_SIZE];
                    _socket.ReceiveTimeout = 5000;
                    var numBytesReceived = 0;
                    try
                    {
                        numBytesReceived = _socket.Receive(buffer);
                    }
                    catch (SocketException se)
                    {
                        logger.Debug("Timeout waiting for UDP reply from server");

                    }

                    if (numBytesReceived == 0 || buffer[0] != '+')
                    {
                        logger.Error("No reply from server");
                    }

                    peerSockets.Add(_socket);

                    // Broadcast to local network that this client is hoping to receive messages on this entry point
                    var localEndpoint = _socket.LocalEndPoint as IPEndPoint;
                    localUDPDiscoveryService.BroadcastClientAvailable(new UDPDiscoveryService.LocalNetworkClientDetail
                    {
                        socket_owner_id = clientId,
                        address = _localIP,
                        port = localEndpoint.Port,
                        required_destination_id = peer.id
                    });
                }

            var success = await api.SetConnectionStatus(bandID.Text, MusterAPIExtended.ConnectionPhases.LOCAL_DISCOVERY_DONE, clientId);
        }

        private async void SetupPeerSockets()
        {
            SendUDPMessagesToServer();

            bool ready = false;
            while (!ready)
            {
                ready = await api.ConnectionPhaseAllResponded(currentBand, bandID.Text, MusterAPIExtended.ConnectionPhases.LOCAL_DISCOVERY_DONE);
                await Task.Delay(1000); // don't block GUI
            }            

            peerEndpoints = await api.GetEndpointsForBand(bandID.Text, clientId);

            if (peerEndpoints == null || peerEndpoints.Count != peerSockets.Count)
            {
                logger.Error("Did not receive the expected number of endpoints");
                logger.Error("Endpoints: {endpoints}", peerEndpoints);
                logger.Error("{endpoint_count}/{socket_count}", peerEndpoints.Count, peerSockets.Count);
                MessageBox.Show("Error connecting to other ringers. Try clicking 'Join/refresh band' again.");                
                return;
            }

            var localClients = localUDPDiscoveryService.LocalClients;
            foreach (var client in localClients)
                logger.Debug("Local client {address}:{port}", client.address, client.port);

            var otherBandMembers = GetOtherBandMembers();

            for (int idx = 0; idx < peerSockets.Count; idx++)
            {
                var _socket = peerSockets[idx];

                string _targetId = "";
                string _targetIp = "";
                int _targetPort = 0;
                foreach (var ep in peerEndpoints)
                {
                    if (ep.target_id == otherBandMembers[idx].id)
                    {
                        _targetId = ep.target_id;
                        _targetIp = ep.ip;
                        _targetPort = ep.port;
                        break;
                    }
                }

                //Use client's local network if local peer
                bool isLocal = false;
                foreach (var localEP in localClients)
                    if ((_targetId == localEP.socket_owner_id) && (localEP.required_destination_id == clientId))
                    {
                        isLocal = true;
                        logger.Debug("Connecting to {targetId} over local network using address {address}:{port}", _targetId, localEP.address, localEP.port);
                        _socket.Connect(localEP.address, localEP.port);
                        break;
                    }

                // Otherwise, connect over the internet
                if (!isLocal)
                {
                    logger.Debug("Connecting to {targetId} over internet using address {address}:{port}", _targetId, _targetIp, _targetPort);
                    _socket.Connect(_targetIp, _targetPort);
                }

                var ctokenSource = new CancellationTokenSource();
                peerCancellation.Add(ctokenSource);

                var runParameters = new ListenerTask.ListenerConfig
                {
                    cancellationToken = ctokenSource.Token,
                    peerChannel = idx,
                    srcSocket = peerSockets[idx],
                    BellStrikeEvent = BellStrike,
                    EchoBackEvent = SocketEcho
                };

                var listenerTask = new Task(() =>
                {
                    runParameters.srcSocket.ReceiveTimeout = 5000;

                    byte[] buffer = new byte[UDP_BLOCK_SIZE];
                    while (!runParameters.cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            // Blocks until a message returns on this socket from a remote host.
                            var bytesReceived = runParameters.srcSocket.Receive(buffer);

                            var message = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                            logger.Debug("Received '{message}' from {source}", message, runParameters.srcSocket.RemoteEndPoint.ToString());

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
                                    Debug.WriteLine($"Received reply to test message from peer #{runParameters.peerChannel} at {runParameters.srcSocket.RemoteEndPoint.ToString()}.");
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

            TestConnection();
        }

        private List<MusterAPI.Member> GetOtherBandMembers()
        {
            var members = currentBand.members;
            var peers = new List<MusterAPI.Member>();
            foreach (var member in members)
            {
                if (member.id != clientId)
                    peers.Add(member);
            }
            return peers;
        }

        private void SocketEcho(int peerChannel)
        {
            int delta = 0;
            int peerCount = 0;
            foreach (var member in currentBand.members)
            {
                if (member.id == clientId)
                    delta++;
                else
                {
                    if (peerCount == peerChannel)
                    {
                        bandDetails.Rows[peerCount + delta].Cells[2].Value = "Connected";
                        return;
                    }
                    peerCount++;
                }
            }
        }

        private void BellStrike(int bell)
        {
            RingBell(bell);
        }

        private void Disconnect_Click(object sender, EventArgs e)
        {
            DisconnectAll();
            clientId = null;
            bandDetails.Rows.Clear();
            localUDPDiscoveryService.ClearLocalClients();
        }

        private void Test_Click(object sender, EventArgs e)
        {
            TestConnection();
        }

        private void TestConnection()
        {
            for (int idx = 0; idx < currentBand.members.Length; idx++)
                if (currentBand.members[idx].id != clientId)
                    bandDetails.Rows[idx].Cells[2].Value = "Testing connection";

            foreach (var sock in peerSockets)
            {
                Debug.WriteLine($"Sending test message to {sock.RemoteEndPoint.ToString()}.");
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
                try
                {
                    cancellationToken.Cancel();
                }
                catch (ObjectDisposedException se)
                {
                    // Do nothing
                }
                cancellationToken.Dispose();
            }

            foreach (var peerListener in peerListeners)
            {
                peerListener.Wait();
                peerListener.Dispose();
            }

            ClosePeerSockets();

            for (int idx = 0; idx < currentBand.members.Length; idx++)
            {
                var message = currentBand.members[idx].id == clientId ? "" : "Disconnected";
                bandDetails.Rows[idx].Cells[2].Value = message;
            }
        }

        private void Muster_KeyDown(object sender, KeyEventArgs e)
        {
            Debug.WriteLine($"New key: {e.KeyValue}");
            int bellNumber = e.KeyValue - 'A';

            if ((e.KeyValue >= 'A' && e.KeyValue < 'A' + numberOfBells) || (e.KeyValue == '?'))
            {
                var txBytes = Encoding.ASCII.GetBytes($"{e.KeyCode}");
                foreach (var _socket in peerSockets)
                {
                    if (_socket.Connected)
                    {
                        Debug.WriteLine($"Sending message to: {_socket.RemoteEndPoint.ToString()}");
                        Task.Factory.StartNew(() => { _socket.Send(txBytes); });
                    }
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
            var foundHandle = IntPtr.Zero;
            // Inspired by the Abel connection in Graham John's Handbell Manager (https://github.com/GACJ/handbellmanager)
            Process[] currentProcesses = Process.GetProcesses();
            foreach (Process p in currentProcesses)
            {
                if (Convert.ToString(p.ProcessName).ToUpper() == "ABEL3")
                {
                    foundHandle = p.MainWindowHandle;

                    string ChildWindow = "AfxMDIFrame140s";
                    string GrandchildWindow = "AfxFrameOrView140s";

                    foundHandle = FindWindowEx(foundHandle, IntPtr.Zero, ChildWindow, "");
                    foundHandle = FindWindowEx(foundHandle, IntPtr.Zero, GrandchildWindow, "");
                    if (foundHandle != IntPtr.Zero)
                        break;
                }
            }

            if (foundHandle != AbelHandle)
            {
                AbelHandle = foundHandle;
            }

            if (AbelHandle == IntPtr.Zero)
            {
                abelConnectLabel.Text = "Abel: Not connected";
                abelConnectLabel.ForeColor = Color.DarkOrange;
                abelConnectLabel.Font = new Font(abelConnectLabel.Font, FontStyle.Bold);
            }
            else
            {
                abelConnectLabel.Text = "Abel: Connected";
                abelConnectLabel.ForeColor = Color.CadetBlue;
                abelConnectLabel.Font = new Font(abelConnectLabel.Font, FontStyle.Regular);
            }
        }

        private void AbelConnect_Tick(object sender, EventArgs e)
        {
            FindAbel();
        }

        private void DisplayVersionInformation()
        {
            var ver = getRunningVersion();
            aboutText.Text = $"Muster (version {ver.ToString()}). Written by Dave Richards and Jonathan Agg";
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
    }
}

