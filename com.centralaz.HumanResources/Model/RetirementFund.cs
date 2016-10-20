using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Rock.Model;
using System;

namespace com.centralaz.HumanResources.Model
{
    /// <summary>
    /// A Room Reservation
    /// </summary>
    [Table( "_com_centralaz_HumanResources_RetirementFund" )]
    [DataContract]
    public class RetirementFund : Rock.Data.Model<RetirementFund>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public int FundValueId { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public bool IsFixedAmount { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public double EmployeeAmount { get; set; }

        [DataMember]
        public double EmployerAmount { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public DateTime ActiveDate { get; set; }

        [DataMember]
        public DateTime? InactiveDate { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public bool IsActive { get; set; }
        #endregion

        #region Virtual Properties

        public virtual PersonAlias PersonAlias { get; set; }

        public virtual DefinedValue FundValue { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class RetirementFundConfiguration : EntityTypeConfiguration<RetirementFund>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetirementFundConfiguration"/> class.
        /// </summary>
        public RetirementFundConfiguration()
        {
            this.HasRequired( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.FundValue ).WithMany().HasForeignKey( r => r.FundValueId ).WillCascadeOnDelete( false );
        }
    }

    #endregion


}