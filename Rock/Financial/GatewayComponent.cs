// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.Data;
using Rock.Attribute;
using Rock.Extension;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Base class for financial provider components
    /// </summary>
    public abstract class GatewayComponent : Component
    {
        /// <summary>
        /// Gets a value indicating whether gateway provider needs first and last name on credit card as two distinct fields.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [split name on card]; otherwise, <c>false</c>.
        /// </value>
        public virtual bool SplitNameOnCard 
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the supported payment schedules.
        /// </summary>
        /// <value>
        /// The supported payment schedules.
        /// </value>
        public virtual List<DefinedValueCache> SupportedPaymentSchedules
        {
            get { return new List<DefinedValueCache>(); }
        }

        /// <summary>
        /// Gets the batch time offset.  By default online payments will be grouped into batches with a start time
        /// of 12:00:00 AM.  However if the the payment gateway groups transactions into batches based on a different
        /// time, this offset can specified so that Rock will use the same time when creating batches for online
        /// transactions
        /// </summary>
        public virtual TimeSpan BatchTimeOffset
        {
            get { return new TimeSpan( 0 ); }
        }

        /// <summary>
        /// Returnes a boolean value indicating if 'Saved Account' functionality is supported for the given currency type. 
        /// </summary>
        /// <param name="currencyType">Type of the currency.</param>
        /// <returns></returns>
        public virtual bool SupportsSavedAccount( DefinedValueCache currencyType )
        {
            return true;
        }

        /// <summary>
        /// Authorizes the specified payment information.
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public virtual FinancialTransaction Authorize( PaymentInfo paymentInfo, out string errorMessage )
        {
            errorMessage = "Gateway does not support Authorizations";
            return null;
        }
    
        /// <summary>
        /// Charges the specified payment info.
        /// </summary>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract FinancialTransaction Charge( PaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Adds the scheduled payment.
        /// </summary>
        /// <param name="schedule">The schedule.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract FinancialScheduledTransaction AddScheduledPayment( PaymentSchedule schedule, PaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Updates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="paymentInfo">The payment info.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool UpdateScheduledPayment( FinancialScheduledTransaction transaction, PaymentInfo paymentInfo, out string errorMessage );

        /// <summary>
        /// Cancels the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool CancelScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage );

        /// <summary>
        /// Reactivates the scheduled payment.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool ReactivateScheduledPayment( FinancialScheduledTransaction transaction, out string errorMessage );

        /// <summary>
        /// Gets the scheduled payment status.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool GetScheduledPaymentStatus( FinancialScheduledTransaction transaction, out string errorMessage );

        /// <summary>
        /// Gets the payments that have been processed for any scheduled transactions
        /// </summary>
        /// <param name="startDate">The start date.</param>
        /// <param name="endDate">The end date.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract List<Payment> GetPayments( DateTime startDate, DateTime endDate, out string errorMessage );

        /// <summary>
        /// Gets an optional reference number needed to process future transaction from saved account.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract string GetReferenceNumber( FinancialTransaction transaction, out string errorMessage );

        /// <summary>
        /// Gets an optional reference number needed to process future transaction from saved account.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract string GetReferenceNumber( FinancialScheduledTransaction scheduledTransaction, out string errorMessage );
    }
}
