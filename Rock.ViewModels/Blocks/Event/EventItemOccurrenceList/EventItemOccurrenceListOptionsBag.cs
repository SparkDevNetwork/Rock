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
using System.Collections.Generic;

namespace Rock.ViewModels.Blocks.Event.EventItemOccurrenceList
{
    /// <summary>
    /// The additional configuration options for the Event Item Occurrence List block.
    /// </summary>
    public class EventItemOccurrenceListOptionsBag
    {
        /// <summary>
        /// Gets or sets the registration instance page URL from the block settings.
        /// </summary>
        /// <value>
        /// The registration instance page URL.
        /// </value>
        public string RegistrationInstancePageUrl { get; set; }

        /// <summary>
        /// Gets or sets the group detail page URL from the block settings.
        /// </summary>
        /// <value>
        /// The group detail page URL.
        /// </value>
        public string GroupDetailPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the content item detail page URL from the block settings
        /// </summary>
        /// <value>
        /// The content item detail page URL.
        /// </value>
        public string ContentItemDetailPageUrl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block should be displayed to the user.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the block should be visible; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets available campus items for the Campus filter.
        /// </summary>
        /// <value>
        /// The campus items.
        /// </value>
        public List<ListItemBag> CampusItems { get; set; }
    }
}
