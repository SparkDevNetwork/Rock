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

namespace Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInScheduleBuilder
{
    /// <summary>
    /// The Check-in Schedule Builder Options Bag
    /// </summary>
    public class CheckInScheduleBuilderOptionsBag
    {
        /// <summary>
        /// Gets or sets whether the page has a valid check-in configuration parameter.
        /// </summary>
        public bool HasValidCheckInConfigurationPageParam { get; set; }

        /// <summary>
        /// Maps each campus Guid to its root location Guid.
        /// Used by the client to scope the parent location picker when the campus context changes.
        /// </summary>
        public Dictionary<string, string> CampusRootLocations { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// The list of GroupTypes that can be chosen from
        /// </summary>
        public List<Guid> GroupTypes { get; set; }

        /// <summary>
        /// The list of Areas that can be chosen from
        /// </summary>
        public List<ListItemBag> Areas { get; set; }

        /// <summary>
        /// Gets or sets the Guid of the currently-selected area (empty / <c>null</c> for "All Areas"), resolved
        /// from the configuration-scoped person preference.
        /// </summary>
        public Guid? SelectedAreaGuid { get; set; }

        /// <summary>
        /// Gets or sets the hashed identifier of the check-in configuration. The client scopes the shared
        /// selected-area person preference to this configuration entity when persisting a new selection.
        /// </summary>
        public string ConfigurationIdKey { get; set; }

        /// <summary>
        /// Gets or sets the display name of the check-in configuration or <c>null</c> when this block is rendered
        /// without a configuration.
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// The Default Schedule Category
        /// </summary>
        public ListItemBag DefaultScheduleCategory { get; set; }

        /// <summary>
        /// Gets or sets the navigation urls.
        /// </summary>
        /// <value>The navigation urls.</value>
        public Dictionary<string, string> NavigationUrls { get; set; } = new Dictionary<string, string>();
    }
}
