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

namespace Rock.ViewModels.Controls
{
    /// <summary>
    /// Identifies a single search result item for the Universal Item Search
    /// Picker field type.
    /// </summary>
    public class UniversalItemSearchPickerItemBag
    {
        /// <summary>
        /// Gets or sets the value that identifies this item in the database.
        /// </summary>
        /// <value>The value that identifies this item in the database.</value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the title of this search result item.
        /// </summary>
        /// <value>The title of this search result item.</value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the optional description that provides additional
        /// context about this specific item.
        /// </summary>
        /// <value>The optional description.</value>
        public string Description { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the optional details. Each detail item will be displayed
        /// with the Value displayed as the "key" and the Text as the displayed
        /// value.
        /// </para>
        /// <para>
        /// For example, if your detail item was Value="Campus" and
        /// Text="North Ridge" then it might display something like
        /// "Campus: North Ridge".
        /// </para>
        /// </summary>
        /// <value>The optional details.</value>
        public List<ListItemBag> Details { get; set; }

        /// <summary>
        /// Gets or sets the optional labels to display with the item. These are
        /// usually displayed as a colored pill style. The Text is displayed inside
        /// the label and the Value specifies the color name to use with label.
        /// Currently the standard bootstrap color names (primary, info, danger, etc.)
        /// are supported.
        /// </summary>
        /// <value>The optional labels.</value>
        public List<ListItemBag> Labels { get; set; }
    }
}
