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

namespace Rock.ViewModels.Blocks.Cms.PersonalLinks
{
    /// <summary>
    /// The bag containing the data required to render the Personal Links block.
    /// </summary>
    public class PersonalLinksInitializationBox
    {
        /// <summary>
        /// Gets or sets a value indicating whether the block is visible.
        /// False when no person is logged in — the bookmark icon is hidden.
        /// </summary>
        public bool IsBlockVisible { get; set; }

        /// <summary>
        /// Gets or sets the person-specific localStorage key used to store quick return items.
        /// </summary>
        public string QuickLinksLocalStorageKey { get; set; }

        /// <summary>
        /// Gets or sets the modification hash used to detect when the personal links
        /// cached in localStorage are out of date with the server.
        /// </summary>
        public string PersonalLinksModificationHash { get; set; }

        /// <summary>
        /// Gets or sets the title of the current page, captured server-side so the
        /// Add Link form pre-fills the name with the same value the old WebForms
        /// block used (RockPage.Title) rather than the runtime document.title.
        /// </summary>
        public string CurrentPageTitle { get; set; }

        /// <summary>
        /// Gets or sets the absolute URL of the current page, captured server-side
        /// from the proxy-safe request URI so the Add Link form pre-fills the URL
        /// consistently across reverse-proxied deployments.
        /// </summary>
        public string CurrentPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the navigation URLs (e.g., the configured Manage Links page URL).
        /// </summary>
        public Dictionary<string, string> NavigationUrls { get; set; }
    }
}
