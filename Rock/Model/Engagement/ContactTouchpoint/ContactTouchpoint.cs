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

using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Engagement;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a touchpoint with for a <see cref="Rock.Model.Contact"/>.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "ContactTouchpoint" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )]
    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.CONTACT_TOUCHPOINT )]
    public partial class ContactTouchpoint : Entity<ContactTouchpoint>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the contact identifier.
        /// </summary>
        [DataMember]
        public int ContactId { get; set; }

        /// <summary>
        /// Gets or sets the type of the touchpoint.
        /// </summary>
        [DataMember]
        public TouchpointType Type { get; set; }

        /// <summary>
        /// Gets or sets the scheduled date time.
        /// </summary>
        [DataMember]
        public DateTime ScheduledDateTime { get; set; }

        /// <summary>
        /// Gets or sets the completed date time.
        /// </summary>
        [DataMember]
        public DateTime? CompletedDateTime { get; set; }

        /// <summary>
        /// The system note.
        /// </summary>
        [DataMember]
        [MaxLength( 1000 )]
        public string SystemNote { get; set; }

        /// <summary>
        /// Gets or sets the communication medium.
        /// </summary>
        [DataMember]
        public TouchpointCommunicationMedium? CommunicationMedium { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        [DataMember]
        [MaxLength( 500 )]
        public string Note { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the contact.
        /// </summary>
        [DataMember]
        public virtual Contact Contact { get; set; }

        #endregion

        #region Entity Configuration

        /// <summary>
        /// ContactTouchpoint Configuration class.
        /// </summary>
        public partial class ContactTouchpointConfiguration : EntityTypeConfiguration<ContactTouchpoint>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ContactTouchpointConfiguration"/> class.
            /// </summary>
            public ContactTouchpointConfiguration()
            {
                this.HasRequired( p => p.Contact ).WithMany().HasForeignKey( p => p.ContactId ).WillCascadeOnDelete( false );
            }
        }

        #endregion
    }
}
