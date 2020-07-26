// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using Newtonsoft.Json;
using UAParser;

namespace Rock.SendGrid.Webhook
{
    /// <summary>
    /// A class to represent the events sent by send grid to the webhook.
    /// </summary>
    [JsonObject( IsReference = false )]
    public class SendGridEvent
    {
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [JsonProperty( PropertyName = "email" )]
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the timestamp.
        /// </summary>
        /// <value>
        /// The timestamp.
        /// </value>
        [JsonProperty( PropertyName = "timestamp" )]
        public int Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the type of the event.
        /// </summary>
        /// <value>
        /// The type of the event.
        /// </value>
        [JsonProperty( PropertyName = "event" )]
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the SMTP identifier.
        /// </summary>
        /// <value>
        /// The SMTP identifier.
        /// </value>
        [JsonProperty( PropertyName = "smtp-id" )]
        public string SmtpId { get; set; }

        /// <summary>
        /// Gets or sets the user agent.
        /// </summary>
        /// <value>
        /// The user agent.
        /// </value>
        [JsonProperty( PropertyName = "useragent" )]
        public string UserAgent { get; set; }

        /// <summary>
        /// Gets or sets the ip address.
        /// </summary>
        /// <value>
        /// The ip address.
        /// </value>
        [JsonProperty( PropertyName = "ip" )]
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the send grid event identifier.
        /// </summary>
        /// <value>
        /// The send grid event identifier.
        /// </value>
        [JsonProperty( PropertyName = "sg_event_id" )]
        public string SendGridEventId { get; set; }

        /// <summary>
        /// Gets or sets the send grid message identifier.
        /// </summary>
        /// <value>
        /// The send grid message identifier.
        /// </value>
        [JsonProperty( PropertyName = "sg_message_id" )]
        public string SendGridMessageId { get; set; }

        /// <summary>
        /// Gets or sets the event type reason.
        /// </summary>
        /// <value>
        /// The event type reason.
        /// </value>
        [JsonProperty( PropertyName = "reason" )]
        public string EventTypeReason { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [JsonProperty( PropertyName = "status" )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the server response.
        /// </summary>
        /// <value>
        /// The server response.
        /// </value>
        [JsonProperty( PropertyName = "response" )]
        public string ServerResponse { get; set; }

        /// <summary>
        /// Gets or sets the TLS.
        /// </summary>
        /// <value>
        /// The TLS.
        /// </value>
        [JsonProperty( PropertyName = "tls" )]
        public string Tls { get; set; }

        /// <summary>
        /// Gets or sets the URL.
        /// </summary>
        /// <value>
        /// The URL.
        /// </value>
        [JsonProperty( PropertyName = "url" )]
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the URL offset.
        /// </summary>
        /// <value>
        /// The URL offset.
        /// </value>
        [JsonProperty( PropertyName = "urloffset" )]
        public int UrlOffset { get; set; }

        /// <summary>
        /// Gets or sets the delivery attempt count.
        /// </summary>
        /// <value>
        /// The delivery attempt count.
        /// </value>
        [JsonProperty( PropertyName = "attempt" )]
        public string DeliveryAttemptCount { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [JsonProperty( PropertyName = "category" )]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the type of the bounce.
        /// </summary>
        /// <value>
        /// The type of the bounce.
        /// </value>
        [JsonProperty( PropertyName = "type" )]
        public string BounceType { get; set; }

        /// <summary>
        /// Gets or sets the workflow action unique identifier.
        /// </summary>
        /// <value>
        /// The workflow action unique identifier.
        /// </value>
        [JsonProperty( PropertyName = "workflow_action_guid" )]
        public string WorkflowActionGuid { get; set; }

        /// <summary>
        /// Gets or sets the communication recipient unique identifier.
        /// </summary>
        /// <value>
        /// The communication recipient unique identifier.
        /// </value>
        [JsonProperty( PropertyName = "communication_recipient_guid" )]
        public string CommunicationRecipientGuid { get; set; }

        /// <summary>
        /// Gets the client os.
        /// </summary>
        /// <value>
        /// The client os.
        /// </value>
        public string ClientOs
        {
            get
            {
                var clientInfo = GetClientInfo();
                return clientInfo?.OS.Family ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the client browser.
        /// </summary>
        /// <value>
        /// The client browser.
        /// </value>
        public string ClientBrowser
        {
            get
            {
                var clientInfo = GetClientInfo();
                return clientInfo?.UA.Family ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the type of the client device.
        /// </summary>
        /// <value>
        /// The type of the client device.
        /// </value>
        public string ClientDeviceType
        {
            get
            {
                var clientInfo = GetClientInfo();
                return clientInfo?.Device.Family ?? string.Empty;
            }
        }

        /// <summary>
        /// Gets the client device brand.
        /// </summary>
        /// <value>
        /// The client device brand.
        /// </value>
        public string ClientDeviceBrand
        {
            get
            {
                var clientInfo = GetClientInfo();
                return clientInfo?.Device.Brand ?? string.Empty;
            }
        }

        private ClientInfo _clientInfo = null;
        private ClientInfo GetClientInfo()
        {
            if ( _clientInfo == null && UserAgent.IsNotNullOrWhiteSpace() )
            {
                var parser = Parser.GetDefault();
                _clientInfo = parser.Parse( UserAgent );
            }
            return _clientInfo;
        }
    }
}
