//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.IO;
using System.Runtime.Serialization;
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// CheckInSchedule EF Model.
    /// </summary>
    [Table( "Schedule" )]
    [DataContract]
    public partial class Schedule : Model<Schedule>, ICategorized
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Schedule name.
        /// </summary>
        /// <value>
        /// File Name.
        /// </value>
        [Required]
        [AlternateKey]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Description.
        /// </summary>
        /// <value>
        /// Notes about the job..
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the content lines of the iCalendar
        /// </summary>
        /// <value>
        /// The content of the iCalendar.
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
        /// Gets or sets the number of minutes prior to schedule start that check-in should be active
        /// </summary>
        /// <value>
        /// The check in start offset
        /// </value>
        [DataMember]
        public int? CheckInStartOffsetMinutes { get; set; }

        /// <summary>
        /// Gets or sets the number of minutes following schedule start that check-in should be active
        /// </summary>
        /// <value>
        /// The check in end offset
        /// </value>
        [DataMember]
        public int? CheckInEndOffsetMinutes { get; set; }

        /// <summary>
        /// Gets or sets the effective start date.
        /// </summary>
        /// <value>
        /// The effective start date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EffectiveStartDate { get; private set; }

        /// <summary>
        /// Gets or sets the effective end date.
        /// </summary>
        /// <value>
        /// The effective end date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EffectiveEndDate { get; private set; }

        /// <summary>
        /// Gets or sets the category id.
        /// </summary>
        /// <value>
        /// The category id.
        /// </value>
        [DataMember]
        public int? CategoryId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets a value indicating whether this instance is check in enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is check in enabled; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsCheckInEnabled
        {
            get
            {
                return CheckInStartOffsetMinutes.HasValue;
            }
        }

        /// <summary>
        /// Gets a value indicating whether check-in is currently active for this instance.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
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

                if (calEvent != null && calEvent.DTStart != null)
                {
                    var checkInStart = calEvent.DTStart.AddMinutes(0 - CheckInStartOffsetMinutes.Value);

                    var checkInEnd = calEvent.DTEnd;
                    if (CheckInEndOffsetMinutes.HasValue)
                    {
                        checkInEnd = calEvent.DTStart.AddMinutes(CheckInEndOffsetMinutes.Value);
                    }

                    if (DateTimeOffset.Now.TimeOfDay.TotalSeconds < checkInStart.TimeOfDay.TotalSeconds ||
                        DateTimeOffset.Now.TimeOfDay.TotalSeconds > checkInStart.TimeOfDay.TotalSeconds)
                    {
                        return false;
                    }

                    var occurrences = calEvent.GetOccurrences( DateTime.Now.Date );
                    return occurrences.Count > 0;
                }

                return false;
            }
        }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        [DataMember]
        public virtual Category Category { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the iCalender Event.
        /// </summary>
        /// <value>
        /// The iCalender Event.
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
        /// Gets the next check in start time.
        /// </summary>
        /// <param name="beginDateTime">The begin date time.</param>
        /// <returns></returns>
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

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
