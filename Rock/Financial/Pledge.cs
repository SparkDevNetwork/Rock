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
using Rock.Crm;
using Rock.Core;
using Rock.Data;

namespace Rock.Financial
{
    /// <summary>
    /// Pledge POCO class.
    /// </summary>
    [Table("financialPledge")]
    public partial class Pledge : ModelWithAttributes<Pledge>, IAuditable
    {
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
        public int? FundId { get; set; }

        /// <summary>
        /// Gets or sets the amount.
        /// </summary>
        /// <value>
        /// The amount.
        /// </value>
        [DataMember]
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the start date.
        /// </summary>
        /// <value>
        /// The start date.
        /// </value>
        [DataMember]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date.
        /// </summary>
        /// <value>
        /// The end date.
        /// </value>
        [DataMember]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the frequency type id.
        /// </summary>
        /// <value>
        /// The frequency type id.
        /// </value>
        [DataMember]
        public int? FrequencyTypeId { get; set; }

        /// <summary>
        /// Gets or sets the frequency amount.
        /// </summary>
        /// <value>
        /// The frequency amount.
        /// </value>
        [DataMember]
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
		/// Static Method to return an object based on the id
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public static Pledge Read( int id )
		{
			return Read<Pledge>( id );
		}

        /// <summary>
        /// Gets the auth entity.
        /// </summary>
        public override string AuthEntity { get { return "Financial.TransactionDetail"; } }
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