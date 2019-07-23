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

namespace Rock
{
    /// <summary>
    /// Represents a period of time that can be expressed as either having a fixed start and end date, or as relative to the current date and time.
    /// </summary>
    public class TimePeriod
    {
        private DateTime? _DateStart;
        private DateTime? _DateEnd;
        private int? _NumberOfTimeUnits = null;
        private TimePeriodRangeSpecifier _Range = TimePeriodRangeSpecifier.All;
        private TimePeriodUnitSpecifier? _TimeUnit = null;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePeriod"/> class.
        /// </summary>
        public TimePeriod()
        {
            this.SetToUnboundedDateRange();
        }

        public TimePeriod( string delimitedSettingsString )
        {
            this.FromDelimitedString( delimitedSettingsString );
        }

        public TimePeriod( string delimitedSettingsString, string delimiter )
        {
            this.FromDelimitedString( delimitedSettingsString, delimiter );
        }

        public TimePeriod( DateTime? startDate, DateTime? endDate )
        {
            this.SetToSpecificDateRange( startDate, endDate );
        }

        public TimePeriod( DateRange dateRange )
        {
            this.SetSpecificDateRange( dateRange );
        }

        public TimePeriod( TimePeriodRangeSpecifier range, TimePeriodUnitSpecifier unit, int numberOfTimeUnits )
        {
            this.SetToRelativePeriod( range, unit, numberOfTimeUnits );
        }

        public TimePeriod( TimePeriodUnitSpecifier unit )
        {
            this.SetToCurrentPeriod( unit );
        }

        #endregion

        /// <summary>
        /// Set an unbounded range that includes all possible values.
        /// </summary>
        public void SetToUnboundedDateRange()
        {
            _Range = TimePeriodRangeSpecifier.All;
            _TimeUnit = null;
            _NumberOfTimeUnits = null;
            _DateStart = null;
            _DateEnd = null;
        }

        /// <summary>
        /// Set a specific date range.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        public void SetToSpecificDateRange( DateTime? startDate, DateTime? endDate )
        {
            _Range = TimePeriodRangeSpecifier.DateRange;
            _TimeUnit = null;
            _NumberOfTimeUnits = null;
            _DateStart = startDate;
            _DateEnd = endDate;
        }

        /// <summary>
        /// Set a specific date range.
        /// </summary>
        /// <param name="dateRange"></param>
        public void SetSpecificDateRange( DateRange dateRange )
        {
            this.SetToSpecificDateRange( dateRange.Start, dateRange.End );
        }

        /// <summary>
        /// Set a period relative to the current date.
        /// </summary>
        /// <param name="range"></param>
        /// <param name="unit"></param>
        /// <param name="numberOfTimeUnits"></param>
        public void SetToRelativePeriod( TimePeriodRangeSpecifier range, TimePeriodUnitSpecifier unit, int numberOfTimeUnits )
        {
            _Range = range;
            _TimeUnit = unit;
            _NumberOfTimeUnits = numberOfTimeUnits;
            _DateStart = null;
            _DateEnd = null;
        }

        /// <summary>
        /// Set a current period.
        /// </summary>
        /// <param name="unit"></param>
        public void SetToCurrentPeriod( TimePeriodUnitSpecifier unit )
        {
            _Range = TimePeriodRangeSpecifier.Current;
            _TimeUnit = unit;
            _NumberOfTimeUnits = null;
            _DateStart = null;
            _DateEnd = null;
        }

        /// <summary>
        /// Gets the type of date range specified for this time period.
        /// </summary>
        /// <value>
        /// The last or current.
        /// </value>
        public TimePeriodRangeSpecifier Range
        {
            get
            {
                return _Range;
            }
        }

        /// <summary>
        /// Gets the number of time units (x Hours, x Days, etc) depending on TimeUnit selection
        /// </summary>
        /// <value>
        /// The number of time units.
        /// </value>
        public int? NumberOfTimeUnits
        {
            get { return _NumberOfTimeUnits; }
        }

        /// <summary>
        /// Gets the time unit (Hour, Day, Year, etc)
        /// </summary>
        /// <value>
        /// The time unit.
        /// </value>
        public TimePeriodUnitSpecifier? TimeUnit
        {
            get
            {
                return _TimeUnit;
            }
        }

