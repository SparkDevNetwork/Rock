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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System;

using Rock.Data;
using Rock.Security;

namespace Rock.Model
{
    /// <summary>
    /// Represents details about the bank account or credit card that was used to make a payment
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialPaymentDetail" )]
    [DataContract]
    public partial class FinancialPaymentDetail : Model<FinancialPaymentDetail>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the Masked Account Number (Last 4 of Account Number prefixed with 12 *'s)
        /// </summary>
        /// <value>
        /// The account number masked.
        /// </value>
        [DataMember]
        public string AccountNumberMasked { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the currency type <see cref="Rock.Model.DefinedValue"/> indicating the currency that the
        /// transaction was made in.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32" /> representing the DefinedValueId of the CurrencyType <see cref="Rock.Model.DefinedValue" /> for this transaction.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE )]
        public int? CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the credit card type <see cref="Rock.Model.DefinedValue"/> indicating the credit card brand/type that was used
        /// to make this transaction. This value will be null for transactions that were not made by credit card.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the credit card type <see cref="Rock.Model.DefinedValue"/> that was used to make this transaction.
        /// This value will be null for transactions that were not made by credit card.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE )]
        public int? CreditCardTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the billing location identifier.
        /// </summary>
        /// <value>
        /// The billing location identifier.
        /// </value>
        [DataMember]
        public int? BillingLocationId { get; set; }

        /// <summary>
        /// Gets or sets the Gateway Person Identifier.
        /// This would indicate id the customer vault information on the gateway.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the Gateway Person Identifier of the account.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string GatewayPersonIdentifier { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialPersonSavedAccount"/> id that was used for this transaction (if there was one)
        /// </summary>
        /// <value>
        /// The financial person saved account.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? FinancialPersonSavedAccountId { get; set; }

        /// <summary>
        /// Gets or sets the name on card encrypted.
        /// </summary>
        /// <value>
        /// The name on card encrypted.
        /// </value>
        [DataMember]
        [MaxLength( 256 )]
        [Obsolete( "Use NameOnCard" )]
        [RockObsolete( "1.12.4" )]
        public string NameOnCardEncrypted
        {
            get
            {
                // We are only checking null here because empty string is valid.
                if ( _nameOnCard.IsNull() )
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

        private string _nameOnCardEncrypted = null;

        /// <summary>
        /// Gets the name on card.
        /// </summary>
        /// <value>
        /// The name on card.
        /// </value>
        [DataMember]
        public string NameOnCard
        {
            get
            {
                // We are only checking null here because empty string is valid.
                if ( _nameOnCard == null && _nameOnCardEncrypted != null )
                {
                    /* MDP 07-20-2021

                    If Decryption Fails, just set NameOnCard to EmptyString (not null).
                    This will prevent it from endlessly trying to decrypt it.
                    */

                    _nameOnCard = Encryption.DecryptString( _nameOnCardEncrypted ) ?? string.Empty;
                }

                return _nameOnCard;
            }
            set
            {
                _nameOnCard = value;
            }
        }

        private string _nameOnCard = null;

        /// <summary>
        /// Gets or sets the expiration month encrypted. Use <seealso cref="ExpirationMonth"/> to get the unencrypted version of Month.
        /// </summary>
        /// <value>
        /// The expiration month encrypted.
        /// </value>
        [DataMember]
        [MaxLength( 256 )]
        [Obsolete( "Use ExpirationMonth" )]
        [RockObsolete( "1.12.4" )]
        public string ExpirationMonthEncrypted
        {
            get
            {
                if ( _expirationMonth != null )
                {
                    return Encryption.EncryptString( _expirationMonth.Value.ToString() );
                }

                return _expirationMonthEncrypted;
            }

            set
            {
                _expirationMonthEncrypted = value;
            }
        }

        private string _expirationMonthEncrypted = null;

        /// <summary>
        /// Important Note: that this could be a 2 digit or 4 digit year, so use <seealso cref="ExpirationYear"/> to get the unencrypted version of this which will always return a 4 digit year.
        /// </summary>
        /// <value>
        /// The expiration year encrypted.
        /// </value>
        [DataMember]
        [MaxLength( 256 )]
        [Obsolete( "Use ExpirationYear" )]
        [RockObsolete( "1.12.4" )]
        public string ExpirationYearEncrypted
        {
            get
            {
                if ( _expirationYear != null )
                {
                    return Encryption.EncryptString( _expirationYear.Value.ToString() );
                }

                return _expirationYearEncrypted;
            }
            set
            {
                _expirationYearEncrypted = value;
            }
        }

        private string _expirationYearEncrypted = null;

        /// <summary>
        /// Gets the card expiration date.
        /// </summary>
        /// <value>
        /// The card expiration date.
        /// </value>
        [DataMember]
        public DateTime? CardExpirationDate
        {
            get
            {
                var expMonth = ExpirationMonth;
                var expYear = ExpirationYear;
                if ( expMonth.HasValue && expYear.HasValue )
                {
                    return new DateTime( expYear.Value, expMonth.Value, DateTime.DaysInMonth( expYear.Value, expMonth.Value ) );
                }

                return null;
            }
            private set
            {
                _cardExpirationDate = value;
                if ( _cardExpirationDate == null )
                {
                    _expirationMonth = null;
                    _expirationYear = null;
                }
                else
                {
                    _expirationMonth = _cardExpirationDate.Value.Month;
                    _expirationYear = _cardExpirationDate.Value.Year;
                }
            }
        }

        private DateTime? _cardExpirationDate = null;

        /// <summary>
        /// Gets the expiration month
        /// </summary>
        /// <value>
        /// The expiration month.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public int? ExpirationMonth
        {
            get
            {
                if ( _expirationMonth == null && _expirationMonthEncrypted != null )
                {
                    /* MDP 07-20-2021

                     If Decryption Fails, just set Month Year to 01/99
                     This will help prevent endlessly trying to decrypt it 

                    */

                    _expirationMonth = Encryption.DecryptString( _expirationMonthEncrypted ).AsIntegerOrNull() ?? 01;
                }

                // check if month is between 1 and 12
                if ( _expirationMonth.HasValue && ( _expirationMonth < 0 || _expirationMonth > 12 ) )
                {
                    // invalid month
                    return null;
                }

                return _expirationMonth;
            }
            set
            {
                _expirationMonth = value;
            }
        }

        private int? _expirationMonth = null;

        /// <summary>
        /// Gets the 4 digit year
        /// </summary>
        /// <value>
        /// The expiration year.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public int? ExpirationYear
        {
            get
            {
                if ( _expirationYear == null && _expirationYearEncrypted != null )
                {
                    /* MDP 07-20-2021

                     If Decryption Fails, just set Month Year to 01/99 (which would mean a 4 digit year of 1999)
                     This will help prevent endlessly trying to decrypt it 
                    */

                    _expirationYear = ToFourDigitYear( Encryption.DecryptString( _expirationYearEncrypted ).AsIntegerOrNull() ) ?? 1999;
                }

                return _expirationYear;
            }

            set
            {
                _expirationYear = ToFourDigitYear( value );
            }
        }

        private int? _expirationYear = null;

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the currency type <see cref="Rock.Model.DefinedValue"/> indicating the type of currency that was used for this
        /// transaction.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> indicating the type of currency that was used for the transaction.
        /// </value>
        [DataMember]
        public virtual DefinedValue CurrencyTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the credit card type <see cref="Rock.Model.DefinedValue"/> indicating the type of credit card that was used for this transaction.
        /// If this was not a credit card based transaction, this value will be null.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue" /> indicating the type of credit card that was used for this transaction. This value is null
        /// for transactions that were not made by credit card.
        /// </value>
        [DataMember]
        public virtual DefinedValue CreditCardTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the billing <see cref="Rock.Model.Location"/>.
        /// </summary>
        /// <value>
        /// The billing location.
        /// </value>
        [DataMember]
        public virtual Location BillingLocation { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialPersonSavedAccount"/> that was used for this transaction (if there was one)
        /// </summary>
        /// <value>
        /// The financial person saved account.
        /// </value>
        [DataMember]
        public virtual FinancialPersonSavedAccount FinancialPersonSavedAccount { get; set; }

        /// <summary>
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        [RockObsolete( "1.14" )]
        [Obsolete( "Does nothing. No longer needed. We replaced this with a private property under the SaveHook class for this entity.", true )]
        public virtual History.HistoryChangeList HistoryChangeList { get; set; }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// FinancialPersonBankAccount Configuration class.
    /// </summary>
    public partial class FinancialPaymentDetailConfiguration : EntityTypeConfiguration<FinancialPaymentDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialPersonBankAccountConfiguration"/> class.
        /// </summary>
        public FinancialPaymentDetailConfiguration()
        {
            this.HasOptional( t => t.CurrencyTypeValue ).WithMany().HasForeignKey( t => t.CurrencyTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.CreditCardTypeValue ).WithMany().HasForeignKey( t => t.CreditCardTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.BillingLocation ).WithMany().HasForeignKey( t => t.BillingLocationId ).WillCascadeOnDelete( false );

            /*
             * 2020-06-12 - JH
             *
             * When a FinancialPersonSavedAccount record that this FinancialPaymentDetail references is deleted, SQL will simply null-out the
             * FinancialPaymentDetail.FinancialPersonSavedAccountId field. See here for how we manually introduced this "ON DELETE SET NULL"
             * behavior:
             *
             * https://github.com/SparkDevNetwork/Rock/commit/6953aa1986d46c9c84663ce818333425c0807c01#diff-e0c4fac8254b21998bb9235c3dee4ee9R36
             */
            this.HasOptional( t => t.FinancialPersonSavedAccount ).WithMany().HasForeignKey( t => t.FinancialPersonSavedAccountId ).WillCascadeOnDelete( true );
        }
    }

    #endregion Entity Configuration
}