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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Transactions;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Streak in Rock.
    /// </summary>
    [RockDomain( "Streaks" )]
    [Table( "Streak" )]
    [DataContract]
    public partial class Streak : Model<Streak>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="StreakType"/> to which this Streak belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_StreakTypeId", IsUnique = false )]
        [Index( "IX_StreakTypeId_PersonAliasId", 0, IsUnique = true )]
        public int StreakTypeId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_PersonAliasId", IsUnique = false )]
        [Index( "IX_StreakTypeId_PersonAliasId", 1, IsUnique = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> when the person was enrolled in the streak type.
        /// This is not the Streak Type start date.
        /// </summary>
        [DataMember]
        [Required]
        [Column( TypeName = "Date" )]
        public DateTime EnrollmentDate
        {
            get => _enrollmentDate;
            set => _enrollmentDate = value.Date;
        }
        private DateTime _enrollmentDate = RockDateTime.Today;

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> when the person deactivated their Streak. If null, the Streak is active.
        /// </summary>
        [DataMember]
        public DateTime? InactiveDateTime { get; set; }

        /// <summary>
        /// Gets or sets the location identifier by which the person's exclusions will be sourced.
        /// </summary>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// The sequence of bits that represent engagement. The least significant bit (right side) is representative of the StreakType's
        /// StartDate. More significant bits (going left) are more recent dates.
        /// </summary>
        [DataMember]
        public byte[] EngagementMap { get; set; }

        /// <summary>
        /// The sequence of bits that represent exclusions exclusive to this streak. The least significant bit (right side) is representative
        /// of the StreakType's StartDate. More significant bits (going left) are more recent dates.
        /// </summary>
        [DataMember]
        public byte[] ExclusionMap { get; set; }

        #endregion Entity Properties

        #region Denormalized Entity Properties

        /// <summary>
        /// The date that the current streak began
        /// </summary>
        [DataMember]
        public DateTime? CurrentStreakStartDate { get; set; }

        /// <summary>
        /// The current number of non excluded occurrences attended in a row
        /// </summary>
        [DataMember]
        public int CurrentStreakCount { get; set; }

        /// <summary>
        /// The date the longest streak began
        /// </summary>
        [DataMember]
        public DateTime? LongestStreakStartDate { get; set; }

        /// <summary>
        /// The date the longest streak ended
        /// </summary>
        [DataMember]
        public DateTime? LongestStreakEndDate { get; set; }

        /// <summary>
        /// The longest number of non excluded occurrences attended in a row
        /// </summary>
        [DataMember]
        public int LongestStreakCount { get; set; }

        /// <summary>
        /// The number of engagements on occurrences
        /// </summary>
        [DataMember]
        public int EngagementCount { get; set; }

        #endregion Denormalized Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the Person Alias.
        /// </summary>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the StreakType.
        /// </summary>
        [DataMember]
        public virtual StreakType StreakType { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        [DataMember]
        public virtual Location Location { get; set; }

        /// <summary>
        /// Gets or sets the Location.
        /// </summary>
        [DataMember]
        public virtual bool IsActive
        {
            get => !InactiveDateTime.HasValue;
        }

        /// <summary>
        /// Gets or sets the streak achievement attempts.
        /// </summary>
        /// <value>
        /// The streak type achievement types.
        /// </value>
        [DataMember]
        [JsonIgnore]
        public virtual ICollection<StreakAchievementAttempt> StreakAchievementAttempts
        {
            get => _streakAchievementAttempts ?? ( _streakAchievementAttempts = new Collection<StreakAchievementAttempt>() );
            set => _streakAchievementAttempts = value;
        }
        private ICollection<StreakAchievementAttempt> _streakAchievementAttempts;

        #endregion Virtual Properties

        #region Entity Configuration

        /// <summary>
        /// Sequence Enrollment Configuration class.
        /// </summary>
        public partial class StreakConfiguration : EntityTypeConfiguration<Streak>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="StreakConfiguration"/> class.
            /// </summary>
            public StreakConfiguration()
            {
                HasRequired( se => se.StreakType ).WithMany( s => s.Streaks ).HasForeignKey( se => se.StreakTypeId ).WillCascadeOnDelete( true );
                HasRequired( se => se.PersonAlias ).WithMany().HasForeignKey( se => se.PersonAliasId ).WillCascadeOnDelete( true );

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
        public override void PreSaveChanges( DbContext dbContext, DbEntityEntry entry )
        {
            _isDeleted = entry.State == System.Data.Entity.EntityState.Deleted;
            base.PreSaveChanges( dbContext, entry );
        }
        private bool _isDeleted = false;

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( DbContext dbContext )
        {
            base.PostSaveChanges( dbContext );

            if ( !_isDeleted )
            {
                // Running this as a task allows possibly changed streak type properties to be
                // propogated to the streak type cache. Also there isn't really a reason that
                // the data context save operation needs to wait while this is done.
                Task.Run( () => StreakService.HandlePostSaveChanges( Id ) );
            }
        }

        #endregion Update Hook

        #region Overrides

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;
                var streakTypeCache = StreakTypeCache.Get( StreakTypeId );

                if ( streakTypeCache != null && EnrollmentDate < streakTypeCache.StartDate )
                {
                    ValidationResults.Add( new ValidationResult( $"The enrollment date cannot be before the streak type start date, {streakTypeCache.StartDate.ToShortDateString()}." ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        #endregion Overrides
    }
}
