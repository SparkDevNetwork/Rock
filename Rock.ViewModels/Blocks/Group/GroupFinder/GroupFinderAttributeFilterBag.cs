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
    /// A featured attribute filter rendered as pills in the What section of the filter bar.
    /// </summary>
    public class GroupFinderAttributeFilterBag
    {
        /// <summary>
        /// Gets or sets the attribute key this filter targets.
        /// </summary>
        public string AttributeKey { get; set; }

        /// <summary>
        /// Gets or sets the display label for the filter.
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Gets or sets the attribute's icon CSS class.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the selectable pill options (value and text) for the attribute.
        /// </summary>
        public List<ListItemBag> Options { get; set; }
    }
}
