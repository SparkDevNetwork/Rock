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

namespace Rock.ViewModels.Blocks.Group.GroupFinder
{
    /// <summary>
    /// The available choices used to build the Group Finder custom settings UI, scoped to the selected group types.
    /// </summary>
    public class GroupFinderCustomSettingsOptionsBag
    {
        /// <summary>
        /// Gets or sets the attributes eligible to be featured as "What" pills: filterable attributes of the selected group types that are also Single-select, Multi-select, or Boolean.
        /// </summary>
        public List<ListItemBag> AvailableFeaturedAttributes { get; set; }

        /// <summary>
        /// Gets or sets the attributes eligible for the "More Filters" modal: filterable attributes of the selected group types.
        /// </summary>
        public List<ListItemBag> AvailableDisplayAttributes { get; set; }

        /// <summary>
        /// Gets or sets the attributes eligible to be shown on the card: all attributes of the selected group types.
        /// </summary>
        public List<ListItemBag> AvailableCardAttributes { get; set; }
    }
}
