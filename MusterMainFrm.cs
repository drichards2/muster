﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net;
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
        private int nextBell;
        private const int numberOfBells = 8;
        private const int MAX_PEERS = 6;

        private List<SoundPlayer> bellSamples = new List<SoundPlayer>();
        private List<Socket> peerSockets = new List<Socket>(MAX_PEERS);
        private List<Task> peerListeners = new List<Task>(MAX_PEERS);
        private List<CancellationTokenSource> peerCancellation = new List<CancellationTokenSource>(MAX_PEERS);

        private IntPtr AbelHandle;

        public Muster()
        {
            InitializeComponent();

            FindAbel();

            for(int i = 0; i<12;i++)
            {
                SendKeystroke("A");
                System.Threading.Thread.Sleep(200);
            }

        }

        private void Holepunch_Click(object sender, EventArgs e)
        {
            DisconnectAll();

            Console.WriteLine($"Requesting to connect {connectionList.Rows.Count} peers");
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

                    byte[] data = Encoding.ASCII.GetBytes($"Connect{ipAddr.ToString()}Please");
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
                                   if (buffer[i] >= '1' && buffer[i] <= '8')
                                   {
                                       runParameters.BellStrikeEvent?.Invoke(buffer[i] - '1');
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

        private void DisconnectAll()
        {
            foreach (var cancellationToken in peerCancellation)
            {
                cancellationToken.Cancel();
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

        private void RoundsTimer_Tick(object sender, EventArgs e)
        {
            if (nextBell >= 2 * numberOfBells)
            {
                nextBell = 0;
            }
            else
            {
                var timestamp = DateTime.Now;

                Console.WriteLine($"{nextBell}|{timestamp.Second}|{timestamp.Millisecond}");
                bellSamples[nextBell % numberOfBells].Play();
                nextBell = nextBell + 1;
            }
        }

        private void Muster_KeyDown(object sender, KeyEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"New key: {e.KeyValue}");
            int bellNumber = e.KeyValue - '1';

            if ( (e.KeyValue >= '1' && e.KeyValue <= '8') || (e.KeyValue == '?'))
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
            if (bellNumber >= 0 && bellNumber < 8)
            {
                bellSamples[bellNumber].Play();
            }
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

        private void Disconnect_Click(object sender, EventArgs e)
        {
            DisconnectAll();
        }

        private void Test_Click(object sender, EventArgs e)
        {
            TestConnection();
        }

        [DllImport ("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        private void SendKeystroke(string command)
        {
            if (AbelHandle != null)
            {
                SetForegroundWindow(AbelHandle);
                SendKeys.SendWait(command);
            }
        }

        private void FindAbel() 
        {
			Process[] currentProcesses = Process.GetProcesses();
			foreach (Process p in currentProcesses)
			{
				if (Convert.ToString(p.ProcessName).ToUpper() == "ABEL3")
				{
					AbelHandle = p.MainWindowHandle;
				}
			}
        }
    }
}
