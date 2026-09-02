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
using System.Linq;

using Rock.Data;
using Rock.Net;
using Rock.Web.Cache;

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

        /// <summary>
        /// Filters out occurrences whose event item is tagged for personalization segments
        /// or request filters that the current request does not match. Event items with no
        /// tags of an enabled type are always included.
        /// </summary>
        /// <returns>The filtered occurrences.</returns>
        public static IQueryable<EventItemOccurrence> FilterByPersonalization( this IQueryable<EventItemOccurrence> occurrences, RockContext rockContext, bool filterByPersonalizationSegments, bool filterByRequestFilters, IEnumerable<int> matchedSegmentIds, IEnumerable<int> matchedRequestFilterIds )
        {
            if ( rockContext == null )
            {
                return occurrences;
            }

            if ( filterByPersonalizationSegments )
            {
                occurrences = FilterByPersonalizationType( occurrences, rockContext, PersonalizationType.Segment, matchedSegmentIds );
            }

            if ( filterByRequestFilters )
            {
                occurrences = FilterByPersonalizationType( occurrences, rockContext, PersonalizationType.RequestFilter, matchedRequestFilterIds );
            }

            return occurrences;
        }

        /// <summary>
        /// Filters out occurrences whose event item is tagged for personalization segments
        /// or request filters that the specified request does not match. Event items with no
        /// tags of an enabled type are always included.
        /// </summary>
        /// <returns>The filtered occurrences.</returns>
        public static IQueryable<EventItemOccurrence> FilterByPersonalization( this IQueryable<EventItemOccurrence> occurrences, RockContext rockContext, bool filterByPersonalizationSegments, bool filterByRequestFilters, RockRequestContext requestContext )
        {
            if ( requestContext == null )
            {
                return occurrences;
            }

            return occurrences.FilterByPersonalization(
                rockContext,
                filterByPersonalizationSegments,
                filterByRequestFilters,
                requestContext.PersonalizationSegmentIds,
                requestContext.PersonalizationRequestFilterIds );
        }

        /// <summary>
        /// Filters out occurrences whose event item is tagged for the specified personalization
        /// type but matches none of the supplied identifiers.
        /// </summary>
        /// <returns>The filtered occurrences.</returns>
        private static IQueryable<EventItemOccurrence> FilterByPersonalizationType( IQueryable<EventItemOccurrence> occurrences, RockContext rockContext, PersonalizationType personalizationType, IEnumerable<int> matchedIds )
        {
            var entityTypeId = EntityTypeCache.Get<EventItem>().Id;
            var matchedIdList = matchedIds?.ToList() ?? new List<int>();

            var taggedEventItemIdQry = rockContext.Set<PersonalizedEntity>()
                .Where( pe => pe.EntityTypeId == entityTypeId
                    && pe.PersonalizationType == personalizationType )
                .Select( pe => pe.EntityId );

            var matchedEventItemIdQry = rockContext.Set<PersonalizedEntity>()
                .Where( pe => pe.EntityTypeId == entityTypeId
                    && pe.PersonalizationType == personalizationType
                    && matchedIdList.Contains( pe.PersonalizationEntityId ) )
                .Select( pe => pe.EntityId );

            // An untagged event item is always visible because tagging narrows an item's audience rather than widening it.
            return occurrences.Where( o => !taggedEventItemIdQry.Contains( o.EventItemId )
                || matchedEventItemIdQry.Contains( o.EventItemId ) );
        }
    }
}
