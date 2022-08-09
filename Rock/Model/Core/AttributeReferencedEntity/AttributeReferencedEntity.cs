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

namespace Rock.Model
{
    /// <summary>
    /// Contains a list of entities that are being referenced by the attribute.
    /// This is used to keep the persisted values up to date when another entity
    /// is modified that is referenced by the attribute.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AttributeReferencedEntity")]
    [DataContract]
    [NotAudited]
    public partial class AttributeReferencedEntity
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the identifier of this instance.
        /// </summary>
        /// <value>The identifier of this instance.</value>
        [Key]
        [DataMember]
        [IncludeForReporting]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the attribute identifier that is referencing the entity.
        /// </summary>
        /// <value>The attribute identifier that is referencing the entity.</value>
        public int AttributeId { get; set; }

        /// <summary>
        /// Gets or sets the entity type identifier of the entity being referenced.
        /// </summary>
        /// <value>The entity type identifier of the entity being referenced.</value>
        [Index( "IX_EntityTypeId_EntityId", Order = 1 )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the entity being referenced.
        /// </summary>
        /// <value>The identifier of the entity being referenced.</value>
        [Index( "IX_EntityTypeId_EntityId", Order = 2 )]
        public int EntityId { get; set; }

        #endregion

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the attribute that is referencing the entity.
        /// </summary>
        /// <value>The attribute value that is referencing the entity.</value>
        public virtual Attribute Attribute { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// AttributeReferencedEntity Configuration class.
    /// </summary>
    public partial class AttributeReferencedEntityConfiguration : EntityTypeConfiguration<AttributeReferencedEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeReferencedEntityConfiguration"/> class.
        /// </summary>
        public AttributeReferencedEntityConfiguration()
        {
            this.HasRequired( m => m.Attribute ).WithMany().HasForeignKey( m => m.AttributeId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}
