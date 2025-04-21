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

namespace Rock.CheckIn.v2.Filters
{
    /// <summary>
    /// Performs filtering of check-in opportunities based on the location
    /// capacity threshold.
    /// </summary>
    internal class ThresholdOpportunityFilter : OpportunityFilter
    {
        #region Methods

        /// <inheritdoc/>
        public override void FilterLocations( OpportunityCollection opportunities )
        {
            var startCount = opportunities.Locations.Count;

            if ( startCount == 0 )
            {
                return;
            }

            base.FilterLocations( opportunities );

            // If we removed the last location then mark them as unavailable
            // and set a helpful message to display in the UI.
            if ( opportunities.Locations.Count == 0 && !Person.IsUnavailable )
            {
                Person.IsUnavailable = true;
                Person.UnavailableMessage = "All Locations Are Full";
            }
        }

        /// <inheritdoc/>
        public override bool IsLocationValid( LocationOpportunity location )
        {
            // If there are no limits, then always allow check-in.
            if ( !location.Capacity.HasValue )
            {
                return true;
            }

            // If there are enough spots for an additional person, then
            // allow check-in.
            if ( location.CurrentCount < location.Capacity.Value )
            {
                return true;
            }

            // If we are over limit, but the person is already checked into
            // the location, then allow check-in.
            return location.CurrentPersonIds.Contains( Person.Person.Id );
        }

        #endregion
    }
}
