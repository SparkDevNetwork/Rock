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

using System;

namespace Rock.ViewModels.Blocks.Group.GroupTypeMap
{
    /// <summary>
    /// The configuration the Group Type Map block ships to its Obsidian component. These values
    /// come from block settings and do not change for the life of the rendered block.
    /// </summary>
    public class GroupTypeMapOptionsBag
    {
        /// <summary>
        /// Gets or sets the height of the map in pixels.
        /// </summary>
        public int MapHeight { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the Map Style defined value. Passed to loadMapResources
        /// on the client so the rendered map matches the site's configured style / map id.
        /// </summary>
        public Guid? MapStyleValueGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an info window should be displayed when a
        /// map point is clicked.
        /// </summary>
        public bool IsInfoWindowShown { get; set; }
    }
}
