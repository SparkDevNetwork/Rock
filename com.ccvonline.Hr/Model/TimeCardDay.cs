using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using com.ccvonline.Hr.Data;

namespace com.ccvonline.Hr.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_com_ccvonline_Hr_TimeCardDay" )]
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
        public decimal? TotalWorkedDuration
        {
            get
            {
                // make sure this mirrors the database computed calcuation which is
                // (DATEDIFF(MINUTE, StartDateTime, isnull(EndDateTime, LunchStartDateTime)) / 60.00) - isnull((DATEDIFF(MINUTE, LunchStartDateTime, LunchEndDateTime) / 60.00), 0)
                var totalWorkedTimeSpan = ( ( EndDateTime ?? LunchStartDateTime ) - StartDateTime ) - ( ( this.LunchEndDateTime - this.LunchStartDateTime ) ?? TimeSpan.Zero );
                if ( totalWorkedTimeSpan.HasValue )
                {
                    return Convert.ToDecimal( totalWorkedTimeSpan.Value.TotalHours );
                }
                else
                {
                    return (decimal?)null;
                }
            }

            set
            {
                //
            }
        }

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

        /// <summary>
        /// WeekNumber of the year
        /// </summary>
        /// <returns></returns>
        public virtual int WeekOfYear()
        {
            var firstDayOfWeek = this.TimeCard.TimeCardPayPeriod.StartDate.DayOfWeek;
            System.Globalization.Calendar cal = new System.Globalization.GregorianCalendar( System.Globalization.GregorianCalendarTypes.USEnglish );
            return cal.GetWeekOfYear( this.StartDateTime, System.Globalization.CalendarWeekRule.FirstFullWeek, firstDayOfWeek );
        }

        /// <summary>
        /// Determines whether this time card has any hours entered.
        /// </summary>
        /// <returns></returns>
        public virtual bool HasHoursEntered()
        {
            return TotalWorkedDuration > 0 || PaidHolidayHours > 0 || PaidSickHours > 0 || PaidVacationHours > 0;
        }

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
