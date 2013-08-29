//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a credit card that is passed to financial gateway
    /// </summary>
    public class CreditCard
    {
        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        public decimal Amount { get; set; }

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
        /// The information obtained from a card-present swipe
        /// </summary>
        public string SwipeInfo { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCard"/> class.
        /// </summary>
        public CreditCard()
        { 
            Amount = 0;
            NameOnCard = string.Empty;
            LastNameOnCard = string.Empty;
            BillingStreet = string.Empty;
            BillingCity = string.Empty;
            BillingState = string.Empty;
            BillingZip = string.Empty;
            Number = string.Empty;
            Code = string.Empty;
            ExpirationDate = DateTime.MinValue;
            SwipeInfo = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCard" /> struct.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <param name="code">The code.</param>
        /// <param name="expirationDate">The expiration date.</param>
        public CreditCard( string number, string code, DateTime expirationDate )
            : this()
        {
            Number = number;
            Code = code;
            ExpirationDate = expirationDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreditCard" /> struct.
        /// </summary>
        /// <param name="swipeInfo">The swipe info.</param>
        public CreditCard( string swipeInfo )
            : this()
        {
            SwipeInfo = swipeInfo;
        }
    }
}
