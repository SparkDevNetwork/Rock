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
using System.Threading;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( "Rock.Lava.Tests" )]
namespace Rock.Common
{
    /// <summary>
    /// Special Class that returns current Current DateTime based on the TimeZone set in Web.Config
    /// This is done because the system may be hosted in a different timezone than the organization
    /// Rock developers should use RockDateTime.Now instead of DateTime.Now
    /// </summary>
    public class RockDateTime
    {
#if NETCORE
        /// <summary>
        /// Gets the time zone information for this execution context.
        /// </summary>
        /// <value>
        /// A <see cref="System.TimeZoneInfo"/> that represents the organization's time zone.
        /// </value>
        public static TimeZoneInfo OrgTimeZoneInfo
        {
            get => _orgTimeZoneInfo.Value ?? DefaultTimeZoneInfo;
            internal set => _orgTimeZoneInfo.Value = value;
        }

        /// <summary>
        /// The time zone information for this execution context. This is stored as an
        /// AsyncLocal so that unit tests can change this value to verify proper operation
        /// without conflicting with other tests. Using AsyncLocal is thread-safe and
        /// async-safe. Retrieving the value from an AsyncLocal takes an extra 0.001ms over
        /// an unwrapped TimeZoneInfo property.
        /// </summary>
        private static readonly AsyncLocal<TimeZoneInfo> _orgTimeZoneInfo = new AsyncLocal<TimeZoneInfo>();
#else

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
#endif
        /// <summary>
        /// Gets or sets the default time zone information.
        /// </summary>
        /// <value>
        /// The default time zone information.
        /// </value>
        private static TimeZoneInfo DefaultTimeZoneInfo { get; set; } = TimeZoneInfo.Local;

        public static void Initialize( string timeZoneSetting )
        {
            if ( string.IsNullOrWhiteSpace( timeZoneSetting ) )
            {
                DefaultTimeZoneInfo = TimeZoneInfo.Local;
            }
            else
            {
                // if Web.Config has the OrgTimeZone set to the special "Local" (intended for Developer Mode), just use the Local DateTime. However, a production install of Rock will always have a real Time Zone string
                if ( timeZoneSetting.Equals( "Local", StringComparison.OrdinalIgnoreCase ) )
                {
                    DefaultTimeZoneInfo = TimeZoneInfo.Local;
                }
                else
                {
                    DefaultTimeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById( timeZoneSetting );
                }
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

            return ( DateTime? ) null;
        }
    }
}
