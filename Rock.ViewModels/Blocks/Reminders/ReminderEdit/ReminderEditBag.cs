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

using Rock.ViewModels.Utility;

namespace Rock.ViewModels.Blocks.Reminders.ReminderEdit
{
    /// <summary>
    /// Contains the reminder data used to populate the edit form.
    /// </summary>
    public class ReminderEditBag
    {
        /// <summary>
        /// Gets or sets the date the reminder is scheduled for.
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
        /// Gets or sets the selected reminder type.
        /// </summary>
        public ListItemBag ReminderType { get; set; }

        /// <summary>
        /// Gets or sets the person alias assigned to the reminder.
        /// </summary>
        public ListItemBag PersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the number of days between repeat occurrences.
        /// </summary>
        public int? RenewPeriodDays { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times the reminder should repeat.
        /// </summary>
        public int? RenewMaxCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is an existing reminder
        /// (as opposed to a new one being created). Controls visibility of the
        /// Complete checkbox.
        /// </summary>
        public bool IsExistingReminder { get; set; }

        /// <summary>
        /// Gets or sets the display description of the entity the reminder is for
        /// (e.g., "Ted Decker").
        /// </summary>
        public string EntityDescription { get; set; }
    }
}
