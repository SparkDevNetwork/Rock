using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Xml;

namespace SignNowSDK
{
    public class Link
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentId"></param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>An Array of History for the Document</returns>
        public static dynamic Create(string AccessToken, string DocumentId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/link", Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

            request.RequestFormat = DataFormat.Json;
            request.AddBody(new { document_id = DocumentId });

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
