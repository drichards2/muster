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
            public delegate void RegisterOK(int peerChannel);
            public delegate void BellStrike(int bell);

            public CancellationToken cancellationToken;
            public Socket srcSocket;
            public int peerChannel;
            public RegisterOK EchoBackEvent;
            public BellStrike BellStrikeEvent;
        }
    }
}
