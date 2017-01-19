using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Xml;

namespace SignNowSDK
{
    public class User
    {
        /// <summary>
        /// Creates a New User SignNow Account
        /// </summary>
        /// <param name="Email">New User's Email Address</param>
        /// <param name="Password">New User's Password</param>
        /// <param name="FirstName">New User's First Name</param>
        /// <param name="LastName">New User's Last Name</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>The ID of the new user account and verification status.</returns>
        public static dynamic Create(string Email, string Password, string FirstName = "", string LastName = "", string ClientId = "", string ClientSecret = "", string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var clientCredentials = Config.EncodedClientCredentials;

            if (ClientId != "" && ClientSecret != "")
            {
                clientCredentials = Config.encodeClientCredentials(ClientId, ClientSecret);
            }

            var request = new RestRequest("/user", Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Basic " + Config.EncodedClientCredentials);

                request.RequestFormat = DataFormat.Json;
                request.AddBody(new { email = Email, password = Password, first_name = FirstName, last_name = LastName });

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
        /// Retrieves a User Account
        /// </summary>
        /// <param name="AccessToken">User's Access Token</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>User Account Information</returns>
        public static dynamic Get(string AccessToken,  string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/user", Method.GET)
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
