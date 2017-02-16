using Newtonsoft.Json;
using RestSharp;
using System;
using System.Net;
using System.Dynamic;
using System.Xml;

namespace SignNowSDK
{
    public class Webhook
    {
        /// <summary>
        /// Get a List of Current Webhook Subscriptions
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>List of Subscriptions</returns>
        public static dynamic List(string AccessToken, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/event_subscription", Method.GET)
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
        /// Create Webhook that will be Triggered when the Specified Event Takes Place
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="EventType">document.create, document.update, document.delete, invite.create, invite.update</param>
        /// <param name="CallbackUrl">The URL called when the even is triggered.</param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>ID, Created, Updated</returns>
        public static dynamic Create(string AccessToken, string EventType, string CallbackUrl, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/event_subscription", Method.POST)
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", "Bearer " + AccessToken);

            request.RequestFormat = DataFormat.Json;

            dynamic jsonObj = new ExpandoObject();
            jsonObj.@event = EventType;
            jsonObj.callback_url = CallbackUrl;

            request.AddBody(jsonObj);

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
        /// Deletes a Webhook
        /// </summary>
        /// <param name="AccessToken"></param>
        /// <param name="SubscriptionId"></param>
        /// <param name="ResultFormat">JSON, XML</param>
        /// <returns>{"status":"success"}</returns>
        public static dynamic Delete(string AccessToken, string SubscriptionId, string ResultFormat = "JSON")
        {
            var client = new RestClient();
            client.BaseUrl = new Uri(Config.ApiHost);

            var request = new RestRequest("/event_subscription/" + SubscriptionId, Method.DELETE)
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
