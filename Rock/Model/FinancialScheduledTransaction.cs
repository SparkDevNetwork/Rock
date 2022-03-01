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
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Lava;
using Rock.Utility;

namespace Rock.Model
{
    /// <summary>
    /// Represents a financial transaction schedule in Rock. A user can schedule transactions for varying frequencies, number of transactions and 
    /// and time period. A scheduled transaction can include multiple <see cref="Rock.Model.FinancialScheduledTransactionDetail"/> items so that a single 
    /// scheduled transaction can include payments/gifts for multiple <see cref="Rock.Model.FinancialAccount">Financial Accounts</see>/accounts.
    /// </summary>
    /// <remarks>
    /// Several examples include - A one time transaction to occur on 1/1/2014; an ongoing weekly transaction; a weekly transaction for 10 weeks; a monthly transaction from 1/1/2014 - 12/31/2014.
    /// </remarks>
    [RockDomain( "Finance" )]
    [Table( "FinancialScheduledTransaction" )]
    [DataContract]
    public partial class FinancialScheduledTransaction : Model<FinancialScheduledTransaction>, IHasActiveFlag
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the authorized person alias identifier.
        /// </summary>
        /// <value>
        /// The authorized person alias identifier.
        /// </value>
        [DataMember]
        public int AuthorizedPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the transaction type value identifier.
        /// </summary>
        /// <value>
        /// The transaction type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE )]
        public int? TransactionTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the source type value identifier.
        /// </summary>
        /// <value>
        /// The source type value identifier.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE )]
        public int? SourceTypeValueId { get; set; }
        
        /// <summary>
        /// Gets or sets the DefinedValueId of the transaction frequency <see cref="Rock.Model.DefinedValue"/> that represents the frequency that this 
        /// transaction will occur.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of this transaction's frequency <see cref="Rock.Model.DefinedValue"/>.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_FREQUENCY )]
        public int TransactionFrequencyValueId { get; set; }

        /// <summary>
        /// Gets or sets the start date for this schedule. The first transaction will occur on or after this date.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the start date for this schedule.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date for this transaction schedule. Transactions will cease to occur on or before this date.  This property is nullable for ongoing 
        /// schedules or for schedules that will end after a specified number of payments/transaction occur (in the <see cref="NumberOfPayments"/> property).
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the end date for this transaction schedule. If there isn't an end date for this transaction schedule
        /// this value will be null.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of times that this payment should repeat in this schedule.  If there is not a set number of payments, this value will be null. 
        /// This property is overridden by the schedule's <see cref="EndDate"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32" /> representing the maximum number of times that this payment should repeat.
        /// </value>
        [DataMember]
        public int? NumberOfPayments { get; set; }

        /// <summary>
        /// Gets or sets the date of the next payment in this schedule.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date of the next payment in this schedule.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? NextPaymentDate { get; set; }

        /// <summary>
        /// Gets or sets the date and time of the last status update. In other words,
        /// the date and time the gateway was last queried for the status of the scheduled profile/transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime" /> representing the date and time of the last status update.
        /// </value>
        [DataMember]
        public DateTime? LastStatusUpdateDateTime { get; set; }

        /// <summary>
        /// The status of the scheduled transactions provided by the payment gateway (i.e. Active, Cancelled, etc).
        /// If the gateway doesn't have a status field, this will be null;
        /// The payment gateway component maps this based on the <seealso cref="StatusMessage"/>.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [DataMember]
        public FinancialScheduledTransactionStatus? Status { get; set; }

        /// <summary>
        /// Gets or sets the raw scheduled transaction status message returned from the Gateway
        /// If the gateway doesn't have a status field, this will be null;
        /// </summary>
        /// <value>
        /// The status message.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string StatusMessage { get; set; }

        /// <summary>
        /// Gets or sets a flag indicating if this scheduled transaction is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this scheduled transaction is active; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; } = true;

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
        /// Gets or sets the transaction code used for this scheduled transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the transaction code for this scheduled transaction.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets a summary of the scheduled transaction. This would store any comments made.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a summary of the scheduled transaction.
        /// </value>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the payment gateway's payment schedule key/identifier.  This is the value that uniquely identifies the payment schedule on 
        /// with the payment gateway.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the financial gateway's unique identifier for the payment schedule.
        /// </value>
        [DataMember]
        public string GatewayScheduleId { get; set; }

