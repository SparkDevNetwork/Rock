using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Web.UI.Controls
{
    public class SlidingDateRangePicker
    {
        public static string GetDelimitedValues( SlidingDateRangeType slidingDateRangeMode, TimeUnitType? timeUnit = null, int? numberOfTimeUnits = 1, DateTime? dateRangeModeStart = null, DateTime? dateRangeModeEnd = null )
        {
            timeUnit = timeUnit ?? Enum.GetValues( typeof( TimeUnitType ) ).Cast<TimeUnitType>().ToArray().First();

            return string.Format(
                "{0}|{1}|{2}|{3}|{4}",
                slidingDateRangeMode,
                ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming ).HasFlag( slidingDateRangeMode ) ? numberOfTimeUnits : ( int? ) null,
                ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming | SlidingDateRangeType.Current ).HasFlag( slidingDateRangeMode ) ? timeUnit : ( TimeUnitType? ) null,
                slidingDateRangeMode == SlidingDateRangeType.DateRange ? dateRangeModeStart : null,
                slidingDateRangeMode == SlidingDateRangeType.DateRange ? dateRangeModeEnd : null );
        }

        public static string SanitizeDelimitedValues( string value, TimeUnitType? defaultTimeUnit = null )
        {
            string[] splitValues = ( value ?? string.Empty ).Split( '|' );
            defaultTimeUnit = defaultTimeUnit ?? Enum.GetValues( typeof( TimeUnitType ) ).Cast<TimeUnitType>().ToArray().First();

            SlidingDateRangeType slidingDateRangeMode;
            int? numberOfTimeUnits;
            TimeUnitType timeUnit;
            DateTime? dateRangeModeStart;
            DateTime? dateRangeModeEnd;

            if ( splitValues.Length == 5 )
            {
                slidingDateRangeMode = splitValues[0].ConvertToEnum<SlidingDateRangeType>();
                numberOfTimeUnits = splitValues[1].AsIntegerOrNull() ?? 1;
                timeUnit = splitValues[2].ConvertToEnumOrNull<TimeUnitType>() ?? defaultTimeUnit.Value;
                dateRangeModeStart = splitValues[3].AsDateTime();
                dateRangeModeEnd = splitValues[4].AsDateTime();
            }
            else
            {
                slidingDateRangeMode = SlidingDateRangeType.All;
                numberOfTimeUnits = 1;
                timeUnit = defaultTimeUnit.Value;
                dateRangeModeStart = null;
                dateRangeModeEnd = null;
            }

            return GetDelimitedValues( slidingDateRangeMode, timeUnit, numberOfTimeUnits, dateRangeModeStart, dateRangeModeEnd );
        }

        /// <summary>
        /// Calculates the date range from delimited values in format SlidingDateRangeType|Number|TimeUnitType|StartDate|EndDate
        /// NOTE: The Displayed End Date is one day before the actual end date.
        /// So, if your date range is displayed as 1/3/2015 to 1/4/2015, this will return 1/5/2015 12:00 AM as the End Date
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static DateRange CalculateDateRangeFromDelimitedValues( string value )
        {
            string[] splitValues = ( value ?? "1||4||" ).Split( '|' );
            DateRange result = new DateRange();
            if ( splitValues.Length == 5 )
            {
                var slidingDateRangeMode = splitValues[0].ConvertToEnum<SlidingDateRangeType>();
                var numberOfTimeUnits = splitValues[1].AsIntegerOrNull() ?? 1;
                TimeUnitType? timeUnit = splitValues[2].ConvertToEnumOrNull<TimeUnitType>();
                DateTime currentDateTime = RockDateTime.Now;
                if ( slidingDateRangeMode == SlidingDateRangeType.Current )
                {
                    switch ( timeUnit )
                    {
                        case TimeUnitType.Hour:
                            result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 );
                            result.End = result.Start.Value.AddHours( 1 );
                            break;

                        case TimeUnitType.Day:
                            result.Start = currentDateTime.Date;
                            result.End = result.Start.Value.AddDays( 1 );
                            break;

                        case TimeUnitType.Week:

                            // from http://stackoverflow.com/a/38064/1755417
                            int diff = currentDateTime.DayOfWeek - RockDateTime.FirstDayOfWeek;
                            if ( diff < 0 )
                            {
                                diff += 7;
                            }

                            result.Start = currentDateTime.AddDays( -1 * diff ).Date;
                            result.End = result.Start.Value.AddDays( 7 );
                            break;

                        case TimeUnitType.Month:
                            result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 );
                            result.End = result.Start.Value.AddMonths( 1 );
                            break;

                        case TimeUnitType.Year:
                            result.Start = new DateTime( currentDateTime.Year, 1, 1 );
                            result.End = new DateTime( currentDateTime.Year + 1, 1, 1 );
                            break;
                    }
                }
                else if ( ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous ).HasFlag( slidingDateRangeMode ) )
                {
                    // Last X Days/Hours. NOTE: addCount is the number of X that it go back (it'll actually subtract)
                    int addCount = numberOfTimeUnits;

                    // if we are getting "Last" round up to include the current day/week/month/year
                    int roundUpCount = slidingDateRangeMode == SlidingDateRangeType.Last ? 1 : 0;

                    switch ( timeUnit )
                    {
                        case TimeUnitType.Hour:
                            result.End = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 ).AddHours( roundUpCount );
                            result.Start = result.End.Value.AddHours( -addCount );
                            break;

                        case TimeUnitType.Day:
                            result.End = currentDateTime.Date.AddDays( roundUpCount );
                            result.Start = result.End.Value.AddDays( -addCount );
                            break;

                        case TimeUnitType.Week:
                            // from http://stackoverflow.com/a/38064/1755417
                            int diff = currentDateTime.DayOfWeek - RockDateTime.FirstDayOfWeek;
                            if ( diff < 0 )
                            {
                                diff += 7;
                            }

                            result.End = currentDateTime.AddDays( -1 * diff ).Date.AddDays( 7 * roundUpCount );
                            result.Start = result.End.Value.AddDays( -addCount * 7 );
                            break;

                        case TimeUnitType.Month:
                            result.End = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 ).AddMonths( roundUpCount );
                            result.Start = result.End.Value.AddMonths( -addCount );
                            break;

                        case TimeUnitType.Year:
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
                else if ( ( SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming ).HasFlag( slidingDateRangeMode ) )
                {
                    // Next X Days,Hours,etc
                    int addCount = numberOfTimeUnits;

                    // if we are getting "Upcoming", round up to exclude the current day/week/month/year
                    int roundUpCount = slidingDateRangeMode == SlidingDateRangeType.Upcoming ? 1 : 0;

                    switch ( timeUnit )
                    {
                        case TimeUnitType.Hour:
                            result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, currentDateTime.Hour, 0, 0 ).AddHours( roundUpCount );
                            result.End = result.Start.Value.AddHours( addCount );
                            break;

                        case TimeUnitType.Day:
                            result.Start = currentDateTime.Date.AddDays( roundUpCount );
                            result.End = result.Start.Value.AddDays( addCount );
                            break;

                        case TimeUnitType.Week:
                            // from http://stackoverflow.com/a/38064/1755417
                            int diff = currentDateTime.DayOfWeek - RockDateTime.FirstDayOfWeek;
                            if ( diff < 0 )
                            {
                                diff += 7;
                            }

                            result.Start = currentDateTime.AddDays( -1 * diff ).Date.AddDays( 7 * roundUpCount );
                            result.End = result.Start.Value.AddDays( addCount * 7 );
                            break;

                        case TimeUnitType.Month:
                            result.Start = new DateTime( currentDateTime.Year, currentDateTime.Month, 1 ).AddMonths( roundUpCount );
                            result.End = result.Start.Value.AddMonths( addCount );
                            break;

                        case TimeUnitType.Year:
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
                else if ( slidingDateRangeMode == SlidingDateRangeType.DateRange )
                {
                    result.Start = splitValues[3].AsDateTime();
                    DateTime? endDateTime = splitValues[4].AsDateTime();
                    if ( endDateTime.HasValue )
                    {
                        // add a day to the end since the compare will be "< EndDateTime"
                        result.End = endDateTime.Value.AddDays( 1 );
                    }
                    else
                    {
                        result.End = null;
                    }
                }

                // To avoid confusion about the day or hour of the end of the date range, subtract a microsecond off our 'less than' end date
                // for example, if our end date is 2019-11-7, we actually want all the data less than 2019-11-8, but if a developer does EndDate.DayOfWeek, they would want 2019-11-7 and not 2019-11-8
                // So, to make sure we include all the data for 2019-11-7, but avoid the confusion about what DayOfWeek of the end, we'll compromise by subtracting a millisecond from the end date
                if ( result.End.HasValue && timeUnit != TimeUnitType.Hour )
                {
                    result.End = result.End.Value.AddMilliseconds( -1 );
                }

            }

            return result;
        }

        /// <summary>
        /// Formats the delimited values as a phrase such as "Last 14 Days"
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string FormatDelimitedValues( string value )
        {
            string[] splitValues = ( value ?? string.Empty ).Split( '|' );
            string result = string.Empty;
            if ( splitValues.Length == 5 )
            {
                var slidingDateRangeMode = splitValues[0].ConvertToEnum<SlidingDateRangeType>();
                var numberOfTimeUnits = splitValues[1].AsIntegerOrNull() ?? 1;
                var timeUnitType = splitValues[2].ConvertToEnumOrNull<TimeUnitType>();
                string timeUnitText = timeUnitType != null ? timeUnitType.ConvertToString().PluralizeIf( numberOfTimeUnits != 1 ) : null;
                var start = splitValues[3].AsDateTime();
                var end = splitValues[4].AsDateTime();
                if ( slidingDateRangeMode == SlidingDateRangeType.Current )
                {
                    return string.Format( "{0} {1}", slidingDateRangeMode.ConvertToString(), timeUnitText );
                }
                else if ( ( SlidingDateRangeType.Last | SlidingDateRangeType.Previous | SlidingDateRangeType.Next | SlidingDateRangeType.Upcoming ).HasFlag( slidingDateRangeMode ) )
                {
                    return string.Format( "{0} {1} {2}", slidingDateRangeMode.ConvertToString(), numberOfTimeUnits, timeUnitText );
                }
                else
                {
                    // DateRange
                    var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( value );
                    return dateRange.ToStringAutomatic();
                }
            }

            return result;
        }

        /// <summary>
        ///
        /// </summary>
        [Flags]
        public enum SlidingDateRangeType
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
            /// The date range
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
        ///
        /// </summary>
        public enum TimeUnitType
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

        /// <summary>
        /// Where to put the HTML element of the daterange preview
        /// </summary>
        public enum DateRangePreviewLocation
        {
            /// <summary>
            /// Top
            /// </summary>
            Top,

            /// <summary>
            /// Right
            /// </summary>
            Right,

            /// <summary>
            /// Hide the preview
            /// </summary>
            None
        }
    }
}
