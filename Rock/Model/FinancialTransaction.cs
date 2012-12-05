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
using Rock.Data;
using Rock.Model;

namespace Rock.Model
{
    /// <summary>
    /// Transaction POCO class.
    /// </summary>
    [Table("FinancialTransaction")]
    public partial class FinancialTransaction : Model<FinancialTransaction>
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [MaxLength(250)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the transaction date time.
        /// </summary>
        /// <value>
        /// The transaction date time.
        /// </value>
        public DateTime? TransactionDateTime { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        [MaxLength(50)]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        public int? EntityId { get; set; }

        /// <summary>
        /// Gets or sets the batch id.
        /// </summary>
        /// <value>
        /// The batch id.
        /// </value>
        public int? BatchId { get; set; }

        /// <summary>
        /// Gets or sets the currency type value id.
        /// </summary>
        /// <value>
        /// The currency type value id.
        /// </value>
        public int? CurrencyTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the credit card type value id.
        /// </summary>
        /// <value>
        /// The credit card type value id.
        /// </value>
        public int? CreditCardTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the refund transaction id.
        /// </summary>
        /// <value>
        /// The refund transaction id.
        /// </value>
        public int? RefundTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the transaction image id.
        /// </summary>
        /// <value>
        /// The transaction image id.
        /// </value>
        public int? TransactionImageId { get; set; }

        /// <summary>
        /// Gets or sets the transaction code.
        /// </summary>
        /// <value>
        /// The transaction code.
        /// </value>
        [MaxLength(50)]
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the payment gateway id.
        /// </summary>
        /// <value>
        /// The payment gateway id.
        /// </value>
        public int? PaymentGatewayId { get; set; }

        /// <summary>
        /// Gets or sets the source type value id.
        /// </summary>
        /// <value>
        /// The source type value id.
        /// </value>
        public int? SourceTypeValueId { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        [MaxLength(500)]
        public string Summary { get; set; }

        /// <summary>
        /// Gets the dto.
        /// </summary>
        /// <returns></returns>
        public override IDto Dto
        {
            get { return this.ToDto(); }
        }

        /// <summary>
        /// Static Method to return an object based on the id
        /// </summary>
        /// <param name="id">The id.</param>
        /// <returns></returns>
        public static FinancialTransaction Read( int id )
        {
            return Read<FinancialTransaction>( id );
        }

        /// <summary>
        /// Gets or sets the batch.
        /// </summary>
        /// <value>
        /// The batch.
        /// </value>
        public virtual FinancialBatch Batch { get; set; }

        /// <summary>
        /// Gets or sets the currency type value.
        /// </summary>
        /// <value>
        /// The currency type value.
        /// </value>
        public virtual DefinedValue CurrencyTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the credit card type value.
        /// </summary>
        /// <value>
        /// The credit card type value.
        /// </value>
        public virtual DefinedValue CreditCardTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the payment gateway.
        /// </summary>
        /// <value>
        /// The payment gateway.
        /// </value>
        public virtual PaymentGateway PaymentGateway { get; set; }

        /// <summary>
        /// Gets or sets the source type value.
        /// </summary>
        /// <value>
        /// The source type value.
        /// </value>
        public virtual DefinedValue SourceTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the transaction details.
        /// </summary>
        /// <value>
        /// The transaction details.
        /// </value>
        public virtual ICollection<FinancialTransactionDetail> TransactionDetails { get; set; }

        /// <summary>
        /// Gets or sets the transaction funds.
        /// </summary>
        /// <value>
        /// The transaction funds.
        /// </value>
        public virtual ICollection<FinancialTransactionFund> TransactionFunds { get; set; }
        //public virtual ICollection<Fund> Funds { get; set; }

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
    }

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
            this.HasOptional(b => b.Batch).WithMany(t => t.Transactions).HasForeignKey(t => t.BatchId).WillCascadeOnDelete(false);
            this.HasOptional(t => t.CurrencyTypeValue).WithMany().HasForeignKey(t => t.CurrencyTypeValueId).WillCascadeOnDelete(false);
            this.HasOptional(t => t.CreditCardTypeValue).WithMany().HasForeignKey(t => t.CreditCardTypeValueId).WillCascadeOnDelete(false);
            this.HasOptional(t => t.PaymentGateway).WithMany(g => g.Transactions).HasForeignKey(t => t.PaymentGatewayId).WillCascadeOnDelete(false);
            this.HasOptional(t => t.SourceTypeValue).WithMany().HasForeignKey(t => t.SourceTypeValueId).WillCascadeOnDelete(false);
        }
    }
}