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
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{

    /// <summary>
    /// Represents a personal device used for notifications.
    /// </summary>
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
        public int PersonAliasId { get; set; }

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
        public int PersonalDeviceTypeId { get; set; }

        /// <summary>
        /// Gets or sets whether or not notifications are enabled for this device.
        /// </summary>
        /// <value>
        /// True or false based on whether or not notifications are enabled.
        /// </value>
        [DataMember]
        public bool NotificationsEnabled { get; set; }

        #endregion

        #region Virtual Properties

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

        #endregion

        #region Public Methods


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
            this.HasRequired( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.PersonalDeviceType ).WithMany().HasForeignKey( r => r.PersonalDeviceTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}