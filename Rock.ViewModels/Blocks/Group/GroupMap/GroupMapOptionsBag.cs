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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Group.GroupMap
{
    /// <summary>
    /// The configuration the Group Map block ships to its Obsidian component. These values
    /// come from block settings and do not change for the life of the rendered block.
    /// </summary>
    public class GroupMapOptionsBag
    {
        /// <summary>
        /// Gets or sets the height of the map in pixels.
        /// </summary>
        public int MapHeight { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Map Style defined value. Passed to loadMapResources
        /// on the client so the rendered map matches the site's configured style / map id.
        /// </summary>
        public System.Guid? MapStyleValueGuid { get; set; }

        /// <summary>
        /// Gets or sets the list of colors used when rendering multiple polygons (geofences).
        /// </summary>
        public List<string> PolygonColors { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the campus filter is shown. The campus
        /// filter only narrows the connection-status / families layers.
        /// </summary>
        public bool IsCampusFilterShown { get; set; }

        /// <summary>
        /// Gets or sets the campuses available in the campus filter, already narrowed by the
        /// block's configured Campus Types and Campus Statuses.
        /// </summary>
        public List<ListItemBag> Campuses { get; set; }

        /// <summary>
        /// Gets or sets the group types available in the options panel's group-type picker
        /// (group types that can have a location).
        /// </summary>
        public List<ListItemBag> GroupTypes { get; set; }

        /// <summary>
        /// Gets or sets the connection statuses (with colors) rendered as togglable
        /// families layers.
        /// </summary>
        public List<GroupMapConnectionStatusBag> ConnectionStatuses { get; set; }
    }
}
