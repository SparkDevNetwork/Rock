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
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;

using Rock.Data;
using Rock.Model;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Communication.Transport
{
    /// <summary>
    /// Sends a communication through SMTP protocol
    /// </summary>
    public abstract class SMTPComponent : TransportComponent
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
        public virtual string StatusNote { get { return string.Empty; } }

        #endregion

        /// <summary>
        /// Sends the specified rock message.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        public override bool Send( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var emailMessage = rockMessage as RockEmailMessage;
            if ( emailMessage != null )
            {
                // Common Merge Field
                var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, rockMessage.CurrentPerson );
                foreach ( var mergeField in rockMessage.AdditionalMergeFields )
                {
                    mergeFields.AddOrReplace( mergeField.Key, mergeField.Value );
                }

                string fromAddress = emailMessage.FromEmail;
                string fromName = emailMessage.FromName;

                // Resolve any possible merge fields in the from address
                fromAddress = fromAddress.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
                fromName = fromName.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );

                // From - if none is set, use the one in the Organization's GlobalAttributes.
                var globalAttributes = GlobalAttributesCache.Get();
                if ( string.IsNullOrWhiteSpace( fromAddress ) )
                {
                    fromAddress = globalAttributes.GetValue( "OrganizationEmail" );
                }

                if ( string.IsNullOrWhiteSpace( fromName ) )
                {
                    fromName = globalAttributes.GetValue( "OrganizationName" );
                }

                if ( fromAddress.IsNullOrWhiteSpace() )
                {
                    errorMessages.Add( "A From address was not provided and no Organization email address is configured." );
                    return false;
                }
                
                MailMessage message = new MailMessage();

                // Reply To
                try
                {
                    if ( emailMessage.ReplyToEmail.IsNotNullOrWhiteSpace() )
                    {
                        // Resolve any possible merge fields in the replyTo address
                        message.ReplyToList.Add( new MailAddress( emailMessage.ReplyToEmail.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands ) ) );
                    }
                }
                catch { }

                message.IsBodyHtml = true;
                message.Priority = MailPriority.Normal;

                using ( var smtpClient = GetSmtpClient() )
                {
                    foreach( var recipientData in rockMessage.GetRecipientData() )
                    {
                        try
                        { 
                            foreach( var mergeField in mergeFields )
                            {
                                recipientData.MergeFields.AddOrIgnore( mergeField.Key, mergeField.Value );
                            }

                            message.To.Clear();
                            message.CC.Clear();
                            message.Bcc.Clear();
                            message.Headers.Clear();

                            // Set From/To and check safe sender
                            message.From = new MailAddress( fromAddress, fromName );
                            message.To.Add( new MailAddress( 
                                recipientData.To.ResolveMergeFields( recipientData.MergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands ),
                                recipientData.Name.ResolveMergeFields( recipientData.MergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands ) ) );
                            CheckSafeSender( message, globalAttributes );

                            // cc
                            foreach ( string cc in emailMessage.CCEmails.Where( e => e != "" ) )
                            {
                                // Resolve any possible merge fields in the cc address
                                string ccRecipient = cc.ResolveMergeFields( recipientData.MergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
                                message.CC.Add( new MailAddress( ccRecipient ) );
                            }

                            // bcc
                            foreach ( string bcc in emailMessage.BCCEmails.Where( e => e != "" ) )
                            {
                                // Resolve any possible merge fields in the cc address
                                string bccRecipient = bcc.ResolveMergeFields( recipientData.MergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
                                message.Bcc.Add( new MailAddress( bccRecipient ) );
                            }

                            // Subject
                            string subject = ResolveText( emailMessage.Subject, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, recipientData.MergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );

                            // Body
                            string body = ResolveText( emailMessage.Message, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, recipientData.MergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
                            body = Regex.Replace( body, @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty );

                            message.Subject = subject;
                            message.Body = body;

                            var metaData = new Dictionary<string, string>( emailMessage.MessageMetaData );

                            // If a communication is going to get created, create a guid for tracking the opens/clicks
                            Guid? recipientGuid = null;
                            if ( emailMessage.CreateCommunicationRecord )
                            {
                                recipientGuid = Guid.NewGuid();
                                metaData.Add( "communication_recipient_guid", recipientGuid.Value.ToString() );
                            }

                            using ( var rockContext = new RockContext() )
                            {
                                // Recreate the attachments
                                message.Attachments.Clear();
                                if ( emailMessage.Attachments.Any() )
                                {
                                    var binaryFileService = new BinaryFileService( rockContext );
                                    foreach ( var binaryFileId in emailMessage.Attachments.Where( a => a != null ).Select( a => a.Id ) )
                                    {
                                        var attachment = binaryFileService.Get( binaryFileId );
                                        if ( attachment != null )
                                        {
                                            message.Attachments.Add( new Attachment( attachment.ContentStream, attachment.FileName ) );
                                        }
                                    }
                                }

                                AddAdditionalHeaders( message, metaData );

                                smtpClient.Send( message );
                            }

                            if ( emailMessage.CreateCommunicationRecord )
                            {
                                var transaction = new SaveCommunicationTransaction( recipientData.To, emailMessage.FromName, emailMessage.FromName, subject, body );
                                transaction.RecipientGuid = recipientGuid;
                                RockQueue.TransactionQueue.Enqueue( transaction );
                            }
                        }
                        catch (Exception ex)
                        {
                            errorMessages.Add( ex.Message );
                            ExceptionLogService.LogException( ex );
                        }
                    }
                }
            }

            return !errorMessages.Any();
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>Medi
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        public override void Send( Rock.Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            using ( var communicationRockContext = new RockContext() )
            {
                // Requery the Communication
                communication = new CommunicationService( communicationRockContext )
                    .Queryable()
                    .Include( a => a.CreatedByPersonAlias.Person )
                    .Include( a => a.CommunicationTemplate )
                    .FirstOrDefault( c => c.Id == communication.Id );

                bool hasPendingRecipients;
                if ( communication != null && 
                    communication.Status == Model.CommunicationStatus.Approved && 
                    ( !communication.FutureSendDateTime.HasValue || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 ))
                {
                    var qryRecipients = new CommunicationRecipientService( communicationRockContext ).Queryable();

                    hasPendingRecipients = qryRecipients
                        .Where( r => 
                            r.CommunicationId == communication.Id &&
                            r.Status == Model.CommunicationRecipientStatus.Pending &&
                            r.MediumEntityTypeId.HasValue &&
                            r.MediumEntityTypeId.Value == mediumEntityTypeId )
                        .Any();
                }
                else
                {
                    hasPendingRecipients = false;
                }

                if ( !hasPendingRecipients )
                {
                    return;
                }

                var currentPerson = communication.CreatedByPersonAlias?.Person;
                var globalAttributes = GlobalAttributesCache.Get();
                string publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );
                var cssInliningEnabled = communication.CommunicationTemplate?.CssInliningEnabled ?? false;

                string fromAddress = communication.FromEmail;
                string fromName = communication.FromName;

                // Resolve any possible merge fields in the from address
                fromAddress = fromAddress.ResolveMergeFields( mergeFields, currentPerson, communication.EnabledLavaCommands );
                fromName = fromName.ResolveMergeFields( mergeFields, currentPerson, communication.EnabledLavaCommands );

                // From - if none is set, use the one in the Organization's GlobalAttributes.
                if ( string.IsNullOrWhiteSpace( fromAddress ) )
                {
                    fromAddress = globalAttributes.GetValue( "OrganizationEmail" );
                }

                if ( string.IsNullOrWhiteSpace( fromName ) )
                {
                    fromName = globalAttributes.GetValue( "OrganizationName" );
                }
                
                MailMessage message = new MailMessage();

                // Reply To
                try
                {
                    string replyTo = communication.ReplyToEmail;
                    if ( !string.IsNullOrWhiteSpace( replyTo ) )
                    {
                        // Resolve any possible merge fields in the replyTo address
                        message.ReplyToList.Add( new MailAddress( replyTo.ResolveMergeFields( mergeFields, currentPerson ) ) );
                    }
                }
                catch { }

                message.IsBodyHtml = true;
                message.Priority = MailPriority.Normal;

                using ( var smtpClient = GetSmtpClient() )
                {
                    var personEntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
                    var communicationEntityTypeId = EntityTypeCache.Get( "Rock.Model.Communication" ).Id;
                    var communicationCategoryGuid = Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid();

                    bool recipientFound = true;
                    while ( recipientFound )
                    {
                        // make a new rockContext per recipient
                        var recipientRockContext = new RockContext();
                        var recipient = Rock.Model.Communication.GetNextPending( communication.Id, mediumEntityTypeId, recipientRockContext );
                        if ( recipient != null )
                        {
                            if ( ValidRecipient( recipient, communication.IsBulkCommunication ) )
                            {
                                try
                                {
                                    message.To.Clear();
                                    message.CC.Clear();
                                    message.Bcc.Clear();
                                    message.Headers.Clear();
                                    message.AlternateViews.Clear();

                                    // Set From/To and check safe sender
                                    message.From = new MailAddress( fromAddress, fromName );
                                    message.To.Add( new MailAddress( recipient.PersonAlias.Person.Email, recipient.PersonAlias.Person.FullName ) );
                                    CheckSafeSender( message, globalAttributes );

                                    // Create merge field dictionary
                                    var mergeObjects = recipient.CommunicationMergeValues( mergeFields );
                                        
                                    // CC
                                    string cc = communication.CCEmails;
                                    if ( !string.IsNullOrWhiteSpace( cc ) )
                                    {
                                        // Resolve any possible merge fields in the cc address
                                        cc = cc.ResolveMergeFields( mergeObjects, currentPerson );
                                        foreach ( string ccRecipient in cc.SplitDelimitedValues() )
                                        {
                                            message.CC.Add( new MailAddress( ccRecipient ) );
                                        }
                                    }

                                    // BCC
                                    string bcc = communication.BCCEmails;
                                    if ( !string.IsNullOrWhiteSpace( bcc ) )
                                    {
                                        bcc = bcc.ResolveMergeFields( mergeObjects, currentPerson );
                                        foreach ( string bccRecipient in bcc.SplitDelimitedValues() )
                                        {
                                            // Resolve any possible merge fields in the bcc address
                                            message.Bcc.Add( new MailAddress( bccRecipient ) );
                                        }
                                    }

                                    // Subject
                                    message.Subject = ResolveText( communication.Subject, currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );

                                    // Plain text
                                    if ( mediumAttributes.ContainsKey( "DefaultPlainText" ) )
                                    {
                                        string plainText = ResolveText( mediumAttributes["DefaultPlainText"], currentPerson, communication.EnabledLavaCommands, mergeObjects, publicAppRoot );
                                        if ( !string.IsNullOrWhiteSpace( plainText ) )
                                        {
                                            AlternateView plainTextView = AlternateView.CreateAlternateViewFromString( plainText, new System.Net.Mime.ContentType( MediaTypeNames.Text.Plain ) );
                                            message.AlternateViews.Add( plainTextView );
                                        }
                                    }

                                    // Add Html view
                                    // Get the unsubscribe content and add a merge field for it
                                    string htmlBody = communication.Message;
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
                                        AlternateView htmlView = AlternateView.CreateAlternateViewFromString( htmlBody, new System.Net.Mime.ContentType( MediaTypeNames.Text.Html ) );
                                        message.AlternateViews.Add( htmlView );
                                    }

                                    // Add any additional headers that specific SMTP provider needs
                                    var metaData = new Dictionary<string, string>();
                                    metaData.Add( "communication_recipient_guid", recipient.Guid.ToString() );
                                    AddAdditionalHeaders( message, metaData );

                                    // Recreate the attachments
                                    message.Attachments.Clear();
                                    foreach ( var binaryFile in communication.GetAttachments( CommunicationType.Email ).Select( a => a.BinaryFile ) )
                                    {
                                        message.Attachments.Add( new Attachment( binaryFile.ContentStream, binaryFile.FileName ) );
                                    }

                                    smtpClient.Send( message );
                                    recipient.Status = CommunicationRecipientStatus.Delivered;

                                    string statusNote = StatusNote;
                                    if ( !string.IsNullOrWhiteSpace( statusNote ) )
                                    {
                                        recipient.StatusNote = statusNote;
                                    }

                                    recipient.TransportEntityTypeName = this.GetType().FullName;

                                    try
                                    {
                                        var historyChangeList = new History.HistoryChangeList();
                                        historyChangeList.AddChange(
                                            History.HistoryVerb.Sent,
                                            History.HistoryChangeType.Record,
                                            $"Communication" )
                                            .SetRelatedData( message.From.DisplayName, communicationEntityTypeId, communication.Id )
                                            .SetCaption( message.Subject );

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
                            }

                            recipientRockContext.SaveChanges();
                        }
                        else
                        {
                            recipientFound = false;
                        }
                    }
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

        /// <summary>
        /// Checks to make sure the sender's email address domain is one from the
        /// SystemGuid.DefinedType.COMMUNICATION_SAFE_SENDER_DOMAINS.  If it is not
        /// it will replace the From address with the one defined by the OrganizationEmail
        /// global attribute.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="globalAttributes">The global attributes.</param>
        private void CheckSafeSender( MailMessage message, GlobalAttributesCache globalAttributes )
        {
            if ( message != null && message.From != null )
            {
                string from = message.From.Address;
                string fromName = message.From.DisplayName;

                // Get the safe sender domains
                var safeDomainValues = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_SAFE_SENDER_DOMAINS.AsGuid() ).DefinedValues;
                var safeDomains = safeDomainValues.Select( v => v.Value ).ToList();

                // Check to make sure the From email domain is a safe sender
                var fromParts = from.Split( new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries );
                if ( fromParts.Length != 2 || !safeDomains.Contains( fromParts[1], StringComparer.OrdinalIgnoreCase ) )
                {
                    // The sending email address is not a safe sender domain, but check to see if all the recipients have a domain
                    // that does not require a safe sender domain
                    bool unsafeToDomain = false;
                    foreach ( var to in message.To )
                    {
                        bool safe = false;
                        var toParts = to.Address.Split( new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries );
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
                        string orgEmail = globalAttributes.GetValue( "OrganizationEmail" );
                        if ( !string.IsNullOrWhiteSpace( orgEmail ) && !orgEmail.Equals( from, StringComparison.OrdinalIgnoreCase ) )
                        {
                            message.From = new MailAddress( orgEmail, fromName );

                            bool addReplyTo = true;
                            foreach ( var replyTo in message.ReplyToList )
                            {
                                if ( replyTo.Address.Equals( from, StringComparison.OrdinalIgnoreCase ) )
                                {
                                    addReplyTo = false;
                                    break;
                                }
                            }

                            if ( addReplyTo )
                            {
                                message.ReplyToList.Add( new MailAddress( from, fromName ) );
                            }
                        }
                    }
                }
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
            int mediumEntityId = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )?.Id ?? 0;
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
            int mediumEntityId = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )?.Id ?? 0;
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
            int mediumEntityId = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )?.Id ?? 0;
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
            int mediumEntityId = EntityTypeCache.Read( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )?.Id ?? 0;
            Send( message, mediumEntityId, null, out errorMessages );
        }

        #endregion

    }
}
