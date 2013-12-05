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
    /// Represents a financial pledge that an individual has made to be given to the specified <see cref="Rock.Model.FinancialAccount"/>/fund.  This includes
    /// the fund that the pledge is directed to, the amount, the pledge frequency and the time period for the pledge.
    /// </summary>
    [Table( "FinancialPledge" )]
    [DataContract]
    public partial class FinancialPledge : Model<FinancialPledge>
    {

        #region Entity Properties

        /// <summary>
        /// Gets or sets the PersonId of the <see cref="Rock.Model.Person"/> who made the pledge.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the PersonId of the <see cref="Rock.Model.Person"/> who made the pledge.
        /// </value>
        [DataMember]
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the AccountId of the <see cref="Rock.Model.FinancialAccount"/> that the pledge is directed toward.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32"/> representing the AccountId of the <see cref="Rock.Model.FinancialAccount"/> that the pledge is directed toward.
        /// </value>
        [DataMember]
        public int? AccountId { get; set; }

        /// <summary>
        /// Gets or sets the pledge amount that is promised to be given at the specified <see cref="PledgeFrequencyValue"/>.
        /// </summary>
        /// <value>
        /// A <see cref="System.Decimal"/> representing the amount to be pledged at the specified frequency.
        /// </value>
        /// <remarks>
        /// An example is that a person pledges $100.00 to be given monthly for the next year. This value will be $100.00 and the grand total of the pledge would be $1,200.00
        /// </remarks>
        [DataMember]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Gets or sets the DefinedValueId of the pledge frequency <see cref="Rock.Model.DefinedValue" /> representing how often the pledgor is promising to give the pledge amount.
        /// </summary>
        /// <value>
        /// A <see cref="System.Int32" /> representing the pledge frequency <see cref="Rock.Model.DefinedValue"/>.
        /// </value>
        [DataMember]
        [DefinedValue( SystemGuid.DefinedType.FINANCIAL_FREQUENCY )]
        public int? PledgeFrequencyValueId { get; set; }

        /// <summary>
        /// Gets or sets the start date of the pledge period.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the start date of the pledge period.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the pledge period.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the end date of the pledge period.
        /// </value>
        [DataMember]
        [Column( TypeName = "Date" )]
        public DateTime EndDate { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Person"/> who is making the pledge.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.Person"/> who is making the pledge.
        /// </value>
        [DataMember]
        public virtual Person Person { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.FinancialAccount"/> or fund that the pledge is being directed toward.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.FinancialAccount"/> or fund that the pledge is being directed toward.
        /// </value>
        [DataMember]
        public virtual FinancialAccount Account { get; set; }

        /// <summary>
        /// Gets or sets the pledge frequency <see cref="Rock.Model.DefinedValue"/>. This is how often the <see cref="Rock.Model.Person"/> who is 
        /// making the pledge promises to give the <see cref="TotalAmount"/>
        /// </summary>
        /// <value>
        /// The frequency of the pledge
        /// </value>
        [DataMember]
        public virtual DefinedValue PledgeFrequencyValue { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this pledge.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this pledge.
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