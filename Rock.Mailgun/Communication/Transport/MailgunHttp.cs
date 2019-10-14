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
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;

using RestSharp;
using RestSharp.Authenticators;

using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends email communication using Mailgun's HTTP API
    /// </summary>
    /// <seealso cref="Rock.Communication.TransportComponent" />
    [Description( "Sends a communication through Mailgun's HTTP API" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Mailgun HTTP" )]

    [TextField( "Base URL", "The API URL provided by Mailgun, keep the default in most cases", true, @"https://api.mailgun.net/v3", "", 0, "BaseURL" )]
    [TextField( "Resource", "The URL part provided by Mailgun, keep the default in most cases", true, @"{domian}/messages", "", 1, "Resource" )]
    [TextField( "Domain", "The email domain (e.g. rocksolidchurchdemo.com).", true, "", "", 2, "Domain" )]
    [TextField( "API Key", "The API Key provided by Mailgun (starts with \"key-\").", true, "", "", 3, "APIKey" )]
    [BooleanField( "Track Opens", "", true, "", 4, "TrackOpens" )]
    public class MailgunHttp : TransportComponent
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
        /// Sends the specified rock message.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier. Not used.</param>
        /// <param name="mediumAttributes">The medium attributes. Not used.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Send( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var emailMessage = rockMessage as RockEmailMessage;
            if ( emailMessage == null )
            {
                return !errorMessages.Any();
            }

            var globalAttributes = GlobalAttributesCache.Get();

            string fromAddress = emailMessage.FromEmail.IsNullOrWhiteSpace() ? globalAttributes.GetValue( "OrganizationEmail" ) : emailMessage.FromEmail;
            string fromName = emailMessage.FromName.IsNullOrWhiteSpace() ? globalAttributes.GetValue( "OrganizationName" ) : emailMessage.FromName;

            if ( fromAddress.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "A From address was not provided." );
                return false;
            }

            // Common Merge Field
            var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, rockMessage.CurrentPerson );
            foreach ( var mergeField in rockMessage.AdditionalMergeFields )
            {
                mergeFields.AddOrReplace( mergeField.Key, mergeField.Value );
            }

            // Resolve any possible merge fields in the from address
            fromAddress = fromAddress.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
            fromName = fromName.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );

            RestRequest restRequest = null;
            foreach ( var recipientData in rockMessage.GetRecipientData() )
            {
                try
                {
                    restRequest = new RestRequest( GetAttributeValue("Resource"), Method.POST );
                    restRequest.AddParameter( "domian", GetAttributeValue("Domain"), ParameterType.UrlSegment );

                    // Reply To
                    if ( emailMessage.ReplyToEmail.IsNotNullOrWhiteSpace() )
                    {
                        // Resolve any possible merge fields in the replyTo address
                        restRequest.AddParameter( "h:Reply-To", emailMessage.ReplyToEmail.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands ) );
                    }

                    foreach ( var mergeField in mergeFields )
                    {
                        recipientData.MergeFields.AddOrIgnore( mergeField.Key, mergeField.Value );
                    }

                    // From
                    restRequest.AddParameter( "from", new MailAddress( fromAddress, fromName).ToString() );

                    // To
                    restRequest.AddParameter(
                        "to",
                        new MailAddress(
                        recipientData.To.ResolveMergeFields( recipientData.MergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands ),
                        recipientData.Name.ResolveMergeFields( recipientData.MergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands ) ) );

                    // Safe Sender checks
                    CheckSafeSender( restRequest, fromAddress, globalAttributes.GetValue( "OrganizationEmail" ) );

                    // CC
                    foreach ( string cc in emailMessage.CCEmails.Where( e => e != string.Empty ) )
                    {
                        string ccRecipient = cc.ResolveMergeFields( recipientData.MergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
                        restRequest.AddParameter("cc", ccRecipient);
                    }

                    // BCC
                    foreach ( string bcc in emailMessage.BCCEmails.Where( e => e != string.Empty ) )
                    {
                        string bccRecipient = bcc.ResolveMergeFields( recipientData.MergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
                        restRequest.AddParameter( "bcc", bccRecipient );
                    }

                    // Subject
                    string subject = ResolveText( emailMessage.Subject, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, recipientData.MergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot ).Left(998);
                    restRequest.AddParameter( "subject", subject );

                    // Body (html)
                    string body = ResolveText( emailMessage.Message, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, recipientData.MergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
                    body = Regex.Replace( body, @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty );
                    restRequest.AddParameter( "html", body );

                    // Communication record for tracking opens & clicks
                    var metaData = new Dictionary<string, string>( emailMessage.MessageMetaData );
                    Guid? recipientGuid = null;

                    if ( emailMessage.CreateCommunicationRecord )
                    {
                        recipientGuid = Guid.NewGuid();
                        metaData.Add( "communication_recipient_guid", recipientGuid.Value.ToString() );
                    }

                    // Additional headers
                    AddAdditionalHeaders( restRequest, metaData );

                    // Attachments
                    if ( emailMessage.Attachments.Any() )
                    {
                        using ( var rockContext = new RockContext() )
                        {
                            var binaryFileService = new BinaryFileService( rockContext );
                            foreach ( var binaryFileId in emailMessage.Attachments.Where( a => a != null ).Select( a => a.Id ) )
                            {
                                var attachment = binaryFileService.Get( binaryFileId );
                                if (attachment != null )
                                {
                                    MemoryStream ms = new MemoryStream();
                                    attachment.ContentStream.CopyTo( ms );
                                    restRequest.AddFile( "attachment", ms.ToArray(), attachment.FileName );
                                }
                            }
                        }
                    }

                    // Send it
                    RestClient restClient = new RestClient
                    {
                        BaseUrl = new Uri(GetAttributeValue( "BaseURL" ) ),
                        Authenticator = new HttpBasicAuthenticator( "api", GetAttributeValue( "APIKey" ) )
                    };

                    // Call the API and get the response
                    Response = restClient.Execute( restRequest );

                    // Create the communication record
                    if ( emailMessage.CreateCommunicationRecord )
                    {
                        var transaction = new SaveCommunicationTransaction( recipientData.To, emailMessage.FromName, emailMessage.FromName, subject, body );
                        transaction.RecipientGuid = recipientGuid;
                        RockQueue.TransactionQueue.Enqueue( transaction );
                    }
                }
                catch ( Exception ex )
                {
                    errorMessages.Add( ex.Message );
                    ExceptionLogService.LogException( ex );
                }
            }

            return !errorMessages.Any();
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        public override void Send( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            using ( var communicationRockContext = new RockContext() )
            {
                // Requery the Communication
                communication = new CommunicationService( communicationRockContext )
                    .Queryable().Include( a => a.CreatedByPersonAlias.Person ).Include( a => a.CommunicationTemplate )
                    .FirstOrDefault( c => c.Id == communication.Id );

                // If there are no pending recipients than just exit the method
                if ( communication != null &&
                    communication.Status == Model.CommunicationStatus.Approved &&
                    ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ) )
                {
                    var qryRecipients = new CommunicationRecipientService( communicationRockContext ).Queryable();
                    if ( !qryRecipients
                        .Where( r =>
                            r.CommunicationId == communication.Id &&
                            r.Status == Model.CommunicationRecipientStatus.Pending &&
                            r.MediumEntityTypeId.HasValue &&
                            r.MediumEntityTypeId.Value == mediumEntityTypeId )
                        .Any())
                    {
                        return;
                    }
                }

                var currentPerson = communication.CreatedByPersonAlias?.Person;
                var globalAttributes = GlobalAttributesCache.Get();
                string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();
                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );
                var cssInliningEnabled = communication.CommunicationTemplate?.CssInliningEnabled ?? false;

                string fromAddress = string.IsNullOrWhiteSpace( communication.FromEmail ) ? globalAttributes.GetValue( "OrganizationEmail" ) : communication.FromEmail;
                string fromName = string.IsNullOrWhiteSpace( communication.FromName ) ? globalAttributes.GetValue( "OrganizationName" ) : communication.FromName;

                // Resolve any possible merge fields in the from address
                fromAddress = fromAddress.ResolveMergeFields( mergeFields, currentPerson, communication.EnabledLavaCommands );
                fromName = fromName.ResolveMergeFields( mergeFields, currentPerson, communication.EnabledLavaCommands );
                Parameter replyTo = new Parameter();
                
                // Reply To
                if ( communication.ReplyToEmail.IsNotNullOrWhiteSpace() )
                {
                    // Resolve any possible merge fields in the replyTo address
                    replyTo.Name = "h:Reply-To";
                    replyTo.Type = ParameterType.GetOrPost;
                    replyTo.Value = communication.ReplyToEmail.ResolveMergeFields( mergeFields, currentPerson );
                }

                var personEntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
                var communicationEntityTypeId = EntityTypeCache.Get( "Rock.Model.Communication" ).Id;
                var communicationCategoryGuid = Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid();

                RestRequest restRequest = null;

                // Loop through recipients and send the email
                bool recipientFound = true;
                while ( recipientFound )
                {
                    var recipientRockContext = new RockContext();
                    var recipient = Model.Communication.GetNextPending( communication.Id, mediumEntityTypeId, recipientRockContext );

                    // This means we are done, break the loop
                    if (recipient == null )
                    {
                        recipientFound = false;
                        break;
                    }

                    // Not valid save the obj with the status messages then go to the next one
                    if ( !ValidRecipient( recipient, communication.IsBulkCommunication ) )
                    {
                        recipientRockContext.SaveChanges();
                        continue;
                    }

                    try
                    {
                        // Create merge field dictionary
                        var mergeObjects = recipient.CommunicationMergeValues( mergeFields );

                        // Create the request obj
                        restRequest = new RestRequest( GetAttributeValue( "Resource" ), Method.POST );
                        restRequest.AddParameter( "domian", GetAttributeValue( "Domain" ), ParameterType.UrlSegment );

                        // ReplyTo
                        if ( communication.ReplyToEmail.IsNotNullOrWhiteSpace() )
                        {
                            restRequest.AddParameter( replyTo );
                        }

                        // From
                        restRequest.AddParameter( "from", new MailAddress( fromAddress, fromName ).ToString() );

                        // To
                        restRequest.AddParameter( "to",  new MailAddress( recipient.PersonAlias.Person.Email, recipient.PersonAlias.Person.FullName ).ToString() );
                        
                        // Safe sender checks
                        CheckSafeSender( restRequest, fromAddress, globalAttributes.GetValue( "OrganizationEmail" ) );

                        // CC
                        if ( communication.CCEmails.IsNotNullOrWhiteSpace() )
                        {
                            string ccRecipients = communication.CCEmails.ResolveMergeFields( mergeObjects, currentPerson );
                            foreach ( var ccRecipient in ccRecipients )
                            {
                                restRequest.AddParameter( "cc", ccRecipient );
                            }
                        }

                        // BCC
                        if ( communication.BCCEmails.IsNotNullOrWhiteSpace() )
                        {
                            string bccRecipients = communication.CCEmails.ResolveMergeFields( mergeObjects, currentPerson );
                            foreach ( var bccRecipient in bccRecipients )
                            {
                                restRequest.AddParameter( "bcc", bccRecipient );
                            }
                        }

                        // Subject
                        string subject = ResolveText( communication.Subject, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );
                        restRequest.AddParameter( "subject", subject );

                        // Body Plain Text
                        if ( mediumAttributes.ContainsKey( "DefaultPlainText" ) )
                        {
                            string plainText = ResolveText( mediumAttributes["DefaultPlainText"], currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );
                            if ( !string.IsNullOrWhiteSpace( plainText ) )
                            {
                                AlternateView plainTextView = AlternateView.CreateAlternateViewFromString( plainText, new ContentType( MediaTypeNames.Text.Plain ) );
                                restRequest.AddParameter( "text", plainTextView );
                            }
                        }

                        // Body HTML
                        string htmlBody = communication.Message;
                        
                        // Get the unsubscribe content and add a merge field for it
                        if ( communication.IsBulkCommunication && mediumAttributes.ContainsKey( "UnsubscribeHTML" ) )
                        {
                            string unsubscribeHtml = ResolveText( mediumAttributes["UnsubscribeHTML"], currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );
                            mergeObjects.AddOrReplace( "UnsubscribeOption", unsubscribeHtml );
                            htmlBody = ResolveText( htmlBody, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );

                            // Resolve special syntax needed if option was included in global attribute
                            if ( Regex.IsMatch( htmlBody, @"\[\[\s*UnsubscribeOption\s*\]\]" ) )
                            {
                                htmlBody = Regex.Replace( htmlBody, @"\[\[\s*UnsubscribeOption\s*\]\]", unsubscribeHtml );
                            }

                            // Add the unsubscribe option at end if it wasn't included in content
                            if ( !htmlBody.Contains( unsubscribeHtml ) )
                            {
                                htmlBody += unsubscribeHtml;
                            }
                        }
                        else
                        {
                            htmlBody = ResolveText( htmlBody, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );
                            htmlBody = Regex.Replace( htmlBody, @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty );
                        }

                        if ( !string.IsNullOrWhiteSpace( htmlBody ) )
                        {
                            if ( cssInliningEnabled )
                            {
                                // move styles inline to help it be compatible with more email clients
                                htmlBody = htmlBody.ConvertHtmlStylesToInlineAttributes();
                            }

                            // add the main Html content to the email
                            restRequest.AddParameter( "html", htmlBody );
                        }

                        // Headers
                        AddAdditionalHeaders( restRequest, new Dictionary<string, string>() { { "communication_recipient_guid", recipient.Guid.ToString() } } );

                        // Attachments
                        foreach ( var attachment in communication.GetAttachments( CommunicationType.Email).Select( a => a.BinaryFile ) )
                        {
                            MemoryStream ms = new MemoryStream();
                            attachment.ContentStream.CopyTo( ms );
                            restRequest.AddFile( "attachment", ms.ToArray(), attachment.FileName );
                        }

                        // Send the email
                        // Send it
                        RestClient restClient = new RestClient
                        {
                            BaseUrl = new Uri( GetAttributeValue( "BaseURL" ) ),
                            Authenticator = new HttpBasicAuthenticator( "api", GetAttributeValue( "APIKey" ) )
                        };

                        // Call the API and get the response
                        Response = restClient.Execute( restRequest );

                        // Update recipient status and status note
                        recipient.Status = Response.StatusCode == HttpStatusCode.OK ? CommunicationRecipientStatus.Delivered : CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = Response.StatusDescription;
                        recipient.TransportEntityTypeName = this.GetType().FullName;

                        // Log it
                        try
                        {
                            var historyChangeList = new History.HistoryChangeList();
                            historyChangeList.AddChange(
                                History.HistoryVerb.Sent,
                                History.HistoryChangeType.Record,
                                $"Communication" )
                                .SetRelatedData( fromName, communicationEntityTypeId, communication.Id )
                                .SetCaption( subject );

                            HistoryService.SaveChanges( recipientRockContext, typeof( Rock.Model.Person ), communicationCategoryGuid, recipient.PersonAlias.PersonId, historyChangeList, false, communication.SenderPersonAliasId );
                        }
                        catch ( Exception ex )
                        {
                            ExceptionLogService.LogException( ex, null );
                        }
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex );
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Exception: " + ex.Messages().AsDelimited( " => " );
                    }

                    recipientRockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Validates the recipient.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="isBulkCommunication">if set to <c>true</c> [is bulk communication].</param>
        /// <returns></returns>
        public override bool ValidRecipient( CommunicationRecipient recipient, bool isBulkCommunication )
        {
            bool valid = base.ValidRecipient( recipient, isBulkCommunication );
            if ( valid )
            {
                var person = recipient?.PersonAlias?.Person;
                if ( person != null )
                {
                    if ( string.IsNullOrWhiteSpace( person.Email ) )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "No Email Address";
                        valid = false;
                    }
                    else if ( !person.IsEmailActive )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Recipient Email Address is not active";
                        valid = false;
                    }
                    else if ( person.EmailPreference == Model.EmailPreference.DoNotEmail )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Communication Preference of 'Do Not Send Communication'";
                        valid = false;
                    }
                    else if ( person.EmailPreference == Model.EmailPreference.NoMassEmails && isBulkCommunication )
                    {
                        recipient.Status = CommunicationRecipientStatus.Failed;
                        recipient.StatusNote = "Communication Preference of 'No Bulk Communication'";
                        valid = false;
                    }
                }
            }

            return valid;
        }

        private void CheckSafeSender( RestRequest restRequest, string fromEmail, string organizationEmail )
        {
            List<string> toEmailAddresses = restRequest.Parameters.Where( p => p.Name == "to" ).Select( p => p.Value.ToString() ).ToList();

            // Get the safe sender domains
            var safeDomainValues = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_SAFE_SENDER_DOMAINS.AsGuid() ).DefinedValues;
            var safeDomains = safeDomainValues.Select( v => v.Value ).ToList();

            // Check to make sure the From email domain is a safe sender, if so then we are done.
            var fromParts = fromEmail.Split( new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries );
            if ( fromParts.Length == 2 && safeDomains.Contains( fromParts[1], StringComparer.OrdinalIgnoreCase ) )
            {
                return;
            }

            // The sender domain is not considered safe so check all the recipients to see if they have a domain that does not requrie a safe sender
            bool unsafeToDomain = false;

            foreach ( var toEmailAddress in toEmailAddresses )
            {
                bool safe = false;
                var toParts = toEmailAddress.Split( new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries );
                if ( toParts.Length == 2 && safeDomains.Contains( toParts[1], StringComparer.OrdinalIgnoreCase ) )
                {
                    var domain = safeDomainValues.FirstOrDefault( dv => dv.Value.Equals( toParts[1], StringComparison.OrdinalIgnoreCase ) );
                    safe = domain != null && domain.GetAttributeValue( "SafeToSendTo" ).AsBoolean();
                }

                if ( !safe )
                {
                    unsafeToDomain = true;
                    break;
                }
            }

            if ( unsafeToDomain )
            {
                if ( !string.IsNullOrWhiteSpace( organizationEmail ) && !organizationEmail.Equals( fromEmail, StringComparison.OrdinalIgnoreCase ) )
                {
                    // update the from param to the organizationemail
                    Parameter fromParam = restRequest.Parameters.Where( p => p.Name == "from" ).FirstOrDefault();
                    if ( fromParam != null )
                    {
                        restRequest.Parameters.Remove( fromParam );
                    }

                    restRequest.AddParameter( "from", organizationEmail );

                    Parameter replyParam = restRequest.Parameters.Where( p => p.Name == "h:Reply-To" && p.Value.ToString() == organizationEmail ).FirstOrDefault();

                    // Check the list of reply to address and add the org one if needed
                    if ( replyParam == null )
                    {
                        restRequest.AddParameter( "h:Reply-To", organizationEmail );
                    }
                }
            }
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

        #region Obsolete
        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( Communication communication, Dictionary<string, string> mediumAttributes ) instead", true )]
        public override void Send( Rock.Model.Communication communication )
        {
            int mediumEntityId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )?.Id ?? 0;
            Send( communication, mediumEntityId, null );
        }

        /// <summary>
        /// Sends the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public override void Send( SystemEmail template, List<RecipientData> recipients, string appRoot, string themeRoot )
        {
            Send( template, recipients, appRoot, themeRoot, false );
        }

        /// <summary>
        /// Sends the specified template.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="createCommunicationHistory">if set to <c>true</c> [create communication history].</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public void Send( SystemEmail template, List<RecipientData> recipients, string appRoot, string themeRoot, bool createCommunicationHistory )
        {
            var message = new RockEmailMessage();
            message.FromEmail = template.From;
            message.FromName = template.FromName;
            message.SetRecipients( recipients );
            template.To.SplitDelimitedValues().ToList().ForEach( to => message.AddRecipient( to ) );
            message.CCEmails = template.Cc.SplitDelimitedValues().ToList();
            message.BCCEmails = template.Bcc.SplitDelimitedValues().ToList();
            message.Subject = template.Subject;
            message.Message = template.Body;
            message.ThemeRoot = themeRoot;
            message.AppRoot = appRoot;
            message.CreateCommunicationRecord = createCommunicationHistory;

            var errorMessages = new List<string>();
            int mediumEntityId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )?.Id ?? 0;
            Send( message, mediumEntityId, null, out errorMessages );
        }

        /// <summary>
        /// Sends the specified medium data to the specified list of recipients.
        /// </summary>
        /// <param name="mediumData">The medium data.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public override void Send( Dictionary<string, string> mediumData, List<string> recipients, string appRoot, string themeRoot )
        {
            Send( mediumData, recipients, appRoot, themeRoot, true );
        }

        /// <summary>
        /// Sends the specified medium data to the specified list of recipients.
        /// </summary>
        /// <param name="mediumData">The medium data.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="createCommunicationHistory">if set to <c>true</c> [create communication history].</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public void Send( Dictionary<string, string> mediumData, List<string> recipients, string appRoot, string themeRoot, bool createCommunicationHistory )
        {
            Send( mediumData, recipients, appRoot, themeRoot, createCommunicationHistory, null );
        }

        /// <summary>
        /// Sends the specified medium data to the specified list of recipients.
        /// </summary>
        /// <param name="mediumData">The medium data.</param>
        /// <param name="recipients">The recipients.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="createCommunicationHistory">if set to <c>true</c> [create communication history].</param>
        /// <param name="metaData">The meta data.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public void Send( Dictionary<string, string> mediumData, List<string> recipients, string appRoot, string themeRoot, bool createCommunicationHistory, Dictionary<string, string> metaData )
        {
            var message = new RockEmailMessage();
            message.FromEmail = mediumData.GetValueOrNull( "From" ) ?? string.Empty;
            message.ReplyToEmail = mediumData.GetValueOrNull( "ReplyTo" ) ?? string.Empty;
            message.SetRecipients( recipients );
            message.Subject = mediumData.GetValueOrNull( "Subject" ) ?? string.Empty;
            message.Message = mediumData.GetValueOrNull( "Body" ) ?? string.Empty;
            message.ThemeRoot = themeRoot;
            message.AppRoot = appRoot;
            message.CreateCommunicationRecord = createCommunicationHistory;
            message.MessageMetaData = metaData;

            var errorMessages = new List<string>();
            int mediumEntityId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )?.Id ?? 0;
            Send( message, mediumEntityId, null, out errorMessages );
        }

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="from">From.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public override void Send( List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null )
        {
            Send( recipients, from, string.Empty, subject, body, appRoot, themeRoot, null );
        }

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="from">From.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="attachments">Attachments.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public override void Send( List<string> recipients, string from, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null )
        {
            Send( recipients, from, string.Empty, subject, body, appRoot, themeRoot, attachments );
        }

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="from">From.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="attachments">Attachments.</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public override void Send( List<string> recipients, string from, string fromName, string subject, string body, string appRoot = null, string themeRoot = null, List<Attachment> attachments = null )
        {
            Send( recipients, from, fromName, subject, body, appRoot, themeRoot, attachments, true );
        }

        /// <summary>
        /// Sends the specified recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="from">From.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="body">The body.</param>
        /// <param name="appRoot">The application root.</param>
        /// <param name="themeRoot">The theme root.</param>
        /// <param name="attachments">Attachments.</param>
        /// <param name="createCommunicationHistory">if set to <c>true</c> [create communication history].</param>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use Send( RockMessage message, out List<string> errorMessage ) method instead", true )]
        public void Send( List<string> recipients, string from, string fromName, string subject, string body, string appRoot, string themeRoot, List<Attachment> attachments, bool createCommunicationHistory )
        {
            var message = new RockEmailMessage();
            message.FromEmail = from;
            message.FromName = fromName;
            message.SetRecipients( recipients );
            message.Subject = subject;
            message.Message = body;
            message.ThemeRoot = themeRoot;
            message.AppRoot = appRoot;
            message.CreateCommunicationRecord = createCommunicationHistory;

            foreach ( var attachment in attachments )
            {
                var binaryFile = new BinaryFile();
                binaryFile.ContentStream = attachment.ContentStream;
                binaryFile.FileName = attachment.Name;
                message.Attachments.Add( binaryFile );
            }

            var errorMessages = new List<string>();
            int mediumEntityId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )?.Id ?? 0;
            Send( message, mediumEntityId, null, out errorMessages );
        }

        #endregion
    }
}
