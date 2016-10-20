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
    [Table( "_com_centralaz_HumanResources_ContributionElection" )]
    [DataContract]
    public class ContributionElection : Rock.Data.Model<ContributionElection>, Rock.Data.IRockEntity
    {

        #region Entity Properties

        [Required]
        [DataMember( IsRequired = true )]
        public int PersonAliasId { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public int FinancialAccountId { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public bool IsFixedAmount { get; set; }

        [Required]
        [DataMember( IsRequired = true )]
        public double Amount { get; set; }

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

        public virtual FinancialAccount FinancialAccount { get; set; }

        #endregion

    }

    #region Entity Configuration


    public partial class ContributionElectionConfiguration : EntityTypeConfiguration<ContributionElection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContributionElectionConfiguration"/> class.
        /// </summary>
        public ContributionElectionConfiguration()
        {
            this.HasRequired( r => r.PersonAlias ).WithMany().HasForeignKey( r => r.PersonAliasId ).WillCascadeOnDelete( false );
            this.HasRequired( r => r.FinancialAccount ).WithMany().HasForeignKey( r => r.FinancialAccountId ).WillCascadeOnDelete( false );
        }
    }

    #endregion


}