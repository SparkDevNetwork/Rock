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

namespace Rock.ViewModels.Blocks.Finance.PublicScheduledTransactionList
{
    /// <summary>
    /// One row in the Public Scheduled Transaction List block. Rendered as a
    /// DisplayCard on the client. After a successful or failed Cancel, the
    /// server returns a replacement bag with AlertMessage / AlertType populated
    /// (and the DisplayCard fields left blank); the client swaps the row's
    /// DisplayCard for an inline alert so the row keeps its place in the layout.
    /// </summary>
    public class ScheduledTransactionItemBag
    {
        /// <summary>
        /// Gets or sets the hashed identifier of the scheduled transaction.
        /// Used as the v-for key and as the argument to the Cancel block action.
        /// </summary>
        public string IdKey { get; set; }

        /// <summary>
        /// Gets or sets the CSS class of the DisplayCard icon
        /// (e.g. "ti ti-credit-card" or "ti ti-building-bank").
        /// </summary>
        public string IconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the DisplayCard title (e.g. "$125.00 (Contribution)").
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the DisplayCard description
        /// (e.g. "Visa Ending in 6789 • Expires 11/28").
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the human-formatted next-payment line displayed in the
        /// middle section of the DisplayCard (e.g. "Next on June 3, 2026").
        /// Null when the schedule has no NextPaymentDate.
        /// </summary>
        public string NextPaymentText { get; set; }

        /// <summary>
        /// Gets or sets the authorized person's full name displayed in the
        /// middle section of the DisplayCard (e.g. "Ted Decker").
        /// </summary>
        public string PersonName { get; set; }

        /// <summary>
        /// Gets or sets the CSS class of the icon used for the frequency pill
        /// (e.g. "ti ti-refresh" for recurring, "ti ti-gift" for one-time).
        /// </summary>
        public string FrequencyIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the frequency pill label (e.g. "Every Week", "One-Time").
        /// </summary>
        public string FrequencyLabel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Edit (or Transfer)
        /// button should appear for this row. Hidden for event-registration
        /// transactions and when the relevant edit-page attribute is empty.
        /// </summary>
        public bool ShowEditButton { get; set; }

        /// <summary>
        /// Gets or sets the Edit button tooltip label. Either the literal "Edit"
        /// or the value of the Transfer Button Label setting when the row's
        /// gateway differs from the configured Transfer-To gateway.
        /// </summary>
        public string EditButtonText { get; set; }

        /// <summary>
        /// Gets or sets the CSS class of the icon shown on the Edit button.
        /// A pencil icon on regular Edit rows, or an exchange-arrows icon
        /// when the row routes through the Transfer flow.
        /// </summary>
        public string EditIconCssClass { get; set; }

        /// <summary>
        /// Gets or sets the resolved Edit-button target URL. Already includes
        /// the ScheduledTransactionGuid query parameter and (when applicable)
        /// the transfer=true flag, so the Vue side does not need to know
        /// about the hosted-vs-classic-vs-transfer routing rules.
        /// </summary>
        public string EditUrl { get; set; }

        /// <summary>
        /// Gets or sets the inline-alert message to display in place of the
        /// DisplayCard for this row. Populated only after a Cancel; null while
        /// the row is showing its normal DisplayCard content.
        /// </summary>
        public string AlertMessage { get; set; }

        /// <summary>
        /// Gets or sets the alert type ("success" or "danger") that pairs with
        /// AlertMessage. Null while the row is showing its normal DisplayCard content.
        /// </summary>
        public string AlertType { get; set; }
    }
}
