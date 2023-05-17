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

namespace Rock.Lava
{
    /// <summary>
    /// Provides functions to convert dates and times between UTC, Rock Organization Time, and Local System Time
    /// for the Lava library. This is necessary because the system may be hosted in a different timezone
    /// than the Rock organization.
    /// </summary>
    /// <remarks>
    /// Use DateTimeOffset or UTC DateTime types to store dates and avoid potential confusion between timezones.
    /// However, it is important that these date manipulation functions preserve or adjust for the timezone offset of input parameters,
    /// because they may represent client dates and times that do not correspond to the Rock organization timezone setting.
    /// </remarks>
    public class LavaDateTime
    {
        #region Factory methods

        /// <summary>
        /// Gets the current Rock date/time as a DateTimeOffset based on the configured OrgTimeZone setting.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTimeOffset"/> that represents the current Rock date/time based on the Organization's TimeZone.
        /// </value>
        public static DateTimeOffset NowOffset
        {
            get
            {
                return TimeZoneInfo.ConvertTime( DateTimeOffset.UtcNow, RockDateTime.OrgTimeZoneInfo );
            }
        }

        /// <summary>
        /// Gets the current Rock date/time based on the configured OrgTimeZone setting.
        /// The Kind is set to Unspecified to indicate that it is neither local system or UTC time.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTimeOffset"/> that represents the current Rock date/time based on the Organization's TimeZone.
        /// </value>
        public static DateTime NowDateTime
        {
            get
            {
                var dateTime = RockDateTime.Now;

                dateTime = DateTime.SpecifyKind( dateTime, DateTimeKind.Unspecified );

                return dateTime;
            }
        }

        /// <summary>
        /// Creates a new datetime for the Rock timezone, expressed in either UTC or as an Unspecified timezone.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static DateTime NewUtcDateTime( int year, int month, int day, int hour, int minute, int second, DateTimeKind kind = DateTimeKind.Utc )
        {
            var dto = NewDateTimeOffset( year, month, day, hour, minute, second );

            if ( kind == DateTimeKind.Utc )
            {
                return dto.UtcDateTime;
            }
            else
            {
                // Return the DateTime value for the Rock timezone as an Unspecified type to avoid any confusion with the Local timezone,
                // because the local system may be operating in a different zone to the Rock organization.
                var dateTime = dto.DateTime;

                dateTime = DateTime.SpecifyKind( dateTime, DateTimeKind.Unspecified );

                return dateTime;
            }
        }

        /// <summary>
        /// Creates a new datetime for the Rock timezone.
        /// The Kind property is set to Unspecified to indicate the Rock application timezone setting.
        /// This is important because it may differ from a Local time if the system is operating in a different timezone to the Rock organization.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <param name="kind"></param>
        /// <returns></returns>
        public static DateTime NewDateTime( int year, int month, int day, int hour, int minute, int second )
        {
            var dto = NewDateTimeOffset( year, month, day, hour, minute, second );

            var dateTime = dto.DateTime;
            dateTime = DateTime.SpecifyKind( dateTime, DateTimeKind.Unspecified );

            return dateTime;
        }

        /// <summary>
        /// Creates a new datetime for the Rock timezone.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="hour"></param>
        /// <param name="minute"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static DateTimeOffset NewDateTimeOffset( int year, int month, int day, int hour, int minute, int second )
        {
            return NewDateTimeOffset( year, month, day, hour, minute, second, null );
        }

        /// <summary>
        /// Creates a new datetime for the Rock timezone.
        /// </summary>
        /// <param name="ticks"></param>
        /// <returns></returns>
        public static DateTimeOffset NewDateTimeOffset( long ticks )
        {
            var dto = new DateTimeOffset( ticks, RockDateTime.OrgTimeZoneInfo.GetUtcOffset( new DateTime( ticks ) ) );

            return dto;
        }

        private static DateTimeOffset NewDateTimeOffset( int year, int month, int day, int hour, int minute, int second, TimeSpan? offset )
        {
            // Get the correct offset for the timezone, which will be date dependent if the timezone supports Daylight Saving Time (DST).
            if ( offset == null )
            {
                var dateTime = new DateTime( year, month, day, hour, minute, second, DateTimeKind.Unspecified );
                offset = RockDateTime.OrgTimeZoneInfo.GetUtcOffset( dateTime );
            }

            // Automatically correct leap-year dates if they are invalid.
            DateTimeOffset dateTimeOffset;
            if ( !DateTime.IsLeapYear( year ) && month == 2 && day == 29 )
            {
                dateTimeOffset = new DateTimeOffset( year, 2, 28, hour, minute, second, offset.Value );
            }
            else
            {
                dateTimeOffset = new DateTimeOffset( year, month, day, hour, minute, second, offset.Value );
            }

            return dateTimeOffset;
        }

