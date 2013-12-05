//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Represents a financial transaction schedule in RockChMS. A user can schedule transactions for varying frequencies, number of transactions and 
    /// and time period. A scheduled transaction can include multiple <see cref="Rock.Model.FinancialScheduledTransactionDetail"/> items so that a single 
    /// scheduled transaction can include payments/gifts for multiple <see cref="Rock.Model.FinancialAccount">Financial Accounts</see>/funds.
    /// </summary>
    /// <remarks>
    /// Several examples include - A one time transaction to occur on 1/1/2014; an ongoing weekly transaction; a weekly transaction for 10 weeks; a monthly transaction from 1/1/2014 - 12/31/2014.
    /// </remarks>
    [Table( "FinancialScheduledTransaction" )]
    [DataContract]
    public partial class FinancialScheduledTransaction : Model<FinancialScheduledTransaction>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who authorized the scheduled transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who authorized the scheduled transaction.
        /// </value>
        [DataMember]
        public int AuthorizedPersonId { get; set; }

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
        /// Gets or sets the date and time of the last status update.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime" /> representing the date and time of the last status update.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
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
        /// Gets or sets the EntityTypeId of the Financial Gateway's service <see cref="Rock.Model.EntityType"/>. Represents the
        /// financial gateway that will be used for this transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the EntityTypeId of the Financial Gateway's service <see cref="Rock.Model.EntityType"/>
        /// </value>
        [DataMember]
        public int? GatewayEntityTypeId { get; set; }

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
        /// Gets or sets the <see cref="Rock.Model.Person"/> that authorized the scheduled transaction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> that authorized the scheduled transaction.
        /// </value>
        public virtual Person AuthorizedPerson { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> for the financial gateway.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> for the financial gateway.
        /// </value>
        [DataMember]
        public virtual EntityType GatewayEntityType { get; set; }

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
        /// Gets or sets <see cref="Rock.Model.FinancialTransaction">FinancialTransactions</see> that have been processed and have have cleared for this scheduled transaction profile. 
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

        #endregion

        #region Public Methods

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
            this.HasRequired( t => t.AuthorizedPerson ).WithMany().HasForeignKey( t => t.AuthorizedPersonId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.GatewayEntityType ).WithMany().HasForeignKey( t => t.GatewayEntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( t => t.TransactionFrequencyValue ).WithMany().HasForeignKey( t => t.TransactionFrequencyValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}