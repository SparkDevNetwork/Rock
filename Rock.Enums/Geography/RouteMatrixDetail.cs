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

namespace Rock.Enums.Geography
{
    /// <summary>
    /// Selects how much data a route matrix request returns, which in turn determines its billing tier.
    /// </summary>
    public enum RouteMatrixDetail
    {
        /// <summary>
        /// Every available field for each element. Can include Pro or Enterprise fields that bill at those higher tiers.
        /// </summary>
        Full,

        /// <summary>
        /// Only the distance for each element, which keeps the request within the Essentials billing tier.
        /// </summary>
        DistanceOnly,

        /// <summary>
        /// The distance and the static (non-traffic) drive time for each element, which still keeps the request within the Essentials billing tier.
        /// </summary>
        DistanceAndDuration
    }
}
