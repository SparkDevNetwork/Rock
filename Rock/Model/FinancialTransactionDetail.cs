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
    /// Represents a transaction detail line item for a <see cref="Rock.Model.FinancialTransaction"/> in RockChMS.
    /// </summary>
    [Table( "FinancialTransactionDetail" )]
    [DataContract]
    public partial class FinancialTransactionDetail : Model<FinancialTransactionDetail>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the TransactionId of the <see cref="Rock.Model.FinancialTransaction"/> that this 
        /// detail item is a part of.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the TransactionDetailId of the <see cref="Rock.Model.FinancialTransaction"/>
        /// that this detail item is a part of.
        /// </value>
        [DataMember]
        public int TransactionId { get; set; }

        /// <summary>
        /// Gets or sets the AccountId of the <see cref="Rock.Model.FinancialAccount"/>/fund that the <see cref="Amount"/> of this 
        /// detail line item should be credited towards.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the <see cref="Rock.Model.FinancialAccount"/>/fund that is affected by this
        /// transaction detail.
        /// </value>
        [DataMember]
        public int AccountId { get; set; }

        /// <summary>
        /// Gets or sets the amount of the transaction detail.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the amount of the transaction detail.
        /// </value>
        [DataMember]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the summary of the transaction detail.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing the summary of the transaction detail.
        /// </value>
        [MaxLength( 500 )]
        [DataMember]
        public string Summary { get; set; }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        [DataMember]
        public int? EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <value>
        /// The entity id.
        /// </value>
        [DataMember]
        public int? EntityId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialTransaction"/> that this detail item belongs to.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialTransaction"/> that this detail item belongs to.
        /// </value>
        [DataMember]
        public virtual FinancialTransaction Transaction { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialAccount"/> that is affected by this detail line item.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialAccount"/> that is affected by this detail line item.
        /// </value>
        [DataMember]
        public virtual FinancialAccount Account { get; set; }

        /// <summary>
        /// Gets or sets the type of the entity.
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        [DataMember]
        public virtual EntityType EntityType { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this detail item.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this detail item.
        /// </returns>
        public override string ToString()
        {
            return this.Amount.ToString();
        }

        #endregion

    }

    #region Entity Configuration

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
            this.HasRequired( d => d.Transaction ).WithMany( t => t.TransactionDetails ).HasForeignKey( d => d.TransactionId ).WillCascadeOnDelete( true );
            this.HasRequired( d => d.Account ).WithMany().HasForeignKey( d => d.AccountId ).WillCascadeOnDelete( false );
            this.HasOptional( d => d.EntityType ).WithMany().HasForeignKey( d => d.EntityTypeId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}