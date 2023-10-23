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
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Streak in Rock.
    /// </summary>
    [RockDomain( "Engagement" )]
    [Table( "Streak" )]
    [DataContract]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.STREAK )]
    public partial class Streak : Model<Streak>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Rock.Model.StreakType"/> to which this Streak belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_StreakTypeId", IsUnique = false )]
        [Index( "IX_StreakTypeId_PersonAliasId", 0, IsUnique = true )]
        public int StreakTypeId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/> identifier.
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
        /// Gets or sets the <see cref="Rock.Model.Location"/> identifier by which the person's exclusions will be sourced.
        /// </summary>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// The sequence of bits that represent engagement. The least significant bit (right side) is representative of the StreakType's
        /// StartDate. More significant bits (going left) are more recent dates.
        /// </summary>
        [DataMember]
        [CodeGenExclude( CodeGenFeature.ViewModelFile )]
        public byte[] EngagementMap { get; set; }

        /// <summary>
        /// The sequence of bits that represent exclusions exclusive to this streak. The least significant bit (right side) is representative
        /// of the StreakType's StartDate. More significant bits (going left) are more recent dates.
        /// </summary>
        [DataMember]
        [CodeGenExclude( CodeGenFeature.ViewModelFile )]
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

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        [DataMember]
        public virtual PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.StreakType"/>.
        /// </summary>
        [DataMember]
        public virtual StreakType StreakType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Location"/>.
        /// </summary>
        [DataMember]
        public virtual Location Location { get; set; }

        #endregion Navigation Properties

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
    }
}
