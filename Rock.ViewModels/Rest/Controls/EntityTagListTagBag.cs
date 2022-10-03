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

using System;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Identifies a single tag to the EntityTagList control.
    /// </summary>
    public class EntityTagListTagBag
    {
        /// <summary>
        /// Gets or sets the identifier key of the tag.
        /// </summary>
        /// <value>The identifier key of the tag.</value>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the entity type unique identifier.
        /// </summary>
        /// <value>The entity type unique identifier.</value>
        public Guid EntityTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        /// <value>The name of the tag.</value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class to display with tag.
        /// </summary>
        /// <value>The icon CSS class to display with tag.</value>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the color of the background of the tag.
        /// </summary>
        /// <value>The color of the background of the tag.</value>
        public string BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the category the tag belongs to.
        /// </summary>
        /// <value>The category the tag belongs to.</value>
        public ListItemBag Category { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this tag is personal.
        /// </summary>
        /// <value><c>true</c> if this tag is personal; otherwise, <c>false</c>.</value>
        public bool IsPersonal { get; set; }
    }
}
