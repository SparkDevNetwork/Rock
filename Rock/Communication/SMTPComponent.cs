// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends a communication through SMTP protocol
    /// </summary>
    public abstract class SMTPComponent : TransportComponent
    {
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
        public virtual string StatusNote { get { return string.Empty; } }
            
        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public override void Send( Rock.Model.Communication communication )
        {
            var rockContext = new RockContext();

            // Requery the Communication object
            communication = new CommunicationService( rockContext ).Get( communication.Id );

            if ( communication != null &&
                communication.Status == Model.CommunicationStatus.Approved &&
                communication.Recipients.Where( r => r.Status == Model.CommunicationRecipientStatus.Pending ).Any() &&
                (!communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo(RockDateTime.Now) <= 0))
            {

                var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();

                string fromAddress = communication.GetChannelDataValue( "FromAddress" );
                string replyTo = communication.GetChannelDataValue( "ReplyTo" );

                // Check to make sure sending domain is a safe sender
                var safeDomains = globalAttributes.GetValue( "SafeSenderDomains" ).SplitDelimitedValues().ToList();
                var emailParts = fromAddress.Split( new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries );
                if (emailParts.Length != 2 || !safeDomains.Contains(emailParts[1],StringComparer.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(replyTo))
                    {
                        replyTo = fromAddress;
                    }
                    fromAddress = globalAttributes.GetValue("OrganizationEmail");
                }

                // From
                MailMessage message = new MailMessage();
                message.From = new MailAddress(
                    fromAddress,
                    communication.GetChannelDataValue( "FromName" ) );

                // Reply To
                if ( !string.IsNullOrWhiteSpace( replyTo ) )
                {
                    message.ReplyToList.Add( new MailAddress( replyTo ) );
                }

                // CC
                string cc = communication.GetChannelDataValue( "CC" );
                if ( !string.IsNullOrWhiteSpace( cc ) )
                {
                    foreach ( string ccRecipient in cc.SplitDelimitedValues() )
                    {
                        message.CC.Add( new MailAddress( ccRecipient ) );
                    }
                }

                // BCC
                string bcc = communication.GetChannelDataValue( "BCC" );
                if ( !string.IsNullOrWhiteSpace( bcc ) )
                {
                    foreach ( string bccRecipient in bcc.SplitDelimitedValues() )
                    {
                        message.Bcc.Add( new MailAddress( bccRecipient ) );
                    }
                }

                message.IsBodyHtml = true;
                message.Priority = MailPriority.Normal;

                var smtpClient = GetSmtpClient();

                // Add Attachments
                string attachmentIds = communication.GetChannelDataValue( "Attachments" );
                if ( !string.IsNullOrWhiteSpace( attachmentIds ) )
                {
                    var binaryFileService = new BinaryFileService( rockContext );

                    foreach(string idVal in attachmentIds.SplitDelimitedValues())
                    {
                        int binaryFileId = int.MinValue;
                        if (int.TryParse(idVal, out binaryFileId))
                        {
                            var binaryFile = binaryFileService.Get(binaryFileId);
                            if ( binaryFile != null )
                            {
                                Stream stream = new MemoryStream( binaryFile.Data.Content );
                                message.Attachments.Add( new Attachment( stream, binaryFile.FileName ) );
                            }
                        }
                    }
                }

                var recipientService = new CommunicationRecipientService( rockContext );

                var globalConfigValues = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );
                
                bool recipientFound = true;
                while ( recipientFound )
                {
                    var recipient = recipientService.Get( communication.Id, CommunicationRecipientStatus.Pending ).FirstOrDefault();
                    if ( recipient != null )
                    {
                        if ( string.IsNullOrWhiteSpace( recipient.Person.Email ) )
                        {
                            recipient.Status = CommunicationRecipientStatus.Failed;
                            recipient.StatusNote = "No Email Address";
                        }
                        else
                        {
                            message.To.Clear();
                            message.Headers.Clear();
                            message.AlternateViews.Clear();

                            message.To.Add( new MailAddress( recipient.Person.Email, recipient.Person.FullName ) );

                            // Create merge field dictionary
                            var mergeObjects = recipient.CommunicationMergeValues( globalConfigValues );

                            // Subject
                            message.Subject = communication.Subject.ResolveMergeFields( mergeObjects );

                            // Add any additional headers that specific SMTP provider needs
                            AddAdditionalHeaders( message, recipient );

                            // Add text view first as last view is usually treated as the preferred view by email readers (gmail)
                            string plainTextBody = Rock.Communication.Channel.Email.ProcessTextBody( communication, globalAttributes, mergeObjects );
                            if ( !string.IsNullOrWhiteSpace( plainTextBody ) )
                            {
                                AlternateView plainTextView = AlternateView.CreateAlternateViewFromString( plainTextBody, new ContentType( MediaTypeNames.Text.Plain ) );
                                message.AlternateViews.Add( plainTextView );
                            }

                            // Add Html view
                            string htmlBody = Rock.Communication.Channel.Email.ProcessHtmlBody( communication, globalAttributes, mergeObjects );
                            if ( !string.IsNullOrWhiteSpace( htmlBody ) )
                            {
                                AlternateView htmlView = AlternateView.CreateAlternateViewFromString( htmlBody, new ContentType( MediaTypeNames.Text.Html ) );
                                message.AlternateViews.Add( htmlView );
                            }

                            try
                            {
                                smtpClient.Send( message );
                                recipient.Status = CommunicationRecipientStatus.Delivered;

                                string statusNote = StatusNote;
                                if (!string.IsNullOrWhiteSpace(statusNote))
                                {
                                    recipient.StatusNote = statusNote;
                                }

                                recipient.TransportEntityTypeName = this.GetType().FullName;
                            }
                            catch ( Exception ex )
                            {
                                recipient.Status = CommunicationRecipientStatus.Failed;
                                recipient.StatusNote = "SMTP Exception: " + ex.Message;
                            }
                        }

                        rockContext.SaveChanges();
                    }
                    else
                    {
                        recipientFound = false;
                    }
                }
            }
        }

        /// <summary>
        /// Sends the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot"></param>
        /// <param name="themeRoot"></param>
        public override void Send( SystemEmail template, Dictionary<string, Dictionary<string, object>> recipients, string appRoot, string themeRoot )
        {
            var globalAttributes = GlobalAttributesCache.Read();

            string from = template.From;
            if (string.IsNullOrWhiteSpace(from))
            {
                from = globalAttributes.GetValue( "OrganizationEmail" );
            }

            string fromName = template.FromName;
            if ( string.IsNullOrWhiteSpace( fromName ) )
            {
                fromName = globalAttributes.GetValue( "OrganizationName" );
            }

            if ( !string.IsNullOrWhiteSpace( from ) )
            {
                MailMessage message = new MailMessage();
                if (string.IsNullOrWhiteSpace(fromName))
                {
                    message.From = new MailAddress( from );
                }
                else
                {
                    message.From = new MailAddress( from, fromName );
                }

                if ( !string.IsNullOrWhiteSpace( template.Cc ) )
                {
                    foreach ( string ccRecipient in template.Cc.SplitDelimitedValues() )
                    {
                        message.CC.Add( new MailAddress( ccRecipient ) );
                    }
                }

                if ( !string.IsNullOrWhiteSpace( template.Bcc ) )
                {
                    foreach ( string ccRecipient in template.Bcc.SplitDelimitedValues() )
                    {
                        message.CC.Add( new MailAddress( ccRecipient ) );
                    }
                }

                message.IsBodyHtml = true;
                message.Priority = MailPriority.Normal;

                var smtpClient = GetSmtpClient();

                var globalConfigValues = Rock.Web.Cache.GlobalAttributesCache.GetMergeFields( null );

                foreach ( KeyValuePair<string, Dictionary<string, object>> recipient in recipients )
                {
                    var mergeObjects = recipient.Value;
                    globalConfigValues.ToList().ForEach( g => mergeObjects[g.Key] = g.Value );

                    List<string> sendTo = SplitRecipient( template.To );
                    sendTo.Add( recipient.Key );
                    foreach ( string to in sendTo )
                    {
                        message.To.Clear();
                        message.To.Add( to );

                        string subject = template.Subject.ResolveMergeFields( mergeObjects );
                        string body = Regex.Replace( template.Body.ResolveMergeFields( mergeObjects ), @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty );

                        if (!string.IsNullOrWhiteSpace(themeRoot))
                        {
                            subject = subject.Replace( "~~/", themeRoot );
                            body = body.Replace( "~~/", themeRoot );
                        }

                        if (!string.IsNullOrWhiteSpace(appRoot))
                        {
                            subject = subject.Replace( "~/", appRoot );
                            body = body.Replace( "~/", appRoot );
                            body = body.Replace( @" src=""/", @" src=""" + appRoot );
                            body = body.Replace( @" href=""/", @" href=""" + appRoot );
                        }

                        message.Subject = subject;
                        message.Body = body;

                        smtpClient.Send( message );
                    }
                }
            }
        }

        /// <summary>
        /// Sends the specified channel data to the specified list of recipients.
        /// </summary>
        /// <param name="channelData">The channel data.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        public override void Send(Dictionary<string, string> channelData, List<string> recipients, string appRoot, string themeRoot)
        {
            try
            {
                var globalAttributes = GlobalAttributesCache.Read();

                string from = string.Empty;
                string fromName = string.Empty;
                channelData.TryGetValue( "From", out from );

                if ( string.IsNullOrWhiteSpace( from ) )
                {
                    from = globalAttributes.GetValue( "OrganizationEmail" );
                    fromName = globalAttributes.GetValue( "OrganizationName" );
                }

                if ( !string.IsNullOrWhiteSpace( from ) )
                {
                    MailMessage message = new MailMessage();

                    if ( string.IsNullOrWhiteSpace( fromName ) )
                    {
                        message.From = new MailAddress( from );
                    }
                    else
                    {
                        message.From = new MailAddress( from, fromName );
                    }

                    message.IsBodyHtml = true;
                    message.Priority = MailPriority.Normal;

                    var smtpClient = GetSmtpClient();

                    string subject = string.Empty;
                    channelData.TryGetValue( "Subject", out subject );

                    string body = string.Empty;
                    channelData.TryGetValue( "Body", out body );

                    message.To.Clear();
                    recipients.ForEach( r => message.To.Add( r ) );

                    if ( !string.IsNullOrWhiteSpace( themeRoot ) )
                    {
                        subject = subject.Replace( "~~/", themeRoot );
                        body = body.Replace( "~~/", themeRoot );
                    }

                    if ( !string.IsNullOrWhiteSpace( appRoot ) )
                    {
                        subject = subject.Replace( "~/", appRoot );
                        body = body.Replace( "~/", appRoot );
                        body = body.Replace( @" src=""/", @" src=""" + appRoot );
                        body = body.Replace( @" href=""/", @" href=""" + appRoot );
                    }

                    message.Subject = subject;
                    message.Body = body;

                    smtpClient.Send( message );
                }
            }

            catch (Exception ex)
            {
                ExceptionLogService.LogException( ex, null );
            }
        }

        /// <summary>
        /// Adds any additional headers.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="recipient">The recipient.</param>
        public virtual void AddAdditionalHeaders( MailMessage message, CommunicationRecipient recipient )
        {
        }
        
        private SmtpClient GetSmtpClient()
        {
            // Create SMTP Client
            SmtpClient smtpClient = new SmtpClient( Server, Port );
            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtpClient.EnableSsl = UseSSL;

            if ( !string.IsNullOrEmpty( Username ) )
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential( Username, Password );
            }

            return smtpClient;
        }

        private List<string> SplitRecipient( string recipients )
        {
            if ( String.IsNullOrWhiteSpace( recipients ) )
                return new List<string>();
            else
                return new List<string>( recipients.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ) );
        }

    }
}
