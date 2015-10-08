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
using System.Linq;

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
        public Communication CreateEmailCommunication( 
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
            var recipients = new PersonService( (RockContext)this.Context )
                .Queryable()
                .Where( p => recipientEmails.Contains( p.Email ) )
                .ToList();

            if ( recipients.Any() )
            {
                Rock.Model.Communication communication = new Rock.Model.Communication();
                communication.Status = CommunicationStatus.Approved;
                communication.SenderPersonAliasId = senderPersonAliasId;
                communication.Subject = subject;
                Add( communication );

                communication.IsBulkCommunication = bulkCommunication;
                communication.MediumEntityTypeId = EntityTypeCache.Read( "Rock.Communication.Medium.Email" ).Id;
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

                // add the MediumData to the communication
                communication.MediumData.Clear();
                communication.MediumData.Add( "FromName", fromName );
                communication.MediumData.Add( "FromAddress", fromAddress );
                communication.MediumData.Add( "ReplyTo", replyTo );
                communication.MediumData.Add( "Subject", subject );
                communication.MediumData.Add( "HtmlMessage", htmlMessage );
                communication.MediumData.Add( "TextMessage", textMessage );

                return communication;
            }

            return null;
        }

    }
}
