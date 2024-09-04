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
using System.Threading;

namespace Rock
{
    /// <summary>
    /// Special Class that returns current DateTime based on a predefined organization
    /// time zone. This is done because the system may be hosted in a different timezone
    /// than the organization.
    /// </summary>
    /// <remarks>
    /// Rock developers should use <see cref="RockDateTime.Now"/> instead of <see cref="DateTime.Now"/>.
    /// </remarks>
    public class RockDateTime
    {
        /// <summary>
        /// Gets the time zone information for this execution context.
        /// </summary>
        /// <value>
        /// A <see cref="System.TimeZoneInfo"/> that represents the organization's time zone.
        /// </value>
        public static TimeZoneInfo OrgTimeZoneInfo
        {
            get => _orgTimeZoneInfo.Value ?? _defaultTimeZoneInfo;
        }

        /// <summary>
        /// The time zone information for this execution context. This is stored as an
        /// AsyncLocal so that unit tests can change this value to verify proper operation
        /// without conflicting with other tests. Using AsyncLocal is thread-safe and
        /// async-safe. Retrieving the value from an AsyncLocal takes an extra 0.001ms over
        /// an unwrapped TimeZoneInfo property.
        /// </summary>
        private static readonly AsyncLocal<TimeZoneInfo> _orgTimeZoneInfo = new AsyncLocal<TimeZoneInfo>();

        /// <summary>
        /// Gets or sets the default time zone information.
        /// </summary>
        private static TimeZoneInfo _defaultTimeZoneInfo = TimeZoneInfo.Local;

        /// <summary>
        /// This is something similar to the GregorianCalendar's TwoDigitYearMax property which we'll use to
        /// fix the two-digit 'year' for credit card checking.
        /// See https://learn.microsoft.com/en-us/dotnet/core/compatibility/globalization/8.0/twodigityearmax-default
        /// </summary>
        private const int TwoDigitYearMax = 2059;

