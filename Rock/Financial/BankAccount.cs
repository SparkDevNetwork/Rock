//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Financial
{
    /// <summary>
    /// Information about a bank account that is passed to financial gateway
    /// </summary>
    public struct BankAccount
    {
        /// <summary>
        /// The account number
        /// </summary>
        public string AccountNumber;

        /// <summary>
        /// The routing number
        /// </summary>
        public string RoutingNumber;

        /// <summary>
        /// The account type
        /// </summary>
        public BankAccountType AccountType;

        /// <summary>
        /// Initializes a new instance of the <see cref="BankAccount" /> struct.
        /// </summary>
        /// <param name="accountNumber">The account number.</param>
        /// <param name="routingNumber">The routing number.</param>
        /// <param name="accountType">Type of the account.</param>
        public BankAccount( string accountNumber, string routingNumber, BankAccountType accountType )
        {
            AccountNumber = accountNumber;
            RoutingNumber = routingNumber;
            AccountType = accountType;
        }
            
    }

    /// <summary>
    /// Type of bank account
    /// </summary>
    public enum BankAccountType
    {
        /// <summary>
        /// Checking Account
        /// </summary>
        Checking = 0,

        /// <summary>
        /// Savings Account
        /// </summary>
        Savings = 1,

    }
 
}
