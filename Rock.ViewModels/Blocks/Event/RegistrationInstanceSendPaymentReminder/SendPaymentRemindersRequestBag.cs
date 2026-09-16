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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceSendPaymentReminder
{
    /// <summary>
    /// Request payload posted when the user clicks Send Reminders.
    /// </summary>
    public class SendPaymentRemindersRequestBag
    {
        /// <summary>
        /// Gets or sets the registration IdKeys selected in the grid. Only
        /// registrations whose key is in this list will receive a reminder.
        /// </summary>
        public List<string> SelectedKeys { get; set; }

        /// <summary>
        /// Gets or sets the "From Name" entered on the form.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the "From Email" entered on the form.
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the subject line entered on the form.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the message body (Lava source) entered on the form.
        /// Merge fields are resolved per-registration at send time.
        /// </summary>
        public string MessageBody { get; set; }
    }
}
