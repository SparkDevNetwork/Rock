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

using Rock.Enums.Cms;
using Rock.Model;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Mobile.MobilePageDetail
{
    /// <summary>
    /// The page settings shown in the view and edit panels (the "Details" panel).
    /// </summary>
    public class MobilePageDetailsBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the unique identifier of the page. Used by the copy-to-clipboard
        /// button and when launching custom block actions in the builder.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the public name of the page (stored as both the page title
        /// and browser title).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the internal name used to identify the page in Rock. Not
        /// visible to app users.
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets the description of the page. In view mode this is only shown
        /// when it has content.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the layout template that defines the zones available on this
        /// page. In view mode this links to the Mobile Layout Detail block.
        /// </summary>
        public ListItemBag Layout { get; set; }

        /// <summary>
        /// Gets or sets when the page appears in the flyout menu and tab bar.
        /// </summary>
        public DisplayInNavWhen DisplayInNavWhen { get; set; }

        /// <summary>
        /// Gets or sets whether the page is powered by Rock blocks (native) or opens
        /// a URL in a browser.
        /// </summary>
        public MobilePageType PageType { get; set; }

        /// <summary>
        /// Gets or sets the URL opened when a user navigates to this page. Used when
        /// <see cref="PageType"/> is an internal or external web page.
        /// </summary>
        public string WebPageUrl { get; set; }

        /// <summary>
        /// Gets or sets the route that maps this mobile page to its web equivalent by
        /// matching route paths.
        /// </summary>
        public string Route { get; set; }

        /// <summary>
        /// Gets or sets one or more CSS classes applied to the page body element
        /// (space-separated).
        /// </summary>
        public string BodyCssClass { get; set; }

        /// <summary>
        /// Gets or sets the icon displayed alongside the page in navigation.
        /// </summary>
        public ListItemBag PageIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the navigation bar is hidden and the
        /// status bar background is transparent, extending content to the top edge of
        /// the screen.
        /// </summary>
        public bool HideNavigationBar { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the page replaces the entire shell
        /// when open, preventing access to the flyout and tab bar.
        /// </summary>
        public bool ShowFullScreen { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the page reloads each time it
        /// becomes visible.
        /// </summary>
        public bool AutoRefresh { get; set; }

        /// <summary>
        /// Gets or sets the Lava executed on the client when a page event fires.
        /// </summary>
        public string LavaEventHandler { get; set; }

        /// <summary>
        /// Gets or sets the CSS applied only to elements on this page, scoped away from
        /// the rest of the app.
        /// </summary>
        public string CssStyles { get; set; }

        /// <summary>
        /// Gets or sets the context parameters for blocks on this page that load content
        /// based on a context entity.
        /// </summary>
        public List<MobilePageContextParameterBag> ContextParameters { get; set; }
    }
}
