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

namespace Rock.ViewModels.Blocks.Finance.PublicScheduledTransactionList
{
    /// <summary>
    /// The top-level bag returned by the Public Scheduled Transaction List block.
    /// </summary>
    public class PublicScheduledTransactionListBag
    {
        /// <summary>
        /// Gets or sets the list of scheduled-transaction items the current
        /// person owns (or is on the giving-group / business owner side of).
        /// Empty when the viewer is anonymous or has no active schedules.
        /// </summary>
        public List<ScheduledTransactionItemBag> Items { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to surface the empty-state
        /// message. False for anonymous viewers (those see a silent block,
        /// matching the original WebForms behavior).
        /// </summary>
        public bool IsEmpty { get; set; }

        /// <summary>
        /// Gets or sets the empty-state message, already formatted using the
        /// block's Transaction Label setting (e.g. "No gifts currently exist.").
        /// </summary>
        public string EmptyMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Add button should appear.
        /// True only when the New Scheduled Transaction Page attribute is set.
        /// </summary>
        public bool ShowAddButton { get; set; }

        /// <summary>
        /// Gets or sets the Add button label, formatted as "Create New {Transaction Label}".
        /// </summary>
        public string AddButtonText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to render the block-header
        /// section (icon + title + description) above the list of scheduled
        /// transactions. Driven by the Show Block Header attribute.
        /// </summary>
        public bool ShowBlockHeader { get; set; }

        /// <summary>
        /// Gets or sets the block-header title displayed above the list when
        /// ShowBlockHeader is enabled. Default "Manage Giving Profiles".
        /// </summary>
        public string BlockHeaderTitle { get; set; }

        /// <summary>
        /// Gets or sets the block-header supporting text displayed below the
        /// title when ShowBlockHeader is enabled.
        /// </summary>
        public string BlockHeaderDescription { get; set; }

        /// <summary>
        /// Gets or sets the CSS class of the icon displayed in the block-header
        /// tile when ShowBlockHeader is enabled (e.g. "ti ti-cash").
        /// </summary>
        public string BlockHeaderIconCssClass { get; set; }
    }
}
