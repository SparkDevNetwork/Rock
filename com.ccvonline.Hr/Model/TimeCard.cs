using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using com.ccvonline.Hr.Data;
using Rock;

namespace com.ccvonline.Hr.Model
{
    /// <summary>
    /// 
    /// </summary>
    [Table( "_com_ccvonline_Hr_TimeCard" )]
    [DataContract]
    public class TimeCard : Model<TimeCard>
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

        #region methods

        /// <summary>
        /// Totals the worked hours per week.
        /// </summary>
        /// <param name="includeRegularDates">if set to <c>true</c> [include regular dates].</param>
        /// <param name="includeHolidayDates">if set to <c>true</c> [include holiday dates].</param>
        /// <returns></returns>
        public List<HoursPerTimeCardDay> GetTotalWorkedHoursPerDay( bool includeRegularDates, bool includeHolidayDates )
        {
            Rock.Model.Schedule timeCardHolidaySchedule = new Rock.Model.ScheduleService( new Rock.Data.RockContext() ).Get( com.ccvonline.Hr.SystemGuid.Schedule.TIMECARD_HOLIDAY_SCHEDULE.AsGuid() );
            List<DateTime> holidayDates = new List<DateTime>();
            if ( timeCardHolidaySchedule != null )
            {
                DDay.iCal.Event calEvent = timeCardHolidaySchedule.GetCalenderEvent();
                if ( calEvent != null )
                {
                    holidayDates = calEvent.GetOccurrences( this.TimeCardPayPeriod.StartDate, this.TimeCardPayPeriod.EndDate ).Select( a => a.Period.StartTime.Date ).ToList();
                }
            }

            var timeCardDaysQry = TimeCardDays.AsQueryable();

            // note: coded as "else ifs vs if,if" to make it more readable
            if ( includeRegularDates && !includeHolidayDates )
            {
                timeCardDaysQry = timeCardDaysQry.Where( a => !holidayDates.Contains( a.StartDateTime.Date ) );
            }
            else if ( !includeRegularDates && includeHolidayDates )
            {
                timeCardDaysQry = timeCardDaysQry.Where( a => holidayDates.Contains( a.StartDateTime.Date ) );
            }
            else if ( !includeRegularDates && !includeHolidayDates )
            {
                timeCardDaysQry = timeCardDaysQry.Where( a => 1 == 0 );
            }
            else
            {
                // include all worked hours if includeRegularDates and includeHolidayDates
            }

            return timeCardDaysQry.Select( x => new HoursPerTimeCardDay
            {
                TimeCardDay = x,
                Hours = x.TotalWorkedDuration
            } ).ToList();
        }

        /// <summary>
        /// Gets the worked holiday hours.
        /// </summary>
        /// <returns></returns>
        public List<HoursPerTimeCardDay> GetWorkedHolidayHours()
        {
            // Number of hours/week where the person worked on a holiday 
            return GetTotalWorkedHoursPerDay( false, true );
        }

        /// <summary>
        /// Gets the regular hours.
        /// </summary>
        /// <returns></returns>
        public List<HoursPerTimeCardDay> GetRegularHours()
        {
            /// Number of hours/week where the person worked, up to 40 hours, then subtract HolidayWorkedHours()
            /// The idea is that if a person is getting paid extra for HolidayWorkedHours, those won't count towards regular time
            var totalWorkedHoursPerWeekMax40 = this.GetTotalWorkedHoursPerDay( true, true ).Select( a => new HoursPerTimeCardDay
            {
                TimeCardDay = a.TimeCardDay,
                Hours = a.Hours
            } ).ToList();

            Dictionary<int, decimal> totalRegularByWeek = new Dictionary<int, decimal>();
            foreach ( var day in totalWorkedHoursPerWeekMax40 )
            {
                int weekOfYear = day.TimeCardDay.WeekOfYear();
                if ( !totalRegularByWeek.ContainsKey( weekOfYear ) )
                {
                    totalRegularByWeek.Add( weekOfYear, 0 );
                }

                decimal totalRegular = totalRegularByWeek[weekOfYear];

                if ( totalRegular < 40 )
                {
                    // if less than 40 so far, increment the total by the day's hours
                    totalRegular += day.Hours ?? 0;
                    if ( totalRegular > 40 )
                    {
                        // if we spilled over 40, only count the hours for this day before the 40 was reached
                        day.Hours -= totalRegular - 40;
                        totalRegular = 40;
                    }
                }
                else
                {
                    // if we already worked 40 hours, don't count additional hours as regular hours
                    day.Hours = 0;
                }

                totalRegularByWeek[weekOfYear] = totalRegular;
            }

            var totalWorkedHolidayHours = GetWorkedHolidayHours();

            var regularHours = totalWorkedHoursPerWeekMax40.ToList();
            foreach ( var week in regularHours )
            {
                var holidayHours = totalWorkedHolidayHours.FirstOrDefault( a => a.TimeCardDay.Id == week.TimeCardDay.Id );
                if ( holidayHours != null )
                {
                    week.Hours = week.Hours - holidayHours.Hours;
                }
            }

            return regularHours;
        }

