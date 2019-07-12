using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Sequence Enrollment in Rock.
    /// </summary>
    [RockDomain( "Sequences" )]
    [Table( "SequenceEnrollment" )]
    [DataContract]
    public partial class SequenceEnrollment : Model<SequenceEnrollment>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Id of the <see cref="Sequence"/> to which this enrollment belongs. This property is required.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_SequenceId", IsUnique = false )]
        [Index( "IX_SequenceId_PersonAliasId", 0, IsUnique = true )]
        public int SequenceId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        [Required]
        [DataMember( IsRequired = true )]
        [Index( "IX_PersonAliasId", IsUnique = false )]
        [Index( "IX_SequenceId_PersonAliasId", 1, IsUnique = true )]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="DateTime"/> when the person was enrolled in the sequence.
        /// This is not the sequence start date.
        /// </summary>
        [DataMember]
        [Required]
        [Column( TypeName = "Date" )]
        public DateTime EnrollmentDate
        {
            get => _enrollmentDate;
            set => _enrollmentDate = value.Date;
        }
        private DateTime _enrollmentDate = RockDateTime.Now;

        /// <summary>
        /// Gets or sets the location identifier by which the person's exclusions will be sourced.
        /// </summary>
        [DataMember]
        public int? LocationId { get; set; }

        /// <summary>
        /// The sequence of bits that represent engagement. The least significant bit (right side) is representative of the Sequence's
        /// StartDate. More significant bits (going left) are more recent dates.
        /// </summary>
        [DataMember]
        public byte[] EngagementMap { get; set; }

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

        #region Entity Configuration

        /// <summary>
        /// Sequence Enrollment Configuration class.
        /// </summary>
        public partial class SequenceEnrollmentConfiguration : EntityTypeConfiguration<SequenceEnrollment>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="SequenceEnrollmentConfiguration"/> class.
            /// </summary>
            public SequenceEnrollmentConfiguration()
            {
                HasRequired( se => se.Sequence ).WithMany( s => s.SequenceEnrollments ).HasForeignKey( se => se.SequenceId ).WillCascadeOnDelete( true );
                HasRequired( se => se.PersonAlias ).WithMany().HasForeignKey( se => se.PersonAliasId ).WillCascadeOnDelete( true );

                HasOptional( se => se.Location ).WithMany().HasForeignKey( se => se.LocationId ).WillCascadeOnDelete( false );
            }
        }

        #endregion Entity Configuration

        #region Update Hook

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( DbContext dbContext )
        {
            SequenceEnrollmentService.UpdateStreakPropertiesAsync( Id );
            base.PostSaveChanges( dbContext );
        }

        #endregion Update Hook
    }
}
