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
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;

namespace Rock.Model
{
    /// <summary>
    /// Represents a financial transaction in Rock.
    /// </summary>
    [RockDomain( "Finance" )]
    [Table( "FinancialTransaction" )]
    [DataContract]
    [Analytics( false, false )]
    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.FINANCIAL_TRANSACTION )]
    public partial class FinancialTransaction : Model<FinancialTransaction>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the authorized person identifier.
        /// </summary>
        /// <value>
        /// The authorized person identifier.
        /// </value>
        [DataMember]
        [Index( "IX_TransactionDateTime_TransactionTypeValueId_Person", 2 )]
        public int? AuthorizedPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the transaction as anonymous when displayed publicly, for example on a list of fundraising contributors
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show as anonymous]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ShowAsAnonymous { get; set; }

        /// <summary>
        /// Gets or sets BatchId of the <see cref="Rock.Model.FinancialBatch"/> that contains this transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the BatchId of the <see cref="Rock.Model.FinancialBatch"/> that contains the transaction.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? BatchId { get; set; }

        /// <summary>
        /// Gets or sets the gateway identifier.
        /// </summary>
        /// <value>
        /// The gateway identifier.
        /// </value>
        [DataMember]
        public int? FinancialGatewayId { get; set; }

        /// <summary>
        /// Gets or sets the financial payment detail identifier.
        /// </summary>
        /// <value>
        /// The financial payment detail identifier.
        /// </value>
        [DataMember]
        public int? FinancialPaymentDetailId { get; set; }

        /// <summary>
        /// Gets or sets date and time that the transaction occurred. This is the local server time.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the time that the transaction occurred. This is the local server time.
        /// </value>
        [DataMember]
        [Index( "IX_TransactionDateTime_TransactionTypeValueId_Person", 0 )]
        public DateTime? TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets date and time that the transaction should be processed after. This is the local server time.
        /// </summary>
        [DataMember]
        public DateTime? FutureProcessingDateTime { get; set; }

        /// <summary>
        /// For Credit Card transactions, this is the response code that the gateway returns.
        /// For Scanned Checks, this is the check number.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the transaction code of the transaction.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets a summary of the transaction. This would store any comments made.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a summary of the transaction.
        /// </value>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the TransactionType <see cref="Rock.Model.DefinedValue"/> indicating
        /// the type of the transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the TransactionType <see cref="Rock.Model.DefinedValue"/> for this transaction.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE )]
        [Index( "IX_TransactionDateTime_TransactionTypeValueId_Person", 1 )]
        public int TransactionTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the source type <see cref="Rock.Model.DefinedValue"/> for this transaction. Representing the source (method) of this transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the source type <see cref="Rock.Model.DefinedValue"/> for this transaction.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE )]
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets an encrypted version of a scanned check's raw track of the MICR data.
        /// Note that different scanning hardware might use different special characters for fields such as Transit and On-US.
        /// Also, encryption of the same values results in different encrypted data, so this field can't be used for check matching
        /// </summary>
        /// <value>
        /// The check micr encrypted.
        /// A <see cref="System.String"/> representing an encrypted version of a scanned check's MICR track data
        /// </value>
        [DataMember]
        [HideFromReporting]
        public string CheckMicrEncrypted { get; set; }

        /// <summary>
        /// One Way Encryption (SHA1 Hash) of Raw Track of the MICR read. The same raw MICR will result in the same hash.
        /// Enables detection of duplicate scanned checks
        /// Note: duplicate detection requires that the duplicate check was scanned using the same scanner type (Ranger vs Magtek)
        /// </summary>
        /// <value>
        /// The check micr hash.
        /// </value>
        [DataMember]
        [MaxLength( 128 )]
        [Index]
        [HideFromReporting]
        public string CheckMicrHash { get; set; }

        /// <summary>
        /// Gets or sets the micr status (if this Transaction is from a scanned check)
        /// Fail means that the check scanner detected a bad MICR read, but the user choose to Upload it anyway
        /// </summary>
        /// <value>
        /// The micr status.
        /// </value>
        [DataMember]
        [HideFromReporting]
        public MICRStatus? MICRStatus { get; set; }

        /// <summary>
        /// Gets or sets an encrypted version of a scanned check's parsed MICR in the format {routingnumber}_{accountnumber}_{checknumber}
        /// </summary>
        /// <value>
        /// The check micr encrypted.
        /// A <see cref="System.String"/> representing an encrypted version of a scanned check's parsed MICR data in the format {routingnumber}_{accountnumber}_{checknumber}
        /// </value>
        [DataMember]
        [HideFromReporting]
        public string CheckMicrParts { get; set; }

        /// <summary>
        /// Gets or sets the ScheduledTransactionId of the <see cref="Rock.Model.FinancialScheduledTransaction" /> that triggered
        /// this transaction. If this was an ad-hoc/on demand transaction, this property will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the ScheduledTransactionId of the <see cref="Rock.Model.FinancialScheduledTransaction"/>
        /// </value>
        [DataMember]
        public int? ScheduledTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the PersonAliasId of the <see cref="Rock.Model.PersonAlias"/> who processed the transaction. For example, if the transaction is
        /// from a scanned check, the ProcessedByPersonAlias is the person who matched (or started to match) the check to the person who wrote the check.
        /// </summary>
        /// <value>
        /// The processed by person alias identifier.
        /// </value>
        public int? ProcessedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the processed date time. For example, if the transaction is from a scanned check, the ProcessedDateTime is when the transaction
        /// was matched (or started to match) to the person who wrote the check.
        /// </summary>
        /// <value>
        /// The processed date time.
        /// </value>
        public DateTime? ProcessedDateTime { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the transaction has been settled by the processor/gateway.
        /// </summary>
        /// <value>
        /// The is settled.
        /// </value>
        [DataMember]
        public bool? IsSettled { get; set; }

        /// <summary>
        /// The group/batch identifier used by the processor/gateway when the transaction has been settled.
        /// </summary>
        /// <value>
        /// The settled group identifier.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string SettledGroupId { get; set; }

        /// <summary>
        /// Gets or sets the date that the transaction was settled by the processor/gateway.
        /// </summary>
        /// <value>
        /// The settled date.
        /// </value>
        [DataMember]
        public DateTime? SettledDate { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if the transaction has been reconciled or not.
        /// </summary>
        /// <value>
        /// The is settled.
        /// </value>
        [DataMember]
        public bool? IsReconciled { get; set; }

        /// <summary>
        /// Gets the status of the transaction provided by the payment gateway (i.e. Pending, Complete, Failed)
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [DataMember]
        [MaxLength( 50 )]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the status message.
        /// </summary>
        /// <value>
        /// The status message.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets the non cash asset type <see cref="Rock.Model.DefinedValue"/> identifier.
        /// </summary>
        /// <value>
        /// The non cash asset type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_NONCASH_ASSET_TYPE )]
        public int? NonCashAssetTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the foreign currency code value identifier.
        /// </summary>
        /// <value>
        /// The foreign currency code value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_CURRENCY_CODE )]
        public int? ForeignCurrencyCodeValueId { get; set; }

        /// <summary>
        /// Gets Sunday date.
        /// </summary>
        /// <value>
        /// The Sunday date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        [Index( "IX_SundayDate" )]
        public DateTime? SundayDate
        {
            get
            {
                // NOTE: This is the In-Memory get, LinqToSql will get the value from the database.
                // Also, on an Insert/Update this will be the value saved to the database
                return TransactionDateTime?.SundayDate();
            }

            set
            {
                // don't do anything here since EF uses this for loading, and we also want to ignore if somebody other than EF tries to set this
            }
        }

        /// <summary>
        /// Gets the transaction date key.
        /// </summary>
        /// <value>
        /// The transaction date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? TransactionDateKey
        {
            get => ( TransactionDateTime == null || TransactionDateTime.Value == default ) ?
                        ( int? ) null :
                        TransactionDateTime.Value.ToString( "yyyyMMdd" ).AsInteger();

            private set { }
        }

        /// <summary>
        /// Gets the settled date key.
        /// </summary>
        /// <value>
        /// The settled date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? SettledDateKey
        {
            get => ( SettledDate == null || SettledDate.Value == default ) ?
                        ( int? ) null :
                        SettledDate.Value.ToString( "yyyyMMdd" ).AsInteger();

            private set { }
        }

        #endregion Entity Properties

        #region Navigation Properties

        /// <summary>
        /// Gets or sets the authorized <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The authorized person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias AuthorizedPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialBatch"/> that contains the transaction.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.FinancialBatch"/> that contains the transaction.
        /// </value>
        [LavaVisible]
        public virtual FinancialBatch Batch { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialGateway">gateway</see>.
        /// </summary>
        /// <value>
        /// The gateway.
        /// </value>
        [DataMember]
        public virtual FinancialGateway FinancialGateway { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialPaymentDetail"/>.
        /// </summary>
        /// <value>
        /// The financial payment detail.
        /// </value>
        [DataMember]
        public virtual FinancialPaymentDetail FinancialPaymentDetail { get; set; }

        /// <summary>
        /// Gets or sets the transaction type <see cref="Rock.Model.DefinedValue"/> indicating the type of transaction that occurred.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> indicating the type of transaction that occurred.
        /// </value>
        [DataMember]
        public virtual DefinedValue TransactionTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the source type <see cref="Rock.Model.DefinedValue"/> indicating where the transaction originated from; the source of the transaction.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> indicating where the transaction originated from; the source of the transaction.
        /// </value>
        [DataMember]
        public virtual DefinedValue SourceTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialTransactionRefund">refund</see> transaction that is associated with this transaction. If this transaction is not a refund transaction this value will be null.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialTransactionRefund">refund transaction</see> associated with this transaction. This will be null if the transaction
        /// is not a refund transaction.
        /// </value>
        [LavaVisible]
        public virtual FinancialTransactionRefund RefundDetails { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialScheduledTransaction">Scheduled Transaction</see> that initiated this transaction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialScheduledTransaction"/> that initiated this transaction.
        /// </value>
        [LavaVisible]
        public virtual FinancialScheduledTransaction ScheduledTransaction { get; set; }

        /// <summary>
        /// Gets or sets the PersonAlias of the <see cref="Rock.Model.PersonAlias"/> who processed the transaction. For example, if the transaction is
        /// from a scanned check, the ProcessedByPersonAlias is the person who matched (or started to match) the check to the person who wrote the check.
        /// </summary>
        /// <value>
        /// The processed by person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias ProcessedByPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialTransactionDetail">Transaction Detail</see> line items for this transaction.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.FinancialTransactionDetail" /> line items for this transaction.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialTransactionDetail> TransactionDetails
        {
            get { return _transactionDetails ?? ( _transactionDetails = new Collection<FinancialTransactionDetail>() ); }
            set { _transactionDetails = value; }
        }

        private ICollection<FinancialTransactionDetail> _transactionDetails;

        /// <summary>
        /// Gets or sets a collection containing any <see cref="Rock.Model.FinancialTransactionImage">images</see> associated with this transaction. An example of this
        /// would be a scanned image of a check.
        /// </summary>
        /// <value>
        /// A collection of <see cref="Rock.Model.FinancialTransactionImage">FinancialTransactionImages</see> associated with this transaction.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialTransactionImage> Images
        {
            get { return _images ?? ( _images = new Collection<FinancialTransactionImage>() ); }
            set { _images = value; }
        }

        private ICollection<FinancialTransactionImage> _images;

        /// <summary>
        /// Gets or sets the refunds.
        /// </summary>
        /// <value>
        /// The refunds.
        /// </value>
        [LavaVisible]
        public virtual ICollection<FinancialTransactionRefund> Refunds
        {
            get { return _refunds ?? ( _refunds = new Collection<FinancialTransactionRefund>() ); }
            set { _refunds = value; }
        }

        private ICollection<FinancialTransactionRefund> _refunds;

        /// <summary>
        /// Gets or sets the history change list.
        /// </summary>
        /// <value>
        /// The history change list.
        /// </value>
        [NotMapped]
        [RockObsolete( "1.14" )]
        [Obsolete( "Does nothing. No longer needed. We replaced this with a private property under the SaveHook class for this entity.", true )]
        public virtual History.HistoryChangeList HistoryChangeList { get; set; }

        /// <summary>
        /// Gets or sets the batch history change list.
        /// </summary>
        /// <value>
        /// The batch history change list.
        /// </value>
        [NotMapped]
        [RockObsolete( "1.14" )]
        [Obsolete( "Does nothing. No longer needed. We replaced this with a private property under the SaveHook class for this entity.", true )]
        public virtual Dictionary<int, History.HistoryChangeList> BatchHistoryChangeList { get; set; }

        /// <summary>
        /// Gets or sets the non cash asset type <see cref="Rock.Model.DefinedValue"/>.
        /// </summary>
        /// <value>
        /// The non cash asset type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue NonCashAssetTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the transaction source date.
        /// </summary>
        /// <value>
        /// The transaction source date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate TransactionSourceDate { get; set; }

        /// <summary>
        /// Gets or sets the settled source date.
        /// </summary>
        /// <value>
        /// The settled source date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate SettledSourceDate { get; set; }

        /// <summary>
        /// Provides a <see cref="Dictionary{TKey, TValue}"/> of actions that this model supports, and the description of each.
        /// </summary>
        public override Dictionary<string, string> SupportedActions
        {
            get
            {
                var supportedActions = base.SupportedActions;
                supportedActions.AddOrReplace( "Refund", "The roles and/or users that have access to refund a transaction." );
                return supportedActions;
            }
        }

        #endregion Navigation Properties
    }

    #region Entity Configuration

    /// <summary>
    /// Transaction Configuration class.
    /// </summary>
    public partial class FinancialTransactionConfiguration : EntityTypeConfiguration<FinancialTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionConfiguration"/> class.
        /// </summary>
        public FinancialTransactionConfiguration()
        {
            this.HasOptional( t => t.AuthorizedPersonAlias ).WithMany().HasForeignKey( t => t.AuthorizedPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Batch ).WithMany( t => t.Transactions ).HasForeignKey( t => t.BatchId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.FinancialGateway ).WithMany().HasForeignKey( t => t.FinancialGatewayId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.FinancialPaymentDetail ).WithMany().HasForeignKey( t => t.FinancialPaymentDetailId ).WillCascadeOnDelete( false );
            this.HasRequired( t => t.TransactionTypeValue ).WithMany().HasForeignKey( t => t.TransactionTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.SourceTypeValue ).WithMany().HasForeignKey( t => t.SourceTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.RefundDetails ).WithRequired( r => r.FinancialTransaction ).WillCascadeOnDelete( true );
            this.HasOptional( t => t.ScheduledTransaction ).WithMany( s => s.Transactions ).HasForeignKey( t => t.ScheduledTransactionId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.ProcessedByPersonAlias ).WithMany().HasForeignKey( t => t.ProcessedByPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.NonCashAssetTypeValue ).WithMany().HasForeignKey( t => t.NonCashAssetTypeValueId ).WillCascadeOnDelete( false );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier OccurrenceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasOptional( r => r.TransactionSourceDate ).WithMany().HasForeignKey( r => r.TransactionDateKey ).WillCascadeOnDelete( false );
            this.HasOptional( r => r.SettledSourceDate ).WithMany().HasForeignKey( r => r.SettledDateKey ).WillCascadeOnDelete( false );
        }
    }

    #endregion Entity Configuration
}
