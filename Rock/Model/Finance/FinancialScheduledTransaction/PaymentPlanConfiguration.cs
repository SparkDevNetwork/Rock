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
using System.Collections.Generic;
using Rock.Attribute;
using Rock.Financial;

namespace Rock.Model
{
    /// <summary>
    /// Payment plan configuration.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         <strong>This is an internal API</strong> that supports the Rock
    ///         infrastructure and not subject to the same compatibility standards
    ///         as public APIs. It may be changed or removed without notice in any
    ///         release and should therefore not be directly used in any plug-ins.
    ///     </para>
    /// </remarks>
    [RockInternal( "1.16.6" )]
    public class PaymentPlanConfiguration
    {
        /// <summary>
        /// Gets or sets the amount per payment.
        /// </summary>
        public decimal AmountPerPayment { get; set; }

        /// <summary>
        /// Gets or sets the payment frequency.
        /// </summary>
        public PaymentFrequencyConfiguration PaymentFrequencyConfiguration { get; set; }

        /// <summary>
        /// Gets or sets the allowed payment frequency configurations.
        /// </summary>
        public List<PaymentFrequencyConfiguration> AllowedPaymentFrequencyConfigurations { get; set; }

        /// <summary>
        /// Gets or sets the minimum number of payments allowed for selection.
        /// </summary>
        public int MinNumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the number of payments.
        /// </summary>
        public int NumberOfPayments { get; set; }

        /// <summary>
        /// Gets the planned amount covered by this payment plan configuration.
        /// </summary>
        public decimal PlannedAmount => AmountPerPayment * NumberOfPayments;

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Copies payment plan information to a financial scheduled transaction.
        /// </summary>
        /// <param name="financialScheduledTransaction">The target financial scheduled transaction to which payment details will be copied.</param>
        public void CopyPaymentPlanDetailsTo( FinancialScheduledTransaction financialScheduledTransaction )
        {
            financialScheduledTransaction.StartDate = StartDate;
            financialScheduledTransaction.TransactionFrequencyValueId = PaymentFrequencyConfiguration.PaymentFrequency.Id;
            financialScheduledTransaction.NumberOfPayments = NumberOfPayments;
        }

        /// <summary>
        /// Copies payment plan information to a financial scheduled transaction detail.
        /// </summary>
        /// <param name="financialScheduledTransactionDetail">The target financial scheduled transaction detail to which payment details will be copied.</param>
        public void CopyPaymentPlanDetailsTo( FinancialScheduledTransactionDetail financialScheduledTransactionDetail )
        {
            financialScheduledTransactionDetail.Amount = AmountPerPayment;
        }

        /// <summary>
        /// Copies payment plan information to a payment info.
        /// </summary>
        /// <param name="paymentInfo">The target payment info to which payment details will be copied.</param>
        public void CopyPaymentPlanDetailsTo( PaymentInfo paymentInfo )
        {
            paymentInfo.Amount = AmountPerPayment;
        }

        /// <summary>
        /// Checks if this payment plan configuration is valid for updating an existing payment plan.
        /// </summary>
        /// <param name="validationError">Will contain a message for the first error encountered during validation, if any.</param>
        /// <returns><see langword="true"/> if the payment plan configuration can be used to update an existing payment plan; otherwise, <see langword="false"/>.</returns>
        public bool IsValidForUpdatingExistingPaymentPlan( out string validationError )
        {
            if ( this.StartDate <= RockDateTime.Today )
            {
                validationError = "Start date must be a future date";
                return false;
            }

            if ( this.PaymentFrequencyConfiguration == null )
            {
                validationError = "Payment frequency is required";
                return false;
            }

            if ( this.NumberOfPayments < this.MinNumberOfPayments )
            {
                validationError = $"Number of payments must be at least {this.MinNumberOfPayments}";
                return false;
            }

            if ( this.AmountPerPayment <= 0 )
            {
                validationError = "Amount per payment must be a positive value";
                return false;
            }

            validationError = null;
            return true;
        }
    }
}
