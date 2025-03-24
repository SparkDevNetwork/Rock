// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.SqlTypes;
using System.Text;

using Rock.Data;
using Rock.Financial;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// FinancialPaymentDetail Logic
    /// </summary>
    public partial class FinancialPaymentDetail : Model<FinancialPaymentDetail>
    {
        #region Virtual Properties

        /// <summary>
        /// Gets the expiration date formatted as mm/yy, as per ISO7813 https://en.wikipedia.org/wiki/ISO/IEC_7813
        /// </summary>
        /// <value>
        /// The expiration date.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public string ExpirationDate
        {
            get
            {
                int? expMonth = ExpirationMonth;
                int? expYear = ExpirationYear;
                if ( expMonth.HasValue && expYear.HasValue )
                {
                    // ExpirationYear returns 4 digits, but just in case, check if it is 4 digits before just getting the last 2
                    string expireYY = expYear.Value.ToString();
                    if ( expireYY.Length == 4 )
                    {
                        expireYY = expireYY.Substring( 2 );
                    }

                    return $"{expMonth.Value:00}/{expireYY:00}";
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the type of the currency and credit card.
        /// </summary>
        /// <value>
        /// The type of the currency and credit card.
        /// </value>
        [NotMapped]
        public virtual string CurrencyAndCreditCardType
        {
            get
            {
                var sb = new StringBuilder();

                if ( CurrencyTypeValue != null )
                {
                    sb.Append( CurrencyTypeValue.Value );
                }

                if ( CreditCardTypeValue != null )
                {
                    sb.AppendFormat( " - {0}", CreditCardTypeValue.Value );
                }

                return sb.ToString();
            }
        }

        #endregion Virtual Properties

        #region Private/Internal Methods

        /// <summary>
        /// Get a four digit year value.
        /// </summary>
        /// <remarks>Changed to 'internal' to be able to test this with Rock.Test.</remarks>
        /// <param name="year">The year.</param>
        /// <returns></returns>
        internal int? ToFourDigitYear( int? year )
        {
            int? fourDigitYear;
            if ( year == null || year >= 100 )
            {
                fourDigitYear = year;
            }
            else
            {
                fourDigitYear = RockDateTime.ToFourDigitYearForCreditCardExpiration( year.Value );
            }

            // make sure it is a valid year (between 1753 and 9999)
            if ( fourDigitYear.HasValue && fourDigitYear >= SqlDateTime.MinValue.Value.Year && fourDigitYear <= SqlDateTime.MaxValue.Value.Year )
            {
                return fourDigitYear;
            }
            else
            {
                // invalid year
                return null;
            }
        }

        /// <summary>
        /// Retrieves the image associated with the credit card type for the current financial payment detail.
        /// </summary>
        /// <returns>
        /// A string representing the path to the credit card image, or an empty string if no image is available.
        /// </returns>
        internal string GetCreditCardImageSource()
        {
            return GetCreditCardImageSourceByTypeId( CreditCardTypeValueId );
        }

        /// <summary>
        /// Retrieves the image source associated with a given credit card type.
        /// </summary>
        /// <param name="creditCardTypeValueId">The defined value ID representing the credit card type.</param>
        /// <returns>
        /// A string representing the path to the credit card image, or an empty string if no image is available.
        /// </returns>
        /// <remarks>
        /// This method returns an empty string if <paramref name="creditCardTypeValueId"/> is null or invalid. 
        /// It uses the <see cref="Rock.Web.Cache.DefinedValueCache"/> to look up the image associated with the credit card type.
        /// </remarks>
        internal static string GetCreditCardImageSourceByTypeId( int? creditCardTypeValueId )
        {
            if ( !creditCardTypeValueId.HasValue )
            {
                return string.Empty;
            }

            // Retrieve the defined value cache for the given credit card type ID.
            var creditCardTypeValueCache = Rock.Web.Cache.DefinedValueCache.Get( creditCardTypeValueId.Value );
            if ( creditCardTypeValueCache == null )
            {
                return string.Empty;
            }

            // Retrieve and return the associated image source or an empty string if not set.
            return creditCardTypeValueCache.GetAttributeValue( SystemKey.CreditCardTypeAttributeKey.IconImage ) ?? string.Empty;
        }

        /// <summary>
        /// Gets the description for the account, including masked account number and expiration date if available.
        /// </summary>
        /// <returns>
        /// A string representing the account description. If the expiration date and/or account number are available, 
        /// they are included in the description. Otherwise, an empty string is returned.
        /// </returns>
        internal string GetDescription()
        {
            return GetAccountDescription( AccountNumberMasked, ExpirationMonth, ExpirationYear );
        }

        /// <summary>
        /// Generates a description based on the account number and expiration details.
        /// </summary>
        /// <param name="accountNumberMasked">The masked account number (e.g., "****1234").</param>
        /// <param name="expirationMonth">The expiration month.</param>
        /// <param name="expirationYear">The expiration year.</param>
        /// <returns>
        /// A string containing the description. Includes expiration date and the last four digits of the account number if available.
        /// </returns>
        internal static string GetAccountDescription( string accountNumberMasked, int? expirationMonth, int? expirationYear )
        {
            string expirationDate = null;

            if ( expirationMonth.HasValue && expirationYear.HasValue )
            {
                // ExpirationYear returns 4 digits, but ensure it is 4 digits before getting the last 2.
                string expireYY = expirationYear.Value.ToString();
                if ( expireYY.Length == 4 )
                {
                    expireYY = expireYY.Substring( 2 );
                }

                expirationDate = $"{expirationMonth.Value:00}/{expireYY:00}";
            }

            // Build the description based on available data.
            if ( expirationDate != null )
            {
                return accountNumberMasked != null && accountNumberMasked.Length >= 4
                    ? $"Ending in {accountNumberMasked.Right( 4 )} and Expires {expirationDate}"
                    : $"Expires {expirationDate}";
            }
            else
            {
                return accountNumberMasked != null && accountNumberMasked.Length >= 4
                    ? $"Ending in {accountNumberMasked.Right( 4 )}"
                    : string.Empty;
            }
        }

        #endregion Private/Internal Methods

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            // Return the Account Number, or an empty string to avoid potential downstream issues caused by an unexpected null value.
            return this.AccountNumberMasked.ToStringSafe();
        }

        /// <summary>
        /// Clears the payment information.
        /// Use this before telling a gateway to update the payment info for an existing transaction.
        /// </summary>
        public void ClearPaymentInfo()
        {
            AccountNumberMasked = null;
            GatewayPersonIdentifier = null;
            FinancialPersonSavedAccountId = null;

            CurrencyTypeValueId = null;
            CreditCardTypeValueId = null;

            _expirationMonthEncrypted = null;
            _expirationYearEncrypted = null;
            NameOnCard = null;
            ExpirationMonth = null;
            ExpirationYear = null;
        }

        /// <summary>
        /// Sets any payment information that the <seealso cref="GatewayComponent">paymentGateway</seealso> didn't set
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="paymentGateway">The payment gateway.</param>
        /// <param name="rockContext">The rock context.</param>
        public void SetFromPaymentInfo( PaymentInfo paymentInfo, GatewayComponent paymentGateway, RockContext rockContext )
        {
            /* 2020-08-27 MDP
             This method should only update values haven't been set yet. So
                1) Make sure paymentInfo has the data (isn't null or whitespace)
                2) Don't overwrite data in this (FinancialPaymentDetail) that already has the data set.
             */

            if ( AccountNumberMasked.IsNullOrWhiteSpace() && paymentInfo.MaskedNumber.IsNotNullOrWhiteSpace() )
            {
                AccountNumberMasked = paymentInfo.MaskedNumber;
            }

            if ( paymentInfo is ReferencePaymentInfo referencePaymentInfo )
            {
                if ( GatewayPersonIdentifier.IsNullOrWhiteSpace() )
                {
                    GatewayPersonIdentifier = referencePaymentInfo.GatewayPersonIdentifier;
                }

                if ( !FinancialPersonSavedAccountId.HasValue )
                {
                    FinancialPersonSavedAccountId = referencePaymentInfo.FinancialPersonSavedAccountId;
                }
            }

            if ( !CurrencyTypeValueId.HasValue && paymentInfo.CurrencyTypeValue != null )
            {
                CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;
            }

            if ( !CreditCardTypeValueId.HasValue && paymentInfo.CreditCardTypeValue != null )
            {
                CreditCardTypeValueId = paymentInfo.CreditCardTypeValue.Id;
            }

            if ( paymentInfo is CreditCardPaymentInfo )
            {
                var ccPaymentInfo = ( CreditCardPaymentInfo ) paymentInfo;

                string nameOnCard = paymentGateway.SplitNameOnCard ? ccPaymentInfo.NameOnCard + " " + ccPaymentInfo.LastNameOnCard : ccPaymentInfo.NameOnCard;

                // since the Address info could coming from an external system (the Gateway), don't do Location validation when creating a new location
                var newLocation = new LocationService( rockContext ).Get(
                    ccPaymentInfo.BillingStreet1, ccPaymentInfo.BillingStreet2, ccPaymentInfo.BillingCity, ccPaymentInfo.BillingState, ccPaymentInfo.BillingPostalCode, ccPaymentInfo.BillingCountry, new GetLocationArgs { ValidateLocation = false, CreateNewLocation = true } );

                if ( NameOnCard.IsNullOrWhiteSpace() && nameOnCard.IsNotNullOrWhiteSpace() )
                {
                    NameOnCard = nameOnCard;
                }

                if ( !ExpirationMonth.HasValue )
                {
                    ExpirationMonth = ccPaymentInfo.ExpirationDate.Month;
                }

                if ( !ExpirationYear.HasValue )
                {
                    ExpirationYear = ccPaymentInfo.ExpirationDate.Year;
                }

                if ( !BillingLocationId.HasValue && newLocation != null )
                {
                    BillingLocationId = newLocation.Id;
                }
            }
            else if ( paymentInfo is SwipePaymentInfo )
            {
                var swipePaymentInfo = ( SwipePaymentInfo ) paymentInfo;

                if ( NameOnCard.IsNullOrWhiteSpace() && swipePaymentInfo.NameOnCard.IsNotNullOrWhiteSpace() )
                {
                    NameOnCard = swipePaymentInfo.NameOnCard;
                }

                if ( !ExpirationMonth.HasValue )
                {
                    ExpirationMonth = swipePaymentInfo.ExpirationDate.Month;
                }

                if ( !ExpirationYear.HasValue )
                {
                    ExpirationYear = swipePaymentInfo.ExpirationDate.Year;
                }
            }
            else
            {
                // since the Address info could coming from an external system (the Gateway), don't do Location validation when creating a new location
                var newLocation = new LocationService( rockContext ).Get(
                    paymentInfo.Street1, paymentInfo.Street2, paymentInfo.City, paymentInfo.State, paymentInfo.PostalCode, paymentInfo.Country, new GetLocationArgs { ValidateLocation = false, CreateNewLocation = true } );

                if ( !BillingLocationId.HasValue && newLocation != null )
                {
                    BillingLocationId = newLocation.Id;
                }

            }
        }

        #endregion Public Methods
    }
}