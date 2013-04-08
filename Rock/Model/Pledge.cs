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

using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Pledge POCO class.
    /// </summary>
    [Table("Pledge")]
    [DataContract]
    public partial class Pledge : Model<Pledge>, IValidatableObject
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
        public int? FrequencyTypeValueId { get; set; }

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
        [DataMember]
        public virtual Person Person { get; set; }

        /// <summary>
        /// Gets or sets the fund.
        /// </summary>
        /// <value>
        /// The fund.
        /// </value>
        [DataMember]
        public virtual Fund Fund { get; set; }

        /// <summary>
        /// Gets or sets the type of the frequency.
        /// </summary>
        /// <value>
        /// The type of the frequency.
        /// </value>
        [DataMember]
        public virtual DefinedValue FrequencyTypeValue { get; set; }

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

        /// <summary>
        /// Performs custom validation rules
        /// </summary>
        /// <param name="validationContext">The validation context.</param>
        /// <returns></returns>
        public IEnumerable<ValidationResult> Validate( ValidationContext validationContext )
        {
            if ( Fund != null && !Fund.IsPledgable )
            {
                yield return new ValidationResult( "Fund must allow pledges.", new[] { "Fund" } );
            }
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
            this.HasOptional(p => p.FrequencyTypeValue).WithMany().HasForeignKey(p => p.FrequencyTypeValueId).WillCascadeOnDelete(false);
        }
    }
}