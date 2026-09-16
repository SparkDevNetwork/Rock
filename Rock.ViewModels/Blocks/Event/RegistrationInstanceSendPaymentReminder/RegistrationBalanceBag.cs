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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceSendPaymentReminder
{
    /// <summary>
    /// A single registration row in the outstanding balance grid.
    /// </summary>
    public class RegistrationBalanceBag
    {
        /// <summary>
        /// Gets or sets the registration's IdKey, used both as the grid row
        /// key and as the handle for identifying selected registrations when
        /// the user clicks Send.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the "Last Name, First Name" display string used by
        /// the grid Name column. Pre-composed on the server so that sorting
        /// by the visible column matches the original WebForms block.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the email address the reminder will be delivered to,
        /// pulled from the registration's ConfirmationEmail.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the date the registration was created, used by the
        /// Date Registered grid column.
        /// </summary>
        public DateTime? RegisteredDateTime { get; set; }

        /// <summary>
        /// Gets or sets the server-formatted text for the Last Reminder Sent
        /// column ("Today", "N days", or "Unknown"). Matches the Humanizer
        /// quantity used by the original WebForms block.
        /// </summary>
        public string LastReminderText { get; set; }

        /// <summary>
        /// Gets or sets the raw last-reminder timestamp, retained for tooltip
        /// use and for any client-side sorting that needs the real date
        /// rather than the formatted text.
        /// </summary>
        public DateTime? LastReminderDateTime { get; set; }

        /// <summary>
        /// Gets or sets the registration's total cost.
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the amount already paid against this registration.
        /// </summary>
        public decimal Paid { get; set; }

        /// <summary>
        /// Gets or sets the remaining balance due on this registration,
        /// after adjusting for any active payment plan.
        /// </summary>
        public decimal BalanceDue { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the row should render with
        /// the grid's standard "active" styling. Set to false when the
        /// registration was reminded within the registration template's
        /// PaymentReminderTimeSpan, triggering the grid's
        /// <c>markInactiveRows</c> greyout. Recently-reminded rows remain
        /// selectable — the distinction is purely visual.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
