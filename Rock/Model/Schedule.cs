//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
    /// Represents a Scheduled event in RockChMS.  Several places where this has been used includes Check-in scheduling and Kiosk scheduling.
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
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
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
                return _iCalendarContent;
            }
            set
            {
                _iCalendarContent = value;
                DDay.iCal.Event calendarEvent = GetCalenderEvent();
                if ( calendarEvent != null )
                {
                    if ( calendarEvent.DTStart != null )
                    {
                        EffectiveStartDate = calendarEvent.DTStart.Value;
                    }
                    else
                    {
                        EffectiveStartDate = null;
                    }

                    if ( calendarEvent.RecurrenceDates.Count() > 0 )
                    {
                        var dateList = calendarEvent.RecurrenceDates[0];
                        EffectiveEndDate = dateList.OrderBy( a => a.StartTime ).Last().StartTime.Value;
                    }
                    else if ( calendarEvent.RecurrenceRules.Count() > 0 )
                    {
                        var rrule = calendarEvent.RecurrenceRules[0];
                        if ( rrule.Until > DateTime.MinValue )
                        {
                            EffectiveEndDate = rrule.Until;
                        }
                        else if ( rrule.Count > 0 )
                        {
                            // not really a perfect way to figure out end date.  safer to assume null
                            EffectiveEndDate = null;
                        }
                    }
                    else
                    {
                        if ( calendarEvent.End != null )
                        {
                            EffectiveEndDate = calendarEvent.End.Value;
                        }
                        else
                        {
                            EffectiveEndDate = null;
                        }
                    }
                }
                else
                {
                    EffectiveStartDate = null;
                    EffectiveEndDate = null;
                }
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
        /// Gets a value indicating whether check-in is currently active for this Schedule.
        /// </summary>
        /// <value>
        ///  A <see cref="System.Boolean"/> that is  <c>true</c> if Check-in is currently active for this Schedule ; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsCheckInActive
        {
            get
            {
                if ( !IsCheckInEnabled )
                {
                    return false;
                }

                if ( EffectiveStartDate.HasValue && EffectiveStartDate.Value.CompareTo( DateTimeOffset.Now.DateTime ) > 0 )
                {
                    return false;
                }

                if ( EffectiveEndDate.HasValue && EffectiveEndDate.Value.CompareTo( DateTimeOffset.Now.DateTime ) < 0 )
                {
                    return false;
                }

                var calEvent = this.GetCalenderEvent();

                if ( calEvent != null && calEvent.DTStart != null )
                {
                    var checkInStart = calEvent.DTStart.AddMinutes( 0 - CheckInStartOffsetMinutes.Value );
                    if ( DateTimeOffset.Now.TimeOfDay.TotalSeconds < checkInStart.TimeOfDay.TotalSeconds )
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

                    if ( checkInEndDateCompare == 0 && DateTimeOffset.Now.TimeOfDay.TotalSeconds > checkInEnd.TimeOfDay.TotalSeconds )
                    {
                        // Same day, but end time has passed
                        return false;
                    }

                    var occurrences = calEvent.GetOccurrences( DateTime.Now.Date );
                    return occurrences.Count > 0;
                }

                return false;
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
            //// iCal is stored as a list of Calendar's each with a list of Events, etc.  
            //// We just need one Calendar and one Event

            StringReader stringReader = new StringReader( iCalendarContent.Trim() );
            var calendarList = DDay.iCal.iCalendar.LoadFromStream( stringReader );
            DDay.iCal.Event calendarEvent = null;
            if ( calendarList.Count > 0 )
            {
                var calendar = calendarList[0] as DDay.iCal.iCalendar;
                if ( calendar != null )
                {
                    calendarEvent = calendar.Events[0] as DDay.iCal.Event;
                }
            }

            return calendarEvent;
        }

        /// <summary>
        /// Gets the next Check-in start date for this Schedule.  
        /// </summary>
        /// <param name="beginDateTime">A <see cref="System.DateTimeOffset"/> representing the base date.</param>
        /// <returns>A <see cref="System.DateTime"/> containing the next time that Check-in begins for this schedule.</returns>
        public virtual DateTime? GetNextCheckInStartTime( DateTimeOffset beginDateTime )
        {
            if ( !IsCheckInEnabled )
            {
                return null;
            }

            // Get the effective start datetime if there's not a specific effective 
            // start time
            DateTime fromDate = beginDateTime.DateTime;
            if ( EffectiveStartDate.HasValue && EffectiveStartDate.Value.CompareTo( fromDate ) > 0 )
            {
                fromDate = EffectiveStartDate.Value;
            }

            DateTime? nextStartTime = null;

            DDay.iCal.Event calEvent = GetCalenderEvent();

            if ( calEvent != null )
            {
                var occurrences = calEvent.GetOccurrences( fromDate, fromDate.AddMonths( 1 ) );
                if ( occurrences.Count > 0 )
                {
                    var nextOccurance = occurrences[0];
                    nextStartTime = nextOccurance.Period.StartTime.Date.AddMinutes( 0 - CheckInStartOffsetMinutes.Value );
                }
            }

            // If no start time was found, return null
            if ( !nextStartTime.HasValue )
            {
                return null;
            }

            // If the Effective end date is prior to next start time, return null
            if ( EffectiveEndDate.HasValue && EffectiveEndDate.Value.CompareTo( nextStartTime.Value ) < 0 )
            {
                return null;
            }

            return nextStartTime.Value;
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
            if ( calendarEvent != null && calendarEvent.DTStart != null)
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
                                    result += string.Format("{0} months", rrule.Interval);
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
                    // Just return the Name of the schedule
                }
            }

            return result;
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
}
