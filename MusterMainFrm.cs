﻿using Newtonsoft.Json;
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

        private List<Socket> peerSockets = new List<Socket>(MAX_PEERS);

//        private Socket serverSocket;
        private UdpClient udpClient;

        private Task listenerTask;
        private CancellationTokenSource cancellationTokenSource;

        private IntPtr AbelHandle;

        private string userID;

        private static readonly HttpClient client = new HttpClient();
        //private string serverAddress = "http://virtserver.swaggerhub.com/drichards2/muster/1.0.0/";
        //private string serverAddress = "http://localhost:5000/v1/";
        private string serverAddress = "https://muster.norfolk-st.co.uk/v1/";

        public Muster()
        {
            InitializeComponent();
            userID = GenerateRandomString();
            Debug.WriteLine("Generated user ID: " + userID);

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
            var newBandID = await SendCreateBandRequest(serverAddress);
            bandID.Text = newBandID;
            connectionList.Rows.Clear();
        }

        private static async Task<string> SendCreateBandRequest(string serverAddress)
        {
            // Avoid deadlock: https://stackoverflow.com/questions/14435520/why-use-httpclient-for-synchronous-connection
            var response = await client.PostAsync(serverAddress + "bands", null);
            
            if ((int)response.StatusCode == 201)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                if (responseString.Length > 0)
                {
                    var newbandID = responseString;
                    newbandID = newbandID.Replace("\"", ""); //swaggerhub includes double quotes at start and end
                    Debug.WriteLine("Created new band with ID: " + newbandID);
                    return newbandID;
                }
                else
                    // Trust the server to send a sensible band ID
                    return "";
            }
            else
            {
                MessageBox.Show("Could not create new band.");
                Debug.WriteLine("Error creating band: " + response.ReasonPhrase);
                return "";
            }
        }

        private async void JoinBand_Click(object sender, EventArgs e)
        {
            var member = new Member
            {
                id = userID,
                name = NameInput.Text,
                location = LocationInput.Text
            };
            var didSucceed = await SendJoinBandRequest(serverAddress, bandID.Text, member);

            if (didSucceed)
            {
                SendUDPMessageToServer();
            }
            
            await GetTheBandBackTogether();
        }

        private static async Task<bool> SendJoinBandRequest(string serverAddress, string bandID, Member member)
        {
            Debug.WriteLine("Joining band: " + bandID);
            var json = JsonConvert.SerializeObject(member);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PutAsync(serverAddress + "bands/" + bandID + "/members", content);
            if ((int)response.StatusCode == 204)
            {
                return true;
            }
            else
            {
                // TODO: Separate these cases out - "refreshing" the band needs to be supported
                MessageBox.Show("Either you've already joined, or there's no record of band ID '" + bandID + "'. Will now refresh the band.");
                Debug.WriteLine("Error joining band " + bandID + ": " + response.ReasonPhrase);
                return false;
            }
        }

        private void SendUDPMessageToServer()
        {
            udpClient = new UdpClient();
            udpClient.Connect(IPAddress.Parse(holePunchIP.Text).ToString(), int.Parse(holePunchPort.Text));
            byte[] data = Encoding.ASCII.GetBytes($"{bandID.Text}:{userID}");
            var sent = udpClient.Send(data, data.Length);
            if (sent != data.Length)
            {
                MessageBox.Show("Something's gone wrong.");
                Debug.WriteLine("Error sending UDP message to server.");
            }
        }

        private async Task GetTheBandBackTogether()
        {
            connectionList.Rows.Clear();
            var band = await FindBandMembers(serverAddress, bandID.Text);

            if (band != null)
                foreach (var member in band.members)
                    if (member.id != userID)
                        connectionList.Rows.Add(member.name, member.location, member.address, member.port, "Disconnected");
        }

        private static async Task<Band> FindBandMembers(string serverAddress, string bandID)
        {
            Debug.WriteLine("Finding band members in band: " + bandID);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Muster Client");

            var response = await client.GetAsync(serverAddress + "bands/" + bandID);

            if ((int)response.StatusCode == 200)
            {
                var band = JsonConvert.DeserializeObject<Band>(await response.Content.ReadAsStringAsync());
                return band;
            }
            else
            {
                MessageBox.Show("No record of band ID '" + bandID + "'");
                Debug.WriteLine("No record of band ID '" + bandID + "': " + response.ReasonPhrase);
                return null;
            }
        }

        private void ContactServer_Click(object sender, EventArgs e)
        {
            // Resend UDP message to server in case of emergency
            // TODO: Only allow this to be used when user has already joined a band
            SendUDPMessageToServer();
        }


        private void Connect_Click(object sender, EventArgs e)
        {
            SetupIncomingSocket();
            SetupOutgoingSockets();
        }

        private void ClosePeerSockets()
        {
            foreach (var peerSocket in peerSockets)
            {
                peerSocket?.Dispose();
            }
            peerSockets.Clear();
        }

        private void SetupOutgoingSockets()
        {
/*            ClosePeerSockets();

            for (int connectRows = 0; connectRows < connectionList.Rows.Count; connectRows++)
            {
                var row = connectionList.Rows[connectRows];
                if (row.IsNewRow)
                    continue;

                if (IPAddress.TryParse(row.Cells[2].Value.ToString(), out var ipAddr) &&
                    int.TryParse(row.Cells[3].Value.ToString(), out var port))
                {
                    var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _socket.Connect(ipAddr, port);
                    peerSockets.Add(_socket);

                    row.Cells[4].Value = "Connecting";
                }
            }
*/
        }

        private void DisconnectListener()
        {
            cancellationTokenSource?.Cancel();
            listenerTask?.Wait();
            listenerTask?.Dispose();
            cancellationTokenSource?.Dispose();

            cancellationTokenSource = null;
            listenerTask = null;
        }

        private void SetupIncomingSocket()
        {
            if (udpClient == null)
                return;

            var localEndpoint = udpClient.Client.LocalEndPoint as IPEndPoint;
            var port = localEndpoint.Port;

            //Creates an IPEndPoint to record the IP Address and port number of the sender. 
            // The IPEndPoint will allow you to read datagrams sent from any source.
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, port);

            DisconnectListener();

            cancellationTokenSource = new CancellationTokenSource();
            var runParameters = new ListenerTask.ListenerConfig
            {
                cancellationToken = cancellationTokenSource.Token,
                srcSocket = null,
                BellStrikeEvent = BellStrike,
                EchoBackEvent = SocketEcho
            };

            udpClient.Client.ReceiveTimeout = 5000;

            listenerTask = new Task(() => {
                while (!runParameters.cancellationToken.IsCancellationRequested)
                    {
                     try{
                         // Blocks until a message returns on this socket from a remote host.
                         var bytesReceived = udpClient.Receive(ref RemoteIpEndPoint); 
                         Debug.WriteLine("This is the message you received " +
                                                   String.Join("", bytesReceived));
                         Debug.WriteLine("This message was sent from " +
                                                     RemoteIpEndPoint.Address.ToString() +
                                                     " on their port number " +
                                                     RemoteIpEndPoint.Port.ToString());

                        for (int i = 0; i < bytesReceived.Length; i++)
                        {
                            if (bytesReceived[i] >= 'A' && bytesReceived[i] < 'A' + numberOfBells)
                            {
                                runParameters.BellStrikeEvent?.Invoke(bytesReceived[i] - 'A');
                            }
                            else if (bytesReceived[i] == '?')
                            {
                                //udpClient.Send(new byte[] { (byte)'#' }, 1);
                            }
                            else if (bytesReceived[i] == '#')
                            {
                                //runParameters.EchoBackEvent?.Invoke();
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

        private void SocketEcho()
        {
            /*
            for (var socket in peerSockets)
            {
                connectionList.Rows[peerChannel].Cells[4].Value = "Connected";
            }
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

        private void DisconnectAll()
        {
            DisconnectListener();
            ClosePeerSockets();
//            serverSocket.Dispose();

            foreach (DataGridViewRow row in connectionList.Rows)
                if (!row.IsNewRow)
                    row.Cells[4].Value = "Disconnected";
        }

        private void Muster_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"New key: {e.KeyValue}");
            int bellNumber = e.KeyValue - 'A';

            if ((e.KeyValue >= 'A' && e.KeyValue < 'A' + numberOfBells) || (e.KeyValue == '?'))
            {
                var txBytes = Encoding.ASCII.GetBytes($"{bandID.Text}!{e.KeyCode}");
                Task.Factory.StartNew(() => { udpClient.Send(txBytes, txBytes.Length); });
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
            var stringLength = 20;
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

    public class Band
    {
        public Member[] members{ get; set; }
    }
    public class Member
    {
        public string name{ get; set; }
        public string location{ get; set; }
        public string id{ get; set; }
        public string address{ get; set; }
        public int port{ get; set; }
    }
}