        /// <summary>
        /// Parse a delimited string and set the time period properties.
        /// The delimited string uses an identical format to the SlidingDateRangePicker.
        /// </summary>
        /// <param name="value"></param>
        public void FromDelimitedString( string value, string delimiter = "|" )
        {
            string[] splitValues = ( value ?? string.Empty ).SplitDelimitedValues( delimiter );

            if ( splitValues.Length > 0 )
            {
                _Range = splitValues[0].ConvertToEnumOrNull<TimePeriodRangeSpecifier>() ?? TimePeriodRangeSpecifier.All;
            }
            else
            {
                _Range = TimePeriodRangeSpecifier.All;
            }

            if ( splitValues.Length > 1 )
            {
                _NumberOfTimeUnits = splitValues[1].AsIntegerOrNull();
            }
            else
            {
                _NumberOfTimeUnits = null;
            }

            if ( splitValues.Length > 2 )
            {
                var timeUnit = splitValues[2].ConvertToEnumOrNull<TimePeriodUnitSpecifier>();

                _TimeUnit = timeUnit;
            }
            else
            {
                _TimeUnit = null;
            }

            if ( splitValues.Length > 3 )
            {

                _DateStart = splitValues[3].AsDateTime();
            }
            else
            {
                _DateStart = null;
            }

            if ( splitValues.Length > 4 )
            {
                _DateEnd = splitValues[4].AsDateTime();
            }
            else
            {
                _DateEnd = null;
            }
        }

        /// <summary>
        /// Returns a delimited string representing the time period properties.
        /// The delimited string uses an identical format to the SlidingDateRangePicker.
        /// </summary>
        /// <param name="value"></param>
        public string ToDelimitedString( string delimiter = "|" )
        {
            var delimitedString = string.Format(
                "{0}<delimiter>{1}<delimiter>{2}<delimiter>{3}<delimiter>{4}",
                this.Range,
                ( TimePeriodRangeSpecifier.Last | TimePeriodRangeSpecifier.Previous | TimePeriodRangeSpecifier.Next | TimePeriodRangeSpecifier.Upcoming ).HasFlag( this.Range ) ? this.NumberOfTimeUnits : ( int? ) null,
                ( TimePeriodRangeSpecifier.Last | TimePeriodRangeSpecifier.Previous | TimePeriodRangeSpecifier.Next | TimePeriodRangeSpecifier.Upcoming | TimePeriodRangeSpecifier.Current ).HasFlag( this.Range ) ? this.TimeUnit : ( TimePeriodUnitSpecifier? ) null,
                this.Range == TimePeriodRangeSpecifier.DateRange ? _DateStart : null,
                this.Range == TimePeriodRangeSpecifier.DateRange ? _DateEnd : null );

            delimitedString = delimitedString.Replace( "<delimiter>", delimiter );

            return delimitedString;
        }

