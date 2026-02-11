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

using Rock.Enums.Engagement;

namespace Rock.Model
{
    /// <summary>
    /// Data access/service class for <see cref="Rock.Model.ContactTouchpoint"/> entity objects.
    /// </summary>
    public partial class ContactTouchpointService
    {
        /// <summary>
        /// Determines if the last contact date is within the recent touchpoint
        /// period based on the specified cadence.
        /// </summary>
        /// <param name="cadence">The cadence of a touchpoint.</param>
        /// <param name="lastContactDate">The last contact date for the touchpoint.</param>
        /// <param name="referenceDate">The date to use when comparing the <paramref name="lastContactDate"/>. This is usually <c>RockDateTime.Now</c>.</param>
        /// <returns><c>true</c> if the <paramref name="lastContactDate"/> is within the expected <paramref name="cadence"/>.</returns>
        public static bool HasRecentTouchpoint( OutreachCadence cadence, DateTime lastContactDate, DateTime referenceDate )
        {
            var daysSinceLastContact = ( referenceDate - lastContactDate ).TotalDays;

            switch ( cadence )
            {
                case OutreachCadence.Daily:
                    return daysSinceLastContact < 1;

                case OutreachCadence.Weekly:
                    return daysSinceLastContact < 7;

                case OutreachCadence.EveryOtherWeek:
                    return daysSinceLastContact < 14;

                case OutreachCadence.Monthly:
                    return daysSinceLastContact < 30;

                case OutreachCadence.EveryOtherMonth:
                    return daysSinceLastContact < 60;

                case OutreachCadence.Quarterly:
                    return daysSinceLastContact < 90;

                default:
                    return true;
            }
        }

        /// <summary>
        /// Calculates the average number of touchpoints per enabled day
        /// across all contacts for a specified touchpoint type.
        /// </summary>
        /// <param name="contacts">The set of contacts for which to calculate the daily touchpoint count.</param>
        /// <param name="touchpointType">The type of touchpoint to consider when calculating the cadence for each contact.</param>
        /// <param name="enabledDaysOfWeek">The number of days per week that are enabled for scheduling touchpoints. Must be between 0 and 7.</param>
        /// <returns>The average number of touchpoints per enabled day, aggregated across all contacts.</returns>
        public static double GetDailyTouchpointCount( IEnumerable<Contact> contacts, TouchpointType touchpointType, int enabledDaysOfWeek )
        {
            var count = 0;

            if ( enabledDaysOfWeek <= 0 )
            {
                return 0;
            }

            foreach ( var contact in contacts )
            {
                var cadence = OutreachCadence.Paused;

                if ( touchpointType == TouchpointType.Prayer )
                {
                    cadence = contact.PrayerCadence;
                }
                else if ( touchpointType == TouchpointType.Connection )
                {
                    cadence = contact.ConnectionCadence;
                }

                switch ( cadence )
                {
                    case OutreachCadence.Daily:
                        count += 52 * enabledDaysOfWeek;
                        break;

                    case OutreachCadence.Weekly:
                        count += 52;
                        break;

                    case OutreachCadence.EveryOtherWeek:
                        count += 26;
                        break;

                    case OutreachCadence.Monthly:
                        count += 12;
                        break;

                    case OutreachCadence.EveryOtherMonth:
                        count += 6;
                        break;

                    case OutreachCadence.Quarterly:
                        count += 4;
                        break;
                }
            }

            var daysInYear = 52 * enabledDaysOfWeek;

            return count / ( double ) daysInYear;
        }
    }
}
