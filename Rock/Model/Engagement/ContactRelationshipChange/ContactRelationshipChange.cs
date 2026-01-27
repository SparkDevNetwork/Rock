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

using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Enums.Engagement;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a change in relationship strength for a <see cref="Rock.Model.Contact"/>.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "ContactRelationshipChange" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [CodeGenExclude( CodeGenFeature.DefaultRestController )]
    [SystemGuid.EntityTypeGuid( SystemGuid.EntityType.CONTACT_RELATIONSHIP_CHANGE )]
    public partial class ContactRelationshipChange : Entity<ContactRelationshipChange>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the contact identifier.
        /// </summary>
        [DataMember]
        public int ContactId { get; set; }

        /// <summary>
        /// Gets or sets the previous relationship strength.
        /// </summary>
        [DataMember]
        public RelationshipStrength PreviousRelationshipStrength { get; set; }

        /// <summary>
        /// Gets or sets the new relationship strength.
        /// </summary>
        [DataMember]
        public RelationshipStrength NewRelationshipStrength { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the change was app influenced growth.
        /// </summary>
        [DataMember]
        public bool? HasAcceptedJesus { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether acceptance was influenced by the app.
        /// </summary>
        [DataMember]
        public bool? WasAcceptanceInfluencedByApp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the contact has been baptized.
        /// </summary>
        [DataMember]
        public bool? HasBeenBaptized { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the baptism was influenced by the app.
        /// </summary>
        [DataMember]
        public bool? WasBaptismInfluencedByApp { get; set; }

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
        /// ContactRelationshipStrengthChanges Configuration class.
        /// </summary>
        public partial class ContactRelationshipChangesConfiguration : EntityTypeConfiguration<ContactRelationshipChange>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ContactRelationshipChangesConfiguration"/> class.
            /// </summary>
            public ContactRelationshipChangesConfiguration()
            {
                this.HasRequired( p => p.Contact ).WithMany().HasForeignKey( p => p.ContactId ).WillCascadeOnDelete( false );
            }
        }

        #endregion
    }
}
