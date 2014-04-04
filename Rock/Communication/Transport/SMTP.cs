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
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends a communication through SMTP protocol
    /// </summary>
    [Description( "Sends a communication through SMTP protocol" )]
    [Export(typeof(TransportComponent))]
    [ExportMetadata("ComponentName", "SMTP")]
    [TextField("Server", "", true, "", "", 0)]
    [IntegerField("Port", "", false, 25, "", 1 )]
    [TextField("User Name", "", false, "", "", 2)]
    [TextField("Password", "", false, "", "", 3)]
    [BooleanField("Use SSL", "", false, "", 4)]
    public class SMTP : TransportComponent
    {
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
                // From
                MailMessage message = new MailMessage();
                message.From = new MailAddress(
                    communication.GetChannelDataValue( "FromAddress" ),
                    communication.GetChannelDataValue( "FromName" ) );

                // Reply To
                string replyTo = communication.GetChannelDataValue( "ReplyTo" );
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

                var globalAttributes = Rock.Web.Cache.GlobalAttributesCache.Read();
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
                            message.To.Add( new MailAddress( recipient.Person.Email, recipient.Person.FullName ) );

                            // Create merge field dictionary
                            var mergeObjects = MergeValues( globalConfigValues, recipient );

                            message.Subject = communication.Subject.ResolveMergeFields( mergeObjects );

                            string unsubscribeHtml = communication.GetChannelDataValue( "UnsubscribeHTML" );
                            string htmlBody = communication.GetChannelDataValue( "HtmlMessage" );
                            string plainTextBody = communication.GetChannelDataValue( "TextMessage" );

                            // If there is unsubscribe html (would have been added by channel PreSend), inject it
                            if ( !string.IsNullOrWhiteSpace( htmlBody ) && !string.IsNullOrWhiteSpace( unsubscribeHtml ) )
                            {
                                string newHtml = Regex.Replace( htmlBody, @"\{\{\s*UnsubscribeOption\s*\}\}", unsubscribeHtml );
                                if ( htmlBody != newHtml )
                                {
                                    // If the content changed, then the merge field was found and newHtml has the unsubscribe contents
                                    htmlBody = newHtml;
                                }
                                else
                                {
                                    // If it didn't change, the body did not contain merge field so add unsubscribe contents at end
                                    htmlBody += unsubscribeHtml;
                                }
                            }

                            // Add text view first as last view is usually treated as the the preferred view by email readers (gmail)
                            if ( !string.IsNullOrWhiteSpace( plainTextBody ) )
                            {
                                plainTextBody = plainTextBody.ResolveMergeFields( mergeObjects );
                                AlternateView plainTextView = AlternateView.CreateAlternateViewFromString( plainTextBody, new ContentType( MediaTypeNames.Text.Plain ) );
                                message.AlternateViews.Add( plainTextView );
                            }

                            if ( !string.IsNullOrWhiteSpace( htmlBody ) )
                            {
                                string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();
                                htmlBody = htmlBody.Replace( @" src=""/", @" src=""" + publicAppRoot );
                                htmlBody = htmlBody.Replace( @" href=""/", @" href=""" + publicAppRoot );
                                htmlBody = htmlBody.ResolveMergeFields( mergeObjects );
                                AlternateView htmlView = AlternateView.CreateAlternateViewFromString( htmlBody, new ContentType( MediaTypeNames.Text.Html ) );
                                message.AlternateViews.Add( htmlView );
                            }

                            try
                            {
                                smtpClient.Send( message );
                                recipient.Status = CommunicationRecipientStatus.Success;
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
                        string subject = template.Subject;
                        string body = template.Body;
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

                        message.To.Clear();
                        message.To.Add( to );
                        message.Subject = subject.ResolveMergeFields( mergeObjects );
                        message.Body = body.ResolveMergeFields( mergeObjects );
                        smtpClient.Send( message );
                    }
                }
            }
        }

        private SmtpClient GetSmtpClient()
        {
            // Create SMTP Client
            SmtpClient smtpClient = new SmtpClient( GetAttributeValue( "Server" ) );

            int port = int.MinValue;
            if ( int.TryParse( GetAttributeValue( "Port" ), out port ) )
            {
                smtpClient.Port = port;
            }

            smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;

            bool useSSL = false;
            smtpClient.EnableSsl = bool.TryParse( GetAttributeValue( "UseSSL" ), out useSSL ) && useSSL;

            string userName = GetAttributeValue( "UserName" );
            string password = GetAttributeValue( "Password" );
            if ( !string.IsNullOrEmpty( userName ) )
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential( userName, password );
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
