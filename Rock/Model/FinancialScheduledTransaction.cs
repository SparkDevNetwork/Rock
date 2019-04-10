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
        /// Gets or sets the the maximum number of times that this payment should repeat in this schedule.  If there is not a set number of payments, this value will be null. 
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
        /// Gets or sets a flag indicating if this scheduled transaction is active.
        /// </summary>
        /// <value>
        /// A <see cref="System.Boolean"/> that is <c>true</c> if this scheduled transaction is active; otherwise <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

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
        /// Gets or sets a summary of the scheduled transaction.
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

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the authorized person alias.
        /// </summary>
        /// <value>
        /// The authorized person alias.
        /// </value>
        [LavaInclude]
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
        [LavaInclude]
        public decimal TotalAmount 
        {
            get { return ScheduledTransactionDetails.Sum( d => d.Amount ); }
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
        /// Gets or sets the history change list.
        /// </summary>
        /// <value>
        /// The history change list.
        /// </value>
        [NotMapped]
        public virtual History.HistoryChangeList HistoryChangeList { get; set; }

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
        /// Pres the save.
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        /// <param name="entry"></param>
        public override void PreSaveChanges( Rock.Data.DbContext dbContext, System.Data.Entity.Infrastructure.DbEntityEntry entry )
        {
            var rockContext = (RockContext)dbContext;

            HistoryChangeList = new History.HistoryChangeList();

            switch ( entry.State )
            {
                case System.Data.Entity.EntityState.Added:
                    {
                        HistoryChangeList.AddChange( History.HistoryVerb.Add, History.HistoryChangeType.Record, "Transaction" );

                        string person = History.GetValue<PersonAlias>( AuthorizedPersonAlias, AuthorizedPersonAliasId, rockContext );

                        History.EvaluateChange( HistoryChangeList, "Authorized Person", string.Empty, person );
                        History.EvaluateChange( HistoryChangeList, "Gateway", string.Empty, History.GetValue<FinancialGateway>( FinancialGateway, FinancialGatewayId, rockContext ) );
                        History.EvaluateChange( HistoryChangeList, "Gateway Schedule Id", string.Empty, GatewayScheduleId );
                        History.EvaluateChange( HistoryChangeList, "Transaction Code", string.Empty, TransactionCode );
                        History.EvaluateChange( HistoryChangeList, "Type", (int?)null, TransactionTypeValue, TransactionTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Source", (int?)null, SourceTypeValue, SourceTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Frequency", (int?)null, TransactionFrequencyValue, TransactionFrequencyValueId);
                        History.EvaluateChange( HistoryChangeList, "Start Date", (DateTime?)null, StartDate );
                        History.EvaluateChange( HistoryChangeList, "End Date", (DateTime?)null, EndDate );
                        History.EvaluateChange( HistoryChangeList, "Number of Payments", (int?)null, NumberOfPayments );
                        History.EvaluateChange( HistoryChangeList, "Is Active", (bool?)null, IsActive );
                        History.EvaluateChange( HistoryChangeList, "Card Reminder Date", (DateTime?)null, CardReminderDate );
                        History.EvaluateChange( HistoryChangeList, "Last Reminded Date", (DateTime?)null, LastRemindedDate );

                        break;
                    }

                case System.Data.Entity.EntityState.Modified:
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
                        History.EvaluateChange( HistoryChangeList, "Type", entry.OriginalValues["TransactionTypeValueId"].ToStringSafe().AsIntegerOrNull(), TransactionTypeValue, TransactionTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Source", entry.OriginalValues["SourceTypeValueId"].ToStringSafe().AsIntegerOrNull(), SourceTypeValue, SourceTypeValueId );
                        History.EvaluateChange( HistoryChangeList, "Frequency", entry.OriginalValues["TransactionFrequencyValueId"].ToStringSafe().AsIntegerOrNull(), TransactionFrequencyValue, TransactionFrequencyValueId );
                        History.EvaluateChange( HistoryChangeList, "Start Date", entry.OriginalValues["StartDate"].ToStringSafe().AsDateTime(), StartDate );
                        History.EvaluateChange( HistoryChangeList, "End Date", entry.OriginalValues["EndDate"].ToStringSafe().AsDateTime(), EndDate );
                        History.EvaluateChange( HistoryChangeList, "Number of Payments", entry.OriginalValues["EndDate"].ToStringSafe().AsIntegerOrNull(), NumberOfPayments );
                        History.EvaluateChange( HistoryChangeList, "Is Active", entry.OriginalValues["IsActive"].ToStringSafe().AsBooleanOrNull(), IsActive );
                        History.EvaluateChange( HistoryChangeList, "Card Reminder Date", entry.OriginalValues["CardReminderDate"].ToStringSafe().AsDateTime(), CardReminderDate );
                        History.EvaluateChange( HistoryChangeList, "Last Reminded Date", entry.OriginalValues["LastRemindedDate"].ToStringSafe().AsDateTime(), LastRemindedDate );

                        break;
                    }

                case System.Data.Entity.EntityState.Deleted:
                    {
                        HistoryChangeList.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "Transaction" );

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
            if ( HistoryChangeList.Any() )
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
        }
    }

    #endregion

}