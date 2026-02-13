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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceLinkageDetail
{
    /// <summary>
    /// Represents the details for a registration instance linkage, including group, campus, calendar item, and display information.
    /// </summary>
    public class RegistrationInstanceLinkageDetailBag
    {
        /// <summary>
        /// Gets or sets the identifier key for the registration instance linkage.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the URL slug used for the registration instance linkage.
        /// </summary>
        public string UrlSlug { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the group associated with the linkage.
        /// </summary>
        public ListItemBag Group { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the campus associated with the linkage.
        /// </summary>
        public ListItemBag Campus { get; set; }

        /// <summary>
        /// Gets or sets the calendar item details associated with the linkage.
        /// </summary>
        public RegistrationInstanceLinkageDetailCalendarItemBag CalendarItem { get; set; }

        /// <summary>
        /// Gets or sets the public name to display for the registration instance linkage.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the context information associated with the registration instance linkage.
        /// </summary>
        public RegistrationInstanceLinkageContextBag Context { get; set; }
    }
}
