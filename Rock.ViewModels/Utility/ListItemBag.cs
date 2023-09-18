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

namespace Rock.ViewModels.Utility
{
    /// <summary>
    /// Identifies a single item that will be displayed through some UI control.
    /// </summary>
    public class ListItemBag
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>
        /// The text.
        /// </value>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the category for this item.
        /// </summary>
        /// <value>
        /// The category for this item.
        /// </value>
        /// <remarks>Categories are only supported on certain UI controls.</remarks>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets disabled for this item.
        /// </summary>
        /// <value>
        /// Wether or not this item is disabled (visible but not selectable).
        /// </value>
        /// <remarks>Disabled is only supported on certain UI controls.</remarks>
        public bool? Disabled { get; set; }
    }
}
