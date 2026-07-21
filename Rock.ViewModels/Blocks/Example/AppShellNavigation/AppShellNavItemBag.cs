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
    /// Represents a single page node in the App Shell Navigation tree. Mirrors
    /// the shape produced by <c>PageCache.GetMenuProperties</c> so the sidebar
    /// renders Rock's real, security-trimmed page hierarchy.
    /// </summary>
    public class AppShellNavItemBag
    {
        /// <summary>
        /// Gets or sets the page identifier.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the display title for the page.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the resolved URL for the page.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class configured for the page.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this page is the one currently
        /// being viewed (drives the active-item styling).
        /// </summary>
        public bool IsCurrent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this page is an ancestor of the
        /// page currently being viewed (used to expand the containing group on load).
        /// </summary>
        public bool IsParentOfCurrent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this page's child pages should
        /// be displayed in the navigation.
        /// </summary>
        public bool DisplayChildPages { get; set; }

        /// <summary>
        /// Gets or sets the child page nodes.
        /// </summary>
        public List<AppShellNavItemBag> Children { get; set; }
    }
}
