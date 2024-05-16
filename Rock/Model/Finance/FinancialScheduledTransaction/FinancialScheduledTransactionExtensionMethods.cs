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

using System.Data.Entity;
using System.Linq;
using Rock.Attribute;

namespace Rock.Model
{
    /// <summary>
    /// Extension method class for Registration
    /// </summary>
    public static partial class FinancialScheduledTransactionExtensionMethods
    {
        /// <summary>
        /// Gets a query containing financial scheduled transactions and their payment plan representations.
        /// </summary>
        /// <remarks>
        /// <strong>This is an internal API</strong> that supports the Rock
        /// infrastructure and not subject to the same compatibility standards
        /// as public APIs. It may be changed or removed without notice in any
        /// release and should therefore not be directly used in any plug-ins.
        /// </remarks>
        /// <param name="financialScheduledTransactionQuery">The source query.</param>
        /// <returns>An <see cref="IQueryable"/> of <see cref="FinancialScheduledTransaction"/> and their <see cref="PaymentPlan"/> representations.</returns>
        [RockInternal( "1.16.6" )]
        public static IQueryable<FinancialScheduledTransactionPaymentPlanPair> SelectPaymentPlanPairs( this IQueryable<FinancialScheduledTransaction> financialScheduledTransactionQuery )
        {
            // IMPORTANT: Any updates to this logic should also be made to
            // - GetPaymentPlan( this FinancialScheduledTransaction financialScheduledTransaction )
            // - IncludePaymentPlanDependencies( this IQueryable<FinancialScheduledTransaction> financialScheduledTransactionQuery ).

            return financialScheduledTransactionQuery
                // Get the data to build the run-time payment plans...
                .Select( financialScheduledTransaction => new
                {
                    Source = financialScheduledTransaction,
                    PaymentPlanData = financialScheduledTransaction == null
                        ? null
                        : new
                        {
                            FinancialScheduledTransactionGuid = financialScheduledTransaction.Guid,
                            financialScheduledTransaction.IsActive,

                            // There should only be one scheduled transaction detail for a payment plan,
                            // but this will account for future cases where there may be multiple schedules.
                            AmountPerPayment = financialScheduledTransaction.ScheduledTransactionDetails.Sum( d => d.Amount ),
                            NumberOfPayments = financialScheduledTransaction.NumberOfPayments ?? 0,

                            // The number of payments, amount per payment, and start date can be updated by administrators.
                            // When calculating the number of payments processed for the latest payment plan configuration,
                            // only consider transactions that were created after the latest payment plan's start date.
                            ProcessedNumberOfPayments = financialScheduledTransaction.Transactions.Count( t => t.TransactionDateTime.HasValue && t.TransactionDateTime.Value > financialScheduledTransaction.StartDate ),

                            financialScheduledTransaction.StartDate,
                            TransactionFrequencyValueGuid = financialScheduledTransaction.TransactionFrequencyValue.Guid,
                        }
                } )
                // ...then build the payment plans.
                .Select( pair => new FinancialScheduledTransactionPaymentPlanPair
                {
                    FinancialScheduledTransaction = pair.Source,
                    PaymentPlan = pair.PaymentPlanData == null
                        ? null
                        : new PaymentPlan
                        {
                            FinancialScheduledTransactionGuid = pair.PaymentPlanData.FinancialScheduledTransactionGuid,
                            IsActive = pair.PaymentPlanData.IsActive,

                            StartDate = pair.PaymentPlanData.StartDate,
                            TransactionFrequencyValueGuid = pair.PaymentPlanData.TransactionFrequencyValueGuid,
                            AmountPerPayment = pair.PaymentPlanData.AmountPerPayment,

                            NumberOfPayments = pair.PaymentPlanData.NumberOfPayments,
                            NumberOfPaymentsProcessed = pair.PaymentPlanData.ProcessedNumberOfPayments,
                            NumberOfPaymentsRemaining = pair.PaymentPlanData.NumberOfPayments - pair.PaymentPlanData.ProcessedNumberOfPayments,

                            PlannedAmount = pair.PaymentPlanData.AmountPerPayment * pair.PaymentPlanData.NumberOfPayments,
                            PlannedAmountProcessed = pair.PaymentPlanData.AmountPerPayment * pair.PaymentPlanData.ProcessedNumberOfPayments,
                            PlannedAmountRemaining = pair.PaymentPlanData.AmountPerPayment * ( pair.PaymentPlanData.NumberOfPayments - pair.PaymentPlanData.ProcessedNumberOfPayments ),
                        }
                } );
        }
        
