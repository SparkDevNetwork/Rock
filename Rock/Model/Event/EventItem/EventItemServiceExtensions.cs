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
using System.Linq;

namespace Rock.Model
{
    /// <summary>
    /// Linq filter methods for EventItem queries.
    /// </summary>
    public static class EventItemServiceExtensions
    {
        /// <summary>
        /// Filter to exclude EventItems that are not associated with an active calendar.
        /// </summary>
        /// <returns></returns>
        public static IQueryable<EventItem> HasActiveCalendarItems( this IQueryable<EventItem> eventItems )
        {

            var items = eventItems
                .Where( e => e.EventCalendarItems.Any( c => c.EventCalendar.IsActive ) );

            return items;
        }

        /// <summary>
        /// Filter to exclude EventItems that do not have an occurrence on or after the specified date.
        /// </summary>
        /// <returns></returns>
        public static IQueryable<EventItem> HasOccurrencesOnOrAfterDate( this IQueryable<EventItem> eventItems, DateTime effectiveDate )
        {
            var items = eventItems
                .Where( e => e.EventItemOccurrences.Any( o => o.Schedule.EffectiveEndDate == null
                             || o.Schedule.EffectiveEndDate >= effectiveDate ) );

            return items;
        }

        /// <summary>
        /// Filter to exclude EventItems that do not exist in the specified calendar.
        /// </summary>
        /// <returns></returns>
        public static IQueryable<EventItem> InCalendar( this IQueryable<EventItem> eventItems, int calendarId )
        {
            var items = eventItems
                .Where( e => e.EventCalendarItems.Any( c => c.EventCalendar.Id == calendarId ) );

            return items;
        }
    }
}
