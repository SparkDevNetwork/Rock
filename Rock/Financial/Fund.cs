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
using Rock.CRM;
using Rock.Core;
using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// Fund POCO class.
    /// </summary>
    [Table("financialFund")]
    public partial class Fund : ModelWithAttributes<Fund>, IAuditable
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        [MaxLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the public.
        /// </summary>
        /// <value>
        /// The name of the public.
        /// </value>
        [DataMember]
        [MaxLength(50)]
        public string PublicName { get; set; }

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
        /// Gets or sets the parent fund id.
        /// </summary>
        /// <value>
        /// The parent fund id.
        /// </value>
        [DataMember]
        public int? ParentFundId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [tax deductible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tax deductible]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsTaxDeductible { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Fund"/> is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [DataMember]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [DataMember]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Fund"/> is pledgable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if pledgable; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsPledgable { get; set; }

        /// <summary>
        /// Gets or sets the gl code.
        /// </summary>
        /// <value>
        /// The gl code.
        /// </value>
        [DataMember]
        [MaxLength(50)]
        public string GlCode { get; set; }

        /// <summary>
        /// Gets or sets the fund type id.
        /// </summary>
        /// <value>
        /// The fund type id.
        /// </value>
        [DataMember]
        public int? FundTypeId { get; set; }

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
        public virtual ICollection<TransactionFund> TransactionFunds { get; set; }
        //public virtual ICollection<Transaction> Transactions { get; set; }

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
        public override string AuthEntity { get { return "Financial.Fund"; } }
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
            this.HasOptional(p => p.CreatedByPerson).WithMany().HasForeignKey(p => p.CreatedByPersonId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.ModifiedByPerson).WithMany().HasForeignKey(p => p.ModifiedByPersonId).WillCascadeOnDelete(false);
        }
    }

    /// <summary>
    /// Data Transformation Object
    /// </summary>
    public partial class FundDTO : DTO<Fund>
    {
        /// <summary>
        /// Instantiate new DTO object
        /// </summary>
        public FundDTO()
        {
        }

        /// <summary>
        /// Instantiate new DTO object from Model
        /// </summary>
        /// <param name="auth"></param>
        public FundDTO( Fund fund )
        {
            CopyFromModel( fund );
        }

        /// <summary>
        /// Copy DTO to Model
        /// </summary>
        /// <param name="fund"></param>
        public override void CopyFromModel( Fund fund )
        {
        }

        /// <summary>
        /// Copy Model to DTO
        /// </summary>
        /// <param name="fund"></param>
        public override void CopyToModel( Fund fund )
        {
        }
    }
}