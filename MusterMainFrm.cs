using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Muster
{
    public partial class Muster : Form
    {
        private int nextBell;
        private List<WaveOutEvent> bellSounds = new List<WaveOutEvent>();
        private List<WaveFileReader> bellSamples = new List<WaveFileReader>();

        public Muster()
        {
            InitializeComponent();

            for (var i=0; i<8; i++)
            {
                var wavReader = new WaveFileReader(System.IO.Path.Combine("soundfiles", $"handbell{i + 1}.wav"));
                bellSamples.Add(wavReader);
                var sample = new WaveOutEvent();
                sample.Init(wavReader);
                bellSounds.Add( sample );
            }
        }

        private void Go_Click(object sender, EventArgs e)
        {
            nextBell = 0;
            roundsTimer.Enabled = true;
        }

        private void RoundsTimer_Tick(object sender, EventArgs e)
        {
            if (nextBell >= 2 * bellSounds.Count)
            {
                nextBell = 0;
            }
            else
            {
                bellSamples[nextBell % bellSounds.Count].Position = 0;
                bellSounds[nextBell % bellSounds.Count].Play();
                nextBell = nextBell + 1;
            }
        }
    }
}
