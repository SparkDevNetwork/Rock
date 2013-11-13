//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
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
        /// Gets the reference identifier for a processed transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract int GetReferenceId( FinancialTransaction transaction, out string errorMessage );
        
    }
}
