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
        /// Creates the email communication.
        /// </summary>
        /// <param name="recipientEmails">The recipient emails.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="replyTo">The reply to.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="htmlMessage">The HTML message.</param>
        /// <param name="textMessage">The text message.</param>
        /// <param name="bulkCommunication">if set to <c>true</c> [bulk communication].</param>
        /// <param name="recipientStatus">The recipient status.</param>
        /// <param name="senderPersonAliasId">The sender person alias identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use method without textMessage argument", true )]
        public Communication CreateEmailCommunication
        (
            List<string> recipientEmails,
            string fromName,
            string fromAddress,
            string replyTo,
            string subject,
            string htmlMessage,
            string textMessage,
            bool bulkCommunication,
            CommunicationRecipientStatus recipientStatus = CommunicationRecipientStatus.Delivered,
            int? senderPersonAliasId = null )
        {
            return CreateEmailCommunication( recipientEmails, fromName, fromAddress, replyTo, subject, htmlMessage, bulkCommunication, recipientStatus, senderPersonAliasId );
        }

        /// <summary>
        /// Creates the email communication.
        /// </summary>
        /// <param name="recipientEmails">The recipient emails.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="replyTo">The reply to.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        /// <param name="bulkCommunication">if set to <c>true</c> [bulk communication].</param>
        /// <param name="recipientStatus">The recipient status.</param>
        /// <param name="senderPersonAliasId">The sender person alias identifier.</param>
        /// <returns></returns>
        [RockObsolete( "1.7" )]
        [Obsolete( "Use method with send date time argument", true )]
        public Communication CreateEmailCommunication
        (
            List<string> recipientEmails,
            string fromName,
            string fromAddress,
            string replyTo,
            string subject,
            string message,
            bool bulkCommunication,
            CommunicationRecipientStatus recipientStatus = CommunicationRecipientStatus.Delivered,
            int? senderPersonAliasId = null )
        {
            DateTime? sendDateTime = null;
            if ( recipientStatus == CommunicationRecipientStatus.Delivered )
            {
                sendDateTime = RockDateTime.Now;
            }

            return CreateEmailCommunication( recipientEmails, fromName, fromAddress, replyTo, subject, message, bulkCommunication, sendDateTime, recipientStatus, senderPersonAliasId );
        }

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
                .Select( a => new RockEmailMessageRecipient( a, null ) )
                .ToList();

            return this.CreateEmailCommunication( recipients, fromName, fromAddress, replyTo, subject, message, bulkCommunication, sendDateTime, recipientStatus, senderPersonAliasId );
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
        /// <param name="fromPerson">From person. If null the name for the communication will be From: unknown person.</param>
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
            var communication = new Rock.Model.Communication();
            communication.Name = communicationName;
            communication.CommunicationType = CommunicationType.SMS;
            communication.SenderPersonAliasId = fromPerson?.PrimaryAliasId;
            communication.IsBulkCommunication = false;
            communication.Status = CommunicationStatus.Approved;
            communication.SMSMessage = message;
            communication.SMSFromDefinedValueId = fromPhone.Id;

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
                .Where( a => a.Status == CommunicationRecipientStatus.Pending );

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
                    ( !c.FutureSendDateTime.HasValue && c.CreatedDateTime.HasValue && c.CreatedDateTime.Value >= beginWindow && c.CreatedDateTime.Value <= endWindow )
                    || ( c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value >= beginWindow ) );
            }
            else
            {
                // Also limit to communications that have either been created within a reasonable timeframe (typically no older than 3 days ago)
                // or are Scheduled to be sent (also within that reasonable timeframe. In other words, if it was scheduled to be sent, but stil hasn't been sent 3 days after it was scheduled, don't include it)
                queuedQry = queuedQry.Where( c =>
                    ( !c.FutureSendDateTime.HasValue && c.CreatedDateTime.HasValue && c.CreatedDateTime.Value >= beginWindow && c.CreatedDateTime.Value <= endWindow )
                    || ( c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value >= beginWindow && c.FutureSendDateTime.Value <= currentDateTime ) );
            }

            // just in case SendDateTime is null (pre-v8 communication), also limit to communications that either have a ListGroupId or has PendingRecipients
            queuedQry = queuedQry.Where( c => c.ListGroupId.HasValue || qryPendingRecipients.Any( r => r.CommunicationId == c.Id ) );

            return queuedQry;
        }

    }
}
