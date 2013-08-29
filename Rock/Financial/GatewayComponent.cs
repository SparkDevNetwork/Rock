//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;

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
        /// Initiates a credit-card sale transaction.  If succesful, a transaction id is returned
        /// </summary>
        /// <param name="creditCard">The credit card.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public abstract string Charge( CreditCard creditCard, out string errorMessage);

        /// <summary>
        /// Initiates an ach sale transaction.  If succesful, a transaction id is returned
        /// </summary>
        public abstract string Charge( BankAccount bankAccount, out string errorMessage );

        /// <summary>
        /// Creates the scheduled transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="creditCard">The credit card.</param>
        /// <returns></returns>
        public abstract FinancialScheduledTransaction CreateScheduledTransaction( FinancialScheduledTransaction transaction, CreditCard creditCard );

        /// <summary>
        /// Creates the scheduled transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="bankAccount">The bank account.</param>
        /// <returns></returns>
        public abstract FinancialScheduledTransaction CreateScheduledTransaction( FinancialScheduledTransaction transaction, BankAccount bankAccount );

        /// <summary>
        /// Updates the scheduled transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public abstract FinancialScheduledTransaction UpdateScheduledTransaction( FinancialScheduledTransaction transaction);

        /// <summary>
        /// Cancels the scheduled transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        public abstract bool CancelScheduledTransaction( FinancialScheduledTransaction transaction );

        /// <summary>
        /// Processes any post back from the financial gateway.
        /// </summary>
        /// <param name="request">The request.</param>
        public abstract void ProcessPostBack( System.Web.HttpRequest request );

        /// <summary>
        /// Downloads any new transactions from the financial gateway
        /// </summary>
        public abstract void DownloadNewTransactions();
    
    }
}
