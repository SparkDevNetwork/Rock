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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using Rock.Data;
using Rock.Financial;
using Rock.Security;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents details about the bank account or credit card that was used to make a payment
    /// </summary>
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
        /// This value value will be null for transactions that were not made by credit card.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE )]
        public int? CreditCardTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the name on card encrypted.
        /// </summary>
        /// <value>
        /// The name on card encrypted.
        /// </value>
        [DataMember]
        [MaxLength( 256 )]
        public string NameOnCardEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the expiration month encrypted.
        /// </summary>
        /// <value>
        /// The expiration month encrypted.
        /// </value>
        [DataMember]
        [MaxLength( 256 )]
        public string ExpirationMonthEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the expiration year encrypted.
        /// </summary>
        /// <value>
        /// The expiration year encrypted.
        /// </value>
        [DataMember]
        [MaxLength( 256 )]
        public string ExpirationYearEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the billing location identifier.
        /// </summary>
        /// <value>
        /// The billing location identifier.
        /// </value>
        [DataMember]
        public int? BillingLocationId { get; set; }

        #endregion

        #region Virtual Properties

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
                return Rock.Security.Encryption.DecryptString( this.NameOnCardEncrypted );
            }
        }

        /// <summary>
        /// Gets the expiration month by decrypting ExpirationMonthEncrypted
        /// </summary>
        /// <value>
        /// The expiration month.
        /// </value>
        [DataMember]
        public int? ExpirationMonth
        {
            get
            {
                return Rock.Security.Encryption.DecryptString( this.ExpirationMonthEncrypted ).AsIntegerOrNull();
            }
        }

        /// <summary>
        /// Gets the expiration year by decrypting ExpirationYearEncrypted
        /// </summary>
        /// <value>
        /// The expiration year.
        /// </value>
        [DataMember]
        public int? ExpirationYear
        {
            get
            {
                return Rock.Security.Encryption.DecryptString( this.ExpirationYearEncrypted ).AsIntegerOrNull();
            }
        }

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
        /// Gets or sets the billing location.
        /// </summary>
        /// <value>
        /// The billing location.
        /// </value>
        [DataMember]
        public virtual Location BillingLocation { get; set; }

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
        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.AccountNumberMasked;
        }

        /// <summary>
        /// Sets from payment information.
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="paymentGateway">The payment gateway.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="changes">The changes.</param>
        public void SetFromPaymentInfo( PaymentInfo paymentInfo, GatewayComponent paymentGateway, RockContext rockContext, List<string> changes = null )
        {
            if ( changes != null )
            {
                if ( !string.IsNullOrWhiteSpace( paymentInfo.MaskedNumber ) )
                {
                    History.EvaluateChange( changes, "Account Number", AccountNumberMasked, paymentInfo.MaskedNumber );
                }

                if ( paymentInfo.CurrencyTypeValue != null )
                {
                    History.EvaluateChange( changes, "Currency Type", DefinedValueCache.GetName( CurrencyTypeValueId ), paymentInfo.CurrencyTypeValue.Value );
                }

                if ( paymentInfo.CreditCardTypeValue != null )
                {
                    History.EvaluateChange( changes, "Credit Card Type", DefinedValueCache.GetName( CreditCardTypeValueId ), paymentInfo.CreditCardTypeValue.Value );
                }
            }

            if ( !string.IsNullOrWhiteSpace( paymentInfo.MaskedNumber ) )
            {
                AccountNumberMasked = paymentInfo.MaskedNumber;
            }

            if ( paymentInfo.CurrencyTypeValue != null )
            {
                CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;
            }

            if ( paymentInfo.CreditCardTypeValue != null )
            {
                CreditCardTypeValueId = paymentInfo.CreditCardTypeValue.Id;
            }

            if ( paymentInfo is CreditCardPaymentInfo )
            {
                var ccPaymentInfo = (CreditCardPaymentInfo)paymentInfo;

                string nameOnCard = paymentGateway.SplitNameOnCard ? ccPaymentInfo.NameOnCard + " " + ccPaymentInfo.LastNameOnCard : ccPaymentInfo.NameOnCard;
                var newLocation = new LocationService( rockContext ).Get(
                    ccPaymentInfo.BillingStreet1, ccPaymentInfo.BillingStreet2, ccPaymentInfo.BillingCity, ccPaymentInfo.BillingState, ccPaymentInfo.BillingPostalCode, ccPaymentInfo.BillingCountry );

                if ( changes != null )
                {
                    string oldNameOnCard = Encryption.DecryptString( NameOnCardEncrypted );
                    History.EvaluateChange( changes, "Name on Card", oldNameOnCard, nameOnCard );
                    History.EvaluateChange( changes, "Expiration Month", Encryption.DecryptString( ExpirationMonthEncrypted ), ccPaymentInfo.ExpirationDate.Month.ToString() );
                    History.EvaluateChange( changes, "Expiration Year", Encryption.DecryptString( ExpirationYearEncrypted ), ccPaymentInfo.ExpirationDate.Year.ToString() );
                    History.EvaluateChange( changes, "Billing Location", BillingLocation != null ? BillingLocation.ToString() : string.Empty, newLocation != null ? newLocation.ToString() : string.Empty );
                }

                NameOnCardEncrypted = Encryption.EncryptString( nameOnCard );
                ExpirationMonthEncrypted = Encryption.EncryptString( ccPaymentInfo.ExpirationDate.Month.ToString() );
                ExpirationYearEncrypted = Encryption.EncryptString( ccPaymentInfo.ExpirationDate.Year.ToString() );
                BillingLocationId = newLocation != null ? newLocation.Id : (int?)null;
            }
            else if ( paymentInfo is SwipePaymentInfo )
            {
                var swipePaymentInfo = (SwipePaymentInfo)paymentInfo;

                if ( changes != null )
                {
                    string oldNameOnCard = Encryption.DecryptString( NameOnCardEncrypted );
                    History.EvaluateChange( changes, "Name on Card", oldNameOnCard, swipePaymentInfo.NameOnCard );
                    History.EvaluateChange( changes, "Expiration Month", Encryption.DecryptString( ExpirationMonthEncrypted ), swipePaymentInfo.ExpirationDate.Month.ToString() );
                    History.EvaluateChange( changes, "Expiration Year", Encryption.DecryptString( ExpirationYearEncrypted ), swipePaymentInfo.ExpirationDate.Year.ToString() );
                }

                NameOnCardEncrypted = Encryption.EncryptString( swipePaymentInfo.NameOnCard );
                ExpirationMonthEncrypted = Encryption.EncryptString( swipePaymentInfo.ExpirationDate.Month.ToString() );
                ExpirationYearEncrypted = Encryption.EncryptString( swipePaymentInfo.ExpirationDate.Year.ToString() );
            }
        }

        #endregion
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
        }
    }

    #endregion
}