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

namespace Rock.ViewModels.Blocks.Engagement.ConnectionTypeDetail
{
    /// <summary>
    /// Minimal bag representation of a Connection Status.
    /// </summary>
    public class ConnectionStatusBag
    {
        /// <summary>
        /// Gets or sets the Guid.
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the highlight color.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is default.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether completing a request requires entering a note.
        /// </summary>
        public bool IsNoteRequiredOnCompletion { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether choosing this Status will set the Request's State to Inactive.
        /// </summary>
        public bool AutoInactivateState { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the number of days added to the calculated due date for a request status.
        /// </summary>
        public int? RequestStatusDueDateOffsetInDays { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the number of days before the due date when a request status is considered "due soon."
        /// </summary>
        public int? RequestStatusDueSoonOffsetInDays { get; set; }

        /// <summary>
        /// Gets or sets a value that when set, automatically moves the request to Future Follow-Up for the specified number of days.
        /// </summary>
        public int? AutoFutureFollowUpPauseInDays { get; set; }

        /// <summary>
        /// Gets or sets the automations.
        /// </summary>
        public List<ConnectionStatusAutomationBag> Automations { get; set; }
    }
}
