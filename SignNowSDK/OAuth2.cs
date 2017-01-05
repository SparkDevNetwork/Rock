using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Xml;

namespace SignNowSDK
{
    public class OAuth2
    {
        /// <summary>
        /// Request an Access Token using the User's Credentials
        /// </summary>
        /// <param name="Email"></param>
        /// <param name="Password"></param>
        /// <param name="Scope">A space delimited list of API URIs e.g. "user%20documents%20user%2Fdocumentsv2"</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>New Access Token, Token Type, Expires In, Refresh Token, ID, Scope</returns>
        public static dynamic RequestToken(string Email, string Password, string Scope = "*", string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/oauth2/token", Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Basic " + Config.EncodedClientCredentials)
                .AddHeader("Content-Type", "application/x-www-form-urlencoded")
                .AddParameter("username", Email)
                .AddParameter("password", Password)
                .AddParameter("grant_type", "password")
                .AddParameter("scope", Scope);

                request.RequestFormat = DataFormat.Json;

            var response = client.Execute(request);

            dynamic results = "";

            if (response.StatusCode == HttpStatusCode.OK)
            {
                results = response.Content;
            }
            else
            {
                Console.WriteLine(response.Content.ToString());
                results = response.Content.ToString();
            }

            if (ResultFormat == "JSON")
            {
                results = JsonConvert.DeserializeObject(results);
            }
            else if (ResultFormat == "XML")
            {
                results = (XmlDocument)JsonConvert.DeserializeXmlNode(results, "root");
            }

            return results;
        }

        /// <summary>
        /// Verify a User's Access Token
        /// </summary>
        /// <param name="AccessToken">User's Access Token</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Access Token, Token Type, Expires In, Refresh Token, Scope</returns>
        public static dynamic Verify(string AccessToken, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/oauth2/token", Method.GET)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

            var response = client.Execute(request);

            dynamic results = "";

            if (response.StatusCode == HttpStatusCode.OK)
            {
                results = response.Content;
            }
            else
            {
                Console.WriteLine(response.Content.ToString());
                results = response.Content.ToString();
            }

            if (ResultFormat == "JSON")
            {
                results = JsonConvert.DeserializeObject(results);
            }
            else if (ResultFormat == "XML")
            {
                results = (XmlDocument)JsonConvert.DeserializeXmlNode(results, "root");
            }

            return results;
        }
    }
}
