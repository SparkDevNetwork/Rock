//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Financial
{
    /// <summary>
    /// TransactionFund POCO class.
    /// </summary>
    [Table("financialTransactionFund")]
    public partial class TransactionFund
    {
        /// <summary>
        /// Gets or sets the transaction id.
        /// </summary>
        /// <value>
        /// The transaction id.
        /// </value>
        [Key]
        [DataMember]
        public int TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the fund id.
        /// </summary>
        /// <value>
        /// The fund id.
        /// </value>
        [Key]
        [DataMember]
        public int FundId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [DataMember]
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the transaction.
        /// </summary>
        /// <value>
        /// The transaction.
        /// </value>
        public virtual Transaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets the fund.
        /// </summary>
        /// <value>
        /// The fund.
        /// </value>
        public virtual Fund Fund { get; set; }
    }

    /// <summary>
    /// TransactionFund Configuration class
    /// </summary>
    public partial class TransactionFundConfiguration : EntityTypeConfiguration<TransactionFund>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionFundConfiguration"/> class.
        /// </summary>
        public TransactionFundConfiguration()
        {
            this.HasRequired(t => t.Transaction).WithMany(t => t.TransactionFunds).HasForeignKey(t => t.TransactionId);
            this.HasRequired(t => t.Fund).WithMany(f => f.TransactionFunds).HasForeignKey(t => t.FundId);
        }
    }
}