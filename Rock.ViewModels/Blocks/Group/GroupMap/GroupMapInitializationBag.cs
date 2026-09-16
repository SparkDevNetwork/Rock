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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Group.GroupMap
{
    /// <summary>
    /// The runtime data the Group Map block ships to its Obsidian component for the current
    /// request: the resolved group, the server-derived marker colors, the initial map view,
    /// and the person's saved filter preferences.
    /// </summary>
    public class GroupMapInitializationBag
    {
        /// <summary>
        /// Gets or sets the integer Id of the resolved group. The client passes this to the
        /// api/Groups/GetMapInfo/* endpoints when loading the marker layers.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the name of the resolved group.
        /// </summary>
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the guard message shown when the map cannot be displayed (e.g. no
        /// GroupId on the query string, or the group could not be found). When set, the
        /// client shows the message instead of the map.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the marker color for the group's own locations, derived server-side
        /// from the Map Style's Colors attribute.
        /// </summary>
        public string GroupColor { get; set; }

        /// <summary>
        /// Gets or sets the marker color for child group locations.
        /// </summary>
        public string ChildGroupColor { get; set; }

        /// <summary>
        /// Gets or sets the marker color for group member locations.
        /// </summary>
        public string MemberColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether child groups are shown by default. Sourced
        /// from the person's preference, falling back to the "Show Child Groups as Default"
        /// block setting.
        /// </summary>
        public bool IsShowChildGroupsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the group type Ids the person has selected in the settings panel to
        /// scope the child-group layer. Used when calling the child-groups map endpoint.
        /// </summary>
        public List<int> SelectedGroupTypeIds { get; set; }

        /// <summary>
        /// Gets or sets the group type Guids the person has selected in the settings panel to
        /// scope the child-group layer. Used to pre-select the settings picker.
        /// </summary>
        public List<Guid> SelectedGroupTypeGuids { get; set; }
    }
}
