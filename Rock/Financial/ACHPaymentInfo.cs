//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

namespace Rock.Financial
{
    /// <summary>
    /// Information about a bank payment to be processed by a financial gateway
    /// </summary>
    public class ACHPaymentInfo : PaymentInfo
    {
        /// <summary>
        /// Gets or sets the name of the bank.
        /// </summary>
        public string BankName { get; set; }

        /// <summary>
        /// The account number
        /// </summary>
        public string BankAccountNumber { get; set; }

        /// <summary>
        /// The routing number
        /// </summary>
        public string BankRoutingNumber { get; set; }

        /// <summary>
        /// The account type
        /// </summary>
        public BankAccountType AccountType { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACHPaymentInfo"/> class.
        /// </summary>
        public ACHPaymentInfo()
            : base()
        {
            AccountType = BankAccountType.Checking;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACHPaymentInfo"/> class.
        /// </summary>
        /// <param name="bankAccountNumber">The account number.</param>
        /// <param name="bankRoutingNumber">The routing number.</param>
        /// <param name="accountType">Type of the account.</param>
        public ACHPaymentInfo( string bankAccountNumber, string bankRoutingNumber, BankAccountType accountType )
            : this()
        {
            BankAccountNumber = bankAccountNumber;
            BankRoutingNumber = bankRoutingNumber;
            AccountType = accountType;
        }

        /// <summary>
        /// Gets the payment method.
        /// </summary>
        public override string PaymentMethod 
        {
            get { return "Bank Account (ACH)"; }
        }

        /// <summary>
        /// Gets the account number.
        /// </summary>
        public override string AccountNumber 
        {
            get { return BankAccountNumber.Masked(); }
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
