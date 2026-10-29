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

using System.Collections.Generic;
using System.Linq;

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Handles preferred group filtering. If any groups are marked as preferred
    /// then the non-preferred groups are removed, but only for the schedules
    /// that the preferred groups are available on. This should happen after all
    /// other normal filtering is applied.
    /// </summary>
    internal class PreferredGroupsOpportunityFilter : OpportunityFilter
    {
        /// <inheritdoc/>
        public override void FilterGroups( OpportunityCollection opportunities )
        {
            var openLocationIds = new HashSet<string>( opportunities.Locations
                .Where( l => !l.IsClosed )
                .Select( l => l.Id ) );

            /*
                10/29/2026 - MSE

                A preferred group only takes priority on the schedules where it
                is actually available. Previously, if any preferred group had an
                open location then every non-preferred group was removed outright,
                which also removed them from schedules the preferred group is not
                offered on and left the person with no options for those services.
                So we now only remove the location and schedule pairs of the
                non-preferred groups that share a schedule with a preferred group,
                the same way DuplicateCheckInOpportunityFilter prunes by schedule.

                Only look for preferred groups that have an open location (per
                schedule). Otherwise, we might end up removing all other groups
                and then a later filter would remove the location because it is
                closed and prevent check-in.

                Reason: Prefer Enrolled Groups must not eliminate groups on
                schedules where the enrolled group is not available (#7067).
            */
            var preferredScheduleIds = new HashSet<string>( opportunities.Groups
                .Where( g => g.IsPreferredGroup )
                .SelectMany( g => g.Locations.Union( g.OverflowLocations ) )
                .Where( l => openLocationIds.Contains( l.LocationId ) )
                .Select( l => l.ScheduleId ) );

            // If there are no preferred groups then there is nothing to remove.
            if ( preferredScheduleIds.Count == 0 )
            {
                return;
            }

            // Remove the non-preferred group locations for the schedules that
            // have a preferred group available.
            foreach ( var group in opportunities.Groups.Where( g => !g.IsPreferredGroup ) )
            {
                group.Locations.RemoveAll( l => preferredScheduleIds.Contains( l.ScheduleId ) );
                group.OverflowLocations.RemoveAll( l => preferredScheduleIds.Contains( l.ScheduleId ) );
            }

            // Remove any non-preferred groups that no longer have anywhere to
            // be checked into.
            opportunities.Groups.RemoveAll( g => !g.IsPreferredGroup
                && g.Locations.Count == 0
                && g.OverflowLocations.Count == 0 );
        }
    }
}