        /// <summary>
        /// Initializes the specified organization time zone information.
        /// </summary>
        /// <param name="organizationTimeZoneInfo">The organization time zone information.</param>
        public static void Initialize( TimeZoneInfo organizationTimeZoneInfo = null )
        {
            // Initialize the default time zone.
            _defaultTimeZoneInfo = organizationTimeZoneInfo ?? TimeZoneInfo.Local;

            // Initialize the default graduation date.
#pragma warning disable CS0618 // Type or member is obsolete
            var graduationDateWithCurrentYear = new DateTime( Today.Year, 6, 1 );

            if ( graduationDateWithCurrentYear < Today )
            {
                // if the graduation date already occurred this year, return next year' graduation date
                CurrentGraduationDate = graduationDateWithCurrentYear.AddYears( 1 );
            }
            else
            {
                CurrentGraduationDate = graduationDateWithCurrentYear;
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Returns the date and time of the application host server. This prperty
        /// should be used whenever it is necessary to use the local server clock
        /// instead of <see cref="Now"/>.
        /// </summary>
        public static DateTime SystemDateTime => DateTime.Now;

        /// <summary>
        /// Gets current datetime based on the OrgTimeZone setting set in web.config.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> that current datetime based on the Organization's TimeZone.
        /// </value>
        public static DateTime Now
        {
            get
            {
                return TimeZoneInfo.ConvertTime( DateTime.UtcNow, OrgTimeZoneInfo );
            }
        }

        /// <summary>
        /// Gets the current date based on the OrgTimeZone setting set in web.config.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> that current date based on the Organization's TimeZone.
        /// </value>
        public static DateTime Today
        {
            get
            {
                var currentRockDateTime = RockDateTime.Now;
                return currentRockDateTime.Date;
            }
        }

        /// <summary>
        /// Determines whether the given date is a max date value. This discards the time portion to accommodate dates that have dropped it.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>
        ///   <c>true</c> if [is maximum date] [the specified date time]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMaxDate( DateTime dateTime )
        {
            return dateTime.Date == DateTime.MaxValue.Date;
        }

        /// <summary>
        /// Determines whether the given date is a min date value. This discards the time portion to accommodate dates that have dropped it.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>
        ///   <c>true</c> if [is minimum date] [the specified date time]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsMinDate( DateTime dateTime )
        {
            return dateTime.Date == DateTime.MinValue.Date;
        }

        /// <summary>
        /// Converts the local date time to rock date time.
        /// Use this to convert a local datetime (for example, the datetime of a file stored on the server) to the Rock OrgTimeZone
        /// </summary>
        /// <param name="localDateTime">The local date time.</param>
        /// <returns></returns>
        public static DateTime ConvertLocalDateTimeToRockDateTime( DateTime localDateTime )
        {
            return TimeZoneInfo.ConvertTime( localDateTime, OrgTimeZoneInfo );
        }

        /// <summary>
        /// Converts the Rock date time to local date time.
        /// Use this to convert a rock <see cref="DateTime"/> (for example, a
        /// <see cref="DateTime"/> found in the database) to the local server
        /// time zone.
        /// </summary>
        /// <param name="rockDateTime">The Rock date time.</param>
        /// <returns>The local <see cref="DateTime"/>.</returns>
        public static DateTime ConvertRockDateTimeToLocalDateTime( DateTime rockDateTime )
        {
            return TimeZoneInfo.ConvertTime( rockDateTime, OrgTimeZoneInfo, TimeZoneInfo.Local );
        }

        /// <summary>
        /// The Default DayOfWeek that Rock uses if a custom FirstDayOfWeek isn't set
        /// </summary>
        public static DayOfWeek DefaultFirstDayOfWeek = DayOfWeek.Monday;

        /// <summary>
        /// Gets the first day of week which defaults to Monday <seealso cref="DefaultFirstDayOfWeek"/>
        /// </summary>
        /// <value>
        /// The first day of week.
        /// </value>
        public static DayOfWeek FirstDayOfWeek { get; internal set; } = DefaultFirstDayOfWeek;

        /// <summary>
        /// Creates a new datetime based on year, month, day, and handles 2/29 for non leap years (returns 2/28 in this case)
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <returns></returns>
        public static DateTime? New( int year, int month, int day )
        {
            return New( year, month, day, 0, 0, 0, 0 );
        }

        /// <summary>
        /// Creates a new datetime based on year, month, day, and handles 2/29 for non leap years (returns 2/28 in this case)
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <param name="hour">The year.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <param name="millisecond">The millisecond.</param>
        /// <returns></returns>
        public static DateTime? New( int year, int month, int day, int hour, int minute, int second, int millisecond )
        {
            try
            {
                if ( !DateTime.IsLeapYear( year ) && month == 2 && day == 29 )
                {
                    return new DateTime( year, 2, 28, hour, minute, second, millisecond );
                }
                return new DateTime( year, month, day, hour, minute, second, millisecond );
            }
            catch { }

            return ( DateTime? ) null;
        }

        /// <summary>
        /// Gets the current graduation date base on the GradeTransitionDate GlobalAttribute and current datetime.
        /// For example, if the Grade Transition Date is June 1st and the current date is June 1st or earlier, it will return June 1st of the current year;
        /// otherwise, it will return June 1st of next year
        /// </summary>
        /// <value>
        /// The current graduation date.
        /// </value>
        [RockObsolete("1.13")]
        [Obsolete( "Use PersonService.GetCurrentGraduationDate() instead." )]
        public static DateTime CurrentGraduationDate { get; internal set; }

        /// <summary>
        /// Gets the current graduation year based on <see cref="CurrentGraduationDate"/>
        /// </summary>
        /// <value>
        /// The current graduation year.
        /// </value>
        [RockObsolete( "1.13" )] 
        [Obsolete( "Use PersonService.GetCurrentGraduationYear() instead." )]
        public static int CurrentGraduationYear => CurrentGraduationDate.Year;

        /// <summary>
        /// Gets the Date of which Sunday is associated with the specified Date/Time, based on <see cref="RockDateTime.FirstDayOfWeek" />
        /// </summary>
        /// <param name="inputDate">The input date.</param>
        /// <returns></returns>
        public static DateTime GetSundayDate( DateTime inputDate )
        {
            var firstDayOfWeek = RockDateTime.FirstDayOfWeek;
            return GetSundayDate( inputDate, firstDayOfWeek );
        }

        /// <summary>
        /// Gets the Date of which Sunday is associated with the specified Date/Time, based on what the First Day Of Week is defined as.
        /// </summary>
        /// <param name="inputDate">The date time.</param>
        /// <param name="firstDayOfWeek">The first day of week. Use the override of this method with only the inputDate to assume the system setting.</param>
        /// <returns></returns>
        public static DateTime GetSundayDate( DateTime inputDate, DayOfWeek firstDayOfWeek )
        {
            /*
             * 12-09-2019 BJW
             * 
             * I restored the ability to specify the firstDayOfWeek so that each StreakType could have a custom setting. Some streaks, like those
             * for serving, make sense to measure Wed - Tues so that an entire three day "weekend" of services is counted as the same Sunday Date.
             * However, just because the church wants the streak type to have a Wed start of week for the streak type doesn't mean they want the
             * entire system to be configured this way.
             * 
             * See Rock.Model.StreakType.FirstDayOfWeek
             * Also see task: https://app.asana.com/0/1120115219297347/1152845555131825/f 
             */

            // Get the number of days until the next Sunday date
            int sundayDiff = 7 - ( int ) inputDate.DayOfWeek;

            // Figure out which DayOfWeek would be the lastDayOfWeek ( which would be the DayOfWeek before the firstDayOfWeek )
            DayOfWeek lastDayOfWeek;

            if ( firstDayOfWeek == DayOfWeek.Sunday )
            {
                // The day before Sunday is Saturday
                lastDayOfWeek = DayOfWeek.Saturday;
            }
            else
            {
                // if the startOfWeek isn't Sunday, we can just subtract by 1
                lastDayOfWeek = firstDayOfWeek - 1;
            }

            //// There are 3 cases to deal with, and it can get confusing if Sunday isn't the last day of the week
            //// 1) The input date's DOW is Sunday. Today is the Sunday, so the Sunday Date is today
            //// 2) The input date's DOW is after the Last DOW (Today is Thursday, and the week ends next week on Tuesday).
            //// 3) The input date's DOW is before the Last DOW (Today is Monday, but the week ends this week on Tuesday)
            DateTime sundayDate;

            if ( inputDate.DayOfWeek == DayOfWeek.Sunday )
            {
                sundayDate = inputDate;
            }
            else if ( lastDayOfWeek < inputDate.DayOfWeek )
            {
                // If the lastDayOfWeek after the current day of week, we can simply add 
                sundayDate = inputDate.AddDays( sundayDiff );
            }
            else
            {
                // If the current DayOfWeek is on or after the lastDayOfWeek, it'll be the *previous* Sunday.
                // For example, if the Last Day of the Week is Monday (10/7/2019),
                // the Sunday Date will *before* the current date (10/6/2019)
                sundayDate = inputDate.AddDays( sundayDiff - 7 );
            }

            return sundayDate.Date;
        }

        /// <summary>
        /// Returns a human-friendly description of the time of day.
        /// </summary>
        /// <param name="inputDateTime">A valid date/time value</param>
        /// <returns></returns>
        public static string GetTimeOfDay( DateTime inputDateTime )
        {
            string response;
            var hour = inputDateTime.Hour;
            if ( hour >= 5 && hour < 12 )
            {
                response = "Morning";
            }
            else if ( hour >= 12 && hour < 17 )
            {
                response = "Afternoon";
            }
            else if ( hour >= 17 && hour < 21 )
            {
                response = "Evening";
            }
            else
            {
                response = "Night";
            }

            return response;
        }

        /// <summary>
        /// Converts the year value to the appropriate century by using the
        /// TwoDigitYearMax property.  For example, if the TwoDigitYearMax value is 2059,
        /// then a two digit value of 60 will get converted to 1960 while a two digit
        /// value of 59 will get converted to 2059.
        /// </summary>
        /// <remarks>Taken from https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Private.CoreLib/src/System/Globalization/Calendar.cs#L669C1-L685</remarks>
        /// <param name="year"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentOutOfRangeException">if the number given is nagative</exception>
        public static int ToFourDigitYearForCreditCardExpiration( int year )
        {
            if ( year < 0 )
            {
                throw new ArgumentOutOfRangeException( "year", "year cannot be negative." );
            }

            if ( year < 100 )
            {
                return ( TwoDigitYearMax / 100 - ( year > TwoDigitYearMax % 100 ? 1 : 0 ) ) * 100 + year;
            }

            // If the year value is above 100, just return the year value.  Don't have to do
            // the TwoDigitYearMax comparison.
            return year;
        }
    }
}
