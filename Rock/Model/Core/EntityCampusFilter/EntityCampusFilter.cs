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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Model for filtering entities by campus.
    /// </summary>
    /// <seealso cref="T:Rock.Data.Model{Rock.Model.EntityCampusFilter}" />
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "EntityCampusFilter" )]
    [DataContract]
    [CodeGenerateRest( Enums.CodeGenerateRestEndpoint.None, DisableEntitySecurity = true )]
    [Rock.SystemGuid.EntityTypeGuid( "A736A9FB-F2A5-4458-B126-FAD6BD3F3B78")]
    public partial class EntityCampusFilter : Model<EntityCampusFilter>
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
        /// Gets or sets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int CampusId { get; set; }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        [DataMember]
        public virtual Campus Campus { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration
    /// <summary>
    /// EntityCampusFilterConfiguration class
    /// </summary>
    /// <seealso cref="T:System.Data.Entity.ModelConfiguration.EntityTypeConfiguration{Rock.Model.EntityCampusFilter}" />
    public partial class EntityCampusFilterConfiguration : EntityTypeConfiguration<EntityCampusFilter>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCampusFilterConfiguration"/> class.
        /// </summary>
        public EntityCampusFilterConfiguration()
        {
            this.HasRequired( e => e.EntityType ).WithMany().HasForeignKey( e => e.EntityTypeId).WillCascadeOnDelete( true );
            this.HasRequired( e => e.Campus ).WithMany().HasForeignKey( e => e.CampusId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration

}
