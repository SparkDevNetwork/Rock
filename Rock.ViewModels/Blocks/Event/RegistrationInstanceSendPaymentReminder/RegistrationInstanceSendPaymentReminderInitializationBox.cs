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

using Rock.ViewModels.Core.Grid;

namespace Rock.ViewModels.Blocks.Event.RegistrationInstanceSendPaymentReminder
{
    /// <summary>
    /// Initial state delivered to the Send Payment Reminder block.
    /// </summary>
    public class RegistrationInstanceSendPaymentReminderInitializationBox
    {
        /// <summary>
        /// Gets or sets the grid definition. The grid is populated client-side
        /// from <see cref="Registrations"/>, but the definition still drives
        /// the toolbar action URLs (e.g. Merge Template).
        /// </summary>
        public GridDefinitionBag GridDefinition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the resolved registration
        /// instance has any registrations with an outstanding balance. When
        /// false, the block renders only the empty-state notification.
        /// </summary>
        public bool HasOutstandingBalances { get; set; }

        /// <summary>
        /// Gets or sets the empty-state message shown when no registrations
        /// have an outstanding balance. May contain HTML.
        /// </summary>
        public string EmptyStateMessage { get; set; }

        /// <summary>
        /// Gets or sets the instructions paragraph shown above the grid. May
        /// contain HTML and is pre-rendered on the server because it embeds
        /// the registration template's payment reminder time span.
        /// </summary>
        public string InstructionsHtml { get; set; }

        /// <summary>
        /// Gets or sets the default "From Name" for the reminder email, taken
        /// from the registration template and resolved against a sample
        /// registration's merge fields.
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets the default "From Email" for the reminder email, taken
        /// from the registration template and resolved against a sample
        /// registration's merge fields.
        /// </summary>
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the default subject line for the reminder email, taken
        /// from the registration template and resolved against a sample
        /// registration's merge fields.
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the default message body (Lava source) taken from the
        /// registration template's PaymentReminderEmailTemplate.
        /// </summary>
        public string MessageBody { get; set; }

        /// <summary>
        /// Gets or sets the pre-rendered HTML for the initial preview iframe,
        /// produced by resolving the default message body against a sample
        /// registration.
        /// </summary>
        public string PreviewHtml { get; set; }

        /// <summary>
        /// Gets or sets the list of registrations with outstanding balances,
        /// one row per grid row.
        /// </summary>
        public List<RegistrationBalanceBag> Registrations { get; set; }

        /// <summary>
        /// Gets or sets the registration IdKeys that should be checked by
        /// default when the grid first renders. A registration is pre-selected
        /// only when it has been reminded at least once and the template has a
        /// PaymentReminderTimeSpan, and the days since the last reminder meet
        /// or exceed that threshold. Registrations that have never been
        /// reminded, or instances whose template has no time span configured,
        /// are left unchecked for the user to opt in manually.
        /// </summary>
        public List<string> PreSelectedKeys { get; set; }

        /// <summary>
        /// Gets or sets the URL for the "Return to Registration Instance"
        /// link shown under the success notification. Empty when the
        /// RegistrationInstancePage attribute is not configured.
        /// </summary>
        public string RegistrationInstancePageUrl { get; set; }

        /// <summary>
        /// Gets or sets the registration instance name, used as the title of
        /// the Outstanding Balances grid's Excel export so downloaded files
        /// are scoped to the specific event instance.
        /// </summary>
        public string RegistrationInstanceName { get; set; }
    }
}
