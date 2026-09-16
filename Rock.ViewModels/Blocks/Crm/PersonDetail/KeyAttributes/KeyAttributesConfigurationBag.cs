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

namespace Rock.ViewModels.Blocks.Crm.PersonDetail.KeyAttributes
{
    /// <summary>
    /// The data required to drive the configure dialog, where the current
    /// person selects which attributes to bookmark.
    /// </summary>
    public class KeyAttributesConfigurationBag
    {
        /// <summary>
        /// Gets or sets the selectable categories. The value is the category
        /// unique identifier, or an empty string for the "Uncategorized"
        /// group. Only categories containing at least one view-authorized
        /// attribute are included.
        /// </summary>
        public List<ListItemBag> Categories { get; set; }

        /// <summary>
        /// Gets or sets the view-authorized attributes for each category,
        /// keyed by the category value used in <see cref="Categories"/>. Each
        /// attribute's value is its unique identifier and the text is its name.
        /// </summary>
        public Dictionary<string, List<ListItemBag>> AttributesByCategory { get; set; }

        /// <summary>
        /// Gets or sets the unique identifiers of the currently bookmarked
        /// attributes, in their saved display order.
        /// </summary>
        public List<string> SelectedAttributeGuids { get; set; }
    }
}
