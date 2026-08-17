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

using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelNavigation
{
    /// <summary>
    /// The response data for the grid in the Content Channel Navigation block.
    /// Contains both the grid definition and data since columns change per channel.
    /// </summary>
    public class ContentChannelNavigationGetGridDataResponseBag
    {
        /// <summary>
        /// Gets or sets the grid data containing the rows of content channel items.
        /// </summary>
        public GridDataBag GridData { get; set; }

        /// <summary>
        /// Gets or sets the grid definition describing the columns for the selected channel.
        /// This changes dynamically based on the channel's type configuration.
        /// </summary>
        public GridDefinitionBag GridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the display name of the selected content channel.
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether item reordering is currently enabled.
        /// True when items are manually ordered and no filters are active.
        /// </summary>
        public bool IsReorderEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person can edit items in this channel.
        /// Controls visibility of the add button, delete column, and security column.
        /// </summary>
        public bool CanEdit { get; set; }

        /// <summary>
        /// Gets or sets the URL for adding a new item to this channel.
        /// </summary>
        public string AddItemUrl { get; set; }

        /// <summary>
        /// Gets or sets the attribute definitions for filters in the grid settings modal.
        /// These change per channel based on attributes qualified by channel type or channel ID.
        /// </summary>
        public List<PublicAttributeBag> AttributeFilters { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the date columns should
        /// include time in addition to the date. When false, only the date
        /// portion is displayed.
        /// </summary>
        public bool IsIncludeTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any items have a future
        /// start date (are scheduled). Controls the date status icon column.
        /// </summary>
        public bool HasScheduledItems { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any items have event
        /// occurrences. Controls the event occurrences column.
        /// </summary>
        public bool HasEventOccurrences { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether any user-applied filters
        /// are active for the current grid data request. This is determined
        /// server-side after converting filter values to their internal
        /// representation, so it is the authoritative source for whether
        /// the grid settings indicator should appear active.
        /// </summary>
        public bool HasActiveFilters { get; set; }
    }
}
