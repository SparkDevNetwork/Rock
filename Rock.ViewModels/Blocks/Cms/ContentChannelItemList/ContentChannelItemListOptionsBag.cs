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

using Rock.Model;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemList
{
    /// <summary>
    /// The additional configuration options for the Content Channel Item List block.
    /// </summary>
    public class ContentChannelItemListOptionsBag
    {
        /// <summary>
        /// Gets or sets the name of the content item.
        /// </summary>
        public string ContentItemName { get; set; }

        /// <summary>
        /// Gets or sets whether to include the time in the date/time fields.
        /// </summary>
        public bool IncludeTime { get; set; }

        /// <summary>
        /// Gets or sets whether the items are manually ordered.
        /// </summary>
        public bool IsManuallyOrdered { get; set; }

        /// <summary>
        /// Gets or sets whether the content library is enabled.
        /// </summary>
        public bool IsContentLibraryEnabled { get; set; }

        /// <summary>
        /// Gets or sets the date type for the content channel.
        /// </summary>
        public ContentChannelDateType DateType { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the content channel.
        /// </summary>
        public int ContentChannelId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the library license.
        /// </summary>
        public Guid LibraryLicenseGuid { get; set; }

        /// <summary>
        /// Gets or sets the name of the library license.
        /// </summary>
        public string LibraryLicenseName { get; set; }

        /// <summary>
        /// Gets or sets whether to show the filter options.
        /// </summary>
        public bool ShowFilters { get; set; }

        /// <summary>
        /// Gets or sets whether to show the reorder column.
        /// </summary>
        public bool ShowReorderColumn { get; set; }

        /// <summary>
        /// Gets or sets whether to show the start date/time column.
        /// </summary>
        public bool ShowStartDateTimeColumn { get; set; }

        /// <summary>
        /// Gets or sets whether to show the expiration date/time column.
        /// </summary>
        public bool ShowExpireDateTimeColumn { get; set; }

        /// <summary>
        /// Gets or sets whether to show the total views columns.
        /// </summary>
        public bool ShowTotalViewsColumns { get; set; }

        /// <summary>
        /// Gets or sets whether to show the priority column.
        /// </summary>
        public bool ShowPriorityColumn { get; set; }

        /// <summary>
        /// Gets or sets whether to show the occurrences column.
        /// </summary>
        public bool ShowOccurrencesColumn { get; set; }

        /// <summary>
        /// Gets or sets whether to show the status column.
        /// </summary>
        public bool ShowStatusColumn { get; set; }

        /// <summary>
        /// Gets or sets whether to show the Item URL column.
        /// </summary>
        public bool ShowItemUrlColumn { get; set; }

        /// <summary>
        /// Gets or sets whether to show the linked media column.
        /// </summary>
        public bool ShowLinkedMediaColumn { get; set; }

        /// <summary>
        /// Gets or sets whether to show the security column.
        /// </summary>
        public bool ShowSecurityColumn { get; set; }
    }
}
