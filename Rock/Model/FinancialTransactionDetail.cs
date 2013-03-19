//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// TransactionDetail POCO class.
    /// </summary>
    [Table("FinancialTransactionDetail")]
    [DataContract]
    public partial class FinancialTransactionDetail : Model<FinancialTransactionDetail>
    {
        /// <summary>
        /// Gets or sets the transaction id.
        /// </summary>
        /// <value>
        /// The transaction id.
        /// </value>
        [DataMember]
        public int? TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        [MaxLength(50)]
        [DataMember]
        public string Entity { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        [DataMember]
        public string EntityId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [DataMember]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the summary.
        /// </summary>
        /// <value>
        /// The summary.
        /// </value>
        [MaxLength(500)]
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the transaction.
        /// </summary>
        /// <value>
        /// The transaction.
        /// </value>
        [DataMember]
        public virtual FinancialTransaction Transaction { get; set; }

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
    /// TransactionDetail Configuration class
    /// </summary>
    public partial class FinancialTransactionDetailConfiguration : EntityTypeConfiguration<FinancialTransactionDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionDetailConfiguration"/> class.
        /// </summary>
        public FinancialTransactionDetailConfiguration()
        {
            this.HasOptional(d => d.Transaction).WithMany(t => t.TransactionDetails).HasForeignKey(t => t.TransactionId).WillCascadeOnDelete(false);
        }
    }
}