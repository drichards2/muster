////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	MusterAPIExtended.cs
//
// summary:	Implements the extended Muster API class
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.Threading.Tasks;

namespace Muster
{
    /// <summary>   An extended Muster API. </summary>
    class MusterAPIExtended : MusterAPI
    {

        /// <summary>   Connection phases. </summary>
        public class ConnectionPhases
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Indicate connection phase is complete. </summary>
            ///
            /// <value> CONNECT </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            static public string CONNECT => "connect";

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Indicate the endpoints are registered. </summary>
            ///
            /// <value> ENDPOINTS_REGISTERED </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            static public string ENDPOINTS_REGISTERED => "epreg";

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Indicate the local discovery is complete. </summary>
            ///
            /// <value> LOCAL_DISCOVERY_DONE </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            static public string LOCAL_DISCOVERY_DONE => "discdone";

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Indicate the binding is complete. </summary>
            ///
            /// <value> BINDING_DONE </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            static public string BINDING_DONE => "binddone";
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets UDP end point. </summary>
        ///
        /// <exception cref="ArgumentException">    Thrown when one or more arguments have unsupported or
        ///                                         illegal values.
        /// </exception>
        ///
        /// <returns>   An asynchronous result that yields the UDP end point. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets UDP port. </summary>
        ///
        /// <returns>   An asynchronous result that yields the UDP port. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<int> GetUDPPort()
        {
            var config = await GetServerConfig();
            var port = config?.UdpPort;
            if (port.HasValue)
                return port.Value;
            else
                return 0;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if one peer has completed a particular connection phase. </summary>
        ///
        /// <param name="bandID">   Identifier for the band. </param>
        /// <param name="phase">    The phase. </param>
        ///
        /// <returns>   An asynchronous result that yields true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<bool> ConnectionPhaseAnyResponse(string bandID, string phase)
        {
            var connection = await GetConnectionPhase(bandID, phase);
            return connection.Count > 0;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Query if all peers have completed a particular connection phase. </summary>
        ///
        /// <param name="workingBand">  The working band. </param>
        /// <param name="bandID">       Identifier for the band. </param>
        /// <param name="phase">        The phase. </param>
        ///
        /// <returns>   An asynchronous result that yields true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

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
