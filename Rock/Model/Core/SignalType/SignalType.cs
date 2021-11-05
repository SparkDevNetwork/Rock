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
    /// Represents a type of Person signal that the organization uses to flag "special care" individuals.  
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "SignalType" )]
    [DataContract]
    public partial class SignalType : Model<SignalType>, IOrdered, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the SignalType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the SignalType name.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [Index( IsUnique = true )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the HTML color of the SignalType. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the SignalType color.
        /// </value>
        [Required]
        [MaxLength( 100 )]
        [DataMember( IsRequired = true )]
        public string SignalColor { get; set; }
        
        /// <summary>
        /// Gets or sets the icon CSS class of the SignalType.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the SignalType icon class.
        /// </value>
        [MaxLength( 100 )]
        [DataMember]
        public string SignalIconCssClass { get; set; }

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

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String"/> containing the SignalType's Name that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> containing the SignalType's Name that represents this instance.
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
            return SignalTypeCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            SignalTypeCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// SignalType Configuration class.
    /// </summary>
    public partial class SignalTypeConfiguration : EntityTypeConfiguration<SignalType>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SignalTypeConfiguration"/> class.
        /// </summary>
        public SignalTypeConfiguration()
        {
        }
    }

    #endregion
}
