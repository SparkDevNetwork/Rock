﻿// <copyright>
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
        /// The billing street line 1
        /// </summary>
        public string BillingStreet1 { get; set; }

        /// <summary>
        /// The billing street line 1
        /// </summary>
        public string BillingStreet2 { get; set; }

        /// <summary>
        /// The billing city
        /// </summary>
        public string BillingCity { get; set; }

        /// <summary>
        /// The billing state
        /// </summary>
        public string BillingState { get; set; }

        /// <summary>
        /// The billing zip/postal code
        /// </summary>
        public string BillingPostalCode { get; set; }

        /// <summary>
        /// The billing country
        /// </summary>
        public string BillingCountry { get; set; }

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
            Regex rgx = new Regex( @"[^\d]" );
            string ccNum = rgx.Replace( number, "" );

            Number = ccNum;
            Code = code;
            ExpirationDate = expirationDate;
        }

        /// <summary>
        /// Gets the account number.
        /// </summary>
        public override string MaskedNumber
        {
            get { return Number.Masked( true ); }
        }

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        public override DefinedValueCache CurrencyTypeValue
        {
            get { return DefinedValueCache.Get( new Guid( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_CREDIT_CARD ) ); }
        }

        /// <summary>
        /// Gets the credit card type value id.
        /// </summary>
        public override DefinedValueCache CreditCardTypeValue
        {
            get
            {
                return GetCreditCardType( Number.AsNumeric() );
            }
        }

        /// <summary>
        /// Gets the type of the credit card based on evaluating the RegExPattern of each credit card type defined value and returning the first match
        /// </summary>
        /// <param name="ccNumber">The cc number.</param>
        /// <returns></returns>
        public static DefinedValueCache GetCreditCardType( string ccNumber )
        {
            foreach ( var dv in DefinedTypeCache.Get( new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE ) ).DefinedValues )
            {
                string pattern = dv.GetAttributeValue( "RegExPattern" );
                if ( !string.IsNullOrWhiteSpace( pattern ) )
                {
                    var re = new Regex( pattern );
                    if ( re.IsMatch( ccNumber ) )
                    {
                        return dv;
                    }
                }
            }

            return null;
        }

    }
}
