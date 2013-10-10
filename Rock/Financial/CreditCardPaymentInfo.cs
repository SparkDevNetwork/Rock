//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Text.RegularExpressions;

using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a credit card payment to be processed by a financial gateway
    /// </summary>
    public class CreditCardPaymentInfo : PaymentInfo
    {
        /// <summary>
        /// The name on card
        /// </summary>
        public string NameOnCard { get; set; }

        /// <summary>
        /// The last name on card (Only used if gateway provider requires split first name and last name fields
        /// </summary>
        public string LastNameOnCard { get; set; }

        /// <summary>
        /// The billing street
        /// </summary>
        public string BillingStreet { get; set; }

        /// <summary>
        /// The billing city
        /// </summary>
        public string BillingCity { get; set; }

        /// <summary>
        /// The billing state
        /// </summary>
        public string BillingState { get; set; }

        /// <summary>
        /// The billing zip
        /// </summary>
        public string BillingZip { get; set; }

        /// <summary>
        /// The credit card number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// The card SVN number
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// The credit card expiration date
        /// </summary>
        public DateTime ExpirationDate { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCardPaymentInfo"/> class.
        /// </summary>
        public CreditCardPaymentInfo()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCardPaymentInfo"/> class.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="code">The code.</param>
        /// <param name="expirationDate">The expiration date.</param>
        public CreditCardPaymentInfo( string number, string code, DateTime expirationDate )
            : this()
        {
            Number = number;
            Code = code;
            ExpirationDate = expirationDate;
        }

        /// <summary>
        /// Gets the account number.
        /// </summary>
        public override string MaskedNumber
        {
            get { return Number.Masked(); }
        }

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        public override DefinedValueCache CurrencyTypeValue
        {
            get { return DefinedValueCache.Read( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) ); }
        }

        /// <summary>
        /// Gets the credit card type value id.
        /// </summary>
        public override DefinedValueCache CreditCardTypeValue
        {
            get
            {
                string cc = Number.AsNumeric();
                foreach ( var dv in DefinedTypeCache.Read( new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ) ).DefinedValues )
                {
                    string pattern = dv.GetAttributeValue( "RegExPattern" );
                    if ( !string.IsNullOrWhiteSpace( pattern ) )
                    {
                        var re = new Regex( pattern );
                        if ( re.IsMatch( cc ) )
                        {
                            return dv;
                        }
                    }
                }

                return null;
            }
        }

    }
}
