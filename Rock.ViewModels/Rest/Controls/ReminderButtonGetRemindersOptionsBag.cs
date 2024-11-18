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
    /// The options that can be passed to the GetReminders API action of
    /// the ReminderButton control.
    /// </summary>
    public class ReminderButtonGetRemindersOptionsBag
    {
        /// <summary>
        /// Gets or sets the Entity Type Guid
        /// </summary>
        public Guid EntityTypeGuid { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the Entity Guid
        /// </summary>
        public Guid EntityGuid { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or sets the Guid for the View Reminders page
        /// </summary>
        public Guid ViewRemindersPage { get; set; } = Guid.Empty;

        /// <summary>
        /// Gets or set the Guid for Reminder edit page
        /// </summary>
        public Guid EditReminderPage { get; set; } = Guid.Empty;
    }
}
