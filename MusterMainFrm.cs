﻿using Newtonsoft.Json;
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
        private const int numberOfBells = 12;
        private const int MAX_PEERS = 6;

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

        private static readonly HttpClient client = new HttpClient();
        //private string endpointAddress = "http://virtserver.swaggerhub.com/drichards2/muster/1.0.0/";
        private string endpointAddress = "https://muster.norfolk-st.co.uk/v1/";

        public Muster()
        {
            InitializeComponent();

            api.APIEndpoint = endpointAddress;
            NameInput.Text = Environment.UserName;

            UdpEndPointResolver = api.GetUDPEndPoint();

            FindAbel();
        }

        private async void MakeNewBand_Click(object sender, EventArgs e)
        {
            var newBandID = await api.CreateBand();
            bandID.Text = newBandID;

            bandDetails.Items.Clear();
        }


        private async void JoinBand_Click(object sender, EventArgs e)
        {
            var member = new MusterAPI.Member
            {
                id = null,
                name = NameInput.Text,
                location = LocationInput.Text
            };
            clientId = await api.SendJoinBandRequest(bandID.Text, member);

            if (clientId != null)
            {
                bool timeToConnect = false;
                while (!timeToConnect)
                {
                    var connectionStatus = await api.GetConnectionStatus(bandID.Text);
                    timeToConnect = connectionStatus.request_client_connect;
                    Thread.Sleep(1000);
                }

                currentBand = await GetTheBandBackTogether();

                foreach (var peer in currentBand.members)
                {
                    bandDetails.Items.Add($"{peer.name}\t{peer.location}\t{peer.id}");
                }

                SetupPeerSockets();
            }
        }


        private async Task<MusterAPI.Band> GetTheBandBackTogether()
        {
            //connectionList.Rows.Clear();
            return await api.GetBand(bandID.Text);

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

        private async void Connect_Click(object sender, EventArgs e)
        {
            var success = await api.SetConnectionStatus(bandID.Text);
            if (!success)
            {
                MessageBox.Show("Try clicking 'Connect' again.");
                Debug.WriteLine("Could not set connection status.");
            }
        }

        private void SendUDPMessagesToServer()
        {
            DisconnectAll();

            int numPeers = currentBand.members.Length - 1;
            Debug.WriteLine($"Requesting to connect {numPeers} peers");
            if (numPeers > MAX_PEERS)
            {
                MessageBox.Show($"Can't connect {numPeers} peers - {MAX_PEERS} is the maximum");
                return;
            }

            foreach (var peer in currentBand.members)
                if (peer.id != clientId)
                {
                    byte[] data = Encoding.ASCII.GetBytes($"{bandID.Text}:{clientId}:{peer.id}");

                    var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    var sent = _socket.SendTo(data, UdpEndPoint);
                    if (sent != data.Length)
                    {
                        MessageBox.Show("Something's gone wrong.");
                        Debug.WriteLine("Error sending UDP message to server.");
                    }

                    byte[] buffer = new byte[1024];
                    var numBytesReceived = _socket.Receive(buffer);

                    if (numBytesReceived == 0 || buffer[0] != '+')
                    {
                        Debug.WriteLine("Did not receive reply from server.");
                    }

                    peerSockets.Add(_socket);
                }
        }

        private async void SetupPeerSockets()
        {
            SendUDPMessagesToServer();

            Thread.Sleep(1000);

            var peerEndpoints = await api.GetEndpointsForBand(bandID.Text, clientId);

            for (int idx = 0; idx < peerEndpoints.Count; idx++)
            {
                var _socket = peerSockets[idx];
                _socket.Connect(peerEndpoints[idx].ip, peerEndpoints[idx].port);

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

        private void SocketEcho(int peerChannel)
        {
            /*
            Debug.WriteLine($"Received echo request: {peerChannel}");
            connectionList.Rows[peerChannel].Cells[4].Value = "Connected";
            */
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
            /*
            foreach (DataGridViewRow row in connectionList.Rows)
            {
                if (row.IsNewRow)
                    continue;

                row.Cells[4].Value = "Waiting for reply";
            }
            */
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

            /*
            foreach (DataGridViewRow row in connectionList.Rows)
                if (!row.IsNewRow)
                    row.Cells[4].Value = "Disconnected";
            */
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
    }

}

