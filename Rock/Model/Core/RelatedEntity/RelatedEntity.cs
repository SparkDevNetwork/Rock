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

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// <para>The Related Entity to allow linking entities (of the same or different types) to each other.</para>
    /// <para>See notes on <seealso cref="RelatedEntityPurposeKey"/> for details on how this can be used to have entities
    ///  be related to each other. </para>
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "RelatedEntity" )]
    [DataContract]
    [CodeGenerateRest( DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "BD29E403-BA47-4688-BE29-45A38CE8BD03")]
    public partial class RelatedEntity : Model<RelatedEntity>, IOrdered
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of source entity.
        /// <para>See notes on <seealso cref="RelatedEntityPurposeKey"/> for details.</para>
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId for the <see cref="Rock.Model.EntityType"/> of the  source entity.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
#if REVIEW_WEBFORMS
        [Index( "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey", IsUnique = true, Order = 1 )]
#endif
        public int SourceEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityId of the <see cref="Rock.Model.EntityType" /> of the source.
        /// <para>See notes on <seealso cref="RelatedEntityPurposeKey"/> for details.</para>
        /// </summary>
        /// <value>
        /// The source entity identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Range( 1, int.MaxValue, ErrorMessage = "SourceEntityId must be greater than zero" )]
#if REVIEW_WEBFORMS
        [Index( "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey", IsUnique = true, Order = 2 )]
#endif
        public int SourceEntityId { get; set; }

        /// <summary>
        /// Gets or sets the EntityTypeId for the <see cref="Rock.Model.EntityType" /> of target entity.
        /// <para>See notes on <seealso cref="RelatedEntityPurposeKey"/> for details.</para>
        /// </summary>
        /// <value>
        /// The target entity type identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
#if REVIEW_WEBFORMS
        [Index( "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey", IsUnique = true, Order = 3 )]
#endif
        public int TargetEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the EntityId of the <see cref="Rock.Model.EntityType" /> of the target.
        /// <para>See notes on <seealso cref="RelatedEntityPurposeKey"/> for details.</para>
        /// </summary>
        /// <value>
        /// The target entity identifier.
        /// </value>
        [Required]
        [Range( 1, int.MaxValue, ErrorMessage = "TargetEntityId must be greater than zero" )]
        [DataMember( IsRequired = true )]
#if REVIEW_WEBFORMS
        [Index( "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey", IsUnique = true, Order = 4 )]
#endif
        public int TargetEntityId { get; set; }

        /// <summary>
        /// Gets or sets the purpose key. This indicates the purpose of the relationship. For example:
        /// <para>See notes on <seealso cref="RelatedEntityPurposeKey"/> for details.</para>
        /// </summary>
        /// <value>
        /// The purpose key.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
#if REVIEW_WEBFORMS
        [Index( "IDX_SourceEntityTypeIdSourceEntityIdTargetEntityTypeIdTargetEntityIdPurposeKey", IsUnique = true, Order = 5 )]
#endif
        public string PurposeKey { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this Site was created by and is part of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this Site is part of the Rock core system/framework, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the qualifier value.
        /// See more details on <seealso cref="RelatedEntityPurposeKey"/>.
        /// </summary>
        /// <value>
        /// The qualifier value.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string QualifierValue { get; set; }

        /// <summary>
        /// Gets or sets the quantity.
        /// </summary>
        /// <value>
        /// The quantity.
        /// </value>
        [DataMember]
        public int? Quantity { get; set; }

        /// <summary>
        /// Gets or sets the note.
        /// </summary>
        /// <value>
        /// The note.
        /// </value>
        [DataMember]
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the additional settings json.
        /// </summary>
        /// <value>
        /// The additional settings json.
        /// </value>
        [DataMember]
        public string AdditionalSettingsJson { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the type of the source entity.
        /// <para>See notes on <seealso cref="RelatedEntityPurposeKey"/> for details.</para>
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType SourceEntityType { get; set; }

        /// <summary>
        /// Gets or sets the type of the target entity.
        /// <para>See notes on <seealso cref="RelatedEntityPurposeKey"/> for details.</para>
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType TargetEntityType { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// RelatedEntity Configuration class.
    /// </summary>
    public partial class RelatedEntityConfiguration : EntityTypeConfiguration<RelatedEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntitySetItemConfiguration"/> class.
        /// </summary>
        public RelatedEntityConfiguration()
        {
            this.HasRequired( a => a.SourceEntityType ).WithMany().HasForeignKey( a => a.SourceEntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( a => a.TargetEntityType ).WithMany().HasForeignKey( a => a.TargetEntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion
}