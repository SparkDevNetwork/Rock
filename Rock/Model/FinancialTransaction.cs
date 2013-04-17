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
    public partial class FinancialTransaction : Model<FinancialTransaction>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the authorized person id.
        /// </summary>
        /// <value>
        /// The authorized person id.
        /// </value>
        [DataMember]
        public int AuthorizedPersonId { get; set; }

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
        public int? GatewayId { get; set; }

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
        public virtual FinancialGateway Gateway { get; set; }

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
            this.HasOptional( t => t.Gateway ).WithMany().HasForeignKey( t => t.GatewayId ).WillCascadeOnDelete( false );
            this.HasRequired( t => t.TransactionTypeValue ).WithMany().HasForeignKey( t => t.TransactionTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.CurrencyTypeValue ).WithMany().HasForeignKey( t => t.CurrencyTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.CreditCardTypeValue ).WithMany().HasForeignKey( t => t.CreditCardTypeValueId ).WillCascadeOnDelete( false );
            this.HasOptional( t => t.SourceTypeValue ).WithMany().HasForeignKey( t => t.SourceTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}