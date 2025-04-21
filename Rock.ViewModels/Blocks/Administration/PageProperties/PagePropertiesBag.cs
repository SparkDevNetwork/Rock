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
using Rock.Enums.Cms;
using Rock.Model;
using Rock.ViewModels.Controls;
using System;

namespace Rock.ViewModels.Blocks.Administration.PageProperties
{
    /// <summary>
    /// Contains the information needed to render the Page Properties block.
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class PagePropertiesBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets a value indicating whether [allow indexing].
        /// </summary>
        public bool AllowIndexing { get; set; }

        /// <summary>
        /// Gets or sets the body CSS class.
        /// </summary>
        public string BodyCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether icon is displayed in breadcrumb.
        /// </summary>
        public bool BreadCrumbDisplayIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Page Name is displayed in the breadcrumb.
        /// </summary>
        public bool BreadCrumbDisplayName { get; set; }

        /// <summary>
        /// Gets or sets the browser title to use for the page.
        /// </summary>
        public string BrowserTitle { get; set; }

        /// <summary>
        /// Gets or sets the cache control header settings.
        /// </summary>
        public RockCacheabilityBag CacheControlHeaderSettings { get; set; }

        /// <summary>
        /// Gets or sets a user defined description of the page.  This will be added as a meta tag for the page 
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating when the Page should be displayed in the navigation.
        /// </summary>
        public DisplayInNavWhen DisplayInNavWhen { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if view state should be enabled on the page. 
        /// </summary>
        public bool EnableViewState { get; set; }

        /// <summary>
        /// Gets or sets HTML content to add to the page header area of the page when rendered.
        /// </summary>
        public string HeaderContent { get; set; }

        /// <summary>
        /// Gets or sets the icon Rock.Model.BinaryFile.
        /// </summary>
        public ListItemBag IconBinaryFile { get; set; }

        /// <summary>
        /// Gets or sets the icon binary file identifier.
        /// </summary>
        public int? IconBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the icon CSS class name for a font vector based icon.
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the admin footer should be displayed when a Site Administrator is logged in.
        /// </summary>
        public bool IncludeAdminFooter { get; set; }

        /// <summary>
        /// Gets or sets the internal name to use when administering this page
        /// </summary>
        public string InternalName { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Page is part of the Rock core system/framework.
        /// </summary>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the Rock.Model.Layout that the pages uses.
        /// </summary>
        public ListItemBag Layout { get; set; }

        /// <summary>
        /// Gets or sets the median page load time in seconds. Typically calculated from a set of
        /// Rock.Model.Interaction.InteractionTimeToServe values.
        /// </summary>
        public double? MedianPageLoadTimeDurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Page's children Pages should be displayed in the menu.
        /// </summary>
        public bool MenuDisplayChildPages { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Page description should be displayed in the menu.
        /// </summary>
        public bool MenuDisplayDescription { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the Page icon should be displayed in the menu.
        /// </summary>
        public bool MenuDisplayIcon { get; set; }

        /// <summary>
        /// Gets or sets a number indicating the order of the page in the menu and in the site map.
        /// This will also affect the page order in the menu. This property is required.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the length of time (in seconds) in that rendered output is cached. This property is required.
        /// </summary>
        public int OutputCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating whether breadcrumbs are displayed on Page
        /// </summary>
        public bool PageDisplayBreadCrumb { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Page description should be displayed on the page.
        /// </summary>
        public bool PageDisplayDescription { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Page icon should be displayed on the Page.
        /// </summary>
        public bool PageDisplayIcon { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Page Title should be displayed on the page (if the Rock.Model.Layout supports it).
        /// </summary>
        public bool PageDisplayTitle { get; set; }

        /// <summary>
        /// Gets or sets the title of the of the Page to use as the page caption, in menu's, breadcrumb display etc.
        /// </summary>
        public string PageTitle { get; set; }

        /// <summary>
        /// Gets or sets the Page entity for the parent page.
        /// </summary>
        public ListItemBag ParentPage { get; set; }

        /// <summary>
        /// Gets or sets the Id of the parent Page.
        /// </summary>
        public int? ParentPageId { get; set; }

        /// <summary>
        /// Gets or sets the rate limit period (in seconds).
        /// </summary>
        public int? RateLimitPeriodDurationSeconds { get; set; }

        /// <summary>
        /// Gets or sets the rate limit request per period.
        /// </summary>
        public int? RateLimitRequestPerPeriod { get; set; }

        /// <summary>
        /// Gets or sets a flag that indicates if the Page requires SSL encryption.
        /// </summary>
        public bool RequiresEncryption { get; set; }

        /// <summary>
        /// Gets or sets the page link.
        /// </summary>
        /// <value>
        /// The page link.
        /// </value>
        public string PageUrl { get; set; }

        /// <summary>
        /// Gets or sets the site.
        /// </summary>
        /// <value>
        /// The site.
        /// </value>
        public ListItemBag Site { get; set; }

        /// <summary>
        /// Gets or sets the page route.
        /// </summary>
        /// <value>
        /// The page route.
        /// </value>
        public string PageRoute { get; set; }

        /// <summary>
        /// Gets or sets the intents.
        /// </summary>
        /// <value>
        /// The intents.
        /// </value>
        public List<ListItemBag> Intents { get; set; }

        /// <summary>
        /// Gets or sets the block contexts.
        /// </summary>
        /// <value>
        /// The block contexts.
        /// </value>
        public List<BlockContextInfoBag> BlockContexts { get; set; }

        /// <summary>
        /// Gets or sets the page identifier.
        /// </summary>
        /// <value>
        /// The page identifier.
        /// </value>
        public int PageId { get; set; }
    }
}
