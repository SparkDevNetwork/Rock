// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data.Entity.ModelConfiguration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using DDay.iCal;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a Scheduled event in Rock.  Several places where this has been used includes Check-in scheduling and Kiosk scheduling.
    /// </summary>
    [Table( "Schedule" )]
    [DataContract]
    public partial class Schedule : Model<Schedule>, ICategorized
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
        public DateTime? EffectiveStartDate { get; private set; }

        /// <summary>
        /// Gets or sets that date that this Schedule expires and becomes inactive. This value is inclusive and the schedule will be inactive after this date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> value that represents the date that this Schedule ends and becomes inactive.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EffectiveEndDate { get; private set; }

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
                return CheckInStartOffsetMinutes.HasValue;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this schedule is currently active.
        /// </summary>
        /// <value>
        /// <c>true</c> if this schedule is currently active; otherwise, <c>false</c>.
        /// </value>
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
                DDay.iCal.Event calendarEvent = this.GetCalenderEvent();
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the Schedule's iCalender Event.
        /// </summary>
        /// <value>
        /// A <see cref="DDay.iCal.Event"/> representing the iCalendar event for this Schedule.
        /// </value>
        public virtual DDay.iCal.Event GetCalenderEvent() 
        {
            return ScheduleICalHelper.GetCalenderEvent( iCalendarContent );
        }

        /// <summary>
        /// Gets the next Check-in start date for this Schedule.  
        /// </summary>
        /// <param name="beginDateTime">A <see cref="System.DateTime"/> representing the base date.</param>
        /// <returns>A <see cref="System.DateTime"/> containing the next time that Check-in begins for this schedule.</returns>
        public virtual DateTime? GetNextCheckInStartTime( DateTime beginDateTime )
        {
            DateTime? nextStartTime = null;

            if ( IsCheckInEnabled )
            {
                DDay.iCal.Event calEvent = GetCalenderEvent();
                if ( calEvent != null )
                {
                    var scheduledStartTimes = this.GetScheduledStartTimes( beginDateTime, beginDateTime.AddMonths( 1 ) );
                    if ( scheduledStartTimes.Count > 0 )
                    {
                        var nextScheduledStartTime = scheduledStartTimes[0];
                        nextStartTime = nextScheduledStartTime.AddMinutes( 0 - CheckInStartOffsetMinutes.Value );
                    }
                }
            }

            return nextStartTime;
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
            DDay.iCal.Event calEvent = GetCalenderEvent();
            if ( calEvent != null )
            {
                // use ThreadSafe helper method to get occurrences
                var occurrences = ScheduleICalHelper.GetOccurrences( calEvent, beginDateTime, endDateTime );
                
                foreach ( var startDateTime in occurrences.Where( a => a.Period != null && a.Period.StartTime != null ).Select( a => a.Period.StartTime.Value ) )
                {
                    // ensure the the datetime is DateTimeKind.Local since iCal returns DateTimeKind.UTC
                    result.Add( DateTime.SpecifyKind( startDateTime, DateTimeKind.Local ) );
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the Friendly Text of the Calendar Event.
        /// For example, "Every 3 days at 10:30am", "Monday, Wednesday, Friday at 5:00pm", "Saturday at 4:30pm"
        /// </summary>
        /// <returns>A <see cref="System.String"/> containing a friendly description of the Schedule.</returns>
        public string ToFriendlyScheduleText()
        {
            // init the result to just the schedule name just in case we can't figure out the FriendlyText
            string result = this.Name;

            DDay.iCal.Event calendarEvent = this.GetCalenderEvent();
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

                            result = rrule.ByDay.Select( a => a.DayOfWeek.ConvertToString() ).ToList().AsDelimited( "," );

                            if ( rrule.Interval > 1 )
                            {
                                result += string.Format( " every {0} weeks", rrule.Interval );
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
                    // not any type of recurring, runs once
                    result = "Once at " + calendarEvent.DTStart.Value.ToString();
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
            var calEvent = this.GetCalenderEvent();
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

                var occurrences = ScheduleICalHelper.GetOccurrences( calEvent, time.Date );
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

            var calEvent = this.GetCalenderEvent();
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

                var occurrences = ScheduleICalHelper.GetOccurrences( calEvent, time.Date );
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
        /// The "nth" names for DayName of Month (First, Secord, Third, Forth, Last)
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
    /// Helper class for grouping attendance records associated into logical occurrences based on
    /// a given schedule
    /// </summary>
    public class ScheduleOccurrence
    {
        /// <summary>
        /// Gets or sets the logical occurrence start date time.
        /// </summary>
        /// <value>
        /// The occurrence start date time.
        /// </value>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the logical occurrence end date time.
        /// </summary>
        /// <value>
        /// The occurrence end date time.
        /// </value>
        public DateTime EndDateTime { get; set; }

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
        /// Gets a value indicating whether attendance has been entered for this occurrence.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [attendance entered]; otherwise, <c>false</c>.
        /// </value>
        public bool AttendanceEntered
        {
            get
            {
                return Attendance != null && Attendance.Any( a => a.DidAttend.HasValue );
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
                return Attendance != null &&
                    !Attendance.Where( a => a.DidAttend.HasValue ).Any() &&
                    Attendance.Where( a => a.DidNotOccur.HasValue ).Any();
            }
        }

        /// <summary>
        /// Gets or sets the attendance records associated with this occurrence
        /// </summary>
        /// <value>
        /// The attendance.
        /// </value>
        public List<Attendance> Attendance { get; set; }

        /// <summary>
        /// Gets the number of people attended.
        /// </summary>
        /// <value>
        /// The attended.
        /// </value>
        public int NumberAttended
        {
            get
            {
                return Attendance
                    .Where( a =>
                        a.PersonAlias != null &&
                        a.DidAttend.HasValue &&
                        a.DidAttend.Value )
                    .Select( a => a.PersonAlias.PersonId )
                    .Distinct()
                    .Count();
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
                var people = new Dictionary<int, bool>();
                foreach ( var person in Attendance
                    .Where( a =>
                        a.PersonAlias != null &&
                        a.DidAttend.HasValue )
                    .Select( a => new
                    {
                        PersonId = a.PersonAlias.PersonId,
                        DidAttend = a.DidAttend.Value
                    } )
                    .Distinct() )
                {
                    if ( person.DidAttend )
                    {
                        people.AddOrReplace( person.PersonId, true );
                    }
                    else
                    {
                        people.AddOrIgnore( person.PersonId, false );
                    }
                }

                int attended = people.Where( p => p.Value ).Count();
                int total = people.Count();
                if ( total > 0 )
                {
                    return (double)( attended ) / (double)total;
                }
                else
                {
                    return 0.0d;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleOccurrence"/> class.
        /// </summary>
        /// <param name="occurrence">The occurrence.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="scheduleName">Name of the schedule.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="locationName">Name of the location.</param>
        public ScheduleOccurrence( DDay.iCal.Occurrence occurrence, int? scheduleId = null, string scheduleName = "", int? locationId = null, string locationName = "" )
        {
            StartDateTime = occurrence.Period.StartTime.Value;
            EndDateTime = occurrence.Period.EndTime.Value;
            ScheduleId = scheduleId;
            ScheduleName = scheduleName;
            LocationId = locationId;
            LocationName = locationName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduleOccurrence"/> class.
        /// </summary>
        /// <param name="startDateTime">The start date time.</param>
        /// <param name="endDateTime">The end date time.</param>
        /// <param name="scheduleId">The schedule identifier.</param>
        /// <param name="scheduleName">Name of the schedule.</param>
        /// <param name="locationId">The location identifier.</param>
        /// <param name="locationName">Name of the location.</param>
        public ScheduleOccurrence( DateTime startDateTime, DateTime endDateTime, int? scheduleId = null, string scheduleName = "", int? locationId = null, string locationName = "" )
        {
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            ScheduleId = scheduleId;
            ScheduleName = scheduleName;
            LocationId = locationId;
            LocationName = locationName;
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
        /// Gets the calender event.
        /// </summary>
        /// <param name="iCalendarContent">Content of the i calendar.</param>
        /// <returns></returns>
        public static DDay.iCal.Event GetCalenderEvent( string iCalendarContent )
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
