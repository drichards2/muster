using NAudio.Wave;
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
        
        private List<SoundPlayer> bellSamples = new List<SoundPlayer>();       

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
            nextBell = 0;
            //roundsTimer.Enabled = true;
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

        private Socket _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        private void GoInternet_Click(object sender, EventArgs e)
        {            
            _socket.Connect(IPAddress.Parse(holePunchIP.Text), int.Parse(holePunchPort.Text));

            byte[] data = Encoding.ASCII.GetBytes("TestString");

            var sent = _socket.Send(data);                                

        }
    }
}
