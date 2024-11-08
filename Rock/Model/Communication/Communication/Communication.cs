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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Communication;
using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a communication in Rock (i.e. email, SMS message, etc.).
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "Communication" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "C4CCBD91-1264-48BF-BC33-92751C8948B5")]
    public partial class Communication : Model<Communication>, ICommunicationDetails
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the communication.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the communication type value identifier.
        /// </summary>
        /// <value>
        /// The communication type value identifier.
        /// </value>
        [Required]
        [DataMember]
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Gets or sets the URL from where this communication was created (grid)
        /// </summary>
        /// <value>
        /// The URL referrer.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string UrlReferrer { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group">list</see> that email is being sent to.
        /// </summary>
        /// <value>
        /// The list group identifier.
        /// </value>
        [DataMember]
        public int? ListGroupId { get; set; }

        /// <summary>
        /// Gets or sets the segments that list is being filtered to (comma-delimited list of dataview guids).
        /// </summary>
        /// <value>
        /// The segments.
        /// </value>
        [DataMember]
        public string Segments { get; set; }

        /// <summary>
        /// Gets or sets if communication is targeted to people in all selected segments or any selected segments.
        /// </summary>
        /// <value>
        /// The segment criteria.
        /// </value>
        [DataMember]
        public SegmentCriteria SegmentCriteria { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.CommunicationTemplate"/> that was used to compose this communication
        /// </summary>
        /// <value>
        /// The communication template identifier.
        /// </value>
        /// <remarks>
        /// [IgnoreCanDelete] since there is a ON DELETE SET NULL cascade on this
        /// </remarks>
        [IgnoreCanDelete]
        public int? CommunicationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the sender <see cref="Rock.Model.PersonAlias"/> identifier.
        /// </summary>
        /// <value>
        /// The sender person alias identifier.
        /// </value>
        [DataMember]
        public int? SenderPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the is bulk communication.
        /// </summary>
        /// <value>
        /// The is bulk communication.
        /// </value>
        [DataMember]
        public bool IsBulkCommunication { get; set; }

        /// <summary>
        /// Gets or sets the datetime that communication was sent. This also indicates that communication shouldn't attempt to send again.
        /// </summary>
        /// <value>
        /// The send date time.
        /// </value>
        [DataMember]
        public DateTime? SendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the future send date for the communication. This allows a user to schedule when a communication is sent 
        /// and the communication will not be sent until that date and time.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> value that represents the FutureSendDate for the communication.  If no future send date is provided, this value will be null.
        /// </value>
        [DataMember]
        public DateTime? FutureSendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status of the Communication.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.CommunicationStatus"/> enum value that represents the status of the Communication.
        /// </value>
        [DataMember]
        public CommunicationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the reviewer person alias identifier.
        /// </summary>
        /// <value>
        /// The reviewer person alias identifier.
        /// </value>
        [DataMember]
        public int? ReviewerPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the date and time stamp of when the Communication was reviewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the Communication was reviewed.
        /// </value>
        [DataMember]
        public DateTime? ReviewedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the note that was entered by the reviewer.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a note that was entered by the reviewer.
        /// </value>
        [DataMember]
        public string ReviewerNote { get; set; }

        /// <summary>
        /// Gets or sets a JSON string containing any additional merge fields for the Communication.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> that contains any additional merge fields for the Communication.
        /// </value>
        [DataMember]
        public string AdditionalMergeFieldsJson
        {
            get
            {
                return AdditionalMergeFields.ToJson();
            }

            set
            {
                AdditionalMergeFields = value.FromJsonOrNull<List<string>>() ?? new List<string>();
            }
        }

        /// <summary>
        /// Gets or sets a comma-delimited list of enabled LavaCommands
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        [DataMember]
        public string EnabledLavaCommands { get; set; }

        #region Email Fields

        /// <summary>
        /// Gets or sets the name of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the communication.
        /// </value>
        [DataMember]
        [MaxLength( 1000 )]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets from email address.
        /// </summary>
        /// <value>
        /// From email address.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply to email address.
        /// </summary>
        /// <value>
        /// The reply to email address.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list of CC'ed email addresses.
        /// </summary>
        /// <value>
        /// A comma separated list of CC'ed email addresses.
        /// </value>
        [DataMember]
        public string CCEmails { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list of BCC'ed email addresses.
        /// </summary>
        /// <value>
        /// A comma separated list of BCC'ed email addresses.
        /// </value>
        [DataMember]
        public string BCCEmails { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message meta data.
        /// </summary>
        /// <value>
        /// The message meta data.
        /// </value>
        [DataMember]
        public string MessageMetaData { get; set; }

        #endregion

        #region SMS Properties

        /// <summary>
        /// Gets or sets the SMS from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        [DataMember]
        [Obsolete( "Use SmsFromSystemPhoneNumberId instead." )]
        [RockObsolete( "1.15" )]
        public int? SMSFromDefinedValueId { get; set; }

        /// <summary>
        /// Gets or sets the system phone number identifier used for SMS sending.
        /// </summary>
        /// <value>
        /// The system phone number identifier used for SMS sending.
        /// </value>
        [DataMember]
        public int? SmsFromSystemPhoneNumberId { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string SMSMessage { get; set; }

        #endregion

        #region Push Notification Properties

        /// <summary>
        /// Gets or sets the push notification title.
        /// </summary>
        /// <value>
        /// Push notification title.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets push sound.
        /// </summary>
        /// <value>
        /// Push sound.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PushSound { get; set; }

        /// <summary>
        /// Gets or sets the push <see cref="Rock.Model.BinaryFile">image file</see> identifier.
        /// </summary>
        /// <value>
        /// The push image file identifier.
        /// </value>
        [DataMember]
        public int? PushImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the push open action.
        /// </summary>
        /// <value>
        /// The push open action.
        /// </value>
        [DataMember]
        public PushOpenAction? PushOpenAction { get; set; }

        /// <summary>
        /// Gets or sets the push open message.
        /// </summary>
        /// <value>
        /// The push open message.
        /// </value>
        [DataMember]
        public string PushOpenMessage { get; set; }

        /// <summary>
        /// Gets or sets the push open message structured content JSON.
        /// </summary>
        /// <value>
        /// The push open message structured content JSON.
        /// </value>
        [DataMember]
        public string PushOpenMessageJson { get; set; }

        /// <summary>
        /// Gets or sets the push data.
        /// </summary>
        /// <value>
        /// The push data.
        /// </value>
        [DataMember]
        public string PushData { get; set; }
        #endregion

        /// <summary>
        /// Option to prevent communications from being sent to people with the same email/SMS addresses.
        /// This will mean two people who share an address will not receive a personalized communication, only one of them will.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [exclude duplicate recipient address]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ExcludeDuplicateRecipientAddress { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SystemCommunication"/> that this communication is associated with.
        /// </summary>
        /// <value>
        /// The system communication.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? SystemCommunicationId { get; set; }

        /// <summary>
        /// Gets the send date key.
        /// </summary>
        /// <value>
        /// The send date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? SendDateKey
        {
            get => ( SendDateTime == null || SendDateTime.Value == default ) ?
                        ( int? ) null :
                        SendDateTime.Value.ToString( "yyyyMMdd" ).AsInteger();

            private set
            {
            }
        }
        
        /// <summary>
        /// Gets or sets the number of days to wait after the communication is sent to send the email metrics reminder communication.
        /// </summary>
        /// <value>
        /// The number of days to wait after the communication is sent to send the email metrics reminder communication.
        /// </value>
        [DataMember]
        public int? EmailMetricsReminderOffsetDays { get; set; }
        
        /// <summary>
        /// Gets or sets the datetime that the email metrics reminder communication was sent.
        /// </summary>
        /// <value>
        /// The email metrics reminder communication send date time.
        /// </value>
        [DataMember]
        public DateTime? EmailMetricsReminderSentDateTime { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the list <see cref="Rock.Model.Group" />.
        /// </summary>
        /// <value>
        /// The list group.
        /// </value>
        [DataMember]
        public virtual Group ListGroup { get; set; }

        /// <summary>
        /// Gets or sets the sender <see cref="Rock.Model.PersonAlias" />.
        /// </summary>
        /// <value>
        /// The sender person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias SenderPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the reviewer <see cref="Rock.Model.PersonAlias" />.
        /// </summary>
        /// <value>
        /// The reviewer person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ReviewerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> for the Communication.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> of the Communication.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationRecipient> Recipients
        {
            get
            {
                return _recipients ?? ( _recipients = new Collection<CommunicationRecipient>() );
            }

            set
            {
                _recipients = value;
            }
        }

        private ICollection<CommunicationRecipient> _recipients;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.CommunicationAttachment">attachments</see>.
        /// NOTE: In most cases, you should use GetAttachments( CommunicationType ) instead.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationAttachment> Attachments
        {
            get
            {
                return _attachments ?? ( _attachments = new Collection<CommunicationAttachment>() );
            }

            set
            {
                _attachments = value;
            }
        }

        private ICollection<CommunicationAttachment> _attachments;

        /// <summary>
        /// Gets or sets the additional merge field list. When a communication is created
        /// from a grid, the grid may add additional merge fields that will be available
        /// for the communication.
        /// </summary>
        /// <value>
        /// A <see cref="System.Collections.Generic.List{String}"/> of values containing the additional merge field list.
        /// </value>
        [DataMember]
        public virtual List<string> AdditionalMergeFields
        {
            get
            {
                return _additionalMergeFields;
            }

            set
            {
                _additionalMergeFields = value;
            }
        }

        private List<string> _additionalMergeFields = new List<string>();

        /// <summary>
        /// Gets or sets the SMS from defined value.
        /// </summary>
        /// <value>
        /// The SMS from defined value.
        /// </value>
        [DataMember]
        [Obsolete( "Use SmsFromSystemPhoneNumber instead." )]
        [RockObsolete( "1.15" )]
        public virtual DefinedValue SMSFromDefinedValue { get; set; }

        /// <summary>
        /// Gets or sets the system phone number used for SMS sending.
        /// </summary>
        /// <value>
        /// The system phone number used for SMS sending.
        /// </value>
        [DataMember]
        public virtual SystemPhoneNumber SmsFromSystemPhoneNumber { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.CommunicationTemplate"/> that was used to compose this communication
        /// </summary>
        /// <value>
        /// The communication template.
        /// </value>
        [DataMember]
        public virtual CommunicationTemplate CommunicationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AnalyticsSourceDate">send source date</see>.
        /// </summary>
        /// <value>
        /// The send source date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate SendSourceDate { get; set; }

        /// <inheritdoc cref="SystemCommunicationId"/>
        [DataMember]
        public virtual SystemCommunication SystemCommunication { get; set;  }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Communication Configuration class.
    /// </summary>
    public partial class CommunicationConfiguration : EntityTypeConfiguration<Communication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationConfiguration"/> class.
        /// </summary>
        public CommunicationConfiguration()
        {
            this.HasOptional( c => c.SenderPersonAlias ).WithMany().HasForeignKey( c => c.SenderPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.ReviewerPersonAlias ).WithMany().HasForeignKey( c => c.ReviewerPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.SmsFromSystemPhoneNumber ).WithMany().HasForeignKey( c => c.SmsFromSystemPhoneNumberId ).WillCascadeOnDelete( false );

            // the Migration will manually add a ON DELETE SET NULL for ListGroupId
            this.HasOptional( c => c.ListGroup ).WithMany().HasForeignKey( c => c.ListGroupId ).WillCascadeOnDelete( false );

            // the Migration will manually add a ON DELETE SET NULL for CommunicationTemplateId
            this.HasOptional( c => c.CommunicationTemplate ).WithMany().HasForeignKey( c => c.CommunicationTemplateId ).WillCascadeOnDelete( false );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier OccurrenceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasOptional( r => r.SendSourceDate ).WithMany().HasForeignKey( r => r.SendDateKey ).WillCascadeOnDelete( false );

            // the Migration will manually add a ON DELETE SET NULL for SystemCommunicationId
            this.HasOptional( r => r.SystemCommunication ).WithMany().HasForeignKey( r => r.SystemCommunicationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
