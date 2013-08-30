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
        /// Gets the supported frequency values.
        /// </summary>
        /// <value>
        /// The supported frequency values.
        /// </value>
        public virtual List<DefinedValueCache> SupportedFrequencyValues
        {
            get { return new List<DefinedValueCache>(); }
        }

        /// <summary>
        /// Initiates a credit-card sale transaction.  If succesful, a transaction id is returned
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="creditCard">The credit card.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool Charge( FinancialTransaction transaction, CreditCard creditCard, out string errorMessage);

        /// <summary>
        /// Initiates an ach sale transaction.  If succesful, a transaction id is returned
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="bankAccount">The bank account.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool Charge( FinancialTransaction transaction, BankAccount bankAccount, out string errorMessage );

        /// <summary>
        /// Creates the scheduled transaction.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="creditCard">The credit card.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool CreateScheduledTransaction( FinancialScheduledTransaction scheduledTransaction, CreditCard creditCard, out string errorMessage );

        /// <summary>
        /// Creates the scheduled transaction.
        /// </summary>
        /// <param name="scheduledTransaction">The scheduled transaction.</param>
        /// <param name="bankAccount">The bank account.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract bool CreateScheduledTransaction( FinancialScheduledTransaction scheduledTransaction, BankAccount bankAccount, out string errorMessage );

        /// <summary>
        /// Updates the scheduled transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public abstract FinancialScheduledTransaction UpdateScheduledTransaction( FinancialScheduledTransaction transaction, out string errorMessage );

        /// <summary>
        /// Cancels the scheduled transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public abstract bool CancelScheduledTransaction( FinancialScheduledTransaction transaction, out string errorMessage );

        /// <summary>
        /// Processes any post back from the financial gateway.
        /// </summary>
        /// <param name="request">The request.</param>
        public abstract void ProcessPostBack( System.Web.HttpRequest request );

        /// <summary>
        /// Downloads any new transactions from the financial gateway
        /// </summary>
        public abstract DataTable DownloadNewTransactions( DateTime startDate, DateTime endDate, out string errorMessage );
    
    }
}
