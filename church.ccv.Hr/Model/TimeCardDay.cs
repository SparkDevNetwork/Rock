using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using church.ccv.Hr.Data;

namespace church.ccv.Hr.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_church_ccv_Hr_TimeCardDay" )]
    [DataContract]
    public class TimeCardDay : Model<TimeCardDay>, Rock.Data.IRockEntity
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
                // make sure this mirrors the database computed calcuation (see \church.ccv.Hr\Migrations\002_AlterColumns.cs)
                TimeSpan? totalWorkedTimeSpan;
                if (!EndDateTime.HasValue)
                {
                    if ( !LunchStartDateTime.HasValue)
                    {
                        // No EndTime and no LunchStart entered yet
                        totalWorkedTimeSpan = null;
                    }
                    else
                    {
                        // No EndTime, but they did punch out for lunch
                        totalWorkedTimeSpan = LunchStartDateTime.Value - StartDateTime;
                    }
                }
                else
                {
                    if ( !LunchStartDateTime.HasValue || !LunchEndDateTime.HasValue )
                    {
                        // They entered an EndDateTime, but didn't fill out lunch, so don't subtract lunch
                        totalWorkedTimeSpan = EndDateTime.Value - StartDateTime;
                    }
                    else
                    {
                        // The entered an EndDateTime, and punched Out and In for Lunch, so subtract lunch
                        totalWorkedTimeSpan = ( EndDateTime.Value - StartDateTime ) - ( LunchEndDateTime.Value - LunchStartDateTime );
                    }
                }
                
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
        /// Gets or sets the paid holiday hours, NOT including EarnedHolidayHours
        /// </summary>
        /// <value>
        /// The paid holiday hours.
        /// </value>
        [DataMember]
        public decimal? PaidHolidayHours { get; set; }

        /// <summary>
        /// Gets or sets the earned holiday hours (earned because they worked on a holiday)
        /// </summary>
        /// <value>
        /// The earned holiday hours.
        /// </value>
        [DataMember]
        public decimal? EarnedHolidayHours { get; set; }

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
        /// Gets the total holiday hours including EarnedHolidayHours
        /// </summary>
        /// <value>
        /// The total holiday hours.
        /// </value>
        public decimal? TotalHolidayHours
        {
            get
            {
                if ( EarnedHolidayHours.HasValue )
                {
                    return ( PaidHolidayHours ?? 0 ) + ( EarnedHolidayHours ?? 0 );
                }
                else
                {
                    return PaidHolidayHours;
                }
            }
        }

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
        /// Determines whether this time card has any hours (including Holiday, Paid, and Vacation, etc) entered.
        /// </summary>
        /// <returns></returns>
        public virtual bool HasHoursEntered()
        {
            return TotalWorkedDuration > 0 || TotalHolidayHours > 0 || PaidSickHours > 0 || PaidVacationHours > 0;
        }

        /// <summary>
        /// Gets the earned holiday hours.
        /// </summary>
        /// <param name="isHoliday">if set to <c>true</c> [is holiday].</param>
        /// <returns></returns>
        public virtual decimal? GetEarnedHolidayHours( bool isHoliday )
        {
            if ( isHoliday )
            {
                var earnedHours = ( ( this.TotalWorkedDuration ?? 0 ) / 2 );
                return earnedHours.ToNearestQtrHour();
            }
            else
            {
                return null;
            }
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

    /// <summary>
    /// 
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Rounds an Hour (as Decimal) to the nearest QTR hour.
        /// Example: ToNearestQtrHr(1.55) = 1.50;
        /// </summary>
        /// <param name="hours">The hours.</param>
        /// <returns></returns>
        public static decimal? ToNearestQtrHour( this decimal? hours )
        {
            return hours.HasValue ? hours.Value.ToNearestQtrHour() : (decimal?)null;
        }

        /// <summary>
        /// Rounds an Hour (as Decimal) to the nearest QTR hour.
        /// Example: ToNearestQtrHr(1.55) = 1.50;
        /// </summary>
        /// <param name="hours">The hours.</param>
        /// <returns></returns>
        public static decimal ToNearestQtrHour( this decimal hours )
        {
            return hours - ( hours % 0.25M );
        }

        /// <summary>
        /// Rounds a TimeSpan to the nearest QTR Hour
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public static TimeSpan ToNearestQtrHour( this TimeSpan timeSpan )
        {
            return TimeSpan.FromMinutes( 15 * Math.Round( timeSpan.TotalMinutes / 15 ) );
        }

        /// <summary>
        /// Rounds a TimeSpan to the nearest QTR Hour
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns></returns>
        public static TimeSpan? ToNearestQtrHour( this TimeSpan? timeSpan )
        {
            if ( timeSpan.HasValue )
            {
                return timeSpan.Value.ToNearestQtrHour();
            }
            else
            {
                return null;
            }
        }
    }
}
