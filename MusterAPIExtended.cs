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

        public async Task<bool> ConnectionPhaseAnyResponse(string bandID, string phase)
        {
            var connection = await GetConnectionPhase(bandID, phase);
            return connection.Count > 0;
        }
        public async Task<bool> ConnectionPhaseAllResponded(Band workingBand, string bandID, string phase)
        {
            var connection = await GetConnectionPhase(bandID, phase);
            foreach (var member in workingBand.members)
            {
                if (!connection.Contains(member.id))
                    return false;
            }
            return true;
        }
    }
}
