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

namespace Rock.Financial
{
    /// <summary>
    /// Fund POCO class.
    /// </summary>
    [Table("Fund")]
    public partial class Fund : Model<Fund>
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the public.
        /// </summary>
        /// <value>
        /// The name of the public.
        /// </value>
        [MaxLength(50)]
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [MaxLength(250)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the parent fund id.
        /// </summary>
        /// <value>
        /// The parent fund id.
        /// </value>
        public int? ParentFundId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [tax deductible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tax deductible]; otherwise, <c>false</c>.
        /// </value>
        public bool IsTaxDeductible { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Fund"/> is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Fund"/> is pledgable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if pledgable; otherwise, <c>false</c>.
        /// </value>
        public bool IsPledgable { get; set; }

        /// <summary>
        /// Gets or sets the gl code.
        /// </summary>
        /// <value>
        /// The gl code.
        /// </value>
        [MaxLength(50)]
        public string GlCode { get; set; }

        /// <summary>
        /// Gets or sets the fund type id.
        /// </summary>
        /// <value>
        /// The fund type id.
        /// </value>
        public int? FundTypeId { get; set; }

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
        /// Gets or sets the parent fund.
        /// </summary>
        /// <value>
        /// The parent fund.
        /// </value>
        public virtual Fund ParentFund { get; set; }

        /// <summary>
        /// Gets or sets the type of the fund.
        /// </summary>
        /// <value>
        /// The type of the fund.
        /// </value>
        public virtual DefinedValue FundType { get; set; }

        /// <summary>
        /// Gets or sets the child funds.
        /// </summary>
        /// <value>
        /// The child funds.
        /// </value>
        public virtual ICollection<Fund> ChildFunds { get; set; }

        /// <summary>
        /// Gets or sets the pledges.
        /// </summary>
        /// <value>
        /// The pledges.
        /// </value>
        public virtual ICollection<Pledge> Pledges { get; set; }

        /// <summary>
        /// Gets or sets the transaction funds.
        /// </summary>
        /// <value>
        /// The transaction funds.
        /// </value>
        public virtual ICollection<FinancialTransactionFund> TransactionFunds { get; set; }
        //public virtual ICollection<Transaction> Transactions { get; set; }

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
        public static Fund Read( int id )
        {
            return Read<Fund>( id );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }
    }

    /// <summary>
    /// Fund Configuration class.
    /// </summary>
    public partial class FundConfiguration : EntityTypeConfiguration<Fund>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FundConfiguration"/> class.
        /// </summary>
        public FundConfiguration()
        {
            this.HasOptional(f => f.ParentFund).WithMany(f => f.ChildFunds).HasForeignKey(f => f.ParentFundId).WillCascadeOnDelete(false);
            this.HasOptional(f => f.FundType).WithMany().HasForeignKey(f => f.FundTypeId).WillCascadeOnDelete(false);
        }
    }
}