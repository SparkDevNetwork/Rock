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
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Rock.Communication.Transport;
using Rock.Data;
using Rock.Model;
using Rock.Transactions;
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
        /// Send the implementation specific email. This class will call this method and pass the post processed data in a  rock email message which
        /// can then be used to send the implementation specific message.
        /// </summary>
        /// <param name="rockEmailMessage">The rock email message.</param>
        /// <returns></returns>
        protected abstract EmailSendResponse SendEmail( RockEmailMessage rockEmailMessage );

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

            var templateMailMessage = GetTemplateRockEmailMessage( emailMessage, mergeFields, globalAttributes );
            var organizationEmail = globalAttributes.GetValue( "OrganizationEmail" );

            foreach ( var rockMessageRecipient in rockMessage.GetRecipients() )
            {
                try
                {
                    var recipientEmailMessage = GetRecipientRockEmailMessage( templateMailMessage, rockMessageRecipient, mergeFields, organizationEmail );

                    var result = SendEmail( recipientEmailMessage );

                    // Create the communication record
                    if ( recipientEmailMessage.CreateCommunicationRecord )
                    {
                        var transaction = new SaveCommunicationTransaction( rockMessageRecipient, recipientEmailMessage.FromName, recipientEmailMessage.FromEmail, recipientEmailMessage.Subject, recipientEmailMessage.Message );
                        transaction.RecipientGuid = recipientEmailMessage.MessageMetaData["communication_recipient_guid"].AsGuidOrNull();
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

                var templateEmailMessage = GetTemplateRockEmailMessage( communication, mergeFields, globalAttributes );
                var organizationEmail = globalAttributes.GetValue( "OrganizationEmail" );

                var publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();

                var cssInliningEnabled = communication.CommunicationTemplate?.CssInliningEnabled ?? false;

                var personEntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
                var communicationEntityTypeId = EntityTypeCache.Get( "Rock.Model.Communication" ).Id;
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
            return "";
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

        private RockEmailMessage GetTemplateRockEmailMessage( RockMessage rockMessage, Dictionary<string, object> mergeFields, GlobalAttributesCache globalAttributes )
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
            templateRockEmailMessage.ReplyToEmail = emailMessage.ReplyToEmail;

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

        private RockEmailMessage GetTemplateRockEmailMessage( Model.Communication communication, Dictionary<string, object> mergeFields, GlobalAttributesCache globalAttributes )
        {
            var resultEmailMessage = new RockEmailMessage();

            var publicAppRoot = globalAttributes.GetValue( "PublicApplicationRoot" ).EnsureTrailingForwardslash();
            var cssInliningEnabled = communication.CommunicationTemplate?.CssInliningEnabled ?? false;

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
            var replyToEmail = "";
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
            string subject = ResolveText( emailMessage.Subject, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, rockMessageRecipient.MergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot ).Left( 998 );
            recipientEmail.Subject = subject;

            // Plain Text Message
            recipientEmail.PlainTextMessage = ResolveText( emailMessage.PlainTextMessage, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, rockMessageRecipient.MergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );

            // Body (HTML)
            string body = ResolveText( emailMessage.Message, emailMessage.CurrentPerson, emailMessage.EnabledLavaCommands, rockMessageRecipient.MergeFields, emailMessage.AppRoot, emailMessage.ThemeRoot );
            body = Regex.Replace( body, @"\[\[\s*UnsubscribeOption\s*\]\]", string.Empty );
            recipientEmail.Message = body;


            Guid? recipientGuid = null;
            recipientEmail.CreateCommunicationRecord = emailMessage.CreateCommunicationRecord;
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
            recipientEmail.Subject = subject;

            // Body Plain Text
            if ( mediumAttributes.ContainsKey( "DefaultPlainText" ) )
            {
                var plainText = ResolveText( mediumAttributes["DefaultPlainText"],
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
                string unsubscribeHtml = ResolveText( mediumAttributes["UnsubscribeHTML"],
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

            recipientEmail.MessageMetaData["communication_recipient_guid"] = communicationRecipient.Guid.ToString();

            return recipientEmail;
        }

    }
}
