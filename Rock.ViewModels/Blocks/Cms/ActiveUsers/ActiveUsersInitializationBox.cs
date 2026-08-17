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

namespace Rock.ViewModels.Blocks.Cms.ActiveUsers
{
    /// <summary>
    /// The top-level initialization object returned by the Active Users block.
    /// </summary>
    public class ActiveUsersInitializationBox
    {
        /// <summary>
        /// Gets or sets the name of the site whose active users are being displayed.
        /// </summary>
        public string SiteName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the site name should be rendered as a title above the list.
        /// </summary>
        public bool ShowSiteName { get; set; }

        /// <summary>
        /// Gets or sets a warning message to display instead of the list. Set when no site is configured
        /// or when page views are not enabled for the configured site.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the informational message shown when there are no active users.
        /// </summary>
        public string EmptyMessage { get; set; }

        /// <summary>
        /// Gets or sets the list of currently active users to display.
        /// </summary>
        public List<ActiveUserBag> ActiveUsers { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether per-user tooltips listing recent page titles should be shown.
        /// </summary>
        public bool ShowTooltip { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the guest visitor counts should be displayed.
        /// </summary>
        public bool ShowGuestVisitors { get; set; }

        /// <summary>
        /// Gets or sets the number of guest visitors that have been active in the last 5 minutes.
        /// </summary>
        public int RecentGuestCount { get; set; }

        /// <summary>
        /// Gets or sets the number of guest visitors active between 5 and 15 minutes ago.
        /// </summary>
        public int InactiveGuestCount { get; set; }
    }
}
