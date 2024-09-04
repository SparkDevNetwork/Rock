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
using Rock.Attribute;

namespace Rock.Model
{
    /// <summary>
    /// A payment plan.
    /// </summary>
    /// <remarks>
    /// This only contains data for this payment plan configuration; data from previous configurations (number of payments, amount per payment, etc) are excluded.
    /// <para>
    /// <strong>This is an internal API</strong> that supports the Rock
    /// infrastructure and not subject to the same compatibility standards
    /// as public APIs. It may be changed or removed without notice in any
    /// release and should therefore not be directly used in any plug-ins.
    /// </para>
    /// </remarks>
    [RockInternal( "1.16.6" )]
    public sealed class PaymentPlan
    {
        /// <summary>
        /// Gets or sets the financial scheduled transaction unique identifier.
        /// </summary>
        public Guid FinancialScheduledTransactionGuid { get; set; }

        /// <summary>
        /// Gets or sets the amount per payment.
        /// </summary>
        public decimal AmountPerPayment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating if this payment plan is active.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the number of payments (paid and unpaid).
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the number of payments that have already been processed by the payment gateway.
        /// </summary>
        /// <remarks>
        /// This value may be inaccurate if payments haven't been synced from the payment gateway to Rock.
        /// <para>
        /// This only contains data for this payment plan configuration; data from previous configurations (number of payments, amount per payment, etc) are excluded.
        /// </para>
        /// </remarks>
        public int NumberOfPaymentsProcessed { get; set; }

        /// <summary>
        /// Gets or sets the number of payments that still need to be processed by the payment gateway.
        /// </summary>
        /// <remarks>
        ///     This value may be inaccurate if payments haven't been synced from the payment gateway to Rock.
        ///     <para>
        ///         This only contains data for this payment plan configuration; data from previous configurations (number of payments, amount per payment, etc) are excluded.
        ///     </para>
        /// </remarks>
        public int NumberOfPaymentsRemaining { get; set; }

        /// <summary>
        /// The total amount covered by this payment plan.
        /// </summary>
        /// <remarks>
        /// This value may be inaccurate if payments haven't been synced from the payment gateway to Rock.
        /// </remarks>
        public decimal PlannedAmount { get; set; }

        /// <summary>
        /// The amount that has already been processed by the payment gateway.
        /// </summary>
        /// <remarks>
        ///     This value may be inaccurate if payments haven't been synced from the payment gateway to Rock.
        ///     <para>
        ///         This only contains data for this payment plan configuration; data from previous configurations (number of payments, amount per payment, etc) are excluded.
        ///     </para>
        /// </remarks>
        public decimal PlannedAmountProcessed { get; set; }

        /// <summary>
        /// The amount that has yet to be processed by the payment gateway.
        /// </summary>
        /// <remarks>
        ///     This value may be inaccurate if payments haven't been synced from the payment gateway to Rock.
        ///     <para>
        ///         This only contains data for this payment plan configuration; data from previous configurations (number of payments, amount per payment, etc) are excluded.
        ///     </para>
        /// </remarks>
        public decimal PlannedAmountRemaining { get; set; }

        /// <summary>
        /// Gets or sets the start date for the payment plan.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the payment frequency unique identifier.
        /// </summary>
        public Guid TransactionFrequencyValueGuid { get; set; }
    }
}