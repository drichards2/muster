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
    internal class UDPDiscoveryService : IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        internal class LocalNetworkClientDetail
        {
            public string socket_owner_id;
            public string address;
            public int port;
            public string required_destination_id;
        }

        private Socket listener;
        CancellationTokenSource ctokenSource = new CancellationTokenSource();

        public const int NETWORK_LISTEN_PORT = 55115;

        private List<LocalNetworkClientDetail> _clients = new List<LocalNetworkClientDetail>();

        private ConcurrentQueue<LocalNetworkClientDetail> queue = new ConcurrentQueue<LocalNetworkClientDetail>();
        public List<LocalNetworkClientDetail> LocalClients { get
            {
                while (queue.TryDequeue(out var newClient))
                {
                    logger.Debug("UDP dequeue client {client}", newClient);
                    _clients.Add(newClient);
                }
                return _clients;
            } }


        public void BroadcastClientAvailable(LocalNetworkClientDetail clientDetail)
        {
            var broadcastAddress = new IPEndPoint(IPAddress.Broadcast, NETWORK_LISTEN_PORT);
            string detailAsJSON = JsonConvert.SerializeObject(clientDetail);
            byte[] broadcastPacket = Encoding.ASCII.GetBytes(detailAsJSON);
            listener.SendTo(broadcastPacket, broadcastAddress);
        }

        public void ClearLocalClients()
        {
            logger.Debug("Clear local clients");
            while (queue.TryDequeue(out var newClient))
                ; // Do nothing

            _clients.Clear();
        }

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
                            logger.Debug("Queued client detail {client}", clientDetail);
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
        private bool disposedValue = false; // To detect redundant calls

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

        // This code added to correctly implement the disposable pattern.
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
