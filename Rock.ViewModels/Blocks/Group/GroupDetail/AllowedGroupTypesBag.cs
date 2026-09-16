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

namespace Rock.ViewModels.Blocks.Group.GroupDetail
{
    /// <summary>
    /// The Add-mode Group Type dropdown source: the group types the user may pick for the
    /// selected parent group, plus an optional warning explaining why the list is empty.
    /// </summary>
    public class AllowedGroupTypesBag
    {
        /// <summary>
        /// Gets or sets the group types available for selection.
        /// </summary>
        public List<ListItemBag> Items { get; set; }

        /// <summary>
        /// Gets or sets the warning shown when <see cref="Items"/> is empty, explaining whether
        /// the block's group type settings or the parent group's allowed child group types caused
        /// it. Null when at least one group type is available.
        /// </summary>
        public string Warning { get; set; }
    }
}
