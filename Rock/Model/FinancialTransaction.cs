//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
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
    [Table("FinancialTransaction")]
    [DataContract]
    public partial class FinancialTransaction : Model<FinancialTransaction>
    {

        #region Entity Properties
       
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [MaxLength(250)]
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        [DataMember]
        public DateTime? TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Entity Type Id.
        /// </summary>
        /// <value>
        /// Entity Type Id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        [Required]
        [DataMember( IsRequired = true )]
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the batch id.
        /// </summary>
        /// <value>
        /// The batch id.
        /// </value>
        [DataMember]
        public int? BatchId { get; set; }

        /// <summary>
        /// Gets or sets the currency type value id.
        /// </summary>
        /// <value>
        /// The currency type value id.
        /// </value>
        [DataMember]
        public int? CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the credit card type value id.
        /// </summary>
        /// <value>
        /// The credit card type value id.
        /// </value>
        [DataMember]
        public int? CreditCardTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [DataMember]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the refund transaction id.
        /// </summary>
        /// <value>
        /// The refund transaction id.
        /// </value>
        [DataMember]
        public int? RefundTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the transaction image id.
        /// </summary>
        /// <value>
        /// The transaction image id.
        /// </value>
        [DataMember]
        public int? TransactionImageId { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        [MaxLength(50)]
        [DataMember]
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the payment gateway id.
        /// </summary>
        /// <value>
        /// The payment gateway id.
        /// </value>
        [DataMember]
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        /// Gets or sets the source type value id.
        /// </summary>
        /// <value>
        /// The source type value id.
        /// </value>
        [DataMember]
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        [MaxLength(500)]
        [DataMember]
        public string Summary { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        /// <summary>
        /// Gets or sets the batch.
        /// </summary>
        /// <value>
        /// The batch.
        /// </value>
        [DataMember]
        public virtual FinancialBatch Batch { get; set; }

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
        /// Gets or sets the payment gateway.
        /// </summary>
        /// <value>
        /// The payment gateway.
        /// </value>
        [DataMember]
        public virtual PaymentGateway PaymentGateway { get; set; }

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
        public virtual ICollection<FinancialTransactionDetail> TransactionDetails { get; set; }

        /// <summary>
        /// Gets or sets the transaction funds.
        /// </summary>
        /// <value>
        /// The transaction funds.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialTransactionFund> TransactionFunds { get; set; }
        //public virtual ICollection<Fund> Funds { get; set; }

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
            //this.HasMany(p => p.Funds).WithMany(c => c.Transactions).Map(m => { m.MapLeftKey("TransactionId"); m.MapRightKey("FundId"); m.ToTable("financialTransactionFund"); });
            this.HasRequired( p => p.EntityType ).WithMany().HasForeignKey( p => p.EntityTypeId ).WillCascadeOnDelete( false );
            this.HasOptional( b => b.Batch ).WithMany( t => t.Transactions ).HasForeignKey( t => t.BatchId ).WillCascadeOnDelete( false );
            this.HasOptional(t => t.CurrencyTypeValue).WithMany().HasForeignKey(t => t.CurrencyTypeValueId).WillCascadeOnDelete(false);
            this.HasOptional(t => t.CreditCardTypeValue).WithMany().HasForeignKey(t => t.CreditCardTypeValueId).WillCascadeOnDelete(false);
            this.HasOptional(t => t.PaymentGateway).WithMany(g => g.Transactions).HasForeignKey(t => t.PaymentGatewayId).WillCascadeOnDelete(false);
            this.HasOptional(t => t.SourceTypeValue).WithMany().HasForeignKey(t => t.SourceTypeValueId).WillCascadeOnDelete(false);
        }
    }

    #endregion

}