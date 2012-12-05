//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// TransactionFund POCO class.
    /// </summary>
    [Table("FinancialTransactionFund")]
    public partial class FinancialTransactionFund
    {
        /// <summary>
        /// Gets or sets the transaction id.
        /// </summary>
        /// <value>
        /// The transaction id.
        /// </value>
        [Key]
        [Column(Order = 0)]
        public int TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the fund id.
        /// </summary>
        /// <value>
        /// The fund id.
        /// </value>
        [Key]
        [Column(Order = 1)]
        public int FundId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Gets or sets the transaction.
        /// </summary>
        /// <value>
        /// The transaction.
        /// </value>
        public virtual FinancialTransaction Transaction { get; set; }

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
    public partial class FinancialTransactionFundConfiguration : EntityTypeConfiguration<FinancialTransactionFund>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialTransactionFundConfiguration"/> class.
        /// </summary>
        public FinancialTransactionFundConfiguration()
        {
            //this.HasKey(t => t.TransactionId);
            //this.HasKey(t => t.FundId);
            this.HasRequired(t => t.Transaction).WithMany(t => t.TransactionFunds).HasForeignKey(t => t.TransactionId);
            this.HasRequired(t => t.Fund).WithMany(f => f.TransactionFunds).HasForeignKey(t => t.FundId);
        }
    }
}