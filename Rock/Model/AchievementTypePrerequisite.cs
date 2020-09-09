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
    /// Represents a Achievement Type Prerequisite in Rock.
    /// </summary>
    [RockDomain( "Achievements" )]
    [Table( "AchievementTypePrerequisite" )]
    [DataContract]
    public partial class AchievementTypePrerequisite : Model<AchievementTypePrerequisite>, ICacheable
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="AchievementType"/> to which this prerequisite belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int AchievementTypeId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the <see cref="AchievementType"/> that is the prerequisite. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        public int PrerequisiteAchievementTypeId { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the AchievementType.
        /// </summary>
        [DataMember]
        public virtual AchievementType AchievementType { get; set; }

        /// <summary>
        /// Gets or sets the Prerequisite AchievementType.
        /// </summary>
        [DataMember]
        public virtual AchievementType PrerequisiteAchievementType { get; set; }

        #endregion Virtual Properties

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return AchievementTypePrerequisiteCache.Get( Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            AchievementTypePrerequisiteCache.UpdateCachedEntity( Id, entityState );
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

                if ( AchievementTypeId == PrerequisiteAchievementTypeId )
                {
                    ValidationResults.Add( new ValidationResult( $"{nameof( AchievementTypeId )} cannot be equal to {nameof( PrerequisiteAchievementTypeId )}" ) );
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
        public partial class AchievementTypePrerequisiteConfiguration : EntityTypeConfiguration<AchievementTypePrerequisite>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="AchievementTypePrerequisiteConfiguration"/> class.
            /// </summary>
            public AchievementTypePrerequisiteConfiguration()
            {
                HasRequired( statp => statp.AchievementType )
                    .WithMany( stat => stat.Prerequisites )
                    .HasForeignKey( statp => statp.AchievementTypeId )
                    .WillCascadeOnDelete( true );

                HasRequired( statp => statp.PrerequisiteAchievementType )
                    .WithMany( stat => stat.Dependencies )
                    .HasForeignKey( statp => statp.PrerequisiteAchievementTypeId )
                    // This has to be false because otherwise SQL server doesn't like the possibility of dependency cycles
                    .WillCascadeOnDelete( false );
            }
        }

        #endregion
    }
}
