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
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Rock.Communication.Transport;
using Rock.Data;
using Rock.Logging;
using Rock.Model;
using Rock.Tasks;
using Rock.Transactions;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// This abstract class implements the code needed to create an email with all of the validation, lava substitution, and error checking completed.
    /// </summary>
    /// <seealso cref="Rock.Communication.TransportComponent" />
    public abstract class EmailTransportComponent : TransportComponent
    {
        /// <summary>
        /// Send the implementation specific email.
        /// </summary>
        /// <param name="rockEmailMessage">The rock email message.</param>
        /// <returns></returns>
        /// <remarks>
        /// This class will call this method and pass the post processed data in a rock email message which
        /// can then be used to send the implementation specific message.
        /// </remarks>
        protected abstract EmailSendResponse SendEmail( RockEmailMessage rockEmailMessage );

        /// <summary>
        /// Sends the email asynchronous.
        /// </summary>
        /// <param name="rockEmailMessage">The rock email message.</param>
        /// <returns></returns>
        /// <remarks>
        /// This class will call this method and pass the post processed data in a rock email message which
        /// can then be used to send the implementation specific message.
        /// </remarks>
        protected virtual Task<EmailSendResponse> SendEmailAsync( RockEmailMessage rockEmailMessage )
        {
            throw new NotImplementedException();
        }

        #region Async Send Methods
        /// <summary>
        /// Sends the RockMessage asynchronously via email.
        /// </summary>
        /// <param name="rockMessage">The rock message.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <returns></returns>
        /// <remarks>
        /// In order for this method to be used the derived class must implement the IAsyncTransport interface.
        /// </remarks>
        public async Task<SendMessageResult> SendAsync( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            var sendResult = new SendMessageResult();

            var emailMessage = rockMessage as RockEmailMessage;
            if ( emailMessage == null )
            {
                sendResult.Errors.Add( "No email message was provided." );
                return sendResult;
            }

            var mergeFields = GetAllMergeFields( rockMessage.CurrentPerson, rockMessage.AdditionalMergeFields );
            var globalAttributes = GlobalAttributesCache.Get();
            var fromAddress = GetFromAddress( emailMessage, mergeFields, globalAttributes );

            if ( fromAddress.IsNullOrWhiteSpace() )
            {
                sendResult.Errors.Add( "A From address was not provided." );
                return sendResult;
            }

            var templateMailMessage = GetTemplateRockEmailMessage( emailMessage, mergeFields, globalAttributes, mediumAttributes );
            var organizationEmail = globalAttributes.GetValue( "OrganizationEmail" );

            var sendTask = new List<Task<SendMessageResult>>();
            var asyncTransport = this as IAsyncTransport;
            var maxParallelization = asyncTransport?.MaxParallelization ?? 10;

            using ( var mutex = new SemaphoreSlim( maxParallelization ) )
            {
                foreach ( var rockMessageRecipient in rockMessage.GetRecipients() )
                {
                    await mutex.WaitAsync().ConfigureAwait( false );

                    sendTask.Add( ThrottleHelper.ThrottledExecute( () => SendEmailToRecipientAsync( templateMailMessage, rockMessageRecipient, mergeFields, organizationEmail ), mutex ) );
                }

                /*
                 * Now that we have fired off all of the task, we need to wait for them to complete, get their results,
                 * and then process that result. Once all of the task have been completed we can continue.
                 */
                while ( sendTask.Count > 0 )
                {
                    var sendRecipientResultTask = await Task.WhenAny( sendTask ).ConfigureAwait( false );
                    sendTask.Remove( sendRecipientResultTask );

                    var sendRecipientResult = await sendRecipientResultTask.ConfigureAwait( false );
                    sendResult.MessagesSent += sendRecipientResult.MessagesSent;
                    sendResult.Errors.AddRange( sendRecipientResult.Errors );
                    sendResult.Warnings.AddRange( sendRecipientResult.Warnings );
                }
            }

            return sendResult;
        }

        /// <summary>
        /// Sends the Communication asynchronously to the email recipients.
        /// </summary>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <remarks>
        /// In order for this method to be used the derived class must implement the IAsyncTransport interface.
        /// </remarks>
        public async Task SendAsync( Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            using ( var communicationRockContext = new RockContext() )
            {
                // Requery the Communication
                communication = GetSendableCommunication( communication.Id, communicationRockContext );

                if ( communication == null )
                {
                    return;
                }

                // If there are no pending recipients than just exit the method
                var communicationRecipientService = new CommunicationRecipientService( communicationRockContext );

                var unprocessedRecipientCount = communicationRecipientService
                    .Queryable()
                    .ByCommunicationId( communication.Id )
                    .ByStatus( CommunicationRecipientStatus.Pending )
                    .ByMediumEntityTypeId( mediumEntityTypeId )
                    .Count();

                if ( unprocessedRecipientCount == 0 )
                {
                    return;
                }

                var currentPerson = communication.CreatedByPersonAlias?.Person;
                var mergeFields = GetAllMergeFields( currentPerson, communication.AdditionalLavaFields );

                var globalAttributes = GlobalAttributesCache.Get();

                var templateEmailMessage = GetTemplateRockEmailMessage( communication, mergeFields, globalAttributes, mediumAttributes );
                var organizationEmail = globalAttributes.GetValue( "OrganizationEmail" );

                var cssInliningEnabled = communication.CommunicationTemplate?.CssInliningEnabled ?? false;

                var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
                var communicationEntityTypeId = EntityTypeCache.Get<Model.Communication>().Id;
                var communicationCategoryGuid = Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid();

                // Loop through recipients and send the email
                var sendingTask = new List<Task>( unprocessedRecipientCount );
                var asyncTransport = this as IAsyncTransport;
                var maxParallelization = asyncTransport?.MaxParallelization ?? 10;

                using ( var mutex = new SemaphoreSlim( maxParallelization ) )
                {
                    var recipientFound = true;
                    while ( recipientFound )
                    {
                        var getRecipientTimer = System.Diagnostics.Stopwatch.StartNew();
                        var recipient = GetNextPending( communication.Id, mediumEntityTypeId, communication.IsBulkCommunication );
                        RockLogger.Log.Debug( RockLogDomains.Communications, "{0}: It took {1} ticks to get the next pending recipient.", nameof( SendAsync ), getRecipientTimer.ElapsedTicks );

                        // This means we are done, break the loop
                        if ( recipient == null )
                        {
                            recipientFound = false;
                            continue;
                        }

                        var startMutexWait = System.Diagnostics.Stopwatch.StartNew();
                        await mutex.WaitAsync().ConfigureAwait( false );

                        RockLogger.Log.Debug( RockLogDomains.Communications, "{0}: Starting to send {1} to {2} with a {3} tick wait.", nameof( SendAsync ), communication.Name, recipient.Id, startMutexWait.ElapsedTicks );
                        sendingTask.Add( ThrottleHelper.ThrottledExecute( () => SendToRecipientAsync( recipient.Id, communication, mediumEntityTypeId, mediumAttributes, mergeFields, templateEmailMessage, organizationEmail ), mutex ) );
                    }

                    /*
                     * Now that we have fired off all of the task, we need to wait for them to complete. 
                     * Once all of the task have been completed we can continue.
                     */
                    while ( sendingTask.Count > 0 )
                    {
                        var completedTask = await Task.WhenAny( sendingTask ).ConfigureAwait( false );
                        sendingTask.Remove( completedTask );
                    }
                }
            }
        }
        #endregion

        #region Non-async Send Methods
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
            if ( emailMessage == null )
            {
                return false;
            }

            var mergeFields = GetAllMergeFields( rockMessage.CurrentPerson, rockMessage.AdditionalMergeFields );
            var globalAttributes = GlobalAttributesCache.Get();
            var fromAddress = GetFromAddress( emailMessage, mergeFields, globalAttributes );

            if ( fromAddress.IsNullOrWhiteSpace() )
            {
                errorMessages.Add( "A From address was not provided." );
                return false;
            }

            var templateMailMessage = GetTemplateRockEmailMessage( emailMessage, mergeFields, globalAttributes, mediumAttributes );
            var organizationEmail = globalAttributes.GetValue( "OrganizationEmail" );

            foreach ( var rockMessageRecipient in rockMessage.GetRecipients() )
            {
                try
                {
                    var recipientEmailMessage = GetRecipientRockEmailMessage( templateMailMessage, rockMessageRecipient, mergeFields, organizationEmail );

                    var result = SendEmail( recipientEmailMessage );

                    var sendMessageResult = HandleEmailSendResponse( rockMessageRecipient, recipientEmailMessage, result );

                    errorMessages.AddRange( sendMessageResult.Errors );
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
                    .Queryable()
                    .Include( a => a.CreatedByPersonAlias.Person )
                    .Include( a => a.CommunicationTemplate )
                    .FirstOrDefault( c => c.Id == communication.Id );

                var isApprovedCommunication = communication != null && communication.Status == Model.CommunicationStatus.Approved;
                var isReadyToSend = communication != null &&
                                        ( !communication.FutureSendDateTime.HasValue
                                        || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 );

                if ( !isApprovedCommunication || !isReadyToSend )
                {
                    return;
                }

                // If there are no pending recipients than just exit the method
                var communicationRecipientService = new CommunicationRecipientService( communicationRockContext );

                var hasUnprocessedRecipients = communicationRecipientService
                    .Queryable()
                    .ByCommunicationId( communication.Id )
                    .ByStatus( CommunicationRecipientStatus.Pending )
                    .ByMediumEntityTypeId( mediumEntityTypeId )
                    .Any();

                if ( !hasUnprocessedRecipients )
                {
                    return;
                }

                var currentPerson = communication.CreatedByPersonAlias?.Person;
                var mergeFields = GetAllMergeFields( currentPerson, communication.AdditionalLavaFields );

                var globalAttributes = GlobalAttributesCache.Get();

                var templateEmailMessage = GetTemplateRockEmailMessage( communication, mergeFields, globalAttributes, mediumAttributes );
                var organizationEmail = globalAttributes.GetValue( "OrganizationEmail" );

                var personEntityTypeId = EntityTypeCache.Get<Person>().Id;
                var communicationEntityTypeId = EntityTypeCache.Get<Model.Communication>().Id;
                var communicationCategoryGuid = Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid();

                // Loop through recipients and send the email
                var recipientFound = true;
                while ( recipientFound )
                {
                    using ( var recipientRockContext = new RockContext() )
                    {
                        var recipient = Model.Communication.GetNextPending( communication.Id, mediumEntityTypeId, recipientRockContext );

                        // This means we are done, break the loop
                        if ( recipient == null )
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
                            var recipientEmailMessage = GetRecipientRockEmailMessage( templateEmailMessage, communication, recipient, mergeObjects, organizationEmail, mediumAttributes );

                            var result = SendEmail( recipientEmailMessage );

                            // Update recipient status and status note
                            recipient.Status = result.Status;
                            recipient.StatusNote = result.StatusNote;
                            recipient.TransportEntityTypeName = this.GetType().FullName;

                            // Log it
                            try
                            {
                                var historyChangeList = new History.HistoryChangeList();
                                historyChangeList.AddChange(
                                    History.HistoryVerb.Sent,
                                    History.HistoryChangeType.Record,
                                    $"Communication" )
                                    .SetRelatedData( recipientEmailMessage.FromName, communicationEntityTypeId, communication.Id )
                                    .SetCaption( recipientEmailMessage.Subject );

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
        }
        #endregion

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

        /// <summary>
        /// Gets the list of safe domains from COMMUNICATION_SAFE_SENDER_DOMAINS.
        /// </summary>
        /// <returns></returns>
        protected List<string> GetSafeDomains()
        {
            // Get the safe sender domains
            var safeDomainValues = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_SAFE_SENDER_DOMAINS.AsGuid() ).DefinedValues;
            return safeDomainValues.Select( v => v.Value ).ToList();
        }

        /// <summary>
        /// Gets the domain part of the email address everything after the @.
        /// </summary>
        /// <param name="emailAddress">The email address.</param>
        /// <returns></returns>
        protected string GetEmailDomain( string emailAddress )
        {
            var fromParts = emailAddress.Split( new char[] { '@' }, StringSplitOptions.RemoveEmptyEntries );
            if ( fromParts.Length == 2 )
            {
                return fromParts[1];
            }

            return string.Empty;
        }

        /// <summary>
        /// Checks the safe sender.
        /// </summary>
        /// <param name="toEmailAddresses">To email addresses.</param>
        /// <param name="fromEmail">From email.</param>
        /// <param name="organizationEmail">The organization email.</param>
        /// <returns></returns>
        protected virtual SafeSenderResult CheckSafeSender( List<string> toEmailAddresses, MailAddress fromEmail, string organizationEmail )
        {
            var result = new SafeSenderResult();

            // Get the safe sender domains
            var safeDomains = GetSafeDomains();

            // Check to make sure the From email domain is a safe sender, if so then we are done.
            var emailDomain = GetEmailDomain( fromEmail.Address );
            if ( emailDomain.IsNotNullOrWhiteSpace() && safeDomains.Contains( emailDomain, StringComparer.OrdinalIgnoreCase ) )
            {
                return result;
            }

            // The sender domain is not considered safe so check all the recipients to see if they have a domain that does not requrie a safe sender
            var safeDomainValues = DefinedTypeCache.Get( SystemGuid.DefinedType.COMMUNICATION_SAFE_SENDER_DOMAINS.AsGuid() ).DefinedValues;
            foreach ( var toEmailAddress in toEmailAddresses )
            {
                bool safe = false;
                var toEmailDomain = GetEmailDomain( toEmailAddress );
                if ( toEmailDomain.IsNotNullOrWhiteSpace() && safeDomains.Contains( toEmailDomain, StringComparer.OrdinalIgnoreCase ) )
                {
                    var domain = safeDomainValues.FirstOrDefault( dv => dv.Value.Equals( toEmailDomain, StringComparison.OrdinalIgnoreCase ) );
                    safe = domain != null && domain.GetAttributeValue( "SafeToSendTo" ).AsBoolean();
                }

                if ( !safe )
                {
                    result.IsUnsafeDomain = true;
                    break;
                }
            }

            if ( result.IsUnsafeDomain )
            {
                if ( !string.IsNullOrWhiteSpace( organizationEmail ) && !organizationEmail.Equals( fromEmail.Address, StringComparison.OrdinalIgnoreCase ) )
                {
                    result.SafeFromAddress = new MailAddress( organizationEmail, fromEmail.DisplayName );
                    result.ReplyToAddress = fromEmail;
                }
            }

            return result;
        }

        private string GetFromName( RockEmailMessage emailMessage, Dictionary<string, object> mergeFields, GlobalAttributesCache globalAttributes )
        {
            string fromName = emailMessage.FromName.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
            fromName = fromName.IsNullOrWhiteSpace() ? globalAttributes.GetValue( "OrganizationName" ) : fromName;
            return fromName;
        }

        private string GetFromAddress( RockEmailMessage emailMessage, Dictionary<string, object> mergeFields, GlobalAttributesCache globalAttributes )
        {
            // Resolve any possible merge fields in the from address
            string fromAddress = emailMessage.FromEmail.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
            fromAddress = fromAddress.IsNullOrWhiteSpace() ? globalAttributes.GetValue( "OrganizationEmail" ) : fromAddress;
            return fromAddress;
        }

        private Dictionary<string, object> GetAllMergeFields( Person currentPerson, Dictionary<string, object> additionalMergeFields )
        {
            // Common Merge Field
            var mergeFields = Lava.LavaHelper.GetCommonMergeFields( null, currentPerson );

            if ( additionalMergeFields != null )
            {
                foreach ( var mergeField in additionalMergeFields )
                {
                    mergeFields.AddOrReplace( mergeField.Key, mergeField.Value );
                }
            }

            return mergeFields;
        }

        private string GetRecipientReplyToAddress( RockEmailMessage emailMessage, Dictionary<string, object> mergeFields, SafeSenderResult safeSenderResult )
        {
            var replyToAddress = string.Empty;
            if ( emailMessage.ReplyToEmail.IsNotNullOrWhiteSpace() )
            {
                replyToAddress = emailMessage.ReplyToEmail.ResolveMergeFields( mergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands );
            }

            if ( safeSenderResult.IsUnsafeDomain && safeSenderResult.ReplyToAddress != null )
            {
                if ( replyToAddress.IsNullOrWhiteSpace() )
                {
                    replyToAddress = safeSenderResult.ReplyToAddress.ToString();
                }
                else
                {
                    replyToAddress += $",{safeSenderResult.ReplyToAddress.ToString()}";
                }
            }

            return replyToAddress;
        }

        private RockEmailMessage GetTemplateRockEmailMessage( RockMessage rockMessage, Dictionary<string, object> mergeFields, GlobalAttributesCache globalAttributes, Dictionary<string, string> mediumAttributes )
        {
            var templateRockEmailMessage = new RockEmailMessage();

            var emailMessage = rockMessage as RockEmailMessage;
            if ( emailMessage == null )
            {
                return null;
            }

            templateRockEmailMessage.AppRoot = emailMessage.AppRoot;
            templateRockEmailMessage.CurrentPerson = emailMessage.CurrentPerson;
            templateRockEmailMessage.EnabledLavaCommands = emailMessage.EnabledLavaCommands;
            templateRockEmailMessage.CssInliningEnabled = emailMessage.CssInliningEnabled;
            if ( mediumAttributes.ContainsKey( "CSSInliningEnabled" ) )
            {
                var mediumCssInlining = mediumAttributes["CSSInliningEnabled"].AsBoolean();
                templateRockEmailMessage.CssInliningEnabled = templateRockEmailMessage.CssInliningEnabled || mediumCssInlining;
            }

            templateRockEmailMessage.ReplyToEmail = emailMessage.ReplyToEmail;
            templateRockEmailMessage.SystemCommunicationId = emailMessage.SystemCommunicationId;
            templateRockEmailMessage.CreateCommunicationRecord = emailMessage.CreateCommunicationRecord;
            templateRockEmailMessage.SendSeperatelyToEachRecipient = emailMessage.SendSeperatelyToEachRecipient;
            templateRockEmailMessage.ThemeRoot = emailMessage.ThemeRoot;

            var fromAddress = GetFromAddress( emailMessage, mergeFields, globalAttributes );
            var fromName = GetFromName( emailMessage, mergeFields, globalAttributes );

            if ( fromAddress.IsNullOrWhiteSpace() )
            {
                return null;
            }

            templateRockEmailMessage.FromEmail = fromAddress;
            templateRockEmailMessage.FromName = fromName;

            // CC
            templateRockEmailMessage.CCEmails = emailMessage.CCEmails;

            // BCC
            templateRockEmailMessage.BCCEmails = emailMessage.BCCEmails;

            templateRockEmailMessage.Subject = emailMessage.Subject;
            templateRockEmailMessage.Message = emailMessage.Message;
            templateRockEmailMessage.PlainTextMessage = emailMessage.PlainTextMessage;

            // Attachments
            if ( emailMessage.Attachments.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var binaryFileService = new BinaryFileService( rockContext );
                    foreach ( var binaryFileId in emailMessage.Attachments.Where( a => a != null ).Select( a => a.Id ) )
                    {
                        var attachment = binaryFileService.Get( binaryFileId );

                        // We need to call content stream to make sure it is loaded while we have the rock context.
                        var attachmentString = attachment.ContentStream;
                        templateRockEmailMessage.Attachments.Add( attachment );
                    }
                }
            }

            // Communication record for tracking opens & clicks
            templateRockEmailMessage.MessageMetaData = emailMessage.MessageMetaData;

            return templateRockEmailMessage;
        }

        private RockEmailMessage GetTemplateRockEmailMessage( Model.Communication communication, Dictionary<string, object> mergeFields, GlobalAttributesCache globalAttributes, Dictionary<string, string> mediumAttributes )
        {
            var resultEmailMessage = new RockEmailMessage();

            var publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" );
            var cssInliningEnabled = communication.CommunicationTemplate?.CssInliningEnabled ?? false;
            if ( mediumAttributes.ContainsKey( "CSSInliningEnabled" ) )
            {
                var mediumCssInlining = mediumAttributes["CSSInliningEnabled"].AsBoolean();
                cssInliningEnabled = cssInliningEnabled || mediumCssInlining;
            }

            resultEmailMessage.AppRoot = publicAppRoot;
            resultEmailMessage.CssInliningEnabled = cssInliningEnabled;
            resultEmailMessage.CurrentPerson = communication.CreatedByPersonAlias?.Person;
            resultEmailMessage.EnabledLavaCommands = communication.EnabledLavaCommands;
            resultEmailMessage.FromEmail = communication.FromEmail;
            resultEmailMessage.FromName = communication.FromName;

            var fromAddress = GetFromAddress( resultEmailMessage, mergeFields, globalAttributes );
            var fromName = GetFromName( resultEmailMessage, mergeFields, globalAttributes );

            resultEmailMessage.FromEmail = fromAddress;
            resultEmailMessage.FromName = fromName;

            // Reply To
            var replyToEmail = string.Empty;
            if ( communication.ReplyToEmail.IsNotNullOrWhiteSpace() )
            {
                // Resolve any possible merge fields in the replyTo address
                replyToEmail = communication.ReplyToEmail.ResolveMergeFields( mergeFields, resultEmailMessage.CurrentPerson );
            }

            resultEmailMessage.ReplyToEmail = replyToEmail;

            // Attachments
            resultEmailMessage.Attachments = communication.GetAttachments( CommunicationType.Email ).Select( a => a.BinaryFile ).ToList();

            // Load up the content stream while the context is still active.
            for ( int i = 0; i < resultEmailMessage.Attachments.Count; i++ )
            {
                var _ = resultEmailMessage.Attachments[i].ContentStream;
            }

            return resultEmailMessage;
        }

        private RockEmailMessage GetRecipientRockEmailMessage( RockEmailMessage emailMessage, RockMessageRecipient rockMessageRecipient, Dictionary<string, object> mergeFields, string organizationEmail )
        {
            var recipientEmail = new RockEmailMessage();
            recipientEmail.CurrentPerson = emailMessage.CurrentPerson;
            recipientEmail.EnabledLavaCommands = emailMessage.EnabledLavaCommands;
            recipientEmail.AppRoot = emailMessage.AppRoot;
            recipientEmail.CssInliningEnabled = emailMessage.CssInliningEnabled;
            recipientEmail.SendSeperatelyToEachRecipient = emailMessage.SendSeperatelyToEachRecipient;
            recipientEmail.ThemeRoot = emailMessage.ThemeRoot;

            // CC
            recipientEmail.CCEmails = emailMessage.CCEmails;

            // BCC
            recipientEmail.BCCEmails = emailMessage.BCCEmails;

            // Attachments
            recipientEmail.Attachments = emailMessage.Attachments;

            // Communication record for tracking opens & clicks
            recipientEmail.MessageMetaData = new Dictionary<string, string>( emailMessage.MessageMetaData );

            foreach ( var mergeField in mergeFields )
            {
                rockMessageRecipient.MergeFields.AddOrIgnore( mergeField.Key, mergeField.Value );
            }

            // To
            var toEmailAddress = new RockEmailMessageRecipient( null, null )
            {
                To = rockMessageRecipient
                    .To
                    .ResolveMergeFields( rockMessageRecipient.MergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands ),
                Name = rockMessageRecipient
                    .Name
                    .ResolveMergeFields( rockMessageRecipient.MergeFields, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands )
            };

            recipientEmail.SetRecipients( new List<RockEmailMessageRecipient> { toEmailAddress } );

            var fromMailAddress = new MailAddress( emailMessage.FromEmail, emailMessage.FromName );
            var checkResult = CheckSafeSender( new List<string> { toEmailAddress.EmailAddress }, fromMailAddress, organizationEmail );

            // Reply To
            recipientEmail.ReplyToEmail = GetRecipientReplyToAddress( emailMessage, mergeFields, checkResult );

            // From
            if ( checkResult.IsUnsafeDomain && checkResult.SafeFromAddress != null )
            {
                recipientEmail.FromName = checkResult.SafeFromAddress.DisplayName;
                recipientEmail.FromEmail = checkResult.SafeFromAddress.Address;
            }
            else
            {
                recipientEmail.FromName = fromMailAddress.DisplayName;
                recipientEmail.FromEmail = fromMailAddress.Address;
            }

            // Subject
            var subject = ResolveText( emailMessage.Subject, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, rockMessageRecipient.MergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot ).Left( 998 );
            recipientEmail.Subject = subject.IsNullOrWhiteSpace() ? subject : Regex.Replace( subject, @"(\r?\n?)", String.Empty );

            // Plain Text Message
            recipientEmail.PlainTextMessage = ResolveText( emailMessage.PlainTextMessage, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, rockMessageRecipient.MergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );

            // Body (HTML)
            string body = ResolveText( emailMessage.Message, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, rockMessageRecipient.MergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            if ( body.IsNotNullOrWhiteSpace() )
            {
                body = Regex.Replace( body, @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty );

                if ( emailMessage.CssInliningEnabled )
                {
                    // move styles inline to help it be compatible with more email clients
                    body = body.ConvertHtmlStylesToInlineAttributes();
                }
            }

            recipientEmail.Message = body;

            Guid? recipientGuid = null;
            recipientEmail.SystemCommunicationId = emailMessage.SystemCommunicationId;
            recipientEmail.CreateCommunicationRecord = emailMessage.CreateCommunicationRecord;

            // Headers

            if ( emailMessage.CreateCommunicationRecord )
            {
                recipientGuid = Guid.NewGuid();
                recipientEmail.MessageMetaData["communication_recipient_guid"] = recipientGuid.ToString();
            }

            return recipientEmail;
        }

        private RockEmailMessage GetRecipientRockEmailMessage( RockEmailMessage emailMessage, Model.Communication communication, CommunicationRecipient communicationRecipient, Dictionary<string, object> mergeFields, string organizationEmail, Dictionary<string, string> mediumAttributes )
        {
            var recipientEmail = new RockEmailMessage();
            recipientEmail.CurrentPerson = emailMessage.CurrentPerson;
            recipientEmail.EnabledLavaCommands = emailMessage.EnabledLavaCommands;
            recipientEmail.AppRoot = emailMessage.AppRoot;
            recipientEmail.CssInliningEnabled = emailMessage.CssInliningEnabled;

            // CC
            if ( communication.CCEmails.IsNotNullOrWhiteSpace() )
            {
                string[] ccRecipients = communication
                    .CCEmails
                    .ResolveMergeFields( mergeFields, emailMessage.CurrentPerson )
                    .Replace( ";", "," )
                    .Split( ',' );

                foreach ( var ccRecipient in ccRecipients )
                {
                    recipientEmail.CCEmails.Add( ccRecipient );
                }
            }

            // BCC
            if ( communication.BCCEmails.IsNotNullOrWhiteSpace() )
            {
                string[] bccRecipients = communication
                    .BCCEmails
                    .ResolveMergeFields( mergeFields, emailMessage.CurrentPerson )
                    .Replace( ";", "," )
                    .Split( ',' );

                foreach ( var bccRecipient in bccRecipients )
                {
                    recipientEmail.BCCEmails.Add( bccRecipient );
                }
            }

            // Attachments
            recipientEmail.Attachments = emailMessage.Attachments;

            // Communication record for tracking opens & clicks
            recipientEmail.MessageMetaData = new Dictionary<string, string>( emailMessage.MessageMetaData );

            // To
            var toEmailAddress = new RockEmailMessageRecipient( null, null )
            {
                To = communicationRecipient.PersonAlias.Person.Email,
                Name = communicationRecipient.PersonAlias.Person.FullName
            };

            recipientEmail.SetRecipients( new List<RockEmailMessageRecipient> { toEmailAddress } );

            var fromMailAddress = new MailAddress( emailMessage.FromEmail, emailMessage.FromName );
            var checkResult = CheckSafeSender( new List<string> { toEmailAddress.EmailAddress }, fromMailAddress, organizationEmail );

            // Reply To
            recipientEmail.ReplyToEmail = GetRecipientReplyToAddress( emailMessage, mergeFields, checkResult );

            // From
            if ( checkResult.IsUnsafeDomain && checkResult.SafeFromAddress != null )
            {
                recipientEmail.FromName = checkResult.SafeFromAddress.DisplayName;
                recipientEmail.FromEmail = checkResult.SafeFromAddress.Address;
            }
            else
            {
                recipientEmail.FromName = fromMailAddress.DisplayName;
                recipientEmail.FromEmail = fromMailAddress.Address;
            }

            // Subject
            var subject = ResolveText( communication.Subject, emailMessage.CurrentPerson, communication.EnabledLavaCommands, mergeFields, emailMessage.AppRoot );
            recipientEmail.Subject = subject.IsNullOrWhiteSpace() ? subject : Regex.Replace( subject, @"(\r?\n?)", String.Empty );

            // Body Plain Text
            if ( mediumAttributes.ContainsKey( "DefaultPlainText" ) )
            {
                var plainText = ResolveText(
                    mediumAttributes["DefaultPlainText"],
                    emailMessage.CurrentPerson,
                    communication.EnabledLavaCommands,
                    mergeFields,
                    emailMessage.AppRoot );

                if ( !string.IsNullOrWhiteSpace( plainText ) )
                {
                    recipientEmail.PlainTextMessage = plainText;
                }
            }

            // Body (HTML)
            string htmlBody = communication.Message;

            // Get the unsubscribe content and add a merge field for it
            if ( communication.IsBulkCommunication && mediumAttributes.ContainsKey( "UnsubscribeHTML" ) )
            {
                string unsubscribeHtml = ResolveText(
                    mediumAttributes["UnsubscribeHTML"],
                    emailMessage.CurrentPerson,
                    communication.EnabledLavaCommands,
                    mergeFields,
                    emailMessage.AppRoot );

                mergeFields.AddOrReplace( "UnsubscribeOption", unsubscribeHtml );

                htmlBody = ResolveText( htmlBody, emailMessage.CurrentPerson, communication.EnabledLavaCommands, mergeFields, emailMessage.AppRoot );

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
                htmlBody = ResolveText( htmlBody, emailMessage.CurrentPerson, communication.EnabledLavaCommands, mergeFields, emailMessage.AppRoot );
                htmlBody = Regex.Replace( htmlBody, @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty );
            }

            if ( !string.IsNullOrWhiteSpace( htmlBody ) )
            {
                if ( emailMessage.CssInliningEnabled )
                {
                    // move styles inline to help it be compatible with more email clients
                    htmlBody = htmlBody.ConvertHtmlStylesToInlineAttributes();
                }

                // add the main Html content to the email
                recipientEmail.Message = htmlBody;
            }

            // Headers
            var globalAttributes = GlobalAttributesCache.Get();

            // communication_recipient_guid
            recipientEmail.MessageMetaData["communication_recipient_guid"] = communicationRecipient.Guid.ToString();

            // List-Unsubscribe & List-Unsubscribe-Post
            AddListUnsubscribeEmailHeaders( recipientEmail, communication, communicationRecipient, mediumAttributes, globalAttributes, mergeFields );

            // List-Id
            AddListIdEmailHeaders( recipientEmail, communication, globalAttributes );

            return recipientEmail;
        }

        private void AddListIdEmailHeaders( RockEmailMessage recipientEmail, Rock.Model.Communication communication, GlobalAttributesCache globalAttributes )
        {
            const string ListIdHeaderKey = "List-Id";
            var hostDomain = new UriBuilder( globalAttributes.GetValue( "PublicApplicationRoot" ) ).Host;
            
            if ( communication.ListGroupId.HasValue )
            {
                recipientEmail.MessageMetaData[ListIdHeaderKey] = $"{communication.ListGroup?.Name ?? globalAttributes.GetValue( "OrganizationName" )} <{communication.ListGroupId.Value}.{hostDomain}>";
            }
            else
            {
                recipientEmail.MessageMetaData[ListIdHeaderKey] = $"{globalAttributes.GetValue( "OrganizationName" )} <general.{hostDomain}>";
            }
        }

        private void AddListUnsubscribeEmailHeaders( RockEmailMessage recipientEmail, Rock.Model.Communication communication, CommunicationRecipient communicationRecipient, IDictionary<string, string> mediumAttributes, GlobalAttributesCache globalAttributes, IDictionary<string, object> mergeFields )
        {
            var listUnsubscribeHeaderValues = new List<string>();

            var unsubscribeEmail = mediumAttributes.GetValueOrNull( "RequestUnsubscribeEmail" );
            if ( unsubscribeEmail.IsNullOrWhiteSpace() )
            {
                // Default to the organization email if the RequestUnsubscribeEmail attribute value is missing or white space.
                unsubscribeEmail = globalAttributes.GetValue( "OrganizationEmail" );
            }
            if ( unsubscribeEmail.IsNotNullOrWhiteSpace() )
            {
                listUnsubscribeHeaderValues.Add( $"<mailto:{unsubscribeEmail}>" );
            }

            var isOneClickUnsubscribeEnabled = mediumAttributes.GetValueOrNull( "EnableOneClickUnsubscribe" ).AsBoolean();
            if ( isOneClickUnsubscribeEnabled )
            {
                // If Enable One-Click Unsubscribe is true, then the URL should be the People API Unsubscribe endpoint.
                var personActionIdentifier = communicationRecipient.PersonAlias.Person.GetPersonActionIdentifier( "Unsubscribe" );
                var rootUrl = globalAttributes.GetValue( "PublicApplicationRoot" ).RemoveTrailingForwardslash();
                var queryParams = new List<string>();

                if ( communication?.ListGroupId.HasValue == true )
                {
                    queryParams.Add( $"communicationListIdKey={communication.ListGroup?.IdKey ?? communication.ListGroupId.ToString()}" );
                }

                if ( communication != null )
                {
                    queryParams.Add( $"communicationIdKey={communication.IdKey}" );
                }

                var queryString = string.Empty;
                if ( queryParams.Any() )
                {
                    queryString = $"?{string.Join( "&", queryParams )}";
                }

                listUnsubscribeHeaderValues.Add( $"<{rootUrl}/api/People/OneClickUnsubscribe/{personActionIdentifier}{queryString}>" );
            }
            else if ( mediumAttributes.TryGetValue( "UnsubscribeURL", out var unsubscribeUrl )
                      && unsubscribeUrl.IsNotNullOrWhiteSpace() )
            {
                // If Enable One-Click Unsubscribe is false, then use the UnsubscribeURL value.
                var httpValue = unsubscribeUrl.ResolveMergeFields( mergeFields, recipientEmail.CurrentPerson, recipientEmail.EnabledLavaCommands );
                if ( httpValue.IsNotNullOrWhiteSpace() )
                {
                    listUnsubscribeHeaderValues.Add( $"<{httpValue}>" );
                }
            }

            if ( listUnsubscribeHeaderValues.Any() )
            {
                recipientEmail.MessageMetaData["List-Unsubscribe"] = string.Join( ", ", listUnsubscribeHeaderValues );

                // Only add the post header if the List-Unsubscribe header is added.
                if ( isOneClickUnsubscribeEnabled )
                {
                    recipientEmail.MessageMetaData["List-Unsubscribe-Post"] = "List-Unsubscribe=One-Click";
                }
            }
        }

        /// <summary>
        /// Returns the communication only if it is okay to send.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private Model.Communication GetSendableCommunication( int communicationId, RockContext rockContext )
        {
            var communication = new CommunicationService( rockContext )
                    .Queryable()
                    .Include( a => a.CreatedByPersonAlias.Person )
                    .Include( a => a.CommunicationTemplate )
                    .FirstOrDefault( c => c.Id == communicationId );

            var isApprovedCommunication = communication != null && communication.Status == Model.CommunicationStatus.Approved;
            var isReadyToSend = communication != null &&
                                    ( !communication.FutureSendDateTime.HasValue
                                    || communication.FutureSendDateTime.Value.CompareTo( RockDateTime.Now ) <= 0 );

            if ( !isApprovedCommunication || !isReadyToSend )
            {
                return null;
            }

            return communication;
        }

        /// <summary>
        /// Gets the next pending recipient from the communication.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        /// <param name="mediumEntityId">The medium entity identifier.</param>
        /// <param name="isBulkCommunication">if set to <c>true</c> [is bulk communication].</param>
        /// <returns></returns>
        private Rock.Model.CommunicationRecipient GetNextPending( int communicationId, int mediumEntityId, bool isBulkCommunication )
        {
            using ( var rockContext = new RockContext() )
            {
                var recipient = Model.Communication.GetNextPending( communicationId, mediumEntityId, rockContext );
                if ( ValidRecipient( recipient, isBulkCommunication ) )
                {
                    return recipient;
                }
                else
                {
                    rockContext.SaveChanges();
                    return GetNextPending( communicationId, mediumEntityId, isBulkCommunication );
                }
            }
        }

        /// <summary>
        /// Sends to the communication recipient asynchronous.
        /// </summary>
        /// <param name="recipientId">The recipient identifier.</param>
        /// <param name="communication">The communication.</param>
        /// <param name="mediumEntityTypeId">The medium entity type identifier.</param>
        /// <param name="mediumAttributes">The medium attributes.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="templateEmailMessage">The template email message.</param>
        /// <param name="organizationEmail">The organization email.</param>
        private async Task SendToRecipientAsync(
           int recipientId,
           Model.Communication communication,
           int mediumEntityTypeId,
           Dictionary<string, string> mediumAttributes,
           Dictionary<string, object> mergeFields,
           RockEmailMessage templateEmailMessage,
           string organizationEmail )
        {
            var methodTimer = System.Diagnostics.Stopwatch.StartNew();
            using ( var rockContext = new RockContext() )
            {
                var recipient = new CommunicationRecipientService( rockContext ).Get( recipientId );

                if ( recipient == null )
                {
                    return;
                }

                try
                {
                    // Create merge field dictionary
                    var mergeObjects = recipient.CommunicationMergeValues( mergeFields );
                    var recipientEmailMessage = GetRecipientRockEmailMessage( templateEmailMessage, communication, recipient, mergeObjects, organizationEmail, mediumAttributes );

                    var result = await SendEmailAsync( recipientEmailMessage ).ConfigureAwait( false );

                    // Update recipient status and status note
                    recipient.Status = result.Status;
                    if ( result.Status == CommunicationRecipientStatus.Failed )
                    {
                        recipient.StatusNote = result.StatusNote;
                    }

                    recipient.TransportEntityTypeName = this.GetType().FullName;

                    // Log it
                    try
                    {
                        var historyChangeList = new History.HistoryChangeList();
                        var communicationEntityTypeId = EntityTypeCache.Get<Model.Communication>().Id;
                        var communicationCategoryGuid = Rock.SystemGuid.Category.HISTORY_PERSON_COMMUNICATIONS.AsGuid();

                        historyChangeList.AddChange(
                            History.HistoryVerb.Sent,
                            History.HistoryChangeType.Record,
                            $"Communication" )
                            .SetRelatedData( recipientEmailMessage.FromName, communicationEntityTypeId, communication.Id )
                            .SetCaption( recipientEmailMessage.Subject );

                        HistoryService.SaveChanges( rockContext, typeof( Rock.Model.Person ), communicationCategoryGuid, recipient.PersonAlias.PersonId, historyChangeList, false, communication.SenderPersonAliasId );
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

                rockContext.SaveChanges();
            }

            RockLogger.Log.Debug( RockLogDomains.Communications, "{0}: Took {1} ticks to retrieve and send the email.", nameof( SendToRecipientAsync ), methodTimer.ElapsedTicks );
        }

        /// <summary>
        /// Handles the exception response.
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <returns></returns>
        private SendMessageResult HandleExceptionResponse( Exception ex )
        {
            var sendResult = new SendMessageResult();
            sendResult.Errors.Add( ex.Message );
            ExceptionLogService.LogException( ex );
            return sendResult;
        }

        /// <summary>
        /// Handles the email send response.
        /// </summary>
        /// <param name="rockMessageRecipient">The rock message recipient.</param>
        /// <param name="recipientEmailMessage">The recipient email message.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        private SendMessageResult HandleEmailSendResponse( RockMessageRecipient rockMessageRecipient, RockEmailMessage recipientEmailMessage, EmailSendResponse result )
        {
            var sendResult = new SendMessageResult();
            if ( result.Status == CommunicationRecipientStatus.Failed )
            {
                sendResult.Errors.Add( result.StatusNote );
            }
            else
            {
                sendResult.MessagesSent = 1;
            }

            // Create the communication record
            if ( recipientEmailMessage.CreateCommunicationRecord )
            {
                var transaction = new SaveCommunicationTransaction(
                    rockMessageRecipient,
                    recipientEmailMessage.FromName,
                    recipientEmailMessage.FromEmail,
                    recipientEmailMessage.Subject,
                    recipientEmailMessage.Message );

                transaction.SystemCommunicationId = recipientEmailMessage.SystemCommunicationId;

                transaction.RecipientGuid = recipientEmailMessage.MessageMetaData["communication_recipient_guid"].AsGuidOrNull();
                transaction.RecipientStatus = result.Status;
                transaction.Enqueue();
            }

            return sendResult;
        }

        /// <summary>
        /// Sends the email to recipient asynchronous.
        /// </summary>
        /// <param name="templateMailMessage">The template mail message.</param>
        /// <param name="rockMessageRecipient">The rock message recipient.</param>
        /// <param name="mergeFields">The merge fields.</param>
        /// <param name="organizationEmail">The organization email.</param>
        /// <returns></returns>
        private async Task<SendMessageResult> SendEmailToRecipientAsync( RockEmailMessage templateMailMessage, RockMessageRecipient rockMessageRecipient, Dictionary<string, object> mergeFields, string organizationEmail )
        {
            try
            {
                var recipientEmailMessage = GetRecipientRockEmailMessage( templateMailMessage, rockMessageRecipient, mergeFields, organizationEmail );

                var result = await SendEmailAsync( recipientEmailMessage ).ConfigureAwait( false );

                return HandleEmailSendResponse( rockMessageRecipient, recipientEmailMessage, result );
            }
            catch ( Exception ex )
            {
                return HandleExceptionResponse( ex );
            }
        }
    }
}
