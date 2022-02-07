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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{

    /// <summary>
    /// Represents a personal device used for notifications.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "PersonalDevice" )]
    [DataContract]
    public partial class PersonalDevice : Model<PersonalDevice>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [DataMember]
        public int? PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the registration id of the device.
        /// </summary>
        /// <value>
        /// The device registration id.
        /// </value>
        [DataMember]
        public string DeviceRegistrationId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the Device Type <see cref="DefinedValue" /> representing what type of device this is.
        /// </summary>
        /// <value>
        /// A <see cref="int"/> representing the Id of the <see cref="DefinedValue"/> identifying the personal device type. This cannot be null.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSONAL_DEVICE_TYPE )]
        public int? PersonalDeviceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the platform value identifier (i.e. iOS, Android, etc)
        /// </summary>
        /// <value>
        /// The platform value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM )]
        public int? PlatformValueId { get; set; }

        /// <summary>
        /// Gets or sets the device unique identifier (MEID/IMEI)
        /// </summary>
        /// <value>
        /// The device unique identifier.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string DeviceUniqueIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the device version.
        /// </summary>
        /// <value>
        /// The device version.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string DeviceVersion { get; set; }

        /// <summary>
        /// Gets or sets the MAC address.
        /// </summary>
        /// <value>
        /// The mac address.
        /// </value>
        [DataMember]
        [MaxLength( 12 )]
        public string MACAddress { get; set; }

        /// <summary>
        /// Gets or sets whether or not notifications are enabled for this device.
        /// </summary>
        /// <value>
        /// True or false based on whether or not notifications are enabled.
        /// </value>
        [DataMember]
        public bool NotificationsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active personal device. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this personal device is active, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the site identifier.
        /// </summary>
        /// <value>
        /// The site identifier.
        /// </value>
        [DataMember]
        public int? SiteId { get; set; }

        /// <summary>
        /// Gets or sets the manufacturer.
        /// </summary>
        /// <value>
        /// The manufacturer.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the date and time this device was last seen initiating
        /// contact to the Rock server.
        /// </summary>
        /// <value>
        /// The date and time this device was last seen initiating contact to the
        /// Rock server.
        /// </value>
        public DateTime? LastSeenDateTime { get; set; }

        /// <summary>
        /// Gets or sets the date and time this device was last verified as active
        /// and available for contact.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///     This value serves a different purpose than <see cref="LastSeenDateTime"/>.
        ///     The <see cref="LastSeenDateTime"/> property tells us when the user last
        ///     performed some action (like launching the app) that initiated contact with
        ///     us. In contrast, this property tells us when we last verified that the
        ///     device is still valid.
        ///     </para>
        ///     <para>
        ///     For example, a mobile application might not have been launched in six
        ///     months (<see cref="LastSeenDateTime"/> of six months ago) but this
        ///     value might be only two weeks ago since that was the last time we
        ///     pinged the device to verify it is still installed.
        ///     </para>
        /// </remarks>
        /// <value>
        /// The date and time this device was last verified as active and available
        /// for contact.
        /// </value>
        public DateTime? LastVerifiedDateTime { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the personal device type.
        /// </summary>
        /// <value>
        /// The device type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue PersonalDeviceType { get; set; }

        /// <summary>
        /// Gets or sets the site.
        /// </summary>
        /// <value>
        /// The site.
        /// </value>
        [DataMember]
        public virtual Site Site { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Configuration class.
    /// </summary>
    public partial class PersonalDeviceConfiguration : EntityTypeConfiguration<PersonalDevice>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalDeviceConfiguration"/> class.
        /// </summary>
        public PersonalDeviceConfiguration()
        {
            this.HasOptional( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.PersonalDeviceType ).WithMany().HasForeignKey( r => r.PersonalDeviceTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.Site ).WithMany().HasForeignKey( r => r.SiteId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}