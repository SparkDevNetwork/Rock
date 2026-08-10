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

namespace Rock.ViewModels.Blocks.Group.GroupFinder
{
    /// <summary>
    /// A single group's map marker. The coordinates are fuzzed for privacy; the true group location is never sent to the client.
    /// </summary>
    public class GroupFinderMapMarkerBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the group this marker represents, matching its card.
        /// </summary>
        public string GroupGuid { get; set; }

        /// <summary>
        /// Gets or sets the fuzzed latitude the marker is plotted at.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the fuzzed longitude the marker is plotted at.
        /// </summary>
        public double Longitude { get; set; }

        /// <summary>
        /// Gets or sets the radius, in meters, of the privacy circle drawn around the marker. The true location falls somewhere within this circle.
        /// </summary>
        public double CircleRadiusMeters { get; set; }
    }
}
