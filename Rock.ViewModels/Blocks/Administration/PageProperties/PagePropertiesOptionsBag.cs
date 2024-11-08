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

namespace Rock.ViewModels.Blocks.Administration.PageProperties
{
    /// <summary>
    /// Additional block configuration for the Page properties block.
    /// </summary>
    public class PagePropertiesOptionsBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block should be visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets the name of the site.
        /// </summary>
        /// <value>
        /// The name of the site.
        /// </value>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance can administrate.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can administrate; otherwise, <c>false</c>.
        /// </value>
        public bool CanAdministrate { get; set; }

        /// <summary>
        /// Gets or sets the sites items.
        /// </summary>
        /// <value>
        /// The sites items.
        /// </value>
        public List<ListItemBag> SitesItems { get; set; }

        /// <summary>
        /// Gets or sets the layout items.
        /// </summary>
        /// <value>
        /// The layout items.
        /// </value>
        public List<ListItemBag> LayoutItems { get; set; }

        /// <summary>
        /// Gets or sets the display when items.
        /// </summary>
        /// <value>
        /// The display when items.
        /// </value>
        public List<ListItemBag> DisplayWhenItems { get; set; }


        /// <summary>
        /// Gets or sets the intent defined type unique identifier.
        /// </summary>
        /// <value>
        /// The intent defined type unique identifier.
        /// </value>
        public string IntentDefinedTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable full edit mode].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable full edit mode]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableFullEditMode { get; set; }
    }
}
