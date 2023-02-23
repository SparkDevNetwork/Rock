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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a phone number that is managed by the system.
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "SystemPhoneNumber" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( "66D62A9F-13CD-4160-8653-211B2A4ABF16" )]
    public partial class SystemPhoneNumber : Model<SystemPhoneNumber>, IOrdered, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the friendly name of the phone number.
        /// </summary>
        /// <value>
        /// The friendly name of the phone number.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [MaxLength( 100 )]
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
        /// Gets or sets the phone number. This should be in E.123 format,
        /// such as <c>+16235553324</c>.
        /// </summary>
        /// <value>
        /// The phone number.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [MaxLength( 20 )]
        public string Number { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this phone number is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this phone number is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the sort and display order of the <see cref="SystemPhoneNumber"/>.
        /// This is an ascending order, so the lower the value the higher the
        /// sort priority.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the sort order of the <see cref="SystemPhoneNumber"/>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the person alias who should receive
        /// responses to the SMS number. This person must have a phone number
        /// with SMS enabled or no response will be sent.
        /// </summary>
        /// <value>The person alias identifier who should receive responses.</value>
        [DataMember]
        public int? AssignedToPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance support SMS.
        /// </summary>
        /// <value><c>true</c> if this instance supports SMS; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsSmsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this phone number will
        /// forward incoming messages to <see cref="AssignedToPersonAliasId"/>.
        /// </summary>
        /// <value><c>true</c> if this phohe number will forward incoming messages; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsSmsForwardingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the workflow type that will be
        /// launched when an SMS message is received on this number.
        /// </summary>
        /// <value>The workflow type identifier to be launched when an SMS message is received.</value>
        [DataMember]
        public int? SmsReceivedWorkflowTypeId { get; set; }

        /// <summary>
        /// Gets or sets the notification group identifier. Active members of
        /// this group will be notified when a new SMS message comes in to
        /// this phone number.
        /// </summary>
        /// <value>The SMS notification group identifier.</value>
        [DataMember]
        public int? SmsNotificationGroupId { get; set; }

        /// <summary>
        /// Gets or sets the mobile application site identifier. This is
        /// used when determining what devices to send push notifications to.
        /// </summary>
        /// <value>The mobile application site identifier.</value>
        [DataMember]
        public int? MobileApplicationSiteId { get; set; }

        /// <summary>
        /// Gets or sets the provider identifier.
        /// </summary>
        /// <value>
        /// The provider identifier.
        /// </value>
        [DataMember()]
        [MaxLength( 50 )]
        public string ProviderIdentifier { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the person alias who should receive responses to the SMS
        /// number. This person must have a phone number with SMS enabled
        /// or no response will be sent.
        /// </summary>
        /// <value>The person alias who should receive responses.</value>
        [DataMember]
        public virtual PersonAlias AssignedToPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the workflow type that will be launched when an
        /// SMS message is received on this number.
        /// </summary>
        /// <value>The workflow type to be launched when an SMS message is received.</value>
        [DataMember]
        public virtual WorkflowType SmsReceivedWorkflowType { get; set; }

        /// <summary>
        /// Gets or sets the SMS notification group. Active members of this
        /// group will be notified when a new SMS message comes in to this
        /// phone number.
        /// </summary>
        /// <value>The SMS notification group.</value>
        [DataMember]
        public virtual Group SmsNotificationGroup { get; set; }

        /// <summary>
        /// Gets or sets the SMS mobile application site. This is used
        /// when determining what devices to send push notifications to.
        /// </summary>
        /// <value>The SMS mobile application site.</value>
        [DataMember]
        public virtual Site MobileApplicationSite { get; set; }

        #endregion Navigation Properties

        #region Methods

        /// <summary>
        /// Returns a <see cref="string" /> that represents this phone number.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this phone number.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// System Phone Number Configuration class.
    /// </summary>
    public partial class SystemPhoneNumberConfiguration : EntityTypeConfiguration<SystemPhoneNumber>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemPhoneNumberConfiguration" /> class.
        /// </summary>
        public SystemPhoneNumberConfiguration()
        {
            this.HasOptional( s => s.AssignedToPersonAlias ).WithMany().HasForeignKey( s => s.AssignedToPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( s => s.SmsReceivedWorkflowType ).WithMany().HasForeignKey( s => s.SmsReceivedWorkflowTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( s => s.SmsNotificationGroup ).WithMany().HasForeignKey( s => s.SmsNotificationGroupId ).WillCascadeOnDelete( false );
            this.HasOptional( s => s.MobileApplicationSite ).WithMany().HasForeignKey( s => s.MobileApplicationSiteId ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
