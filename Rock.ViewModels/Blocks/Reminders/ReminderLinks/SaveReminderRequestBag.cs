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

namespace Rock.ViewModels.Blocks.Reminders.ReminderLinks
{
    /// <summary>
    /// The payload the client sends to the SaveReminder block action when the
    /// user submits the Add Reminder form.
    /// </summary>
    public class SaveReminderRequestBag
    {
        /// <summary>
        /// Gets or sets the selected reminder type identifier. The server validates
        /// that this id is in the allow-list returned by GetReminderLinksData to
        /// prevent a forged value from bypassing the Reminder Type authorization check.
        /// </summary>
        public int ReminderTypeId { get; set; }

        /// <summary>
        /// Gets or sets the reminder date. Required on the client; the server also
        /// guards against a null value.
        /// </summary>
        public DateTime? ReminderDate { get; set; }

        /// <summary>
        /// Gets or sets the optional free-text note captured from the Note field.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the person alias guid chosen in the Assign Reminder To
        /// picker. This is the Rock.Model.PersonAlias.Guid of the
        /// person's primary alias (the value the Obsidian PersonPicker emits).
        /// Null / empty means the reminder should be assigned to the current person.
        /// </summary>
        public Guid? PersonAliasGuid { get; set; }

        /// <summary>
        /// Gets or sets the optional number of days between repeats (the Repeat
        /// Every field). Null disables repeating.
        /// </summary>
        public int? RepeatDays { get; set; }

        /// <summary>
        /// Gets or sets the optional maximum number of times the reminder should
        /// repeat (the Number of Times to Repeat field). Null repeats indefinitely
        /// when <see cref="RepeatDays"/> is provided.
        /// </summary>
        public int? RepeatTimes { get; set; }
    }
}