        /// <summary>
        /// The JSON for <see cref="PreviousGatewayScheduleIds"/>. If this is null,
        /// there are no PreviousGatewayScheduleIds.
        /// </summary>
        /// <value></value>
        [DataMember]
        public string PreviousGatewayScheduleIdsJson
        {
            get
            {
                // If there are any PreviousGatewayScheduleIds, store them as JSON.
                // Otherwise, store as NULL so it is easy to figure out which scheduled transaction have PreviousGatewayScheduleIds.
                if ( PreviousGatewayScheduleIds != null && PreviousGatewayScheduleIds.Any() )
                {
                    // at least one PreviousGatewayScheduleId, so store it in the database
                    return PreviousGatewayScheduleIds?.ToJson();
                }
                else
                {
                    // no PreviousGatewayScheduleIds, so leave PreviousGatewayScheduleIdsJson as null;
                    return null;
                }
            }

            set
            {
                PreviousGatewayScheduleIds = value.FromJsonOrNull<List<string>>() ?? new List<string>();
            }
        }

        /// <summary>
        /// Gets or sets the date to remind user to update scheduled transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date to remind the user to update the scheduled transaction.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? CardReminderDate { get; set; }

        /// <summary>
        /// Gets or sets the date that user was last reminded to update scheduled transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date that the user was last reminded to update the scheduled transaction.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? LastRemindedDate { get; set; }

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
        /// Gets or sets the inactivate date time.
        /// </summary>
        /// <value>
        /// The inactivate date time.
        /// </value>
        [DataMember]
        public DateTime? InactivateDateTime { get; set; }
        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the authorized <see cref="Rock.Model.PersonAlias"/>.
        /// </summary>
        /// <value>
        /// The authorized person alias.
        /// </value>
        [LavaVisible]
        public virtual PersonAlias AuthorizedPersonAlias { get; set; }

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
        /// Gets or sets the foreign currency code type <see cref="Rock.Model.DefinedValue"/> indicating where the transaction originated from; the source of the transaction.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> representing the foreign currency code.
        /// </value>
        [DataMember]
        public virtual DefinedValue ForeignCurrencyCodeValue { get; set; }

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
        /// Gets or sets the <see cref="Rock.Model.DefinedValue"/> representing the payment frequency associated with this 
        /// scheduled transaction.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> representing the payment frequency associated with this scheduled transaction.
        /// </value>
        [DataMember]
        public virtual DefinedValue TransactionFrequencyValue { get; set; }


        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialScheduledTransactionDetail">transaction details</see> for this scheduled transaction.
        /// </summary>
        /// <value>
        /// A collection containing the <see cref="Rock.Model.FinancialScheduledTransactionDetail">FinancialScheduleTransactionDetails</see> for this scheduled transaction.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialScheduledTransactionDetail> ScheduledTransactionDetails
        {
            get { return _scheduledTransactionDetails ?? ( _scheduledTransactionDetails = new Collection<FinancialScheduledTransactionDetail>() ); }
            set { _scheduledTransactionDetails = value; }
        }
        private ICollection<FinancialScheduledTransactionDetail> _scheduledTransactionDetails;

        /// <summary>
        /// Gets or sets <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> that have been processed and have cleared for this scheduled transaction profile. 
        /// </summary>
        /// <value>
        /// A collection of the processed and cleared <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> for this scheduled transaction profile.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialTransaction> Transactions
        {
            get { return _transactions ?? ( _transactions = new Collection<FinancialTransaction>() ); }
            set { _transactions = value; }
        }
        private ICollection<FinancialTransaction> _transactions;

        /// <summary>
        /// Gets the total amount.
        /// </summary>
        /// <value>
        /// The total amount.
        /// </value>
        [LavaVisible]
        public decimal TotalAmount 
        {
            get { return ScheduledTransactionDetails.Sum( d => d.Amount ); }
        }

        /// <summary>
        /// Gets or sets the history change list.
        /// </summary>
        /// <value>
        /// The history change list.
        /// </value>
        [NotMapped]
        public virtual History.HistoryChangeList HistoryChangeList { get; set; }

        /// <summary>
        /// This will be any previous <see cref="GatewayScheduleId"/> that this <see cref="FinancialScheduledTransaction"/> has had.
        /// This might be used in a case where a <see cref="Rock.Financial.GatewayComponent" /> may have changed what schedule id it used.
        /// </summary>
        /// <value>The previous gateway schedule ids.</value>
        [NotMapped]
        public virtual List<string> PreviousGatewayScheduleIds { get; set; } = new List<string>();

