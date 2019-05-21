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
    /// Field Type POCO Entity.
    /// </summary>
    [RockDomain( "Core" )]
    [NotAudited]
    [Table( "FieldType" )]
    [DataContract]
    public partial class FieldType : Model<FieldType>, ICacheable
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets a flag indicating if this FieldType is part of of the Rock core system/framework. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this FieldType is part of the Rock core system/framework; otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// Gets or sets the Name of the FieldType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the FieldType.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets a user defined description of the FieldType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the description of the FieldType.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the Assembly name of the .dll file that contains the FieldType class. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains the Assembly name of the .dll file that contains the FieldType class.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Assembly { get; set; }
        
        /// <summary>
        /// Gets or sets the fully qualified name, with Namespace, of the FieldType class. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that contains the fully qualified name of the FieldType class.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string Class { get; set; }

        #endregion

        #region Virtual Properties

        #endregion

        #region Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this FieldType.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this FieldType.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return FieldTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            FieldTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Field Type Configuration class.
    /// </summary>
    public partial class FieldTypeConfiguration : EntityTypeConfiguration<FieldType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FieldTypeConfiguration"/> class.
        /// </summary>
        public FieldTypeConfiguration()
        {
        }
    }

    #endregion

}
