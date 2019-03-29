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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

using DDay.iCal;

using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Scheduled event in Rock.  Several places where this has been used includes Check-in scheduling and Kiosk scheduling.
    /// </summary>
    [RockDomain( "Core" )]
    [Table( "Schedule" )]
    [DataContract]
    public partial class Schedule : Model<Schedule>, ICategorized, IHasActiveFlag
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Name of the Schedule. This property is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the Name of the Schedule.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        [IncludeForReporting]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a user defined Description of the Schedule.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Description of the Schedule.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the content lines of the iCalendar
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/>representing the  content of the iCalendar.
        /// </value>
        [DataMember]
        public string iCalendarContent
        {
            get
            {
                return _iCalendarContent ?? string.Empty;
            }
            set
            {
                _iCalendarContent = value;
            }
        }
        private string _iCalendarContent;

        /// <summary>
        /// Gets or sets the number of minutes prior to the Schedule's start time  that Check-in should be active. 0 represents that Check-in 
        /// will not be available to the beginning of the event.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing how many minutes prior the Schedule's start time that Check-in should be active. 
        /// 0 means that Check-in will not be available to the Schedule's start time. This schedule will not be available if this value is <c>Null</c>.
        /// </value>
        [DataMember]
        public int? CheckInStartOffsetMinutes { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes following schedule start that Check-in should be active. 0 represents that Check-in will only be available
        /// until the Schedule's start time.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing how many minutes following the Schedule's end time that Check-in should be active. 0 represents that Check-in
        /// will only be available until the Schedule's start time.
        /// </value>
        [DataMember]
        public int? CheckInEndOffsetMinutes { get; set; }

        /// <summary>
        /// Gets or sets the Date that the Schedule becomes effective/active. This property is inclusive, and the schedule will be inactive before this date. 
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> value that represents the date that this Schedule becomes active.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EffectiveStartDate { get; set; }

        /// <summary>
        /// Gets or sets that date that this Schedule expires and becomes inactive. This value is inclusive and the schedule will be inactive after this date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> value that represents the date that this Schedule ends and becomes inactive.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EffectiveEndDate { get; set; }

        /// <summary>
        /// Gets or sets the weekly day of week.
        /// </summary>
        /// <value>
        /// The weekly day of week.
        /// </value>
        [DataMember]
        public DayOfWeek? WeeklyDayOfWeek { get; set; }

        /// <summary>
        /// Gets or sets the weekly time of day.
        /// </summary>
        /// <value>
        /// The weekly time of day.
        /// </value>
        [DataMember]
        public TimeSpan? WeeklyTimeOfDay { get; set; }

        /// <summary>
        /// Gets or sets the CategoryId of the <see cref="Rock.Model.Category"/> that this Schedule belongs to.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the CategoryId of the <see cref="Rock.Model.Category"/> that this Schedule belongs to. This property will be null
        /// if the Schedule does not belong to a Category.
        /// </value>
        [DataMember]
        [IncludeForReporting]
        public int? CategoryId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets a value indicating whether Check-in is enabled for this Schedule.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this instance is check in enabled; otherwise, <c>false</c>.
        /// <remarks>
        /// The <c>CheckInStartOffsetMinutes</c> is used to determine if Check-in is enabled. If the value is <c>null</c>, it is determined that Check-in is not 
        /// enabled for this Schedule.
        /// </remarks>
        /// </value>
        public virtual bool IsCheckInEnabled
        {
            get
            {
                return CheckInStartOffsetMinutes.HasValue && IsActive;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this schedule is currently active.
        /// </summary>
        /// <value>
        /// <c>true</c> if this schedule is currently active; otherwise, <c>false</c>.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use WasScheduleActive( DateTime time ) method instead.", false )]
        public virtual bool IsScheduleActive
        {
            get
            {
                return WasScheduleActive( RockDateTime.Now );
            }
        }

        /// <summary>
        /// Gets a value indicating whether check-in is currently active for this Schedule.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> that is  <c>true</c> if Check-in is currently active for this Schedule ; otherwise, <c>false</c>.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use WasCheckInActive( DateTime time ) method instead.", false )]
        public virtual bool IsCheckInActive
        {
            get
            {
                return WasCheckInActive( RockDateTime.Now );
            }
        }

        /// <summary>
        /// Gets a value indicating whether this schedule (or it's check-in window) is currently active.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is schedule or checkin active; otherwise, <c>false</c>.
        /// </value>
        [RockObsolete( "1.8" )]
        [Obsolete( "Use WasScheduleOrCheckInActive( DateTime time ) method instead.", false )]
        public virtual bool IsScheduleOrCheckInActive
        {
            get
            {
                return WasScheduleOrCheckInActive( RockDateTime.Now );
            }
        }

        /// <summary>
        /// Gets the type of the schedule.
        /// </summary>
        /// <value>
        /// The type of the schedule.
        /// </value>
        public virtual ScheduleType ScheduleType
        {
            get
            {
                DDay.iCal.Event calendarEvent = this.GetCalendarEvent();
                if ( calendarEvent != null && calendarEvent.DTStart != null )
                {
                    return !string.IsNullOrWhiteSpace( this.Name ) ?
                        ScheduleType.Named : ScheduleType.Custom;
                }
                else
                {
                    return WeeklyDayOfWeek.HasValue ?
                        ScheduleType.Weekly : ScheduleType.None;
                }
            }
        }

        /// <summary>
        /// Gets the next start time.
        /// </summary>
        /// <returns></returns>
        [NotMapped]
        [LavaInclude]
        [RockObsolete( "1.8" )]
        [Obsolete( "Use GetNextStartDateTime( DateTime currentDateTime ) instead." )]
        public virtual DateTime? NextStartDateTime
        {
            get
            {
                return GetNextStartDateTime( RockDateTime.Now );
            }
        }

        /// <summary>
        /// Gets the next start date time.
        /// </summary>
        /// <param name="currentDateTime">The current date time.</param>
        /// <returns></returns>
        public DateTime? GetNextStartDateTime( DateTime currentDateTime )
        {
            if ( this.IsActive )
            {
                var occurrences = GetScheduledStartTimes( currentDateTime, currentDateTime.AddYears( 1 ) );
                return occurrences.Min( o => (DateTime?)o );
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the first start date time.
        /// </summary>
        /// <value>
        /// The first start date time.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual DateTime? FirstStartDateTime => GetFirstStartDateTime();

        /// <summary>
        /// Gets the first start date time this week.
        /// </summary>
        /// <value>
        /// The first start date time this week.
        /// </value>
        [NotMapped]
        [LavaInclude]
        public virtual DateTime? FirstStartDateTimeThisWeek
        {
            get
            {
                var endDate = RockDateTime.Today.SundayDate();
                var startDate = endDate.AddDays( -7 );
                var occurrences = GetScheduledStartTimes( startDate, endDate );
                return occurrences.Min( o => (DateTime?)o );
            }
        }

        /// <summary>
        /// Gets the start time of day.
        /// </summary>
        /// <value>
        /// The start time of day.
        /// </value>
        [LavaInclude]
        public virtual TimeSpan StartTimeOfDay
        {
            get
            {
                DDay.iCal.Event calendarEvent = this.GetCalendarEvent();
                if ( calendarEvent != null && calendarEvent.DTStart != null )
                {
                    return calendarEvent.DTStart.TimeOfDay;
                }

                if ( WeeklyTimeOfDay.HasValue )
                {
                    return WeeklyTimeOfDay.Value;
                }

                return new TimeSpan();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Category"/> that this Schedule belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Category"/> that this Schedule belongs to.  If it does not belong to a <see cref="Rock.Model.Category"/> this value will be null.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets the friendly schedule text.
        /// </summary>
        /// <value>
        /// The friendly schedule text.
        /// </value>
        [LavaInclude]
        [DataMember]
        public virtual string FriendlyScheduleText
        {
            get { return ToFriendlyScheduleText(); }
        }

        /// <summary>
        /// Gets or sets a flag indicating if this is an active schedule. This value is required.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> value that is <c>true</c> if this schedule is active, otherwise <c>false</c>.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        [Previewable]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        #endregion

        #region Public Methods

        /// <summary>
        /// Pres the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="state">The state.</param>
        public override void PreSaveChanges( Data.DbContext dbContext, EntityState state )
        {
            var calEvent = GetCalendarEvent();
            if ( calEvent != null )
            {
                EffectiveStartDate = calEvent.DTStart != null ? calEvent.DTStart.Value.Date : (DateTime?)null;
                EffectiveEndDate = calEvent.DTEnd != null ? calEvent.DTEnd.Value.Date : (DateTime?)null;
            }

            base.PreSaveChanges( dbContext, state );
        }

        /// <summary>
        /// Gets the Schedule's iCalender Event.
        /// </summary>
        /// <value>
        /// A <see cref="DDay.iCal.Event"/> representing the iCalendar event for this Schedule.
        /// </value>
        [Obsolete( "Use GetCalendarEvent() instead " )]
        public virtual DDay.iCal.Event GetCalenderEvent()
        {
            return ScheduleICalHelper.GetCalendarEvent( iCalendarContent );
        }

        /// <summary>
        /// Gets the Schedule's iCalender Event.
        /// </summary>
        /// <value>
        /// A <see cref="DDay.iCal.Event"/> representing the iCalendar event for this Schedule.
        /// </value>
        public virtual DDay.iCal.Event GetCalendarEvent()  
        {
            return ScheduleICalHelper.GetCalendarEvent( iCalendarContent );
        }

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        public IList<Occurrence> GetOccurrences( DateTime beginDateTime, DateTime? endDateTime = null )
        {
            var occurrences = new List<Occurrence>();

            DDay.iCal.Event calEvent = GetCalendarEvent();
            if ( calEvent != null && calEvent.DTStart != null )
            {
                var exclusionDates = new List<DateRange>();
                if ( this.CategoryId.HasValue && this.CategoryId.Value > 0 )
                {
                    var category = CategoryCache.Get( this.CategoryId.Value );
                    if ( category != null )
                    {
                        exclusionDates = category.ScheduleExclusions
                            .Where( e => e.Start.HasValue && e.End.HasValue )
                            .ToList();
                    }
                }

                foreach( var occurrence in endDateTime.HasValue ?
                    ScheduleICalHelper.GetOccurrences( calEvent, beginDateTime, endDateTime.Value ) :
                    ScheduleICalHelper.GetOccurrences( calEvent, beginDateTime ) )
                {
                    bool exclude = false;
                    if ( exclusionDates.Any() && occurrence.Period.StartTime != null )
                    {
                        var occurrenceStart = occurrence.Period.StartTime.Value;
                        if ( exclusionDates.Any( d =>
                            d.Start.Value <= occurrenceStart &&
                            d.End.Value >= occurrenceStart ) )
                        {
                            exclude = true;
                        }
                    }

                    if ( !exclude )
                    {
                        occurrences.Add( occurrence );
                    }
                }
            }

            return occurrences;
        }

        /// <summary>
        /// Gets the check in times.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <returns></returns>
        public virtual List<CheckInTimes> GetCheckInTimes( DateTime beginDateTime )
        {
            var result = new List<CheckInTimes>();

            if ( IsCheckInEnabled )
            {
                var occurrences = GetOccurrences( beginDateTime, beginDateTime.Date.AddDays( 1 ) );
                foreach ( var occurrence in occurrences
                    .Where( a =>
                        a.Period != null &&
                        a.Period.StartTime != null &&
                        a.Period.EndTime != null )
                    .Select( a => new {
                        Start = a.Period.StartTime.Value,
                        End = a.Period.EndTime.Value 
                    }) )
                {
                    var checkInTimes = new CheckInTimes();
                    checkInTimes.Start = DateTime.SpecifyKind( occurrence.Start, DateTimeKind.Local );
                    checkInTimes.End = DateTime.SpecifyKind( occurrence.End, DateTimeKind.Local );
                    checkInTimes.CheckInStart = checkInTimes.Start.AddMinutes( 0 - CheckInStartOffsetMinutes.Value );
                    if ( CheckInEndOffsetMinutes.HasValue )
                    {
                        checkInTimes.CheckInEnd = checkInTimes.Start.AddMinutes( CheckInEndOffsetMinutes.Value );
                    }
                    else
                    {
                        checkInTimes.CheckInEnd = checkInTimes.End;
                    }

                    result.Add( checkInTimes );
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the next check in start time.
        /// </summary>
        /// <param name="begindateTime">The begindate time.</param>
        /// <returns></returns>
        public virtual DateTime? GetNextCheckInStartTime( DateTime begindateTime )
        {
            var checkInTimes = GetCheckInTimes( begindateTime );
            if ( checkInTimes != null && checkInTimes.Any() )
            {
                return checkInTimes.FirstOrDefault().CheckInStart;
            }

            return null;
        }

        /// <summary>
        /// Gets a list of scheduled start datetimes between the two specified dates, sorted by datetime.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        public virtual List<DateTime> GetScheduledStartTimes( DateTime beginDateTime, DateTime endDateTime )
        {
            var result = new List<DateTime>();

            var occurrences = GetOccurrences( beginDateTime, endDateTime );
            foreach ( var startDateTime in occurrences
                .Where( a =>
                    a.Period != null &&
                    a.Period.StartTime != null )
                .Select( a => a.Period.StartTime.Value ) )
            {
                // ensure the the datetime is DateTimeKind.Local since iCal returns DateTimeKind.UTC
                result.Add( DateTime.SpecifyKind( startDateTime, DateTimeKind.Local ) );
            }

            return result;
        }


        /// <summary>
        /// Gets the first start date time.
        /// </summary>
        /// <returns></returns>
        public virtual DateTime? GetFirstStartDateTime()
        {
            DateTime? firstStartTime = null;
            
            if ( this.EffectiveStartDate.HasValue )
            {
                var scheduledStartTimes = this.GetScheduledStartTimes( this.EffectiveStartDate.Value, this.EffectiveStartDate.Value.AddMonths( 1 ) );
                if ( scheduledStartTimes.Count > 0 )
                {
                    firstStartTime = scheduledStartTimes[0];
                }
            }

            return firstStartTime;
        }

        /// <summary>
        /// Determines whether this instance has a non-empty schedule.
        /// </summary>
        /// <returns></returns>
        public virtual bool HasSchedule()
        {
            DDay.iCal.Event calEvent = GetCalendarEvent();
            if ( calEvent != null && calEvent.DTStart != null )
            {
                return true;
            }
            else
            {
                // if there is no CalEvent, it might be scheduled using WeeklyDayOfWeek
                return WeeklyDayOfWeek.HasValue;
            }
        }

        /// <summary>
        /// returns true if there is a blank schedule or a schedule that is incomplete
        /// </summary>
        /// <returns></returns>
        public virtual bool HasScheduleWarning()
        {
            DDay.iCal.Event calEvent = GetCalendarEvent();
            if ( calEvent != null && calEvent.DTStart != null )
            {
                if ( calEvent.RecurrenceRules.Any() )
                {
                    IRecurrencePattern rrule = calEvent.RecurrenceRules[0];
                    if ( rrule.Frequency == FrequencyType.Weekly )
                    {
                        // if it has a Weekly schedule, but no days are selected, return true that is has a warning
                        if ( !rrule.ByDay.Any() )
                        {
                            return true;
                        }
                    }
                    else if ( rrule.Frequency == FrequencyType.Monthly )
                    {
                        // if it has a Monthly schedule, but not configured, return true that is has a warning
                        if ( !rrule.ByDay.Any() && !rrule.ByMonthDay.Any() )
                        {
                            return true;
                        }
                    }
                }

                // is scheduled, and doesn't have any warnings
                return false;
            }
            else
            {
                // if there is no CalEvent, it might be scheduled using WeeklyDayOfWeek, but if it isn't, return true that is has a warning
                return !WeeklyDayOfWeek.HasValue;
            }
        }

        /// <summary>
        /// Gets the Friendly Text of the Calendar Event.
        /// For example, "Every 3 days at 10:30am", "Monday, Wednesday, Friday at 5:00pm", "Saturday at 4:30pm"
        /// </summary>
        /// <returns>A <see cref="System.String"/> containing a friendly description of the Schedule.</returns>
        public string ToFriendlyScheduleText()
        {
            return ToFriendlyScheduleText( false );
        }

        /// <summary>
        /// Gets the Friendly Text of the Calendar Event.
        /// For example, "Every 3 days at 10:30am", "Monday, Wednesday, Friday at 5:00pm", "Saturday at 4:30pm"
        /// </summary>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns>
        /// A <see cref="System.String" /> containing a friendly description of the Schedule.
        /// </returns>
        public string ToFriendlyScheduleText( bool condensed )
        {
            // init the result to just the schedule name just in case we can't figure out the FriendlyText
            string result = this.Name;

            DDay.iCal.Event calendarEvent = this.GetCalendarEvent();
            if ( calendarEvent != null && calendarEvent.DTStart != null )
            {
                string startTimeText = calendarEvent.DTStart.Value.TimeOfDay.ToTimeString();
                if ( calendarEvent.RecurrenceRules.Any() )
                {
                    // some type of recurring schedule

                    IRecurrencePattern rrule = calendarEvent.RecurrenceRules[0];
                    switch ( rrule.Frequency )
                    {
                        case FrequencyType.Daily:
                            result = "Daily";

                            if ( rrule.Interval > 1 )
                            {
                                result += string.Format( " every {0} days", rrule.Interval );
                            }

                            result += " at " + startTimeText;

                            break;

                        case FrequencyType.Weekly:

                            result = rrule.ByDay.Select( a => a.DayOfWeek.ConvertToString().Pluralize() ).ToList().AsDelimited( "," );
                            if ( string.IsNullOrEmpty( result ) )
                            {
                                // no day selected, so it has an incomplete schedule
                                return "No Scheduled Days";
                            }

                            if ( rrule.Interval > 1 )
                            {
                                result = string.Format( "Every {0} weeks: ", rrule.Interval ) + result;
                            }
                            else
                            {
                                result = "Weekly: " + result;
                            }

                            result += " at " + startTimeText;

                            break;

                        case FrequencyType.Monthly:

                            if ( rrule.ByMonthDay.Count > 0 )
                            {
                                // Day X of every X Months (we only support one day in the ByMonthDay list)
                                int monthDay = rrule.ByMonthDay[0];
                                result = string.Format( "Day {0} of every ", monthDay );
                                if ( rrule.Interval > 1 )
                                {
                                    result += string.Format( "{0} months", rrule.Interval );
                                }
                                else
                                {
                                    result += "month";
                                }

                                result += " at " + startTimeText;
                            }
                            else if ( rrule.ByDay.Count > 0 )
                            {
                                // The Nth <DayOfWeekName> (we only support one day in the ByDay list)
                                IWeekDay bydate = rrule.ByDay[0];
                                if ( NthNames.ContainsKey( bydate.Offset ) )
                                {
                                    result = string.Format( "The {0} {1} of every month", NthNames[bydate.Offset], bydate.DayOfWeek.ConvertToString() );
                                }
                                else
                                {
                                    // unsupported case (just show the name)
                                }

                                result += " at " + startTimeText;
                            }
                            else
                            {
                                // unsupported case (just show the name)
                            }

                            break;

                        default:
                            // some other type of recurring type (probably specific dates).  Just return the Name of the schedule

                            break;
                    }
                }
                else
                {
                    // not any type of recurring, might be one-time or from specific dates, etc
                    var dates = calendarEvent.GetOccurrences( DateTime.MinValue, DateTime.MaxValue ).Where( a =>
                                a.Period != null &&
                                a.Period.StartTime != null )
                            .Select( a => a.Period.StartTime.Value )
                            .OrderBy( a => a ).ToList();

                    if ( dates.Count() > 1 )
                    {
                        if ( condensed || dates.Count() > 99 )
                        {
                            result = string.Format( "Multiple dates between {0} and {1}", dates.First().ToShortDateString(), dates.Last().ToShortDateString() );
                        }
                        else
                        {

                            var listHtml = "<ul class='list-unstyled'>" + Environment.NewLine;
                            foreach ( var date in dates )
                            {
                                listHtml += string.Format( "<li>{0}</li>", date.ToShortDateTimeString() ) + Environment.NewLine;
                            }

                            listHtml += "</ul>";

                            result = listHtml;
                        }
                    }
                    else if ( dates.Count() == 1)
                    {
                        result = "Once at " + calendarEvent.DTStart.Value.ToShortDateTimeString();
                    }
                    else
                    {
                        return "No Schedule";
                    }
                }
            }
            else
            {
                if ( WeeklyDayOfWeek.HasValue )
                {
                    result = WeeklyDayOfWeek.Value.ConvertToString();
                    if ( WeeklyTimeOfDay.HasValue )
                    {
                        result += " at " + WeeklyTimeOfDay.Value.ToTimeString();
                    }
                }
                else
                {
                    // no start time.  Nothing scheduled
                    return "No Schedule";
                }
            }

            return result;
        }

        /// <summary>
        /// Returns value indicating if the schedule was active at a current time.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public bool WasScheduleActive( DateTime time )
        {
            var calEvent = this.GetCalendarEvent();
            if ( calEvent != null && calEvent.DTStart != null )
            {
                if ( time.TimeOfDay.TotalSeconds < calEvent.DTStart.TimeOfDay.TotalSeconds )
                {
                    return false;
                }

                if ( time.TimeOfDay.TotalSeconds > calEvent.DTEnd.TimeOfDay.TotalSeconds )
                {
                    return false;
                }

                var occurrences = GetOccurrences( time.Date );
                return occurrences.Count > 0;
            }

            return false;
        }

        /// <summary>
        /// Returns value indicating if check-in was active at a current time for this schedule.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public bool WasCheckInActive( DateTime time )
        {
            if ( !IsCheckInEnabled )
            {
                return false;
            }

            var calEvent = this.GetCalendarEvent();
            if ( calEvent != null && calEvent.DTStart != null )
            {
                var checkInStart = calEvent.DTStart.AddMinutes( 0 - CheckInStartOffsetMinutes.Value );
                if ( time.TimeOfDay.TotalSeconds < checkInStart.TimeOfDay.TotalSeconds )
                {
                    return false;
                }

                var checkInEnd = calEvent.DTEnd;
                if ( CheckInEndOffsetMinutes.HasValue )
                {
                    checkInEnd = calEvent.DTStart.AddMinutes( CheckInEndOffsetMinutes.Value );
                }

                // If compare is greater than zero, then check-in offset end resulted in an end time in next day, in 
                // which case, don't need to compare time
                int checkInEndDateCompare = checkInEnd.Date.CompareTo( checkInStart.Date );

                if ( checkInEndDateCompare < 0 )
                {
                    // End offset is prior to start (Would have required a neg number entered)
                    return false;
                }

                if ( checkInEndDateCompare == 0 && time.TimeOfDay.TotalSeconds > checkInEnd.TimeOfDay.TotalSeconds )
                {
                    // Same day, but end time has passed
                    return false;
                }

                var occurrences = GetOccurrences( time.Date );
                return occurrences.Count > 0;
            }

            return false;
        }

        /// <summary>
        /// Returns value indicating if check-in was active at a current time for this schedule.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public bool WasScheduleOrCheckInActive( DateTime time )
        {
            return WasScheduleActive( time ) || WasCheckInActive( time );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.ToFriendlyScheduleText();
        }

        #endregion

        #region consts

        /// <summary>
        /// The "nth" names for DayName of Month (First, Second, Third, Forth, Last)
        /// </summary>
        public static readonly Dictionary<int, string> NthNames = new Dictionary<int, string> { 
            {1, "First"}, 
            {2, "Second"}, 
            {3, "Third"}, 
            {4, "Fourth"}, 
            {-1, "Last"} 
        };

        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// File Configuration class.
    /// </summary>
    public partial class ScheduleConfiguration : EntityTypeConfiguration<Schedule>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleConfiguration"/> class.
        /// </summary>
        public ScheduleConfiguration()
        {
            this.HasOptional( s => s.Category ).WithMany().HasForeignKey( s => s.CategoryId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// Schedule Type
    /// </summary>
    [Flags]
    public enum ScheduleType
    {
        /// <summary>
        /// None
        /// </summary>
        None = 0,

        /// <summary>
        /// Weekly
        /// </summary>
        Weekly = 1,

        /// <summary>
        /// Custom
        /// </summary>
        Custom = 2,

        /// <summary>
        /// Custom
        /// </summary>
        Named = 4,
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Start/End Times for Check-in
    /// </summary>
    public class CheckInTimes
    {
        /// <summary>
        /// Gets or sets the start.
        /// </summary>
        /// <value>
        /// The start.
        /// </value>
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the end.
        /// </summary>
        /// <value>
        /// The end.
        /// </value>
        public DateTime End { get; set; }

        /// <summary>
        /// Gets or sets the check in start.
        /// </summary>
        /// <value>
        /// The check in start.
        /// </value>
        public DateTime CheckInStart { get; set; }

        /// <summary>
        /// Gets or sets the check in end.
        /// </summary>
        /// <value>
        /// The check in end.
        /// </value>
        public DateTime CheckInEnd { get; set; }
    }

    /// <summary>
    /// Helper class for grouping attendance records associated into logical occurrences based on
    /// a given schedule
    /// </summary>
    public class ScheduleOccurrence
    {
        /// <summary>
        /// Gets or sets the logical occurrence date of the occurrence
        /// </summary>
        /// <value>
        /// The occurrence date.
        /// </value>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the logical start date/time ( only used for ordering )
        /// </summary>
        /// <value>
        /// The start date time.
        /// </value>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Gets or sets the schedule identifier.
        /// </summary>
        /// <value>
        /// The schedule identifier.
        /// </value>
        public int? ScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the name of the schedule.
        /// </summary>
        /// <value>
        /// The name of the schedule.
        /// </value>
        public string ScheduleName { get; set; }

        /// <summary>
        /// Gets or sets the location identifier.
        /// </summary>
        /// <value>
        /// The location identifier.
        /// </value>
        public int? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the location.
        /// </summary>
        /// <value>
        /// The name of the location.
        /// </value>
        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets the location path.
        /// </summary>
        /// <value>
        /// The location path.
        /// </value>
        public string LocationPath { get; set; }

        /// <summary>
        /// Gets or sets the total count.
        /// </summary>
        /// <value>
        /// The total count.
        /// </value>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the did attend count.
        /// </summary>
        /// <value>
        /// The did attend count.
        /// </value>
        public int DidAttendCount { get; set; }

        /// <summary>
        /// Gets or sets the did not occur count.
        /// </summary>
        /// <value>
        /// The did not occur count.
        /// </value>
        public int DidNotOccurCount { get; set; }

        /// <summary>
        /// Gets a value indicating whether attendance has been entered for this occurrence.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [attendance entered]; otherwise, <c>false</c>.
        /// </value>
        public bool AttendanceEntered
        {
            get
            {
                return DidAttendCount > 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether occurrence did not occur for the selected
        /// start time. This is determined by not having any attendance records with 
        /// a 'DidAttend' value, and at least one attendance record with 'DidNotOccur'
        /// value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [did not occur]; otherwise, <c>false</c>.
        /// </value>
        public bool DidNotOccur
        {
            get
            {
                return DidAttendCount <= 0 && DidNotOccurCount > 0;
            }
        }

        /// <summary>
        /// Gets the attendance percentage.
        /// </summary>
        /// <value>
        /// The percentage.
        /// </value>
        public double Percentage
        {
            get
            {
                if ( TotalCount > 0 )
                {
                    return (double)( DidAttendCount ) / (double)TotalCount;
                }
                else
                {
                    return 0.0d;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleOccurrence" /> class.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="scheduleName">Name of the schedule.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="locationName">Name of the location.</param>
        /// <param name="locationPath">The location path.</param>
        /// ,
        public ScheduleOccurrence( DateTime date, TimeSpan startTime, int? scheduleId = null, string scheduleName = "", int? locationId = null, string locationName = "", string locationPath = "" )
        {
            Date = date;
            StartTime = startTime;
            ScheduleId = scheduleId;
            ScheduleName = scheduleName;
            LocationId = locationId;
            LocationName = locationName;
            LocationPath = locationPath;
        }
    }

    /// <summary>
    /// DDay.ical LoadFromStream is not threadsafe, so use locking
    /// </summary>
    public static class ScheduleICalHelper
    {
        private static object _initLock;
        private static Dictionary<string, DDay.iCal.Event> _iCalSchedules = new Dictionary<string, Event>();

        static ScheduleICalHelper()
        {
            ScheduleICalHelper._initLock = new object();
        }

        /// <summary>
        /// Gets the calendar event.
        /// </summary>
        /// <param name="iCalendarContent">Content of the i calendar.</param>
        /// <returns></returns>
        [Obsolete( "Use GetCalendarEvent( iCalendarContent ) instead " )]
        public static DDay.iCal.Event GetCalenderEvent( string iCalendarContent )
        {
            return GetCalendarEvent( iCalendarContent );
        }

        /// <summary>
        /// Gets the calendar event.
        /// </summary>
        /// <param name="iCalendarContent">Content of the i calendar.</param>
        /// <returns></returns>
        public static DDay.iCal.Event GetCalendarEvent( string iCalendarContent )
        {
            string trimmedContent = iCalendarContent.Trim();

            if ( string.IsNullOrWhiteSpace( trimmedContent ) )
            {
                return null;
            }

            DDay.iCal.Event calendarEvent = null;

            lock ( ScheduleICalHelper._initLock )
            {
                if ( _iCalSchedules.ContainsKey( trimmedContent ) )
                {
                    return _iCalSchedules[trimmedContent];
                }

                StringReader stringReader = new StringReader( trimmedContent );
                var calendarList = DDay.iCal.iCalendar.LoadFromStream( stringReader );

                //// iCal is stored as a list of Calendar's each with a list of Events, etc.  
                //// We just need one Calendar and one Event
                if ( calendarList.Count > 0 )
                {
                    var calendar = calendarList[0] as DDay.iCal.iCalendar;
                    if ( calendar != null )
                    {
                        calendarEvent = calendar.Events[0] as DDay.iCal.Event;
                        _iCalSchedules.AddOrReplace( trimmedContent, calendarEvent );
                    }
                }
            }

            return calendarEvent;
        }

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="icalEvent">The ical event.</param>
        /// <param name="startTime">The start time.</param>
        /// <returns></returns>
        public static IList<Occurrence> GetOccurrences( DDay.iCal.Event icalEvent, DateTime startTime )
        {
            lock ( ScheduleICalHelper._initLock )
            {
                return icalEvent.GetOccurrences( startTime );
            }
        }

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="icalEvent">The ical event.</param>
        /// <param name="startTime">The start time.</param>
        /// <param name="endTime">The end time.</param>
        /// <returns></returns>
        public static IList<Occurrence> GetOccurrences( DDay.iCal.Event icalEvent, DateTime startTime, DateTime endTime )
        {
            lock ( ScheduleICalHelper._initLock )
            {
                return icalEvent.GetOccurrences( startTime, endTime );
            }
        }
    }

    #endregion
}
