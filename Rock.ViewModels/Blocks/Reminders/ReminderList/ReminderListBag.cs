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

namespace Rock.ViewModels.Blocks.Reminders.ReminderList
{
    /// <summary>
    /// A single reminder card view as rendered by the Obsidian list. All
    /// presentation strings are pre-computed server-side so the Vue layer can
    /// stay declarative.
    /// </summary>
    public class ReminderListBag
    {
        /// <summary>
        /// Gets or sets the hashed reminder identifier used for all mutating
        /// block actions (Reschedule, Reassign, Delete, Mark Complete). Never
        /// expose the raw integer Id.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the reminder date as a Rock-timezone <see cref="DateTimeOffset"/>.
        /// Serializes to ISO 8601 with offset on the wire so the Reschedule
        /// modal's <c>DatePicker</c> can round-trip without culture-aware parsing.
        /// </summary>
        public DateTimeOffset? ReminderDate { get; set; }

        /// <summary>
        /// Gets or sets the friendly "Due Today" / "Due 2 Days Ago" /
        /// "Due in 3 Days" label. Pre-computed so the client does not have to
        /// recompute relative-date math.
        /// </summary>
        public string DueLabel { get; set; }

        /// <summary>
        /// Gets or sets the semantic color hint used to style the due label.
        /// One of "danger" (overdue), "warning" (today), or "default" (future).
        /// </summary>
        public string DueLabelColor { get; set; }

        /// <summary>
        /// Gets or sets the reminder note text (displayed as the card description).
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the reminder type name shown in the colored tag on the
        /// right of the card.
        /// </summary>
        public string ReminderTypeName { get; set; }

        /// <summary>
        /// Gets or sets the reminder type highlight color used as the tag dot
        /// background.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets the entity friendly name rendered as the card title
        /// (e.g. "Alex Decker", "A/V Team").
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// Gets or sets the optional resolved URL the entity name links to.
        /// Empty when the entity type does not have a <c>LinkUrlLavaTemplate</c>.
        /// </summary>
        public string EntityUrl { get; set; }

        /// <summary>
        /// Gets or sets the Tabler icon CSS class to render in lieu of a person
        /// avatar (used for Group reminders). Empty for person reminders.
        /// </summary>
        public string EntityIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the person profile photo URL (with size parameters).
        /// Empty when the reminder is not attached to a person.
        /// </summary>
        public string ProfilePhotoUrl { get; set; }

        /// <summary>
        /// Gets or sets the IdKey of the underlying person, used to wire up the
        /// hover popover on the avatar. Empty when the reminder is not attached
        /// to a person.
        /// </summary>
        public string PersonIdKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this reminder is for a Person /
        /// PersonAlias entity. Drives avatar vs. icon rendering on the client.
        /// </summary>
        public bool IsPersonReminder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this reminder is for a Group
        /// entity. Drives avatar vs. icon rendering on the client.
        /// </summary>
        public bool IsGroupReminder { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this reminder has been
        /// completed. Controls the circle vs. checkmark on the left of the card.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this reminder is configured to
        /// renew (RenewPeriodDays set), which drives the recurring clock icon
        /// next to the due label.
        /// </summary>
        public bool IsRenewing { get; set; }

        /// <summary>
        /// Gets or sets the "Every N Days" recurrence text shown on the second
        /// aside line for renewing reminders. Empty for non-renewing reminders.
        /// </summary>
        public string RecurrenceText { get; set; }
    }
}
