////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	ListenerTask.cs
//
// summary:	Implements the listener task class
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Muster
{
    /// <summary>   A listener task. </summary>
    class ListenerTask
    {
        /// <summary>   A listener configuration. </summary>
        internal class ListenerConfig
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Method to call to broadcast that socket is alive. </summary>
            ///
            /// <param name="peerChannel">  The peer channel. </param>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public delegate void BroadcastAlive(int peerChannel);

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Bell strike. </summary>
            ///
            /// <param name="keyStroke">    The key stroke. </param>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public delegate void BellStrike(char keyStroke);

            /// <summary>   A token that allows processing to be cancelled. </summary>
            public CancellationToken cancellationToken;
            /// <summary>   Source socket. </summary>
            public Socket srcSocket;
            /// <summary>   The peer channel. </summary>
            public int peerChannel;
            /// <summary>   The echo back event. </summary>
            public BroadcastAlive EchoBackEvent;
            /// <summary>   The bell strike event. </summary>
            public BellStrike BellStrikeEvent;
        }
    }
}
