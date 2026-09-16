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

namespace Rock.ViewModels.Blocks.Group.GroupMap
{
    /// <summary>
    /// A person connection status rendered as a togglable families layer on the group map.
    /// </summary>
    public class GroupMapConnectionStatusBag
    {
        /// <summary>
        /// Gets or sets the connection status defined value Id. Passed to the
        /// api/Groups/GetMapInfo/{groupId}/Families/{statusId} endpoint when the layer is toggled on.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the pluralized connection status name shown on the layer toggle.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the marker / swatch color for this layer (hex, without the leading '#').
        /// </summary>
        public string Color { get; set; }
    }
}
