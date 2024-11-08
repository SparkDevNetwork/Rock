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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Rock email template.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "SystemCommunication" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.SYSTEM_COMMUNICATION )]
    public partial class SystemCommunication : Model<SystemCommunication>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if the email template is part of the Rock core system/framework.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if the EmailTemplate is part of the Rock core system/framework otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this item is available for use.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this item is available for use.
        /// </value>
        [DataMember]
        public bool? IsActive { get; set; }

        /// <summary>
        /// Gets or sets the category identifier.
        /// </summary>
        /// <value>
        /// The category identifier.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        /// <summary>
        /// Gets or sets the Title of the EmailTemplate 
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing the Title of the EmailTemplate.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the From email address.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the from email address.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string From { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the To email addresses that emails using this template should be delivered to.  If there is not a predetermined distribution list, this property can 
        /// remain empty.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a list of email addresses that the message should be delivered to. If there is not a predetermined email list, this property will 
        /// be null.
        /// </value>
        [DataMember]
        public string To { get; set; }

        /// <summary>
        /// Gets or sets the email addresses that should be sent a CC or carbon copy of an email using this template. If there is not a predetermined distribution list, this property
        /// can remain empty.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a list of email addresses that should be sent a CC or carbon copy of an email that uses this template. If there is not a predetermined
        /// distribution list, this property will be null.
        /// </value>
        [DataMember]
        public string Cc { get; set; }

        /// <summary>
        /// Gets or sets the email addresses that should be sent a BCC or blind carbon copy of an email using this template. If there is not a predetermined distribution list; this property 
        /// can remain empty.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> representing a list of email addresses that should be sent a BCC or blind carbon copy of an email that uses this template. If there is not a predetermined
        /// distribution list this property will remain null.
        /// </value>
        [DataMember]
        public string Bcc { get; set; }

        /// <summary>
        /// Gets or sets the subject of an email that uses this template.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the subject of an email that uses this template.
        /// </value>
        [Required(AllowEmptyStrings = true)]
        [MaxLength( 1000 )]
        [DataMember( IsRequired = true )]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the Body template that is used for emails that use this template.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the body template for emails that use this template.
        /// </value>
        [Required(AllowEmptyStrings = true)]
        [DataMember( IsRequired = true )]
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether CSS styles should be inlined in the message body to ensure compatibility with oldere HTML rendering engines.
        /// </summary>
        /// <value>
        ///   <c>true</c> if CSS style inlining is enabled; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool CssInliningEnabled { get; set; } = true;

        #region SMS Properties

        /// <summary>
        /// Gets or sets the SMS message content.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the message text.
        /// </value>
        [DataMember]
        public string SMSMessage { get; set; }

        /// <summary>
        /// Gets or sets the SMS from number.
        /// </summary>
        /// <value>
        /// The identifier of a Defined Value that identifies the SMS Sender.
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

        #region Push Notification Properties

        /// <summary>
        /// Gets or sets the title of the notification.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the notification title.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        /// <value>
        /// A <see cref="System.String" /> containing the notification text.
        /// </value>
        [DataMember]
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets the name of the sound alert to use for the notification.
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

        /// <summary>
        /// A Dictionary of Key,DefaultValue for Lava MergeFields that can be used when processing Lava in the SystemCommunication.
        /// By convention, a Key with a 'Color' suffix will indicate that the Value is selected using a ColorPicker - otherwise, it is just text.
        /// </summary>
        /// <value>
        /// The merge fields.
        /// </value>
        [DataMember]
        public virtual Dictionary<string, string> LavaFields { get; set; } = new Dictionary<string, string>();

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Title;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Email Template Configuration class.
    /// </summary>
    public partial class SystemCommunicationConfiguration : EntityTypeConfiguration<SystemCommunication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemCommunicationConfiguration"/> class.
        /// </summary>
        public SystemCommunicationConfiguration()
        {
            this.HasOptional( t => t.Category ).WithMany().HasForeignKey( t => t.CategoryId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.SmsFromSystemPhoneNumber ).WithMany().HasForeignKey( c => c.SmsFromSystemPhoneNumberId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}
