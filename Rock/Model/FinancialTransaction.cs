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
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a financial transaction in Rock.
    /// </summary>
    [Table( "FinancialTransaction" )]
    [DataContract]
    public partial class FinancialTransaction : Model<FinancialTransaction>, IAnalytic
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
        /// For Credit Card transactions, this is the response code that the gateway returns 
        /// For Scanned Checks, this is the check number
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the transaction code of the transaction.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets a summary of the transaction.
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
        /// Gets or sets the processed date time. For example, if the transaction is from a scanned check, the ProcessedDateTime is when is when the transaction 
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
        /// Gets or sets the sunday date.
        /// </summary>
        /// <value>
        /// The sunday date.
        /// </value>
        [DataMember]
        [DatabaseGenerated( DatabaseGeneratedOption.Computed )]
        [Column( TypeName = "Date" )]
        public DateTime? SundayDate { get; set; }

        #endregion Entity Properties

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the authorized person alias.
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
        [LavaInclude]
        public virtual FinancialBatch Batch { get; set; }

        /// <summary>
        /// Gets or sets the gateway.
        /// </summary>
        /// <value>
        /// The gateway.
        /// </value>
        [DataMember]
        public virtual FinancialGateway FinancialGateway { get; set; }

        /// <summary>
        /// Gets or sets the financial payment detail.
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
        [LavaInclude]
        public virtual FinancialTransactionRefund RefundDetails { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialScheduledTransaction">Scheduled Transaction</see> that initiated this transaction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialScheduledTransaction"/> that initiated this transaction.
        /// </value>
        [LavaInclude]
        public virtual FinancialScheduledTransaction ScheduledTransaction { get; set; }

        /// <summary>
        /// Gets or sets the PersonAlias of the <see cref="Rock.Model.PersonAlias"/> who processed the transaction. For example, if the transaction is 
        /// from a scanned check, the ProcessedByPersonAlias is the person who matched (or started to match) the check to the person who wrote the check.
        /// </summary>
        /// <value>
        /// The processed by person alias.
        /// </value>
        [LavaInclude]
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
        [LavaInclude]
        public virtual ICollection<FinancialTransactionRefund> Refunds
        {
            get { return _refunds ?? ( _refunds = new Collection<FinancialTransactionRefund>() ); }
            set { _refunds = value; }
        }
        private ICollection<FinancialTransactionRefund> _refunds;

        /// <summary>
        /// Gets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        [LavaInclude]
        [BoundFieldTypeAttribute( typeof( Rock.Web.UI.Controls.CurrencyField ) )]
        public virtual decimal TotalAmount
        {
            get { return TransactionDetails.Sum( d => d.Amount ); }
        }

        /// <summary>
        /// Gets or sets the history changes.
        /// </summary>
        /// <value>
        /// The history changes.
        /// </value>
        [NotMapped]
        public List<string> HistoryChanges { get; set; }

        /// <summary>
        /// Gets or sets the batch history changes.
        /// </summary>
        /// <value>
        /// The batch history changes.
        /// </value>
        [NotMapped]
        private Dictionary<int, List<string>> BatchHistoryChanges { get; set; }

        #endregion Virtual Properties

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this transaction.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this transaction.
        /// </returns>
        public override string ToString()
        {
            return this.TotalAmount.ToStringSafe();
        }

        /// <summary>
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            var rockContext = (RockContext)dbContext;

            HistoryChanges = new List<string>();
            BatchHistoryChanges = new Dictionary<int, List<string>>();

            switch ( entry.State )
            {
                case System.Data.Entity.EntityState.Added:
                    {
                        HistoryChanges.Add( "Created Transaction" );

                        string person = History.GetValue<PersonAlias>( AuthorizedPersonAlias, AuthorizedPersonAliasId, rockContext );

                        History.EvaluateChange( HistoryChanges, "Authorized Person", string.Empty, person );
                        History.EvaluateChange( HistoryChanges, "Batch", string.Empty, History.GetValue<FinancialBatch>( Batch, BatchId, rockContext ) );
                        History.EvaluateChange( HistoryChanges, "Gateway", string.Empty, History.GetValue<FinancialGateway>( FinancialGateway, FinancialGatewayId, rockContext ) );
                        History.EvaluateChange( HistoryChanges, "Transaction Date/Time", (DateTime?)null, TransactionDateTime );
                        History.EvaluateChange( HistoryChanges, "Transaction Code", string.Empty, TransactionCode );
                        History.EvaluateChange( HistoryChanges, "Summary", string.Empty, Summary );
                        History.EvaluateChange( HistoryChanges, "Type", (int?)null, TransactionTypeValue, TransactionTypeValueId );
                        History.EvaluateChange( HistoryChanges, "Source", (int?)null, SourceTypeValue, SourceTypeValueId );
                        History.EvaluateChange( HistoryChanges, "Scheduled Transaction Id", (int?)null, ScheduledTransactionId );
                        History.EvaluateChange( HistoryChanges, "Processed By", string.Empty, History.GetValue<PersonAlias>( ProcessedByPersonAlias, ProcessedByPersonAliasId, rockContext ) );
                        History.EvaluateChange( HistoryChanges, "Processed Date/Time", (DateTime?)null, ProcessedDateTime );
                        History.EvaluateChange( HistoryChanges, "Status", string.Empty, Status );
                        History.EvaluateChange( HistoryChanges, "Status Message", string.Empty, StatusMessage );

                        int? batchId = Batch != null ? Batch.Id : BatchId;
                        if ( batchId.HasValue )
                        {
                            BatchHistoryChanges.Add( batchId.Value, new List<string> { string.Format( "Added <span class='field-name'>{0:C2}</span> transaction for <span class='field-value'>{1}</span>.", this.TotalAmount, person ) } );
                        }

                        break;
                    }

                case System.Data.Entity.EntityState.Modified:
                    {
                        string origPerson = History.GetValue<PersonAlias>( null, entry.OriginalValues["AuthorizedPersonAliasId"].ToStringSafe().AsIntegerOrNull(), rockContext );
                        string person = History.GetValue<PersonAlias>( AuthorizedPersonAlias, AuthorizedPersonAliasId, rockContext );
                        History.EvaluateChange( HistoryChanges, "Authorized Person", origPerson, person );

                        int? origBatchId = entry.OriginalValues["BatchId"].ToStringSafe().AsIntegerOrNull();
                        int? batchId = Batch != null ? Batch.Id : BatchId;
                        if ( !batchId.Equals( origBatchId ) )
                        {
                            string origBatch = History.GetValue<FinancialBatch>( null, origBatchId, rockContext );
                            string batch = History.GetValue<FinancialBatch>( Batch, BatchId, rockContext );
                            History.EvaluateChange( HistoryChanges, "Batch", origBatch, batch );
                        }

                        int? origGatewayId = entry.OriginalValues["FinancialGatewayId"].ToStringSafe().AsIntegerOrNull();
                        if ( !FinancialGatewayId.Equals( origGatewayId ) )
                        {
                            History.EvaluateChange( HistoryChanges, "Gateway", History.GetValue<FinancialGateway>( null, origGatewayId, rockContext ), History.GetValue<FinancialGateway>( FinancialGateway, FinancialGatewayId, rockContext ) );
                        }

                        History.EvaluateChange( HistoryChanges, "Transaction Date/Time", entry.OriginalValues["TransactionDateTime"].ToStringSafe().AsDateTime(), TransactionDateTime );
                        History.EvaluateChange( HistoryChanges, "Transaction Code", entry.OriginalValues["TransactionCode"].ToStringSafe(), TransactionCode );
                        History.EvaluateChange( HistoryChanges, "Summary", entry.OriginalValues["Summary"].ToStringSafe(), Summary );
                        History.EvaluateChange( HistoryChanges, "Type", entry.OriginalValues["TransactionTypeValueId"].ToStringSafe().AsIntegerOrNull(), TransactionTypeValue, TransactionTypeValueId );
                        History.EvaluateChange( HistoryChanges, "Source", entry.OriginalValues["SourceTypeValueId"].ToStringSafe().AsIntegerOrNull(), SourceTypeValue, SourceTypeValueId );
                        History.EvaluateChange( HistoryChanges, "Scheduled Transaction Id", entry.OriginalValues["ScheduledTransactionId"].ToStringSafe().AsIntegerOrNull(), ScheduledTransactionId );
                        History.EvaluateChange( HistoryChanges, "Processed By", entry.OriginalValues["ProcessedByPersonAliasId"].ToStringSafe().AsIntegerOrNull(), ProcessedByPersonAlias, ProcessedByPersonAliasId, rockContext );
                        History.EvaluateChange( HistoryChanges, "Processed Date/Time", entry.OriginalValues["ProcessedDateTime"].ToStringSafe().AsDateTime(), ProcessedDateTime );
                        History.EvaluateChange( HistoryChanges, "Status", entry.OriginalValues["Status"].ToStringSafe(), Status );
                        History.EvaluateChange( HistoryChanges, "Status Message", entry.OriginalValues["StatusMessage"].ToStringSafe(), StatusMessage );

                        if ( !batchId.Equals( origBatchId ) )
                        {
                            if ( origBatchId.HasValue )
                            {
                                BatchHistoryChanges.Add( origBatchId.Value, new List<string> { string.Format( "Removed <span class='field-name'>{0:C2}</span> transaction for <span class='field-value'>{1}</span>.", this.TotalAmount, person ) } );
                            }
                            if ( batchId.HasValue )
                            {
                                BatchHistoryChanges.Add( batchId.Value, new List<string> { string.Format( "Added <span class='field-name'>{0:C2}</span> transaction for <span class='field-value'>{1}</span>.", this.TotalAmount, person ) } );
                            }
                        }
                        else
                        {
                            if ( batchId.HasValue )
                            {
                                BatchHistoryChanges.Add( batchId.Value, new List<string> { string.Format( "Updated <span class='field-name'>Transaction</span> ID: <span class='field-value'>{0}</span>.", Id ) } );
                            }
                        }
                        break;
                    }

                case System.Data.Entity.EntityState.Deleted:
                    {
                        HistoryChanges.Add( "Deleted Transaction" );

                        int? batchId = Batch != null ? Batch.Id : BatchId;
                        if ( batchId.HasValue )
                        {
                            string batch = History.GetValue<FinancialBatch>( Batch, BatchId, rockContext );
                            string person = History.GetValue<PersonAlias>( AuthorizedPersonAlias, AuthorizedPersonAliasId, rockContext );
                            BatchHistoryChanges.Add( batchId.Value, new List<string> { string.Format( "Deleted <span class='field-name'>{0:C2}</span> transaction for <span class='field-value'>{1}</span>.", this.TotalAmount, person ) } );
                        }

                        // since images have a cascade delete relationship, make sure the PreSaveChanges gets called 
                        var childImages = new FinancialTransactionImageService( dbContext as RockContext ).Queryable().Where( a => a.TransactionId == this.Id );
                        foreach ( var image in childImages )
                        {
                            image.PreSaveChanges( dbContext, entry.State );
                        }
                        break;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( DbContext dbContext )
        {
            if ( HistoryChanges.Any() )
            {
                HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialTransaction ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), this.Id, HistoryChanges, true, this.ModifiedByPersonAliasId );
            }

            foreach ( var keyVal in BatchHistoryChanges )
            {
                if ( keyVal.Value.Any() )
                {
                    HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialBatch ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), keyVal.Key, keyVal.Value, string.Empty, typeof( FinancialTransaction ), this.Id, true, this.ModifiedByPersonAliasId );
                }
            }

            base.PostSaveChanges( dbContext );
        }

        #endregion
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
        }
    }

    #endregion Entity Configuration

    #region Enumerations

    /// <summary>
    /// The gender of a person
    /// </summary>
    public enum MICRStatus
    {
        /// <summary>
        /// Success means the scanned MICR contains no invalid read chars ('!' for Canon and '?' for Magtek)
        /// </summary>
        Success = 0,

        /// <summary>
        /// Fail means the scanned MICR contains at least one invalid read char ('!' for Canon and '?' for Magtek)
        /// but the user chose to Upload it anyway
        /// </summary>
        Fail = 1
    }

    /// <summary>
    /// For giving analysis reporting
    /// </summary>
    public enum TransactionGraphBy
    {
        /// <summary>
        /// The total
        /// </summary>
        Total = 0,

        /// <summary>
        /// The financial account
        /// </summary>
        FinancialAccount = 1,

        /// <summary>
        /// The campus
        /// </summary>
        Campus = 2,
    }

    #endregion

    #region Helper Classes

    /// <summary>
    /// Special Class to use when uploading a FinancialTransaction from a Scanned Check thru the Rest API.
    /// The Rest Client can't be given access to the DataEncryptionKey, so they'll upload it (using SSL)
    /// with the plain text CheckMicr and the Rock server will encrypt prior to saving to database
    /// </summary>
    [DataContract]
    [NotMapped]
    [RockClientInclude( "Special Class to use when uploading a FinancialTransaction from a Scanned Check thru the Rest API" )]
    public class FinancialTransactionScannedCheck
    {
        /// <summary>
        /// Gets or sets the financial transaction.
        /// </summary>
        /// <value>
        /// The financial transaction.
        /// </value>
        [DataMember]
        public FinancialTransaction FinancialTransaction { get; set; }

        /// <summary>
        /// Gets or sets the scanned check MICR (the raw track data)
        /// </summary>
        /// <value>
        /// The scanned check MICR.
        /// </value>
        [DataMember]
        public string ScannedCheckMicrData { get; set; }

        /// <summary>
        /// Gets or sets the scanned check parsed MICR in the format {routingnumber}_{accountnumber}_{checknumber}
        /// </summary>
        /// <value>
        /// The scanned check micr parts.
        /// </value>
        [DataMember]
        public string ScannedCheckMicrParts { get; set; }
    }

    #endregion
}