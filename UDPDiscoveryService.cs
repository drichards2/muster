////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	UDPDiscoveryService.cs
//
// summary:	Implements the UDP discovery service class
////////////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Muster
{
    /// <summary>   A service for accessing UDP discovery information. </summary>
    internal class UDPDiscoveryService : IDisposable
    {
        /// <summary>   The logger. </summary>
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>   A local network client detail. </summary>
        internal class LocalNetworkClientDetail
        {
            /// <summary>   Identifier for the socket owner. </summary>
            public string socket_owner_id;
            /// <summary>   The address. </summary>
            public string address;
            /// <summary>   The port. </summary>
            public int port;
            /// <summary>   Identifier for the required destination. </summary>
            public string required_destination_id;

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Returns a string that represents the current object. </summary>
            ///
            /// <returns>   A string that represents the current object. </returns>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public override string ToString()
            {
                return $"Owner: {socket_owner_id} will send to target: {required_destination_id} from {address}:{port}";
            }
        }

        /// <summary>   The listener. </summary>
        private Socket listener;
        /// <summary>   The cancellation token source. </summary>
        CancellationTokenSource ctokenSource = new CancellationTokenSource();

        /// <summary>   The network port to listen on. </summary>
        public const int NETWORK_LISTEN_PORT = 55115;

        /// <summary>   The clients. </summary>
        private List<LocalNetworkClientDetail> _clients = new List<LocalNetworkClientDetail>();

        /// <summary>   The queue. </summary>
        private ConcurrentQueue<LocalNetworkClientDetail> queue = new ConcurrentQueue<LocalNetworkClientDetail>();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets the local clients. </summary>
        ///
        /// <value> The local clients. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public List<LocalNetworkClientDetail> LocalClients { get
            {
                while (queue.TryDequeue(out var newClient))
                {
                    logger.Debug("UDP dequeue client {client}", newClient.ToString());
                    _clients.Add(newClient);
                }
                return _clients;
            } }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Check details are received for a client. </summary>
        ///
        /// <param name="clientID"> Identifier for the client. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool CheckDetailReceivedFor(string clientID)
        {
            bool isSeen = false;
            foreach (var detail in LocalClients)
            {
                if (detail.socket_owner_id == clientID)
                {
                    isSeen = true;
                    break;
                }
            }
            return isSeen;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Broadcast endpoint information to local peers. </summary>
        ///
        /// <param name="clientDetail"> The client detail. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void BroadcastClientAvailable(LocalNetworkClientDetail clientDetail)
        {
            logger.Debug($"Broadcasting local detail {clientDetail.address}:{clientDetail.port}");
            var broadcastAddress = new IPEndPoint(IPAddress.Broadcast, NETWORK_LISTEN_PORT);
            string detailAsJSON = JsonConvert.SerializeObject(clientDetail);
            byte[] broadcastPacket = Encoding.ASCII.GetBytes(detailAsJSON);
            listener.SendTo(broadcastPacket, broadcastAddress);
        }

        /// <summary>   Clears the local clients. </summary>
        public void ClearLocalClients()
        {
            logger.Debug("Clear local clients");
            while (queue.TryDequeue(out var newClient))
                ; // Do nothing

            _clients.Clear();
        }

        /// <summary>   Default constructor. </summary>
        public UDPDiscoveryService()
        {
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            listener.EnableBroadcast = true;

            IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, NETWORK_LISTEN_PORT);
            listener.Bind(groupEP);

 
            var listenerTask = new Task(() =>
            {
                listener.ReceiveTimeout = 5000;

                const int BLOCK_SIZE = 1024;
                byte[] buffer = new byte[BLOCK_SIZE];
                while (!ctokenSource.Token.IsCancellationRequested)
                {
                    try
                    {
                        // Blocks until a message returns on this socket from a remote host.
                        var bytesReceived = listener.Receive(buffer);
                        logger.Debug("UDP Discovery message {udp_packet}", buffer);

                        try
                        {
                            string jsonRepr = Encoding.ASCII.GetString(buffer.Take(bytesReceived).ToArray());
                            logger.Debug("Received client detail {client_json}", jsonRepr);
                            var clientDetail = JsonConvert.DeserializeObject<LocalNetworkClientDetail>(jsonRepr);
                            logger.Debug("Queued client detail {client}", clientDetail.ToString());
                            queue.Enqueue(clientDetail);
                        }
                        catch
                        {

                        }
                    }
                    catch (SocketException se)
                    {
                        // Probably OK?
                    }
                }
            });
            listenerTask.Start();
        }

        #region IDisposable Support
        /// <summary>   To detect redundant calls. </summary>
        private bool disposedValue = false;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   This code added to correctly implement the disposable pattern. </summary>
        ///
        /// <param name="disposing">    True to release both managed and unmanaged resources; false to
        ///                             release only unmanaged resources.
        /// </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ctokenSource.Cancel();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.

                queue = null;

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UDPDiscoveryService()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        /// <summary>   This code added to correctly implement the disposable pattern. </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
