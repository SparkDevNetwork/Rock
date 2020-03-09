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
    /// Represents a Streak Type Achievement Type Prerequisite in Rock.
    /// </summary>
    [RockDomain( "Streaks" )]
    [Table( "StreakTypeAchievementTypePrerequisite" )]
    [DataContract]
    public partial class StreakTypeAchievementTypePrerequisite : Model<StreakTypeAchievementTypePrerequisite>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="StreakTypeAchievementType"/> to which this prerequisite belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int StreakTypeAchievementTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="StreakTypeAchievementType"/> that is the prerequisite. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int PrerequisiteStreakTypeAchievementTypeId { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the StreakTypeAchievementType.
        /// </summary>
        [DataMember]
        public virtual StreakTypeAchievementType StreakTypeAchievementType { get; set; }

        /// <summary>
        /// Gets or sets the Prerequisite StreakTypeAchievementType.
        /// </summary>
        [DataMember]
        public virtual StreakTypeAchievementType PrerequisiteStreakTypeAchievementType { get; set; }

        #endregion Virtual Properties

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return StreakTypeAchievementTypePrerequisiteCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            StreakTypeAchievementTypePrerequisiteCache.UpdateCachedEntity( Id, entityState );
        }

        #endregion ICacheable

        #region Overrides

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;

                if ( StreakTypeAchievementTypeId == PrerequisiteStreakTypeAchievementTypeId )
                {
                    ValidationResults.Add( new ValidationResult( "StreakTypeAchievementTypeId cannot be equal to PrerequisiteStreakTypeAchievementTypeId" ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        #endregion Overrides

        #region Entity Configuration

        /// <summary>
        /// Configuration class.
        /// </summary>
        public partial class StreakTypeAchievementTypePrerequisiteConfiguration : EntityTypeConfiguration<StreakTypeAchievementTypePrerequisite>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StreakTypeAchievementTypePrerequisiteConfiguration"/> class.
            /// </summary>
            public StreakTypeAchievementTypePrerequisiteConfiguration()
            {
                HasRequired( statp => statp.StreakTypeAchievementType )
                    .WithMany( stat => stat.Prerequisites )
                    .HasForeignKey( statp => statp.StreakTypeAchievementTypeId )
                    .WillCascadeOnDelete( true );

                HasRequired( statp => statp.PrerequisiteStreakTypeAchievementType )
                    .WithMany( stat => stat.Dependencies )
                    .HasForeignKey( statp => statp.PrerequisiteStreakTypeAchievementTypeId )
                    // This has to be false because otherwise SQL server doesn't like the possibility of dependency cycles
                    .WillCascadeOnDelete( false );
            }
        }

        #endregion
    }
}
