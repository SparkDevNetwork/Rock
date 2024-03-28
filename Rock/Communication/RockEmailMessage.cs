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
using System.Net.Mail;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Rock Email Message
    /// </summary>
    /// <seealso cref="Rock.Communication.RockMessage" />
    public class RockEmailMessage : RockMessage
    {
        /// <summary>
        /// Gets the medium entity type identifier.
        /// </summary>
        /// <value>
        /// The medium entity type identifier.
        /// </value>
        public override int MediumEntityTypeId
        {
            get
            {
                return EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() ).Id;
            }
        }

        /// <summary>
        /// Gets or sets the from person identifier. Setting this will ensure we associate the correct
        /// sender with the email, if there are multiple people with the same <see cref="FromEmail"/>.
        /// </summary>
        /// <value>
        /// From person identifier.
        /// </value>
        public int? FromPersonId { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets from email.
        /// </summary>
        /// <value>
        /// From email.
        /// </value>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply to email.
        /// </summary>
        /// <value>
        /// The reply to email.
        /// </value>
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the cc emails.
        /// </summary>
        /// <value>
        /// The cc emails.
        /// </value>
        public List<string> CCEmails { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the BCC emails.
        /// </summary>
        /// <value>
        /// The BCC emails.
        /// </value>
        public List<string> BCCEmails { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the subject.
        /// </summary>
        /// <value>
        /// The subject.
        /// </value>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the text only message.
        /// </summary>
        /// <value>
        /// The text only message.
        /// </value>
        public string PlainTextMessage { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message meta data.
        /// </summary>
        /// <value>
        /// The message meta data.
        /// </value>
        public Dictionary<string, string> MessageMetaData { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the email headers.
        /// </summary>
        /// <value>
        /// The email headers.
        /// </value>
        public Dictionary<string, string> EmailHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the email attachments.
        /// </summary>
        /// <value>
        /// The email attachments.
        /// </value>
        public List<Attachment> EmailAttachments { get; set; } = new List<Attachment>();

        /// <summary>
        /// Gets or sets a value indicating whether CSS styles should be inlined in the message body to ensure compatibility with older HTML rendering engines.
        /// </summary>
        /// <value>
        ///   <c>true</c> if CSS style inlining is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool CssInliningEnabled { get; set; }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RockEmailMessage"/> class.
        /// </summary>
        public RockEmailMessage() : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockEmailMessage"/> class.
        /// </summary>
        /// <param name="systemCommunication">The system email.</param>
        public RockEmailMessage( SystemCommunication systemCommunication ) : this()
        {
            InitEmailMessage( systemCommunication );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockEmailMessage" /> class.
        /// </summary>
        /// <param name="systemGuid">The system communication unique identifier.</param>
        public RockEmailMessage( Guid systemGuid ) : this()
        {
            using ( var rockContext = new RockContext() )
            {
                var systemCommunication = new SystemCommunicationService( rockContext ).Get( systemGuid );

                if ( systemCommunication != null )
                {
                    InitEmailMessage( systemCommunication );
                }
            }
        }

        /// <summary>
        /// Initializes the email message.
        /// </summary>
        /// <param name="systemCommunication">The system email.</param>
        private void InitEmailMessage( SystemCommunication systemCommunication )
        {
            if ( systemCommunication == null )
            {
                return;
            }

            this.FromEmail = systemCommunication.From;
            this.FromName = systemCommunication.FromName;

            var recipients = systemCommunication.To.SplitDelimitedValues().ToList().Select( a => RockEmailMessageRecipient.CreateAnonymous( a, null ) ).ToList();
            this.SetRecipients( recipients );

            this.CCEmails = systemCommunication.Cc.SplitDelimitedValues().ToList();
            this.BCCEmails = systemCommunication.Bcc.SplitDelimitedValues().ToList();
            this.Subject = systemCommunication.Subject;
            this.Message = systemCommunication.Body;
            this.CssInliningEnabled = systemCommunication.CssInliningEnabled;
            this.SystemCommunicationId = systemCommunication.Id;
        }

        #endregion

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        public void SetRecipients( List<RockEmailMessageRecipient> recipients )
        {
            this.Recipients = new List<RockMessageRecipient>();
            this.Recipients.AddRange( recipients );
        }

        #region Obsolete

        /// <summary>
        /// Initializes a new instance of the <see cref="RockEmailMessage"/> class.
        /// </summary>
        /// <param name="systemEmail">The system email.</param>
        [Obsolete( "Use constructor RockEmailMessage( SystemCommunication ) instead.", true )]
        [RockObsolete( "1.10")]
        public RockEmailMessage( SystemEmail systemEmail ) : this()
        {
            InitEmailMessage( systemEmail );
        }

        /// <summary>
        /// Initializes the email message.
        /// </summary>
        /// <param name="systemEmail">The system email.</param>
        [Obsolete("", true)]
        [RockObsolete( "1.10" )]
        private void InitEmailMessage( SystemEmail systemEmail )
        {
            if ( systemEmail != null )
            {
                this.FromEmail = systemEmail.From;
                this.FromName = systemEmail.FromName;
                var recipients = systemEmail.To.SplitDelimitedValues().ToList().Select( a => RockEmailMessageRecipient.CreateAnonymous( a, null ) ).ToList();
                this.SetRecipients( recipients );
                this.CCEmails = systemEmail.Cc.SplitDelimitedValues().ToList();
                this.BCCEmails = systemEmail.Bcc.SplitDelimitedValues().ToList();
                this.Subject = systemEmail.Subject;
                this.Message = systemEmail.Body;
            }
        }

        #endregion
    }
}
