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
//
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Rock.Logging;
using Rock.Model;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends a communication through SMTP protocol
    /// </summary>
    public abstract class SMTPComponent : EmailTransportComponent, IAsyncTransport
    {
        #region Properties

        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        /// <value>
        /// The server.
        /// </value>
        public virtual string Server
        {
            get
            {
                return GetAttributeValue( "Server" );
            }
        }

        /// <summary>
        /// Gets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        public virtual int Port
        {
            get
            {
                return GetAttributeValue( "Port" ).AsIntegerOrNull() ?? 25;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [use SSL].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use SSL]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool UseSSL
        {
            get
            {
                return GetAttributeValue( "UseSSL" ).AsBooleanOrNull() ?? false;
            }
        }

        /// <summary>
        /// Gets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        public virtual string Username
        {
            get
            {
                return GetAttributeValue( "Username" );
            }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        public virtual string Password
        {
            get
            {
                return GetAttributeValue( "Password" );
            }
        }

        /// <summary>
        /// Gets the recipient status note.
        /// </summary>
        /// <value>
        /// The status note.
        /// </value>
        public virtual string StatusNote { get => string.Empty; }

        /// <summary>
        /// Gets the maximum parallelization.
        /// </summary>
        /// <value>
        /// The maximum parallelization.
        /// </value>
        public virtual int MaxParallelization
        {
            get
            {
                return GetAttributeValue( "MaxParallelization" ).AsIntegerOrNull() ?? 1;
            }
        }

        #endregion

        /// <summary>
        /// Send the implementation specific email. This class will call this method and pass the post processed data in a  rock email message which
        /// can then be used to send the implementation specific message.
        /// </summary>
        /// <param name="rockEmailMessage">The rock email message.</param>
        /// <returns></returns>
        protected override EmailSendResponse SendEmail( RockEmailMessage rockEmailMessage )
        {
            var mailMessage = GetMailMessageFromRockEmailMessage( rockEmailMessage );
            var smtpClient = GetSmtpClient();
            smtpClient.Send( mailMessage );

            return new EmailSendResponse
            {
                Status = CommunicationRecipientStatus.Delivered,
                StatusNote = StatusNote
            };
        }

        /// <summary>
        /// Sends the email asynchronous.
        /// </summary>
        /// <param name="rockEmailMessage">The rock email message.</param>
        /// <returns></returns>
        protected override async Task<EmailSendResponse> SendEmailAsync( RockEmailMessage rockEmailMessage )
        {
            var mailMessage = GetMailMessageFromRockEmailMessage( rockEmailMessage );
            var smtpClient = GetSmtpClient();

            RockLogger.Log.Debug( RockLogDomains.Communications, "{0}: Starting to send {1} to {2}.", nameof( SendEmailAsync ), rockEmailMessage.Subject, rockEmailMessage.GetRecipients().FirstOrDefault()?.To );
            await smtpClient.SendMailAsync( mailMessage ).ConfigureAwait( false );

            return new EmailSendResponse
            {
                Status = CommunicationRecipientStatus.Delivered,
                StatusNote = StatusNote
            };
        }

        internal MailMessage GetMailMessageFromRockEmailMessage( RockEmailMessage rockEmailMessage )
        {
            var mailMessage = new MailMessage
            {
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };

            // From
            mailMessage.From = new MailAddress( rockEmailMessage.FromEmail, rockEmailMessage.FromName );

            // Reply to
            try
            {
                if ( rockEmailMessage.ReplyToEmail.IsNotNullOrWhiteSpace() )
                {
                    mailMessage.ReplyToList.Add( new MailAddress( rockEmailMessage.ReplyToEmail ) );
                }
            }
            catch
            {
                // intentionally left blank
            }

            // To
            var recipients = rockEmailMessage.GetRecipients().ToList();
            recipients.ForEach( r => mailMessage.To.Add( new MailAddress( r.To, r.Name ) ) );

            // cc
            rockEmailMessage
                .CCEmails
                .Where( e => e.IsNotNullOrWhiteSpace() )
                .ToList()
                .ForEach( e => mailMessage.CC.Add( new MailAddress( e ) ) );

            // bcc
            rockEmailMessage
                .BCCEmails
                .Where( e => e.IsNotNullOrWhiteSpace() )
                .ToList()
                .ForEach( e => mailMessage.Bcc.Add( new MailAddress( e ) ) );

            // Subject
            mailMessage.Subject = rockEmailMessage.Subject;

            // Plain text
            if ( rockEmailMessage.PlainTextMessage.IsNotNullOrWhiteSpace() )
            {
                var plainTextView = AlternateView.CreateAlternateViewFromString( rockEmailMessage.PlainTextMessage, new System.Net.Mime.ContentType( MediaTypeNames.Text.Plain ) );
                mailMessage.AlternateViews.Add( plainTextView );
            }

            // Body
            var htmlBody = rockEmailMessage.Message;

            if ( !string.IsNullOrWhiteSpace( htmlBody ) )
            {
                /*
                 * 2021-11-04 Ethan Drotning
                 * Do not check rockEmailMessage.CssInliningEnabled here. This is being taken care of in the parent abstract class EmailTransportComponent methods.
                 * SMTP, SendGrid, MailGun and other child component classes don't have to worry about it unless they override the methods without calling the parent.
                 */

                var htmlView = AlternateView.CreateAlternateViewFromString( htmlBody, new System.Net.Mime.ContentType( MediaTypeNames.Text.Html ) );
                mailMessage.AlternateViews.Add( htmlView );
            }

            if ( rockEmailMessage.Attachments.Any() )
            {
                foreach ( var attachment in rockEmailMessage.Attachments )
                {
                    if ( attachment != null )
                    {
                        mailMessage.Attachments.Add( new Attachment( attachment.ContentStream, attachment.FileName ) );
                    }
                }
            }

            // Headers
            AddAdditionalHeaders( mailMessage, rockEmailMessage.MessageMetaData );
            AddAdditionalHeaders( mailMessage, rockEmailMessage.EmailHeaders );

            return mailMessage;
        }

        /// <summary>
        /// Adds any additional headers.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="headers">The headers.</param>
        public virtual void AddAdditionalHeaders( MailMessage message, Dictionary<string, string> headers )
        {
            if ( headers != null )
            {
                foreach ( var header in headers )
                {
                    message.Headers.Add( header.Key, header.Value );
                }
            }
        }

        /// <summary>
        /// Creates an SmtpClient using this Server, Port and SSL settings.
        /// </summary>
        /// <returns></returns>
        private SmtpClient GetSmtpClient()
        {
            SmtpClient smtpClient = null;
            if ( smtpClient == null )
            {
                // Create SMTP Client
                smtpClient = new SmtpClient( Server, Port )
                {
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    EnableSsl = UseSSL
                };

                if ( !string.IsNullOrEmpty( Username ) )
                {
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new System.Net.NetworkCredential( Username, Password );
                }
            }

            return smtpClient;
        }
    }
}
