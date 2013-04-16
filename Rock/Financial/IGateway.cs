//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using Rock.Model;

namespace Rock.Financial
{
    public interface IGateway
    {
        /// <summary>
        /// Charges the specified transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="creditCard">The credit card.</param>
        /// <param name="testTransaction">if set to <c>true</c> [test transaction].</param>
        /// <returns></returns>
        FinancialTransaction Charge( FinancialTransaction transaction, CreditCard creditCard, bool testTransaction = false );

        /// <summary>
        /// Charges the specified transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="bankAccount">The bank account.</param>
        /// <param name="testTransaction">if set to <c>true</c> [test transaction].</param>
        /// <returns></returns>
        FinancialTransaction Charge( FinancialTransaction transaction, BankAccount bankAccount, bool testTransaction = false );

        /// <summary>
        /// Creates the scheduled transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="creditCard">The credit card.</param>
        /// <returns></returns>
        FinancialScheduledTransaction CreateScheduledTransaction( FinancialScheduledTransaction transaction, CreditCard creditCard );

        /// <summary>
        /// Creates the scheduled transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="bankAccount">The bank account.</param>
        /// <returns></returns>
        FinancialScheduledTransaction CreateScheduledTransaction( FinancialScheduledTransaction transaction, BankAccount bankAccount );

        /// <summary>
        /// Updates the scheduled transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        FinancialScheduledTransaction UpdateScheduledTransaction( FinancialScheduledTransaction transaction);

        /// <summary>
        /// Cancels the scheduled transaction.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <returns></returns>
        bool CancelScheduledTransaction( FinancialScheduledTransaction transaction );

        /// <summary>
        /// Processes any post back from the financial gateway.
        /// </summary>
        /// <param name="request">The request.</param>
        void ProcessPostBack( System.Web.HttpRequest request );

        /// <summary>
        /// Downloads any new transactions from the financial gateway
        /// </summary>
        void DownloadNewTransactions();
    }
}
