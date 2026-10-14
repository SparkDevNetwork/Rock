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
using System.Collections.Generic;

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Event.RegistrationTemplateDetail
{
    /// <summary>
    /// A placement configuration that describes how registrants are placed into groups.
    /// </summary>
    public class RegistrationTemplatePlacementBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the placement configuration.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the display order of the placement configuration.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the name of the placement configuration.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the group type that placement groups are limited to.
        /// </summary>
        public ListItemBag GroupType { get; set; }

        /// <summary>
        /// Gets or sets the CSS icon class of the placement. When blank the icon of the group type is used.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a registrant can be placed in more than one group.
        /// </summary>
        public bool AllowMultiplePlacements { get; set; }

        /// <summary>
        /// Gets or sets the groups that are linked as placement groups to every registration
        /// instance of the template. The value of each item is the unique identifier of the group.
        /// </summary>
        public List<ListItemBag> SharedGroups { get; set; }
    }
}
