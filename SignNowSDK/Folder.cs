using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Xml;

namespace SignNowSDK
{
    public class Folder
    {
        /// <summary>
        /// Gets a List of Folders
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Folders, Document & Template Counts</returns>
        public static dynamic List(string AccessToken, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/folder", Method.GET)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="FolderId">ID of the Folder to Get</param>
        /// <param name="Params">Option Filter and Sort By Params</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>List of documents in the folder.</returns>
        public static dynamic Get(string AccessToken, string FolderId, string Params = "", string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            string qsParams = (Params != "") ? "?" + Params : "";

            var request = new RestRequest("/folder/" + FolderId + qsParams, Method.GET)
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
