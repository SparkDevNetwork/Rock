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
    /// A campus option in the Group Finder Campus filter, carrying a short badge for its avatar.
    /// </summary>
    public class GroupFinderCampusBag
    {
        /// <summary>
        /// Gets or sets the short letter badge (avatar) for the campus.
        /// </summary>
        public string Badge { get; set; }

        /// <summary>
        /// Gets or sets the campus name shown beside the badge.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the campus unique identifier.
        /// </summary>
        public string Value { get; set; }
    }
}
