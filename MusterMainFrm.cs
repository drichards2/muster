﻿using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net;
using System.Net.Sockets;
using System.Text;
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

        public Muster()
        {
            InitializeComponent();

            for (var i=0; i<8; i++)
            {
                var soundReader = new SoundPlayer(System.IO.Path.Combine("soundfiles", $"handbell{i + 1}.wav"));
                soundReader.LoadAsync();
                bellSamples.Add(soundReader);                
            }
        }

        private void Go_Click(object sender, EventArgs e)
        {
            Disconnect();

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

                    byte[] data = Encoding.ASCII.GetBytes("ConnectPlease");
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

                    byte[] data = Encoding.ASCII.GetBytes("OK?");
                    peerSockets[connectRows].Send(data);
                }
            }


        }

        private void Disconnect()
        {
            foreach (var oldSock in peerSockets)
            {
                oldSock.Disconnect(false);
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

    }
}