        #endregion

        #region Conversion functions

        /// <summary>
        /// Convert a DateTime value to a DateTimeOffset, according to the type of DateTime input.
        /// </summary>
        /// <param name="dateTime">The date/time value.</param>
        /// <returns></returns>
        public static DateTimeOffset ConvertToDateTimeOffset( DateTime dateTime )
        {
            DateTimeOffset dateTimeOffset;
            if ( dateTime.Kind == DateTimeKind.Utc )
            {
                // Convert UTC time to Rock time.
                dateTimeOffset = new DateTimeOffset( dateTime, TimeSpan.Zero );
                dateTimeOffset = TimeZoneInfo.ConvertTime( dateTimeOffset, RockDateTime.OrgTimeZoneInfo );
            }
            else if ( dateTime.Kind == DateTimeKind.Local )
            {
                // Convert local system time to Rock time.
                var rockDateTime = TimeZoneInfo.ConvertTime( dateTime, RockDateTime.OrgTimeZoneInfo );
                dateTimeOffset = new DateTimeOffset( rockDateTime, RockDateTime.OrgTimeZoneInfo.GetUtcOffset( rockDateTime ) );
            }
            else
            {
                // Assume the value is specified in Rock time.
                dateTimeOffset = new DateTimeOffset( dateTime, RockDateTime.OrgTimeZoneInfo.GetUtcOffset( dateTime ) );
            }

            return dateTimeOffset;
        }

        /// <summary>
        /// Converts a date/time expressed in UTC or system Local time to the equivalent Rock date/time.
        /// </summary>
        /// <param name="dateTime">The date/time value.</param>
        /// <returns></returns>
        public static DateTime ConvertToRockDateTime( DateTime dateTime )
        {
            DateTime rockDateTime;

            if ( dateTime.Kind == DateTimeKind.Utc || dateTime.Kind == DateTimeKind.Local )
            {
                rockDateTime = TimeZoneInfo.ConvertTime( dateTime, RockDateTime.OrgTimeZoneInfo );

                // Return the Rock datetime with a kind of Unspecified, because it does not necessarily correspond to local system time.
                rockDateTime = DateTime.SpecifyKind( rockDateTime, DateTimeKind.Unspecified );
            }
            else
            {
                // Assume an Unspecified Date Kind refers to the Rock timezone.
                rockDateTime = dateTime;
            }

            return rockDateTime;
        }

        /// <summary>
        /// Converts a date/time offset to a Rock date/time value.
        /// </summary>
        /// <param name="offset">The date/time offset.</param>
        /// <returns></returns>
        public static DateTime ConvertToRockDateTime( DateTimeOffset offset )
        {
            var rockDateTime = TimeZoneInfo.ConvertTime( offset, RockDateTime.OrgTimeZoneInfo ).DateTime;

            // Return the Rock datetime with a kind of Unspecified, because it does not necessarily correspond to local system time.
            rockDateTime = DateTime.SpecifyKind( rockDateTime, DateTimeKind.Unspecified );

            return rockDateTime;
        }

        /// <summary>
        /// Converts a date/time offset to a Rock date/time offset value.
        /// </summary>
        /// <param name="value">The date/time offset.</param>
        /// <returns></returns>
        public static DateTimeOffset ConvertToRockOffset( DateTimeOffset value )
        {
            return TimeZoneInfo.ConvertTime( value, RockDateTime.OrgTimeZoneInfo );
        }

        /// <summary>
        /// Converts a value to a date/time offset in the Rock timezone.
        /// If the Date Kind is Local or Unspecified, the date value is assumed to be for the Rock timezone.
        /// </summary>
        /// <param name="dateTime">The Rock date/time.</param>
        /// <returns></returns>
        public static DateTimeOffset ConvertToRockOffset( DateTime dateTime )
        {
            if ( dateTime.Kind == DateTimeKind.Utc || dateTime.Kind == DateTimeKind.Local )
            {
                // Convert a UTC or Local datetime to the Rock timezone.
                dateTime = ConvertToRockDateTime( dateTime );

                return new DateTimeOffset( dateTime, RockDateTime.OrgTimeZoneInfo.GetUtcOffset( dateTime ) );
            }
            else
            {
                // Assume an Unspecified Date Kind refers to the Rock timezone.
                return new DateTimeOffset( dateTime, RockDateTime.OrgTimeZoneInfo.GetUtcOffset( dateTime ) );
            }
        }

