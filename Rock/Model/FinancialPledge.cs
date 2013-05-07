//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Financial Pledge POCO class.
    /// </summary>
    [Table( "FinancialPledge" )]
    [DataContract]
    public partial class FinancialPledge : Model<FinancialPledge>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the fund id.
        /// </summary>
        /// <value>
        /// The fund id.
        /// </value>
        [DataMember]
        public int? AccountId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [DataMember]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the frequency type id.
        /// </summary>
        /// <value>
        /// The frequency type id.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_PLEDGE_FREQUENCY )]
        public int? PledgeFrequencyValueId { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime EndDate { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        [DataMember]
        public virtual Person Person { get; set; }

        /// <summary>
        /// Gets or sets the account.
        /// </summary>
        /// <value>
        /// The fund.
        /// </value>
        [DataMember]
        public virtual FinancialAccount Account { get; set; }

        /// <summary>
        /// Gets or sets the frequency of the pledge
        /// </summary>
        /// <value>
        /// The frequency of the pledge
        /// </value>
        [DataMember]
        public virtual DefinedValue PledgeFrequencyValue { get; set; }

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
            return this.TotalAmount.ToString();
        }

        #endregion

    }

    #region Entity Configuration

    /// <summary>
    /// Pledge Configuration class.
    /// </summary>
    public partial class FinancialPledgeConfiguration : EntityTypeConfiguration<FinancialPledge>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FinancialPledgeConfiguration"/> class.
        /// </summary>
        public FinancialPledgeConfiguration()
        {
            this.HasOptional( p => p.Person ).WithMany().HasForeignKey( p => p.PersonId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.Account ).WithMany().HasForeignKey( p => p.AccountId ).WillCascadeOnDelete( false );
            this.HasOptional( p => p.PledgeFrequencyValue ).WithMany().HasForeignKey( p => p.PledgeFrequencyValueId ).WillCascadeOnDelete( false );
        }

    }

    #endregion

}