﻿// <copyright>
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

using RestSharp;
using RestSharp.Authenticators;

using Rock.Attribute;
using Rock.Model;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends email communication using Mailgun's HTTP API
    /// </summary>
    /// <seealso cref="Rock.Communication.TransportComponent" />
    [Description( "Sends a communication through Mailgun's HTTP API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Mailgun HTTP" )]

    [TextField( "Base URL", "The API URL provided by Mailgun, keep the default in most cases.", true, @"https://api.mailgun.net/v3", "", 0, "BaseURL" )]
    [TextField( "Resource", "The URL part provided by Mailgun, keep the default in most cases.", true, @"{domian}/messages", "", 1, "Resource" )]
    [TextField( "Domain", "The email domain (e.g. rocksolidchurchdemo.com).", true, "", "", 2, "Domain" )]
    [TextField( "API Key", "The Private API Key provided by Mailgun.", true, "", "", 3, "APIKey" )]
    [BooleanField( "Track Opens", "Allow Mailgun to track opens, clicks, and unsubscribes.", true, "", 4, "TrackOpens" )]
    [BooleanField( "Replace Unsafe Sender", "Defaults to \"Yes\".  If set to \"No\" Mailgun will allow relaying email \"on behalf of\" regardless of the sender's domain.  The safe sender list will still be used for adding a \"Sender\" header.", true, "", 5 )]
    public class MailgunHttp : EmailTransportComponent
    {
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
            get { return GetAttributeValue( "TrackOpens" ).AsBoolean( true ); }
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
            var restRequest = GetRestRequestFromRockEmailMessage( rockEmailMessage );

            var restClient = new RestClient
            {
                BaseUrl = new Uri( GetAttributeValue( "BaseURL" ) ),
                Authenticator = new HttpBasicAuthenticator( "api", GetAttributeValue( "APIKey" ) )
            };

            // Call the API and get the response
            Response = restClient.Execute( restRequest );

            return new EmailSendResponse
            {
                Status = Response.StatusCode == HttpStatusCode.OK ? CommunicationRecipientStatus.Delivered : CommunicationRecipientStatus.Failed,
                StatusNote = Response.StatusDescription
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
            if ( GetAttributeValue( "ReplaceUnsafeSender" ).AsBoolean( true ) )
            {
                return base.CheckSafeSender( toEmailAddresses, fromEmail, organizationEmail );
            }
            return new SafeSenderResult();
        }

        private void AddAdditionalHeaders( RestRequest restRequest, Dictionary<string, string> headers )
        {
            // Add tracking settings
            restRequest.AddParameter( "o:tracking", CanTrackOpens.ToYesNo() );
            restRequest.AddParameter( "o:tracking-opens", CanTrackOpens.ToYesNo() );
            restRequest.AddParameter( "o:tracking-clicks", CanTrackOpens.ToYesNo() );

            // Add additional JSON info
            if ( headers != null )
            {
                var variables = new List<string>();
                foreach ( var param in headers )
                {
                    variables.Add( string.Format( "\"{0}\":\"{1}\"", param.Key, param.Value ) );
                }

                restRequest.AddParameter( "v:X-Mailgun-Variables", string.Format( @"{{{0}}}", variables.AsDelimited( "," ) ) );
            }
        }

        private RestRequest GetRestRequestFromRockEmailMessage( RockEmailMessage rockEmailMessage )
        {
            var restRequest = new RestRequest( GetAttributeValue( "Resource" ), Method.POST );
            restRequest.AddParameter( "domian", GetAttributeValue( "Domain" ), ParameterType.UrlSegment );

            // To
            rockEmailMessage.GetRecipients().ForEach( r => restRequest.AddParameter( "to", new MailAddress( r.To, r.Name ).ToString() ) );

            // Reply To
            if ( rockEmailMessage.ReplyToEmail.IsNotNullOrWhiteSpace() )
            {
                var replyTo = new Parameter
                {
                    Name = "h:Reply-To",
                    Type = ParameterType.GetOrPost,
                    Value = rockEmailMessage.ReplyToEmail
                };

                restRequest.AddParameter( replyTo );
            }

            var fromEmailAddress = new MailAddress( rockEmailMessage.FromEmail, rockEmailMessage.FromName );
            restRequest.AddParameter( "from", fromEmailAddress.ToString() );

            var safeSenderDomains = GetSafeDomains();
            var fromDomain = GetEmailDomain( rockEmailMessage.FromEmail );
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

            // Communication record for tracking opens & clicks
            AddAdditionalHeaders( restRequest, rockEmailMessage.MessageMetaData );

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
