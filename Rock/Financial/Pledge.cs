//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Rock.Data;
using Rock.Model;

namespace Rock.Financial
{
    /// <summary>
    /// Pledge POCO class.
    /// </summary>
    [Table("Pledge")]
    public partial class Pledge : Model<Pledge>
    {
        /// <summary>
        /// Gets or sets the person id.
        /// </summary>
        /// <value>
        /// The person id.
        /// </value>
        public int? PersonId { get; set; }

        /// <summary>
        /// Gets or sets the fund id.
        /// </summary>
        /// <value>
        /// The fund id.
        /// </value>
        public int? FundId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the frequency type id.
        /// </summary>
        /// <value>
        /// The frequency type id.
        /// </value>
        public int? FrequencyTypeId { get; set; }

        /// <summary>
        /// Gets or sets the frequency amount.
        /// </summary>
        /// <value>
        /// The frequency amount.
        /// </value>
        public decimal? FrequencyAmount { get; set; }

        /// <summary>
        /// Gets or sets the person.
        /// </summary>
        /// <value>
        /// The person.
        /// </value>
        public virtual Person Person { get; set; }

        /// <summary>
        /// Gets or sets the fund.
        /// </summary>
        /// <value>
        /// The fund.
        /// </value>
        public virtual Fund Fund { get; set; }

        /// <summary>
        /// Gets or sets the type of the frequency.
        /// </summary>
        /// <value>
        /// The type of the frequency.
        /// </value>
        public virtual DefinedValue FrequencyType { get; set; }

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
        public static Pledge Read( int id )
        {
            return Read<Pledge>( id );
        }

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
    /// Pledge Configuration class.
    /// </summary>
    public partial class PledgeConfiguration : EntityTypeConfiguration<Pledge>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PledgeConfiguration"/> class.
        /// </summary>
        public PledgeConfiguration()
        {
            this.HasOptional(p => p.Person).WithMany(p => p.Pledges).HasForeignKey(p => p.PersonId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.Fund).WithMany(f => f.Pledges).HasForeignKey(p => p.FundId).WillCascadeOnDelete(false);
            this.HasOptional(p => p.FrequencyType).WithMany().HasForeignKey(p => p.FrequencyTypeId).WillCascadeOnDelete(false);
        }
    }
}