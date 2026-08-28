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
    /// A group that could not be placed on the map because it has no geo-located location. Shown
    /// in the "some groups could not be mapped" warning.
    /// </summary>
    public class GroupTypeMapUnmappedGroupBag
    {
        /// <summary>
        /// Gets or sets the name of the group.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the group detail page URL, or an empty string when no Group Detail Page
        /// is configured. When present the group's name is shown as a link.
        /// </summary>
        public string DetailPageUrl { get; set; }
    }
}
