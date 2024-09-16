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

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;
using Ical.Net;
using Ical.Net.DataTypes;
using Rock.Lava;
using Rock.Web.Cache;
using Ical.Net.CalendarComponents;
using System.ComponentModel.DataAnnotations;
using Rock.Attribute;

namespace Rock.Model
{
    public partial class Schedule
    {
        #region Properties

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
                _getICalEvent = null;
                _iCalendarContent = value;
            }
        }

        private string _iCalendarContent;

        #region Additional Lava Properties

        /// <summary>
        /// Gets the weekly time of day in friendly text, such as "7:00 PM".
        /// </summary>
        /// <value>
        /// The weekly time of day in friendly text or an empty string if not valid.
        /// </value>
        [LavaVisible]
        public string WeeklyTimeOfDayText => WeeklyTimeOfDay?.ToTimeString() ?? string.Empty;

        /*
            2021-02-17 - DJL

            These properties exist to simplify Lava code that needs to query if the schedule or check-in is currently active.
            They have been reinstated at the request of the community after being marked obsolete in v1.8.

            Reason: Community Request, Issue #3471 (https://github.com/SparkDevNetwork/Rock/issues/3471)
        */

        /// <summary>
        /// Gets a value indicating whether this schedule is currently active. This
        /// is based on <see cref="RockDateTime.Now" />. Use <see cref="Campus.CurrentDateTime"/> and <see cref="WasScheduleActive(DateTime)"/>
        /// to get this based on the Campus's current datetime. 
        /// </summary>
        /// <value>
        /// <c>true</c> if this schedule is currently active; otherwise, <c>false</c>.
        /// </value>
        [LavaVisible]
        public virtual bool IsScheduleActive
        {
            get
            {
                return WasScheduleActive( RockDateTime.Now );
            }
        }

        /// <summary>
        /// Gets a value indicating whether check-in is currently active for this schedule. This
        /// is based on <see cref="RockDateTime.Now" />. Use <see cref="Campus.CurrentDateTime"/> and <see cref="WasCheckInActive(DateTime)"/>
        /// to get this based on the Campus's current datetime. 
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> that is  <c>true</c> if Check-in is currently active for this Schedule ; otherwise, <c>false</c>.
        /// </value>
        [LavaVisible]
        public virtual bool IsCheckInActive
        {
            get
            {
                return WasCheckInActive( RockDateTime.Now );
            }
        }

        /// <summary>
        /// Gets the next start time based on <see cref="RockDateTime.Now" />. Use <see cref="Campus.CurrentDateTime"/>
        /// and <see cref="GetNextStartDateTime(DateTime)"/> to get this based on the Campus's current datetime. 
        /// </summary>
        /// <returns></returns>
        [LavaVisible]
        public virtual DateTime? NextStartDateTime
        {
            get
            {
                return GetNextStartDateTime( RockDateTime.Now );
            }
        }

        #endregion

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
                var calendarEvent = GetICalEvent();
                if ( calendarEvent != null && calendarEvent.DtStart != null )
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
        /// Gets the next start date time.
        /// </summary>
        /// <param name="currentDateTime">The current date time.</param>
        /// <returns></returns>
        public DateTime? GetNextStartDateTime( DateTime currentDateTime )
        {
            if ( this.IsActive )
            {
                // Increase this from 1 to 2 years to catch events more than a year out. See github issue #4812.
                var endDate = currentDateTime.AddYears( 2 );

                var calEvent = GetICalEvent();

                RecurrencePattern rrule = null;

                if ( calEvent != null )
                {
                    if ( calEvent.RecurrenceRules.Any() )
                    {
                        rrule = calEvent.RecurrenceRules[0];
                    }
                }

                /* 2020-06-24 MP
                 * To improve performance, only go out a week (or so) if this is a weekly or daily schedule.
                 * If this optimization fails to find a next scheduled date, fall back to looking out a full two years
                 */

                if ( rrule?.Frequency == FrequencyType.Weekly )
                {
                    var everyXWeeks = rrule.Interval;
                    endDate = currentDateTime.AddDays( everyXWeeks * 7 );
                }
                else if ( rrule?.Frequency == FrequencyType.Daily )
                {
                    var everyXDays = rrule.Interval;
                    endDate = currentDateTime.AddDays( everyXDays );
                }

                var occurrences = GetScheduledStartTimes( currentDateTime, endDate );
                var nextOccurrence = occurrences.Min( o => ( DateTime? ) o );
                if ( nextOccurrence == null && endDate < currentDateTime.AddYears( 2 ) )
                {
                    // if tried an earlier end date, but didn't get a next datetime,
                    // use the regular way and see if there is a next schedule date within the next two years
                    endDate = currentDateTime.AddYears( 2 );
                    occurrences = GetScheduledStartTimes( currentDateTime, endDate );
                    nextOccurrence = occurrences.Min( o => ( DateTime? ) o );
                }

                return nextOccurrence;
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
        [LavaVisible]
        public virtual DateTime? FirstStartDateTime => GetFirstStartDateTime();

        /// <summary>
        /// Gets the first start date time this week.
        /// </summary>
        /// <value>
        /// The first start date time this week.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual DateTime? FirstStartDateTimeThisWeek
        {
            get
            {
                var endDate = RockDateTime.Today.SundayDate();
                var startDate = endDate.AddDays( -7 );
                var occurrences = GetScheduledStartTimes( startDate, endDate );
                return occurrences.Min( o => ( DateTime? ) o );
            }
        }

        /// <summary>
        /// Gets the start time of day.
        /// </summary>
        /// <value>
        /// The start time of day.
        /// </value>
        [LavaVisible]
        public virtual TimeSpan StartTimeOfDay
        {
            get
            {
                var calendarEvent = GetICalEvent();
                if ( calendarEvent != null && calendarEvent.DtStart != null )
                {
                    return calendarEvent.DtStart.Value.TimeOfDay;
                }

                if ( WeeklyTimeOfDay.HasValue )
                {
                    return WeeklyTimeOfDay.Value;
                }

                return new TimeSpan();
            }
        }

        /// <summary>
        /// Gets the duration in minutes.
        /// </summary>
        /// <value>
        /// The duration in minutes.
        /// </value>
        [LavaVisible]
        public virtual int DurationInMinutes
        {
            get
            {
                var calendarEvent = GetICalEvent();
                if ( calendarEvent != null && calendarEvent.DtStart != null && calendarEvent.DtEnd != null )
                {
                    return ( int ) calendarEvent.DtEnd.Subtract( calendarEvent.DtStart ).TotalMinutes;
                }

                return 0;
            }
        }

        /// <summary>
        /// Gets the friendly schedule text.
        /// </summary>
        /// <value>
        /// The friendly schedule text.
        /// </value>
        [LavaVisible]
        [DataMember]
        [NotMapped]
        public virtual string FriendlyScheduleText
        {
            get { return ToFriendlyScheduleText(); }
            // Empty setter so XML serialization can happen without exceptions whiles maintaining readonly status.
            private set { }
        }

        /// <summary>
        /// Gets or sets the shortened name of the attribute.
        /// If null or whitespace then the full name is returned.
        /// </summary>
        /// <value>
        /// The name of the abbreviated.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string AbbreviatedName
        {
            get
            {
                if ( _abbreviatedName.IsNullOrWhiteSpace() )
                {
                    return Name.Truncate( 50, false );
                }

                return _abbreviatedName;
            }

            set
            {
                _abbreviatedName = value;
            }
        }

        private string _abbreviatedName;

        #endregion

        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            if ( this.Name.IsNotNullOrWhiteSpace() )
            {
                return NamedScheduleCache.Get( this.Id );
            }

            return null;
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            NamedScheduleCache.FlushItem( this.Id );
        }

        #endregion ICacheable

        #region Public Methods

        /// <summary>
        /// Ensures that the EffectiveStartDate and EffectiveEndDate are set correctly based on the CalendarContent.
        /// Returns true if any changes were made.
        /// </summary>
        /// <returns></returns>
        internal bool EnsureEffectiveStartEndDates()
        {
            /*
             * 12/6/2019 BJW
             *
             * There was an issue in this code because DateTime.MaxValue was being used to represent [no end date]. Because EffectiveEndDate
             * is a Date only in SQL, the time portion gets dropped.  Because 12/31/9999 11:59 != 12/31/9999 00:00, this caused false readings
             * when testing for schedules with no end dates. When there was no end date, Rock was calculating all occurrences with
             * GetOccurrences( DateTime.MinValue, DateTime.MaxValue ) which caused timeouts.  We should only call that GetOccurrences method
             * like that when we know there are a reasonable number of occurrences. 
             */

            var calEvent = GetICalEvent();
            if ( calEvent == null )
            {
                EffectiveEndDate = null;
                EffectiveStartDate = null;
                return false;
            }

            var originalEffectiveEndDate = EffectiveEndDate;
            var originalEffectiveStartDate = EffectiveStartDate;

            // Set initial values for start and end dates.
            // End date is set to MaxValue as a placeholder - infinitely repeating schedules should be stored with
            // a null end date value.
            DateTime? effectiveStartDateTime = calEvent.DtStart?.Value.Date;
            DateTime? effectiveEndDateTime = DateTime.MaxValue;

            // In Rock it is possible to set a rule with an end date, no end date or a number
            // of occurrences. The count property in the iCal rule refers to the count of occurrences.
            var endDateRules = calEvent.RecurrenceRules.Where( rule => rule.Count <= 0 );
            var countRules = calEvent.RecurrenceRules.Where( rule => rule.Count > 0 );

            var hasRuleWithEndDate = endDateRules.Any();
            var hasRuleWithCount = countRules.Any();
            var hasDates = calEvent.RecurrenceDates.Any();

            bool adjustEffectiveDateForLastOccurrence = false;

            // If there are any recurrence rules with no end date, the Effective End Date is infinity
            // iCal rule.Until will be min value date if it is representing no end date (backwards from Rock using max value)
            if ( hasRuleWithEndDate )
            {
                if ( endDateRules.Any( rule => RockDateTime.IsMinDate( rule.Until ) ) )
                {
                    effectiveEndDateTime = null;
                }
                else
                {
                    effectiveEndDateTime = endDateRules.Max( rule => rule.Until );
                }
            }

            if ( hasRuleWithCount )
            {
                if ( countRules.Any( rule => rule.Count > 999 ) && !hasRuleWithEndDate )
                {
                    // If there is a count rule greater than 999 (limit in the UI), and no end date rule was applied,
                    // we don't want to calculate occurrences because it will be too costly. Treat this as no end date.
                    effectiveEndDateTime = null;
                }
                else
                {
                    // This case means that there are count rules and they are <= 999. Go ahead and calculate the actual occurrences
                    // to get the EffectiveEndDate.
                    adjustEffectiveDateForLastOccurrence = true;
                }
            }

            // If specific recurrence dates exist, adjust the Effective End Date to the last specified date 
            // if it occurs after the Effective End Date required by the recurrence rules.
            if ( hasDates )
            {
                // If the Schedule does not have any other rules, reset the Effective End Date to the placeholder value
                // to ensure it is recalculated.
                if ( !hasRuleWithEndDate && !hasRuleWithCount )
                {
                    effectiveEndDateTime = DateTime.MaxValue;
                }

                adjustEffectiveDateForLastOccurrence = true;
            }

            if ( adjustEffectiveDateForLastOccurrence
                 && effectiveEndDateTime != null )
            {
                var occurrences = GetICalOccurrences( DateTime.MinValue, DateTime.MaxValue );

                if ( occurrences.Any() )
                {
                    var lastOccurrenceDate = occurrences.Any() // It is possible for an event to have no occurrences
                        ? occurrences.OrderByDescending( o => o.Period.StartTime.Date ).First().Period.EndTime.Value
                        : effectiveStartDateTime;

                    if ( effectiveEndDateTime == DateTime.MaxValue
                         || lastOccurrenceDate > effectiveEndDateTime )
                    {
                        effectiveEndDateTime = lastOccurrenceDate;
                    }
                }
            }

            // At this point, if no EffectiveEndDate is set then assume this is a one-time event and set the EffectiveEndDate to the EffectiveStartDate.
            if ( effectiveEndDateTime == DateTime.MaxValue && !adjustEffectiveDateForLastOccurrence )
            {
                effectiveEndDateTime = effectiveStartDateTime;
            }

            // Add the Duration of the event to ensure that the effective end date corresponds to the day on which the event concludes.
            if ( effectiveEndDateTime != null && effectiveEndDateTime != DateTime.MaxValue )
            {
                effectiveEndDateTime = effectiveEndDateTime.Value.AddMinutes( DurationInMinutes );
            }

            // Set the Effective Start and End dates. The dates are inclusive but do not have a time component.
            EffectiveStartDate = effectiveStartDateTime?.Date;
            EffectiveEndDate = effectiveEndDateTime?.Date;

            return ( EffectiveEndDate?.Date != originalEffectiveEndDate?.Date ) || ( EffectiveStartDate?.Date != originalEffectiveStartDate?.Date );
        }

        /// <summary>
        /// Gets the Schedule's iCalender Event.
        /// </summary>
        /// <value>
        /// A <see cref="DDay.iCal.Event"/> representing the iCalendar event for this Schedule.
        /// </value>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetICalEvent() instead " )]
        public virtual DDay.iCal.Event GetCalendarEvent()
        {
            return ScheduleICalHelper.GetCalendarEvent( iCalendarContent );
        }

        /// <summary>
        /// Gets the Schedule's iCalender Event.
        /// </summary>
        /// <value>
        /// A <see cref="Ical.Net.CalendarComponents.CalendarEvent"/> representing the iCalendar event for this Schedule.
        /// </value>
        public virtual CalendarEvent GetICalEvent()
        {
            if ( _getICalEvent == null )
            {
                _getICalEvent = InetCalendarHelper.CreateCalendarEvent( iCalendarContent );
            }

            return _getICalEvent;
        }

        private CalendarEvent _getICalEvent = null;

        /// <summary>
        /// Gets the occurrences.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetICalOccurrences() instead." )]
        public IList<DDay.iCal.Occurrence> GetOccurrences( DateTime beginDateTime, DateTime? endDateTime = null )
        {
            return this.GetOccurrences( beginDateTime, endDateTime, null );
        }

        /// <summary>
        /// Gets the occurrences with option to override the ICal.Event.DTStart
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="scheduleStartDateTimeOverride">The schedule start date time override.</param>
        /// <returns></returns>
        [RockObsolete( "1.12" )]
        [Obsolete( "Use GetICalOccurrences() instead." )]
        public IList<DDay.iCal.Occurrence> GetOccurrences( DateTime beginDateTime, DateTime? endDateTime, DateTime? scheduleStartDateTimeOverride )
        {
            var occurrences = new List<DDay.iCal.Occurrence>();

            DDay.iCal.Event calEvent = GetCalendarEvent();
            if ( calEvent == null )
            {
                return occurrences;
            }

            if ( scheduleStartDateTimeOverride.HasValue )
            {
                calEvent.DTStart = new DDay.iCal.iCalDateTime( scheduleStartDateTimeOverride.Value );
            }

            if ( calEvent.DTStart != null )
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

                foreach ( var occurrence in endDateTime.HasValue ?
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
        /// Gets the occurrences.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <returns></returns>
        public IList<Occurrence> GetICalOccurrences( DateTime beginDateTime, DateTime? endDateTime = null )
        {
            return this.GetICalOccurrences( beginDateTime, endDateTime, null );
        }

        /// <summary>
        /// Gets the occurrences with option to override the ICal.Event.DTStart
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="scheduleStartDateTimeOverride">The schedule start date time override.</param>
        /// <returns>A list of <see cref="Occurrence"/> objects that fall within the specified date range.</returns>
        public IList<Occurrence> GetICalOccurrences( DateTime beginDateTime, DateTime? endDateTime, DateTime? scheduleStartDateTimeOverride )
        {
            return GetICalOccurrences( beginDateTime, endDateTime, scheduleStartDateTimeOverride, CategoryId, iCalendarContent, () => GetICalEvent() );
        }

        /// <summary>
        /// Gets the occurrences with option to override the ICal.Event.DTStart
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="scheduleStartDateTimeOverride">The schedule start date time override.</param>
        /// <param name="categoryId">The category identifier that the schedule is in.</param>
        /// <param name="iCalendarContent">The raw iCal content.</param>
        /// <param name="calendarEventFactory">The calendar event factory that will return an instance of the <see cref="CalendarEvent"/>.</param>
        /// <returns>A list of <see cref="Occurrence" /> objects that fall within the specified date range.</returns>
        internal static IList<Occurrence> GetICalOccurrences( DateTime beginDateTime, DateTime? endDateTime, DateTime? scheduleStartDateTimeOverride, int? categoryId, string iCalendarContent, Func<CalendarEvent> calendarEventFactory )
        {
            var occurrences = new List<Occurrence>();

            DateTime? scheduleStartDateTime;

            if ( scheduleStartDateTimeOverride.HasValue )
            {
                scheduleStartDateTime = scheduleStartDateTimeOverride;
            }
            else
            {
                var calEvent = calendarEventFactory();
                if ( calEvent == null )
                {
                    return occurrences;
                }

                scheduleStartDateTime = calEvent.DtStart?.Value;
            }

            if ( scheduleStartDateTime != null )
            {
                var exclusionDates = new List<DateRange>();
                if ( categoryId.HasValue && categoryId.Value > 0 )
                {
                    var category = CategoryCache.Get( categoryId.Value );
                    if ( category != null )
                    {
                        exclusionDates = category.ScheduleExclusions
                            .Where( e => e.Start.HasValue && e.End.HasValue )
                            .ToList();
                    }
                }

                foreach ( var occurrence in InetCalendarHelper.GetOccurrences( iCalendarContent, beginDateTime, endDateTime, scheduleStartDateTimeOverride ) )
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
            if ( IsCheckInEnabled )
            {
                return GetCheckInTimes( beginDateTime, CheckInStartOffsetMinutes.Value, CheckInEndOffsetMinutes, iCalendarContent, () => GetICalEvent() );
            }

            return new List<CheckInTimes>();
        }

        /// <summary>
        /// Gets the check in times.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="checkInStartOffsetMinutes">The check in start offset minutes.</param>
        /// <param name="checkInEndOffsetMinutes">The check in end offset minutes.</param>
        /// <param name="iCalendarContent">The raw iCal content.</param>
        /// <param name="calendarEventFactory">The calendar event factory that will return an instance of the <see cref="CalendarEvent"/>.</param>
        /// <returns>A list of <see cref="CheckInTimes"/> objects.</returns>
        internal static List<CheckInTimes> GetCheckInTimes( DateTime beginDateTime, int checkInStartOffsetMinutes, int? checkInEndOffsetMinutes, string iCalendarContent, Func<CalendarEvent> calendarEventFactory )
        {
            var result = new List<CheckInTimes>();

            var occurrences = GetICalOccurrences( beginDateTime, beginDateTime.Date.AddDays( 1 ), null, null, iCalendarContent, calendarEventFactory );

            foreach ( var occurrence in occurrences
                .Where( a =>
                    a.Period != null &&
                    a.Period.StartTime != null &&
                    a.Period.EndTime != null )
                .Select( a => new
                {
                    Start = a.Period.StartTime.Value,
                    End = a.Period.EndTime.Value
                } ) )
            {
                var checkInTimes = new CheckInTimes();
                checkInTimes.Start = DateTime.SpecifyKind( occurrence.Start, DateTimeKind.Local );
                checkInTimes.End = DateTime.SpecifyKind( occurrence.End, DateTimeKind.Local );
                checkInTimes.CheckInStart = checkInTimes.Start.AddMinutes( 0 - checkInStartOffsetMinutes );
                if ( checkInEndOffsetMinutes.HasValue )
                {
                    checkInTimes.CheckInEnd = checkInTimes.Start.AddMinutes( checkInEndOffsetMinutes.Value );
                }
                else
                {
                    checkInTimes.CheckInEnd = checkInTimes.End;
                }

                result.Add( checkInTimes );
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

            var occurrences = GetICalOccurrences( beginDateTime, endDateTime );
            foreach ( var startDateTime in occurrences
                .Where( a =>
                    a.Period != null &&
                    a.Period.StartTime != null )
                .Select( a => a.Period.StartTime.Value ) )
            {
                // ensure the datetime is DateTimeKind.Local since iCal returns DateTimeKind.UTC
                result.Add( DateTime.SpecifyKind( startDateTime, DateTimeKind.Local ) );
            }

            return result;
        }

        /// <summary>
        /// Gets a list of scheduled start datetimes between the two specified dates, sorted by datetime.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="excludeOccurrencesAlreadyStarted">Whether to exclude occurrences whose start time has already passed.</param>
        /// <returns>A list of scheduled start datetimes between the two specified dates, sorted by datetime.</returns>
        [RockInternal( "1.16.1" )]
        public virtual List<DateTime> GetScheduledStartTimes( DateTime beginDateTime, DateTime endDateTime, bool excludeOccurrencesAlreadyStarted )
        {
            return GetScheduledStartTimes( beginDateTime, endDateTime )
                .Where( a => !excludeOccurrencesAlreadyStarted || a > beginDateTime )
                .ToList();
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
            var calEvent = GetICalEvent();
            if ( calEvent != null && calEvent.DtStart != null )
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
            var calEvent = GetICalEvent();
            if ( calEvent != null && calEvent.DtStart != null )
            {
                if ( calEvent.RecurrenceRules.Any() )
                {
                    var rrule = calEvent.RecurrenceRules[0];
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

            var calendarEvent = GetICalEvent();
            if ( calendarEvent != null && calendarEvent.DtStart != null )
            {
                string startTimeText = calendarEvent.DtStart.Value.TimeOfDay.ToTimeString();
                if ( calendarEvent.RecurrenceRules.Any() )
                {
                    // some type of recurring schedule
                    var rrule = calendarEvent.RecurrenceRules[0];
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
                                // The Nth <DayOfWeekName>.  We only support one *day* in the ByDay list, but multiple *offsets*.
                                // So, it can be the "The first and third Monday" of every month.
                                var bydate = rrule.ByDay[0];
                                var offsetNames = NthNamesAbbreviated.Where( a => rrule.ByDay.Select( o => o.Offset ).Contains( a.Key ) ).Select( a => a.Value );
                                if ( offsetNames != null )
                                {
                                    result = string.Format( "The {0} {1} of every month", offsetNames.JoinStringsWithCommaAnd(), bydate.DayOfWeek.ConvertToString() );
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
                    var dates = InetCalendarHelper.GetOccurrences( iCalendarContent, DateTime.MinValue, DateTime.MaxValue, null )
                        .Where( a => a.Period != null && a.Period.StartTime != null )
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
                    else if ( dates.Count() == 1 )
                    {
                        result = "Once at " + calendarEvent.DtStart.Value.ToShortDateTimeString();
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
        /// Returns value indicating if the schedule was active at the specified time.
        /// </summary>
        /// <param name="time">The time at which to use when determining if check-in was active.</param>
        /// <returns><c>true</c> if the schedule was active; <c>false</c> otherwise.</returns>
        public bool WasScheduleActive( DateTime time )
        {
            return WasScheduleActive( time, GetICalEvent(), CategoryId, iCalendarContent );
        }

        /// <summary>
        /// Returns value indicating if the schedule was active at the specified time.
        /// </summary>
        /// <param name="time">The time at which to use when determining if check-in was active.</param>
        /// <param name="calEvent">The calendar event.</param>
        /// <param name="categoryId">The category identifier the schedule belongs to.</param>
        /// <param name="iCalendarContent">The iCal calendar content text.</param>
        /// <returns><c>true</c> if the schedule was active; <c>false</c> otherwise.</returns>
        internal static bool WasScheduleActive( DateTime time, CalendarEvent calEvent, int? categoryId, string iCalendarContent )
        {
            if ( calEvent != null && calEvent.DtStart != null )
            {
                // If compare is greater than zero, then the End Day is in the next day.
                var endDateTimeIsNextDay = calEvent.DtEnd.Date > calEvent.DtStart.Date;
                if ( endDateTimeIsNextDay )
                {
                    /*
                       edrotning 2023-09-28
                       Since we are just comparing the time and not the date here, the end time is going to be smaller than the start time, this is because the start Time is on the previous day.
                       The given time falls outside of the start time to midnight and the midnight to end time windows
                       has to be greater than the end time, which is going to be an earlier time than the start time.
                    */
                        if ( time.TimeOfDay.TotalSeconds < calEvent.DtStart.Value.TimeOfDay.TotalSeconds
                            && time.TimeOfDay.TotalSeconds >= calEvent.DtEnd.Value.TimeOfDay.TotalSeconds )
                        {
                            return false;
                        }
                }
                else
                {
                    // Start and end time are on the same day, so simple compare of seconds

                    if ( time.TimeOfDay.TotalSeconds < calEvent.DtStart.Value.TimeOfDay.TotalSeconds )
                    {
                        // The given time is earlier than the event's start time
                        return false;
                    }

                    if ( time.TimeOfDay.TotalSeconds > calEvent.DtEnd.Value.TimeOfDay.TotalSeconds )
                    {
                        // The given time is later than the event's end time
                        return false;
                    }
                }

                // After verifying the times, get the occurrences for this schedule for the provided date.
                var occurrences = GetICalOccurrences( time.Date, null, null, categoryId, iCalendarContent, () => calEvent );
                return occurrences.Count > 0;
            }

            return false;
        }

        /// <summary>
        /// Returns value indicating if check-in was active at the specified time.
        /// </summary>
        /// <param name="time">The time at which to use when determining if check-in was active.</param>
        /// <returns><c>true</c> if check-in was active; <c>false</c> otherwise.</returns>
        public bool WasCheckInActive( DateTime time )
        {
            if ( !IsCheckInEnabled )
            {
                return false;
            }

            return WasCheckInActive( time,
                GetICalEvent(),
                CheckInStartOffsetMinutes.Value,
                CheckInEndOffsetMinutes,
                CategoryId,
                iCalendarContent );
        }

        /// <summary>
        /// Returns value indicating if check-in was active at the specified time.
        /// </summary>
        /// <param name="time">The time at which to use when determining if check-in was active.</param>
        /// <param name="calEvent">The calendar event.</param>
        /// <param name="checkInStartOffsetMinutes">The check in start offset minutes.</param>
        /// <param name="checkInEndOffsetMinutes">The check in end offset minutes.</param>
        /// <param name="categoryId">The category identifier the schedule belongs to.</param>
        /// <param name="iCalendarContent">The iCal calendar content text.</param>
        /// <returns><c>true</c> if check-in was active; <c>false</c> otherwise.</returns>
        internal static bool WasCheckInActive( DateTime time, CalendarEvent calEvent, int checkInStartOffsetMinutes, int? checkInEndOffsetMinutes, int? categoryId, string iCalendarContent )
        {
            if ( calEvent != null && calEvent.DtStart != null )
            {
                // Is the current time earlier the event's allowed check-in window?
                var checkInStart = calEvent.DtStart.AddMinutes( 0 - checkInStartOffsetMinutes );
                if ( time.TimeOfDay.TotalSeconds < checkInStart.Value.TimeOfDay.TotalSeconds )
                {
                    return false;
                }

                var checkInEnd = calEvent.DtEnd;
                if ( checkInEndOffsetMinutes.HasValue )
                {
                    checkInEnd = calEvent.DtStart.AddMinutes( checkInEndOffsetMinutes.Value );
                }

                // If compare is greater than zero, then check-in offset end resulted in an end time in next day, in 
                // which case, don't need to compare time
                int checkInEndDateCompare = checkInEnd.Date.CompareTo( checkInStart.Date );

                if ( checkInEndDateCompare < 0 )
                {
                    // End offset is prior to start (Would have required a neg number entered)
                    return false;
                }

                // Is the current time later then the event's allowed check-in window?
                if ( checkInEndDateCompare == 0 && time.TimeOfDay.TotalSeconds > checkInEnd.Value.TimeOfDay.TotalSeconds )
                {
                    // Same day, but end time has passed
                    return false;
                }

                var occurrences = GetICalOccurrences( time.Date, null, null, categoryId, iCalendarContent, () => calEvent );
                return occurrences.Count > 0;
            }

            return false;
        }

        /// <summary>
        /// Determines whether a schedule is active for check-out for the specified time.
        /// </summary>
        /// <example>
        /// CheckOut Window: 5/1/2013 11:00:00 PM - 5/2/2013 2:00:00 AM
        /// 
        ///  * Current time: 8/8/2019 11:01:00 PM - returns true
        ///  * Current time: 8/8/2019 10:59:00 PM - returns false
        ///  * Current time: 8/8/2019 1:00:00 AM - returns true
        ///  * Current time: 8/8/2019 2:01:00 AM - returns false
        ///
        /// Note: Add any other test cases you want to test to the "Rock.Tests.Rock.Model.ScheduleCheckInTests" project.
        /// </example>
        /// <param name="time">The time.</param>
        /// <returns>
        ///   <c>true</c> if the schedule is active for check out at the specified time; otherwise, <c>false</c>.
        /// </returns>
        private bool IsScheduleActiveForCheckOut( DateTime time )
        {
            return IsScheduleActiveForCheckOut( time, GetICalEvent(), CheckInStartOffsetMinutes.Value, CategoryId, iCalendarContent );
        }

        /// <summary>
        /// Determines whether a schedule is active for check-out for the specified time.
        /// </summary>
        /// <example>
        /// CheckOut Window: 5/1/2013 11:00:00 PM - 5/2/2013 2:00:00 AM
        /// 
        ///  * Current time: 8/8/2019 11:01:00 PM - returns true
        ///  * Current time: 8/8/2019 10:59:00 PM - returns false
        ///  * Current time: 8/8/2019 1:00:00 AM - returns true
        ///  * Current time: 8/8/2019 2:01:00 AM - returns false
        ///
        /// Note: Add any other test cases you want to test to the "Rock.Tests.Rock.Model.ScheduleCheckInTests" project.
        /// </example>
        /// <param name="time">The time.</param>
        /// <param name="calEvent">The calendar event that represents the schedule.</param>
        /// <param name="checkInStartOffsetMinutes">The check in start offset minutes.</param>
        /// <param name="categoryId">The category identifier the schedule belongs to.</param>
        /// <param name="iCalendarContent">The iCal calendar content text.</param>
        /// <returns>
        ///   <c>true</c> if the schedule is active for check out at the specified time; otherwise, <c>false</c>.
        /// </returns>
        internal static bool IsScheduleActiveForCheckOut( DateTime time, CalendarEvent calEvent, int checkInStartOffsetMinutes, int? categoryId, string iCalendarContent )
        {
            if ( calEvent == null || calEvent.DtStart == null )
            {
                return false;
            }

            // For check-out, we use the start time + duration to determine the end of the window...
            // ...in iCal, this is the DTEnd value
            var checkOutEnd = calEvent.DtEnd;
            var checkInStart = calEvent.DtStart.AddMinutes( 0 - checkInStartOffsetMinutes );

            // Check if the end time spilled over to a different day...
            int checkOutEndDateCompare = checkOutEnd.Date.CompareTo( checkInStart.Date );

            if ( checkOutEndDateCompare < 0 )
            {
                // invalid condition, end before the start
                return false;
            }
            else if ( checkOutEndDateCompare == 0 )
            {
                // the start and end are on the same day, so we can do simple time checking
                // Is the current time earlier the event's allowed check-in window?
                if ( time.TimeOfDay.TotalSeconds < checkInStart.Value.TimeOfDay.TotalSeconds )
                {
                    // Same day, but it's too early
                    return false;
                }

                // Is the current time later than the event's allowed check-in window?
                if ( time.TimeOfDay.TotalSeconds > checkOutEnd.Value.TimeOfDay.TotalSeconds )
                {
                    // Same day, but end time has passed
                    return false;
                }
            }
            else if ( checkOutEndDateCompare > 0 )
            {
                // Does the end time spill over to a different day...
                // if so, we have to look for crossover conditions

                // The current time is before the start time and later than the end time:
                // ex: 11PM-2AM window, and it's 10PM -- not in the window
                // ex: 11PM-2AM window, and it's 3AM -- not in the window
                if ( time.TimeOfDay.TotalSeconds < checkInStart.Value.TimeOfDay.TotalSeconds && time.TimeOfDay.TotalSeconds > checkOutEnd.AsSystemLocal.TimeOfDay.TotalSeconds )
                {
                    return false;
                }
            }

            var occurrences = GetICalOccurrences( time.Date, null, null, categoryId, iCalendarContent, () => calEvent );
            return occurrences.Count > 0;
        }

        /// <summary>
        /// Determines if the schedule or check in is active for check out.
        /// Check-out can happen while check-in is active or until the event
        /// ends (start time + duration).
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public bool WasScheduleOrCheckInActiveForCheckOut( DateTime time )
        {
            return IsScheduleActiveForCheckOut( time ) || WasScheduleActive( time ) || WasCheckInActive( time );
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
        /// Returns the schedule's name if defined, or a friendly text of the calendar event if not.
        /// For example, "Every 3 days at 10:30am", "Monday, Wednesday, Friday at 5:00pm", "Saturday at 4:30pm"
        /// </summary>
        /// <returns>The schedule's name if defined, or a friendly text of the calendar event if not.</returns>
        public override string ToString()
        {
            return ToString( false );
        }

        /// <summary>
        /// Returns the schedule's name if defined, or a friendly text of the calendar event if not.
        /// For example, "Every 3 days at 10:30am", "Monday, Wednesday, Friday at 5:00pm", "Saturday at 4:30pm"
        /// </summary>
        /// <param name="condensed">Whether to return condensed friendly text (with no HTML markup, for example).</param>
        /// <returns>The schedule's name if defined, or a friendly text of the calendar event if not.</returns>
        public string ToString( bool condensed )
        {
            if ( this.Name.IsNotNullOrWhiteSpace() )
            {
                return this.Name;
            }
            else
            {
                return this.ToFriendlyScheduleText( condensed );
            }
        }

        #endregion
    }
}
