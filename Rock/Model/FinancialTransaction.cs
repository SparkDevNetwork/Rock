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
    /// Transaction POCO class.
    /// </summary>
    [Table( "FinancialTransaction" )]
    [DataContract]
    public partial class FinancialTransaction : FinancialTransactionBase<FinancialTransaction>
    {
    }

    /// <summary>
    /// abstract base class for FinancialTransaction so that we can have child classes like FinancialTransactionRefund
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FinancialTransactionBase<T> : Model<T> where T: Model<T>, Rock.Security.ISecured, new()
    {

        #region Entity Properties
       
        /// <summary>
        /// Gets or sets the authorized person id.
        /// </summary>
        /// <value>
        /// The authorized person id.
        /// </value>
        [DataMember]
        public int? AuthorizedPersonId { get; set; }

        /// <summary>
        /// Gets or sets the batch id.
        /// </summary>
        /// <value>
        /// The batch id.
        /// </value>
        [DataMember]
        public int? BatchId { get; set; }

        /// <summary>
        /// Gets or sets the gateway id.
        /// </summary>
        /// <value>
        /// The gateway id.
        /// </value>
        [DataMember]
        public int? GatewayEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        [DataMember]
        public DateTime? TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [DataMember]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the transaction type value id.
        /// </summary>
        /// <value>
        /// The transaction type value id.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE )]
        public int TransactionTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the currency type value id.
        /// </summary>
        /// <value>
        /// The currency type value id.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE )]
        public int? CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the credit card type value id.
        /// </summary>
        /// <value>
        /// The credit card type value id.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_CREDIT_CARD_TYPE )]
        public int? CreditCardTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the source type value id.
        /// </summary>
        /// <value>
        /// The source type value id.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_SOURCE_TYPE )]
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the check micr encrypted.
        /// Plain Text format is {routingnumber}_{accountnumber}_{checknumber}
        /// </summary>
        /// <value>
        /// The check micr encrypted.
        /// </value>
        [DataMember]
        public string CheckMicrEncrypted { get; set; }

        /// <summary>
        /// Gets or sets the scheduled transaction id.
        /// </summary>
        /// <value>
        /// The scheduled transaction id.
        /// </value>
        [DataMember]
        public int? ScheduledTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the scheduled transaction key.  This is a key returned by the financial gateway to uniquely identify a recurring payment schedule
        /// </summary>
        /// <value>
        /// The scheduled transaction key.
        /// </value>
        [DataMember]
        public string ScheduledTransactionKey { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the authorized person.
        /// </summary>
        /// <value>
        /// The authorized person.
        /// </value>
        public virtual Person AuthorizedPerson { get; set; }

        /// <summary>
        /// Gets or sets the batch.
        /// </summary>
        /// <value>
        /// The batch.
        /// </value>
        [DataMember]
        public virtual FinancialBatch Batch { get; set; }

        /// <summary>
        /// Gets or sets the gateway.
        /// </summary>
        /// <value>
        /// The gateway.
        /// </value>
        [DataMember]
        public virtual EntityType GatewayEntityType { get; set; }

        /// <summary>
        /// Gets or sets the transaction type value.
        /// </summary>
        /// <value>
        /// The transaction type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue TransactionTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the currency type value.
        /// </summary>
        /// <value>
        /// The currency type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue CurrencyTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the credit card type value.
        /// </summary>
        /// <value>
        /// The credit card type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue CreditCardTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the source type value.
        /// </summary>
        /// <value>
        /// The source type value.
        /// </value>
        [DataMember]
        public virtual DefinedValue SourceTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the refund.
        /// </summary>
        /// <value>
        /// The refund.
        /// </value>
        [DataMember]
        public virtual FinancialTransactionRefund Refund { get; set; }

        /// <summary>
        /// Gets or sets the scheduled transaction.
        /// </summary>
        /// <value>
        /// The scheduled transaction.
        /// </value>
        [DataMember]
        public virtual FinancialScheduledTransaction ScheduledTransaction { get; set; }

        /// <summary>
        /// Gets or sets the transaction details.
        /// </summary>
        /// <value>
        /// The transaction details.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialTransactionDetail> TransactionDetails 
        {
            get { return _transactionDetails ?? ( _transactionDetails = new Collection<FinancialTransactionDetail>() ); }
            set { _transactionDetails = value; }
        }
        private ICollection<FinancialTransactionDetail> _transactionDetails;

        /// <summary>
        /// Gets or sets the images.
        /// </summary>
        /// <value>
        /// The images.
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
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
    /// with the plaintext CheckMicr and the Rock server will encrypt prior to saving to database
    /// </summary>
    [DataContract]
    [NotMapped]
    public class FinancialTransactionScannedCheck : FinancialTransactionBase<FinancialTransactionScannedCheck>
    {
        /// <summary>
        /// Gets or sets the scanned check micr.
        /// </summary>
        /// <value>
        /// The scanned check micr.
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