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
using System.Globalization;
using Humanizer;
using Humanizer.Localisation;

namespace Rock.Lava.Filters
{
    public static partial class TemplateFilters
    {
        /// <summary>
        /// Attempts to convert string to DateTime. Returns null if unsuccessful.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string AsDateTime( object input )
        {
            var result = input.ToStringSafe().AsDateTime();

            return result.ToStringSafe();
        }

        /* [2021-07-31] DL
         * 
         * Lava Date filters may return DateTime, DateTimeOffset, or string values according to their purpose.
         * Where possible, a filter should return a DateTime value specified in UTC, or a DateTimeOffset.
         * Local DateTime values may give unexpected results if the Rock timezone setting is different from the server timezone.
         * Where a date string is accepted as an input parameter, the Rock timezone is implied unless a timezone is specified.
         */

        /// <summary>
        /// Formats a date using a .NET date format string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string Date( object input, string format = null )
        {
            if ( input == null )
            {
                return null;
            }

            string output;

            // If input is "now", use the current Rock date/time as the input value.
            if ( input.ToString().ToLower() == "now" )
            {
                // To correctly include the Rock configured timezone, we need to use a DateTimeOffset.
                // The DateTime object can only represent local server time or UTC time.
                input = LavaDateTime.NowOffset;
            }

            // Use the General Short Date/Long Time format by default.
            if ( string.IsNullOrWhiteSpace( format ) )
            {
                format = "G";
            }
            // Consider special 'Standard Date' and 'Standard Time' formats.
            else if ( format == "sd" )
            {
                format = "d";
            }
            else if ( format == "st" )
            {
                format = "t";
            }
            // If the format string is a single character, add a space to produce a valid custom format string.
            // (refer http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx#UsingSingleSpecifiers)
            else if ( format.Length == 1 )
            {
                format = " " + format;
            }

            if ( input is DateTimeOffset inputDateTimeOffset )
            {
                // Preserve the value of the specified offset and return the formatted datetime value.
                output = inputDateTimeOffset.ToString( format ).Trim();
            }
            else
            {
                // Convert the input to a valid Rock DateTimeOffset if possible.
                DateTimeOffset? inputDateTime;

                if ( input is DateTime dt )
                {
                    inputDateTime = LavaDateTime.ConvertToRockOffset( dt );
                }
                else
                {
                    inputDateTime = LavaDateTime.ParseToOffset( input.ToString(), null );
                }

                if ( !inputDateTime.HasValue )
                {
                    // Not a valid date, so return the input unformatted.
                    output = input.ToString().Trim();
                }
                else
                {
                    output = LavaDateTime.ToString( inputDateTime.Value, format ).Trim();
                }
            }
            return output;
        }

        /// <summary>
        /// Adds a time interval to a date.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        public static DateTime? DateAdd( object input, object amount, string interval = "d" )
        {
            DateTime? date = null;

            if ( input == null )
            {
                return null;
            }

            if ( input.ToString() == "Now" )
            {
                date = RockDateTime.Now;
            }
            else
            {
                DateTime d;
                bool success = DateTime.TryParse( input.ToString(), out d );
                if ( success )
                {
                    date = d;
                }
            }

            var integerAmount = amount.ToIntSafe();

            if ( date.HasValue )
            {
                switch ( interval )
                {
                    case "y":
                        date = date.Value.AddYears( integerAmount );
                        break;
                    case "M":
                        date = date.Value.AddMonths( integerAmount );
                        break;
                    case "w":
                        date = date.Value.AddDays( integerAmount * 7 );
                        break;
                    case "d":
                        date = date.Value.AddDays( integerAmount );
                        break;
                    case "h":
                        date = date.Value.AddHours( integerAmount );
                        break;
                    case "m":
                        date = date.Value.AddMinutes( integerAmount );
                        break;
                    case "s":
                        date = date.Value.AddSeconds( integerAmount );
                        break;
                }
            }

            return date;
        }

