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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentCollectionDetail
{
    /// <summary>
    /// Class ContentCollectionBag.
    /// Implements the <see cref="Rock.ViewModels.Utility.EntityBagBase" />
    /// </summary>
    /// <seealso cref="Rock.ViewModels.Utility.EntityBagBase" />
    public class ContentCollectionBag : EntityBagBase
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether personalization request filters should be enabled.
        /// </summary>
        public bool EnableRequestFilters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether personalization segments should be enabled.
        /// </summary>
        public bool EnableSegments { get; set; }

        /// <summary>
        /// Gets or sets the filter settings.
        /// </summary>
        public FilterSettingsBag FilterSettings { get; set; }

        /// <summary>
        /// Gets or sets the last index date time.
        /// </summary>
        public DateTime? LastIndexDateTime { get; set; }

        /// <summary>
        /// Gets or sets the last index item count.
        /// </summary>
        public int? LastIndexItemCount { get; set; }

        /// <summary>
        /// Gets or sets the collection key.
        /// </summary>
        public string CollectionKey { get; set; }

        /// <summary>
        /// Gets or sets the name of the ContentCollection. This property is required.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether trending is enabled.
        /// </summary>
        public bool TrendingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the trending max items. This property is required.
        /// </summary>
        public int TrendingMaxItems { get; set; }

        /// <summary>
        /// Gets or sets the trending window day. This property is required.
        /// </summary>
        public int TrendingWindowDay { get; set; }

        /// <summary>
        /// Gets or sets the trending gravity to apply more weight to items that are newer.
        /// </summary>
        public decimal TrendingGravity { get; set; }

        /// <summary>
        /// Gets or sets the sources that are currently configured for this
        /// collection. This value is not used during save operations.
        /// </summary>
        public List<ContentSourceBag> Sources { get; set; }
    }
}
