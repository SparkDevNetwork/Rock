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

namespace Rock.Model.CMS.ContentChannelItem.Options
{
    /// <summary>
    /// The options for downloading a Content Library Item as a Content Channel Item.
    /// </summary>
    public class ContentLibraryItemDownloadOptions
    {
        /// <summary>
        /// Gets or sets the unique identifier of the Content Library item to download. (Required)
        /// </summary>
        /// <value>
        /// The content library item unique identifier.
        /// </value>
        public Guid ContentLibraryItemGuidToDownload { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the Content Channel into which the item will be downloaded. (Required)
        /// </summary>
        /// <value>
        /// The content channel unique identifier.
        /// </value>
        public Guid DownloadIntoContentChannelGuid { get; set; }

        /// <summary>
        /// Gets or sets the current person who is downloading the item.
        /// </summary>
        /// <value>
        /// The current person.
        /// </value>
        public Person CurrentPersonPerformingDownload { get; set; }

        /// <summary>
        /// Gets or sets the Content Channel Item status that will be used for the downloaded item.
        /// </summary>
        /// <value>
        /// The content channel item status.
        /// </value>
        public ContentChannelItemStatus? ContentChannelItemStatusOverride { get; set; }

        /// <summary>
        /// Gets or sets the Content Channel Item start date that will be used for the downloaded item.
        /// </summary>
        /// <value>
        /// The content channel item start date.
        /// </value>
        public DateTime? ContentChannelItemStartDateOverride { get; set; }

        /// <summary>
        /// Gets or sets the Content Channel Item end date that will be used for the downloaded item.
        /// </summary>
        /// <value>
        /// The content channel item end date.
        /// </value>
        public DateTime? ContentChannelItemEndDateOverride { get; set; }
    }
}
