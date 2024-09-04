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
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;

using Rock.Attribute;
using Rock.Model;
using Rock.Utility;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends email communication using Mailgun's HTTP API
    /// </summary>
    /// <seealso cref="Rock.Communication.TransportComponent" />
    [Description( "Sends a communication through Mailgun's HTTP API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Mailgun HTTP" )]

    #region Attributes

    [TextField( "Base URL",
        Key = AttributeKey.BaseURL,
        Description = "The API URL provided by Mailgun, keep the default in most cases.",
        DefaultValue = "https://api.mailgun.net/v3",
        Order = 0,
        IsRequired = true )]

    [TextField( "Resource",
        Key = AttributeKey.Resource,
        Description = "The URL part provided by Mailgun, keep the default in most cases.",
        DefaultValue = "{domain}/messages",
        Order = 1,
        IsRequired = true )]

    [TextField( "Domain",
        Key = AttributeKey.Domain,
        Description = "A comma-delimited list of email domains (e.g., rocksolidchurchdemo.com, rockdemo.com) authorized for sending emails. When the 'From' email address domain matches one of these domains, it will be utilized as the Mailgun \"domain\" in the REST API call; otherwise the first value will be used. These should match the sending domain(s) as listed in your Mailgun account.",
        DefaultValue = "",
        Order = 2,
        IsRequired = true )]

    [TextField( "API Key",
        Key = AttributeKey.APIKey,
        Description = @"The API Key provided by Mailgun. Newly-created Mailgun accounts should use one of the account-wide ""Mailgun API keys"" or domain-specific ""Sending API keys"" for this value. Preexisting Mailgun accounts might refer to this value as the ""Private API key"".",
        DefaultValue = "",
        Order = 3,
        IsRequired = true )]

    [TextField( "HTTP Webhook Signing Key",
        Key = AttributeKey.HTTPWebhookSigningKey,
        Description = "The HTTP Webhook Signing Key provided by Mailgun. Newly-created Mailgun accounts will have separate API and Webhook keys.",
        DefaultValue = "",
        Order = 4,
        IsRequired = true )]

    [BooleanField( "Track Opens",
        Key = AttributeKey.TrackOpens,
        Description = "Allow Mailgun to track opens, clicks, and unsubscribes.",
        DefaultBooleanValue = true,
        Order = 5,
        IsRequired = false )]

    [BooleanField( "Replace Unsafe Sender",
        Key = AttributeKey.ReplaceUnsafeSender,
        Description = @"Defaults to ""Yes"". If set to ""No"" Mailgun will allow relaying email ""on behalf of"" regardless of the sender's domain. The safe sender list will still be used for adding a ""Sender"" header.",
        DefaultBooleanValue = true,
        Order = 6,
        IsRequired = false )]

    [IntegerField( "Concurrent Send Workers",
        Key = AttributeKey.MaxParallelization,
        Description = "The maximum number of emails that will be sent concurrently.",
        DefaultIntegerValue = 10,
        Order = 7,
        IsRequired = false )]

    #endregion Attributes

    [Rock.SystemGuid.EntityTypeGuid( "35E39CA7-9383-421C-BBFA-0A6CC7AF1BAC")]
    public class MailgunHttp : EmailTransportComponent, IAsyncTransport
    {
        #region Keys

        private static class AttributeKey
        {
            public const string BaseURL = "BaseURL";
            public const string Resource = "Resource";
            public const string Domain = "Domain";
            public const string APIKey = "APIKey";
            public const string HTTPWebhookSigningKey = "HTTPWebhookSigningKey";
            public const string TrackOpens = "TrackOpens";
            public const string ReplaceUnsafeSender = "ReplaceUnsafeSender";
            public const string MaxParallelization = "MaxParallelization";
        }

        #endregion Keys

        /// <summary>
        /// Gets the response returned from the Mailgun API REST call.
        /// </summary>
        /// <value>
        /// The response.
        /// </value>
        public IRestResponse Response { get; set; }

        /// <summary>
        /// Gets a value indicating whether transport has ability to track recipients opening the communication.
        /// Mailgun automatically trackes opens, clicks, and unsubscribes. Use this to override domain setting.
        /// </summary>
        /// <value>
        /// <c>true</c> if transport can track opens; otherwise, <c>false</c>.
        /// </value>
        public override bool CanTrackOpens
        {
            get { return GetAttributeValue( AttributeKey.TrackOpens ).AsBoolean( true ); }
        }

        /// <summary>
        /// Gets the maximum parallelization.
        /// </summary>
        /// <value>
        /// The maximum parallelization.
        /// </value>
        public int MaxParallelization
        {
            get
            {
                return GetAttributeValue( AttributeKey.MaxParallelization ).AsIntegerOrNull() ?? 10;
            }
        }

        /// <summary>
        /// Gets the HTTP Status description returned from Mailgun's API
        /// </summary>
        /// <value>
        /// The status note.
        /// </value>
        public string StatusNote
        {
            get
            {
                return Response.StatusDescription;
            }
        }

        /// <summary>
        /// Gets a value indicating whether transport has ability to track recipients opening the communication.
        /// Mailgun automatically trackes opens, clicks, and unsubscribes. Use this to override domain setting.
        /// </summary>
        /// <value>
        /// <c>true</c> if transport can track opens; otherwise, <c>false</c>.
        /// </value>
        protected override EmailSendResponse SendEmail( RockEmailMessage rockEmailMessage )
        {
            return AsyncHelpers.RunSync( () => SendEmailAsync( rockEmailMessage ) );
        }

        /// <summary>
        /// Sends the email asynchronous.
        /// </summary>
        /// <param name="rockEmailMessage">The rock email message.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        /// <remarks>
        /// This class will call this method and pass the post processed data in a rock email message which
        /// can then be used to send the implementation specific message.
        /// </remarks>
        protected override async Task<EmailSendResponse> SendEmailAsync( RockEmailMessage rockEmailMessage )
        {
            var restRequest = GetRestRequestFromRockEmailMessage( rockEmailMessage );

            var restClient = new RestClient
            {
                BaseUrl = new Uri( GetAttributeValue( AttributeKey.BaseURL ) ),
                Authenticator = new HttpBasicAuthenticator( "api", GetAttributeValue( AttributeKey.APIKey ) )
            };

            var retriableStatusCode = new List<HttpStatusCode>()
            {
                HttpStatusCode.InternalServerError,
                HttpStatusCode.BadGateway,
                HttpStatusCode.ServiceUnavailable,
                HttpStatusCode.GatewayTimeout,
                (HttpStatusCode) 429
            };

            var methodRetry = new MethodRetry();

            // Call the API and get the response
            Response = await methodRetry.ExecuteAsync( () => restClient.ExecuteTaskAsync( restRequest ), ( response ) => !retriableStatusCode.Contains( response.StatusCode ) ).ConfigureAwait( false );
            if ( Response.StatusCode != HttpStatusCode.OK )
            {
                throw new Exception( Response.ErrorMessage ?? Response.StatusDescription );
            }

            return new EmailSendResponse
            {
                Status = Response.StatusCode == HttpStatusCode.OK ? CommunicationRecipientStatus.Delivered : CommunicationRecipientStatus.Failed,
                StatusNote = $"HTTP Status Code: {Response.StatusCode} \r\n Status Description: {Response.StatusDescription}"
            };
        }

        /// <summary>
        /// Checks the safe sender.
        /// </summary>
        /// <param name="toEmailAddresses">To email addresses.</param>
        /// <param name="fromEmail">From email.</param>
        /// <param name="organizationEmail">The organization email.</param>
        /// <returns></returns>
        protected override SafeSenderResult CheckSafeSender( List<string> toEmailAddresses, MailAddress fromEmail, string organizationEmail )
        {
            if ( GetAttributeValue( AttributeKey.ReplaceUnsafeSender ).AsBoolean( true ) )
            {
                return base.CheckSafeSender( toEmailAddresses, fromEmail, organizationEmail );
            }

            return new SafeSenderResult();
        }

        private void AddAdditionalHeaders( RestRequest restRequest, Dictionary<string, string> metadata, Dictionary<string, string> headers )
        {
            // Add tracking settings
            restRequest.AddParameter( "o:tracking", CanTrackOpens.ToYesNo() );
            restRequest.AddParameter( "o:tracking-opens", CanTrackOpens.ToYesNo() );
            restRequest.AddParameter( "o:tracking-clicks", CanTrackOpens.ToYesNo() );

            // Add additional JSON info
            if ( metadata != null )
            {
                var variables = new List<string>();
                foreach ( var param in metadata )
                {
                    variables.Add( string.Format( "\"{0}\":\"{1}\"", param.Key, param.Value ) );
                }

                restRequest.AddParameter( "v:X-Mailgun-Variables", string.Format( @"{{{0}}}", variables.AsDelimited( "," ) ) );
            }

            // Add headers
            if ( headers != null )
            {
                foreach (var header in headers )
                {
                    restRequest.AddParameter( $"h:{header.Key}", header.Value );
                }
            }
        }

        private RestRequest GetRestRequestFromRockEmailMessage( RockEmailMessage rockEmailMessage )
        {
            var restRequest = new RestRequest( GetAttributeValue( AttributeKey.Resource ), Method.POST );

            // To
            rockEmailMessage.GetRecipients().ForEach( r => restRequest.AddParameter( "to", new MailAddress( r.To, r.Name ).ToString() ) );

            // Reply To
            if ( rockEmailMessage.ReplyToEmail.IsNotNullOrWhiteSpace() )
            {
                restRequest.AddParameter( "h:Reply-To", rockEmailMessage.ReplyToEmail, ParameterType.GetOrPost );
            }

            var fromEmailAddress = new MailAddress( rockEmailMessage.FromEmail, rockEmailMessage.FromName );
            restRequest.AddParameter( "from", fromEmailAddress.ToString() );

            var safeSenderDomains = GetSafeDomains();
            var fromDomain = GetEmailDomain( rockEmailMessage.FromEmail );
            var configuredDomains = GetAttributeValue( "Domain" ).SplitDelimitedValues();

            // Now find a matching domain or use the first one in the list if no match was found.
            var matchingDomain = configuredDomains.FirstOrDefault( d => string.Equals( d, fromDomain, StringComparison.OrdinalIgnoreCase ) );
            if ( matchingDomain is null )
            {
                matchingDomain = configuredDomains.FirstOrDefault();
            } 

            restRequest.AddParameter( "domain", matchingDomain, ParameterType.UrlSegment );

            if ( safeSenderDomains.Contains( fromDomain ) )
            {
                restRequest.AddParameter( "h:Sender", fromEmailAddress.ToString() );
            }

            // CC
            rockEmailMessage
                .CCEmails
                .Where( e => e != string.Empty )
                .ToList()
                .ForEach( e => restRequest.AddParameter( "cc", e ) );

            // BCC
            rockEmailMessage
                .BCCEmails
                .Where( e => e != string.Empty )
                .ToList()
                .ForEach( e => restRequest.AddParameter( "bcc", e ) );

            // Subject
            restRequest.AddParameter( "subject", rockEmailMessage.Subject );

            // Body (plain text)
            if ( rockEmailMessage.PlainTextMessage.IsNotNullOrWhiteSpace() )
            {
                AlternateView plainTextView = AlternateView.CreateAlternateViewFromString( rockEmailMessage.PlainTextMessage, new ContentType( MediaTypeNames.Text.Plain ) );
                restRequest.AddParameter( "text", plainTextView );
            }

            // Body (html)
            restRequest.AddParameter( "html", rockEmailMessage.Message );

            // Communication record for tracking opens & clicks, and headers.
            AddAdditionalHeaders( restRequest, rockEmailMessage.MessageMetaData, rockEmailMessage.EmailHeaders );

            // Attachments
            foreach ( var attachment in rockEmailMessage.Attachments )
            {
                MemoryStream ms = new MemoryStream();
                attachment.ContentStream.CopyTo( ms );
                restRequest.AddFile( "attachment", ms.ToArray(), attachment.FileName );
            }

            return restRequest;
        }
    }
}
