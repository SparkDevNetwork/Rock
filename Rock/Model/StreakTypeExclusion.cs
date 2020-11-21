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
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Streak Type Exclusion in Rock.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "StreakTypeExclusion" )]
    [DataContract]
    public partial class StreakTypeExclusion : Model<StreakTypeExclusion>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="StreakType"/> to which this exclusion map belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int StreakTypeId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier by which the streak type's exclusions will be associated.
        /// </summary>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// The sequence of bits that represent exclusions. The least significant bit is representative of the Streak Type's StartDate.
        /// More significant bits (going left) are more recent dates.
        /// </summary>
        [DataMember]
        public byte[] ExclusionMap { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Sequence.
        /// </summary>
        [DataMember]
        public virtual StreakType StreakType { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        [DataMember]
        public virtual Location Location { get; set; }

        #endregion Virtual Properties

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return StreakTypeExclusionCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            StreakTypeExclusionCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion ICacheable

        #region Entity Configuration

        /// <summary>
        /// Streak Type Exclusion Configuration class.
        /// </summary>
        public partial class StreakTypeExclusionConfiguration : EntityTypeConfiguration<StreakTypeExclusion>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StreakTypeExclusionConfiguration"/> class.
            /// </summary>
            public StreakTypeExclusionConfiguration()
            {
                HasRequired( soe => soe.StreakType ).WithMany( s => s.StreakTypeExclusions ).HasForeignKey( soe => soe.StreakTypeId ).WillCascadeOnDelete( true );

                HasOptional( se => se.Location ).WithMany().HasForeignKey( se => se.LocationId ).WillCascadeOnDelete( false );
            }
        }

        #endregion Entity Configuration

        #region Update Hook

        /// <summary>
        /// Perform tasks prior to saving changes to this entity.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The entry.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, DbEntityEntry entry )
        {
            // Add a transaction to process denormalized data refreshes
            new StreakTypeExclusionChangeTransaction( entry ).Enqueue();
            base.PreSaveChanges( dbContext, entry );
        }

        #endregion Update Hook
    }
}
