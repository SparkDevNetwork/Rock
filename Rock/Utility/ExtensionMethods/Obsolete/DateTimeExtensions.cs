﻿// <copyright>
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

namespace Rock
{
    /// <summary>
    /// DateTime and TimeStamp Extensions
    /// </summary>
    public static partial class ExtensionMethods
    {
        #region DateTime Extensions

        /// <summary>
        /// Returns the age at the current date.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static int Age( DateTime? start )
        {
            if ( start.HasValue )
                return Age( start.Value );
            else
                return 0;
        }

        /// <summary>
        /// Returns the age at the current date.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static int Age( DateTime start )
        {
            var now = RockDateTime.Today;
            int age = now.Year - start.Year;
            if ( start > now.AddYears( -age ) )
            {
                age--;
            }

            return age;
        }

        /// <summary>
        /// The total months.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static int TotalMonths( DateTime end, DateTime start )
        {
            return ( end.Year * 12 + end.Month ) - ( start.Year * 12 + start.Month );
        }

        /// <summary>
        /// The total years.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static int TotalYears( DateTime end, DateTime start )
        {
            return ( end.Year ) - ( start.Year );
        }

        /// <summary>
        /// Returns a friendly elapsed time string.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <param name="includeTime">if set to <c>true</c> [include time].</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToElapsedString( DateTime? dateTime, bool condensed = false, bool includeTime = true )
        {
            if ( dateTime.HasValue )
            {
                return ToElapsedString( dateTime.Value, condensed, includeTime );
            }
            else
                return string.Empty;
        }

        /// <summary>
        /// Returns a friendly elapsed time string.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <param name="includeTime">if set to <c>true</c> [include time].</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToElapsedString( DateTime dateTime, bool condensed = false, bool includeTime = true )
        {
            DateTime start = dateTime;
            DateTime end = RockDateTime.Now;

            string direction = " Ago";
            TimeSpan timeSpan = end.Subtract( start );
            if ( timeSpan.TotalMilliseconds < 0 )
            {
                direction = " From Now";
                start = end;
                end = dateTime;
                timeSpan = timeSpan.Negate();
            }

            string duration = "";

            if ( timeSpan.TotalHours < 24 && includeTime )
            {
                // Less than one second
                if ( timeSpan.TotalSeconds < 2 )
                    duration = string.Format( "1{0}", condensed ? "sec" : " Second" );
                else if ( timeSpan.TotalSeconds < 60 )
                    duration = string.Format( "{0:N0}{1}", Math.Truncate( timeSpan.TotalSeconds ), condensed ? "sec" : " Seconds" );
                else if ( timeSpan.TotalMinutes < 2 )
                    duration = string.Format( "1{0}", condensed ? "min" : " Minute" );
                else if ( timeSpan.TotalMinutes < 60 )
                    duration = string.Format( "{0:N0}{1}", Math.Truncate( timeSpan.TotalMinutes ), condensed ? "min" : " Minutes" );
                else if ( timeSpan.TotalHours < 2 )
                    duration = string.Format( "1{0}", condensed ? "hr" : " Hour" );
                else if ( timeSpan.TotalHours < 24 )
                    duration = string.Format( "{0:N0}{1}", Math.Truncate( timeSpan.TotalHours ), condensed ? "hr" : " Hours" );
            }

            if ( duration == "" )
            {
                if ( timeSpan.TotalDays < 2 )
                    duration = string.Format( "1{0}", condensed ? "day" : " Day" );
                else if ( timeSpan.TotalDays < 31 )
                    duration = string.Format( "{0:N0}{1}", Math.Truncate( timeSpan.TotalDays ), condensed ? "days" : " Days" );
                else if ( TotalMonths( end, start ) <= 1 )
                    duration = string.Format( "1{0}", condensed ? "mon" : " Month" );
                else if ( TotalMonths( end, start ) <= 18 )
                    duration = string.Format( "{0:N0}{1}", TotalMonths( end, start ), condensed ? "mon" : " Months" );
                else if ( TotalYears( end, start ) <= 1 )
                    duration = string.Format( "1{0}", condensed ? "yr" : " Year" );
                else
                    duration = string.Format( "{0:N0}{1}", TotalYears( end, start ), condensed ? "yrs" : " Years" );
            }

            return duration + ( condensed ? "" : direction );
        }