        #endregion

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
        /// Method that will be called on an entity immediately before the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry">The database entity entry.</param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, DbEntityEntry entry )
        {
            var rockContext = (RockContext)dbContext;

            HistoryChangeList = new History.HistoryChangeList();

            switch ( entry.State )
            {
                case EntityState.Added:
                    {
                        HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Transaction" );

                        string person = History.GetValue<PersonAlias>( AuthorizedPersonAlias, AuthorizedPersonAliasId, rockContext );

                        History.EvaluateChange( HistoryChangeList, "Authorized Person", string.Empty, person );
                        History.EvaluateChange( HistoryChangeList, "Gateway", string.Empty, History.GetValue<FinancialGateway>( FinancialGateway, FinancialGatewayId, rockContext ) );
                        History.EvaluateChange( HistoryChangeList, "Gateway Schedule Id", string.Empty, GatewayScheduleId );
                        History.EvaluateChange( HistoryChangeList, "Transaction Code", string.Empty, TransactionCode );
                        History.EvaluateChange( HistoryChangeList, "Summary", string.Empty, Summary );
                        History.EvaluateChange( HistoryChangeList, "Type", ( null as int? ), TransactionTypeValue, TransactionTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Source", ( null as int? ), SourceTypeValue, SourceTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Frequency", ( null as int? ), TransactionFrequencyValue, TransactionFrequencyValueId );
                        History.EvaluateChange( HistoryChangeList, "Start Date", ( null as DateTime? ), StartDate );
                        History.EvaluateChange( HistoryChangeList, "End Date", ( null as DateTime? ), EndDate );
                        History.EvaluateChange( HistoryChangeList, "Number of Payments", ( null as int? ), NumberOfPayments );
                        History.EvaluateChange( HistoryChangeList, "Is Active", ( null as bool? ), IsActive );
                        History.EvaluateChange( HistoryChangeList, "Card Reminder Date", ( null as DateTime? ), CardReminderDate );
                        History.EvaluateChange( HistoryChangeList, "Last Reminded Date", ( null as DateTime? ), LastRemindedDate );
                        var isOrganizationCurrency = new RockCurrencyCodeInfo( ForeignCurrencyCodeValueId ).IsOrganizationCurrency;
                        if ( !isOrganizationCurrency )
                        {
                            History.EvaluateChange( HistoryChangeList, "Currency Code", ( null as int? ), ForeignCurrencyCodeValue, ForeignCurrencyCodeValueId );
                        }

                        break;
                    }

                case EntityState.Modified:
                    {
                        string origPerson = History.GetValue<PersonAlias>( null, entry.OriginalValues["AuthorizedPersonAliasId"].ToStringSafe().AsIntegerOrNull(), rockContext );
                        string person = History.GetValue<PersonAlias>( AuthorizedPersonAlias, AuthorizedPersonAliasId, rockContext );
                        History.EvaluateChange( HistoryChangeList, "Authorized Person", origPerson, person );

                        int? origGatewayId = entry.OriginalValues["FinancialGatewayId"].ToStringSafe().AsIntegerOrNull();
                        if ( !FinancialGatewayId.Equals( origGatewayId ) )
                        {
                            History.EvaluateChange( HistoryChangeList, "Gateway", History.GetValue<FinancialGateway>( null, origGatewayId, rockContext ), History.GetValue<FinancialGateway>( FinancialGateway, FinancialGatewayId, rockContext ) );
                        }

                        History.EvaluateChange( HistoryChangeList, "Gateway Schedule Id", entry.OriginalValues["GatewayScheduleId"].ToStringSafe(), GatewayScheduleId );
                        History.EvaluateChange( HistoryChangeList, "Transaction Code", entry.OriginalValues["TransactionCode"].ToStringSafe(), TransactionCode );
                        History.EvaluateChange( HistoryChangeList, "Summary", entry.OriginalValues["Summary"].ToStringSafe(), Summary );
                        History.EvaluateChange( HistoryChangeList, "Type", entry.OriginalValues["TransactionTypeValueId"].ToStringSafe().AsIntegerOrNull(), TransactionTypeValue, TransactionTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Source", entry.OriginalValues["SourceTypeValueId"].ToStringSafe().AsIntegerOrNull(), SourceTypeValue, SourceTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Frequency", entry.OriginalValues["TransactionFrequencyValueId"].ToStringSafe().AsIntegerOrNull(), TransactionFrequencyValue, TransactionFrequencyValueId );
                        History.EvaluateChange( HistoryChangeList, "Start Date", entry.OriginalValues["StartDate"].ToStringSafe().AsDateTime(), StartDate );
                        History.EvaluateChange( HistoryChangeList, "End Date", entry.OriginalValues["EndDate"].ToStringSafe().AsDateTime(), EndDate );
                        History.EvaluateChange( HistoryChangeList, "Number of Payments", entry.OriginalValues["EndDate"].ToStringSafe().AsIntegerOrNull(), NumberOfPayments );
                        History.EvaluateChange( HistoryChangeList, "Is Active", entry.OriginalValues["IsActive"].ToStringSafe().AsBooleanOrNull(), IsActive );
                        History.EvaluateChange( HistoryChangeList, "Card Reminder Date", entry.OriginalValues["CardReminderDate"].ToStringSafe().AsDateTime(), CardReminderDate );
                        History.EvaluateChange( HistoryChangeList, "Last Reminded Date", entry.OriginalValues["LastRemindedDate"].ToStringSafe().AsDateTime(), LastRemindedDate );
                        History.EvaluateChange( HistoryChangeList, "Currency Code", entry.OriginalValues["ForeignCurrencyCodeValueId"].ToStringSafe().AsIntegerOrNull(), ForeignCurrencyCodeValue, ForeignCurrencyCodeValueId );

                        break;
                    }

                case EntityState.Deleted:
                    {
                        HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Transaction" );

                        // If a FinancialPaymentDetail was linked to this FinancialScheduledTransaction and is now orphaned, delete it.
                        var financialPaymentDetailService = new FinancialPaymentDetailService( rockContext );
                        financialPaymentDetailService.DeleteOrphanedFinancialPaymentDetail( entry );

                        break;
                    }
            }

            base.PreSaveChanges( dbContext, entry );
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Data.DbContext dbContext )
        {
            if ( HistoryChangeList?.Any() == true )
            {
                HistoryService.SaveChanges( (RockContext)dbContext, typeof( FinancialScheduledTransaction ), Rock.SystemGuid.Category.HISTORY_FINANCIAL_TRANSACTION.AsGuid(), this.Id, HistoryChangeList, true, this.ModifiedByPersonAliasId );
            }

            base.PostSaveChanges( dbContext );
        }


