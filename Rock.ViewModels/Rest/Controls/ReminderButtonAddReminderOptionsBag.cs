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
    /// The options that can be passed to the AddReminder API action of
    /// the ReminderButton control
    /// </summary>
    public class ReminderButtonAddReminderOptionsBag
    {
        /// <summary>
        /// The identifier for the entity type of the entity that the reminder pertains to
        /// </summary>
        public Guid EntityTypeGuid { get; set; } = Guid.Empty;

        /// <summary>
        /// The Identifier for the entity that the reminder pertains to
        /// </summary>
        public Guid EntityGuid { get; set; } = Guid.Empty;

        /// <summary>
        /// The identifier for the type of reminder
        /// </summary>
        public Guid ReminderTypeGuid { get; set; } = Guid.Empty;

        /// <summary>
        /// The note of what the reminder is about
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Date of the reminder
        /// </summary>
        public string ReminderDate { get; set; }

        /// <summary>
        /// How often to renew/repeat this reminder in number of days
        /// </summary>
        public int? RenewPeriodDays { get; set; } = null;

        /// <summary>
        /// How many times to renew/repeat this reminder
        /// </summary>
        public int? RenewMaxCount { get; set; } = null;

        /// <summary>
        /// The identifier of the person this reminder is assigned to
        /// </summary>
        public Guid AssignedToGuid { get; set; } = Guid.Empty;
    }
}
