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
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a type or category of binary files in Rock, and configures how binary files of this type are stored and accessed.
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "PersonBadge" )]
    [DataContract]
    public partial class PersonBadge : Model<PersonBadge>, IOrdered, ICacheable
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the given Name of the PersonBadge. This value is an alternate key and is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the given Name of the PersonBadge. 
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a description of the PersonBadge.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the PersonBadge.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Id of the badge component
        /// </summary>
        /// <value>
        /// An <see cref="System.Int32" /> representing the Id of the badge component entity type
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the storage mode <see cref="Rock.Model.EntityType"/>.
        /// </summary>
        /// <value>
        /// The storage mode <see cref="Rock.Model.EntityType"/>.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return PersonBadgeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            PersonBadgeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class PersonBadgeConfiguration : EntityTypeConfiguration<PersonBadge>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonBadgeConfiguration"/> class.
        /// </summary>
        public PersonBadgeConfiguration()
        {
            this.HasRequired( b => b.EntityType ).WithMany().HasForeignKey( b => b.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}
