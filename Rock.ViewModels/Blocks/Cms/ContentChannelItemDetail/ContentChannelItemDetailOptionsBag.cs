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

using Rock.Model;
using Rock.ViewModels.Core.Grid;
using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Cms.ContentChannelItemDetail
{
    /// <summary>
    /// The additional configuration options for the Content Channel Item Detail block.
    /// </summary>
    public class ContentChannelItemDetailOptionsBag
    {
        /// <summary>
        /// Gets or sets the channel name, rendered as the highlight label in the panel header.
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// Gets or sets whether the error is an authorization failure (renders danger-styled).
        /// Defaults to false (misconfiguration errors render warning-styled).
        /// </summary>
        public bool IsUnauthorizedErrorShown { get; set; }

        /// <summary>
        /// Gets or sets whether the footer Delete button is shown (ShowDeleteButton block setting).
        /// Defaults to false.
        /// </summary>
        public bool IsDeleteButtonShown { get; set; }

        /// <summary>
        /// Gets or sets whether the channel type includes time, selecting date-time vs date-only pickers.
        /// Defaults to true.
        /// </summary>
        public bool IncludeTime { get; set; } = true;

        /// <summary>
        /// Gets or sets the channel type date range mode, which drives date control visibility and labels.
        /// </summary>
        public ContentChannelDateType DateRangeType { get; set; }

        /// <summary>
        /// Gets or sets whether the Sort Priority control is hidden (DisablePriority). Defaults to false.
        /// </summary>
        public bool IsPriorityHidden { get; set; }

        /// <summary>
        /// Gets or sets the header-area event-occurrence labels. Empty for new items and items with no occurrences.
        /// </summary>
        public List<OccurrenceLabelBag> OccurrenceLabels { get; set; }

        /// <summary>
        /// Gets or sets whether the Approval Status toggle is shown. Defaults to false.
        /// </summary>
        public bool IsApprovalToggleShown { get; set; }

        /// <summary>
        /// Gets or sets whether the content channel has the Content Library feature enabled. Defaults to false.
        /// </summary>
        public bool IsContentLibraryEnabled { get; set; }

        /// <summary>
        /// Gets or sets the grouped Content Topic options (Value = Guid, Category = domain name). Empty when library is not enabled.
        /// </summary>
        public List<ListItemBag> ContentTopics { get; set; }

        /// <summary>
        /// Gets or sets the personalization segment options. Null when personalization is not enabled.
        /// </summary>
        public List<ListItemBag> SegmentOptions { get; set; }

        /// <summary>
        /// Gets or sets the request filter options. Null when personalization is not enabled.
        /// </summary>
        public List<ListItemBag> RequestFilterOptions { get; set; }

        /// <summary>
        /// Gets or sets whether the Personalization Options stack is shown. Defaults to false.
        /// </summary>
        public bool IsPersonalizationShown { get; set; }

        /// <summary>
        /// Gets or sets whether the Content Intent picker is shown. Defaults to false.
        /// </summary>
        public bool IsContentIntentShown { get; set; }

        /// <summary>
        /// Gets or sets the Content Intent options (active values plus item's current selections).
        /// Null when the feature is off.
        /// </summary>
        public List<ListItemBag> IntentOptions { get; set; }

        /// <summary>
        /// Gets or sets whether the Tags control is shown. Defaults to false.
        /// </summary>
        public bool IsTaggingShown { get; set; }

        /// <summary>
        /// Gets or sets the tag category Guid filter, or null for no restriction.
        /// </summary>
        public string TagCategoryGuid { get; set; }

        /// <summary>
        /// Gets or sets the grid column definition for the child-items grid.
        /// </summary>
        public GridDefinitionBag ChildItemsGridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the grid column definition for the parent-items grid.
        /// </summary>
        public GridDefinitionBag ParentItemsGridDefinition { get; set; }

        /// <summary>
        /// Gets or sets the child channels the current person may add items under, VIEW filtered and ordered by name.
        /// </summary>
        public List<ListItemBag> AddChildChannelOptions { get; set; }
    }
}
