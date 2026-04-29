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

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemView
{
    /// <summary>
    /// The options used to populate the custom settings UI for the <see cref="Rock.Blocks.Cms.ContentChannelItemView"/> block.
    /// </summary>
    public class ContentChannelItemViewCustomSettingsOptionsBag
    {
        /// <summary>
        /// Gets or sets the list of content channels that can be configured for this block.
        /// </summary>
        public List<ListItemBag> ContentChannels { get; set; }

        /// <summary>
        /// Gets or sets the list of selectable content channel item statuses.
        /// </summary>
        public List<ListItemBag> ContentChannelItemStatuses { get; set; }

        /// <summary>
        /// Gets or sets the list of available cache tags.
        /// </summary>
        public List<ListItemBag> CacheTags { get; set; }

        /// <summary>
        /// Gets or sets the list of channel and item attributes that can be selected for the meta description.
        /// </summary>
        public List<ListItemBag> MetaDescriptionAttributes { get; set; }

        /// <summary>
        /// Gets or sets the list of channel and item image attributes that can be selected for og:image and twitter:image meta tags.
        /// </summary>
        public List<ListItemBag> ImageAttributes { get; set; }

        /// <summary>
        /// Gets or sets the list of channel and item attributes that can be selected for og:title and twitter:title meta tags.
        /// </summary>
        public List<ListItemBag> TitleAttributes { get; set; }

        /// <summary>
        /// Gets or sets the list of channel and item attributes that can be selected for og:description and twitter:description meta tags.
        /// </summary>
        public List<ListItemBag> DescriptionAttributes { get; set; }

        /// <summary>
        /// Gets or sets the list of supported Open Graph object type values.
        /// </summary>
        public List<ListItemBag> OpenGraphTypes { get; set; }

        /// <summary>
        /// Gets or sets the list of supported Twitter card type values.
        /// </summary>
        public List<ListItemBag> TwitterCards { get; set; }

        /// <summary>
        /// Gets or sets the list of supported launch workflow condition values.
        /// </summary>
        public List<ListItemBag> LaunchWorkflowConditions { get; set; }
    }
}
