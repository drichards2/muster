﻿using System;
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
            public delegate void BroadcastAlive(int peerChannel);
            public delegate void BellStrike(char keyStroke);

            public CancellationToken cancellationToken;
            public Socket srcSocket;
            public int peerChannel;
            public BroadcastAlive EchoBackEvent;
            public BellStrike BellStrikeEvent;
        }
    }
}
