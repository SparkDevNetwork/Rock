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

namespace Rock.ViewModels.Blocks.Reminders.ReminderEdit
{
    /// <summary>
    /// Contains the data sent from the client when saving a reminder.
    /// </summary>
    public class ReminderEditSaveBag
    {
        /// <summary>
        /// Gets or sets the selected reminder type unique identifier.
        /// </summary>
        public Guid ReminderTypeGuid { get; set; }

        /// <summary>
        /// Gets or sets the reminder date as an ISO 8601 string.
        /// </summary>
        public string ReminderDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reminder is complete.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets the note text for the reminder.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the primary alias GUID of the person to assign the reminder to.
        /// </summary>
        public Guid PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the number of days between repeat occurrences.
        /// </summary>
        public int? RenewPeriodDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times the reminder should repeat.
        /// </summary>
        public int? RenewMaxCount { get; set; }
    }
}
