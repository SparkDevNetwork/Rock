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

namespace Rock.ViewModels.Blocks.Reminders.ReminderLinks
{
    /// <summary>
    /// A single row in the Existing Reminders list shown above the Add Reminder form.
    /// </summary>
    public class ExistingReminderBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier used by the client when invoking
        /// MarkReminderComplete / CancelReminderReoccurrence / DeleteReminder.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the reminder date pre-formatted server-side as a short
        /// date string.
        /// </summary>
        public string ReminderDate { get; set; }

        /// <summary>
        /// Gets or sets the reminder note text.
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Gets or sets the reminder type name rendered in the colored tag next to the note.
        /// </summary>
        public string ReminderTypeName { get; set; }

        /// <summary>
        /// Gets or sets the reminder type highlight color used as the tag background.
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the reminder is configured to renew,
        /// which controls the visibility of the recurring clock icon and the
        /// Cancel Reoccurrence action.
        /// </summary>
        public bool IsRenewing { get; set; }
    }
}
