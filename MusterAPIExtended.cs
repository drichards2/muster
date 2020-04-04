using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Muster
{
    class MusterAPIExtended : MusterAPI
    {

        public async Task<IPEndPoint> GetUDPEndPoint()
        {
            var addresses = Dns.GetHostAddresses(APIServer);
            if (addresses.Length == 0)
            {
                throw new ArgumentException(
                    "Unable to retrieve address from specified host name",
                    APIServer
                );
            }

            var port = await GetUDPPort();

            return new IPEndPoint(addresses[0], port); // Port gets validated here.
        }

        public async Task<int> GetUDPPort()
        {
            var config = await GetServerConfig();
            var port = config?.UdpPort;
            if (port.HasValue)
                return port.Value;
            else
                return 0;
        }
    }
}