        /// <summary>
        /// Gets a payment plan representation of a <see cref="FinancialScheduledTransaction"/>.
        /// </summary>
        /// <remarks>
        /// When possible, use <see cref="IncludePaymentPlanDependencies(IQueryable{Rock.Model.FinancialScheduledTransaction})"/>
        /// before accessing this to ensure dependent data is eagerly-loaded.
        /// <para>
        /// Prefer <see cref="SelectPaymentPlanPairs(IQueryable{FinancialScheduledTransaction})"/> if retrieving payment plan pairs in a query.
        /// </para>
        /// <para><strong>This is an internal API</strong> that supports the Rock
        /// infrastructure and not subject to the same compatibility standards
        /// as public APIs. It may be changed or removed without notice in any
        /// release and should therefore not be directly used in any plug-ins.
        /// </para>
        /// </remarks>
        /// <param name="financialScheduledTransaction">The financial scheduled transaction for which to get payment plan data.</param>
        /// <returns>
        /// The payment plan representation of a <see cref="FinancialScheduledTransaction"/>.
        /// </returns>
        [RockInternal( "1.16.6" )]
        public static PaymentPlan GetPaymentPlan( this FinancialScheduledTransaction financialScheduledTransaction )
        {
            // IMPORTANT: Any updates to this logic should also be made to
            // - SelectPaymentPlanPairs( this IQueryable<FinancialScheduledTransaction> financialTransactionsQuery )
            // - IncludePaymentPlanDependencies( this IQueryable<FinancialScheduledTransaction> financialScheduledTransactionQuery ).

            if ( financialScheduledTransaction == null )
            {
                return null;
            }

            var paymentPlanData = new
            {
                FinancialScheduledTransactionGuid = financialScheduledTransaction.Guid,
                financialScheduledTransaction.IsActive,

                // There should only be one scheduled transaction detail for a payment plan,
                // but this will account for future cases where there may be multiple schedules.
                AmountPerPayment = financialScheduledTransaction.ScheduledTransactionDetails.Sum( d => d.Amount ),
                NumberOfPayments = financialScheduledTransaction.NumberOfPayments ?? 0,

                // The number of payments, amount per payment, and start date can be updated by administrators.
                // When calculating the number of payments processed for the latest payment plan configuration,
                // only consider transactions that were created after the latest payment plan's start date.
                ProcessedNumberOfPayments = financialScheduledTransaction.Transactions.Count( t => t.TransactionDateTime.HasValue && t.TransactionDateTime.Value > financialScheduledTransaction.StartDate ),

                financialScheduledTransaction.StartDate,
                TransactionFrequencyValueGuid = financialScheduledTransaction.TransactionFrequencyValue.Guid,
            };

            return new PaymentPlan
            {
                FinancialScheduledTransactionGuid = paymentPlanData.FinancialScheduledTransactionGuid,
                IsActive = paymentPlanData.IsActive,

                StartDate = paymentPlanData.StartDate,
                TransactionFrequencyValueGuid = paymentPlanData.TransactionFrequencyValueGuid,
                AmountPerPayment = paymentPlanData.AmountPerPayment,

                NumberOfPayments = paymentPlanData.NumberOfPayments,
                NumberOfPaymentsProcessed = paymentPlanData.ProcessedNumberOfPayments,
                NumberOfPaymentsRemaining = paymentPlanData.NumberOfPayments - paymentPlanData.ProcessedNumberOfPayments,

                PlannedAmount = paymentPlanData.AmountPerPayment * paymentPlanData.NumberOfPayments,
                PlannedAmountProcessed = paymentPlanData.AmountPerPayment * paymentPlanData.ProcessedNumberOfPayments,
                PlannedAmountRemaining = paymentPlanData.AmountPerPayment * ( paymentPlanData.NumberOfPayments - paymentPlanData.ProcessedNumberOfPayments ),
            };
        }

        /// <summary>
        /// Includes all the dependencies needed to get <see cref="FinancialScheduledTransaction.PaymentPlan"/> properties without having to query the DB.
        /// </summary>
        /// <remarks>
        /// <strong>This is an internal API</strong> that supports the Rock
        /// infrastructure and not subject to the same compatibility standards
        /// as public APIs. It may be changed or removed without notice in any
        /// release and should therefore not be directly used in any plug-ins.
        /// </remarks>
        /// <param name="financialScheduledTransactionQuery">The financial scheduled transaction query.</param>
        /// <returns>
        /// The financial scheduled transaction query.
        /// </returns>
        [RockInternal( "1.16.6" )]
        public static IQueryable<FinancialScheduledTransaction> IncludePaymentPlanDependencies( this IQueryable<FinancialScheduledTransaction> financialScheduledTransactionQuery )
        {
            if ( financialScheduledTransactionQuery == null )
            {
                return null;
            }

            return financialScheduledTransactionQuery
                .Include( f => f.ScheduledTransactionDetails )
                .Include( f => f.Transactions )
                .Include( f => f.TransactionFrequencyValue );
        }
    }
}
