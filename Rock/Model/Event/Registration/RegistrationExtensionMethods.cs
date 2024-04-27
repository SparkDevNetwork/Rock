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

using System.Linq;
using Rock.Attribute;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Extension method class for Registration
    /// </summary>
    public static partial class RegistrationExtensionMethods
    {
        /// <summary>
        /// Gets the payments.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static IQueryable<FinancialTransactionDetail> GetPayments( this Registration registration, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return new RegistrationService( rockContext ).GetPayments( registration != null ? registration.Id : 0 );
        }

        /// <summary>
        /// Gets the total paid.
        /// </summary>
        /// <param name="registration">The registration.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static decimal GetTotalPaid( this Registration registration, RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();
            return new RegistrationService( rockContext ).GetTotalPayments( registration != null ? registration.Id : 0 );
        }

        /// <summary>
        /// Gets registrations and their associated (active and inactive) payment plans.
        /// </summary>
        /// <remarks>
        /// <strong>This is an internal API</strong> that supports the Rock
        /// infrastructure and not subject to the same compatibility standards
        /// as public APIs. It may be changed or removed without notice in any
        /// release and should therefore not be directly used in any plug-ins.
        /// </remarks>
        /// <param name="registrationsQuery"></param>
        /// <returns>An <see cref="IQueryable"/> of <see cref="Registration"/> and (active and inactive) <see cref="PaymentPlan"/> pairs. If a registration does not have a payment plan, the pair will contain the source registration and a <see langword="null"/> payment plan.</returns>
        [RockInternal( "1.16.6" )]
        public static IQueryable<RegistrationPaymentPlan> GetPaymentPlans( this IQueryable<Registration> registrationsQuery )
        {
            return registrationsQuery
                // Get the registration data to build the run-time payment plans...
                .Select( r => new
                {
                    Registration = r,
                    PaymentPlanData = r.PaymentPlanFinancialScheduledTransaction == null
                        ? null
                        : new
                        {
                            r.PaymentPlanFinancialScheduledTransaction.IsActive,

                            // There should only be one scheduled transaction detail for a payment plan,
                            // but this will account for future cases where there may be multiple schedules.
                            AmountPerPayment = r.PaymentPlanFinancialScheduledTransaction.ScheduledTransactionDetails.Sum( d => d.Amount ),
                            NumberOfPayments = r.PaymentPlanFinancialScheduledTransaction.NumberOfPayments ?? 0,

                            // The number of payments, amount per payment, and start date can be updated by administrators.
                            // When calculating the number of payments processed for the latest payment plan configuration,
                            // only consider transactions that were created after the latest payment plan's start date.
                            ProcessedNumberOfPayments = r.PaymentPlanFinancialScheduledTransaction.Transactions.Count( t => t.TransactionDateTime.HasValue && t.TransactionDateTime.Value > r.PaymentPlanFinancialScheduledTransaction.StartDate ),

                            r.PaymentPlanFinancialScheduledTransaction.StartDate,
                            TransactionFrequencyValueGuid = r.PaymentPlanFinancialScheduledTransaction.TransactionFrequencyValue.Guid,
                        }
                } )
                // ...then build the payment plans.
                .Select( r => new RegistrationPaymentPlan
                {
                    Registration = r.Registration,
                    PaymentPlan = r.PaymentPlanData == null
                        ? null
                        : new PaymentPlan
                        {
                            IsActive = r.PaymentPlanData.IsActive,

                            StartDate = r.PaymentPlanData.StartDate,
                            TransactionFrequencyValueGuid = r.PaymentPlanData.TransactionFrequencyValueGuid,
                            AmountPerPayment = r.PaymentPlanData.AmountPerPayment,

                            NumberOfPayments = r.PaymentPlanData.NumberOfPayments,
                            NumberOfPaymentsProcessed = r.PaymentPlanData.ProcessedNumberOfPayments,
                            NumberOfPaymentsRemaining = r.PaymentPlanData.NumberOfPayments - r.PaymentPlanData.ProcessedNumberOfPayments,

                            PlannedAmount = r.PaymentPlanData.AmountPerPayment * r.PaymentPlanData.NumberOfPayments,
                            PlannedAmountProcessed = r.PaymentPlanData.AmountPerPayment * r.PaymentPlanData.ProcessedNumberOfPayments,
                            PlannedAmountRemaining = r.PaymentPlanData.AmountPerPayment * ( r.PaymentPlanData.NumberOfPayments - r.PaymentPlanData.ProcessedNumberOfPayments ),
                        }
                } );
        }
    }
}
