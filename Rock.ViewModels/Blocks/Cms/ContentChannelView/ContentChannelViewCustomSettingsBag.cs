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
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelView
{
    /// <summary>
    /// The settings used by the Content Channel View block.
    /// </summary>
    public class ContentChannelViewCustomSettingsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the content channel that items are displayed from.
        /// </summary>
        public Guid? ContentChannelGuid { get; set; }

        /// <summary>
        /// Gets or sets the content channel item statuses that should be included when displaying items.
        /// </summary>
        public List<ContentChannelItemStatus> ContentChannelItemStatuses { get; set; }

        /// <summary>
        /// Gets or sets the Lava template used when formatting the list of content channel items.
        /// </summary>
        public string LavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of items to display.
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block should set the page title using the channel name or content item.
        /// </summary>
        public bool IsPageTitleUpdateEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content data and attribute values should be merged using Lava.
        /// </summary>
        public bool IsItemMergeFieldEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <c>ItemTagList</c> Lava merge field should be populated.
        /// </summary>
        public bool IsItemTagListMergeFieldEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an additional <c>ArchiveSummary</c> collection should be made available in Lava for building a month/year summary list of items.
        /// </summary>
        public bool IsArchiveSummaryMergeFieldEnabled { get; set; }

        /// <summary>
        /// Gets or sets the page used to navigate to for content item details.
        /// </summary>
        public PageRouteValueBag DetailPage { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to cache the content items returned by the selected filter.
        /// </summary>
        public int ItemCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to cache the rendered output.
        /// </summary>
        public int OutputCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the cache tags used to link cached content so that it can be expired as a group.
        /// </summary>
        public List<string> CacheTags { get; set; }

        /// <summary>
        /// Gets or sets the data filter used to filter the items that are displayed.
        /// </summary>
        public DataViewFilterBag DataViewFilter { get; set; }

        /// <summary>
        /// Gets or sets the item attribute key used when filtering items by the block's Context entity.
        /// </summary>
        public string ContextFilterAttributeKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block should evaluate query string parameters for additional filter criteria.
        /// </summary>
        public bool IsPageParameterFilteringEnabled { get; set; }

        /// <summary>
        /// Gets or sets the computed key (entity^attributeKey) of the attribute used for the meta description.
        /// </summary>
        public string MetaDescriptionAttributeValueKey { get; set; }

        /// <summary>
        /// Gets or sets the computed key (entity^attributeKey) of the attribute used for the meta image.
        /// </summary>
        public string MetaImageAttributeValueKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an RSS autodiscover link should be added to the page head.
        /// </summary>
        public bool IsSetRssAutodiscoverLinkEnabled { get; set; }

        /// <summary>
        /// Gets or sets how personalization (segments and request filters) affects the items that are returned.
        /// </summary>
        public PersonalizationFilterType PersonalizationFilterType { get; set; }

        /// <summary>
        /// Gets or sets the collection of items by which to order the results.
        /// </summary>
        /// <remarks>
        ///     <para>Each ListItemBag.Value corresponds to the ListItemBag.Value of a valid key option.</para>
        ///     <para>Each ListItemBag.Text corresponds to the ListItemBag.Value of a valid value option.</para>
        /// </remarks>
        public List<ListItemBag> OrderItemsBy { get; set; }
    }
}
