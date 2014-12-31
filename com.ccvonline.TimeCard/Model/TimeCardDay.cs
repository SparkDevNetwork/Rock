using System;
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
    [Table( "_com_ccvonline_TimeCard_TimeCardDay" )]
    [DataContract]
    public class TimeCardDay : Model<TimeCardDay>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the time card identifier.
        /// </summary>
        /// <value>
        /// The time card identifier.
        /// </value>
        [Required]
        [DataMember]
        public int TimeCardId { get; set; }

        /// <summary>
        /// Gets or sets the start date time.
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        [Required]
        [DataMember]
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the lunch start date time (Lunch Out)
        /// </summary>
        /// <value>
        /// The lunch start date time (Lunch Out)
        /// </value>
        [DataMember]
        public DateTime? LunchStartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the lunch end date time (Lunch In)
        /// </summary>
        /// <value>
        /// The lunch end date time (Lunch In)
        /// </value>
        [DataMember]
        public DateTime? LunchEndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the end date time.
        /// </summary>
        /// <value>
        /// The end date time.
        /// </value>
        [DataMember]
        public DateTime? EndDateTime { get; set; }

        /// <summary>
        /// Gets or sets the total duration of the worked.
        /// </summary>
        /// <value>
        /// The total duration of the worked.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        public decimal? TotalWorkedDuration { get; set; }

        /// <summary>
        /// Gets or sets the paid holiday hours.
        /// </summary>
        /// <value>
        /// The paid holiday hours.
        /// </value>
        [DataMember]
        public decimal? PaidHolidayHours { get; set; }

        /// <summary>
        /// Gets or sets the paid vacation hours.
        /// </summary>
        /// <value>
        /// The paid vacation hours.
        /// </value>
        [DataMember]
        public decimal? PaidVacationHours { get; set; }

        /// <summary>
        /// Gets or sets the paid sick hours.
        /// </summary>
        /// <value>
        /// The paid sick hours.
        /// </value>
        [DataMember]
        public decimal? PaidSickHours { get; set; }

        /// <summary>
        /// Gets or sets the notes.
        /// </summary>
        /// <value>
        /// The notes.
        /// </value>
        [MaxLength( 200 )]
        [DataMember]
        public string Notes { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the time card.
        /// </summary>
        /// <value>
        /// The time card.
        /// </value>
        public virtual TimeCard TimeCard { get; set; }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class TimeCardDayConfiguration : EntityTypeConfiguration<TimeCardDay>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeCardDayConfiguration"/> class.
        /// </summary>
        public TimeCardDayConfiguration()
        {
            this.HasRequired( a => a.TimeCard ).WithMany( a => a.TimeCardDays ).HasForeignKey( a => a.TimeCardId ).WillCascadeOnDelete( true );
        }
    }
}
