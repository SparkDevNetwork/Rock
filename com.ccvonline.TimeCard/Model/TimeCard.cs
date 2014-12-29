using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using com.ccvonline.TimeCard.Data;

namespace com.ccvonline.TimeCard.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_com_ccvonline_TimeCard_TimeCard" )]
    [DataContract]
    public class TimeCard : NamedModel<TimeCard>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the time card pay period identifier.
        /// </summary>
        /// <value>
        /// The time card pay period identifier.
        /// </value>
        [Required]
        [DataMember]
        public int TimeCardPayPeriodId { get; set; }

        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>
        /// The person alias identifier.
        /// </value>
        [Required]
        [DataMember]
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the time card status.
        /// </summary>
        /// <value>
        /// The time card status.
        /// </value>
        [Required]
        [DataMember]
        public TimeCardStatus TimeCardStatus { get; set; }

        /// <summary>
        /// Gets or sets the submitted to person alias identifier.
        /// </summary>
        /// <value>
        /// The submitted to person alias identifier.
        /// </value>
        [DataMember]
        public int? SubmittedToPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the submitted date time.
        /// </summary>
        /// <value>
        /// The submitted date time.
        /// </value>
        [DataMember]
        public DateTime? SubmittedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the approved by person alias identifier.
        /// </summary>
        /// <value>
        /// The approved by person alias identifier.
        /// </value>
        [DataMember]
        public int? ApprovedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the approved date time.
        /// </summary>
        /// <value>
        /// The approved date time.
        /// </value>
        [DataMember]
        public DateTime? ApprovedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the exported date time.
        /// </summary>
        /// <value>
        /// The exported date time.
        /// </value>
        [DataMember]
        public DateTime? ExportedDateTime { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the time card pay period.
        /// </summary>
        /// <value>
        /// The time card pay period.
        /// </value>
        public virtual TimeCardPayPeriod TimeCardPayPeriod { get; set; }

        /// <summary>
        /// Gets or sets the person alias.
        /// </summary>
        /// <value>
        /// The person alias.
        /// </value>
        public virtual Rock.Model.PersonAlias PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the submitted to person alias.
        /// </summary>
        /// <value>
        /// The submitted to person alias.
        /// </value>
        public virtual Rock.Model.PersonAlias SubmittedToPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the approved by person alias.
        /// </summary>
        /// <value>
        /// The approved by person alias.
        /// </value>
        public virtual Rock.Model.PersonAlias ApprovedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the time card days.
        /// </summary>
        /// <value>
        /// The time card days.
        /// </value>
        public virtual ICollection<TimeCardDay> TimeCardDays { get; set; }

        /// <summary>
        /// Gets or sets the time card histories.
        /// </summary>
        /// <value>
        /// The time card histories.
        /// </value>
        public virtual ICollection<TimeCardHistory> TimeCardHistories { get; set; }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TimeCardStatus
    {
        /// <summary>
        /// In Progress of entering Times
        /// </summary>
        InProgress = 0,

        /// <summary>
        /// Submitted, but not yet Approved
        /// </summary>
        Submitted = 1,

        /// <summary>
        /// Approved
        /// </summary>
        Approved = 2,

        /// <summary>
        /// Exported
        /// </summary>
        Exported = 3
    }

    /// <summary>
    /// 
    /// </summary>
    public class TimeCardConfiguration : EntityTypeConfiguration<TimeCard>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeCardConfiguration"/> class.
        /// </summary>
        public TimeCardConfiguration()
        {
            this.HasRequired( p => p.PersonAlias ).WithMany().HasForeignKey( p => p.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.SubmittedToPersonAlias ).WithMany().HasForeignKey( p => p.SubmittedToPersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.ApprovedByPersonAlias ).WithMany().HasForeignKey( p => p.ApprovedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( p => p.TimeCardPayPeriod ).WithMany().HasForeignKey( p => p.TimeCardPayPeriodId ).WillCascadeOnDelete( true );
        }
    }
}