        /// <summary>
        /// Returns the difference between two date/time values, measured in the specified interval.
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="interval">The interval in which the difference is measured.</param>
        /// <returns></returns>
        public static Int64? DateDiff( object startDate, object endDate, string interval )
        {
            var startDto = GetDateTimeOffsetFromInputParameter( startDate, null );
            var endDto = GetDateTimeOffsetFromInputParameter( endDate, null );

            if ( startDto != null && endDto != null )
            {
                var difference = endDto.Value - startDto.Value;

                switch ( interval )
                {
                    case "d":
                        return ( Int64 ) difference.TotalDays;
                    case "h":
                        return ( Int64 ) difference.TotalHours;
                    case "m":
                        return ( Int64 ) difference.TotalMinutes;
                    case "w":
                        return ( Int64 )( difference.TotalDays / 7 );
                    case "M":
                        return ( Int64 ) GetMonthsBetween( startDto.Value, endDto.Value );
                    case "Y":
                        // Return the difference between the dates as the number of whole years.
                        var years = LavaDateTime.ConvertToRockDateTime( endDto.Value ).TotalYears( LavaDateTime.ConvertToRockDateTime( startDto.Value ) );
                        return years;
                    case "s":
                        return ( Int64 ) difference.TotalSeconds;
                    default:
                        return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the number of days in the input month.
        /// </summary>
        /// <param name="input">An optional object that can be parsed as a date/time value, or the keyword 'Now' to specify the current month.</param>
        /// <param name="oMonth">A month of the year, only valid if </param>
        /// <param name="oYear">The o year.</param>
        /// <returns></returns>
        public static int? DaysInMonth( object input, object oMonth = null, object oYear = null )
        {
            int? month;
            int? year;

            if ( input.ToString().IsNotNullOrWhiteSpace() )
            {
                DateTime? date;

                if ( input.ToString().ToLower() == "now" )
                {
                    date = RockDateTime.Now;
                }
                else
                {
                    date = input.ToString().AsDateTime();
                }

                if ( date.HasValue )
                {
                    month = date.Value.Month;
                    year = date.Value.Year;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                if ( oYear == null )
                {
                    year = RockDateTime.Now.Year;
                }
                else
                {
                    year = oYear.ToString().AsIntegerOrNull();
                }

                month = oMonth.ToString().AsIntegerOrNull();
            }

            if ( month.HasValue && year.HasValue )
            {
                return System.DateTime.DaysInMonth( year.Value, month.Value );
            }

            return null;
        }

        /// <summary>
        /// Returns a human-friendly description of the number of days between the input date and today.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string DaysFromNow( object input )
        {
            DateTime dtInputDate = GetDateFromObject( input ).Date;
            DateTime dtCompareDate = RockDateTime.Now.Date;

            int daysDiff = ( dtInputDate - dtCompareDate ).Days;

            string response = string.Empty;

            switch ( daysDiff )
            {
                case -1:
                    {
                        response = "yesterday";
                        break;
                    }

                case 0:
                    {
                        response = "today";
                        break;
                    }

                case 1:
                    {
                        response = "tomorrow";
                        break;
                    }

                default:
                    {
                        if ( daysDiff > 0 )
                        {
                            response = string.Format( "in {0} days", daysDiff );
                        }
                        else
                        {
                            response = string.Format( "{0} days ago", daysDiff * -1 );
                        }

                        break;
                    }
            }

            return response;
        }

        /// <summary>
        /// Returns the number of complete days that have elapsed between now and a given date.
        /// Partial days are ignored. For example, if the current date is 4-Jan-2010 10:00am, it is 2 days since 1-Jan-2010 11:00am.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static int? DaysSince( object input )
        {
            var days = DaysUntil( input );

            if ( days.HasValue )
            {
                return days.Value * -1;
            }

            return null;
        }

        /// <summary>
        /// Returns the number of complete days between now and a given date.
        /// Partial days are ignored. For example, if the current date is 1-Jan-2010 10:00am, there are 2 days until 4-Jan-2010 11:00am.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static int? DaysUntil( object input )
        {
            DateTime date;

            if ( input == null )
            {
                return null;
            }

            if ( input is DateTime )
            {
                date = ( DateTime ) input;
            }
            else
            {
                DateTime.TryParse( input.ToString(), out date );
            }

            if ( date == null )
            {
                return null;
            }

            return ( date - RockDateTime.Now ).Days;
        }

        /// <summary>
        /// Returns a human-friendly description of the difference between the input date/time and the current date/time.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="compareDateTime">The compare date.</param>
        /// <remarks>
        /// Note that this function does not return the expected measure of "weeks" for a period between 7 and 28 days.
        /// https://github.com/Humanizr/Humanizer/issues/765
        /// </remarks>
        /// <returns></returns>
        public static string HumanizeDateTime( object input, object compareDateTime = null )
        {
            if ( input == null )
            {
                return string.Empty;
            }

            DateTime dtInput;
            DateTime dtCompare;

            if ( input != null && input is DateTime )
            {
                dtInput = ( DateTime ) input;
            }
            else
            {
                if ( input == null || !DateTime.TryParse( input.ToString(), out dtInput ) )
                {
                    return string.Empty;
                }
            }

            if ( compareDateTime == null || !DateTime.TryParse( compareDateTime.ToString(), out dtCompare ) )
            {
                dtCompare = RockDateTime.Now;
            }

            return dtInput.Humanize( true, dtCompare );
        }

        /// <summary>
        /// Returns a human-friendly description of the difference between two input date/times with a specified level of precision.
        /// Supports 'Now' as an end date parameter value.
        /// </summary>
        /// <param name="sStartDate">The start date.</param>
        /// <param name="sEndDate">The end date.</param>
        /// <param name="precision">The precision, either as a level of accuracy or a specific unit of measure.</param>
        /// <returns></returns>
        public static string HumanizeTimeSpan( object sStartDate, object sEndDate, object precision = null, string direction = "min" )
        {
            if ( precision == null )
            {
                return HumanizeTimeSpanInternal( sStartDate, sEndDate );
            }

            int? precisionUnit = precision.ToStringSafe().AsIntegerOrNull();

            // If the precision parameter is a string that cannot be converted to an integer, assume it represents a unit of measure.
            if ( precision is String
                 && precisionUnit == null )
            {
                return HumanizeTimeSpanInternal( sStartDate, sEndDate, precision.ToString(), "min" );
            }

            var startDateTime = GetDateFromObject( sStartDate );
            var endDateTime = GetDateFromObject( sEndDate );

            if ( startDateTime != DateTime.MinValue
                 && endDateTime != DateTime.MinValue )
            {
                var difference = endDateTime - startDateTime;

                return difference.Humanize( precisionUnit.Value );
            }
            else
            {
                return "Could not parse one or more of the dates provided into a valid DateTime";
            }
        }

        /// <summary>
        /// Returns a human-friendly description of the difference between two input date/times with a specified level of precision.
        /// </summary>
        /// <param name="startDate">The s start date.</param>
        /// <param name="endDate">The s end date.</param>
        /// <param name="unit">The minimum unit.</param>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        private static string HumanizeTimeSpanInternal( object startDate, object endDate, string unit = "Day", string direction = "min" )
        {
            DateTime startDateTime = GetDateFromObject( startDate );
            DateTime endDateTime = GetDateFromObject( endDate );

            TimeUnit unitValue = TimeUnit.Day;

            switch ( unit )
            {
                case "Year":
                    unitValue = TimeUnit.Year;
                    break;
                case "Month":
                    unitValue = TimeUnit.Month;
                    break;
                case "Week":
                    unitValue = TimeUnit.Week;
                    break;
                case "Day":
                    unitValue = TimeUnit.Day;
                    break;
                case "Hour":
                    unitValue = TimeUnit.Hour;
                    break;
                case "Minute":
                    unitValue = TimeUnit.Minute;
                    break;
                case "Second":
                    unitValue = TimeUnit.Second;
                    break;
            }

            if ( startDateTime != DateTime.MinValue && endDateTime != DateTime.MinValue )
            {
                TimeSpan difference = endDateTime - startDateTime;

                if ( direction.ToLower() == "max" )
                {
                    return difference.Humanize( maxUnit: unitValue );
                }
                else
                {
                    return difference.Humanize( minUnit: unitValue );
                }
            }
            else
            {
                return "Could not parse one or more of the dates provided into a valid DateTime";
            }
        }

        /// <summary>
        /// Advances the date to a specific day in the next 7 days.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="sDayOfWeek">The starting day of week.</param>
        /// <param name="includeCurrentDay">if set to <c>true</c> includes the current day as the current week.</param>
        /// <param name="numberOfWeeks">The number of weeks (must be non-zero).</param>
        /// <returns></returns>
        //public static DateTime? NextDayOfTheWeek( object input, string sDayOfWeek, object includeCurrentDay, object numberOfWeeks )
        //{
        //    int weeks = numberOfWeeks.ToStringSafe().AsIntegerOrNull() ?? 1;
        //    bool includeCurrent = includeCurrentDay.ToStringSafe().AsBoolean( false );

        //    return NextDayOfTheWeek( input, sDayOfWeek, includeCurrent, weeks );
        //}

        /// <summary>
        /// Advances the date to a specific day in the next 7 days.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="dayOfWeek">The starting day of week.</param>
        /// <returns></returns>
        public static DateTime? NextDayOfTheWeek( object input, string dayOfWeek )
        {
            return NextDayOfTheWeek( input, dayOfWeek, false, 1 );
        }

        /// <summary>
        /// Advances the date to a specific day in the next 7 days.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="dayOfWeek">The starting day of week.</param>
        /// <param name="includeCurrentDay">if set to <c>true</c> includes the current day as the current week.</param>
        /// <returns></returns>
        public static DateTime? NextDayOfTheWeek( object input, string dayOfWeek, object includeCurrentDay )
        {
            return NextDayOfTheWeek( input, dayOfWeek, includeCurrentDay, 1 );
        }

        /// <summary>
        /// Returns the date corresponding to the next day of the week that matches a specified weekday.
        /// </summary>
        /// <param name="input">The input. A DateTime value or string, or the keyword 'Now' to represent the current date.</param>
        /// <param name="dayOfWeek">The day of week.</param>
        /// <param name="includeCurrentDay">if set to <c>true</c> includes the current day as the current week.</param>
        /// <param name="numberOfWeeks">The number of weeks (must be non-zero).</param>
        /// <returns></returns>
        public static DateTime? NextDayOfTheWeek( object input, string dayOfWeek, object includeCurrentDay, object numberOfWeeks )
        {
            DateTime date;
            DayOfWeek dayOfWeekValue;

            if ( input == null )
            {
                return null;
            }

            bool includeCurrent = includeCurrentDay.ToStringSafe().AsBoolean( false );

            // Check for invalid number of weeks
            int weeks = numberOfWeeks.ToIntSafe( 1 );

            if ( weeks == 0 )
            {
                return null;
            }

            // Get the date value
            if ( input is DateTime )
            {
                date = ( DateTime ) input;
            }
            else
            {
                if ( input.ToString() == "Now" )
                {
                    date = RockDateTime.Now;
                }
                else
                {
                    DateTime.TryParse( input.ToString(), out date );
                }
            }

            if ( date == null )
            {
                return null;
            }

            // Get the day of week value
            if ( !Enum.TryParse( dayOfWeek, out dayOfWeekValue ) )
            {
                return null;
            }

            // Calculate the offset
            int daysUntilWeekDay;

            if ( includeCurrent )
            {
                daysUntilWeekDay = ( ( int ) dayOfWeekValue - ( int ) date.DayOfWeek + 7 ) % 7;
            }
            else
            {
                daysUntilWeekDay = ( ( ( ( int ) dayOfWeekValue - 1 ) - ( int ) date.DayOfWeek + 7 ) % 7 ) + 1;
            }

            // When a positive number of weeks is given, since the number of weeks defaults to 1
            // (which means the current week) we need to shift the numberOfWeeks down by 1 so
            // the calculation below is correct.
            if ( weeks >= 1 )
            {
                weeks--;
            }

            return date.AddDays( daysUntilWeekDay + ( weeks * 7 ) );
        }

        /// <summary>
        /// Sets the time to midnight on the date provided.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static DateTime? ToMidnight( object input )
        {
            if ( input == null )
            {
                return null;
            }

            if ( input is DateTime )
            {
                return ( ( DateTime ) input ).Date;
            }

            if ( input.ToString() == "Now" )
            {
                return RockDateTime.Now.Date;
            }
            else
            {
                DateTime date;

                if ( DateTime.TryParse( input.ToString(), out date ) )
                {
                    return date.Date;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the date of the first Sunday following the input date.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>
        public static string SundayDate( object input )
        {
            if ( input == null )
            {
                return null;
            }

            DateTime date = DateTime.MinValue;

            if ( input.ToString() == "Now" )
            {
                date = RockDateTime.Now;
            }
            else
            {
                if ( !DateTime.TryParse( input.ToString(), out date ) )
                {
                    return null;
                }
            }

            if ( date != DateTime.MinValue )
            {
                return date.SundayDate().ToShortDateString();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns a human-friendly description of the time of day.
        /// </summary>
        /// <param name="input">A valid date/time value</param>
        /// <returns></returns>
        public static string TimeOfDay( object input )
        {
            var dtoInput = GetDateTimeOffsetFromInputParameter( input, null );

            if ( dtoInput == null )
            {
                return string.Empty;
            }

            var response = RockDateTime.GetTimeOfDay( dtoInput.Value.DateTime );
            return response;
        }

        /// <summary>
        /// Return true if the date is within the provided range, false if outside of range
        /// </summary>
        /// <param name="input">The input date.</param>
        /// <param name="start">The range start date.</param>
        /// <param name="end">The range end date.</param>
        /// <param name="format">Optional format string.</param>
        /// <returns>boolean value of input being between the start and end dates.</returns>
        public static bool IsDateBetween( object input, object start, object end, string format = null )
        {
            DateTime? target = ParseInputForIsDateBetween( input, "target", format );
            DateTime? rangeStart = ParseInputForIsDateBetween( start, "start", format );
            DateTime? rangeEnd = ParseInputForIsDateBetween( end, "end", format );

            if ( target.HasValue && rangeEnd.HasValue && rangeStart.HasValue )
            {
                if ( DateTime.Compare( target.Value, rangeStart.Value ) >= 0 && DateTime.Compare( target.Value, rangeEnd.Value ) <= 0 )
                {
                    return true;
                }
                return false;
            }
            throw new Exception( "Unable to parse input, start, and end" );
        }

        #region Support Functions

        /// <summary>
        /// Convert a generic object to a DateTime value.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        private static DateTime GetDateFromObject( object date )
        {
            DateTime oDateTime = DateTime.MinValue;

            if ( date is String )
            {
                if ( ( string ) date == "Now" )
                {
                    return RockDateTime.Now;
                }
                else
                {
                    if ( DateTime.TryParse( ( string ) date, out oDateTime ) )
                    {
                        return oDateTime;
                    }
                    else
                    {
                        return DateTime.MinValue;
                    }
                }
            }
            else if ( date is DateTime )
            {
                return ( DateTime ) date;
            }

            return DateTime.MinValue;
        }

        /// <summary>
        /// Converts an input value to a DateTimeOffset.
        /// </summary>
        /// <param name="input">The date.</param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static DateTimeOffset? GetDateTimeOffsetFromInputParameter( object input, DateTimeOffset? defaultValue )
        {
            if ( input is String inputString )
            {
                if ( inputString.Trim().ToLower() == "now" )
                {
                    return LavaDateTime.NowOffset;
                }
                else
                {
                    return LavaDateTime.ParseToOffset( inputString, defaultValue );
                }
            }
            else if ( input is DateTime dt )
            {
                return LavaDateTime.ConvertToDateTimeOffset( dt );
            }
            else if ( input is DateTimeOffset inputDateTimeOffset )
            {
                return inputDateTimeOffset;
            }

            return defaultValue;
        }

        /// <summary>
        /// Get the number of months between two dates.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private static int GetMonthsBetween( DateTime from, DateTime to )
        {
            if ( from > to )
            {
                return GetMonthsBetween( to, from );
            }

            var monthDiff = Math.Abs( ( to.Year * 12 + ( to.Month - 1 ) ) - ( from.Year * 12 + ( from.Month - 1 ) ) );

            if ( from.AddMonths( monthDiff ) > to || to.Day < from.Day )
            {
                return monthDiff - 1;
            }
            else
            {
                return monthDiff;
            }
        }

        private static int GetMonthsBetween( DateTimeOffset from, DateTimeOffset to )
        {
            if ( from > to )
            {
                return GetMonthsBetween( to, from );
            }

            var monthDiff = Math.Abs( ( to.Year * 12 + ( to.Month - 1 ) ) - ( from.Year * 12 + ( from.Month - 1 ) ) );

            if ( from.AddMonths( monthDiff ) > to || to.Day < from.Day )
            {
                return monthDiff - 1;
            }
            else
            {
                return monthDiff;
            }
        }

        /// <summary>
        /// This filter parses the user input for the lava filter for the IsDateBetween method. Should not be modified without testing with that filter.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="parameter">Which parameter from lava filter, used to set time of day in some cases.</param>
        /// <param name="format">Optional format string for parsing string to datetime.</param>
        /// <returns></returns>
        private static DateTime ParseInputForIsDateBetween( object input, string parameter, string format = null )
        {
            DateTime result;
            if ( input.GetType() == typeof( string ) )
            {
                if ( format != null )
                {
                    var isValid = DateTime.TryParseExact( input.ToString(), format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result );
                    if ( !isValid )
                    {
                        throw new Exception( "Unable to parse " + parameter + " input as date with format: \"" + format + "\"." );
                    }
                }
                else
                {
                    var isValid = DateTime.TryParse( input.ToString(), out result );
                    if ( isValid )
                    {
                        if ( parameter == "end" )
                        {
                            result = new DateTime( result.Year, result.Month, result.Day, 23, 59, 59 );
                        }
                        else if ( parameter == "start" )
                        {
                            result = new DateTime( result.Year, result.Month, result.Day, 0, 0, 0 );
                        }
                    }
                    else
                    {
                        throw new Exception( "Unable to parse " + parameter + " input as date." );
                    }
                }
            }
            else if ( input.GetType() == typeof( DateTimeOffset ) )
            {
                var offset = ( DateTimeOffset ) input;
                result = offset.DateTime;
            }
            else if ( input.GetType() == typeof( DateTime ) )
            {
                result = ( DateTime ) input;
            }
            else
            {
                throw new Exception( "Invalid Input: " + parameter + " input must be of type string or date" );
            }
            return result;
        }

        #endregion
    }
}
