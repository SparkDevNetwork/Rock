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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Model;
using Rock.Utility;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Used to send communication through SendGrid's HTTP API.
    /// </summary>
    /// <seealso cref="Rock.Communication.EmailTransportComponent" />
    [Description( "Sends a communication through SendGrid's HTTP API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "SendGrid HTTP" )]

    [TextField( "Base URL",
        Description = "The API URL provided by SendGrid, keep the default in most cases.",
        IsRequired = true,
        DefaultValue = @"https://api.sendgrid.com",
        Order = 0,
        Key = AttributeKey.BaseUrl )]
    [TextField( "API Key",
        Description = "The API Key provided by SendGrid.",
        IsRequired = true,
        Order = 3,
        Key = AttributeKey.ApiKey )]
    [BooleanField( "Track Opens",
        Description = "Allow SendGrid to track opens, clicks, and unsubscribes.",
        DefaultValue = "true",
        Order = 4,
        Key = AttributeKey.TrackOpens )]
    [IntegerField( "Concurrent Send Workers",
        IsRequired = false,
        DefaultIntegerValue = 10,
        Order = 5,
        Key = AttributeKey.MaxParallelization )]
    [Rock.SystemGuid.EntityTypeGuid( "BF3123A0-E5D1-42E0-9B95-C077DCD3C8BF")]
    public class SendGridHttp : EmailTransportComponent, IAsyncTransport
    {
        /// <summary>
        /// Class for storing attribute keys.
        /// </summary>
        public class AttributeKey
        {
            /// <summary>
            /// The track opens
            /// </summary>
            public const string TrackOpens = "TrackOpens";

            /// <summary>
            /// The API key
            /// </summary>
            public const string ApiKey = "APIKey";

            /// <summary>
            /// The base URL
            /// </summary>
            public const string BaseUrl = "BaseURL";

            /// <summary>
            /// The maximum parallelization
            /// </summary>
            public const string MaxParallelization = "MaxParallelization";
        }

        /// <summary>
        /// Gets the maximum parallelization.
        /// </summary>
        /// <value>
        /// The maximum parallelization.
        /// </value>
        public virtual int MaxParallelization
        {
            get => GetAttributeValue( AttributeKey.MaxParallelization ).AsIntegerOrNull() ?? 10;
        }

        /// <summary>
        /// Gets a value indicating whether transport has ability to track recipients opening the communication.
        /// Mailgun automatically trackes opens, clicks, and unsubscribes. Use this to override domain setting.
        /// </summary>
        /// <value>
        /// <c>true</c> if transport can track opens; otherwise, <c>false</c>.
        /// </value>
        public override bool CanTrackOpens
        {
            get => GetAttributeValue( AttributeKey.TrackOpens ).AsBoolean( true );
        }

        /// <summary>
        /// Send the implementation specific email. This class will call this method and pass the post processed data in a  rock email message which
        /// can then be used to send the implementation specific message.
        /// </summary>
        /// <param name="rockEmailMessage">The rock email message.</param>
        /// <returns></returns>
        protected override EmailSendResponse SendEmail( RockEmailMessage rockEmailMessage )
        {
            return AsyncHelpers.RunSync( () => SendEmailAsync( rockEmailMessage ) );
        }

        /// <summary>
        /// Sends the email asynchronous.
        /// </summary>
        /// <param name="rockEmailMessage">The rock email message.</param>
        /// <returns></returns>
        protected override async Task<EmailSendResponse> SendEmailAsync( RockEmailMessage rockEmailMessage )
        {
            var client = new SendGridClient( GetAttributeValue( AttributeKey.ApiKey ), host: GetAttributeValue( AttributeKey.BaseUrl ) );
            var sendGridMessage = GetSendGridMessageFromRockEmailMessage( rockEmailMessage );

            // Send it
            var retriableStatusCode = new List<HttpStatusCode>()
            {
                HttpStatusCode.InternalServerError,
                HttpStatusCode.BadGateway,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.GatewayTimeout,
                (HttpStatusCode) 429
            };

            var sendWithRetry = new MethodRetry();
            var response = await sendWithRetry.ExecuteAsync<Response>(
                async () => await client.SendEmailAsync( sendGridMessage ).ConfigureAwait( false ),
                ( sendGridResponse ) => !retriableStatusCode.Contains( sendGridResponse.StatusCode ) )
                .ConfigureAwait( false );

            return new EmailSendResponse
            {
                Status = response.StatusCode == HttpStatusCode.Accepted ? CommunicationRecipientStatus.Delivered : CommunicationRecipientStatus.Failed,
                StatusNote = $"HTTP Status Code: {response.StatusCode} \r\n Response Body: {await response.Body.ReadAsStringAsync().ConfigureAwait( false )}"
            };
        }

        private SendGridMessage GetSendGridMessageFromRockEmailMessage( RockEmailMessage rockEmailMessage )
        {
            var sendGridMessage = new SendGridMessage();

            /*
                2021-04-30 MSB

                The SendGrid API requires email addresses to be unique between the to, cc and bcc, and will return a bad request error
                if email addresses are duplicated between any of the three. The below code has been modified to make sure
                that the To email addresses don't exist in the CC list, and that the To and CC email addresses don't exist
                in the BCC list.

                Reason: SendGrid v3 API behavior
            */
            // To
            var toEmail = rockEmailMessage.GetRecipients();
            toEmail.ForEach( r => sendGridMessage.AddTo( r.To, r.Name ) );

            if ( rockEmailMessage.ReplyToEmail.IsNotNullOrWhiteSpace() )
            {
                sendGridMessage.ReplyTo = new EmailAddress( rockEmailMessage.ReplyToEmail );
            }

            sendGridMessage.From = new EmailAddress( rockEmailMessage.FromEmail, rockEmailMessage.FromName );

            // CC
            var ccEmailAddresses = rockEmailMessage
                .CCEmails
                .Where( cc => cc != string.Empty )
                .Where( cc => !toEmail.Any( te => te.To == cc ) )
                .Select( cc => new EmailAddress { Email = cc } )
                .ToList();

            if ( ccEmailAddresses.Count > 0 )
            {
                sendGridMessage.AddCcs( ccEmailAddresses );
            }

            // BCC
            var bccEmailAddresses = rockEmailMessage
                .BCCEmails
                .Where( bcc => bcc != string.Empty )
                .Where( bcc => !toEmail.Any( te => te.To == bcc ) )
                .Where( bcc => !ccEmailAddresses.Any( te => te.Email == bcc ) )
                .Select( bcc => new EmailAddress { Email = bcc } )
                .ToList();

            if ( bccEmailAddresses.Count > 0 )
            {
                sendGridMessage.AddBccs( bccEmailAddresses );
            }

            // Subject
            sendGridMessage.Subject = rockEmailMessage.Subject;

            // Body (plain text)
            sendGridMessage.PlainTextContent = rockEmailMessage.PlainTextMessage;

            // Body (html)
            sendGridMessage.HtmlContent = rockEmailMessage.Message;

            // Communication record for tracking opens & clicks
            if ( rockEmailMessage.MessageMetaData != null && rockEmailMessage.MessageMetaData.Count > 0 )
            {
                sendGridMessage.CustomArgs = rockEmailMessage.MessageMetaData;
            }

            if ( CanTrackOpens )
            {
                sendGridMessage.TrackingSettings = new TrackingSettings
                {
                    ClickTracking = new ClickTracking { Enable = true },
                    OpenTracking = new OpenTracking { Enable = true }
                };
            }

            // Headers
            if ( rockEmailMessage.EmailHeaders?.Any() == true )
            {
                sendGridMessage.AddHeaders( rockEmailMessage.EmailHeaders );
            }

            // Attachments
            if ( rockEmailMessage.Attachments.Any() )
            {
                foreach ( var attachment in rockEmailMessage.Attachments )
                {
                    if ( attachment != null )
                    {
                        MemoryStream ms = new MemoryStream();
                        attachment.ContentStream.CopyTo( ms );
                        sendGridMessage.AddAttachment( attachment.FileName, Convert.ToBase64String( ms.ToArray() ) );
                    }
                }
            }

            return sendGridMessage;
        }
    }
}