        /// <summary>
        /// Gets the overtime hours.
        /// </summary>
        /// <returns></returns>
        public List<HoursPerTimeCardDay> GetOvertimeHours()
        {
            // Overtime hours is TotalWorkedHoursPerWeek(true, true) – RegularHours() – HolidayWorkedHours() 
            var totalWorkedHoursPerDay = this.GetTotalWorkedHoursPerDay( true, true );
            var regularHoursPerDay = this.GetRegularHours();
            var workedHolidayHoursPerDay = this.GetWorkedHolidayHours();

            var overtimeHours = totalWorkedHoursPerDay;
            foreach ( var day in overtimeHours )
            {
                var regularHours = regularHoursPerDay.FirstOrDefault( a => a.TimeCardDay.Id == day.TimeCardDay.Id );
                if ( regularHours != null )
                {
                    day.Hours = day.Hours - regularHours.Hours;
                }

                var holidayWorkedHours = workedHolidayHoursPerDay.FirstOrDefault( a => a.TimeCardDay.Id == day.TimeCardDay.Id );
                if ( holidayWorkedHours != null )
                {
                    day.Hours = day.Hours - holidayWorkedHours.Hours;
                }
            }

            return overtimeHours;
        }

        /// <summary>
        /// Paids the vacation hours.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, IEnumerable<TimeCardDay>> GroupByWeekNum( IEnumerable<TimeCardDay> timeCardDays = null )
        {
            timeCardDays = timeCardDays ?? this.TimeCardDays;
            var result = timeCardDays.ToList().Select( a => new
            {
                WeekOfYear = a.WeekOfYear(),
                TimeCardDay = a
            } )
            .GroupBy( a => a.WeekOfYear )
            .ToDictionary( k => k.Key, v => v.Select( a => a.TimeCardDay ) );

            return result;
        }

        /// <summary>
        /// Paids the vacation hours.
        /// </summary>
        /// <returns></returns>
        public List<HoursPerWeek> PaidVacationHours()
        {
            return this.GroupByWeekNum().Select( x => new HoursPerWeek
            {
                WeekOfYear = x.Key,
                Hours = x.Value.Sum( xx => xx.PaidVacationHours )
            } ).ToList();
        }

        /// <summary>
        /// Paids the holiday hours.
        /// </summary>
        /// <returns></returns>
        public List<HoursPerWeek> PaidHolidayHours()
        {
            return this.GroupByWeekNum().Select( x => new HoursPerWeek
            {
                WeekOfYear = x.Key,
                Hours = x.Value.Sum( xx => xx.PaidHolidayHours )
            } ).ToList();
        }

        /// <summary>
        /// Paids the sick hours.
        /// </summary>
        /// <returns></returns>
        public List<HoursPerWeek> PaidSickHours()
        {
            return this.GroupByWeekNum().Select( x => new HoursPerWeek
            {
                WeekOfYear = x.Key,
                Hours = x.Value.Sum( xx => xx.PaidSickHours )
            } ).ToList();
        }

        /// <summary>
        /// Determines whether any of the time cards have hours entered.
        /// </summary>
        /// <returns></returns>
        public bool HasHoursEntered()
        {
            return this.TimeCardDays.Any( a => a.HasHoursEntered() );
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class HoursPerTimeCardDay
    {
        /// <summary>
        /// Gets or sets the week start date.
        /// </summary>
        /// <value>
        /// The week start date.
        /// </value>
        public TimeCardDay TimeCardDay { get; set; }

        /// <summary>
        /// Gets or sets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>
        public decimal? Hours { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if ( TimeCardDay != null )
            {
                return string.Format( "{0} {1}", TimeCardDay.StartDateTime.Date.ToShortDateString(), Hours );
            }

            return base.ToString();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class HoursPerWeek
    {
        /// <summary>
        /// Gets or sets the week of year.
        /// </summary>
        /// <value>
        /// The week of year.
        /// </value>
        public int WeekOfYear { get; set; }

        /// <summary>
        /// Gets or sets the hours.
        /// </summary>
        /// <value>
        /// The hours.
        /// </value>
        public decimal? Hours { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format( "{0} {1}", WeekOfYear, Hours );
        }
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
