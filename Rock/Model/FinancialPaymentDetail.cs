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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;
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
        /// Gets the expiration date formatted as mm/yy
        /// </summary>
        /// <value>
        /// The expiration date.
        /// </value>
        [NotMapped]
        public string ExpirationDate
        {
            get
            {
                int? expMonth = ExpirationMonth;
                int? expYear = ExpirationYear;
                if ( expMonth.HasValue && expYear.HasValue )
                {
                    return $"{expMonth.Value:00}/{expYear.Value:00}";
                }
                return null;
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
        [RockObsolete( "1.8" )]
        [Obsolete( "Use HistoryChangeList" )]
        public virtual List<string> HistoryChanges { get; set; }

        /// <summary>
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        public virtual History.HistoryChangeList HistoryChangeList { get; set; }

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
        [RockObsolete( "1.7.1" )]
        [Obsolete( "Use other SetFromPaymentInfo", true )]
        public void SetFromPaymentInfo( PaymentInfo paymentInfo, GatewayComponent paymentGateway, RockContext rockContext, List<string> changes )
        {
            this.SetFromPaymentInfo( paymentInfo, paymentGateway, rockContext );
        }

        /// <summary>
        /// Sets from payment information.
        /// </summary>
        /// <param name="paymentInfo">The payment information.</param>
        /// <param name="paymentGateway">The payment gateway.</param>
        /// <param name="rockContext">The rock context.</param>
        public void SetFromPaymentInfo( PaymentInfo paymentInfo, GatewayComponent paymentGateway, RockContext rockContext ) 
        {
            if ( AccountNumberMasked.IsNullOrWhiteSpace() && paymentInfo.MaskedNumber.IsNotNullOrWhiteSpace() )
            {
                AccountNumberMasked = paymentInfo.MaskedNumber;
            }

            if ( !CurrencyTypeValueId.HasValue && paymentInfo.CurrencyTypeValue != null )
            {
                CurrencyTypeValueId = paymentInfo.CurrencyTypeValue.Id;
            }

            if ( !CreditCardTypeValueId.HasValue &&  paymentInfo.CreditCardTypeValue != null )
            {
                CreditCardTypeValueId = paymentInfo.CreditCardTypeValue.Id;
            }

            if ( paymentInfo is CreditCardPaymentInfo )
            {
                var ccPaymentInfo = (CreditCardPaymentInfo)paymentInfo;

                string nameOnCard = paymentGateway.SplitNameOnCard ? ccPaymentInfo.NameOnCard + " " + ccPaymentInfo.LastNameOnCard : ccPaymentInfo.NameOnCard;
                var newLocation = new LocationService( rockContext ).Get(
                    ccPaymentInfo.BillingStreet1, ccPaymentInfo.BillingStreet2, ccPaymentInfo.BillingCity, ccPaymentInfo.BillingState, ccPaymentInfo.BillingPostalCode, ccPaymentInfo.BillingCountry );

                if ( NameOnCard.IsNullOrWhiteSpace() && NameOnCard.IsNotNullOrWhiteSpace() )
                {
                    NameOnCardEncrypted = Encryption.EncryptString( nameOnCard );
                }

                if ( !ExpirationMonth.HasValue )
                {
                    ExpirationMonthEncrypted = Encryption.EncryptString( ccPaymentInfo.ExpirationDate.Month.ToString() );
                }

                if ( !ExpirationYear.HasValue )
                {
                    ExpirationYearEncrypted = Encryption.EncryptString( ccPaymentInfo.ExpirationDate.Year.ToString() );
                }

                if ( !BillingLocationId.HasValue && newLocation != null )
                {
                    BillingLocationId = newLocation.Id;
                }
            }
            else if ( paymentInfo is SwipePaymentInfo )
            {
                var swipePaymentInfo = (SwipePaymentInfo)paymentInfo;

                if ( NameOnCard.IsNullOrWhiteSpace() && NameOnCard.IsNotNullOrWhiteSpace() )
                {
                    NameOnCardEncrypted = Encryption.EncryptString( swipePaymentInfo.NameOnCard );
                }

                if ( !ExpirationMonth.HasValue )
                {
                    ExpirationMonthEncrypted = Encryption.EncryptString( swipePaymentInfo.ExpirationDate.Month.ToString() );
                }

                if ( !ExpirationYear.HasValue )
                {
                    ExpirationYearEncrypted = Encryption.EncryptString( swipePaymentInfo.ExpirationDate.Year.ToString() );
                }
            }
            else
            {
                var newLocation = new LocationService( rockContext ).Get(
                    paymentInfo.Street1, paymentInfo.Street2, paymentInfo.City, paymentInfo.State, paymentInfo.PostalCode, paymentInfo.Country );

                if ( !BillingLocationId.HasValue && newLocation != null )
                {
                    BillingLocationId = newLocation.Id;
                }

            }
        }

        /// <summary>
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = (RockContext)dbContext;
            HistoryChangeList = new History.HistoryChangeList();

            switch ( entry.State )
            {
                case EntityState.Added:
                    {
                        History.EvaluateChange( HistoryChangeList, "Account Number", string.Empty, AccountNumberMasked );
                        History.EvaluateChange( HistoryChangeList, "Currency Type", (int?)null, CurrencyTypeValue, CurrencyTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Credit Card Type", (int?)null, CreditCardTypeValue, CreditCardTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Name On Card", string.Empty, AccountNumberMasked, true );
                        History.EvaluateChange( HistoryChangeList, "Expiration Month", string.Empty, ExpirationMonthEncrypted, true );
                        History.EvaluateChange( HistoryChangeList, "Expiration Year", string.Empty, ExpirationYearEncrypted, true );
                        History.EvaluateChange( HistoryChangeList, "Billing Location", string.Empty, History.GetValue<Location>( BillingLocation, BillingLocationId, rockContext ) );
                        break;
                    }
                case EntityState.Modified:
                case EntityState.Deleted:
                    {
                        History.EvaluateChange( HistoryChangeList, "Account Number", entry.OriginalValues["AccountNumberMasked"].ToStringSafe(), AccountNumberMasked );
                        History.EvaluateChange( HistoryChangeList, "Currency Type", entry.OriginalValues["CurrencyTypeValueId"].ToStringSafe().AsIntegerOrNull(), CurrencyTypeValue, CurrencyTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Credit Card Type", entry.OriginalValues["CreditCardTypeValueId"].ToStringSafe().AsIntegerOrNull(), CreditCardTypeValue, CreditCardTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Name On Card", entry.OriginalValues["AccountNumberMasked"].ToStringSafe(), AccountNumberMasked, true );
                        History.EvaluateChange( HistoryChangeList, "Expiration Month", entry.OriginalValues["ExpirationMonthEncrypted"].ToStringSafe(), ExpirationMonthEncrypted, true );
                        History.EvaluateChange( HistoryChangeList, "Expiration Year", entry.OriginalValues["ExpirationYearEncrypted"].ToStringSafe(), ExpirationYearEncrypted, true );
                        History.EvaluateChange( HistoryChangeList, "Billing Location", History.GetValue<Location>( null, entry.OriginalValues["BillingLocationId"].ToStringSafe().AsIntegerOrNull(), rockContext ), History.GetValue<Location>( BillingLocation, BillingLocationId, rockContext ) );
                        break;
                    }
            }

            if ( entry.State == EntityState.Added || entry.State == EntityState.Modified )
            {
                // Ensure that CurrencyTypeValueId is set. The UI tries to prevent it, but just in case, if it isn't, set it to Unknown
                if ( !this.CurrencyTypeValueId.HasValue )
                {
                    this.CurrencyTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.CURRENCY_TYPE_UNKNOWN.AsGuid() )?.Id;
                }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            if ( HistoryChangeList.Any() )
            {
                foreach ( var txn in new FinancialTransactionService( (RockContext)dbContext )
                    .Queryable().AsNoTracking()
                    .Where( t => t.FinancialPaymentDetailId == this.Id )
                    .Select( t => new { t.Id, t.BatchId } )
                    .ToList() )
                {
                    HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialTransaction ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), txn.Id, HistoryChangeList, true, this.ModifiedByPersonAliasId, dbContext.SourceOfChange );
                    var batchHistory = new History.HistoryChangeList();
                    batchHistory.AddChange( History.HistoryVerb.Modify, History.HistoryChangeType.Property, "Transaction" );
                    HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialBatch ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), txn.BatchId.Value, batchHistory, string.Empty, typeof( FinancialTransaction ), txn.Id, true, this.ModifiedByPersonAliasId, dbContext.SourceOfChange );
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