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

using System.Linq;

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Handles preferred group filtering. If any groups are marked as preferred
    /// then all non-preferred groups are removed. This should happen after all
    /// other normal filtering is applied.
    /// </summary>
    internal class PreferredGroupsOpportunityFilter : OpportunityFilter
    {
        /// <inheritdoc/>
        public override void FilterGroups( OpportunityCollection opportunities )
        {
            var openLocations = opportunities.Locations
                .Where( l => !l.IsClosed )
                .Select( l => l.Id )
                .ToList();

            // Only look for preferred groups if there are any open locations.
            // Otherwise, we might end up removing all other groups and then
            // a later filter would remove the location because it is closed
            // and prevent check-in.
            var preferredGroups = opportunities.Groups
                .Where( g => g.IsPreferredGroup == true
                    && g.Locations.Union( g.OverflowLocations ).Any( l => openLocations.Contains( l.LocationId ) ) )
                .ToList();

            // If we have any preferred groups then we need to remove all
            // non-preferred grous.
            if ( preferredGroups.Count > 0 )
            {
                opportunities.Groups.RemoveAll( g => !g.IsPreferredGroup );
            }
        }
    }
}
