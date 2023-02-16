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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Rock.Tests.Shared
{
    public static class TestConfigurationHelper
    {
        /// <summary>
        /// Sets the RockDateTime timezone to the current system local timezone.
        /// </summary>
        public static void SetRockDateTimeToLocalTimezone()
        {
            RockDateTime.Initialize( TimeZoneInfo.Local );
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that is suitable for testing an operating environment
        /// in which the organization timezone does not match the local system timezone.
        /// This configuration simulates a Rock server hosted in a different timezone to the Rock organization.
        /// </summary>
        public static void SetRockDateTimeToAlternateTimezone()
        {
            var tz = GetTestTimeZoneAlternate();
            RockDateTime.Initialize( tz );
        }

        /// <summary>
        /// Gets a timezone that is different from the local timezone, suitable for testing an operating environment
        /// in which the organization timezone does not match the local system timezone.
        /// </summary>
        public static TimeZoneInfo GetTestTimeZoneAlternate()
        {
            TimeZoneInfo tz;

            // Set to India Standard Time, or an alternative if that is the local timezone in the current environment.
            tz = TimeZoneInfo.FindSystemTimeZoneById( "India Standard Time" );

            Assert.That.IsNotNull( tz, "Timezone 'IST' is not available in this environment." );

            if ( tz.Id == TimeZoneInfo.Local.Id )
            {
                tz = TimeZoneInfo.FindSystemTimeZoneById( "US Mountain Standard Time" );

                Assert.That.IsNotNull( tz, "Timezone 'MST' is not available in this environment." );
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
        public static TimeZoneInfo GetTestTimeZoneDaylightSaving()
        {
            var timezoneDst = TimeZoneInfo.GetSystemTimeZones()
                .FirstOrDefault( x => x.SupportsDaylightSavingTime );

            Assert.That.IsNotNull( timezoneDst, "A timezone configured for Daylight Saving Time (DST) could not be found in this environment." );

            return timezoneDst;
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that is suitable for testing an operating environment
        /// in which the organization timezone supports daylight saving time.
        /// </summary>
        public static void SetRockDateTimeToDaylightSavingTimezone()
        {
            var tz = GetTestTimeZoneDaylightSaving();
            RockDateTime.Initialize( tz );
        }

        /// <summary>
        /// Gets a timezone that is suitable for testing an operating environment
        /// in which the organization timezone does not support daylight saving time.
        /// </summary>
        public static TimeZoneInfo GetTestTimeZoneStandard()
        {
            var timezoneStd = TimeZoneInfo.GetSystemTimeZones()
                .FirstOrDefault( x => !x.SupportsDaylightSavingTime );

            Assert.That.IsNotNull( timezoneStd, "A timezone without Daylight Saving Time (DST) could not be found in this environment." );

            return timezoneStd;
        }

        /// <summary>
        /// Sets the RockDateTime timezone to a value that is suitable for testing an operating environment
        /// in which the organization timezone does not support daylight saving time.
        /// </summary>
        public static void SetRockDateTimeToStandardTimezone()
        {
            var tz = GetTestTimeZoneStandard();
            RockDateTime.Initialize( tz );
        }

    }
}
