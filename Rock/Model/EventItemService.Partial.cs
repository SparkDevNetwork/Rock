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
    public partial class EventItemService
    {
        /// <summary>
        /// Gets the active calendar items.
        /// </summary>
        /// <returns></returns>
        public IQueryable<EventItem> GetActiveItems()
        {
            // Filter for EventItems that have at least one Occurrence associated with a Schedule having an EffectiveDate now or in the future.
            return Queryable()
                .Where( e => e.IsActive && e.IsApproved )
                .HasActiveCalendarItems()
                .HasOccurrencesOnOrAfterDate( RockDateTime.Now.Date );
        }

        /// <summary>
        /// Gets the active items by calendar identifier.
        /// </summary>
        /// <param name="calendarId">The calendar identifier.</param>
        /// <returns></returns>
        public IQueryable<EventItem> GetActiveItemsByCalendarId( int calendarId )
        {
            return GetActiveItems()
                .InCalendar( calendarId );
        }

        /// <summary>
        /// Gets the indexable active items.
        /// </summary>
        /// <returns></returns>
        public IQueryable<EventItem> GetIndexableActiveItems()
        {
            return this.GetActiveItems()
                        .Where( e => e.EventCalendarItems.Any( c =>
                                        c.EventCalendar.IsActive
                                        && c.EventCalendar.IsIndexEnabled ) );
        }
    }

    #region Extension Methods

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

    #endregion 
}