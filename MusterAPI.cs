using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Muster
{
    internal class MusterAPI
    {

        public class ServerConfig
        {
            public int UdpPort;
        }

        public class Band
        {
            public Member[] members { get; set; }
        }
        public class Member
        {
            public string name { get; set; }
            public string location { get; set; }
            public string id { get; set; }
        }

        public class Endpoint
        {

        }

        public string APIServer { get; set; } = "muster.norfolk-st.co.uk";
        public string APIEndpoint { get; set; } = "https://muster.norfolk-st.co.uk/v1/";

        private static readonly HttpClient client = new HttpClient();


        public async Task<ServerConfig> GetServerConfig()
        {

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Muster Client");

            var response = await client.GetAsync(APIEndpoint + "config");

            if ((int)response.StatusCode == 200)
            {
                return JsonConvert.DeserializeObject<ServerConfig>(await response.Content.ReadAsStringAsync());
            }
            return null;
        }

        public async Task<string> CreateBand()
        {
            // Avoid deadlock: https://stackoverflow.com/questions/14435520/why-use-httpclient-for-synchronous-connection
            var response = await client.PostAsync(APIEndpoint + "bands", null);

            if ((int)response.StatusCode == 201)
            {
                return await ReadStringResponse(response);
            }
            else
            {
                Debug.WriteLine("Error creating band: " + response.ReasonPhrase);
                return "";
            }
        }

        private async Task<string> ReadStringResponse(HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            if (responseString.Length > 0)
            {
                var newbandID = responseString;
                newbandID = newbandID.Replace("\"", ""); //swaggerhub includes double quotes at start and end
                Debug.WriteLine("Created new band with ID: " + newbandID);
                return newbandID;
            }
            else
            {
                return "";
            }
        }

        public async Task<string> SendJoinBandRequest(string bandID, Member member)
        {
            Debug.WriteLine("Joining band: " + bandID);
            var json = JsonConvert.SerializeObject(member);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PutAsync(APIEndpoint + "bands/" + bandID + "/members", content);
            if ((int)response.StatusCode == 204)
            {
                return await ReadStringResponse(response);
            }
            else
            {
                Debug.WriteLine("Error joining band " + bandID + ": " + response.ReasonPhrase);
                return "";
            }
        }

        public async Task<Band> GetBand(string bandID)
        {
            Debug.WriteLine("Finding band members in band: " + bandID);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("User-Agent", "Muster Client");

            var response = await client.GetAsync(APIEndpoint + "bands/" + bandID);

            if ((int)response.StatusCode == 200)
            {
                var band = JsonConvert.DeserializeObject<Band>(await response.Content.ReadAsStringAsync());
                return band;
            }
            else
            {
                Debug.WriteLine("No record of band ID '" + bandID + "': " + response.ReasonPhrase);
                return null;
            }
        }
    }
}
