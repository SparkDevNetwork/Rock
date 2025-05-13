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

namespace Rock.ViewModels.Rest.Controls
{
    /// <summary>
    /// Representation of a Reminder retrieved from the GetReminders API action of
    /// the ReminderButton control.
    /// </summary>
    public class ReminderButtonGetRemindersReminderBag
    {
        /// <summary>
        /// The Reminder's Unique Identifier
        /// </summary>
        public Guid Guid { get; set; }

        /// <summary>
        /// The Reminder's Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Date to be reminded
        /// </summary>
        public string ReminderDate { get; set; }

        /// <summary>
        /// Reminder type's representative color
        /// </summary>
        public string HighlightColor { get; set; }

        /// <summary>
        /// Text name of the type of reminder
        /// </summary>
        public string ReminderTypeName { get; set; }

        /// <summary>
        /// Text note regarding what the reminder is about/for
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Whether or not the reminder renews
        /// </summary>
        public bool IsRenewing { get; set; }
    }
}
