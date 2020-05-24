////////////////////////////////////////////////////////////////////////////////////////////////////
// file:	MusterAPI.cs
//
// summary:	Implements the Muster API class
////////////////////////////////////////////////////////////////////////////////////////////////////

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Muster
{
    /// <summary>   A Muster API. </summary>
    internal class MusterAPI
    {
        /// <summary>   The logger. </summary>
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>   A server configuration. </summary>
        public class ServerConfig
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets or sets the UDP port. </summary>
            ///
            /// <value> The UDP port. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public int UdpPort { get; set; }
        }

        /// <summary>   A band. </summary>
        public class Band
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets or sets the members. </summary>
            ///
            /// <value> The members. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public Member[] members { get; set; }
        }
        /// <summary>   A member. </summary>
        public class Member
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets or sets the name. </summary>
            ///
            /// <value> The name. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public string name { get; set; }

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets or sets the location. </summary>
            ///
            /// <value> The location. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public string location { get; set; }

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets or sets the identifier. </summary>
            ///
            /// <value> The identifier. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public string id { get; set; }
        }

        /// <summary>   An endpoint. </summary>
        public class Endpoint
        {
            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets or sets the identifier of the target. </summary>
            ///
            /// <value> The identifier of the target. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public string target_id {get; set;}

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets or sets the IP. </summary>
            ///
            /// <value> The IP. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public string ip {get; set;}

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets or sets the port. </summary>
            ///
            /// <value> The port. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public int port {get; set;}

            ////////////////////////////////////////////////////////////////////////////////////////////////////
            /// <summary>   Gets or sets a value indicating whether the peer is local. </summary>
            ///
            /// <value> True if check local, false if not. </value>
            ////////////////////////////////////////////////////////////////////////////////////////////////////

            public bool check_local {get; set;}
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the API server. </summary>
        ///
        /// <value> The API server. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public string APIServer { get; set; } = "muster.norfolk-st.co.uk";

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets or sets the API endpoint. </summary>
        ///
        /// <value> The API endpoint. </value>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public string APIEndpoint { get; set; } = "https://muster.norfolk-st.co.uk/v1/";

        /// <summary>   The client. </summary>
        private static readonly HttpClient client = new HttpClient();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets server configuration. </summary>
        ///
        /// <returns>   An asynchronous result that yields the server configuration. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<ServerConfig> GetServerConfig()
        {
            AcceptJson();

            var response = await client.GetAsync(APIEndpoint + "config");

            if ((int)response.StatusCode == 200)
            {
                return JsonConvert.DeserializeObject<ServerConfig>(await response.Content.ReadAsStringAsync());
            }
            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Creates the band. </summary>
        ///
        /// <returns>   An asynchronous result that yields the new band. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<string> CreateBand()
        {
            // Avoid deadlock: https://stackoverflow.com/questions/14435520/why-use-httpclient-for-synchronous-connection
            var response = await client.PostAsync(APIEndpoint + "bands", null);

            if ((int)response.StatusCode == 201)
            {
                var newbandID = await ReadStringResponse(response);
                logger.Debug("Created new band with ID: " + newbandID);
                return newbandID;
            }
            else
            {
                logger.Error("Error creating band: " + response.ReasonPhrase);
                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Reads string response. </summary>
        ///
        /// <param name="response"> The response. </param>
        ///
        /// <returns>   An asynchronous result that yields the string response. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        private async Task<string> ReadStringResponse(HttpResponseMessage response)
        {            
            var responseString = await response.Content.ReadAsStringAsync();
            if (responseString.Length > 0)
            {
                return JsonConvert.DeserializeObject<string>(responseString);
            }
            else
            {
                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sends a join band request. </summary>
        ///
        /// <param name="bandID">   Identifier for the band. </param>
        /// <param name="member">   The member. </param>
        ///
        /// <returns>   An asynchronous result that yields a Tuple&lt;string,bool&gt; </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<Tuple<string,bool>> SendJoinBandRequest(string bandID, Member member)
        {
            logger.Debug("Joining band: " + bandID);
            var json = JsonConvert.SerializeObject(member);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(APIEndpoint + "bands/" + bandID + "/members", content);
            string clientID = null;
            if ((int)response.StatusCode == 201)
            {
                clientID = await ReadStringResponse(response);
                logger.Debug("Joined band with client ID: " + clientID);
                return new Tuple<string,bool>(clientID, false);
            }
            else
            {
                logger.Error("Error joining band " + bandID + ": " + response.ReasonPhrase);
                return new Tuple<string,bool>(clientID, (int)response.StatusCode == 403);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sends a leave band request. </summary>
        ///
        /// <param name="bandID">   Identifier for the band. </param>
        /// <param name="clientID"> Identifier for the client. </param>
        ///
        /// <returns>   An asynchronous result that yields true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<bool> SendLeaveBandRequest(string bandID, string clientID)
        {
            logger.Debug("Leaving band: " + bandID);
            var response = await client.DeleteAsync(APIEndpoint + "bands/" + bandID + "/members/" + clientID);
            if ((int)response.StatusCode == 204)
                return true;
            else
            {
                logger.Error("Error leaving band '" + bandID + "': " + response.ReasonPhrase);
                return false;
            }
        }

        /// <summary>   Accept JSON. </summary>
        private void AcceptJson()
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Muster Client");
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a band. </summary>
        ///
        /// <param name="bandID">   Identifier for the band. </param>
        ///
        /// <returns>   An asynchronous result that yields the band. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<Band> GetBand(string bandID)
        {
            logger.Debug("Finding band members in band: " + bandID);

            AcceptJson();

            var response = await client.GetAsync(APIEndpoint + "bands/" + bandID);

            if ((int)response.StatusCode == 200)
            {
                var band = JsonConvert.DeserializeObject<Band>(await response.Content.ReadAsStringAsync());
                return band;
            }
            else
            {
                logger.Error("No record of band ID '" + bandID + "': " + response.ReasonPhrase);
                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets connection phase. </summary>
        ///
        /// <param name="bandID">   Identifier for the band. </param>
        /// <param name="phase">    The phase. </param>
        ///
        /// <returns>   An asynchronous result that yields the connection phase. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<List<string>> GetConnectionPhase(string bandID, string phase)
        {
            logger.Debug($"Getting status for band >{bandID}< at phase >{phase}<");

            AcceptJson();

            var response = await client.GetAsync($"{APIEndpoint}bands/{bandID}/connection/{phase}");

            if ((int)response.StatusCode == 200)
            {
                var connection = JsonConvert.DeserializeObject<List<string>>(await response.Content.ReadAsStringAsync());
                return connection;
            }
            else
            {
                logger.Error($"Could not get connection status of '{bandID}/{phase}': {response.ReasonPhrase}");
                return null;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Sets connection status. </summary>
        ///
        /// <param name="bandID">   Identifier for the band. </param>
        /// <param name="phase">    The phase. </param>
        /// <param name="clientID"> Identifier for the client. </param>
        ///
        /// <returns>   An asynchronous result that yields true if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<bool> SetConnectionStatus(string bandID, string phase, string clientID)
        {
            logger.Debug($"Setting status for band >{bandID}< at phase >{phase}<");

            var json = JsonConvert.SerializeObject(clientID);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{APIEndpoint}bands/{bandID}/connection/{phase}", content);
            if ((int)response.StatusCode == 200)
            {
                return true;
            }
            else
            {
                logger.Error($"Could not set connection status of '{bandID}/{phase}': {response.ReasonPhrase}");
                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets endpoints for band. </summary>
        ///
        /// <param name="bandID">   Identifier for the band. </param>
        /// <param name="clientID"> Identifier for the client. </param>
        ///
        /// <returns>   An asynchronous result that yields the endpoints for band. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public async Task<List<Endpoint>> GetEndpointsForBand(string bandID, string clientID)
        {
            logger.Debug("Getting endpoints for band " + bandID + " for client " + clientID);

            AcceptJson();

            var response = await client.GetAsync(APIEndpoint + "bands/" + bandID + "/endpoints/" + clientID);

            if ((int)response.StatusCode == 200)
            {
                var raw = await response.Content.ReadAsStringAsync();
                var endpoints = JsonConvert.DeserializeObject<List<Endpoint>>(raw);
                return endpoints;
            }
            else
            {
                logger.Error("Could not get endpoints for '" + bandID + "' for client '" + clientID + "' : " + response.ReasonPhrase);
                return null;
            }
        }
    }
}
