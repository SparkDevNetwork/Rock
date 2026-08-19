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

namespace Rock.ViewModels.Blocks.Group.GroupList
{
    /// <summary>
    /// The response for the Group List block's GetAvailableGroups action, used to
    /// populate the add-member drop-down picker.
    /// </summary>
    public class GroupListAvailableGroupsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the candidate group dataset is too
        /// large to load into a drop-down list. When true, no groups are returned and
        /// the add-member modal should fall back to the tree group picker instead.
        /// </summary>
        public bool IsDatasetTooLarge { get; set; }

        /// <summary>
        /// Gets or sets the groups the current person may add the context person to.
        /// Empty when <see cref="IsDatasetTooLarge"/> is true.
        /// </summary>
        public List<ListItemBag> Groups { get; set; }
    }
}
