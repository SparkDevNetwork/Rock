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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Core.AttributeCategoryList
{
    /// <summary>
    /// The bag that contains the editable fields for an attribute category.
    /// </summary>
    public class AttributeCategoryBag
    {
        /// <summary>
        /// Gets or sets the encrypted identifier key for the category.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the category.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the entity type associated with this category.
        /// A null value indicates this is a Global Attributes category.
        /// </summary>
        public ListItemBag EntityType { get; set; }

        /// <summary>
        /// Gets or sets the CSS class for the category's icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the highlight color for the category.
        /// </summary>
        public string HighlightColor { get; set; }
    }
}
