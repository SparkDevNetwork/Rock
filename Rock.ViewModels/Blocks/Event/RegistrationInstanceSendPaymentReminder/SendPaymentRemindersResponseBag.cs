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

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceSendPaymentReminder
{
    /// <summary>
    /// Response payload returned after the Send Reminders block action runs.
    /// </summary>
    public class SendPaymentRemindersResponseBag
    {
        /// <summary>
        /// Gets or sets the number of registrations that received a reminder
        /// email. Registrations without a confirmation email are skipped and
        /// not included in this count.
        /// </summary>
        public int SentCount { get; set; }

        /// <summary>
        /// Gets or sets the human-readable summary string to render in the
        /// success notification (for example, "Payment reminders have been
        /// sent to 3 individuals.").
        /// </summary>
        public string Message { get; set; }
    }
}
