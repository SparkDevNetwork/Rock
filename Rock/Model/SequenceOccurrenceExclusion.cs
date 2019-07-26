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
using System;
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
    /// Represents a Sequence Occurrence Exclusion in Rock.
    /// </summary>
    [RockDomain( "Sequences" )]
    [Table( "SequenceOccurrenceExclusion" )]
    [DataContract]
    public partial class SequenceOccurrenceExclusion : Model<SequenceOccurrenceExclusion>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Sequence"/> to which this exclusion map belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int SequenceId { get; set; }

        /// <summary>
        /// Gets or sets the location identifier by which the sequences's exclusions will be associated.
        /// </summary>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// The sequence of bits that represent exclusions. The least significant bit is representative of the Sequence's StartDate.
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
        public virtual Sequence Sequence { get; set; }

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
            return SequenceOccurrenceExclusionCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            SequenceOccurrenceExclusionCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion ICacheable

        #region Entity Configuration

        /// <summary>
        /// Sequence Occurrence Exclusion Configuration class.
        /// </summary>
        public partial class SequenceOccurrenceExclusionConfiguration : EntityTypeConfiguration<SequenceOccurrenceExclusion>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SequenceOccurrenceExclusionConfiguration"/> class.
            /// </summary>
            public SequenceOccurrenceExclusionConfiguration()
            {
                HasRequired( soe => soe.Sequence ).WithMany( s => s.SequenceOccurrenceExclusions ).HasForeignKey( soe => soe.SequenceId ).WillCascadeOnDelete( true );

                HasOptional( se => se.Location ).WithMany().HasForeignKey( se => se.LocationId ).WillCascadeOnDelete( false );
            }
        }

        #endregion Entity Configuration

        #region Update Hook

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            SequenceService.UpdateEnrollmentStreakPropertiesAsync( SequenceId );
            base.PostSaveChanges( dbContext );
        }

        #endregion Update Hook
    }
}
