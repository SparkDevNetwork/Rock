//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

using Rock;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// CheckInSchedule EF Model.
    /// </summary>
    [Table("Schedule")]
    public partial class Schedule : Model<Schedule>
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
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the frequency.
        /// </summary>
        /// <value>
        /// The frequency.
        /// </value>
        public ScheduleFrequency Frequency { get; set; }

        /// <summary>
        /// Gets or sets the frequency qualifier.
        /// </summary>
        /// <value>
        /// The frequency qualifier.
        /// </value>
        [MaxLength(100)]
        public string FrequencyQualifier { get; set; }

        /// <summary>
        /// Gets or sets the start time.
        /// </summary>
        /// <value>
        /// The start time.
        /// </value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time.
        /// </summary>
        /// <value>
        /// The end time.
        /// </value>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Gets or sets the check in start time.
        /// </summary>
        /// <value>
        /// The check in start time.
        /// </value>
        public DateTime? CheckInStartTime { get; set; }

        /// <summary>
        /// Gets or sets the check in end time.
        /// </summary>
        /// <value>
        /// The check in end time.
        /// </value>
        public DateTime? CheckInEndTime { get; set; }

        /// <summary>
        /// Gets or sets the effective start date.
        /// </summary>
        /// <value>
        /// The effective start date.
        /// </value>
        public DateTimeOffset? EffectiveStartDate { get; set; }

        /// <summary>
        /// Gets or sets the effective end date.
        /// </summary>
        /// <value>
        /// The effective end date.
        /// </value>
        public DateTimeOffset? EffectiveEndDate { get; set; }

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
            // If there is not a checkin start and end time, or the check-in start happens
            // to be greater than the check-in end time, return null
            if (CheckInStartTime.HasValue && 
                CheckInEndTime.HasValue &&
                CheckInStartTime.Value.CompareTo(CheckInEndTime.Value) < 0)
            {
                return true;
            }
            return false;
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

                if ( EffectiveStartDate.HasValue && EffectiveStartDate.Value.CompareTo( DateTimeOffset.Now ) > 0 )
                {
                    return false;
                }

                if ( EffectiveEndDate.HasValue && EffectiveEndDate.Value.CompareTo( DateTimeOffset.Now ) < 0 )
                {
                    return false;
                }

                if ( CheckInStartTime.Value.TimeOfDay.TotalSeconds > DateTimeOffset.Now.TimeOfDay.TotalSeconds ||
                    CheckInEndTime.Value.TimeOfDay.TotalSeconds <= DateTimeOffset.Now.TimeOfDay.TotalSeconds )
                {
                    return false;
                }

                switch ( Frequency )
                {
                    case ScheduleFrequency.Monthly:

                        if ( DateTimeOffset.Now.Day.ToString() == FrequencyQualifier)
                        {
                            return true;
                        }

                        break;

                    case ScheduleFrequency.Weekly:

                        // Get the list of days that this schedule is effective
                        var days = new List<string>( FrequencyQualifier.Split( ',' ) );
                        if ( days.Contains( DateTimeOffset.Now.DayOfWeek.ToString() ) )
                        {
                            return true;
                        }

                        break;

                    case ScheduleFrequency.Daily:

                        return true;

                    case ScheduleFrequency.OneTime:

                        DateTime theDate = DateTime.MinValue;
                        if ( DateTime.TryParse( FrequencyQualifier, out theDate ) )
                        {
                            if ( theDate.Date.CompareTo( DateTimeOffset.Now.Date ) == 0 )
                            {
                                return true;
                            }
                        }

                        break;
                }

                return false;
   
            }
        }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        #endregion

        #region Public Methods

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
            DateTimeOffset from = beginDateTime;
            if ( EffectiveStartDate.HasValue && EffectiveStartDate.Value.CompareTo( from ) > 0 )
            {
                from = EffectiveStartDate.Value;
            }

            DateTime? nextStartTime = null;
            DateTime? nextEndTime = null;

            switch ( Frequency )
            {
                case ScheduleFrequency.Monthly:

                    int dom = 0;
                    if ( Int32.TryParse( FrequencyQualifier, out dom ) )
                    {
                        // Get the start/end time for the selected day of the month using the effective start time's month
                        nextStartTime = CombineDateTime( new DateTime( from.Year, from.Month, dom ), CheckInStartTime.Value );
                        nextEndTime = CombineDateTime( new DateTime( from.Year, from.Month, dom ), CheckInEndTime.Value );

                        // If the end time is prior to the effective start time, increment the start time by a month
                        if ( nextEndTime.Value.CompareTo( from ) <= 0 )
                        {
                            nextStartTime = nextStartTime.Value.AddMonths( 1 );
                        }
                    }
                    break;

                case ScheduleFrequency.Weekly:

                    // Get the list of days that this schedule is effective
                    var days = new List<string>( FrequencyQualifier.Split( ',' ) );

                    // Evaluate today and the next seven days, and if this schedule is 
                    // effective for that day of the week, save the date to a list of dates
                    var dates = new List<DateTime>();
                    for ( int i = 0; i <= 7; i++ )
                    {
                        if ( days.Contains( from.Date.AddDays( i ).DayOfWeek.ToString() ) )
                        {
                            dates.Add( from.Date.AddDays( i ) );
                        }
                    }

                    // Evaluate each date.  Select the first date where the end time is not
                    // prior to the effective start date
                    foreach ( var date in dates )
                    {
                        nextStartTime = CombineDateTime( date, CheckInStartTime.Value );
                        nextEndTime = CombineDateTime( date, CheckInEndTime.Value );
                        if ( nextEndTime.Value.CompareTo( from.DateTime ) > 0 )
                        {
                            break;
                        }
                    }

                    break;

                case ScheduleFrequency.Daily:

                    // Set the start time to the effective date unless the check-in end time is 
                    // past the effective start time, in that case, increment the start time by
                    // a day.
                    nextStartTime = CombineDateTime( from.DateTime, CheckInStartTime.Value );
                    nextEndTime = CombineDateTime( from.DateTime, CheckInEndTime.Value );
                    if ( nextEndTime.Value.CompareTo( from.DateTime ) <= 0 )
                    {
                        nextStartTime = nextStartTime.Value.AddDays( 1 );
                    }

                    break;

                case ScheduleFrequency.OneTime:

                    // Get the one time date.  If the check-in end time is past the effective 
                    // start date, then set the start date to null
                    DateTime theDate = DateTime.MinValue;
                    if ( DateTime.TryParse( FrequencyQualifier, out theDate ) )
                    {
                        nextStartTime = CombineDateTime( theDate, CheckInStartTime.Value );
                        nextEndTime = CombineDateTime( theDate, CheckInEndTime.Value );

                        if ( nextEndTime.Value.CompareTo( from.DateTime ) <= 0 )
                        {
                            nextStartTime = null;
                        }
                    }

                    break;
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

        #region Private Methods

        private DateTime CombineDateTime(DateTime date, DateTime time)
        {
            return date.Date.Add(time.TimeOfDay);
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static Schedule Read( int id )
        {
            return Read<Schedule>( id );
        }

        /// <summary>
        /// Static method to return an object based on the GUID.
        /// </summary>
        /// <param name="guid">The GUID.</param>
        /// <returns></returns>
        public static Schedule Read( Guid guid )
        {
            return Read<Schedule>( guid );
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
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The frequency type
    /// </summary>
    public enum ScheduleFrequency
    {
        /// <summary>
        /// Daily
        /// </summary>
        Daily = 0,

        /// <summary>
        /// Weekly
        /// </summary>
        Weekly = 1,

        /// <summary>
        /// Monthly
        /// </summary>
        Monthly = 2,

        /// <summary>
        /// One Time
        /// </summary>
        OneTime = 3
    }

    #endregion


}
