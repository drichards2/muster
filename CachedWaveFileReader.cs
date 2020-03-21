using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Muster
{
    class CachedWaveFileReader : IWaveProvider
    {        
        private WaveFormat waveFormat;
        private byte[] waveBytes = null;

        public CachedWaveFileReader(string waveFile, int maxBytes = 10000000)
        {
            List<byte> waveList = new List<byte>(maxBytes);

            using (var baseReader = new WaveFileReader(waveFile))
            {
                waveFormat = baseReader.WaveFormat;


                const int BLOCK_SIZE = 1000;
                byte[] workingBlock = new byte[BLOCK_SIZE];

                for (var index = 0; index < maxBytes; index += BLOCK_SIZE)
                {
                    var bytesRead = baseReader.Read(workingBlock, 0, BLOCK_SIZE);

                    waveList.AddRange(workingBlock.Take(bytesRead));
                    if (bytesRead < BLOCK_SIZE)
                        break;
                }
            }

            waveBytes = waveList.ToArray();
        }

        public WaveFormat WaveFormat => waveFormat;

        public int Position;

        public void Rewind()
        {
            Position = 0;
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            var bytesToCopy = Math.Min(count, waveBytes.Length - Position);
            bytesToCopy = Math.Max(bytesToCopy, 0);
            Buffer.BlockCopy(waveBytes, Position, buffer, offset, bytesToCopy);
            Position += bytesToCopy;
            return bytesToCopy;
        }
    }
}