        /// <summary>
        /// Creates a new datetime offset value for the specified date, discarding the time portion but preserving the timezone offset of the input value.
        /// </summary>
        /// <param name="value">The date/time offset.</param>
        /// <returns></returns>
        public static DateTimeOffset ConvertToDateOnlyOffset( DateTimeOffset value )
        {
            return NewDateTimeOffset( value.Year, value.Month, value.Day, 0, 0, 0, value.Offset );
        }

        #endregion

        #region Parse functions

        /// <summary>
        /// Parse an input date string to a UTC date. If the timezone is not specified, the Rock organization timezone is assumed.
        /// </summary>
        /// <param name="input">A string of text to be parsed.</param>
        /// <param name="defaultValue">The value returned if the input string does not represent a valid date/time.</param>
        /// <returns></returns>
        public static DateTime? ParseToUtc( string input, DateTime? defaultValue = null )
        {
            var dtoParsed = ParseToOffset( input, defaultValue );
            if ( dtoParsed == null )
            {
                return defaultValue;
            }

            return dtoParsed.Value.UtcDateTime;
        }

        /// <summary>
        /// Parse an input date string to a DateTimeOffset. If the timezone is not specified, the Rock organization time zone is assumed.
        /// </summary>
        /// <param name="input">A string representing a valid date/time.</param>
        /// <param name="defaultValue">The value returned if the input string does not represent a valid date/time.</param>
        /// <returns></returns>
        public static DateTimeOffset? ParseToOffset( string input, DateTimeOffset? defaultValue = null )
        {
            var rockTimeZone = RockDateTime.OrgTimeZoneInfo;

            // Try to parse a datetime offset from the input string.
            DateTimeOffset dto;
            bool isParsed;

            var stringValue = input;

            // First, try to parse the datetime string by appending the default Rock timezone offset.
            // There is no simple method of detecting if a valid datetime string includes timezone information,
            // but we can be sure the parse attempt will fail if multiple offsets are specified.
            var nowRockTime = TimeZoneInfo.ConvertTime( DateTimeOffset.UtcNow, rockTimeZone );

            isParsed = DateTimeOffset.TryParse( stringValue + " " + nowRockTime.ToString( "zzz" ), out dto );

            if ( isParsed )
            {
                // If the input string parsed correctly with the default timezone, check if it should be adjusted for DST.
                if ( rockTimeZone.SupportsDaylightSavingTime )
                {
                    var utcOffset = rockTimeZone.GetUtcOffset( dto );
                    var dstOffsetString = ( utcOffset.Hours < 0 ? "-" : "+" ) + ( utcOffset.Hours > 9 ? "" : "0" ) + Math.Abs( utcOffset.Hours ) + ":" + ( utcOffset.Minutes > 9 ? "" : "0" ) + Math.Abs( utcOffset.Minutes );

                    isParsed = DateTimeOffset.TryParse( stringValue + " " + dstOffsetString, out dto );
                }
            }
            else
            {
                // Parsing with the additional timezone information failed, so assume that the input string is either invalid
                // or already specifies a timezone.
                isParsed = DateTimeOffset.TryParse( stringValue, out dto );
            }

            if ( isParsed )
            {
                return dto;
            }

            return defaultValue;
        }

        #endregion

        #region Render methods

        /// <summary>
        /// Render a DateTime value in the specified format.
        /// If the value is specified as UTC, it will be converted to the Rock timezone.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="formatString"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static string ToString( DateTime? value, string formatString = null, CultureInfo cultureInfo = null )
        {
            if ( value == null )
            {
                return string.Empty;
            }

            var rockDt = value.Value;

            if ( rockDt.Kind == DateTimeKind.Utc )
            {
                // Convert the UTC Date to the Rock Organization timezone.
                rockDt = TimeZoneInfo.ConvertTimeFromUtc( rockDt, RockDateTime.OrgTimeZoneInfo );
            }

            var dateString = rockDt.ToString( formatString ?? "G", cultureInfo );

            return dateString;
        }

        /// <summary>
        /// Render a DateTimeOffset value in the specified format.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="formatString"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static string ToString( DateTimeOffset? value, string formatString = null, CultureInfo cultureInfo = null )
        {
            if ( value == null )
            {
                return string.Empty;
            }

            var dateString = value.Value.ToString( formatString ?? "G", cultureInfo );

            return dateString;
        }

        #endregion
    }
}
