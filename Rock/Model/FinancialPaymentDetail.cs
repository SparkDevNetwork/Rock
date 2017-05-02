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
using System.Linq;
using System.Data.Entity;
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

        /// <summary>
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        public List<string> HistoryChanges { get; set; }

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
        public void SetFromPaymentInfo( PaymentInfo paymentInfo, GatewayComponent paymentGateway, RockContext rockContext )
        {
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

                NameOnCardEncrypted = Encryption.EncryptString( nameOnCard );
                ExpirationMonthEncrypted = Encryption.EncryptString( ccPaymentInfo.ExpirationDate.Month.ToString() );
                ExpirationYearEncrypted = Encryption.EncryptString( ccPaymentInfo.ExpirationDate.Year.ToString() );
                BillingLocationId = newLocation != null ? newLocation.Id : (int?)null;
            }
            else if ( paymentInfo is SwipePaymentInfo )
            {
                var swipePaymentInfo = (SwipePaymentInfo)paymentInfo;

                NameOnCardEncrypted = Encryption.EncryptString( swipePaymentInfo.NameOnCard );
                ExpirationMonthEncrypted = Encryption.EncryptString( swipePaymentInfo.ExpirationDate.Month.ToString() );
                ExpirationYearEncrypted = Encryption.EncryptString( swipePaymentInfo.ExpirationDate.Year.ToString() );
            }
        }

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            var rockContext = (RockContext)dbContext;
            HistoryChanges = new List<string>();

            switch ( entry.State )
            {
                case System.Data.Entity.EntityState.Added:
                    {
                        History.EvaluateChange( HistoryChanges, "Account Number", string.Empty, AccountNumberMasked );
                        History.EvaluateChange( HistoryChanges, "Currency Type", (int?)null, CurrencyTypeValue, CurrencyTypeValueId );
                        History.EvaluateChange( HistoryChanges, "Credit Card Type", (int?)null, CreditCardTypeValue, CreditCardTypeValueId );
                        History.EvaluateChange( HistoryChanges, "Name On Card", string.Empty, AccountNumberMasked, true );
                        History.EvaluateChange( HistoryChanges, "Expiration Month", string.Empty, ExpirationMonthEncrypted, true );
                        History.EvaluateChange( HistoryChanges, "Expiration Year", string.Empty, ExpirationYearEncrypted, true );
                        History.EvaluateChange( HistoryChanges, "Billing Location", string.Empty, History.GetValue<Location>( BillingLocation, BillingLocationId, rockContext ) );
                        break;
                    }
                case System.Data.Entity.EntityState.Modified:
                case System.Data.Entity.EntityState.Deleted:
                    {
                        History.EvaluateChange( HistoryChanges, "Account Number", entry.OriginalValues["AccountNumberMasked"].ToStringSafe(), AccountNumberMasked );
                        History.EvaluateChange( HistoryChanges, "Currency Type", entry.OriginalValues["CurrencyTypeValueId"].ToStringSafe().AsIntegerOrNull(), CurrencyTypeValue, CurrencyTypeValueId );
                        History.EvaluateChange( HistoryChanges, "Credit Card Type", entry.OriginalValues["CreditCardTypeValueId"].ToStringSafe().AsIntegerOrNull(), CreditCardTypeValue, CreditCardTypeValueId );
                        History.EvaluateChange( HistoryChanges, "Name On Card", entry.OriginalValues["AccountNumberMasked"].ToStringSafe(), AccountNumberMasked, true );
                        History.EvaluateChange( HistoryChanges, "Expiration Month", entry.OriginalValues["ExpirationMonthEncrypted"].ToStringSafe(), ExpirationMonthEncrypted, true );
                        History.EvaluateChange( HistoryChanges, "Expiration Year", entry.OriginalValues["ExpirationYearEncrypted"].ToStringSafe(), ExpirationYearEncrypted, true );
                        History.EvaluateChange( HistoryChanges, "Billing Location", History.GetValue<Location>( null, entry.OriginalValues["BillingLocationId"].ToStringSafe().AsIntegerOrNull(), rockContext ), History.GetValue<Location>( BillingLocation, BillingLocationId, rockContext ) );
                        break;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Posts the save changes.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            if ( HistoryChanges.Any() )
            {
                foreach ( var txn in new FinancialTransactionService( (RockContext)dbContext )
                    .Queryable().AsNoTracking()
                    .Where( t => t.FinancialPaymentDetailId == this.Id )
                    .Select( t => new { t.Id, t.BatchId } )
                    .ToList() )
                {
                    HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialTransaction ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), txn.Id, HistoryChanges, true, this.ModifiedByPersonAliasId );
                    var batchHistory = new List<string> { string.Format( "Updated <span class='field-name'>Transaction</span> ID: <span class='field-value'>{0}</span>.", txn.Id ) };
                    HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialBatch ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), txn.BatchId.Value, batchHistory, string.Empty, typeof( FinancialTransaction ), txn.Id, true, this.ModifiedByPersonAliasId );
                }
            }

            base.PostSaveChanges( dbContext );
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