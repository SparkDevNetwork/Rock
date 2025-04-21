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

using Rock.ViewModels.CheckIn;

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on location overflow
    /// status. This should be the last location filter executed.
    /// </summary>
    internal class LocationOverflowOpportunityFilter : OpportunityFilter
    {
        /*
         * Overflow locations will only be used if there are NO other
         * non-overflow locations available, across all groups.
         * 
         * Meaning, suppose Noah matches group "Kindergarten" with location
         * 101; and also matches group "5yr olds" with location 201 and
         * overflow location 202.
         * 
         * If location 201 is full, but location 101 is not full, then the
         * overflow of 202 will NOT be used.
         * 
         * If location 201 AND location 101 are both full, then the overflow
         * of 202 will be used.
         * 
         * If location 201 AND location 101 AND locaiton 202 are all full,
         * then no locations will be available for check-in.
         */

        #region Methods

        /// <inheritdoc/>
        public override void FilterLocations( OpportunityCollection opportunities )
        {
            // Groups have already been filtered out at this point.
            var normalLocationIds = opportunities.Groups.SelectMany( g => g.Locations.Select( l => l.LocationId ) ).ToList();
            var overflowLocationIds = opportunities.Groups.SelectMany( g => g.OverflowLocations.Select( l => l.LocationId ) ).ToList();

            // If we have no overflow locations defined then we don't
            // need to do anything.
            if ( !overflowLocationIds.Any() )
            {
                return;
            }

            var hasNormalLocations = opportunities.Locations.Any( l => normalLocationIds.Contains( l.Id ) );

            if ( hasNormalLocations )
            {
                // We have normal locations that are still valid so we need to
                // remove any locations that are only overflow.
                var overflowOnlyLocationIds = overflowLocationIds.Except( normalLocationIds );

                opportunities.Locations.RemoveAll( l => overflowOnlyLocationIds.Contains( l.Id ) );

                foreach ( var group in opportunities.Groups )
                {
                    if ( group.OverflowLocations.Count > 0 )
                    {
                        group.OverflowLocations = new List<LocationAndScheduleBag>();
                    }
                }
            }
            else
            {
                // We don't have normal locations that are still valid, so we
                // need to switch over to overflow. We do this by updating all
                // groups to use their overflow locations instead.
                foreach ( var group in opportunities.Groups )
                {
                    group.Locations = group.OverflowLocations;

                    if ( group.OverflowLocations.Count > 0 )
                    {
                        group.OverflowLocations = new List<LocationAndScheduleBag>();
                    }
                }
            }
        }

        #endregion
    }
}
