﻿using System;
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
    internal class PeerConnectionManager
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public string bandID
        {
            get { return bandIDDisplay.Text; }
            internal set { bandIDDisplay.Text = value; }
        }
        public AbelAPI simulator { get; set; }
        public bool EnableKeepAlives { get; set; }
        public DataGridView bandDetails { get; set; }
        public TextBox bandIDDisplay { get; set; }

        private const int MAX_PEERS = 6;
        private const int UDP_BLOCK_SIZE = 1024;

        private readonly MusterAPIExtended serverAPI = new MusterAPIExtended();

        private List<Socket> peerSockets = new List<Socket>(MAX_PEERS);
        private List<Task> peerListeners = new List<Task>(MAX_PEERS);
        private List<CancellationTokenSource> peerCancellation = new List<CancellationTokenSource>(MAX_PEERS);
        private List<UDPDiscoveryService.LocalNetworkClientDetail> localClientDetails = new List<UDPDiscoveryService.LocalNetworkClientDetail>(MAX_PEERS);
        private CancellationTokenSource joinBandCancellation = new CancellationTokenSource();

        private string clientId;
        private MusterAPI.Band currentBand;
        private List<MusterAPI.Endpoint> peerEndpoints = new List<MusterAPI.Endpoint>(MAX_PEERS);
        private UDPDiscoveryService localUDPDiscoveryService = new UDPDiscoveryService();

        //private string endpointAddress = "http://virtserver.swaggerhub.com/drichards2/muster/1.0.0/";
        private string endpointAddress = "https://muster.norfolk-st.co.uk/v1/";

        private Stopwatch TimeSinceLastTX = new Stopwatch();
        private bool SendKeepAlives = false;
        private float TXThreshold = 5000;

        private IPEndPoint ServerUdpEndPoint => UdpEndPointResolver.Result;
        private Task<IPEndPoint> UdpEndPointResolver;

        public PeerConnectionManager()
        {
            serverAPI.APIEndpoint = endpointAddress;
            UdpEndPointResolver = serverAPI.GetUDPEndPoint();
        }

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

        private bool CheckIfConnected()
        {
            bool isAlreadyConnected = false;
            foreach (var peer in peerSockets)
                if (peer.Connected)
                    isAlreadyConnected = true;
            return isAlreadyConnected;
        }

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
                    var status = peer.id != clientId ? "" : "Ready";
                    bandDetails.Rows.Add(peer.name, peer.location, status);
                }

                SetupPeerSockets();
            }
        }
        private async Task<MusterAPI.Band> GetTheBandBackTogether()
        {
            return await serverAPI.GetBand(bandID);
        }
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
                        logger.Debug("Received '{message}' from {source}", message, runParameters.srcSocket.RemoteEndPoint.ToString());

                        for (int i = 0; i < bytesReceived; i++)
                        {
                            if (simulator.IsValidAbelKeystroke((char)buffer[i]))
                            {
                                runParameters.BellStrikeEvent?.Invoke((char)buffer[i]);
                            }
                            else if (buffer[i] == '?')
                            {
                                runParameters.srcSocket.Send(new byte[] { (byte)'#' });
                            }
                            else if (buffer[i] == '#')
                            {
                                logger.Debug($"Received reply to test message from peer #{runParameters.peerChannel} at {runParameters.srcSocket.RemoteEndPoint.ToString()}.");
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

        private void BellStrike(char keyStroke)
        {
            simulator.RingBell(keyStroke);
        }

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
                    logger.Debug($"Sending test message to {sock.RemoteEndPoint.ToString()}.");
                    sock.Send(new byte[] { (byte)'?' });
                }
            }

            // Reset timer since last transmission to zero
            if (SendKeepAlives)
                TimeSinceLastTX.Restart();
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
            localClientDetails.Clear();

            if (currentBand != null)
            {
                for (int idx = 0; idx < currentBand.members.Length; idx++)
                {
                    var message = currentBand.members[idx].id == clientId ? "" : "Disconnected";
                    if (bandDetails.Rows.Count > idx)
                        bandDetails.Rows[idx].Cells[2].Value = message;
                }
            }
        }

        public void keepAlive_Tick()
        {
            if (SendKeepAlives)
                if (TimeSinceLastTX.ElapsedMilliseconds > TXThreshold)
                {
                    TestConnection();
                }
        }

        public void SendAndRingKeyStroke(RingingEvent ringingEvent)
        {
            if (simulator.IsValidAbelCommand(ringingEvent))
            {
                var txBytes = Encoding.ASCII.GetBytes($"{ringingEvent.ToByte()}");
                foreach (var _socket in peerSockets)
                {
                    if (_socket.Connected)
                    {
                        logger.Debug($"Sending message to: {_socket.RemoteEndPoint.ToString()}");
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