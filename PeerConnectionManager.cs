////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	PeerConnectionManager.cs
//
// summary:	Implements the peer connection manager class
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Muster
{
    /// <summary>   Manager for peer connections. </summary>
    internal class PeerConnectionManager
    {
        /// <summary>   The logger. </summary>
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the identifier of the band. </summary>
        ///
        /// <value> The identifier of the band. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public string bandID
        {
            get { return bandIDDisplay.Text; }
            internal set { bandIDDisplay.Text = value; }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the simulator. </summary>
        ///
        /// <value> The simulator. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public BeltowerAPI simulator { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets a value indicating whether the keep alive messaging is enabled. </summary>
        ///
        /// <value> True if keep alives are enabled, false if not. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool EnableKeepAlives { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the band details. </summary>
        ///
        /// <value> The band details. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public DataGridView bandDetails { get; set; }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the band identifier display. </summary>
        ///
        /// <value> The band identifier display. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public TextBox bandIDDisplay { get; set; }

        /// <summary>   The maximum number of peers. </summary>
        private const int MAX_PEERS = 6;
        /// <summary>   Size of the UDP block. </summary>
        private const int UDP_BLOCK_SIZE = 1024;

        /// <summary>   The server API. </summary>
        private readonly MusterAPIExtended serverAPI = new MusterAPIExtended();

        /// <summary>   The peer sockets. </summary>
        private List<Socket> peerSockets = new List<Socket>(MAX_PEERS);
        /// <summary>   The peer listeners. </summary>
        private List<Task> peerListeners = new List<Task>(MAX_PEERS);
        /// <summary>   The peer cancellation token source. </summary>
        private List<CancellationTokenSource> peerCancellation = new List<CancellationTokenSource>(MAX_PEERS);
        /// <summary>   The local client details. </summary>
        private List<UDPDiscoveryService.LocalNetworkClientDetail> localClientDetails = new List<UDPDiscoveryService.LocalNetworkClientDetail>(MAX_PEERS);
        /// <summary>   The join band cancellation token source. </summary>
        private CancellationTokenSource joinBandCancellation = new CancellationTokenSource();

        /// <summary>   Identifier for the client. </summary>
        private string clientId;
        /// <summary>   The current band. </summary>
        private MusterAPI.Band currentBand;
        /// <summary>   The peer endpoints. </summary>
        private List<MusterAPI.Endpoint> peerEndpoints = new List<MusterAPI.Endpoint>(MAX_PEERS);
        /// <summary>   The local UDP discovery service. </summary>
        private UDPDiscoveryService localUDPDiscoveryService = new UDPDiscoveryService();

        //private string endpointAddress = "http://virtserver.swaggerhub.com/drichards2/muster/1.0.0/";
        /// <summary>   The endpoint address. </summary>
        private string endpointAddress = "https://muster.norfolk-st.co.uk/v1/";

        /// <summary>   The time since the last transmission. </summary>
        private Stopwatch TimeSinceLastTX = new Stopwatch();
        /// <summary>   Specifying whether to send keep alives. </summary>
        private bool SendKeepAlives = false;
        /// <summary>   The transmit threshold. </summary>
        private float TXThreshold = 5000;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the server UDP end point. </summary>
        ///
        /// <value> The server UDP end point. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private IPEndPoint ServerUdpEndPoint => UdpEndPointResolver.Result;
        /// <summary>   The UDP end point resolver. </summary>
        private Task<IPEndPoint> UdpEndPointResolver;

        /// <summary>   Default constructor. </summary>
        public PeerConnectionManager()
        {
            serverAPI.APIEndpoint = endpointAddress;
            UdpEndPointResolver = serverAPI.GetUDPEndPoint();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Creates a new band. </summary>
        ///
        /// <returns>   An asynchronous result. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task CreateNewBand()
        {
            bool isAlreadyConnected = CheckIfConnected();
            if (isAlreadyConnected)
            {
                logger.Debug("Already a member of a band. Aborting request to make a new band.");
                MessageBox.Show("Already a member of a band. To make a new band, disconnect from the existing band and click 'Make a new band' again.");
                return;
            }

            var newBandID = await serverAPI.CreateBand();
            bandID = newBandID;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Determine if connected. </summary>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool CheckIfConnected()
        {
            bool isAlreadyConnected = false;
            foreach (var peer in peerSockets)
                if (peer.Connected)
                    isAlreadyConnected = true;
            return isAlreadyConnected;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Join band request. </summary>
        ///
        /// <param name="name">     The name. </param>
        /// <param name="location"> The location. </param>
        ///
        /// <returns>   An asynchronous result. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task JoinBandRequest(string name, string location)
        {
            if (bandID.Length == 0)
            {
                MessageBox.Show("The band ID is empty.\nEither click 'Make a new band', or type in the ID of an existing band. Then click 'Join/refresh band' again.");
                return;
            }

            bool isAlreadyConnected = CheckIfConnected();
            if (isAlreadyConnected)
            {
                logger.Debug("Already a member of a band. Aborting request to join a band.");
                MessageBox.Show("Already a member of a band. To join a new band, disconnect from the existing band and click 'Join/refresh band' again.");
                return;
            }

            var shouldJoin = clientId == null || currentBand == null;
            if (shouldJoin)
            {
                var member = new MusterAPI.Member
                {
                    id = null,
                    name = name,
                    location = location
                };
                var result = await serverAPI.SendJoinBandRequest(bandID, member);

                clientId = result.Item1;
                var bandStarted = result.Item2;

                if (clientId == null)
                {
                    if (bandStarted)
                        MessageBox.Show("Could not join this band as they've already started ringing. Try joining or making a new band.");
                    else
                        MessageBox.Show("Could not find this band. Check the band ID is correct and try clicking 'Join/refresh band' again.");
                    return;
                }
            }

            if (clientId != null)
            {
                bool timeToConnect = false;
                joinBandCancellation.Dispose();
                joinBandCancellation = new CancellationTokenSource();

                var timeOutCancellation = new CancellationTokenSource();
                timeOutCancellation.CancelAfter(300 * 1000);

                while (!timeToConnect)
                {
                    if (timeOutCancellation.Token.IsCancellationRequested)
                    {
                        logger.Debug($"Timed out joining band ID {bandID} while waiting to start.");
                        MessageBox.Show("Communication error while waiting to start ringing. Ask everyone to click 'Join/refresh band' again.");
                        return;
                    }

                    if (joinBandCancellation.Token.IsCancellationRequested)
                    {
                        logger.Debug($"Cancelled joining band ID {bandID} while waiting to start.");
                        return;
                    }

                    currentBand = await GetTheBandBackTogether();

                    bandDetails.Rows.Clear();
                    foreach (var peer in currentBand.members)
                    {
                        bandDetails.Rows.Add(peer.name, peer.location, "Waiting to start");
                    }

                    timeToConnect = await serverAPI.ConnectionPhaseAnyResponse(bandID, MusterAPIExtended.ConnectionPhases.CONNECT);
                    await Task.Delay(1000); // don't block GUI
                }

                currentBand = await GetTheBandBackTogether();

                bandDetails.Rows.Clear();
                foreach (var peer in currentBand.members)
                {
                    var status = peer.id != clientId ? "Connecting" : "Connected";
                    bandDetails.Rows.Add(peer.name, peer.location, status);
                }

                SetupPeerSockets();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the band back together. </summary>
        ///
        /// <returns>   An asynchronous result that returns the band members. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task<MusterAPI.Band> GetTheBandBackTogether()
        {
            return await serverAPI.GetBand(bandID);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sends the connect request. </summary>
        ///
        /// <returns>   An asynchronous result. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task SendConnectRequest()
        {
            if (currentBand == null || clientId == null)
            {
                MessageBox.Show("First, join a band. When everyone has joined, one band member should click 'Start ringing'.");
                return;
            }

            var success = await serverAPI.SetConnectionStatus(bandID, MusterAPIExtended.ConnectionPhases.CONNECT, clientId);
            if (!success)
            {
                MessageBox.Show("Try clicking 'Connect' again.");
                logger.Error("Could not set connection status.");
            }
        }

        /// <summary>   Sets up the peer sockets. </summary>
        private async void SetupPeerSockets()
        {
            SendUDPMessagesToServer();

            await AllClientsFinishedLocalDiscovery();

            await GetEndpointsFromServer();

            await AllClientsReceivedLocalDetails();

            var localClientsReceived = localUDPDiscoveryService.LocalClients;
            foreach (var client in localClientsReceived)
            {
                logger.Debug("Local client {address}:{port}", client.address, client.port);
            }

            var otherBandMembers = GetOtherBandMembers();

            for (int idx = 0; idx < peerSockets.Count; idx++)
            {
                BindSocket(idx, otherBandMembers[idx].id, localClientsReceived);
                AddListenerToSocket(idx);
            }

            // Start sending keep alive messages if it's enabled
            if (EnableKeepAlives)
            {
                SendKeepAlives = true;
                TimeSinceLastTX.Start();
            }

            TestConnection();
        }

        /// <summary>   Sends the UDP messages to the server. </summary>
        private async void SendUDPMessagesToServer()
        {
            DisconnectAll();

            int numPeers = currentBand.members.Length - 1;
            logger.Debug("Requesting to connect {numPeers} peers", numPeers);
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

            if (localClientDetails.Count != 0)
            {
                logger.Debug("Unexpected local client details remaining. Removing them again.");
                localClientDetails.Clear();
            }

            foreach (var peer in currentBand.members)
                if (peer.id != clientId)
                {
                    byte[] data = Encoding.ASCII.GetBytes($"{bandID}:{clientId}:{peer.id}");

                    var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    var sent = _socket.SendTo(data, ServerUdpEndPoint);
                    if (sent != data.Length)
                    {
                        logger.Error("Problem sending UDP to server, {bytes_expected} expected, {bytes_transmitted} transmitted", data.Length, sent);
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
                        logger.Error("Timeout waiting for UDP reply from server");
                    }

                    if (numBytesReceived == 0 || buffer[0] != '+')
                    {
                        logger.Error("No reply from server");
                    }

                    peerSockets.Add(_socket);

                    // Broadcast to local network that this client is hoping to receive messages on this entry point
                    var localEndpoint = _socket.LocalEndPoint as IPEndPoint;
                    var _localDetail = new UDPDiscoveryService.LocalNetworkClientDetail
                    {
                        socket_owner_id = clientId,
                        address = _localIP,
                        port = localEndpoint.Port,
                        required_destination_id = peer.id
                    };
                    localUDPDiscoveryService.BroadcastClientAvailable(_localDetail);
                    localClientDetails.Add(_localDetail);
                }

            var success = await serverAPI.SetConnectionStatus(bandID, MusterAPIExtended.ConnectionPhases.LOCAL_DISCOVERY_DONE, clientId);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Wait for all clients to finish local discovery. </summary>
        ///
        /// <returns>   An asynchronous result. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task AllClientsFinishedLocalDiscovery()
        {
            bool ready = false;
            joinBandCancellation.Dispose();
            joinBandCancellation = new CancellationTokenSource();

            var timeOutCancellation = new CancellationTokenSource();
            timeOutCancellation.CancelAfter(30 * 1000);

            while (!ready)
            {
                foreach (var localDetail in localClientDetails)
                    localUDPDiscoveryService.BroadcastClientAvailable(localDetail);

                if (timeOutCancellation.Token.IsCancellationRequested)
                {
                    logger.Error($"Timed out joining band {bandID} while waiting for local discovery to be completed.");
                    MessageBox.Show("Error connecting to other ringers. Ask everyone to join a new band and try again.");
                    return;
                }

                if (joinBandCancellation.Token.IsCancellationRequested)
                {
                    logger.Debug($"Cancelled joining band ID {bandID} while waiting for local discovery to be completed.");
                    return;
                }

                ready = await serverAPI.ConnectionPhaseAllResponded(currentBand, bandID, MusterAPIExtended.ConnectionPhases.LOCAL_DISCOVERY_DONE);
                await Task.Delay(1000); // don't block GUI
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets endpoints from server. </summary>
        ///
        /// <returns>   An asynchronous result. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task GetEndpointsFromServer()
        {
            peerEndpoints = await serverAPI.GetEndpointsForBand(bandID, clientId);

            if (peerEndpoints == null || peerEndpoints.Count != peerSockets.Count)
            {
                logger.Error("Did not receive the expected number of endpoints");
                logger.Error("Endpoints: {endpoints}", peerEndpoints);
                if (peerEndpoints != null)
                    logger.Error("{endpoint_count}/{socket_count}", peerEndpoints.Count, peerSockets.Count);
                MessageBox.Show("Error connecting to other ringers. Ask everyone to join a new band and try again.");
                return;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Wait for all clients to have received local details. </summary>
        ///
        /// <returns>   An asynchronous result. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task AllClientsReceivedLocalDetails()
        {
            bool allReady = false;
            bool clientReady = false;
            joinBandCancellation.Dispose();
            joinBandCancellation = new CancellationTokenSource();

            var timeOutCancellation = new CancellationTokenSource();
            timeOutCancellation.CancelAfter(30 * 1000);

            while (!allReady)
            {
                foreach (var localDetail in localClientDetails)
                    localUDPDiscoveryService.BroadcastClientAvailable(localDetail);

                if (timeOutCancellation.Token.IsCancellationRequested)
                {
                    logger.Error($"Timed out joining band {bandID} while waiting for everyone to confirm local discovery worked.");
                    MessageBox.Show("Error connecting to other ringers. Ask everyone to join a new band and try again.");
                    return;
                }

                if (joinBandCancellation.Token.IsCancellationRequested)
                {
                    logger.Debug($"Cancelled joining band ID {bandID} while waiting for everyone to confirm local discovery worked.");
                    return;
                }

                var anyMissing = false;
                foreach (var ep in peerEndpoints)
                    if (ep.check_local)
                        anyMissing = !localUDPDiscoveryService.CheckDetailReceivedFor(ep.target_id);

                // Send status to the server once we're ready, but not again on subsequent loop iterations
                if (!anyMissing && !clientReady)
                {
                    var status = await serverAPI.SetConnectionStatus(bandID, MusterAPIExtended.ConnectionPhases.ENDPOINTS_REGISTERED, clientId);
                    clientReady = status;
                }

                allReady = await serverAPI.ConnectionPhaseAllResponded(currentBand, bandID, MusterAPIExtended.ConnectionPhases.ENDPOINTS_REGISTERED);
                await Task.Delay(1000); // don't block GUI
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets other band members. </summary>
        ///
        /// <returns>   The other band members. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Bind socket. </summary>
        ///
        /// <param name="idx">                  Zero-based index of the. </param>
        /// <param name="target_id">            Identifier for the target. </param>
        /// <param name="localClientsReceived"> The local clients received. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void BindSocket(int idx, string target_id, List<UDPDiscoveryService.LocalNetworkClientDetail> localClientsReceived)
        {
            var _socket = peerSockets[idx];

            // For each peer, find the corresponding endpoint
            MusterAPI.Endpoint _relevantEP = null;
            foreach (var ep in peerEndpoints)
            {
                if (ep.target_id == target_id)
                {
                    _relevantEP = ep;
                    break;
                }
            }

            if (_relevantEP == null)
            {
                logger.Error("Could not find endpoint for {target}", target_id);
                MessageBox.Show("Connecting to other ringers failed. Ask everyone to join a new band and try again.");
                return;
            }

            // Use client's local network if local peer
            if (_relevantEP.check_local)
            {
                // Find the relevent local client detail
                foreach (var localEP in localClientsReceived)
                    if ((localEP.socket_owner_id == _relevantEP.target_id) && (localEP.required_destination_id == clientId))
                    {
                        logger.Debug("Connecting to {targetId} over local network using address {address}:{port}", _relevantEP.target_id, localEP.address, localEP.port);
                        _socket.Connect(localEP.address, localEP.port);
                        break;
                    }

                if (!_socket.Connected)
                {
                    logger.Error("Could not find local details for {target}", _relevantEP.target_id);
                    MessageBox.Show("Connecting to other ringers failed. Ask everyone to join a new band and try again.");
                    return;
                }
            }
            else // Otherwise, connect over the internet
            {
                logger.Debug("Connecting to {targetId} over internet using address {address}:{port}", _relevantEP.target_id, _relevantEP.ip, _relevantEP.port);
                _socket.Connect(_relevantEP.ip, _relevantEP.port);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Adds a listener to socket. </summary>
        ///
        /// <param name="idx">  Zero-based index of the socket. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void AddListenerToSocket(int idx)
        {
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
                        logger.Debug("Received {message} from {source}", message, runParameters.srcSocket.RemoteEndPoint.ToString());

                        for (int i = 0; i < bytesReceived; i++)
                        {
                            if (simulator.IsValidKeystroke((char)buffer[i]))
                            {
                                runParameters.BellStrikeEvent?.Invoke((char)buffer[i]);
                            }
                            else if (buffer[i] == '?')
                            {
                                runParameters.srcSocket.Send(new byte[] { (byte)'#' });
                            }
                            else if (buffer[i] == '#')
                            {
                                logger.Debug("Received reply to test from peer #{peerID} at {peerAddress}.", runParameters.peerChannel, runParameters.srcSocket.RemoteEndPoint.ToString());
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Socket echo callback. </summary>
        ///
        /// <param name="peerChannel">  The peer channel. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Process a received bell strike. </summary>
        ///
        /// <param name="keyStroke">    The key stroke. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private void BellStrike(char keyStroke)
        {
            simulator.RingBell(keyStroke);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Leave band. </summary>
        ///
        /// <returns>   An asynchronous result. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task LeaveBand()
        {
            if (bandID != null && clientId != null)
                await serverAPI.SendLeaveBandRequest(bandID, clientId);
            joinBandCancellation.Cancel();
            DisconnectAll();
            clientId = null;
            bandDetails.Rows.Clear();
            localUDPDiscoveryService.ClearLocalClients();
            SendKeepAlives = false;
            TimeSinceLastTX.Reset();
        }

        /// <summary>   Tests connection. </summary>
        public void TestConnection()
        {
            if (clientId == null || currentBand == null)
                return;

            if (bandDetails.Rows.Count < currentBand.members.Length)
            {
                logger.Debug($"Abandoning testing connection. Something's gone wrong.");
                return;
            }

            for (int idx = 0; idx < currentBand.members.Length; idx++)
                if (currentBand.members[idx].id != clientId)
                    bandDetails.Rows[idx].Cells[2].Value = "Testing connection";

            foreach (var sock in peerSockets)
            {
                if (sock.Connected)
                {
                    logger.Debug("Sending test message to {dest}.", sock.RemoteEndPoint.ToString());
                    sock.Send(new byte[] { (byte)'?' });
                }
            }

            // Reset timer since last transmission to zero
            if (SendKeepAlives)
                TimeSinceLastTX.Restart();
        }

        /// <summary>   Closes peer sockets. </summary>
        private void ClosePeerSockets()
        {
            foreach (var peerSocket in peerSockets)
            {
                peerSocket?.Dispose();
            }
            peerSockets.Clear();
        }

        /// <summary>   Disconnects all open sockets. </summary>
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
            localClientDetails.Clear();

            if (currentBand != null)
            {
                for (int idx = 0; idx < currentBand.members.Length; idx++)
                {
                    var message = currentBand.members[idx].id == clientId ? "Connected" : "Disconnected";
                    if (bandDetails.Rows.Count > idx)
                        bandDetails.Rows[idx].Cells[2].Value = message;
                }
            }
        }

        /// <summary>   Keep alive tick. </summary>
        public void keepAlive_Tick()
        {
            if (SendKeepAlives)
                if (TimeSinceLastTX.ElapsedMilliseconds > TXThreshold)
                {
                    TestConnection();
                }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sends and rings a key stroke. </summary>
        ///
        /// <param name="ringingEvent"> The ringing event. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void SendAndRingKeyStroke(RingingEvent ringingEvent)
        {
            if (simulator.IsValidCommand(ringingEvent))
            {
                var txBytes = Encoding.ASCII.GetBytes($"{ringingEvent.ToChar()}");
                foreach (var _socket in peerSockets)
                {
                    if (_socket.Connected)
                    {
                        logger.Debug("Sending message to: {dest}", _socket.RemoteEndPoint.ToString());
                        Task.Factory.StartNew(() => { _socket.Send(txBytes); });
                    }
                }
            }

            if (SendKeepAlives)
                TimeSinceLastTX.Restart();

            simulator.SendRingingEvent(ringingEvent);
        }
    }
}