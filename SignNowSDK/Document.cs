using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Extensions;
using System;
using System.Net;
using System.IO;
using System.Xml;

namespace SignNowSDK
{
    public class Document
    {
        /// <summary>
        /// Uploads a File and Creates a Document
        /// Accepted File Types: .doc, .docx, .pdf, .png
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="FilePath">Local Path to the File</param>
        /// <param name="ExtractFields">If set TRUE the document will be checked for special field tags. If any exist they will be converted to fields.</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>ID of the document that was created</returns>
        public static dynamic Create(string AccessToken, string FilePath, bool ExtractFields = false, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var path = (ExtractFields) ? "/document/fieldextract" : "/document";

            var request = new RestRequest(path, Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken)
                .AddFile("file", Path.GetFullPath(FilePath));

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
        /// Updates an Existing Document
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentID">Document Id</param>
        /// <param name="DataObj">Data Object (ex. dynamic new { fields = new[] { new { x = 10, y = 10, width = 122... } } }</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Document ID</returns>
        public static dynamic Update(string AccessToken, string DocumentId, dynamic DataObj, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/document/" + DocumentId, Method.PUT)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

                request.RequestFormat = DataFormat.Json;
                request.AddBody(DataObj);

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
        /// Retrieve a Document Resource
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentId">Document Id</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Document Information, Status, Fields...</returns>
        public static dynamic Get(string AccessToken, string DocumentId, bool WithAnnotations = false, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            var qsWithAnnotations = (WithAnnotations) ? "?with_annotation=true" : "";

            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/document/" + DocumentId + qsWithAnnotations, Method.GET)
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
        /// Deletes an Existing Document
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentId">Document Id</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>{"status":"success"}</returns>
        public static dynamic Delete(string AccessToken, string DocumentId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/document/" + DocumentId, Method.DELETE)
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
        /// Downloads a Collapsed Document
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentId">Document Id</param>
        /// <param name="SaveFilePath">Local Path to Save File</param>
        /// <param name="SaveFileName">File Name without Extension</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Collapsed document in PDF format saved to a the location provided.</returns>
        public static dynamic Download(string AccessToken, string DocumentId, string SaveFilePath = "", string SaveFileName = "my-collapsed-document", string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/document/" + DocumentId + "/download?type=collapsed", Method.GET)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

            var response = client.Execute(request);

            dynamic results = "";

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var path = (SaveFilePath != "") ? Path.GetDirectoryName(SaveFilePath) + "\\" + SaveFileName + ".pdf" : Directory.GetCurrentDirectory() + "\\" + SaveFileName + ".pdf";
                client.DownloadData(request).SaveAs(path);

                dynamic jsonObject = new JObject();
                jsonObject.file = path;

                results = JsonConvert.SerializeObject(jsonObject);
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
        /// Send a Role-based or Free Form Document Invite
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentId"></param>
        /// <param name="DataObj">Data Object (ex. dynamic new { to = new[] { new { email = "name@domain.com", role_id = ... } } }</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <param name="DisableEmail">Disable the Notification Email</param>
        /// <returns>{"result":"success"}</returns>
        public static dynamic Invite(string AccessToken, string DocumentId, dynamic DataObj, string ResultFormat = "JSON", bool DisableEmail = false)
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);
            var disableEmail = (DisableEmail) ? "?email=disable" : "";

            var request = new RestRequest("/document/" + DocumentId + "/invite" + disableEmail, Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

            request.RequestFormat = DataFormat.Json;
            request.AddBody(DataObj);

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
        /// Cancel Invite
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentId"></param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>{"status":"success"}</returns>
        public static dynamic CancelInvite(string AccessToken, string DocumentId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/document/" + DocumentId + "/fieldinvitecancel", Method.PUT)
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
        /// Create a One-time Use Download URL
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentId"></param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>URL to download the document as a PDF</returns>
        public static dynamic Share(string AccessToken, string DocumentId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/document/" + DocumentId + "/download/link", Method.POST)
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
        /// Merges Existing Documents
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DataObj">Data Object (ex. dynamic new { to = new[] { new { name = "My New Merged Doc", document_ids = ... } } }</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Location the PDF file was saved to.</returns>
        public static dynamic Merge(string AccessToken, dynamic DataObj, string SaveFilePath = "", string SaveFileName = "my-merged-document", string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/document/merge", Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

            request.RequestFormat = DataFormat.Json;
            request.AddBody(DataObj);

            var response = client.Execute(request);

            dynamic results = "";

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var path = (SaveFilePath != "") ? Path.GetDirectoryName(SaveFilePath) + "\\" + SaveFileName + ".pdf" : Directory.GetCurrentDirectory() + "\\" + SaveFileName + ".pdf";
                client.DownloadData(request).SaveAs(path);

                dynamic jsonObject = new JObject();
                jsonObject.file = path;

                results = JsonConvert.SerializeObject(jsonObject);
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
        /// Get Document History
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DocumentId"></param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Array of history for the document.</returns>
        public static dynamic History(string AccessToken, string DocumentId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/document/" + DocumentId + "/history", Method.GET)
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
