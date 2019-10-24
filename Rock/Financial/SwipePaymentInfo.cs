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
using System;
using System.Text.RegularExpressions;

using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a credit card swipe payment to be processed by a financial gateway.  A swipe
    /// payment is used when the card is present (physically used)
    /// </summary>
    public class SwipePaymentInfo: PaymentInfo
    {

        private string _creditCardNumber = string.Empty;
        private int _creditCardExpireMonth = -1;
        private int _creditCardExpireYear = -1;
        private string _creditCardNameOnCard = string.Empty;
        
        /// <summary>
        /// The information obtained from a card-present swipe
        /// </summary>
        public string SwipeInfo { get; set; }

        /// <summary>
        /// Gets the credit card number.
        /// </summary>
        public string Number {
            get
            {
                if ( _creditCardNumber == string.Empty )
                {
                    ParseSwipeInfo();
                }

                return _creditCardNumber;
            }
        }

        /// <summary>
        /// Gets the expiration date.
        /// </summary>
        public DateTime ExpirationDate {
            get
            {
                if ( _creditCardExpireMonth == -1 )
                {
                    ParseSwipeInfo();
                }

                if (_creditCardExpireMonth == -1 )
                {
                    return DateTime.MaxValue;
                } else
                {
                    return new DateTime( _creditCardExpireYear, _creditCardExpireMonth, 1 );
                }
            }
        }

        /// <summary>
        /// Gets the name on card.
        /// </summary>
        public string NameOnCard {
            get
            {
                if ( _creditCardNameOnCard == string.Empty )
                {
                    ParseSwipeInfo();
                }

                return _creditCardNameOnCard;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwipePaymentInfo" /> struct.
        /// </summary>
        /// <param name="swipeInfo">The swipe info.</param>
        public SwipePaymentInfo( string swipeInfo )
            : base()
        {
            SwipeInfo = swipeInfo;
        }

        /// <summary>
        /// Gets the credit card type value id.
        /// </summary>
        public override DefinedValueCache CreditCardTypeValue
        {
            get
            {
                string cc = Number.AsNumeric();
                foreach ( var dv in DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ) ).DefinedValues )
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

        /// <summary>
        /// Gets the account number.
        /// </summary>
        public override string MaskedNumber
        {
            get { return "Swiped"; }
        }

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        public override DefinedValueCache CurrencyTypeValue
        {
            get { return DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) ); }
        }


        /// <summary>
        /// Parses the swipe information into the credit card parts.
        /// </summary>
        private void ParseSwipeInfo()
        {
            if ( !string.IsNullOrWhiteSpace(this.SwipeInfo) )
            {
                if ( this.SwipeInfo.StartsWith( "%B" ) )
                {
                    int endPointer = this.SwipeInfo.IndexOf( '?' );
                    string[] trackComponents = this.SwipeInfo.Substring( 2, endPointer - 2 ).Split( '^' );
                    if (trackComponents.Length >= 3 )
                    {
                        _creditCardNumber = trackComponents[0];
                        _creditCardNameOnCard = trackComponents[1];
                        int.TryParse( trackComponents[2].Substring( 0, 2 ), out _creditCardExpireYear );
                        int.TryParse( trackComponents[2].Substring( 2, 2 ), out _creditCardExpireMonth );
                    }
                }
                else if ( this.SwipeInfo.Contains( ";" ) )
                {
                    int startPointer = this.SwipeInfo.IndexOf( ';' );
                    string[] trackComponents = this.SwipeInfo.Substring( startPointer + 1 ).Split( '=' );
                    if (trackComponents.Length >= 2 )
                    {
                        _creditCardNumber = trackComponents[0];
                        int.TryParse( trackComponents[1].Substring( 0, 2 ), out _creditCardExpireYear );
                        int.TryParse( trackComponents[1].Substring( 2, 2 ), out _creditCardExpireMonth );
                    }
                }
            }
        }

    }
}
