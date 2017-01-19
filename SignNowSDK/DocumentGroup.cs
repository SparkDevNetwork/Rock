using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.IO;
using System.Xml;
using System.Web.Script.Serialization;

namespace SignNowSDK
{
    public class DocumentGroup
    {
        /// <summary>
        /// Get a list of document groups for the user. If a GroupId is provided then it will return the document and role information for the document group.
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="GroupId">GroupId is optional</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Returns a list or single document group.</returns>
        public static dynamic Get(string AccessToken, string GroupId = "", string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var url = (GroupId.Length > 0) ? "/documentgroup/" + GroupId : "/user/documentgroups";

            var request = new RestRequest(url, Method.GET)
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
        /// Create a document group from a list of document ids.
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="DataObj">Data Object (ex. dynamic new { document_ids = new[] { "1234567890", "0987654321" }, group_name = "My Document Group Name" }</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Returns an Id of the new document group.</returns>
        public static dynamic Create(string AccessToken, dynamic DataObj, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/documentgroup", Method.POST)
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
        /// Deletes the document group.  Documents within the group are not deleted.  Document groups cannot be deleted while they have an group invite.
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="GroupId">Document Group Id</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Returns a Success Message</returns>
        public static dynamic Delete(string AccessToken, string GroupId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/documentgroup/" + GroupId, Method.DELETE)
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
        /// Creates a multi-step invite for a document group.
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="GroupId">Document Group Id</param>
        /// <param name="DataObj">Data Object containing the Group Invite Parameters</param>
        /// <param name="ResultFormat"></param>
        /// <returns>Returns the new Document Group Invite Id</returns>
        public static dynamic Invite(string AccessToken, string GroupId, dynamic DataObj, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/documentgroup/" + GroupId + "/groupinvite", Method.POST)
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
        /// Get Invite action information for a group invite, including the status of each step and action.  This can only can be called by the owner of the document group.
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="GroupId">Document Group Id</param>
        /// <param name="InviteId">Document Group Invite Id</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Returns the Group Invite</returns>
        public static dynamic GetInvite(string AccessToken, string GroupId, string InviteId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/documentgroup/" + GroupId + "/groupinvite/" + InviteId, Method.GET)
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
        /// Cancels a group invite.  All documents will be unshared with invitees.  Any signatures that occurred before canceling will remain on the documents.
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="GroupId">Document Group Invite Id</param>
        /// <param name="InviteId">Document Group Invite Id</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Returns a Success Message</returns>
        public static dynamic CancelInvite(string AccessToken, string GroupId, string InviteId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/documentgroup/" + GroupId + "/groupinvite/" + InviteId + "/cancelinvite", Method.POST)
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
        /// Returns back all pending invite information as well as invites already signed for a group invite for the user that makes the call.  If the user making the call is the document owner, it will return pending invites for that user and ALL actions that have already been fulfilled.
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="GroupId">Document Group Id</param>
        /// <param name="InviteId">Document Group Invite Id</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Returns all pending invite information.</returns>
        public static dynamic GetPendingInvites(string AccessToken, string GroupId, string InviteId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/documentgroup/" + GroupId + "/groupinvite/" + InviteId + "/pendinginvites", Method.GET)
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
        /// Resends invite emails to those with pending invites for the group invite.
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="GroupId">Document Group Id</param>
        /// <param name="InviteId">Document Group Invite Id</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Returns a Success Message</returns>
        public static dynamic ResendInvites(string AccessToken, string GroupId, string InviteId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/documentgroup/" + GroupId + "/groupinvite/" + InviteId + "/resendinvites", Method.POST)
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
        /// Used to wither replace inviters for a particular step or update the invite attributes for a user at a particular step.
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="GroupId">Documet Group Id</param>
        /// <param name="InviteId">Document Group Invite Id</param>
        /// <param name="StepId">Document Group Invite Step Id</param>
        /// <param name="DataObj">Data Object conatining new invite step parameters.</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Returns a Success Message</returns>
        public static dynamic ReplaceInviters(string AccessToken, string GroupId, string InviteId, string StepId, dynamic DataObj, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/documentgroup/" + GroupId + "/groupinvite/" + InviteId + "/step/" + StepId, Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

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
        /// Upload Document(s) and Send a Document Group Invite
        /// Accepted File Types: .doc, .docx, .pdf, .png
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="Files">Local Path to the File(s)</param>
        /// <param name="DataObj">Document Group Body</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>Document Id(s), Document Group Id(s), Step Id(s), Invite Id</returns>
        public static dynamic UploadInvite(string AccessToken, string[] Files, string DataObj, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);
            //client.BaseUrl = new Uri("https://hookb.in");

            // /documentgroup/uploadinvite
            // /ZY385BVN
            var request = new RestRequest("/documentgroup/uploadinvite", Method.POST)
                //var request = new RestRequest("/ZY385BVN", Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

            request.Timeout = 300000;

            //append files to request
            for (var i = 0; i < Files.Length; i++)
            {
                request.AddFile("file"+(i+1), Path.GetFullPath(Files[i]));
            }

            var jss = new JavaScriptSerializer();
            request.RequestFormat = DataFormat.Json;
            request.AddParameter("data", DataObj);
             
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
