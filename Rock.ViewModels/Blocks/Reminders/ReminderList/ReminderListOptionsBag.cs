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

namespace Rock.ViewModels.Blocks.Reminders.ReminderList
{
    /// <summary>
    /// Static option lists that populate the view-options dropdowns. Generated
    /// once per initialization (or when the underlying data could change due to
    /// a filter that narrows the reminder-type set) and shipped to the client.
    /// </summary>
    public class ReminderListOptionsBag
    {
        /// <summary>
        /// Gets or sets the reminder types the current person is authorized to
        /// see, optionally filtered by the block's Include / Exclude settings.
        /// Value is the reminder type's IdKey; Text is its Name.
        /// </summary>
        public List<ListItemBag> ReminderTypes { get; set; }

        /// <summary>
        /// Gets or sets the entity-type choices for the "Show Reminders For"
        /// dropdown. Always contains "All Entities"; "People" and "Groups"
        /// appear only when the current person has reminders for those types.
        /// Value matches the <c>EntityTypeFilter</c> enum.
        /// </summary>
        public List<ListItemBag> EntityTypeFilters { get; set; }

        /// <summary>
        /// Gets or sets the sort-by choices. Value matches the <c>SortBy</c> enum.
        /// </summary>
        public List<ListItemBag> SortOptions { get; set; }

        /// <summary>
        /// Gets or sets the due-date filter choices (Due, This Week, This Month,
        /// Custom Date Range, All). Value matches the <c>DueFilter</c> enum.
        /// </summary>
        public List<ListItemBag> DueFilterOptions { get; set; }
    }
}