        /// <summary>
        /// Returns a DateRange object that represents the start and end dates of the time period.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public DateRange GetDateRange( TimePeriodDateRangeBoundarySpecifier boundary = TimePeriodDateRangeBoundarySpecifier.Inclusive )
        {
            DateRange result = new DateRange();

            DateTime currentDateTime = RockDateTime.Now;

            if ( this.Range == TimePeriodRangeSpecifier.Current )
            {
                switch ( this.TimeUnit )
                {
                    case TimePeriodUnitSpecifier.Hour:
                        result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 );
                        result.End = result.Start.Value.AddHours( 1 );
                        break;

                    case TimePeriodUnitSpecifier.Day:
                        result.Start = currentDateTime.Date;
                        result.End = result.Start.Value.AddDays( 1 );
                        break;

                    case TimePeriodUnitSpecifier.Week:
                        // from http://stackoverflow.com/a/38064/1755417
                        int diff = currentDateTime.DayOfWeek - RockDateTime.FirstDayOfWeek;
                        if ( diff < 0 )
                        {
                            diff += 7;
                        }

                        result.Start = currentDateTime.AddDays( -1 * diff ).Date;
                        result.End = result.Start.Value.AddDays( 7 );
                        break;

                    case TimePeriodUnitSpecifier.Month:
                        result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 );
                        result.End = result.Start.Value.AddMonths( 1 );
                        break;

                    case TimePeriodUnitSpecifier.Year:
                        result.Start = new DateTime( currentDateTime.Year, 1, 1 );
                        result.End = new DateTime( currentDateTime.Year + 1, 1, 1 );
                        break;
                }
            }
            else if ( ( TimePeriodRangeSpecifier.Last | TimePeriodRangeSpecifier.Previous ).HasFlag( this.Range ) )
            {
                // Last X Days/Hours. NOTE: addCount is the number of X that it go back (it'll actually subtract)
                int addCount = _NumberOfTimeUnits.GetValueOrDefault( 0 );

                // if we are getting "Last" round up to include the current day/week/month/year
                int roundUpCount = this.Range == TimePeriodRangeSpecifier.Last ? 1 : 0;

                switch ( this.TimeUnit )
                {
                    case TimePeriodUnitSpecifier.Hour:
                        result.End = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 ).AddHours( roundUpCount );
                        result.Start = result.End.Value.AddHours( -addCount );
                        break;

                    case TimePeriodUnitSpecifier.Day:
                        result.End = currentDateTime.Date.AddDays( roundUpCount );
                        result.Start = result.End.Value.AddDays( -addCount );
                        break;

                    case TimePeriodUnitSpecifier.Week:
                        // from http://stackoverflow.com/a/38064/1755417
                        int diff = currentDateTime.DayOfWeek - RockDateTime.FirstDayOfWeek;
                        if ( diff < 0 )
                        {
                            diff += 7;
                        }

                        result.End = currentDateTime.AddDays( -1 * diff ).Date.AddDays( 7 * roundUpCount );
                        result.Start = result.End.Value.AddDays( -addCount * 7 );
                        break;

                    case TimePeriodUnitSpecifier.Month:
                        result.End = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 ).AddMonths( roundUpCount );
                        result.Start = result.End.Value.AddMonths( -addCount );
                        break;

