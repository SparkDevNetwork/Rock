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
    /// Financial Account POCO class.
    /// </summary>
    [Table( "FinancialAccount" )]
    [DataContract]
    public partial class FinancialAccount : Model<FinancialAccount>, IOrdered
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the parent account id.
        /// </summary>
        /// <value>
        /// The parent fund id.
        /// </value>
        [DataMember]
        public int? ParentAccountId { get; set; }

        /// <summary>
        /// Gets or sets the campus id.
        /// </summary>
        /// <value>
        /// The campus id.
        /// </value>
        [DataMember]
        public int? CampusId { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [Required]
        [MaxLength( 50 )]
        [DataMember( IsRequired = true )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the public.
        /// </summary>
        /// <value>
        /// The name of the public.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [tax deductible].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tax deductible]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsTaxDeductible { get; set; }

        /// <summary>
        /// Gets or sets the general ledger code.
        /// </summary>
        /// <value>
        /// The gl code.
        /// </value>
        [MaxLength( 50 )]
        [DataMember]
        public string GlCode { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        [DataMember]
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="FinancialAccount"/> is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        private bool _isActive = true;

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the fund type id.
        /// </summary>
        /// <value>
        /// The fund type id.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_ACCOUNT_TYPE )]
        public int? AccountTypeValueId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the parent account.
        /// </summary>
        /// <value>
        /// The parent fund.
        /// </value>
        public virtual FinancialAccount ParentAccount { get; set; }

        /// <summary>
        /// Gets or sets the campus.
        /// </summary>
        /// <value>
        /// The campus.
        /// </value>
        public virtual Campus Campus { get; set; }

        /// <summary>
        /// Gets or sets the type of the account.
        /// </summary>
        /// <value>
        /// The type of the fund.
        /// </value>
        [DataMember]
        public virtual DefinedValue AccountTypeValue { get; set; }

        /// <summary>
        /// Gets or sets the child accounts.
        /// </summary>
        /// <value>
        /// The child funds.
        /// </value>
        [DataMember]
        public virtual ICollection<FinancialAccount> ChildAccounts 
        {
            get { return _childAccounts ?? ( _childAccounts = new Collection<FinancialAccount>() ); }
            set { _childAccounts = value; }
        }
        private ICollection<FinancialAccount> _childAccounts;

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
            return this.PublicName;
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Fund Configuration class.
    /// </summary>
    public partial class FinancialAccountConfiguration : EntityTypeConfiguration<FinancialAccount>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialAccountConfiguration"/> class.
        /// </summary>
        public FinancialAccountConfiguration()
        {
            this.HasOptional( a => a.ParentAccount ).WithMany( a => a.ChildAccounts ).HasForeignKey( a => a.ParentAccountId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.Campus ).WithMany().HasForeignKey( a => a.CampusId ).WillCascadeOnDelete( false );
            this.HasOptional( a => a.AccountTypeValue ).WithMany().HasForeignKey( a => a.AccountTypeValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

}