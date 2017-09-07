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
        [Obsolete( "Use method without textMessage argument")]
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
            var recipients = new PersonService( (RockContext)this.Context )
                .Queryable()
                .Where( p => recipientEmails.Contains( p.Email ) )
                .ToList();

            if ( recipients.Any() )
            {
                Rock.Model.Communication communication = new Rock.Model.Communication();
                Add( communication );

                communication.CommunicationType = CommunicationType.Email;
                communication.Status = CommunicationStatus.Approved;
                communication.SenderPersonAliasId = senderPersonAliasId;
                communication.FromName = fromName;
                communication.FromEmail = fromAddress;
                communication.ReplyToEmail = replyTo;
                communication.Subject = subject;
                communication.Message = message;
                communication.IsBulkCommunication = bulkCommunication;
                communication.FutureSendDateTime = null;

                // add each person as a recipient to the communication
                foreach ( var person in recipients )
                {
                    int? personAliasId = person.PrimaryAliasId;
                    if ( personAliasId.HasValue )
                    {
                        var communicationRecipient = new CommunicationRecipient();
                        communicationRecipient.PersonAliasId = personAliasId.Value;
                        communicationRecipient.Status = recipientStatus;
                        communication.Recipients.Add( communicationRecipient );
                    }
                }

                return communication;
            }

            return null;
        }


        /// <summary>
        /// Gets the queued communications
        /// </summary>
        /// <param name="expirationDays">The expiration days.</param>
        /// <param name="delayMinutes">The delay minutes.</param>
        /// <param name="includeFuture">if set to <c>true</c> [include future].</param>
        /// <param name="includePending">if set to <c>true</c> [include pending].</param>
        /// <returns></returns>
        public IQueryable<Communication> GetQueued( int expirationDays, int delayMinutes, bool includeFuture, bool includePending )
        {
            var beginWindow = RockDateTime.Now.AddDays( 0 - expirationDays );
            var endWindow = RockDateTime.Now.AddMinutes( 0 - delayMinutes );
            var nowDate = RockDateTime.Now;

            var qryPendingRecipients = new CommunicationRecipientService( (RockContext)Context )
                .Queryable()
                .Where( a => a.Status == CommunicationRecipientStatus.Pending );

            return Queryable()
                .Where( c =>
                    ( c.Status == CommunicationStatus.Approved || ( includePending && c.Status == CommunicationStatus.PendingApproval ) ) &&
                    qryPendingRecipients.Where( r => r.CommunicationId == c.Id ).Any() &&
                    (
                        ( !c.FutureSendDateTime.HasValue && c.CreatedDateTime.HasValue && c.CreatedDateTime.Value.CompareTo( beginWindow ) >= 0 && c.CreatedDateTime.Value.CompareTo( endWindow ) <= 0 ) ||
                        ( c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value.CompareTo( beginWindow ) >= 0 && ( includeFuture || c.FutureSendDateTime.Value.CompareTo( nowDate ) <= 0 ) )
                    ) );
        }

    }
}
