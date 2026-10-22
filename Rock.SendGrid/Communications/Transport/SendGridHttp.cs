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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Model;
using Rock.SendGrid.Webhook;
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
    [TextField( "Event Webhook Verification Key",
        Description = "The Verification Key from SendGrid (Settings > Mail Settings > Event Webhook, with Signed Event Webhook turned on). When set, Rock rejects event webhook requests that are not signed with this key.",
        IsRequired = false,
        Order = 6,
        Key = AttributeKey.EventWebhookVerificationKey )]
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

            /// <summary>
            /// The SendGrid Signed Event Webhook verification key (public key).
            /// </summary>
            public const string EventWebhookVerificationKey = "EventWebhookVerificationKey";
        }

        /// <summary>
        /// Set to 1 once the missing verification key has been logged, so it
        /// is logged only once per application start.
        /// </summary>
        private static int _hasLoggedMissingVerificationKey = 0;

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
        /// Determines whether a request to the SendGrid event webhook may be
        /// processed. The SendGrid HTTP transport must be active and, when an
        /// Event Webhook Verification Key is set, the request must carry a
        /// valid SendGrid signature of the raw body.
        /// </summary>
        /// <param name="body">The raw request body bytes, exactly as received.</param>
        /// <param name="signature">The value of the X-Twilio-Email-Event-Webhook-Signature header.</param>
        /// <param name="timestamp">The value of the X-Twilio-Email-Event-Webhook-Timestamp header.</param>
        /// <param name="isSignatureVerified"><c>true</c> when the request was allowed because its SendGrid signature was verified; <c>false</c> when it was not checked (no verification key) or the request was not allowed.</param>
        /// <param name="errorMessage">The message to return to the caller when the request is not allowed.</param>
        /// <returns><c>true</c> if the request may be processed; otherwise <c>false</c>.</returns>
        [RockInternal( "17.12", true )]
        public static bool IsEventWebhookRequestAllowed( byte[] body, string signature, string timestamp, out bool isSignatureVerified, out string errorMessage )
        {
            /*
                10/21/2026 - MSE

                The event webhook is anonymous and its events can deactivate email
                addresses, change recipient status and move workflows. Only accept
                events when this transport is active (sites that do not use SendGrid
                get nothing from SendGrid), and check SendGrid's signature when the
                administrator has entered the verification key. A blank key is still
                allowed for now so existing SendGrid sites keep working; a later
                release will reject requests when the key is blank.

                Reason: The SendGrid event webhook accepted unsigned events.
            */
            isSignatureVerified = false;

            var transport = TransportContainer.GetComponent( typeof( SendGridHttp ).FullName ) as SendGridHttp;
            if ( transport == null || !transport.IsActive )
            {
                errorMessage = "SendGrid is not an active communication transport.";
                return false;
            }

            var verificationKey = transport.GetAttributeValue( AttributeKey.EventWebhookVerificationKey );
            if ( verificationKey.IsNullOrWhiteSpace() )
            {
                LogMissingVerificationKeyOnce();
                errorMessage = string.Empty;
                return true;
            }

            if ( !SendGridEventWebhookSignature.IsValid( verificationKey, body, signature, timestamp ) )
            {
                errorMessage = "Invalid request signature.";
                return false;
            }

            isSignatureVerified = true;
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>
        /// Logs that the Event Webhook Verification Key is not set, only once
        /// per application start so busy webhooks do not flood the exception log.
        /// </summary>
        private static void LogMissingVerificationKeyOnce()
        {
            if ( System.Threading.Interlocked.CompareExchange( ref _hasLoggedMissingVerificationKey, 1, 0 ) != 0 )
            {
                return;
            }

            ExceptionLogService.LogException( "The SendGrid HTTP transport has no Event Webhook Verification Key, so SendGrid event webhook requests are not verified. Set it within Admin Tools > Communications > Communication Transports > SendGrid HTTP." );
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
                /*
                    2025-01-21 KBH

                    GitHub Issue: https://github.com/SparkDevNetwork/Rock/issues/6149

                    The EmailAddress constructor is no longer able to parse the result from rockEmailMessage.ReplyToEmail
                    correctly (i.e. "Admin" <admin@organization.com>). The new approach uses a regular expression to extract
                    both the name and email address, allowing for the proper construction of the EmailAddress object and avoiding
                    issues with invalid or improperly formatted ReplyTo fields.

                    Reason: SendGrid is no longer able to parse "Admin" <admin@organization.com> correctly.
                 */
                var mailAddressRegex = new Regex( @"^""(?<name>[^""]+)""\s*<(?<email>[\w\.-]+@[\w\.-]+\.\w+)>$" );
                var match = mailAddressRegex.Match( rockEmailMessage.ReplyToEmail );

                if ( match.Success )
                {
                    var name = match.Groups["name"].Value;
                    var email = match.Groups["email"].Value;

                    sendGridMessage.ReplyTo = new EmailAddress( email, name );
                }
                else
                {
                    sendGridMessage.ReplyTo = new EmailAddress( rockEmailMessage.ReplyToEmail );
                }
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
