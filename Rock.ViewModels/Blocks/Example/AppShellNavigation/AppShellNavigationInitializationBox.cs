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

namespace Rock.ViewModels.Blocks.Example.AppShellNavigation
{
    /// <summary>
    /// The bag containing the data required to render the App Shell Navigation
    /// block: the real, security-trimmed Rock page-navigation tree. The account
    /// menu is a separate Login Status block hosted in the layout footer; the
    /// remaining visual widgets (notifications, projects, spending, organizations)
    /// are static demo content rendered client-side.
    /// </summary>
    public class AppShellNavigationInitializationBox
    {
        /// <summary>
        /// Gets or sets the top-level navigation items (the real, security-trimmed
        /// page tree from the configured root page).
        /// </summary>
        public List<AppShellNavItemBag> NavItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the universal search trigger
        /// should be shown.
        /// </summary>
        public bool IsSearchEnabled { get; set; }

        /// <summary>
        /// Gets or sets the URL for the home/brand link at the top of the sidebar.
        /// </summary>
        public string HomePageUrl { get; set; }
    }
}
