//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.CRM;
using Rock.Core;
using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// Transaction POCO class.
    /// </summary>
    [Table("financialTransaction")]
    public partial class Transaction : ModelWithAttributes<Transaction>, IAuditable
    {
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        [MaxLength(250)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the transaction date.
        /// </summary>
        /// <value>
        /// The transaction date.
        /// </value>
        [DataMember]
        public DateTime? TransactionDate { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        [DataMember]
        [MaxLength(50)]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        [DataMember]
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
        /// Gets or sets the currency type id.
        /// </summary>
        /// <value>
        /// The currency type id.
        /// </value>
        [DataMember]
        public int? CurrencyTypeId { get; set; }

        /// <summary>
        /// Gets or sets the credit card type id.
        /// </summary>
        /// <value>
        /// The credit card type id.
        /// </value>
        [DataMember]
        public int? CreditCardTypeId { get; set; }

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
        [DataMember]
        [MaxLength(50)]
        public string TransactionCode { get; set; }

        /// <summary>
        /// Gets or sets the gateway id.
        /// </summary>
        /// <value>
        /// The gateway id.
        /// </value>
        [DataMember]
        public int? GatewayId { get; set; }

        /// <summary>
        /// Gets or sets the source type id.
        /// </summary>
        /// <value>
        /// The source type id.
        /// </value>
        [DataMember]
        public int? SourceTypeId { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        [DataMember]
        [MaxLength(500)]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the batch.
        /// </summary>
        /// <value>
        /// The batch.
        /// </value>
        public virtual Batch Batch { get; set; }

        /// <summary>
        /// Gets or sets the type of the currency.
        /// </summary>
        /// <value>
        /// The type of the currency.
        /// </value>
        public virtual DefinedValue CurrencyType { get; set; }

        /// <summary>
        /// Gets or sets the type of the credit card.
        /// </summary>
        /// <value>
        /// The type of the credit card.
        /// </value>
        public virtual DefinedValue CreditCardType { get; set; }

        /// <summary>
        /// Gets or sets the transaction details.
        /// </summary>
        /// <value>
        /// The transaction details.
        /// </value>
        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; }

        /// <summary>
        /// Gets or sets the transaction funds.
        /// </summary>
        /// <value>
        /// The transaction funds.
        /// </value>
        public virtual ICollection<TransactionFund> TransactionFunds { get; set; }
        //public virtual ICollection<Fund> Funds { get; set; }

        /// <summary>
        /// Gets or sets the modified date time.
        /// </summary>
        /// <value>
        /// The modified date time.
        /// </value>
        [DataMember]
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created date time.
        /// </summary>
        /// <value>
        /// The created date time.
        /// </value>
        [DataMember]
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the created by person id.
        /// </summary>
        /// <value>
        /// The created by person id.
        /// </value>
        [DataMember]
        public int? CreatedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the modified by person id.
        /// </summary>
        /// <value>
        /// The modified by person id.
        /// </value>
        [DataMember]
        public int? ModifiedByPersonId { get; set; }

        /// <summary>
        /// Gets or sets the created by person.
        /// </summary>
        /// <value>
        /// The created by person.
        /// </value>
        public virtual Person CreatedByPerson { get; set; }

        /// <summary>
        /// Gets or sets the modified by person.
        /// </summary>
        /// <value>
        /// The modified by person.
        /// </value>
        public virtual Person ModifiedByPerson { get; set; }

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
        public override string AuthEntity { get { return "Financial.Transaction"; } }
    }

    /// <summary>
    /// Transaction Configuration class.
    /// </summary>
    public partial class TransactionConfiguration : EntityTypeConfiguration<Transaction>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionConfiguration"/> class.
        /// </summary>
        public TransactionConfiguration()
        {
            //this.HasMany(p => p.Funds).WithMany(c => c.Transactions).Map(m => { m.MapLeftKey("TransactionId"); m.MapRightKey("FundId"); m.ToTable("financialTransactionFund"); });
            this.HasOptional(b => b.Batch).WithMany(t => t.Transactions).HasForeignKey(t => t.BatchId).WillCascadeOnDelete(false);
            this.HasOptional(t => t.CurrencyType).WithMany().HasForeignKey(t => t.CurrencyTypeId).WillCascadeOnDelete(false);
            this.HasOptional(t => t.CreditCardType).WithMany().HasForeignKey(t => t.CreditCardTypeId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.CreatedByPerson).WithMany().HasForeignKey(p => p.CreatedByPersonId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.ModifiedByPerson).WithMany().HasForeignKey(p => p.ModifiedByPersonId).WillCascadeOnDelete(false);
        }
    }
}