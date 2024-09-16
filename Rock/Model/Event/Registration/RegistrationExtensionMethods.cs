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

using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using DotLiquid.Util;
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
        /// Gets registrations and their associated (active/inactive) payment plans.
        /// </summary>
        /// <remarks>
        /// <strong>This is an internal API</strong> that supports the Rock
        /// infrastructure and not subject to the same compatibility standards
        /// as public APIs. It may be changed or removed without notice in any
        /// release and should therefore not be directly used in any plug-ins.
        /// </remarks>
        /// <param name="registrationsQuery"></param>
        /// <returns>An <see cref="IQueryable"/> of <see cref="Registration"/> and (active/inactive) <see cref="PaymentPlan"/> pairs. If a registration does not have a payment plan, the pair will contain the source registration and a <see langword="null"/> payment plan.</returns>
        [RockInternal( "1.16.6" )]
        public static IQueryable<RegistrationPaymentPlanPair> SelectPaymentPlanPairs( this IQueryable<Registration> registrationsQuery )
        {
            // Create the payment plan subquery.
            var financialScheduledTransactionPaymentPlanPairs = registrationsQuery
                .Select( r => r.PaymentPlanFinancialScheduledTransaction )
                .SelectPaymentPlanPairs();

            return registrationsQuery
                .Select( r => new RegistrationPaymentPlanPair
                {
                    Registration = r,
                    PaymentPlan = financialScheduledTransactionPaymentPlanPairs
                        .Where( p => r.PaymentPlanFinancialScheduledTransactionId.HasValue && p.FinancialScheduledTransaction.Id == r.PaymentPlanFinancialScheduledTransactionId )
                        .Select( p => p.PaymentPlan )
                        .FirstOrDefault()
                } );
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
        /// <param name="registrationQuery">The query.</param>
        /// <returns>
        /// The query.
        /// </returns>
        [RockInternal( "1.16.6" )]
        public static IQueryable<Registration> IncludePaymentPlanDependencies( this IQueryable<Registration> registrationQuery )
        {
            if ( registrationQuery == null )
            {
                return null;
            }

            return registrationQuery
                .Include( f => f.PaymentPlanFinancialScheduledTransaction.ScheduledTransactionDetails )
                .Include( f => f.PaymentPlanFinancialScheduledTransaction.Transactions )
                .Include( f => f.PaymentPlanFinancialScheduledTransaction.TransactionFrequencyValue );
        }
    }
}
