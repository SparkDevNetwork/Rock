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

namespace Rock.Transactions
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Transactions.ITransaction" />
    public class SaveCommunicationTransaction : ITransaction
    {
        /// <summary>
        /// If <see cref="Recipients"/> is not specified, this is the list of email addresses to set the email to
        /// </summary>
        /// <value>
        /// The recipient emails.
        /// </value>
        [RockObsolete( "1.10" )]
        [Obsolete( "This has a issue where the wrong person(s) might be logged as the recipient. Use Recipients instead to ensure the correct person is associated with the communication.", true )]
        public List<string> RecipientEmails
        {
            get => _recipientEmailAddresses;
            set => _recipientEmailAddresses = value;
        }

        private List<string> _recipientEmailAddresses;

        /// <summary>
        /// Gets or sets the rock message recipients.
        /// </summary>
        /// <value>
        /// The rock message recipients.
        /// </value>
        public List<RockMessageRecipient> Recipients { get; set; }

        /// <summary>
        /// Gets or sets the from person identifier. Setting this will ensure we associate the correct sender
        /// with the communication, if there are multiple people with the same <see cref="FromAddress"/>.
        /// <para>
        /// If <see cref="FromName"/> or <see cref="FromAddress"/> are not set and a person matching this
        /// identifier is found, those property values will be set from this matching person.
        /// </para>
        /// </summary>
        /// <value>
        /// The from person identifier.
        /// </value>
        public int? FromPersonId { get; set; }

        /// <summary>
        /// Gets or sets the from name.
        /// </summary>
        /// <value>
        /// The from name.
        /// </value>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the from address.
        /// </summary>
        /// <value>
        /// The from address.
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
        public CommunicationRecipientStatus RecipientStatus { get; set; }

        /// <summary>
        /// On optional guid to use if one and only one recipient will be created. Used for tracking opens/clicks.
        /// </summary>
        /// <value>
        /// The recipient unique identifier.
        /// </value>
        public Guid? RecipientGuid { get; set; }

        /// <summary>
        /// Gets or sets the DateTime that communication was sent.
        /// </summary>
        /// <value>
        /// The send date time.
        /// </value>
        public DateTime? SendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the system communication identifier.
        /// </summary>
        /// <value>
        /// The system communication identifier.
        /// </value>
        public int? SystemCommunicationId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommunicationTransaction"/> class.
        /// </summary>
        public SaveCommunicationTransaction()
        {
            RecipientStatus = CommunicationRecipientStatus.Delivered;
            SendDateTime = RockDateTime.Now;
            BulkCommunication = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommunicationTransaction"/> class.
        /// </summary>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        private SaveCommunicationTransaction( string fromName, string fromAddress, string subject, string message )
            : this()
        {
            FromName = fromName;
            FromAddress = fromAddress;
            ReplyTo = fromAddress;
            Subject = subject;
            HtmlMessage = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommunicationTransaction"/> class.
        /// </summary>
        /// <param name="to">The email address (or a delimited list of email addresses) that the message should be sent to</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "This has a issue where the wrong person(s) might be logged as the recipient. Use the constructor that takes RockMessageRecipient as a parameter to ensure the correct person is associated with the communication.", true )]
        public SaveCommunicationTransaction( string to, string fromName, string fromAddress, string subject, string message )
            : this( fromName, fromAddress, subject, message )
        {
            RecipientEmails = to.SplitDelimitedValues().ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommunicationTransaction"/> class.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        [RockObsolete( "1.10" )]
        [Obsolete( "This has a issue where the wrong person(s) might be logged as the recipient. Use the constructor that takes RockMessageRecipient as a parameter to ensure the correct person is associated with the communication.", true )]
        public SaveCommunicationTransaction( List<string> to, string fromName, string fromAddress, string subject, string message )
            : this( fromName, fromAddress, subject, message )
        {
            RecipientEmails = to;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommunicationTransaction" /> class.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public SaveCommunicationTransaction( List<RockMessageRecipient> recipients, string fromName, string fromAddress, string subject, string message )
            : this( fromName, fromAddress, subject, message )
        {
            this.Recipients = recipients.ToList();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveCommunicationTransaction" /> class.
        /// </summary>
        /// <param name="recipient">The recipient.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="fromAddress">From address.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="message">The message.</param>
        public SaveCommunicationTransaction( RockMessageRecipient recipient, string fromName, string fromAddress, string subject, string message )
            : this( fromName, fromAddress, subject, message )
        {
            this.Recipients = new List<RockMessageRecipient>();
            this.Recipients.Add( recipient );
        }

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public void Execute()
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );
                int? senderPersonAliasId = null;
                if ( this.FromPersonId.HasValue )
                {
                    var sender = personService.GetNoTracking( this.FromPersonId.Value );
                    senderPersonAliasId = sender?.PrimaryAliasId;

                    if ( this.FromName.IsNullOrWhiteSpace() )
                    {
                        this.FromName = sender?.FullName;
                    }

                    if ( this.FromAddress.IsNullOrWhiteSpace() )
                    {
                        this.FromAddress = sender?.Email;
                    }
                }
                else
                {
                    if ( this.FromAddress.IsNotNullOrWhiteSpace() )
                    {
                        var sender = personService
                            .Queryable().AsNoTracking()
                            .Where( p => p.Email == this.FromAddress )
                            .FirstOrDefault();
                        senderPersonAliasId = sender?.PrimaryAliasId;
                    }
                }

                if ( this.Recipients?.Any() != true && _recipientEmailAddresses != null )
                {
                    this.Recipients = new List<RockMessageRecipient>();
                    this.Recipients.AddRange( _recipientEmailAddresses.Select( a => RockEmailMessageRecipient.CreateAnonymous( a, null ) ).ToList() );
                }

                if ( this.Recipients?.Any() == true )
                {
                    var emailRecipients = this.Recipients.OfType<RockEmailMessageRecipient>().ToList();
                    var createEmailCommunicationArgs = new CommunicationService.CreateEmailCommunicationArgs
                    {
                        Recipients = emailRecipients,
                        FromName = this.FromName,
                        FromAddress = this.FromAddress,
                        ReplyTo = this.ReplyTo,
                        Subject = this.Subject,
                        Message = this.HtmlMessage,
                        BulkCommunication = this.BulkCommunication,
                        SendDateTime = this.SendDateTime,
                        RecipientStatus = this.RecipientStatus,
                        SenderPersonAliasId = senderPersonAliasId,
                        SystemCommunicationId = this.SystemCommunicationId
                    };

                    var communication = new CommunicationService( rockContext ).CreateEmailCommunication( createEmailCommunicationArgs );

                    if ( communication != null  )
                    {
                        if ( communication.Recipients.Count() == 1 && this.RecipientGuid.HasValue )
                        {
                            communication.Recipients.First().Guid = this.RecipientGuid.Value;
                        }
                    }

                    rockContext.SaveChanges();
                }
            }
        }
    }
}