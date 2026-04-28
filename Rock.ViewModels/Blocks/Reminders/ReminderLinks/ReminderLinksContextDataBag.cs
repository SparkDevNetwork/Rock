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

namespace Rock.ViewModels.Blocks.Reminders.ReminderLinks
{
    /// <summary>
    /// The payload returned by the GetReminderLinksData, SaveReminder, and reminder
    /// action block actions. Contains everything the Add Reminder modal needs to
    /// render: reminder types, existing reminders, header text, and refreshed counts.
    /// </summary>
    public class ReminderLinksContextDataBag
    {
        /// <summary>
        /// Gets or sets a value indicating whether the current page context supports
        /// creating a reminder. False when there is no scoped context entity or when
        /// no reminder types exist for the context entity type — causes the Vue
        /// component to hide the Add Reminder menu item.
        /// </summary>
        public bool CanAddReminder { get; set; }

        /// <summary>
        /// Gets or sets the list of reminder types the current person is authorized
        /// to use for the context entity type. Populates the Reminder Type dropdown.
        /// Value is the reminder type Id as a string.
        /// </summary>
        public List<ListItemBag> ReminderTypes { get; set; }

        /// <summary>
        /// Gets or sets the context entity's ToString() value, used as the modal
        /// title (e.g. "Reminder For Ted Decker").
        /// </summary>
        public string EntityDescription { get; set; }

        /// <summary>
        /// Gets or sets up to the top two active existing reminders for the
        /// current person for the context entity.
        /// </summary>
        public List<ExistingReminderBag> ExistingReminders { get; set; }

        /// <summary>
        /// Gets or sets the pre-substituted Existing Reminders header sentence
        /// (e.g. "You currently have reminders for this Person. The most recent 2
        /// are listed below."). Empty when there are no existing reminders.
        /// </summary>
        public string ExistingReminderText { get; set; }

        /// <summary>
        /// Gets or sets the refreshed counts so the bell icon and badges can update
        /// after an action (save, complete, cancel, delete) without a second round trip.
        /// </summary>
        public ReminderLinksCountsBag Counts { get; set; }

        /// <summary>
        /// Gets or sets the current person as the initial value of the Assign Reminder
        /// To person picker. Value is the primary <see cref="Rock.Model.PersonAlias"/>
        /// Guid (matches the value the Obsidian PersonPicker emits and what
        /// SaveReminder expects on <see cref="SaveReminderRequestBag.PersonAliasGuid"/>).
        /// </summary>
        public ListItemBag CurrentPerson { get; set; }
    }
}
