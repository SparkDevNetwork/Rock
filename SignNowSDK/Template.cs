using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Xml;

namespace SignNowSDK
{
    public class Template
    {

        /// <summary>
        /// Create a Template by Flattening an Existing Document
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentId">The ID of the Document to make a Template</param>
        /// <param name="DocumentName">New Template Name</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>The ID of the new Template</returns>
        public static dynamic Create(string AccessToken, string DocumentId, string DocumentName = "", string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/template", Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

            dynamic reqObj;

            if (DocumentName != "")
            {
                reqObj = new { document_id = DocumentId, document_name = DocumentName };
            }
            else
            {
                reqObj = new { document_id = DocumentId };
            }

            request.RequestFormat = DataFormat.Json;
            request.AddBody(reqObj);

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
        /// Create a New Document by Copying a Flattened Template
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentId"></param>
        /// <param name="DocumentName"></param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>New Document ID and Name</returns>
        public static dynamic Copy(string AccessToken, string DocumentId, string DocumentName = "", string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/template/" + DocumentId + "/copy", Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

            if (DocumentName != "")
            {
                request.RequestFormat = DataFormat.Json;
                request.AddBody(new { document_name = DocumentName });
            }            

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
