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
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents an entity's interaction intent, as defined in the related "interaction intent" defined value.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "EntityIntent" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.ENTITY_INTENT )]
    public partial class EntityIntent : Model<EntityIntent>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the interaction intent defined value identifier.
        /// </summary>
        /// <value>
        /// The interaction intent defined value identifier
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int IntentValueId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [LavaVisible]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the intent defined value.
        /// </summary>
        /// <value>
        /// The intent defined value.
        /// </value>
        [LavaVisible]
        public virtual DefinedValue IntentValue { get; set; }

        #endregion Navigation Properties

        #region Entity Configuration

        /// <summary>
        /// EntityIntent Configuration class.
        /// </summary>
        public partial class EntityIntentConfiguration : EntityTypeConfiguration<EntityIntent>
        {
            /// <summary>
            /// Initializes a enw instance of the <see cref="EntityIntentConfiguration"/> class.
            /// </summary>
            public EntityIntentConfiguration()
            {
                this.HasRequired( a => a.EntityType ).WithMany().HasForeignKey( a => a.EntityTypeId ).WillCascadeOnDelete( true );
                this.HasRequired( a => a.IntentValue ).WithMany().HasForeignKey( a => a.IntentValueId ).WillCascadeOnDelete( true );
            }
        }

        #endregion Entity Configuration
    }
}
