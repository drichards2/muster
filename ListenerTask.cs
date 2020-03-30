using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Muster
{
    class ListenerTask
    {
        internal class ListenerConfig
        {
            public delegate void BroadcastAlive();
            public delegate void BellStrike(int bell);

            public CancellationToken cancellationToken;
            public UdpClient srcSocket;
            public int peerChannel;
            public BroadcastAlive EchoBackEvent;
            public BellStrike BellStrikeEvent;
        }
    }
}
