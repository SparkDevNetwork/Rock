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
using System.Collections.Generic;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Financial
{
    /// <summary>
    /// Information about a payment transaction that has been processed
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Gets or sets the gross amount. This value will always be in the organization's currency.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the net amount (Amount minus FeeAmount). This value will always be in the organization's currency.
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// Gets or sets the fee amount. This value will always be in the organization's currency.
        /// </summary>
        public decimal? FeeAmount { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the foreign key (some gateways may use this as another identifier)
        /// </summary>
        public string ForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        public DateTime TransactionDateTime { get; set; }

        /// <summary>
        /// Gets the currency type value.
        /// </summary>
        public virtual DefinedValueCache CurrencyTypeValue { get; set; }

        /// <summary>
        /// Gets the credit card type value id.
        /// </summary>
        public virtual DefinedValueCache CreditCardTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the gateway schedule id.
        /// </summary>
        public string GatewayScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the Gateway Person Identifier. Usually a reference to the gateway's saved customer info which the gateway would have previously collected payment info.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the Gateway Person Identifier of the account.
        /// </value>
        public string GatewayPersonIdentifier { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether schedule is still active.
        /// </summary>
        public bool? ScheduleActive { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the transaction has been settled by the processor/gateway.
        /// </summary>
        public bool? IsSettled { get; set; }

        /// <summary>
        /// The group/batch identifier used by the processor/gateway when the transaction has been settled.
        /// </summary>
        public string SettledGroupId { get; set; }

        /// <summary>
        /// Gets or sets the date that the transaction was settled by the processor/gateway.
        /// </summary>
        public DateTime? SettledDate { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        public string StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is failure.
        /// </summary>
        public bool IsFailure { get; set; }

        /// <summary>
        /// Additional payment attributes
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; }

        /// <summary>
        /// Gets or sets the Masked Account Number (Last 4 of Account Number prefixed with 12 *'s)
        /// </summary>
        /// <value>
        /// The account number masked.
        /// </value>
        public string AccountNumberMasked { get; set; }

        private string _nameOnCardEncrypted = null;
        /// <summary>
        /// Gets or sets the name on card encrypted.
        /// </summary>
        /// <value>
        /// The name on card encrypted.
        /// </value>
        [Obsolete("Use NameOnCard")]
        [RockObsolete( "1.12.4" )]
        public string NameOnCardEncrypted
        {
            get
            {
                // We are only checking null here because empty string is valid.
                if ( _nameOnCard == null )
                {
                    return _nameOnCardEncrypted;
                }
                return Encryption.EncryptString( _nameOnCard );
            }
            set
            {
                _nameOnCardEncrypted = value;
            }
        }

        private string _nameOnCard = null;
        /// <summary>
        /// Gets or sets the name on card.
        /// </summary>
        /// <value>
        /// The name on card.
        /// </value>
        public string NameOnCard
        {
            get
            {
                // We are only checking null here because empty string is valid.
                if ( _nameOnCard == null )
                {
                    return Encryption.DecryptString( _nameOnCardEncrypted );
                }
                return _nameOnCard;
            }
            set
            {
                _nameOnCard = value;
            }
        }

        private string _expirationMonthEncrypted = null;
        private string _expirationYearEncrypted = null;

        /// <summary>
        /// Gets or sets the expiration month encrypted. Use <seealso cref="ExpirationMonth"/> to get the unencrypted version of Month.
        /// </summary>
        /// <value>
        /// The expiration month encrypted.
        /// </value>
        [Obsolete( "Use ExpirationMonth" )]
        [RockObsolete( "1.12.4" )]
        public string ExpirationMonthEncrypted
        {
            get
            {
                if ( _expirationMonth == null )
                {
                    return _expirationMonthEncrypted;
                }
                return Encryption.EncryptString( _expirationMonth.Value.ToString() );
            }
            set
            {
                _expirationMonthEncrypted = value;
            }
        }

        /// <summary>
        /// Important Note: that this could be a 2 digit or 4 digit year, so use <seealso cref="ExpirationYear"/> to get the unencrypted version of this which will always return a 4 digit year.
        /// </summary>
        /// <value>
        /// The expiration year encrypted.
        /// </value>
        [Obsolete( "Use ExpirationYear" )]
        [RockObsolete( "1.12.4" )]
        public string ExpirationYearEncrypted
        {
            get
            {
                if ( _expirationYear == null )
                {
                    return _expirationYearEncrypted;
                }
                return Encryption.EncryptString( _expirationYear.Value.ToString() );
            }
            set
            {
                _expirationYearEncrypted = value;
            }
        }

        private int? _expirationMonth = null;
        private int? _expirationYear = null;

        /// <summary>
        /// Gets the expiration month by decrypting ExpirationMonthEncrypted
        /// </summary>
        /// <value>
        /// The expiration month.
        /// </value>
        public int? ExpirationMonth
        {
            /* MDP 2020-03-13
               NOTE: This is not really a [DataMember] (see <seealso cref="FinancialPaymentDetailConfiguration"/>)
            */

            get
            {
                return _expirationMonth ?? Encryption.DecryptString( _expirationMonthEncrypted ).AsIntegerOrNull();
            }
            set
            {
                _expirationMonth = value;
            }
        }

        /// <summary>
        /// Gets the 4 digit year by decrypting ExpirationYearEncrypted and correcting to a 4 digit year if ExpirationYearEncrypted is just a 2 digit year
        /// </summary>
        /// <value>
        /// The expiration year.
        /// </value>
        public int? ExpirationYear
        {
            /* MDP 2020-03-13
               NOTE: This is not really a [DataMember] (see <seealso cref="FinancialPaymentDetailConfiguration"/>)
            */

            get
            {
                return _expirationYear ?? ToFourDigitYear( Encryption.DecryptString( _expirationYearEncrypted ).AsIntegerOrNull() );
            }

            set
            {
                _expirationYear = ToFourDigitYear( value );
            }
        }

        private int? ToFourDigitYear( int? year )
        {
            if ( year == null || year >= 100 )
            {
                return year;
            }
            else
            {
                return RockDateTime.ToFourDigitYearForCreditCardExpiration( year.Value );
            }
        }
        /// <summary>
        /// Gets or sets the foreign currency code value identifier.
        /// </summary>
        /// <value>
        /// The foreign currency code value identifier.
        /// </value>
        public int? ForeignCurrencyCodeValueId { get; set; }

        /// <summary>
        /// Gets or sets the foreign currency amount.
        /// </summary>
        /// <value>
        /// The foreign currency amount.
        /// </value>
        public decimal? ForeignCurrencyAmount { get; set; }
    }
}