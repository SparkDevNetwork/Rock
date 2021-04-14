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
using System.Linq;
using Rock.Communication;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Communication POCO Service class
    /// </summary>
    public partial class CommunicationService
    {
        /// <summary>
        /// Creates the email communication 
        /// </summary>
        /// <param name="recipientEmails">A list of email addresses to use for finding which people to send the email to</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="replyTo">The reply to.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <param name="bulkCommunication">if set to <c>true</c> [bulk communication].</param>
        /// <param name="sendDateTime">The send date time.</param>
        /// <param name="recipientStatus">The recipient status.</param>
        /// <param name="senderPersonAliasId">The sender person alias identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.10" )]
        [Obsolete( "This has a issue where the wrong person(s) might be logged as the recipient. Use the CreateEmailCommunication method that takes List<RockEmailMessageRecipient> as a parameter instead." )]
        public Communication CreateEmailCommunication
        (
            List<string> recipientEmails,
            string fromName,
            string fromAddress,
            string replyTo,
            string subject,
            string message,
            bool bulkCommunication,
            DateTime? sendDateTime,
            CommunicationRecipientStatus recipientStatus = CommunicationRecipientStatus.Delivered,
            int? senderPersonAliasId = null )
        {
            var recipients = new PersonService( ( RockContext ) Context )
                .Queryable()
                .Where( p => recipientEmails.Contains( p.Email ) )
                .ToList();

            return this.CreateEmailCommunication( recipients.Select( a => new RockEmailMessageRecipient( a, null ) ).ToList(), fromName, fromAddress, replyTo, subject, message, bulkCommunication, sendDateTime, recipientStatus, senderPersonAliasId );
        }

        /// <summary>
        /// Creates the email communication
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="replyTo">The reply to.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <param name="bulkCommunication">if set to <c>true</c> [bulk communication].</param>
        /// <param name="sendDateTime">The send date time.</param>
        /// <param name="recipientStatus">The recipient status.</param>
        /// <param name="senderPersonAliasId">The sender person alias identifier.</param>
        /// <returns></returns>
        public Communication CreateEmailCommunication
        (
            List<RockEmailMessageRecipient> recipients,
            string fromName,
            string fromAddress,
            string replyTo,
            string subject,
            string message,
            bool bulkCommunication,
            DateTime? sendDateTime,
            CommunicationRecipientStatus recipientStatus = CommunicationRecipientStatus.Delivered,
            int? senderPersonAliasId = null )
        {
            var recipientsWithPersonIds = recipients.Where( a => a.PersonId.HasValue ).Select( a => a.PersonId ).ToList();
            var recipientEmailsUnknownPersons = recipients.Where( a => a.PersonId == null ).Select( a => a.EmailAddress );

            var recipientPersonList = new PersonService( ( RockContext ) Context )
                .Queryable()
                .Where( p => recipientsWithPersonIds.Contains( p.Id ) )
                .ToList();

            if ( !recipientPersonList.Any() && recipientEmailsUnknownPersons.Any( a => a != null ) )
            {
                // For backwards compatibility, if no PersonIds where specified, but there are recipients that are only specified by EmailAddress, take a guess at the personIds by looking for matching email addresses
                recipientPersonList = new PersonService( ( RockContext ) Context )
                .Queryable()
                .Where( p => recipientEmailsUnknownPersons.Contains( p.Email ) )
                .ToList();
            }

            if ( !recipientPersonList.Any() )
            {
                return null;
            }

            var communication = new Communication
            {
                CommunicationType = CommunicationType.Email,
                Status = CommunicationStatus.Approved,
                ReviewedDateTime = RockDateTime.Now,
                ReviewerPersonAliasId = senderPersonAliasId,
                SenderPersonAliasId = senderPersonAliasId
            };

            communication.FromName = fromName.TrimForMaxLength( communication, "FromName" );
            communication.FromEmail = fromAddress.TrimForMaxLength( communication, "FromEmail" );
            communication.ReplyToEmail = replyTo.TrimForMaxLength( communication, "ReplyToEmail" );
            communication.Subject = subject.TrimForMaxLength( communication, "Subject" );
            communication.Message = message;
            communication.IsBulkCommunication = bulkCommunication;
            communication.FutureSendDateTime = null;
            communication.SendDateTime = sendDateTime;
            Add( communication );

            // add each person as a recipient to the communication
            foreach ( var person in recipientPersonList )
            {
                var personAliasId = person.PrimaryAliasId;
                if ( !personAliasId.HasValue )
                    continue;

                var communicationRecipient = new CommunicationRecipient
                {
                    PersonAliasId = personAliasId.Value,
                    Status = recipientStatus,
                    SendDateTime = sendDateTime
                };
                communication.Recipients.Add( communicationRecipient );
            }

            return communication;

        }

        /// <summary>
        /// Creates an SMS communication with a CommunicationRecipient and adds it to the context.
        /// </summary>
        /// <param name="fromPerson">the Sender for the communication (For the communication.SenderPersonAlias). If null the name for the communication will be From: unknown person.</param>
        /// <param name="toPersonAliasId">To person alias identifier. If null the CommunicationRecipient is not created</param>
        /// <param name="message">The message.</param>
        /// <param name="fromPhone">From phone.</param>
        /// <param name="responseCode">The response code. If null/empty/whitespace then one is generated</param>
        /// <param name="communicationName">Name of the communication.</param>
        /// <returns></returns>
        public Communication CreateSMSCommunication( Person fromPerson, int? toPersonAliasId, string message, DefinedValueCache fromPhone, string responseCode, string communicationName )
        {
            RockContext rockContext = ( RockContext ) this.Context;

            if ( responseCode.IsNullOrWhiteSpace() )
            {
                responseCode = Rock.Communication.Medium.Sms.GenerateResponseCode( rockContext );
            }

            // add communication for reply
            var communication = new Rock.Model.Communication
            {
                Name = communicationName,
                CommunicationType = CommunicationType.SMS,
                Status = CommunicationStatus.Approved,
                ReviewedDateTime = RockDateTime.Now,
                // NOTE: if this communication was created from a mobile device, fromPerson should never be null since a Nameless Person record should have been created if a regular person record wasn't found
                ReviewerPersonAliasId = fromPerson?.PrimaryAliasId,
                SenderPersonAliasId = fromPerson?.PrimaryAliasId,
                IsBulkCommunication = false,
                SMSMessage = message,
                SMSFromDefinedValueId = fromPhone.Id
            };
            
            if ( toPersonAliasId != null )
            {
                var recipient = new Rock.Model.CommunicationRecipient();
                recipient.Status = CommunicationRecipientStatus.Pending;
                recipient.PersonAliasId = toPersonAliasId.Value;
                recipient.ResponseCode = responseCode;
                recipient.MediumEntityTypeId = EntityTypeCache.Get( "Rock.Communication.Medium.Sms" ).Id;
                recipient.SentMessage = message;
                communication.Recipients.Add( recipient );
            }

            Add( communication );
            return communication;
        }

        /// <summary>
        /// Gets the queued communications.
        /// </summary>
        /// <param name="expirationDays">The expiration days.</param>
        /// <param name="delayMinutes">The delay minutes.</param>
        /// <param name="includeFuture">if set to <c>true</c> [include future].</param>
        /// <param name="includePendingApproval">if set to <c>true</c> communications that haven't been approved yet will be included.</param>
        /// <returns></returns>
        public IQueryable<Communication> GetQueued( int expirationDays, int delayMinutes, bool includeFuture, bool includePendingApproval )
        {
            var beginWindow = RockDateTime.Now.AddDays( 0 - expirationDays );
            var endWindow = RockDateTime.Now.AddMinutes( 0 - delayMinutes );
            var currentDateTime = RockDateTime.Now;

            // Conditions for communications that should be queued for Sending (indicated by includeFuture == false and includePending == false)
            // -  communications that haven't been sent yet (SendDateTime is null)
            // -  communication is approved (or includePendingApproval == false)
            // - FutureSendDateTime is not set (not scheduled), and communication was created within a reasonable window based on expiration days (for example, no older than 3 days ago)
            //   - OR - FutureSendDateTime IS set (scheduled), and the FutureSendDateTime is Now (or within the expiration window)

            // Limit to communications that haven't been sent yet
            var queuedQry = Queryable().Where( c => !c.SendDateTime.HasValue );

            var qryPendingRecipients = new CommunicationRecipientService( ( RockContext ) Context )
                .Queryable()
                .Where( a => a.Status == CommunicationRecipientStatus.Pending )
                .Select( cr => new { Id = cr.CommunicationId } );

            if ( includePendingApproval )
            {
                // Also limit to communications that are Approved or Pending Approval
                queuedQry = queuedQry.Where( c => c.Status == CommunicationStatus.Approved || c.Status == CommunicationStatus.PendingApproval );
            }
            else
            {
                // Also limit to communications that are Approved 
                queuedQry = queuedQry.Where( c => c.Status == CommunicationStatus.Approved );
            }

            if ( includeFuture )
            {
                // Also limit to communications that have either been created within a reasonable timeframe (typically no older than 3 days ago) or are Scheduled to be sent at some point
                queuedQry = queuedQry.Where( c =>
                    // Use the reviewed date to get the communications to send
                    ( !c.FutureSendDateTime.HasValue && c.ReviewedDateTime.HasValue && c.ReviewedDateTime.Value >= beginWindow && c.ReviewedDateTime.Value <= endWindow )
                    // Use the created date to get the communications to send this is for communications that are created by legacy plugins that have switched to using reviewed date.
                    || ( !c.FutureSendDateTime.HasValue && !c.ReviewedDateTime.HasValue && c.CreatedDateTime.HasValue && c.CreatedDateTime.Value >= beginWindow && c.CreatedDateTime.Value <= endWindow )
                    // Get all future communications.
                    || ( c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value >= beginWindow ) );
            }
            else
            {
                // Also limit to communications that have either been created within a reasonable timeframe (typically no older than 3 days ago)
                // or are Scheduled to be sent (also within that reasonable timeframe. In other words, if it was scheduled to be sent, but stil hasn't been sent 3 days after it was scheduled, don't include it)
                queuedQry = queuedQry.Where( c =>
                    // Use the reviewed date to get the communications to send
                    ( !c.FutureSendDateTime.HasValue && c.ReviewedDateTime.HasValue && c.ReviewedDateTime.Value >= beginWindow && c.ReviewedDateTime.Value <= endWindow )
                    // Use the created date to get the communications to send this is for communications that are created by legacy plugins that have switched to using reviewed date.
                    || ( !c.FutureSendDateTime.HasValue && !c.ReviewedDateTime.HasValue && c.CreatedDateTime.HasValue && c.CreatedDateTime.Value >= beginWindow && c.CreatedDateTime.Value <= endWindow )
                    // Get all future communication that are need to be sent.
                    || ( c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value >= beginWindow && c.FutureSendDateTime.Value <= currentDateTime ) );
            }

            // just in case SendDateTime is null (pre-v8 communication), also limit to communications that either have a ListGroupId or has PendingRecipients
            var listGroupQuery = Queryable().Where( c => c.ListGroupId.HasValue ).Select( c => new { c.Id } );
            var communicationListQry = qryPendingRecipients.Union( listGroupQuery );

            var returnQry = Queryable()
                .Where( c => queuedQry.Any( c2 => c2.Id == c.Id ) )
                .Where( c => communicationListQry.Any( c2 => c2.Id == c.Id ) );
            return returnQry;
        }

        /// <summary>
        /// Copies the specified communication identifier.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        /// <param name="currentPersonAliasId">The current person alias identifier.</param>
        /// <returns></returns>
        public Communication Copy( int communicationId, int? currentPersonAliasId )
        {
            var dataContext = ( RockContext ) Context;

            var service = new CommunicationService( dataContext );
            var communicationRecipientService = new CommunicationRecipientService( dataContext );
            var communication = service.Get( communicationId );
            if ( communication != null )
            {
                var newCommunication = communication.Clone( false );
                newCommunication.CreatedByPersonAlias = null;
                newCommunication.CreatedByPersonAliasId = null;
                newCommunication.CreatedDateTime = RockDateTime.Now;
                newCommunication.ModifiedByPersonAlias = null;
                newCommunication.ModifiedByPersonAliasId = null;
                newCommunication.ModifiedDateTime = RockDateTime.Now;
                newCommunication.Id = 0;
                newCommunication.Guid = Guid.Empty;
                newCommunication.SenderPersonAliasId = currentPersonAliasId;
                newCommunication.Status = CommunicationStatus.Draft;
                newCommunication.ReviewerPersonAliasId = null;
                newCommunication.ReviewedDateTime = null;
                newCommunication.ReviewerNote = string.Empty;
                newCommunication.SendDateTime = null;

                // Get the recipients from the original communication,
                // but only for recipients that are using the person's primary alias id.
                // This will avoid an issue where a copied communication will include the same person multiple times
                // if they have been merged since the original communication was created
                var primaryAliasRecipients = communicationRecipientService.Queryable()
                    .Where( a => a.CommunicationId == communication.Id )
                    .Select( a => new
                    {
                        a.PersonAlias.Person,
                        a.AdditionalMergeValuesJson,
                        a.PersonAliasId
                    } ).ToList()
                    .GroupBy( a => a.Person.PrimaryAliasId )
                    .Select( s => new
                    {
                        PersonAliasId = s.Key,
                        AdditionalMergeValuesJson = s.Where( a => a.PersonAliasId == s.Key ).Select( x => x.AdditionalMergeValuesJson ).FirstOrDefault()
                    } )
                    .Where( s => s.PersonAliasId.HasValue )
                    .ToList();

                foreach ( var primaryAliasRecipient in primaryAliasRecipients )
                {
                    newCommunication.Recipients.Add( new CommunicationRecipient()
                    {
                        PersonAliasId = primaryAliasRecipient.PersonAliasId.Value,
                        Status = CommunicationRecipientStatus.Pending,
                        StatusNote = string.Empty,
                        AdditionalMergeValuesJson = primaryAliasRecipient.AdditionalMergeValuesJson
                    } );
                }

                foreach ( var attachment in communication.Attachments.ToList() )
                {
                    var newAttachment = new CommunicationAttachment();
                    newAttachment.BinaryFileId = attachment.BinaryFileId;
                    newAttachment.CommunicationType = attachment.CommunicationType;
                    newCommunication.Attachments.Add( newAttachment );
                }

                return newCommunication;
            }

            return null;
        }
    }

    #region Extension Methods

    public static partial class CommunicationExtensionMethods
    {
        /// <summary>
        /// Communications that were not recently approved within since the given cutoffTime.
        /// </summary>
        /// <param name="communications">The communications.</param>
        /// <param name="cutoffTime">The cutoff time.</param>
        /// <returns></returns>
        public static IQueryable<Communication> NotRecentlyApproved( this IQueryable<Communication> communications, DateTime cutoffTime )
        {
            // Make sure communication wasn't just recently approved
            return communications.Where( c => !c.ReviewedDateTime.HasValue || c.ReviewedDateTime.Value < cutoffTime );
        }

        /// <summary>
        /// Communications that were, if scheduled, are within the given window.
        /// </summary>
        /// <param name="communications">The communications.</param>
        /// <param name="startWindow">The start window.</param>
        /// <param name="endWindow">The end window.</param>
        /// <returns></returns>
        public static IQueryable<Communication> IfScheduledAreInWindow( this IQueryable<Communication> communications, DateTime startWindow, DateTime endWindow )
        {
            return communications.Where( c =>
                (
                    !c.FutureSendDateTime.HasValue ||
                    ( c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value >= startWindow && c.FutureSendDateTime.Value <= endWindow )
                )
            );
        }
    }

    #endregion
}