                    case TimePeriodUnitSpecifier.Year:
                        result.End = new DateTime( currentDateTime.Year, 1, 1 ).AddYears( roundUpCount );
                        result.Start = result.End.Value.AddYears( -addCount );
                        break;
                }

                // don't let Last,Previous have any future dates
                var cutoffDate = RockDateTime.Now.Date.AddDays( 1 );
                if ( result.End.Value.Date > cutoffDate )
                {
                    result.End = cutoffDate;
                }
            }
            else if ( ( TimePeriodRangeSpecifier.Next | TimePeriodRangeSpecifier.Upcoming ).HasFlag( this.Range ) )
            {
                // Next X Days,Hours,etc
                int addCount = _NumberOfTimeUnits.GetValueOrDefault( 0 );

                // if we are getting "Upcoming", round up to exclude the current day/week/month/year
                int roundUpCount = this.Range == TimePeriodRangeSpecifier.Upcoming ? 1 : 0;

                switch ( this.TimeUnit )
                {
                    case TimePeriodUnitSpecifier.Hour:
                        result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 ).AddHours( roundUpCount );
                        result.End = result.Start.Value.AddHours( addCount );
                        break;

                    case TimePeriodUnitSpecifier.Day:
                        result.Start = currentDateTime.Date.AddDays( roundUpCount );
                        result.End = result.Start.Value.AddDays( addCount );
                        break;

                    case TimePeriodUnitSpecifier.Week:
                        // from http://stackoverflow.com/a/38064/1755417
                        int diff = currentDateTime.DayOfWeek - RockDateTime.FirstDayOfWeek;
                        if ( diff < 0 )
                        {
                            diff += 7;
                        }

                        result.Start = currentDateTime.AddDays( -1 * diff ).Date.AddDays( 7 * roundUpCount );
                        result.End = result.Start.Value.AddDays( addCount * 7 );
                        break;

                    case TimePeriodUnitSpecifier.Month:
                        result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 ).AddMonths( roundUpCount );
                        result.End = result.Start.Value.AddMonths( addCount );
                        break;

                    case TimePeriodUnitSpecifier.Year:
                        result.Start = new DateTime( currentDateTime.Year, 1, 1 ).AddYears( roundUpCount );
                        result.End = result.Start.Value.AddYears( addCount );
                        break;
                }

                // don't let Next,Upcoming have any past dates
                if ( result.Start.Value.Date < RockDateTime.Now.Date )
                {
                    result.Start = RockDateTime.Now.Date;
                }
            }
            else if ( this.Range == TimePeriodRangeSpecifier.DateRange )
            {
                result.Start = _DateStart;

                DateTime? endDateTime = _DateEnd;

                if ( endDateTime.HasValue )
                {
                    // Return the last millisecond of the day.
                    result.End = endDateTime.Value.Date.AddDays( 1 ).AddMilliseconds( -1 );
                }
                else
                {
                    result.End = null;
                }
            }

            // If time unit is days, weeks, months or years subtract a second from time so that end time is with same period
            if ( result.End.HasValue && this.TimeUnit != TimePeriodUnitSpecifier.Hour )
            {
                result.End = result.End.Value.AddSeconds( -1 );
            }

            // Boundary dates are calculated to be inclusive; they represent the earliest or latest datetime that is considered to be within the specified time period,
            // to an accuracy of 1ms. If exclusive dates have been requested, adjust them now.
            if ( boundary == TimePeriodDateRangeBoundarySpecifier.Exclusive )
            {
                if ( result.Start != null )
                {
                    result.Start = result.Start.Value.AddMilliseconds( -1 );
                }

                if ( result.End != null )
                {
                    result.End = result.End.Value.AddMilliseconds( 1 );
                }
            }

            return result;
        }

        /// <summary>
        /// Get a friendly description of the time period.
        /// For example: "Last 14 Days"
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public string GetDescription()
        {
            var numberOfTimeUnits = _NumberOfTimeUnits.GetValueOrDefault( 1 );

            string timeUnitText = ( _TimeUnit != null ) ? _TimeUnit.ConvertToString().PluralizeIf( numberOfTimeUnits != 1 ) : null;

            if ( _Range == TimePeriodRangeSpecifier.Current )
            {
                return string.Format( "{0} {1}", _Range.ConvertToString(), timeUnitText );
            }
            if ( _Range == TimePeriodRangeSpecifier.All )
            {
                return "All";
            }
            else if ( _Range == TimePeriodRangeSpecifier.DateRange )
            {
                // Get the DateRange description, but only show the time component if Range is less than a full day.
                var dateRange = GetDateRange();

                string dateTimeFormat;

                if ( _TimeUnit == TimePeriodUnitSpecifier.Hour )
                {
                    dateTimeFormat = "g";
                }
                else
                {
                    dateTimeFormat = "d";
                }

                return dateRange.ToStringAutomatic( "d", dateTimeFormat );
            }
            else
            {
                // Relative period
                return string.Format( "{0} {1} {2}", _Range.ConvertToString(), numberOfTimeUnits, timeUnitText );
            }
        }


        #region Equality Implementation

        public override bool Equals( object other )
        {
            var otherPeriod = other as TimePeriod;

            if ( otherPeriod == null )
            {
                return false;
            }

            // For unbounded range, ignore other settings.
            if ( _Range == TimePeriodRangeSpecifier.All
                 && otherPeriod.Range == TimePeriodRangeSpecifier.All )
            {
                return true;
            }

            // For specific date range, test the start and end dates to the required degree of granularity.
            if ( _Range == TimePeriodRangeSpecifier.DateRange )
            {
                var thisRange = this.GetDateRange();
                var otherRange = otherPeriod.GetDateRange();

                if ( !DateTimeIsEqual( thisRange.Start, otherRange.Start, _TimeUnit ?? TimePeriodUnitSpecifier.Hour ) )
                {
                    return false;
                }

                if ( !DateTimeIsEqual( thisRange.End, otherRange.End, _TimeUnit ?? TimePeriodUnitSpecifier.Hour ) )
                {
                    return false;
                }
            }
            // For relative ranges, test the relevant properties.
            else if ( _Range != otherPeriod.Range
                 || _TimeUnit != otherPeriod.TimeUnit
                 || _NumberOfTimeUnits != otherPeriod.NumberOfTimeUnits )
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if a DateTime is equal to a specified degree of accuracy.
        /// </summary>
        /// <param name="firstDate"></param>
        /// <param name="secondDate"></param>
        /// <param name="unitOfAccuracy"></param>
        /// <returns></returns>
        private bool DateTimeIsEqual( DateTime? firstDate, DateTime? secondDate, TimePeriodUnitSpecifier unitOfAccuracy )
        {
            if ( firstDate == null
                 && secondDate == null )
            {
                return true;
            }

            if ( firstDate == null
                 || secondDate == null )
            {
                return false;
            }

            var first = firstDate.Value;
            var second = secondDate.Value;

            if ( first.Year != second.Year )
            {
                return false;
            }

            if ( unitOfAccuracy == TimePeriodUnitSpecifier.Year )
            {
                return true;
            }

            if ( first.Month != second.Month )
            {
                return false;
            }

            if ( unitOfAccuracy == TimePeriodUnitSpecifier.Month )
            {
                return true;
            }

            if ( first.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstDay, RockDateTime.FirstDayOfWeek ) != second.GetWeekOfYear( System.Globalization.CalendarWeekRule.FirstDay, RockDateTime.FirstDayOfWeek ) )
            {
                return false;
            }

            if ( unitOfAccuracy == TimePeriodUnitSpecifier.Week )
            {
                return true;
            }

            if ( first.Day != second.Day )
            {
                return false;
            }

            if ( unitOfAccuracy == TimePeriodUnitSpecifier.Day )
            {
                return true;
            }

            if ( first.Hour != second.Hour )
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 27;
            hash = ( 13 * hash ) + _Range.GetHashCode();
            hash = ( 13 * hash ) + _TimeUnit.GetHashCode();
            hash = ( 13 * hash ) + _NumberOfTimeUnits.GetHashCode();
            hash = ( 13 * hash ) + _DateStart.GetValueOrDefault().GetHashCode();
            hash = ( 13 * hash ) + _DateEnd.GetValueOrDefault().GetHashCode();

            return hash;
        }

        #endregion
    }

    #region Enums

    /// <summary>
    /// Specifies how the start and end dates of the time period are calculated.
    /// </summary>
    public enum TimePeriodDateRangeBoundarySpecifier
    {
        /// <summary>
        /// The boundary dates represent the earliest and latest datetime values that can be included in the specified date range, to an accuracy of 1ms.
        /// </summary>
        Inclusive = 0,

        /// <summary>
        /// The boundary dates represent the earliest and latest datetime values that are excluded by the specified date range, to an accuracy of 1ms.
        /// </summary>
        Exclusive = 1
    }

    /// <summary>
    /// Specifies how the start and end dates of the time period are determined.
    /// </summary>
    public enum TimePeriodRangeSpecifier
    {
        /// <summary>
        /// All
        /// </summary>
        All = -1,

        /// <summary>
        /// The last X days,weeks,months, etc (inclusive of current day,week,month,...) but cuts off so it doesn't include future dates
        /// </summary>
        Last = 0,

        /// <summary>
        /// The current day,week,month,year
        /// </summary>
        Current = 1,

        /// <summary>
        /// The period is defined by a specific start date and end date.
        /// </summary>
        DateRange = 2,

        /// <summary>
        /// The previous X days,weeks,months, etc (excludes current day,week,month,...)
        /// </summary>
        Previous = 4,

        /// <summary>
        /// The next X days,weeks,months, etc (inclusive of current day,week,month,...), but cuts off so it doesn't include past dates
        /// </summary>
        Next = 8,

        /// <summary>
        /// The upcoming X days,weeks,months, etc (excludes current day,week,month,...)
        /// </summary>
        Upcoming = 16
    }

    /// <summary>
    /// Specifies the unit of time in which the period is measured.
    /// </summary>
    public enum TimePeriodUnitSpecifier
    {
        /// <summary>
        /// The hour
        /// </summary>
        Hour = 0,

        /// <summary>
        /// The day
        /// </summary>
        Day = 1,

        /// <summary>
        /// The week
        /// </summary>
        Week = 2,

        /// <summary>
        /// The month
        /// </summary>
        Month = 3,

        /// <summary>
        /// The year
        /// </summary>
        Year = 4
    }

    #endregion
}
