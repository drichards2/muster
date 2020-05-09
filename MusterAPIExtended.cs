////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	MusterAPIExtended.cs
//
// summary:	Implements the muster a pi extended class
////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.Net;
using System.Threading.Tasks;

namespace Muster
{
    /// <summary>   A muster a pi extended. </summary>
    class MusterAPIExtended : MusterAPI
    {

        /// <summary>   A connection phases. </summary>
        public class ConnectionPhases
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets the connect. </summary>
            ///
            /// <value> The connect. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            static public string CONNECT => "connect";

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets the endpoints registered. </summary>
            ///
            /// <value> The endpoints registered. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            static public string ENDPOINTS_REGISTERED => "epreg";

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets the local discovery done. </summary>
            ///
            /// <value> The local discovery done. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            static public string LOCAL_DISCOVERY_DONE => "discdone";

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets the binding done. </summary>
            ///
            /// <value> The binding done. </value>
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
        /// <summary>   Connection phase any response. </summary>
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
        /// <summary>   Connection phase all responded. </summary>
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
