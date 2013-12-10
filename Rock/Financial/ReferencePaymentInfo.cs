//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a reference payment to be processed by a financial gateway.  A 
    /// reference payment is initiated using a code returned by a previous payment (i.e. using
    /// a saved account number)
    /// </summary>
    public class ReferencePaymentInfo : PaymentInfo
    {
        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the reference number.
        /// </summary>
        public string ReferenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the masked account number.
        /// </summary>
        public string MaskedAccountNumber { get; set; }

        /// <summary>
        /// Gets or sets the initial currency type value.
        /// </summary>
        /// <value>
        /// The initial currency type value.
        /// </value>
        public DefinedValueCache InitialCurrencyTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the initial credit card type value id.
        /// </summary>
        /// <value>
        /// The initial credit card type value id.
        /// </value>
        public DefinedValueCache InitialCreditCardTypeValue { get; set; }

        /// <summary>
        /// Gets the account number.
        /// </summary>
        public override string MaskedNumber
        {
            get { return MaskedAccountNumber; }
        }

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        public override DefinedValueCache CurrencyTypeValue
        {
            get { return InitialCurrencyTypeValue; }
        }

        /// <summary>
        /// Gets the credit card type value id.
        /// </summary>
        public override DefinedValueCache CreditCardTypeValue
        {
            get { return InitialCreditCardTypeValue; }
        }
    }
}
