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
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;

namespace Rock.Tests.Shared
{
    /// <summary>
    /// A helper class for testing components related to date/time.
    /// </summary>
    /// <remarks>This class will be moved to Rock.Tests.Shared in the future.</remarks>
    public class DateTimeTestHelper
    {
        /// <summary>
        /// Sets the RockDateTime timezone.
        /// </summary>
        public static void SetRockTimeZone( TimeZoneInfo tz )
        {
            RockDateTime.Initialize( tz );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to the current system local timezone.
        /// </summary>
        public static void SetRockDateTimeToLocalTimezone()
        {
            SetRockTimeZone( TimeZoneInfo.Local );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a region that is ahead of UTC time (UTC+HH:MM).
        /// This configuration simulates a Rock server hosted in a different timezone to the Rock organization.
        /// </summary>
        public static void SetRockDateTimeToUtcPositiveTimezone()
        {
            var tz = GetTestTimeZoneForUtcPositive();
            SetRockTimeZone( tz );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that behind UTC time (UTC-HH:MM).
        /// This configuration simulates a Rock server hosted in a different timezone to the Rock organization.
        /// </summary>
        public static void SetRockDateTimeToUtcNegativeTimezone()
        {
            var tz = GetTestTimeZoneForUtcNegative();
            SetRockTimeZone( tz );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that is suitable for testing an operating environment
        /// in which the organization timezone supports daylight saving time.
        /// </summary>
        public static void SetRockDateTimeToDaylightSavingTimezone()
        {
            var tz = GetTestTimeZoneForDaylightSaving();
            SetRockTimeZone( tz );
        }

        /// <summary>
        /// Gets a timezone that is different from the local timezone, suitable for testing an operating environment
        /// in which the organization timezone does not match the local system timezone.
        /// </summary>
        public static TimeZoneInfo GetTestTimeZoneForUtcPositive()
        {
            TimeZoneInfo tz;

            // Set to India Standard Time (UTC+05:30), or an alternative if that is the local timezone in the current environment.
            tz = TimeZoneInfo.FindSystemTimeZoneById( "India Standard Time" );

            Assert.That.IsNotNull( tz, "Timezone 'IST' is not available in this environment." );

            if ( tz.Id == TimeZoneInfo.Local.Id )
            {
                // Set to Tokyo Standard Time aka Japan Standard Time (UTC+09:00)
                try
                {
                    tz = TimeZoneInfo.FindSystemTimeZoneById( "Tokyo Standard Time" );
                }
                catch( TimeZoneNotFoundException )
                {
                    tz = TimeZoneInfo.FindSystemTimeZoneById( "Japan Standard Time" );
                }

                Assert.That.IsNotNull( tz, "Timezone 'Tokyo Standard Time' or 'Japan Standard Time' is not available in this environment." );
            }

            // To simplify the process of testing date/time differences, we need to ensure that the selected timezone is not subject to Daylight Saving Time.
            // If a DST-affected timezone is used, some tests will fail when executed across DST boundary dates.
            Assert.That.IsFalse( tz.SupportsDaylightSavingTime, "Test Timezone should not be configured for Daylight Saving Time (DST)." );

            return tz;
        }

        /// <summary>
        /// Gets a timezone that is different from the local timezone, suitable for testing an operating environment
        /// in which the organization timezone does not match the local system timezone.
        /// </summary>
        public static TimeZoneInfo GetTestTimeZoneForUtcNegative()
        {
            TimeZoneInfo tz;

            // Set to UCT-07:00, or an alternative if that is the local timezone in the current environment.
            tz = TimeZoneInfo.FindSystemTimeZoneById( "US Mountain Standard Time" );

            Assert.That.IsNotNull( tz, "Timezone 'MST' is not available in this environment." );

            if ( tz.Id == TimeZoneInfo.Local.Id )
            {
                // Set to UCT-07:00.
                tz = TimeZoneInfo.FindSystemTimeZoneById( "Hawaiian Standard Time" );
                Assert.That.IsNotNull( tz, "Timezone 'Hawaiian Standard Time' is not available in this environment." );
            }

            // To simplify the process of testing date/time differences, we need to ensure that the selected timezone is not subject to Daylight Saving Time.
            // If a DST-affected timezone is used, some tests will fail when executed across DST boundary dates.
            Assert.That.IsFalse( tz.SupportsDaylightSavingTime, "Test Timezone should not be configured for Daylight Saving Time (DST)." );

            return tz;
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that is suitable for testing an operating environment
        /// in which the organization timezone supports daylight saving time.
        /// </summary>
        public static TimeZoneInfo GetTestTimeZoneForDaylightSaving()
        {
            // Set to Central Standard Time (CST), a timezone that supports Daylight Saving Time (DST).
            var tz = TimeZoneInfo.FindSystemTimeZoneById( "Central Standard Time" );

            Assert.That.IsNotNull( tz, "Timezone 'CST' is not available in this environment." );

            Assert.That.IsTrue( tz.SupportsDaylightSavingTime, "Test Timezone should be configured for Daylight Saving Time (DST)." );

            return tz;
        }

        /// <summary>
        /// For each of the known test timezones, process the specified action.
        /// </summary>
        /// <param name="testMethod"></param>
        /// <remarks>
        /// Operations involving calendar events are sensitive to timezone conversions.
        /// In Rock, we need to be concerned with the distinction between the local server/system time and Rock time,
        /// and the effects of daylight saving time (DST) in some regions.
        /// For this reason, calendar operations need to be tested for timezones with these characteristics:
        /// * positive UTC offset
        /// * negative UTC offset
        /// * daylight saving time.
        /// </remarks>
        public static void ExecuteForTimeZones( Action<TimeZoneInfo> testMethod )
        {
            // Test the local timezone first, because if the test is broken for all timezones
            // it will be the simplest to fix.
            var timeZones = new List<TimeZoneInfo>
            {
                RockDateTime.OrgTimeZoneInfo,
                GetTestTimeZoneForUtcNegative(),
                GetTestTimeZoneForUtcPositive(),
                GetTestTimeZoneForDaylightSaving()
            };

            ExecuteForTimeZones( testMethod, timeZones );
        }

        /// <summary>
        /// For each of the specified timezones, process the specified action.
        /// </summary>
        /// <param name="testMethod"></param>
        /// <param name="timeZones"></param>
        public static void ExecuteForTimeZones( Action<TimeZoneInfo> testMethod, List<TimeZoneInfo> timeZones )
        {
            // Get the current timezone.
            var tzCurrent = RockDateTime.OrgTimeZoneInfo;

            try
            {
                // Execute the test action for each of the test timezones.
                foreach ( var timeZone in timeZones )
                {
                    Debug.WriteLine( $"**\n** Executing Test for Timezone \"{timeZone.DisplayName}\"...\n**\n" );
                    SetRockTimeZone( timeZone );

                    try
                    {
                        testMethod( timeZone );
                    }
                    catch ( Exception ex )
                    {
                        if ( timeZone.Id != tzCurrent.Id )
                        {
                            // Add timezone details to the exception.
                            ex = new Exception( $"Test failed for alternate timezone \"{timeZone.DisplayName}\". See inner exception for details.", ex );
                        }
                        throw ex;
                    }
                }
            }
            finally
            {
                // Restore the original timezone for subsequent tests.
                SetRockTimeZone( tzCurrent );
            }
        }

        public void AssertDateIsUtc( DateTime? dateTime )
        {
            // Verify that we have an expected output date in UTC, to avoid any possible ambiguity between Rock time and Local time.
            if ( dateTime == null || dateTime.Value.Kind != DateTimeKind.Utc )
            {
                throw new Exception( "Expected DateTime must be expressed in UTC." );
            }
        }

        /// <summary>
        /// Write the results of template rendering to the debug output, with some additional configuration details.
        /// Useful to document the result of a test that would otherwise produce no output.
        /// </summary>
        /// <param name="outputString"></param>
        public void DebugWriteRenderResult( ILavaEngine engine, string inputString, string outputString )
        {
            Debug.Print( $"\n** [{engine.EngineName}] Input:\n{inputString}" );
            Debug.Print( $"\n** [{engine.EngineName}] Output:\n{outputString}" );
        }

        /// <summary>
        /// Get the current Rock date and time as an Unspecified DateTime type.
        /// </summary>
        /// <returns></returns>
        public static DateTime GetRockNowDateTimeAsUnspecifiedKind()
        {
            var now = DateTime.SpecifyKind( RockDateTime.Now, DateTimeKind.Unspecified );
            return now;
        }
    }
}
