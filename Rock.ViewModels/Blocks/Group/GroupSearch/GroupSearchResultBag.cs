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

namespace Rock.ViewModels.Blocks.Group.GroupSearch
{
    /// <summary>
    /// Contains a group search result row.
    /// </summary>
    public class GroupSearchResultBag
    {
        /// <summary>
        /// Gets or sets the identifier of the group.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the parent group hierarchy HTML that appears before the group link.
        /// </summary>
        public string Structure { get; set; }

        /// <summary>
        /// Gets or sets the group type name.
        /// </summary>
        public string GroupType { get; set; }

        /// <summary>
        /// Gets or sets the number of members in the group.
        /// </summary>
        public int MemberCount { get; set; }

        /// <summary>
        /// Gets or sets the campus name.
        /// </summary>
        public string Campus { get; set; }

        /// <summary>
        /// Gets or sets the target URL for the group.
        /// </summary>
        public string Url { get; set; }
    }
}
