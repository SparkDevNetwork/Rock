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

namespace Rock.ViewModels.Blocks.Group.GroupTypeMap
{
    /// <summary>
    /// A single mappable group: the marker's position and the identifiers the client passes back
    /// to render its info window.
    /// </summary>
    public class GroupTypeMapGroupBag
    {
        /// <summary>
        /// Gets or sets the Id of the group. Passed to the GetInfoWindow block action when the
        /// marker is clicked.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the Id of the location that produced the marker (the group's first
        /// geo-located location). Used to resolve the address portion of the info window.
        /// </summary>
        public int LocationId { get; set; }

        /// <summary>
        /// Gets or sets the name of the group, used as the marker's title.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the latitude of the marker.
        /// </summary>
        public double Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude of the marker.
        /// </summary>
        public double Longitude { get; set; }
    }
}