        /// <summary>
        /// Returns a string in FB style relative format (x seconds ago, x minutes ago, about an hour ago, etc.).
        /// or if max days has already passed in FB DateTime format (February 13 at 11:28am or November 5, 2011 at 1:57pm).
        /// </summary>
        /// <param name="dateTime">the DateTime to convert to relative time.</param>
        /// <param name="maxDays">maximum number of days before formatting in FB date-time format (ex. November 5, 2011 at 1:57pm)</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToRelativeDateString( DateTime? dateTime, int? maxDays = null )
        {
            if ( dateTime.HasValue )
            {
                return ToRelativeDateString( dateTime.Value, maxDays );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns the value for <see cref="DateTime.ToShortDateString"/> or empty string if the date is null
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        public static string ToShortDateString( DateTime? dateTime )
        {
            if ( dateTime.HasValue )
            {
                return dateTime.Value.ToShortDateString();
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns the dateTime in ISO-8601 ( https://en.wikipedia.org/wiki/ISO_8601 ) format. Use this when serializing a date/time as an AttributeValue, UserPreference, etc
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToISO8601DateString( DateTime? dateTime )
        {
            if ( dateTime.HasValue )
            {
                return ToISO8601DateString( dateTime.Value );
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns the dateTime in ISO-8601 ( https://en.wikipedia.org/wiki/ISO_8601 ) format. Use this when serializing a date/time as an AttributeValue, UserPreference, etc
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToISO8601DateString( DateTime dateTime )
        {
            return dateTime.ToString( "o" ) ?? string.Empty;
        }

        /// <summary>
        /// Returns a string in relative format (x seconds ago, x minutes ago, about an hour ago, in x seconds,
        /// in x minutes, in about an hour, etc.) or if time difference is greater than max days in long format (February
        /// 13 at 11:28am or November 5, 2011 at 1:57pm).
        /// </summary>
        /// <param name="dateTime">the DateTime to convert to relative time.</param>
        /// <param name="maxDays">maximum number of days before formatting in long format (ex. November 5, 2011 at 1:57pm) </param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToRelativeDateString( DateTime dateTime, int? maxDays = null )
        {
            try
            {
                DateTime now = RockDateTime.Now;

                string nowText = "just now";
                string format = "{0} ago";
                TimeSpan timeSpan = now - dateTime;
                if ( dateTime > now )
                {
                    nowText = "now";
                    format = "in {0}";
                    timeSpan = dateTime - now;
                }

                double seconds = timeSpan.TotalSeconds;
                double minutes = timeSpan.TotalMinutes;
                double hours = timeSpan.TotalHours;
                double days = timeSpan.TotalDays;
                double weeks = days / 7;
                double months = days / 30;
                double years = days / 365;

                // Just return in long format if max days has passed.
                if ( maxDays.HasValue && days > maxDays )
                {
                    if ( now.Year == dateTime.Year )
                    {
                        return dateTime.ToString( @"MMMM d a\t h:mm tt" );
                    }
                    else
                    {
                        return dateTime.ToString( @"MMMM d, yyyy a\t h:mm tt" );
                    }
                }

                if ( Math.Round( seconds ) < 5 )
                {
                    return nowText;
                }
                else if ( minutes < 1.0 )
                {
                    return string.Format( format, Math.Floor( seconds ) + " seconds" );
                }
                else if ( Math.Floor( minutes ) == 1 )
                {
                    return string.Format( format, "1 minute" );
                }
                else if ( hours < 1.0 )
                {
                    return string.Format( format, Math.Floor( minutes ) + " minutes" );
                }
                else if ( Math.Floor( hours ) == 1 )
                {
                    return string.Format( format, "about an hour" );
                }
                else if ( days < 1.0 )
                {
                    return string.Format( format, Math.Floor( hours ) + " hours" );
                }
                else if ( Math.Floor( days ) == 1 )
                {
                    return string.Format( format, "1 day" );
                }
                else if ( weeks < 1 )
                {
                    return string.Format( format, Math.Floor( days ) + " days" );
                }
                else if ( Math.Floor( weeks ) == 1 )
                {
                    return string.Format( format, "1 week" );
                }
                else if ( months < 3 )
                {
                    return string.Format( format, Math.Floor( weeks ) + " weeks" );
                }
                else if ( months <= 12 )
                {
                    return string.Format( format, Math.Floor( months ) + " months" );
                }
                else if ( Math.Floor( years ) <= 1 )
                {
                    return string.Format( format, "1 year" );
                }
                else
                {
                    return string.Format( format, Math.Floor( years ) + " years" );
                }
            }
            catch ( Exception )
            {
            }
            return "";
        }

        /// <summary>
        /// Converts the date to an Epoch of milliseconds since 1970/1/1.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static long ToJavascriptMilliseconds( DateTime dateTime )
        {
            return ( long ) ( dateTime.ToUniversalTime() - new DateTime( 1970, 1, 1 ) ).TotalMilliseconds;
        }

        /// <summary>
        /// Converts the date to a string containing month and day values ( culture-specific ).
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToMonthDayString( DateTime dateTime )
        {
            var dateTimeFormat = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            string mdp = dateTimeFormat.ShortDatePattern;
            mdp = mdp.Replace( dateTimeFormat.DateSeparator + "yyyy", "" ).Replace( "yyyy" + dateTimeFormat.DateSeparator, "" );
            return dateTime.ToString( mdp );
        }

        /// <summary>
        /// Returns the date of the start of the month for the specified date/time.
        /// For example 3/23/2021 11:15am will return 3/1/2021 00:00:00.
        /// </summary>
        /// <param name="dt">The DateTime.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime StartOfMonth( DateTime dt )
        {
            return new DateTime( dt.Year, dt.Month, 1 );
        }

        /// <summary>
        /// Returns the Date of the last day of the month.
        /// For example 3/23/2021 11:15am will return 3/31/2021 00:00:00.
        /// </summary>
        /// <param name="dt">The DateTime</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime EndOfMonth( DateTime dt )
        {
            return new DateTime( dt.Year, dt.Month, 1 ).AddMonths( 1 ).Subtract( new TimeSpan( 1, 0, 0, 0, 0 ) );
        }

        /// <summary>
        /// Returns the date of the start of the week for the specified date/time.
        /// Use <see cref="RockDateTime.FirstDayOfWeek"/> for startOfWeek if you want to have this based on the configured FirstDateOfWeek setting
        /// </summary>
        /// <param name="dt">The DateTime.</param>
        /// <param name="startOfWeek">The start of week.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime StartOfWeek( DateTime dt, DayOfWeek startOfWeek )
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if ( diff < 0 )
            {
                diff += 7;
            }

            return dt.AddDays( -1 * diff ).Date;
        }

        /// <summary>
        /// Returns the date of the last day of the week for the specified date/time.
        /// Use <see cref="RockDateTime.FirstDayOfWeek"/> for startOfWeek if you want to have this based on the configured FirstDateOfWeek setting.
        /// from http://stackoverflow.com/a/38064/1755417
        /// </summary>
        /// <param name="dt">The DateTime.</param>
        /// <param name="startOfWeek">The start of week.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime EndOfWeek( DateTime dt, DayOfWeek startOfWeek )
        {
            return dt.StartOfWeek( startOfWeek ).AddDays( 6 );
        }

        /// <summary>
        /// Gets the Date of which Sunday is associated with the specified Date/Time, based on <see cref="RockDateTime.FirstDayOfWeek" />
        /// </summary>
        /// <param name="dt">The DateTime.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime SundayDate( DateTime dt )
        {
            return RockDateTime.GetSundayDate( dt );
        }

        /// <summary>
        /// Gets the Date of which Sunday is associated with the specified Date/Time, based on <see cref="RockDateTime.FirstDayOfWeek"/>
        /// </summary>
        /// <param name="dt">The date to check.</param>
        /// <param name="startOfWeek">The start of week.</param>
        /// <returns></returns>
        [Obsolete( "Use GetSundayDate without the firstDayOfWeek parameter" )]
        [RockObsolete("1.10")]
        public static DateTime SundayDate( DateTime dt, DayOfWeek startOfWeek = DayOfWeek.Monday )
        {
            return RockDateTime.GetSundayDate( dt );
        }

        /// <summary>
        /// Gets the week of month.
        /// Use <see cref="RockDateTime.FirstDayOfWeek"/> for firstDayOfWeek if you want to have this based on the configured FirstDateOfWeek setting.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="firstDayOfWeek">The first day of week. For example" RockDateTime.FirstDayOfWeek</param>
        /// <returns></returns>
        /// <remarks>
        /// from http://stackoverflow.com/a/2136549/1755417 but with an option to specify the FirstDayOfWeek
        /// </remarks>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static int GetWeekOfMonth( DateTime dateTime, DayOfWeek firstDayOfWeek )
        {
            DateTime firstDayOfMonth = new DateTime( dateTime.Year, dateTime.Month, 1 );

            // note: CalendarWeekRule doesn't matter since we are subtracting
            return GetWeekOfYear( dateTime, CalendarWeekRule.FirstDay, firstDayOfWeek ) - GetWeekOfYear( firstDayOfMonth, CalendarWeekRule.FirstDay, firstDayOfWeek ) + 1;
        }

        /// <summary>
        /// The _gregorian calendar
        /// from http://stackoverflow.com/a/2136549/1755417
        /// </summary>
        private static GregorianCalendar _gregorianCalendar = new GregorianCalendar();

        /// <summary>
        /// Gets the week of year.
        /// Use <see cref="RockDateTime.FirstDayOfWeek"/> for firstDayOfWeek if you want to have this based on the configured FirstDateOfWeek setting.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="calendarWeekRule">The calendar week rule.</param>
        /// <param name="firstDayOfWeek">The first day of week.</param>
        /// <returns></returns>
        /// <remarks>
        /// from http://stackoverflow.com/a/2136549/1755417, but with an option to specify the FirstDayOfWeek (for example RockDateTime.FirstDayOfWeek)
        /// </remarks>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static int GetWeekOfYear( DateTime dateTime, CalendarWeekRule calendarWeekRule, DayOfWeek firstDayOfWeek )
        {
            return _gregorianCalendar.GetWeekOfYear( dateTime, calendarWeekRule, firstDayOfWeek );
        }

        /// <summary>
        /// Converts a DateTime to the short date/time format.
        /// </summary>
        /// <param name="dt">The DateTime.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToShortDateTimeString( DateTime dt )
        {
            return dt.ToShortDateString() + " " + dt.ToShortTimeString();
        }

        /// <summary>
        /// To the RFC822 date time.
        /// From https://madskristensen.net/blog/convert-a-date-to-the-rfc822-standard-for-use-in-rss-feeds/
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToRfc822DateTime( DateTime dateTime )
        {
            int offset = TimeZone.CurrentTimeZone.GetUtcOffset( DateTime.Now ).Hours;
            string timeZone = "+" + offset.ToString().PadLeft( 2, '0' );

            if ( offset < 0 )
            {
                int i = offset * -1;
                timeZone = "-" + i.ToString().PadLeft( 2, '0' );
            }

            return dateTime.ToString( "ddd, dd MMM yyyy HH:mm:ss " + timeZone.PadRight( 5, '0' ) );
        }

        /// <summary>
        /// Returns a formatted age based on the specified birthdate.
        /// If the age is less than a year, it will be the age in months or days (depending on how old they are).
        /// For example: 14 yrs, 1 yr, 6 mos, 4 days
        /// </summary>
        /// <param name="dateTime">The BirthDate.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string GetFormattedAge( DateTime dateTime )
        {
            DateTime today = RockDateTime.Today;
            int age = today.Year - dateTime.Year;
            if ( dateTime > today.AddYears( -age ) )
            {
                // their birthdate is after today's date, so they aren't a year older yet
                age--;
            }

            if ( age > 0 )
            {
                return age + ( age == 1 ? " yr" : " yrs" );
            }
            else if ( age < -1 )
            {
                return string.Empty;
            }

            int months = today.Month - dateTime.Month;
            if ( dateTime.Year < today.Year )
            {
                months = months + 12;
            }
            if ( dateTime.Day > today.Day )
            {
                months--;
            }
            if ( months > 0 )
            {
                return months + ( months == 1 ? " mo" : " mos" );
            }

            int days = today.Day - dateTime.Day;
            if ( days < 0 )
            {
                // Add the number of days in the birth month
                var birthMonth = new DateTime( dateTime.Year, dateTime.Month, 1 );
                days = days + birthMonth.AddMonths( 1 ).AddDays( -1 ).Day;
            }
            return days + ( days == 1 ? " day" : " days" );
        }

        /// <summary>
        ///  Determines whether the DateTime is in the future.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>true if the value is in the future, false if not.</returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static bool IsFuture( DateTime dateTime )
        {
            return dateTime > RockDateTime.Now;
        }

        /// <summary>
        ///  Determines whether the DateTime is in the past.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>true if the value is in the past, false if not.</returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static bool IsPast( DateTime dateTime )
        {
            return dateTime < RockDateTime.Now;
        }

        /// <summary>
        ///  Determines whether the DateTime is today.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns>true if the value is in the past, false if not.</returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static bool IsToday( DateTime dateTime )
        {
            return dateTime.Date == RockDateTime.Today;
        }

        /// <summary>
        /// Gets the date key. For example: 3/24/2021 11:15 am, will return "20210324".
        /// This handy for the various DateKey columns used in some of Rock tables
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static int AsDateKey( DateTime dateTime )
        {
            return dateTime.ToString( "yyyyMMdd" ).AsInteger();
        }

        /// <summary>
        ///  Gets the Start of the Day for the specified DateTime.
        ///  For example: 3/24/2021 11:15 am, will return 3/24/2021 00:00:00.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime StartOfDay( DateTime dateTime )
        {
            return new DateTime( dateTime.Year, dateTime.Month, dateTime.Day );
        }

        /// <summary>
        ///  Gets the End of the Day (last millisecond) for the specified DateTime.
        ///  For example: 3/24/2021 11:15 am, will return 3/24/2021 23:59:59:999.
        ///  Gets the DateTime of the last day of the year with the time set to "23:59:59:999". The last moment of the last day of the year.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime EndOfDay( DateTime dateTime )
        {
            return new DateTime( dateTime.Year, dateTime.Month, dateTime.Day ).AddDays( 1 ).Subtract( new TimeSpan( 0, 0, 0, 0, 1 ) );
        }

        /// <summary>
        ///  Gets the End of the Year (last millisecond) for the specified DateTime.
        ///  For example: 3/24/2021 11:15 am, will return 12/31/2021 23:59:59:999.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime EndOfYear( DateTime dateTime )
        {
            return new DateTime( dateTime.Year, 1, 1 ).AddYears( 1 ).Subtract( new TimeSpan( 1, 0, 0, 0, 0 ) );
        }

        /// <summary>
        /// Returns the date of the start of the year for the specified date/time.
        /// For example 3/23/2021 11:15am will return 1/1/2021 00:00:00.
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime StartOfYear( DateTime dateTime )
        {
            return new DateTime( dateTime.Year, 1, 1 );
        }

        /// <summary>
        /// Gets the next weekday.
        /// https://stackoverflow.com/questions/6346119/datetime-get-next-tuesday
        /// </summary>
        /// <param name="dateTime">The date time.</param>
        /// <param name="day">The day.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime GetNextWeekday( DateTime dateTime, DayOfWeek day)
        {
            // The (... + 7) % 7 ensures we end up with a value in the range [0, 6]
            int daysToAdd = ((int) day - (int) dateTime.DayOfWeek + 7) % 7;
            return dateTime.AddDays(daysToAdd);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> that is in <see cref="RockDateTime.OrgTimeZoneInfo"/>
        /// into a <see cref="DateTimeOffset"/> that is also in the organization time zone.
        /// <param name="dateTime">The Rock date time.</param>
        /// <returns>The <see cref="DateTimeOffset"/> instance that specifies the same point in time.</returns>
        /// </summary>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTimeOffset ToRockDateTimeOffset( DateTime dateTime )
        {
            // We can only apply a time zone offset to an unspecified type.
            var unspecifiedDateTime = DateTime.SpecifyKind( dateTime, DateTimeKind.Unspecified );

            return new DateTimeOffset( unspecifiedDateTime, RockDateTime.OrgTimeZoneInfo.GetUtcOffset( unspecifiedDateTime ) );
        }

        #endregion DateTime Extensions

        #region TimeSpan Extensions

        /// <summary>
        /// Returns a TimeSpan as h:mm AM/PM (culture invariant)
        /// Examples: 1:45 PM, 12:01 AM
        /// </summary>
        /// <param name="timespan">The timespan.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static string ToTimeString( TimeSpan timespan )
        {
            // since the comments on this say HH:MM AM/PM, make sure to return the time in that format
            return RockDateTime.Today.Add( timespan ).ToString( "h:mm tt", System.Globalization.CultureInfo.InvariantCulture );
        }

        #endregion TimeSpan Extensions

        #region Time/Date Rounding 

        /// <summary>
        /// Rounds the specified rounding interval.
        /// from https://stackoverflow.com/a/4108889/1755417
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="roundingInterval">The rounding interval.</param>
        /// <param name="roundingType">Type of the rounding.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static TimeSpan Round( TimeSpan time, TimeSpan roundingInterval, MidpointRounding roundingType )
        {
            return new TimeSpan(
                Convert.ToInt64( Math.Round(
                    time.Ticks / ( decimal ) roundingInterval.Ticks,
                    roundingType
                ) ) * roundingInterval.Ticks
            );
        }

        /// <summary>
        /// Rounds the specified rounding interval.
        /// from https://stackoverflow.com/a/4108889/1755417
        /// </summary>
        /// <example>
        /// new TimeSpan(0, 2, 26).Round( TimeSpan.FromSeconds(5)); // rounds to 00:02:25
        /// new TimeSpan(3, 34, 0).Round( TimeSpan.FromMinutes(30); // round to 03:30
        /// </example>
        /// <param name="time">The time.</param>
        /// <param name="roundingInterval">The rounding interval.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static TimeSpan Round( TimeSpan time, TimeSpan roundingInterval )
        {
            return Round( time, roundingInterval, MidpointRounding.ToEven );
        }

        /// <summary>
        /// Rounds the specified rounding interval.
        /// from https://stackoverflow.com/a/4108889/1755417
        /// </summary>
        /// <example>
        /// new DateTime(2010, 11, 4, 10, 28, 27).Round( TimeSpan.FromMinutes(1) ); // rounds to 2010.11.04 10:28:00
        /// new DateTime(2010, 11, 4, 13, 28, 27).Round( TimeSpan.FromDays(1) ); // rounds to 2010.11.05 00:00
        /// </example>
        /// <param name="datetime">The DateTime.</param>
        /// <param name="roundingInterval">The rounding interval.</param>
        /// <returns></returns>
        [RockObsolete( "1.13" )]
        [Obsolete( "Use the extension methods in the Rock.Common assembly instead." )]
        public static DateTime Round( DateTime datetime, TimeSpan roundingInterval )
        {
            return new DateTime( Round( datetime - DateTime.MinValue, roundingInterval ).Ticks );
        }

        #endregion Time/Date Rounding 
    }
}
