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
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class AddCommunicationRecipients : BusStartedTask<AddCommunicationRecipients.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                int? senderPersonAliasId = null;
                if ( message.FromPersonId.HasValue )
                {
                    var sender = personService.GetNoTracking( message.FromPersonId.Value );
                    senderPersonAliasId = sender?.PrimaryAliasId;
                }
                else
                {
                    if ( message.FromAddress.IsNotNullOrWhiteSpace() )
                    {
                        var sender = personService
                            .Queryable().AsNoTracking()
                            .Where( p => p.Email == message.FromAddress )
                            .FirstOrDefault();
                        senderPersonAliasId = sender?.PrimaryAliasId;
                    }
                }

                Model.Communication communication;

                if ( message.Recipients?.Any() == true )
                {
                    communication = new CommunicationService( rockContext ).CreateEmailCommunication( message.Recipients, message.FromName, message.FromAddress, message.ReplyTo, message.Subject, message.HtmlMessage, message.BulkCommunication, message.SendDateTime, message.RecipientStatus, senderPersonAliasId );

                    if ( communication != null && communication.Recipients.Count() == 1 && message.RecipientGuid.HasValue )
                    {
                        communication.Recipients.First().Guid = message.RecipientGuid.Value;
                    }

                    rockContext.SaveChanges();
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the rock message recipients.
            /// </summary>
            /// <value>
            /// The rock message recipients.
            /// </value>
            public List<RockEmailMessageRecipient> Recipients { get; set; }

            /// <summary>
            /// Gets from PersonId of the person sending the email
            /// </summary>
            /// <value>
            /// From person identifier.
            /// </value>
            public int? FromPersonId { get; private set; }

            /// <summary>
            /// If <see cref="FromPersonId"/> is not specified, this is the name used for From address
            /// </summary>
            /// <value>
            /// From name.
            /// </value>
            public string FromName { get; set; }

            /// <summary>
            /// If <see cref="FromPersonId"/> is not specified, this is the email address used for From address
            /// </summary>
            /// <value>
            /// From address.
            /// </value>
            public string FromAddress { get; set; }

            /// <summary>
            /// Gets or sets the reply to.
            /// </summary>
            /// <value>
            /// The reply to.
            /// </value>
            public string ReplyTo { get; set; }

            /// <summary>
            /// Gets or sets the subject.
            /// </summary>
            /// <value>
            /// The subject.
            /// </value>
            public string Subject { get; set; }

            /// <summary>
            /// Gets or sets the HTML message.
            /// </summary>
            /// <value>
            /// The HTML message.
            /// </value>
            public string HtmlMessage { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [bulk communication].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [bulk communication]; otherwise, <c>false</c>.
            /// </value>
            public bool BulkCommunication { get; set; }

            /// <summary>
            /// Gets or sets the recipient status.
            /// </summary>
            /// <value>
            /// The recipient status.
            /// </value>
            public CommunicationRecipientStatus RecipientStatus { get; set; } = CommunicationRecipientStatus.Delivered;

            /// <summary>
            /// On optional guid to use if one and only one recipient will be created. Used for tracking opens/clicks.
            /// </summary>
            /// <value>
            /// The recipient unique identifier.
            /// </value>
            public Guid? RecipientGuid { get; set; }

            /// <summary>
            /// Gets or sets the datetime that communication was sent.
            /// </summary>
            /// <value>
            /// The send date time.
            /// </value>
            public DateTime? SendDateTime { get; set; }
        }
    }
}