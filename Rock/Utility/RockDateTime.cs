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
using System.Configuration;

using Rock.Web.Cache;

namespace Rock
{
    /// <summary>
    /// Special Class that returns current Current DateTime based on the TimeZone set in Web.Config
    /// This is done because the system may be hosted in a different timezone than the organization
    /// Rock developers should use RockDateTime.Now instead of DateTime.Now
    /// </summary>
    public class RockDateTime
    {

        private static TimeZoneInfo _orgTimeZoneInfo = null;
        /// <summary>
        /// Gets the time zone information from the OrgTimeZone setting set in web.config.
        /// </summary>
        /// <value>
        /// A <see cref="System.TimeZoneInfo"/> that represents the organization's time zone.
        /// </value>
        public static TimeZoneInfo OrgTimeZoneInfo
        {
            get
            {
                if ( _orgTimeZoneInfo == null )
                {
                    string orgTimeZoneSetting = ConfigurationManager.AppSettings["OrgTimeZone"];

                    if ( string.IsNullOrWhiteSpace( orgTimeZoneSetting ) )
                    {
                        _orgTimeZoneInfo = TimeZoneInfo.Local;
                    }
                    else
                    {
                        // if Web.Config has the OrgTimeZone set to the special "Local" (intended for Developer Mode), just use the Local DateTime. However, a production install of Rock will always have a real Time Zone string
                        if ( orgTimeZoneSetting.Equals( "Local", StringComparison.OrdinalIgnoreCase ) )
                        {
                            _orgTimeZoneInfo = TimeZoneInfo.Local;
                        }
                        else
                        {
                            _orgTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById( orgTimeZoneSetting );
                        }
                    }
                }
                return _orgTimeZoneInfo;
            }
        }

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
        /// Gets the first day of week which is Monday within Rock
        /// </summary>
        /// <value>
        /// The first day of week.
        /// </value>
        public static DayOfWeek FirstDayOfWeek
        {
            get
            {
                return DayOfWeek.Monday;
            }
        }

        /// <summary>
        /// Creates a new datetime based on year, month, day, and handles 2/29 for non leap years (returns 2/28 in this case)
        /// </summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <returns></returns>
        public static DateTime? New( int year, int month, int day )
        {
            try
            {
                if ( !DateTime.IsLeapYear( year ) && month == 2 && day == 29 )
                {
                    return new DateTime( year, 2, 28 );
                }
                return new DateTime( year, month, day );
            }
            catch { }

            return (DateTime?)null;
        }

        /// <summary>
        /// Gets the current graduation date base on the GradeTransitionDate GlobalAttribute and current datetime.
        /// For example, if the Grade Transition Date is June 1st and the current date is June 1st or earlier, it will return June 1st of the current year;
        /// otherwise, it will return June 1st of next year
        /// </summary>
        /// <value>
        /// The current graduation date.
        /// </value>
        public static DateTime CurrentGraduationDate
        {
            get
            {
                var graduationDateWithCurrentYear = GlobalAttributesCache.Get().GetValue( "GradeTransitionDate" ).MonthDayStringAsDateTime() ?? new DateTime( RockDateTime.Today.Year, 6, 1 );
                if ( graduationDateWithCurrentYear < RockDateTime.Today )
                {
                    // if the graduation date already occurred this year, return next year' graduation date
                    return graduationDateWithCurrentYear.AddYears( 1 );
                }
                else
                {
                    return graduationDateWithCurrentYear;
                }
            }
        }

        /// <summary>
        /// Gets the current graduation year based on <see cref="CurrentGraduationDate"/>
        /// </summary>
        /// <value>
        /// The current graduation year.
        /// </value>
        public static int CurrentGraduationYear => CurrentGraduationDate.Year;
    }
}
