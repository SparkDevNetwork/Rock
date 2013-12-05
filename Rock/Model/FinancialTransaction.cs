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
    /// Represents a financial transaction in RockChMS.  This class implements the <c>FinancialTransactionBase</c> base class.
    /// </summary>
    [Table( "FinancialTransaction" )]
    [DataContract]
    public partial class FinancialTransaction : FinancialTransactionBase<FinancialTransaction>
    {
    }

    /// <summary>
    /// An abstracted base class for FinancialTransaction so that we can have child classes like <see cref="FinancialTransactionRefund"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FinancialTransactionBase<T> : Model<T> where T: Model<T>, Rock.Security.ISecured, new()
    {

        #region Entity Properties
       
        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who authorized the transaction. In the event of a gift this would be
        /// the giver; in the event of a purchase this would be the purchaser.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who authorized the transaction.
        /// </value>
        [DataMember]
        public int? AuthorizedPersonId { get; set; }

        /// <summary>
        /// Gets or sets BatchId of the <see cref="Rock.Model.FinancialBatch"/> that contains this transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the BatchId of the <see cref="Rock.Model.FinancialBatch"/> that contains the transaction.
        /// </value>
        [DataMember]
        public int? BatchId { get; set; }

        /// <summary>
        /// Gets or sets EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the financial gateway (service) that processed this transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32" /> representing the EntityTypeId of the <see cref="Rock.Model.EntityType"/> for the financial gateway (service) that processed this transaction.
        /// </value>
        [DataMember]
        public int? GatewayEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets date and time that the transaction occurred. This is the local server time.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the time that the transaction occurred. This is the local server time.
        /// </value>
        [DataMember]
        public DateTime? TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the amount of the transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal" /> representing the amount of the transaction.
        /// </value>
        [DataMember]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the transaction code for the transaction.
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
        public int TransactionTypeValueId { get; set; }

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
        /// Gets or sets the DefinedValueId of the source type <see cref="Rock.Model.DefinedValue"/> for this transaction. Representing the source (method) of this transaction.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the DefinedValueId of the source type <see cref="Rock.Model.DefinedValue"/> for this transaction.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE )]
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets an encrypted version of a scanned check's MICR information.
        /// Plain Text format is {routingnumber}_{accountnumber}_{checknumber}
        /// </summary>
        /// <value>
        /// The check micr encrypted.
        /// A <see cref="System.String"/> representing an encrypted version of a scanned check's MICR information.
        /// </value>
        [DataMember]
        public string CheckMicrEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the ScheduledTransactionId of the <see cref="Rock.Model.FinancialScheduledTransaction" /> that triggered
        /// this transaction. If this was an ad-hoc/on demand transaction, this property will be null.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the ScheduledTransactionId of the <see cref="Rock.Model.FinancialScheduledTransaction"/> 
        /// </value>
        [DataMember]
        public int? ScheduledTransactionId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the the <see cref="Rock.Model.Person"/> who authorized the transaction. For a gift this is a the giver, for a purchase this is the purchaser.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> who authorized the transaction.
        /// </value>
        public virtual Person AuthorizedPerson { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialBatch"/> that contains the transaction. 
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.FinancialBatch"/> that contains the transaction.
        /// </value>
        [DataMember]
        public virtual FinancialBatch Batch { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.EntityType"/> of the Payment Gateway service that was used to process this transaction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.EntityType"/> of the payment gateway service that was used.  If this was not an electronic transaction, this value will be null.
        /// </value>
        [DataMember]
        public virtual EntityType GatewayEntityType { get; set; }


        /// <summary>
        /// Gets or sets the transaction type <see cref="Rock.Model.DefinedValue"/> indicating the type of transaction that occurred.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.DefinedValue"/> indicating the type of transaction that occurred.
        /// </value>
        [DataMember]
        public virtual DefinedValue TransactionTypeValue { get; set; }

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
        [DataMember]
        public virtual FinancialTransactionRefund Refund { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialScheduledTransaction">Scheduled Transaction</see> that initiated this transaction.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialScheduledTransaction"/> that initiated this transaction.
        /// </value>
        [DataMember]
        public virtual FinancialScheduledTransaction ScheduledTransaction { get; set; }

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
            return this.Amount.ToString();
        }

        #endregion

    }


    /// <summary>
    /// Special Class to use when uploading a FinancialTransaction from a Scanned Check thru the Rest API.
    /// The Rest Client can't be given access to the DataEncryptionKey, so they'll upload it (using SSL) 
    /// with the plain text CheckMicr and the Rock server will encrypt prior to saving to database
    /// </summary>
    [DataContract]
    [NotMapped]
    public class FinancialTransactionScannedCheck : FinancialTransactionBase<FinancialTransactionScannedCheck>
    {
        /// <summary>
        /// Gets or sets the scanned check MICR.
        /// </summary>
        /// <value>
        /// The scanned check MICR.
        /// </value>
        [DataMember]
        public string ScannedCheckMicr { get; set; }
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
            this.HasRequired( t => t.AuthorizedPerson ).WithMany().HasForeignKey( t => t.AuthorizedPersonId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Batch ).WithMany( t => t.Transactions ).HasForeignKey( t => t.BatchId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.GatewayEntityType ).WithMany().HasForeignKey( t => t.GatewayEntityTypeId ).WillCascadeOnDelete( false );
            this.HasRequired( t => t.TransactionTypeValue ).WithMany().HasForeignKey( t => t.TransactionTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.CurrencyTypeValue ).WithMany().HasForeignKey( t => t.CurrencyTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.CreditCardTypeValue ).WithMany().HasForeignKey( t => t.CreditCardTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.SourceTypeValue ).WithMany().HasForeignKey( t => t.SourceTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.Refund ).WithRequired().WillCascadeOnDelete( true );
            this.HasOptional( t => t.ScheduledTransaction ).WithMany( s => s.Transactions ).HasForeignKey( t => t.ScheduledTransactionId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}