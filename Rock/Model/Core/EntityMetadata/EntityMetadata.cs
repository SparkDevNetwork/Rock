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

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// <para>
    /// Contains metadata about an entity.
    /// </para>
    /// <para>
    /// Metadata is information that is not required for the entity to function,
    /// but provides additional information or computed/cached values.
    /// Modifying metadata does not cause the entity to be considered modified.
    /// </para>
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "EntityMetadata" )]
    internal class EntityMetadata
    {
        #region Entity Properties

        /// <summary>
        /// The identifier of the <see cref="EntityType"/> that indicates the
        /// type of entity that this metadata is for.
        /// </summary>
        [Key]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// The identifier of the entity that this metadata is for.
        /// </summary>
        [Key]
        public int EntityId { get; set; }

        /// <summary>
        /// The key of the metadata value. This must be unique per entity.
        /// </summary>
        [Key]
        [Required]
        [MaxLength( 100 )]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var entityType = EntityTypeCache.Get( EntityTypeId );

            if ( entityType != null )
            {
                return $"Metadata '{Key}' for {entityType.Name} Id {EntityId}";
            }
            else
            {
                return $"Metadata '{Key}' for {EntityTypeId} Id {EntityId}";
            }
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Entity Change Configuration class.
    /// </summary>
    internal partial class EntityMetadataConfiguration : EntityTypeConfiguration<EntityMetadata>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityTypeConfiguration" /> class.
        /// </summary>
        public EntityMetadataConfiguration()
        {
            this.HasKey( p => new { p.EntityTypeId, p.EntityId, p.Key } );

            // A foreign key exists on EntityTypeId to EntityType table. This
            // is created manually in the migration because we can't set it up
            // here without a navigation property. And having a navigation
            // property would require us to make this class non-internal.
        }
    }

    #endregion
}