        #endregion

    }

    #region Entity Configuration


    /// <summary>
    /// Scheduled Transaction Configuration class.
    /// </summary>
    public partial class FinancialScheduledTransactionConfiguration : EntityTypeConfiguration<FinancialScheduledTransaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialScheduledTransactionConfiguration"/> class.
        /// </summary>
        public FinancialScheduledTransactionConfiguration()
        {
            this.HasRequired( t => t.AuthorizedPersonAlias ).WithMany().HasForeignKey( t => t.AuthorizedPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.TransactionTypeValue ).WithMany().HasForeignKey( t => t.TransactionTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.SourceTypeValue ).WithMany().HasForeignKey( t => t.SourceTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.FinancialGateway ).WithMany().HasForeignKey( t => t.FinancialGatewayId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.FinancialPaymentDetail ).WithMany().HasForeignKey( t => t.FinancialPaymentDetailId ).WillCascadeOnDelete( false );
            this.HasRequired( t => t.TransactionFrequencyValue ).WithMany().HasForeignKey( t => t.TransactionFrequencyValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.ForeignCurrencyCodeValue ).WithMany().HasForeignKey( t => t.ForeignCurrencyCodeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The status of a Scheduled Transaction
    /// </summary>
    public enum FinancialScheduledTransactionStatus
    {
        /// <summary>
        /// Scheduled Transaction is operating normally
        /// </summary>
        Active = 0,

        /// <summary>
        /// Scheduled Transaction completed
        /// </summary>
        Completed = 1,

        /// <summary>
        /// Scheduled Transaction is paused
        /// </summary>
        Paused = 2,

        /// <summary>
        /// Scheduled Transaction is cancelled
        /// </summary>
        Canceled = 3,

        /// <summary>
        /// Scheduled Transaction is failed
        /// </summary>
        Failed = 4,

        /// <summary>
        /// Scheduled Transaction is Past Due
        /// </summary>
        PastDue = 5
    }

    #endregion

}