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
    /// Performs filtering of check-in opportunities based on the location status.
    /// </summary>
    internal class LocationClosedOpportunityFilter : OpportunityFilter
    {
        #region Methods

        /// <inheritdoc/>
        public override bool IsLocationValid( LocationOpportunity location )
        {
            return !location.IsClosed;
        }

        #endregion
    }
}
