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
    public class ContentChannelViewCustomSettingsBag
    {
        public Guid? ContentChannelGuid { get; set; }

        public List<ContentChannelItemStatus> ContentChannelItemStatuses { get; set; }

        public string LavaTemplate { get; set; }

        public int ItemsPerPage { get; set; }

        public bool IsPageTitleUpdateEnabled { get; set; }

        public bool IsItemMergeFieldEnabled { get; set; }

        public bool IsItemTagListMergeFieldEnabled { get; set; }

        public bool IsArchiveSummaryMergeFieldEnabled { get; set; }

        public PageRouteValueBag DetailPage { get; set; }

        public int ItemCacheDuration { get; set; }

        public int OutputCacheDuration { get; set; }

        public List<string> CacheTags { get; set; }

        public DataViewFilterBag DataViewFilter { get; set; }

        public string ContextFilterAttributeKey { get; set; }

        public bool IsPageParameterFilteringEnabled { get; set; }

        public string MetaDescriptionAttributeValueKey { get; set; }

        public string MetaImageAttributeValueKey { get; set; }

        public bool IsSetRssAutodiscoverLinkEnabled { get; set; }

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
