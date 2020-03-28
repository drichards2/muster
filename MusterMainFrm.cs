﻿using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
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
        private List<Task> peerListeners = new List<Task>(MAX_PEERS);
        private List<CancellationTokenSource> peerCancellation = new List<CancellationTokenSource>(MAX_PEERS);

        private IntPtr AbelHandle;

        private static readonly HttpClient client = new HttpClient();
        private string serverAddress = "http://virtserver.swaggerhub.com/drichards2/muster/1.0.0/";

        public Muster()
        {
            InitializeComponent();

            FindAbel();

            for(int i = 0; i < numberOfBells; i++)
            {
                RingBell(i);
                System.Threading.Thread.Sleep(230);
            }

            GetTheBandBackTogether();
        }

        private void MakeNewBand_Click(object sender, EventArgs e)
        {
            SendCreateBandRequest();
        }

        private async Task SendCreateBandRequest()
        {
            var response = await client.PostAsync(serverAddress + "bands", new FormUrlEncodedContent(new Dictionary<string, string>()));
            if ((int)response.StatusCode != 201)
            {
                Debug.WriteLine("Error creating band: " + response.ReasonPhrase);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            if (responseString.Length > 0)
            {
                var newbandID = JsonConvert.DeserializeObject<string>(responseString);
                Debug.WriteLine("Created new band with ID: " + newbandID);
                bandID.Text = newbandID;
            }
        }

        private void JoinBand_Click(object sender, EventArgs e)
        {
            var member = new Member();
            member.id = "sdfsdgdf";
            member.name = "Jonathan";
            member.location = "Ditton Lane";
            SendJoinBandRequest(bandID.Text, member);
        }

        private async Task SendJoinBandRequest(string bandID, Member member)
        {  
            var json = JsonConvert.SerializeObject(member);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PutAsync(serverAddress + "bands/" + bandID + "/members", content);
            if ((int)response.StatusCode != 204)
            {
                Debug.WriteLine("Error joining band " + bandID + ": " + response.ReasonPhrase);
            }
        }

        private async Task GetTheBandBackTogether()
        {
            Debug.WriteLine("Finding band members in band: " + bandID.Text);
            connectionList.Rows.Clear();

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Muster Client");

            var streamTask = client.GetStringAsync(serverAddress + "bands/" + bandID.Text);
            
            var msg = await streamTask;
            var band = JsonConvert.DeserializeObject<Band>(msg);
            foreach (var member in band.members)
            {
                connectionList.Rows.Add(member.address, member.port);
            }
        }

        private void Holepunch_Click(object sender, EventArgs e)
        {
            DisconnectAll();

            Console.WriteLine($"Requesting to connect {connectionList.Rows.Count - 1} peers");
            if (connectionList.Rows.Count > (MAX_PEERS + 1))
            {
                MessageBox.Show($"Can't connect {connectionList.Rows.Count - 1} peers - {MAX_PEERS} is the maximum");
                return;
            }

            foreach (DataGridViewRow row in connectionList.Rows)
            {
                if (row.IsNewRow)
                    continue;

                row.Cells[2].Value = "Not connected";
                if (IPAddress.TryParse(row.Cells[0].Value.ToString(), out var ipAddr))
                {
                    var _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _socket.Connect(IPAddress.Parse(holePunchIP.Text), int.Parse(holePunchPort.Text));

                    byte[] data = Encoding.ASCII.GetBytes($"Connect{ipAddr.ToString()}InBand{bandID.Text}Please");
                    var sent = _socket.Send(data);

                    peerSockets.Add(_socket);

                    if (sent == data.Length)
                        row.Cells[2].Value = "Set port";
                    else
                        row.Cells[2].Value = "Broken";
                }
            }
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            if ( (connectionList.Rows.Count-1) != peerSockets.Count)
            {
                MessageBox.Show("There seems to be a mismatch between open sockets and peers requested. Abort abort.");
                return;
            }

            for (int connectRows = 0; connectRows< connectionList.Rows.Count; connectRows++)
            {
                var row = connectionList.Rows[connectRows];
                if (row.IsNewRow)
                    continue;

                if (IPAddress.TryParse(row.Cells[0].Value.ToString(), out var ipAddr) &&
                    int.TryParse(row.Cells[1].Value.ToString(), out var port) )
                {
                    peerSockets[connectRows].Connect(ipAddr, port);

                    var ctokenSource = new CancellationTokenSource();
                    peerCancellation.Add(ctokenSource);
                    var runParameters = new ListenerTask.ListenerConfig();
                    runParameters.cancellationToken = ctokenSource.Token;
                    runParameters.peerChannel = connectRows;
                    runParameters.srcSocket = peerSockets[connectRows];
                    runParameters.BellStrikeEvent = BellStrike;
                    runParameters.EchoBackEvent = SocketEcho;

                    var newTask = new Task(() =>
                    {
                       runParameters.srcSocket.ReceiveTimeout = 5000;

                       const int BLOCK_SIZE = 1024;
                       byte[] buffer = new byte[BLOCK_SIZE];
                       while (!runParameters.cancellationToken.IsCancellationRequested)
                       {
                           try
                           {
                               var bytesReceived = runParameters.srcSocket.Receive(buffer);

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
                    }
                    );

                    peerListeners.Add(newTask);
                    newTask.Start();
                }
            }
        }

        private void SocketEcho(int peerChannel)
        {
            Console.WriteLine($"Received echo request: {peerChannel}");
            connectionList.Rows[peerChannel].Cells[2].Value = "Connected";
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

                row.Cells[2].Value = "Waiting for reply";
            }
            foreach (var sock in peerSockets)
            {
                sock.Send(new byte[] { (byte)'?' });
            }
        }

        private void DisconnectAll()
        {
            foreach (var cancellationToken in peerCancellation)
            {
                try
                {
                    cancellationToken.Cancel();
                }
                catch (ObjectDisposedException ode)
                {
                }

            }
            foreach (var peerListener in peerListeners)
            {
                peerListener.Wait();
                peerListener.Dispose();
            }
            foreach (var cancellationToken in peerCancellation)
            {
                cancellationToken.Dispose();
            }

            foreach (var oldSock in peerSockets)
            {
                oldSock.Dispose();
            }
            peerSockets.Clear();
        }

        private void Muster_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"New key: {e.KeyValue}");
            int bellNumber = e.KeyValue - 'A';

            if ( (e.KeyValue >= 'A' && e.KeyValue < 'A' + numberOfBells) || (e.KeyValue == '?'))
            {
                var txBytes = new byte[] { (byte)e.KeyValue };
                foreach (var _socket in peerSockets)
                {
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

        const int WM_KEYDOWN= 0x100;
        const int WM_KEYUP = 0x101;
        const int WM_CHAR = 0x102;

        private void SendKeystroke(int bell)
        {
            if (AbelHandle != null)
            {
                PostMessage(AbelHandle, WM_CHAR, 'A'+bell, 0);
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
                    
                    AbelHandle = FindWindowEx(AbelHandle, IntPtr.Zero, ChildWindow,  "");
                    AbelHandle = FindWindowEx(AbelHandle, IntPtr.Zero, GrandchildWindow,  "");
                }
            }
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

