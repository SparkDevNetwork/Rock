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
    /// Contains a list of entities that are being referenced by the attribute values.
    /// This is used to keep the persisted values up to date when another entity
    /// is modified that is referenced by attribute values.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "AttributeValueReferencedEntity" )]
    [DataContract]
    [NotAudited]
    public partial class AttributeValueReferencedEntity
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
        /// Gets or sets the attribute value identifier that is referencing the entity.
        /// This will be null if the source is an attribute.
        /// </summary>
        /// <value>The attribute value identifier that is referencing the entity.</value>
        [Index( "IX_AttributeValueId" )]
        public int AttributeValueId { get; set; }

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
        /// Gets or sets the attribute value that is referencing the entity.
        /// </summary>
        /// <value>The attribute value that is referencing the entity.</value>
        public virtual AttributeValue AttributeValue { get; set; }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// AttributeValueReferencedEntity Configuration class.
    /// </summary>
    public partial class AttributeValueReferencedEntityConfiguration : EntityTypeConfiguration<AttributeValueReferencedEntity>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeValueReferencedEntityConfiguration"/> class.
        /// </summary>
        public AttributeValueReferencedEntityConfiguration()
        {
            this.HasRequired( m => m.AttributeValue ).WithMany().HasForeignKey( m => m.AttributeValueId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}
