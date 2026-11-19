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

namespace Rock.ViewModels.Blocks.Event.RegistrationDetail
{
    /// <summary>
    /// The summary of the active payment plan of a registration.
    /// </summary>
    public class RegistrationPaymentPlanSummaryBag
    {
        /// <summary>
        /// Gets or sets the label of the payment amount, which includes the frequency.
        /// </summary>
        public string PaymentAmountLabel { get; set; }

        /// <summary>
        /// Gets or sets the formatted payment amount and number of payments.
        /// </summary>
        public string PaymentAmountText { get; set; }

        /// <summary>
        /// Gets or sets the formatted next payment date, or null when there is no next payment.
        /// </summary>
        public string NextPaymentDateText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the current person can change or delete the payment plan.
        /// </summary>
        public bool IsEditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the change button is disabled.
        /// </summary>
        public bool IsChangeDisabled { get; set; }

        /// <summary>
        /// Gets or sets the reason the change button is disabled, or null when there is no reason to show.
        /// </summary>
        public string ChangeDisabledReason { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the delete button is disabled.
        /// </summary>
        public bool IsDeleteDisabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the remaining planned amount does
        /// not match the balance due.
        /// </summary>
        public bool IsAmountMismatch { get; set; }
    }
}
