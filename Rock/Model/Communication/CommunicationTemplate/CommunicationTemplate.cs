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
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Communication;
using Rock.Data;
using Rock.Security;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a communication Template in Rock (i.e. email, SMS message, etc.).
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "CommunicationTemplate" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.COMMUNICATION_TEMPLATE )]
    public partial class CommunicationTemplate : Model<CommunicationTemplate>, ICommunicationDetails, ICampusFilterable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the Communication Template
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the communication template
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this PageContext is a part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if the PageContext is part of the core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active communication template. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this schedule is active, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether [CSS inlining enabled].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [CSS inlining enabled]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CssInliningEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who is the sender of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person" /> who is the sender of the Communication.
        /// </value>
        [DataMember]
        public int? SenderPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the image file identifier for the Template Preview Image
        /// </summary>
        /// <value>
        /// The image file identifier.
        /// </value>
        [DataMember]
        public int? ImageFileId { get; set; }

        /// <summary>
        /// Gets or sets the logo binary file identifier that email messages using this template can use for the logo in the message content
        /// </summary>
        /// <value>
        /// The logo binary file identifier.
        /// </value>
        [DataMember]
        public int? LogoBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

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
        /// Gets or sets from email.
        /// </summary>
        /// <value>
        /// From email.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply to email.
        /// </summary>
        /// <value>
        /// The reply to email.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets the cc emails.
        /// </summary>
        /// <value>
        /// The cc emails.
        /// </value>
        [DataMember]
        public string CCEmails { get; set; }

        /// <summary>
        /// Gets or sets the BCC emails.
        /// </summary>
        /// <value>
        /// The BCC emails.
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

        /// <summary>
        /// The internal storage for <see cref="CommunicationTemplate.LavaFields"/>
        /// </summary>
        /// <value>
        /// The lava fields json
        /// </value>
        [DataMember]
        public string LavaFieldsJson
        {
            get
            {
                return LavaFields.ToJson( indentOutput: false );
            }

            set
            {
                LavaFields = value.FromJsonOrNull<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            }
        }

        #endregion

        #region SMS Properties

        /// <summary>
        /// Gets or sets from number.
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
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
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
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PushSound { get; set; }

        /// <summary>
        /// Gets or sets the push image file identifier.
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

        #endregion

        #region Navigation Properties

        /// <summary>
        /// A Dictionary of Key,DefaultValue for Lava MergeFields that can be used when processing Lava in the CommunicationTemplate
        /// By convention, a Key with a 'Color' suffix will indicate that the Value is selected using a ColorPicker. Otherwise,it is just text
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        [DataMember]
        public virtual Dictionary<string, string> LavaFields { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets the attachments.
        /// NOTE: In most cases, you should use GetAttachments( CommunicationType ) instead.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationTemplateAttachment> Attachments
        {
            get { return _attachments ?? ( _attachments = new Collection<CommunicationTemplateAttachment>() ); }
            set { _attachments = value; }
        }
        private ICollection<CommunicationTemplateAttachment> _attachments;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> of the Communication's sender.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.PersonAlias"/> that represents the Communication's sender.
        /// </value>
        [DataMember]
        public virtual PersonAlias SenderPersonAlias { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is personal (has a SenderPersonAliasId value) or not
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is personal; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public virtual bool IsPersonal => SenderPersonAliasId.HasValue;

        /// <summary>
        /// Gets or sets the image file for the Template Preview Image
        /// </summary>
        /// <value>
        /// The image file
        /// </value>
        [DataMember]
        public virtual BinaryFile ImageFile {get; set;}

        /// <summary>
        /// Gets or sets the logo binary file that email messages using this template can use for the logo in the message content
        /// </summary>
        /// <value>
        /// The logo binary file.
        /// </value>
        [DataMember]
        public virtual BinaryFile LogoBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the push image file.
        /// </summary>
        /// <value>
        /// The push image file.
        /// </value>
        [DataMember]
        public virtual BinaryFile PushImageBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

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

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Communication Template Configuration class.
    /// </summary>
    public partial class CommunicationTemplateConfiguration : EntityTypeConfiguration<CommunicationTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationTemplateConfiguration"/> class.
        /// </summary>
        public CommunicationTemplateConfiguration()
        {
            this.HasOptional( c => c.Category ).WithMany().HasForeignKey( c => c.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.ImageFile ).WithMany().HasForeignKey( c => c.ImageFileId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.LogoBinaryFile ).WithMany().HasForeignKey( c => c.LogoBinaryFileId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.PushImageBinaryFile ).WithMany().HasForeignKey( c => c.PushImageBinaryFileId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.SenderPersonAlias ).WithMany().HasForeignKey( c => c.SenderPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.SmsFromSystemPhoneNumber ).WithMany().HasForeignKey( c => c.SmsFromSystemPhoneNumberId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}