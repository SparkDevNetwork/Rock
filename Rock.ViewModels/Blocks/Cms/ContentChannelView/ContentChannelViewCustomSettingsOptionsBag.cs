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

namespace Rock.ViewModels.Blocks.Cms.ContentChannelView
{
    /// <summary>
    /// The options that populate the Content Channel View block's custom settings editor.
    /// </summary>
    public class ContentChannelViewCustomSettingsOptionsBag
    {
        /// <summary>
        /// Gets or sets the content channels available for selection.
        /// </summary>
        public List<ListItemBag> ContentChannels { get; set; }

        /// <summary>
        /// Gets or sets the item statuses available for selection.
        /// </summary>
        public List<ListItemBag> ContentChannelItemStatuses { get; set; }

        /// <summary>
        /// Gets or sets the active cache tag values available for selection.
        /// </summary>
        public List<ListItemBag> CacheTags { get; set; }

        /// <summary>
        /// Gets or sets the item attribute keys available for binding to the block's Context entity.
        /// </summary>
        public List<ListItemBag> ContextFilterAttributes { get; set; }

        /// <summary>
        /// Gets or sets the channel and item attributes eligible as the meta description source.
        /// </summary>
        public List<ListItemBag> MetaDescriptionAttributes { get; set; }

        /// <summary>
        /// Gets or sets the channel and item Image attributes eligible as the meta image source.
        /// </summary>
        public List<ListItemBag> MetaImageAttributes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the RSS autodiscover link toggle should be shown in the editor.
        /// </summary>
        public bool IsSetRssAutodiscoverLinkVisible { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the personalization filter type selector should be shown in the editor.
        /// </summary>
        public bool IsPersonalizationVisible { get; set; }

        /// <summary>
        /// Gets or sets the personalization filter type choices available for selection.
        /// </summary>
        public List<ListItemBag> PersonalizationFilterTypes { get; set; }

        /// <summary>
        /// Gets or sets the sort field choices available to the order editor.
        /// </summary>
        /// <remarks>
        /// Combines the standard item columns (Title, Priority, Status, StartDateTime, ExpireDateTime, Order) with per-attribute keys prefixed <c>Attribute:</c>.
        /// </remarks>
        public List<ListItemBag> OrderItemsByKeyOptions { get; set; }

        /// <summary>
        /// Gets or sets the sort direction choices paired with <see cref="OrderItemsByKeyOptions"/> in the order editor.
        /// </summary>
        public List<ListItemBag> OrderItemsByValueOptions { get; set; }
    }
}
