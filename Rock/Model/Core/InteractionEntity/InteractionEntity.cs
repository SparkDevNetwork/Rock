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

namespace Rock.Model
{
    /// <summary>
    /// Relates entities to the interaction that was active when they were
    /// created.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "InteractionEntity" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.INTERACTION_ENTITY )]
    public partial class InteractionEntity
    {

        #region Entity Properties

        /// <summary>
        /// The primary key identifier of this record.
        /// </summary>
        [Key]
        [DataMember]
        public int Id { get; set; }

        /// <summary>
        /// The identifier of the <see cref="EntityType"/> for the entity that
        /// is related to the interaction.
        /// </summary>
        [DataMember]
        [Index( "IX_EntityTypeId_EntityId", 0 )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// The identifier of the entity that is related to the interaction.
        /// </summary>
        [DataMember]
        [Index( "IX_EntityTypeId_EntityId", 1 )]
        public int EntityId { get; set; }

        /// <summary>
        /// The unique identifier of the <see cref="Interaction"/>. This will
        /// be set when the record is written since the
        /// <see cref="Interaction"/> may not have been written yet so we
        /// wouldn't know the identifier.
        /// </summary>
        [DataMember]
        public Guid InteractionGuid { get; set; }

        /// <summary>
        /// The identifier of the <see cref="Interaction"/>. This will be set
        /// automatically by <see cref="Jobs.PopulateInteractionSessionData"/>.
        /// </summary>
        [DataMember]
        [IgnoreCanDelete]
        [Index( "IX_InteractionId_CreatedDateTime", 0 )]
        public int? InteractionId { get; set; }

        /// <summary>
        /// The date and time this record was created. This is used when
        /// trying to populate <see cref="InteractionId"/> so that we can skip
        /// records that are too old to be considered valid if we haven't
        /// already matched.
        /// </summary>
        [DataMember]
        [Index( "IX_InteractionId_CreatedDateTime", 1 )]
        public DateTime CreatedDateTime { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// The <see cref="EntityType"/> of the <see cref="EntityId"/> that
        /// this record is related to.
        /// </summary>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// The <see cref="Interaction"/> that was active when the related
        /// entity was created.
        /// </summary>
        [DataMember]
        public virtual Interaction Interaction { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Interaction Entity Configuration class.
    /// </summary>
    public partial class InteractionEntityConfiguration : EntityTypeConfiguration<InteractionEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionEntityConfiguration"/> class.
        /// </summary>
        public InteractionEntityConfiguration()
        {
            this.HasOptional( ie => ie.Interaction ).WithMany().HasForeignKey( ie => ie.InteractionId ).WillCascadeOnDelete( false );
            this.HasRequired( ie => ie.EntityType ).WithMany().HasForeignKey( ie => ie.EntityTypeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion

}
