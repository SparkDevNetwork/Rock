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

namespace Rock.ViewModels.Blocks.Event.RegistrationEntry
{
    /// <summary>
    /// Contains the payment plan information for the Registration Entry block.
    /// </summary>
    public class RegistrationEntryPaymentPlanBag
    {
        /// <summary>
        /// Gets or sets the financial scheduled transaction unique identifier.
        /// </summary>
        public Guid FinancialScheduledTransactionGuid { get; set; }

        /// <summary>
        /// Gets or sets the transaction frequency unique identifier.
        /// </summary>
        /// <value>
        /// The transaction frequency unique identifier.
        /// </value>
        public Guid TransactionFrequencyGuid { get; set; }

        /// <summary>
        /// Gets or sets the amount to pay per payment.
        /// </summary>
        /// <value>
        /// The amount to pay per payment.
        /// </value>
        public decimal AmountPerPayment { get; set; }

        /// <summary>
        /// Gets or sets the number of payments.
        /// </summary>
        /// <value>
        /// The number of payments.
        /// </value>
        public int TotalNumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the start date of the recurring payments.
        /// </summary>
        /// <value>
        /// The start date of the recurring payments.
        /// </value>
        public DateTimeOffset StartDate { get; set; }

        /// <summary>
        /// Gets or sets the user-friendly text for the transaction frequency.
        /// </summary>
        public string TransactionFrequencyText { get; set; }

        /// <summary>
        /// Gets or sets the total amount that this payment plan will cover.
        /// </summary>
        public decimal TotalPlannedAmount { get; set; }

        /// <summary>
        /// Gets or sets the remaining amount that has not been paid for this payment plan.
        /// </summary>
        public decimal RemainingPlannedAmount { get; set; }

        /// <summary>
        /// Gets or sets the remaining number of payments for this payment plan.
        /// </summary>
        public int RemainingNumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the number of payments that have already been processed for this payment plan.
        /// </summary>
        public int ProcessedNumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the amount that has already been processed for this payment plan.
        /// </summary>
        public decimal ProcessedPlannedAmount { get; set; }
    }
}
