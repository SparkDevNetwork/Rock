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
using Rock.ViewModels.Rest.Controls;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemView
{
    /// <summary>
    /// The settings used by the ContentChannelItemView block.
    /// </summary>
    public class ContentChannelItemViewCustomSettingsBag
    {
        /// <summary>
        /// Gets or sets the unique identifier of the content channel that limits which items can be displayed.
        /// </summary>
        public Guid? ContentChannelGuid { get; set; }

        /// <summary>
        /// Gets or sets the content channel item statuses that should be considered approved for display.
        /// </summary>
        public List<ContentChannelItemStatus> ContentChannelItemStatuses { get; set; }

        /// <summary>
        /// Gets or sets the Lava template used to format the content channel item.
        /// </summary>
        public string LavaTemplate { get; set; }

        /// <summary>
        /// Gets or sets the URL parameter name used to determine which content channel item to display.
        /// </summary>
        public string ContentChannelQueryParameter { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the most recent item should be shown when no query parameter value is provided.
        /// </summary>
        public bool IsDisplayMostRecentEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the block should set the page title to the content channel item title.
        /// </summary>
        public bool IsPageTitleUpdateEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the content data and attribute values should be merged using Lava.
        /// </summary>
        public bool IsItemMergeFieldEnabled { get; set; }

        /// <summary>
        /// Gets or sets the page used to view a content item.
        /// </summary>
        public PageRouteValueBag DetailPage { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to cache the content channel item.
        /// </summary>
        public int ItemCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the number of seconds to cache the rendered output.
        /// </summary>
        public int OutputCacheDuration { get; set; }

        /// <summary>
        /// Gets or sets the cache tags applied to cached content for this block.
        /// </summary>
        public List<string> CacheTags { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether interactions should be logged when an item is viewed.
        /// </summary>
        public bool IsLogInteractionsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether interactions should be written only when an individual is logged in.
        /// </summary>
        public bool IsWriteInteractionOnlyIfIndividualLoggedInEnabled { get; set; }

        /// <summary>
        /// Gets or sets the workflow type that should be launched when an item is viewed.
        /// </summary>
        public ListItemBag WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the workflow should be launched only when an individual is logged in.
        /// </summary>
        public bool IsLaunchWorkflowOnlyIfIndividualLoggedInEnabled { get; set; }

        /// <summary>
        /// Gets or sets the condition that determines when the configured workflow should be launched.
        /// </summary>
        public string LaunchWorkflowCondition { get; set; }

        /// <summary>
        /// Gets or sets the computed key (entity^attributeKey) of the attribute used for the meta description.
        /// </summary>
        public string MetaDescriptionAttributeValueKey { get; set; }

        /// <summary>
        /// Gets or sets the Open Graph object type used in the og:type meta tag.
        /// </summary>
        public string OpenGraphType { get; set; }

        /// <summary>
        /// Gets or sets the computed key (entity^attributeKey) of the attribute used for the og:title meta tag.
        /// </summary>
        public string OpenGraphTitleAttributeValueKey { get; set; }

        /// <summary>
        /// Gets or sets the computed key (entity^attributeKey) of the attribute used for the og:description meta tag.
        /// </summary>
        public string OpenGraphDescriptionAttributeValueKey { get; set; }

        /// <summary>
        /// Gets or sets the computed key (entity^attributeKey) of the attribute used for the og:image meta tag.
        /// </summary>
        public string OpenGraphImageAttributeValueKey { get; set; }

        /// <summary>
        /// Gets or sets the computed key (entity^attributeKey) of the attribute used for the twitter:title meta tag.
        /// </summary>
        public string TwitterTitleAttributeValueKey { get; set; }

        /// <summary>
        /// Gets or sets the computed key (entity^attributeKey) of the attribute used for the twitter:description meta tag.
        /// </summary>
        public string TwitterDescriptionAttributeValueKey { get; set; }

        /// <summary>
        /// Gets or sets the computed key (entity^attributeKey) of the attribute used for the twitter:image meta tag.
        /// </summary>
        public string TwitterImageAttributeValueKey { get; set; }

        /// <summary>
        /// Gets or sets the Twitter card type ("none", "summary", or "summary_large_image").
        /// </summary>
        public string TwitterCard { get; set; }
    }
}
